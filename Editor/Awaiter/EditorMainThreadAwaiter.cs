using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Async.Editor
{
    class EditorMainThreadAwaiter : IAwaiter
    {
        public bool IsCompleted
        {
            [DebuggerHidden]
            get => false;
        }

        [DebuggerHidden]
        public void GetResult() { }

        [DebuggerHidden]
        void INotifyCompletion.OnCompleted(Action continuation)
        {
            var next = EditorMainThreadScheduler.Instance;
            if (next.ThreadId == Thread.CurrentThread.ManagedThreadId)
            {
                try
                {
                    MainThreadScheduler.Current = EditorMainThreadScheduler.Instance;
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
                EditorMainThreadScheduler.Instance.Post(() =>
                {
                    try
                    {
                        MainThreadScheduler.Current = EditorMainThreadScheduler.Instance;
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
