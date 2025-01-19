using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using xTile.Dimensions;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using xTile;
using StardewValley.Characters;
using StardewValley.Network;
using StardewValley.Objects;
using System.Linq;
using xTile.Tiles;
using StardewValley.Locations;
using Object = StardewValley.Object;
using StardewValley.GameData;
using StardewValley.Menus;
using StardewValley.Extensions;
using System.Security.Cryptography;

namespace PolyamorySweetLove
{
    [XmlType("Mods_ApryllForever_PolyamorySweetLove_LantanaTemple")]
    public class LantanaTemple : PolyamoryLocation
    {
		static IModHelper Helper;

		public static IMonitor Monitor;

     
		

		internal static void Setup(IModHelper Helper)
		{
			LantanaTemple.Helper = Helper;
			//Helper.Events.GameLoop.DayStarted += OnDayStarted;
		}

		public LantanaTemple() { }

		

		public LantanaTemple(IModContentHelper content)
        : base(content, "LantanaTemple", "LantanaTemple")
        {


		}

        protected override void resetLocalState()
        {
			
			base.resetLocalState();
			
		

        }


      



	
		static string NuclearShopDialogue = "Hey there love! what may I do for you today?";

		public override bool performAction(string action, Farmer who, Location tileLocation)
		{


			 if (action == "CallistaShrine")
			{
               

            }
				return base.performAction(action, who, tileLocation);
        }

     

        public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
        {
            switch (base.getTileIndexAt(tileLocation, "Buildings"))
            {
                
                case 958:
                case 1080:
                case 1081:
                    base.ShowMineCartMenu("Default","Custom_LantanaLagoon");
                    return true;
            }
                return base.checkAction(tileLocation, viewport, who);
        }

	

	}
}