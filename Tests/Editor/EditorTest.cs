using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.EditorCoroutines.Editor;
using UnityEngine;
using Random = UnityEngine.Random;
namespace Unity.Async.Tests.Editor
{
    public class EditorTest : TestBase
    {
        [OneTimeTearDown]
        public virtual void OneTimeTearDown()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        void Run(Task task)
        {
            task.ContinueWith(t =>
            {
                if (t.Exception != null)
                {
                    Debug.LogException(t.Exception);
                }
            });

        }

        //[Test]
        //public void EditorWaitForSeconds()
        //{
        //    Run(new Task(async () =>
        //   {
        //       var dt = DateTime.Now;
        //       await new EditorWaitForSeconds(0.2f);
        //       var s = DateTime.Now.Subtract(dt).TotalSeconds;
        //       Debug.Log(s);
        //       Assert.GreaterOrEqual(s, 0.2f);
        //   }));
        //}

        [Test]
        public async void EditorWaitForSeconds()
        {
            var dt = DateTime.Now;
            await new EditorWaitForSeconds(0.2f);
            var s = DateTime.Now.Subtract(dt).TotalSeconds;
            Debug.Log(s);
            Assert.GreaterOrEqual(s, 0.2f);
        }



        [Test]
        public void Routine()
        {
            Run(new Task(async () =>
            {
                var dt = DateTime.Now;
                await WaitForSeconds_Routine(0.2f).Await();
                Assert.GreaterOrEqual(DateTime.Now.Subtract(dt).TotalSeconds, 0.2f);
            }));
        }

        IEnumerator WaitForSeconds_Routine(float seconds)
        {
            yield return new EditorWaitForSeconds(seconds);
        }

        //[Test]
        //public async void Wait_Routine()
        //{
        //    var dt = DateTime.Now;
        //    await WaitForSeconds_1(0.2f);
        //    float t2 = (float)DateTime.Now.Subtract(dt).TotalSeconds;
        //    UnityEngine.Assertions.Assert.IsTrue(t2 > 0.2f);
        //}
        //IEnumerator Add()
        //{
        //    yield return new EditorWaitForSeconds(0.2f);
        //}

        [Test]
        public void Post()
        {
            var context = SynchronizationContext.Current;

            Debug.Log("MainThreadId: " + Thread.CurrentThread.ManagedThreadId);
            Debug.Log(context.GetType().FullName);

            Debug.Log($"[{Thread.CurrentThread.ManagedThreadId}] Post Start");
            for (int i = 0; i < 3; i++)
            {
                context.Post((s) =>
                {
                    Debug.Log($"[{Thread.CurrentThread.ManagedThreadId}] Execute Post " + s);
                }, $"[{Thread.CurrentThread.ManagedThreadId}] => {i}");
            }
            Debug.Log($"[{Thread.CurrentThread.ManagedThreadId}] Post End");

            Debug.Log($"[{Thread.CurrentThread.ManagedThreadId}] Send Start");
            for (int i = 0; i < 3; i++)
            {
                context.Send((s) =>
                {
                    Debug.Log($"[{Thread.CurrentThread.ManagedThreadId}] Execute Send " + s);
                }, $"[{Thread.CurrentThread.ManagedThreadId}] => {i}");
            }
            Debug.Log($"[{Thread.CurrentThread.ManagedThreadId}] Send End");

            Task.Run(() =>
            {
                Debug.Log($"[{Thread.CurrentThread.ManagedThreadId}] Post Start");
                for (int i = 0; i < 3; i++)
                {
                    context.Post((s) =>
                    {
                        Debug.Log($"[{Thread.CurrentThread.ManagedThreadId}] Execute Post " + s);
                    }, $"[{Thread.CurrentThread.ManagedThreadId}] => {i}");
                }
                Debug.Log($"[{Thread.CurrentThread.ManagedThreadId}] Post End");

                Debug.Log($"[{Thread.CurrentThread.ManagedThreadId}] Send Start");
                for (int i = 0; i < 3; i++)
                {
                    context.Send((s) =>
                    {
                        Debug.Log($"[{Thread.CurrentThread.ManagedThreadId}] Execute Send " + s);
                    }, $"[{Thread.CurrentThread.ManagedThreadId}] => {i}");
                }
                Debug.Log($"[{Thread.CurrentThread.ManagedThreadId}] Send End");
            });

            //Run(new Task(async () =>
            //{
            //    var dt = DateTime.Now;
            //    await WaitForSeconds_Routine(0.2f).Await();
            //    Assert.GreaterOrEqual(DateTime.Now.Subtract(dt).TotalSeconds, 0.2f);
            //}));
        }


        [Test]
        public async void WaitForTime()
        {
            var dt = DateTime.Now;
            await new WaitForTime(0.1f);
            var s = DateTime.Now.Subtract(dt).TotalSeconds;
            Debug.Log(s);
            Assert.GreaterOrEqual(s, 0.1f);
        }

        [Test]
        public async void WaitForTime_TimeSpan()
        {
            var dt = DateTime.Now;
            await new WaitForTime(new TimeSpan(0, 0, 0, 0, 100));
            var s = DateTime.Now.Subtract(dt).TotalSeconds;
            Debug.Log(s);
            Assert.GreaterOrEqual(s, 0.1f);
        }

        [Test]
        public async void CompareExchange()
        {
            int n = 1;

            int n2 = Interlocked.CompareExchange(ref n, 2, 1);
            Debug.Log(n + ", " + n2);
            n = 1;
            n2 = Interlocked.CompareExchange(ref n, 2, 2);
            Debug.Log(n + ", " + n2);
        }

        [Test]
        public async void RegisterCanceled()
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(1);
            Thread.Sleep(10);
            Debug.Log("IsCancellationRequested: " + cancellationTokenSource.IsCancellationRequested);
            cancellationTokenSource.Token.Register(() =>
            {
                Debug.Log("Register Canceled");
            });
        }


    }
}
