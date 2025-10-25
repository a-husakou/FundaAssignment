namespace FundaAssignment.Application.Common;

public interface ICalculatedResultStore
{
    Task StoreMakelaarItemsAsync(string searchTerm, SortedList<int, MakelaarInfo> items);
    /// <summary>
    /// Retrieves the sorted calculated makelaar results for a given search term.
    /// </summary>
    Task<CalculatedMakelaarData?> GetCalculatedDataAsync(string searchTerm);
}

