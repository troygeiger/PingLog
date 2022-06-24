using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;

namespace PingLog
{
    internal class TracertVerb : VerbBase<TracertCmdOptions>
    {
        Ping pinger;
        private byte[] buffer = Encoding.ASCII.GetBytes("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
        static Dictionary<string, string> hostCache = new Dictionary<string, string>();

        public TracertVerb(TracertCmdOptions options) : base("Tracert", options)
        {
            pinger = new Ping();
            buffer = BuildBuffer(options.BufferSize);
        }

        public override void Run()
        {
            string ip = Options.GetIpHost();
            for (int i = 1; i <= Options.Count; i++)
            {
                try
                {
                    WriteLog("");
                    WriteLog($"Tracing route to {Options.Host}({ip}) over a maximum of {Options.MaxHops} hops");
                    WriteLog("");
                    WriteLog("Hop  [------ Trip Times -----]  Address");
                    TraceRoute(ip, 1);
                    WriteLog("");
                    WriteLog($"Trace seq {i} completed.");
                }
                catch (System.Exception ex)
                {
                    WriteLog(ex.Message);
                }

                Thread.Sleep(Options.PingDelay);
            }
        }

        /// <summary>
        /// Do not call directly
        /// </summary>
        /// <param name="hostNameOrAddress"></param>
        /// <param name="hop"></param>
        private void TraceRoute(string hostIP, int hop)
        {
            if (hop > Options.MaxHops)
            {
                WriteLog("Max hops exceeded.");
                return;
            }

            PingOptions pingerOptions = new PingOptions(hop, true);


            PingReply? reply = default(PingReply);
            StringBuilder times = new StringBuilder();
            int timeoutCount = 0;
            string? host = null;
            bool ttlExp = false;
            Stopwatch stopwatch = new Stopwatch();

            //Run multiple times per Ttl
            for (int i = 0; i < 3; i++)
            {
                stopwatch.Restart();
                reply = pinger.Send(hostIP, Options.Timeout, buffer, pingerOptions);

                stopwatch.Stop();
                switch (reply.Status)
                {
                    case IPStatus.Success:
                    case IPStatus.TtlExpired:
                        times.Append(string.Format("{0,6} ms", stopwatch.ElapsedMilliseconds)); //reply.RoundtripTime));
                        host = reply.Address.ToString();
                        ttlExp = reply.Status == IPStatus.TtlExpired;
                        break;
                    case IPStatus.TimedOut:
                        times.Append("     *   ");
                        timeoutCount++;
                        break;
                    default:
                        WriteLog($"{string.Format("{0,3}", hop)}  {reply.Address}  reports: {reply.Status}");
                        return;
                }
            }

            if (timeoutCount == 3)
            {
                WriteTraceHopStatus(hop, times.ToString(), "Request timed out.");
                TraceRoute(hostIP, hop + 1);
                return;
            }

            writeTrace(hop, host, times.ToString());
            if (ttlExp)
            {
                //recurse to get the next address...
                TraceRoute(hostIP, hop + 1);
            }


        }

        private void writeTrace(int hop, PingReply reply)
        {
            string host = GetHost(reply.Address.ToString());

            WriteLog($"{string.Format("{0,3}", hop)}{string.Format("{0,6}", reply.RoundtripTime)} ms" +
                $"  {(host != null ? $"{host} [{reply.Address}]" : reply.Address.ToString())}");
        }

        private void writeTrace(int hop, string? hopIP, string times)
        {
            string host = GetHost(hopIP);

            WriteLog($"{string.Format("{0,3}", hop)}{times}" +
                $"  {(host != null ? $"{host} [{hopIP}]" : hopIP)}");
        }

        private void WriteTraceHopStatus(int hop, string times, string hopMessage)
        {
            WriteLog($"{string.Format("{0,3}", hop)}{times}" +
               $"  {hopMessage}");
        }

        private string GetHost(string? ip)
        {
            if (ip == null) return "";
            try
            {
                if (hostCache.ContainsKey(ip))
                {
                    return hostCache[ip];
                }
                var result = Dns.GetHostEntry(ip);
                hostCache.Add(ip, result.HostName);
                return result.HostName;
            }
            catch (Exception)
            {
                hostCache.Add(ip, "");
            }
            return "";
        }
    }
}