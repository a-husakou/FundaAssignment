namespace FundaAssignment.TrendingMakelaarApi.App;

/// <summary>
/// Configuration for supported Funda search terms.
/// Since there is no much information about the use cases and future extended filtering support
/// No framework for filtering is implemented that will accept a structured input and produce search terms. 
/// With only 2 supported use-cases, the responsibility of mapping user input to these search terms lies with the caller.
/// </summary>
public class FilterConfig
{
    // TODO move to appsettings
    public HashSet<string> FilterSearchTerms { get; } = new HashSet<string> { "/amsterdam", "/amsterdam/tuin" };
}