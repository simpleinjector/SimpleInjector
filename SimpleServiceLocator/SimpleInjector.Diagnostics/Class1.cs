namespace SimpleInjector.Diagnostics
{
    using System.Collections.Generic;
    using System.Linq;
    using SimpleInjector.Diagnostics.Analyzers;


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