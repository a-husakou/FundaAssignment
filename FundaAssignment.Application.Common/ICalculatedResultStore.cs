namespace FundaAssignment.Application.Common;

public interface ICalculatedResultStore
{
    Task StoreCalculatedResultAsync(string searchTerm, SortedList<int, MakelaarInfo> result);
    /// <summary>
    /// Retrieves the sorted calculated makelaar results for a given search term.
    /// </summary>
    Task<IEnumerable<CalculatedMakelaarResult>?> GetCalculatedResultAsync(string searchTerm);
}

