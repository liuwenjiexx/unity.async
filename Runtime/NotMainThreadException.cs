using System.Collections;
using System.Collections.Generic;
using System;

namespace Unity.Async
{
    public class NotMainThreadException : Exception
    {
        public NotMainThreadException(string message)
            : base(message)
        { }

        public NotMainThreadException()
            : base("Not Main Thread")
        { }
    }
}