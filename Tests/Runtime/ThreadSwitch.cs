using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;
using Assert = UnityEngine.Assertions.Assert;
using Debug = UnityEngine.Debug;
using Async.Editor;

namespace Async.Tests
{
    public class ThreadSwitch : TestBase
    {

        [UnityTest]
        public IEnumerator Main_To_Main()
        {
            yield return new Func<Task>(async () =>
            {
                Debug.Log("start frame: " + Time.frameCount);
                AssertMainThread();
                await SwitchThread.Main;
                AssertMainThread();
                Debug.Log("end frame: " + Time.frameCount);

            })().AsRoutine();
        }

        [UnityTest]
        public IEnumerator Main_To_Sub()
        {
            yield return new Func<Task>(async () =>
            {
                AssertMainThread();
                await SwitchThread.Sub;
                AssertSubThread();
            })().AsRoutine();
        }

        [UnityTest]
        public IEnumerator Sub_To_Main()
        {
            yield return Task.Run(async () =>
            {
                AssertSubThread();
                await SwitchThread.Main;
                AssertMainThread();
            }).AsRoutine();
        }

        [UnityTest]
        public IEnumerator Sub_To_Sub()
        {
            yield return Task.Run(async () =>
            {
                Debug.Log("Start ThreadId: " + CurrentThreadId);
                AssertSubThread();
                await SwitchThread.Sub;
                AssertSubThread();
                Debug.Log("End ThreadId: " + CurrentThreadId);
            }).AsRoutine();
        }

        /// <summary>
        /// 顺序等待子任务
        /// </summary>
        [UnityTest]
        public IEnumerator Wait_All_Sub()
        {
            yield return new Func<Task>(async () =>
            {
                AssertMainThread();
                Debug.Log("Start frame: " + Time.frameCount);
                float startTime = Time.time;
                for (int i = 0; i < 10; i++)
                {
                    await SwitchThread.Sub;
                    Debug.Log(i + " Sub Start. ThreadId: " + CurrentThreadId);
                    AssertSubThread();
                    await Task.Delay(100);
                    AssertSubThread();
                    Debug.Log(i + " Sub Done. ThreadId: " + CurrentThreadId);
                }
                await SwitchThread.Main;
                AssertMainThread();
            //Total time 1s
            Debug.Log("All Sub Done. Time: " + (Time.time - startTime) + ", end frame: " + Time.frameCount);
            })().AsRoutine();
        }

        /// <summary>
        /// 并发等待子任务
        /// </summary>
        [UnityTest]
        public IEnumerator Parallel_Wait_All_Sub()
        {
            yield return new Func<Task>(async () =>
            {
                AssertMainThread();
                Debug.Log("Start frame: " + Time.frameCount);
                var startTime = DateTime.Now;
                Task[] tasks = new Task[10];
                for (int i = 0; i < 10; i++)
                {
                    var j = i;
                    tasks[i] = Task.Run(async () =>
                     {
                         Debug.Log(j + " Sub Start. ThreadId: " + CurrentThreadId);
                         AssertSubThread();
                         await Task.Delay(100);
                         AssertSubThread();
                         Debug.Log(j + " Sub Done. ThreadId: " + CurrentThreadId);
                     });
                }
                Task.WaitAll(tasks);
                AssertMainThread();
            //total time 0.1s
            Debug.Log("All Sub Done. Time: " + (DateTime.Now - startTime).TotalSeconds.ToString("0.#") + ", end frame: " + Time.frameCount);
            })().AsRoutine();
        }


        [UnityTest]
        public IEnumerator Thread_Switch()
        {
            yield return new Func<Task>(async () =>
            {
                AssertMainThread();
                await SwitchThread.Sub;
                AssertSubThread();
                await SwitchThread.Main;
                AssertMainThread();
                await SwitchThread.Sub;
                AssertSubThread();
                await SwitchThread.Sub;
                AssertSubThread();
                await SwitchThread.Main;
                AssertMainThread();
                await SwitchThread.Main;
                AssertMainThread();
            })().AsRoutine();
        }

        [UnityTest]
        public IEnumerator EditorMainThread()
        {
            yield return new Func<Task>(async () =>
            {
                await SwitchThread.EditorMain;
                Assert.AreEqual(MainThreadScheduler.Current, EditorMainThreadScheduler.Instance, "Switch EditorMainThread");
                AssertMainThread();
                await new WaitForSeconds(0.2f);

                Assert.AreEqual(MainThreadScheduler.Current, EditorMainThreadScheduler.Instance, "WaitForSeconds");

            })().AsRoutine();
        }
    }
}
