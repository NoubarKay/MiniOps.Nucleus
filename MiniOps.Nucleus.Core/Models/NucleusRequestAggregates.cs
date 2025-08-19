namespace Nucleus.Core.Models;

public class NucleusRequestAggregates
{
    public DateTime BucketTime { get; set; }
    public int TotalRequests { get; set; } = 0;
    public int SuccessRequests { get; set; } = 0;
    public int FailedRequests { get; set; } = 0;
}