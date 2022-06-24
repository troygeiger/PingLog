using System.Net.NetworkInformation;

namespace PingLog
{
    internal class PingVerb : VerbBase<PingCmdOption>
    {
        Action<PingReply, int, int> writeHandler;
        Queue<string> displayList = new Queue<string>();
        List<short> times = new List<short>();

        public PingVerb(PingCmdOption options) : base("Ping", options)
        {
            if (!string.IsNullOrEmpty(options.RuntimeString) && TimeSpan.TryParse(options.RuntimeString, out TimeSpan span))
            {
                options.Count = (int)span.TotalMilliseconds / options.PingDelay;
            }

            writeHandler = options.OnlyOutputUnsuccessful
                ? new Action<PingReply, int, int>(WriteLogUnsuccessful)
                : new Action<PingReply, int, int>(WriteLogNormal);
            Console.Clear();
            Console.CursorVisible = false;
            WriteToConsole = false;
        }

        public override void Run()
        {
            var ping = new Ping();
            byte[] buffer = BuildBuffer(Options.BufferSize);
            var ip = Options.GetIpHost();
            for (int i = 1; i <= Options.Count; i++)
            {
                try
                {
                    var result = ping.Send(ip, Options.Timeout, buffer);
                    times.Add(result.Status == IPStatus.Success ? (short)result.RoundtripTime : (short)Options.Timeout);
                    writeHandler(result, i, buffer.Length);

                }
                catch (System.Exception ex)
                {
                    WriteLog(ex.Message);
                }

                Thread.Sleep(Options.PingDelay);
            }
        }

        private void ConsoleWriteStatus(int seq)
        {
            TimeSpan remTime = TimeSpan.FromMilliseconds(Options.PingDelay * (Options.Count - seq));
            string seqStr = $"Current Seq={seq} of {Options.Count} ({(seq / (double)Options.Count):0.0%}); Remaining Time={remTime:g};";
            int blankSize = (Console.WindowWidth > 0 ? Console.WindowWidth : 30) - seqStr.Length;
            Console.Write($"{seqStr}{new string(' ', blankSize > 0 ? blankSize : 0)}");
        }

        private void WritePingOutput(PingReply result, int seq, int bufferLength)
        {
            double jitter = 0;
            double avgTime = 0;
            if (times.Count >= 5)
            {
                short[] diffs = new short[4];
                short[] block = times.TakeLast(4).ToArray();

                short last = times.TakeLast(5).First();
                for (var i = 0; i < diffs.Length; i++)
                {
                    diffs[i] = (short)(last > block[i] ? last - block[i] : block[i] - last);
                    last = diffs[i];
                }
                jitter = diffs.Average(s => s);
            }

            if (times.Count >= 2)
            {
                avgTime = times.TakeLast(20).Average(s=>s);
            }

            string msg = $"Reply from {result.Address}: seq={seq} status={result.Status} sent_bytes={bufferLength} rec_bytes={result.Buffer.Length} time={result.RoundtripTime}ms avg={avgTime:0}ms jitter={jitter:0}ms";
            displayList.Enqueue($"[{DateTime.Now}] {msg}");
            WriteLog(msg);
            if (Console.WindowHeight > 0)
            {
                while (displayList.Count - (Console.WindowHeight - 1) > 0)
                {
                    displayList.Dequeue();
                }
            }
            bool flag = false;
            foreach (string item in displayList)
            {
                if (flag) Console.WriteLine();
                Console.Write(item);
                flag = true;
            }
        }

        private void WriteLogNormal(PingReply result, int seq, int bufferLength)
        {
            Console.Clear();
            ConsoleWriteStatus(seq);
            WritePingOutput(result, seq, bufferLength);
        }

        private void WriteLogUnsuccessful(PingReply result, int seq, int bufferLength)
        {
            Console.Clear();
            ConsoleWriteStatus(seq);
            if (result.Status != IPStatus.Success)
            {
                WritePingOutput(result, seq, bufferLength);
            }
        }
    }
}