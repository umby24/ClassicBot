using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using ClassicBot;
using ClassicBot.World;

namespace StressBot {
    class Program
    {
        private static Bot[] BotsArr;

        static void Main(string[] args) {
            int Bots = 20;
            bool dupelicateNames = false;
            BotsArr = new ClassicBot.Bot[Bots];
            var r = new Random();

            for (int i = 0; i < Bots; i++)
            {
                string name = "uBot" + (dupelicateNames ? r.Next(i) : i);
                BotsArr[i] = new Bot(false, name, "", false);
                BotsArr[i].ErrorMessage += message => Console.WriteLine("Error:" + i + ": " + message);
                BotsArr[i].Connect("potato", false, "127.0.0.1", 25565);
            }

            while (true)
            {
                UpdateLoop();
                Thread.Sleep(1000);
            }
        }

        private static void UpdateLoop()
        {
            Console.Clear();

            foreach (Bot b in BotsArr)
            {
                if (b == null)
                    Console.WriteLine("Waiting...");
                else if (!b.Nm.BaseSock.Connected)
                    Console.WriteLine("Disconnected.");
                else
                    Console.WriteLine("Connected");

            }
        }
    }
}
