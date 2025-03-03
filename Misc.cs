using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Extensions;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml.Linq;

namespace PolyamorySweetLove
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry
    {
        private static Dictionary<string, int> topOfHeadOffsets = new Dictionary<string, int>();

        public static void ReloadSpouses(Farmer farmer)
        {
            currentSpouses[farmer.UniqueMultiplayerID] = new Dictionary<string, NPC>();
            currentUnofficialSpouses[farmer.UniqueMultiplayerID] = new Dictionary<string, NPC>();
            string ospouse = farmer.spouse;
            if (ospouse != null)
            {
                var npc = Game1.getCharacterFromName(ospouse);
                if (npc is not null)
                {
                    currentSpouses[farmer.UniqueMultiplayerID][ospouse] = npc;
                }
            }
            SMonitor.Log($"Checking for extra spouses in {farmer.friendshipData.Count()} friends");
            foreach (string friend in farmer.friendshipData.Keys)
            {
                if (farmer.friendshipData[friend].IsMarried() && friend != farmer.spouse)
                {
                    var npc = Game1.getCharacterFromName(friend, true);
                    if (npc != null)
                    {
                        currentSpouses[farmer.UniqueMultiplayerID][friend] = npc;
                        currentUnofficialSpouses[farmer.UniqueMultiplayerID][friend] = npc;
                    }
                }
            }
            if (farmer.spouse is null && currentSpouses[farmer.UniqueMultiplayerID].Any())
                farmer.spouse = currentSpouses[farmer.UniqueMultiplayerID].First().Key;
            SMonitor.Log($"reloaded {currentSpouses[farmer.UniqueMultiplayerID].Count} spouses for {farmer.Name} {farmer.UniqueMultiplayerID}");
        }
        public static Dictionary<string, NPC> GetSpouses(Farmer farmer, bool all)
        {
            if (!currentSpouses.ContainsKey(farmer.UniqueMultiplayerID) || ((currentSpouses[farmer.UniqueMultiplayerID].Count == 0 && farmer.spouse != null)))
            {
                ReloadSpouses(farmer);
            }
            if (farmer.spouse == null && currentSpouses[farmer.UniqueMultiplayerID].Count > 0)
            {
                farmer.spouse = currentSpouses[farmer.UniqueMultiplayerID].First().Key;
            }
            return all ? currentSpouses[farmer.UniqueMultiplayerID] : currentUnofficialSpouses[farmer.UniqueMultiplayerID];
        }

        internal static void ResetDivorces()
        {
            if (!Config.PreventHostileDivorces)
                return;
            List<string> friends = Game1.player.friendshipData.Keys.ToList();
            foreach (string f in friends)
            {
                if (Game1.player.friendshipData[f].Status == FriendshipStatus.Divorced)
                {
                    SMonitor.Log($"Wiping divorce for {f}");
                    if (Game1.player.friendshipData[f].Points < 8 * 250)
                        Game1.player.friendshipData[f].Status = FriendshipStatus.Friendly;
                    else
                        Game1.player.friendshipData[f].Status = FriendshipStatus.Dating;
                }
            }
        }

        public static string GetRandomSpouse(Farmer f)
        {
            var spouses = GetSpouses(f, true);
            if (spouses.Count == 0)
                return null;
            ShuffleDic(ref spouses);
            return spouses.Keys.ToArray()[0];
        }
        public static void PlaceSpousesInFarmhouse(FarmHouse farmHouse)
        {
            //string shakeTimer = Helper.Reflection.GetField<string>(__instance, "shakeTimer").GetValue();
            //farmHouse = Game1.RequireLocation<FarmHouse>(Game1.player.homeLocation.Value);
            //if (SortSpouseOrder == true)
            {
                //spousesortigncodehere



            }


            Point porchspot = farmHouse.getPorchStandingSpot();
            Point kitchenspot = farmHouse.getKitchenStandingSpot();
            Point bedspot = farmHouse.getBedSpot();
            


            Farmer farmer = farmHouse.owner;

            if (farmer == null)
                return;

            List<NPC> allSpouses = GetSpouses(farmer, true).Values.ToList();

            if (allSpouses.Count == 0)
            {
                SMonitor.Log("no spouses");
                return;
            }

            ShuffleList(ref allSpouses);

            List<string> bedSpouses = new List<string>();
            string bedSpouse = null;

            List<string> patioSpouses = new List<string>();
            string patioSpouse = null;

            List<string> porchSpouses = new List<string>();
            string porchSpouse = null;

            List<string> kitchenSpouses = new List<string>();
            string kitchenSpouse = null;

            foreach (NPC spouse in allSpouses)
            {
                if (spouse is null)
                    continue;
                SMonitor.Log("PSL - initiating spouse placement for" + spouse.Name);

                if (!farmHouse.Equals(spouse.currentLocation))
                {
                    Game1.warpCharacter(spouse, "FarmHouse", getRandomOpenPointInFarmHouse(myRand, 0, 60));
                    spouse.faceDirection(myRand.Next(0, 4));
                    SMonitor.Log("PSL - Warping NPC from where ever to Random Open Place for begin placement - " + spouse.Name);
                }

                else
                {
                    spouse.setTilePosition(farmHouse.getRandomOpenPointInHouse(myRand));
                    spouse.faceDirection(myRand.Next(0, 4));
                    SMonitor.Log("PSL - Setting NPC Random Open Place for begin placement - " + spouse.Name);

                }

            }

            PlaceSpousesInFarmHouseA(farmHouse);

                /*
                foreach (NPC spouse in allSpouses)
                {
                    if (spouse is null)
                        continue;

                    if (!farmHouse.Equals(spouse.currentLocation))
                    {
                        if (spouse.TilePoint == porchspot)
                        {
                            porchSpouses.Add(spouse.Name);
                            SMonitor.Log($"{spouse.Name} is on the porch ({spouse.currentLocation.Name})");
                        }
                        SMonitor.Log($"{spouse.Name} is not in farm house ({spouse.currentLocation.Name})");
                        continue;
                    }

                    if(spouse.TilePoint == kitchenspot)
                    {
                        kitchenSpouses.Add(spouse.Name);
                        SMonitor.Log($"{spouse.Name} is in the Kitchen ({spouse.currentLocation.Name})");
                    }

                    if(spouse.TilePoint == bedspot)
                    {
                        bedSpouses.Add(spouse.Name);
                        SMonitor.Log($"{spouse.Name} is in the Bed ({spouse.currentLocation.Name})");
                    }

                    int type = myRand.Next(0, 100);

                    SMonitor.Log($"spouse rand {type}, bed: {Config.PercentChanceForSpouseInBed} kitchen {Config.PercentChanceForSpouseInKitchen}");

                    if (type < Config.PercentChanceForSpouseInBed)
                    {
                        if (bedSpouses.Count < 1 && (Config.RoommateRomance || !farmer.friendshipData[spouse.Name].IsRoommate()) && HasSleepingAnimation(spouse.Name))
                        {
                            SMonitor.Log("made bed spouse: " + spouse.Name);
                            bedSpouses.Add(spouse.Name);
                        }

                    }
                    else if (type < Config.PercentChanceForSpouseInBed + Config.PercentChanceForSpouseInKitchen)
                    {
                        if (kitchenSpouse == null)
                        {
                            SMonitor.Log("made kitchen spouse: " + spouse.Name);
                            kitchenSpouse = spouse.Name;
                        }
                    }
                    else if (type < Config.PercentChanceForSpouseInBed + Config.PercentChanceForSpouseInKitchen + Config.PercentChanceForSpouseAtPatio && patioSpouses.Count < 1)
                    {
                        if (!Game1.isRaining && !Game1.IsWinter && !Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth).Equals("Sat") && !spouse.Name.Equals("Krobus") && spouse.Schedule == null && patioSpouses.Count <=1)
                        {
                            patioSpouses.Add(spouse.Name);
                            SMonitor.Log("made patio spouse: " + spouse.Name);
                            spouse.setUpForOutdoorPatioActivity();
                            SMonitor.Log($"{spouse.Name} at {spouse.currentLocation.Name} {spouse.TilePoint}");
                        }
                    }
                } */
                /*
                foreach (NPC spouse in allSpouses)
            {
                if (spouse is null)
                    continue;
                SMonitor.Log("placing " + spouse.Name);

                if (patioSpouses.Count > 1 && patioSpouses.Contains(spouse.Name))
                {
                    Game1.warpCharacter(spouse, "FarmHouse", getRandomOpenPointInFarmHouse(myRand, 0, 60));
                    spouse.setTilePosition(farmHouse.getRandomOpenPointInHouse(myRand));
                    spouse.faceDirection(myRand.Next(0, 4));
                    SMonitor.Log($"{spouse.Name} spouse random loc {spouse.TilePoint}");
                    spouse.setSpouseRoomMarriageDialogue();// I CAN SET THIS TO ANYTHING!!!        //setRandomAfternoonMarriageDialogue(Game1.timeOfDay, farmHouse, false);
                    patioSpouses.Remove(spouse.Name);
                }

                if (porchSpouses.Count > 1 && porchSpouses.Contains(spouse.Name))
                {
                    Game1.warpCharacter(spouse, "FarmHouse", getRandomOpenPointInFarmHouse(myRand, 0, 60));
                    spouse.setTilePosition(farmHouse.getRandomOpenPointInHouse(myRand));
                    spouse.faceDirection(myRand.Next(0, 4));
                    SMonitor.Log($"{spouse.Name} spouse random loc {spouse.TilePoint}");
                    spouse.setSpouseRoomMarriageDialogue();// I CAN SET THIS TO ANYTHING!!!        //setRandomAfternoonMarriageDialogue(Game1.timeOfDay, farmHouse, false);
                    porchSpouses.Remove(spouse.Name);
                }
                Point spouseRoomSpot = new Point(-1, -1);

                if (spouseRoomSpot.X < 0 && farmer.spouse == spouse.Name)
                {
                    spouseRoomSpot = farmHouse.GetSpouseRoomSpot();
                    SMonitor.Log($"Using default spouse spot {spouseRoomSpot}");
                }

                if (!farmHouse.Equals(spouse.currentLocation))
                {
                    SMonitor.Log($"{spouse.Name} is not in farm house ({spouse.currentLocation.Name})");
                    continue;
                }

                SMonitor.Log("in farm house");
                spouse.shouldPlaySpousePatioAnimation.Value = false;

                Vector2 bedPos = GetSpouseBedPosition(farmHouse, spouse.Name);
                if (bedSpouses.Count > 1 && bedSpouses.Contains(spouse.Name))
                {
                    Game1.warpCharacter(spouse, "FarmHouse", getRandomOpenPointInFarmHouse(myRand, 0, 60));
                    spouse.setTilePosition(farmHouse.getRandomOpenPointInHouse(myRand));
                    spouse.faceDirection(myRand.Next(0, 4));
                    SMonitor.Log($"{spouse.Name} spouse random location instead of piled in the bed {spouse.TilePoint}");
                    
                    spouse.checkForMarriageDialogue(600, farmHouse);// I CAN SET THIS TO ANYTHING!!! 
                    bedSpouses.Remove(spouse.Name);

                }
                if (bedSpouses.Count > 0 && bedSpouses.Contains(spouse.Name) && bedPos != Vector2.Zero)
                {
                    SMonitor.Log($"putting {spouse.Name} in bed");
                    spouse.position.Value = GetSpouseBedPosition(farmHouse, spouse.Name);
                }

                if (kitchenSpouses.Count > 1 && kitchenSpouses.Contains(spouse.Name))
                {
                    Game1.warpCharacter(spouse, "FarmHouse", getRandomOpenPointInFarmHouse(myRand, 0, 60));
                    spouse.setTilePosition(farmHouse.getRandomOpenPointInHouse(myRand));
                    spouse.faceDirection(myRand.Next(0, 4));
                    SMonitor.Log($"{spouse.Name} spouse random location instead of piled in the Kitchen!!! {spouse.TilePoint}");
                    spouse.checkForMarriageDialogue(600, farmHouse);// I CAN SET THIS TO ANYTHING!!! 
                    kitchenSpouses.Remove(spouse.Name);
                }

                else if (kitchenSpouse == spouse.Name && !IsTileOccupied(farmHouse, farmHouse.getKitchenStandingSpot(), spouse.Name))
                {
                    SMonitor.Log($"{spouse.Name} is in kitchen");

                    spouse.setTilePosition(farmHouse.getKitchenStandingSpot());
                    spouse.setRandomAfternoonMarriageDialogue(Game1.timeOfDay, farmHouse, false);
                }
                else if (spouseRoomSpot.X > -1 && !IsTileOccupied(farmHouse, spouseRoomSpot, spouse.Name))
                {
                    SMonitor.Log($"{spouse.Name} is in spouse room");
                    spouse.setTilePosition(spouseRoomSpot);
                    spouse.setSpouseRoomMarriageDialogue();
                }
                else
                {
                    spouse.setTilePosition(farmHouse.getRandomOpenPointInHouse(myRand));
                    spouse.faceDirection(myRand.Next(0, 4));
                    SMonitor.Log($"{spouse.Name} spouse random loc {spouse.TilePoint}");
                    spouse.setRandomAfternoonMarriageDialogue(Game1.timeOfDay, farmHouse, false);
                }
            }*/
        }

        private static bool IsTileOccupied(GameLocation location, Point tileLocation, string characterToIgnore)
        {
            Rectangle tileLocationRect = new Rectangle(tileLocation.X * 64 + 1, tileLocation.Y * 64 + 1, 62, 62);

            for (int i = 0; i < location.characters.Count; i++)
            {
                if (location.characters[i] != null && !location.characters[i].Name.Equals(characterToIgnore) && location.characters[i].GetBoundingBox().Intersects(tileLocationRect))
                {
                    SMonitor.Log($"Tile {tileLocation} is occupied by {location.characters[i].Name}");

                    return true;
                }
            }
            return false;
        }

        public static Point GetSpouseBedEndPoint(FarmHouse fh, string name)
        {
            var bedSpouses = GetBedSpouses(fh);

            Point bedStart = fh.GetSpouseBed().GetBedSpot();
            int bedWidth = GetBedWidth();
            bool up = fh.upgradeLevel > 1;

            int x = (int)(bedSpouses.IndexOf(name) / (float)(bedSpouses.Count) * (bedWidth - 1));
            if (x < 0)
                return Point.Zero;
            return new Point(bedStart.X + x, bedStart.Y);
        }
        public static Vector2 GetSpouseBedPosition(FarmHouse fh, string name)
        {
            var allBedmates = GetBedSpouses(fh);

            Point bedStart = GetBedStart(fh);
            int x = 64 + (int)((allBedmates.IndexOf(name) + 1) / (float)(allBedmates.Count + 1) * (GetBedWidth() - 1) * 64);
            return new Vector2(bedStart.X * 64 + x, bedStart.Y * 64 + bedSleepOffset - (GetTopOfHeadSleepOffset(name) * 4));
        }

        public static Point GetBedStart(FarmHouse fh)
        {
            if (fh?.GetSpouseBed()?.GetBedSpot() == null)
                return Point.Zero;
            return new Point(fh.GetSpouseBed().GetBedSpot().X - 1, fh.GetSpouseBed().GetBedSpot().Y - 1);
        }

        public static bool IsInBed(FarmHouse fh, Rectangle box)
        {
            int bedWidth = GetBedWidth();
            Point bedStart = GetBedStart(fh);
            Rectangle bed = new Rectangle(bedStart.X * 64, bedStart.Y * 64, bedWidth * 64, 3 * 64);

            if (box.Intersects(bed))
            {
                return true;
            }
            return false;
        }
        public static int GetBedWidth()
        {
            if (polyamorySweetBedAPI != null)
            {
                return polyamorySweetBedAPI.GetBedWidth();
            }
            else
            {
                return 3;
            }
        }
        public static List<string> GetBedSpouses(FarmHouse fh)
        {
            if (Config.RoommateRomance)
                return GetSpouses(fh.owner, true).Keys.ToList();

            return GetSpouses(fh.owner, true).Keys.ToList().FindAll(s => !fh.owner.friendshipData[s].RoommateMarriage);
        }

        public static List<string> ReorderSpousesForSleeping(List<string> sleepSpouses)
        {
            List<string> configSpouses = Config.SpouseSleepOrder.Split(',').Where(s => s.Length > 0).ToList();
            List<string> spouses = new List<string>();
            foreach (string s in configSpouses)
            {
                if (sleepSpouses.Contains(s))
                    spouses.Add(s);
            }

            foreach (string s in sleepSpouses)
            {
                if (!spouses.Contains(s))
                {
                    spouses.Add(s);
                    configSpouses.Add(s);
                }
            }
            string configString = string.Join(",", configSpouses);
            if (configString != Config.SpouseSleepOrder)
            {
                Config.SpouseSleepOrder = configString;
                SHelper.WriteConfig(Config);
            }

            return spouses;
        }


        public static int GetTopOfHeadSleepOffset(string name)
        {
            var nomena = Game1.getCharacterFromName(name);

            if (nomena.Name != nomena.getTextureName())
            {
                name = nomena.getTextureName();
            }





            if (topOfHeadOffsets.ContainsKey(name))
            {
                return topOfHeadOffsets[name];
            }
            //SMonitor.Log($"dont yet have offset for {name}");
            int top = 0;

            if (name == "Krobus")
                return 8;




            Texture2D tex = Game1.content.Load<Texture2D>($"Characters\\{name}");


            int sleepidx;
            string sleepAnim = SleepAnimation(name);
            if (sleepAnim == null || !int.TryParse(sleepAnim.Split('/')[0], out sleepidx))
                sleepidx = 8;

            if ((sleepidx * 16) / 64 * 32 >= tex.Height)
            {
                sleepidx = 8;
            }


            Color[] colors = new Color[tex.Width * tex.Height];
            tex.GetData(colors);

            //SMonitor.Log($"sleep index for {name} {sleepidx}");

            int startx = (sleepidx * 16) % 64;
            int starty = (sleepidx * 16) / 64 * 32;

            //SMonitor.Log($"start {startx},{starty}");

            for (int i = 0; i < 16 * 32; i++)
            {
                int idx = startx + (i % 16) + (starty + i / 16) * 64;
                if (idx >= colors.Length)
                {
                    SMonitor.Log($"Sleep pos couldn't get pixel at {startx + i % 16},{starty + i / 16} ");
                    break;
                }
                Color c = colors[idx];
                if (c != Color.Transparent)
                {
                    top = i / 16;
                    break;
                }
            }
            topOfHeadOffsets.Add(name, top);
            return top;
        }


        public static bool HasSleepingAnimation(string name)
        {
            var nomena = Game1.getCharacterFromName(name);
            if (nomena.Name != nomena.getTextureName())
            {
                name = nomena.getTextureName();
            }

            string sleepAnim = SleepAnimation(name);
            if (sleepAnim == null || !sleepAnim.Contains("/"))
                return false;

            if (!int.TryParse(sleepAnim.Split('/')[0], out int sleepidx))
                return false;

            //if (!NPC.Name.Equals(__instance.getTextureName()))
            // {

            //   name = __instance.getTextureName();

            // }

            //Texture2D tex = SHelper.GameContent.Load<Texture2D>($"Characters/{name}");

            //if (tex == null)
            {
                try
                {
                    Texture2D tex = SHelper.GameContent.Load<Texture2D>($"Characters/{name}");
                    if (sleepidx / 4 * 32 >= tex.Height)
                    {
                        return false;
                    }
                }

                catch
                {
                    //NPC fubar = new NPC();
                    //var nombre = fubar.getTextureName();

                    //Texture2D tex = SHelper.GameContent.Load<Texture2D>($"Characters/{nombre}");
                    //if (sleepidx / 4 * 32 >= tex.Height)
                    //{
                    //    return false;
                    //}

                    SMonitor.Log("Unable to get texture for NPC. Probably the names are wrong somewhere in the NPC source.");
                }
            }
            //SMonitor.Log($"tex height for {name}: {tex.Height}");
            return true;
        }

        private static string SleepAnimation(string name)
        {
            string anim = null;
            if (Game1.content.Load<Dictionary<string, string>>("Data\\animationDescriptions").ContainsKey(name.ToLower() + "_sleep"))
            {
                anim = Game1.content.Load<Dictionary<string, string>>("Data\\animationDescriptions")[name.ToLower() + "_sleep"];
            }
            else if (Game1.content.Load<Dictionary<string, string>>("Data\\animationDescriptions").ContainsKey(name + "_Sleep"))
            {
                anim = Game1.content.Load<Dictionary<string, string>>("Data\\animationDescriptions")[name + "_Sleep"];
            }
            return anim;
        }


        internal static void NPCDoAnimation(NPC npc, string npcAnimation)
        {
            Dictionary<string, string> animationDescriptions = SHelper.GameContent.Load<Dictionary<string, string>>("Data\\animationDescriptions");
            if (!animationDescriptions.ContainsKey(npcAnimation))
                return;

            string[] rawData = animationDescriptions[npcAnimation].Split('/');
            var animFrames = Utility.parseStringToIntArray(rawData[1], ' ');

            List<FarmerSprite.AnimationFrame> anim = new List<FarmerSprite.AnimationFrame>();
            for (int i = 0; i < animFrames.Length; i++)
            {
                anim.Add(new FarmerSprite.AnimationFrame(animFrames[i], 100, 0, false, false, null, false, 0));
            }
            SMonitor.Log($"playing animation {npcAnimation} for {npc.Name}");
            npc.Sprite.setCurrentAnimation(anim);
        }

        public static void ResetSpouses(Farmer f, bool force = false)
        {
            if (force)
            {
                currentSpouses.Remove(f.UniqueMultiplayerID);
                currentUnofficialSpouses.Remove(f.UniqueMultiplayerID);
            }
            Dictionary<string, NPC> spouses = GetSpouses(f, true);
            if (f.spouse == null)
            {
                if (spouses.Count > 0)
                {
                    SMonitor.Log("No official spouse, setting official spouse to: " + spouses.First().Key);
                    f.spouse = spouses.First().Key;
                }
            }

            foreach (string name in f.friendshipData.Keys)
            {
                
                /*if (f.friendshipData[name].IsEngaged())
                {
                    SMonitor.Log($"{f.Name} is engaged to: {name} {f.friendshipData[name].CountdownToWedding} days until wedding");
                    if (f.friendshipData[name].WeddingDate.TotalDays < new WorldDate(Game1.Date).TotalDays)
                    {
                        SMonitor.Log("invalid engagement: " + name);
                        f.friendshipData[name].WeddingDate.TotalDays = new WorldDate(Game1.Date).TotalDays + 1;
                    }
                   // if (f.spouse != name)
                   // {
                     //   SMonitor.Log("setting spouse to engagee: " + name); Angel of the Morning - post-proposal bug
                      //  f.spouse = name;
                   // }
                }*/
                if (f.friendshipData[name].IsMarried() && f.spouse != name)
                {
                    //SMonitor.Log($"{f.Name} is married to: {name}");
                    if (f.spouse != null && f.friendshipData[f.spouse] != null && !f.friendshipData[f.spouse].IsMarried() && !f.friendshipData[f.spouse].IsEngaged())
                    {
                        SMonitor.Log("invalid ospouse, setting ospouse to " + name);
                        f.spouse = name;
                    }
                    if (f.spouse == null)
                    {
                        SMonitor.Log("null ospouse, setting ospouse to " + name);
                        f.spouse = name;
                    }
                }
            }
            ReloadSpouses(f);
        }
        public static void SetAllNPCsDatable()
        {
            if (!Config.RomanceAllVillagers)
                return;

            List<string> mojovision = new List<string>();

            mojovision.Add("Mateo");

            mojovision.Add("Hector");

            mojovision.Add("Cirrus");

            mojovision.Add("Dandelion");

            mojovision.Add("Roslin");

            mojovision.Add("Solomon");

            mojovision.Add("Stiles");


           



            Farmer f = Game1.player;
            if (f == null)
            {
                return;
            }
            foreach (string friend in f.friendshipData.Keys)
            {
                NPC npc = Game1.getCharacterFromName(friend);


                if (!mojovision.Contains(friend))





                    if (npc != null && !npc.datable.Value && npc is NPC && !(npc is Child) && (npc.Age == 0 || npc.Age == 1) && !mojovision.Contains(friend))
                {
                    SMonitor.Log($"Making {npc.Name} datable.");
                    npc.datable.Value = true;
                }
            }
        }


        public static void ShuffleList<T>(ref List<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = myRand.Next(n + 1);
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
        public static void ShuffleDic<T1, T2>(ref Dictionary<T1, T2> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = myRand.Next(n + 1);
                var value = list[list.Keys.ToArray()[k]];
                list[list.Keys.ToArray()[k]] = list[list.Keys.ToArray()[n]];
                list[list.Keys.ToArray()[n]] = value;
            }
        }

        public static List<string> ReorderSpouses(List<string> sleepSpouses)
        {
            List<string> configSpouses = Config.SpouseOrder.Split(',').Where(s => s.Length > 0).ToList();
            List<string> spouses = new List<string>();
            foreach (string s in configSpouses)
            {
                if (sleepSpouses.Contains(s))
                    spouses.Add(s);
            }

            foreach (string s in sleepSpouses)
            {
                if (!spouses.Contains(s))
                {
                    spouses.Add(s);
                    configSpouses.Add(s);
                }
            }
            string configString = string.Join(",", configSpouses);
            if (configString != Config.SpouseOrder)
            {
                Config.SpouseOrder = configString;
                SHelper.WriteConfig(Config);
            }

            return spouses;
        }






        public static void PlaceSpousesInFarmHouseA(FarmHouse farmHouse)
        {
            NPC kitchenspouse = new NPC();
            NPC patioSpouse = new NPC();

            Farmer farmer = farmHouse.owner;

            if (farmer == null)
                return;

            List<NPC> allSpouses = GetSpouses(farmer, true).Values.ToList();

            if (allSpouses.Count == 0)
            {
                SMonitor.Log("no spouses");
                return;
            }

            foreach (NPC spouse in allSpouses)
            {
                if (spouse is null)
                    continue;


                Random r = Utility.CreateDaySaveRandom(Game1.player.UniqueMultiplayerID);
                int heartsWithSpouse = farmer.getFriendshipHeartLevelForNPC(spouse.Name);


                // For cannot find spouse
                //
                if (Game1.IsMasterGame && (spouse.currentLocation == null || !spouse.currentLocation.Equals(farmHouse)))
                {
                    Game1.warpCharacter(spouse, farmer.homeLocation.Value, farmHouse.getSpouseBedSpot(spouse.Name));
                    SMonitor.Log("PSL - Warping "+spouse.Name+"to farmhouse because NPC was someother strange place.");
                    spouse.modData["SpouseNeedsPlaced"] = "";
                }

                //GenericDialogue
                if (Game1.isRaining)
                {
                    spouse.marriageDefaultDialogue.Value = new MarriageDialogueReference("MarriageDialogue", "Rainy_Day_" + r.Next(5), false);
                }
                else
                {
                    spouse.marriageDefaultDialogue.Value = new MarriageDialogueReference("MarriageDialogue", "Indoor_Day_" + r.Next(5), false);
                }

                //Birth-Related Dialogue
                //
                spouse.currentMarriageDialogue.Add(new MarriageDialogueReference(spouse.marriageDefaultDialogue.Value.DialogueFile, spouse.marriageDefaultDialogue.Value.DialogueKey, spouse.marriageDefaultDialogue.Value.IsGendered, spouse.marriageDefaultDialogue.Value.Substitutions));
                if (farmer.GetSpouseFriendship().DaysUntilBirthing == 0)
                {
                    //spouse.setTilePosition(farmHouse.getKitchenStandingSpot());
                    spouse.setTilePosition(farmHouse.getRandomOpenPointInHouse(r));
                    spouse.currentMarriageDialogue.Clear();
                    spouse.addMarriageDialogue("Strings\\StringsFromCSFiles", "PolyamorySweet.GaveBirthToday", true);

                    return;
                }
                if (spouse.daysAfterLastBirth >= 0)
                {
                    spouse.daysAfterLastBirth--;
                    switch (spouse.getSpouse().getChildrenCount())
                    {
                        case 1:
                            //spouse.setTilePosition(farmHouse.getKitchenStandingSpot());
                            spouse.setTilePosition(farmHouse.getRandomOpenPointInHouse(r));
                            if (!spouse.spouseObstacleCheck(new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4406", false), farmHouse))
                            {
                                spouse.currentMarriageDialogue.Clear();
                                spouse.addMarriageDialogue("MarriageDialogue", "OneKid_" + r.Next(4), false);
                            }
                            return;
                        case 2:
                            //spouse.setTilePosition(farmHouse.getKitchenStandingSpot());
                            spouse.setTilePosition(farmHouse.getRandomOpenPointInHouse(r));
                            if (!spouse.spouseObstacleCheck(new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4406", false), farmHouse))
                            {
                                spouse.currentMarriageDialogue.Clear();
                                spouse.addMarriageDialogue("MarriageDialogue", "TwoKids_" + r.Next(4), false);
                            }
                            return;
                    }
                }

                //Various Dialogues
                //
                spouse.setTilePosition(farmHouse.getRandomOpenPointInHouse(r));  //spouse.setTilePosition(farmHouse.getKitchenStandingSpot());
                if (!spouse.sleptInBed.Value)
                {
                    spouse.currentMarriageDialogue.Clear();
                    spouse.addMarriageDialogue("MarriageDialogue", "NoBed_" + r.Next(4), false);
                    return;
                }
                if (spouse.tryToGetMarriageSpecificDialogue(Game1.currentSeason + "_" + Game1.dayOfMonth) != null)
                {
                    if (spouse != null)
                    {
                        spouse.currentMarriageDialogue.Clear();
                        spouse.addMarriageDialogue("MarriageDialogue", Game1.currentSeason + "_" + Game1.dayOfMonth, false);
                    }
                    return;
                }
                if (spouse.Schedule != null)
                {
                    if (spouse.ScheduleKey == "marriage_" + Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth))
                    {
                        spouse.currentMarriageDialogue.Clear();
                        spouse.addMarriageDialogue("MarriageDialogue", "funLeave_" + spouse.Name, false);
                    }
                    else if (spouse.ScheduleKey == "marriageJob")
                    {
                        spouse.currentMarriageDialogue.Clear();
                        spouse.addMarriageDialogue("MarriageDialogue", "jobLeave_" + spouse.Name, false);
                    }
                    return;
                }

                // PATIO PLACEMENT
                //

                if (ModEntry.PatioPlacement == false)
                {
                    if (!Game1.isRaining && !Game1.IsWinter && Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth).Equals("Sat") && farmer == Game1.MasterPlayer && !spouse.Name.Equals("Krobus"))
                    {
                        spouse.setUpForOutdoorPatioActivity();
                        ModEntry.PatioPlacement = true;
                        return;
                    }
                }


                //Rage Dialogue Below!!!
                //
                int minHeartLevelForNegativeDialogue = 12;
                if (Game1.Date.TotalDays - farmer.GetSpouseFriendship().LastGiftDate?.TotalDays <= 1)
                {
                    minHeartLevelForNegativeDialogue--;
                }
                if (farmer.GetDaysMarried() > 7 && r.NextDouble() < (double)(1f - (float)Math.Max(1, heartsWithSpouse) / (float)minHeartLevelForNegativeDialogue))
                {
                    Furniture f = farmHouse.getRandomFurniture(r);
                    if (f != null && f.isGroundFurniture() && f.furniture_type.Value != 15 && f.furniture_type.Value != 12)
                    {
                        Point p = new Point((int)f.tileLocation.X - 1, (int)f.tileLocation.Y);
                        if (farmHouse.CanItemBePlacedHere(new Vector2(p.X, p.Y)))
                        {
                            spouse.setTilePosition(p);
                            spouse.faceDirection(1);
                            switch (r.Next(10))
                            {
                                case 0:
                                    spouse.currentMarriageDialogue.Clear();
                                    spouse.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4420", false);
                                    break;
                                case 1:
                                    spouse.currentMarriageDialogue.Clear();
                                    spouse.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4421", false);
                                    break;
                                case 2:
                                    spouse.currentMarriageDialogue.Clear();
                                    spouse.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4422", true);
                                    break;
                                case 3:
                                    spouse.currentMarriageDialogue.Clear();
                                    spouse.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4423", false);
                                    break;
                                case 4:
                                    spouse.currentMarriageDialogue.Clear();
                                    spouse.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4424", false);
                                    break;
                                case 5:
                                    spouse.currentMarriageDialogue.Clear();
                                    spouse.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4425", false);
                                    break;
                                case 6:
                                    spouse.currentMarriageDialogue.Clear();
                                    spouse.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4426", false);
                                    break;
                                case 7:
                                    {
                                        spouse.currentMarriageDialogue.Clear();
                                        spouse.addMarriageDialogue("Strings\\StringsFromCSFiles", r.Choose("NPC.cs.4427", "NPC.cs.4429", "NPC.cs.4431"), false);
                                    }
                                    break;
                                case 8:
                                    spouse.currentMarriageDialogue.Clear();
                                    spouse.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4432", false);
                                    break;
                                case 9:
                                    spouse.currentMarriageDialogue.Clear();
                                    spouse.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4433", false);
                                    break;
                            }
                            return;
                        }
                    }
                    spouse.spouseObstacleCheck(new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4406", false), farmHouse, force: true);
                    return;
                }
                Friendship friendship = farmer.GetSpouseFriendship();
                if (friendship.DaysUntilBirthing != -1 && friendship.DaysUntilBirthing <= 7 && r.NextBool())
                {
                    if (!Config.GayPregnancies)
                    {
                        if(spouse.isAdoptionSpouse())
                    {
                            spouse.setTilePosition(farmHouse.getKitchenStandingSpot());
                            if (!spouse.spouseObstacleCheck(new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4439", false), farmHouse))
                            {
                                if (r.NextBool())
                                {
                                    spouse.currentMarriageDialogue.Clear();
                                }
                                if (r.NextBool())
                                {
                                    spouse.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4440", false, Game1.player.displayName);
                                }
                                else
                                {
                                    spouse.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4441", false, "%endearment");
                                }
                            }
                            return;
                        }
                    }
                    if (spouse.Gender == Gender.Female)
                    {
                        if (!Config.ImpregnatingFemmeNPC)
                        {
                            spouse.setTilePosition(farmHouse.getKitchenStandingSpot());
                            if (!spouse.spouseObstacleCheck(r.NextBool() ? new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4442", false) : new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4443", false), farmHouse))
                            {
                                if (r.NextBool())
                                {
                                    spouse.currentMarriageDialogue.Clear();
                                }
                                spouse.currentMarriageDialogue.Add(r.NextBool() ? new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4444", false, Game1.player.displayName) : new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4445", false, "%endearment"));
                            }
                        }
                        else
                        {
                            spouse.setTilePosition(farmHouse.getKitchenStandingSpot());
                            if (!spouse.spouseObstacleCheck(new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4446", true), farmHouse))
                            {
                                if (r.NextBool())
                                {
                                    spouse.currentMarriageDialogue.Clear();
                                }
                                spouse.currentMarriageDialogue.Add(r.NextBool() ? new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4447", true, Game1.player.displayName) : new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4448", false, "%endearment"));
                            }
                        }
                        return;
                    }
                    spouse.setTilePosition(farmHouse.getKitchenStandingSpot());
                    if (!spouse.spouseObstacleCheck(new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4446", true), farmHouse))
                    {
                        if (r.NextBool())
                        {
                            spouse.currentMarriageDialogue.Clear();
                        }
                        spouse.currentMarriageDialogue.Add(r.NextBool() ? new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4447", true, Game1.player.displayName) : new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4448", false, "%endearment"));
                    }
                    return;
                }
                if (r.NextDouble() < 0.07)
                {
                    switch (Game1.player.getChildrenCount())
                    {
                        case 1:
                            spouse.setTilePosition(farmHouse.getKitchenStandingSpot());
                            if (!spouse.spouseObstacleCheck(new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4449", true), farmHouse))
                            {
                                spouse.currentMarriageDialogue.Clear();
                                spouse.addMarriageDialogue("MarriageDialogue", "OneKid_" + r.Next(4), false);
                            }
                            return;
                        case 2:
                            spouse.setTilePosition(farmHouse.getKitchenStandingSpot());
                            if (!spouse.spouseObstacleCheck(new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4452", true), farmHouse))
                            {
                                spouse.currentMarriageDialogue.Clear();
                                spouse.addMarriageDialogue("MarriageDialogue", "TwoKids_" + r.Next(4), false);
                            }
                            return;
                    }
                }
                Farm farm = Game1.getFarm();
                if (spouse.currentMarriageDialogue.Count > 0 && spouse.currentMarriageDialogue[0].IsItemGrabDialogue(spouse))
                {
                    //spouse.setTilePosition(farmHouse.getKitchenStandingSpot());
                    //spouse.spouseObstacleCheck(new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4455", true), farmHouse);
                    spouse.setTilePosition(farmHouse.getRandomOpenPointInHouse(myRand));
                    spouse.faceDirection(myRand.Next(0, 4));
                    spouse.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4455", true);

                }

                if(ModEntry.PorchPlacement==false)
                {
                    if (!Game1.isRaining && r.NextDouble() < 0.4 && !NPC.checkTileOccupancyForSpouse(farm, Utility.PointToVector2(farmHouse.getPorchStandingSpot())) && !spouse.Name.Equals("Krobus"))
                    {
                        bool filledBowl = false;
                        if (!NPC.hasSomeoneFedThePet)
                        {
                            foreach (Building building in farm.buildings)
                            {
                                if (building is PetBowl bowl && !bowl.watered.Value)
                                {
                                    filledBowl = true;
                                    bowl.watered.Value = true;
                                    NPC.hasSomeoneFedThePet = true;
                                }
                            }
                        }
                        if (r.NextDouble() < 0.8 && Game1.season != Season.Winter && !NPC.hasSomeoneWateredCrops) //was 0.6
                        {
                            Vector2 origin = Vector2.Zero;
                            int tries = 0;
                            bool foundWatered = false;
                            for (; tries < Math.Min(50, farm.terrainFeatures.Length); tries++)
                            {
                                if (!origin.Equals(Vector2.Zero))
                                {
                                    break;
                                }
                                if (Utility.TryGetRandom(farm.terrainFeatures, out var tile, out var feature) && feature is HoeDirt dirt && dirt.needsWatering())
                                {
                                    if (!dirt.isWatered())
                                    {
                                        origin = tile;
                                    }
                                    else
                                    {
                                        foundWatered = true;
                                    }
                                }
                            }
                            if (!origin.Equals(Vector2.Zero))
                            {
                                foreach (Vector2 currentPosition in new Microsoft.Xna.Framework.Rectangle((int)origin.X - 30, (int)origin.Y - 30, 60, 60).GetVectors())
                                {
                                    if (farm.isTileOnMap(currentPosition) && farm.terrainFeatures.TryGetValue(currentPosition, out var terrainFeature) && terrainFeature is HoeDirt dirt && Game1.IsMasterGame && dirt.needsWatering())
                                    {
                                        dirt.state.Value = 1;
                                    }
                                }
                                spouse.faceDirection(2);
                                spouse.currentMarriageDialogue.Clear();
                                spouse.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4462", true);
                                if (filledBowl)
                                {
                                    if (Utility.getAllPets().Count > 1 && Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.en)
                                    {
                                        spouse.addMarriageDialogue("Strings\\StringsFromCSFiles", "MultiplePetBowls_watered", false, Game1.player.getPetDisplayName());
                                    }
                                    else
                                    {
                                        spouse.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4463", false, Game1.player.getPetDisplayName());
                                    }
                                }
                                spouse.addMarriageDialogue("MarriageDialogue", "Outdoor_" + r.Next(5), false);
                                NPC.hasSomeoneWateredCrops = true;
                            }
                            else
                            {
                                spouse.faceDirection(2);
                                if (foundWatered)
                                {
                                    spouse.currentMarriageDialogue.Clear();
                                    if (Game1.gameMode == 6)
                                    {
                                        if (r.NextBool())
                                        {
                                            spouse.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4465", false, "%endearment");
                                        }
                                        else
                                        {
                                            spouse.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4466", false, "%endearment");
                                            spouse.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4462", true);
                                            if (filledBowl)
                                            {
                                                if (Utility.getAllPets().Count > 1 && Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.en)
                                                {
                                                    spouse.addMarriageDialogue("Strings\\StringsFromCSFiles", "MultiplePetBowls_watered", false, Game1.player.getPetDisplayName());
                                                }
                                                else
                                                {
                                                    spouse.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4463", false, Game1.player.getPetDisplayName());
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        spouse.currentMarriageDialogue.Clear();
                                        spouse.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4470", true);
                                    }
                                }
                                else
                                {
                                    spouse.currentMarriageDialogue.Clear();
                                    spouse.addMarriageDialogue("MarriageDialogue", "Outdoor_" + r.Next(5), false);
                                }
                            }
                        }
                        else if (r.NextDouble() < 0.6 && !NPC.hasSomeoneFedTheAnimals)
                        {
                            bool fedAnything = false;
                            foreach (Building b in farm.buildings)
                            {
                                if (b.GetIndoors() is AnimalHouse animalHouse && b.daysOfConstructionLeft.Value <= 0 && Game1.IsMasterGame)
                                {
                                    animalHouse.feedAllAnimals();
                                    fedAnything = true;
                                }
                            }
                            spouse.faceDirection(2);
                            if (fedAnything)
                            {
                                NPC.hasSomeoneFedTheAnimals = true;
                                spouse.currentMarriageDialogue.Clear();
                                spouse.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4474", true);
                                if (filledBowl)
                                {
                                    if (Utility.getAllPets().Count > 1 && Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.en)
                                    {
                                        spouse.addMarriageDialogue("Strings\\StringsFromCSFiles", "MultiplePetBowls_watered", false, Game1.player.getPetDisplayName());
                                    }
                                    else
                                    {
                                        spouse.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4463", false, Game1.player.getPetDisplayName());
                                    }
                                }
                                spouse.addMarriageDialogue("MarriageDialogue", "Outdoor_" + r.Next(5), false);
                            }
                            else
                            {
                                spouse.currentMarriageDialogue.Clear();
                                spouse.addMarriageDialogue("MarriageDialogue", "Outdoor_" + r.Next(5), false);
                            }
                            if (Game1.IsMasterGame)
                            {
                                foreach (Building building2 in farm.buildings)
                                {
                                    if (building2 is PetBowl bowl && !bowl.watered.Value)
                                    {
                                        filledBowl = true;
                                        bowl.watered.Value = true;
                                        NPC.hasSomeoneFedThePet = true;
                                    }
                                }
                            }
                        }
                        else if (!NPC.hasSomeoneRepairedTheFences)
                        {
                            int tries = 0;
                            spouse.faceDirection(2);
                            Vector2 origin = Vector2.Zero;
                            for (; tries < Math.Min(50, farm.objects.Length); tries++)
                            {
                                if (!origin.Equals(Vector2.Zero))
                                {
                                    break;
                                }
                                if (Utility.TryGetRandom(farm.objects, out var tile, out var obj) && obj is Fence)
                                {
                                    origin = tile;
                                }
                            }
                            if (!origin.Equals(Vector2.Zero))
                            {
                                foreach (Vector2 currentPosition in new Microsoft.Xna.Framework.Rectangle((int)origin.X - 10, (int)origin.Y - 10, 20, 20).GetVectors())
                                {
                                    if (farm.isTileOnMap(currentPosition) && farm.objects.TryGetValue(currentPosition, out var obj) && obj is Fence fence && Game1.IsMasterGame)
                                    {
                                        fence.repair();
                                    }
                                }
                                spouse.currentMarriageDialogue.Clear();
                                spouse.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4481", true);
                                if (filledBowl)
                                {
                                    if (Utility.getAllPets().Count > 1 && Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.en)
                                    {
                                        spouse.addMarriageDialogue("Strings\\StringsFromCSFiles", "MultiplePetBowls_watered", false, Game1.player.getPetDisplayName());
                                    }
                                    else
                                    {
                                        spouse.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4463", false, Game1.player.getPetDisplayName());
                                    }
                                }
                                spouse.addMarriageDialogue("MarriageDialogue", "Outdoor_" + r.Next(5), false);
                                NPC.hasSomeoneRepairedTheFences = true;
                            }
                            else
                            {
                                spouse.currentMarriageDialogue.Clear();
                                spouse.addMarriageDialogue("MarriageDialogue", "Outdoor_" + r.Next(5), false);
                            }
                        }
                        Game1.warpCharacter(spouse, "Farm", farmHouse.getPorchStandingSpot());
                        spouse.popOffAnyNonEssentialItems();
                        spouse.faceDirection(2);
                    }

                    ModEntry.PorchPlacement = true;
                }


                else if (spouse.Name.Equals("Krobus") && Game1.isRaining && r.NextDouble() < 0.4 && !NPC.checkTileOccupancyForSpouse(farm, Utility.PointToVector2(farmHouse.getPorchStandingSpot())))
                {
                    spouse.addMarriageDialogue("MarriageDialogue", "Outdoor_" + r.Next(5), false);
                    Game1.warpCharacter(spouse, "Farm", farmHouse.getPorchStandingSpot());
                    spouse.popOffAnyNonEssentialItems();
                    spouse.faceDirection(2);
                }
                else if (farmer.GetDaysMarried() >= 1 && r.NextDouble() < 0.045)
                {
                    if (r.NextDouble() < 0.75)
                    {
                        Point spot = farmHouse.getRandomOpenPointInHouse(r, 1);
                        Furniture new_furniture;
                        try
                        {
                            new_furniture = ItemRegistry.Create<Furniture>(Utility.getRandomSingleTileFurniture(r)).SetPlacement(spot);
                        }
                        catch
                        {
                            new_furniture = null;
                        }
                        if (new_furniture != null && spot.X > 0 && farmHouse.CanItemBePlacedHere(new Vector2(spot.X - 1, spot.Y)))
                        {
                            farmHouse.furniture.Add(new_furniture);
                            spouse.setTilePosition(spot.X - 1, spot.Y);
                            spouse.faceDirection(1);
                            spouse.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4486", false, "%endearmentlower");
                            if (Game1.random.NextBool())
                            {
                                spouse.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4488", true);
                            }
                            else
                            {
                                spouse.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4489", false);
                            }
                        }
                        else
                        {
                            spouse.setTilePosition(farmHouse.getKitchenStandingSpot());
                            spouse.spouseObstacleCheck(new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4490", false), farmHouse);
                        }
                        return;
                    }
                    Point p = farmHouse.getRandomOpenPointInHouse(r);
                    if (p.X <= 0)
                    {
                        return;
                    }
                    spouse.setTilePosition(p.X, p.Y);
                    spouse.faceDirection(0);
                    if (r.NextBool())
                    {
                        string wall = farmHouse.GetWallpaperID(p.X, p.Y);
                        if (wall != null)
                        {
                            string wallpaperId = r.ChooseFrom(spouse.GetData()?.SpouseWallpapers) ?? r.Next(112).ToString();
                            farmHouse.SetWallpaper(wallpaperId, wall);
                            spouse.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4496", false);
                        }
                    }
                    else
                    {
                        string floor = farmHouse.getFloorRoomIdAt(p);
                        if (floor != null)
                        {
                            string floorId = r.ChooseFrom(spouse.GetData()?.SpouseFloors) ?? r.Next(40).ToString();
                            farmHouse.SetFloor(floorId, floor);
                            spouse.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4497", false);
                        }
                    }
                }
                else if (Game1.isRaining && r.NextDouble() < 0.08 && heartsWithSpouse < 11 && farmer.GetDaysMarried() > 7 && spouse.Name != "Krobus")
                {
                    foreach (Furniture f in farmHouse.furniture)
                    {
                        if (f.furniture_type.Value == 13 && farmHouse.CanItemBePlacedHere(new Vector2((int)f.tileLocation.X, (int)f.tileLocation.Y + 1)))
                        {
                            spouse.setTilePosition((int)f.tileLocation.X, (int)f.tileLocation.Y + 1);
                            spouse.faceDirection(0);
                            spouse.currentMarriageDialogue.Clear();
                            spouse.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4498", true); //"*sigh*... sometimes I miss my old life.$s",
                            return;
                        }
                    }
                    spouseObstacleCheck(new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4499", false), farmHouse, spouse, force: true);
                }
                else if (r.NextDouble() < 0.45)
                {
                    Vector2 spot = Utility.PointToVector2(farmHouse.GetSpouseRoomSpot());
                    spouse.setTilePosition((int)spot.X, (int)spot.Y);
                    spouse.faceDirection(0);
                    spouse.setSpouseRoomMarriageDialogue();
                    if (spouse.Name == "Sebastian" && Game1.netWorldState.Value.hasWorldStateID("sebastianFrog"))
                    {
                        Point frog_spot = farmHouse.GetSpouseRoomCorner();
                        frog_spot.X += 2;
                        frog_spot.Y += 5;
                        spouse.setTilePosition(frog_spot);
                        spouse.faceDirection(2);
                    }
                }
                else
                {
                    spouse.setTilePosition(farmHouse.getKitchenStandingSpot());
                    spouse.faceDirection(0);
                    if (r.NextDouble() < 0.2)
                    {
                        spouse.setRandomAfternoonMarriageDialogue(Game1.timeOfDay, farmHouse);
                    }
                }

            }
        }

        public static bool spouseObstacleCheck(MarriageDialogueReference backToBedMessage, GameLocation currentLocation, NPC spouse, bool force = false)
        {
            if (force || NPC.checkTileOccupancyForSpouse(currentLocation, spouse.Tile, spouse.Name))
            {
                FarmHouse farmHouse = Game1.RequireLocation<FarmHouse>(Game1.player.homeLocation.Value);


                Game1.warpCharacter(spouse, "FarmHouse", ModEntry.GetSpouseBedPosition(farmHouse, spouse.Name));  //Using my method for bed position


                spouse.faceDirection(1);
                spouse.currentMarriageDialogue.Clear();
                spouse.currentMarriageDialogue.Add(backToBedMessage);
                spouse.shouldSayMarriageDialogue.Value = true;
                return true;
            }
            else
            {

                FarmHouse farmHouse = Game1.RequireLocation<FarmHouse>(Game1.player.homeLocation.Value);
                Game1.warpCharacter(spouse, "FarmHouse", farmHouse.getSpouseBedSpot(spouse.Name));
                spouse.faceDirection(1);
                spouse.currentMarriageDialogue.Clear();
                spouse.currentMarriageDialogue.Add(backToBedMessage);
                spouse.shouldSayMarriageDialogue.Value = true;
                return true;


            }


            return false;
        }


    }
}