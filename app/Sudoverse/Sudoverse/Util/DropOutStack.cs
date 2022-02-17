using System;

namespace Sudoverse.Util
{
    /// <summary>
    /// A stack which drops the lowest element (which was pushed least recently of all current
    /// elements), if it gets too large.
    /// 
    /// Note that the stack may hold references internally, which are not released once an element
    /// is removed. So, memory allocated by its elements may linger for longer than they are truly
    /// accessible. This stack should only be used if the memory usage of a full stack is always
    /// acceptable, even if it is not currently used.
    /// </summary>
    /// <typeparam name="T">The type of the elements of the stack.</typeparam>
    public sealed class DropOutStack<T>
    {
        private T[] elements;
        private int top, bottom;

        /// <summary>
        /// Indicates whether this stack is empty.
        /// </summary>
        public bool Empty => top == bottom;

        /// <summary>
        /// Creates a new drop out stack that drops the oldest element if it exceeds the given
        /// <tt>capacity</tt>.
        /// </summary>
        /// <param name="capacity">The maximum number of elements of the created stack.</param>
        public DropOutStack(int capacity)
        {
            elements = new T[capacity + 1];
            top = 0;
            bottom = 0;
        }

        /// <summary>
        /// Pushes an element on top of the stack. If it is full, the oldest element is dropped.
        /// </summary>
        /// <param name="t">The element to push.</param>
        public void Push(T t)
        {
            elements[top] = t;
            top = (top + 1) % elements.Length;

            if (top == bottom) bottom = (bottom + 1) % elements.Length;
        }

        /// <summary>
        /// Pops the element on top of the stack, i.e. which was most recently pushed.
        /// </summary>
        /// <returns>The top element of the stack.</returns>
        /// <exception cref="InvalidOperationException">If the stack is empty.</exception>
        public T Pop()
        {
            if (top == bottom)
                throw new InvalidOperationException("drop out stack empty");

            top = (top + elements.Length - 1) % elements.Length;
            var result = elements[top];
            return result;
        }

        /// <summary>
        /// Removes all elements from this stack.
        /// </summary>
        public void Clear()
        {
            top = 0;
            bottom = 0;
        }
    }
}
