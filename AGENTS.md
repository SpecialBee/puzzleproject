# Repository Guidelines

## Project Structure & Module Organization

This is a Unity 2022.3.62f3 project. Core game code lives in `Assets/Scripts`: `Battle/` contains board, tile, manager, item, skill, and monster logic, while `MainMenu/` contains stage/menu scripts. Scenes are in `Assets/Scenes` (`MainMenu`, `StageSelect`, `Battle`). Runtime prefabs and data assets live in `Assets/Prefab`, `Assets/TileDatas`, `Assets/Item,skill datas`, and `Assets/MonterGimmick`. Art and imported UI assets are in `Assets/Sprite`, `Assets/Material`, `Assets/TextMesh Pro`, and `Assets/_GUI Pro-FantasyRPG`. Dependencies and editor settings are tracked in `Packages/` and `ProjectSettings/`.

## Unity 2D Architecture Rules

- All manager classes must use the singleton pattern with Awake() duplicate check.
  Pattern: `if (Instance == null) Instance = this; else { Destroy(gameObject); return; }`
- Do not use DontDestroyOnLoad except in DataManager.
- ScriptableObject assets are preferred over hardcoded data for TileData, ItemData, SkillData, and GimmickData.
- Never call FindObjectOfType<> at runtime. Always reference via singleton Instance or Inspector field.
- UI logic must be separated from game logic. Manager classes must not hold references to UI GameObjects directly; delegate to a dedicated UI class (e.g. RewardUI, PlayerUI).

## Scene Lifecycle

| Scene       | Managers present                                              |
|-------------|---------------------------------------------------------------|
| MainMenu    | DataManager (DontDestroyOnLoad)                               |
| StageSelect | DataManager                                                   |
| Battle      | GridManager, HandManager, MonsterManager, PlayerManager,      |
|             | RewardManager, RewardUI, SkillManager, InventoryManager       |

DataManager persists across all scenes. All other managers are destroyed and recreated on scene load.
When writing scene-specific logic, assume other-scene managers are null and guard with null checks.

## Prohibited Patterns

- Do NOT use `GetComponent<>` or `GetComponentInChildren<>` to find Button components at runtime for event binding. Bind onClick immediately after `Instantiate()`.
- Do NOT use `GameObject.Find()` or `FindObjectOfType<>`.
- Do NOT place UI GameObject arrays (e.g. `rewardButtons[]`) in manager classes. UI arrays belong in dedicated UI classes.
- Do NOT add game logic inside `Update()` unless it requires per-frame polling. Use event-driven calls instead.
- Do NOT use coroutines for simple delayed calls under 0.5s; prefer direct calls with WaitForSeconds only when animation sync is needed.

## Script Placement Rules

| Folder                        | Contents                                      |
|-------------------------------|-----------------------------------------------|
| Assets/Scripts/Battle/        | All battle scene MonoBehaviour and data classes|
| Assets/Scripts/MainMenu/      | Stage select and main menu scripts            |
| Assets/Scripts/UI/            | Dedicated UI controller classes (e.g. RewardUI)|
| Assets/Tests/EditMode/        | Edit mode unit tests (*Tests.cs)              |
| Assets/Tests/PlayMode/        | Play mode integration tests (*Tests.cs)       |

When creating a new UI controller, place it in Assets/Scripts/UI/.

## Known Unimplemented Features (Do Not Implement Unless Instructed)

The following SkillEffect cases in SkillManager.cs are intentionally stubbed:
- MakeJoker, ManaShield, Heal, DirectDamage, SkipMonsterTurn, CleanseBoard

Do not implement these unless explicitly requested. Current behavior (mana refund + warning log) is intentional.

## Build, Test, and Development Commands

Open the project with Unity Hub, or from PowerShell:

```powershell
& "C:\Program Files\Unity\Hub\Editor\2022.3.62f3\Editor\Unity.exe" -projectPath .
```

Run Unity Test Framework suites in batch mode:

```powershell
& "C:\Program Files\Unity\Hub\Editor\2022.3.62f3\Editor\Unity.exe" -batchmode -projectPath . -runTests -testPlatform EditMode -testResults Logs\EditModeTests.xml -quit
& "C:\Program Files\Unity\Hub\Editor\2022.3.62f3\Editor\Unity.exe" -batchmode -projectPath . -runTests -testPlatform PlayMode -testResults Logs\PlayModeTests.xml -quit
```

No custom build script is currently present; create builds through Unity `File > Build Settings` unless a documented CI build method is added.

## Coding Style & Naming Conventions

Use 4-space indentation and keep one C# class per file, matching file and class names, for example `GridManager.cs` and `TileData.cs`. Use PascalCase for types, methods, enums, and Unity event methods. Use camelCase for inspector-facing fields, consistent with existing scripts such as `tileSlotPrefab`, `gridParent`, and `statValue`. Prefer `[Header]`, `[Tooltip]`, and `ScriptableObject` assets for designer-facing data. Keep project scripts under `Assets/Scripts` and avoid modifying imported package code unless the change is intentional.

## Testing Guidelines

`com.unity.test-framework` is installed, but no `Assets/Tests` folder exists yet. Add Edit Mode tests under `Assets/Tests/EditMode` and Play Mode tests under `Assets/Tests/PlayMode`. Name test files `*Tests.cs`. Prioritize coverage for grid placement, bingo detection, skill/item effects, manager state changes, and scene lifecycle behavior.

## Commit & Pull Request Guidelines

Recent history uses short date-style commits such as `04/24` and `0430`, plus revert commits. Prefer clearer imperative messages going forward, such as `Add tile lock gimmick` or `Fix reward reroll state`. Pull requests should include a short summary, affected scenes/assets, manual test notes, linked issues, and screenshots or clips for UI or gameplay changes. Always commit Unity `.meta` files when adding, moving, or renaming assets.

## Agent-Specific Instructions

Do not edit or commit regenerated folders such as `Library/`, `Logs/`, `obj/`, `.vs/`, or `UserSettings/`. Preserve Unity asset GUIDs; move or rename assets in the Unity Editor when possible.
