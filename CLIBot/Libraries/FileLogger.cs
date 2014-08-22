using System.IO;

namespace CLIBot.Libraries {
    public class FileLogger {
        public string FileName;

        public FileLogger(string filename) {
            FileName = filename;
        }

        public void Log(string message) {
            File.AppendAllText(FileName, message + "\n");
        }
    }
}
