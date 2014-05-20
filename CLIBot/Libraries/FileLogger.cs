using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CLIBot.Libraries {
    public class FileLogger {
        public string FileName;

        public FileLogger(string Filename) {
            FileName = Filename;
        }

        public void Log(string Message) {
            File.AppendAllText(FileName, Message + "\n");
        }
    }
}
