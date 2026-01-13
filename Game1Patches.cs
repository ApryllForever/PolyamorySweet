using StardewModdingAPI;
using StardewValley;

namespace PolyamorySweetLove
{
    public static class Game1Patches
    {
        private static IMonitor Monitor;
        public static string lastGotCharacter = null;

        // call this method from your Entry class
        public static void Initialize(IMonitor monitor)
        {
            Monitor = monitor;
        }

        public static void getCharacterFromName_Prefix(string name)
        {
            if (EventPatches.startingLoadActors)
                lastGotCharacter = name;
        }

        public static void getAvailableWeddingEvent_Postfix(Event __result)
        {
            ModEntry.WeddingToday = __result;
        }

        // Work here Amy! :D You nned to make this work foreach farmer 12/31/25
        public static void queueWeddingsForToday()
        {
            Game1.weddingsToday.Clear();
            Game1.weddingToday = false;
            if (!Game1.canHaveWeddingOnDay(Game1.dayOfMonth, Game1.season))
            {
                return;
            }
            foreach (Farmer farmer in from farmer in Game1.getOnlineFarmers()
                                      orderby farmer.UniqueMultiplayerID
                                      select farmer)
            {
                if (farmer.spouse != null && farmer.isEngaged() && farmer.friendshipData[farmer.spouse].CountdownToWedding < 1)
                {
                    Game1.weddingsToday.Add(farmer.UniqueMultiplayerID);
                }
                if (!farmer.team.IsEngaged(farmer.UniqueMultiplayerID))
                {
                    continue;
                }
                long? spouse = farmer.team.GetSpouse(farmer.UniqueMultiplayerID);
                if (spouse.HasValue && !Game1.weddingsToday.Contains(spouse.Value))
                {
                    Farmer? spouse_farmer = Game1.GetPlayer(spouse.Value);
                    if (spouse_farmer != null && Game1.getOnlineFarmers().Contains(spouse_farmer) && Game1.getOnlineFarmers().Contains(farmer) &&  Game1.player.team.GetFriendship(farmer.UniqueMultiplayerID, spouse.Value).CountdownToWedding < 1)
                    {
                        Game1.weddingsToday.Add(farmer.UniqueMultiplayerID);
                    }
                }
            }
        }




    }
}