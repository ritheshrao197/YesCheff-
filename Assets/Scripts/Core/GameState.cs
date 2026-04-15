// GameState.cs
// Central orchestrator. Drives the high-level game state machine:
// MainMenu → Playing → Paused → GameOver → (restart)

namespace YesChef.Core
{
    public enum GameState { MainMenu, Playing, Paused, GameOver }
}
