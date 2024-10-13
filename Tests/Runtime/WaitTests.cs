using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TestTools;

namespace Unity.Async.Tests
{
    public class WaitTests : TestBase
    {
        [UnityTest]
        public IEnumerator WaitTest()
        {
            yield return new Func<Task>(async () =>
            {
                bool isDone = false;
                Task.Delay(100)
                .ContinueWith((t) =>
                {
                    isDone = true;
                });

                await new Wait(() =>
                {
                    return isDone;
                });
                Debug.Log(isDone);
                Assert.IsTrue(isDone);
            })().AsRoutine();
        }



        //[UnityTest]
        //public IEnumerator Timeout_NotException()
        //{
        //    yield return new Func<Task>(async () =>
        //    {
        //        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        //        cancellationTokenSource.CancelAfter(100);
        //        try
        //        {
        //            await new Wait(() =>
        //            {
        //                return false;
        //            }).Timeout(0.1f, false);
        //        }
        //        catch (Exception ex)
        //        {
        //            throw;
        //        }

        //    })().AsRoutine();
        //}


        [UnityTest]
        public IEnumerator WaitResultTest()
        {
            yield return new Func<Task>(async () =>
            {
                int n = 0;
                bool isDone = false;
                Task.Delay(100)
                .ContinueWith((t) =>
                {
                    n = 1;
                    isDone = true;
                });

                int result = await new Wait<int>((out int result) =>
                {
                    result = n;
                    return isDone;
                });
                Debug.Log(result);
                Assert.AreEqual(1, result);
            })().AsRoutine();
        }

        [UnityTest]
        public IEnumerator WaitNotNull()
        {
            yield return new Func<Task>(async () =>
            {
                string value = null;
                bool isDone = false;
                Task.Delay(100)
                .ContinueWith((t) =>
                {
                    value = "abc";
                    isDone = true;
                });

                string result = await new WaitNotNull<string>(() => value);
                Debug.Log(result);
                Assert.AreEqual("abc", result);

            })().AsRoutine();
        }

        //[UnityTest]
        //public async void WaitForTime2()
        //{
        //    var dt = DateTime.Now;
        //    await new WaitForTime(0.1f);
        //    var s = DateTime.Now.Subtract(dt).TotalSeconds;
        //    Debug.Log(s);
        //    Assert.GreaterOrEqual(s, 0.1f);
        //}

        [UnityTest]
        public IEnumerator WaitForTime()
        {
            yield return new Func<Task>(async () =>
            {
                var dt = DateTime.Now;
                await new WaitForTime(0.1f);
                var s = DateTime.Now.Subtract(dt).TotalSeconds;
                Debug.Log(s);
                Assert.GreaterOrEqual(s, 0.1f);
            })().AsRoutine();
        }
   
        [UnityTest]
        public IEnumerator WaitForTime_TimeSpan()
        {
            yield return new Func<Task>(async () =>
            {
                var dt = DateTime.Now;
                await new WaitForTime(new TimeSpan(0, 0, 0, 0, 100));
                var s = DateTime.Now.Subtract(dt).TotalSeconds;
                Debug.Log(s);
                Assert.GreaterOrEqual(s, 0.1f);
            })().AsRoutine();
        }

    }
}