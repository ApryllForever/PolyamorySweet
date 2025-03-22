using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using PolyamorySweetLove;
using SpaceCore.Events;
using StardewModdingAPI;
using StardewModdingAPI.Enums;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Audio;
using StardewValley.Characters;
using StardewValley.Events;
using StardewValley.Extensions;
using StardewValley.GameData.Shops;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Pathfinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using xTile;
using xTile.Dimensions;
using System.Collections.Specialized;
using StardewValley.Objects;


namespace PolyamorySweetLove
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {


        public static bool BabyTonight = false;
        public static string BabyTonightSpouse = String.Empty;

        public static bool Proposal_Sweet;
        public static bool AphroditeFlowerGiven = false;

        public static bool PatioPlacement = false;
        public static bool PorchPlacement = false;


        public static bool justEngageinated
        {
            get; set;
        }


        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;
        public static Multiplayer mp;
        public static Random myRand;
        public static string farmHelperSpouse = null;
        internal static NPC tempOfficialSpouse;
        public static int bedSleepOffset = 76;

        public static string spouseToDivorce = null;
        public static int divorceHeartsLost;

        public static Dictionary<long, Dictionary<string, NPC>> currentSpouses = new Dictionary<long, Dictionary<string, NPC>>();
        public static Dictionary<long, Dictionary<string, NPC>> currentUnofficialSpouses = new Dictionary<long, Dictionary<string, NPC>>();
        public static SortedDictionary<NPC, int> sortedSpouses = new SortedDictionary<NPC, int>();

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();
            context = this;
            if (!Config.EnableMod)
                return;

            SMonitor = Monitor;
            SHelper = helper;

            mp = helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();
            myRand = new Random();

            helper.Events.GameLoop.ReturnedToTitle += GameLoop_ReturnedToTitle;
            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
            helper.Events.GameLoop.OneSecondUpdateTicked += GameLoop_OneSecondUpdateTicked;

            helper.Events.Content.AssetRequested += Content_AssetRequested;

            helper.Events.Specialized.LoadStageChanged += OnLoadStageChanged;

            SpaceEvents.BeforeGiftGiven += AforeGiftGiven;
            SpaceEvents.AfterGiftGiven += AfterGiftGiven;

            Helper.ConsoleCommands.Add("Proposal_Sweet_Attempt", "Attempt engagement to a character.", new Action<string, string[]>(proposalAttempt));
            Helper.ConsoleCommands.Add("Proposal_Sweet_Force", "Force engagement to a character.", new Action<string, string[]>(proposalForce));
            Helper.ConsoleCommands.Add("Delete_Weddings", "Deletes all scheduled weddings for the one entering the command.", new Action<string, string[]>(weddingDelete));


            PathFindControllerPatches.Initialize(Monitor, Config, helper);
            Divorce.Initialize(Monitor, Config, helper);
            NPCPatches.Initialize(Monitor, Config, helper);
            Game1Patches.Initialize(Monitor);
            LocationPatches.Initialize(Monitor, Config, helper);
            FarmerPatches.Initialize(Monitor, Config, helper);
            UIPatches.Initialize(Monitor, Config, helper);
            EventPatches.Initialize(Monitor, Config, helper);

            var harmony = new Harmony(ModManifest.UniqueID);


            // npc patches

            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), nameof(NPC.marriageDuties)),
               postfix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_marriageDuties_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), nameof(NPC.getSpouse)),
               prefix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_getSpouse_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), nameof(NPC.isRoommate)),
               prefix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_isRoommate_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), nameof(NPC.isMarried)),
               prefix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_isMarried_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), nameof(NPC.isMarriedOrEngaged)),
               prefix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_isMarriedOrEngaged_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), nameof(NPC.tryToReceiveActiveObject)),
               prefix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_tryToReceiveActiveObject_Prefix)),
               postfix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_tryToReceiveActiveObject_Postfix)),
               transpiler: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_tryToReceiveActiveObject_Transpiler))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), "engagementResponse"),
               prefix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_engagementResponse_Prefix)),
               postfix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_engagementResponse_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), nameof(NPC.spouseObstacleCheck)),
               prefix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_spouseObstacleCheck_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), nameof(NPC.playSleepingAnimation)),
               prefix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_playSleepingAnimation_Prefix)),
               postfix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_playSleepingAnimation_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), nameof(NPC.GetDispositionModifiedString)),
               prefix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_GetDispositionModifiedString_Prefix)),
               postfix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_GetDispositionModifiedString_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), "loadCurrentDialogue"),
               prefix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_loadCurrentDialogue_Prefix)),
               postfix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_loadCurrentDialogue_Postfix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), nameof(NPC.tryToRetrieveDialogue)),
               prefix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_tryToRetrieveDialogue_Prefix))
            );

            //This was moved to Kiss
            //
            //harmony.Patch(
            // original: AccessTools.Method(typeof(NPC), nameof(NPC.checkAction)),
            // prefix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_checkAction_Prefix))
            //);


            // Child patches

            harmony.Patch(
               original: typeof(Character).GetProperty("displayName").GetMethod,
               postfix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.Character_displayName_Getter_Postfix))
            );

            // Path patches

            harmony.Patch(
               original: AccessTools.Constructor(typeof(PathFindController), new Type[] { typeof(Character), typeof(GameLocation), typeof(Point), typeof(int), typeof(bool) }),
               prefix: new HarmonyMethod(typeof(PathFindControllerPatches), nameof(PathFindControllerPatches.PathFindController_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Constructor(typeof(PathFindController), new Type[] { typeof(Character), typeof(GameLocation), typeof(Point), typeof(int), typeof(PathFindController.endBehavior) }),
               prefix: new HarmonyMethod(typeof(PathFindControllerPatches), nameof(PathFindControllerPatches.PathFindController_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Constructor(typeof(PathFindController), new Type[] { typeof(Character), typeof(GameLocation), typeof(Point), typeof(int), typeof(PathFindController.endBehavior), typeof(int) }),
               prefix: new HarmonyMethod(typeof(PathFindControllerPatches), nameof(PathFindControllerPatches.PathFindController_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Constructor(typeof(PathFindController), new Type[] { typeof(Character), typeof(GameLocation), typeof(Point), typeof(int) }),
               prefix: new HarmonyMethod(typeof(PathFindControllerPatches), nameof(PathFindControllerPatches.PathFindController_Prefix))
            );


            // Location patches

            harmony.Patch(
               original: AccessTools.Method(typeof(FarmHouse), nameof(FarmHouse.GetSpouseBed)),
               postfix: new HarmonyMethod(typeof(LocationPatches), nameof(LocationPatches.FarmHouse_GetSpouseBed_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(FarmHouse), nameof(FarmHouse.getSpouseBedSpot)),
               prefix: new HarmonyMethod(typeof(LocationPatches), nameof(LocationPatches.FarmHouse_getSpouseBedSpot_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(Beach), nameof(Beach.checkAction)),
               prefix: new HarmonyMethod(typeof(LocationPatches), nameof(LocationPatches.Beach_checkAction_Prefix))
            );

            harmony.Patch(
              original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.answerDialogue)),
              prefix: new HarmonyMethod(typeof(LocationPatches), nameof(LocationPatches.GameLocation_answerDialogue_Prefix))
           );

            harmony.Patch(
               original: AccessTools.Method(typeof(Beach), "resetLocalState"),
               postfix: new HarmonyMethod(typeof(LocationPatches), nameof(LocationPatches.Beach_resetLocalState_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), "checkEventPrecondition", new Type[] { typeof(string), typeof(bool) }),
               prefix: new HarmonyMethod(typeof(LocationPatches), nameof(LocationPatches.GameLocation_checkEventPrecondition_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(ManorHouse), nameof(ManorHouse.performAction), new Type[] { typeof(string[]), typeof(Farmer), typeof(Location) }),
               prefix: new HarmonyMethod(typeof(LocationPatches), nameof(LocationPatches.ManorHouse_performAction_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(ManorHouse), nameof(ManorHouse.answerDialogueAction)),
               prefix: new HarmonyMethod(typeof(LocationPatches), nameof(LocationPatches.ManorHouse_answerDialogueAction_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.answerDialogueAction)),
               prefix: new HarmonyMethod(typeof(LocationPatches), nameof(LocationPatches.GameLocation_answerDialogueAction_Prefix))
            );


            // pregnancy patches

            harmony.Patch(
               original: AccessTools.Method(typeof(Utility), nameof(Utility.pickPersonalFarmEvent)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Utility_pickPersonalFarmEvent_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(QuestionEvent), nameof(QuestionEvent.setUp)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.QuestionEvent_setUp_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(BirthingEvent), nameof(BirthingEvent.setUp)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.BirthingEvent_setUp_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(BirthingEvent), nameof(BirthingEvent.tickUpdate)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.BirthingEvent_tickUpdate_Prefix))
            );


            // Farmer patches

            harmony.Patch(
               original: AccessTools.Method(typeof(Farmer), nameof(Farmer.doDivorce)),
               prefix: new HarmonyMethod(typeof(FarmerPatches), nameof(FarmerPatches.Farmer_doDivorce_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(Farmer), nameof(Farmer.isMarriedOrRoommates)),
               prefix: new HarmonyMethod(typeof(FarmerPatches), nameof(FarmerPatches.Farmer_isMarried_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(Farmer), nameof(Farmer.getSpouse)),
               postfix: new HarmonyMethod(typeof(FarmerPatches), nameof(FarmerPatches.Farmer_getSpouse_Postfix))
            );
            harmony.Patch(
               original: AccessTools.PropertyGetter(typeof(Farmer), nameof(Farmer.spouse)),
               postfix: new HarmonyMethod(typeof(FarmerPatches), nameof(FarmerPatches.Farmer_spouse_Postfix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(Farmer), nameof(Farmer.GetSpouseFriendship)),
               prefix: new HarmonyMethod(typeof(FarmerPatches), nameof(FarmerPatches.Farmer_GetSpouseFriendship_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(Farmer), nameof(Farmer.checkAction)),
               prefix: new HarmonyMethod(typeof(FarmerPatches), nameof(FarmerPatches.Farmer_checkAction_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(Farmer), nameof(Farmer.getChildren)),
               prefix: new HarmonyMethod(typeof(FarmerPatches), nameof(FarmerPatches.Farmer_getChildren_Prefix))
            );


            // UI patches

            harmony.Patch(
               original: AccessTools.Method(typeof(SocialPage), "drawNPCSlot"),
               prefix: new HarmonyMethod(typeof(UIPatches), nameof(UIPatches.SocialPage_drawNPCSlot_prefix))
            // transpiler: new HarmonyMethod(typeof(UIPatches), nameof(UIPatches.SocialPage_drawSlot_transpiler))
            );

            //  harmony.Patch(
            //   original: AccessTools.Method(typeof(SocialPage), "drawFarmerSlot"),
            //  transpiler: new HarmonyMethod(typeof(UIPatches), nameof(UIPatches.SocialPage_drawSlot_transpiler))
            //  );

            harmony.Patch(
               original: AccessTools.Method(typeof(SocialPage.SocialEntry), nameof(SocialPage.SocialEntry.IsMarriedToAnyone)),
               prefix: new HarmonyMethod(typeof(UIPatches), nameof(UIPatches.SocialPage_isMarriedToAnyone_Prefix))
            );

            harmony.Patch(
               original: typeof(DialogueBox).GetConstructor(new Type[] { typeof(List<string>) }),
               prefix: new HarmonyMethod(typeof(UIPatches), nameof(UIPatches.DialogueBox_Prefix))
            );


            // Event patches

            harmony.Patch(
               original: AccessTools.Method(typeof(Event), nameof(Event.answerDialogueQuestion)),
               prefix: new HarmonyMethod(typeof(EventPatches), nameof(EventPatches.Event_answerDialogueQuestion_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(Event.DefaultCommands), nameof(Event.DefaultCommands.LoadActors)),
               prefix: new HarmonyMethod(typeof(EventPatches), nameof(EventPatches.Event_command_loadActors_Prefix)),
               postfix: new HarmonyMethod(typeof(EventPatches), nameof(EventPatches.Event_command_loadActors_Postfix))
            );


            // Game1 patches

            harmony.Patch(
               original: AccessTools.GetDeclaredMethods(typeof(Game1)).Where(m => m.Name == "getCharacterFromName" && m.ReturnType == typeof(NPC)).First(),
               prefix: new HarmonyMethod(typeof(Game1Patches), nameof(Game1Patches.getCharacterFromName_Prefix))
            );
            //Child Patch
            harmony.Patch(
             original: AccessTools.DeclaredMethod(typeof(Child), nameof(Child.checkAction)),
             prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Child_checkAction_Prefix))
            );
        }

        public override object GetApi()
        {
            return new PolyamorySweetLoveAPI();
        }

        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (!Config.EnableMod)
                return;
            if (e.NameWithoutLocale.IsEquivalentTo("Data/Shops"))
            {
                e.Edit(delegate (IAssetData data)
                {
                    var dict = data.AsDictionary<string, ShopData>();
                    try
                    {
                        for (int i = 0; i < dict.Data["DesertTrade"].Items.Count; i++)
                        {
                            if (dict.Data["DesertTrade"].Items[i].ItemId == "(O)808")
                                dict.Data["DesertTrade"].Items[i].Condition = "PLAYER_FARMHOUSE_UPGRADE Current 1, !PLAYER_HAS_ITEM Current 808";
                        }
                    }
                    catch
                    {

                    }
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Data/Events/HaleyHouse"))
            {
                e.Edit(delegate (IAssetData idata)
                {
                    IDictionary<string, string> data = idata.AsDictionary<string, string>().Data;

                    string key = "195012/f Haley 2500/f Emily 2500/f Penny 2500/f Abigail 2500/f Leah 2500/f Maru 2500/o Abigail/o Penny/o Leah/o Emily/o Maru/o Haley/o Shane/o Harvey/o Sebastian/o Sam/o Elliott/o Alex/e 38/e 2123343/e 10/e 901756/e 54/e 15/k 195019";
                    if (data.TryGetValue(key, out string value))
                    {
                        data[key] = Regex.Replace(value, "(pause 1000/speak Maru \\\")[^$]+.a\\\"", $"$1{SHelper.Translation.Get("confrontation-female")}$h\"/emote Haley 21 true/emote Emily 21 true/emote Penny 21 true/emote Maru 21 true/emote Leah 21 true/emote Abigail 21").Replace("/dump girls 3", "");
                        //data["91740942"] = data["195012/f Haley 2500/f Emily 2500/f Penny 2500/f Abigail 2500/f Leah 2500/f Maru 2500/o Abigail/o Penny/o Leah/o Emily/o Maru/o Haley/o Shane/o Harvey/o Sebastian/o Sam/o Elliott/o Alex/e 38/e 2123343/e 10/e 901756/e 54/e 15/k 195019"];
                    }
                    key = "195012/f Olivia 2500/f Sophia 2500/f Claire 2500/f Haley 2500/f Emily 2500/f Penny 2500/f Abigail 2500/f Leah 2500/f Maru 2500/o Abigail/o Penny/o Leah/o Emily/o Maru/o Haley/o Shane/o Harvey/o Sebastian/o Sam/o Elliott/o Alex/e 38/e 2123343/e 10/e 901756/e 54/e 15/k 195019";
                    if (data.TryGetValue(key, out value))
                    {
                        data[key] = Regex.Replace(value, "(pause 1000/speak Maru \\\")[^$]+.a\\\"", $"$1{SHelper.Translation.Get("confrontation-female")}$h\"/emote Haley 21 true/emote Emily 21 true/emote Penny 21 true/emote Maru 21 true/emote Leah 21 true/emote Abigail 21").Replace("/dump girls 3", "");
                        //data["91740942"] = data["195012/f Haley 2500/f Emily 2500/f Penny 2500/f Abigail 2500/f Leah 2500/f Maru 2500/o Abigail/o Penny/o Leah/o Emily/o Maru/o Haley/o Shane/o Harvey/o Sebastian/o Sam/o Elliott/o Alex/e 38/e 2123343/e 10/e 901756/e 54/e 15/k 195019"];
                    }

                    if (data.TryGetValue("choseToExplain", out value))
                    {
                        data["choseToExplain"] = Regex.Replace(value, "(pause 1000/speak Maru \\\")[^$]+.a\\\"", $"$1{SHelper.Translation.Get("confrontation-female")}$h\"/emote Haley 21 true/emote Emily 21 true/emote Penny 21 true/emote Maru 21 true/emote Leah 21 true/emote Abigail 21").Replace("/dump girls 4", "");
                    }
                    if (data.TryGetValue("lifestyleChoice", out value))
                    {
                        data["lifestyleChoice"] = Regex.Replace(value, "(pause 1000/speak Maru \\\")[^$]+.a\\\"", $"$1{SHelper.Translation.Get("confrontation-female")}$h\"/emote Haley 21 true/emote Emily 21 true/emote Penny 21 true/emote Maru 21 true/emote Leah 21 true/emote Abigail 21").Replace("/dump girls 4", "");
                    }

                });

            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Data/Events/Saloon"))
            {
                e.Edit(delegate (IAssetData idata)
                {
                    IDictionary<string, string> data = idata.AsDictionary<string, string>().Data;
                    string key = "195013/f Shane 2500/f Sebastian 2500/f Sam 2500/f Harvey 2500/f Alex 2500/f Elliott 2500/o Abigail/o Penny/o Leah/o Emily/o Maru/o Haley/o Shane/o Harvey/o Sebastian/o Sam/o Elliott/o Alex/e 911526/e 528052/e 9581348/e 43/e 384882/e 233104/k 195099";
                    if (!data.TryGetValue(key, out string value))
                    {
                        Monitor.Log("Missing event key for Saloon!");
                        return;
                    }
                    data[key] = Regex.Replace(value, "(pause 1000/speak Sam \\\")[^$]+.a\\\"", $"$1{SHelper.Translation.Get("confrontation-male")}$h\"/emote Shane 21 true/emote Sebastian 21 true/emote Sam 21 true/emote Harvey 21 true/emote Alex 21 true/emote Elliott 21").Replace("/dump guys 3", "");
                    //data["19501342"] = Regex.Replace(aData, "(pause 1000/speak Sam \\\")[^$]+.a\\\"",$"$1{SHelper.Translation.Get("confrontation-male")}$h\"/emote Shane 21 true/emote Sebastian 21 true/emote Sam 21 true/emote Harvey 21 true/emote Alex 21 true/emote Elliott 21").Replace("/dump guys 3", "");
                    if (data.TryGetValue("choseToExplain", out value))
                    {
                        data["choseToExplain"] = Regex.Replace(value, "(pause 1000/speak Sam \\\")[^$]+.a\\\"", $"$1{SHelper.Translation.Get("confrontation-male")}$h\"/emote Shane 21 true/emote Sebastian 21 true/emote Sam 21 true/emote Harvey 21 true/emote Alex 21 true/emote Elliott 21").Replace("/dump guys 4", "");
                    }
                    if (data.TryGetValue("crying", out value))
                    {
                        data["crying"] = Regex.Replace(value, "(pause 1000/speak Sam \\\")[^$]+.a\\\"", $"$1{SHelper.Translation.Get("confrontation-male")}$h\"/emote Shane 21 true/emote Sebastian 21 true/emote Sam 21 true/emote Harvey 21 true/emote Alex 21 true/emote Elliott 21").Replace("/dump guys 4", "");
                    }
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Strings/StringsFromCSFiles"))
            {
                e.Edit(delegate (IAssetData idata)
                {
                    IDictionary<string, string> data = idata.AsDictionary<string, string>().Data;
                    data["NPC.cs.3985"] = Regex.Replace(data["NPC.cs.3985"], @"\.\.\.\$s.+", $"$n#$b#$c 0.5#{data["ResourceCollectionQuest.cs.13681"]}#{data["ResourceCollectionQuest.cs.13683"]}");
                    Monitor.Log($"NPC.cs.3985 is set to \"{data["NPC.cs.3985"]}\"");
                });

            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Data/animationDescriptions"))
            {
                e.Edit(delegate (IAssetData idata)
                {
                    IDictionary<string, string> data = idata.AsDictionary<string, string>().Data;
                    List<string> sleepKeys = data.Keys.ToList().FindAll((s) => s.EndsWith("_Sleep"));
                    foreach (string key in sleepKeys)
                    {
                        if (!data.ContainsKey(key.ToLower()))
                        {
                            Monitor.Log($"adding {key.ToLower()} to animationDescriptions");
                            data.Add(key.ToLower(), data[key]);
                        }
                    }
                });

            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Data/EngagementDialogue"))
            {
                if (!Config.RomanceAllVillagers)
                    return;
                e.Edit(delegate (IAssetData idata)
                {
                    IDictionary<string, string> data = idata.AsDictionary<string, string>().Data;
                    Farmer f = Game1.player;
                    if (f == null)
                    {
                        return;
                    }
                    foreach (string friend in f.friendshipData.Keys)
                    {
                        if (!data.ContainsKey(friend + "0"))
                        {
                            data[friend + "0"] = "";
                        }
                        if (!data.ContainsKey(friend + "1"))
                        {
                            data[friend + "1"] = "";
                        }
                    }
                });
            }
            else if (Config.RomanceAllVillagers && (e.NameWithoutLocale.BaseName.StartsWith("Characters/schedules/") || e.NameWithoutLocale.BaseName.StartsWith("Characters\\schedules\\")))
            {
                try
                {
                    string name = e.NameWithoutLocale.BaseName.Replace("Characters/schedules/", "").Replace("Characters\\schedules\\", "");
                    NPC npc = Game1.getCharacterFromName(name);
                    if (npc != null && npc.Age < 2 && !(npc is Child))
                    {

                        if (Game1.characterData[npc.Name].CanBeRomanced)
                        {
                            Monitor.Log($"can edit schedule for {name}");
                            e.Edit(delegate (IAssetData idata)
                            {
                                IDictionary<string, string> data = idata.AsDictionary<string, string>().Data;
                                List<string> keys = new List<string>(data.Keys);
                                foreach (string key in keys)
                                {
                                    if (!data.ContainsKey($"marriage_{key}"))
                                        data[$"marriage_{key}"] = data[key];
                                }
                            });
                        }
                    }
                }
                catch
                {
                }


            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Strings/Locations"))
            {
                e.Edit(delegate (IAssetData idata)
                {
                    IDictionary<string, string> data = idata.AsDictionary<string, string>().Data;
                    data["Beach_Mariner_PlayerBuyItem_AnswerYes"] = data["Beach_Mariner_PlayerBuyItem_AnswerYes"].Replace("5000", Config.PendantPrice + "");
                });
            }
        }


        public static void AforeGiftGiven(object sender, EventArgsBeforeReceiveObject e)
        {
            /*
            if (sender != Game1.player)
                return;
            ModEntry.ResetSpouses(Game1.player);
            var roomie = Game1.player.isRoommate("");
            NPC c = e.Npc;
            Friendship friendship;
            Game1.player.friendshipData.TryGetValue(c.Name, out friendship);



            if (e.Gift.Name.Equals("Áine Flower"))
            {

                e.Cancel = true;

                if (Button == true)
                {
                   

                    //if (c.Equals(Game1.player.spouse) || c.Equals(roomie))
                    if (ModEntry.GetSpouses(Game1.player, true).ContainsKey(c.Name))
                    {

                        {
                            Game1.player.spouse = c.Name;
                            ModEntry.ResetSpouses(Game1.player);
                            Game1.currentLocation.playSound("dwop", null, null, SoundContext.NPC);

                            {
                                FarmHouse fh = Utility.getHomeOfFarmer(Game1.player);
                                fh.showSpouseRoom();
                                if (Game1.player.currentLocation == fh)
                                {
                                    SHelper.Reflection.GetMethod(fh, "resetLocalState").Invoke();
                                }
                                else
                                {
                                    Game1.addHUDMessage(new HUDMessage("The room and patio will change when you enter the farmhouse."));
                                }
                            }
                        }
                    }


                    else if (friendship.Points < 2000)
                    {
                        Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:AineFlower_reject", c.displayName));
                    }
                    else
                    {
                        Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:AineFlower_accept", c.displayName));
                        Game1.player.changeFriendship(5, c);
                    }


                    e.Cancel = true;


                }
            }*/

        }

        public static void AfterGiftGiven(object sender, EventArgsGiftGiven e)
        {
            if (sender != Game1.player)
                return;
            if (e.Gift.Name.Equals("Mermaid Bouquet"))
            {
                e.Npc.CurrentDialogue.Pop();

                Friendship friendship;
                Game1.player.friendshipData.TryGetValue(e.Npc.Name, out friendship);

                {
                    SMonitor.Log($"Try give mermaid bouquet to {e.Npc.Name}");

                    string accept = $"Characters\\Dialogue\\{e.Npc.Name}:AcceptBouquet";
                    string rejectDivorced = $"Characters\\Dialogue\\{e.Npc.Name}:RejectBouquet_Divorced";
                    string rejectNotDatable = $"Characters\\Dialogue\\{e.Npc.Name}:RejectBouquet_NotDatable";
                    string rejectNpcAlreadyMarried = $"Characters\\Dialogue\\{e.Npc.Name}:RejectBouquet_NpcAlreadyMarried";
                    string rejectVeryLowHearts = $"Characters\\Dialogue\\{e.Npc.Name}:RejectBouquet_VeryLowHearts";
                    string rejectLowHearts = $"Characters\\Dialogue\\{e.Npc.Name}:RejectBouquet_LowHearts";

                    if (!e.Npc.datable.Value)
                    {

                        if ((e.Npc.Dialogue.ContainsKey("RejectBouquet_NotDatable")))
                        {
                            e.Npc.setNewDialogue(rejectNotDatable, false, false);
                        }

                        else
                        {

                            if (Game1.random.NextBool())
                            {
                                Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3955", e.Npc.displayName));
                            }
                            else
                            {
                                e.Npc.CurrentDialogue.Push(Game1.random.NextBool() ? new Dialogue(e.Npc, "Strings\\StringsFromCSFiles:NPC.cs.3956", false) : new Dialogue(e.Npc, "Strings\\StringsFromCSFiles:NPC.cs.3957", true));
                            }
                        }

                        Game1.drawDialogue(e.Npc);

                    }
                    else
                    {
                        if (friendship?.IsDating() == true)
                        {
                            Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:AlreadyDatingBouquet", e.Npc.displayName));

                        }
                        if (friendship?.IsDivorced() == true)

                            if ((e.Npc.Dialogue.ContainsKey("RejectBouquet_Divorced")))
                            {
                                e.Npc.setNewDialogue(rejectDivorced, false, false);
                            }
                            else
                            {
                                {
                                    e.Npc.CurrentDialogue.Push(new Dialogue(e.Npc, "Strings\\Characters:Divorced_bouquet", true));
                                    Game1.drawDialogue(e.Npc);
                                }
                            }
                        if (friendship?.Points < Config.MinPointsToDate / 2f)
                        {
                            if ((e.Npc.Dialogue.ContainsKey("RejectBouquet_VeryLowHearts")))
                            {
                                e.Npc.setNewDialogue(rejectVeryLowHearts, false, false);
                            }
                            else
                            {
                                e.Npc.CurrentDialogue.Push(Game1.random.NextBool() ? new Dialogue(e.Npc, "Strings\\StringsFromCSFiles:NPC.cs.3958", false) : new Dialogue(e.Npc, "Strings\\StringsFromCSFiles:NPC.cs.3959", true));
                                Game1.drawDialogue(e.Npc);
                            }

                        }
                        if (friendship?.Points < Config.MinPointsToDate)
                        {
                            if ((e.Npc.Dialogue.ContainsKey("RejectBouquet_LowHearts")))
                            {
                                e.Npc.setNewDialogue(rejectLowHearts, false, false);
                            }
                            else
                            {
                                e.Npc.CurrentDialogue.Push(new Dialogue(e.Npc, "Strings\\StringsFromCSFiles:NPC.cs." + Game1.random.Choose("3960", "3961"), false));
                                Game1.drawDialogue(e.Npc);
                            }

                        }

                        //AcceptBouquet
                        if (friendship?.IsDating() == false)
                        {
                            friendship.Status = FriendshipStatus.Dating;
                            Multiplayer mp = ModEntry.SHelper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();
                            mp.globalChatInfoMessage("Dating", new string[]
                            {
                                    Game1.player.Name,
                                    e.Npc.displayName
                            });
                        }


                        if ((e.Npc.Dialogue.ContainsKey("AcceptBouquet")))
                        {
                            e.Npc.setNewDialogue(accept, false, false);
                        }
                        else
                        {
                            e.Npc.CurrentDialogue.Push(new Dialogue(e.Npc, "Strings\\StringsFromCSFiles:NPC.cs." + Game1.random.Choose("3962", "3963"), true));
                        }

                        Game1.player.changeFriendship(25, e.Npc);
                        Game1.player.reduceActiveItemByOne();
                        Game1.player.completelyStopAnimatingOrDoingAction();
                        e.Npc.doEmote(20, true);
                        Game1.drawDialogue(e.Npc);

                    }
                }
            }

            if (e.Gift.Name.Equals("Atagartis Pendant"))
            {
                e.Npc.CurrentDialogue.Pop();
                {
                    //RejectMermaidPendant_Divorced
                    //RejectMermaidPendant_NeedHouseUpgrade
                    //RejectMermaidPendant_NotDatable
                    //RejectMermaidPendant_NpcWithSomeoneElse
                    //RejectMermaidPendant_PlayerWithSomeoneElse
                    //RejectMermaidPendant_Under8Hearts
                    //RejectMermaidPendant_Under10Hearts
                    //RejectMermaidPendant_Under10Hearts_AskedAgain
                    //RejectMermaidPendant

                    string acceptpendant = $"Strings\\StringsFromCSFiles\\{e.Npc.Name}:{e.Npc.Name}_Engaged";
                    string rejectDivorced = $"Characters\\Dialogue\\{e.Npc.Name}:RejectMermaidPendant_Divorced";
                    string rejectNotDatable = $"Characters\\Dialogue\\{e.Npc.Name}:RejectMermaidPendant_NotDatable";
                    string rejectNpcAlreadyMarried = $"Characters\\Dialogue\\{e.Npc.Name}:RejectMermaidPendant_NpcWithSomeoneElse";
                    string rejectPlayerAlreadyMarried = $"Characters\\Dialogue\\{e.Npc.Name}:RejectMermaidPendant_PlayerWithSomeoneElse";
                    string rejectUnder8Hearts = $"Characters\\Dialogue\\{e.Npc.Name}:RejectMermaidPendant_Under8Hearts";
                    string rejectUnderTenHearts = $"Characters\\Dialogue\\{e.Npc.Name}:RejectMermaidPendant_Under10Hearts";
                    string rejectUnderTenHeartsAskedAgain = $"Characters\\Dialogue\\{e.Npc.Name}:RejectMermaidPendant_Under10Hearts_AskedAgain";


                    SMonitor.Log($"Try give pendant to {e.Npc.Name}");
                    if (Game1.player.isEngaged())
                    {
                        SMonitor.Log($"Tried to give pendant while engaged");

                        if ((e.Npc.Dialogue.ContainsKey("RejectMermaidPendant_PlayerWithSomeoneElse")))
                        {
                            e.Npc.setNewDialogue(rejectPlayerAlreadyMarried, false, false);

                        }

                        else
                        {
                            e.Npc.CurrentDialogue.Push(new Dialogue(e.Npc, "Strings\\StringsFromCSFiles:NPC.cs." + Game1.random.Choose("3965", "3966"), true));
                            Game1.drawDialogue(e.Npc);
                        }

                    }


                    if (!e.Npc.datable.Value /*|| __instance.isMarriedOrEngaged() */ )
                    {
                        SMonitor.Log($"Tried to give pendant to someone not datable");

                        if ((e.Npc.Dialogue.ContainsKey("RejectMermaidPendant_NotDatable")))
                        {
                            e.Npc.setNewDialogue(rejectNotDatable, false, false);
                        }
                        else
                        {

                            if (ModEntry.myRand.NextDouble() < 0.5)
                            {
                                Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3969", e.Npc.displayName));

                            }
                            e.Npc.CurrentDialogue.Push(new Dialogue(e.Npc, "Strings\\StringsFromCSFiles:NPC.cs." + ((e.Npc.Gender == Gender.Female) ? "3970" : "3971"), false));
                            Game1.drawDialogue(e.Npc);

                        }
                    }

                    else if (e.Npc.datable.Value && Game1.player.friendshipData.ContainsKey(e.Npc.Name) && Game1.player.friendshipData.ContainsKey(e.Npc.Name) && Game1.player.friendshipData[e.Npc.Name].Points < Config.MinPointsToMarry * 0.6f)
                    {
                        SMonitor.Log($"Tried to give pendant to someone with far fewer hearts than the configured amount for marriage.");

                        if (!Game1.player.friendshipData[e.Npc.Name].ProposalRejected)
                        {
                            if ((e.Npc.Dialogue.ContainsKey("RejectMermaidPendant_Under8Hearts")))
                            {
                                e.Npc.setNewDialogue(rejectUnderTenHearts, false, false);
                            }
                            else
                            {
                                e.Npc.CurrentDialogue.Push(new Dialogue(e.Npc, "Strings\\StringsFromCSFiles:NPC.cs." + Game1.random.Choose("3972", "3973"), false));
                            }
                            Game1.drawDialogue(e.Npc);
                            Game1.player.changeFriendship(-50, e.Npc);
                            Game1.player.friendshipData[e.Npc.Name].ProposalRejected = true;

                        }
                        if ((e.Npc.Dialogue.ContainsKey("RejectMermaidPendant_Under10Hearts_AskedAgain")))
                        {
                            e.Npc.setNewDialogue(rejectUnderTenHeartsAskedAgain, false, false);
                        }
                        else
                        {
                            e.Npc.CurrentDialogue.Push(new Dialogue(e.Npc, "Strings\\StringsFromCSFiles:NPC.cs." + Game1.random.Choose("3974", "3975"), true));
                        }
                        Game1.drawDialogue(e.Npc);
                        Game1.player.changeFriendship(-100, e.Npc);

                    }

                    else if (e.Npc.datable.Value && Game1.player.friendshipData.ContainsKey(e.Npc.Name) && Game1.player.friendshipData[e.Npc.Name].Points < Config.MinPointsToMarry)
                    {
                        SMonitor.Log($"Tried to give pendant to someone with fewer hearts than the config");

                        if (!Game1.player.friendshipData[e.Npc.Name].ProposalRejected)
                        {
                            if ((e.Npc.Dialogue.ContainsKey("RejectMermaidPendant_Under10Hearts")))
                            {
                                e.Npc.setNewDialogue(rejectUnderTenHearts, false, false);
                            }
                            else
                            {

                                e.Npc.CurrentDialogue.Push(new Dialogue(e.Npc, "Strings\\StringsFromCSFiles:NPC.cs." + Game1.random.Choose("3972", "3973"), false));
                            }
                            Game1.drawDialogue(e.Npc);
                            Game1.player.changeFriendship(-20, e.Npc);
                            Game1.player.friendshipData[e.Npc.Name].ProposalRejected = true;

                        }
                        e.Npc.CurrentDialogue.Push(new Dialogue(e.Npc, "Strings\\StringsFromCSFiles:NPC.cs." + Game1.random.Choose("3974", "3975"), true));
                        Game1.drawDialogue(e.Npc);
                        Game1.player.changeFriendship(-50, e.Npc);

                    }

                    //Proposal Success Code

                    else
                    {
                        SMonitor.Log($"Tried to give pendant to someone marriable");
                        if (!Game1.player.isEngaged() && Game1.player.HouseUpgradeLevel >= 1)
                        {
                            typeof(NPC).GetMethod("engagementResponse", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(e.Npc, new object[] { Game1.player, false });

                        }

                        else
                        {

                            SMonitor.Log($"Can't marry");
                            if (ModEntry.myRand.NextDouble() < 0.5)
                            {
                                Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3969", e.Npc.displayName));

                            }
                            e.Npc.CurrentDialogue.Push(new Dialogue(e.Npc, "Strings\\StringsFromCSFiles:NPC.cs.3972", false));
                            Game1.drawDialogue(e.Npc);
                        }
                    }
                }
            }

            if (e.Gift.Name.Equals("Roomie B Gone"))
            {
                if (e.Npc.isRoommate())
                {

                    //FriendshipStatus friendshipStatus = new FriendshipStatus();

                    //e.Npc.divorcedFromFarmer = true;

                    //Game1.player.friendshipData[e.Npc.Name].Status = FriendshipStatus.Divorced;
                    //FriendshipStatus
                    e.Npc.CurrentDialogue.Push(new Dialogue(e.Npc, "Strings\\StringsFromCSFiles:RoomieBGone", false));
                    Game1.drawDialogue(e.Npc);
                    ModEntry.spouseToDivorce = e.Npc.Name;
                    Game1.player.divorceTonight.Value = true;
                }
                else
                {
                    e.Npc.CurrentDialogue.Clear();
                    e.Npc.CurrentDialogue.Push(new Dialogue(e.Npc, "Strings\\StringsFromCSFiles:NotRoomie", false));
                    Game1.drawDialogue(e.Npc);

                }



            }



        }

        private void weddingDelete(string arg1, string[] arg2) //The purpose here is to fix the wedding infinity bug.
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Game not loaded.", LogLevel.Error);
                return;
            }

           foreach( KeyValuePair<string,Friendship> wiggle in Game1.player.friendshipData.Pairs)  
            {
                Friendship friendship;
                Game1.player.friendshipData.TryGetValue(wiggle.Key, out friendship);
                friendship.WeddingDate = null;
            }

            List<NPC> all_characters = Utility.getAllCharacters();
            foreach (NPC character in all_characters)
            {
                if (character == null) continue;
               if (!character.IsVillager) continue;

                Friendship friendship;
                Game1.player.friendshipData.TryGetValue(character.Name, out friendship);

                if(friendship == null) continue;

                if(friendship.Status == FriendshipStatus.Engaged)
                {
                    friendship.WeddingDate = null;
                    friendship.Status = FriendshipStatus.Friendly;
                }

                friendship.WeddingDate = null;
            }

            Monitor.Log("Removing all pending weddings for "+ Game1.player.displayName +".", LogLevel.Info);

        }

        private void proposalAttempt(string arg1, string[] arg2)
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Game not loaded.", LogLevel.Error);
                return;
            }

            {
                NPC monica = Game1.getCharacterFromName(arg2[0]);

                if (monica != null)
                {
                    //Button = true;
                    AttemptEngagement(monica, Game1.player);
                    //typeof(NPC).GetMethod("engagementResponse", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(monica, new object[] { Game1.player, false });
                }

                Monitor.Log($"{Game1.player.Name} attemping to propose to {monica}.", LogLevel.Info);
            }
        }

        private void proposalForce(string arg1, string[] arg2)
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Game not loaded.", LogLevel.Error);
                return;
            }
            {
                FarmHouse farmHouse = Utility.getHomeOfFarmer(Game1.player);
                NPC JaneGrey = Game1.getCharacterFromName(arg2[0]);
                TryForce(JaneGrey, Game1.player);
                Monitor.Log($"{Game1.player.Name} is now engaged to {JaneGrey}.", LogLevel.Info);
            }
        }

        public static void AttemptEngagement(NPC __instance, Farmer who) //(NPC __instance, ref Farmer who,
        {
            Friendship friendship;
            who.friendshipData.TryGetValue(__instance.Name, out friendship);

            string acceptpendant = $"Strings\\StringsFromCSFiles\\{__instance.Name}:{__instance.Name}_Engaged";
            string rejectDivorced = $"Characters\\Dialogue\\{__instance.Name}:RejectMermaidPendant_Divorced";
            string rejectNotDatable = $"Characters\\Dialogue\\{__instance.Name}:RejectMermaidPendant_NotDatable";
            string rejectNpcAlreadyMarried = $"Characters\\Dialogue\\{__instance.Name}:RejectMermaidPendant_NpcWithSomeoneElse";
            string rejectPlayerAlreadyMarried = $"Characters\\Dialogue\\{__instance.Name}:RejectMermaidPendant_PlayerWithSomeoneElse";
            string rejectUnder8Hearts = $"Characters\\Dialogue\\{__instance.Name}:RejectMermaidPendant_Under8Hearts";
            string rejectUnderTenHearts = $"Characters\\Dialogue\\{__instance.Name}:RejectMermaidPendant_Under10Hearts";
            string rejectUnderTenHeartsAskedAgain = $"Characters\\Dialogue\\{__instance.Name}:RejectMermaidPendant_Under10Hearts_AskedAgain";

            SMonitor.Log($"Try give pendant to {__instance.Name}");
            if (who.isEngaged())
            {
                SMonitor.Log($"Tried to give pendant while player already currently engaged");

                if ((__instance.Dialogue.ContainsKey("RejectMermaidPendant_PlayerWithSomeoneElse")))
                {
                    __instance.setNewDialogue(rejectPlayerAlreadyMarried, false, false);
                }

                else
                {
                    __instance.CurrentDialogue.Push(new Dialogue(__instance, "Strings\\StringsFromCSFiles:NPC.cs." + Game1.random.Choose("3965", "3966"), true));
                    Game1.drawDialogue(__instance);
                }
            }

            else if (!__instance.datable.Value)
            {
                SMonitor.Log($"Tried to give pendant to someone not datable");

                if ((__instance.Dialogue.ContainsKey("RejectMermaidPendant_NotDatable")))
                {
                    __instance.setNewDialogue(rejectNotDatable, false, false);
                }

                else
                {
                    if (ModEntry.myRand.NextDouble() < 0.5)
                    {
                        Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3969", __instance.displayName));

                    }
                }
                __instance.CurrentDialogue.Push(new Dialogue(__instance, "Strings\\StringsFromCSFiles:NPC.cs." + ((__instance.Gender == Gender.Female) ? "3970" : "3971"), false));
                Game1.drawDialogue(__instance);

            }
            else if (__instance.datable.Value && who.friendshipData.ContainsKey(__instance.Name) && who.friendshipData[__instance.Name].Points <  Config.MinPointsToMarry) //Math.Min(10, Config.MinPointsToMarry)
            {
                SMonitor.Log($"Tried to give pendant to someone with fewer hearts than the Config amount.");

                if (!who.friendshipData[__instance.Name].ProposalRejected)
                {
                    if ((__instance.Dialogue.ContainsKey("RejectMermaidPendant_Under10Hearts")))
                    {
                        __instance.setNewDialogue(rejectUnderTenHearts, false, false);
                    }
                    else
                    {
                        __instance.CurrentDialogue.Push(new Dialogue(__instance, "Strings\\StringsFromCSFiles:NPC.cs." + Game1.random.Choose("3972", "3973"), false));
                    }
                    Game1.drawDialogue(__instance);
                    who.changeFriendship(-50, __instance);
                    who.friendshipData[__instance.Name].ProposalRejected = true;

                }
                if ((__instance.Dialogue.ContainsKey("RejectMermaidPendant_Under10Hearts_AskedAgain")))
                {
                    __instance.setNewDialogue(rejectUnderTenHeartsAskedAgain, false, false);
                }
                else
                {
                    __instance.CurrentDialogue.Push(new Dialogue(__instance, "Strings\\StringsFromCSFiles:NPC.cs." + Game1.random.Choose("3974", "3975"), true));
                }
                Game1.drawDialogue(__instance);
                who.changeFriendship(-100, __instance);

            }
            else
            {

                //WORK HERE APRYLL. PUT MEGAN UP.
                SMonitor.Log($"Tried to give pendant to someone marriable");
                if (who.HouseUpgradeLevel >= 1)
                {


                    Game1.changeMusicTrack("silence");
                    who.spouse = __instance.Name;

                    {
                        Game1.Multiplayer.globalChatInfoMessage("Engaged", Game1.player.Name, __instance.GetTokenizedDisplayName());
                    }

                    //Friendship friendship = who.friendshipData[base.Name];
                    friendship.Status = FriendshipStatus.Engaged;

                    WorldDate worldDate = new WorldDate(Game1.Date);
                    worldDate.TotalDays += 3;
                    while (!Game1.canHaveWeddingOnDay(worldDate.DayOfMonth, worldDate.Season))
                    {
                        worldDate.TotalDays++;
                    }

                    friendship.WeddingDate = worldDate;

                    __instance.modData.Add("PolyamorySweetWeddingDate", friendship.WeddingDate.TotalDays.ToString()); //This adds a way for people to be able to get the wedding date in Content Patcher.

                    __instance.CurrentDialogue.Clear();


                    {
                        Dialogue dialogue2 = StardewValley.Dialogue.TryGetDialogue(__instance, "Data\\EngagementDialogue:" + __instance.Name + "0");
                        if (dialogue2 != null)
                        {
                            __instance.CurrentDialogue.Push(dialogue2);
                        }

                        dialogue2 = StardewValley.Dialogue.TryGetDialogue(__instance, "Strings\\StringsFromCSFiles:" + __instance.Name + "_Engaged");
                        if (dialogue2 != null)
                        {
                            __instance.CurrentDialogue.Push(dialogue2);
                        }
                        else
                        {
                            __instance.CurrentDialogue.Push(new Dialogue(__instance, "Strings\\StringsFromCSFiles:NPC.cs.3980"));
                        }
                    }

                    Dialogue obj = __instance.CurrentDialogue.Peek();
                    obj.onFinish = (Action)Delegate.Combine(obj.onFinish, (Action)delegate
                    {
                        Game1.changeMusicTrack("none", track_interruptable: true);
                        GameLocation.HandleMusicChange(null, Game1.player.currentLocation);
                    });
                    who.changeFriendship(1, __instance);
                    who.reduceActiveItemByOne();
                    who.completelyStopAnimatingOrDoingAction();
                    Game1.drawDialogue(__instance);

                    //typeof(NPC).GetMethod("engagementResponse", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { who, false });

                }
                SMonitor.Log($"Can't marry");
                if (ModEntry.myRand.NextDouble() < 0.5)
                {
                    Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3969", __instance.displayName));

                }
                __instance.CurrentDialogue.Push(new Dialogue(__instance, "Strings\\StringsFromCSFiles:NPC.cs.3972", false));
                Game1.drawDialogue(__instance);

            }

        }


        public static void TryForce(NPC __instance, Farmer who)
        {
            if (__instance.IsVillager)
            {
                if (who.HouseUpgradeLevel == 0)
                {
                    SMonitor.Log($"You must upgrade your house in order to do this sorts of thing!", LogLevel.Alert);
                }

                else if (who.HouseUpgradeLevel >= 1)
                {
                    Friendship friendship;
                    who.friendshipData.TryGetValue(__instance.Name, out friendship);



                    //Friendship friendship = who.friendshipData[base.Name];
                    if (friendship != null)
                    {

                        Game1.changeMusicTrack("silence");
                        who.spouse = __instance.Name;

                        {
                            Game1.Multiplayer.globalChatInfoMessage("Engaged", Game1.player.Name, __instance.GetTokenizedDisplayName());
                        }

                        friendship.Status = FriendshipStatus.Engaged;

                        WorldDate worldDate = new WorldDate(Game1.Date);
                        worldDate.TotalDays += 3;
                        while (!Game1.canHaveWeddingOnDay(worldDate.DayOfMonth, worldDate.Season))
                        {
                            worldDate.TotalDays++;
                        }

                        friendship.WeddingDate = worldDate;

                        __instance.CurrentDialogue.Clear();

                        {
                            Dialogue dialogue2 = StardewValley.Dialogue.TryGetDialogue(__instance, "Data\\EngagementDialogue:" + __instance.Name + "0");
                            if (dialogue2 != null)
                            {
                                __instance.CurrentDialogue.Push(dialogue2);
                            }

                            dialogue2 = StardewValley.Dialogue.TryGetDialogue(__instance, "Strings\\StringsFromCSFiles:" + __instance.Name + "_Engaged");
                            if (dialogue2 != null)
                            {
                                __instance.CurrentDialogue.Push(dialogue2);
                            }
                            else
                            {
                                __instance.CurrentDialogue.Push(new Dialogue(__instance, "Strings\\StringsFromCSFiles:NPC.cs.3980"));
                            }
                        }

                        Dialogue obj = __instance.CurrentDialogue.Peek();
                        obj.onFinish = (Action)Delegate.Combine(obj.onFinish, (Action)delegate
                        {
                            Game1.changeMusicTrack("none", track_interruptable: true);
                            GameLocation.HandleMusicChange(null, Game1.player.currentLocation);
                        });

                        who.changeFriendship(1, __instance);
                        //who.reduceActiveItemByOne();
                        who.completelyStopAnimatingOrDoingAction();
                        Game1.drawDialogue(__instance);
                    }
                    else
                    {
                        SMonitor.Log($"You must have met the person to start the relationship!", LogLevel.Alert);

                    }
                }
            }
            else
            {
                SMonitor.Log($"The entity you are to force is not marriagable. You cannot do this!!!", LogLevel.Alert);

            }

        }

        public static bool Child_checkAction_Prefix(Farmer who, GameLocation l, Child __instance, ref bool __result)
        {
            if (who.ActiveItem != null && who.ActiveItem.Name.Equals("Lilith Token"))
            {
                if (__instance.idOfParent.Value != who.UniqueMultiplayerID)
                {
                    // This is not the player's child
                    __result = false;
                    return false;
                }

                who.reduceActiveItemByOne();
                who.completelyStopAnimatingOrDoingAction();

                Game1.Multiplayer.broadcastSprites(Game1.currentLocation, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(536, 1945, 8, 8), new Vector2(156f, 388f), flipped: false, 0f, Color.White)
                {
                    interval = 50f,
                    totalNumberOfLoops = 99999,
                    animationLength = 7,
                    layerDepth = 0.038500004f,
                    scale = 4f
                });
                for (int i = 0; i < 20; i++)
                {
                    Game1.Multiplayer.broadcastSprites(Game1.currentLocation, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(372, 1956, 10, 10), new Vector2(2f, 6f) * 64f + new Vector2(Game1.random.Next(-32, 64), Game1.random.Next(16)), flipped: false, 0.002f, Color.LightGray)
                    {
                        alpha = 0.75f,
                        motion = new Vector2(1f, -0.5f),
                        acceleration = new Vector2(-0.002f, 0f),
                        interval = 99999f,
                        layerDepth = 0.0384f + (float)Game1.random.Next(100) / 10000f,
                        scale = 3f,
                        scaleChange = 0.01f,
                        rotationChange = (float)Game1.random.Next(-5, 6) * (float)Math.PI / 256f,
                        delayBeforeAnimationStart = i * 25
                    });
                }
                Game1.playSound("fireball");
                Game1.Multiplayer.broadcastSprites(Game1.currentLocation, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(388, 1894, 24, 22), 100f, 6, 9999, new Vector2(2f, 5f) * 64f, flicker: false, flipped: true, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
                {
                    motion = new Vector2(4f, -2f)
                });
                if (who.getChildrenCount() > 1)
                {
                    Game1.Multiplayer.broadcastSprites(Game1.currentLocation, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(388, 1894, 24, 22), 100f, 6, 9999, new Vector2(2f, 5f) * 64f, flicker: false, flipped: true, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
                    {
                        motion = new Vector2(4f, -1.5f),
                        delayBeforeAnimationStart = 50
                    });
                }

                string message = Game1.content.LoadString("Strings\\Locations:WitchHut_Goodbye", __instance.getName());
                Game1.showGlobalMessage(message);

                DelayedAction.functionAfterDelay(delegate { ModEntry.NukaKid(__instance, who); }, 60);

                List<NPC> allSpouses = ModEntry.GetSpouses(who, true).Values.ToList();
                foreach (NPC wifie in allSpouses)
                {
                    who.changeFriendship(-1337, wifie);
                }

                Game1.Multiplayer.globalChatInfoMessage("EvilShrine", who.Name);

                __result = true;
                return false;
            }

            return true;
        }

        public static void NukaKid(Child child, Farmer who)
        {
            FarmHouse farmhouse = Utility.getHomeOfFarmer(who);
            for (int i = farmhouse.characters.Count - 1; i >= 0; i--)
            {
                if (farmhouse.characters[i] == child)
                {
                    farmhouse.GetChildBed((int)child.Gender)?.mutex.ReleaseLock();
                    if (child.hat.Value != null)
                    {
                        Hat hat = child.hat.Value;
                        child.hat.Value = null;
                        who.team.returnedDonations.Add(hat);
                        who.team.newLostAndFoundItems.Value = true;
                    }
                    farmhouse.characters.RemoveAt(i);
                    Game1.stats.Increment("childrenTurnedToDoves");
                }
            }


        }


        public static Point getRandomOpenPointInFarmHouse(Random r, int buffer = 0, int tries = 60)
        {
            FarmHouse farmhouse = Utility.getHomeOfFarmer(Game1.player);

            for (int i = 0; i < tries; i++)
            {
                Map map = farmhouse.Map;
                Point result = new Point(r.Next(map.Layers[0].LayerWidth), r.Next(map.Layers[0].LayerHeight));
                Microsoft.Xna.Framework.Rectangle rect = new Microsoft.Xna.Framework.Rectangle(result.X - buffer, result.Y - buffer, 1 + buffer * 2, 1 + buffer * 2);
                bool flag = false;
                foreach (Point point in rect.GetPoints())
                {
                    int x = point.X;
                    int y = point.Y;
                    flag = farmhouse.getTileIndexAt(x, y, "Back") == -1 || !farmhouse.CanItemBePlacedHere(new Vector2(x, y)) || farmhouse.isTileOnWall(x, y);
                    if (farmhouse.getTileIndexAt(x, y, "Back") == 0 && farmhouse.getTileSheetIDAt(x, y, "Back") == "indoor")
                    {
                        flag = true;
                    }

                    if (flag)
                    {
                        break;
                    }
                }

                if (!flag)
                {
                    return result;
                }
            }

            return Point.Zero;
        }


        private void OnLoadStageChanged(object sender, LoadStageChangedEventArgs e)
        {
            if (e.NewStage == LoadStage.CreatedInitialLocations || e.NewStage == LoadStage.SaveAddedLocations)
            {
                Game1.locations.Add(new LantanaLagoon(Helper.ModContent));
                Game1.locations.Add(new LantanaTemple(Helper.ModContent));
            }

        }


       
     







    }
    }