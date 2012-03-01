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
            TShockAPI.Commands.ChatCommands.Add(new Command("xmute", DoMute, "xmute"));
            TShockAPI.Commands.ChatCommands.Add(new Command("xmute", UnMute, "xunmute"));
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
                bool permanent = false;
                int UnbanDate;
                TimeSpan t = EBUtils.ParseTimeSpan(args.Parameters[1]);
                if (t.TotalSeconds > 0)
                {
                    UnbanDate = BanDate + (int)t.TotalSeconds;
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
                string Player = args.Parameters[0]; 
                List<EBPlayer> plrs = EBPlayer.GetPlayersByName(Player);
                if (plrs.Count > 1)
                {
                    args.Player.SendMessage("There are 2 or more players matching to " + Player, Color.Orange);
                    return;
                }
                else if (plrs.Count == 1)
                {
                    foreach (EBPlayer plr in plrs)
                    {
                        Player = plr.TSPlayer.Name;
                    }
                }
                if (EBUtils.IsPlayerBanned(args.Parameters[0]))
                {
                    args.Player.SendMessage("Player is already banned.", Color.Red);
                    return;
                }
                int BanDate = EBUtils.UnixTimestamp();
                bool permanent = false;
                int UnbanDate;
                TimeSpan t = EBUtils.ParseTimeSpan(args.Parameters[1]);
                if (t.TotalSeconds > 0)
                {
                    UnbanDate = BanDate + (int)t.TotalSeconds;
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
                TShock.Utils.ForceKick(plrs[0].TSPlayer, "You have been banned: " + Reason);
                args.Player.SendMessage(Player + " has been banned!", Color.Yellow);
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
                EBData.DB.Query("UPDATE BannedPlayer SET UnbanDate = " + now + " WHERE LOWER(Player) = '" + Player.ToLower() + "' AND (UnbanDate>" + now + " OR UnbanDate = 0)");
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

        internal static void DoMute(CommandArgs args)
        {
            if (args.Parameters.Count > 1)
            {
                string Player = args.Parameters[0];
                List<EBPlayer> plrs = EBPlayer.GetPlayersByName(Player);
                if (plrs.Count > 1)
                {
                    args.Player.SendMessage("There are 2 or more players matching to " + Player, Color.Orange);
                    return;
                }
                else if (plrs.Count == 1)
                {
                    foreach (EBPlayer plr in plrs)
                    {
                        Player = plr.TSPlayer.Name;
                    }
                }
                if (EBUtils.IsPlayerMuted(Player))
                {
                    args.Player.SendMessage("Player is already muted.", Color.Red);
                    return;
                }

                int MuteDate = EBUtils.UnixTimestamp();
                bool permanent = false;
                int UnmuteDate;
                TimeSpan t = EBUtils.ParseTimeSpan(args.Parameters[1]);
                if (t.TotalSeconds > 0)
                {
                    UnmuteDate = MuteDate + (int)t.TotalSeconds;
                }
                else
                {
                    UnmuteDate = 0;
                    permanent = true;
                }
                string MutedBy = args.Player.Name;
                string Reason = EBUtils.JoinArgs(args, (permanent) ? 1 : 2);
                if (String.IsNullOrEmpty(Reason))
                {
                    Reason = "No reason.";
                }

                EBData.DB.QueryReader("INSERT INTO MutedPlayer (Player, MuteDate, UnmuteDate, MutedBy, Reason) VALUES ('" + Player + "', '" + MuteDate + "', '" + UnmuteDate + "', '" + MutedBy + "', '" + Reason + "')");
                EBData.DB.Dispose();
                plrs[0].TSPlayer.SendMessage("You have been muted: " + Reason, Color.Yellow);
                args.Player.SendMessage(Player + " has been muted!.", Color.Yellow);
            }
            else
            {
                args.Player.SendMessage("Extended mute:", Color.Yellow);
                args.Player.SendMessage("/xmute <player> [seconds] <reason>", Color.Yellow);
                args.Player.SendMessage("/xunmute <player>", Color.Yellow);
            }
        }

        internal static void UnMute(CommandArgs args)
        {
            if (args.Parameters.Count > 0)
            {
                string Player = args.Parameters[0];
                List<EBPlayer> plrs = EBPlayer.GetPlayersByName(Player);
                if (plrs.Count > 1)
                {
                    args.Player.SendMessage("There are 2 or more players matching to " + Player, Color.Orange);
                    return;
                }
                else if (plrs.Count == 1)
                {
                    foreach (EBPlayer plr in plrs)
                    {
                        Player = plr.TSPlayer.Name;
                    }
                } 
                if (!EBUtils.IsPlayerMuted(Player))
                {
                    args.Player.SendMessage("Player is not muted!", Color.Red);
                    return;
                }
                int now = EBUtils.UnixTimestamp();
                EBData.DB.Query("UPDATE MutedPlayer SET UnmuteDate = " + now + " WHERE LOWER(Player) = '" + Player.ToLower() + "' AND (UnmuteDate>" + now + " OR UnmuteDate = 0)");
                EBData.DB.Dispose();
                plrs[0].TSPlayer.SendMessage("You have been unmuted.", Color.Yellow);
                args.Player.SendMessage(Player + " has been unmuted.", Color.Yellow);
            }
            else
            {
                args.Player.SendMessage("Extended mute:", Color.Yellow);
                args.Player.SendMessage("/xmute <player> [seconds] <reason>", Color.Yellow);
                args.Player.SendMessage("/xunmute <player>", Color.Yellow);
            }
        }
    }
}
