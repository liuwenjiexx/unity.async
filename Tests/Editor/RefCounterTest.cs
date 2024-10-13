using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
namespace Unity.Async.Tests.Editor
{
    public class RefCounterTest
    {

        [Test]
        public void Using()
        {
            RefCounter counter = new RefCounter();

            Assert.AreEqual(0, counter.Count);
            using (counter.Use())
            {
                Assert.AreEqual(1, counter.Count);
            }
            Assert.AreEqual(0, counter.Count);
        }

        [Test]
        public void Dispose()
        {
            RefCounter counter = new RefCounter();

            Assert.AreEqual(0, counter.Count);
            var use = counter.Use();
            Assert.AreEqual(1, counter.Count);
            use.Dispose();
            Assert.AreEqual(0, counter.Count);
        }

        [Test]
        public void Repeat_Dispose()
        {
            RefCounter counter = new RefCounter();

            Assert.AreEqual(0, counter.Count);
            var use = counter.Use();
            Assert.AreEqual(1, counter.Count);
            use.Dispose();
            Assert.AreEqual(0, counter.Count);
            use.Dispose();
            Assert.AreEqual(0, counter.Count);
        }

        [Test]
        public void Nest()
        {
            RefCounter counter = new RefCounter();

            Assert.AreEqual(0, counter.Count);
            using (counter.Use())
            {
                Assert.AreEqual(1, counter.Count);
                using (counter.Use())
                {
                    Assert.AreEqual(2, counter.Count);
                }
                Assert.AreEqual(1, counter.Count);
            }
            Assert.AreEqual(0, counter.Count);
        }

        [Test]
        public void Using2()
        {
            RefCounter counter = new RefCounter();

            using (counter.Use(out var n))
            {
                Assert.AreEqual(1, n);
                Assert.AreEqual(1, counter.Count);
                using (counter.Use(out var n2))
                {
                    Assert.AreEqual(2, n2);
                    Assert.AreEqual(2, counter.Count);
                }
                Assert.AreEqual(1, counter.Count);
            }
            Assert.AreEqual(0, counter.Count);
        }
        [Test]
        public void Using3()
        {
            RefCounter counter = new RefCounter();

            using (counter.Use(out var n))
            {
                Assert.AreEqual(1, n);
                Assert.AreEqual(1, counter.Count);    
            }
            Assert.AreEqual(0, counter.Count);
            using (counter.Use(out var n2))
            {
                Assert.AreEqual(1, n2);
                Assert.AreEqual(1, counter.Count);
            }
            Assert.AreEqual(0, counter.Count);
        }
        [Test]
        public void MultiThread()
        {
            RefCounter counter = new RefCounter();

            List<Task> tasks = new List<Task>();
            HashSet<int> result = new();
            int total = 10;
            foreach (var index in Enumerable.Range(1, total))
            {
                int delay = Random.Range(1, 100);
                tasks.Add(Task.Run(() =>
                {
                    using (counter.Use(out var n))
                    {
                        Thread.Sleep(delay);
                        result.Add(n);
                    }
                }));
            }

            Task.WaitAll(tasks.ToArray());
            Assert.AreEqual(0, counter.Count);
            Assert.AreEqual(total, result.Count);
            for (int i = 0; i < total; i++)
            {
                CollectionAssert.Contains(result, i + 1);
            }
        }
    }
}