using System;
using System.Collections.Generic;
using System.Threading;
using Cli.Commands;

namespace Cli {
    public class CommandHandler {
        private Dictionary<string, ICommand> _commands;
        private ClassicBot.ClassicBot _bot;
        
        public CommandHandler(ClassicBot.ClassicBot bot) {
            _bot = bot;
            _commands = new Dictionary<string, ICommand>();
            _commands.Add("!say", new SayCommand());
            _commands.Add("!follow", new FollowCommand());
            _commands.Add("!where", new WhereAmICommand());
            _commands.Add("!export", new ExportCommand());
            _commands.Add("!import", new ImportCommand());
        }

        public void HandleCommand(string message) {
            if (!message.Contains(":"))
                return;
            
            string stripped = ClassicBot.Common.Utility.StripFormatting(message);
            string username = stripped.Substring(0, stripped.IndexOf(":"));
            string baseMessage = stripped.Replace(username + ":", "").Trim();

            if (!baseMessage.StartsWith("!")) // -- Message is not a command/
                return;

            string[] splitMessage = baseMessage.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries);
            
            if (splitMessage.Length == 0)
                return;
            
            ICommand cmd;
            
            if (!_commands.TryGetValue(splitMessage[0], out cmd))
                return; // -- Command not found.
            
            cmd.Execute(_bot, username, message, splitMessage);
        }
    }
}