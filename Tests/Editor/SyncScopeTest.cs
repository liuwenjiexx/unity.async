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
    public class SyncScopeTest
    {
        int mainThreadId;

        [SetUp]
        public void SetUp()
        {
            mainThreadId = Thread.CurrentThread.ManagedThreadId;
            Debug.Log("MainThreadId: " + mainThreadId);
        }
        void CheckMainThread()
        {
            if (Thread.CurrentThread.ManagedThreadId != mainThreadId)
                throw new NotMainThreadException();

        }

        [Test]
        public async void Sync()
        {
            List<Task> tasks = new();
            SyncLock lockScope = new SyncLock(1000);
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(Sync_2(i, lockScope));
            }

            await new Wait(() => tasks.TrueForAll(o => o.IsCompleted));
            Assert.AreEqual(0, lockScope.Count);
            Debug.Log("Count: " + lockScope.Count);
        }

        async Task Sync_2(int value, SyncLock lockScope)
        {
            //Debug.Log("Thread: " + Thread.CurrentThread.ManagedThreadId+ " Start");
            using (var l = lockScope.Sync())
            {
                //Debug.Log("Value start: " + value);
                //await new Wait(() => !l.IsDone);
                await l;
                Debug.Log("Thread: " + Thread.CurrentThread.ManagedThreadId + " Value: " + value);
                await new EditorWaitForSeconds(Random.value * 0.1f);
            }
        }

        [Test]
        public void Unrelease()
        {
            SyncLock lockScope = new SyncLock(10);
            Assert.AreEqual(0, lockScope.Count);
            Assert.IsTrue(lockScope.Sync().IsDone);
            Assert.AreEqual(1, lockScope.Count);
            //Thread.Sleep(10);
            GC.Collect();
            Thread.Sleep(20);
            Assert.AreEqual(0, lockScope.Count);
        }


        [Test]
        public async void Timeout()
        {
            SyncLock lockScope = new SyncLock(20);
            bool isTimeout = false;
            Assert.IsTrue(lockScope.Sync(() =>
            {
                CheckMainThread();
                isTimeout = true;
                Debug.Log("Timeout");
            }).IsDone);
            Assert.AreEqual(1, lockScope.Count);
            await Task.Delay(50);
            Assert.IsTrue(isTimeout);
            
            Assert.AreEqual(0, lockScope.Count);
            Debug.Log("Count: " + lockScope.Count);
        }

        [Test]
        public void Timeout2()
        {
            SyncLock lockScope = new SyncLock(20);
            int timeout = 0;
        
            List<Task> tasks = new List<Task>();
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(Wait(lockScope, 100, onTimeout: () =>
                {
                    timeout++;
                }));
            }
            Thread.Sleep(50);
            Task.Run(() =>
            {
                Task.WaitAll(tasks.ToArray());
                Assert.AreEqual(0, lockScope.Count);
                Debug.Log("Count: " + lockScope.Count);
            });
        }

        async Task Wait(SyncLock lockScope, int ms, Action onTimeout = null)
        {
            using (var l = lockScope.Sync())
            {
                await l;
                if (ms > 0)
                {
                    await new EditorWaitForSeconds(ms / 1000f);
                }
            }
        }
    }
}
