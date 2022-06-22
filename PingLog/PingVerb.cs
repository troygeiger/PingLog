using System.Net.NetworkInformation;

namespace PingLog
{
    internal class PingVerb : VerbBase<PingCmdOption>
    {

        public PingVerb(PingCmdOption options) : base("Ping", options) { }

        public override void Run()
        {
            var ping = new Ping();
            byte[] buffer = BuildBuffer(Options.BufferSize);
            var ip = Options.GetIpHost();
            for(int i = 1; i <= Options.Count; i++)
            {
                try
                {
                    var result = ping.Send(ip, Options.Timeout, buffer);
                    WriteLog($"Reply from {result.Address}: seq={i} status={result.Status} sent_bytes={buffer.Length} rec_bytes={result.Buffer.Length} time={result.RoundtripTime}ms");
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