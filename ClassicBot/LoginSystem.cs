using System;

using ClassicBot.Classes;

namespace ClassicBot {
	public enum ServiceTypes {
		Classicube,
		Minecraft
	}

	public class LoginSystem {
		Main ClientMain;

		public LoginSystem (Main Core) {
			ClientMain = Core;
		}

		public bool VerifyNames(string ServerNameOrUrl, bool IsUrl=false) {
			if (ClientMain.Service == ServiceTypes.Classicube)
				return VerifyClassicube(ServerNameOrUrl, IsUrl);
			else
				return VerifyMinecraft();
		}

        /// <summary>
        /// Logs in to classicube and retreives server information.
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="IsUrl"></param>
        /// <returns></returns>
		bool VerifyClassicube(string Name, bool IsUrl=false) {
			var CCLogin = new Classicube_Interaction(ClientMain.Username, ClientMain.Password);

			if (!CCLogin.Login())
				return false;

            string[] Info;

            if (!IsUrl) 
			    Info = CCLogin.GetServerInfo(Name);
            else
                Info = CCLogin.GetServerByUrl(Name);

			ClientMain.IP = Info[0];
			ClientMain.Port = int.Parse(Info[1]);
			ClientMain.MPPass = Info[2];

			return true;
		}

		bool VerifyMinecraft() {
			return true;
		}
	}
}

