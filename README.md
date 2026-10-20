# Vertigo Games - Game Developer Demo
### Wheel of Fortune (Critical Strike Card Game Mechanic)

A highly polished, mobile-optimized **Wheel of Fortune** demo built with **Unity 6 (6000.3.23f1)** for Vertigo Games. The game re-imagines the Critical Strike "Card Game" risk-reward mechanic into a dynamic, data-driven spinning wheel experience with multi-tier zones, scalable rewards, revival mechanics, and complete multi-aspect mobile UI support.

---

## 🎮 Gameplay Overview & Rules

The game is a high-stakes gambling experience where the player balances escalating rewards against the ever-present threat of the bomb:

- **Core Mechanic:** The player spins a customized wheel instead of selecting cards.
- **Multiple Zones & Progression:**
  - **Normal Zones (Bronze Wheel):** Includes high-value rewards and a Bomb slice. Landing on the bomb forfeits all session rewards unless revived.
  - **Safe Zones (Every 5th Zone - Silver Wheel):** Risk-free spin with zero bombs and upgraded rewards.
  - **Super Zones (Every 30th Zone - Golden Wheel):** High-tier spin with legendary weapon and currency rewards, zero bombs.
  - **Zone Multiplier:** Rewards scale dynamically as the player advances (x1, x2, x3... up to x60+), reflected in real time on the wheel slices and inventory payouts.
- **Cash Out / Exit Rule:** The player can only walk away with collected rewards when the wheel is **Idle** and currently at a **Safe Zone** or **Super Zone**.
- **Bomb & Revive System:**
  - Hitting a bomb triggers the explosion modal.
  - Players can **Give Up** (forfeiting current session inventory and resetting the run) or **Revive** using Banked Gold (cost scales progressively).
- **Cash Out & Victory Sequences:**
  - Completing Zone 60 triggers the celebratory **Mission Complete** sequence.
  - Cashing out transitions into a multi-phase **Chest Opening & Card Reveal** animation, followed by a final rewards summary grid.

---

## 📱 Deliverables & Links

| Deliverable | Link / Path | Description |
| :--- | :--- | :--- |
| **Android APK Release** | [GitHub Releases](../../releases) | Working `.apk` build built with Unity 6000.3.23f1 |
| **Gameplay Video** | [Watch Gameplay Video](#) *(Insert YouTube/Drive Link)* | Complete playthrough demonstrating Zones, Safe/Super zones, Revive, and Cash Out |
| **Screenshots (20:9)** | [`Screenshots/20_9/`](./Screenshots/20_9/) | Ultra-wide mobile aspect ratio (2400x1080) |
| **Screenshots (16:9)** | [`Screenshots/16_9/`](./Screenshots/16_9/) | Standard widescreen mobile aspect ratio (1920x1080) |
| **Screenshots (4:3)** | [`Screenshots/4_3/`](./Screenshots/4_3/) | Tablet / iPad aspect ratio (1440x1080) |

---

## 📸 Screenshots & Aspect Ratio Previews

The UI hierarchy is designed with **Canvas Scaler in Expand Mode**, anchor-pivot calibration, and 9-sliced sprites to guarantee pixel-perfect presentation across varying mobile aspect ratios (**20:9**, **16:9**, and **4:3**).

---

### 1. Normal Zone (Bronze Wheel & Bomb)
*Standard progression zone featuring 8 slices (including Bomb), live reward multiplier, and disabled cash out.*

| 20:9 (2400x1080) | 16:9 (1920x1080) | 4:3 (1440x1080) |
| :---: | :---: | :---: |
| <img width="2400" height="1080" alt="Capture_20261020_003138_20_9" src="https://github.com/user-attachments/assets/c4d25ddf-6401-45a4-b390-5a9d632f2f39" /> | <img width="1920" height="1080" alt="Capture_20261020_003138_16_9" src="https://github.com/user-attachments/assets/d702bbd8-4f4a-4a1e-9ce5-d270bace6e13" /> | <img width="1440" height="1080" alt="Capture_20261020_003138_4_3" src="https://github.com/user-attachments/assets/587c5176-45db-4fd6-8ad2-2607117a1194" /> |


---

### 2. Safe Zone (Silver Wheel - Risk-Free)
*Occurs every 5th zone. Features upgraded silver wheel, zero bomb risk, and active Cash Out (Exit) button.*

| 20:9 (2400x1080) | 16:9 (1920x1080) | 4:3 (1440x1080) |
| :---: | :---: | :---: |
| <img width="2400" height="1080" alt="Capture_20261020_003015_20_9" src="https://github.com/user-attachments/assets/b160e5e1-1583-4452-b80a-f57392bc490d" /> | <img width="1920" height="1080" alt="Capture_20261020_003015_16_9" src="https://github.com/user-attachments/assets/f5ee94b5-78ba-46f9-8cf8-da223c9d31bf" /> | <img width="1440" height="1080" alt="Capture_20261020_003015_4_3" src="https://github.com/user-attachments/assets/42182ffc-24e7-40cc-ba04-ff46d740a629" /> |


---

### 3. Super Zone (Golden Wheel - Milestone Rewards)
*Occurs every 30th zone. Features luxurious golden wheel, zero bomb risk, rare weapon drops, and active Cash Out.*

| 20:9 (2400x1080) | 16:9 (1920x1080) | 4:3 (1440x1080) |
| :---: | :---: | :---: |
| <img width="2400" height="1080" alt="Capture_20261020_003203_20_9" src="https://github.com/user-attachments/assets/c93d6c72-0bb7-43a8-a816-e62d89a761df" /> |<img width="1920" height="1080" alt="Capture_20261020_003203_16_9" src="https://github.com/user-attachments/assets/8c5093ea-3c7a-4034-a65d-99393bb2f069" /> | <img width="1440" height="1080" alt="Capture_20261020_003203_4_3" src="https://github.com/user-attachments/assets/37a92af8-0695-4272-b2d5-ec9aea28c4d8" /> |

---


### 4. Bomb Exploded (Revive & Give Up Modal)
*Explosion modal presenting the choice between forfeiting rewards (Give Up) or reviving with Gold.*

| 20:9 (2400x1080) | 16:9 (1920x1080) | 4:3 (1440x1080) |
| :---: | :---: | :---: |
| <img width="2400" height="1080" alt="Capture_20261020_003108_20_9" src="https://github.com/user-attachments/assets/9be0a5c8-edc7-42f4-9ca6-eaceea391510" /> | <img width="1920" height="1080" alt="Capture_20261020_003108_16_9" src="https://github.com/user-attachments/assets/790b2f48-17fa-4335-9cfa-d797867f7b14" /> | <img width="1440" height="1080" alt="Capture_20261020_003108_4_3" src="https://github.com/user-attachments/assets/5aaff2f5-e53a-48d7-a846-f939617af743" /> |


---

### 5. Cash Out & Rewards Summary
*End-of-run reward reveal sequence displaying collected cards, quantities, and bank synchronization.*

| 20:9 (2400x1080) | 16:9 (1920x1080) | 4:3 (1440x1080) |
| :---: | :---: | :---: |
| <img width="2400" height="1080" alt="Capture_20261020_003216_20_9" src="https://github.com/user-attachments/assets/65929d8c-1aa4-4c27-ac2b-bda694953491" /> | <img width="1920" height="1080" alt="Capture_20261020_003216_16_9" src="https://github.com/user-attachments/assets/6b1b04b2-b07f-469a-aeee-b4ff00266a26" /> | <img width="1440" height="1080" alt="Capture_20261020_003216_4_3" src="https://github.com/user-attachments/assets/e969c236-ab07-4094-82ed-e8681d3662e3" /> |


---

## 🏗️ Software Architecture & Design Patterns

The codebase strictly adheres to **SOLID principles**, mobile performance best practices, and clean code standards:

```
[GameCore]
   ├── GameManager (Finite State Machine: Idle, Spinning, Rewarding, Bomb, CashOut)
   ├── ZoneManager (Config-driven zone progression & reward scaling)
   ├── WheelController (Physics-less deterministic wheel simulation & easing)
   ├── SessionInventoryManager (Session bag & atomic banking bridge)
   └── AudioManager (Sound effects, dynamic pitch modulation)
```

### 1. Observer Pattern (`GameEvents`)
All systems communicate through a strongly typed, static event bus (`GameEvents.cs`). Views and controllers are completely decoupled:
- The UI listens to `OnZoneChanged`, `OnInventoryUpdated`, `OnGameStateChanged`, etc.
- No direct component coupling exists between the wheel mechanics, zone progress, and UI presentation.

### 2. Finite State Machine (`GameState`)
`GameManager.cs` orchestrates gameplay states:
`Idle` ➔ `Spinning` ➔ `Rewarding` ➔ `BombHit` / `ZoneTransition` ➔ `CashingOut` ➔ `MissionComplete`.
Prevents race conditions, double clicks, and unwanted player interactions during animations.

### 3. Data-Driven Architecture (ScriptableObjects)
All game content is configurable from the Unity Inspector without recompiling:
- `WheelConfigSO`: Defines slice count, sprites, reward assignments, base amounts, and drop weights.
- `RewardDataSO`: Defines item identity, localization, icons, valuable flags, and bomb status.
- `ZoneManagerConfigSO`: Controls safe zone intervals, super zone intervals, wheel overrides, and multiplier steps.

### 4. Lightweight JSON Save Infrastructure (`SaveService`)
Replaced `PlayerPrefs` with a clean, atomic JSON serialization service:
- Model: `PlayerSaveData` (DTO) storing Gold, Cash, and banked items.
- Disk location: `Application.persistentDataPath/player_save.json`.
- Automatic one-time migration: Seamlessly migrates existing legacy PlayerPrefs data on first startup.
- Default wallet: 100 starting Gold.

---

## 🛠️ Testing & Debugging Suite

A dedicated `TestManager` and custom Inspector (`TestManagerEditor`) are included:
- **Target Zone Jump:** Instantly jump to any zone (1–60) with automatic wheel and zone bar synchronization.
- **Quick Presets:** Instant shortcuts for *Zone 1 (Start)*, *Zone 5 (Safe Zone)*, and *Zone 30 (Super Zone)*.
- **Wallet Controls:** Quick buttons to add +100 Gold or Reset Wallet to default (100 Gold).
- **Bomb Controls:** Toggles to force bomb or disable bomb for deterministic QA flows.
- **Unity Menu Shortcuts:**
  - `Vertigo > Save System > Reset Save Data (Default 100 Gold)`
  - `Vertigo > Save System > Open Save Folder`

---

## 📁 Project Structure

```
Assets/_Project/
├── Art/
│   ├── Atlases/         # VertigoDemoAtlas (Sprite Atlas v2)
│   ├── Audio/           # High quality sound effects (Spin, Win, Bomb, Chest, etc.)
│   ├── Fonts/           # TextMeshPro font assets & materials
│   └── Sprites/         # UI sliced borders, wheel bases, reward icons, cards
├── Data/
│   ├── Audio/           # AudioConfig ScriptableObjects
│   ├── Rewards/         # RewardDataSO definitions (Gold, Cash, Weapons, Bomb)
│   ├── Wheels/          # Bronze, Silver, and Golden WheelConfigSO assets
│   └── Zones/           # ZoneManagerConfigSO progression rules
├── Prefabs/
│   └── UI/              # Reusable reward cards & component prefabs
├── Scenes/
│   └── MainGame.unity   # Core production scene
└── Scripts/
    ├── Editor/          # Custom Inspectors & automation utilities
    └── Runtime/
        ├── Audio/       # Audio manager & SFX triggers
        ├── Core/        # Interfaces, Enums, GameEvents, Singleton, SaveService
        ├── Inventory/   # Session inventory & wallet controllers
        ├── Rewards/     # Reward data models & slot views
        ├── Testing/     # In-game QA / TestManager
        ├── UI/          # Popups, card views, summary screens
        ├── Wheel/       # Wheel controller, slice views, rotation physics
        └── Zone/        # Zone manager, strip view, progression logic
```

---

### Developed for Vertigo Games Game Developer Demo Evaluation.
**Unity Version:** `6000.3.23f1`
