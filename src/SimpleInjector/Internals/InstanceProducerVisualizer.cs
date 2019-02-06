#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2013 - 2015 Simple Injector Contributors
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
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using Advanced;

    internal static class InstanceProducerVisualizer
    {
        private const string ExpressionNotCreatedYetMessage = "{ Expression not created yet }";

        internal static string VisualizeIndentedObjectGraph(this InstanceProducer producer, VisualizationOptions options)
        {
            if (!producer.IsExpressionCreated)
            {
                return ExpressionNotCreatedYetMessage;
            }

            var set = new HashSet<InstanceProducer>(InstanceProducer.EqualityComparer);
            var outputBuilder = new StringBuilder();
            producer.VisualizeIndentedObjectGraph(indentingDepth: 0, lastDependency: true, set: set, options: options, outputBuilder: outputBuilder);
            return outputBuilder.ToString();
        }

        internal static string VisualizeInlinedAndTruncatedObjectGraph(this InstanceProducer producer,
            int maxLength)
        {
            if (!producer.IsExpressionCreated)
            {
                return ExpressionNotCreatedYetMessage;
            }

            string implementationName = producer.ImplementationType.ToFriendlyName();

            var visualizedDependencies =
                producer.VisualizeInlinedDependencies(maxLength - implementationName.Length - 2);

            return string.Format(CultureInfo.InvariantCulture, "{0}({1})",
                implementationName,
                string.Join(", ", visualizedDependencies));
        }

        private static void VisualizeIndentedObjectGraph(this InstanceProducer producer, int indentingDepth, bool lastDependency, HashSet<InstanceProducer> set, VisualizationOptions options, StringBuilder outputBuilder)
        {
            var indenting = new string(' ', indentingDepth * 4);

            outputBuilder
                .Append(indenting)
                .Append(producer.ImplementationType.ToFriendlyName())
                .Append("(");

            var dependencies = producer
                                .GetRelationships()
                                .Select(relationship => relationship.Dependency)
                                .ToList();

            var numberOfDependencies = dependencies.Count;
            if (numberOfDependencies > 0)
            {
                outputBuilder
                    .Append((options.IncludeLifestyleInformation) ? " // " + producer.Lifestyle.Name : string.Empty)
                    .AppendLine();

                for (int counter = 0; counter < numberOfDependencies; counter++)
                {
                    var dependency = dependencies[counter];
                    dependency.VisualizeIndentedObjectSubGraph(indentingDepth + 1, counter + 1 == numberOfDependencies, set, options, outputBuilder);
                }
                outputBuilder
                    .Append(indenting)
                    .Append(")")
                    .Append(!lastDependency ? "," : string.Empty);
            }
            else
            {
                outputBuilder
                    .Append(")")
                    .Append(!lastDependency ? "," : string.Empty)
                    .Append((options.IncludeLifestyleInformation) ? " // " + producer.Lifestyle.Name : string.Empty);
            }

            if (indentingDepth != 0)
            {
                outputBuilder.AppendLine();
            }
        }

        private static void VisualizeIndentedObjectSubGraph(this InstanceProducer dependency,
            int indentingDepth, bool isLastSibling, HashSet<InstanceProducer> set, VisualizationOptions options, StringBuilder outputBuilder)
        {
            bool isCyclicGraph = set.Contains(dependency);

            if (isCyclicGraph)
            {
                outputBuilder
                    .Append(dependency.VisualizeCyclicProducerWithoutDependencies(indentingDepth))
                    .AppendLine();
                return;
            }

            set.Add(dependency);

            try
            {
                dependency.VisualizeIndentedObjectGraph(indentingDepth, isLastSibling, set, options, outputBuilder);
            }
            finally
            {
                set.Remove(dependency);
            }
        }

        private static string VisualizeCyclicProducerWithoutDependencies(this InstanceProducer producer,
            int indentingDepth)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}{1}(/* cyclic dependency graph detected */)",
                new string(' ', indentingDepth * 4),
                producer.ImplementationType.ToFriendlyName());
        }

        private static IEnumerable<string> VisualizeInlinedDependencies(this InstanceProducer producer,
            int maxLength)
        {
            var relationships = new Stack<KnownRelationship>(producer.GetRelationships().Reverse());

            if (!relationships.Any())
            {
                yield break;
            }

            while (maxLength > 0 && relationships.Any())
            {
                var relationship = relationships.Pop();

                bool lastDependency = !relationships.Any();

                string childGraph = relationship.Dependency.VisualizeInlinedAndTruncatedObjectGraph(
                    !lastDependency ? maxLength - ", ...".Length : maxLength);

                maxLength -= childGraph.Length;

                bool displayingThisGraphWillCauseAnOverflow =
                    (!lastDependency && maxLength < ", ...".Length) || maxLength < 0;

                if (displayingThisGraphWillCauseAnOverflow)
                {
                    yield return "...";
                    yield break;
                }
                else
                {
                    yield return childGraph;
                }

                maxLength -= ", ".Length;
            }

            if (relationships.Any())
            {
                yield return "...";
            }
        }
    }
}