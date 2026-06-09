using System.Collections.Generic;

namespace CSharpGame
{
    public class Recipe
    {
        public string Name { get; set; } = "Công Thức";
        public List<ItemMaterialRequirement> RequiredMaterials { get; set; } = new();
        public Item ResultItem { get; set; }

        public Recipe(string name, List<ItemMaterialRequirement> requirements, Item result)
        {
            Name = name;
            RequiredMaterials = requirements;
            ResultItem = result;
        }
    }

    public class ItemMaterialRequirement
    {
        public string MaterialName { get; set; }
        public int Count { get; set; }

        public ItemMaterialRequirement(string name, int count)
        {
            MaterialName = name;
            Count = count;
        }
    }
}
