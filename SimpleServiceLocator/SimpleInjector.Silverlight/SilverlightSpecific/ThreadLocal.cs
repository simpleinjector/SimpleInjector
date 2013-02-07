#pragma warning disable 0618
namespace System.Threading
{
    using System;
    using System.Runtime.CompilerServices;
    using SimpleInjector.Advanced;

    // NOTE: Silverlight lacks a ThreadLocal<T>.
    // Source: http://ayende.com/blog/4825 (but fixed to actually work in Silverlight)
    internal sealed class ThreadLocal<T>
    {
        [ThreadStatic]
        private static ConditionalWeakTable<object, ThreadLocalValue> threadStaticWeakTable;

        private readonly Func<T> valueCreator;

        public ThreadLocal() : this(() => default(T))
        {
        }

        public ThreadLocal(Func<T> valueCreator)
        {
            this.valueCreator = valueCreator;
        }

        public T Value
        {
            get
            {
                ThreadLocalValue threadLocalValue;

                var weakTable = threadStaticWeakTable;
                
                if (weakTable == null || weakTable.TryGetValue(this, out threadLocalValue) == false)
                {
                    var value = this.valueCreator();
                    this.Value = value;
                    return value;
                }

                return (T)threadLocalValue.Value;
            }

            set
            {
                var weakTable = threadStaticWeakTable;

                if (weakTable == null)
                {
                    threadStaticWeakTable = weakTable = new ConditionalWeakTable<object, ThreadLocalValue>();
                }

                weakTable.GetOrCreateValue(this).Value = value;
            }
        }
    }
}
#pragma warning restore 0612