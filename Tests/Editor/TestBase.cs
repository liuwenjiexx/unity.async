using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
namespace Async.Tests.Editor
{
    public class TestBase
    {
        public int MainThreadId;

        public int CurrentThreadId => Thread.CurrentThread.ManagedThreadId;

        [OneTimeSetUp]
        public virtual void OneTimeSetUp()
        {
            MainThreadId = CurrentThreadId;
        }

        public void AssertMainThread()
        {
            Assert.AreEqual(MainThreadId, CurrentThreadId);
        }

        public void AssertNotMainThread()
        {
            Assert.AreNotEqual(MainThreadId, CurrentThreadId);
        }

    }
}