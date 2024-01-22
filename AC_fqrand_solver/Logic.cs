using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AC_fqrand_solver
{
    public enum ListPriority
    {
        Common = 0,
        Uncommon = 1,
        Rare = 2
    }

    public enum MarketTrend : int
    {
        Spike = 0,
        Random = 1,
        Falling = 2
    }

    public static class Logic
    {
        public static uint __qrand_idum { get; set; } = 0;
        private static float __qrand_itemp = 0.0f;

        /*
         * While fqrand has 2^32 possible seeds,
         * the resultant only has 2^23 possible values due to the nine bits used in the float conversion.
         * This means that there are only 8,388,608 unique "values" for fqrand.
         * It also means that each "value" has 512 (2^9) seeds resulting in it.
         * 
         * 
         * We could probably take a hash of the upper 23 bits and check if it resulted in a desired value.
         * If not, we could cast out all seeds with that and save a lot of execution time in logic-heavy
         * searches.
         * 
         * OPTIMIZATION #2 (For RNG-chain searches):
         * Generate all possible float values (using ints) between [0, 1) (2^23 values), and check the resultant
         * against a desired result. All floats which result in the correct value should be considered as a start seed.
         * All 512 seed variants must be checked.
         * 
         * for (long i = val << 9; i < (val + 1) << 9; i++)
         * {
         *      uint seed = (uint)val;
         * }
         */
        public static unsafe float fqrand()
        {
            // Update "seed"
            __qrand_idum = (__qrand_idum * 0x19660D) + 0x3C6EF35F;
            // OR the lower 23 bits with 1.0f's hexadecimal representation
            var temp = (__qrand_idum >> 9) | 0x3F800000;
            // Set itemp by reinterpreting temp as a float value
            __qrand_itemp = *(float*)&temp;
            // Subtract 1.0f from it since otherwise our range would be [1.0, 2.0)
            return __qrand_itemp - 1.0f;
        }

        private static unsafe float fqrand_seed(in uint idum, out uint newIdum)
        {
            newIdum = (idum * 0x19660D) + 0x3C6EF35F;
            var temp = (newIdum >> 9) | 0x3F800000;
            var temp_f = *(float*)&temp;
            return temp_f - 1.0f;
        }

        public static void SetSeed()
        {
            TextBox txtTimeInput = Application.OpenForms["Form1"].Controls["txtTimeInput"] as TextBox;
            if (uint.TryParse(txtTimeInput.Text, System.Globalization.NumberStyles.HexNumber, null, out var val))
                __qrand_idum = val;
        }

        public static ushort[][] SortListsByPriority(in ushort[][] itemLists, in ListPriority listAPrio, in ListPriority listBPrio, in ListPriority listCPrio)
        {
            var sortedLists = new ushort[itemLists.Length][];
            sortedLists[(int)listAPrio] = itemLists[0];
            sortedLists[(int)listBPrio] = itemLists[1];
            sortedLists[(int)listCPrio] = itemLists[2];
            return sortedLists;
        }

        public static ushort GetRandomItemFromList(in ushort[] list) => list[(int)(fqrand() * list.Length)];
        public static ushort GetRandomItemFromList__ThreadSafe(in ushort[] list, in uint idum, out uint newIdum) => list[(int)(fqrand_seed(idum, out newIdum) * list.Length)];
        public static bool IsItemDuplicate(in ushort[] list, ushort itemId) => list.Any(o => o == itemId);
        public static ushort[] GetItemListForABC(in ushort[] commonList, in ushort[] uncommonList, in ushort[] rareList, in int goodsPower)
        {
            var randomNum = (int)(fqrand() * 100);
            if (randomNum < goodsPower + 5)
            {
                return rareList;
            }
            else if (randomNum < goodsPower + 40)
            {
                return uncommonList;
            }
            else
            {
                return commonList;
            }
        }

        public static ushort[] GetItemListForABC__ThreadSafe(in ushort[] commonList, in ushort[] uncommonList, in ushort[] rareList, in int goodsPower, in uint idum, out uint newIdum)
        {
            var randomNum = (int)(fqrand_seed(idum, out newIdum) * 100);
            if (randomNum < goodsPower + 5)
            {
                return rareList;
            }
            else if (randomNum < goodsPower + 40)
            {
                return uncommonList;
            }
            else
            {
                return commonList;
            }
        }

        private static readonly byte[] cloth_season_cnt = { 32, 10, 11, 9, 9 };

        public static int GetClothSeason(in int month, in int day)
        {
            switch (month)
            {
                case 1:
                    return 4;
                case 2:
                    if (day < 25)
                        return 4;
                    return 1;
                case 3:
                case 4:
                    return 1;
                case 5:
                    if (day < 27)
                        return 1;
                    return 2;
                case 6:
                case 7:
                    return 2;
                case 8:
                    if (day < 27)
                        return 2;
                    return 3;
                case 9:
                case 10:
                    return 3;
                case 11:
                    if (day < 27)
                        return 3;
                    return 4;
                case 12:
                    return 4;
                default:
                    throw new ArgumentOutOfRangeException(nameof(month));
            }
        }

        public static int GetRandomClothingForNooks(out int selected, in ushort[] clothingList, in int month, in int day)
        {
            var baseCount = cloth_season_cnt[0];
            var season = GetClothSeason(month, day);
            var countForSeason = cloth_season_cnt[season];
            selected = (int)((baseCount + countForSeason) * fqrand());
            if (baseCount <= selected)
            {
                for (var i = 1; i < season; i++)
                {
                    selected += cloth_season_cnt[i];
                }
            }
            return baseCount + countForSeason;
        }

        // NOTE: Diaries use the Furniture list priority values.
        public enum ItemCategory
        { 
            Furniture,
            Stationery,
            Clothing,
            Carpets,
            Wallpaper,
            Diaries
        }

        public enum ShopType
        {
            Cranny,
            Nook_n_Go,
            Nookway,
            Nookingtons
        }

        public static ushort[] GetItemsForNooks(in int numItemsToGenerate, in ushort rareItem, in ItemCategory category, in ListPriority listAPrio, in ListPriority listBPrio, in ListPriority listCPrio, in int goodsPower, in int month, in int day)
        {
            var itemList = new ushort[numItemsToGenerate];
            var numItemsGenerated = 0;
            while (numItemsGenerated < numItemsToGenerate)
            {
                ushort[][] itemLists = null;
                switch(category)
                {
                    case ItemCategory.Furniture:
                        itemLists = ItemLists.FurnitureLists;
                        break;
                    case ItemCategory.Clothing:
                        itemLists = ItemLists.ClothingLists;
                        break;
                    case ItemCategory.Stationery:
                        itemLists = ItemLists.StationeryLists;
                        break;
                    case ItemCategory.Carpets:
                        itemLists = ItemLists.CarpetLists;
                        break;
                    case ItemCategory.Wallpaper:
                        itemLists = ItemLists.WallpaperLists;
                        break;
                    case ItemCategory.Diaries:
                        itemLists = ItemLists.DiaryLists;
                        break;
                }

                // Sort ABC lists by priority
                itemLists = SortListsByPriority(itemLists, listAPrio, listBPrio, listCPrio);
                ushort[] commonList = itemLists[0], uncommonList = itemLists[1], rareList = itemLists[2];

                // Check if we need to add the item.
                if (category == ItemCategory.Clothing)
                {
                    var selectedList = GetItemListForABC(commonList, uncommonList, rareList, goodsPower);
                    GetRandomClothingForNooks(out var selected, selectedList, month, day);
                    var item = selectedList[selected];
                    if (!IsItemDuplicate(itemList, item))
                        itemList[numItemsGenerated++] = item;
                }
                else
                {
                    ushort item = GetRandomItemFromList(GetItemListForABC(commonList, uncommonList, rareList, goodsPower));
                    if (rareItem != item && !IsItemDuplicate(itemList, item))
                        itemList[numItemsGenerated++] = item;
                }
            }

            return itemList;
        }

        public static ushort[] GetItemsForNooksFurnitureOnly(in int numItemsToGenerate, in ushort rareItem, in ushort[] commonList, in ushort[] uncommonList, in ushort[] rareList, in int goodsPower, in uint startIdum)
        {
            var itemList = new ushort[numItemsToGenerate];
            var numItemsGenerated = 0;
            var idum = startIdum;

            while (numItemsGenerated < numItemsToGenerate)
            {
                ushort item = GetRandomItemFromList__ThreadSafe(GetItemListForABC__ThreadSafe(commonList, uncommonList, rareList, goodsPower, idum, out idum), idum, out idum);
                if (rareItem != item && !IsItemDuplicate(itemList, item))
                    itemList[numItemsGenerated++] = item;
            }

            return itemList;
        }

        public static ushort GetRareItemForNooks(in ShopType type, in ListPriority listAPrio, in ListPriority listBPrio, in ListPriority listCPrio)
        {
            if (type == ShopType.Nookway || type == ShopType.Nookingtons)
            {
                var rareList = SortListsByPriority(ItemLists.FurnitureLists, listAPrio, listBPrio, listCPrio)[2];
                return rareList[(int)(fqrand() * rareList.Length)];
            }
            return 0;
        }

        public static ushort GetRareItemForNooks__ThreadSafe(in ShopType type, in ListPriority listAPrio, in ListPriority listBPrio, in ListPriority listCPrio, in uint idum, out uint newIdum)
        {
            if (type == ShopType.Nookway || type == ShopType.Nookingtons)
            {
                var rareList = SortListsByPriority(ItemLists.FurnitureLists, listAPrio, listBPrio, listCPrio)[2];
                return rareList[(int)(fqrand_seed(idum, out newIdum) * rareList.Length)];
            }
            newIdum = idum;
            return 0;
        }

        private static readonly ushort[] toolList =
        {
            0x2202, 0x2200, 0x2203, 0x2201
        };

        public static ushort[] SelectToolsForNooks(int numTools, ShopType shop, uint shopSalesSum = 0)
        {
            // NOTE: shopSalesSum is stubbed at the moment. It's only used in Nook's Cranny.
            float toolRange;
            if (shop == ShopType.Cranny)
            {
                if (shopSalesSum < 3000)
                {
                    toolRange = 1.0f;
                }
                else if (shopSalesSum < 8000)
                {
                    toolRange = 2.0f;
                }
                else if (shopSalesSum < 12000)
                {
                    toolRange = 3.0f;
                }
                else
                {
                    toolRange = 4.0f;
                }
            }
            else
            {
                toolRange = 4.0f;
            }

            // Clamp number of tools to generate to the total that can be generated.
            if (numTools > toolRange)
            {
                numTools = (int)toolRange;
            }

            ushort[] tools = new ushort[numTools + 1];
            int numToolsGenerated = 0;
            while (numToolsGenerated < numTools)
            {
                ushort selectedTool = toolList[(int)(toolRange * fqrand())];
                if (!IsItemDuplicate(tools, selectedTool))
                    tools[numToolsGenerated++] = selectedTool;
            }

            // Paint would go here but it doesn't appear to use fqrand so I'm not emulating it. It also relies on data from shop flags.

            // Umbrella selection is its own function but I'm just integrating it here.
            // It technically takes a "count" argument but it's only ever passed one so we're just gonna skip that logic.
            tools[numToolsGenerated] = (ushort)(0x2204 + (int)(32 * fqrand()));

            return tools.ToArray();
        }

        public static ushort[] SelectPlantsForNooks(int numSaplings, int numPlants, ShopType shop, in int month, in int day)
        {
            List<ushort> plants = new();

            var count = numSaplings;
            // Halloween Candy Code, not set here.
            if (month == 10 && day > 15 && day < 31)
            {
                count = 0;
                numPlants = numSaplings;
            }

            if (shop > ShopType.Nook_n_Go && count > 0)
            {
                count--;
                plants.Add(0x2901);
            }

            while (count > 0)
            {
                plants.Add(0x2900);
                count--;
            }

            bool[] selectTbl = new bool[9];
            while (numPlants > 0)
            {
                var selected = (int)(9 * fqrand());
                if (selectTbl[selected] == false)
                {
                    selectTbl[selected] = true;
                    plants.Add((ushort)(0x2902 + selected));
                    numPlants--;
                }
            }

            return plants.ToArray();
        }

        public static List<uint> BruteForceIdumNookwayNookingtons(in long startIdum, in long endIdum, ShopType shopType, ListPriority listAPrio, ListPriority listBPrio, ListPriority listCPrio, int goodsPower, in ushort spotlightItem, in ushort[] comparisonFurniture, IProgress<uint> progress)
        {
            List<uint> possibleIdums = new(10);
            long idumValue;
            int numFurniture = shopType == ShopType.Nookway ? 3 : 5;
            ushort[][] sortedLists = SortListsByPriority(ItemLists.FurnitureLists, listAPrio, listBPrio, listCPrio);
            ushort[] commonList = sortedLists[0], uncommonList = sortedLists[1], rareList = sortedLists[2];

            for (idumValue = startIdum; idumValue <= endIdum; idumValue++)
            {
                //__qrand_idum = (uint)idumValue;
                ushort rareItem = GetRareItemForNooks__ThreadSafe(shopType, listAPrio, listBPrio, listCPrio, (uint)idumValue, out var idum);
                if (rareItem == spotlightItem)
                {
                    ushort[] itemList = GetItemsForNooksFurnitureOnly(numFurniture, rareItem, commonList, uncommonList, rareList, goodsPower, idum);
                    var valid = true;
                    for (int i = 0; i < numFurniture; i++)
                    {
                        if (itemList[i] != comparisonFurniture[i])
                        {
                            valid = false;
                            break;
                        }
                    }
                    if (valid)
                    {
                        possibleIdums.Add((uint)idumValue);
                        progress?.Report((uint)idumValue);
                    }
                }
            }
            return possibleIdums;
        }

        public static async Task<List<uint>[]> ThreadedBruteForceNookwayNookingtons(ShopType type, ListPriority listAPrio, ListPriority listBPrio, ListPriority listCPrio, int goodsPower, ushort rareItem, ushort[] comparisonFurniture, IProgress<uint> progress)
        {
            List<uint> possibleIdums = new();
            List<Task<List<uint>>> tasks = new();
            var numThreads = Environment.ProcessorCount;
            if (numThreads > 3)
                numThreads -= 2; // Save two threads for computer usage.
            var step = uint.MaxValue / numThreads;
            for (var i = 0; i < numThreads; i++)
            {
                var startId = i * step;
                var endId = i == numThreads - 1 ? uint.MaxValue : (i + 1) * step;
                tasks.Add(Task.Factory.StartNew(() => BruteForceIdumNookwayNookingtons(startId, endId, type, listAPrio, listBPrio, listCPrio, goodsPower, rareItem, comparisonFurniture, progress)));
            }

            return await Task.WhenAll(tasks);
        }

        public static ushort[] GenerateNookwayStock(in ListPriority[] listAPrios, in ListPriority[] listBPrios, in ListPriority[] listCPrios, int goodsPower, in int month, in int day)
        {
            List<ushort> stock = new();
            // Rare Item
            ushort rareItem = GetRareItemForNooks(ShopType.Nookway, listAPrios[0], listBPrios[0], listCPrios[0]);
            stock.Add(rareItem);
            // Random Furniture
            stock.AddRange(GetItemsForNooks(3, rareItem, ItemCategory.Furniture, listAPrios[0], listBPrios[0], listCPrios[0], goodsPower, month, day));
            // Random Stationery
            stock.AddRange(GetItemsForNooks(2, 0, ItemCategory.Stationery, listAPrios[1], listBPrios[1], listCPrios[1], goodsPower, month, day));
            // Random Diaries
            stock.AddRange(GetItemsForNooks(1, 0, ItemCategory.Diaries, listAPrios[0], listBPrios[0], listCPrios[0], goodsPower, month, day));
            // Random Clothes
            stock.AddRange(GetItemsForNooks(3, 0, ItemCategory.Clothing, listAPrios[2], listBPrios[2], listCPrios[2], goodsPower, month, day));
            // Random Carpets
            stock.AddRange(GetItemsForNooks(2, 0, ItemCategory.Carpets, listAPrios[3], listBPrios[3], listCPrios[3], goodsPower, month, day));
            // Random Wallpapers
            stock.AddRange(GetItemsForNooks(2, 0, ItemCategory.Wallpaper, listAPrios[4], listBPrios[4], listCPrios[4], goodsPower, month, day));
            // Random Tools & Umbrella
            stock.AddRange(SelectToolsForNooks(2, ShopType.Nookway));
            // Random Plants & Flowers
            stock.AddRange(SelectPlantsForNooks(2, 4, ShopType.Nookway, month, day));

            return stock.ToArray();
        }

        // Turnip Methods Emulation

        private static readonly float[][] marketTrendPercentageTable =
        {
            new float[] { 0.5f, 0.3f, 0.2f },
            new float[] { 0.6f, 0.2f, 0.2f },
            new float[] { 0.6f, 0.3f, 0.1f }
        };

        public static MarketTrend DecideTradeMarketTrend(in MarketTrend currentTrend)
        {
            float[] nextTrendPercentages = marketTrendPercentageTable[(int)currentTrend];
            float rng = fqrand();
            int i;
            for (i = 0; i < 2; i++)
            {
                if (rng < nextTrendPercentages[i]) break;
                rng -= nextTrendPercentages[i];
            }
            return (MarketTrend)i;
        }

        // Trend Type B
        public static ushort[] DecideTurnipPricesRandomTrend(in ushort sundayPrice)
        {
            ushort[] prices = new ushort[7];
            prices[0] = sundayPrice;
            float increaseMultiplier = 1.05f;
            float decreaseChance = 0.1f;
            float maximumPriceForDecrease = sundayPrice * 0.8f;
            float currentPrice = sundayPrice;

            for (int i = 1; i < 7; i++)
            {
                float rng = fqrand();
                if (decreaseChance <= rng)
                {
                    currentPrice *= increaseMultiplier;
                    decreaseChance += 0.14f;
                    increaseMultiplier += 0.01f;
                }
                else
                {
                    currentPrice *= 0.7f;
                    if (maximumPriceForDecrease < currentPrice)
                    {
                        currentPrice = maximumPriceForDecrease;
                    }
                    decreaseChance *= 0.3f;
                    if (decreaseChance > 0.1f)
                    {
                        decreaseChance = 0.1f;
                    }
                }
                prices[i] = (ushort)currentPrice;
            }

            return prices;
        }

        // Trend Type A
        public static ushort[] DecideTurnipPricesSpikeTrend(in ushort sundayPrice)
        {
            ushort[] prices = DecideTurnipPricesRandomTrend(sundayPrice);
            ushort spikeAmount = (ushort)(sundayPrice * 8);
            prices[(int)(5 * fqrand()) + 1] = spikeAmount; // Monday - Friday for spike
            return prices;
        }

        // Trend Type C
        public static ushort[] DecideTurnipPriceFallingTrend(in ushort sundayPrice)
        {
            ushort[] prices = new ushort[7];
            prices[0] = sundayPrice;
            const float decreaseAdjust = 0.15f;
            const float defaultDecreaseMultiplier = 0.8f;
            float currentPrice = sundayPrice;

            for (var i = 1; i < 7; i++)
            {
                currentPrice *= defaultDecreaseMultiplier + (decreaseAdjust * fqrand());
                prices[i] = (ushort)currentPrice;
            }

            return prices;
        }

        public static ushort DecideTurnipPriceForSunday() => (ushort)(((fqrand() - 0.5f) * 0.6f + 1.0f) * 100.0f);

        public static ushort[] DecidePriceScheduleWithoutSunday(in MarketTrend currentTrend, in ushort sundayPrice, out MarketTrend newTrend)
        {
            newTrend = DecideTradeMarketTrend(currentTrend);
            return newTrend switch
            {
                MarketTrend.Spike => DecideTurnipPricesSpikeTrend(sundayPrice),
                MarketTrend.Random => DecideTurnipPricesRandomTrend(sundayPrice),
                MarketTrend.Falling => DecideTurnipPriceFallingTrend(sundayPrice),
                _ => throw new ArgumentOutOfRangeException(nameof(newTrend)),
            };
        }

        public static ushort[] DecidePriceSchedule(in MarketTrend currentTrend, out MarketTrend newTrend)
        {
            ushort sundayPrice = DecideTurnipPriceForSunday();
            return DecidePriceScheduleWithoutSunday(currentTrend, sundayPrice, out newTrend);
        }
    }
}
