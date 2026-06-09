namespace CSharpGame
{
    public class TalentManager
    {
        public TalentTree Tree { get; set; } = new();
        public int UnspentTalentPoints { get; set; } = 0;

        public bool UpgradeTalent(string nodeId)
        {
            if (UnspentTalentPoints <= 0) return false;

            var node = Tree.Nodes.Find(n => n.Id == nodeId);
            if (node == null || node.CurrentLevel >= node.MaxLevel) return false;

            node.CurrentLevel++;
            UnspentTalentPoints--;
            return true;
        }
    }
}
