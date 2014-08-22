using System;
using System.Threading;
using System.IO;
using ClassicBot;

namespace CLIBot.Classes {
    public class Importer {
        public Thread ImportThread;
        public string ImportFile;
        public bool Importing;

        public void ImportMBot(Bot client, short x, short y, short z) {
            if (!File.Exists("Imports\\" + ImportFile + ".mbot")) { 
                client.SendChat("File not found.");
                return;
            }

            client.SendChat("Importing.");

            using (var sr = new StreamReader("Imports\\" + ImportFile + ".mbot")) 
                sr.ReadToEnd().Split(new[] {'\n'}, StringSplitOptions.RemoveEmptyEntries);

            //ImportThread = new Thread(() => ImportMbotThread(BlockLines, Client, X, Y, Z));
            //ImportThread.Start();
            //ImportThread = new Thread(ImportMbotThread);
            //ImportThread.Start(BlockLines, Client, X, Y, Z);
        }

        public void ImportMbotThread(string[] lines, Bot client, short baseX, short baseY, short baseZ) {
            foreach (string m in lines) {
                string[] mySplit = m.Split(',');
                int x, y, z, type;

                int.TryParse(mySplit[0], out x);
                int.TryParse(mySplit[1], out y);
                int.TryParse(mySplit[2], out z);
                int.TryParse(mySplit[3].Replace(":", ""), out type);

                if (type != 0)
                    client.PlaceBlock(baseX + x, baseY + y, baseZ + z, (byte)type);
            }
            client.SendChat("Done importing.");
        }
    }
}
