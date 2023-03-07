namespace HangfireSamples.Jobs;

public interface IMyRecurringJob
{
    public void DoSomethingReentrant();
}

public sealed class MyRecurringJob : IMyRecurringJob
{
    public void DoSomethingReentrant()
    {
        Console.WriteLine("IMyRecurringJob doing something");
    }
}