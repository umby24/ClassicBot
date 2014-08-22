namespace ClassicBot.Classes {
    public class Assistant {
        public static string StripColors(string message) {
            message = message.Replace("&0", "");
            message = message.Replace("&1", "");
            message = message.Replace("&2", "");
            message = message.Replace("&3", "");
            message = message.Replace("&4", "");
            message = message.Replace("&5", "");
            message = message.Replace("&6", "");
            message = message.Replace("&7", "");
            message = message.Replace("&8", "");
            message = message.Replace("&9", "");
            message = message.Replace("&a", "");
            message = message.Replace("&b", "");
            message = message.Replace("&c", "");
            message = message.Replace("&d", "");
            message = message.Replace("&e", "");
            message = message.Replace("&f", "");

            return message;
        }
    }
}
