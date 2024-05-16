using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using PerformanceOfRecordStruct;


var summary = BenchmarkRunner.Run<BenchmarkLINQPerformance>();

public class BenchmarkLINQPerformance
{
    private const int N = 1000;

    private IQueryable<RunAggregates> monthlyAggregatesQuery = default!;
    private IQueryable<RunAggregates> excessDailyAtStartQuery = default!;
    private IQueryable<RunAggregates> excessDailyAtEndQuery = default!;

    private readonly Random random = new();

    [GlobalSetup]
    public void GlobalSetup()
    {
        var data = Enumerable.Range(0, N).Select(i =>
            {
                int tr = (int)random.NextInt64(10, 100);
                int fr = (int)random.NextInt64(0, 10);

                var aggr = new RunAggregates
                {
                    InstrumentId = $"Id~{i}.480",
                    CompletedSuccessfullyCount = tr,
                    CompletedWithErrorsCount = fr,
                    CompletedWithWarningsCount = 0,
                    ActiveTimeInMilliseconds = tr * 1000,
                    NVATimeInMilliseconds = fr * 100
                };

                return aggr;
            });

        monthlyAggregatesQuery = data.AsQueryable();

        excessDailyAtStartQuery = data.
            Select(d => new RunAggregates
            {
                InstrumentId = d.InstrumentId,
                ActiveTimeInMilliseconds = d.ActiveTimeInMilliseconds / 4,
                NVATimeInMilliseconds = d.ActiveTimeInMilliseconds / 4,
                CompletedSuccessfullyCount = d.CompletedSuccessfullyCount / 4,
                CompletedWithErrorsCount = d.CompletedWithErrorsCount / 4
            }).AsQueryable();

        excessDailyAtEndQuery = data.
            Select(d => new RunAggregates
            {
                InstrumentId = d.InstrumentId,
                ActiveTimeInMilliseconds = d.ActiveTimeInMilliseconds / 5,
                NVATimeInMilliseconds = d.ActiveTimeInMilliseconds / 5,
                CompletedSuccessfullyCount = d.CompletedSuccessfullyCount / 5,
                CompletedWithErrorsCount = d.CompletedWithErrorsCount / 5
            }).AsQueryable();

    }

    [Benchmark]
    public int MonthlyForRecordStruct()
    {
        var result = RunQueriesServiceWithStruct.GetAggregatesFromMonthlyView(
            monthlyAggregatesQuery, excessDailyAtStartQuery, excessDailyAtEndQuery);
        return result.Count();
    }

    [Benchmark]
    public int DailyForRecordStruct()
    {
        var result = RunQueriesServiceWithStruct.GetAggregatesFromDailyView(monthlyAggregatesQuery);
        return result.Count();
    }

    [Benchmark]
    public int MonthlyForClass()
    {
        var result = RunQueriesServiceWithClass.GetAggregatesFromMonthlyView(
            monthlyAggregatesQuery, excessDailyAtStartQuery, excessDailyAtEndQuery);
        return result.Count();
    }

    [Benchmark]
    public int DailyForClass()
    {
        var result = RunQueriesServiceWithClass.GetAggregatesFromDailyView(monthlyAggregatesQuery);
        return result.Count();
    }

}



//var o = new BenchmarkLINQPerformance();
//o.GlobalSetup();

//Console.WriteLine(o.MonthlyForRecordStruct());
//Console.WriteLine(o.DailyForRecordStruct());
//Console.WriteLine(o.MonthlyForClass());
//Console.WriteLine(o.DailyForClass());
