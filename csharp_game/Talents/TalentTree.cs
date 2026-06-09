using System.Collections.Generic;

namespace CSharpGame
{
    public class TalentNode
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int CurrentLevel { get; set; } = 0;
        public int MaxLevel { get; set; } = 5;
        public string BonusType { get; set; } // e.g. "Strength", "CritChance"
        public float BonusPerLevel { get; set; }

        public TalentNode(string id, string name, int maxLevel, string bonusType, float bonusVal)
        {
            Id = id;
            Name = name;
            MaxLevel = maxLevel;
            BonusType = bonusType;
            BonusPerLevel = bonusVal;
        }
    }

    public class TalentTree
    {
        public List<TalentNode> Nodes { get; } = new();

        public TalentTree()
        {
            // Initial placeholders
            Nodes.Add(new TalentNode("t1", "Sức Mạnh Bền Bỉ", 5, "Strength", 2f));
            Nodes.Add(new TalentNode("t2", "Cơ Hồi Sát Thủ", 5, "CritChance", 0.02f));
        }
    }
}
