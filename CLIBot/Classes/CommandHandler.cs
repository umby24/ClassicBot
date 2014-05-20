using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClassicBot;
using ClassicBot.Classes;

namespace CLIBot.Classes {
    public interface Command {
        string Command {get;}
        string Plugin {get;}
        string Group { get; }
        string Help { get; }
        bool Guests { get; }

        void Run(string Command, string[] args, string Text1, string Text2, Main ServerMain, CLIBotClass Bot);
    }

    public class CommandHandler {
        public Dictionary<string, Command> CommandDict;
        public Dictionary<string, List<string>> Groups;

        public CommandHandler() {
            CommandDict = new Dictionary<string, Command>(StringComparer.InvariantCultureIgnoreCase);
            CommandDict.Add("say", new SayCommand());
            CommandDict.Add("headb", new HeadBobCommand());
            CommandDict.Add("import", new ImportCommand());
            CommandDict.Add("imports", new ImportsCommand());
            CommandDict.Add("cancel", new CancelImportCommand());
            RegisterGroups();
        }

        public void HandleConsoleCommand(string Raw, string CommandPrefix, Main ServerMain, CLIBotClass Bot) {
            Raw = Assistant.StripColors(Raw);
            string command = Raw.Substring(0, Raw.IndexOf(" "));
            command = command.Replace(CommandPrefix, "");

            string Text1 = Raw.Substring(command.Length + 1, Raw.Length - (command.Length + 1));
            string[] args = Text1.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);

            if (CommandDict.ContainsKey(command)) 
                CommandDict[command].Run(command, args, Text1, "", ServerMain, Bot);
            
        }

        public void HandleServerCommand(string Raw, string CommandPrefix, Main ServerMain, CLIBotClass Bot) {
            Raw = Assistant.StripColors(Raw);
            var splits = Raw.Split(' ');

            if (splits.Length > 1 && splits[1].StartsWith(CommandPrefix)) {
                string Name = splits[0].Replace("<", "").Replace(">", "").Replace("[", "").Replace("]", "").Replace(" ", "").Replace(":", "");
                string Text1 = Raw.Substring(Raw.IndexOf(splits[2]), Raw.Length - (Raw.IndexOf(splits[2])));
                string[] args = Text1.Split(' ');

                if (CommandDict.ContainsKey(splits[1].Replace(CommandPrefix, "")) )
                    CommandDict[splits[1].Replace(CommandPrefix, "")].Run(splits[1], args, Text1, "", ServerMain, Bot);
                
            }

        }

        void RegisterGroups() {
            if (Groups != null)
                Groups.Clear();
            else
                Groups = new Dictionary<string, List<string>>(StringComparer.InvariantCultureIgnoreCase);

            foreach (string command in CommandDict.Keys) {
                if (Groups.ContainsKey(CommandDict[command].Group)) 
                    Groups[CommandDict[command].Group].Add(command.Replace("/", ""));
                 else 
                    Groups.Add(CommandDict[command].Group, new List<string>() { command.Replace("/", "") });
            }
        }
    }
}
