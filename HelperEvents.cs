using HarmonyLib;
using Microsoft.Xna.Framework;
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