#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2019 Simple Injector Contributors
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and 
 * associated documentation files (the "Software"), to deal in the Software without restriction, including 
 * without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
 * copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the 
 * following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial 
 * portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT 
 * LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO 
 * EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER 
 * IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE 
 * USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

namespace SimpleInjector.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    internal sealed class ObjectGraphStringBuilder
    {
        private const int IndentSize = 4;

        private readonly StringBuilder builder = new StringBuilder();
        private readonly Stack<ProducerEntry> producers = new Stack<ProducerEntry>();
        private readonly bool writeLifestyles;

        private ProducerEntry stillToWriteLifestyleEntry;
        private int indentingDepth;

        public ObjectGraphStringBuilder(bool writeLifestyles)
        {
            this.writeLifestyles = writeLifestyles;
        }

        public override string ToString() => this.builder.ToString();

        internal void BeginInstanceProducer(InstanceProducer producer)
        {
            if (this.producers.Count > 0)
            {
                this.AppendLifestyle(this.producers.Peek());
                this.AppendNewLine();
            }

            this.producers.Push(new ProducerEntry(producer));

            this.Append(producer.ImplementationType.ToFriendlyName());
            this.Append("(");

            this.indentingDepth++;
        }

        internal void AppendCyclicInstanceProducer(InstanceProducer producer, bool last)
        {
            this.BeginInstanceProducer(producer);
            this.Append("/* cyclic dependency graph detected */");
            this.EndInstanceProducer(last);
        }

        internal void EndInstanceProducer(bool last)
        {
            var entry = this.producers.Pop();

            this.indentingDepth--;

            this.Append(")");

            if (!last)
            {
                this.Append(",");
                if (this.stillToWriteLifestyleEntry != null)
                {
                    this.AppendLifestyle(this.stillToWriteLifestyleEntry);
                    this.stillToWriteLifestyleEntry = null;
                }

                this.AppendLifestyle(entry);
            }

            if (!entry.LifestyleWritten)
            {
                this.stillToWriteLifestyleEntry = entry;
            }

            if (!this.producers.Any())
            {
                this.AppendLifestyle(this.stillToWriteLifestyleEntry);
            }
        }

        private void AppendNewLine()
        {
            this.Append(Environment.NewLine);
            this.AppendIndent();
        }

        private void AppendLifestyle(ProducerEntry entry)
        {
            if (this.writeLifestyles && !entry.LifestyleWritten)
            {
                this.Append(" // ");
                this.Append(entry.Producer.Lifestyle.Name);
                entry.LifestyleWritten = true;
            }
        }

        private void AppendIndent()
        {
            for (int i = 0; i < this.indentingDepth * IndentSize; i++)
            {
                this.builder.Append(' ');
            }
        }

        private void Append(string value) => this.builder.Append(value);

        private sealed class ProducerEntry
        {
            public ProducerEntry(InstanceProducer producer)
            {
                this.Producer = producer;
            }

            public InstanceProducer Producer { get; }
            public bool LifestyleWritten { get; set; }
        }
    }
}