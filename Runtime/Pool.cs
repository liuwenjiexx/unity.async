using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace Unity.Async
{

    class Pool<T>
    {
        private Queue<T> freelist = new Queue<T>();
        private IFactory factory;

        public Pool()
            : this(DefaultFactory.instance)
        {
        }

        public Pool(IFactory factory)
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            this.factory = factory;
        }

        public T Get()
        {
            T target;

            if (freelist.Count > 0)
            {
                target = freelist.Dequeue();
            }
            else
            {
                target = factory.Create();
            }
            factory.OnUse(target);
            return target;
        }

        public void Release(T target)
        {
            factory.OnRelease(target);
            freelist.Enqueue(target);
        }


        public interface IFactory
        {
            /// <summary>
            /// 创建实例
            /// </summary>
            T Create();

            void OnUse(T target);

            /// <summary>
            /// 释放
            /// </summary>
            /// <param name="obj"></param>
            void OnRelease(T target);
        }

        private class DefaultFactory : IFactory
        {
            public static DefaultFactory instance;

            public T Create()
            {
                return Activator.CreateInstance<T>();
            }

            public void OnUse(T target)
            {
            }

            public void OnRelease(T target)
            {
            }

        }

    }
}
