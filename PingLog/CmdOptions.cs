using System.Net;
using CommandLine;

namespace PingLog;

internal class CmdOptionsBase
{
    [Option('h', "host", HelpText = "Host or IP address", Required = true)]
    public string? Host { get; internal set; }

    [Option('o', "output", HelpText = "Optional Output path; Otherwise the executable location + {host}.txt is used.")]
    public string? OutPath { get; set; }

    [Option('d', "delay", HelpText = "The delay between pings in milliseconds.", Default = 1000)]
    public int PingDelay { get; set; } = 1000;

    [Option('t', "timeout", HelpText = "Specify the timeout, in milliseconds, for the ping operation.", Default = 5000)]
    public int Timeout { get; set; } = 5000;

    [Option('b', "buffer", HelpText = "Specify the buffer size used for pings/tracert.", Default = 64)]
    public int BufferSize { get; set; } = 64;

    [Option('c', "count", HelpText = "The number of retries before ending.", Default = int.MaxValue)]
    public int Count { get; set; } = int.MaxValue;

    public bool HostDefined { get => !string.IsNullOrWhiteSpace(Host); }

    public bool OutPathDefined { get => !string.IsNullOrWhiteSpace(OutPath); }

    public string GetIpHost()
    {
        try
        {
            var result = Dns.GetHostEntry(Host!);
            return result.AddressList.FirstOrDefault()?.ToString() ?? Host!;

        }
        catch (System.Exception)
        {
            return Host;
        }
    }

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