using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Netcode;
using PolyamorySweetLove;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Characters;
using StardewValley.Events;
using StardewValley.Extensions;
using StardewValley.Locations;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace PolyamorySweetLove
{
    public partial class ModEntry
    {
        public static NetFarmerPairDictionary<Friendship, NetRef<Friendship>> friendshipData = new NetFarmerPairDictionary<Friendship, NetRef<Friendship>>();
        public static bool Utility_pickPersonalFarmEvent_Prefix(ref FarmEvent __result)
        {
            if (!Config.EnableMod)
                return true;
            Random r = Utility.CreateRandom(Game1.stats.DaysPlayed, Game1.uniqueIDForThisGame / 2, 470124797.0, Game1.player.UniqueMultiplayerID);

            SMonitor.Log("picking event");
            if (Game1.weddingToday)
            {
                __result = null;
                return false;
            }

            if (ModEntry.BabyTonight)
            {


                SMonitor.Log("Requesting a baby using Aphrodite Flower!");
                lastPregnantSpouse = Game1.getCharacterFromName(BabyTonightSpouse);
                __result= new QuestionEvent(1);

                return false;
            }

            List<NPC> allSpouses = GetSpouses(Game1.player, true).Values.ToList();

            ShuffleList(ref allSpouses);

            List<string> farmerlist = new List<string>();
            List<string> farmerSpouse = new List<string>();

            foreach (Farmer allFarmer in Game1.getAllFarmers())
            {

                farmerlist.Add(allFarmer.Name);
                if (allSpouses.ToString().Contains(allFarmer.Name))
                {
                    farmerSpouse.Add(allFarmer.Name);
                }
         

            }

            foreach (NPC spouse in allSpouses)
            {
                if (spouse == null)
                {
                    SMonitor.Log($"Utility_pickPersonalFarmEvent_Prefix spouse is null");
                    continue;
                }
                Farmer f = spouse.getSpouse();

                Friendship friendship = f.friendshipData[spouse.Name];

                if (friendship.DaysUntilBirthing <= 0 && friendship.NextBirthingDate != null)
                {
                    lastPregnantSpouse = null;
                    lastBirthingSpouse = spouse;



                    if (spouse.IsVillager)
                    {
                        __result = new BirthingEvent();
                        return false;
                    }

                    //if (spouse.IsVillager)
                    if (!farmerlist.Contains(spouse.Name))
                    {
                        __result = new BirthingEvent();
                         return false;
                    }
                    else if(Game1.IsMultiplayer)
                    {
                        long spouseID;

                        if (Game1.player.team.GetSpouse(Game1.player.UniqueMultiplayerID).HasValue )
                        {
                            spouseID = Game1.player.team.GetSpouse(Game1.player.UniqueMultiplayerID).Value;
                            if (Game1.otherFarmers.ContainsKey(spouseID))
                            {
                                __result = new PlayerCoupleBirthingEvent();
                            }
                        }

                  
                    }


                    return false;
                }
            }




          


            if (plannedParenthoodAPI is not null && plannedParenthoodAPI.GetPartnerTonight() is not null)
            {
                SMonitor.Log($"Handing farm sleep event off to Polyamory Sweet Parenthood");
                return true;
            }

            lastBirthingSpouse = null;
            lastPregnantSpouse = null;

            foreach (NPC spouse in allSpouses)
            {
                if (spouse == null)
                    continue;
                Farmer f = spouse.getSpouse();
                if (!Config.RoommateRomance && f.friendshipData[spouse.Name].RoommateMarriage)
                    continue;

                int heartsWithSpouse = f.getFriendshipHeartLevelForNPC(spouse.Name);
                Friendship friendship = f.friendshipData[spouse.Name];
                List<Child> kids = f.getChildren();
                int maxChildren = childrenAPI == null ? Config.MaxChildren : childrenAPI.GetMaxChildren();
                FarmHouse fh = Utility.getHomeOfFarmer(f);

                // bool can = spouse.daysAfterLastBirth <= 0 && fh.cribStyle.Value > 0 && fh.upgradeLevel >= 2 && friendship.DaysUntilBirthing < 0 && heartsWithSpouse >= 10 && friendship.DaysMarried >= 7 && (kids.Count < maxChildren);
                //this was the old preg check
                long spouseID = 0;
                
                if (Game1.IsMultiplayer)
                {
                    // foreach (KeyValuePair<FarmerPair, Friendship> pair in friendshipData.Pairs)
                    //{
                    //    if (pair.Key.Contains(farmer) && (pair.Value.IsEngaged() || pair.Value.IsMarried()))
                    //   {
                    //spouseID = pair.Key.GetOther(farmer);
                    //  }
                    // }

                    if (Game1.player.team.GetSpouse(Game1.player.UniqueMultiplayerID) != null)
                    {
                        spouseID =   Game1.player.team.GetSpouse(Game1.player.UniqueMultiplayerID).Value;
                    }
                }

                bool? flag = spouse?.canGetPregnant();

                //if(Config.GayPregnancies) Classic example of me forgetting what I was doing halfway through...
               // {
                ///    flag = true;
               // }

                SMonitor.Log($"Checking ability to get pregnant: {spouse.Name} {flag}:{(fh.cribStyle.Value > 0 ? $" no crib" : "")}{(Utility.getHomeOfFarmer(f).upgradeLevel < 2 ? $" house level too low {Utility.getHomeOfFarmer(f).upgradeLevel}" : "")}{(friendship.DaysMarried < 7 ? $", not married long enough {friendship.DaysMarried}" : "")}{(friendship.DaysUntilBirthing >= 0 ? $", already pregnant (gives birth in: {friendship.DaysUntilBirthing})" : "")}");

                if (!farmerlist.Contains(spouse.Name))
                {
                    if (flag.HasValue && flag.GetValueOrDefault() && Game1.player.currentLocation == Game1.getLocationFromName(Game1.player.homeLocation.Value) && r.NextDouble() < Config.PercentChanceForBirthingQuestion && GameStateQuery.CheckConditions(spouse.GetData()?.SpouseWantsChildren))
                    //if (flag.HasValue && flag.GetValueOrDefault() && Game1.player.currentLocation == Game1.getLocationFromName(Game1.player.homeLocation.Value) && r.NextDouble() < Config.PercentChanceForBirthingQuestion && GameStateQuery.CheckConditions(spouse.GetData()?.SpouseWantsChildren))
                    {
                        SMonitor.Log("Requesting a baby!");
                        lastPregnantSpouse = spouse;
                        __result= new QuestionEvent(1);
                        return false;
                    }
                }

                else
                if (spouseID != 0)
                {

                    if (Game1.otherFarmers.TryGetValue(spouseID, out var farmereposa))
                    {

                        Farmer eposa = farmereposa;
                        if (eposa.currentLocation == Game1.player.currentLocation && (spouse.currentLocation == Game1.getLocationFromName(eposa.homeLocation.Value) || eposa.currentLocation == Game1.getLocationFromName(Game1.player.homeLocation.Value)) && Utility.playersCanGetPregnantHere(spouse.currentLocation as FarmHouse))
                        {
                            SMonitor.Log("Requesting player to get pregnant!");

                            __result = new QuestionEvent(3);
                            return false;
                        }
                    }
                }
    //pre 1.3 way
                //SMonitor.Log($"Checking ability to get pregnant: {spouse.Name} {flag}:{(fh.cribStyle.Value > 0 ? $" no crib" : "")}{(Utility.getHomeOfFarmer(f).upgradeLevel < 2 ? $" house level too low {Utility.getHomeOfFarmer(f).upgradeLevel}" : "")}{(friendship.DaysMarried < 7 ? $", not married long enough {friendship.DaysMarried}" : "")}{(friendship.DaysUntilBirthing >= 0 ? $", already pregnant (gives birth in: {friendship.DaysUntilBirthing})" : "")}");
               // if (can && Game1.player.currentLocation == Game1.getLocationFromName(Game1.player.homeLocation.Value) && myRand.NextDouble() < 0.05)
               // {
                //    SMonitor.Log("Requesting a baby!");
                 //   lastPregnantSpouse = spouse;
                  //  __result = new QuestionEvent(1);
                  //  return false;
               // }
            }
            return true;
        }

        public static NPC lastPregnantSpouse;
        private static NPC lastBirthingSpouse;

        public static bool QuestionEvent_setUp_Prefix(int ___whichQuestion, ref bool __result)
        {
            if (Config.EnableMod && ___whichQuestion == 1)
            {
                if (lastPregnantSpouse == null)
                {
                    SMonitor.Log("PSL - lastPregnantSpouse was null, we are somehow screwed, returning to vanilla question event.");
                    __result = true;
                    return false;
                }
                Response[] answers = new Response[]
                {
                    new Response("Yes", Game1.content.LoadString("Strings\\Events:HaveBabyAnswer_Yes")),
                    new Response("Not", Game1.content.LoadString("Strings\\Events:HaveBabyAnswer_No"))
                };

                if ( Config.GayPregnancies)
                {
                    Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\Events:HavePlayerBabyQuestion", lastPregnantSpouse.Name), answers, new GameLocation.afterQuestionBehavior(answerPregnancyQuestion), lastPregnantSpouse);
                }

                else if (!lastPregnantSpouse.isAdoptionSpouse())
                {
                    Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\Events:HavePlayerBabyQuestion", lastPregnantSpouse.Name), answers, new GameLocation.afterQuestionBehavior(answerPregnancyQuestion), lastPregnantSpouse);
                }

                else
                {
                    Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\Events:HavePlayerBabyQuestion_Adoption", lastPregnantSpouse.Name), answers, new GameLocation.afterQuestionBehavior(answerPregnancyQuestion), lastPregnantSpouse);
                }
                Game1.messagePause = true;
                __result = false;
                return false;
            }
            return true;
        }

        public static bool BirthingEvent_tickUpdate_Prefix(GameTime time, BirthingEvent __instance, ref bool __result, ref int ___timer, string ___soundName, ref bool ___playedSound, string ___message, ref bool ___naming, bool ___getBabyName, bool ___isMale, string ___babyName)
        {
            if (!Config.EnableMod || !___getBabyName)
            {
                SMonitor.Log("PSL - ___getBabyName was false!");
                return true;
            }

            if (Config.ImpregnatingMother)
            {
                if (Game1.player.Gender == Gender.Female)
                {
                    ___isMale = false;
                }
            }

            Game1.player.CanMove = false;
            ___timer += time.ElapsedGameTime.Milliseconds;
            Game1.fadeToBlackAlpha = 1f;

            if (!___naming)
            {
                //Game1.activeClickableMenu = new NamingMenu(new NamingMenu.doneNamingBehavior(__instance.returnBabyName), Game1.content.LoadString(___isMale ? "Strings\\Events:BabyNamingTitle_Male" : "Strings\\Events:BabyNamingTitle_Female"), "");
                Game1.activeClickableMenu = new NamingMenu(__instance.returnBabyName, Game1.content.LoadString(___isMale ? "Strings\\Events:BabyNamingTitle_Male" : "Strings\\Events:BabyNamingTitle_Female"), "");

                ___naming = true;
            }
            if (___babyName != null && ___babyName != "" && ___babyName.Length > 0)
            {
                if (lastBirthingSpouse != null)
                {

                }

                try
                {

                    SMonitor.Log("PSL - Setting up the new baby!");

                    bool isDarkSkinned;

                    if (!Game1.IsMultiplayer)
                    {
                        //double chance = (lastBirthingSpouse.Name.Equals("Maru") || lastBirthingSpouse.Name.Equals("Krobus")) ? 0.5 : 0.0;
                        //chance += (Game1.player.hasDarkSkin() ? 0.5 : 0.0);
                        //isDarkSkinned = new Random((int)Game1.uniqueIDForThisGame + (int)Game1.stats.DaysPlayed).NextDouble() < chance;
                        if (lastBirthingSpouse != null)
                        {
                            double chance = (lastBirthingSpouse.hasDarkSkin() ? 0.5 : 0.0) + (Game1.player.hasDarkSkin() ? 0.5 : 0.0);
                            isDarkSkinned = Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed).NextBool(chance);
                        }
                        else
                        {
                            NPC spouse = Game1.player.getSpouse();
                            double chance = (spouse.hasDarkSkin() ? 0.5 : 0.0) + (Game1.player.hasDarkSkin() ? 0.5 : 0.0);
                            isDarkSkinned = Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed).NextBool(chance);

                        }

                    }

                    else
                    {
                        NPC spouse = Game1.player.getSpouse();
                        double chance = (Game1.player.hasDarkSkin()? 0.5 : 0.0);
                        chance += (spouse.hasDarkSkin() ? 0.5 : 0.0);

                        isDarkSkinned = new Random((int)Game1.uniqueIDForThisGame + (int)Game1.stats.DaysPlayed).NextDouble() < chance;
                    }


                    string newBabyName = ___babyName;
                    List<NPC> all_characters = Utility.getAllCharacters();
                    bool collision_found = false;
                    do
                    {
                        collision_found = false;
                        using (List<NPC>.Enumerator enumerator = all_characters.GetEnumerator())
                        {
                            while (enumerator.MoveNext())
                            {
                                if (Game1.characterData.ContainsKey(newBabyName))
                                {
                                    newBabyName += " ";
                                    collision_found = true;
                                    break;
                                }
                                if (enumerator.Current.Name.Equals(newBabyName))
                                {
                                    newBabyName += " ";
                                    collision_found = true;
                                    break;
                                }
                            }
                        }
                    }
                    while (collision_found);


                    Child baby = new Child(newBabyName, ___isMale, isDarkSkinned, Game1.player)
                    {
                        Age = 0,
                        Position = new Vector2(16f, 4f) * 64f + new Vector2(0f + myRand.Next(-64, 48), -24f + myRand.Next(-24, 24)),
                    };
                    baby.modData["ApryllForever.PolyamorySweetLove/OtherParent"] = lastBirthingSpouse.displayName;

                    Utility.getHomeOfFarmer(Game1.player).characters.Add(baby);
                    Game1.playSound("smallSelect");
                    Game1.getCharacterFromName(lastBirthingSpouse.Name).daysAfterLastBirth = 5;
                    Game1.player.friendshipData[lastBirthingSpouse.Name].NextBirthingDate = null;
                    if (Game1.player.getChildrenCount() == 2)
                    {
                        Game1.getCharacterFromName(lastBirthingSpouse.Name).shouldSayMarriageDialogue.Value = true;
                        Game1.getCharacterFromName(lastBirthingSpouse.Name).currentMarriageDialogue.Insert(0, new MarriageDialogueReference("Data\\ExtraDialogue", "NewChild_SecondChild" + myRand.Next(1, 3), true, new string[0]));
                        Game1.getSteamAchievement("Achievement_FullHouse");
                    }
                    else if (lastBirthingSpouse.isAdoptionSpouse() && !Config.GayPregnancies)
                    {
                        Game1.getCharacterFromName(lastBirthingSpouse.Name).currentMarriageDialogue.Insert(0, new MarriageDialogueReference("Data\\ExtraDialogue", "NewChild_Adoption", true, new string[]
                        {
                        ___babyName
                        }));
                    }
                    else
                    {
                        Game1.getCharacterFromName(lastBirthingSpouse.Name).currentMarriageDialogue.Insert(0, new MarriageDialogueReference("Data\\ExtraDialogue", "NewChild_FirstChild", true, new string[]
                        {
                        ___babyName
                        }));
                    }
                    string spouseName = lastBirthingSpouse.Name;
                    Game1.morningQueue.Enqueue(delegate
                    {
                        mp.globalChatInfoMessage("Baby", new string[]
                        {
                        Lexicon.capitalize(Game1.player.Name),
                            spouseName,
                        Lexicon.getGenderedChildTerm(___isMale),
                        Lexicon.getPronoun(___isMale),
                        baby.displayName
                        });
                    });
                    if (Game1.keyboardDispatcher != null)
                    {
                        Game1.keyboardDispatcher.Subscriber = null;
                    }
                    Game1.player.Position = Utility.PointToVector2(Utility.getHomeOfFarmer(Game1.player).getBedSpot()) * 64f;
                    Game1.globalFadeToClear(null, 0.02f);
                    lastBirthingSpouse = null;
                    __result = true;
                    return false;
                }
                catch
                {

                    return true;
                }
            }
            __result = false;
            return false;
        }


        public static bool BirthingEvent_setUp_Prefix(ref bool ___isMale, ref string ___message, ref bool __result)
        {
            if (!Config.EnableMod)
                return true;
            if (lastBirthingSpouse == null)
            {
                __result = true;
                return false;
            }
            NPC spouse = lastBirthingSpouse;
            Game1.player.CanMove = false;

            if (Config.ImpregnatingMother)
            {
                if (Game1.player.Gender == Gender.Female)
                {
                    ___isMale = false;



                }
                else
                {
                    ___isMale = myRand.NextDouble() > Config.PercentChanceForBirthSex;
                }
            }
            else
            {
                ___isMale = myRand.NextDouble() > Config.PercentChanceForBirthSex;
            }

            if (Config.ImpregnatingFemmeNPC)
            {
                ___message = Game1.content.LoadString("Strings\\Events:BirthMessage_PlayerMother", Lexicon.getGenderedChildTerm(___isMale), Game1.player.displayName);
                SMonitor.Log("PSL - Player is giving birth! Impregnating Femme NPC");
            }

            else if (Config.GayPregnancies )
            {
                ___message = Game1.content.LoadString("Strings\\Events:BirthMessage_SpouseMother", Lexicon.getGenderedChildTerm(___isMale), spouse.displayName);
                SMonitor.Log("PSL - Wife is giving birth, lesbian pregnancy!");
            }


            else if (spouse.isAdoptionSpouse() && !Config.GayPregnancies)
            {
                ___message = Game1.content.LoadString("Strings\\Events:BirthMessage_Adoption", Lexicon.getGenderedChildTerm(___isMale));
                SMonitor.Log("PSL - Adopted Baby is here!");
            }
            else if (spouse.Gender == 0)
            {
                ___message = Game1.content.LoadString("Strings\\Events:BirthMessage_PlayerMother", Lexicon.getGenderedChildTerm(___isMale));
                SMonitor.Log("PSL - Player is giving birth!");
            }

            else
            {
                ___message = Game1.content.LoadString("Strings\\Events:BirthMessage_SpouseMother", Lexicon.getGenderedChildTerm(___isMale), spouse.displayName);
                SMonitor.Log("PSL - Wife is giving birth!");
            }
            __result = false;
            return false;
        }

        public static void answerPregnancyQuestion(Farmer who, string answer)
        {
            if (answer == "Yes" && who is not null && lastPregnantSpouse is not null && who.friendshipData.ContainsKey(lastPregnantSpouse.Name))
            {
                WorldDate birthingDate = new WorldDate(Game1.Date);
                birthingDate.TotalDays += 14;
                who.friendshipData[lastPregnantSpouse.Name].NextBirthingDate = birthingDate;
                SMonitor.Log("PSL - The pregnancy has begun!!!");

                //lastPregnantSpouse.isAdoptionSpouse(); why in the fucking hell is this line even here??? It seems to be fucking everything up in regards to sapphic pregnancies.
            }
        }
    }
}