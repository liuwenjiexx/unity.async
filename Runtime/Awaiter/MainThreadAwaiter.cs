using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Unity.Async
{
    class MainThreadAwaiter : IAwaiter
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
                },null);
            }
        }
    }



}
