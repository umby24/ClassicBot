using System;
using System.Collections.Generic;
using ClassicBot;
using ClassicBot.Classes;

namespace CLIBot.Classes {
    public interface ICommand {
        string Command {get;}
        string Plugin {get;}
        string Group { get; }
        string Help { get; }
        bool Guests { get; }

        void Run(string command, string[] args, string text1, string text2, Bot serverBot);
    }

    public class CommandHandler {
        public Dictionary<string, ICommand> CommandDict;
        public Dictionary<string, List<string>> Groups;

        public CommandHandler() {
            CommandDict = new Dictionary<string, ICommand>(StringComparer.InvariantCultureIgnoreCase) {
                {"say", new SayCommand()},
                {"headb", new HeadBobCommand()},
                {"import", new ImportCommand()},
                {"imports", new ImportsCommand()},
                {"cancel", new CancelImportCommand()}
            };
            RegisterGroups();
        }

        public void HandleConsoleCommand(string raw, string commandPrefix, Bot serverBot) {
            raw = Assistant.StripColors(raw);
            string command = raw.Substring(0, raw.IndexOf(" "));
            command = command.Replace(commandPrefix, "");

            string text1 = raw.Substring(command.Length + 1, raw.Length - (command.Length + 1));
            string[] args = text1.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);

            if (CommandDict.ContainsKey(command)) 
                CommandDict[command].Run(command, args, text1, "", serverBot);
            
        }

        public void HandleServerCommand(string raw, string commandPrefix, Bot serverBot) {
            raw = Assistant.StripColors(raw);
            var splits = raw.Split(' ');

            if (splits.Length > 1 && splits[1].StartsWith(commandPrefix)) {
                string name = splits[0].Replace("<", "").Replace(">", "").Replace("[", "").Replace("]", "").Replace(" ", "").Replace(":", "");
                string text1 = raw.Substring(raw.IndexOf(splits[2]), raw.Length - (raw.IndexOf(splits[2])));
                string[] args = text1.Split(' ');

                if (CommandDict.ContainsKey(splits[1].Replace(commandPrefix, "")) )
                    CommandDict[splits[1].Replace(commandPrefix, "")].Run(splits[1], args, text1, "", serverBot);
                
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
                    Groups.Add(CommandDict[command].Group, new List<string> { command.Replace("/", "") });
            }
        }
    }
}
