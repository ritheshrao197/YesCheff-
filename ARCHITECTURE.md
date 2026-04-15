# YesCheff! Architecture Document

This document outlines the software architecture, design patterns, and structural decisions behind the **YesCheff!** Unity project.

## 1. High-Level Architecture Overview

YesCheff! is designed with a heavy emphasis on **modularity**, **decoupling**, and **data-driven design**. 

The core architecture follows an **Event-Driven Architecture (EDA)** pattern. Systems and UI components rarely hold direct references to each other. Instead, they communicate by raising and subscribing to global events. This ensures that adding new features, UI panels, or gameplay mechanics has minimal impact on existing code.

### Core Pillars
- **Event-Driven Communication:** Facilitated by a central static `GameEvents` class.
- **Data-Driven Configuration:** Utilizing Unity `ScriptableObject` assets to separate game balancing (ingredient types, recipes, player speeds) from logic.
- **Interface Segregation:** Ensuring interacting systems rely on small, specific contracts (e.g., `IInteractable`).
- **State Machine Gameplay Loop:** Managed centrally by the `GameManager`.

---

## 2. Directory Structure and Modules

The `Assets/Scripts` directory is divided into discrete namespaces/modules to prevent "spaghetti code."

- `Core/`
  - Contains the `GameManager` (State Machine) and `GameEvents` (Event Bus).
- `Player/`
  - `PlayerController` and `PlayerInteractionSensor`. Handles input, movement, and resolving overlaps with interactables.
- `Stations/`
  - Implements the different kitchen modules: `Refrigerator`, `Table`, `Stove`, `Trash`, `CustomerWindow`. All inherit from `BaseStation` or implement `IInteractable`.
- `Ingredients/`
  - Defines what an ingredient is. Includes runtime MonoBehaviours (`Ingredient.cs`) and ScriptableObject data containers (`IngredientData.cs`, `IngredientRegistry.cs`).
- `Orders/`
  - Handles the procedural generation of orders (`OrderManager`) and the definition of what an order requires (`Order`).
- `Systems/`
  - Standalone managers for specific mechanics: `TimerSystem`, `ScoreSystem`, and `SaveSystem`.
- `UI/`
  - Listens to `GameEvents` and updates the Canvas elements (HUD, Menus, Score Popups). Uses Managers like `UIManager` and `StationProgressManager`.

---

## 3. Key Design Patterns

### 3.1 Event Bus (Event-Driven Architecture)
**File:** `Core/GameEvents.cs`
- **Concept:** A static class containing `event Action` delegates.
- **Usage:** When the player chops a vegetable, the `Table` station raises `GameEvents.RaiseChopProgressChanged`. The `StationProgressManager` in the UI namespace listens to this event and updates the progress bar slider. The `Table` knows nothing about the UI, and the UI knows nothing about the `Table` directly.

### 3.2 State Machine
**File:** `Core/GameManager.cs`
- **Concept:** The game operates in distinct states: `Start`, `Playing`, `Paused`, `End`.
- **Usage:** The `GameManager` handles transitions between these states (e.g., pausing time, disabling player input, resetting scores). Systems subscribe to `GameStateChanged` to react accordingly (e.g., UI showing the Pause menu).

### 3.3 Strategy / Interface Segregation
**File:** `Stations/BaseStation.cs` and `Interfaces/IInteractable.cs`
- **Concept:** The player does not need to know what a "Stove" is. It only needs to know it is interacting with an `IInteractable`.
- **Usage:** `PlayerController` casts a physics sphere to find `IInteractable` objects. When the player presses 'Interact', the `PlayerController` calls `interactable.Interact(this)`. The station implements the specific logic (e.g., returning raw food, taking raw food and starting a cooking coroutine).

### 3.4 Data-Driven Design (ScriptableObjects)
**Files:** `Ingredients/IngredientData.cs`, `Ingredients/IngredientRegistry.cs`
- **Concept:** Avoiding hardcoded strings, scores, and lists in MonoBehaviours.
- **Usage:** Game designers can create a new `IngredientData` asset (e.g., "Tomato"), assign it a UI sprite, a base score, and a cooked state. The `IngredientRegistry` holds a list of all valid ingredients. The `OrderManager` reads from this registry to generate random orders without touching any C# scripts.

### 3.5 Singleton / Static Utility (with extreme caution)
- Core managers are intentionally **not** Singletons (no `public static GameManager Instance`). They are linked via serialized fields in the root `GameSystems` prefab or communicate via `GameEvents`. 
- The `SaveSystem` and `GameEvents` are static utility classes because they hold no instance-specific state that changes per scene reload.

---

## 4. System Deep Dives

### 4.1 Order Generation and Fulfillment
1. **Generation:** `OrderManager` routinely spawns `Order` instances if a `CustomerWindow` is empty. It queries the `IngredientRegistry` for random `IngredientData`.
2. **Display:** UI listens to `GameEvents.OrderAssigned` and renders the required icons.
3. **Fulfillment:** Player interacts with `CustomerWindow` while holding an `Ingredient`. The window checks if the ingredient matches any pending items in the `Order`. If it matches, the item is consumed, and the UI updates. If all items match, the order is fulfilled, and `GameEvents.DeliveryScored` is raised.

### 4.2 Local Save System and Security
**File:** `Systems/SaveSystem.cs`
To prevent players from easily modifying their high score in a text file, YesCheff! implements a secure save system.
- **Serialization:** Uses `Newtonsoft.Json` to serialize a `SaveContainer` class.
- **Encryption:** If enabled via `EncryptionConfig.asset` (a ScriptableObject), the JSON string is encrypted using **AES (Advanced Encryption Standard)** before being written to disk (`gameData.rdx`). The decryption keys (`KeyBytes` and `IVBytes`) are stored securely within the compiled asset.

### 4.3 Asynchronous Actions (Coroutines)
- Cooking meat on a `Stove` or chopping vegetables on a `Table` takes time.
- Rather than cluttering `Update()` loops with timers, stations use Unity `Coroutines`. This encapsulates the timed logic neatly and allows for easy pausing (by yielding to `Time.timeScale`).

---

## 5. Adding New Content

Because of the decoupled architecture, scaling the game is straightforward:

- **To add a new Station:** Create `MyNewStation.cs` inheriting from `BaseStation`. Implement `Interact()`. Place the prefab in the scene on the `Interactable` layer. The Player and UI will automatically handle it.
- **To add a new UI Screen:** Create a new Canvas Panel, write a small manager script that subscribes to `GameEvents.GameStateChanged`, and show/hide the panel based on the state.
- **To add a new Ingredient:** Right-click in the Project view, create a new `IngredientData` ScriptableObject, and add it to the `IngredientRegistry`. The `OrderManager` and `Refrigerator` will immediately begin using it.
