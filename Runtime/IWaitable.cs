using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Async
{
    public interface IWaitable
    {

        bool IsDone { get; }

    }


    public interface IWaitable<T> : IWaitable
    {
        T GetResult();
    }

    public interface ICancelable
    {
        CancellationToken CancellationToken { get; }
    }

    public interface IReusable
    {
        void Unused();
    }

    class CustomYieldInstructionWaitable : IWaitable, IReusable
    {
        private CustomYieldInstruction instruction;
        private bool isDone;

        public bool IsDone
        {
            get
            {
                if (!isDone)
                {
                    isDone = !instruction.keepWaiting;
                }
                return isDone;
            }
        }

        public void Initialize(CustomYieldInstruction instruction)
        {
            isDone = false;
            this.instruction = instruction;
        }

        public void Unused()
        {
            instruction = null;
            isDone = false;
            StaticPool<CustomYieldInstructionWaitable>.Release(this);
        }
    }
}
