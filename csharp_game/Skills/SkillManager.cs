using System.Collections.Generic;

namespace CSharpGame
{
    public class SkillManager
    {
        public List<SkillData> LearnedSkills { get; set; } = new();
        public SkillData? ActiveSkill { get; set; }

        public bool UseSkill(Player player, SkillData skill)
        {
            if (player.Mp < skill.ManaCost) return false;
            
            player.Mp -= skill.ManaCost;
            // Execute skill actions
            return true;
        }
    }
}
