using System;
using System.Linq;
using System.Reflection;

namespace SimpleInjector.CodeSamples
{
    public class OuterScopedLifestyle : ScopedLifestyle
    {
        private readonly PropertyInfo parentScopeProperty =
            typeof(Scope).GetProperties(BindingFlags.Instance | BindingFlags.NonPublic)
            .Single(p => p.Name == "ParentScope");

        public OuterScopedLifestyle() : base("Outer Scoped")
        {
        }

        public override int Length => Lifestyle.Scoped.Length + 1;

        protected override Func<Scope> CreateCurrentScopeProvider(Container container) =>
            () => this.GetOuterScope(Lifestyle.Scoped.GetCurrentScope(container));

        protected override Scope GetCurrentScopeCore(Container container) =>
            this.GetOuterScope(Lifestyle.Scoped.GetCurrentScope(container));

        private Scope GetOuterScope(Scope scope)
        {
            if (scope == null) return null;

            var parentScope = this.GetParentScope(scope);

            while (parentScope != null)
            {
                scope = parentScope;
                parentScope = this.GetParentScope(scope);
            }

            return scope;
        }

        private Scope GetParentScope(Scope scope) => (Scope)this.parentScopeProperty.GetValue(scope);
    }
}