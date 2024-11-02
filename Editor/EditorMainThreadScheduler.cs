using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Async.Editor
{
    class EditorMainThreadScheduler : IThreadScheduler
    {
        private static bool isPlaying;
        private static int mainThreadId;
        private static SynchronizationContext synchronizationContext;
        private static EditorMainThreadScheduler instance;

        public static EditorMainThreadScheduler Instance
        {
            get
            {
                if (instance == null)
                    instance = new EditorMainThreadScheduler();
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

        [InitializeOnLoadMethod]
        static void InitializeOnLoadMethod()
        {
            synchronizationContext = SynchronizationContext.Current;
            mainThreadId = Thread.CurrentThread.ManagedThreadId;
            MainThreadScheduler.ResolveScheduler = ResolveScheduler;
            EditorApplication.playModeStateChanged += EditorApplication_playModeStateChanged;

            TaskScheduler.UnobservedTaskException -= MainThreadScheduler.UnobservedTaskException;
            TaskScheduler.UnobservedTaskException += MainThreadScheduler.UnobservedTaskException;
        }

        [RuntimeInitializeOnLoadMethod]
        static void RuntimeInitializeOnLoadMethod()
        {
            isPlaying = true;
        }

        private static void EditorApplication_playModeStateChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.ExitingPlayMode:
                    isPlaying = false;
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    isPlaying = true;
                    break;
            }

        }

        [DebuggerHidden]
        static IThreadScheduler ResolveScheduler(object o)
        {
            //子线程不能使用 Application.isPlaying 判断
            if (!isPlaying)
                return Instance;
            return null;
        }

        [DebuggerHidden]
        public object StartCoroutine(IEnumerator routine)
        {
            return EditorCoroutineUtility.StartCoroutine(routine, routine);
        }

        [DebuggerHidden]
        public void Post(Action action)
        {
            if (action == null)
                return;
            SynchronizationContext.Post(s => action(), null);
        }

        [DebuggerHidden]
        public void Post(SendOrPostCallback action, object state)
        {
            if (action == null)
                return;

            SynchronizationContext.Post(action, state);
        }
        [DebuggerHidden]
        public void Send(Action action)
        {
            if (action == null)
                return;
            if (SynchronizationContext.Current == SynchronizationContext)
            {
                action();
            }
            else
            {
                SynchronizationContext.Post(state => action(), null);
            }
        }

        [DebuggerHidden]
        public void Send(SendOrPostCallback action, object state)
        {
            if (action == null)
                return;
            if (SynchronizationContext.Current == SynchronizationContext)
            {
                action(state);
            }
            else
            {
                SynchronizationContext.Post(action, state);
            }
        }
    }

}