using System;
using System.Threading;

namespace Unity.Async
{
    public class WaitNotNull : IWaitable,/* ITimeoutable,*/ ICancelable
    {
        private Func<object> getResult;
        private bool isDone;
        private CancellationToken cancellationToken;

        public WaitNotNull(Func<object> getResult)
            : this(getResult, CancellationToken.None)
        {
        }

        public WaitNotNull(Func<object> getResult, CancellationToken cancellationToken)
        {
            this.getResult = getResult;
            this.cancellationToken = cancellationToken;
        }

        // public Timeout Timeout { get; set; }

        public bool IsDone
        {
            get
            {
                if (!isDone)
                {
                    var result = getResult();
                    if (result != null)
                    {
                        isDone = true;
                    }
                }
                return isDone;
            }
        }

        public CancellationToken CancellationToken => cancellationToken;
    }

    public class WaitNotNull<T> : IWaitable<T>,/* ITimeoutable, */ICancelable
        where T : class
    {
        private Func<T> getResult;
        private bool isDone;
        private T result;
        private CancellationToken cancellationToken;

        public WaitNotNull(Func<T> getResult)
            : this(getResult, CancellationToken.None)
        {
        }

        public WaitNotNull(Func<T> getResult, CancellationToken cancellationToken)
        {
            this.getResult = getResult;
            this.cancellationToken = cancellationToken;
        }

       // public Timeout Timeout { get; set; }
        public CancellationToken CancellationToken => cancellationToken;

        public bool IsDone
        {
            get
            {
                if (!isDone)
                {
                    result = getResult();
                    if (result != null)
                    {
                        isDone = true;
                    }
                }
                return isDone;
            }
        }

        public T GetResult()
        {
            return result;
        }

         
    }

}
