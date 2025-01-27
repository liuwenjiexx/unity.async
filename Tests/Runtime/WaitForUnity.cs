using NUnit.Framework;
using System;
using System.Collections;
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

    }
}