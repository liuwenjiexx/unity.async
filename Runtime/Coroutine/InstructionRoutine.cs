using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Async
{
    class InstructionRoutine : IEnumerator
    {
        private CoroutineAwaiter awaiter;
        private object instruction;
        private int state;

        public object Current { get => instruction; }

        public bool MoveNext()
        {
            state++;
            if (state == 2)
            {
                awaiter.Complete(null);
                StaticPool<InstructionRoutine>.Release(this);
                return false;
            }
            return true;
        }

        public void Initalize(CoroutineAwaiter awaiter, object instruction)
        {
            this.awaiter = awaiter;
            this.instruction = instruction;
            state = 0;  
        }
        public void Reset()
        {
        }

        class PoolProvider : StaticPool<InstructionRoutine>.IProvider
        {
            public int Priority => 0;

            public InstructionRoutine Create()
            {
                return new InstructionRoutine();
            }

            public void OnRelease(InstructionRoutine target)
            {
                target.instruction = null;
                target.awaiter = null;
            }
        }
    }


}
