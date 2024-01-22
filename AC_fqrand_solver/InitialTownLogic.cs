using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Cloo;
using Cloo.Extensions;
using ILGPU;

namespace AC_fqrand_solver
{
    public sealed class InitialResult
    {
        public uint InitialSeed;
        public uint TrainSeed;
        public MarketTrend StalkMarketTrend;
        public int SundayPrice;
        public int SpikeDay;
    }

    internal class InitialTownLogic
    {
        /* Used for ComboBoxes */
        public static readonly SortedDictionary<byte, string> StartingVillagerDict = new()
        {
            { 0x00, "Bob" },
            { 0x01, "Olivia" },
            { 0x02, "Mitzi" },
            { 0x03, "Kiki" },
            { 0x04, "Tangy" },
            { 0x06, "Tabby" },
            { 0x07, "Monique" },
            { 0x09, "Purrl" },
            { 0x0A, "Kitty" },
            { 0x0B, "Tom" },
            { 0x0C, "Rosie" },
            { 0x12, "Teddy" },
            { 0x13, "Chow" },
            { 0x14, "Dozer" },
            { 0x15, "Nate" },
            { 0x16, "Groucho" },
            { 0x17, "Tutu" },
            { 0x18, "Ursala" },
            { 0x19, "Grizzly" },
            { 0x1A, "Pinky" },
            { 0x1B, "Jay" },
            { 0x1C, "Twiggy" },
            { 0x1D, "Anchovy" },
            { 0x1E, "Piper" },
            { 0x1F, "Admiral" },
            { 0x20, "Otis" },
            { 0x21, "Robin" },
            { 0x22, "Midge" },
            { 0x23, "Ace" },
            { 0x24, "Twirp" },
            { 0x25, "Chuck" },
            { 0x26, "Stu" },
            { 0x27, "Goose" },
            { 0x28, "Betty" },
            { 0x29, "Hector" },
            { 0x2A, "Egbert" },
            { 0x2B, "Ava" },
            { 0x2C, "Hank" },
            { 0x2D, "Leigh" },
            { 0x2E, "Rhoda" },
            { 0x2F, "Vladimir" },
            { 0x30, "Murphy" },
            { 0x31, "Cupcake" },
            { 0x32, "Kody" },
            { 0x33, "Maple" },
            { 0x34, "Pudge" },
            { 0x35, "Olive" },
            { 0x36, "Poncho" },
            { 0x37, "Bluebear" },
            { 0x38, "Patty" },
            { 0x39, "Petunia" },
            { 0x3A, "Bessie" },
            { 0x3B, "Belle" },
            { 0x3C, "Alfonso" },
            { 0x3D, "Boots" },
            { 0x3E, "Liz" },
            { 0x3F, "Biskit" },
            { 0x40, "Goldie" },
            { 0x41, "Daisy" },
            { 0x43, "Portia" },
            { 0x44, "Maddie" },
            { 0x45, "Butch" },
            { 0x46, "Bill" },
            { 0x47, "Pompom" },
            { 0x48, "Joey" },
            { 0x49, "Scoot" },
            { 0x4A, "Derwin" },
            { 0x4B, "Freckles" },
            { 0x4C, "Paolo" },
            { 0x4D, "Dizzy" },
            { 0x4E, "Axel" },
            { 0x4F, "Emerald" },
            { 0x50, "Tad" },
            { 0x51, "Wart Jr." },
            { 0x52, "Cousteau" },
            { 0x53, "Puddles" },
            { 0x54, "Lily" },
            { 0x55, "Jeremiah" },
            { 0x56, "Huck" },
            { 0x57, "Camofrog" },
            { 0x59, "Prince" },
            { 0x5A, "Jambette" },
            { 0x5B, "Billy" },
            { 0x5C, "Chevre" },
            { 0x5D, "Iggy" },
            { 0x5E, "Gruff" },
            { 0x5F, "Sven" },
            { 0x61, "Jane" },
            { 0x62, "Cesar" },
            { 0x63, "Louie" },
            { 0x64, "Peewee" },
            { 0x65, "Rollo" },
            { 0x66, "Bubbles" },
            { 0x67, "Bertha" },
            { 0x68, "Elmer" },
            { 0x69, "Winnie" },
            { 0x6A, "Savannah" },
            { 0x6B, "Ed" },
            { 0x6C, "Cleo" },
            { 0x6D, "Peaches" },
            { 0x6E, "Buck" },
            { 0x75, "Sydney" },
            { 0x76, "Gonzo" },
            { 0x77, "Ozzie" },
            { 0x78, "Yuka" },
            { 0x79, "Huggy" },
            { 0x7A, "Rex" },
            { 0x7B, "Aziz" },
            { 0x7C, "Leopold" },
            { 0x7D, "Samson" },
            { 0x7E, "Penny" },
            { 0x7F, "Dora" },
            { 0x80, "Chico" },
            { 0x81, "Candi" },
            { 0x83, "Anicotti" },
            { 0x84, "Limberg" },
            { 0x85, "Carmen" },
            { 0x8B, "Apollo" },
            { 0x8C, "Buzz" },
            { 0x8D, "Quetzal" },
            { 0x8E, "Amelia" },
            { 0x8F, "Pierce" },
            { 0x90, "Roald" },
            { 0x91, "Aurora" },
            { 0x92, "Hopper" },
            { 0x93, "Cube" },
            { 0x94, "Puck" },
            { 0x95, "Gwen" },
            { 0x96, "Friga" },
            { 0x97, "Curly" },
            { 0x98, "Truffles" },
            { 0x99, "Spork" },
            { 0x9A, "Hugh" },
            { 0x9B, "Rasher" },
            { 0x9C, "Sue E" },
            { 0x9D, "Hambo" },
            { 0x9E, "Lucy" },
            { 0x9F, "Cobb" },
            { 0xA0, "Boris" },
            { 0xA1, "Bunnie" },
            { 0xA2, "Doc" },
            { 0xA3, "Gaston" },
            { 0xA5, "Gabi" },
            { 0xA6, "Dotty" },
            { 0xA9, "Claude" },
            { 0xAA, "Tank" },
            { 0xAB, "Spike" },
            { 0xAD, "Vesta" },
            { 0xAE, "Filbert" },
            { 0xAF, "Hazel" },
            { 0xB0, "Peanut" },
            { 0xB1, "Pecan" },
            { 0xB2, "Ricky" },
            { 0xB3, "Static" },
            { 0xB4, "Mint" },
            { 0xB5, "Nibbles" },
            { 0xB6, "Tybalt" },
            { 0xB7, "Rolf" },
            { 0xB8, "Bangle" },
            { 0xB9, "Lobo" },
            { 0xBA, "Freya" },
            { 0xBB, "Chief" },
            { 0xBC, "Weber" },
            { 0xBD, "Mallary" },
            { 0xBE, "Wolfgang" },
            { 0xBF, "Hornsby" },
            { 0xC0, "Oxford" },
            { 0xC1, "T-Bone" },
            { 0xC2, "Biff" },
            { 0xC3, "Opal" },
            { 0xC4, "Bones" },
            { 0xC5, "Bea" },
            { 0xC6, "Bitty" },
            { 0xC7, "Rocco" },
            { 0xC8, "Lulu" },
            { 0xC9, "Blaire" },
            { 0xCA, "Sally" },
            { 0xCB, "Ellie" },
            { 0xCC, "Eloise" },
            { 0xCD, "Alli" },
            { 0xCE, "Pippy" },
            { 0xCF, "Eunice" },
            { 0xD0, "Baabara" },
            { 0xD1, "Fang" },
            { 0xD2, "Deena" },
            { 0xD3, "Pate" },
            { 0xD4, "Stella" },
            { 0xD5, "Cashmere" },
            { 0xD7, "Cookie" },
            { 0xE8, "Cheri" },
            { 0xEB, "Punchy" }
        };

        public static readonly SortedDictionary<ushort, string> NookItems = new()
        {
            { 0x1008, "blue wardrobe" },
            { 0x102C, "ranch wardrobe" },
            { 0x1030, "blue cabinet" },
            { 0x1044, "cabana dresser" },
            { 0x1058, "pear dresser" },
            { 0x1068, "modern dresser" },
            { 0x106C, "exotic bureau" },
            { 0x10A8, "lily-pad table" },
            { 0x10AC, "refrigerator" },
            { 0x10BC, "red armchair" },
            { 0x10D8, "folk guitar" },
            { 0x10E8, "Papa bear" },
            { 0x10FC, "classic desk" },
            { 0x1100, "classic table" },
            { 0x1108, "rocking chair" },
            { 0x1114, "Keiko figurine" },
            { 0x1128, "Naomi figurine" },
            { 0x112C, "globe" },
            { 0x1134, "regal table" },
            { 0x113C, "eagle pole" },
            { 0x1150, "space heater" },
            { 0x1164, "lovely armchair" },
            { 0x1178, "lovely bed" },
            { 0x1184, "green golf bag" },
            { 0x119C, "ranch armchair" },
            { 0x11C8, "vibraphone" },
            { 0x11D0, "conga drum" },
            { 0x11D8, "ruby econo-chair" },
            { 0x11E8, "folding chair" },
            { 0x1228, "low lantern" },
            { 0x1234, "office chair" },
            { 0x1254, "shrine lantern" },
            { 0x126C, "tulip table" },
            { 0x1278, "blue vase" },
            { 0x1284, "iris chair" },
            { 0x1298, "pine chair" },
            { 0x12A4, "sewing machine" },
            { 0x12D0, "strange painting" },
            { 0x12DC, "perfect painting" },
            { 0x12E8, "pineapple bed" },
            { 0x12EC, "orange chair" },
            { 0x12F8, "apple TV" },
            { 0x1310, "wobbelina" },
            { 0x131C, "exotic bench" },
            { 0x1320, "exotic chair" },
            { 0x132C, "caladium" },
            { 0x1330, "lady palm" },
            { 0x1354, "modern table" },
            { 0x135C, "blue bench" },
            { 0x1374, "green bench" },
            { 0x1378, "green chair" },
            { 0x138C, "cabin bed" },
            { 0x139C, "cabin low table" },
            { 0x13AC, "snake plant" },
            { 0x13B8, "pothos" },
            { 0x13C4, "lime chair" },
            { 0x13D8, "cactus" },
            { 0x13E4, "pine bonsai" },
            { 0x1404, "quince bonsai" },
            { 0x140C, "jasmine bonsai" },
            { 0x1410, "executive toy" },
            { 0x1414, "traffic cone" },
            { 0x1468, "watermelon chair" },
            { 0x1480, "violin" },
            { 0x1494, "handcart" },
            { 0x14A4, "detour arrow" },
            { 0x14E4, "mossy stone" },
            { 0x14F4, "stone couple" },
            { 0x1514, "asteroid" },
            { 0x1534, "cabana vanity" },
            { 0x1544, "lunar rover" },
            { 0x1580, "space shuttle" },
            { 0x1588, "regal sofa" },
            { 0x158C, "regal lamp" },
            { 0x159C, "tea set" },
            { 0x15A4, "gerbera" },
            { 0x1DF8, "phonograph" },
            { 0x1E4C, "kitschy clock" },
            { 0x1E50, "antique clock" },
            { 0x1E58, "tape deck" },
            { 0x1E70, "owl clock" },
            { 0x1EB4, "white pawn" },
            { 0x1EC8, "kiddie table" },
            { 0x1ED8, "kiddie bookcase" },
            { 0x1EE8, "mop" },
            { 0x30E8, "grass model" },
            { 0x30F8, "orange box" },
            { 0x31E8, "G logo" },
            { 0x31EC, "merge sign" },
            { 0x3200, "lefty desk" },
            { 0x3234, "bug zapper" },
            { 0x323C, "coffee machine" },
            { 0x3240, "bird bath" },
            { 0x3248, "radiator" },
            { 0x324C, "lawn chair" },
            { 0x3278, "flip-top desk" },
            { 0x32A4, "Mrs. Flamingo" },
            { 0x32B4, "cow skull" },
            { 0x32B8, "oil drum" },
            { 0x32C0, "western fence" },
            { 0x333C, "neutral corner" },
            { 0x334C, "ringside table" },
            { 0x3350, "speed bag" },
            { 0x100C, "office locker" },
            { 0x1014, "regal armoire" },
            { 0x1018, "cabana wardrobe" },
            { 0x1028, "pear wardrobe" },
            { 0x1064, "blue bureau" },
            { 0x1074, "kiddie bureau" },
            { 0x10A4, "froggy chair" },
            { 0x10C4, "stove" },
            { 0x10C8, "cream sofa" },
            { 0x10DC, "country guitar" },
            { 0x10EC, "Mama bear" },
            { 0x1104, "classic cabinet" },
            { 0x1118, "Yuki figurine" },
            { 0x111C, "Yoko figurine" },
            { 0x1138, "retro TV" },
            { 0x1140, "raven pole" },
            { 0x116C, "lovely lamp" },
            { 0x1174, "lovely chair" },
            { 0x1188, "white golf bag" },
            { 0x1194, "writing chair" },
            { 0x11A0, "ranch tea table" },
            { 0x11B4, "ranch table" },
            { 0x11C0, "Master Sword" },
            { 0x11CC, "biwa lute" },
            { 0x11D4, "extinguisher" },
            { 0x11DC, "gold econo-chair" },
            { 0x11E4, "gold stereo" },
            { 0x11EC, "lovely vanity" },
            { 0x11F0, "birdcage" },
            { 0x120C, "tall cactus" },
            { 0x1214, "classic bed" },
            { 0x122C, "tall lantern" },
            { 0x1238, "cubby hole" },
            { 0x1258, "barrel" },
            { 0x1260, "vaulting horse" },
            { 0x1264, "glass-top table" },
            { 0x1268, "alarm clock" },
            { 0x1270, "daffodil table" },
            { 0x127C, "tulip chair" },
            { 0x129C, "tea vase" },
            { 0x12D4, "rare painting" },
            { 0x12E0, "fine painting" },
            { 0x12F4, "lemon table" },
            { 0x12FC, "table tennis" },
            { 0x130C, "water bird" },
            { 0x1318, "slot machine" },
            { 0x1324, "exotic chest" },
            { 0x134C, "modern desk" },
            { 0x1368, "blue bookcase" },
            { 0x1370, "green bed" },
            { 0x1380, "green counter" },
            { 0x1394, "cabin armchair" },
            { 0x13A0, "aloe" },
            { 0x13A4, "bromeliaceae" },
            { 0x13A8, "coconut palm" },
            { 0x13B4, "rubber tree" },
            { 0x13DC, "metronome" },
            { 0x13E8, "mugho bonsai" },
            { 0x1408, "azalea bonsai" },
            { 0x142C, "holly bonsai" },
            { 0x1440, "soda machine" },
            { 0x1450, "green drum" },
            { 0x146C, "melon chair" },
            { 0x1488, "cello" },
            { 0x14AC, "standing stone" },
            { 0x14DC, "lunar lander" },
            { 0x14E8, "leaning stone" },
            { 0x14FC, "rocket" },
            { 0x1510, "exotic end table" },
            { 0x1518, "cabana lamp" },
            { 0x1524, "scale" },
            { 0x153C, "cabana bookcase" },
            { 0x1568, "modern chair" },
            { 0x1570, "space station" },
            { 0x1584, "regal vanity" },
            { 0x1590, "cabin table" },
            { 0x15A8, "sunflower" },
            { 0x1E08, "white boom box" },
            { 0x1E5C, "CD player" },
            { 0x1E60, "glow clock" },
            { 0x1E6C, "cube clock" },
            { 0x1EB8, "black pawn" },
            { 0x1ECC, "kiddie couch" },
            { 0x1ED4, "kiddie chair" },
            { 0x1F50, "modern lamp" },
            { 0x30EC, "track model" },
            { 0x30F4, "train car model" },
            { 0x31F4, "wet roadway sign" },
            { 0x31F8, "detour sign" },
            { 0x3204, "righty desk" },
            { 0x320C, "flagman sign" },
            { 0x3214, "jersey barrier" },
            { 0x3218, "speed sign" },
            { 0x3220, "teacher's desk" },
            { 0x3244, "barbecue" },
            { 0x3264, "tiki torch" },
            { 0x3268, "birdhouse" },
            { 0x326C, "potbelly stove" },
            { 0x3270, "bus stop" },
            { 0x3290, "Mr. Flamingo" },
            { 0x32C4, "watering trough" },
            { 0x3328, "desert cactus" },
            { 0x3330, "wagon wheel" },
            { 0x3354, "sandbag" },
            { 0x101C, "cabin wardrobe" },
            { 0x1038, "exotic wardrobe" },
            { 0x1040, "regal dresser" },
            { 0x104C, "lovely dresser" },
            { 0x1070, "kiddie dresser" },
            { 0x1078, "kiddie wardrobe" },
            { 0x1088, "fan" },
            { 0x10B8, "red sofa" },
            { 0x10E0, "rock guitar" },
            { 0x10F0, "Baby bear" },
            { 0x10F8, "classic chair" },
            { 0x1110, "writing desk" },
            { 0x1120, "Emi figurine" },
            { 0x1124, "Maki figurine" },
            { 0x1144, "bear pole" },
            { 0x114C, "taiko drum" },
            { 0x1154, "retro stereo" },
            { 0x115C, "classic sofa" },
            { 0x117C, "classic clock" },
            { 0x118C, "blue golf bag" },
            { 0x1190, "regal bookcase" },
            { 0x1198, "ranch couch" },
            { 0x11A8, "ranch bookcase" },
            { 0x11B0, "ranch bed" },
            { 0x11BC, "office desk" },
            { 0x11C4, "N logo" },
            { 0x11E0, "jade econo-chair" },
            { 0x1210, "round cactus" },
            { 0x121C, "lovely table" },
            { 0x1230, "pond lantern" },
            { 0x124C, "science table" },
            { 0x125C, "keg" },
            { 0x1274, "iris table" },
            { 0x1280, "daffodil chair" },
            { 0x1288, "elephant slide" },
            { 0x128C, "toilet" },
            { 0x1294, "pine table" },
            { 0x12A0, "red vase" },
            { 0x12A8, "billiard table" },
            { 0x12D8, "classic painting" },
            { 0x12E4, "worthy painting" },
            { 0x1300, "harp" },
            { 0x1304, "cabin clock" },
            { 0x1308, "train set" },
            { 0x1334, "exotic screen" },
            { 0x133C, "djimbe drum" },
            { 0x1340, "modern bed" },
            { 0x1350, "modern sofa" },
            { 0x1360, "blue chair" },
            { 0x1384, "green lamp" },
            { 0x1388, "green table" },
            { 0x1390, "cabin couch" },
            { 0x13BC, "fan palm" },
            { 0x13C0, "grapefruit table" },
            { 0x13C8, "weeping fig" },
            { 0x13CC, "corn plant" },
            { 0x13D0, "croton" },
            { 0x13D4, "pachira" },
            { 0x13EC, "barber's pole" },
            { 0x13F0, "ponderosa bonsai" },
            { 0x141C, "orange cone" },
            { 0x1424, "maple bonsai" },
            { 0x1428, "hawthorn bonsai" },
            { 0x1444, "manhole cover" },
            { 0x145C, "iron frame" },
            { 0x1470, "watermelon table" },
            { 0x1478, "garbage can" },
            { 0x147C, "trash bin" },
            { 0x14A8, "garden stone" },
            { 0x14E0, "satellite" },
            { 0x14EC, "dark stone" },
            { 0x14F0, "flying saucer" },
            { 0x1500, "Spaceman Sam" },
            { 0x151C, "cabana table" },
            { 0x1530, "cabana screen" },
            { 0x1554, "blue clock" },
            { 0x1558, "mochi pestle" },
            { 0x1560, "green desk" },
            { 0x15AC, "daffodil" },
            { 0x1E04, "red boom box" },
            { 0x1E44, "apple clock" },
            { 0x1E54, "reel-to-reel" },
            { 0x1E64, "odd clock" },
            { 0x1E68, "red clock" },
            { 0x1EE4, "chalk board" },
            { 0x30F0, "dirt model" },
            { 0x31FC, "men at work sign" },
            { 0x3208, "school desk" },
            { 0x3224, "haz-mat barrel" },
            { 0x322C, "saw horse" },
            { 0x3250, "chess table" },
            { 0x3254, "candy machine" },
            { 0x3260, "jackhammer" },
            { 0x3284, "bird feeder" },
            { 0x3288, "teacher's chair" },
            { 0x329C, "hammock" },
            { 0x32B0, "tumbleweed" },
            { 0x32D8, "storefront" },
            { 0x32DC, "picnic table" },
            { 0x3338, "boxing barricade" },
            { 0x3348, "boxing mat" },
            { 0x3358, "weight bench" },
            { 0x3368, "sprinkler" }
        };

        public static readonly SortedDictionary<ushort, string> NookClothing = new()
        {
            {0x2400, "flame shirt"},
            {0x2403, "future shirt"},
            {0x2408, "folk shirt"},
            {0x240C, "rugby shirt"},
            {0x240D, "sherbet gingham"},
            {0x2426, "blossom shirt"},
            {0x242A, "ribbon shirt"},
            {0x242B, "fall plaid shirt"},
            {0x2430, "Anju's shirt"},
            {0x2431, "Kaffe's shirt"},
            {0x2433, "blue grid shirt"},
            {0x2435, "blue tartan"},
            {0x2437, "orange tie-dye"},
            {0x243D, "two-ball shirt"},
            {0x2440, "five-ball shirt"},
            {0x2443, "eight-ball shirt"},
            {0x2446, "jungle camo"},
            {0x2449, "racer shirt"},
            {0x2450, "nebula shirt"},
            {0x2455, "ski sweater"},
            {0x245E, "yodel shirt"},
            {0x2461, "star shirt"},
            {0x2462, "straw shirt"},
            {0x2464, "dice shirt"},
            {0x2468, "cloudy shirt"},
            {0x246B, "desert shirt"},
            {0x246F, "jade check print"},
            {0x2470, "blue check print"},
            {0x2474, "rose shirt"},
            {0x2478, "bear shirt"},
            {0x247A, "silk bloom shirt"},
            {0x247B, "pop bloom shirt"},
            {0x2480, "snowcone shirt"},
            {0x2482, "sharp outfit"},
            {0x2485, "blossoming shirt"},
            {0x2488, "rainbow shirt"},
            {0x248D, "blue stripe knit"},
            {0x2490, "deer shirt"},
            {0x2494, "diamond shirt"},
            {0x2498, "yellow bar shirt"},
            {0x249C, "fish knit"},
            {0x249D, "vertigo shirt"},
            {0x24A2, "heart shirt"},
            {0x24A5, "li'l bro's shirt"},
            {0x24A7, "caveman tunic"},
            {0x24A8, "café shirt"},
            {0x24A9, "tiki shirt"},
            {0x24AA, "A shirt"},
            {0x24AD, "No.2 shirt"},
            {0x24B0, "No.5 shirt"},
            {0x24B3, "BB shirt"},
            {0x24B4, "beatnik shirt"},
            {0x24B9, "twinkle shirt"},
            {0x24BF, "cherry shirt"},
            {0x24C2, "concierge shirt"},
            {0x24C3, "fresh shirt"},
            {0x24C5, "dawn shirt"},
            {0x24C9, "lemon gingham"},
            {0x24CB, "G logo shirt"},
            {0x24CD, "jester shirt"},
            {0x24D2, "trendy top"},
            {0x24D3, "green ring shirt"},
            {0x24D6, "chichi print"},
            {0x24D7, "wave print"},
            {0x24DC, "leather jerkin"},
            {0x24E3, "puzzling shirt"},
            {0x24E5, "houndstooth knit"},
            {0x24E8, "gaudy sweater"},
            {0x24E9, "cozy sweater"},
            {0x24F1, "dreamy shirt"},
            {0x24F6, "thunder shirt"},
            {0x2402, "wavy pink shirt"},
            {0x2404, "bold check shirt"},
            {0x2406, "bad plaid shirt"},
            {0x240B, "optical shirt"},
            {0x240E, "yellow tartan"},
            {0x241C, "dark polka shirt"},
            {0x241D, "lite polka shirt"},
            {0x242F, "botanical shirt"},
            {0x2438, "purple tie-dye"},
            {0x243A, "blue tie-dye"},
            {0x243B, "red tie-dye"},
            {0x243E, "three-ball shirt"},
            {0x2441, "six-ball shirt"},
            {0x2444, "nine-ball shirt"},
            {0x2447, "desert camo"},
            {0x244A, "racer 6 shirt"},
            {0x244C, "spiderweb shirt"},
            {0x244D, "zipper shirt"},
            {0x244E, "bubble shirt"},
            {0x2451, "neo-classic knit"},
            {0x2453, "turnip top"},
            {0x2454, "oft-seen print"},
            {0x2457, "patchwork top"},
            {0x245B, "diner uniform"},
            {0x2463, "noodle shirt"},
            {0x2465, "kiddie shirt"},
            {0x2466, "frog shirt"},
            {0x2469, "fortune shirt"},
            {0x246C, "aurora knit"},
            {0x2471, "red grid shirt"},
            {0x2473, "floral knit"},
            {0x2475, "sunset top"},
            {0x2479, "MVP shirt"},
            {0x247C, "loud bloom shirt"},
            {0x247D, "hot spring shirt"},
            {0x247F, "deep blue tee"},
            {0x2481, "ugly shirt"},
            {0x2483, "painter's smock"},
            {0x2486, "peachy shirt"},
            {0x248A, "loud line shirt"},
            {0x248E, "earthy knit"},
            {0x2491, "blue check shirt"},
            {0x2496, "big bro's shirt"},
            {0x2497, "green bar shirt"},
            {0x2499, "monkey shirt"},
            {0x249E, "misty shirt"},
            {0x24A0, "red scale shirt"},
            {0x24A3, "yellow pinstripe"},
            {0x24AB, "checkered shirt"},
            {0x24AE, "No.3 shirt"},
            {0x24B1, "No.23 shirt"},
            {0x24B5, "moldy shirt"},
            {0x24B6, "houndstooth tee"},
            {0x24B7, "big star shirt"},
            {0x24BA, "funky dot shirt"},
            {0x24BD, "jagged shirt"},
            {0x24C1, "barber shirt"},
            {0x24C4, "far-out shirt"},
            {0x24C7, "red check shirt"},
            {0x24D4, "white ring shirt"},
            {0x24D8, "checkerboard tee"},
            {0x24D9, "subdued print"},
            {0x24E1, "danger shirt"},
            {0x24E6, "uncommon shirt"},
            {0x24EA, "comfy sweater"},
            {0x24EC, "vogue top"},
            {0x24F2, "flowery shirt"},
            {0x24F4, "shortcake shirt"},
            {0x24F5, "whirly shirt"},
            {0x24FC, "fetching outfit"},
            {0x24FE, "melon gingham"},
            {0x2401, "paw shirt"},
            {0x2405, "mint gingham"},
            {0x2407, "speedway shirt"},
            {0x2409, "daisy shirt"},
            {0x240A, "wavy tan shirt"},
            {0x240F, "gelato shirt"},
            {0x241E, "lovely shirt"},
            {0x2427, "icy shirt"},
            {0x2428, "crewel shirt"},
            {0x2429, "tropical shirt"},
            {0x242D, "chevron shirt"},
            {0x2439, "green tie-dye"},
            {0x243C, "one-ball shirt"},
            {0x243F, "four-ball shirt"},
            {0x2442, "seven-ball shirt"},
            {0x2445, "arctic camo"},
            {0x2448, "rally shirt"},
            {0x244B, "fish bone shirt"},
            {0x244F, "yellow bolero"},
            {0x2452, "noble shirt"},
            {0x2456, "circus shirt"},
            {0x2458, "mod top"},
            {0x2459, "hippie shirt"},
            {0x245A, "rickrack shirt"},
            {0x245D, "U R here shirt"},
            {0x2460, "prism shirt"},
            {0x2467, "moody blue shirt"},
            {0x246A, "skull shirt"},
            {0x246D, "winter sweater"},
            {0x246E, "go-go shirt"},
            {0x2472, "flicker shirt"},
            {0x2476, "chain-gang shirt"},
            {0x2477, "spring shirt"},
            {0x247E, "new spring shirt"},
            {0x2484, "spade shirt"},
            {0x2487, "static shirt"},
            {0x248B, "dazed shirt"},
            {0x248C, "red bar shirt"},
            {0x248F, "spunky knit"},
            {0x2492, "light line shirt"},
            {0x2493, "blue pinstripe"},
            {0x2495, "lime line shirt"},
            {0x249A, "polar fleece"},
            {0x249B, "ancient knit"},
            {0x249F, "stormy shirt"},
            {0x24A1, "blue scale shirt"},
            {0x24A4, "club shirt"},
            {0x24A6, "argyle knit"},
            {0x24AC, "No.1 shirt"},
            {0x24AF, "No.4 shirt"},
            {0x24B2, "No.67 shirt"},
            {0x24BE, "denim shirt"},
            {0x24C0, "gumdrop shirt"},
            {0x24C6, "striking outfit"},
            {0x24C8, "berry gingham"},
            {0x24CA, "dragon suit"},
            {0x24CE, "pink tartan"},
            {0x24CF, "waffle shirt"},
            {0x24D0, "gray tartan"},
            {0x24D1, "windsock shirt"},
            {0x24D5, "snappy print"},
            {0x24DA, "airy shirt"},
            {0x24E2, "big dot shirt"},
            {0x24E4, "exotic shirt"},
            {0x24E7, "dapper shirt"},
            {0x24EB, "classic top"},
            {0x24ED, "laced shirt"},
            {0x24EE, "natty shirt"},
            {0x24EF, "citrus gingham"},
            {0x24F9, "toad print"},
            {0x24FB, "mosaic shirt"}
        };

        /* Array containing the 'grow' type for all villagers. 0 = starter, 1 = move-in, 2 = islander */
        private static readonly int[] npc_grow_list =
        {
            0, 0, 0, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 1, 1, 1,
            1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0,
            1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1,
            1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 1, 0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 1, 0, 0, 1, 1, 0, 0, 0, 1, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 1, 0, 2, 2, 2, 2, 2, 2, 2, 2,
            2, 2, 2, 2, 2, 2, 2, 2, 0, 2, 2, 0
        };

        /* Villager personality table */
        private static readonly int[] npc_looks_table =
        {
            2, 5, 0, 0, 1, 4, 1, 5, 3, 5, 5, 4, 1, 0, 0, 1,
            4, 5, 3, 4, 2, 2, 4, 1, 5, 4, 1, 3, 1, 2, 1, 4,
            2, 5, 0, 3, 4, 4, 2, 3, 0, 3, 2, 0, 3, 1, 5, 4,
            4, 5, 3, 0, 2, 0, 3, 1, 1, 5, 0, 1, 2, 3, 0, 2,
            0, 0, 2, 5, 1, 4, 3, 1, 2, 3, 2, 1, 2, 2, 3, 0,
            3, 4, 3, 1, 0, 2, 2, 4, 3, 2, 0, 3, 0, 3, 4, 2,
            5, 5, 4, 3, 4, 2, 1, 0, 2, 1, 0, 3, 5, 0, 3, 0,
            5, 1, 0, 5, 5, 0, 4, 2, 5, 1, 2, 3, 3, 3, 1, 0,
            2, 1, 4, 1, 4, 5, 4, 0, 3, 1, 5, 4, 4, 3, 5, 3,
            3, 0, 4, 2, 2, 5, 5, 3, 1, 2, 2, 4, 5, 3, 0, 3,
            4, 1, 2, 4, 0, 1, 1, 3, 3, 2, 3, 4, 5, 0, 2, 0,
            1, 5, 4, 4, 5, 1, 3, 4, 1, 4, 5, 4, 2, 5, 4, 2,
            4, 2, 3, 5, 2, 0, 5, 4, 1, 5, 0, 0, 5, 5, 1, 0,
            5, 4, 0, 1, 0, 5, 3, 1, 5, 4, 0, 3, 3, 2, 1, 4,
            2, 1, 5, 1, 0, 2, 4, 0, 1, 3, 5, 2, 2, 1
        };

        /* Table of villager indices normally created in mNpc_MakeRandTable. To optimize it, I create it before and copy it. */
        private static readonly int[] fakeTableBase = new int[]
        {
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15,
            16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31,
            32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47,
            48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63,
            64, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79,
            80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 91, 92, 93, 94, 95,
            96, 97, 98, 99, 100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111,
            112, 113, 114, 115, 116, 117, 118, 119, 120, 121, 122, 123, 124, 125, 126, 127,
            128, 129, 130, 131, 132, 133, 134, 135, 136, 137, 138, 139, 140, 141, 142, 143,
            144, 145, 146, 147, 148, 149, 150, 151, 152, 153, 154, 155, 156, 157, 158, 159,
            160, 161, 162, 163, 164, 165, 166, 167, 168, 169, 170, 171, 172, 173, 174, 175,
            176, 177, 178, 179, 180, 181, 182, 183, 184, 185, 186, 187, 188, 189, 190, 191,
            192, 193, 194, 195, 196, 197, 198, 199, 200, 201, 202, 203, 204, 205, 206, 207,
            208, 209, 210, 211, 212, 213, 214, 215, 216, 217, 218, 219, 220, 221, 222, 223,
            224, 225, 226, 227, 228, 229, 230, 231, 232, 233, 234, 235
        };

        const int count = 6;
        const int npc_count = 0xEC;

        /* idum should be found as 0xE1090443 */
        //static readonly int[] correct_villagers = new int[] { 0x94, 0x9B, 0x04, 0x22, 0x97, 0x18 };
        //const int correct_station = 12;
        const int correct_townday = 0x1a;

        private readonly int[] fakeTable = new int[npc_count];

        private int[] correct_villagers;

        public InitialTownLogic(in int[] villagers)
        {
            correct_villagers = villagers;
        }

        public uint __qrand_idum { get; set; } = 0;
        private float __qrand_itemp = 0.0f;

        public uint fqrand_noret()
        {
            return (__qrand_idum = (__qrand_idum * 0x19660D) + 0x3C6EF35F); /* Only update seed, we don't care about float value */
        }

        public unsafe float fqrand()
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

        private bool mNpc_DecideLivingNpcMax_THREADSAFE()
        {

            //int[] fakeTable = new int[npc_count];
            //int[] selectedAnimals = new int[count];

            /* Inlined function mNpc_MakeRandTable */

            //var sw0 = Stopwatch.StartNew();

            int v0 = correct_villagers[0];
            int v1 = correct_villagers[1];
            int v2 = correct_villagers[2];
            int v3 = correct_villagers[3];
            int v4 = correct_villagers[4];
            int v5 = correct_villagers[5];

            uint start_seed = __qrand_idum;

            for (int i = 0; i < npc_count; i++)
            {
                int slot0 = (int)(npc_count * fqrand());
                int slot1 = (int)(npc_count * fqrand());

                if (slot0 == v0)
                {
                    v0 = slot1;
                    if (slot1 == v1)
                    {
                        v1 = slot0;
                    }
                    else if (slot1 == v2)
                    {
                        v2 = slot0;
                    }
                    else if (slot1 == v3)
                    {
                        v3 = slot0;
                    }
                    else if (slot1 == v4)
                    {
                        v4 = slot0;
                    }
                    else if (slot1 == v5)
                    {
                        v5 = slot0;
                    }
                }
                else if (slot0 == v1)
                {
                    v1 = slot1;
                    if (slot1 == v0)
                    {
                        v0 = slot0;
                    }
                    else if (slot1 == v2)
                    {
                        v2 = slot0;
                    }
                    else if (slot1 == v3)
                    {
                        v3 = slot0;
                    }
                    else if (slot1 == v4)
                    {
                        v4 = slot0;
                    }
                    else if (slot1 == v5)
                    {
                        v5 = slot0;
                    }
                }
                else if (slot0 == v2)
                {
                    v2 = slot1;
                    if (slot1 == v0)
                    {
                        v0 = slot0;
                    }
                    else if (slot1 == v1)
                    {
                        v1 = slot0;
                    }
                    else if (slot1 == v3)
                    {
                        v3 = slot0;
                    }
                    else if (slot1 == v4)
                    {
                        v4 = slot0;
                    }
                    else if (slot1 == v5)
                    {
                        v5 = slot0;
                    }
                }
                else if (slot0 == v3)
                {
                    v3 = slot1;
                    if (slot1 == v0)
                    {
                        v0 = slot0;
                    }
                    else if (slot1 == v1)
                    {
                        v1 = slot0;
                    }
                    else if (slot1 == v2)
                    {
                        v2 = slot0;
                    }
                    else if (slot1 == v4)
                    {
                        v4 = slot0;
                    }
                    else if (slot1 == v5)
                    {
                        v5 = slot0;
                    }
                }
                else if (slot0 == v4)
                {
                    v4 = slot1;
                    if (slot1 == v0)
                    {
                        v0 = slot0;
                    }
                    else if (slot1 == v1)
                    {
                        v1 = slot0;
                    }
                    else if (slot1 == v2)
                    {
                        v2 = slot0;
                    }
                    else if (slot1 == v3)
                    {
                        v3 = slot0;
                    }
                    else if (slot1 == v5)
                    {
                        v5 = slot0;
                    }
                }
                else if (slot0 == v5)
                {
                    v5 = slot1;
                    if (slot1 == v0)
                    {
                        v0 = slot0;
                    }
                    else if (slot1 == v1)
                    {
                        v1 = slot0;
                    }
                    else if (slot1 == v2)
                    {
                        v2 = slot0;
                    }
                    else if (slot1 == v3)
                    {
                        v3 = slot0;
                    }
                    else if (slot1 == v4)
                    {
                        v4 = slot0;
                    }
                }
                else if (slot1 == v0)
                {
                    v0 = slot0;
                    if (slot0 == v1)
                    {
                        v1 = slot1;
                    }
                    else if (slot0 == v2)
                    {
                        v2 = slot1;
                    }
                    else if (slot0 == v3)
                    {
                        v3 = slot1;
                    }
                    else if (slot0 == v4)
                    {
                        v4 = slot1;
                    }
                    else if (slot0 == v5)
                    {
                        v5 = slot1;
                    }
                }
                else if (slot1 == v1)
                {
                    v1 = slot0;
                    if (slot0 == v0)
                    {
                        v0 = slot1;
                    }
                    else if (slot0 == v2)
                    {
                        v2 = slot1;
                    }
                    else if (slot0 == v3)
                    {
                        v3 = slot1;
                    }
                    else if (slot0 == v4)
                    {
                        v4 = slot1;
                    }
                    else if (slot0 == v5)
                    {
                        v5 = slot1;
                    }
                }
                else if (slot1 == v2)
                {
                    v2 = slot0;
                    if (slot0 == v0)
                    {
                        v0 = slot1;
                    }
                    else if (slot0 == v1)
                    {
                        v1 = slot1;
                    }
                    else if (slot0 == v3)
                    {
                        v3 = slot1;
                    }
                    else if (slot0 == v4)
                    {
                        v4 = slot1;
                    }
                    else if (slot0 == v5)
                    {
                        v5 = slot1;
                    }
                }
                else if (slot1 == v3)
                {
                    v3 = slot0;
                    if (slot0 == v0)
                    {
                        v0 = slot1;
                    }
                    else if (slot0 == v1)
                    {
                        v1 = slot1;
                    }
                    else if (slot0 == v2)
                    {
                        v2 = slot1;
                    }
                    else if (slot0 == v4)
                    {
                        v4 = slot1;
                    }
                    else if (slot0 == v5)
                    {
                        v5 = slot1;
                    }
                }
                else if (slot1 == v4)
                {
                    v4 = slot0;
                    if (slot0 == v0)
                    {
                        v0 = slot1;
                    }
                    else if (slot0 == v1)
                    {
                        v1 = slot1;
                    }
                    else if (slot0 == v2)
                    {
                        v2 = slot1;
                    }
                    else if (slot0 == v3)
                    {
                        v3 = slot1;
                    }
                    else if (slot0 == v5)
                    {
                        v5 = slot1;
                    }
                }
                else if (slot1 == v5)
                {
                    v5 = slot0;
                    if (slot0 == v0)
                    {
                        v0 = slot1;
                    }
                    else if (slot0 == v1)
                    {
                        v1 = slot1;
                    }
                    else if (slot0 == v2)
                    {
                        v2 = slot1;
                    }
                    else if (slot0 == v3)
                    {
                        v3 = slot1;
                    }
                    else if (slot0 == v4)
                    {
                        v4 = slot1;
                    }
                }
            }

            if (v0 < v1 && v1 < v2 && v2 < v3 && v3 < v4 && v4 < v5)
            {
                __qrand_idum = start_seed;
                for (int i = 0; i < npc_count; i++)
                {
                    fakeTable[i] = i;
                }

                //sw0.Stop();
                //Debug.WriteLine($"Took Loop Assign: {(double)sw0.ElapsedTicks / (double)Stopwatch.Frequency} seconds");


                //var sw1 = Stopwatch.StartNew();
                //Buffer.BlockCopy(fakeTableBase, 0, fakeTable, 0, npc_count * sizeof(int)); /* We could squeeze a bit more performance by using C#'s memcpyimpl. See: https://stackoverflow.com/questions/5099604/any-faster-way-of-copying-arrays-in-c */
                //sw1.Stop();
                //Debug.WriteLine($"Took BlockCopy: {(double)sw1.ElapsedTicks / (double)Stopwatch.Frequency} seconds");

                for (int i = 0; i < npc_count; i++)
                {
                    int slot0 = (int)(npc_count * fqrand());
                    int slot1 = (int)(npc_count * fqrand());

                    (fakeTable[slot1], fakeTable[slot0]) = (fakeTable[slot0], fakeTable[slot1]);
                }

                /* Return to standard function */

                /* Go through the randomized villager index table and select the first ones of each personality type */
                int personality_set_field = 0;
                int npc_idx = 0;
                for (int i = 0; i < npc_count; i++)
                {
                    int idx = fakeTable[i];
                    if (npc_grow_list[idx] == 0)
                    {
                        int personality = npc_looks_table[idx];
                        if (((personality_set_field >> personality) & 1) == 0)
                        {
                            if (idx != correct_villagers[npc_idx])
                            {
                                return false;
                            }

                            //selectedAnimals[npc_idx++] = idx;
                            npc_idx++;
                            if (npc_idx >= count)
                            {
                                return true; /* All villagers have been generated */
                            }

                            personality_set_field |= (1 << personality);
                        }
                    }
                }
            }

            return false;
        }

        public static unsafe void GpuKernel_InitialFilter(in int[] input, long start, long end, int thread)
        {
            int totals = 0;

            // Compute our start index
            int v0 = input[0];
            int v1 = input[1];
            int v2 = input[2];
            int v3 = input[3];
            int v4 = input[4];
            int v5 = input[5];

            uint __qrand_idum;
            float __qrand_itemp = 0.0f;

            for (; start < end; start++)
            {
                __qrand_idum = (uint)start;

                for (int i = 0; i < 0xEC; i++)
                {
                    __qrand_idum = (__qrand_idum * 0x19660D) + 0x3C6EF35F;
                    var f_temp = (__qrand_idum >> 9) | 0x3F800000;
                    __qrand_itemp = *(float*)&f_temp;

                    int slot0 = (int)(0xEC * (__qrand_itemp - 1.0f));

                    __qrand_idum = (__qrand_idum * 0x19660D) + 0x3C6EF35F;
                    f_temp = (__qrand_idum >> 9) | 0x3F800000;
                    __qrand_itemp = *(float*)&f_temp;

                    int slot1 = (int)(0xEC * (__qrand_itemp - 1.0f));

                    if (slot0 == v0)
                    {
                        v0 = slot1;
                        if (slot1 == v1)
                        {
                            v1 = slot0;
                        }
                        else if (slot1 == v2)
                        {
                            v2 = slot0;
                        }
                        else if (slot1 == v3)
                        {
                            v3 = slot0;
                        }
                        else if (slot1 == v4)
                        {
                            v4 = slot0;
                        }
                        else if (slot1 == v5)
                        {
                            v5 = slot0;
                        }
                    }
                    else if (slot0 == v1)
                    {
                        v1 = slot1;
                        if (slot1 == v0)
                        {
                            v0 = slot0;
                        }
                        else if (slot1 == v2)
                        {
                            v2 = slot0;
                        }
                        else if (slot1 == v3)
                        {
                            v3 = slot0;
                        }
                        else if (slot1 == v4)
                        {
                            v4 = slot0;
                        }
                        else if (slot1 == v5)
                        {
                            v5 = slot0;
                        }
                    }
                    else if (slot0 == v2)
                    {
                        v2 = slot1;
                        if (slot1 == v0)
                        {
                            v0 = slot0;
                        }
                        else if (slot1 == v1)
                        {
                            v1 = slot0;
                        }
                        else if (slot1 == v3)
                        {
                            v3 = slot0;
                        }
                        else if (slot1 == v4)
                        {
                            v4 = slot0;
                        }
                        else if (slot1 == v5)
                        {
                            v5 = slot0;
                        }
                    }
                    else if (slot0 == v3)
                    {
                        v3 = slot1;
                        if (slot1 == v0)
                        {
                            v0 = slot0;
                        }
                        else if (slot1 == v1)
                        {
                            v1 = slot0;
                        }
                        else if (slot1 == v2)
                        {
                            v2 = slot0;
                        }
                        else if (slot1 == v4)
                        {
                            v4 = slot0;
                        }
                        else if (slot1 == v5)
                        {
                            v5 = slot0;
                        }
                    }
                    else if (slot0 == v4)
                    {
                        v4 = slot1;
                        if (slot1 == v0)
                        {
                            v0 = slot0;
                        }
                        else if (slot1 == v1)
                        {
                            v1 = slot0;
                        }
                        else if (slot1 == v2)
                        {
                            v2 = slot0;
                        }
                        else if (slot1 == v3)
                        {
                            v3 = slot0;
                        }
                        else if (slot1 == v5)
                        {
                            v5 = slot0;
                        }
                    }
                    else if (slot0 == v5)
                    {
                        v5 = slot1;
                        if (slot1 == v0)
                        {
                            v0 = slot0;
                        }
                        else if (slot1 == v1)
                        {
                            v1 = slot0;
                        }
                        else if (slot1 == v2)
                        {
                            v2 = slot0;
                        }
                        else if (slot1 == v3)
                        {
                            v3 = slot0;
                        }
                        else if (slot1 == v4)
                        {
                            v4 = slot0;
                        }
                    }
                    else if (slot1 == v0)
                    {
                        v0 = slot0;
                        if (slot0 == v1)
                        {
                            v1 = slot1;
                        }
                        else if (slot0 == v2)
                        {
                            v2 = slot1;
                        }
                        else if (slot0 == v3)
                        {
                            v3 = slot1;
                        }
                        else if (slot0 == v4)
                        {
                            v4 = slot1;
                        }
                        else if (slot0 == v5)
                        {
                            v5 = slot1;
                        }
                    }
                    else if (slot1 == v1)
                    {
                        v1 = slot0;
                        if (slot0 == v0)
                        {
                            v0 = slot1;
                        }
                        else if (slot0 == v2)
                        {
                            v2 = slot1;
                        }
                        else if (slot0 == v3)
                        {
                            v3 = slot1;
                        }
                        else if (slot0 == v4)
                        {
                            v4 = slot1;
                        }
                        else if (slot0 == v5)
                        {
                            v5 = slot1;
                        }
                    }
                    else if (slot1 == v2)
                    {
                        v2 = slot0;
                        if (slot0 == v0)
                        {
                            v0 = slot1;
                        }
                        else if (slot0 == v1)
                        {
                            v1 = slot1;
                        }
                        else if (slot0 == v3)
                        {
                            v3 = slot1;
                        }
                        else if (slot0 == v4)
                        {
                            v4 = slot1;
                        }
                        else if (slot0 == v5)
                        {
                            v5 = slot1;
                        }
                    }
                    else if (slot1 == v3)
                    {
                        v3 = slot0;
                        if (slot0 == v0)
                        {
                            v0 = slot1;
                        }
                        else if (slot0 == v1)
                        {
                            v1 = slot1;
                        }
                        else if (slot0 == v2)
                        {
                            v2 = slot1;
                        }
                        else if (slot0 == v4)
                        {
                            v4 = slot1;
                        }
                        else if (slot0 == v5)
                        {
                            v5 = slot1;
                        }
                    }
                    else if (slot1 == v4)
                    {
                        v4 = slot0;
                        if (slot0 == v0)
                        {
                            v0 = slot1;
                        }
                        else if (slot0 == v1)
                        {
                            v1 = slot1;
                        }
                        else if (slot0 == v2)
                        {
                            v2 = slot1;
                        }
                        else if (slot0 == v3)
                        {
                            v3 = slot1;
                        }
                        else if (slot0 == v5)
                        {
                            v5 = slot1;
                        }
                    }
                    else if (slot1 == v5)
                    {
                        v5 = slot0;
                        if (slot0 == v0)
                        {
                            v0 = slot1;
                        }
                        else if (slot0 == v1)
                        {
                            v1 = slot1;
                        }
                        else if (slot0 == v2)
                        {
                            v2 = slot1;
                        }
                        else if (slot0 == v3)
                        {
                            v3 = slot1;
                        }
                        else if (slot0 == v4)
                        {
                            v4 = slot1;
                        }
                    }
                }

                // We check here if the order is wrong, faster? than doing the rest.
                if (v0 < v1 && v1 < v2 && v2 < v3 && v3 < v4 && v4 < v5)
                {
                    //output[writePos.Value] = (uint)start;
                    totals++;
                }
            }
            Console.WriteLine($"{thread}: Totals: {totals}");
        }

        private bool mSDI_StartInitNew_THREADSAFE()
        {
            if (!mNpc_DecideLivingNpcMax_THREADSAFE())
            {
                return false;
            }

            /*
            for (int i = 0; i < 11; i++)
            {
                fqrand();
            }

            int trainStation = (int)(fqrand() * 15.0f);
            if (trainStation != correct_station)
            {
                return false;
            }

            int townDay = (int)(fqrand() * 30.0f) + 1;
            if (townDay > 3)
            {
                townDay++;
            }

            if (townDay != correct_townday)
            {
                return false;
            }

            fqrand(); // mISL_init
            */
            return true;
        }

        private bool mSDI_StartInitNew_FromSeed(uint seed, int correct_station)
        {

            __qrand_idum = seed;
            for (int i = 0; i < npc_count / 4; i++)
            {
                fqrand();
                fqrand();
                fqrand();
                fqrand();
                fqrand();
                fqrand();
                fqrand();
                fqrand();
            }


            /* Starting after villagers are selected */
            for (int i = 0; i < 11; i++)
            {
                fqrand();
            }

            int trainStation = (int)(fqrand() * 15.0f);
            if (trainStation != correct_station)
            {
                return false;
            }

            /*
            int townDay = (int)(fqrand() * 30.0f) + 1;
            if (townDay > 3)
            {
                townDay++;
            }

            if (townDay != correct_townday)
            {
                return false;
            }

            fqrand(); // mISL_init
            */
            return true;
        }

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int memcmp(IntPtr a1, IntPtr a2, uint count);

        public static unsafe bool Memcmp(in int[] data1, in int[] data2)
        {
            fixed (int* p1 = data1, p2 = data2)
                return memcmp((IntPtr)p1, (IntPtr)p2, (uint)data1.Length * sizeof(int)) == 0;
        }

        private List<uint> BruteForceIdum(int threadId, long start, long end)
        {
            List<uint> result = new();
            var sw = Stopwatch.StartNew();

#if DEBUG
            int x = 0;
#endif
            for (long i = start; i < end; i++)
            {
                __qrand_idum = (uint)i;
                if (mSDI_StartInitNew_THREADSAFE())
                {
                    result.Add((uint)i);
                    Console.WriteLine($"Possible idum: {i:X8}");
                }

#if DEBUG
                if ((x % 3000000) == 0)
                {
                    Console.WriteLine($"Thread #{threadId}: {((double)(i - start) / (double)(end - start)) * 100.0d}%");
                }
                x++;
#endif
            }

            Console.WriteLine($"Thread #{threadId} finished! Took: {(double)sw.ElapsedTicks / (double)Stopwatch.Frequency} seconds!");
            return result;
        }

        public static async Task<List<uint>[]> SeedBruteforce_Thread()
        {
            List<Task<List<uint>>> tasks = new();
            int numThreads = Environment.ProcessorCount;
            if (numThreads > 3)
                numThreads -= 2; // Save two threads for computer usage.
            long step = ((long)uint.MaxValue + 1) / numThreads;
            Console.WriteLine($"# of Threads: {numThreads}");
            for (int i = 0; i < numThreads; i++)
            {
                long startId = i * step;
                long endId = i == numThreads - 1 ? ((long)uint.MaxValue + 1) : (i + 1) * step;
                int threadId = i;
                Console.WriteLine($"Thread #{i} started for range {startId:X8}-{endId:X8}");
                tasks.Add(Task.Factory.StartNew(() => new InitialTownLogic(new int[0]).BruteForceIdum(threadId, startId, endId)));
            }

            return await Task.WhenAll(tasks);
        }

        public static async Task<List<uint>> SeedBruteforce_ComboCPUGPU(int[] villagers, double cpu_responsibility, int cpu_threadcount, string? gpuAdapterName)
        {
            const long NUM = (long)uint.MaxValue + 1;

            long r_cpu = (long)Math.Ceiling(NUM * cpu_responsibility);
            long r_gpu = NUM - r_cpu;

            var sw = Stopwatch.StartNew();
            List<Task<List<uint>>> tasks = new();
            List<uint> combined = new();

            if (r_cpu > 0)
            {
                long step = r_cpu / cpu_threadcount;
                Console.WriteLine($"# of Threads: {cpu_threadcount}");
                for (int i = 0; i < cpu_threadcount; i++)
                {
                    long startId = i * step;
                    long endId = i == cpu_threadcount - 1 ? r_cpu : (i + 1) * step;
                    int threadId = i;
                    Console.WriteLine($"Thread #{i} started for range {startId:X8}-{endId:X8}");
                    tasks.Add(Task.Factory.StartNew(() => new InitialTownLogic(villagers).BruteForceIdum(threadId, startId, endId)));
                }
            }

            if (r_gpu > 0)
            {
                tasks.Add(ILGPULogic.Execute(villagers, r_cpu, r_gpu, gpuAdapterName));
            }

            List<uint>[] results = await Task.WhenAll(tasks);
            foreach (List<uint> result in results)
            {
                combined.AddRange(result);
            }

            sw.Stop();
            Console.WriteLine($"Took {(int)(sw.ElapsedTicks / Stopwatch.Frequency)} seconds");
            
            for (int i = 0; i < combined.Count; i++)
            {
                Console.WriteLine($"Possible seed: {combined[i]:X8}");
            }

            return combined;
        }

        public static List<uint> RefineInitialSeeds(List<uint> seeds, int trainStation)
        {
            List<uint> validSeeds = new();
            foreach (uint seed in seeds)
            {
                if (new InitialTownLogic(null).mSDI_StartInitNew_FromSeed(seed, trainStation))
                {
                    validSeeds.Add(seed);
                }
            }

            return validSeeds;
        }

        private static readonly float[][] marketTrendPercentageTable =
        {
            new float[] { 0.5f, 0.3f, 0.2f },
            new float[] { 0.6f, 0.2f, 0.2f },
            new float[] { 0.6f, 0.3f, 0.1f }
        };

        private MarketTrend DecideTradeMarketTrend(MarketTrend currentTrend)
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

        private static readonly int[] ABCList_priority_candidate =
        {
            0x18, 0x24, 0x48, 0x84, 0x90, 0x60
        };

        private static MarketTrend GetInitialMarketTrend(uint seed)
        {
            var gen = new InitialTownLogic(null);
            gen.__qrand_idum = seed;

            // Villager generation
            for (int i = 0; i < 236*2; i++)
            {
                gen.fqrand();
            }

            // rest of StartInitNew
            for (int i = 0; i < 14; i++)
            {
                gen.fqrand();
            }

            // weather
            gen.fqrand();
            gen.fqrand();

            /* Stalk Market */
            return gen.DecideTradeMarketTrend(MarketTrend.Spike);
        }

        public static List<InitialResult> FindTurnipPriceGenSeed(List<uint> seeds, int startAddRange, int endAddRange, ushort nookFtr, ushort nookUmb)
        {
            List<InitialResult> possibleSeeds = new();
            var gen = new InitialTownLogic(null);
            foreach (uint seed in seeds)
            {
                gen.__qrand_idum = seed;
                for (uint i = 0; i < startAddRange; i++)
                {
                    gen.fqrand(); /* Perform minimum fqrand rolls */
                }

                /* Get initial market trend in case it's a spike so we can have the right # of fqrand calls */
                MarketTrend initialTrend = GetInitialMarketTrend(seed);

                uint last_fqrand; // save seed
                
                /* Don't do any unnecessary calculations while searching for the correct seed to increase performance */
                for (int i = startAddRange; i < endAddRange; i++)
                {
                    last_fqrand = gen.__qrand_idum;
                    gen.fqrand(); // islander

                    /* Stalk Market */
                    int sundayPrice = (int)(((gen.fqrand() - 0.5f) * 0.6f + 1.0f) * 100.0f); // sunday turnip price
                    MarketTrend trend = gen.DecideTradeMarketTrend(initialTrend); // 1x trend

                    /* Monday - Saturday price */
                    gen.fqrand();
                    gen.fqrand();
                    gen.fqrand();
                    gen.fqrand();
                    gen.fqrand();
                    gen.fqrand();

                    int spikeDay = 0;
                    if (trend == MarketTrend.Spike)
                    {
                        spikeDay = (int)(gen.fqrand() * 5.0f) + 1; // spike day
                    }

                    gen.fqrand(); // insect term overlap
                    gen.fqrand(); // fish term overlap
                    gen.fqrand(); // town day re-roll

                    /* ABC list rolls (we only care about furniture right now) */
                    int abc_list_ftr_prio = ABCList_priority_candidate[(int)(gen.fqrand() * 6.0f)];
                    gen.fqrand();
                    int abc_list_clothing_prio = ABCList_priority_candidate[(int)(gen.fqrand() * 6.0f)];
                    gen.fqrand();
                    gen.fqrand();


                    /* Lottery rolls */
                    gen.fqrand();
                    gen.fqrand();
                    gen.fqrand();

                    /* Furniture selection */
                    ListPriority a = (ListPriority)((abc_list_ftr_prio >> 6) & 3);
                    ListPriority b = (ListPriority)((abc_list_ftr_prio >> 4) & 3);
                    ListPriority c = (ListPriority)((abc_list_ftr_prio >> 2) & 3);

                    Logic.__qrand_idum = gen.__qrand_idum;
                    ushort[] ftr = Logic.GetItemsForNooks(1, 0, Logic.ItemCategory.Furniture, a, b, c, 0, 7, 19);

                    /* Stationery - we don't care about the list priorities here */
                    Logic.GetItemsForNooks(1, 0, Logic.ItemCategory.Stationery, ListPriority.Common, ListPriority.Uncommon, ListPriority.Rare, 0, 6, 1);
                    // Sync idum
                    gen.__qrand_idum = Logic.__qrand_idum;


                    /* Clothing selection */
                    /*
                    ListPriority c_a = (ListPriority)((abc_list_clothing_prio >> 6) & 3);
                    ListPriority c_b = (ListPriority)((abc_list_clothing_prio >> 4) & 3);
                    ListPriority c_c = (ListPriority)((abc_list_clothing_prio >> 2) & 3);
                    ushort[] cloth = Logic.GetItemsForNooks(1, 0, Logic.ItemCategory.Clothing, c_a, c_b, c_c, 0, 9, 25);
                    */
                    //if (ftr.Length == 1 && ftr[0] == nookFtr && cloth.Length == 1 && cloth[0] == nookShirt)
                    Logic.GetItemsForNooks(1, 0, Logic.ItemCategory.Clothing, ListPriority.Common, ListPriority.Uncommon, ListPriority.Rare, 0, 6, 1);
                    Logic.GetItemsForNooks(1, 0, Logic.ItemCategory.Carpets, ListPriority.Common, ListPriority.Uncommon, ListPriority.Rare, 0, 6, 1); // Carpets
                    Logic.GetItemsForNooks(1, 0, Logic.ItemCategory.Wallpaper, ListPriority.Common, ListPriority.Uncommon, ListPriority.Rare, 0, 6, 1); // Wallpaper
                    gen.__qrand_idum = Logic.__qrand_idum; // Sync again

                    gen.fqrand(); // call for tool, will always be shovel

                    ushort umbrella = (ushort)(0x2204u + 32.0f * gen.fqrand());

                    if (ftr.Length == 1 && ftr[0] == nookFtr && umbrella == nookUmb) // && cloth.Length == 1 && cloth[0] == nookShirt 
                    {
                        possibleSeeds.Add(new InitialResult
                        {
                            InitialSeed = seed,
                            TrainSeed = last_fqrand,
                            StalkMarketTrend = trend,
                            SundayPrice = sundayPrice,
                            SpikeDay = spikeDay
                        }); // save initial seed - train seed pair
                    }

                    /* Reset seed */
                    gen.__qrand_idum = last_fqrand;
                    gen.fqrand(); // Increment seed
                }
            }

            return possibleSeeds;
        }

        public static async Task DEBUG_FindFQRandCallsFromStartSeedToNextSeed(uint startSeed, uint nextSeed)
        {
            await Task.Factory.StartNew(() =>
            {
                var gen = new InitialTownLogic(null);
                gen.__qrand_idum = startSeed;
                int i = 0;
                while (gen.__qrand_idum != nextSeed)
                {
                    gen.fqrand();
                    i++;
                }

                Console.WriteLine($"{i} fqrand calls");
            });
        }

        private static void DEBUG_FilterDownPreTownSeeds(List<uint> seeds)
        {
            List<uint> out_list = new();
            InitialTownLogic gen = new(null);

            foreach (uint seed in seeds)
            {
                gen.__qrand_idum = seed;
                int fruit = (int)(gen.fqrand() * 5.0f);
                int bg_tex = (int)(gen.fqrand() * 3.0f);
                bool three_stepmode = (gen.fqrand() * 100.0f) < 15; // 85% chance for 2 step, 15% chance for 3 step

                if (fruit == 1 && bg_tex == 2)
                {
                    out_list.Add(seed);
                    Console.WriteLine($"Valid seed: 0x{seed:X8}");
                }
            }
        }

        public static async void DEBUG_FindFQRandSeedsWhichResultInSeed(uint seed)
        {
            DEBUG_FilterDownPreTownSeeds(await ILGPULogic.ExecuteSeedSearchKernel(seed));
            return;

            List<uint> seeds = new();
            const double max = uint.MaxValue + 1L;

            Parallel.For(0x77300000L, 0x77400000L, x =>
            {
                InitialTownLogic gen = new(null);

                gen.__qrand_idum = (uint)x;
                for (int i = 0; i < 500; i++)
                {
                    gen.fqrand_noret();
                }

                uint current_seed = gen.__qrand_idum;
                for (int i = 500; i < 5000; i++)
                {
                    if (current_seed == seed)
                    {
                        Console.WriteLine($"Found possible initial seed: {x:X8} ({i} total fqrand calls)");
                        break;
                    }

                    current_seed = gen.fqrand_noret();
                }
            });
        }
    }
}
