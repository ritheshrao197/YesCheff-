// File: Assets/Scripts/Log/GameLogCategory.cs
// Enum for categorising game logs. Used by GameLogger to prefix messages and enable/disable specific categories.
namespace YesChef.Core
{
    public enum GameLogCategory
    {
        Game,
        Player,
        Stations,
        Orders,
        Timer,
        Score,
        Ingredients
    }
}
