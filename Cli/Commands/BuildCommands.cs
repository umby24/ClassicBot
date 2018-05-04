using System;
using ClassicBot.Common;
using Cli.Classes;

namespace Cli.Commands {
    public class ImportCommand : ICommand {
        public string CommandName => "!import";
        private ClassicBot.ClassicBot _bot;
        private bool _importMode = false;
        private string _importName = "";

        public void Execute(ClassicBot.ClassicBot bot, string sender, string ogMessage, string[] split) {
            if (_importMode) {
                bot.McClient.ClientPlayer.SendMessage("Import mode cancelled.");
                bot.McClient.ClientPlayer.BlockPlaced -= ClientPlayerOnBlockPlaced;
                _importMode = false;
                return;
            }

            if (split.Length < 2) {
                bot.McClient.ClientPlayer.SendMessage("Please provide an import name.");
                return;
            }

            _importName = split[1];
            _bot = bot;
            bot.McClient.ClientPlayer.SendMessage("Place an iron ore to import.");
            bot.McClient.ClientPlayer.BlockPlaced += ClientPlayerOnBlockPlaced;
            _importMode = true;
        }

        private void ClientPlayerOnBlockPlaced(Vector3S value) {
            byte placedBlock = _bot.McClient.ClientPlayer.World.GetBlockId(value);

            if (placedBlock != 15)
                return;

            _bot.McClient.ClientPlayer.SendMessage("Importing...");
            BuildImporter.ImportArea(value, _importName, _bot);
            _bot.McClient.ClientPlayer.SendMessage("Done.");
            _bot.McClient.ClientPlayer.BlockPlaced -= ClientPlayerOnBlockPlaced;
            _importMode = false;
        }
    }

    public class ExportCommand : ICommand {
        public string CommandName => "!export";
        private bool _exportMode = false;
        private Vector3S[] ExportCoords;
        private ClassicBot.ClassicBot _bot;
        private string _exportName = "";
        public void Execute(ClassicBot.ClassicBot bot, string sender, string ogMessage, string[] split) {
            if (_exportMode) {
                bot.McClient.ClientPlayer.SendMessage("Export mode cancelled.");
                bot.McClient.ClientPlayer.BlockPlaced -= ClientPlayerOnBlockPlaced;
                _exportMode = false;
                return;
            }

            if (split.Length < 2) {
                bot.McClient.ClientPlayer.SendMessage("Please provide an export name.");
                return;
            }

            _exportName = split[1];

            _bot = bot;
            ExportCoords = new Vector3S[2];    
            bot.McClient.ClientPlayer.SendMessage("Place 2 Iron-ore markers to mark the export area.");
            bot.McClient.ClientPlayer.BlockPlaced += ClientPlayerOnBlockPlaced;
        }

        private void ClientPlayerOnBlockPlaced(Vector3S value) {
            byte placedBlock = _bot.McClient.ClientPlayer.World.GetBlockId(value);

            if (placedBlock != 15) 
                return;
            
            if (ExportCoords[0] == null) {
                ExportCoords[0] = value;
                return;
            }
            ExportCoords[1] = value;
            // -- Begin export:
            _bot.McClient.ClientPlayer.SendMessage("Exporting...");
            _bot.McClient.ClientPlayer.BlockPlaced -= ClientPlayerOnBlockPlaced;
            _exportMode = false;
            BuildImporter.ExportArea(ExportCoords, _exportName, _bot);
            _bot.McClient.ClientPlayer.SendMessage("Exported.");
        }
    }
}