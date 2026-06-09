namespace CSharpGame
{
    public class FloorManager
    {
        public int CurrentFloor { get; set; } = 0; // Floor 0 = Sanctuary Hub

        public void ChangeFloor(int delta)
        {
            CurrentFloor += delta;
        }

        public bool IsBossFloor()
        {
            return CurrentFloor > 0 && CurrentFloor % 5 == 0;
        }
    }
}
