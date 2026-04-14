# Yes Chef! — Unity Dev Test Implementation

## Folder Structure

```
Assets/
├── Prefabs/
│   ├── Vegetable_Ingredient.prefab
│   ├── Cheese_Ingredient.prefab
│   └── Meat_Ingredient.prefab
│
├── ScriptableObjects/
│   ├── IngredientRegistry.asset
│   └── Ingredients/
│       ├── VegetableData.asset
│       ├── CheeseData.asset
│       └── MeatData.asset
│
└── Scripts/
    ├── Core/
    │   └── GameManager.cs          — Game state machine (MainMenu/Playing/Paused/GameOver)
    ├── Player/
    │   └── PlayerController.cs     — WASD movement, E-interact, hold 1 item, FSM
    ├── Stations/
    │   ├── BaseStation.cs          — Abstract IInteractable base
    │   ├── Refrigerator.cs         — Spawns raw ingredients
    │   ├── Table.cs                — Chops vegetables (2 sec, coroutine)
    │   ├── Stove.cs                — Cooks meat (6 sec, 2 async slots)
    │   ├── Trash.cs                — Destroys held item
    │   └── CustomerWindow.cs       — Accepts prepared ingredients, validates orders
    ├── Ingredients/
    │   ├── IngredientData.cs       — ScriptableObject config per ingredient type
    │   ├── IngredientRegistry.cs   — Catalogue SO, used by Fridge & OrderManager
    │   └── Ingredient.cs           — Runtime MonoBehaviour on each physical ingredient
    ├── Orders/
    │   └── OrderSystem.cs          — Order data class + OrderManager lifecycle
    ├── Systems/
    │   ├── TimerSystem.cs          — 3-min countdown, fires OnTimerExpired
    │   └── ScoreSystem.cs          — Score accumulation + PlayerPrefs high score
    ├── UI/
    │   ├── UIManager.cs            — All screen/HUD wiring, subscribes to all events
    │   ├── OrderWindowUI.cs        — Per-window ingredient slots + elapsed timer
    │   └── FloatingScorePopup.cs   — Pooled "+score" popup that floats and fades
    ├── Interfaces/
    │   └── IInteractable.cs        — Interface: Interact(player) + GetInteractionPrompt()
    └── Editor/
        ├── SceneBuilder.cs         — Menu: YesChef > Build Scene (primitives kitchen)
        ├── IngredientPrefabSetup.cs — Menu: YesChef > Create Ingredient Assets
        └── UISetupHelper.cs        — Menu: YesChef > Build UI (full Canvas hierarchy)
```

---

## Step-by-Step Setup in Unity

### Prerequisites
- Unity 2020.3 LTS or newer (2020.3.16f1 recommended)
- TextMeshPro package (comes with Unity — import Essential Resources if prompted)

---

### Step 1 — Create the Project

1. Open Unity Hub → **New Project** → **3D Core** template
2. Name it `YesChef`
3. Click **Create Project**

---

### Step 2 — Import Scripts

Copy the entire `Assets/Scripts/` folder from this repo into your Unity project's `Assets/` folder.
Unity will compile all scripts automatically.

> If you see errors about `TMPro` namespace: **Window → TextMeshPro → Import TMP Essential Resources**

---

### Step 3 — Create the Interactable Layer

1. **Edit → Project Settings → Tags and Layers**
2. Under **Layers**, find an empty slot (e.g. User Layer 6) and name it `Interactable`
3. This layer is used by `PlayerController`'s overlap-sphere to find stations

---

### Step 4 — Create Ingredient Assets

1. In Unity's top menu: **YesChef → Create Ingredient Assets**
2. This generates:
   - `Assets/ScriptableObjects/Ingredients/VegetableData.asset`
   - `Assets/ScriptableObjects/Ingredients/CheeseData.asset`
   - `Assets/ScriptableObjects/Ingredients/MeatData.asset`
   - `Assets/ScriptableObjects/IngredientRegistry.asset`
   - `Assets/Prefabs/Vegetable_Ingredient.prefab` (green sphere)
   - `Assets/Prefabs/Cheese_Ingredient.prefab` (yellow sphere)
   - `Assets/Prefabs/Meat_Ingredient.prefab` (red sphere)

---

### Step 5 — Build the Scene

1. Open a **new empty scene** (File → New Scene → Basic Built-In)
2. **YesChef → Build Scene**
3. This auto-creates:
   - Floor, 4 walls (grey cubes)
   - Top-down Camera (60° FOV, positioned at y=18)
   - All 5 station types as coloured cubes
   - Player capsule (yellow)
   - GameSystems GameObject with all system components
   - Directional light

---

### Step 6 — Build the UI

1. **YesChef → Build UI**
2. This creates a full Canvas with:
   - Start Screen (title + controls + start button)
   - HUD (score, high score, timer, interaction prompt, station progress bars, 4 order panels)
   - Pause Screen
   - Game Over Screen (final score, new high score banner, restart/quit)

---

### Step 7 — Wire Inspector References

After building, you need to assign references in the Inspector. Here's the checklist:

#### GameSystems GameObject

| Component       | Field               | Assign                                |
|-----------------|---------------------|---------------------------------------|
| `GameManager`   | Timer System        | GameSystems (TimerSystem component)   |
| `GameManager`   | Score System        | GameSystems (ScoreSystem component)   |
| `GameManager`   | Order Manager       | GameSystems (OrderManager component)  |
| `GameManager`   | Player Controller   | Player GameObject                     |
| `OrderManager`  | Ingredient Registry | `ScriptableObjects/IngredientRegistry`|

#### Refrigerator Station

| Field                | Assign                              |
|----------------------|-------------------------------------|
| Ingredient Registry  | `ScriptableObjects/IngredientRegistry` |
| Ingredient Prefab    | Any of the 3 ingredient prefabs     |

> **Tip**: Make the Refrigerator output a *random* prefab by using a custom Prefab registry,
> OR use a single generic prefab and let `Ingredient.Initialise(data)` set the colour.
> The simplest approach: assign `Vegetable_Ingredient.prefab` and rely on runtime colour.
> For a proper random-type fridge, update `Refrigerator.Interact` to pick a random prefab
> from a `List<GameObject>` field and call `Initialise` with matching data.

#### UI_Canvas → UIManager

| Field                    | Assign                                        |
|--------------------------|-----------------------------------------------|
| Start Screen             | `StartScreen` panel                           |
| HUD Screen               | `HUD` GameObject                              |
| Pause Screen             | `PauseScreen` panel                           |
| Game Over Screen         | `GameOverScreen` panel                        |
| Score Text               | `HUD/ScoreText`                               |
| High Score Text          | `HUD/HighScoreText`                           |
| Timer Text               | `HUD/TimerText`                               |
| Interaction Prompt Text  | `HUD/InteractionPrompt`                       |
| Held Item Text           | `HUD/HeldItemText`                            |
| Final Score Text         | `GameOverScreen/FinalScoreText`               |
| New High Score Text      | `GameOverScreen/NewHighScoreText`             |
| Order Window UIs [0-3]   | `HUD/OrderWindow_0` through `OrderWindow_3`   |
| Chop Progress Slider     | Slider inside `HUD/ChopProgressPanel`         |
| Chop Progress Panel      | `HUD/ChopProgressPanel`                       |
| Stove Slot Sliders [0-1] | Sliders inside `StoveSlot0Panel/StoveSlot1Panel` |
| Stove Slot Panels [0-1]  | `HUD/StoveSlot0Panel`, `HUD/StoveSlot1Panel`  |
| Floating Score Prefab    | Create a simple prefab with `FloatingScorePopup` + TMP_Text |
| Popup Canvas             | `UI_Canvas` transform                         |

#### Button onClick Wiring (in each Button's Inspector)

| Button              | Target     | Method                         |
|---------------------|------------|--------------------------------|
| StartButton         | UIManager  | `OnStartButtonClicked()`       |
| PauseButton         | UIManager  | `OnPauseButtonClicked()`       |
| ResumeButton        | UIManager  | `OnResumeButtonClicked()`      |
| RestartButton (×2)  | UIManager  | `OnRestartButtonClicked()`     |
| QuitButton (×2)     | UIManager  | `OnQuitButtonClicked()`        |

#### OrderWindowUI Components (4 panels)

For each `HUD/OrderWindow_N`:

| Field                        | Assign                                     |
|------------------------------|--------------------------------------------|
| Order Timer Text             | `OrderWindow_N/TimerText`                  |
| No Order Text                | `OrderWindow_N/NoOrderText`                |
| Ingredient Slot Container    | `OrderWindow_N/IngredientSlots`            |
| Ingredient Slot Prefab       | A simple prefab: Panel + Image + TMP_Text  |

---

### Step 8 — Create the Ingredient Slot Prefab

1. Right-click in Hierarchy → **UI → Panel** (ensure it's a child of a temporary Canvas)
2. Add **TMP_Text** child inside the Panel
3. Set the Panel size to ~140 × 30
4. Add an `Image` component to the panel (for colour coding)
5. Drag to `Assets/Prefabs/` as `IngredientSlotUI.prefab`
6. Delete from Hierarchy
7. Assign to all 4 `OrderWindowUI.ingredientSlotPrefab` fields

---

### Step 9 — Create the Floating Score Prefab

1. Right-click in Hierarchy → **UI → Text - TextMeshPro** (inside Canvas)
2. Name it `FloatingScorePopup`
3. Add the `FloatingScorePopup` component
4. Assign the TMP_Text child to its `label` field
5. Drag to `Assets/Prefabs/FloatingScorePopup.prefab`
6. Delete from scene
7. Assign to `UIManager.floatingScorePrefab`

---

### Step 10 — Final Checks

- [ ] All station GameObjects are on the **Interactable** layer
- [ ] `PlayerController.interactionMask` includes the **Interactable** layer
- [ ] `OrderManager.ingredientRegistry` is assigned
- [ ] `Refrigerator.ingredientRegistry` and `.ingredientPrefab` are assigned
- [ ] All 4 `CustomerWindow` components have correct `windowIndex` (0–3)
- [ ] All Button `onClick` events are wired
- [ ] All `OrderWindowUI` fields are assigned

---

### Step 11 — Play!

Hit **Play**. The Start Screen appears. Click **Start Game** to begin.

---

## Architecture Notes

### Design Patterns Used

- **Event-driven decoupling**: Systems communicate exclusively via `static event Action<>` delegates. No direct component references between unrelated systems.
- **ScriptableObject data**: `IngredientData` and `IngredientRegistry` are pure data assets, making ingredient tuning easy without touching code.
- **Interface segregation**: `IInteractable` is the only contract the `PlayerController` needs — new station types require zero changes to the player.
- **Object pooling**: `FloatingScorePopup` instances are pre-allocated and recycled, avoiding GC spikes during gameplay.
- **Lightweight FSM**: `PlayerController` tracks `Idle/Moving/Interacting` states — easy to extend with animation hooks.
- **Coroutines for async ops**: Chopping and cooking use coroutines for clean, readable time-based logic without Update() complexity.
- **SOLID compliance**:
  - *Single Responsibility*: Each script has one job (e.g. `TimerSystem` only counts time)
  - *Open/Closed*: Add new stations by subclassing `BaseStation` with zero changes elsewhere
  - *Liskov*: All stations are substitutable via `IInteractable`
  - *Interface Segregation*: `IInteractable` is minimal — only two methods
  - *Dependency Inversion*: High-level `GameManager` depends on abstractions (events), not concrete station types

### Scoring Formula

```
score = sum(ingredient.scoreValue) - floor(secondsSinceOrderSpawned)
```

Score can go negative. The game accumulates all order scores into `ScoreSystem.CurrentScore`.

### Order Logic

Orders are generated with 50/50 chance of 2 or 3 ingredients. Each ingredient slot is independently random from the registry — duplicates are possible (e.g. Meat + Meat + Meat).

### High Score Persistence

`ScoreSystem` uses `PlayerPrefs.SetInt("YesChef_HighScore", value)` — persists between editor sessions and builds.

---

## Extending the Game

- **New ingredient type**: Add entry to `IngredientType` enum, create `IngredientData` asset, add prefab, register in `IngredientRegistry`.
- **New station**: Create a class extending `BaseStation`, override `Interact()`. No other changes needed.
- **Difficulty scaling**: Add a `DifficultySystem` that modifies `OrderManager.respawnDelay` and `TimerSystem.gameDuration` over time.
- **Animations**: Hook into `PlayerController.State` transitions and `Table/Stove` events for Animator `SetTrigger` calls.
- **Sound**: Subscribe to `Table.OnChopComplete`, `Stove.OnSlotComplete`, `OrderManager.OnOrderCompleted` events and play AudioClips.

---

## Controls Reference (shown on Start Screen)

| Key       | Action            |
|-----------|-------------------|
| WASD      | Move player       |
| E         | Interact          |
| Escape    | Pause/Resume      |
