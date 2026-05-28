# Cursed Depths

**Cursed Depths** is a 2D Unity platformer/action game set inside a cursed cave system. The player explores underground areas, fights enemies, uses special abilities, interacts with menus and settings, and moves through multiple designed scenes such as the starting area, puzzle room, aura area, boss door, and boss battle.

Related repository: [Cursed Depths Core](https://github.com/MineKing702/CursedDepths.core)

This repository contains the main Unity project, including scenes, gameplay scripts, animations, sprites, UI assets, music, prefabs, and environment art.

## Team Members

* Andrew Gountis
* Daniel Kaminski

## Project Overview

Cursed Depths was built as a final .NET/C# team project using Unity. The goal of the project was to create a playable game that demonstrates scene design, player movement, combat, enemy behavior, UI programming, settings management, animation, and asset integration.

The game focuses on player exploration and combat inside a fantasy cave environment. The project includes multiple connected Unity scenes, custom scripts, enemy behavior, player health and respawn logic, an ability system, and a home/settings menu.



## How to Run the Project

1. Clone or download this repository.
2. Open the project in **Unity 6000.4.6f1** or a compatible Unity 6 version.
3. Make sure the separate **Cursed Depths Core** package is available if the project references it.
4. Open the Unity project folder.
5. In Unity, open the scene:

   * `Assets/_Scenes/Home Scene.unity`
6. Press **Play** in the Unity Editor.
7. From the home menu, start the game to load the main gameplay scene.

## Main Features

### Player Controller

The player controller includes:

* Horizontal movement
* Jumping
* Sprite direction flipping
* Attack input
* Ground detection
* Health system
* Fall damage
* Death and respawn behavior
* Invincibility timing after taking damage
* Animation support for idle, run, jump, fall, landing, punch, hurt, and death-style behavior

### Combat System

The game includes melee combat where the player can attack nearby enemies. Combat includes attack range, damage, cooldown timing, animation delays, and enemy hit detection.

### Ability System

Cursed Depths includes a reusable ability system built with ScriptableObjects. The ability system supports:

* Equipping abilities
* Ability slots
* Cooldowns
* Ability execution routines
* VFX spawning
* Ability choices
* A sample ability called **Divergent Slice**

Divergent Slice performs a normal attack, waits briefly, then creates a follow-up hit with visual effects.

### Enemy AI

The enemy system includes a reusable `EnemyController` with an enum-based state machine. Enemy behavior supports:

* Idle behavior
* Patrol behavior
* Chasing the player
* Attacking
* Fleeing
* Hurt state
* Death state
* Detection range
* Attack range
* Ledge detection
* Wall-jump style movement support
* Animator parameter mapping

Enemies also include health logic through `EnemyHealth`, allowing them to take damage, play hurt/death animations, disable colliders, and be destroyed after death.

### Scenes and Level Design

The project includes multiple Unity scenes, including:

* Home Scene
* Starting Area
* Area2
* Aura Area
* Puzzle Room
* Boss Door
* Boss Battle

These scenes are used to organize the game into separate areas and gameplay moments.

### Home Menu and Settings

The home menu includes UI behavior for:

* Starting the game
* Opening settings
* Closing settings
* Quitting the game
* Fade-in effects
* Menu transitions

The settings system supports:

* Master volume
* Music volume
* Sound effects volume
* Key rebinding for walking, jumping, and attacking

### Audio

The project includes music and audio management. Audio volume is affected by player settings, including master, music, and sound effects sliders.

### Runtime Bootstrapper

The project includes a runtime bootstrapper that creates required persistent runtime services before the first scene loads. This helps ensure the settings manager exists when the game starts.

## Technologies Used

* C#
* Unity 6
* Unity 2D tools
* Rigidbody2D physics
* Animator Controllers
* ScriptableObjects
* TextMesh Pro
* Unity UI
* Unity scene management
* Custom event-driven settings logic
* Custom player, enemy, and ability scripts

## Repository Structure

Important folders include:

```text
Assets/_Scenes
```

Contains the main game scenes.

```text
Assets/_Scripts
```

Contains custom gameplay, player, enemy, ability, camera, home menu, and settings scripts.

```text
Assets/_Scripts/Abilities
```

Contains the ability system, including ability definitions, slots, ability context, ability choices, and player ability controller.

```text
Assets/_Scripts/Enemies
```

Contains enemy AI, enemy state logic, and enemy setup documentation.

```text
Assets/_Scripts/Home
```

Contains home menu, UI, audio, and settings management scripts.

```text
Assets/_Sounds
```

Contains music and audio files.

```text
Assets/Animations
```

Contains player, enemy, and UI animations.

```text
Assets/_UI
```

Contains custom UI sprites, fonts, and UI packs.

## Team Contributions

### Andrew Gountis

* Worked on Unity gameplay programming
* Helped build and organize the main Unity project
* Worked on player behavior, combat, abilities, and gameplay systems
* Helped integrate art, scenes, assets, and UI
* Used AI tools for planning, programming support, concept work, and documentation help

### Daniel Kaminski

* Worked on Unity project development and gameplay implementation
* Helped with core game structure, scene setup, and project organization
* Helped test gameplay features and improve the final project
* Contributed to documentation, planning, and presentation preparation

## Use of AI Tools

AI tools were used as development support, not as a replacement for understanding the project.

The team used:

* **Suno Music** for AI-assisted music/audio creation.
* **ChatGPT / Codex** for project planning, game concept development, art direction ideas, technical explanations, and README/documentation support.
* **Codex programming assistance** for help with C# scripting, debugging, organization, and implementation ideas.
* **Ludo animations** for animation-related support and workflow assistance.

All AI-assisted code and ideas were reviewed, modified, tested, and integrated by the team. The team is responsible for the final project and can explain how the major systems work.

## Known Issues and Limitations

* Some gameplay balancing values, such as enemy health, player damage, ability cooldowns, and movement speed, may need more tuning.
* Some scenes and assets are still prototype-style and could be polished further.
* Progress saving is limited; the project focuses more on gameplay systems and scene interaction than a full save/load system.
* Some UI and level transitions could be improved with more time.
* Additional enemy types, boss mechanics, and ability upgrades could be added in the future.

## Future Improvements

With more development time, we would like to add:

* More enemy variety
* More player abilities
* Better boss fight mechanics
* More polished UI
* A checkpoint/save system
* More sound effects
* More complete story and lore
* Improved scene transitions
* More playtesting and balancing

## Final Project Summary

Cursed Depths is a Unity/C# game project that combines platforming, combat, enemy AI, settings, audio, UI, and multiple scenes into one playable project. It demonstrates team-based development, Unity programming, C# scripting, asset integration, and responsible AI-assisted development.
