using CommandLine;

namespace process_monitor;

public class Options
{
    [Option('p', "process", Required = false, HelpText = "Set process name, or part of name. Also, you can pass regular expression")]
    public string? ProcessName { get; set; }
    [Option('f', "frequency", Required = false, HelpText = "Process check frequency per minute.You can pass a floating point value like 0.1")]
    public float Frequency { get; set; }
    [Option('t', "liveTime", Required = false, HelpText = "Maximum process live time.You can pass a floating point value like 0.1")]
    public float MaxProcessTime { get; set; }
}