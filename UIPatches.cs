using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using PolyamorySweetLove;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using static StardewValley.Menus.SocialPage;

namespace PolyamorySweetLove
{
    public static class UIPatches
    {
        private static IMonitor Monitor;
        private static IModHelper Helper;





        // call this method from your Entry class
        public static void Initialize(IMonitor monitor, ModConfig config, IModHelper helper)
        {
            Monitor = monitor;
            Helper = helper;
        }
        public static bool SocialPage_drawNPCSlot_prefix(SpriteBatch b,  int i, SocialPage __instance)
        {
            try
            {
                /*

                SocialEntry entry = __instance.GetSocialEntry(i);
                if (entry == null)
                {
                    return true;
                }
                if (__instance.isCharacterSlotClickable(i) && __instance.characterSlots[i].bounds.Contains(Game1.getMouseX(), Game1.getMouseY()))
                {
                    b.Draw(Game1.staminaRect, new Rectangle(__instance.xPositionOnScreen + IClickableMenu.borderWidth - 4, __instance.sprites[i].bounds.Y - 4, __instance.characterSlots[i].bounds.Width, __instance.characterSlots[i].bounds.Height - 12), Color.White * 0.25f);
                }
                __instance.sprites[i].draw(b);
                string name = entry.InternalName;
                Gender gender = entry.Gender;
                bool datable = entry.IsDatable;
                bool isDating = entry.IsDatingCurrentPlayer();
                bool isCurrentSpouse = entry.IsMarriedToCurrentPlayer();
                bool housemate = entry.IsRoommateForCurrentPlayer();
                float lineHeight = Game1.smallFont.MeasureString("W").Y;
                float russianOffsetY = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko) ? ((0f - lineHeight) / 2f) : 0f);
                b.DrawString(Game1.dialogueFont, entry.DisplayName, new Vector2((float)(__instance.xPositionOnScreen + IClickableMenu.borderWidth * 3 / 2 + 64 - 20 + 96) - Game1.dialogueFont.MeasureString(entry.DisplayName).X / 2f, (float)(__instance.sprites[i].bounds.Y + 48) + russianOffsetY - (float)(datable ? 24 : 20)), Game1.textColor);
                for (int hearts = 0; hearts < Math.Max(Utility.GetMaximumHeartsForCharacter(Game1.getCharacterFromName(name)), 10); hearts++)
                {
                    __instance.drawNPCSlotHeart(b, i, entry, hearts, isDating, isCurrentSpouse);
                }
                if (datable || housemate)
                {

                    string text = ((!Game1.content.ShouldUseGenderedCharacterTranslations()) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11635") : ((gender == Gender.Male) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11635").Split('/')[0] : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11635").Split('/').Last()));

                    //if (!isDating)

                    {


                           if (housemate)
                        {
                            text = Game1.content.LoadString("Strings\\StringsFromCSFiles:Housemate");
                        }
                        else if (isCurrentSpouse)
                        {
                            text = ((gender == Gender.Male) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11636") : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11637"));
                        }
                        else if (entry.IsMarriedToAnyone())
                        {
                            text = ((gender == Gender.Male) ? Game1.content.LoadString("Strings\\UI:SocialPage_MarriedToOtherPlayer_MaleNPC") : Game1.content.LoadString("Strings\\UI:SocialPage_MarriedToOtherPlayer_FemaleNPC"));
                        }
                        else if (!Game1.player.isMarriedOrRoommates() && isDating)
                        {
                            text = ((gender == Gender.Male) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11639") : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11640"));
                        }
                        else if (entry.IsDivorcedFromCurrentPlayer())
                        {
                            text = ((gender == Gender.Male) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11642") : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11643"));
                        }




                    }



                    int width = (IClickableMenu.borderWidth * 3 + 128 - 40 + 192) / 2;
                    text = Game1.parseText(text, Game1.smallFont, width);
                    Vector2 textSize = Game1.smallFont.MeasureString(text);
                    b.DrawString(Game1.smallFont, text, new Vector2((float)(__instance.xPositionOnScreen + 192 + 8) - textSize.X / 2f, (float)__instance.sprites[i].bounds.Bottom - (textSize.Y - lineHeight)), Game1.textColor);
               
                    
                    
                    
                    
                    
                    }
                if (!isCurrentSpouse && !entry.IsChild)
                {
                    Utility.drawWithShadow(b, Game1.mouseCursors2, new Vector2(__instance.xPositionOnScreen + 384 + 304, __instance.sprites[i].bounds.Y - 4), new Rectangle(166, 174, 14, 12), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 0.88f, 0, -1, 0.2f);
                    Texture2D mouseCursors = Game1.mouseCursors;
                    Vector2 position = new Vector2(__instance.xPositionOnScreen + 384 + 296, __instance.sprites[i].bounds.Y + 32 + 20);
                    Friendship friendship = entry.Friendship;
                    b.Draw(mouseCursors, position, new Rectangle(227 + ((friendship != null && friendship.GiftsThisWeek >= 2) ? 9 : 0), 425, 9, 9), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
                    Texture2D mouseCursors2 = Game1.mouseCursors;
                    Vector2 position2 = new Vector2(__instance.xPositionOnScreen + 384 + 336, __instance.sprites[i].bounds.Y + 32 + 20);
                    Friendship friendship2 = entry.Friendship;
                    b.Draw(mouseCursors2, position2, new Rectangle(227 + ((friendship2 != null && friendship2.GiftsThisWeek >= 1) ? 9 : 0), 425, 9, 9), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
                    Utility.drawWithShadow(b, Game1.mouseCursors2, new Vector2(__instance.xPositionOnScreen + 384 + 424, __instance.sprites[i].bounds.Y), new Rectangle(180, 175, 13, 11), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 0.88f, 0, -1, 0.2f);
                    Texture2D mouseCursors3 = Game1.mouseCursors;
                    Vector2 position3 = new Vector2(__instance.xPositionOnScreen + 384 + 432, __instance.sprites[i].bounds.Y + 32 + 20);
                    Friendship friendship3 = entry.Friendship;
                    b.Draw(mouseCursors3, position3, new Rectangle(227 + ((friendship3 != null && friendship3.TalkedToToday) ? 9 : 0), 425, 9, 9), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
                }
                if (isCurrentSpouse)
                {
                    if (!housemate || name == "Krobus")
                    {
                        b.Draw(Game1.objectSpriteSheet, new Vector2(__instance.xPositionOnScreen + IClickableMenu.borderWidth * 7 / 4 + 192, __instance.sprites[i].bounds.Y), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, housemate ? 808 : 460, 16, 16), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0.88f);
                    }
                }
                else if (isDating)
                {
                    b.Draw(Game1.objectSpriteSheet, new Vector2(__instance.xPositionOnScreen + IClickableMenu.borderWidth * 7 / 4 + 192, __instance.sprites[i].bounds.Y), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, housemate ? 808 : 458, 16, 16), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0.88f);
                }
                */




                SocialPage.SocialEntry aentry = __instance.GetSocialEntry(i);
                if (aentry.IsChild && aentry != null)
                {
                    if (aentry.DisplayName.EndsWith(")"))
                    {
                        AccessTools.FieldRefAccess<SocialPage.SocialEntry, string>(aentry, "DisplayName") = string.Join(" ", aentry.DisplayName.Split(' ').Reverse().Skip(1).Reverse());
                        __instance.SocialEntries[i] = aentry;
                    }
                }


                
                //List<ClickableTextureComponent> sprites = Helper.Reflection.GetField<List<ClickableTextureComponent>>(__instance, "sprites").GetValue();
               // bool charClick = (bool)Helper.Reflection.GetMethod(__instance, "isCharacterSlotClickable");

                int iq = 0;
                bool charClick = Helper.Reflection.GetMethod(__instance, "isCharacterSlotClickable").Invoke<bool>(iq);


                SocialEntry entry;
                entry = __instance.GetSocialEntry(i);
                if (entry == null)
                {
                    return true;
                }
                if (charClick && __instance.characterSlots[i].bounds.Contains(Game1.getMouseX(), Game1.getMouseY()))
                {
                    b.Draw(Game1.staminaRect, new Microsoft.Xna.Framework.Rectangle(__instance.xPositionOnScreen + IClickableMenu.borderWidth - 4, __instance.sprites[i].bounds.Y - 4, __instance.characterSlots[i].bounds.Width, __instance.characterSlots[i].bounds.Height - 12), Color.White * 0.25f);
                }
                __instance.sprites[i].draw(b);
                string name;
                name = entry.InternalName;
                Gender gender;
                gender = entry.Gender;
                bool datable;
                datable = entry.IsDatable;
                bool isDating;
                isDating = entry.IsDatingCurrentPlayer();
                bool isCurrentSpouse;
                isCurrentSpouse = entry.IsMarriedToCurrentPlayer();
                bool housemate;
                housemate = entry.IsRoommateForCurrentPlayer();
                float lineHeight;
                lineHeight = Game1.smallFont.MeasureString("W").Y;
                float russianOffsetY;
                russianOffsetY = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko) ? ((0f - lineHeight) / 2f) : 0f);
                b.DrawString(Game1.dialogueFont, entry.DisplayName, new Vector2((float)(__instance.xPositionOnScreen + IClickableMenu.borderWidth * 3 / 2 + 64 - 20 + 96) - Game1.dialogueFont.MeasureString(entry.DisplayName).X / 2f, (float)(__instance.sprites[i].bounds.Y + 48) + russianOffsetY - (float)(datable ? 24 : 20)), Game1.textColor);
                for (int hearts = 0; hearts < Math.Max(Utility.GetMaximumHeartsForCharacter(Game1.getCharacterFromName(name)), 10); hearts++)
                {
                    __instance.drawNPCSlotHeart(b, i, entry, hearts, isDating, isCurrentSpouse); // This MotherFucker is private. Fixed 1.6.9!!! Thx Pathos! I luv u!!!!

                    //typeof(SocialPage).GetMethod("drawNPCSlotHeart", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { b,i,entry,hearts,isDating,isCurrentSpouse });




                }
                if (datable || housemate)
                {
                   // if(!isDating)
                    {
                        string text;
                        text = string.Empty;
                        //text = ((!Game1.content.ShouldUseGenderedCharacterTranslations()) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11635") : ((gender == Gender.Male) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11635").Split('/')[0] : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11635").Split('/').Last()));
                        if (housemate)
                        {
                            text = ((gender == Gender.Female) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage_Relationship_Housemate_Female") : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage_Relationship_Housemate_Male"));  //("Strings\\StringsFromCSFiles:Housemate");
                        }
                        else if (isCurrentSpouse)
                        {
                            text = ((gender == Gender.Female) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage_Relationship_Wife") : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage_Relationship_Husband"));
                        }
                        else if (entry.IsMarriedToAnyone())
                        {
                            text = ((gender == Gender.Female) ? Game1.content.LoadString("Strings\\UI:SocialPage_Relationship_MarriedToOtherPlayer_FemaleNpc") : Game1.content.LoadString("Strings\\UI:SocialPage_Relationship_MarriedToOtherPlayer_MaleNpc"));//"sexymarriedass";// ((gender == Gender.Male) ? ("Strings\\UI:SocialPage_MarriedToOtherPlayer_MaleNPC") : ("Strings\\UI:SocialPage_MarriedToOtherPlayer_FemaleNPC"));
                        }
                        else if ( isDating) //!Game1.player.isMarriedOrRoommates() &&
                        {
                            text = ((gender == Gender.Female) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage_Relationship_Girlfriend") : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage_Relationship_Boyfriend")); //""; //((gender == Gender.Male) ? ("Strings\\StringsFromCSFiles:SocialPage.cs.11639") : ("Strings\\StringsFromCSFiles:SocialPage.cs.11640"));
                        }
                        else if (entry.IsDivorcedFromCurrentPlayer())
                        {
                            text = ((gender == Gender.Female) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage_Relationship_ExWife") : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage_Relationship_ExHusband")); //((gender == Gender.Male) ? ("Strings\\StringsFromCSFiles:SocialPage.cs.11642") : ("Strings\\StringsFromCSFiles:SocialPage.cs.11643"));
                        }
                        else
                        {
                            text = ((gender == Gender.Female) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage_Relationship_Single_Female") : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage_Relationship_Single_Male")); //((!Game1.content.ShouldUseGenderedCharacterTranslations()) ? ("Strings\\StringsFromCSFiles:SocialPage.cs.11635") : ((gender == Gender.Male) ? ("Strings\\StringsFromCSFiles:SocialPage.cs.11635").Split('/')[0] : ("Strings\\StringsFromCSFiles:SocialPage.cs.11635").Split('/').Last()));
                        }

                        int width;
                        width = (IClickableMenu.borderWidth * 3 + 128 - 40 + 192) / 2;
                        text = Game1.parseText(text, Game1.smallFont, width);
                        Vector2 textSize;
                        textSize = Game1.smallFont.MeasureString(text);
                        b.DrawString(Game1.smallFont, text, new Vector2((float)(__instance.xPositionOnScreen + 192 + 8) - textSize.X / 2f, (float)__instance.sprites[i].bounds.Bottom - (textSize.Y - lineHeight)), Game1.textColor);
                    }
                   // else
                    
                       // {
                         //   b.Draw(Game1.objectSpriteSheet, new Vector2(__instance.xPositionOnScreen + IClickableMenu.borderWidth * 7 / 4 + 192, sprites[i].bounds.Y), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, housemate ? 808 : 458, 16, 16), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0.88f);
                       // }

                    
                }
                if (!isCurrentSpouse && !entry.IsChild)
                {
                    Utility.drawWithShadow(b, Game1.mouseCursors2, new Vector2(__instance.xPositionOnScreen + 384 + 304, __instance.sprites[i].bounds.Y - 4), new Rectangle(166, 174, 14, 12), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 0.88f, 0, -1, 0.2f);
                    Texture2D mouseCursors;
                    mouseCursors = Game1.mouseCursors;
                    Vector2 position;
                    position = new Vector2(__instance.xPositionOnScreen + 384 + 296, __instance.sprites[i].bounds.Y + 32 + 20);
                    Friendship friendship;
                    friendship = entry.Friendship;
                    b.Draw(mouseCursors, position, new Rectangle(227 + ((friendship != null && friendship.GiftsThisWeek >= 2) ? 9 : 0), 425, 9, 9), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
                    Texture2D mouseCursors2;
                    mouseCursors2 = Game1.mouseCursors;
                    Vector2 position2;
                    position2 = new Vector2(__instance.xPositionOnScreen + 384 + 336, __instance.sprites[i].bounds.Y + 32 + 20);
                    Friendship friendship2;
                    friendship2 = entry.Friendship;
                    b.Draw(mouseCursors2, position2, new Rectangle(227 + ((friendship2 != null && friendship2.GiftsThisWeek >= 1) ? 9 : 0), 425, 9, 9), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
                    Utility.drawWithShadow(b, Game1.mouseCursors2, new Vector2(__instance.xPositionOnScreen + 384 + 424, __instance.sprites[i].bounds.Y), new Rectangle(180, 175, 13, 11), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 0.88f, 0, -1, 0.2f);
                    Texture2D mouseCursors3;
                    mouseCursors3 = Game1.mouseCursors;
                    Vector2 position3;
                    position3 = new Vector2(__instance.xPositionOnScreen + 384 + 432, __instance.sprites[i].bounds.Y + 32 + 20);
                    Friendship friendship3;
                    friendship3 = entry.Friendship;
                    b.Draw(mouseCursors3, position3, new Rectangle(227 + ((friendship3 != null && friendship3.TalkedToToday) ? 9 : 0), 425, 9, 9), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
                }
                if (isCurrentSpouse)
                {
                    if (!housemate || name == "Krobus")
                    {
                        b.Draw(Game1.objectSpriteSheet, new Vector2(__instance.xPositionOnScreen + IClickableMenu.borderWidth * 7 / 4 + 192, __instance.sprites[i].bounds.Y), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, housemate ? 808 : 460, 16, 16), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0.88f);
                    }
                }
                else if (isDating)
                {
                    b.Draw(Game1.objectSpriteSheet, new Vector2(__instance.xPositionOnScreen + IClickableMenu.borderWidth * 7 / 4 + 192, __instance.sprites[i].bounds.Y), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, housemate ? 808 : 458, 16, 16), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0.88f);
                }

                return false;



            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(SocialPage_drawNPCSlot_prefix)}:\n{ex}", LogLevel.Error);
                return true;
            }
        }
        public static bool SocialPage_isMarriedToAnyone_Prefix(SocialPage.SocialEntry __instance, ref bool __result)
        {
            try
            {
                foreach (Farmer farmer in Game1.getAllFarmers())
                {
                    if (farmer.spouse == __instance.InternalName && farmer.friendshipData.TryGetValue(__instance.InternalName, out Friendship friendship) && friendship.IsMarried())
                    {
                        __result = true;
                    }
                }
                __result = false;
                return false;
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(SocialPage_isMarriedToAnyone_Prefix)}:\n{ex}", LogLevel.Error);
            }
            return true;
        }
       
      
        public static IEnumerable<CodeInstruction> SocialPage_drawSlot_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = instructions.ToList();
            if (Helper.ModRegistry.IsLoaded("SG.Partners"))
            {
                Monitor.Log("Keep Your Partners mod is loaded, not patching social page.");
                return codes.AsEnumerable();
            }
            try
            {
                // MethodInfo m_IsMarried = AccessTools.Method(typeof(Farmer), "isMarried", null, null);


                // int index = codes.FindIndex((CodeInstruction c) => c.operand != null && c.opcode == OpCodes.Ldloc_S); //&& c.operand.Equals( 6));
                int index = 172;

                //if (index >= 100)
                {
                 //   codes.Insert(index, new CodeInstruction(OpCodes.And));
                   // codes.Insert(index +1, new CodeInstruction(OpCodes.Brfalse));
                  //  codes.Insert(index+2, new CodeInstruction(OpCodes.Ldloc_S, 4));  // The problem here is that I do not know how to make this !isDating, so we are goint to prefix this, and damn them all!!!
                    //codes.Insert(index+3, new CodeInstruction(OpCodes.Brtrue));
                }



        ///
      //// The Below is the old code
      //////
                //MethodInfo m_IsMarried = AccessTools.Method(typeof(Farmer), "isMarried", null, null);
               
                
                
               // int index = codes.FindIndex((CodeInstruction c) => c.operand != null && c.operand is MethodInfo && (MethodInfo)c.operand == m_IsMarried);
                //if (index > -1)
                //{
                  //  codes[index - 1].opcode = OpCodes.Nop;
                  //  codes[index].opcode = OpCodes.Nop;
                   // codes[index + 1].opcode = OpCodes.Nop;
               // }
                

            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(SocialPage_drawSlot_transpiler)}:\n{ex}", LogLevel.Error);
            }
            return codes.AsEnumerable();
        }
        
        

        public static void DialogueBox_Prefix(ref List<string> dialogues)
        {
            try
            {
                if (dialogues == null || dialogues.Count < 2)
                    return;

                if (dialogues[1] == Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1826"))
                {
                    List<string> newDialogues = new List<string>()
                    {
                        dialogues[0]
                    };



                    List<NPC> spouses = ModEntry.GetSpouses(Game1.player, true).Values.OrderBy(o => Game1.player.friendshipData[o.Name].Points).Reverse().Take(4).ToList();

                    List<int> which = new List<int> { 0, 1, 2, 3 };

                    ModEntry.ShuffleList(ref which);

                    List<int> myWhich = new List<int>(which).Take(spouses.Count).ToList();

                    for (int i = 0; i < spouses.Count; i++)
                    {
                        switch (which[i])
                        {
                            case 0:
                                newDialogues.Add(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1827", spouses[i].displayName));
                                break;
                            case 1:
                                newDialogues.Add(((spouses[i].Gender == 0) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1832") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1834")) + " " + ((spouses[i].Gender == 0) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1837", spouses[i].displayName[0]) : Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1838", spouses[i].displayName[0])));
                                break;
                            case 2:
                                newDialogues.Add(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1843", spouses[i].displayName));
                                break;
                            case 3:
                                newDialogues.Add(((spouses[i].Gender == 0) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1831") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1833")) + " " + ((spouses[i].Gender == 0) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1837", spouses[i].displayName[0]) : Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1838", spouses[i].displayName[i])));
                                break;
                        }
                    }
                    dialogues = new List<string>(newDialogues);
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(DialogueBox_Prefix)}:\n{ex}", LogLevel.Error);
            }
        }
    }
}