using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using ClassicBot;

namespace CLIBot.Classes {
    public struct SayCommand : Command {
        public string Command { get { return "say";  } }
        public string Plugin { get { return ""; } }
        public string Group { get { return "General"; } }
        public string Help { get { return "Usage: say [message]. Makes the bot say a message."; } }
        public bool Guests { get { return false; } }

        public void Run(string Command, string[] args, string Text1, string Text2, Main ServerMain, CLIBotClass Bot) {
            if (args.Length > 0) 
                ServerMain.SendChat(Text1.TrimStart(' '));
        }
    }

    public struct HeadBobCommand : Command {
        public string Command { get { return "headb"; } }
        public string Plugin { get { return ""; } }
        public string Group { get { return "Lulz"; } }
        public string Help { get { return "Usage: Headb [times]. Makes the bot headbang."; } }
        public bool Guests { get { return false; } }

        public void Run(string Command, string[] args, string Text1, string Text2, Main ServerMain, CLIBotClass Bot) {
            if (args.Length > 0) {
                int Times = int.Parse(args[0]);

                for (int i = 0; i < Times; i++) {
                    ServerMain.Position[0] = 0;
                    ServerMain.RefreshLocation();
                    Thread.Sleep(100);
                    ServerMain.Position[0] = 120;
                    ServerMain.RefreshLocation();
                    Thread.Sleep(100);
                }
            }
        }
    }

    public struct ImportCommand : Command {
        public string Command { get { return "import"; } }
        public string Plugin { get { return ""; } }
        public string Group { get { return "Build"; } }
        public string Help { get { return "Imports a previously exported file. Usage: import [filename]"; } }
        public bool Guests { get { return false; } }

        public void Run(string Command, string[] args, string Text1, string Text2, Main ServerMain, CLIBotClass Bot) {
            if (args.Length > 0) {
                if (!File.Exists("Imports\\" + args[0] + ".mbot")) {
                    ServerMain.SendChat("File not found.");
                    return;
                }

                Bot.Imp.ImportFile = args[0];
                Bot.Imp.Importing = true;
                ServerMain.SendChat("Place an iron-ore marker to import.");
            }
        }
    }

    public struct ImportsCommand : Command {
        public string Command { get { return "imports"; } }
        public string Plugin { get { return ""; } }
        public string Group { get { return "Build"; } }
        public string Help { get { return "Shows a list of avaliable imports."; } }
        public bool Guests { get { return false; } }

        public void Run(string Command, string[] args, string Text1, string Text2, Main ServerMain, CLIBotClass Bot) {
            var DirInfo = new DirectoryInfo("Imports\\");
            ServerMain.SendChat("You have " + DirInfo.GetFiles().Length + " imports.");
            string ImportString = "";

            foreach (FileInfo b in DirInfo.GetFiles()) 
                ImportString += b.Name + ", ";

            ServerMain.SendChat(ImportString.Substring(0, ImportString.Length - 2));
        }
    }

    public struct CancelImportCommand : Command {
        public string Command { get { return "cancel"; } }
        public string Plugin { get { return ""; } }
        public string Group { get { return "Build"; } }
        public string Help { get { return "Cancel an ongoing import."; } }
        public bool Guests { get { return false; } }

        public void Run(string Command, string[] args, string Text1, string Text2, Main ServerMain, CLIBotClass Bot) {
            if (Bot.Imp.ImportThread != null)
                Bot.Imp.ImportThread.Abort();

            ServerMain.SendChat("Canceled import.");
        }
    }
}
