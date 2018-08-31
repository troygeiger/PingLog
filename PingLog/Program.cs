using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PingLog
{
    class Program
    {
        private static byte[] buffer = Encoding.ASCII.GetBytes("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
        static string logPath;
        static Dictionary<string, string> hostCache = new Dictionary<string, string>();
        static Arguments argObj;

        static void Main(string[] args)
        {
            argObj = GetArgumentObject(args);
            string ip;

            if (argObj.ShouldClose)
            {
                return;
            }
            if (argObj.HostDefined)
            {
                ip = argObj.Host;
            }
            else
            {
                Console.Write("Please enter the IP address or host to ping: ");
                ip = Console.ReadLine();
            }

            if (argObj.OutPathDefined)
            {
                logPath = Path.GetFullPath(argObj.OutPath);
                if (!Directory.Exists(Path.GetDirectoryName(logPath)))
                {
                    WriteConsoleError("Path defined does not exist.");
                    return;
                }
            }
            else
            {
                DateTime now = DateTime.Now;
                logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                $"{(argObj.Tracert ? "TraceRoute_Log" : "Ping_Log")}_{ip}_{now.Month.ToString("00")}{now.Day.ToString("00")}{now.Year}.txt");
            }

            if (!File.Exists(logPath))
            {
                WriteLog("*****************************************");
                WriteLog("* PingLog.exe                           *");
                WriteLog("* https://github.com/troygeiger/PingLog *");
                WriteLog("*****************************************");
                WriteLog("");
            }

            Ping ping = null;
            
            if (!argObj.Tracert)
            {
                ping = new Ping();
            }
            
            while (true)
            {
                if (!argObj.OutPathDefined)
                {
                    DateTime now = DateTime.Now;
                    logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                    $"{(argObj.Tracert ? "TraceRoute_Log" : "Ping_Log")}_{ip}_{now.Month.ToString("00")}{now.Day.ToString("00")}{now.Year}.txt");
                }
                try
                {
                    if (argObj.Tracert)
                    {
                        TraceRoute(ip);
                    }
                    else
                    {
                        var res = ping.Send(ip);
                        WriteLog($"Reply from {res.Address}: status={res.Status} bytes={res.Buffer.Length} time={res.RoundtripTime}ms");
                    }
                }
                catch (Exception ex)
                {
                    WriteLog(ex.Message);
                    break;
                }

                Thread.Sleep(argObj.PingDelay);
            }

        }

        private static Arguments GetArgumentObject(string[] args)
        {
            Arguments result = new Arguments();
            bool shouldOutputOptions = false;

            for (int i = 0; i < args.Length; i++)
            {
                try
                {
                    switch (args[i])
                    {
                        case "-h":
                            i++;
                            result.Host = args[i];
                            break;
                        case "-p":
                            i++;
                            result.OutPath = args[i];
                            break;
                        case "-d":
                            i++;
                            int d;
                            if (int.TryParse(args[i], out d))
                            {
                                result.PingDelay = d;
                            }
                            break;
                        case "-t":
                            result.Tracert = true;
                            break;
                        case "-mh":
                            i++;
                            int mh;
                            if (int.TryParse(args[i], out mh))
                            {
                                result.MaxHops = mh;
                            }
                            break;
                        case "-?":
                        default:
                            shouldOutputOptions = true;
                            result.ShouldClose = true;
                            break;
                    }
                }
                catch (Exception)
                {

                }
            }
            if (shouldOutputOptions)
            {
                OutputOptions();
            }

            return result;
        }

        private static void WriteLog(string message)
        {
            message = $"[{DateTime.Now}] {message}\r\n";
            Console.Write(message);
            try
            {
                File.AppendAllText(logPath, message);
            }
            catch (Exception)
            {

            }
        }

        private static void WriteConsoleError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[{DateTime.Now}] {message}");
            Console.ResetColor();
            Console.WriteLine("Press enter to exit");
        }

        private static void OutputOptions()
        {
            Console.WriteLine("Usage: PingLog.exe [Options]");
            Console.WriteLine();
            Console.WriteLine("-?\tShow this help.");
            Console.WriteLine("-h\tHost or IP address");
            Console.WriteLine("-p\tOptional Output path; Otherwise the executable location + {host}.txt is used.");
            Console.WriteLine("-d\tThe delay between pings in milliseconds. Default is 1000 (equaling 1 second).");
            Console.WriteLine("-t\tRuns a trace route on host at intervals specified by the -d switch (or default of 1000).");
            Console.WriteLine("-mh\tThe maximum hops the trace route will except. (Default is 30)");
            Console.WriteLine();
        }

        private static void TraceRoute(string hostNameOrAddress)
        {
            WriteLog("");
            WriteLog($"Tracing route to {hostNameOrAddress} over a maximum of {argObj.MaxHops} hops");
            WriteLog("");
            WriteLog("Hop  [------ Trip Times -----]  Address");
            TraceRoute(hostNameOrAddress, 1);
            WriteLog("");
            WriteLog("Trace completed.");
        }

        /// <summary>
        /// Do not call directly
        /// </summary>
        /// <param name="hostNameOrAddress"></param>
        /// <param name="ttl"></param>
        private static void TraceRoute(string hostNameOrAddress, int ttl)
        {
            if (ttl > argObj.MaxHops)
            {
                WriteLog("Max hops exceeded.");
            }

            Ping pinger = new Ping();
            PingOptions pingerOptions = new PingOptions(ttl, true);
            int timeout = 4000;
            
            PingReply reply = default(PingReply);
            StringBuilder times = new StringBuilder();
            int timeoutCount = 0;
            string host = null;
            bool ttlExp = false;
            Stopwatch stopwatch = new Stopwatch();

            //Run multiple times per Ttl
            for (int i = 0; i < 3; i++)
            {
                stopwatch.Restart();
                reply = pinger.Send(hostNameOrAddress, timeout, buffer, pingerOptions);
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
                        WriteLog($"{string.Format("{0,3}", ttl)}  {reply.Address}  reports: {reply.Status}");
                        return;
                }
            }
            
            if (timeoutCount == 3)
            {
                WriteTraceHopStatus(ttl, times.ToString(), "Request timed out.");
                TraceRoute(hostNameOrAddress, ttl + 1);
            }
            else
            {
                writeTrace(ttl, host, times.ToString());
                if (ttlExp)
                {
                    //recurse to get the next address...
                    TraceRoute(hostNameOrAddress, ttl + 1);
                }
            }

        }

        private static void writeTrace(int hop, PingReply reply)
        {
            string host = GetHost(reply.Address.ToString());
            
            WriteLog($"{string.Format("{0,3}", hop)}{string.Format("{0,6}", reply.RoundtripTime)} ms" +
                $"  {(host != null ? $"{host} [{reply.Address}]": reply.Address.ToString())}");
        }

        private static void writeTrace(int hop, string hopIP, string times)
        {
            string host = GetHost(hopIP);

            WriteLog($"{string.Format("{0,3}", hop)}{times}" +
                $"  {(host != null ? $"{host} [{hopIP}]" : hopIP)}");
        }

        private static void WriteTraceHopStatus(int hop, string times, string hopMessage)
        {
            WriteLog($"{string.Format("{0,3}", hop)}{times}" +
               $"  {hopMessage}");
        }

        private static string GetHost(string ip)
        {
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
                hostCache.Add(ip, null);
            }
            return null;
        }

        internal class Arguments
        {
            public bool ShouldClose { get; set; }

            public string Host { get; internal set; }

            public bool HostDefined { get => !string.IsNullOrWhiteSpace(Host); }

            public string OutPath { get; set; }

            public bool OutPathDefined { get => !string.IsNullOrWhiteSpace(OutPath); }

            public int PingDelay { get; set; } = 1000;

            public bool Tracert { get; set; }

            public int MaxHops { get; set; } = 30;
        }

    }
}
