using System;
using System.Collections.Generic;
using TShockAPI;
using TShockAPI.DB;

namespace ExtendedBans
{
    class EBCommands
    {
        internal static void Load()
        {
            TShockAPI.Commands.ChatCommands.Add(new Command("xbans", DoBanIP, "xbanip"));
            TShockAPI.Commands.ChatCommands.Add(new Command("xbans", DoBanPlayer, "xban"));
            TShockAPI.Commands.ChatCommands.Add(new Command("xbans", UnbanIP, "xunbanip"));
            TShockAPI.Commands.ChatCommands.Add(new Command("xbans", UnbanPlayer, "xunban"));
        }

        internal static void DoBanIP(CommandArgs args)
        {
            if (args.Parameters.Count > 1)
            {
                if (!EBUtils.IsValidIP(args.Parameters[0]))
                {
                    args.Player.SendMessage("This is not valid IP!", Color.Red);
                    return;
                }
                string[] ban = EBUtils.IPBanInfo(args.Parameters[0]);
                if (ban[0] == "banned")
                {
                    args.Player.SendMessage("IP mask is already banned.", Color.Red);
                    return;
                }
                string IP = args.Parameters[0];
                int BanDate = EBUtils.UnixTimestamp();
                int mins;
                bool permanent = false;
                int UnbanDate;
                if (System.Int32.TryParse(args.Parameters[1], out mins))
                {
                    UnbanDate = BanDate + mins * 60;
                }
                else
                {
                    UnbanDate = 0;
                    permanent = true;
                }
                string BannedBy = args.Player.Name;
                string Reason = EBUtils.JoinArgs(args, (permanent) ? 1 : 2);
                if (String.IsNullOrEmpty(Reason))
                {
                    Reason = "No reason.";
                }
                EBData.DB.QueryReader("INSERT INTO BannedIP (IP, BanDate, UnbanDate, BannedBy, Reason) VALUES ('" + IP + "', '" + BanDate + "', '" + UnbanDate + "', '" + BannedBy + "', '" + Reason + "')");
                EBData.DB.Dispose(); 
                List<EBPlayer> plrs = EBPlayer.GetPlayersByIPMask(IP);
                foreach (EBPlayer plr in plrs)
                {
                    TShock.Utils.ForceKick(plr.TSPlayer, "You have been banned: " + Reason);
                }
                args.Player.SendMessage(IP + " has been banned!.", Color.Yellow);
            }
            else
            {
                args.Player.SendMessage("Extended bans:", Color.Yellow);
                args.Player.SendMessage("/xban <player> [seconds] <reason>", Color.Yellow);
                args.Player.SendMessage("/xbanip <ip> [seconds] <reason>", Color.Yellow);
                args.Player.SendMessage("/xunbanip <ip>", Color.Yellow);
                args.Player.SendMessage("/xunban <player>", Color.Yellow);
            }
        }

        internal static void DoBanPlayer(CommandArgs args)
        {
            if (args.Parameters.Count > 1)
            {
                string[] ban = EBUtils.PlayerBanInfo(args.Parameters[0]);
                if (ban[0] == "banned")
                {
                    args.Player.SendMessage("Player is already banned.", Color.Red);
                    return;
                }
                string Player = args.Parameters[0];
                int BanDate = EBUtils.UnixTimestamp();
                int mins;
                bool permanent = false;
                int UnbanDate;
                if (System.Int32.TryParse(args.Parameters[1], out mins))
                {
                    UnbanDate = BanDate + mins * 60;
                }
                else
                {
                    UnbanDate = 0;
                    permanent = true;
                }
                string BannedBy = args.Player.Name;
                string Reason = EBUtils.JoinArgs(args, (permanent) ? 1 : 2);
                if (String.IsNullOrEmpty(Reason))
                {
                    Reason = "No reason.";
                }
                EBData.DB.QueryReader("INSERT INTO BannedPlayer (Player, BanDate, UnbanDate, BannedBy, Reason) VALUES ('" + Player + "', '" + BanDate + "', '" + UnbanDate + "', '" + BannedBy + "', '" + Reason + "')");
                EBData.DB.Dispose(); 
                List<EBPlayer> plrs = EBPlayer.GetPlayersByName(Player);
                foreach (EBPlayer plr in plrs)
                {
                    TShock.Utils.ForceKick(plr.TSPlayer, "You have been banned: " + Reason);
                }
                args.Player.SendMessage(Player + " has been banned!.", Color.Yellow);
            }
            else
            {
                args.Player.SendMessage("Extended bans:", Color.Yellow);
                args.Player.SendMessage("/xban <player> [seconds] <reason>", Color.Yellow);
                args.Player.SendMessage("/xbanip <ip> [seconds] <reason>", Color.Yellow);
                args.Player.SendMessage("/xunbanip <ip>", Color.Yellow);
                args.Player.SendMessage("/xunban <player>", Color.Yellow);
            }
        }

        internal static void UnbanIP(CommandArgs args)
        {
            if (args.Parameters.Count > 0)
            {
                if (!EBUtils.IsValidIP(args.Parameters[0]))
                {
                    args.Player.SendMessage("This is not valid IP!", Color.Red);
                    return;
                }
                if (!EBUtils.IsIPBanned(args.Parameters[0]))
                {
                    args.Player.SendMessage("IP is not banned!", Color.Red);
                    string[] ban = EBUtils.IPBanInfo(args.Parameters[0]);
                    if (ban[0] == "banned")
                    {
                        args.Player.SendMessage("This IP mathes to banned mask: " + ban[1], Color.Red);
                    }
                    return;
                }
                int now = EBUtils.UnixTimestamp();
                string IP = args.Parameters[0];
                EBData.DB.Query("UPDATE BannedIP SET UnbanDate = " + now + " WHERE IP = '" + IP + "' AND (UnbanDate>" + now + " OR UnbanDate = 0)");
                EBData.DB.Dispose();
                args.Player.SendMessage(IP + " has been unbanned.", Color.Yellow);
            }
            else
            {
                args.Player.SendMessage("Extended bans:", Color.Yellow);
                args.Player.SendMessage("/xban <player> [seconds] <reason>", Color.Yellow);
                args.Player.SendMessage("/xbanip <ip> [seconds] <reason>", Color.Yellow);
                args.Player.SendMessage("/xunbanip <ip>", Color.Yellow);
                args.Player.SendMessage("/xunban <player>", Color.Yellow);
            }
        }

        internal static void UnbanPlayer(CommandArgs args)
        {
            if (args.Parameters.Count > 0)
            {
                if (!EBUtils.IsPlayerBanned(args.Parameters[0]))
                {
                    args.Player.SendMessage("Player is not banned!", Color.Red);
                    return;
                }
                int now = EBUtils.UnixTimestamp();
                string Player = args.Parameters[0];
                EBData.DB.Query("UPDATE BannedPlayer SET UnbanDate = " + now + " WHERE Player = '" + Player + "' AND (UnbanDate>" + now + " OR UnbanDate = 0)");
                EBData.DB.Dispose();
                args.Player.SendMessage(Player + " has been unbanned.", Color.Yellow);
            }
            else
            {
                args.Player.SendMessage("Extended bans:", Color.Yellow);
                args.Player.SendMessage("/xban <player> [seconds] <reason>", Color.Yellow);
                args.Player.SendMessage("/xbanip <ip> [seconds] <reason>", Color.Yellow);
                args.Player.SendMessage("/xunbanip <ip>", Color.Yellow);
                args.Player.SendMessage("/xunban <player>", Color.Yellow);
            }
        }
    }
}
