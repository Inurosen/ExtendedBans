using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Terraria;
using TShockAPI;

namespace ExtendedBans
{
	[APIVersion(1, 12)]
    public class ExtendedBans : TerrariaPlugin
	{
        public static string SavePath = "tshock";
        public static string EBDir = Path.Combine(SavePath, "extendedbans");
        public static List<EBPlayer> EBPlayers = new List<EBPlayer>();
        bool InitConfig = false;
        public static EBConfigFile Cfg = new EBConfigFile();
        public ExtendedBans(Main game)
            : base(game)
		{
            Order = -1;
		}
		public override void Initialize()
		{
            if (!Directory.Exists(EBDir))
                Directory.CreateDirectory(EBDir);
            if (Cfg.Load())
            {
                EBConfig.UseMysql = bool.Parse(Cfg["usemysql"]);
                EBConfig.MysqlHost = Cfg["mysqlhost"];
                EBConfig.MysqlLogin = Cfg["mysqllogin"];
                EBConfig.MysqlPassword = Cfg["mysqlpassword"];
                EBConfig.MysqlDatabase = Cfg["mysqldatabase"];
                InitConfig = true;
            }
            if (InitConfig)
            {
                EBData.InitXBansDB();
                EBCommands.Load();
                Hooks.NetHooks.GreetPlayer += OnGreetPlayer;
                Hooks.ServerHooks.Connect += OnConnect;
                Hooks.ServerHooks.Join += OnJoin;
                Hooks.ServerHooks.Leave += OnLeave;
                Hooks.ServerHooks.Chat += OnChat;
            }
		}
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (InitConfig)
                {
                    Hooks.NetHooks.GreetPlayer -= OnGreetPlayer;
                    Hooks.ServerHooks.Connect -= OnConnect;
                    Hooks.ServerHooks.Join -= OnJoin;
                    Hooks.ServerHooks.Leave -= OnLeave;
                    Hooks.ServerHooks.Chat -= OnChat;
                }
            }
            base.Dispose(disposing);
        }
        public override Version Version
		{
			get { return new Version("1.2.0304"); }
		}
		public override string Name
		{
			get { return "ExtendedBans"; }
		}
		public override string Author
		{
			get { return "Rosen"; }
		}
		public override string Description
		{
			get { return "Extended bans and mute system."; }
		}

        private void OnConnect(int ply, HandledEventArgs e)
        {
            var player = new TSPlayer(ply);
            if (player == null)
            {
                e.Handled = true;
                return;
            }

            string[] ban = EBUtils.IPBanInfo(player.IP);

            if (ban[0] == "banned")
            {
                TShock.Utils.ForceKick(player, string.Format("You are banned: {0}", ban[5]));
                e.Handled = true;
                return;
            }
        }

        private void OnJoin(int ply, HandledEventArgs e)
        {
            var player = TShock.Players[ply];
            if (player == null)
            {
                e.Handled = true;
                return;
            }

            string[] ban = EBUtils.PlayerBanInfo(player.Name);

            if (ban[0] == "banned")
            {
                TShock.Utils.ForceKick(player, string.Format("You are banned: {0}", ban[5]));
                e.Handled = true;
                return;
            }
        }

        public void OnGreetPlayer(int who, HandledEventArgs e)
        {
            lock (EBPlayers)
                EBPlayers.Add(new EBPlayer(who));
        }

        public void OnLeave(int whoami)
        {
            lock (EBPlayers)
            {
                for (int i = 0; i < EBPlayers.Count; i++)
                {
                    if (EBPlayers[i].Index == whoami)
                    {
                        EBPlayers.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        void OnChat(messageBuffer msg, int ply, string text, HandledEventArgs e)
        {
            if (!text.StartsWith("/"))
            {
                TSPlayer plr = TShock.Players[msg.whoAmI];
                if(EBUtils.IsPlayerMuted(plr.Name)) {
                    plr.SendMessage("You are muted!", Color.Red);
                    e.Handled = true;
                }
            }
        }

	}

}