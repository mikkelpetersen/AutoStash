using ExileCore;

namespace AutoStash;

public enum LogLevel
{
    None,
    Error,
    Debug
}

public class Log
{
    private static readonly AutoStash Instance = AutoStash.Instance;

    private static LogLevel LogLevel
    {
        get
        {
            switch (Instance.Settings.LogLevel)
            {
                case "Debug":
                    return LogLevel.Debug;
                case "Error":
                    return LogLevel.Error;
                default:
                    return LogLevel.None;
            }
        }
    }

    public static void Debug(string message)
    {
        if (LogLevel < LogLevel.Debug)
            return;

        DebugWindow.LogMsg($"{Instance.Name}: {message}", 5);
    }

    public static void Error(string message)
    {
        if (LogLevel < LogLevel.Error)
            return;

        DebugWindow.LogError($"{Instance.Name}: {message}", 5);
    }
}