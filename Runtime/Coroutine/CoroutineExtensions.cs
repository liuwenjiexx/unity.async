using System.Collections;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;


namespace Async
{
    public static partial class Extensions
    {

        public static IEnumerator AsRoutine(this Task task)
        {
            var routine = StaticPool<TaskRoutine>.Get();
            routine.Initalize(task);
            return routine;
        }

        [DebuggerHidden]
        public static async Task Await(this IEnumerator routine)
        {
            await routine;
        }


        [DebuggerHidden]
        public static async Task<T> Await<T>(this IEnumerator routine)
        {
            T result = (T)await routine;
            return result;
        }

        [DebuggerHidden]
        public static IAwaiter<object> GetAwaiter(this IEnumerator coroutine)
        {
            var awaiter = new CoroutineAwaiter<object>(MainThreadScheduler.GetDefault());

            var routine = new CoroutineWrapper<object>(coroutine, awaiter).Run();
            awaiter.Initalize(routine);

            return awaiter;
        }

        [DebuggerHidden]
        public static IAwaiter GetAwaiter(this YieldInstruction instruction)
        {
            return GetAwaiterReturnVoid(instruction);
        }

        [DebuggerHidden]
        public static IAwaiter GetAwaiter(this CustomYieldInstruction instruction)
        {
            //var waitable = StaticPool<CustomYieldInstructionWaitable>.Get();
            //waitable.Initialize(instruction);
            //return new Awaiter2(waitable, MainThreadScheduler.GetDefault());
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