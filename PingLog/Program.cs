using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PingLog
{
    class Program
    {
        static string logPath;

        static void Main(string[] args)
        {
            Console.Write("Please enter the IP address or host to ping: ");
            string ip = Console.ReadLine();

            logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"Ping Log {ip}.txt");
            System.Net.NetworkInformation.Ping ping = new System.Net.NetworkInformation.Ping();
            
            while (true)
            {
                try
                {
                    var res = ping.Send(ip);
                    WriteLog($"Reply from {res.Address}: status={res.Status} bytes={res.Buffer.Length} time={res.RoundtripTime}ms");
                }
                catch (Exception ex)
                {
                    WriteLog(ex.Message);
                    break;
                }

                Thread.Sleep(1000);
            }

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
    }
}
