// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

#pragma warning disable RCS1194 // Implement exception constructors.
namespace SimpleInjector.Internals
{
    using System;
    using System.Collections.Generic;

    [Serializable]
    internal class CyclicDependencyException : ActivationException
    {
        private readonly List<Type> types = new(1);

        public CyclicDependencyException()
        {
            this.OriginatingProducer = null!;
        }

        public CyclicDependencyException(InstanceProducer originatingProducer, Type typeToValidate)
            : base(StringResources.TypeDependsOnItself(typeToValidate))
        {
            this.OriginatingProducer = originatingProducer;
            this.types.Add(typeToValidate);
        }

        public override string Message =>
            base.Message + " " + StringResources.CyclicDependencyGraphMessage(this.types);

        internal IEnumerable<Type> DependencyCycle => this.types;

        internal InstanceProducer OriginatingProducer { get; }

        internal void AddTypeToCycle(Type type)
        {
            this.types.Insert(0, type);
        }
    }
}