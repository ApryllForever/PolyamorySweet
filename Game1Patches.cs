﻿using StardewModdingAPI;
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
    }
}