using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Async
{/*
    public interface ITimeoutable
    {
        public Timeout Timeout { get; set; }
    }
    
    public struct Timeout
    {
        public double AtTime;
        public bool HasTimeout;
        public bool IsThrows;
        public string Error;

        public Timeout(int millisecondsTimeout, bool throws)
            : this(millisecondsTimeout, throws, null)
        {

        }

        public Timeout(int millisecondsTimeout, string error)
            : this(millisecondsTimeout, true, error)
        {
        }

        private Timeout(int millisecondsTimeout, bool throws, string error)
        {
            this.AtTime = Time.realtimeSinceStartupAsDouble + (millisecondsTimeout * 0.001f);
            this.HasTimeout = true;
            this.IsThrows = throws;
            this.Error = error;
        }

        public bool IsTimeout()
        {
            if (Time.realtimeSinceStartupAsDouble > AtTime)
            {
                if (!IsThrows)
                    return true;

                if (!string.IsNullOrEmpty(Error))
                {
                    throw new TimeoutException(Error);
                }
                else
                {
                    throw new TimeoutException();
                }

            }
            return false;
        }
    }*/
}
