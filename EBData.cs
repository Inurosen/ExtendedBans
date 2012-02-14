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
        public static IDbConnection DB;
        public static SqlTableCreator SQLWriter;

        public static void InitXBansDB()
        {
            string sql = Path.Combine(EBDB);
            if (!File.Exists(EBDB))
            {
                SqliteConnection.CreateFile(EBDB);
            }
            DB = new SqliteConnection(string.Format("uri=file://{0},Version=3", sql));
            SQLWriter = new SqlTableCreator(DB, new SqliteQueryCreator());
            var table = new SqlTable("BannedIP",
            new SqlColumn("IP", MySqlDbType.Text) { Unique = true },
            new SqlColumn("BanDate", MySqlDbType.Int32),
            new SqlColumn("UnbanDate", MySqlDbType.Int32),
            new SqlColumn("BannedBy", MySqlDbType.Text),
            new SqlColumn("Reason", MySqlDbType.Text)
            );
            SQLWriter.EnsureExists(table);
            table = new SqlTable("BannedPlayer",
            new SqlColumn("Player", MySqlDbType.Text) { Unique = true },
            new SqlColumn("BanDate", MySqlDbType.Int32),
            new SqlColumn("UnbanDate", MySqlDbType.Int32),
            new SqlColumn("BannedBy", MySqlDbType.Text),
            new SqlColumn("Reason", MySqlDbType.Text)
            );
            SQLWriter.EnsureExists(table);
        }
    }
}
