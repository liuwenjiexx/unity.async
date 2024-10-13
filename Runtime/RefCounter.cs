using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Unity.Async
{
    public class RefCounter
    {
        private int count;

        Pool<Ref> pool;

        public RefCounter()
        {
            pool = new Pool<Ref>(new RefFactory(this));
        }

        public int Count => count;

        public IDisposable Use()
        {
            return Use(out var n);
        }

        public IDisposable Use(out int count)
        {
            Ref obj = pool.Get();
            count = obj.count;
            return obj;
        }


        public static implicit operator int(RefCounter value) => value.Count;


        public override string ToString()
        {
            return count.ToString();
        }

        class RefFactory : Pool<Ref>.IFactory
        {
            private RefCounter owner;
            public RefFactory(RefCounter owner)
            {
                this.owner = owner;
            }

            public Ref Create()
            {
                Ref obj = new Ref(owner);
                return obj;
            }

            public void OnUse(Ref target)
            {
                target.Initilize();
            }

            public void OnRelease(Ref target)
            {

            }

        }

        class Ref : IDisposable
        {
            private RefCounter owner;
            private int state;
            public int count;

            private const int STATE_INITILIZE = 0;
            private const int STATE_DISPOSED = 1;

            public Ref(RefCounter owner)
            {
                this.owner = owner;
                state = STATE_DISPOSED;
            }

            public int Initilize()
            {
                if (Interlocked.CompareExchange(ref state, STATE_INITILIZE, STATE_DISPOSED) != STATE_DISPOSED)
                    throw new Exception();
                count = Interlocked.Increment(ref owner.count);
                return count;
            }

            public void Dispose()
            {
                if (state == STATE_DISPOSED) return;

                if (Interlocked.CompareExchange(ref state, STATE_DISPOSED, STATE_INITILIZE) == STATE_INITILIZE)
                {
                    Interlocked.Decrement(ref owner.count);
                    owner.pool.Release(this);
                }
            }

            ~Ref()
            {
                Dispose();
            }
        }

    }
}