namespace NativeAot2IL.Logging;

public class LibLogWriter : LogWriter
{
    public override void Info(string message)
    {
        Logger.Info($"{message}", "Library");
    }

    public override void Warn(string message)
    {
        Logger.Warn($"{message}", "Library");
    }

    public override void Error(string message)
    {
        Logger.Error($"{message}", "Library");
    }

    public override void Verbose(string message)
    {
        Logger.Verbose($"{message}", "Library");
    }
}
