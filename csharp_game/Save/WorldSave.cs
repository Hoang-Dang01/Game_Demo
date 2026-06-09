namespace CSharpGame
{
    public static class WorldSave
    {
        public static void Save(int currentFloor, int highestFloorReached, SaveData data)
        {
            data.CurrentFloor = currentFloor;
            data.HighestFloorReached = highestFloorReached;
        }

        public static void Load(out int currentFloor, out int highestFloorReached, SaveData data)
        {
            currentFloor = data.CurrentFloor;
            highestFloorReached = data.HighestFloorReached;
        }
    }
}
