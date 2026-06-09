namespace CSharpGame
{
    /// <summary>
    /// Blacksmith NPC — the first and only NPC in Sanctuary v1.
    ///
    /// v1: displays a placeholder UI panel when player presses E nearby.
    /// v2 (Phase 3): connects to CraftingManager for real recipe logic.
    /// v3 (Phase 4): connects to UpgradeManager for +1/+2/+3 upgrades.
    /// </summary>
    public class Blacksmith
    {
        public bool UIOpen { get; private set; } = false;

        // Active tab in the UI: 0 = Craft, 1 = Upgrade
        public int ActiveTab { get; private set; } = 0;

        public void OpenUI()  => UIOpen = true;
        public void CloseUI() => UIOpen = false;
        public void ToggleUI() => UIOpen = !UIOpen;
        public void SetTab(int tab) => ActiveTab = tab;
    }
}
