namespace SimpleMockServer.Domain.TextPart.CSharp;

class CompilationStatistic
{
    public TimeSpan TotalCompilationTime { get; private set; } = TimeSpan.Zero;
    public TimeSpan MaxCompilationTime { get; private set; } = TimeSpan.Zero;
        
    public TimeSpan TotalLoadAssemblyTime { get; private set; } = TimeSpan.Zero;
    public TimeSpan TotalTime { get; private set; } = TimeSpan.Zero;
    
    public int CountLoadAssemblies { get; private set; }

    public void AddCompilationTime(TimeSpan elapsed)
    {
        TotalCompilationTime = TotalCompilationTime.Add(elapsed);
            
        if (elapsed > MaxCompilationTime)
            MaxCompilationTime = elapsed;
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
