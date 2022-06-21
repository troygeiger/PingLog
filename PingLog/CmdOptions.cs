using CommandLine;

namespace PingLog;

internal class CmdOptionsBase
{
    [Option('h', "host", HelpText = "Host or IP address", Required = true)]
    public string? Host { get; internal set; }

    [Option('o', "output", HelpText = "Optional Output path; Otherwise the executable location + {host}.txt is used.")]
    public string? OutPath { get; set; }

    [Option('d', "delay", HelpText = "The delay between pings in milliseconds.")]
    public int PingDelay { get; set; } = 1000;

    public bool HostDefined { get => !string.IsNullOrWhiteSpace(Host); }

    public bool OutPathDefined { get => !string.IsNullOrWhiteSpace(OutPath); }

}

[Verb("ping", true, HelpText = "Run a continuous ping against a host.")]
internal class PingCmdOption : CmdOptionsBase
{
    
}

[Verb("tracert", HelpText = "Runs a continuous tracert against a host.")]
internal class TracertCmdOptions : CmdOptionsBase
{
    [Option('m', "maxhops", HelpText = "The maximum number of hops while performing Tracert.")]
    public int MaxHops { get; set; } = 30;
}