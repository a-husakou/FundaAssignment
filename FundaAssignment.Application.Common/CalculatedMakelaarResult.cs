namespace FundaAssignment.Application.Common
{
    public class CalculatedMakelaarResult
    {
        public MakelaarInfo Makelaar { get; set; }
        public int TotalListings { get; set; }
        public DateTime CalculatedAtUtc { get; set; }
    }
}

