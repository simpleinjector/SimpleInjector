// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Internals
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Advanced;

    internal static class InstanceProducerVisualizer
    {
        private const string ExpressionNotCreatedYetMessage = "{ Expression not created yet }";

        internal static string VisualizeIndentedObjectGraph(
            this InstanceProducer producer, VisualizationOptions options)
        {
            if (!producer.IsExpressionCreated)
            {
                return ExpressionNotCreatedYetMessage;
            }

            var set = new HashSet<InstanceProducer>(InstanceProducer.EqualityComparer);
            var objectGraphBuilder = new ObjectGraphStringBuilder(options);

            producer.VisualizeIndentedObjectGraph(
                indentingDepth: 0, last: true, set: set, objectGraphBuilder: objectGraphBuilder);

            return objectGraphBuilder.ToString();
        }

        internal static string VisualizeInlinedAndTruncatedObjectGraph(
            this InstanceProducer producer, int maxLength)
        {
            if (!producer.IsExpressionCreated)
            {
                return ExpressionNotCreatedYetMessage;
            }

            string implementationName = producer.ImplementationType.ToFriendlyName();

            var visualizedDependencies =
                producer.VisualizeInlinedDependencies(maxLength - implementationName.Length - 2);

            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}({1})",
                implementationName,
                string.Join(", ", visualizedDependencies));
        }

        private static void VisualizeIndentedObjectGraph(
            this InstanceProducer producer,
            int indentingDepth,
            bool last,
            HashSet<InstanceProducer> set,
            ObjectGraphStringBuilder objectGraphBuilder)
        {
            objectGraphBuilder.BeginInstanceProducer(producer);

            var dependencies = producer
                .GetRelationships()
                .Select(relationship => relationship.Dependency)
                .ToList();

            for (int counter = 0; counter < dependencies.Count; counter++)
            {
                var dependency = dependencies[counter];
                dependency.VisualizeIndentedObjectSubGraph(
                    indentingDepth + 1, counter + 1 == dependencies.Count, set, objectGraphBuilder);
            }

            objectGraphBuilder.EndInstanceProducer(last);
        }

        private static void VisualizeIndentedObjectSubGraph(
            this InstanceProducer dependency,
            int indentingDepth,
            bool last,
            HashSet<InstanceProducer> set,
            ObjectGraphStringBuilder objectGraphBuilder)
        {
            bool isCyclicGraph = set.Contains(dependency);

            if (isCyclicGraph)
            {
                objectGraphBuilder.AppendCyclicInstanceProducer(dependency, last);
                return;
            }

            set.Add(dependency);

            try
            {
                dependency.VisualizeIndentedObjectGraph(indentingDepth, last, set, objectGraphBuilder);
            }
            finally
            {
                set.Remove(dependency);
            }
        }

        private static IEnumerable<string> VisualizeInlinedDependencies(
            this InstanceProducer producer, int maxLength)
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