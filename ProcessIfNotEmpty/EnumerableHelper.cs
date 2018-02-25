using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace ProcessIfNotEmpty
{
    public static class EnumerableHelper
    {
        public static TRes ProcessIfNotEmpty<T, TRes>(this IEnumerable<T> source, Func<IEnumerable<T>, TRes> handler, Func<TRes> defaultValue)
        {
            if (handler == null) throw new ArgumentNullException(paramName: nameof(handler));
            if (defaultValue == null) throw new ArgumentNullException(paramName: nameof(defaultValue));

            switch (source)
            {
                case null: return defaultValue();
                case IReadOnlyCollection<T> collection:
                    return collection.Count > 0 ? handler(collection) : defaultValue();
                default:
                    using (var enumerator = new DisposeGuardWrapper<T>(source.GetEnumerator()))
                    {
                        if (enumerator.MoveNext())
                        {
                            return handler(Continue(enumerator.Current, enumerator));
                        }

                    }
                    return defaultValue();
            }
        }

        private static IEnumerable<T> Continue<T>(T first, IEnumerator<T> startedEnumerator)
        {
            yield return first;
            while (startedEnumerator.MoveNext())
            {
                yield return startedEnumerator.Current;
            }
        }

        private class DisposeGuardWrapper<T> : IEnumerator<T>
        {
            private int _disposed;

            private readonly IEnumerator<T> _target;

            private void CheckForDisposing()
            {
                if (Interlocked.CompareExchange(ref this._disposed, 0, 0) == 1)
                {
                    throw new Exception($"Cannot use an already started enumerator out of a scope of {nameof(ProcessIfNotEmpty)}. Consider using a real list");
                }
            }

            public DisposeGuardWrapper(IEnumerator<T> target)
            {
                this.CheckForDisposing();
                this._target = target;
            }

            public void Dispose()
            {
                if (Interlocked.Exchange(ref this._disposed, 1) == 0)
                    this._target.Dispose();
            }

            public bool MoveNext()
            {
                this.CheckForDisposing();
                return this._target.MoveNext();
            }

            public void Reset()
            {
                this.CheckForDisposing();
                this._target.Reset();
            }

            public T Current
            {
                get
                {
                    this.CheckForDisposing();
                    return this._target.Current;
                }
            }

            object IEnumerator.Current => this.Current;
        }
    }
}