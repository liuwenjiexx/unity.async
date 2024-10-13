using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Unity.Async
{
    public interface IThreadScheduler
    {
        int ThreadId { get; }

        SynchronizationContext SynchronizationContext { get; }

        object StartCoroutine(IEnumerator routine);

       /// <summary>
       /// 主线程排队
       /// </summary>
        void Post(SendOrPostCallback action, object state);

        /// <summary>
        /// 同步执行
        /// </summary>
        void Send(SendOrPostCallback action, object state);
    }

}
