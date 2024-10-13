using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Unity.Async
{

    class CoroutineWrapper<T>
    {
        readonly CoroutineAwaiter<T> awaiter;
        readonly Stack<IEnumerator> processStack;

        public CoroutineWrapper(IEnumerator coroutine, CoroutineAwaiter<T> awaiter)
        {
            processStack = new Stack<IEnumerator>();
            processStack.Push(coroutine);
            this.awaiter = awaiter;
        }

        [DebuggerHidden]
        public IEnumerator Run()
        {
            object current;
            bool isDone;
            while (true)
            {
                var topWorker = processStack.Peek();

                isDone = false;
                current = null;

                try
                {
                    isDone = !topWorker.MoveNext();
                }
                catch (Exception e)
                {

                    var objectTrace = GenerateObjectTrace(processStack);

                    if (objectTrace.Any())
                    {
                        awaiter.Complete(default, new Exception(GenerateObjectTraceMessage(objectTrace), e));
                    }
                    else
                    {
                        awaiter.Complete(default, e);
                    }

                    break;
                }

                if (isDone)
                {
                    processStack.Pop();

                    if (processStack.Count == 0)
                    {
                        awaiter.Complete((T)topWorker.Current, null);
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                current = topWorker.Current;
                if (current == null)
                {
                    yield return null;
                }
                else if (current is IEnumerator)
                {
                    processStack.Push((IEnumerator)current);
                }
                else if (current is YieldReturn)
                {
                    if (processStack.Count == 1)
                    {
                        var ret = (YieldReturn)current;

                        awaiter.Complete((T)ret.Value, null);
                        break;
                    }
                    else
                    {
                        yield return null;
                    }
                }
                else if (current is Task)
                {
                    yield return ((Task)current).AsRoutine();
                }
                else
                {
                    yield return current;
                }
            }
        }

        string GenerateObjectTraceMessage(List<Type> objTrace)
        {
            var result = new StringBuilder();

            foreach (var objType in objTrace)
            {
                if (result.Length != 0)
                {
                    result.Append(" -> ");
                }

                result.Append(objType.ToString());
            }

            result.AppendLine();
            return "Unity Coroutine Object Trace: " + result.ToString();
        }

        static List<Type> GenerateObjectTrace(IEnumerable<IEnumerator> enumerators)
        {
            var objTrace = new List<Type>();

            foreach (var enumerator in enumerators)
            {
                var field = enumerator.GetType().GetField("$this", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

                if (field == null)
                {
                    continue;
                }

                var obj = field.GetValue(enumerator);

                if (obj == null)
                {
                    continue;
                }

                var objType = obj.GetType();

                if (!objTrace.Any() || objType != objTrace.Last())
                {
                    objTrace.Add(objType);
                }
            }

            objTrace.Reverse();
            return objTrace;
        }
    }
}
