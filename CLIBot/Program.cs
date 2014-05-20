using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClassicBot;
using ClassicBot.World;
using CLIBot.Libraries;
using CLIBot.Classes;

namespace CLIBot {
    class Program {

        static void Main(string[] args) {
            var myServer = new Main(ServiceTypes.Classicube, true, "Bot", "Bot", false);
            myServer.ClientSupportedExtensions.Add(CPEExtensions.CustomBlocks);

            var myBot = new CLIBotClass();
            myBot.Imp = new Importer();
            myBot.Logger = new FileLogger("Packets.txt");
            myBot.CH = new CommandHandler();

            if (args.Length > 0) {
                if (args[0] == "help" || args[0] == "-h" || args[0] == "--help" || args[0] == "/h") {
                    Console.WriteLine("CLI Classic bot");
                    Console.WriteLine("By Umby24");
                    Console.WriteLine("");
                    Console.WriteLine("Usage: CLIBot.exe [verifynames] [service] [UN] [Pass] [URL, Name, or IP] [Port or LogPackets (optional)]");
                    Console.WriteLine("verifynames may be either true or false");
                    Console.WriteLine("service may be either classicube or minecraft.");
                    Console.WriteLine("LogPackets may be true or false.");
                    return;
                } else {
                    if (args.Length < 5) {
                        Console.WriteLine("Invalid number of arguments. See CLIbot.exe -h");
                        return;
                    }

                    bool Testing;

                    if (!bool.TryParse(args[0], out Testing)) {
                        Console.WriteLine("Verifynames must be either true or false. See CLIbot.exe -h");
                        return;
                    }

                    if (Testing) 
                        myServer.VerifyNames = Testing;

                    if (args[1].ToLower() != "classicube" && args[1].ToLower() != "minecraft") {
                        Console.WriteLine("Invalid service type provided. See CLIbot.exe -h");
                        return;
                    }

                    if (args[1].ToLower() == "classicube")
                        myServer.Service = ServiceTypes.Classicube;
                    else
                        myServer.Service = ServiceTypes.Minecraft;

                    myServer.Username = args[2];
                    myServer.Password = args[3];
                    myBot.IPNameUrl = args[4];

                    if (args.Length >= 6) {
                        Testing = false;
                        int myInt;
                        bool myBool;
                        
                        if (int.TryParse(args[5], out myInt)) {
                            myBot.Port = myInt;
                            Testing = true;

                            if (args.Length > 6) {
                                if (bool.TryParse(args[6], out myBool)) {
                                    myBot.LogPackets = myBool;
                                    Testing = true;
                                }
                            }
                        }

                        if (bool.TryParse(args[5], out myBool)) {
                            myBot.LogPackets = myBool;
                            Testing = true;
                        }

                        if (Testing == false) {
                            Console.WriteLine("Argument 6 invalid. See CLIbot.exe -h");
                            return;
                        }
                    }
                }
            }

            if (myBot.LogPackets) {
                myServer.PacketReceived += (string Packet) => {
                    myBot.Logger.Log(Packet);
                };
            }
            
            myServer.ChatMessage += (string Message) => {
                ColoredConsole.ColorConvertingConsole.WriteLine("[Chat] " + Message);
                //Console.WriteLine(Message);

                if (Message.Contains(myBot.CommandPrefix))
                    myBot.CH.HandleServerCommand(Message, myBot.CommandPrefix, myServer, myBot);
            };

            myServer.DebugMessage += (string Message) => {
                ColoredConsole.ColorConvertingConsole.WriteLine("&6[Debug] &f" + Message);
            };

            myServer.InfoMessage += (string Message) => {
                ColoredConsole.ColorConvertingConsole.WriteLine("&2[Info] &f" + Message);
            };

            myServer.ErrorMessage += (string Message) => {
                ColoredConsole.ColorConvertingConsole.WriteLine("&4[Error] &f" + Message);
            };

            myServer.PacketReceived += (string Message) => {
                ColoredConsole.ColorConvertingConsole.WriteLine("&4[Packet] &f" + Message);
            };

            myServer.BlockChanged += (short X, short Y, short Z) => {
                if (myBot.Imp.Importing == true && myServer.ClientWorld.GetBlockId(X, Y, Z) == 15) 
                    myBot.Imp.ImportMBot(myServer, X, Y, Z);
            };

            if (myBot.IPNameUrl.Contains("http")) {
                myServer.Connect(myBot.IPNameUrl, true, "127.0.0.1", 25565);
            } else if (myBot.IPNameUrl.Contains(".") && myBot.IPNameUrl.Length < 16) {
                myServer.Connect("", false, myBot.IPNameUrl, myBot.Port);
            } else {
                myServer.Connect(myBot.IPNameUrl, false);
            }

            string Input = "";

            do {
                Input = Console.ReadLine();

                if (Input.Contains(myBot.CommandPrefix))
                    myBot.CH.HandleConsoleCommand(Input, myBot.CommandPrefix, myServer, myBot);

            } while (Input != "quit");

            myServer.Disconnect();
        }

        static void myServer_BlockChanged(short X, short Y, short Z) {
            
        }
    }
}
