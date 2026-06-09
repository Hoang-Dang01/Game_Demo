using System.Collections.Generic;

namespace CSharpGame
{
    public static class RecipeDatabase
    {
        public static List<Recipe> Recipes { get; } = new();

        static RecipeDatabase()
        {
            // Placeholder: Add default recipes for Crafting
            // Example:
            // Recipes.Add(new Recipe("Kiếm Rèn Sắt", new() { new("Mảnh Sắt", 3) }, new Item { Name = "Kiếm Rèn Sắt", SlotType = "Weapon", Damage = 25 }));
        }
    }
}
