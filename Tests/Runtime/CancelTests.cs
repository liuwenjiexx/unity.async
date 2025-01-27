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
    public class CancelTests : TestBase
    {

        [UnityTest]
        public IEnumerator Cancel()
        {
            yield return new Func<Task>(async () =>
            {
                try
                {
                    CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                    cancellationTokenSource.CancelAfter(100);
                    await new Wait(() =>
                    {
                        return false;
                    }, cancellationTokenSource.Token);
                }
                catch (OperationCanceledException ex)
                {
                    Debug.Log(ex.Message);
                    return;
                }
                Assert.Fail();
            })().AsRoutine();
        }

        [UnityTest]
        public IEnumerator Cancel2()
        {
            yield return new Func<Task>(async () =>
            {
                int n = 0;
                try
                {
                    CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                    cancellationTokenSource.CancelAfter(100);
                    await new Wait(() =>
                    {
                        return false;
                    }, cancellationTokenSource.Token);
                    n++;
                    await new Wait(() =>
                    {
                        return false;
                    }, cancellationTokenSource.Token);
                    n++;
                }
                catch (OperationCanceledException ex)
                {
                    Debug.Log(ex.Message);
                    return;
                }
                Assert.AreEqual(0, n);
                Assert.Fail();
            })().AsRoutine();
        }

    }
}
