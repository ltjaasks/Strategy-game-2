# Strategy Game Prototype

The game a is tick-based chess inspired strategy game with real-time-tactics elements. Unit movement commands are resolved simultaneously 
and all units moving in a tick move at the same time. Capturing works the same way as in chess. Unit moving onto a stationary unit will capture it.
There is no hp/power system. Simultaneous movement causes some complications and edge cases for capturing that are explained in "resolve tick" section.

Game also has vision mechanics and fog-of-war that hides the tiles/units that ally units have no vision on.

Game ends when either teams "commander" unit is captured.

---

## 🎥 Gameplay Video

[Watch the 3-minute demo here](https://www.youtube.com/watch?v=dIkq_JMOpDs)

---

## 📌 Overview

This project is a strategy game prototype developed in Unity.  
The focus of this project was on:

- Working, clear and fair movement system
- Clear visual feedback to the player
- Working simultaneous movement system
- Fully custom-made assets
- Clear architecture
- Separation of responsibilities

The goal was to design a well working system and not polish visuals for the prototype.

---

## 🛠 Tech Stack

- Unity 6.2 (URP)
- C#
- Shader Graph
- Git (branch-based workflow)

---

## Core Systems

### 1. Tick-Based Movement System
The game was designed with a tick-based movement system because I want the game to be faster paced than chess and have a 
real-time-strategy game feel with chess rules. My vision is to have a large battlefield as a game board in the final version of the game.
The challenge of the game will come from player having to manage multiple units and threats while being pressured by the enemy. With this solution I
can make the game easy to get into, easy to understand but balance the games difficulty with pacing, different types of units, modifiers etc.

#### Breakdown of a game tick
TickManager calls it's Tick() method with certain intervals. Tick in turn calls methods ResolveTick(), MoveEnemies(), UpdateVision() and UpdateFog in
that order. Main logic of a tick happens in TickResolver.cs ResolveTick().

#### Resolve Tick
TickResolver.cs is the most interesting class of the game and quite complex.
Explained shortly it first collects all the intended moves of units then resolves the moves and finally 
applies the move results by calling appropriate methods in each units Unit.cs.

#### Summary of movement resolution:
- Moves are resolved into a Dictionary<Unit, MoveResult>
- First results dictionary is initialized
  - units who intend to wait are marked as "Allowed"
  - other units are marked as "Unresolved"
- Next a loop goes through the results dictionary until no changes are made to it
- On each pass of the loop NextMoves dictionary containing units intended next moves is inspected
- From next moves all units contending for a tile are collected and those units are evaluated to determine one or none that may enter the tile
  - if only one contender -> move is allowed
  - if two contenders -> if neither is attacking cancel both moves (unintentional attacks are not allowed)
  - if two conteders -> if one is attacking -> attacking unit get the tile, other unit is destroyed
  - if three or more contenders -> all moves cancelled
- Resolver also has to check for current occupants and their team to determine movement outcome
  - if occupant moves -> move is allowed
  - if occupant stays -> check if attacking -> attacking -> move allowed, otherwise move cancelled (attack has to be intentional
 
Movement resolution is the most complex system of the game and most worth looking into in this game.


### 2. Fog of War
Game has a fog of war system to incentivise capturing high priority positions in a future version of the game. In later stages
I will implement terrain with high points and forests to make the gameplay more interesting.

Currently all units have a vision range of 3. Vision range is calculated using Manhattan distance. Vision is tracked on each unit and
for each team so future implementation of AI would not have unfair information. The fog is updated each tick after unit movement.

Visually fog is implemented using two separate meshes. A quad placed on top of the tile to more clearly indicate non-visible tiles and
an irregularly shaped rotating blob to create illusion of moving fog.

### 4. Pathfinding
Pathfinding is implemented as a Breadth First Search algorithm. Priority of chosen tiles in cases of equal distance paths is handled
by the order of tiles returned by UnitsData.cs. The algorithm fetches allowed movement tiles for certain unit type from a certain position
and calculates the path. The architecture where UnitsData.cs contains movement tiles per unit from whatever position given as a parameter
allows concentrating pathfinding to one class and scalability by easy way of adding new units and modifying existing units.

### 5. Enemy AI
Current enemy AI in this prototype version is very simple. The "AI" captures if possible and moves units randomly if captures are not possible.
Future implementation will also evaluate board control, vision control, commander safety and high value positions.

---

## Architecture
### Core
#### FogController
- Animates the fog meshes. Sets meshes at a random rotation and rotates them slowly after that.
#### GameManager
- Manages the game state. Ends game when conditions are met.
#### GameState
- Enum for gamestate.
#### GridManager
- Manages the grid. Helper methods to get tiles etc.
#### PathFinder
- Finds path for unit to a target tile. Written in a way so pathfinding is not dependant on the unit type.
#### TickManager
- Runs ticks on a given interval. Runs the tick flow.
#### TickResolver
- Resolves movement during a tick. Calls apply action methods on units when movements are resolved.
#### VisionManager
- Manages each teams vision. Calculates and updates vision of each team every tick.

### Units
#### ActionType
- Enum for unit move type.
#### MoveResult
- Enum for move result. Used in movement resolution.
#### Unit
- Represents unit on the board. Contain relevant data like units grid position and logic for movement. Registered on units manager on creation.
#### UnitAnimator
- Controls animating unit depending on it's current action. Idle, attack and move implemented.
#### UnitMover
- Moves unit model in world space accordingly to game logic.
#### UnitsData
- Contains data of all unit types. Units fetch data from here on creation. E.g vision range, movement cooldown
#### UnitsManager
- Basically contains current board state. Position of all units etc.
#### UnitState
- Enum for unit state.
#### UnitType
- Enum for unit type.
#### UnitView
- Currently obsolete. Relic of old solution but kept around for possible future need.

### Input
#### CameraController
- Controls for camera movement.
#### HighlightType
- Enum for highlight type.
#### PlayerCommandController
- Handles player input like selecting units and hovering tiles.
#### PointerRayCaster
- Tracks player mouse on screen.
#### TileHighlighter
- Concentrated highlighting for visual effects.

