namespace PingLog
{
    internal abstract class VerbBase<T> where T : CmdOptionsBase
    {
        private string _verbName;

        public VerbBase(string verbName, T options)
        {
            _verbName = verbName;
            Options = options;

            options.OutPath = string.IsNullOrEmpty(options.OutPath)
                ? BuildDefaultPath() : options.OutPath;


            if (!Directory.Exists(Path.GetDirectoryName(options.OutPath)))
            {
                throw new DirectoryNotFoundException("Path defined does not exist.");
            }

            if (!File.Exists(options.OutPath))
            {
                WriteLog("*****************************************");
                WriteLog("* PingLog.exe                           *");
                WriteLog("* https://github.com/troygeiger/PingLog *");
                WriteLog("*****************************************");
                WriteLog("");
            }
        }

        private string BuildDefaultPath()
        {
            DateTime now = DateTime.Now;
            return Path.Combine(Environment.CurrentDirectory,
                $"{_verbName}_Log_{Options.Host}_{now.Month:00}{now.Day:00}{now.Year}.txt");
        }

        protected void WriteLog(string message)
        {
            message = $"[{DateTime.Now}] {message}\r\n";
            Console.Write(message);
            try
            {
                File.AppendAllText(Options.OutPath!, message);
            }
            catch (Exception)
            {

            }
        }

        public T Options { get; }

        public virtual void Run()
        {

        }
    }
}