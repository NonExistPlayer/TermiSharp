namespace TermiSharp;

public interface IRunnable
{
    bool IsRunning { get; set; }
    void Run();
    void RunAsync();
}