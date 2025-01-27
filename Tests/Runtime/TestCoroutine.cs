using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Async;
using System.Threading;
using UnityEngine.Profiling;
using System;

namespace Async.Tests
{
    public class TestCoroutine : TestBase
    {

        IEnumerator Run(IEnumerator routine)
        {
            Profiler.BeginSample("Run");
            var r = routine.Await().AsRoutine();
            Profiler.EndSample();
            return r;
        }
        IEnumerator Run<T>(IEnumerator routine)
        {
            Profiler.BeginSample("Run");
            var r = routine.Await<T>().AsRoutine();
            Profiler.EndSample();
            return r;
        }  



        [UnityTest]
        public IEnumerator Awaiter()
        {

            int unityThreadId = Thread.CurrentThread.ManagedThreadId;
            Debug.Log($"Unity Thread Id: {unityThreadId}");

            yield return Run(unityThreadId).AsRoutine();
            yield return null;
        }




        async Task Run(int unityThreadId)
        {
            Debug.Log($"Start");
            Debug.Log($"threadId: {Thread.CurrentThread.ManagedThreadId}");
            Assert.AreEqual(unityThreadId, Thread.CurrentThread.ManagedThreadId);

            await Routine();
            Assert.AreEqual(unityThreadId, Thread.CurrentThread.ManagedThreadId);

            Debug.Log("Done");
            Debug.Log($"threadId: {Thread.CurrentThread.ManagedThreadId}");
        }


        IEnumerator Routine()
        {
            int n = 3;
            while (n-- > 0)
            {
                Debug.Log($"{nameof(Routine)} ThreadId: {Thread.CurrentThread.ManagedThreadId}");
                yield return new WaitForSeconds(0.2f);
            }
        }

        [UnityTest]
        public IEnumerator _WaitForSeconds()
        {
            float t = Time.time;
            yield return Run(Routine_WaitForSeconds(0.1f));
            Assert.GreaterOrEqual(Time.time, t + 0.1f);
        }


        [UnityTest]
        public IEnumerator Coroutine_To_Async()
        {
            yield return Coroutine_To_Async1().AsRoutine();
        }

        async Task Coroutine_To_Async1()
        {
            float startTime = Time.time;
            await Routine_WaitForSeconds(0.1f).Await();
            Assert.GreaterOrEqual(Time.time, startTime + 0.1f);
            Debug.Log(Time.time - startTime);
        }

        [UnityTest]
        public IEnumerator Async_To_Coroutine()
        {
            float startTime = Time.time;
            yield return Async_WaitForSeconds(0.1f).AsRoutine();
            Assert.GreaterOrEqual(Time.time, startTime + 0.1f);
            Debug.Log(Time.time - startTime);
        }

        IEnumerator Routine_WaitForSeconds(float seconds)
        {
            yield return new WaitForSeconds(seconds);
        }

        async Task Async_WaitForSeconds(float seconds)
        {
            await new WaitForSeconds(seconds);
        }



        [UnityTest]
        public IEnumerator Return_Coroutine_AsyncReturn()
        {
            var task = Async_Add(1, 2);
            yield return task.AsRoutine();
            Assert.IsTrue(task.Result == 3);
        }

        [UnityTest]
        public IEnumerator Return_Async_AsyncReturn()
        {
            yield return Task.Run(async () =>
            {
                var result = await Async_Add(1, 2);
                Assert.IsTrue(result == 3);
            }).AsRoutine();
        }

        async Task<int> Async_Add(int a, int b)
        {
            await new WaitForSeconds(0.1f);
            return a + b;
        }




        [UnityTest]
        public IEnumerator Return_Async_CoroutineReturn()
        {
            yield return Task.Run(async () =>
            {
                var result = (int)await Coroutine_ReturnAdd(1, 2);
                Assert.AreEqual(3, result);
                Debug.Log("Result: " + result);
            }).AsRoutine();

        }
        [UnityTest]
        public IEnumerator Return_Async_Await_CoroutineReturn()
        {
            yield return Task.Run(async () =>
            {
                Debug.Log("start thread: " + Thread.CurrentThread.ManagedThreadId);
                var result = Coroutine_ReturnAdd(1, 2).Await<int>().Result;
                Assert.AreEqual(3, result);
                Debug.Log("Result: " + result);
                Debug.Log("end thread: " + Thread.CurrentThread.ManagedThreadId);                
                AssertSubThread();
            }).AsRoutine();

        }
        [UnityTest]
        public IEnumerator Return_Coroutine_CoroutineReturn()
        {
            var task = Coroutine_ReturnAdd(1, 2).Await<int>();
            yield return task.AsRoutine();
            Assert.AreEqual(3, task.Result);
            Debug.Log("Result: " + task.Result);
        }
        IEnumerator Coroutine_ReturnAdd(int a, int b)
        {
            Debug.Log("add return");
            yield return new YieldReturn(a + b); ;
        }
        IEnumerator Coroutine_ReturnString()
        {
            Debug.Log("return string");
            yield return new YieldReturn("Hello World"); ;
        }
        [UnityTest]
        public IEnumerator ReturnString2()
        {
            yield return ReturnString2_().AsRoutine();
        }
        public async Task ReturnString2_()
        {

            string result1 = await ReturnStringRoutine2().Await<string>();
            string result2 = await ReturnStringRoutine3().Await<string>();
            string result = result1 + result2;
            Debug.Log("Result: " + result);
            Assert.AreEqual("abc123", result);
        }

        IEnumerator ReturnStringRoutine2()
        {
            yield return ReturnStringRoutine3();
            yield return new YieldReturn("abc");
        }
        IEnumerator ReturnStringRoutine3()
        {
            yield return null;
            yield return new YieldReturn("123");
        }


        [UnityTest]
        public IEnumerator ReturnString4()
        {
            _ReturnString4();
            yield return null;
        }
        public async Task _ReturnString4()
        {
            var result = ReturnStringRoutine3().Await<string>();
            await result;
            Debug.Log("Result: " + result.Result);
        }

        [UnityTest]
        public IEnumerator FrameReturnString2()
        {
            int frame = Time.frameCount;
            var task = FrameReturnStringRoutine2().Await<string>();
            yield return task.AsRoutine();
            Assert.AreEqual("abc", task.Result);
            int frameEnd = Time.frameCount;

            Debug.Log($"Frame start frame: {frame}, end frame: {frameEnd}");
            Debug.Log("Result: " + task.Result);
        }

        IEnumerator FrameReturnStringRoutine2()
        {
            yield return FrameReturnStringRoutine3();
            yield return YieldReturn.Create("abc");
        }
        IEnumerator FrameReturnStringRoutine3()
        {
            yield return null;
            yield return YieldReturn.Create("123");
        }

        [UnityTest]
        public IEnumerator FrameReturnString3()
        {
            int frame = Time.frameCount;

            yield return MainThreadScheduler.GetDefault().StartCoroutine(FrameReturnStringRoutine3_1());

            int frameEnd = Time.frameCount;
            Debug.Log($"Frame start: {frame}, end: {frameEnd}");
        }

        IEnumerator FrameReturnStringRoutine3_1()
        {
            yield return FrameReturnStringRoutine3_2();
        }
        IEnumerator FrameReturnStringRoutine3_2()
        {
            yield return null;
            yield break;
        }
        //[UnityTest]
        //public IEnumerator ReturnString()
        //{
        //    int frame = Time.frameCount;
        //    var task = StartTask<string>(ReturnStringRoutine("Hello World"));
        //    yield return task.AsRoutine();
        //    Debug.Log("Result: " + task.Result);
        //    Assert.AreEqual("Hello World", task.Result);
        //}
        //async Task ReturnStringRoutine(T result)
        //{
        //    yield return null;
        //    yield return ReturnValue.Create(result);
        //}


        //[UnityTest]
        //public IEnumerator Lazy()
        //{
        //    yield return Task.Run(async () =>
        //    {
        //        Lazy<int> lazy = new Lazy<int>(() => 1);
        //        var value = await lazy;
        //        Assert.AreEqual(1, value);
        //    }).AsRoutine();
        //}


    }
}
