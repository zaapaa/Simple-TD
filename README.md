# Tower Defense Game - OOP in Action

A Unity tower defense game demonstrating Object-Oriented Programming principles with clean architecture patterns.

## Features

### Core Gameplay
- **Tower Defense Mechanics**: Place towers to defend against waves of enemies
- **Multiple Tower Types**: Basic, Missile, and Laser towers with unique behaviors
- **Enemy System**: Various enemy types (Basic, Fast, Strong, Big) with different stats
- **Wave Spawning**: Progressive difficulty with increasing enemy health and spawn rates
- **Economy System**: Earn money by defeating enemies, spend on towers

### Advanced Features
- **Targeting System**: Multiple targeting modes (Nearest, Farthest, Weakest, Strongest, First, Last)
- **Force Targeting**: Right-click enemies to force towers to target specific enemies
- **Wall-to-Tower Upgrades**: Upgrade walls to towers dynamically
- **Visual Feedback**: Targeting indicators, selection systems, and tooltips
- **Area of Effect (AOE)**: Some projectiles have damage falloff
- **Homing Projectiles**: Missiles that track and redirect to new targets

### UI/UX
- **Build Panel**: Tower selection with tooltips showing DPS, range, attack speed
- **Selection System**: Multi-select support with shift/control
- **Upgrade Panel**: Context-sensitive upgrade options
- **Information Display**: Real-time stats for selected objects

## Architecture

### Key Classes
- **Tower**: Base tower class with targeting, attacking, and upgrade logic
- **Enemy**: Enemy behavior with health, pathfinding, and targeting visual feedback
- **Projectile**: Projectile system with homing and AOE capabilities
- **GameUIHandler**: Central UI management and input handling
- **GameManager**: Game state, economy, and wave management
- **EnemyWaveSpawner**: Wave generation and enemy spawning logic

### Design Patterns
- **Component-Based**: Modular tower and enemy behaviors
- **Event-Driven**: UI interactions and game events
- **State Management**: Clean separation of game states
- **Singleton Pattern**: GameManager for global state access

## Controls

### Building
- **Right Click**: Open build panel
- **Left Click**: Place selected tower/select objects
- **Scroll**: Rotate tower during placement
- **Shift/Ctrl + A**: Select all placed objects

### Combat
- **Right Click on Enemy**: Force all selected towers to target that enemy
- **Auto-targeting**: Towers automatically engage enemies based on targeting mode

## Technical Notes

### Performance
- **Object Pooling**: Efficient projectile and enemy management
- **LOD System**: Distance-based visual quality adjustments
- **Optimized Targeting**: Efficient enemy detection and selection

### Extensibility
- **ScriptableObjects**: Easy tower and enemy configuration
- **Modular Components**: Add new behaviors without modifying core systems
- **Clean Interfaces**: ISelectable for consistent interaction patterns

## Getting Started

1. Open the scene in Unity
2. Press Play to start the game
3. Use right-click to open build menu
4. Place towers strategically to defend against enemy waves
5. Upgrade walls to towers when needed
6. Survive as many waves as possible!

## Development Notes

This project demonstrates:
- Clean OOP principles with proper encapsulation
- Interface-based design for extensibility
- Efficient Unity-specific optimizations
- Modular, maintainable code architecture
- Proper separation of concerns
