using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Async
{
    /// <summary>
    /// 使用 <see cref="DateTime"/> 时间, 运行时和编辑器都可以等待时间
    /// </summary>
    public class WaitForTime : IWaitable
    {
        private DateTime doneTime;

        public WaitForTime(TimeSpan timeSpan)
        {
            doneTime = DateTime.Now.Add(timeSpan);
        }

        public WaitForTime(float seconds)
        {
            doneTime = DateTime.Now.AddSeconds(seconds);
        }

        bool IWaitable.IsDone
        {
            get
            {
                return DateTime.Now > doneTime;
            }
        }

    }
}