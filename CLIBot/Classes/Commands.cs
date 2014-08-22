using System.Threading;
using System.IO;
using ClassicBot;

namespace CLIBot.Classes {
    public struct SayCommand : ICommand {
        public string Command { get { return "say";  } }
        public string Plugin { get { return ""; } }
        public string Group { get { return "General"; } }
        public string Help { get { return "Usage: say [message]. Makes the bot say a message."; } }
        public bool Guests { get { return false; } }

        public void Run(string command, string[] args, string text1, string text2, Bot serverBot) {
            if (args.Length > 0) 
                serverBot.SendChat(text1.TrimStart(' '));
        }
    }

    public struct HeadBobCommand : ICommand {
        public string Command { get { return "headb"; } }
        public string Plugin { get { return ""; } }
        public string Group { get { return "Lulz"; } }
        public string Help { get { return "Usage: Headb [times]. Makes the bot headbang."; } }
        public bool Guests { get { return false; } }

        public void Run(string command, string[] args, string text1, string text2, Bot serverBot) {
            if (args.Length > 0) {
                int times = int.Parse(args[0]);

                for (int i = 0; i < times; i++) {
                    serverBot.Position[0] = 0;
                    serverBot.RefreshLocation();
                    Thread.Sleep(100);
                    serverBot.Position[0] = 120;
                    serverBot.RefreshLocation();
                    Thread.Sleep(100);
                }
            }
        }
    }

    public struct ImportCommand : ICommand {
        public string Command { get { return "import"; } }
        public string Plugin { get { return ""; } }
        public string Group { get { return "Build"; } }
        public string Help { get { return "Imports a previously exported file. Usage: import [filename]"; } }
        public bool Guests { get { return false; } }

        public void Run(string command, string[] args, string text1, string text2, Bot serverBot) {
            if (args.Length > 0) {
                if (!File.Exists("Imports\\" + args[0] + ".mbot")) {
                    serverBot.SendChat("File not found.");
                    return;
                }

                Program.Importer.ImportFile = args[0];
                Program.Importer.Importing = true;
                serverBot.SendChat("Place an iron-ore marker to import.");
            }
        }
    }

    public struct ImportsCommand : ICommand {
        public string Command { get { return "imports"; } }
        public string Plugin { get { return ""; } }
        public string Group { get { return "Build"; } }
        public string Help { get { return "Shows a list of avaliable imports."; } }
        public bool Guests { get { return false; } }

        public void Run(string command, string[] args, string text1, string text2, Bot serverBot) {
            var dirInfo = new DirectoryInfo("Imports\\");
            serverBot.SendChat("You have " + dirInfo.GetFiles().Length + " imports.");
            string importString = "";

            foreach (FileInfo b in dirInfo.GetFiles()) 
                importString += b.Name + ", ";

            serverBot.SendChat(importString.Substring(0, importString.Length - 2));
        }
    }

    public struct CancelImportCommand : ICommand {
        public string Command { get { return "cancel"; } }
        public string Plugin { get { return ""; } }
        public string Group { get { return "Build"; } }
        public string Help { get { return "Cancel an ongoing import."; } }
        public bool Guests { get { return false; } }

        public void Run(string command, string[] args, string text1, string text2, Bot serverBot) {
            if (Program.Importer.ImportThread != null)
                Program.Importer.ImportThread.Abort();

            serverBot.SendChat("Canceled import.");
        }
    }
}
