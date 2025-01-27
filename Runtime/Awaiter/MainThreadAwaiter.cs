using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Async
{
    class MainThreadAwaiter : IAwaiter, IAwaitable
    {

        public bool IsCompleted
        {
            [DebuggerHidden]
            get => MainThreadScheduler.IsMainThread;
        }

        [DebuggerHidden]
        public void GetResult() { }

        [DebuggerHidden]
        void INotifyCompletion.OnCompleted(Action continuation)
        {
            var next = MainThreadScheduler.GetCurrentOrDefault();

            if (next.ThreadId == Thread.CurrentThread.ManagedThreadId)
            {
                try
                {
                    MainThreadScheduler.Current = next;
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
                next.Post((s) =>
                {
                    try
                    {
                        MainThreadScheduler.Current = next;
                        continuation();
                    }
                    catch { throw; }
                    finally
                    {
                        MainThreadScheduler.Current = null;
                    }
                }, null);
            }
        }

        public IAwaiter GetAwaiter() => this;

    }



}
