# Tutorial verification

Open `Assets/Scenes/Official levels/Tutorial Level.unity` and choose **Tools > Tutorial > Run Flow Checks** outside Play Mode. The scene is already configured; no Inspector setup is needed.

The check enters Play Mode, runs the four rooms twice, and stops Play Mode afterwards. It uses the saved scene, actual physics triggers, and the buttons' pointer-click callbacks. Results and UI captures are written under `Temp/TutorialChecks/`.

Covered behavior:

- Room 1 starts with one stationary placed mosquito and no spawned mosquitoes.
- Rooms cannot be skipped before completion.
- Rooms 2–4 spawn one mosquito that flies toward its target.
- Target arrival deactivates the mosquito, explains the failed attempt, and spawns one replacement after the retry delay.
- Window exits complete all four rooms; the actual zapper trigger also completes room 4.
- Completed rooms stop spawning and release the cursor while disabling movement and item interaction.
- The Next Room buttons advance one room and restore control.
- The final Main Menu and Play Final Level buttons are reachable through the UI raycaster and load their respective scenes with the correct cursor state.
- No new runtime errors occur during the checks.

For a manual playthrough, use **WASD** to move, **E** to pick up/place an item, the **mouse wheel** to rotate its placement, and **Esc** to pause. Place the room's fan(s) to push mosquitoes through the window. In room 4, UV attraction and the zapper provide the other solution. A held item is returned to its pickup position when continuing to another room.

The automated checks move mosquitoes into triggers to exercise outcomes; they do not simulate a player solving each room through item placement. They validate Editor Play Mode, not a standalone player build.
