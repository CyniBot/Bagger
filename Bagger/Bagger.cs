using Microsoft.Data.Sqlite;
using System.Data;
using Terraria;
using Terraria.ID;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace Bagger
{
    [ApiVersion(2, 1)]
    public class Bagger : TerrariaPlugin
    {
        public Bagger(Main game) : base(game)
        {
        }
        public override string Name => "Bagger";
        public override Version Version => new Version(1, 3, 0);
        public override string Author => "Soofa";
        public override string Description => "Gives people boss bags if they missed the fight.";

        private static IDbConnection db = new SqliteConnection("Data Source=" + Path.Combine(TShock.SavePath, "Bagger.sqlite"));
        public static DatabaseManager dbManager = new DatabaseManager(db);
        public static Config Config = Config.Reload();
        private List<int> DownedBosses = new List<int>();


        public override void Initialize()
        {
            ServerApi.Hooks.GamePostInitialize.Register(this, OnGamePostInitialize);
            ServerApi.Hooks.NpcKilled.Register(this, OnNpcKilled);
            GeneralHooks.ReloadEvent += OnReload;

            Commands.ChatCommands.Add(new Command("bagger.getbags", GetBagsCmd, "getbags", "gb")
            {
                AllowServer = false,
                HelpText = "Get bags for the bosses that you've missed."
            });

            Commands.ChatCommands.Add(new Command("bagger.reset", ResetDatabaseCmd, "resetbagger")
            {
                AllowServer = true,
                HelpText = "Resets the Bagger's database."
            });
        }


        private void OnGamePostInitialize(EventArgs args)
        {
            if (IsDefeated(NPCID.KingSlime)) { DownedBosses.Add(NPCID.KingSlime); }
            if (IsDefeated(NPCID.EyeofCthulhu)) { DownedBosses.Add(NPCID.EyeofCthulhu); }
            if (IsDefeated(NPCID.EaterofWorldsHead)) { DownedBosses.Add(NPCID.EaterofWorldsHead); }
            if (IsDefeated(NPCID.BrainofCthulhu)) { DownedBosses.Add(NPCID.BrainofCthulhu); }
            if (IsDefeated(NPCID.QueenBee)) { DownedBosses.Add(NPCID.QueenBee); }
            if (IsDefeated(NPCID.SkeletronHead)) { DownedBosses.Add(NPCID.SkeletronHead); }
            if (IsDefeated(NPCID.Deerclops)) { DownedBosses.Add(NPCID.Deerclops); }
            if (IsDefeated(NPCID.WallofFlesh)) { DownedBosses.Add(NPCID.WallofFlesh); }
            if (IsDefeated(NPCID.QueenSlimeBoss)) { DownedBosses.Add(NPCID.QueenSlimeBoss); }
            if (IsDefeated(NPCID.TheDestroyer)) { DownedBosses.Add(NPCID.TheDestroyer); }
            if (IsDefeated(NPCID.Spazmatism)) { DownedBosses.Add(NPCID.Spazmatism); }
            if (IsDefeated(NPCID.SkeletronPrime)) { DownedBosses.Add(NPCID.SkeletronPrime); }
            if (IsDefeated(NPCID.Plantera)) { DownedBosses.Add(NPCID.Plantera); }
            if (IsDefeated(NPCID.Golem)) { DownedBosses.Add(NPCID.Golem); }
            if (IsDefeated(NPCID.DukeFishron)) { DownedBosses.Add(NPCID.DukeFishron); }
            if (IsDefeated(NPCID.HallowBoss)) { DownedBosses.Add(NPCID.HallowBoss); }
            if (IsDefeated(NPCID.DD2Betsy)) { DownedBosses.Add(NPCID.DD2Betsy); }
            if (IsDefeated(NPCID.MoonLordCore)) { DownedBosses.Add(NPCID.MoonLordCore); }
        }

        private void OnNpcKilled(NpcKilledEventArgs args)
        {
            if (!args.npc.boss || DownedBosses.Contains(args.npc.type) || !IsDefeated(args.npc.type))
            {
                return;
            }

            DownedBosses.Add(args.npc.type);


            if (!Config.AllowClaimsForContributers)
            {
                foreach (TSPlayer plr in TShock.Players)
                {
                    if (plr != null && plr.Active)
                    {
                        try
                        {
                            BPlayer bplr = dbManager.GetBPlayer(plr.Account.ID) ?? BPlayer.CreateUnregisteredBPlayer(plr);

                            bplr.UpdateMask(args.npc.type);
                            dbManager.InsertOrSavePlayer(bplr);
                        }
                        catch (DatabaseManagerDupticateUUIDException)
                        {
                            plr.SendErrorMessage("[Bagger] Seems like this UUID is used for another account.");
                        }
                    }
                }
            }
        }

        private bool IsDefeated(int type)
        {
            var unlockState = Main.BestiaryDB.FindEntryByNPCID(type).UIInfoProvider.GetEntryUICollectionInfo().UnlockState;
            return unlockState == Terraria.GameContent.Bestiary.BestiaryEntryUnlockState.CanShowDropsWithDropRates_4;
        }

        private void GetBagsCmd(CommandArgs args)
        {
            BPlayer? bplr = null;

            bplr = dbManager.GetBPlayer(args.Player.Account.ID);

            if (bplr == null)
            {
                try
                {
                    dbManager.InsertPlayer(BPlayer.CreateUnregisteredBPlayer(args.Player));
                    bplr = dbManager.GetBPlayer(args.Player.Account.ID)!;
                }
                catch (DatabaseManagerDupticateUUIDException)
                {
                    args.Player.SendErrorMessage("[Bagger] Seems like this UUID is used for another account.");
                    return;
                }
            }

            if ((bplr.ClaimedMask & 1) != 1 && DownedBosses.Contains(NPCID.KingSlime))
            {
                bplr.ClaimedMask |= 1;
                args.Player.GiveItem(Config.KingSlimeDrop.ItemID, Config.KingSlimeDrop.Stack);
            }

            if ((bplr.ClaimedMask & 2) != 2 && DownedBosses.Contains(NPCID.EyeofCthulhu))
            {
                bplr.ClaimedMask |= 2;
                args.Player.GiveItem(Config.EyeOFCthulhuDrop.ItemID, Config.EyeOFCthulhuDrop.Stack);
            }

            if ((bplr.ClaimedMask & 4) != 4 && DownedBosses.Contains(NPCID.EaterofWorldsHead))
            {
                bplr.ClaimedMask |= 4;
                args.Player.GiveItem(Config.EaterOfWorldsDrop.ItemID, Config.EaterOfWorldsDrop.Stack);
            }

            if ((bplr.ClaimedMask & 8) != 8 && DownedBosses.Contains(NPCID.BrainofCthulhu))
            {
                bplr.ClaimedMask |= 8;
                args.Player.GiveItem(Config.BrainOfCthulhuDrop.ItemID, Config.BrainOfCthulhuDrop.Stack);
            }

            if ((bplr.ClaimedMask & 16) != 16 && DownedBosses.Contains(NPCID.QueenBee))
            {
                bplr.ClaimedMask |= 16;
                args.Player.GiveItem(Config.QueenBeeDrop.ItemID, Config.QueenBeeDrop.Stack);
            }

            if ((bplr.ClaimedMask & 32) != 32 && DownedBosses.Contains(NPCID.SkeletronHead))
            {
                bplr.ClaimedMask |= 32;
                args.Player.GiveItem(Config.SkeletronDrop.ItemID, Config.SkeletronDrop.Stack);
            }

            if ((bplr.ClaimedMask & 64) != 64 && DownedBosses.Contains(NPCID.Deerclops))
            {
                bplr.ClaimedMask |= 64;
                args.Player.GiveItem(Config.Deerclops.ItemID, Config.Deerclops.Stack);
            }

            if ((bplr.ClaimedMask & 128) != 128 && DownedBosses.Contains(NPCID.WallofFlesh))
            {
                bplr.ClaimedMask |= 128;
                args.Player.GiveItem(Config.WallOfFleshDrop.ItemID, Config.WallOfFleshDrop.Stack);
            }

            if ((bplr.ClaimedMask & 256) != 256 && DownedBosses.Contains(NPCID.QueenSlimeBoss))
            {
                bplr.ClaimedMask |= 256;
                args.Player.GiveItem(Config.QueenSlimeDrop.ItemID, Config.QueenSlimeDrop.Stack);
            }

            if ((bplr.ClaimedMask & 512) != 512 && DownedBosses.Contains(NPCID.TheDestroyer))
            {
                bplr.ClaimedMask |= 512;
                args.Player.GiveItem(Config.TheDestroyerDrop.ItemID, Config.TheDestroyerDrop.Stack);
            }

            if ((bplr.ClaimedMask & 1024) != 1024 && DownedBosses.Contains(NPCID.Spazmatism))
            {
                bplr.ClaimedMask |= 1024;
                args.Player.GiveItem(Config.TheTwinsDrop.ItemID, Config.TheTwinsDrop.Stack);
            }

            if ((bplr.ClaimedMask & 2048) != 2048 && DownedBosses.Contains(NPCID.SkeletronPrime))
            {
                bplr.ClaimedMask |= 2048;
                args.Player.GiveItem(Config.SkeletronPrimeDrop.ItemID, Config.SkeletronPrimeDrop.Stack);
            }

            if ((bplr.ClaimedMask & 4096) != 4096 && DownedBosses.Contains(NPCID.Plantera))
            {
                bplr.ClaimedMask |= 4096;
                args.Player.GiveItem(Config.PlanteraDrop.ItemID, Config.PlanteraDrop.Stack);
            }

            if ((bplr.ClaimedMask & 8192) != 8192 && DownedBosses.Contains(NPCID.Golem))
            {
                bplr.ClaimedMask |= 8192;
                args.Player.GiveItem(Config.GolemDrop.ItemID, Config.GolemDrop.Stack);
            }

            if ((bplr.ClaimedMask & 16384) != 16384 && DownedBosses.Contains(NPCID.DukeFishron))
            {
                bplr.ClaimedMask |= 16384;
                args.Player.GiveItem(Config.DukeFishronDrop.ItemID, Config.DukeFishronDrop.Stack);
            }

            if ((bplr.ClaimedMask & 32768) != 32768 && DownedBosses.Contains(NPCID.HallowBoss))
            {
                bplr.ClaimedMask |= 32768;
                args.Player.GiveItem(Config.EmpressOfLight.ItemID, Config.EmpressOfLight.Stack);
            }

            if ((bplr.ClaimedMask & 65536) != 65536 && DownedBosses.Contains(NPCID.DD2Betsy))
            {
                bplr.ClaimedMask |= 65536;
                args.Player.GiveItem(Config.BetsyDrop.ItemID, Config.BetsyDrop.Stack);
            }


            if ((bplr.ClaimedMask & 131072) != 131072 && DownedBosses.Contains(NPCID.MoonLordCore))
            {
                bplr.ClaimedMask |= 131072;
                args.Player.GiveItem(Config.MoonlordDrop.ItemID, Config.MoonlordDrop.Stack);
            }

            dbManager.SavePlayer(bplr);
            args.Player.SendSuccessMessage("Successfully claimed the bags.");
        }

        private void ResetDatabaseCmd(CommandArgs args)
        {
            dbManager.ResetPlayers();
            args.Player.SendSuccessMessage("[Bagger] Database has been reset successfully.");
            TShock.Log.Info($"{args.Player.Name} with {args.Player.Account.Name} account name has reset the database.");
        }
        private static void OnReload(ReloadEventArgs args)
        {
            Config = Config.Reload();
            args.Player.SendSuccessMessage("[Bagger] has been reloaded.");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.GamePostInitialize.Deregister(this, OnGamePostInitialize);
                ServerApi.Hooks.NpcKilled.Deregister(this, OnNpcKilled);
                GeneralHooks.ReloadEvent -= OnReload;
            }
            base.Dispose(disposing);
        }
    }
}
