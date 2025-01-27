using System;
using System.Diagnostics;

namespace Async
{
    //public enum SwitchThread
    //{
    //    Main,
    //    Sub,
    //}

    public sealed class SwitchThread
    {
        public static readonly IAwaitable Main = new MainThreadAwaiter();

        public static readonly IAwaitable Sub = new SubThreadAwaiter();

        private static IAwaitable editorMain;

#if UNITY_EDITOR
        public static IAwaitable EditorMain
        {
            get
            {
                if (editorMain == null)
                {
                    Type type = Type.GetType("Async.Editor.EditorMainThreadAwaiter,Async.Editor");
                    Debug.Assert(type != null, "EditorMainThreadAwaiter Type null");
                    editorMain = Activator.CreateInstance(type) as IAwaitable;
                }
                return editorMain;
            }
        }
#endif
    }

}
