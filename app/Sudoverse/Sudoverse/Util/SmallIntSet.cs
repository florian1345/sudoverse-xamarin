using System;
using System.Collections;
using System.Collections.Generic;

namespace Sudoverse.Util
{
    /// <summary>
    /// A set of integers less than 32.
    /// </summary>
    public sealed class SmallIntSet
    {
        public int Data { get; private set; }

        public SmallIntSet()
            : this(0) { }

        public SmallIntSet(int data)
        {
            Data = data;
        }

        public bool Contains(int value) => (Data & (1 << value)) > 0;

        public void Add(int value)
        {
            Data |= 1 << value;
        }

        public void Remove(int value)
        {
            Data &= ~(1 << value);
        }

        public void Toggle(int value)
        {
            Data ^= 1 << value;
        }

        public void Clear()
        {
            Data = 0;
        }
    }
}
