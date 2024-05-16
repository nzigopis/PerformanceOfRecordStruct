namespace PerformanceOfRecordStruct;

public class RunQueriesServiceWithClass()
{
    public static IDictionary<string, RunAggregatesForInstrumentClass> GetAggregatesFromDailyView(
        IQueryable<RunAggregates> aggregatesQuery)
    {
        var result = GetByInstrumentId(aggregatesQuery).
            GroupBy(a => a.InstrumentId).
            ToDictionary(g => g.Key, g => g.First());

        return result;
    }

    public static IEnumerable<RunAggregatesForInstrumentClass> GetAggregatesFromMonthlyView(
        IQueryable<RunAggregates> monthlyAggregatesQuery,
        IQueryable<RunAggregates> excessDailyAtStartQuery,
        IQueryable<RunAggregates> excessDailyAtEndQuery)
    {

        var monthlyAggregates = GetByInstrumentId(monthlyAggregatesQuery);

        var excessDailyAggregatesAtStart =
            GetByInstrumentId(excessDailyAtStartQuery).
            ToLookup(a => a.InstrumentId);

        var excessDailyAggregatesAtEnd =
            GetByInstrumentId(excessDailyAtEndQuery).
            ToLookup(a => a.InstrumentId);

        var resultList = monthlyAggregates.Select(ma =>
        {
            ma.Subtract(excessDailyAggregatesAtStart[ma.InstrumentId].FirstOrDefault());
            ma.Subtract(excessDailyAggregatesAtEnd[ma.InstrumentId].FirstOrDefault());
            return ma;
        }).
        ToList();

        return resultList;
    }

    private static List<RunAggregatesForInstrumentClass> GetByInstrumentId(IQueryable<RunAggregates> aggregatesQuery)
    {
        return aggregatesQuery.
            GroupBy(i => i.InstrumentId).
            Select(g => new RunAggregatesForInstrumentClass(
                g.Key,
                g.Sum(r => r.CompletedSuccessfullyCount + r.CompletedWithWarningsCount + r.CompletedWithErrorsCount),
                g.Sum(r => r.CompletedSuccessfullyCount + r.CompletedWithWarningsCount),
                g.Sum(r => r.CompletedWithErrorsCount),
                g.Sum(r => r.ActiveTimeInMilliseconds),
                g.Sum(r => r.NVATimeInMilliseconds),
                g.Sum(r => r.ActiveTimeInMilliseconds) + g.Sum(r => r.NVATimeInMilliseconds)
                )
            ).ToList();
    }
}

public record RunAggregatesForInstrumentClass
{

    public string InstrumentId { get; set; }
    public long TotalRuns { get; set; }
    public int SuccessfulRuns { get; set; }
    public int FailedRuns { get; set; }
    public double ActiveTimeInMilliseconds { get; set; }
    public double NVATimeInMilliseconds { get; set; }
    public double TotalTimeInMilliseconds { get; set; }

    public RunAggregatesForInstrumentClass(
        string instrumentId,
        long totalRuns,
        int successfulRuns,
        int failedRuns,
        double activeTimeInMilliseconds,
        double nvaTimeInMilliseconds,
        double totalTimeInMilliseconds)
    {
        InstrumentId = instrumentId;
        TotalRuns = totalRuns;
        SuccessfulRuns = successfulRuns;
        FailedRuns = failedRuns;
        ActiveTimeInMilliseconds = activeTimeInMilliseconds;
        NVATimeInMilliseconds = nvaTimeInMilliseconds;
        TotalTimeInMilliseconds = totalTimeInMilliseconds;
    }

    public void Subtract(RunAggregatesForInstrumentClass? a2)
    {
        if (a2 == null)
            return;

        TotalRuns -= a2.TotalRuns;
        SuccessfulRuns -= a2.SuccessfulRuns;

        FailedRuns -= a2.FailedRuns;

        ActiveTimeInMilliseconds -= a2.ActiveTimeInMilliseconds;
        NVATimeInMilliseconds -= a2.NVATimeInMilliseconds;
        TotalTimeInMilliseconds -= a2.TotalTimeInMilliseconds;
    }
}
