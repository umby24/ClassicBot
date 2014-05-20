using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLIBot.Classes {
    public class CLIBotClass {
        public Libraries.FileLogger Logger;
        public Importer Imp;
        public CommandHandler CH;
        public string IPNameUrl = "", CommandPrefix = "!";
        public int Port = -999;
        public bool LogPackets = false;
    }
}
