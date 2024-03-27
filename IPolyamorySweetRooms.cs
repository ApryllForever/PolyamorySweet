using Microsoft.Xna.Framework;
using StardewValley;

namespace PolyamorySweetLove
{
    public interface ISweetRoomsAPI
    {
        public Point GetSpouseTileOffset(NPC spouse);
        public Point GetSpouseTile(NPC spouse);

        public Point GetSpouseRoomCornerTile(NPC spouse);

        public void ResetRooms(GameLocation location);
    }
}