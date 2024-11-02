using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Async
{
    public static partial class Extensions
    {
        private static MainThreadAwaiter MainThreadAwaiter = new MainThreadAwaiter();

        private static SubThreadAwaiter SubThreadAwaiter = new SubThreadAwaiter();

        [DebuggerHidden]
        public static IAwaiter GetAwaiter(this MainThread waitForMainThread) => MainThreadAwaiter;

        [DebuggerHidden]
        public static IAwaiter GetAwaiter(this SubThread waitForSubThread) => SubThreadAwaiter;

        [DebuggerHidden]
        public static IAwaiter GetAwaiter(this IWaitable waitable)
        {
            return new Awaiter(waitable,  MainThreadScheduler.GetDefault());
        }

        [DebuggerHidden]
        public static IAwaiter<T> GetAwaiter<T>(this IWaitable<T> waitable)
        {
            return new Awaiter<T>(waitable,  MainThreadScheduler.GetDefault());
        }

        /*
        public static T Timeout<T>(this T self, int millisecondsTimeout, bool throws = true)
            where T : ITimeoutable
        {
            self.Timeout = new Unity.Async.Timeout(millisecondsTimeout, throws);
            return self;
        }

        public static T Timeout<T>(this T self, int millisecondsTimeout, string error)
          where T : ITimeoutable
        {
            self.Timeout = new Unity.Async.Timeout(millisecondsTimeout, error);
            return self;
        }*/

    }
}