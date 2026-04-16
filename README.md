# YesCheff!

**YesCheff!** is a fast-paced 3D Unity cooking game where players move around a kitchen, grab ingredients, prepare them at various stations, and fulfill customer orders within a time limit to achieve the highest score.

## 🎮 Key Features

- **Interactive Kitchen Stations**:
  - **Refrigerator**: Spawns raw ingredients based on a customizable catalog.
  - **Table**: Chop and prepare vegetables over time.
  - **Stove**: Cook meat using an asynchronous slot system.
  - **Customer Window**: Deliver completed ingredient combinations to fulfill randomized orders.
  - **Trash**: Discard unwanted or burnt items.
- **Dynamic Order System**: Customers request randomized sets of ingredients. Complete them quickly for a higher score!
- **Data-Driven Design**: Heavy use of Unity `ScriptableObjects` (`IngredientData`, `GameConfig`, `PlayerConfig`) for easy balancing and adding new content without touching code.
- **Secure Save System**: Uses an AES-encrypted local save system (`SaveSystem.cs` and `EncryptionConfig.asset`) to securely store high scores.
- **Polished UI**: Floating score popups, dynamic progress bars for stations, and responsive order HUDs.

## 🏗️ Project Architecture

The codebase is built with modularity and maintainability in mind:

- **Event-Driven Architecture**: Core systems and UI elements communicate through a central `GameEvents` module, eliminating hard dependencies.
- **State Machine**: The `GameManager` controls the main game loop (Menu, Playing, Paused, GameOver).
- **Interface Segregation**: The player interacts with all stations through a universal `IInteractable` interface, making it trivial to add new station types.

### 📂 Directory Structure

```text
Assets/
├── Scripts/
│   ├── Core/         — Game loop (GameManager), game state, and global GameEvents.
│   ├── Player/       — Player movement, inputs, and the Interaction Sensor.
│   ├── Stations/     — Logic for Refrigerator, Table, Stove, Trash, and CustomerWindow.
│   ├── Ingredients/  — Ingredient runtime behaviors and ScriptableObject definitions.
│   ├── Orders/       — Order generation and lifecycle management.
│   ├── Systems/      — Core systems like ScoreSystem, TimerSystem, and SaveSystem.
│   └── UI/           — HUD management, floating text, screen transitions, and order windows.
├── ScriptableObjects/— Pre-configured data assets for ingredients, config, and encryption.
├── Prefabs/          — Reusable objects for ingredients, stations, and UI.
└── Scenes/           — Contains the main `GamePlay.unity` scene.
```

## 🚀 Getting Started

### Prerequisites
- **Unity 2020.3 LTS** (or newer recommended)
- **TextMesh Pro** (Import Essential Resources via Window → TextMeshPro if prompted)
- **Newtonsoft.Json** (Used for serialization in the SaveSystem)

### Running the Game
1. Open Unity Hub and add the `YesCheff!` folder as a project.
2. Navigate to `Assets/Scenes/` and open `GamePlay.unity`.
3. Press **Play** in the Unity Editor.
4. Click **Start Game** on the main menu.

### Controls
- **WASD / Arrow Keys**: Move the player.
- **E**: Interact with stations (grab ingredients, place on table/stove, serve).
- **Escape**: Pause the game.

## 🛠️ Extending the Game

- **Add New Ingredients**: Create a new `IngredientData` ScriptableObject, make a prefab, and register it in the `IngredientRegistry`.
- **Add New Stations**: Create a new script inheriting from `BaseStation.cs` and implement the `Interact()` method. No changes to the `PlayerController` are required.
