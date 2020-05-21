// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

#pragma warning disable RCS1194 // Implement exception constructors.
namespace SimpleInjector.Internals
{
    using System;
    using System.Collections.Generic;

#if NET45 || NET461
    using System.Runtime.Serialization;

    [Serializable]
#endif
    internal class CyclicDependencyException : ActivationException
    {
        private readonly List<Type> types = new List<Type>(1);

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

#if NET45 || NET461
        protected CyclicDependencyException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.OriginatingProducer = null!;
        }
#endif

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