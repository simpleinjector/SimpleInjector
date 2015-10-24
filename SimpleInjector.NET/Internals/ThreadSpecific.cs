#pragma warning disable 0618
namespace SimpleInjector.Internals
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    using SimpleInjector.Advanced;

    // This is a custom implementation of ThreadLocal<T>. Custom because Silverlight lacks a ThreadLocal<T> 
    // and ThreadLocal<T> contains a memory leak in .NET 4.5 https://stackoverflow.com/questions/33172615/.
    // Source: http://ayende.com/blog/4825 (but fixed to actually work in Silverlight)
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal sealed class ThreadSpecific<T>
    {
        [ThreadStatic]
        private static ConditionalWeakTable<object, ThreadSpecificValue> threadStaticWeakTable;

        public T Value
        {
            get
            {
                ThreadSpecificValue threadLocalValue;

                var weakTable = threadStaticWeakTable;

                if (weakTable == null || weakTable.TryGetValue(this, out threadLocalValue) == false)
                {
                    return this.Value = default(T);
                }

                return (T)threadLocalValue.Value;
            }

            set
            {
                var weakTable = threadStaticWeakTable;

                if (weakTable == null)
                {
                    threadStaticWeakTable = weakTable = new ConditionalWeakTable<object, ThreadSpecificValue>();
                }

                weakTable.GetOrCreateValue(this).Value = value;
            }
        }
    }
}
#pragma warning restore 0612