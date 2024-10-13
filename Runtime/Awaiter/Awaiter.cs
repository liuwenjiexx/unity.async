using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace Unity.Async
{

    class Awaiter : IAwaiter, IEnumerator
    {
        private bool isCompleted;
        private Exception exception;
        private Action continuation;
        private IThreadScheduler scheduler;
        private bool prevMain;
        IThreadScheduler prev;
        private IWaitable waitable;
       // private Timeout? timeout;
        private CancellationToken cancellationToken;

        public Awaiter(IWaitable waitable, IThreadScheduler scheduler = null)
        {
            this.waitable = waitable;
            if (scheduler == null)
                scheduler = MainThreadScheduler.GetDefault();
            this.scheduler = scheduler;
            this.prevMain = scheduler.ThreadId == Thread.CurrentThread.ManagedThreadId;
            prev = MainThreadScheduler.Current;

            if (waitable is ICancelable)
            {
                ICancelable cancelable = (ICancelable)waitable;
                cancellationToken = cancelable.CancellationToken;
            }
            /*
            if (waitable is ITimeoutable)
            {
                ITimeoutable timeoutable = (ITimeoutable)waitable;
                var t = timeoutable.Timeout;
                if (t.HasTimeout)
                {
                    timeout = t;
                }
            }*/
            Run();
        }

        public bool IsCompleted
        {
            [DebuggerHidden]
            get => isCompleted;
        }

        bool IsMainThread => scheduler.ThreadId == Thread.CurrentThread.ManagedThreadId;

        public object Current => null;



        private void Run()
        {
            if (MainThreadScheduler.IsMainThread)
            {
                scheduler.StartCoroutine(this);
            }
            else
            {
                scheduler.Post(state => scheduler.StartCoroutine(this), null);
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
            this.exception = ex;

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
                        });
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

        [DebuggerHidden]
        public bool MoveNext()
        {
            if (!isCompleted)
            {
                try
                {
                    if (waitable.IsDone)
                    {
                        Complete(null);
                    }
                    else
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                       /* if (timeout.HasValue)
                        {
                            if (timeout.Value.IsTimeout())
                            {
                                Complete(null);
                            }
                        }*/
                    }
                }
                catch (Exception ex)
                {
                    Complete(ex);
                }
            }

            return !isCompleted;
        }

        public void Reset()
        {
        }
    }

    class Awaiter<T> : IAwaiter<T>, IEnumerator
    {
        private bool isCompleted;
        private Exception exception;
        private Action continuation;
        private T result;
        private IThreadScheduler scheduler;
        private bool prevMain;
        IThreadScheduler prev;
        private IWaitable<T> waitable;
     //   private Timeout? timeout;
        private CancellationToken cancellationToken;

        public Awaiter(IWaitable<T> waitable, IThreadScheduler scheduler = null)
        {
            this.waitable = waitable;
            if (scheduler == null)
                scheduler = MainThreadScheduler.GetDefault();
            this.scheduler = scheduler;
            this.prevMain = scheduler.ThreadId == Thread.CurrentThread.ManagedThreadId;
            prev = MainThreadScheduler.Current;

            if (waitable is ICancelable)
            {
                ICancelable cancelable = (ICancelable)waitable;
                cancellationToken = cancelable.CancellationToken;
            }
            /*
            if (waitable is ITimeoutable)
            {
                ITimeoutable timeoutable = (ITimeoutable)waitable;
                var t = timeoutable.Timeout;
                if (t.HasTimeout)
                {
                    timeout = t;
                }
            }*/
            Run();
        }

        public bool IsCompleted
        {
            [DebuggerHidden]
            get => isCompleted;
        }

        bool IsMainThread => scheduler.ThreadId == Thread.CurrentThread.ManagedThreadId;

        public object Current => null;



        private void Run()
        {
            if (MainThreadScheduler.IsMainThread)
            {
                scheduler.StartCoroutine(this);
            }
            else
            {
                scheduler.Post(state => scheduler.StartCoroutine(this), null);
            }
        }

        [DebuggerHidden]
        public T GetResult()
        {

            if (exception != null)
            {
                ExceptionDispatchInfo.Capture(exception).Throw();
            }

            return result;
        }


        public void Complete(T result, Exception ex)
        {
            isCompleted = true;
            this.result = result;
            this.exception = ex;

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
                        });
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

        public bool MoveNext()
        {
            if (!isCompleted)
            {
                try
                {
                    if (waitable.IsDone)
                    {
                        var result = waitable.GetResult();
                        Complete(result, null);
                    }
                    else
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                       /* if (timeout.HasValue)
                        {
                            if (timeout.Value.IsTimeout())
                            {
                                Complete(default, null);
                            }
                        }*/
                    }
                }
                catch (Exception ex)
                {
                    Complete(default, ex);
                }
            }

            return !isCompleted;
        }

        public void Reset()
        {
        }
    }



}
