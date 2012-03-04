using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ExtendedBans
{
    public class EBConfigFile : Dictionary<string, string>
    {
        private static string CConfigFile = Path.Combine(ExtendedBans.EBDir, "xbans.cfg"); 
        public bool Load()
        {
            string[] file;
            if (File.Exists(CConfigFile))
            {
                file = File.ReadAllLines(CConfigFile);
            }
            else
            {
                CreateConfig();
                file = File.ReadAllLines(CConfigFile);
            }
            foreach (var t in file.Where(t => !t.StartsWith("//") && !t.StartsWith("#")).Where(t => t.Split('=').Length == 2))
            {
                Add(t.Split('=')[0].ToLower(), t.Split('=')[1]);
            }
            return CheckKeys();
        }

        private bool CheckKeys()
        {
            var ismysql = false;
            var host = false;
            var login = false;
            var passwd = false;
            var database = false;
            foreach (var pair in this)
            {
                switch (pair.Key)
                {
                    case "usemysql":
                        bool lmsg;
                        if (bool.TryParse(pair.Value, out lmsg))
                        {
                            ismysql = true;

                        }
                        break;
                    case "mysqlhost":
                        host = true;
                        break;
                    case "mysqllogin":
                        login = true;
                        break;
                    case "mysqlpassword":
                        passwd = true;
                        break;
                    case "mysqldatabase":
                        database = true;
                        break;
                }
            }
            return (ismysql && host && login && passwd && database);
        }
        private bool CreateConfig()
        {
            if (!File.Exists(CConfigFile))
            {
                List<string> contents = new List<string>();
                contents.Add("# Mysql connection config.");
                contents.Add("UseMysql=false");
                contents.Add("MysqlHost=localhost");
                contents.Add("MysqlLogin=root");
                contents.Add("MysqlPassword=");
                contents.Add("MysqlDatabase=EBDB");
                File.WriteAllLines(CConfigFile, contents.ToArray());
            }
            return true;
        }
    }

    public class EBConfig
    {
        public static bool UseMysql;
        public static string MysqlHost;
        public static string MysqlLogin;
        public static string MysqlPassword;
        public static string MysqlDatabase;
    }
}