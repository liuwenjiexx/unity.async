using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;


namespace Async
{

    public class CoroutineAwaiter : IAwaiter
    {
        private bool isCompleted;
        private Exception exception;
        private Action continuation;
        private IThreadScheduler scheduler;
        private bool prevMain;
        IThreadScheduler prev;

        public CoroutineAwaiter(IThreadScheduler scheduler=null)
        {
            if (scheduler == null)
                scheduler = MainThreadScheduler.GetDefault();
            this.scheduler = scheduler;
            this.prevMain = scheduler.ThreadId == Thread.CurrentThread.ManagedThreadId;
            prev = MainThreadScheduler.Current;
        }

        public bool IsCompleted
        {
            [DebuggerHidden]
            get { return isCompleted; }
        }

        bool IsMainThread => scheduler.ThreadId == Thread.CurrentThread.ManagedThreadId;

        [DebuggerHidden]
        public void Initalize(IEnumerator routine)
        {
            if (MainThreadScheduler.IsMainThread)
            {
                scheduler.StartCoroutine(routine);
            }
            else
            {
                scheduler.Post(state => scheduler.StartCoroutine(routine), null);
            }
        }

        [DebuggerHidden]
        public void GetResult()
        {
            if (exception != null)
            {
                ExceptionDispatchInfo.Capture(exception).Throw();
            }
        }


        public void Complete(Exception ex)
        {
            isCompleted = true;
            exception = ex;

            if (continuation != null)
            {
                var next = prev ?? scheduler;

                if (prevMain)
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
                else
                {
                    if (IsMainThread)
                    {
                        Task.Run(() =>
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
                            );
                    }
                    else
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
                }
            }
        }

        [DebuggerHidden]
        void INotifyCompletion.OnCompleted(Action continuation)
        {
            this.continuation = continuation;
        }
    }
}
