using System;

namespace Sudoverse.Util
{
    public sealed class DropOutStack<T>
    {
        private T[] elements;
        private int top, bottom;

        public bool Empty => top == bottom;

        public DropOutStack(int capacity)
        {
            elements = new T[capacity + 1];
            top = 0;
            bottom = 0;
        }

        public void Push(T t)
        {
            elements[top] = t;
            top = (top + 1) % elements.Length;

            if (top == bottom) bottom = (bottom + 1) % elements.Length;
        }

        public T Pop()
        {
            if (top == bottom)
                throw new InvalidOperationException("drop out stack empty");

            top = (top + elements.Length - 1) % elements.Length;
            var result = elements[top];
            return result;
        }

        public void Clear()
        {
            top = 0;
            bottom = 0;
        }
    }
}
