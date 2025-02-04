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
    [XmlType("Mods_ApryllForever_PolyamorySweetLove_ClairabelleLagoon")]
    public class LantanaLagoon : PolyamoryLocation
    {
		static IModHelper Helper;

		public static IMonitor Monitor;

        private static Multiplayer multiplayer;


		//Mermaid Related Code Below
		

		internal static void Setup(IModHelper Helper)
		{
			LantanaLagoon.Helper = Helper;
			//Helper.Events.GameLoop.DayStarted += OnDayStarted;
		}

		public LantanaLagoon() { }

		

		public LantanaLagoon(IModContentHelper content)
        : base(content, "LantanaLagoon", "LantanaLagoon")
        {


		}

        protected override void resetLocalState()
        {
			
			base.resetLocalState();
			
			int numSeagulls = Game1.random.Next(6);
            foreach (Vector2 tile in Utility.getPositionsInClusterAroundThisTile(new Vector2(Game1.random.Next(base.map.DisplayWidth / 64), Game1.random.Next(12, base.map.DisplayHeight / 64)), numSeagulls))
            {
                if (!base.isTileOnMap(tile) || (!this.CanItemBePlacedHere(tile) && !base.isWaterTile((int)tile.X, (int)tile.Y)))
                {
                    continue;
                }
                int state;
                state = 3;
                if (base.isWaterTile((int)tile.X, (int)tile.Y) && this.doesTileHaveProperty((int)tile.X, (int)tile.Y, "Passable", "Buildings") == null)
                {
                    state = 2;
                    if (Game1.random.NextDouble() < 0.7)
                    {
                        continue;
                    }
                }
                base.critters.Add(new Seagull(tile * 64f + new Vector2(32f, 32f), state));
            }




            var v1 = new Vector2(53);
            var v2 = new Vector2(22);
            var v3 = new Vector2(34);
            var v4 = new Vector2(67);
            var v5 = new Vector2(76);
            var v6 = new Vector2(51);


            addCritter(new Crow(67, 39));
            addCritter(new Crow(52, 41));
            addCritter(new Crow(46, 55));
            addCritter(new Crow(63, 55));
            addCritter(new Crow(42, 61));
            addCritter(new Crow(42, 43));

            addCritter(new Crow(6, 13));
            addCritter(new Crow(11, 7));



            addCritter(new CrabCritter(new Vector2(9f, 3f) * 64f));
            addCritter(new CrabCritter(new Vector2(13f, 12f) * 64f));
            addCritter(new CrabCritter(new Vector2(5f, 15f) * 64f));
            addCritter(new CrabCritter(new Vector2(9f, 18f) * 64f));

			addJumperFrog(new Vector2(42, 16));
            addJumperFrog(new Vector2(42, 32));
            addJumperFrog(new Vector2(45, 1));
            addJumperFrog(new Vector2(27, 55));

			{
				Point offset = new Point(0, 0); 
				Vector2 vector_offset = new Vector2(offset.X, offset.Y);

				Game1.currentLightSources.Add(new LightSource("4", 4, (new Vector2(19f, 7f) + vector_offset) * 64f, 1f, LightSource.LightContext.None, 0L));
            
            
            }  

        }


        public override void DayUpdate(int dayOfMonth)
        {
            base.DayUpdate(dayOfMonth);
            Microsoft.Xna.Framework.Rectangle tidePools;
            tidePools = new Microsoft.Xna.Framework.Rectangle(17, 6, 7, 19);
            float chance;
            
            /*chance = 1f;
            while (Game1.random.NextDouble() < (double)chance)
            {
                string id;
                id = ((Game1.random.NextDouble() < 0.2) ? "(O)372" : "(O)394");
                Vector2 position;
                position = new Vector2(Game1.random.Next(tidePools.X, tidePools.Right), Game1.random.Next(tidePools.Y, tidePools.Bottom));
                if (this.CanItemBePlacedHere(position))
                {
                    this.dropObject(ItemRegistry.Create<Object>(id), position * 64f, Game1.viewport, initialPlacement: true);
                }
                chance /= 2f;
            }*/
            Microsoft.Xna.Framework.Rectangle seaweedShore;
            seaweedShore = new Microsoft.Xna.Framework.Rectangle(12, 14, 12, 8);
            chance = 0.25f;
            while (Game1.random.NextDouble() < (double)chance)
            {
                if (Game1.random.NextDouble() < 0.15)
                {
                    Vector2 position2;
                    position2 = new Vector2(Game1.random.Next(seaweedShore.X, seaweedShore.Right), Game1.random.Next(seaweedShore.Y, seaweedShore.Bottom));
                    if (this.CanItemBePlacedHere(position2))
                    {
                        this.dropObject(ItemRegistry.Create<Object>("(O)152"), position2 * 64f, Game1.viewport, initialPlacement: true);
                    }
                }
                chance /= 2f;
            }
            for (int i = 0; i < 11; i++)
            {
                this.spawnObjects();
            }
           // string modId = "Butt";
            //if (Helper.ModRegistry.Get(modId) != null)
           // { }
        
        }



	
		static string NuclearShopDialogue = "Hey there love! what may I do for you today?";

		public override bool performAction(string action, Farmer who, Location tileLocation)
		{


			 if (action == "MermaidStore")
			{
                Utility.TryOpenShopMenu("PolyamorySweet.LantanaLoveShop", "MermaidLantana");

            }
				return base.performAction(action, who, tileLocation);
        }

        
        public override bool SeedsIgnoreSeasonsHere()
        {
            return true;
        }

        public override bool CanPlantSeedsHere(string itemId, int tileX, int tileY, bool isGardenPot, out string deniedMessage)
        {
			deniedMessage = string.Empty;
            return true;
        }

        public override bool CanPlantTreesHere(string itemId, int tileX, int tileY, out string deniedMessage)
        {
			deniedMessage = string.Empty;
            return true;
        }

        public override void tryToAddCritters(bool onlyIfOnScreen = false)
        {
            if (Game1.random.NextDouble() < 0.3)
            {
                Vector2 origin2 = Vector2.Zero;
                origin2 = ((Game1.random.NextDouble() < 0.75) ? new Vector2((float)Game1.viewport.X + Utility.RandomFloat(0f, Game1.viewport.Width), Game1.viewport.Y - 64) : new Vector2(Game1.viewport.X + Game1.viewport.Width + 64, Utility.RandomFloat(0f, Game1.viewport.Height)));
                int parrots_to_spawn = 1;
                if (Game1.random.NextDouble() < 0.5)
                {
                    parrots_to_spawn++;
                }
                if (Game1.random.NextDouble() < 0.5)
                {
                    parrots_to_spawn++;
                }
                for (int i = 0; i < parrots_to_spawn; i++)
                {
                    addCritter(new OverheadParrot(origin2 + new Vector2(i * 64, -i * 64)));
                }
            }
            if (!Game1.IsRainingHere(this))
            {
                double mapArea = map.Layers[0].LayerWidth * map.Layers[0].LayerHeight;
                double butterflyChance = Math.Max(0.4, Math.Min(0.25, mapArea / 15000.0));
                addButterflies(butterflyChance, onlyIfOnScreen);
            }
            if (Game1.IsRainingHere(this))
            {
                double mapArea = map.Layers[0].LayerWidth * map.Layers[0].LayerHeight;
                double butterflyChance = Math.Max(0.5, Math.Min(0.25, mapArea / 1500.0));
                addButterflies(butterflyChance, onlyIfOnScreen);
            }
			if (Game1.IsRainingHere(this))
			{
				double mapArea = map.Layers[0].LayerWidth * map.Layers[0].LayerHeight;
				//double frogChance = Math.Max(0.5, Math.Min(0.25, mapArea / 1500.0));
				addFrog();
			}
			if (!Game1.IsRainingHere(this))
			{
				double mapArea = map.Layers[0].LayerWidth * map.Layers[0].LayerHeight;
				double butterflyChance = Math.Max(0.7, Math.Min(0.25, mapArea / 15000.0));
				addBirdies(butterflyChance, onlyIfOnScreen);
			}
			

			double groovery = .5;


            double chance3 = groovery / 2.0;
            double chance4 = groovery / 2.0;
            double chance5 = groovery ;
            //double num3 = num2 * 2.0;

            if (critters.Count <= 200)
            {
                
                
                addBunnies(chance3, onlyIfOnScreen);
                addSquirrels(chance4, onlyIfOnScreen);
                addWoodpecker(chance5, onlyIfOnScreen = false);
               
            }

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

		public override void checkForMusic(GameTime time)
		{
			if (base.IsOutdoors && Game1.isMusicContextActiveButNotPlaying() && !Game1.IsRainingHere(this) && !Game1.eventUp)
			{
				if (Game1.random.NextDouble() < 0.003 && Game1.timeOfDay < 2100)
				{
					localSound("seagulls");
				}
				else if (Game1.isDarkOut(this) && Game1.timeOfDay < 2500)
				{
					Game1.changeMusicTrack("spring_night_ambient", track_interruptable: true);
				}
			}

			base.checkForMusic(time);
		}
		

	}
}