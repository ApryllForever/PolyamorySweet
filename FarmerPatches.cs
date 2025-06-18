
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Extensions;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PolyamorySweetLove
{
    public static class FarmerPatches
    {
        private static IMonitor Monitor;
        private static IModHelper Helper;

        // call this method from your Entry class
        public static void Initialize(IMonitor monitor, ModConfig config, IModHelper helper)
        {
            Monitor = monitor;
            Helper = helper;
        }
        public static bool Farmer_doDivorce_Prefix(ref Farmer __instance)
        {
            try
            {
                Monitor.Log("Trying to divorce");
                __instance.divorceTonight.Value = false;
                if (!__instance.isMarriedOrRoommates() || ModEntry.spouseToDivorce == null)
                {
                    Monitor.Log("Tried to divorce but no spouse to divorce!");
                    return false;
                }

                string key = ModEntry.spouseToDivorce;

                int points = 2000;
                if (ModEntry.divorceHeartsLost < 0)
                {
                    points = 0;
                }
                else
                {
                    points -= ModEntry.divorceHeartsLost * 250;
                }

                if (__instance.friendshipData.ContainsKey(key))
                {
                    Monitor.Log($"Divorcing {key}");
                    __instance.friendshipData[key].Points = Math.Min(2000, Math.Max(0, points));
                    Monitor.Log($"Resulting points: {__instance.friendshipData[key].Points}");

                    __instance.friendshipData[key].Status = points < 1000 ? FriendshipStatus.Divorced : FriendshipStatus.Friendly;
                    Monitor.Log($"Resulting friendship status: {__instance.friendshipData[key].Status}");

                    __instance.friendshipData[key].RoommateMarriage = false;

                    NPC ex = Game1.getCharacterFromName(key);
                    ex.PerformDivorce();

                    if (ex.modData.ContainsKey("ApryllForever.PolyamorySweetLove/WeddingDate")) 
                    {
                        ex.modData.Remove("ApryllForever.PolyamorySweetLove/WeddingDate");
                    }
                    if (__instance.spouse == key)
                    {
                        __instance.spouse = null;
                    }
                    ModEntry.currentSpouses.Remove(__instance.UniqueMultiplayerID);
                    ModEntry.currentUnofficialSpouses.Remove(__instance.UniqueMultiplayerID);
                    ModEntry.ResetSpouses(__instance);
                    Helper.GameContent.InvalidateCache("Maps/FarmHouse1_marriage");
                    Helper.GameContent.InvalidateCache("Maps/FarmHouse2_marriage");

                    Monitor.Log($"New spouse: {__instance.spouse}, married {__instance.isMarriedOrRoommates()}");

                    Utility.getHomeOfFarmer(__instance).showSpouseRoom();
                    Utility.getHomeOfFarmer(__instance).setWallpapers();
                    Utility.getHomeOfFarmer(__instance).setFloors();

                    Game1.getFarm().addSpouseOutdoorArea(__instance.spouse == null ? "" : __instance.spouse);
                    NPC nPC = Game1.getCharacterFromName(key);
                }

                ModEntry.spouseToDivorce = null;
                
                return false;
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(Farmer_doDivorce_Prefix)}:\n{ex}", LogLevel.Error);
            }
            return true;
        }

        public static bool Farmer_isMarried_Prefix(Farmer __instance, ref bool __result)
        {
            try
            {
                __result = __instance.team.IsMarried(__instance.UniqueMultiplayerID) || ModEntry.GetSpouses(__instance, false).Count > 0;
                return false;
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(Farmer_isMarried_Prefix)}:\n{ex}", LogLevel.Error);
            }
            return true;
        }

        public static bool Farmer_checkAction_Prefix(Farmer __instance, Farmer who, GameLocation location, ref bool __result)
        {
            try
            {
                if (who.isRidingHorse())
                {
                    who.Halt();
                }
                if (__instance.hidden.Value)
                {
                    return true;
                }
                if (Game1.CurrentEvent == null && who.CurrentItem != null && who.CurrentItem.ParentSheetIndex == 801 && !Game1.player.isEngaged() && !who.isEngaged())
                {
                    who.Halt();
                    who.faceGeneralDirection(__instance.getStandingPosition(), 0, false);
                    string question2 = Game1.content.LoadString("Strings\\UI:AskToMarry_" + (__instance.IsMale ? "Male" : "Female"), __instance.Name);
                    location.createQuestionDialogue(question2, location.createYesNoResponses(), delegate (Farmer _, string answer)
                    {
                        if (answer == "Yes")
                        {
                            //ModEntry.ActiveFarmerProposal = true;
                           // ModEntry.proposalReceiver = __instance;
                           // ModEntry.proposalSender = Game1.player;

                            who.team.SendProposal(__instance, ProposalType.Marriage, who.CurrentItem.getOne());
                            Game1.activeClickableMenu = new PendingProposalDialog();

                            //who.team.SendProposal(__instance, ProposalType.Marriage, who.CurrentItem.getOne());
                            //Game1.activeClickableMenu = new MermaidPendingProposalDialog();

                            //ModEntry.ActiveFarmerProposal = true;
                        }
                    }, null);

                 


                    __result = true;
                    return false;
                }
                /*
                 * 
                 * Ceres Pendant is not necessary currently, will not work. Was going to be a farmer wedding item; looking more into working with the vanilla code
                 * 
                if (Game1.CurrentEvent == null && who.CurrentItem != null && who.CurrentItem.Name == "Ceres Pendant" && !Game1.player.isEngaged() && !who.isEngaged())
                {
                    who.Halt();
                    who.faceGeneralDirection(__instance.getStandingPosition(), 0, false);
                    string question2 = Game1.content.LoadString("Strings\\UI:AskToMarry_" + (__instance.IsMale ? "Male" : "Female"), __instance.Name);
                    location.createQuestionDialogue(question2, location.createYesNoResponses(), delegate (Farmer _, string answer)
                    {
                        if (answer == "Yes")
                        {
                             ModEntry.ActiveFarmerProposal = true;
                             ModEntry.proposalReceiver = __instance;
                             ModEntry.proposalSender = Game1.player;

                            who.team.SendProposal(__instance, ProposalType.Gift, who.ActiveObject.getOne());
                            Game1.activeClickableMenu = new PendingProposalDialog();


                        }
                    }, null);
                    __result = true;
                    return false;
                }
                */



            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(Farmer_checkAction_Prefix)}:\n{ex}", LogLevel.Error);
            }
            return true;
        }
        public static bool skipSpouse = false;
        public static void Farmer_spouse_Postfix(Farmer __instance, ref string __result)
        {
            if (skipSpouse)
                return;
            try
            {
                skipSpouse = true;
                if (ModEntry.tempOfficialSpouse != null && __instance.friendshipData.ContainsKey(ModEntry.tempOfficialSpouse.Name) && __instance.friendshipData[ModEntry.tempOfficialSpouse.Name].IsMarried())
                {
                    __result = ModEntry.tempOfficialSpouse.Name;
                }
                else
                {
                    var spouses = ModEntry.GetSpouses(__instance, true);
                    string aspouse = null;
                    foreach (var spouse in spouses)
                    {
                        if (aspouse is null)
                            aspouse = spouse.Key;
                    }
                    if (__result is null && aspouse is not null)
                    {
                        __result = aspouse;
                    }
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(Farmer_spouse_Postfix)}:\n{ex}", LogLevel.Error);
            }
            skipSpouse = false;
        }

        public static void Farmer_getSpouse_Postfix(Farmer __instance, ref NPC __result)
        {
            try
            {

                if (ModEntry.tempOfficialSpouse != null && __instance.friendshipData.ContainsKey(ModEntry.tempOfficialSpouse.Name) && __instance.friendshipData[ModEntry.tempOfficialSpouse.Name].IsMarried())
                {
                    __result = ModEntry.tempOfficialSpouse;
                }
                else
                {
                    var spouses = ModEntry.GetSpouses(__instance, true);
                    NPC aspouse = null;
                    foreach (var spouse in spouses)
                    {
                        if (aspouse is null)
                            aspouse = spouse.Value;
                    }
                    if (__result is null && aspouse is not null)
                    {
                        __result = aspouse;
                    }
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(Farmer_getSpouse_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }

        public static bool Farmer_GetSpouseFriendship_Prefix(Farmer __instance, ref Friendship __result)
        {
            try
            {

                if (ModEntry.tempOfficialSpouse != null && __instance.friendshipData.ContainsKey(ModEntry.tempOfficialSpouse.Name) && __instance.friendshipData[ModEntry.tempOfficialSpouse.Name].IsMarried())
                {
                    __result = __instance.friendshipData[ModEntry.tempOfficialSpouse.Name];
                    return false;
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(Farmer_GetSpouseFriendship_Prefix)}:\n{ex}", LogLevel.Error);
            }
            return true;
        }
        internal static bool Farmer_getChildren_Prefix(Farmer __instance, ref List<Child> __result)
        {
            try
            {
                if (EventPatches.startingLoadActors && Game1Patches.lastGotCharacter != null && __instance != null)
                {
                    var spouses = ModEntry.GetSpouses(__instance, true);
                    string? primary = null;
                    if (spouses.Count > 0)
                    {
                        primary = spouses.First().Key;
                    }
                    bool assignUnknown = Game1Patches.lastGotCharacter == primary;  // Assigning children without parent data to the primary spouse (children born while this mod was not installed)
                    __result = Utility.getHomeOfFarmer(__instance)?.getChildren()?.FindAll(c => (c.modData.TryGetValue("ApryllForever.PolyamorySweetLove/OtherParent", out string parent) && parent == Game1Patches.lastGotCharacter) || (parent == null && assignUnknown)) ?? new List<Child>();
                    return false;
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(Farmer_getChildren_Prefix)}:\n{ex}", LogLevel.Error);
            }
            return true;
        }

        public static bool Farmer_GetDaysMarried_Prefix(Farmer __instance, ref int  __result)
        {
            try
            {
                if (ModEntry.tempOfficialSpouse != null && __instance.friendshipData.ContainsKey(ModEntry.tempOfficialSpouse.Name) && __instance.friendshipData[ModEntry.tempOfficialSpouse.Name].IsMarried())
                {
                    __result = __instance.friendshipData[ModEntry.tempOfficialSpouse.Name].DaysMarried;
                    return false;
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(Farmer_GetDaysMarried_Prefix)}:\n{ex}", LogLevel.Error);
            }
            return true;
        }

        public static bool Farmer_getChildrenCount_Prefix(Farmer __instance, ref int __result)
        {
            try
            {
                if (ModEntry.tempOfficialSpouse != null)
                {
                    __result = Utility.getHomeOfFarmer(__instance).getChildren().FindAll(c => (c.modData.TryGetValue("ApryllForever.PolyamorySweetLove/OtherParent", out string parent) && parent == ModEntry.tempOfficialSpouse.Name)).Count;
                    return false;
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(Farmer_getChildrenCount_Prefix)}:\n{ex}", LogLevel.Error);
            }
            return true;
        }

        public static void Utility_CreateDaySaveRandom_Postfix(ref Random __result)
        {
            try
            {
                if (ModEntry.tempOfficialSpouse != null)
                {
                    Farmer farmer = ModEntry.tempOfficialSpouse.getSpouse();
                    List<string> spouses = ModEntry.GetSpouses(farmer, true).Keys.ToList();
                    spouses.Sort();
                    int index = spouses.IndexOf(ModEntry.tempOfficialSpouse.Name);
                    for (int i = 0; i < index; i++)
                        __result.NextBool();
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(Utility_CreateDaySaveRandom_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }



 //* 
 //* This is something I am expirimenting on for errors in Multiplayer weddings.

public static bool FarmerTeam_handleIncomingProposal_Prefix(Proposal proposal, FarmerTeam __instance, ref bool __result)
        {
           

            if (!Game1.IsMultiplayer)
            {
                return true;
            }

            if (proposal.proposalType.Value == ProposalType.Marriage)
            {
                string additionalVar = "";
                string responseYes = "";
                string responseNo = "";
                string questionKey = "";

                if ( Game1.player.isEngaged())
                {
                    proposal.response.Value = ProposalResponse.Rejected;
                    proposal.responseMessageKey.Value = ModEntry.genderedKey("Strings\\UI:AskedToMarry_NotSingle", Game1.player);
                    __result = true;
                    return false;
                }

                questionKey = "Strings\\UI:AskedToMarry";
                responseYes = "Strings\\UI:AskedToMarry_Accepted";
                responseNo = "Strings\\UI:AskedToMarry_Rejected";

                questionKey = ModEntry.genderedKey(questionKey, proposal.sender.Value);
                if (responseYes != null)
                {
                    responseYes = ModEntry.genderedKey(responseYes, Game1.player);
                }
                if (responseNo != null)
                {
                    responseNo = ModEntry.genderedKey(responseNo, Game1.player);
                }


                string question = Game1.content.LoadString(questionKey, proposal.sender.Value.Name);
                Game1.currentLocation.createQuestionDialogue(question, Game1.currentLocation.createYesNoResponses(), delegate (Farmer _, string answer)
                {
                    if (proposal.canceled.Value)
                    {
                        Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:ProposalWithdrawn", proposal.sender.Value.Name));
                        proposal.response.Value = ProposalResponse.Rejected;
                        proposal.responseMessageKey.Value = responseNo;
                    }
                    else if (answer == "Yes")
                    {
                        proposal.response.Value = ProposalResponse.Accepted;
                        proposal.responseMessageKey.Value = responseYes;
                        if ( proposal.proposalType.Value == ProposalType.Marriage)
                        {
                            Item value = proposal.gift.Value;
                            proposal.gift.Value = null;
                            value = Game1.player.addItemToInventory(value);
                            if (value != null)
                            {
                                Game1.currentLocation.debris.Add(new Debris(value, Game1.player.Position));
                            }
                       
                            Friendship friendship2 = __instance.GetFriendship(proposal.sender.Value.UniqueMultiplayerID, Game1.player.UniqueMultiplayerID);
                            friendship2.Status = FriendshipStatus.Engaged;
                            friendship2.Proposer = proposal.sender.Value.UniqueMultiplayerID;
                            WorldDate worldDate2 = new WorldDate(Game1.Date);
                            worldDate2.TotalDays += 3;
                            while (!Game1.canHaveWeddingOnDay(worldDate2.DayOfMonth, worldDate2.Season))
                            {
                                worldDate2.TotalDays++;
                            }
                            friendship2.WeddingDate = worldDate2;
                            Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:PlayerWeddingArranged"));
                            Game1.Multiplayer.globalChatInfoMessage("Engaged", Game1.player.Name, proposal.sender.Value.Name);
                        }

                        //ModEntry.proposalSender = proposal.sender.Value;
                        //ModEntry.proposalReceiver = proposal.receiver.Value;

                        Game1.player.doEmote(20);
                    }
                    else
                    {
                        proposal.response.Value = ProposalResponse.Rejected;
                        proposal.responseMessageKey.Value = responseNo;
                    }
                });

                __result = true;
                return false;
            }

            return true;
        }


        






    }
}