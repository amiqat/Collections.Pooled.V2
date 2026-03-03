// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;

namespace Collections.Pooled
{
    /// <summary>
    /// Equality comparer for hashsets of hashsets
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal sealed class PooledSetEqualityComparer<T> : IEqualityComparer<PooledSet<T>>
    {
        public bool Equals(PooledSet<T> x, PooledSet<T> y)
            => PooledSet<T>.PooledSetEquals(x, y, EqualityComparer<T>.Default);

        public int GetHashCode(PooledSet<T> obj)
        {
            int hashCode = 0; // default to 0 for null/empty set

            if (obj != null)
            {
                foreach (T t in obj)
                {
                    if (t != null)
                    {
                        hashCode ^= t.GetHashCode(); // same hashcode as default comparer
                    }
                }
            }

            return hashCode;
        }

        // Equals method for the comparer itself.
        public override bool Equals(object obj) => obj is PooledSetEqualityComparer<T>;

        public override int GetHashCode() => EqualityComparer<T>.Default.GetHashCode();
    }
}

