using System;
using System.Collections.Generic;

namespace SlotGame
{
    /// <summary>
    /// Thin wrapper around System.Random so the rest of the game never touches an RNG directly.
    /// Seeded from a GUID + tick count so every session is different, but an explicit seed
    /// can be passed for repeatable tests.
    /// </summary>
    public class RandomService
    {
        private readonly Random random;

        public RandomService(int? seed = null)
        {
            int actualSeed = seed ?? (Guid.NewGuid().GetHashCode() ^ Environment.TickCount);
            random = new Random(actualSeed);
        }

        /// <summary>Returns an int in [minInclusive, maxExclusive).</summary>
        public int Range(int minInclusive, int maxExclusive)
        {
            return random.Next(minInclusive, maxExclusive);
        }

        /// <summary>
        /// Weighted random pick. Each symbol's chance = its weight / total weight,
        /// so rare, high-paying symbols simply get a small weight.
        /// </summary>
        public SymbolData PickWeighted(IReadOnlyList<SymbolData> pool)
        {
            int totalWeight = 0;
            for (int i = 0; i < pool.Count; i++)
            {
                totalWeight += pool[i].weight;
            }

            // Roll once, then walk the list subtracting weights until the roll "lands" inside a symbol.
            int roll = random.Next(0, totalWeight);
            for (int i = 0; i < pool.Count; i++)
            {
                roll -= pool[i].weight;
                if (roll < 0)
                {
                    return pool[i];
                }
            }

            return pool[pool.Count - 1]; // Unreachable in practice, keeps the compiler happy.
        }
    }
}
