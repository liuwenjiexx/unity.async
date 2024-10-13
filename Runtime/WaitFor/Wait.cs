using System;
using System.Threading;

namespace Unity.Async
{

    public class Wait : IWaitable, /*ITimeoutable,*/ ICancelable
    {
        private Func<bool> done;
        private bool isDone;
        private CancellationToken cancellationToken;

        public Wait(Func<bool> done)
            : this(done, CancellationToken.None)
        {
        }

        public Wait(Func<bool> done, CancellationToken cancellationToken)
        {
            this.done = done;
            this.cancellationToken = cancellationToken;
        }

        public CancellationToken CancellationToken => cancellationToken;

       // public Timeout Timeout { get; set; }

        public bool IsDone
        {
            get
            {
                if (!isDone)
                {
                    if (done())
                    {
                        isDone = true;
                    }
                }
                return isDone;
            }
        }


    }


    public class Wait<T> : IWaitable<T>, /*ITimeoutable, */ICancelable
    {
        private GetResultDelegate getResult;
        private T result;
        private bool isDone;
        private CancellationToken cancellationToken;

        public Wait(GetResultDelegate getResult)
            : this(getResult, CancellationToken.None)
        {
        }

        public Wait(GetResultDelegate getResult, CancellationToken cancellationToken)
        {
            if (getResult == null) throw new ArgumentNullException(nameof(getResult));
            this.getResult = getResult;
            this.cancellationToken = cancellationToken;

        }

        public delegate bool GetResultDelegate(out T result);
        
        public CancellationToken CancellationToken => cancellationToken;

        //public Timeout Timeout { get; set; }

        public bool IsDone
        {
            get
            {
                if (!isDone)
                {
                    if (getResult(out result))
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