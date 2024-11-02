using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace Async
{    
    /// <summary>
    /// 效率更高的对象池，复用对象避免GC, 
    /// </summary>
    static class StaticPool<T>
    {
        private static Queue<T> freelist = new Queue<T>();
        private static IProvider provider;

        static StaticPool()
        {
            Initalize();
        }

        static void Initalize()
        {
            foreach (var type in Referenced(AppDomain.CurrentDomain.GetAssemblies(), typeof(IProvider).Assembly)
                .SelectMany(o => o.GetTypes()))
            {
                if (type.IsAbstract || !typeof(IProvider).IsAssignableFrom(type))
                    continue;
                var p = Activator.CreateInstance(type) as IProvider;
                if (provider == null || p.Priority > provider.Priority)
                {
                    provider = p;
                }
            }
        }


        public static T Get()
        {
            T target;

            if (freelist.Count > 0)
            {
                target = freelist.Dequeue();
            }
            else
            {
                if (provider != null)
                {
                    target = provider.Create();
                }
                else
                {
                    target = Activator.CreateInstance<T>();
                }
            }
            return target;
        }

        public static void Release(T target)
        {
            if (provider != null)
                provider.OnRelease(target);
            freelist.Enqueue(target);
        }

        static IEnumerable<Assembly> Referenced(IEnumerable<Assembly> assemblies, Assembly referenced)
        {
            string fullName = referenced.FullName;

            foreach (var ass in assemblies)
            {
                if (referenced == ass)
                {
                    yield return ass;
                }
                else
                {
                    foreach (var refAss in ass.GetReferencedAssemblies())
                    {
                        if (fullName == refAss.FullName)
                        {
                            yield return ass;
                            break;
                        }
                    }
                }
            }
        }

        public interface IProvider
        {
            /// <summary>
            /// 如果存在多个提供程序使用优先级值更大的提供程序
            /// </summary>
            int Priority { get; }

            /// <summary>
            /// 创建实例
            /// </summary>
            T Create();

            /// <summary>
            /// 释放
            /// </summary>
            /// <param name="obj"></param>
            void OnRelease(T target);
        }

    }



}