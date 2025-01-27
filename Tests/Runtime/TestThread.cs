using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;
using Assert = UnityEngine.Assertions.Assert;

namespace Async.Tests
{
    public class TestThread : TestBase
    {

        [UnityTest]
        public IEnumerator RunTask()
        {
            AssertMainThread();
            yield return new Func<Task>(async () =>
            {
                AssertMainThread();
                await Task.Run(() =>
                {
                    Debug.Log("Sub Thread: " + CurrentThreadId);
                    AssertSubThread();
                });
                AssertMainThread();
            })().AsRoutine();
        }

        [UnityTest]
        public IEnumerator Main_Delay()
        {
            AssertMainThread();
            yield return new Func<Task>(async () =>
            {
                AssertMainThread();
                await Task.Delay(100);
                AssertMainThread();
            })().AsRoutine();
        }


        [UnityTest]
        public IEnumerator Sub_Delay()
        {
            AssertMainThread();
            yield return Task.Run(async () =>
           {
               Debug.Log("Sub Thread: " + CurrentThreadId);
               AssertSubThread();
               await Task.Delay(100);
               Debug.Log("Sub Thread: " + CurrentThreadId);
               AssertSubThread();
           }).AsRoutine();
        }


        [UnityTest]
        public IEnumerator StartSubThread()
        {
            AssertMainThread();
            yield return Task.Run(async () =>
            {
                Debug.Log("Sub Thread: " + CurrentThreadId);
                AssertSubThread();
                int subThreadId = CurrentThreadId;
                await Task.Run(async () =>
                {
                    Debug.Log("Sub Thread1: " + CurrentThreadId);
                    AssertSubThread();
                    await Task.Delay(100);
                    Debug.Log("Sub Thread2: " + CurrentThreadId);
                    AssertSubThread();
                });

                Debug.Log("Sub Thread: " + CurrentThreadId);
                AssertSubThread();
            }).AsRoutine();
        }

        [UnityTest]
        public IEnumerator MainThread()
        {
            AssertMainThread();
            yield return new Func<Task>(async () =>
            {
                AssertMainThread();
                int subThreadId = CurrentThreadId;
                await Task.Run(async () =>
                {
                    Debug.Log("Sub Thread1: " + CurrentThreadId);
                    AssertSubThread();
                    await Task.Delay(100);
                    Debug.Log("Sub Thread2: " + CurrentThreadId);
                    AssertSubThread();
                });
                AssertMainThread();
            })().AsRoutine();
        }



    }
}
