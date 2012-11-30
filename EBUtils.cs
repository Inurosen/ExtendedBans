using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Net;
using System.Data;
using TShockAPI;
using TShockAPI.DB;

namespace ExtendedBans
{
    class EBUtils
    {
        public static int UnixTimestamp()
        {
            int unixTime = (int)(System.DateTime.UtcNow - new System.DateTime(1970, 1, 1)).TotalSeconds;
            return unixTime;
        }

        public static string[] IPBanInfo(string ip)
        {
            string[] baninfo = new string[6];            
            if (!IsValidIP(ip.Replace("*", "255")))
            {
                return baninfo;
            }
            int now = UnixTimestamp();
            string[] ipnets = ip.Split('.');
            IDataReader DBQuery;
            if (EBConfig.UseMysql)
            {
                DBQuery = EBData.DBMysql.QueryReader("SELECT * FROM BannedIP WHERE UnbanDate>@0 OR UnbanDate = 0", now).Reader;
            }
            else
            {
                DBQuery = EBData.DBSqlite.QueryReader("SELECT * FROM BannedIP WHERE UnbanDate>@0 OR UnbanDate = 0", now).Reader;
            }
            while (DBQuery.Read())
            {
                string ipban = DBQuery.Get<string>("IP");
                string[] nets = ipban.Split('.');
                if (nets[0] == ipnets[0] || nets[0] == "*")
                {
                    if (nets[1] == ipnets[1] || nets[1] == "*")
                    {
                        if (nets[2] == ipnets[2] || nets[2] == "*")
                        {
                            if (nets[3] == ipnets[3] || nets[3] == "*")
                            {
                                baninfo[0] = "banned";
                                baninfo[1] = DBQuery.Get<string>("IP");
                                baninfo[2] = DBQuery.Get<int>("BanDate").ToString();
                                baninfo[3] = DBQuery.Get<int>("UnbanDate").ToString();
                                baninfo[4] = DBQuery.Get<string>("BannedBy");
                                baninfo[5] = DBQuery.Get<string>("Reason");
                                break;
                            }
                        }
                    }
                }
            }
            if (!EBConfig.UseMysql) { EBData.DBSqlite.Dispose(); } else { EBData.DBMysql.Close(); }
            DBQuery.Dispose();
            return baninfo;
        }

        public static bool IsIPBanned(string ip)
        {
            bool yes = false;
            int now = UnixTimestamp();
            IDataReader DBQuery;
            if (EBConfig.UseMysql)
            {
                DBQuery = EBData.DBMysql.QueryReader("SELECT IP FROM BannedIP WHERE IP = @0 AND (UnbanDate>@1 OR UnbanDate = 0)", ip, now).Reader;
            }
            else
            {
                DBQuery = EBData.DBSqlite.QueryReader("SELECT IP FROM BannedIP WHERE IP = @0 AND (UnbanDate>@1 OR UnbanDate = 0)", ip, now).Reader;
            } 
            while(DBQuery.Read())
            {
                if (ip == DBQuery.Get<string>("IP"))
                {
                    yes = true;
                    break;
                }
            }
            if (!EBConfig.UseMysql) { EBData.DBSqlite.Dispose(); } else { EBData.DBMysql.Close(); }
            DBQuery.Dispose();
            return yes;
        }

        public static bool IsPlayerBanned(string plName)
        {
            bool yes = false;
            int now = UnixTimestamp();
            IDataReader DBQuery;
            if (EBConfig.UseMysql)
            {
                DBQuery = EBData.DBMysql.QueryReader("SELECT Player FROM BannedPlayer WHERE LOWER(Player) = @0 AND (UnbanDate>@1 OR UnbanDate = 0)", plName.ToLower(), now).Reader;
            }
            else
            {
                DBQuery = EBData.DBSqlite.QueryReader("SELECT Player FROM BannedPlayer WHERE LOWER(Player) = @0 AND (UnbanDate>@1 OR UnbanDate = 0)", plName.ToLower(), now).Reader;
            }
            while (DBQuery.Read())
            {
                if (plName.ToLower() == DBQuery.Get<string>("Player").ToLower())
                {
                    yes = true;
                    break;
                }
            }
            if (!EBConfig.UseMysql) { EBData.DBSqlite.Dispose(); } else { EBData.DBMysql.Close(); }
            DBQuery.Dispose();
            return yes;
        }

        public static string[] PlayerBanInfo(string plName)
        {
            string[] baninfo = new string[6];
            int now = UnixTimestamp();
            IDataReader DBQuery;
            if (EBConfig.UseMysql)
            {
                DBQuery = EBData.DBMysql.QueryReader("SELECT * FROM BannedPlayer WHERE UnbanDate>@0 OR UnbanDate = 0", now).Reader;
            }
            else
            {
                DBQuery = EBData.DBSqlite.QueryReader("SELECT * FROM BannedPlayer WHERE UnbanDate>@0 OR UnbanDate = 0", now).Reader;
            }
            while (DBQuery.Read())
            {
                string plBanned = DBQuery.Get<string>("Player");
                if (plBanned.ToLower() == plName.ToLower())
                {
                    baninfo[0] = "banned";
                    baninfo[1] = DBQuery.Get<string>("Player");
                    baninfo[2] = DBQuery.Get<int>("BanDate").ToString();
                    baninfo[3] = DBQuery.Get<int>("UnbanDate").ToString();
                    baninfo[4] = DBQuery.Get<string>("BannedBy");
                    baninfo[5] = DBQuery.Get<string>("Reason");
                    break;
                           
                }
            }
            if (!EBConfig.UseMysql) { EBData.DBSqlite.Dispose(); } else { EBData.DBMysql.Close(); }
            DBQuery.Dispose();
            return baninfo;
        }

        public static bool IsValidIP(string addr)
        {
            IPAddress ip;
            bool valid = false;
            if (string.IsNullOrEmpty(addr))
            {

                valid = false;
            }
            else
            {
                valid = IPAddress.TryParse(addr.Replace("*", "255"), out ip);
            }
            return valid;
        }

        public static string JoinArgs(CommandArgs args, int StartPoint)
        {
            string result = "";
            List<string> reason = new List<string>();
            for(int i = StartPoint; i < args.Parameters.Count; i++)
            {
                reason.Add(args.Parameters[i]);
            }
            result = String.Join(" ", reason.ToArray());
            return result;
        }

        public static TimeSpan ParseTimeSpan(string s)
        {
            const string Quantity = "quantity";
            const string Unit = "unit";

            const string Days = @"(d(ays?)?)";
            const string Hours = @"(h((ours?)|(rs?))?)";
            const string Minutes = @"(m((inutes?)|(ins?))?)";
            const string Seconds = @"(s((econds?)|(ecs?))?)";

            Regex timeSpanRegex = new Regex(
                string.Format(@"\s*(?<{0}>\d+)\s*(?<{1}>({2}|{3}|{4}|{5}|\Z))",
                              Quantity, Unit, Days, Hours, Minutes, Seconds),
                              RegexOptions.IgnoreCase);
            MatchCollection matches = timeSpanRegex.Matches(s);
            int l;
            TimeSpan ts = new TimeSpan();
            if (!Int32.TryParse(s.Substring(0, 1), out l))
            {
                return ts;
            }
            foreach (Match match in matches)
            {
                if (Regex.IsMatch(match.Groups[Unit].Value, @"\A" + Days))
                {
                    ts = ts.Add(TimeSpan.FromDays(double.Parse(match.Groups[Quantity].Value)));
                }
                else if (Regex.IsMatch(match.Groups[Unit].Value, Hours))
                {
                    ts = ts.Add(TimeSpan.FromHours(double.Parse(match.Groups[Quantity].Value)));
                }
                else if (Regex.IsMatch(match.Groups[Unit].Value, Minutes))
                {
                    ts = ts.Add(TimeSpan.FromMinutes(double.Parse(match.Groups[Quantity].Value)));
                }
                else if (Regex.IsMatch(match.Groups[Unit].Value, Seconds))
                {
                    ts = ts.Add(TimeSpan.FromSeconds(double.Parse(match.Groups[Quantity].Value)));
                }
                else
                {
                    ts = ts.Add(TimeSpan.FromMinutes(double.Parse(match.Groups[Quantity].Value)));
                }
            }
            return ts;
        }

        public static bool IsPlayerMuted(string plName)
        {
            bool yes = false;
            int now = UnixTimestamp();
            IDataReader DBQuery;
            if (EBConfig.UseMysql)
            {
                DBQuery = EBData.DBMysql.QueryReader("SELECT Player FROM MutedPlayer WHERE LOWER(Player) = @0 AND (UnmuteDate>@1 OR UnmuteDate = 0)", plName.ToLower(), now).Reader;
            }
            else
            {
                DBQuery = EBData.DBSqlite.QueryReader("SELECT Player FROM MutedPlayer WHERE LOWER(Player) = @0 AND (UnmuteDate>@1 OR UnmuteDate = 0)", plName.ToLower(), now).Reader;
            }
            while (DBQuery.Read())
            {
                if (plName.ToLower() == DBQuery.Get<string>("Player").ToLower())
                {
                    yes = true;
                    break;
                }
            }
            if (!EBConfig.UseMysql) { EBData.DBSqlite.Dispose(); } else { EBData.DBMysql.Close(); }
            DBQuery.Dispose();
            return yes;
        }

    }
}
