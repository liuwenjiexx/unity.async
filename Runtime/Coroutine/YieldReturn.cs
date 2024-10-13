using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Unity.Async
{

    /// <summary>
    /// 中断并返回值
    /// </summary>
    public class YieldReturn
    {
        private object value;
        private bool reusable;

        public object Value { get => value; }

        private YieldReturn()
        {
        }
        public YieldReturn(object value)
        {
            this.value = value;
        }

        public static YieldReturn Create(object value)
        {
            var ret = StaticPool<YieldReturn>.Get();
            ret.value = value;
            return ret;
        }
        internal static void Release(YieldReturn returnValue)
        {
            StaticPool<YieldReturn>.Release(returnValue);
        }

        class PoolProvider : StaticPool<YieldReturn>.IProvider
        {
            public int Priority => 0;

            public YieldReturn Create()
            {
                return new YieldReturn() { reusable = true };
            }

            public void OnRelease(YieldReturn target)
            {
                target.value = null;
            }
        }
    }

}