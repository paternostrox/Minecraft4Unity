# :crystal_ball: Minecraft4Unity
A minimal and very optimized version of Minecraft made in Unity.  It features procedural world generation with greedy meshing, data persistence and an inventory system.

## :fire: Basic setup:

Just clone the project and open it in Unity version 2021.3.24f1 LTS or newer, it should also run in any 2020+ version with no issues.
All the packages needed for the project will import automatically when opening it for the first time.

## :desktop_computer: Code description

### Procedural generation

### Data persistence

### Inventory system
The inventory system is similar to what you would see in the original game. 

- Allows the player to stack items until a certain limit predefined by the item type.
- Has a hotbar & backpack UI, so the player can manage which items will be used or kept.
- It has pretty much the same organizational features as the original, including:
  - Item swap: Picking an item from a slot (LMB) and placing it in an occupied slot will automatically pick the item occupying it.
  - Take half: The player can pick half the amount of a stack (RMB) in any stack with more than one items. 
  - Eyedropper: When an item stack is being held, the player can drop items one by one in other slots. 
