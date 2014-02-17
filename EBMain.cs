using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Terraria;
using TShockAPI;
using TerrariaApi.Server;
using TShockAPI.DB;
using TShockAPI.Net;
using TShockAPI.Hooks;

namespace ExtendedBans
{
    [ApiVersion(1, 15)]
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
            Order = 5;
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
                ServerApi.Hooks.NetGreetPlayer.Register(this, OnGreetPlayer);
                ServerApi.Hooks.ServerConnect.Register(this, OnConnect);
                ServerApi.Hooks.ServerJoin.Register(this, OnJoin);
                ServerApi.Hooks.ServerLeave.Register(this, OnLeave);
                ServerApi.Hooks.ServerChat.Register(this, OnChat);
            }
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (InitConfig)
                {
                    ServerApi.Hooks.NetGreetPlayer.Deregister(this, OnGreetPlayer);
                    ServerApi.Hooks.ServerConnect.Deregister(this, OnConnect);
                    ServerApi.Hooks.ServerJoin.Deregister(this, OnJoin);
                    ServerApi.Hooks.ServerLeave.Deregister(this, OnLeave);
                    ServerApi.Hooks.ServerChat.Deregister(this, OnChat);
                }
            }
            base.Dispose(disposing);
        }
        public override Version Version
        {
            get { return new Version("1.14.0210"); }
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

        private void OnConnect(ConnectEventArgs e)
        {
            var player = new TSPlayer(e.Who);
            if (player == null)
            {
                e.Handled = true;
                return;
            }

            string[] ban = EBUtils.IPBanInfo(player.IP);

            if (ban[0] == "banned")
            {
                TShock.Utils.Kick(player, string.Format("You are banned: {0}", ban[5]), true, true, null, true);
                e.Handled = true;
                return;
            }
        }

        private void OnJoin(JoinEventArgs e)
        {
            var player = TShock.Players[e.Who];
            if (player == null)
            {
                e.Handled = true;
                return;
            }

            string[] ban = EBUtils.PlayerBanInfo(player.Name);

            if (ban[0] == "banned")
            {
                TShock.Utils.Kick(player, string.Format("You are banned: {0}", ban[5]), true, true, null, true);
                e.Handled = true;
                return;
            }
        }

        public void OnGreetPlayer(GreetPlayerEventArgs e)
        {
            lock (EBPlayers)
                EBPlayers.Add(new EBPlayer(e.Who));
        }

        public void OnLeave(LeaveEventArgs e)
        {
            lock (EBPlayers)
            {
                for (int i = 0; i < EBPlayers.Count; i++)
                {
                    if (EBPlayers[i].Index == e.Who)
                    {
                        EBPlayers.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        void OnChat(ServerChatEventArgs e)
        {
            string text = e.Text;
            if (!text.StartsWith("/") || text.StartsWith("/me"))
            {
                TSPlayer plr = TShock.Players[e.Who];
                if (EBUtils.IsPlayerMuted(plr.Name))
                {
                    plr.SendMessage("You are muted!", Color.Red);
                    e.Handled = true;
                }
            }
        }

    }

}