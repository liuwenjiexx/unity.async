using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TestTools;

namespace Async.Tests
{

    public class TestException
    {
        public IEnumerator WaitTask(Task task)
        {
            while (!task.IsCompleted)
                yield return null;
            if (task.Exception != null)
                throw task.Exception;
        }


        [UnityTest]
        public IEnumerator Exception0()
        {
            yield return Throws<IgnoreException>(ThrowException()).AsRoutine();
        }

        [UnityTest]
        public IEnumerator Exception()
        {
            yield return Throws<IgnoreException>(ThrowException()).AsRoutine();
        }

        [UnityTest]
        public IEnumerator OneFrameAfter()
        {
            yield return Throws<IgnoreException>(ThrowExceptionWaitOneFrame()).AsRoutine();
        }

        //[UnityTest]
        //public IEnumerator UnhandledException()
        //{
        //    yield return ThrowException();
        //}

        //[UnityTest]
        //public IEnumerator UnhandledException2()
        //{
        //    yield return Task.Run(() =>
        //    {
        //        throw new Exception("");
        //    });
        //}


        [UnityTest]

        public IEnumerator Coroutine_Exception()
        {
            yield return Throws<IgnoreException>(Coroutine_Throw_Exception().Await()).AsRoutine();
        }

        IEnumerator Coroutine_Throw_Exception()
        {
            yield return null;
            throw new IgnoreException();
        }

        class IgnoreException : Exception
        {
            public IgnoreException()
            {
            }
        }

        async Task Throws<T>(Task task)
            where T : Exception
        {
            bool hasEx = false;
            try
            {
                await task;
            }
            catch (Exception ex)
            {
                if (ex is T)
                {
                    hasEx = true;
                }

            }
            if (!hasEx)
                Assert.Fail("Not expected exception: " + typeof(T).Name);
        }


        async Task RunAsyncTest(Task task)
        {
            await task;
        }

        async Task ThrowException()
        {
            throw new IgnoreException();
        }

        async Task ThrowExceptionWaitOneFrame()
        {
            await new WaitForEndOfFrame();
            throw new IgnoreException();
        }

    }
}