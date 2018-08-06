using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            Arguments argObj = GetArgumentObject(args);
            string ip;


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
                logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"Ping Log {ip}.txt");
            }

            
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
                        case "-?":
                        default:
                            shouldOutputOptions = true;
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
        }

        //public static int GetParentProcessId()
        //{
        //    PROCESS_BASIC_INFORMATION pbi = new PROCESS_BASIC_INFORMATION();

        //    //Get a handle to our own process
        //    IntPtr hProc = OpenProcess((ProcessAccessFlags)0x001F0FFF, false, Process.GetCurrentProcess().Id);

        //    try
        //    {
        //        int sizeInfoReturned;
        //        int queryStatus = NtQueryInformationProcess(hProc, (PROCESSINFOCLASS)0, ref pbi, pbi.Size, out sizeInfoReturned);
        //    }
        //    finally
        //    {
        //        if (!hProc.Equals(IntPtr.Zero))
        //        {
        //            //Close handle and free allocated memory
        //            CloseHandle(hProc);
        //            hProc = IntPtr.Zero;
        //        }
        //    }

        //    return (int)pbi.InheritedFromUniqueProcessId;
        //}


        internal class Arguments
        {
            public bool ShouldClose { get; set; }

            public string Host { get; internal set; }

            public bool HostDefined { get => !string.IsNullOrWhiteSpace(Host); }

            public string OutPath { get; set; }

            public bool OutPathDefined { get => !string.IsNullOrWhiteSpace(OutPath); }

            public int PingDelay { get; set; } = 1000;
        }
    }
}
