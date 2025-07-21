# :crystal_ball: Minecraft4Unity
A minimal and very optimized version of Minecraft made in Unity, virtually endless in all three axis.  It features procedural world generation with greedy meshing, data persistence and an inventory system.

![minecraft4unity-title](https://github.com/paternostrox/Minecraft4Unity/assets/19597048/40c69361-4ced-4745-8e24-b096923101d0)


## :fire: Basic setup:

Just clone the project and open it in Unity version 2022.3.7f1 LTS or newer, it should also run in any 2020+ version with no issues.
All the packages needed for the project will import automatically when opening it for the first time.

## :desktop_computer: Code description

### Procedural generation
While exploring the world, the game generates land and objects around the player position. For performance reasons, the game world is divided in chunks, like in the original game. Each chunk goes through the following process:

> 3D Simplex Noise -> Block Data Generation -> Mesh & Collider Generation (Greedy Meshing) -> Object Spawning

The basis for the procedural generation is a simplex noise function that is interpreted by the system. The system attributes different voxel types (e.g. air, stone, brick) for different value ranges. The block data is then used to generate the chunk meshes and colliders. The collider data is then used to spawn non-voxel objects (e.g. characters, items) in the chunk, which are placed randomly on top of ground voxels. An atlas shader takes care of texturing the chunk according to the UV data built by the mesh generation. The Unity job system is used in the most compute-intensive tasks, like in mesh and collider generation.

![procgen3](https://github.com/paternostrox/Minecraft4Unity/assets/19597048/918fb99a-21ce-438d-8ec7-507f97e68ad6)

Special thanks to bbtarzan12 for creating a good basis for procedural voxel terrain generation, which this project is built upon. His project can be found in [here](https://github.com/bbtarzan12/Unity-Procedural-Voxel-Terrain).

### Data persistence
The system supports saving and loading any progress made. The state of the game can be represented as follows:
- Player Data: All player data is stored in a single file. It includes player positional data, stats data (health, mana stamina and skills) and inventory data (items and their organization in the UI).
- World Data: There is two files for each chunk of land (one for terrain data and one for object data). 
  - The terrain data file is a very long byte array, where each byte represents a single terrain voxel.
  - The object data file describes all non-voxel objects placed by the player in the world. Each object type has it's own way of serializing itself, but all objects must have at least positional data.

An autosave system is also provided, saving the state of the game whenever new chunks of land are generated (and old ones are destroyed).

### Inventory system
The inventory system is similar to what you would see in the original game.

![inventory2](https://github.com/paternostrox/Minecraft4Unity/assets/19597048/9ec1320f-ea05-4198-b65d-ce23ea2af9c4)

- Allows the player to stack items until a certain limit predefined by the item type.
- Has a hotbar & backpack UI, so the player can manage which items will be used or kept.
- It has pretty much the same organizational features as the original, including:
  - Item swap: Picking an item from a slot (LMB) and placing it in an occupied slot will automatically pick the item occupying it.
  - Take half: The player can pick half the amount of a stack (RMB) in any stack with more than one items. 
  - Eyedropper: When an item stack is being held, the player can drop items one by one in other slots.
 
## :seedling: How to contribute

1. Fork this [repository](https://github.com/paternostrox/Minecraft4Unity).
2. Create a new branch for your feature or bug fix.
3. Make your changes and test them thoroughly.
4. Submit a [pull request](https://github.com/paternostrox/Minecraft4Unity/pulls) with a clear description of your changes.
