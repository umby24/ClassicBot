using System;
using ClassicBot;
using CLIBot.Libraries;
using CLIBot.Classes;

namespace CLIBot {
    class Program {

        private static CommandHandler _ch;
        public static Importer Importer;
        private static FileLogger _logger;
        private static string _ipNameorUrl, _prefix;
        private static bool _logPackets;
        private static int _port;

        static void Main(string[] args) {
            var myServer = new Bot(true, "Bot", "Bot", false);
            myServer.ClientSupportedExtensions.Add(CPEExtensions.CustomBlocks);

            Importer = new Importer();
            _logger = new FileLogger("Packets.txt");
            _ch = new CommandHandler();
            _prefix = "!";

            if (args.Length > 0) {
                if (args[0] == "help" || args[0] == "-h" || args[0] == "--help" || args[0] == "/h") {
                    Console.WriteLine("CLI Classic bot");
                    Console.WriteLine("By Umby24");
                    Console.WriteLine("");
                    Console.WriteLine("Usage: CLIBot.exe [verifynames] [UN] [Pass] [URL, Name, or IP] [Port or LogPackets (optional)]");
                    Console.WriteLine("verifynames may be either true or false");
                    Console.WriteLine("service may be either classicube or minecraft.");
                    Console.WriteLine("LogPackets may be true or false.");
                    return;
                }

                if (args.Length < 5) {
                    Console.WriteLine("Invalid number of arguments. See CLIbot.exe -h");
                    return;
                }

                bool testing;

                if (!bool.TryParse(args[0], out testing)) {
                    Console.WriteLine("Verifynames must be either true or false. See CLIbot.exe -h");
                    return;
                }

                if (testing)
                    myServer.VerifyNames = true;

                myServer.Username = args[2];
                myServer.Password = args[3];
                _ipNameorUrl = args[4];

                if (args.Length >= 6) {
                    testing = false;
                    int myInt;
                    bool myBool;
                        
                    if (int.TryParse(args[5], out myInt)) {
                        _port = myInt;
                        testing = true;

                        if (args.Length > 6) {
                            if (bool.TryParse(args[6], out myBool)) {
                                _logPackets = myBool;
                            }
                        }
                    }

                    if (bool.TryParse(args[5], out myBool)) {
                        _logPackets = myBool;
                        testing = true;
                    }

                    if (testing == false) {
                        Console.WriteLine("Argument 6 invalid. See CLIbot.exe -h");
                        return;
                    }
                }
            }

            if (_logPackets) {
                myServer.PacketReceived += packet => _logger.Log(packet);
            }
            
            myServer.ChatMessage += message => {
                ColoredConsole.ColorConvertingConsole.WriteLine("[Chat] " + message);

                if (message.Contains(_prefix))
                    _ch.HandleServerCommand(message, _prefix, myServer);
            };

            myServer.InfoMessage += message => ColoredConsole.ColorConvertingConsole.WriteLine("&2[Info] &f" + message);

            myServer.ErrorMessage += message => ColoredConsole.ColorConvertingConsole.WriteLine("&4[Error] &f" + message);

            myServer.BlockChanged += (x, y, z) => {
                if (Importer.Importing && myServer.ClientWorld.GetBlockId(x, y, z) == 15) 
                    Importer.ImportMBot(myServer, x, y, z);
            };

            if (_ipNameorUrl.Contains("http")) {
                myServer.Connect(_ipNameorUrl, true, "127.0.0.1", 25565);
            } else if (_ipNameorUrl.Contains(".") && _ipNameorUrl.Length < 16) {
                myServer.Connect("", false, _ipNameorUrl, _port);
            } else {
                myServer.Connect(_ipNameorUrl, false);
            }

            string input;

            do {
                input = Console.ReadLine();

                if (input != null && input.Contains(_prefix))
                    _ch.HandleConsoleCommand(input, _prefix, myServer);

            } while (input != "quit");

            myServer.Disconnect();
        }
    }
}
