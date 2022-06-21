using CommandLine;
using PingLog;

Parser.Default.ParseArguments<PingCmdOption, TracertCmdOptions>(args)
.WithParsed<PingCmdOption>(o =>new PingVerb(o).Run())
.WithParsed<TracertCmdOptions>(o => new TracertVerb(o).Run());