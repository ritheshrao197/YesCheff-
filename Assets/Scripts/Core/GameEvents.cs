using System;
using YesChef.Ingredients;
using YesChef.Orders;
using YesChef.Stations;

namespace YesChef.Core
{
    public static class GameEvents
    {
        public static event Action<GameState> GameStateChanged;

        public static event Action<float> TimerTicked;
        public static event Action TimerExpired;

        public static event Action<int> ScoreChanged;
        public static event Action<int> HighScoreChanged;
        public static event Action<bool, int> GameEnded;

        public static event Action<Ingredient> PlayerPickedUpIngredient;
        public static event Action PlayerDroppedIngredient;
        public static event Action<string> InteractionPromptChanged;
        public static event Action InteractionPromptCleared;

        public static event Action<Order, int> OrderAssigned;
        public static event Action<Order, int> OrderCompleted;
        public static event Action<int> OrderWindowCleared;
        public static event Action<Order, int> OrderUpdated;
        public static event Action<int, int> DeliveryScored;

        public static event Action<Table> ChopStarted;
        public static event Action<Table, float> ChopProgressChanged;
        public static event Action<Table> ChopCompleted;

        public static event Action<Stove, int> StoveSlotStarted;
        public static event Action<Stove, int, float> StoveSlotProgressChanged;
        public static event Action<Stove, int> StoveSlotCompleted;

        public static void RaiseGameStateChanged(GameState state) => GameStateChanged?.Invoke(state);
        public static void RaiseTimerTicked(float remaining) => TimerTicked?.Invoke(remaining);
        public static void RaiseTimerExpired() => TimerExpired?.Invoke();
        public static void RaiseScoreChanged(int score) => ScoreChanged?.Invoke(score);
        public static void RaiseHighScoreChanged(int score) => HighScoreChanged?.Invoke(score);
        public static void RaiseGameEnded(bool isNewHighScore, int finalScore) => GameEnded?.Invoke(isNewHighScore, finalScore);
        public static void RaisePlayerPickedUpIngredient(Ingredient ingredient) => PlayerPickedUpIngredient?.Invoke(ingredient);
        public static void RaisePlayerDroppedIngredient() => PlayerDroppedIngredient?.Invoke();
        public static void RaiseInteractionPromptChanged(string prompt) => InteractionPromptChanged?.Invoke(prompt);
        public static void RaiseInteractionPromptCleared() => InteractionPromptCleared?.Invoke();
        public static void RaiseOrderAssigned(Order order, int windowIndex) => OrderAssigned?.Invoke(order, windowIndex);
        public static void RaiseOrderCompleted(Order order, int score) => OrderCompleted?.Invoke(order, score);
        public static void RaiseOrderWindowCleared(int windowIndex) => OrderWindowCleared?.Invoke(windowIndex);
        public static void RaiseOrderUpdated(Order order, int windowIndex) => OrderUpdated?.Invoke(order, windowIndex);
        public static void RaiseDeliveryScored(int windowIndex, int score) => DeliveryScored?.Invoke(windowIndex, score);
        public static void RaiseChopStarted(Table table) => ChopStarted?.Invoke(table);
        public static void RaiseChopProgressChanged(Table table, float progress) => ChopProgressChanged?.Invoke(table, progress);
        public static void RaiseChopCompleted(Table table) => ChopCompleted?.Invoke(table);
        public static void RaiseStoveSlotStarted(Stove stove, int slotIndex) => StoveSlotStarted?.Invoke(stove, slotIndex);
        public static void RaiseStoveSlotProgressChanged(Stove stove, int slotIndex, float progress) => StoveSlotProgressChanged?.Invoke(stove, slotIndex, progress);
        public static void RaiseStoveSlotCompleted(Stove stove, int slotIndex) => StoveSlotCompleted?.Invoke(stove, slotIndex);
    }
}
