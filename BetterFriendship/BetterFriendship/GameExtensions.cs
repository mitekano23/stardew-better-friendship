﻿using System.Collections.Generic;
using System.Linq;
using StardewValley;
using StardewValley.Characters;

namespace BetterFriendship
{
    internal static class GameExtensions
    {
        public static bool IsTownsfolk(this NPC npc)
        {
            return npc is not Pet
                   && npc is not Horse
                   && npc is not Junimo
                   && npc is not JunimoHarvester
                   && npc is not TrashBear;
        }

        public static List<(Object, int)> GetTopGiftSuggestions(this NPC npc, ModConfig config)
        {
            return
            Game1.player.Items.Where(x => x is Object)
                .Select(x => (item: x as Object, taste: npc.getGiftTasteForThisItem(x)))
                .Where(x => config.GiftPreference switch
                {
                    "love" => x.taste is 0,
                    "like" => x.taste is 0 or 2,
                    "neutral" => x.taste is not 4 or 6,
                    _ => false
                })
                .TakeTopPrioritized(config)
                .ToList();
        }

        public static bool ShouldOverrideForSpouse(this Character character, ModConfig config) => config.SpousePromptsOverride && character.Name == Game1.player.spouse;

        private static IEnumerable<(Object, int)> TakeTopPrioritized(this IEnumerable<(Object item, int taste)> items,
            ModConfig config)
        {
            if (config.OnlyHighestQuality)
            {
                items = items.GroupBy(x => x.item.name)
                    .Select(x => x.OrderByDescending(y => y.item.Quality).First());
            }

            return items
                .OrderBy(x => x.taste)
                .ThenByDescending(x => x.item.Quality)
                .ThenBy(x => x.item.salePrice())
                .Take(config.GiftCycleCount);
        }
    }
}