/*
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Netcode;
using StardewValley;

namespace PolyamorySweetLove
{
    public static class NPC_WeddingDate
    {
        internal class Holder { public readonly int Value = new(); }

        internal static ConditionalWeakTable<NPC, Holder> values = new();

        public static void set_WeddingDate(this NPC npc, int newVal)
        {
            // We don't actually want a setter for this one, since it should be readonly
            // Net types are weird
            // Or do we? Serialization
        }

        public static int WeddingDate(this NPC npc)
        {
            var holder = values.GetOrCreateValue(npc);
            return holder.Value;
        }
    }
}
*/