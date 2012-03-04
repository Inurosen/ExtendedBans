using System.Data;
using System.IO;
using Mono.Data.Sqlite;
using MySql.Data.MySqlClient;
using TShockAPI.DB;

namespace ExtendedBans
{
    class EBData
    {
        public static string EBDB = Path.Combine(ExtendedBans.EBDir, "xbans.sqlite");
        public static SqlTableCreator SQLWriter;
        public static SqliteConnection DBSqlite;
        public static MySqlConnection DBMysql;

        public static void InitXBansDB()
        {
            if (!EBConfig.UseMysql)
            {
                string sql = Path.Combine(EBDB);
                if (!File.Exists(EBDB))
                {
                    SqliteConnection.CreateFile(EBDB);
                }
                DBSqlite = new SqliteConnection(string.Format("uri=file://{0},Version=3", sql));
                CheckTables(DBSqlite);
                DBSqlite.Dispose();
            }
            else
            {
                DBMysql = new MySqlConnection(string.Format("Data Source={0};User Id={1};Password={2}", EBConfig.MysqlHost, EBConfig.MysqlLogin, EBConfig.MysqlPassword));
                string CMD = string.Format("CREATE DATABASE IF NOT EXISTS {0}", EBConfig.MysqlDatabase);
                RunExec(CMD);
                DBMysql = new MySqlConnection(string.Format("Database={0};Data Source={1};User Id={2};Password={3}", EBConfig.MysqlDatabase, EBConfig.MysqlHost, EBConfig.MysqlLogin, EBConfig.MysqlPassword));
                CheckTables(DBMysql);
                ImportToMysql();
            }
        }

        internal static void CheckTables(IDbConnection connection)
        {
            SQLWriter = new SqlTableCreator(connection, new MysqlQueryCreator());
            var table = new SqlTable("BannedIP",
            new SqlColumn("IP", MySqlDbType.Text),
            new SqlColumn("BanDate", MySqlDbType.Int32),
            new SqlColumn("UnbanDate", MySqlDbType.Int32),
            new SqlColumn("BannedBy", MySqlDbType.Text),
            new SqlColumn("Reason", MySqlDbType.Text)
            );
            SQLWriter.EnsureExists(table);
            table = new SqlTable("BannedPlayer",
            new SqlColumn("Player", MySqlDbType.Text),
            new SqlColumn("BanDate", MySqlDbType.Int32),
            new SqlColumn("UnbanDate", MySqlDbType.Int32),
            new SqlColumn("BannedBy", MySqlDbType.Text),
            new SqlColumn("Reason", MySqlDbType.Text)
            );
            SQLWriter.EnsureExists(table);
            table = new SqlTable("MutedPlayer",
            new SqlColumn("Player", MySqlDbType.Text),
            new SqlColumn("MuteDate", MySqlDbType.Int32),
            new SqlColumn("UnmuteDate", MySqlDbType.Int32),
            new SqlColumn("MutedBy", MySqlDbType.Text),
            new SqlColumn("Reason", MySqlDbType.Text)
);
            SQLWriter.EnsureExists(table);
        }

        public static void RunExec(string query)
        {
            if (EBConfig.UseMysql)
            {
                DBMysql.Open();
                var CMD = new MySqlCommand(query, DBMysql);
                CMD.ExecuteNonQuery();
                DBMysql.Close();

            }
            else
            {
                DBSqlite.Query(query);
                DBSqlite.Dispose();
            }
        }

        public static MySqlDataReader RunMysqlQuery(string query)
        {
            MySqlDataReader result;
            DBMysql.Open();
            var CMD = new MySqlCommand(query, DBMysql);
            result = CMD.ExecuteReader();
            return result;
        }

        internal static void ImportToMysql()
        {
            string sql = Path.Combine(EBDB);
            if (File.Exists(EBDB))
            {
                string[] baninfo = new string[5];
                DBSqlite = new SqliteConnection(string.Format("uri=file://{0},Version=3", sql));
                var DBQuery = DBSqlite.QueryReader("SELECT * FROM BannedIP");
                while (DBQuery.Read())
                {
                    baninfo[0] = DBQuery.Get<string>("IP");
                    baninfo[1] = DBQuery.Get<int>("BanDate").ToString();
                    baninfo[2] = DBQuery.Get<int>("UnbanDate").ToString();
                    baninfo[3] = DBQuery.Get<string>("BannedBy");
                    baninfo[4] = DBQuery.Get<string>("Reason");
                    RunExec("INSERT INTO BannedIP (IP, BanDate, UnbanDate, BannedBy, Reason) VALUES ('" + baninfo[0] + "', '" + int.Parse(baninfo[1]) + "', '" + int.Parse(baninfo[2]) + "', '" + baninfo[3] + "', '" + baninfo[4] + "')");
                }
                DBSqlite.Dispose();
                DBQuery.Dispose();
                DBQuery = DBSqlite.QueryReader("SELECT * FROM BannedPlayer");
                while (DBQuery.Read())
                {
                    baninfo[0] = DBQuery.Get<string>("Player");
                    baninfo[1] = DBQuery.Get<int>("BanDate").ToString();
                    baninfo[2] = DBQuery.Get<int>("UnbanDate").ToString();
                    baninfo[3] = DBQuery.Get<string>("BannedBy");
                    baninfo[4] = DBQuery.Get<string>("Reason");
                    RunExec("INSERT INTO BannedPlayer (Player, BanDate, UnbanDate, BannedBy, Reason) VALUES ('" + baninfo[0] + "', '" + int.Parse(baninfo[1]) + "', '" + int.Parse(baninfo[2]) + "', '" + baninfo[3] + "', '" + baninfo[4] + "')");
                }
                DBSqlite.Dispose();
                DBQuery.Dispose();
                DBQuery = DBSqlite.QueryReader("SELECT * FROM MutedPlayer");
                while (DBQuery.Read())
                {
                    baninfo[0] = DBQuery.Get<string>("Player");
                    baninfo[1] = DBQuery.Get<int>("MuteDate").ToString();
                    baninfo[2] = DBQuery.Get<int>("UnmuteDate").ToString();
                    baninfo[3] = DBQuery.Get<string>("BannedBy");
                    baninfo[4] = DBQuery.Get<string>("MutedBy");
                    RunExec("INSERT INTO MutedPlayer (Player, MuteDate, UnmuteDate, MutedBy, Reason) VALUES ('" + baninfo[0] + "', '" + int.Parse(baninfo[1]) + "', '" + int.Parse(baninfo[2]) + "', '" + baninfo[3] + "', '" + baninfo[4] + "')");
                }
                DBSqlite.Dispose();
                DBSqlite.Close();
                DBQuery.Dispose();
                File.Delete(EBDB);
            }
        }
    }
}
