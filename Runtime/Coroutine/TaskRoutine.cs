using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Async
{
    class TaskRoutine : IEnumerator
    {
        private Task task;
        private bool isDone;

        public object Current => null;

        public Task Task
        {
            get => task;
        }

        public void Initalize(Task task)
        {
            this.task = task;
            isDone = false;
        }


        public bool MoveNext()
        {
            if (isDone)
                return false;
            if (task.IsCompleted)
            {
                var ex = task.Exception;
                isDone = true;
                StaticPool<TaskRoutine>.Release(this);
                if (ex != null)
                    throw ex;
                return false;
            }
            return true;
        }

        public void Reset()
        {

        }

        class PoolProvider : StaticPool<TaskRoutine>.IProvider
        {
            public int Priority => 0;

            public TaskRoutine Create()
            {
                return new TaskRoutine();
            }

            public void OnRelease(TaskRoutine target)
            {
                target.task = null;
            }
        }
    }


}
