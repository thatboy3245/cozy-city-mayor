# Cozy City Mayor — 2D Pixel RPG & Municipal Government Simulator

A single-player 2D cozy pixel RPG and city-building government simulation for Windows where you win a unanimous election over **Cornelius Von Monocle**, manage your city from your **City Hall Desk Computer (`CozyOS`)**, hand-deliver **Property Tax Bills** to citizens, control the **Weather** & **Day Type**, dispatch the **Police Precinct**, read multi-page books in the **Public Library**, and use your **AI-powered On-Screen Smartphone (`MayorPhone`)** to stay on top of your daily schedule.

---

## Key Features

- **Unanimous Election Cutscene (`12 - 0`)**: Start on the Maplewood Town Square election stage against your pompous rival **Cornelius Von Monocle**, choose your campaign platform, watch all 12 citizens cast their ballots for **YOU**, and celebrate under a shower of confetti before entering City Hall.
- **City Hall & Mayor's Desk Computer (`CozyOS v2.6`)**:
  - **1. Overview & Fiscal Policy**: Adjust property tax rates, print fresh property tax bills to your Mayor Bag, review AI morning briefings, and advance the day.
  - **2. Employees Database**: Inspect salary, morale, efficiency, and AI HR evaluations for 6 municipal employees (*Chief Barnaby*, *Officer Maple*, *Penny Ledger*, *Clara Page*, *Master Builder Hugo*, and *Groundskeeper Gus*), and award `$75` Cozy Bonuses.
  - **3. Police Incidents & Dispatch**: Review open police cases (*The Great Croissant Caper*, *Midnight Accordion Serenade*, *Duckling Crosswalk Jam*, and dynamic AI-generated cases) and resolve them via **Cozy Mediation**, **Detective Investigation**, or **Strict Municipal Fine**.
  - **4. Day Type & Weather Harmonizer**: Decide what type of day it is (**Tax Day**, **Cozy Festival Day**, **Civic Construction Day**, **Community Safety Day**, **Library Literacy Day**) and control the live town weather (**Sunny & Warm**, **Cozy Rain**, **Autumn Breeze**, **Gentle Snow**, **Starlight Glow**).
  - **5. City Builder Blueprint**: Construct and upgrade 8 cozy building types (*Cozy Bakery*, *Maple Coffee Shop*, *Community Park*, *Solar Garden*, *Town Clinic*, *Artisan Bookstore*, *Cozy Apartments*, *Retro Arcade*) across 6 town lots.
  - **6. CozyAI Executive Terminal**: Query your AI Chief of Staff for fiscal forecasts, police tactical scans, employee morale reviews, and weather synergy advice.
- **Personal Property Tax Bill Delivery (2D RPG)**: Walk around Maplewood Town Square, approach citizens marked with a golden `TAX BILL $` icon, and choose whether to collect the full tax, offer a Cozy Payment Plan (`+Happiness`), or grant Neighborly Tax Relief (`+Cozy`).
- **Public Library & Cozy Book Reader**: Walk into the Maplewood Public Library and read 8 multi-page books, including **Volume 8: The Live AI Chronicle of Your Term**, which dynamically writes the history of your decisions!
- **On-Screen Smartphone (`MayorPhone 5G`) & Daily Schedule**: Always-accessible corner smartphone HUD (`[P]` or Gamepad `Y`) with 5 apps: **Daily Schedule Checklist**, **Weather/Day Remote**, **Tax & Police Radar**, **CozyAI Chat**, and **Controller & Save Manager**.
- **Native XInput + DirectInput Controller Support**:
  - **XInput (`xinput1_4.dll` / `xinput9_1_0.dll`)**: Full thumbstick, D-Pad, A/B/X/Y, shoulder buttons, and dual-motor haptic vibration/rumble.
  - **DirectInput (`dinput8.dll` + `winmm.dll` `joyGetPosEx`)**: Full support for DirectInput gamepads, joysticks, POV hats, and 12+ buttons.
  - **Keyboard & Mouse**: Full WASD/Arrow keys + point-and-click UI support.

---

## Multi-File Source Architecture (`src/`)

| File | Responsibility |
| :--- | :--- |
| `src/Program.cs` | Entry point, 60 FPS double-buffered `MainForm`, mouse/keyboard routing, `--verify` self-test mode |
| `src/Models.cs` | Core data models, enums, NPC/employee/incident/tax/book/schedule schemas |
| `src/InputSystem.cs` | Native **XInput** (`xinput1_4.dll` + rumble) and **DirectInput** (`dinput8.dll` + `winmm.dll`) + Keyboard/Mouse |
| `src/AudioSystem.cs` | Procedural 16-bit PCM WAV synthesizer generating cozy chimes, fanfare, and sound effects in memory |
| `src/AISystem.cs` | `CozyAISystem` powering executive briefings, advisor Q&A, HR evaluations, and dynamic police incidents |
| `src/LibrarySystem.cs` | 8 multi-page readable books + live AI-generated Mayoral Chronicle |
| `src/ElectionScene.cs` | Opening Election debate, live 12-0 ballot tally animation, and confetti particle engine |
| `src/GameStateEngine.cs` | Central state machine, municipal economy, tax bills, building upgrades, weather particles, save/load |
| `src/WorldController.cs` | 2D RPG movement, doorway transitions, NPC interactions, dialogue choices, and computer/phone input |
| `src/PixelArtRenderer.cs` | Crisp `640x360` nearest-neighbor 2D pixel art renderer for all scenes, sprites, and overlays |

---

## Building the Game & Inno Setup Installer

Run `./build.ps1` in PowerShell to compile `build\CozyCityMayor.exe` and package `installer_output\CozyCityMayor_Setup.exe` (with `EULA.txt`) via Inno Setup 6.
