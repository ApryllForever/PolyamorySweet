using PolyamorySweetLove;
using SpaceCore;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Linq;
using System.Threading;

namespace PolyamorySweetLove
{
    public partial class ModEntry
    {
        public static IKissingAPI kissingAPI;
        public static IPolyamorySweetBedAPI polyamorySweetBedAPI;
        public static IChildrenTweaksAPI childrenAPI;
        public static ISweetRoomsAPI sweetRoomsAPI;
        public static IPlannedParenthoodAPI plannedParenthoodAPI;
        public static IContentPatcherAPI contentPatcherAPI;

        public static void LoadModApis()
        {
            kissingAPI = SHelper.ModRegistry.GetApi<IKissingAPI>("ApryllForever.PolyamorySweetKiss");
            polyamorySweetBedAPI = SHelper.ModRegistry.GetApi<IPolyamorySweetBedAPI>("ApryllForever.PolyamorySweetBed");
            childrenAPI = SHelper.ModRegistry.GetApi<IChildrenTweaksAPI>("aedenthorn.ChildrenTweaks");
            sweetRoomsAPI = SHelper.ModRegistry.GetApi<ISweetRoomsAPI>("ApryllForever.PolyamorySweetRooms");
            plannedParenthoodAPI = SHelper.ModRegistry.GetApi<IPlannedParenthoodAPI>("aedenthorn.PlannedParenthood");

            if (kissingAPI != null)
            {
                SMonitor.Log("Polyamory Sweet Kiss API loaded");
            }
            if (polyamorySweetBedAPI != null)
            {
                SMonitor.Log("Polyamory Sweet Bed API loaded");
            }
            if (childrenAPI != null)
            {
                SMonitor.Log("Polyamory Sweet Children API loaded");
            }
            if (sweetRoomsAPI != null)
            {
                SMonitor.Log("Polyamory Sweet Rooms API loaded");
            }
            if (plannedParenthoodAPI != null)
            {
                SMonitor.Log("Polyamory Sweet Parenthood API loaded");
            }
            contentPatcherAPI = SHelper.ModRegistry.GetApi<IContentPatcherAPI>("Pathoschild.ContentPatcher");
            if (contentPatcherAPI is not null)
            {
                contentPatcherAPI.RegisterToken(context.ModManifest, "PlayerSpouses", () =>
                {
                    Farmer player;

                    if (Context.IsWorldReady)
                        player = Game1.player;
                    else if (SaveGame.loaded?.player != null)
                        player = SaveGame.loaded.player;
                    else
                        return null;

                    var spouses = GetSpouses(player, true).Keys.ToList();
                    spouses.Sort(delegate (string a, string b) {
                        player.friendshipData.TryGetValue(a, out Friendship af);
                        player.friendshipData.TryGetValue(b, out Friendship bf);
                        if (af == null && bf == null)
                            return 0;
                        if (af == null)
                            return -1;
                        if (bf == null)
                            return 1;
                        if (af.WeddingDate == bf.WeddingDate)
                            return 0;
                        return af.WeddingDate > bf.WeddingDate ? -1 : 1;
                    });
                    return spouses.ToArray();
                });

                contentPatcherAPI.RegisterToken(context.ModManifest, "NPCName", new WeddingDateToken());


            }
        }
    }
}