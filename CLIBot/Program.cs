using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClassicBot;
using ClassicBot.World;
using CLIBot.Libraries;

namespace CLIBot {
    class Program {
        static void Main(string[] args) {
            var myServer = new Main(ServiceTypes.Classicube, false, "Bot", "Bot", false);
            string IPNameUrl = "";
            int Port = -999;
            bool LogPackets = false;

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
                    IPNameUrl = args[4];

                    if (args.Length >= 6) {
                        Testing = false;
                        int myInt;
                        bool myBool;
                        
                        if (int.TryParse(args[5], out myInt)) {
                            Port = myInt;
                            Testing = true;

                            if (args.Length > 6) {
                                if (bool.TryParse(args[6], out myBool)) {
                                    LogPackets = myBool;
                                    Testing = true;
                                }
                            }
                        }

                        if (bool.TryParse(args[5], out myBool)) {
                            LogPackets = myBool;
                            Testing = true;
                        }

                        if (Testing == false) {
                            Console.WriteLine("Argument 6 invalid. See CLIbot.exe -h");
                            return;
                        }
                    }
                }
            }

            var MyLogger = new FileLogger("Packets.txt");

            if (LogPackets) {
                myServer.PacketReceived += (string Packet) => {
                    MyLogger.Log(Packet);
                };
            }

            myServer.ChatMessage += (string Message) => {
                ColoredConsole.ColorConvertingConsole.WriteLine("[Chat] " + Message);
            };

            myServer.DebugMessage += (string Message) => {
                Console.WriteLine("[Debug] " + Message);
            };

            myServer.InfoMessage += (string Message) => {
                Console.WriteLine("[Info] " + Message);
            };

            myServer.YouMoved += () => {
                myServer.SendChat(myServer.Location.X.ToString() + " " + myServer.Location.Y.ToString() + " " + myServer.Location.Z.ToString());
            };

            if (IPNameUrl.Contains("http")) {
                myServer.Connect(IPNameUrl, true, "127.0.0.1", 25565);
            } else if (IPNameUrl.Contains(".") && IPNameUrl.Length < 16) {
                myServer.Connect("", false, IPNameUrl, Port);
            } else {
                myServer.Connect(IPNameUrl, false);
            }

            string Input = "";

            do {
                Input = Console.ReadLine();

                if (Input.ToLower().StartsWith("chat "))
                    myServer.SendChat(Input.Substring(5, Input.Length - 5));

            } while (Input != "quit");

            myServer.Disconnect();
        }
    }
}
