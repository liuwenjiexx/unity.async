using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace Async
{
    public interface IWaitable
    {
    
        bool IsDone { get; }

    }


    public interface IWaitable<T> : IWaitable
    {
        T GetResult();
    }

    public interface ICancelable
    {
        CancellationToken CancellationToken { get; }
    }


}
