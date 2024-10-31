using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.ItemTypeDefinitions;

namespace NPCToolKit
{
    internal class ModEntry : Mod
    {
        public Dictionary<string, string> ItemList = new Dictionary<string, string>();
        public override void Entry(IModHelper helper)
        {
                
        }
        public void Extractor()
        {

            foreach (IItemDataDefinition itemType in ItemRegistry.ItemTypes)
            {
                foreach (string id in itemType.GetAllIds())
                {
                    Item item = ItemRegistry.Create(itemType.Identifier + id);
                    if (item.canBeGivenAsGift())
                    {
                        ItemList.Add(item.Name, id);
                    }
                }
            }
        }
    }
}
