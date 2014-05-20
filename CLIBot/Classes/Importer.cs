using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using ClassicBot;

namespace CLIBot.Classes {
    public class Importer {
        public Thread ImportThread;
        public string ImportFile;
        public bool Importing;

        public void ImportMBot(Main Client, short X, short Y, short Z) {
            if (!File.Exists("Imports\\" + ImportFile + ".mbot")) { 
                Client.SendChat("File not found.");
                return;
            }

            Client.SendChat("Importing.");
            string[] BlockLines;

            using (var SR = new StreamReader("Imports\\" + ImportFile + ".mbot")) 
                BlockLines = SR.ReadToEnd().Split(new char[] {'\n'}, StringSplitOptions.RemoveEmptyEntries);

            //ImportThread = new Thread(() => ImportMbotThread(BlockLines, Client, X, Y, Z));
            //ImportThread.Start();
            //ImportThread = new Thread(ImportMbotThread);
            //ImportThread.Start(BlockLines, Client, X, Y, Z);
        }

        public void ImportMbotThread(string[] Lines, Main Client, short BaseX, short BaseY, short BaseZ) {
            foreach (string m in Lines) {
                string[] mySplit = m.Split(',');
                int x = 0, y = 0, z = 0, type = 0;

                int.TryParse(mySplit[0], out x);
                int.TryParse(mySplit[1], out y);
                int.TryParse(mySplit[2], out z);
                int.TryParse(mySplit[3].Replace(":", ""), out type);

                if (type != 0)
                    Client.PlaceBlock(BaseX + x, BaseY + y, BaseZ + z, (byte)type);
            }
            Client.SendChat("Done importing.");
        }
    }
}
