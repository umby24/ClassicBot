using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassicBot.Classes {
    public class Assistant {
        public static string StripColors(string Message) {
            Message = Message.Replace("&0", "");
            Message = Message.Replace("&1", "");
            Message = Message.Replace("&2", "");
            Message = Message.Replace("&3", "");
            Message = Message.Replace("&4", "");
            Message = Message.Replace("&5", "");
            Message = Message.Replace("&6", "");
            Message = Message.Replace("&7", "");
            Message = Message.Replace("&8", "");
            Message = Message.Replace("&9", "");
            Message = Message.Replace("&a", "");
            Message = Message.Replace("&b", "");
            Message = Message.Replace("&c", "");
            Message = Message.Replace("&d", "");
            Message = Message.Replace("&e", "");
            Message = Message.Replace("&f", "");

            return Message;
        }
    }
}
