using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Async
{
    public class SyncLock
    {
        private int timeoutMS;
        private int count;
        private object lockObj = new();


        public SyncLock(int timeoutMS)
        {
            this.timeoutMS = timeoutMS;
        }

        public SyncScope Sync(Action onTimeout = null)
        {
            return new SyncScope(this, onTimeout);
        }

        public int Count => count;

        public class SyncScope : IWaitable, IDisposable
        {
            public SyncLock scope;
            private int state;
            private CancellationTokenSource cancellationTokenSource;
            private CancellationToken cancellationToken;
            private Action onTimeout;

            public bool IsDone
            {
                get
                {
                    if (state == STATE_UNINITIALIZED)
                    {
                        Initialize();
                    }
                    return state != STATE_UNINITIALIZED;
                }
            }

            const int STATE_UNINITIALIZED = 0;
            const int STATE_INITIALIZED = 1;
            const int STATE_DISPOSED = 2;

            internal SyncScope(SyncLock scope, Action onTimeout)
            {
                this.scope = scope;
                state = STATE_UNINITIALIZED;
                cancellationTokenSource = null;
                cancellationToken = default;
                this.onTimeout = onTimeout;
            }

            private void Initialize()
            {
                if (state == STATE_UNINITIALIZED)
                {
                    lock (scope.lockObj)
                    {
                        if (state == STATE_UNINITIALIZED)
                        {
                            if (scope.count == 0)
                            {
                                state = STATE_INITIALIZED;
                                scope.count++;
                                cancellationTokenSource = new CancellationTokenSource(scope.timeoutMS);
                                cancellationToken = cancellationTokenSource.Token;
                                cancellationToken.Register(Timeout);
                            }
                        }
                    }
                }
            }

            private async void Timeout()
            {
                await SwitchThread.Main;

                if (state == STATE_DISPOSED)
                    return;
                lock (scope.lockObj)
                {
                    if (state != STATE_DISPOSED)
                    {
                        Dispose();
                        onTimeout?.Invoke();
                    }
                }
            }

            public void Dispose()
            {
                if (state == STATE_DISPOSED)
                    return;
                lock (scope.lockObj)
                {
                    if (state != STATE_DISPOSED)
                    {
                        if (state == STATE_INITIALIZED)
                        {
                            scope.count--;
                        }
                        state = STATE_DISPOSED;
                        cancellationTokenSource?.Cancel();
                    }
                }
            }
        }

    }
}
