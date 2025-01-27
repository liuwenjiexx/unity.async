using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TestTools;
using Debug = UnityEngine.Debug;

namespace Async.Tests
{
    public class WaitForUnity : TestBase
    {
        [UnityTest]
        public IEnumerator WaitForEndOfFrame()
        {
            yield return new Func<Task>(async () =>
            {
                int frame = Time.frameCount;
                Debug.Log("start frame: " + Time.frameCount);
                await new WaitForEndOfFrame();
                Debug.Log("end frame: " + Time.frameCount);
                Assert.AreEqual(1, Time.frameCount - frame);
            })().AsRoutine();
        }

        [UnityTest]
        public IEnumerator WaitForEndOfFrame_0()
        {
            yield return new Func<Task>(async () =>
            {
                int frame = Time.frameCount;
                Debug.Log("start frame: " + frame);
                await WaitForEndOfFrame_0_0(frame);
                Debug.Log("end frame: " + Time.frameCount);
                Assert.AreEqual(1, Time.frameCount - frame);
            })().AsRoutine();
        }

        async Task WaitForEndOfFrame_0_0(int startFrame)
        {
            Debug.Log("func start frame: " + Time.frameCount);
            await new WaitForEndOfFrame();
            Debug.Log("func end frame: " + Time.frameCount);
        }

        [UnityTest]
        public IEnumerator WaitForEndOfFrame_1()
        {
            yield return new Func<Task>(async () =>
            {
                int frame = Time.frameCount;
                for (int i = 0; i < 3; i++)
                    await new WaitForEndOfFrame();
                Assert.AreEqual(3, Time.frameCount - frame);
            })().AsRoutine();
        }

        [UnityTest]
        public IEnumerator WaitForEndOfFrame_2()
        {
            yield return new Func<Task>(async () =>
            {
                StartWaitForEndOfFrame();
            })().AsRoutine();
        }

        async void StartWaitForEndOfFrame()
        {
            Debug.Log("1");
            StartWaitForEndOfFrame2();
            await new WaitForSeconds(0.3f);
            Debug.Log("2");
        }

        async void StartWaitForEndOfFrame2()
        {
            int frameCount = 3;
            int n = 0;
            for (int i = 0; i < frameCount; i++)
            {
                n++;
                await new WaitForEndOfFrame();
            }
            Debug.Log("n: " + n);
            Assert.AreEqual(frameCount, n);

        }


        [UnityTest]
        public IEnumerator WaitForSeconds()
        {
            float t = Time.time;
            yield return new Func<Task>(async () =>
            {
                AssertMainThread();
                await new WaitForSeconds(0.2f);
                AssertMainThread();
            })().AsRoutine();
            Assert.GreaterOrEqual(Time.time - t, 0.2f);
        }

        [UnityTest]
        public IEnumerator WaitForSeconds_OnSub()
        {
            float t = Time.time;
            yield return Task.Run(async () =>
            {
                AssertSubThread();
                await new WaitForSeconds(0.2f);
                AssertSubThread();
            }).AsRoutine();
            Assert.GreaterOrEqual(Time.time - t, 0.2f);
        }


        [UnityTest]
        public IEnumerator WaitWhile()
        {
            int n = 5;
            yield return Task.Run(async () =>
            {
                await new WaitWhile(() => --n > 0);
            }).AsRoutine();
            Assert.AreEqual(n, 0);
        }

        [UnityTest]
        public IEnumerator WaitUntil()
        {
            int n = 5;
            yield return Task.Run(async () =>
            {
                await new WaitUntil(() => --n <= 0);
            }).AsRoutine();
            Assert.AreEqual(n, 0);
        }

        [UnityTest]
        public IEnumerator WaitUntil2()
        {
            yield break;
            int limit = 2;
            int used = 0;
            int runCount = 0;
            int total = 10;
            List<Task> tasks = new();
            for (int i = 0; i < total; i++)
            {
                var j = i;
                var task = Task.Run(async () =>
                {
                    await new WaitUntil(() => used < limit);
                    used++;
                    Debug.Log($"index: {j}, used: {used}, run count: {runCount}");
                    await new WaitForSeconds(0.2f);
                    used--;
                    runCount++;
                });
                tasks.Add(task);
            }
            yield return Task.WhenAll(tasks).AsRoutine();
            Assert.AreEqual(total, runCount);
        }
        [UnityTest]
        public IEnumerator WaitUntil3()
        {
            yield break;
            int limit = 2;
            int used = 0;
            int runCount = 0;
            int total = 10;
            List<Task> tasks = new();

            var task = Task.Run(async () =>
            {
                Debug.Log($"Thread [{Thread.CurrentThread.ManagedThreadId}]");
                await new WaitUntil(() => used < limit);

                used++;
                int j = 0;
                Debug.Log($"Thread [{Thread.CurrentThread.ManagedThreadId}] index: {j}, used: {used}, run count: {runCount}");
                await new WaitForSeconds(0.2f);
                used--;
                runCount++;
            });
            tasks.Add(task);
            yield return Task.WhenAll(tasks).AsRoutine();
            Assert.AreEqual(total, runCount);
        }

        static int RequestCount = 0;
        static int RequestLimit = 1;
        static int RequestComplete = 0;

        [UnityTest]
        public IEnumerator WaitUntil_Parallel()
        {
            RequestCount = 0;
            RequestLimit = 1;
            RequestComplete = 0;

            List<Task> tasks = new List<Task>();
            for (int i = 0; i < 5; i++)
            {
                tasks.Add(WaitUntil_Parallel_2(i));
            }
            //while (true)
            //{
            //    bool allDone = true;
            //    foreach (var task in tasks)
            //    {
            //        if (!task.IsCompleted)
            //        {
            //            allDone = false;
            //            break;
            //        }
            //    }
            //    if (allDone)
            //        break;
            //    yield return null;
            //}
            yield return Task.WhenAll(tasks).AsRoutine();
            Assert.AreEqual(0, RequestCount);
            Assert.AreEqual(5, RequestComplete);

            Debug.Log($"Thread [{Thread.CurrentThread.ManagedThreadId}] RequestCount: {RequestCount}, RequestComplete: {RequestComplete}");

        }

        static async Task WaitUntil_Parallel_2(int index)
        {
            await new WaitUntil(() =>
            {
                if (RequestCount >= RequestLimit)
                    return false;
                RequestCount++;
                return true;
            });


            Assert.LessOrEqual(RequestCount, RequestLimit);

            Debug.Log($"Thread [{Thread.CurrentThread.ManagedThreadId}] Index: {index}, Enter RequestCount: {RequestCount}");
            await new WaitForEndOfFrame();


            Assert.LessOrEqual(RequestCount, RequestLimit);
            RequestCount--;
            Debug.Log($"Thread [{Thread.CurrentThread.ManagedThreadId}] Index: {index}, Exit RequestCount: {RequestCount}");
            RequestComplete++;
        }

        [UnityTest]
        public IEnumerator Unity_WaitUntil_Parallel()
        {
            RequestCount = 0;
            RequestLimit = 2;
            RequestComplete = 0;

            List<object> tasks = new List<object>();
            for (int i = 0; i < 5; i++)
            {
                tasks.Add(MainThreadScheduler.GetDefault().StartCoroutine(Unity_WaitUntil_Parallel_2(i)));
            }
            foreach (var task in tasks)
            {
                yield return task;
            }

            Assert.AreEqual(0, RequestCount);
            Assert.AreEqual(5, RequestComplete);
        }

        IEnumerator Unity_WaitUntil_Parallel_2(int index)
        {
            yield return new WaitUntil(() =>
            {
                if (RequestCount >= RequestLimit)
                    return false;
                RequestCount++;
                return true;
            });

            Assert.LessOrEqual(RequestCount, RequestLimit);

            Debug.Log($"Thread [{Thread.CurrentThread.ManagedThreadId}] Index: {index}, Enter RequestCount: {RequestCount}");
            yield return new WaitForEndOfFrame();

            Assert.LessOrEqual(RequestCount, RequestLimit);

            RequestCount--;

            Assert.LessOrEqual(RequestCount, RequestLimit);
            Debug.Log($"Thread [{Thread.CurrentThread.ManagedThreadId}] Index: {index}, Exit RequestCount: {RequestCount}");
            RequestComplete++;
        }
    }
}