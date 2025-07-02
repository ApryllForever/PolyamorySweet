using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Netcode;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Delegates;
using StardewValley.Locations;
using StardewValley.Triggers;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using xTile;
using xTile.Layers;
using xTile.Tiles;

namespace PolyamorySweetLove
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry
    {
        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            var sc = Helper.ModRegistry.GetApi<ISpaceCoreApi>("spacechase0.SpaceCore");

            sc.RegisterSerializerType(typeof(PolyamoryLocation));

            sc.RegisterSerializerType(typeof(LantanaLagoon));

            sc.RegisterSerializerType(typeof(LantanaTemple));

            // sc.RegisterCustomProperty(typeof(NPC), "WeddingDate", typeof(int), AccessTools.Method(typeof (NPC_WeddingDate), nameof(NPC_WeddingDate.WeddingDate)), AccessTools.Method(typeof(NPC_WeddingDate), nameof(NPC_WeddingDate.set_WeddingDate)));

            TriggerActionManager.RegisterAction("Mermaid_Hug", ModEntry.Hug);

            // get Generic Mod Config Menu's API (if it's installed)
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            // register mod
            configMenu.Register(
                mod: ModManifest,
                reset: () => Config = new ModConfig(),
                save: () => Helper.WriteConfig(Config)
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Mod Enabled?",
                getValue: () => Config.EnableMod,
                setValue: value => Config.EnableMod = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Min Points To Marry",
                getValue: () => Config.MinPointsToMarry,
                setValue: value => Config.MinPointsToMarry = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Min Points To Date",
                getValue: () => Config.MinPointsToDate,
                setValue: value => Config.MinPointsToDate = value
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Prevent Hostile Divorces",
                getValue: () => Config.PreventHostileDivorces,
                setValue: value => Config.PreventHostileDivorces = value
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Complex Divorces",
                getValue: () => Config.ComplexDivorce,
                setValue: value => Config.ComplexDivorce = value
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Roommate Romance",
                getValue: () => Config.RoommateRomance,
                setValue: value => Config.RoommateRomance = value
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Max children",
                getValue: () => Config.MaxChildren,
                setValue: value => Config.MaxChildren = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Show Parent Names",
                getValue: () => Config.ShowParentNames,
                setValue: value => Config.ShowParentNames = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Buy Pendants Anytime",
                getValue: () => Config.BuyPendantsAnytime,
                setValue: value => Config.BuyPendantsAnytime = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Pendant Price",
                getValue: () => Config.PendantPrice,
                setValue: value => Config.PendantPrice = value
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Percent Chance For Spouse to Be In Bed",
                getValue: () => Config.PercentChanceForSpouseInBed,
                setValue: value => Config.PercentChanceForSpouseInBed = value,
                min: 0,
                max: 100
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Percent Chance For Spouse to Be In Kitchen",
                getValue: () => Config.PercentChanceForSpouseInKitchen,
                setValue: value => Config.PercentChanceForSpouseInKitchen = value,
                min: 0,
                max: 100
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Percent Chance For Spouse to Be In Patio",
                getValue: () => Config.PercentChanceForSpouseAtPatio,
                setValue: value => Config.PercentChanceForSpouseAtPatio = value,
                min: 0,
                max: 100
            );

            configMenu.AddNumberOption(
              mod: ModManifest,
              name: () => "Percent Chance For Pregnancy Question (0.0 - 1)",
              getValue: () => Config.PercentChanceForBirthingQuestion,
              setValue: value => Config.PercentChanceForBirthingQuestion = value,
               tooltip: () => "Sets the chance for a birthing question at night. 1 is 100 percent."
          );

            configMenu.AddNumberOption(
            mod: ModManifest,
            name: () => "Percent Chance For Birth Sex to be a Girl (0.0 - 1)",
            getValue: () => Config.PercentChanceForBirthSex,
            setValue: value => Config.PercentChanceForBirthSex = value,
             tooltip: () => "Sets whether the next baby will be a girl. Default is 0.6, a round number reflecting population norms."
        );


            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Impregnating Mother",
                getValue: () => Config.ImpregnatingMother,
                setValue: value => Config.ImpregnatingMother = value,
                tooltip: () => "Allows a female farmer to impregnate her wife. Must be set to false if Impregnating Femme NPC is set to true."
            );

            configMenu.AddBoolOption(
               mod: ModManifest,
               name: () => "Impregnating Femme NPC",
               getValue: () => Config.ImpregnatingFemmeNPC,
               setValue: value => Config.ImpregnatingFemmeNPC = value,
               tooltip: () => "Allows a female Farmer to get impregnated by her wife. Must be set to false if Impregnating Mother is set to true."
           );
        LoadModApis();
        }

        private void GameLoop_ReturnedToTitle(object sender, StardewModdingAPI.Events.ReturnedToTitleEventArgs e)
        {
            currentSpouses.Clear();
            currentUnofficialSpouses.Clear();
        }
        public static void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            SetAllNPCsDatable(); //What the hell have I done here? What is this???
            ResetSpouses(Game1.player);
        }

        public static void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
        {
            ResetDivorces();

            foreach (Farmer f in Game1.getAllFarmers())
            {
                ResetSpouses(f, true);
                var spouses = GetSpouses(f, false).Keys;
                foreach (string s in spouses)
                {
                    SMonitor.Log($"{f.Name} is married to {s}");
                }
            }
            
            BabyTonight = false;
            BabyTonightSpouse = String.Empty;
            AphroditeFlowerGiven = false;
            PatioPlacement = false;
            PorchPlacement = false;

            //Helps me figure out what day it is in game, for placement purposes specifically
            SMonitor.Log("PSL - Current Date is "+Game1.Date.ToString() +", day is "+Game1.dayOfMonth.ToString());

          


            FixSpouseSpawnLocations();

            //
            //The purpose of this below was for starting the wedding; it cannot without a StartEvent command, so this is just not yet useful. 
            //
           // if(Game1.Date == farmerFarmhandWedding)
           // {
           //     getWeddingEvent();
           //     ModEntry.proposalSender = null;
           //     ModEntry.proposalReceiver = null;
           // }

            foreach (Farmer farmer in Game1.getAllFarmers())
            {
                PlacementCheck(farmer);
            }

        }

        public static void GameLoop_DayEnding(object sender, DayEndingEventArgs e)
        {
            // Encountered a situation where my primary spouse got changed from my engaged spouse
            // That is a problem as that will stop the wedding from occurring
            // Since I'm unsure what caused it, I'm slapping a band-aid check here
            foreach (Farmer farmer in Game1.getAllFarmers())
            {
                foreach (string npc in farmer.friendshipData.Keys)
                {
                    if (farmer.friendshipData[npc].IsEngaged())
                    {
                        SMonitor.Log($"Setting primary spouse for {farmer} to engaged NPC {npc} (was {farmer.spouse})");
                        farmer.spouse = npc;
                        break;
                    }
                }


                //Below is to set the PatioSpouse, who was not set before.
                //
               /*
                * 
                * Do not use yet. 
                * 
                {
                    List<NPC> allSpouses = GetSpouses(farmer, false).Values.ToList();
                    int index = Game1.random.Next(allSpouses.Count);

                    NPC babe = allSpouses[index];

                    // if (Utility.getRandomDouble(0.0, 0.99) < Config.PercentChanceForSpouseAtPatio)            
                    NPCPatches.PatioSpouse = babe;
                    SMonitor.Log($"PSL - Setting patio spouse for {farmer.Name} to "+ NPCPatches.PatioSpouse.Name +".");
                }*/
            }
        }

        // This method is to check if spouses are missing, placed in the void, etc, and if they are on the same tile
        //
        //
        public static void PlacementCheck(Farmer f)
        {

            FarmHouse farmHouse = Utility.getHomeOfFarmer(f);

            Farm farm = new Farm();

            List<NPC> allSpouses = GetSpouses(f, false).Values.ToList();

            Point position = farmHouse.getRandomOpenPointInHouse(myRand);

            if (allSpouses.Count == 0)
            {
                SMonitor.Log("No spouses found for "+f.Name +", ignoring Spouse Placement Check.");
                return;
            }

            if (allSpouses.Count == 1)
            {
                SMonitor.Log("One spouse found for " + f.Name + ", ignoring Spouse Placement Check.");
                return;
            }


            // This was something I was looking into to try to check if two spouses were in the same place, may eventually remove.
            //
            Dictionary<NPC,Point> spousePositionDict = new Dictionary<NPC,Point>();

            foreach (NPC npc in allSpouses)
            {
                spousePositionDict.Add(npc, npc.TilePoint);
                
            }

            foreach (KeyValuePair<NPC, Point> spousePos in spousePositionDict)
            {
                SMonitor.Log("PSL - Spouse Position: " + spousePos.Key.Name + " at " + spousePos.Value.ToString()+ " in " +spousePos.Key.currentLocation.ToString()+".");
            }

            foreach (NPC npc in allSpouses)
            {
                if (spousePositionDict.ContainsKey(npc) )
                {
                    spousePositionDict.Remove(npc);
                }

                foreach (KeyValuePair<NPC, Point> spousePos in spousePositionDict)
                {
                    if(spousePos.Value.Equals(npc.TilePoint))
                    {
                        Game1.warpCharacter(npc, farmHouse, new Vector2(position.X, position.Y));
                        SMonitor.Log(spousePos.Key.Name+" "+ spousePos.Value.ToString()+"was overlapping with "+npc.Name+npc.Tile.ToString()+".");
                    }
                }
            }

            /*
            foreach (NPC spouse in allSpouses)
            {
                if (spouse is null)
                    continue;
                SMonitor.Log("PSL - initiating spouse placement for" + spouse.Name);
                Point position = farmHouse.getRandomOpenPointInHouse(myRand);

                if (farm.Equals(spouse.currentLocation))
                {
                    SMonitor.Log("PSL - " + spouse.Name + " should be on the Patio."); // Marisol was to be patio, was warped to farmhouse
                    continue;
                }

                //THis is making patio placement difficult
                //if (!farmHouse.Equals(spouse.currentLocation))
                //{
                //    Game1.warpCharacter(spouse, farmHouse, new Vector2(position.X, position.Y));
                //    spouse.faceDirection(myRand.Next(0, 4));
                //    SMonitor.Log("PSL - Warping NPC from where ever to Random Open Place - " + spouse.Name);
                //}

                if(farm.Equals(spouse.currentLocation))
                {
                    SMonitor.Log("PSL - " + spouse.Name + " should be on the Patio."); // Marisol was to be patio, was warped to farmhouse
                }

                if(spouse.Position.X <= 1)
                {
                    Game1.warpCharacter(spouse, farmHouse, new Vector2(position.X, position.Y));
                    spouse.faceDirection(myRand.Next(0, 4));
                    SMonitor.Log("PSL - Warping NPC from The Void to Random Open Place - " + spouse.Name);
                }
                if (spouse.Position.Y <= 1)
                {
                    Game1.warpCharacter(spouse, farmHouse, new Vector2(position.X, position.Y));
                    spouse.faceDirection(myRand.Next(0, 4));
                    SMonitor.Log("PSL - Warping NPC from The Void to Random Open Place - " + spouse.Name);
                }
                if(NPC.checkTileOccupancyForSpouse(farmHouse, spouse.Tile,spouse.Name) == false)
                {
                    Game1.warpCharacter(spouse, spouse.currentLocation, new Vector2(position.X, position.Y));
                    spouse.faceDirection(myRand.Next(0, 4));
                    SMonitor.Log("PSL - Warping NPC from occupied tile to Random Open Place - " + spouse.Name); // Problem: This is warping liuterally everyone, including the patio spouse
                }
              


            }
            */

        }


        public static bool Hug(string[] args, TriggerActionContext context, out string error)
        {
            if (!ArgUtility.TryGet(args, 1, out string name, out error, allowBlank: false))
                return false;


            NPC npc = Game1.getCharacterFromName(name);
           // name = npc.Name;
            int spouseFrame = 28;


            bool flip = (npc.FacingDirection == 3);

            ModEntry.SMonitor.Log($"Can hug for Dialogue {npc.Name}");

            int delay = Game1.IsMultiplayer ? 1000 : 1000;
            npc.movementPause = delay;
            npc.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
                    {
                        new FarmerSprite.AnimationFrame(spouseFrame, delay, false, flip, new AnimatedSprite.endOfAnimationBehavior(npc.haltMe), true)
                    }
            );

            ModEntry.SMonitor.Log($"Hugging {npc.Name}");

            ModEntry.mp.broadcastSprites(npc.currentLocation, new TemporaryAnimatedSprite[]
           {
                    new TemporaryAnimatedSprite("LooseSprites\\emojis", new Rectangle(0, 0, 9, 9), 2000f, 1, 0, new Vector2(npc.Tile.X, npc.Tile.Y) * 64f + new Vector2(16f, -64f), false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
                    {
                        motion = new Vector2(0f, -0.5f),
                        alphaFade = 0.01f
                    }
           });


            Game1.player.exhausted.Value = false;
            npc.hasBeenKissedToday.Value = true;
            npc.Sprite.UpdateSourceRect();

            int playerFaceDirection = 1;
            if ( ( flip))
            {
                playerFaceDirection = 3;
            }
            Game1.player.PerformKiss(playerFaceDirection);

            return true;
        }

        public static void GameLoop_OneSecondUpdateTicked(object sender, OneSecondUpdateTickedEventArgs e)
        {
            if (!Config.EnableMod)
                return;

            foreach (GameLocation location in GetAllLocations())
            {

                if (location is FarmHouse fh)
                {
                    if (fh.owner == null)
                        continue;

                    List<string> allSpouses = GetSpouses(fh.owner, false).Keys.ToList();
                    List<string> bedSpouses = ReorderSpousesForSleeping(allSpouses.FindAll((s) => Config.RoommateRomance || !fh.owner.friendshipData[s].RoommateMarriage));

                    using (IEnumerator<NPC> characters = fh.characters.GetEnumerator())
                    {
                        while (characters.MoveNext())
                        {
                            var character = characters.Current;
                            if (!(character.currentLocation == fh))
                            {
                                character.farmerPassesThrough = false;
                                character.HideShadow = false;
                                character.isSleeping.Value = false;
                                continue;
                            }

                            if (allSpouses.Contains(character.Name))
                            {

                                if (IsInBed(fh, character.GetBoundingBox()))
                                {
                                    character.farmerPassesThrough = true;

                                    if (!character.isMoving() && (kissingAPI == null || kissingAPI.LastKissed(character.Name) < 0 || kissingAPI.LastKissed(character.Name) > 2))
                                    {
                                        Vector2 bedPos = GetSpouseBedPosition(fh, character.Name);
                                        if (Game1.timeOfDay >= 2000 || Game1.timeOfDay <= 600)
                                        {
                                            character.position.Value = bedPos;

                                            if (Game1.timeOfDay >= 2200)
                                            {
                                                character.ignoreScheduleToday = true;
                                            }
                                            if (!character.isSleeping.Value)
                                            {
                                                character.isSleeping.Value = true;

                                            }
                                            if (character.Sprite.CurrentAnimation == null)
                                            {
                                                if (!HasSleepingAnimation(character.Name))
                                                {
                                                    character.Sprite.StopAnimation();
                                                    character.faceDirection(0);
                                                }
                                                else
                                                {
                                                    character.playSleepingAnimation();
                                                }
                                            }
                                        }
                                        else
                                        {
                                            character.faceDirection(3);
                                            character.isSleeping.Value = false;
                                        }
                                    }
                                    else
                                    {
                                        character.isSleeping.Value = false;
                                    }
                                    character.HideShadow = true;
                                }
                                else if (Game1.timeOfDay < 2000 && Game1.timeOfDay > 600)
                                {
                                    character.farmerPassesThrough = false;
                                    character.HideShadow = false;
                                    character.isSleeping.Value = false;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}