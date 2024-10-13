using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;
using Unity.Async;
using Assert = UnityEngine.Assertions.Assert;

namespace Unity.Async.Tests
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
                    AssertNotMainThread();
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
               AssertNotMainThread();
               await Task.Delay(100);
               Debug.Log("Sub Thread: " + CurrentThreadId);
               AssertNotMainThread();
           }).AsRoutine();
        }


        [UnityTest]
        public IEnumerator StartSubThread()
        {
            AssertMainThread();
            yield return Task.Run(async () =>
            {
                Debug.Log("Sub Thread: " + CurrentThreadId);
                AssertNotMainThread();
                int subThreadId = CurrentThreadId;
                await Task.Run(async () =>
                {
                    Debug.Log("Sub Thread1: " + CurrentThreadId);
                    AssertNotMainThread();
                    await Task.Delay(100);
                    Debug.Log("Sub Thread2: " + CurrentThreadId);
                    AssertNotMainThread();
                });

                Debug.Log("Sub Thread: " + CurrentThreadId);
                AssertNotMainThread();
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
                    AssertNotMainThread();
                    await Task.Delay(100);
                    Debug.Log("Sub Thread2: " + CurrentThreadId);
                    AssertNotMainThread();
                });
                AssertMainThread();
            })().AsRoutine();
        }



    }
}
