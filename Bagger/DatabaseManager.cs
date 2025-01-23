using MySql.Data.MySqlClient;
using System.Data;
using TShockAPI;
using TShockAPI.DB;

namespace Bagger;

public class DatabaseManager
{

    private IDbConnection _db;

    public DatabaseManager(IDbConnection db)
    {
        _db = db;

        var sqlCreator = new SqlTableCreator(db, new SqliteQueryCreator());

        sqlCreator.EnsureTableStructure(new SqlTable("Players",
            new SqlColumn("PlayerID", MySqlDbType.Int32) { Primary = true, AutoIncrement = true },
            new SqlColumn("AccountID", MySqlDbType.Int32) { Unique = true },
            new SqlColumn("UUID", MySqlDbType.String) { Unique = true },
            new SqlColumn("ClaimedBossesMask", MySqlDbType.Int32)));
    }

    /// <exception cref="NullReferenceException"></exception>
    public BPlayer? GetBPlayer(int accountId)
    {
        using var reader = _db.QueryReader("SELECT * FROM Players WHERE AccountID = @0", accountId);

        while (reader.Read())
        {
            return new BPlayer(
                reader.Get<int>("PlayerID"),
                reader.Get<int>("AccountId"),
                reader.Get<string>("UUID"),
                reader.Get<int>("ClaimedBossesMask")
            );
        }

        return null;
    }

    public bool InsertPlayer(BPlayer bplr)
    {
        int uuidCount = _db.QueryScalar<int>("SELECT COUNT(*) FROM Players WHERE UUID = @0", bplr.UUID);
        if (uuidCount != 0) throw new DatabaseManagerDupticateUUIDException();
        return _db.Query("INSERT INTO Players (AccountID, UUID, ClaimedBossesMask) VALUES (@0, @1, @2)", bplr.AccountID, bplr.UUID, bplr.ClaimedMask) != 0;
    }

    public bool SavePlayer(BPlayer bplr)
    {
        return _db.Query("UPDATE Players SET ClaimedBossesMask = @0 WHERE AccountID = @1", bplr.ClaimedMask, bplr.AccountID) != 0;
    }

    public bool InsertOrSavePlayer(BPlayer bplr)
    {
        if (bplr.PlayerID < 0)
        {
            return InsertPlayer(bplr);
        }

        return SavePlayer(bplr);
    }

    public bool IsPlayerInDb(int accountId)
    {
        return _db.QueryScalar<int>("SELECT COUNT(*) FROM Players WHERE AccountID = @0", accountId) > 0;
    }

    public void ResetPlayers()
    {
        _db.Query("DELETE FROM Players");
    }
}