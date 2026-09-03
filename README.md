# RolePlayer

[![License: AGPL v3](https://img.shields.io/badge/License-AGPL_v3-blue.svg)](https://www.gnu.org/licenses/agpl-3.0)
[![Framework: Dalamud](https://img.shields.io/badge/Framework-Dalamud_v15-ff69b4.svg)](https://goatcorp.github.io/)
[![Target: .NET 10](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)

**RolePlayer** is an advanced, native-feeling Final Fantasy XIV plugin built for the Dalamud framework. Designed specifically for roleplayers and content creators, it provides a modern, intuitive interface to browse, organize, filter, and execute emotes seamlessly in-game.

---

## Key Features

### 🔍 Advanced Emote Browser
* **Comprehensive Library**: Lists all native game emotes, clearly distinguishing unlocked emotes from lock-restricted ones.
* **Unlock Information**: Displays unlock sources (quests, achievements, Mog Station, or item manuals) directly in the details panel for locked emotes.
* **Native In-Game Invocation**: Instantly trigger emotes or open the native game emote window directly from the plugin interface.

### 🎨 Modded Emote Detection (Penumbra Integration)
* **Dynamic IPC Scanning**: Interrogates Penumbra in real-time to detect active emote mod replacements.
* **Exact Mod Attribution**: Maps modified action paths back to Penumbra's logical mod names, visually highlighting modified emotes (`★ Modded`) in green.
* **Filter by Modded Status**: Toggle "Modded Only" filters to quickly view all custom animations currently active on your character.

### 📂 Dynamic Categorization & Custom Organization
* **Native & Custom Grouping**: Group emotes by their native game categories or assign them to custom, user-defined groups with full name and description editing capabilities.
* **Custom Short Tags**: Create and assign custom tags (e.g., `SFW`, `NSFW`, `Dance`, `Combat`) to organize your emote library freely.
* **Relational Counter**: Automatically calculates and displays the number of emotes associated with each custom tag and group.
* **Inline Advanced Filtering**: Filter emotes dynamically by search query, native category, custom group, custom tag, or modded state. Filters persist automatically between sessions.
* **Context Menu Access**: Right-click any emote in the list to execute it, copy its chat command, or instantly assign it to groups, tags, or manual hotbars.

### 🔄 Multi-Pose & Variation Execution (`/cpose`)
* **Contextual Memory Inspection**: Reads the local player's active animation state via `FFXIVClientStructs`.
* **Smart Variation Swapping**: Clicking an active pose emote (e.g., `/sit` or `/groundsit`) automatically sends the `/cpose` command to cycle through alternative poses rather than canceling the animation.
* **Visual Indicator**: Emotes with variation support feature a subtle overlay icon (`Sync`) and contextual tooltip guidance.

### 🎛️ Native-Style Persistent Hotbars
* **Floating Action Bars**: Create an unlimited number of customizable, borderless hotbars that stay on screen even when the main plugin window is closed.
* **Flexible Grids & Layouts**: Choose from multiple grid dispositions (16x1, 8x2, 4x4, 2x8, 1x16) with compact, native-feeling button padding.
* **Dynamic & Manual Modes**:
  * **Dynamic Hotbars**: Automatically populate based on assigned tags, groups, categories, or search filters.
  * **Manual Hotbars**: Manually assign specific emotes using the right-click context menu or details panel.
* **Auto-Pagination**: Hotbars exceeding 16 emotes feature integrated pagination controls.
* **Position Locking & Live Preview**: Lock hotbar positions on screen and inspect live 4x4 previews with aggregate match counts directly from the configuration tab.

---

## Installation & Usage

1. Download the latest release from the [Releases](https://github.com/AlmerisBE/RolePlayer/releases) section or install via your custom Dalamud plugin repository.
2. In-game, use the command `/roleplayer` or `/roleplayer emotes` to open the main Emote Browser.

---

## Developer Architecture

RolePlayer is built adhering strictly to modern software engineering principles:
* **Vertical Slicing (Feature Modules)**: High cohesion and low coupling across domain boundaries.
* **Dependency Injection**: Driven via `Microsoft.Extensions.DependencyInjection`.
* **SOLID & Inversion of Control**: Consuming features own contracts (`Contracts`), decoupled from implementation providers.
* **Pure ImGui Binding**: Powered exclusively by `Dalamud.Bindings.ImGui` for high-performance rendering.
