using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Web;
using Newtonsoft.Json.Linq;

namespace ClassicBot {
	public class Classicube_Interaction {
		private string Username, Password, Cachedpage;
        private CookieContainer NetCookies;
        private DateTime Cachedtime;

        const string LoginURL = "http://www.classicube.net/acc/login";
        const string LogoutURL = "http://www.classicube.net/acc/logout";
        const string HomepageURL = "http://www.classicube.net/";
        const string ServersURL = "http://www.classicube.net/api/serverlist";

        const string FailedMessage = "Login failed (Username or password may be incorrect)";
        const string TokenPattern = "<input id=\"csrf_token\" name=\"csrf_token\" type=\"hidden\" value=\"(.+?)\">";
        const string UsernamePatten = "<a href=\"/acc\" class=\"button\">([a-zA-Z0-9_\\.]{2,16})</a>";

        private Regex Loginregex;
        private Regex Tokenregex;
        private Regex Usernameregex;

        public Classicube_Interaction(string username, string password) {
            Username = username;
            Password = password;

            Loginregex = new Regex(FailedMessage);
            Tokenregex = new Regex(TokenPattern);
            Usernameregex = new Regex(UsernamePatten);

            NetCookies = new CookieContainer();
        }

        public bool Login() {
            var LoginWorker = (HttpWebRequest)WebRequest.Create(LoginURL);
            LoginWorker.UserAgent = "ClassicubeNetSession";
            LoginWorker.Method = "GET";
            LoginWorker.CookieContainer = NetCookies;

            HttpWebResponse response;
            string PageData;
            
            response = (HttpWebResponse)LoginWorker.GetResponse();

            using (Stream responseStream = response.GetResponseStream()) {
                PageData = new StreamReader(responseStream).ReadToEnd();
            }

            if (PageData == null)
                throw new Exception("Failed to download Classicube.net page.");

            var Match = Usernameregex.Match(PageData);

            if (Match.Success) {
                if (Match.Value.ToLower() == Username.ToLower()) {
                    Username = Match.Groups[1].Value;
                    return true;
                } else { // -- Not the right user, need to log out..
                    NetCookies = new CookieContainer(); // -- Clears the old cookies
                    return Login(); // -- Login again.
                }
            }

            Match = Tokenregex.Match(PageData);

            if (!Match.Success) {
                throw new Exception("Unrecognized page served by Classicube.net");
            }

            string AuthToken = Match.Groups[1].Value;
            string RequestStr = "username=";

            RequestStr += HttpUtility.UrlEncode(Username);
            RequestStr += "&password=";
            RequestStr += HttpUtility.UrlEncode(Password);
            RequestStr += "&csrf_token=";
            RequestStr += HttpUtility.UrlEncode(AuthToken);
            RequestStr += "&redirect=";
            RequestStr += HttpUtility.UrlEncode(HomepageURL);

            LoginWorker = (HttpWebRequest)WebRequest.Create(LoginURL);
            LoginWorker.UserAgent = "ClassicubeNetSession";
            LoginWorker.Method = "POST";
            LoginWorker.ContentType = "application/x-www-form-urlencoded";
            LoginWorker.CookieContainer = NetCookies;
            LoginWorker.ContentLength = RequestStr.Length;

            using (Stream PostStream = LoginWorker.GetRequestStream()) {
                PostStream.Write(Encoding.ASCII.GetBytes(RequestStr), 0, RequestStr.Length);
            }

            string LoginResponse = "";

            response = (HttpWebResponse)LoginWorker.GetResponse();

            using (Stream responseStream = response.GetResponseStream()) {
                LoginResponse = new StreamReader(responseStream).ReadToEnd();
            }

            if (LoginResponse == "") {
                throw new Exception("Connection Error");
            }

            Match = Loginregex.Match(LoginResponse);

            if (Match.Success) {
                throw new Exception("Incorrect username or password.");
            }

            Match = Usernameregex.Match(LoginResponse);

            if (Match.Success) {
                return true;
            } else {
                return false;
            }
        }

        public string GetServers() {
            if (Cachedtime != null && (DateTime.Now - Cachedtime).Minutes < 4) {
                return Cachedpage;
            }

            var ServerWorker = (HttpWebRequest)WebRequest.Create(ServersURL);
            ServerWorker.Method = "GET";
            ServerWorker.UserAgent = "ClassicubeNetSession";
            ServerWorker.CookieContainer = NetCookies;

            HttpWebResponse response;
            string PageData;

            response = (HttpWebResponse)ServerWorker.GetResponse();

            using (Stream responseStream = response.GetResponseStream()) {
                PageData = new StreamReader(responseStream).ReadToEnd();
            }

            Cachedpage = PageData;
            Cachedtime = DateTime.Now;

            return PageData;
        }


        public string[] GetServerInfo(string ServerName) {
            var jsonObj = JArray.Parse(GetServers());

            string ip = "", mppass = "";
            int port = 0, maxplayers = 0, onlineplayers = 0;

            foreach (JToken item in jsonObj) {
                if (item["name"].Value<string>().ToLower() == ServerName.ToLower()) {
                    ip = item["ip"].Value<string>();
                    mppass = item["mppass"].Value<string>();
                    port = item["port"].Value<int>();
                    maxplayers = item["maxplayers"].Value<int>();
                    onlineplayers = item["players"].Value<int>();
                    break;
                }
            }

            return new[] { ip, port.ToString(), mppass, onlineplayers.ToString(), maxplayers.ToString() };
        }

        public string[] GetServerByUrl(string URL) {
            var jsonObj = JArray.Parse(GetServers());
            string[] MySplits = URL.Split(new Char[] {'/'}, StringSplitOptions.RemoveEmptyEntries);

            URL = MySplits[MySplits.Length - 1];

            string ip = "", mppass = "";
            int port = 0, maxplayers = 0, onlineplayers = 0;

            foreach (JToken item in jsonObj) {
                if (item["hash"].Value<string>().ToLower() == URL.ToLower()) {
                    ip = item["ip"].Value<string>();
                    mppass = item["mppass"].Value<string>();
                    port = item["port"].Value<int>();
                    maxplayers = item["maxplayers"].Value<int>();
                    onlineplayers = item["players"].Value<int>();
                    break;
                }
            }

            return new[] { ip, port.ToString(), mppass, onlineplayers.ToString(), maxplayers.ToString() };
        }
	}
}

