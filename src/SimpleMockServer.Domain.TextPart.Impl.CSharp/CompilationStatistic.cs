namespace SimpleMockServer.Domain.TextPart.Impl.CSharp;

class CompilationStatistic
{
    public TimeSpan TotalCompilationTime { get; private set; } = TimeSpan.Zero;
    public TimeSpan TotalLoadAssemblyTime { get; private set; } = TimeSpan.Zero;
    public TimeSpan TotalTime { get; private set; } = TimeSpan.Zero;

    public int CountLoadAssemblies { get; private set; }

    public void AddCompilationTime(TimeSpan elapsed)
    {
        TotalCompilationTime = TotalCompilationTime.Add(elapsed);
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
}
