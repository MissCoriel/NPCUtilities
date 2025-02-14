using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using Newtonsoft.Json;
using xTile;
using StardewValley.GameData.Locations;
using StardewValley.GameData.Characters;
using StardewValley.TokenizableStrings;
using Microsoft.Xna.Framework.Audio;

namespace NPCToolKit
{
    internal class ModEntry : Mod
    {

        /// <summary>
        /// This mod is a suplemental program for NPC Creator and general asset acquisition.
        /// The scope is to acquire lists of valid IDs of most common used things in Stardew Valley.
        /// This mod will also help acquire Coordinates and Measure distance for easier Move Commands and Schedules.
        /// </summary>
        public Dictionary<string, string> ItemList = new Dictionary<string, string>();
        public List<string> coords = new List<string>();
        private ConfigModel Config = null!;
        public override void Entry(IModHelper helper)
        {
            helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
            helper.ConsoleCommands.Add("create", "usage: `create <input>` possible inputs: itemlist, maplist, npclist, musiclist, all", CreateCommand);
            helper.ConsoleCommands.Add("get", "usage: `get <input>` possible inputs: location", GetCommand);
            this.Config = helper.ReadConfig<ConfigModel>();
            if(!File.Exists(Path.Combine(Environment.CurrentDirectory, "ExtractedData")))
            {
                Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "ExtractedData"));
                Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "ExtractedData", "Coordinates"));
            }
        }
        public void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
        {
            if (this.Config.GrabLocation.JustPressed())
                GetCoordinates();
            if (this.Config.GetAllInfo.JustPressed())
            {
                ItemExtractor();
                MapExtractor();
                NPCRoster();
                MusicID();
            }
        }
        public void GetCommand(string command, string[] parameters)
        {
            if (parameters[0] != null)
            {
                if (parameters[0] == "location")
                {
                    GetCoordinates();
                }
            }
        }
        public void CreateCommand(string command, string[] parameters)
        {
            ///Console commands to extract data from the game.  It is Mod Aware so ids will be pulled from modded assets.
            if (parameters[0] != null)
            {
                if (parameters[0] == "itemlist")
                {
                    ItemExtractor();
                }
                if (parameters[0] == "maplist")
                {
                    MapExtractor();
                }
                if (parameters[0] == "npclist")
                {
                    NPCRoster();
                }
                if(parameters[0] == "audiolist")
                {
                    MusicID();
                }
                if (parameters[0] == "all") //This is a faster way to acquire all IDs in the scope.
                {
                    ItemExtractor();
                    MapExtractor();
                    NPCRoster();
                    MusicID();
                }

            }
        }
        public void ItemExtractor()
        {

            foreach (IItemDataDefinition itemType in ItemRegistry.ItemTypes)
            {
                foreach (string id in itemType.GetAllIds())
                {
                    Item item = ItemRegistry.Create(itemType.Identifier + id);
                    if (item.canBeGivenAsGift())
                    {
                        try
                        {
                            ItemList.TryAdd(item.DisplayName, id);
                        }
                        catch (Exception ex)
                        {
                            Monitor.Log($"{ex.Message}\nSkipping {item.DisplayName}", LogLevel.Warn);
                        }
                    }
                }
            }
            string path = Path.Combine(Environment.CurrentDirectory, "ExtractedData", "ItemList.json");
            File.WriteAllText(path, JsonConvert.SerializeObject(ItemList, new JsonSerializerSettings { Formatting = Formatting.Indented, NullValueHandling = NullValueHandling.Ignore }));
            Monitor.Log("ItemList saved!", LogLevel.Debug);
        }
        public void MapExtractor()
        {
            List<string> MapList = new List<string>();
            List<string> Filter = new List<string> { "Default", "Farm_Standard", "Farm_Beach", "Farm_Forest", "Farm_FourCorners", "Farm_Hilltop", "Farm_Riverland", "Farm_Wilderness", "Farm_MeadowlandsFarm", "BeachNightMarket", "DesertFestival", "fishingGame", "Temp" };
            foreach (var mapinfo in DataLoader.Locations(Game1.content))
            {
                MapList.Add(mapinfo.Key);
            }
            foreach (string mapFilter in Filter)
            {
                MapList.Remove(mapFilter);
            }
            string path = Path.Combine(Environment.CurrentDirectory, "ExtractedData", "MapList.txt");
            File.WriteAllLines(path, MapList);
            Monitor.Log("MapList saved!", LogLevel.Debug);

        }
        public void NPCRoster()
        {
            Dictionary<string, string> datablenpc = new Dictionary<string, string>();
            Dictionary<string, string> nondatablenpc = new Dictionary<string, string>();
            foreach (var npc in DataLoader.Characters(Game1.content))
            {
                if (npc.Value.CanBeRomanced == true)
                    datablenpc.Add(npc.Key, TokenParser.ParseText(npc.Value.DisplayName));
                if (npc.Value.CanBeRomanced == false)
                    nondatablenpc.Add(npc.Key, TokenParser.ParseText(npc.Value.DisplayName));
            }
            string patha = Path.Combine(Environment.CurrentDirectory, "ExtractedData", "NPCMarriagable.json");
            File.WriteAllText(patha, JsonConvert.SerializeObject(datablenpc, new JsonSerializerSettings { Formatting = Formatting.Indented, NullValueHandling = NullValueHandling.Ignore }));
            string pathb = Path.Combine(Environment.CurrentDirectory, "ExtractedData", "NPCNonMarriageable.json");
            File.WriteAllText(pathb, JsonConvert.SerializeObject(nondatablenpc, new JsonSerializerSettings { Formatting = Formatting.Indented, NullValueHandling = NullValueHandling.Ignore }));

            Monitor.Log("NPCList saved!", LogLevel.Debug);

        }
        public void MusicID()
        {
            string pathmusic = Path.Combine(Environment.CurrentDirectory, "ExtractedData", "MusicList.txt");
            string pathsfx = Path.Combine(Environment.CurrentDirectory, "ExtractedData", "SFXList.txt");
            string pathambient = Path.Combine(Environment.CurrentDirectory, "ExtractedData", "AmbientList.txt");
            List<string> musiclist = new List<string>();
            List<string> soundlist = new List<string>();
            List<string> ambientlist = new List<string>();
            var soundBank = this.Helper.Reflection.GetField<SoundBank>(Game1.soundBank, "soundBank", true).GetValue();
            var cues = this.Helper.Reflection.GetField<Dictionary<string, CueDefinition>>(soundBank, "_cues", true).GetValue();
            var sortedCues = cues.Values.OrderBy(cue => cue.name).ToDictionary(cue => cue.name, cue => string.Join(',', cue.sounds.Select(sound => sound.categoryID).Distinct()));
            // 2 is music 3 is SFX 4 is ambient
            foreach (var def in sortedCues)
            {
                string parseValue = def.Value.ToString();
                if (string.Equals(parseValue, "2"))
                {
                    musiclist.Add(def.Key.ToString());
                }
                if (string.Equals(parseValue, "3"))
                {
                    soundlist.Add(def.Key.ToString());
                }
                if (string.Equals(parseValue, "4"))
                {
                    ambientlist.Add(def.Key.ToString());
                }

            }
            File.WriteAllLines(pathmusic, musiclist);
            File.WriteAllLines(pathsfx, soundlist);
            File.WriteAllLines(pathambient, ambientlist);
            Monitor.Log("Audio Lists saved!", LogLevel.Debug);
        }
        public void GetCoordinates()
        {
            string currentMap = Game1.currentLocation.NameOrUniqueName;
            bool mapData = Game1.currentLocation.ShouldExcludeFromNpcPathfinding();
            if (mapData == true)
            {
                Monitor.Log("This location is not valid for NPCs.. Ignoring", LogLevel.Warn);
            }
            if (mapData == false)
            {
                LoadData();
                int mapCoordX = (int)Game1.player.Tile.X;
                int mapCoordY = (int)Game1.player.Tile.Y;
                int mapCoordF = Game1.player.getFacingDirection();
                string locationdata = $"{currentMap} {mapCoordX} {mapCoordY} {mapCoordF}";
                coords.Add(locationdata);
                Monitor.Log($"{locationdata} added!", LogLevel.Debug);
                SaveData();
            }
        }
        public void SaveData()
        {
            string path = Path.Combine(Environment.CurrentDirectory, "ExtractedData", "Coordinates", "MapCoordinates.txt");
            File.WriteAllLines(path, coords);
            coords.Clear();
        }
        public void LoadData()
        {
            string path = Path.Combine(Environment.CurrentDirectory, "ExtractedData", "Coordinates", "MapCoordinates.txt");
            if (File.Exists(path))
            {
                coords.AddRange(File.ReadAllLines(path));
            }
        }
    }
}
