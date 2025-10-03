using Beaversims.Core.Common;
using Beaversims.Core.Shadow.WclClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beaversims.Core
{
    public class Logger
    {
        public static readonly string dirPath = Path.Combine(Utils.ProjectRoot(), "Shadow", "BeaverLogs");
        private static readonly List<string> _messages = [];

        private string LogFile = "";

        public void Log(string message)
        {
            string line = $"{message}";

            File.AppendAllText(LogFile, line + Environment.NewLine);
        }
        internal Logger(string fileName, Fight fight, int userTypeId)
        {
            var ndir = $"{fight.ReportCode}-{fight.Id}-{userTypeId}";
            var logDir = Path.Combine(dirPath, ndir);
            Directory.CreateDirectory(logDir);
            LogFile = Path.Combine(logDir, fileName + ".log");
            if (File.Exists(LogFile))
            {
                File.Delete(LogFile);
            }
        }
    }
}
