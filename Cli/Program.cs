using System;
using Cli.Classes;

namespace Cli {
    static class Program {
        private static ClassicBot.ClassicBot _cbot;
        private static CommandHandler _commander;
        
        static void Main(string[] args) {
            _cbot = new ClassicBot.ClassicBot("umby24", "127.0.0.1", 13337);
            _cbot.InfoMessage += CbotOnInfoMessage;
            _cbot.DebugMessage += CbotOnDebugMessage;
            _cbot.ErrorMessage += CbotOnErrorMessage;
            _cbot.OnMessage += CbotOnOnMessage;
            _cbot.McClient.ClientPlayer.LevelFinished += WorldOnLevelFinished;
            _commander = new CommandHandler(_cbot);
            var input = "";
            _cbot.Connect();
            
            while (input != "EXIT") {
                input = Console.ReadLine().Trim();
                
                if (input != "EXIT") {
                    _cbot.McClient.ClientPlayer.SendMessage(input);
                }
            }
            
        }

        private static void WorldOnLevelFinished() {
            _cbot.McClient.ClientPlayer.SendMessage("World Finished.");
        }

        private static void CbotOnOnMessage(string value) {
            _commander.HandleCommand(value);
            ColorConvertingConsole.WriteLine(value);
        }

        private static void CbotOnDebugMessage(string value) {
            Console.WriteLine("[DEBUG] " + value);
        }

        private static void CbotOnErrorMessage(string value) {
            ColorConvertingConsole.WriteLine("&4[ERROR] &f" + value);
        }

        private static void CbotOnInfoMessage(string value) {
            Console.WriteLine("[INFO] " + value);
        }
    }
}


