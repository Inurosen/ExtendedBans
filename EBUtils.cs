using System;
using System.Collections.Generic;
using System.Net;
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
            var DBQuery = EBData.DB.QueryReader("SELECT * FROM BannedIP WHERE UnbanDate>" + now + " OR UnbanDate = 0");
            while (DBQuery.Read())
            {
                string ipban = DBQuery.Reader.Get<string>("IP");
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
                                baninfo[1] = DBQuery.Reader.Get<string>("IP");
                                baninfo[2] = DBQuery.Reader.Get<int>("BanDate").ToString();
                                baninfo[3] = DBQuery.Reader.Get<int>("UnbanDate").ToString();
                                baninfo[4] = DBQuery.Reader.Get<string>("BannedBy");
                                baninfo[5] = DBQuery.Reader.Get<string>("Reason");
                                break;
                            }
                        }
                    }
                }
            }
            DBQuery.Connection.Dispose();
            return baninfo;
        }

        public static bool IsIPBanned(string ip)
        {
            bool yes = false;
            int now = UnixTimestamp();
            var DBQuery = EBData.DB.QueryReader("SELECT IP FROM BannedIP WHERE IP = '" + ip + "' AND (UnbanDate>" + now + " OR UnbanDate = 0)");
            while(DBQuery.Read())
            {
                if (ip == DBQuery.Reader.Get<string>("IP"))
                {
                    yes = true;
                    break;
                }
            }
            DBQuery.Connection.Dispose();
            return yes;
        }

        public static bool IsPlayerBanned(string plName)
        {
            bool yes = false;
            int now = UnixTimestamp();
            var DBQuery = EBData.DB.QueryReader("SELECT Player FROM BannedPlayer WHERE Player = '" + plName + "' AND (UnbanDate>" + now + " OR UnbanDate = 0)");
            while (DBQuery.Read())
            {
                if (plName == DBQuery.Reader.Get<string>("Player"))
                {
                    yes = true;
                    break;
                }
            }
            DBQuery.Connection.Dispose();
            return yes;
        }

        public static string[] PlayerBanInfo(string plName)
        {
            string[] baninfo = new string[6];
            int now = UnixTimestamp();
            var DBQuery = EBData.DB.QueryReader("SELECT * FROM BannedPlayer WHERE UnbanDate>" + now + " OR UnbanDate = 0");
            while (DBQuery.Read())
            {
                string plBanned = DBQuery.Reader.Get<string>("Player");
                if (plBanned == plName)
                {
                    baninfo[0] = "banned";
                    baninfo[1] = DBQuery.Reader.Get<string>("Player");
                    baninfo[2] = DBQuery.Reader.Get<int>("BanDate").ToString();
                    baninfo[3] = DBQuery.Reader.Get<int>("UnbanDate").ToString();
                    baninfo[4] = DBQuery.Reader.Get<string>("BannedBy");
                    baninfo[5] = DBQuery.Reader.Get<string>("Reason");
                    break;
                           
                }
            }
            DBQuery.Connection.Dispose();
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

    }
}
