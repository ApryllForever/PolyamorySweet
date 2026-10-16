using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Netcode;
using PolyamorySweetLove;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Delegates;
using StardewValley.Locations;
using StardewValley.Network;
using StardewValley.Triggers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using xTile;
using xTile.Dimensions;
using xTile.Layers;
using xTile.Tiles;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;

namespace PolyamorySweetLove
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry
    {
       // static bool FinalStageCheck = false;
       // public static bool Check610 = false;
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
                name: () => Helper.Translation.Get("config.enable-mod.name"),
                getValue: () => Config.EnableMod,
                setValue: value => Config.EnableMod = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get("config.min-points-to-marry.name"),
                getValue: () => Config.MinPointsToMarry,
                setValue: value => Config.MinPointsToMarry = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get("config.min-points-to-date.name"),
                getValue: () => Config.MinPointsToDate,
                setValue: value => Config.MinPointsToDate = value
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get("config.prevent-hostile-divorces.name"),
                getValue: () => Config.PreventHostileDivorces,
                setValue: value => Config.PreventHostileDivorces = value
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get("config.complex-divorce.name"),
                getValue: () => Config.ComplexDivorce,
                setValue: value => Config.ComplexDivorce = value
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get("config.roommate-romance.name"),
                getValue: () => Config.RoommateRomance,
                setValue: value => Config.RoommateRomance = value
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get("config.max-children.name"),
                getValue: () => Config.MaxChildren,
                setValue: value => Config.MaxChildren = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get("config.show-parent-names.name"),
                getValue: () => Config.ShowParentNames,
                setValue: value => Config.ShowParentNames = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get("config.buy-pendants-anytime.name"),
                getValue: () => Config.BuyPendantsAnytime,
                setValue: value => Config.BuyPendantsAnytime = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get("config.pendant-price.name"),
                getValue: () => Config.PendantPrice,
                setValue: value => Config.PendantPrice = value
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get("config.percent-chance-for-spouse-in-bed.name"),
                getValue: () => Config.PercentChanceForSpouseInBed,
                setValue: value => Config.PercentChanceForSpouseInBed = value,
                min: 0,
                max: 100
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get("config.percent-chance-for-spouse-in-kitchen.name"),
                getValue: () => Config.PercentChanceForSpouseInKitchen,
                setValue: value => Config.PercentChanceForSpouseInKitchen = value,
                min: 0,
                max: 100
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get("config.percent-chance-for-spouse-at-patio.name"),
                getValue: () => Config.PercentChanceForSpouseAtPatio,
                setValue: value => Config.PercentChanceForSpouseAtPatio = value,
                min: 0,
                max: 100
            );

            configMenu.AddNumberOption(
              mod: ModManifest,
              name: () => Helper.Translation.Get("config.percent-chance-for-birthing-question.name"),
              getValue: () => Config.PercentChanceForBirthingQuestion,
              setValue: value => Config.PercentChanceForBirthingQuestion = value,
               tooltip: () => Helper.Translation.Get("config.percent-chance-for-birthing-question.tooltip")
          );

            configMenu.AddNumberOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("config.percent-chance-for-birth-sex.name"),
            getValue: () => Config.PercentChanceForBirthSex,
            setValue: value => Config.PercentChanceForBirthSex = value,
             tooltip: () => Helper.Translation.Get("config.percent-chance-for-birth-sex.tooltip")
        );


            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get("config.impregnating-mother.name"),
                getValue: () => Config.ImpregnatingMother,
                setValue: value => Config.ImpregnatingMother = value,
                tooltip: () => Helper.Translation.Get("config.impregnating-mother.tooltip")
            );

            configMenu.AddBoolOption(
               mod: ModManifest,
               name: () => Helper.Translation.Get("config.impregnating-femme-n-p-c.name"),
               getValue: () => Config.ImpregnatingFemmeNPC,
               setValue: value => Config.ImpregnatingFemmeNPC = value,
               tooltip: () => Helper.Translation.Get("config.impregnating-femme-n-p-c.tooltip")
           );
        LoadModApis();
        }

        private void GameLoop_ReturnedToTitle(object sender, StardewModdingAPI.Events.ReturnedToTitleEventArgs e)
        {
            //FinalStageCheck = false;
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
          //  FinalStageCheck = false;
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

            if (Game1.IsMasterGame)
            {
                SHelper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked_SpousePlacement;

            }

            //Trying to squish the persistent bug
           // FixSpouseSpawnLocations();

            
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

            if (Game1.Date == farmerFarmhandWedding)
            {
                getWeddingEvent();
                Game1.currentLocation.startEvent(getWeddingEvent());

            }

        }

        private static void GameLoop_UpdateTicked_SpousePlacement(object sender, UpdateTickedEventArgs e)
        {
            foreach (Farmer farmer in Game1.getAllFarmers())
            {
                PlaceSpousesInFarmhouses(Game1.RequireLocation<FarmHouse>(farmer.homeLocation.Value, false), farmer);
            }
            Game1.getFarm().addSpouseOutdoorArea(Game1.player.spouse == null ? "" : Game1.player.spouse);
            SHelper.Events.GameLoop.UpdateTicked -= GameLoop_UpdateTicked_SpousePlacement;
        }

        public static void GameLoop_DayEnding(object sender, DayEndingEventArgs e)
        {
            //FinalStageCheck = false;

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

        public static void PlaceSpousesInFarmhouses(FarmHouse farmHouse, Farmer farmer)
        {
            if (farmHouse == null || farmer == null)
                return;



            List<NPC> allSpouses = GetSpouses(farmer, false).Values.ToList();

            Point position = ModEntry.getRandomOpenPoint(farmHouse, myRand, 0, 120);

            if (allSpouses.Count == 0)
            {
                SMonitor.Log("No spouses found for "+farmer.Name +", ignoring Spouse Placement Check.");
                return;
            }

            NPC porchWife = new NPC();
            NPC kitchenWife = new NPC();
            NPC bedWife = new NPC();
            NPC patioWife = new NPC();

            Farmer gettingMarriedFarmer = null;
            string gettingMarriedNPC = null;
            if (WeddingToday != null)
            {
                gettingMarriedFarmer = WeddingToday.farmer;
                gettingMarriedNPC = gettingMarriedFarmer?.spouse;
                WeddingToday = null;
            }

            ShuffleList(ref allSpouses);


            List<string> bedSpouses = new List<string>();
            string kitchenSpouse = null;
            string porchSpouse = null;

            foreach (NPC spouse in allSpouses)
            {
                if (spouse is null)
                    continue;
                
                
                if (!farmHouse.Equals(spouse.currentLocation))
                {
                    if (!Game1.getFarm().Equals(spouse.currentLocation))
                    {
                        SMonitor.Log($"PSL - {spouse.Name} is not in farmhouse, is at {spouse.currentLocation.Name}");
                        continue;
                    }

                    if (gettingMarriedFarmer == farmer && gettingMarriedNPC == spouse.Name)
                    {
                        SMonitor.Log($"PSL - It is {spouse.Name}'s wedding day, staying on porch");
                        porchWife = spouse;
                    }
                    else if (gettingMarriedFarmer == farmer && gettingMarriedNPC != null && spouse.Name != gettingMarriedNPC && spouse.currentLocation == Game1.getFarm() && spouse.TilePoint == farmHouse.getPorchStandingSpot())
                    {
                        // If current farmer is getting married today, no other spouse can be on the porch
                        SMonitor.Log($"PSL - {spouse.Name} is on the porch on {gettingMarriedNPC}'s wedding day");

                        Point newPoint = new(-1, -1);
                        for (int i = 0; i < 100; i++)
                        {
                            newPoint = ModEntry.getRandomOpenPoint(farmHouse, myRand, tries: 100);
                            if (!isFarmHouseTileOccupied(farmHouse, newPoint, spouse.Name))
                                break;
                        }
                        if (newPoint != Point.Zero)
                        {
                            SMonitor.Log($"PSL - Moving {spouse.Name} to {newPoint}");
                            Game1.warpCharacter(spouse, farmer.homeLocation.Value, newPoint);
                            spouse.faceDirection(myRand.Next(4));
                        }


                    }

                }
                int type = myRand.Next(0, 100);

                SMonitor.Log($"PSL - spouse random int {type}, bed %: {Config.PercentChanceForSpouseInBed}, kitchen %: {Config.PercentChanceForSpouseInKitchen}");

                if (type < Config.PercentChanceForSpouseInBed)
                {
                    if (bedSpouses.Count < 1 && (Config.RoommateRomance || !farmer.friendshipData[spouse.Name].IsRoommate()) && HasSleepingAnimation(spouse.Name))
                    {
                        SMonitor.Log("PSL - bed spouse: " + spouse.Name);
                        bedSpouses.Add(spouse.Name);
                    }

                }
                else if (type < Config.PercentChanceForSpouseInBed + Config.PercentChanceForSpouseInKitchen)
                {
                    if (kitchenSpouse == null)
                    {
                        SMonitor.Log("PSL -  kitchen spouse: " + spouse.Name);
                        kitchenSpouse = spouse.Name;
                    }
                }
                else if (type < Config.PercentChanceForSpouseInBed + Config.PercentChanceForSpouseInKitchen + Config.PercentChanceForSpouseAtPatio)
                {
                    if (!Game1.isRaining && !Game1.IsWinter && !Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth).Equals("Sat") && !spouse.Name.Equals("Krobus") && spouse.Schedule == null)
                    {
                        SMonitor.Log("PSL - patio spouse: " + spouse.Name);
                        spouse.setUpForOutdoorPatioActivity();
                        SMonitor.Log($"PSL - {spouse.Name} at {spouse.currentLocation.Name} {spouse.TilePoint} as Patio Spouse");
                    }
                }
            }

            foreach (NPC spouse in allSpouses)
            {
                if (spouse is null)
                    continue;
                if (spouse.currentLocation is not FarmHouse)
                {
                    SMonitor.Log($"PSL - {spouse.Name} is in {spouse.currentLocation.Name}, not Farmhouse");
                    continue;
                }

                Point spouseRoomSpot = position;

                if (sweetRoomsAPI  != null)
                {

                    spouseRoomSpot = sweetRoomsAPI.GetSpouseTile(spouse);
                    if (spouseRoomSpot.X >= 0)
                        SMonitor.Log($"PSL - Room RoomSpot Placement for {spouse}: {spouseRoomSpot} ");
                }
                if ( farmer.spouse == spouse.Name)
                {
                    spouseRoomSpot = farmHouse.GetSpouseRoomSpot();
                    SMonitor.Log($"PSL - {spouse} is Farmer Spouse: using default spouse spot {spouseRoomSpot}");
                }

                SMonitor.Log($"PSL - Placing {spouse} in farm house");
                spouse.shouldPlaySpousePatioAnimation.Value = false;

                Vector2 bedPos = GetSpouseBedPosition(farmHouse, spouse.Name);

                if ((bedSpouses.Count > 0 && bedSpouses.Contains(spouse.Name) && bedPos != Vector2.Zero) || IsInBed(farmHouse, spouse.GetBoundingBox()))
                {
                   // SMonitor.Log($"PSL - Placing {spouse.Name} in bed");
                    //spouse.position.Value = GetSpouseBedPosition(farmHouse, spouse.Name);

                    // Game code would have placed all bed spouses in the same place. Move them to their spot
                    Point bedSpot = ModEntry.GetSpouseBedEndPoint(farmHouse, spouse.Name);
                    SMonitor.Log($"PSL - {spouse.Name} is in bed. Moving to {bedSpot}");
                    spouse.setTilePosition(bedSpot);
                }

                else if (kitchenSpouse == spouse.Name && !IsTileOccupied(farmHouse, farmHouse.getKitchenStandingSpot(), spouse.Name))
                {
                    SMonitor.Log($"PSL - Placing {spouse.Name} in kitchen");

                    spouse.setTilePosition(farmHouse.getKitchenStandingSpot());
                    spouse.setRandomAfternoonMarriageDialogue(Game1.timeOfDay, farmHouse, false);
                }
                else if (spouseRoomSpot.X > -1 && !IsTileOccupied(farmHouse, spouseRoomSpot, spouse.Name))
                {
                    SMonitor.Log($"PSL - {spouse.Name} is in their room");
                    spouse.setTilePosition(spouseRoomSpot);
                    spouse.setSpouseRoomMarriageDialogue();
                }
                else
                {
                    Point point = new(-1, -1);
                    for (int i = 0; i < 100; i++)
                    {
                        point = farmHouse.getRandomOpenPointInHouse(myRand);
                        if (!IsTileOccupied(farmHouse, point, spouse.Name))
                            break;
                    }
                    spouse.setTilePosition(point);
                    spouse.faceDirection(myRand.Next(0, 4));
                    SMonitor.Log($"PSL - Placing {spouse.Name} on {spouse.TilePoint} Randomly");
                    spouse.setRandomAfternoonMarriageDialogue(Game1.timeOfDay, farmHouse, false);
                }
            }

        }

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
                    for (int i = 0; i < 100; i++)
                    {
                        position = farmHouse.getRandomOpenPointInHouse(myRand);
                        if (!IsTileOccupied(farmHouse, position, npc.Name))
                            break;
                    }

                    if (spousePos.Value.Equals(npc.TilePoint))
                    {
                        Point position2 = farmHouse.getRandomOpenPointInHouse(myRand);
                        for (int i = 0; i < 100; i++)
                        {
                            position2 = farmHouse.getRandomOpenPointInHouse(myRand);
                            if (!IsTileOccupied(farmHouse, position, npc.Name))
                                break;
                        }

                        SMonitor.Log("PSL - Placement Check: "+spousePos.Key.Name+" "+ spousePos.Value.ToString()+"was overlapping with "+npc.Name+npc.Tile.ToString()+".");
                        Game1.warpCharacter(npc, farmHouse, new Vector2(position2.X, position2.Y));
                        SMonitor.Log("PSL - Placement Check Fix: Moving "+spousePos.Key.Name+" to "+ spousePos.Value.ToString()+".");
                    }
                }
            }

            
            foreach (NPC spouse in allSpouses)
            {
                if (spouse is null)
                    continue;

                for (int i = 0; i < 100; i++)
                {
                    position = farmHouse.getRandomOpenPointInHouse(myRand);
                    if (!IsTileOccupied(farmHouse, position, spouse.Name))
                        break;
                }

                SMonitor.Log("PSL - initiating spouse placement Check for" + spouse.Name);
               // Point position = farmHouse.getRandomOpenPointInHouse(myRand);

                if (farm.Equals(spouse.currentLocation))
                {
                    SMonitor.Log("PSL - " + spouse.Name + " should be on the Patio or Porch"); // Marisol was to be patio, was warped to farmhouse
                    continue;
                }

                //THis is making patio placement difficult
                //if (!farmHouse.Equals(spouse.currentLocation))
                //{
                //    Game1.warpCharacter(spouse, farmHouse, new Vector2(position.X, position.Y));
                //    spouse.faceDirection(myRand.Next(0, 4));
                //    SMonitor.Log("PSL - Warping NPC from where ever to Random Open Place - " + spouse.Name);
                //}

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
               // if(NPC.checkTileOccupancyForSpouse(farmHouse, spouse.Tile,spouse.Name) == false)
               // {
                //    Game1.warpCharacter(spouse, spouse.currentLocation, new Vector2(position.X, position.Y));
                //    spouse.faceDirection(myRand.Next(0, 4));
               //     SMonitor.Log("PSL - Warping NPC from occupied tile to Random Open Place - " + spouse.Name); // Problem: This is warping liuterally everyone, including the patio spouse
               // }
             
            }
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
                    new TemporaryAnimatedSprite("LooseSprites\\emojis", new Microsoft.Xna.Framework.Rectangle(0, 0, 9, 9), 2000f, 1, 0, new Vector2(npc.Tile.X, npc.Tile.Y) * 64f + new Vector2(16f, -64f), false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
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
            if (Game1.player == null || Game1.currentLocation == null)
                return;
            if (!Context.IsWorldReady)
                return;
            if (Game1.player.currentLocation == null || !Game1.displayFarmer || Game1.player.position == null)
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





/*

 if (Game1.timeOfDay == 600)
            {
                Check610 = false;
            }

                if (FinalStageCheck == false)
            {
                
                if (Game1.timeOfDay == 600)
                {
                    foreach (Farmer f in Game1.getAllFarmers())
                    {
                        if (f == null || !f.IsLocalPlayer)
                            continue;
                        // Reset the spouse's position at the start of the day

                        FarmHouse farmHouse = Utility.getHomeOfFarmer(f);

                        List<NPC> allSpouses = GetSpouses(f, false).Values.ToList();

                      

                        Dictionary<NPC, Point> spousePositionDict = new Dictionary<NPC, Point>();

                        foreach (NPC nPC in allSpouses)
                        {
                            spousePositionDict.Add(nPC, nPC.TilePoint);
                        }

                        foreach (KeyValuePair<NPC, Microsoft.Xna.Framework.Point> spousePos in spousePositionDict)
                        {
                            SMonitor.Log("PSL - Spouse Position Final Check: " + spousePos.Key.Name + " at " + spousePos.Value.ToString()+ " in " +spousePos.Key.currentLocation.ToString()+".");
                        }

                        foreach (NPC npc in allSpouses)
                        {
                            Point position = Microsoft.Xna.Framework.Point.Zero;

                            for (int i = 0; i < 100; i++)
                            {
                                position = farmHouse.getRandomOpenPointInHouse(myRand);
                                if (!IsTileOccupied(farmHouse, position, npc.Name))
                                    break;
                            }




                            if (spousePositionDict.ContainsKey(npc))
                            {
                                spousePositionDict.Remove(npc);
                            }

                            foreach (KeyValuePair<NPC, Microsoft.Xna.Framework.Point> spousePos in spousePositionDict)
                            {
                                if (spousePos.Value.Equals(npc.TilePoint))
                                {
                                    Game1.warpCharacter(npc, farmHouse, new Vector2(position.X, position.Y));
                                    SMonitor.Log("PSL - Final Stage Check" + spousePos.Key.Name+" "+ spousePos.Value.ToString()+"was overlapping with "+npc.Name+npc.Tile.ToString()+".");
                                }
                            }

                           // if(npc.TilePoint.X == 0 || npc.TilePoint.Y == 0)
                           // {
                           //     SMonitor.Log("PSL - Final Stage Check" +npc.Name+" "+ npc.TilePoint.ToString()+"was in the void...");
                           //     Game1.warpCharacter(npc, farmHouse, new Vector2(position.X, position.Y));
                               
                           // }


                            if (npc.currentLocation != farmHouse)
                            {
                                npc.HideShadow = false;
                                npc.isSleeping.Value = false;
                            }
                             if(npc.Position != GetSpouseBedPosition(farmHouse, npc.Name))
                            {
                                if(npc.isSleeping.Value == true)
                                {
                                    npc.isSleeping.Value = false;
                                    npc.Sprite.StopAnimation();
                                    npc.faceDirection(1);
                                    SMonitor.Log("PSL - Final Stage Check: " + npc.Name + " was sleeping, but is now awake.");
                                }

                            }


                        }
                    }
                    FinalStageCheck = true;
                }

            }

            if (Check610 == false)
            {

                if (Game1.timeOfDay == 610)
                {
                    foreach (Farmer f in Game1.getAllFarmers())
                    {
                        if (f == null || !f.IsLocalPlayer)
                            continue;
                        // Reset the spouse's position at the start of the day

                        FarmHouse farmHouse = Utility.getHomeOfFarmer(f);

                        List<NPC> allSpouses = GetSpouses(f, false).Values.ToList();
                      
                        Dictionary<NPC, Microsoft.Xna.Framework.Point> spousePositionDict = new Dictionary<NPC, Point>();

                       

foreach (KeyValuePair<NPC, Microsoft.Xna.Framework.Point> spousePos in spousePositionDict)
{
    SMonitor.Log("PSL - Spouse Position Check610: " + spousePos.Key.Name + " at " + spousePos.Value.ToString()+ " in " +spousePos.Key.currentLocation.ToString()+".");
}

foreach (NPC npc in allSpouses)
{

    SMonitor.Log("PSL - Final Stage Check610 " +npc.Name+"current locatiion is " + npc.TilePoint.ToString());

    if (npc.currentLocation != farmHouse)
    {
        npc.HideShadow = false;
        npc.isSleeping.Value = false;
    }
    if (npc.Position != GetSpouseBedPosition(farmHouse, npc.Name))
    {
        if (npc.isSleeping.Value == true)
        {
            npc.isSleeping.Value = false;
            npc.Sprite.StopAnimation();
            npc.faceDirection(1);
            SMonitor.Log("PSL - Final Stage Check610: " + npc.Name + " was sleeping, but is now awake.");
        }

    }

    if (npc.currentLocation is FarmHouse)
    {

        if (spousePositionDict.ContainsKey(npc))
        {
            spousePositionDict.Remove(npc);
        }
        Microsoft.Xna.Framework.Point posTest = new Point(0, 0);

        Point position = new Microsoft.Xna.Framework.Point(10, 7);

       


        for (int i = 0; i < 100; i++)
        {
            position = farmHouse.getRandomOpenPointInHouse(myRand);
            if (!IsTileOccupied(farmHouse, position, npc.Name))
                break;
        }

        foreach (KeyValuePair<NPC, Point> spousePos in spousePositionDict)
        {
            if (spousePos.Value.Equals(npc.TilePoint))
            {
                Game1.warpCharacter(npc, farmHouse, new Vector2(position.X, position.Y));
                SMonitor.Log("PSL - Final Stage Check610" + spousePos.Key.Name+" "+ spousePos.Value.ToString()+"was overlapping with "+npc.Name+npc.Tile.ToString()+".");
            }
        }

        if (npc.TilePoint.X == 0 || npc.TilePoint.Y == 0)
        {
            Game1.warpCharacter(npc, farmHouse, new Vector2(position.X, position.Y));
            SMonitor.Log("PSL - Final Stage Check610" +npc.Name+" "+ npc.TilePoint.ToString()+"was in the void...");
        }

    }


}
                    }
                    Check610 = true;
                }
            } 

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




*/