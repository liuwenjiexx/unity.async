using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Async
{
    class MainThreadScheduler : IThreadScheduler
    {
        private static int mainThreadId;
        private static SynchronizationContext synchronizationContext;
        private static MainThreadScheduler instance;
        private MonoScheduler monoScheduler;

        public static Func<object, IThreadScheduler> ResolveScheduler;

        [ThreadStatic]
        private static IThreadScheduler current;

        public static IThreadScheduler Current
        {
            [DebuggerHidden]
            get => current;
            internal set => current = value;
        }


        class MonoScheduler : MonoBehaviour
        {

        }

        private static MainThreadScheduler Instance
        {
            [DebuggerHidden]
            get
            {
                if (instance == null)
                {
                    instance = new MainThreadScheduler();
                }
                return instance;
            }
        }

        public int ThreadId
        {
            [DebuggerHidden]
            get => mainThreadId;
        }

        public SynchronizationContext SynchronizationContext
        {
            [DebuggerHidden]
            get => synchronizationContext;
        }

        public static bool IsMainThread
        {
            [DebuggerHidden]
            get => GetDefault().SynchronizationContext == SynchronizationContext.Current;
        }

        [DebuggerHidden]
        private MonoScheduler GetMonoScheduler()
        {
            if (!monoScheduler)
            {
                monoScheduler = new GameObject(nameof(MainThreadScheduler))
                    .AddComponent<MonoScheduler>();
                GameObject.DontDestroyOnLoad(monoScheduler.gameObject);
                monoScheduler.gameObject.hideFlags = HideFlags.HideAndDontSave;

            }

            return monoScheduler;
        }

        internal static void UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            if (!e.Observed)
            {
                Debug.LogException(e.Exception);
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void RuntimeInitializeOnLoadMethod()
        {
            synchronizationContext = SynchronizationContext.Current;
            mainThreadId = Thread.CurrentThread.ManagedThreadId;
            TaskScheduler.UnobservedTaskException -= UnobservedTaskException;
            TaskScheduler.UnobservedTaskException += UnobservedTaskException;
        }

        [DebuggerHidden]
        public object StartCoroutine(IEnumerator routine)
        {
            return GetMonoScheduler().StartCoroutine(routine);
        }

        [DebuggerHidden]
        public void Post(Action action)
        {
            if (action == null)
                return;
            var current = GetDefault();
            //if (SynchronizationContext.Current == current.SynchronizationContext)
            //{
            //    action();
            //}
            //else
            //{
            current.SynchronizationContext.Post(state => action(), null);
            //}
        }

        [DebuggerHidden]
        public void Post(SendOrPostCallback action, object state)
        {
            if (action == null)
                return;
            var current = GetDefault();
            //if (SynchronizationContext.Current == current.SynchronizationContext)
            //{
            //    action(state);
            //}
            //else
            //{
            current.SynchronizationContext.Post(action, state);
            //}
        }

        [DebuggerHidden]
        internal static IThreadScheduler GetDefault()
        {
            var scheduler = ResolveScheduler?.Invoke(null);
            if (scheduler == null)
                scheduler = Instance;
            return scheduler;
        }

        [DebuggerHidden]
        internal static IThreadScheduler GetCurrentOrDefault()
        {
            return Current ?? GetDefault();
        }

        public void Send(Action action)
        {
            if (action == null)
                return;
            var current = GetDefault();
            if (SynchronizationContext.Current == current.SynchronizationContext)
            {
                action();
            }
            else
            {
                current.SynchronizationContext.Send(s => action(), null);
            }
        }

        public void Send(SendOrPostCallback action, object state)
        {
            if (action == null)
                return;
            var current = GetDefault();
            if (SynchronizationContext.Current == current.SynchronizationContext)
            {
                action(state);
            }
            else
            {
                current.SynchronizationContext.Send(s => action(s), state);
            }
        }
    }

}