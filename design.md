# Simple tower defense game

- 2 level layouts
- Player places walls/towers to restrict enemy movements.
- Towers shoot enemies.
- Enemies spawn in waves.
- Enemies have health bars.
- Waves become bigger over time.
- Waves start faster over time.
- Simple economy system
- Enemy path recalculated when player places a wall/tower
  - if no path, block the placement

## Placeables:
- wall: just a simple one
- towers:
  - basic: shoots at enemies
  - missile: shoots a high arcing missile at enemies
  - laser: shoots a continuous laser at enemies

## Enemies:
- cube: basic
- sphere: faster
- cylinder: slower
- bigger cube with texture: spawn cubes on death

## UI/UX:
- left click to select objects, UI for it somewhere
  - show stats of selected object
  - wall: show upgrade buttons
  - tower: show targeting priority buttons
     - nearest (default)/farthest
     - strongest/weakest
     - closest to end/last
- right click:
  - on empty ground to bring up build menu UI
    - when building, show preview of the placeable
  - on enemy to show more stats
     - if tower selected, make tower attack the enemy
- pause button (esc keybind)
- speed buttons (1x, 2x, 4x)
- start next wave button (spacebar)
- money UI

## Main Menu Scene:
- Start button
- Exit button

## Future ideas:
- Sound effects
- Music
- Particle effects
- Better UI/UX
  - Building:
    - Snapping
    - Connectors
- More levels
- More placeables
- More enemies
- Different enemy wave types
- More tower types
- Tower upgrades
- More enemy types
- More economy system
- More game modes
- Placeable health system: enemies attack if no path or special wave type
- Multiplayer