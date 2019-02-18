using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleInjector.Internals
{
    internal sealed class ObjectGraphBuilder
    {
        private readonly StringBuilder builder = new StringBuilder(100);
        private readonly Stack<ProducerEntry> producers = new Stack<ProducerEntry>();
        private readonly bool writeLifestyles;

        private ProducerEntry stillToWriteLifestyleEntry;
        private int indentingDepth;

        public ObjectGraphBuilder(bool writeLifestyles)
        {
            this.writeLifestyles = writeLifestyles;
        }

        public override string ToString() => this.builder.ToString();

        internal void BeginInstanceProducer(InstanceProducer producer)
        {
            if (this.producers.Any())
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
            const string INDENT = "    ";
            for (int i = 0; i < indentingDepth; i++)
            {
                this.Append(INDENT);
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