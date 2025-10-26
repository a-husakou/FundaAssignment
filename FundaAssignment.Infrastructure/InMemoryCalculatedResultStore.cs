using System.Collections.Concurrent;
using FundaAssignment.Application.Common;

namespace FundaAssignment.Infrastructure;

public class InMemoryCalculatedResultStore : ICalculatedResultStore
{
    private readonly ConcurrentDictionary<string, CalculatedMakelaarData> store = new(StringComparer.OrdinalIgnoreCase);

    public Task StoreMakelaarItemsAsync(string searchTerm, SortedList<int, List<MakelaarInfo>> items)
    {
        // items are already sorted in the desired order by the provided comparer
        var descending = new List<CalculatedMakelaarItem>();
        for (int i = 0; i < items.Count; i++)
        {
            var total = items.Keys[i];
            var makelaars = items.Values[i];
            foreach (var makelaar in makelaars)
            {
                descending.Add(new CalculatedMakelaarItem
                {
                    Makelaar = makelaar,
                    TotalListings = total
                });
            }
        }

        var data = new CalculatedMakelaarData
        {
            DescendingSortedItems = descending,
            CalculatedAtUtc = DateTime.UtcNow
        };

        store[searchTerm] = data;
        return Task.CompletedTask;
    }

    public Task<CalculatedMakelaarData?> GetCalculatedDataAsync(string searchTerm)
    {
        store.TryGetValue(searchTerm, out var data);
        return Task.FromResult<CalculatedMakelaarData?>(data);
    }
}
