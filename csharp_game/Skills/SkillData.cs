namespace CSharpGame
{
    public class SkillData
    {
        public string Name { get; set; } = "Kỹ Năng";
        public string Description { get; set; } = "Mô tả kỹ năng";
        public float Cooldown { get; set; } = 1.0f; // in seconds
        public float ManaCost { get; set; } = 10f;
        public float DamagePower { get; set; } = 1.0f; // scaling factor

        public SkillData(string name, string desc, float cd, float mc, float power)
        {
            Name = name;
            Description = desc;
            Cooldown = cd;
            ManaCost = mc;
            DamagePower = power;
        }
    }
}
