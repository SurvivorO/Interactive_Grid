# Grid-Based Cube Movement System

A Unity 3D grid-based interaction system developed as a technical assessment. The project demonstrates configurable grid generation, cell selection, cube spawning, data-driven cube variants, and coordinated grid movement with boundary and collision handling.

## Features

- **Configurable Grid:** Generate a grid with adjustable width, height, and cell spacing through the Unity Inspector.
- **Cell Selection:** Select individual cells using the mouse, with visual feedback for the selected cell.
- **Cube Spawning:** Spawn cubes on selected cells while preventing multiple cubes from occupying the same cell.
- **Data-Driven Cube Variants:** Define cube types using ScriptableObjects, supporting custom names, colors, and materials.
- **Runtime Type Selection:** Cycle through configured cube types using the mouse scroll wheel.
- **Coordinated Movement:** Move all eligible cubes one grid cell at a time using WASD or arrow keys.
- **Boundary Handling:** Prevent cubes from moving outside the grid.
- **Collision Resolution:** Resolve movement before updating cube positions and cell occupancy, preventing overlapping occupancy and allowing adjacent cubes to move together when space is available.
- **Responsive UI:** Display the currently selected cube type and the available controls.

## Technology Stack

- Unity 6 — version 6.6.5f1
- C#
- Unity Input System
- TextMeshPro
- ScriptableObjects for configurable cube data

No external art assets are required for the core mechanics; the grid and cubes use Unity primitives.

## Controls

| Input | Action |
|---|---|
| Left Mouse Button | Select a grid cell |
| Space | Spawn the selected cube type |
| Mouse Scroll Wheel | Cycle through configured cube types |
| W / Up Arrow | Move cubes toward increasing grid Z |
| S / Down Arrow | Move cubes toward decreasing grid Z |
| D / Right Arrow | Move cubes toward increasing grid X |
| A / Left Arrow | Move cubes toward decreasing grid X |

Cubes move one grid cell per movement input. Cubes that cannot move because of grid boundaries or blocked destinations remain stationary.

## Project Setup

### Requirements

- Unity Hub
- Unity Editor 6.6.5f1
- Unity Input System package
- TextMeshPro resources

### Opening the Project

1. Clone or download the repository.
2. Open Unity Hub.
3. Select **Add project from disk** and choose the project directory.
4. Open the project using Unity 6.6.5f1.
5. Open the main gameplay scene `'InteractiveGrid'` from `Assets/Scenes/`.
6. Verify the Inspector references on the GridManager, including the cell prefab, cube prefab, camera, UI reference, and cube type assets.
7. Enter Play mode to test the system.

If Unity prompts you to import TextMeshPro Essential Resources, import them before running the scene.

## Architecture

The project separates grid management, cell state, cube behavior, input handling, configuration data, and UI presentation.

### GridManager

Responsible for:
- Generating the grid from Inspector-configured dimensions.
- Maintaining a two-dimensional array of grid cells.
- Handling selection, spawning, and movement coordination.
- Tracking cube destinations and enforcing grid boundaries.
- Resolving movement conflicts before updating occupancy and transforms.

Movement uses a two-phase approach: calculate and resolve destinations first, then clear old occupancy and assign new cells before updating cube positions.

### GridCell

Represents an individual grid cell. Stores its grid coordinates, selection state, and optional occupying cube. Provides methods to update selection visuals and manage occupancy.

### CubeController

Manages a cube's current cell, grid coordinates, and assigned cube type. Applies the configured appearance and updates the cube's world position when it moves.

### GridInputReader

Uses Unity's Input System and a generated Input Actions wrapper. Exposes input events to other components, keeping input handling separate from grid gameplay logic.

### CubeTypeData

A ScriptableObject that defines a cube type's name, color, and optional material. New variants can be created in the Inspector without modifying the spawning or movement logic.

### GridUI

Updates the selected cube type indicator and provides visual guidance for the available controls.

## Configuration

### Grid

Select the GridManager GameObject and configure:
- Width
- Height
- Cell size
- Cell prefab
- Cube prefab
- Camera reference
- Cube type assets
- UI reference

### Cube Variants

Create assets through **Assets → Create → Grid Assessment → Cube Type**. Configure each asset's name, color, and optional material, then assign the assets to the GridManager's cube type list.

Unassigned entries are skipped during type cycling. At least one valid cube type must be configured to spawn cubes.

## Assumptions and Design Decisions

- Grid coordinates use X and Z for the two horizontal axes; the grid's Y coordinate maps to world Z.
- Each movement input requests a single-cell movement rather than continuous movement.
- All eligible cubes respond to the same movement direction.
- Cubes that would leave the grid remain stationary.
- A cube may enter a cell only if the movement resolution determines that the destination will be vacated.
- Cube appearance is configured through reusable ScriptableObject assets.
- The project prioritizes clear component responsibilities and deterministic occupancy management over introducing additional architectural layers.


## Third-Party Assets and Dependencies

- **Unity Input System:** Used for action-based input handling.
- **TextMeshPro:** Used for UI text rendering.
- **Unity built-in primitives:** Used for the grid cells and cubes.

No additional third-party art assets or external services are required.


## Gameplay Screenshot


<p align="center">
  <img src="Screenshots/gameplay.png" alt="Grid-based cube movement gameplay" width="800">
</p>
