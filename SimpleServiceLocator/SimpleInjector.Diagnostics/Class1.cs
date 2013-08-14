namespace SimpleInjector.Diagnostics
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public enum DiagnosticResultType
    {
        ContainerRegisteredService,
        PotentialLifestyleMismatch,
        ShortCircuitedDependency,
        SingleResponsibilityViolation
    }

    public class DiagnosticGroup
    {
        public Type GroupType { get; private set; }

        public string Name { get; private set; }

        public string Description { get; private set; }

        public DiagnosticResultType DiagnosticType { get; private set; }

        public DiagnosticGroup Parent { get; private set; }

        public IEnumerable<DiagnosticGroup> Children { get; private set; }

        public IEnumerable<DiagnosticResult> Results { get; private set; }
    }

    //var service = new DiagnosticServices(container);

    //DiagnosticResult[] diagnosticResults = service.Diagnose();

    //var warningsThatShouldNotShowUp =
    //    from result in diagnosticResults
    //    where result.GetType() != DiagnosticType.ContainerRegistered
    //    where !(result is ContainerRegisteredDiagnosticResult)
    //    select result;

    //var shorts = diagnosticResults.OfType<ShortCircuitDiagnosticResult>();



    //var rootGroups = diagnosticResults.Select(r => r.Group.Root()).Distinct();


    //Dictionary<string, object> dict = 
    //    rootGroups.ToDictionary(g => g.Name, g => BuildGroupDictionary(g))

	
    //object BuildGroupDictionary(DiagnosticGroup group)
    //{
    //    var children =
    //        from child in group.Children
    //        select new
    //        {
    //            Name = child.Name, Value = BuildGroupDictionary(child)
    //        };
	
    //    var results =
    //        from result in group.Results
    //        select new
    //        {
    //            Name = result.Name, Value = result
    //        };
		
    //    return children.Concat(results)
    //        .ToDictionary(i => i.Name, i => i.Value);
    //}

}
