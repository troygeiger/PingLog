using CommandLine;
using PingLog;

Console.CancelKeyPress += (s, e) =>
{
    Console.CursorVisible = true;
    Console.Clear();
};


Parser.Default.ParseArguments<PingCmdOption, TracertCmdOptions>(args)
.WithParsed<PingCmdOption>(o => new PingVerb(o).Run())
.WithParsed<TracertCmdOptions>(o => new TracertVerb(o).Run());

