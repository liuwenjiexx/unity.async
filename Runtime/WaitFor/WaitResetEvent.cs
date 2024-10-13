using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Unity.Async
{
    public class WaitResetEvent : IWaitable
    {
        private bool initialState;

        public WaitResetEvent()
        {
        }

        public WaitResetEvent(bool initialState)
        {
            this.initialState = initialState;
        }

        public bool IsDone => throw new System.NotImplementedException();

        public CancellationToken CancellationToken => throw new System.NotImplementedException();

        public void WaitOne(int millisecondsTimeout)
        {

        }


        public void Set()
        {

        }

        public void Reset()
        {

        }


    }
}