using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using RestSharp;

namespace ClassicBot.Common {
    public class Classicube {
        private List<ClassicubeServer> _cachedServers;
        private readonly CookieContainer _netCookies;
        private DateTime _cachedtime;

        private const string LoginUrl = "https://www.classicube.net/api/login/";
        private const string ServersUrl = "https://www.classicube.net/api/servers/";


        public Classicube() {
            _netCookies = new CookieContainer();
        }

        public bool Login(string username, string password) {
            var client = new RestClient(LoginUrl);
            var req = new RestRequest(Method.GET);
            client.UserAgent = "ClassicubeNetSession";
            client.CookieContainer = _netCookies;

            IRestResponse response = client.Execute(req); // -- Send a request out to receive a token

            if (response.StatusCode != HttpStatusCode.OK) {
                throw new Exception("Failed to login: " + response.Content); // -- Just make sure we got a 200..
            }

            var parsedResponse = JsonConvert.DeserializeObject<LoginResponse>(response.Content);
            req.Method = Method.POST; // -- Send an authentication request with the token received previously..
            req.AddParameter("username", username);
            req.AddParameter("password", password);
            req.AddParameter("token", parsedResponse.Token);

            response = client.Execute(req);
            parsedResponse = JsonConvert.DeserializeObject<LoginResponse>(response.Content);

            if (parsedResponse.ErrorCount > 0) {
                throw new Exception("There were errors with the request: " + string.Join(", ", parsedResponse.Errors));
            }

            return true;
        }

        public List<ClassicubeServer> GetServers() {
            if (_cachedServers != null && (DateTime.Now - _cachedtime).Minutes < 4) {
                return _cachedServers;
            }

            var client = new RestClient(ServersUrl);
            var req = new RestRequest(Method.GET);
            client.UserAgent = "ClassicubeNetSession";
            client.CookieContainer = _netCookies;

            IRestResponse response = client.Execute(req);

            if (response.StatusCode != HttpStatusCode.OK) {
                throw new Exception("Failed to download server list.");
            }

            var parsedResponse = JsonConvert.DeserializeObject<ServersResponse>(response.Content);
            _cachedServers = parsedResponse.Servers;
            _cachedtime = DateTime.Now;
            return parsedResponse.Servers;
        }

        public ClassicubeServer GetServerInfo(string serverName) {
            List<ClassicubeServer> servers = GetServers(); // -- This method will deal with the caching for us :)
            return servers.FirstOrDefault(a => a.Name.ToLower() == serverName.ToLower());
        }

        public ClassicubeServer GetServerByUrl(string url) {
            List<ClassicubeServer> servers = GetServers();
            string[] mySplits = url.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            url = mySplits[mySplits.Length - 1];

            return servers.FirstOrDefault(a => a.Hash.ToLower() == url.ToLower());
        }
    }
}
