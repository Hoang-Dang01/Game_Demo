using System.Collections.Generic;

namespace CSharpGame
{
    /// <summary>
    /// Handles serialisation of Sanctuary-specific state:
    /// storage chest contents, portal return coordinates,
    /// and Portal Stone inventory count.
    /// </summary>
    public static class SanctuarySave
    {
        public static void Save(SaveData data, SanctuaryManager sanctuary)
        {
            data.StorageItems = new List<Item>(sanctuary.StorageItems);
            data.PortalStoneCount = sanctuary.PortalStoneCount;
        }

        public static void Load(SaveData data, SanctuaryManager sanctuary)
        {
            sanctuary.StorageItems    = new List<Item>(data.StorageItems);
            sanctuary.PortalStoneCount = data.PortalStoneCount;
        }
    }
}
