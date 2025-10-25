namespace FundaAssignment.Application.Common
{
    public class CalculatedMakelaarData
    {
        public IEnumerable<CalculatedMakelaarItem> DescendingSortedItems { get; set; }
        public DateTime CalculatedAtUtc { get; set; }
    }
}