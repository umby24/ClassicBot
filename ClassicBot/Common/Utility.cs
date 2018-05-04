using System;
using System.Text.RegularExpressions;

namespace ClassicBot.Common {
    public static class Utility {
        /// <summary>
        /// Strips colorcode formatting from a chat message.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string StripFormatting(string input) {
            var codeRegex = new Regex("&[0-9a-fA-F]");
            
            return codeRegex.Replace(input, "",9999);
        }
    }
}