using System;
using System.IO;
using System.Text.Json;

namespace CSharpGame
{
    public static class SaveManager
    {
        private const string SaveFileName = "save.json";

        public static void SaveGame(Player player, int currentFloor, int highestFloorReached,
                                    SanctuaryManager sanctuary, PortalDestination returnPortal)
        {
            try
            {
                SaveData data = new SaveData();
                PlayerSave.Save(player, data);
                WorldSave.Save(currentFloor, highestFloorReached, data);
                
                // Copy return portal state
                data.ReturnPortal.Floor = returnPortal.Floor;
                data.ReturnPortal.Seed = returnPortal.Seed;
                data.ReturnPortal.X = returnPortal.X;
                data.ReturnPortal.Y = returnPortal.Y;
                data.ReturnPortal.IsActive = returnPortal.IsActive;

                SanctuarySave.Save(data, sanctuary);

                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(data, options);
                File.WriteAllText(SaveFileName, json);
                Console.WriteLine("[SAVE MANAGER] Game saved successfully.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"[SAVE MANAGER] Error saving game: {e.Message}");
            }
        }

        public static bool LoadGame(Player player, SanctuaryManager sanctuary,
                                    PortalSystem portal, out int currentFloor, out int highestFloorReached)
        {
            currentFloor = 0; // default to sanctuary on fresh load
            highestFloorReached = 1;
            try
            {
                if (!File.Exists(SaveFileName))
                {
                    Console.WriteLine("[SAVE MANAGER] No save file found.");
                    return false;
                }

                string json = File.ReadAllText(SaveFileName);
                SaveData? data = JsonSerializer.Deserialize<SaveData>(json);

                if (data == null) return false;

                PlayerSave.Load(player, data);
                WorldSave.Load(out currentFloor, out highestFloorReached, data);
                SanctuarySave.Load(data, sanctuary);
                portal.LoadFromSaveData(data);

                player.Hp = player.MaxHp;
                player.Mp = player.MaxMp;

                Console.WriteLine("[SAVE MANAGER] Save file loaded successfully.");
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"[SAVE MANAGER] Error loading game: {e.Message}");
                return false;
            }
        }
    }
}
