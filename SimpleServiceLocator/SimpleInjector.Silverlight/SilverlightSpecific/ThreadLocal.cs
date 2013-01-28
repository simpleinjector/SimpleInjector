namespace SimpleInjector.Advanced
{
    // Quite painful but this object must be a public non-generic type to prevent some nasty exception to be 
    // thrown when Activator.CreateInstance tries to create an instance of this type from within the
    // ConditionalWeakTable<TKey, TValue>.
    public class ThreadLocalValue
    {
        public object Value { get; set; }
    }
}

namespace System.Threading
{
    using System;
    using System.Runtime.CompilerServices;
    using SimpleInjector.Advanced;

    // Silverlight lacks a ThreadLocal<T>.
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