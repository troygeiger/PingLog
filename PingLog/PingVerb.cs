using System.Net.NetworkInformation;

namespace PingLog
{
    internal class PingVerb : VerbBase<PingCmdOption>
    {

        public PingVerb(PingCmdOption options) : base("Ping", options) { }

        public override void Run()
        {
            var ping = new Ping();
            while (true)
            {
                try
                {
                    var result = ping.Send(Options.Host!);
                    WriteLog($"Reply from {result.Address}: status={result.Status} bytes={result.Buffer.Length} time={result.RoundtripTime}ms");
                }
                catch (System.Exception ex)
                {
                    WriteLog(ex.Message);
                }
                Thread.Sleep(Options.PingDelay);
            }
        }
    }
}