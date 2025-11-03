namespace Lira.Domain.TextPart.Impl.CSharp;

class CompilationStatistic
{
    public TimeSpan TotalCompilationTime { get; private set; } = TimeSpan.Zero;
    public TimeSpan TotalLoadAssemblyTime { get; private set; } = TimeSpan.Zero;
    public TimeSpan SyntaxTreesTime { get; private set; } = TimeSpan.Zero;
    public TimeSpan TotalTime { get; private set; } = TimeSpan.Zero;

    public int CountLoadAssemblies { get; private set; }
    public int CountFunctionsCompiled { get; private set; }
    public int CountFunctionsTotalFromCache { get; private set; }
    public int CountFunctionsTotal { get; private set; }

    public void AddCompilationTime(TimeSpan elapsed)
    {
        CountFunctionsCompiled++;
        TotalCompilationTime = TotalCompilationTime.Add(elapsed);
    }
    public void AddSyntaxTreesTime(TimeSpan elapsed)
    {
        SyntaxTreesTime = SyntaxTreesTime.Add(elapsed);
    }

    public void AddTotalTime(TimeSpan elapsed)
    {
        TotalTime = TotalTime.Add(elapsed);
    }

    public void AddLoadAssemblyTime(TimeSpan elapsed)
    {
        CountLoadAssemblies++;
        TotalLoadAssemblyTime = TotalLoadAssemblyTime.Add(elapsed);
    }

    public void AddFunctionFromCache()
    {
        CountFunctionsTotalFromCache++;
    }

    public void AddFunctionTotal()
    {
        CountFunctionsTotal++;
    }
}
