using Terraria.ID;
using TShockAPI;

namespace Bagger;


public class BPlayer
{
    public int PlayerID { get; init; }
    public int AccountID { get; init; }
    public string UUID { get; set; }
    public int ClaimedMask { get; set; }


    public BPlayer(int playerId, int accountId, string uuid, int claimedMask)
    {
        PlayerID = playerId;
        AccountID = accountId;
        UUID = uuid;
        ClaimedMask = claimedMask;
    }

    public static BPlayer CreateUnregisteredBPlayer(TSPlayer plr)
    {
        return new BPlayer(
            -1,
            plr.Account.ID,
            plr.UUID,
            0
        );
    }

    public void UpdateMask(int type)
    {
        ClaimedMask = type switch
        {
            NPCID.KingSlime => ClaimedMask | 0b_1,
            NPCID.EyeofCthulhu => ClaimedMask | 0b_10,
            NPCID.EaterofWorldsHead or NPCID.EaterofWorldsBody or NPCID.EaterofWorldsTail => ClaimedMask | 0b_100,
            NPCID.BrainofCthulhu => ClaimedMask | 0b_1000,
            NPCID.QueenBee => ClaimedMask | 0b_1_0000,
            NPCID.SkeletronHead => ClaimedMask | 0b_10_0000,
            NPCID.Deerclops => ClaimedMask | 0b_100_0000,
            NPCID.WallofFlesh => ClaimedMask | 0b_1000_0000,
            NPCID.QueenSlimeBoss => ClaimedMask | 0b_1_0000_0000,
            NPCID.TheDestroyer => ClaimedMask | 0b_10_0000_0000,
            NPCID.Spazmatism or NPCID.Retinazer => ClaimedMask | 0b_100_0000_0000,
            NPCID.SkeletronPrime => ClaimedMask | 0b_1000_0000_0000,
            NPCID.Plantera => ClaimedMask | 0b_1_0000_0000_0000,
            NPCID.Golem => ClaimedMask | 0b_10_0000_0000_0000,
            NPCID.DukeFishron => ClaimedMask | 0b_100_0000_0000_0000,
            NPCID.HallowBoss => ClaimedMask | 0b_1000_0000_0000_0000,
            NPCID.DD2Betsy => ClaimedMask | 0b_1_0000_0000_0000_0000_0000,
            NPCID.MoonLordCore => ClaimedMask | 0b_10_0000_0000_0000_0000_0000,
            _ => ClaimedMask
        };
    }
}
