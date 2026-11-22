# Unity project context

Analyzed 2026-11-22 on `tutorial-fixes` (working tree includes existing scene edits).

- Unity 6000.3.19f1; URP 17.3; Input System 1.19 (`activeInputHandler: 1`); uGUI 2 / TextMesh Pro; AI Navigation 2.0.13. Confirmed in ProjectSettings and Packages.
- Single player, MonoBehaviour components in `Assets/Scripts`, compiled into Assembly-CSharp. Starter Assets has its own runtime/editor assemblies. No first-party test assemblies existed at inspection; Unity Test Framework 1.6 is installed.
- Build order: TitleScene, MainMenuScene, FinalLevel, Tutorial Level. `GameManager` persists between scenes; scene managers own tutorial lessons / final-level waves. Standalone tutorial completion loads configured build scenes directly.
- `MosquitoManager` owns active agents and pooling; `MosquitoSpawner` chooses NavMesh positions and sends each agent to its manager's `pointB`. `MosquitoAgent` handles navigation, flight, temporary attraction, and fan forces. `MosquitoHealth`, `TargetHealth`, and `Window` report outcomes. Keep pool ownership intact when removing mosquitoes.
- `Tutorial_SM` owns four ordered lessons, one active mosquito, retry timing, objectives and completion choices. Room 1 uses a placed idle mosquito; rooms 2–4 spawn. Final room accepts a kill or window exit. Room arrays use the same lesson order.
- `PlayerInteraction` / `PickupItem` own item placement. `PauseMenuController` owns pause and cursor handling; tutorial completion temporarily disables it and player movement/interaction. UI uses serialized Canvas/TMP references and button callbacks.
- Conventions: camelCase fields, serialized private references, no runtime namespaces, XML comments for public intent. Prefer direct components/events over additional managers.
- Unity Editor is available through Windows UI. CLI installed, but Pipeline package is absent and no Unity MCP is connected. Generated solution can provide fallback compilation; Play Mode is needed for trigger/UI verification.
- Evidence: `ProjectSettings/ProjectVersion.txt`, `ProjectSettings/ProjectSettings.asset`, `ProjectSettings/EditorBuildSettings.asset`, `Packages/manifest.json`, `Assets/Scripts`, `Assets/Scenes/Official levels/Tutorial Level.unity`.
