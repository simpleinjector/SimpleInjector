﻿namespace SimpleInjector.CodeSamples
{
    using System;
    using System.Collections.Generic;

    using SimpleInjector.Advanced;

    public class NamedContainer : Container
    {
        private readonly Dictionary<Type, Dictionary<string, InstanceProducer>> namedProducers =
            new Dictionary<Type, Dictionary<string, InstanceProducer>>();

        public TService GetInstance<TService>(string name)
            where TService : class
        {
            return (TService)this.GetInstance(typeof(TService), name);
        }

        public object GetInstance(Type serviceType, string name)
        {
            return this.namedProducers[serviceType][name].GetInstance();
        }

        public void Register<TService, TImplementation>(string name)
            where TImplementation : class, TService
            where TService : class
        {
            this.Register<TService, TImplementation>(this.SelectLifestyle<TService, TImplementation>(), name);
        }

        public void Register<TService, TImplementation>(Lifestyle lifestyle, string name)
            where TImplementation : class, TService
            where TService : class
        {
            var reg = lifestyle.CreateRegistration<TService, TImplementation>(this);
            this.AddRegistration(typeof(TService), reg, name);
        }

        public void Register(Type serviceType, Type implementationType, string name)
        {
            this.Register(serviceType, implementationType, this.SelectLifestyle(serviceType, implementationType), name);
        }
        
        public void Register(Type serviceType, Type implementationType, Lifestyle lifestyle, string name)
        {
            var reg = lifestyle.CreateRegistration(serviceType, implementationType, this);
            this.AddRegistration(serviceType, reg, name);
        }

        public void Register<TService>(Func<TService> instanceCreator, string name)
            where TService : class
        {
            this.Register<TService>(instanceCreator, this.SelectLifestyle(typeof(TService), typeof(TService)), name);
        }

        public void Register<TService>(Func<TService> instanceCreator, Lifestyle lifestyle, string name)
            where TService : class
        {
            var reg = lifestyle.CreateRegistration<TService>(instanceCreator, this);
            this.AddRegistration(typeof(TService), reg, name);
        }

        public void AddRegistration(Type serviceType, Registration registration, string name)
        {
            if (this.IsLocked())
            {
                throw new InvalidOperationException("The container is locked.");
            }

            Dictionary<string, InstanceProducer> dict;

            if (!this.namedProducers.TryGetValue(serviceType, out dict))
            {
                dict = new Dictionary<string, InstanceProducer>();
                this.namedProducers[serviceType] = dict;
            }

            if (this.Options.AllowOverridingRegistrations)
            {
                dict[name] = new InstanceProducer(serviceType, registration);
            }
            else
            {
                dict.Add(name, new InstanceProducer(serviceType, registration));
            }
        }

        public new void Verify()
        {
            base.Verify();

            this.VerifyNamedRegistrations();
        }

        private void VerifyNamedRegistrations()
        {
            foreach (var producersForOneServiceType in this.namedProducers.Values)
            {
                foreach (var producer in producersForOneServiceType.Values)
                {
                    producer.GetInstance();
                }
            }
        }

        private Lifestyle SelectLifestyle<TService, TImplementation>()
        {
            return this.SelectLifestyle(typeof(TService), typeof(TImplementation));
        }

        private Lifestyle SelectLifestyle(Type serviceType, Type implementationType)
        {
            return this.Options.LifestyleSelectionBehavior.SelectLifestyle(serviceType, implementationType);
        }
    }
}