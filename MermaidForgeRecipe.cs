using SpaceCore;
using StardewValley;
using StardewValley.Enchantments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolyamorySweetLove
{
    public class MermaidEnchantment : BaseWeaponEnchantment
    {
        public override bool IsSecondaryEnchantment()
        {
            return true;
        }

        public override bool IsForge()
        {
            return false;
        }

        public override int GetMaximumLevel()
        {
            return 1;
        }

        public override bool ShouldBeDisplayed()
        {
            return false;
        }
    }
}

/*
public class MermaidForgeRecipe : CustomForgeRecipe
{
    public class IngredientMatcher : CustomForgeRecipe.IngredientMatcher
    {
        public override bool HasEnoughFor(Item item)
        {
            return item is StardewValley.Object obj && obj.ParentSheetIndex == 77; // Mermaid Pendant
        }

        public override void Consume(ref Item item)
        {
            if (item is StardewValley.Object obj && obj.ParentSheetIndex == 77)
            {
                item = null; // Consume the Mermaid Pendant
            }
        }
    }
    static StardewValley.Object item = (StardewValley.Object)ItemRegistry.Create("MermaidsKiss");
    public override CustomForgeRecipe.IngredientMatcher BaseItem 
    {
        get;
    } 

    public override CustomForgeRecipe.IngredientMatcher IngredientItem { get; } = new IngredientMatcher();

    public override int CinderShardCost { get; } = 20;

    public override Item CreateResult(Item baseItem, Item ingredItem)
    {
      
        
        return baseItem; 
    }
}
/*
public class SwordUpgradeRecipe : CustomForgeRecipe
{
    public class SwordMatcher : IngredientMatcher
    {
        public override bool HasEnoughFor(Item item)
        {
            return item != null && item.Type == "Sword";
        }

        public override void Consume(ref Item item)
        {
            // Simulate "consuming" by setting item to null
            item = null;
        }
    }

    public class GemMatcher : IngredientMatcher
    {
        public override bool HasEnoughFor(Item item)
        {
            return item != null && item.ItemId == "910" ;
        }

        public override void Consume(ref Item item)
        {
          
                item = null;
        }
    }

    public override IngredientMatcher BaseItem => new SwordMatcher();
    public override IngredientMatcher IngredientItem => new GemMatcher();
    public override int CinderShardCost => 10;

    public override Item CreateResult(Item baseItem, Item ingredItem)
    {
        return new Item
        {
            Type = "UpgradedSword",
            Name = baseItem.Name + " + Gem",
            Power = baseItem.Power + 15
        };
    }
}

*/
