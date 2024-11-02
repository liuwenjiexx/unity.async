using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Async
{
    class SubThreadAwaiter : IAwaiter
    {
        private IThreadScheduler prev;

        internal SubThreadAwaiter()
        {
            prev = MainThreadScheduler.Current;
        }


        public bool IsCompleted
        {
            [DebuggerHidden]
            get => !MainThreadScheduler.IsMainThread;
        }

        [DebuggerHidden]
        public void GetResult() { }

        [DebuggerHidden]
        void INotifyCompletion.OnCompleted(Action continuation)
        {
            var next = prev ?? MainThreadScheduler.GetDefault();
            if (next.ThreadId != Thread.CurrentThread.ManagedThreadId)
            {
                try
                {
                    MainThreadScheduler.Current = prev;
                    continuation();
                }
                catch { throw; }
                finally
                {
                    MainThreadScheduler.Current = null;
                }
            }
            else
            {
                Task.Run(() =>
                {
                    try
                    {
                        MainThreadScheduler.Current = prev;
                        continuation();
                    }
                    catch { throw; }
                    finally
                    {
                        MainThreadScheduler.Current = null;
                    }
                });
            }
        }
    }

}
