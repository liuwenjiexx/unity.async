using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.EditorCoroutines.Editor;
using UnityEngine;

namespace Async.Editor
{
    public static partial class Extensions
    {
        private static EditorMainThreadAwaiter EditorMainThreadAwaiter = new EditorMainThreadAwaiter();


        [DebuggerHidden]
        public static IAwaiter GetAwaiter(this EditorMainThread waitForMainThread) => EditorMainThreadAwaiter;


        [DebuggerHidden]
        public static IAwaiter GetAwaiter(this EditorWaitForSeconds instruction)
        {
            return GetAwaiterReturnVoid(instruction);
        }

        private static IAwaiter GetAwaiterReturnVoid(object instruction)
        {
            var awaiter = new CoroutineAwaiter(MainThreadScheduler.GetDefault());
            var routine = StaticPool<InstructionRoutine>.Get();
            routine.Initalize(awaiter, instruction);
            awaiter.Initalize(routine);
            return awaiter;
        }
    }
}
