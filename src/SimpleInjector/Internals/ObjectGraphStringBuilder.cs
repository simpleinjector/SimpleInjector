// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

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
        private readonly VisualizationOptions visualizationOptions;

        private ProducerEntry stillToWriteLifestyleEntry;
        private int indentingDepth;

        public ObjectGraphStringBuilder(VisualizationOptions visualizationOptions)
        {
            this.visualizationOptions = visualizationOptions;
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

            this.Append(producer.ImplementationType.ToFriendlyName(this.visualizationOptions.UseFullyQualifiedTypeNames));
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
            if (this.visualizationOptions.IncludeLifestyleInformation && !entry.LifestyleWritten)
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