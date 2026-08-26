# PixelHelper for Diablo III

An automated state engine and custom filtering framework built in C# (.NET), focusing on runtime process automation, memory interaction, and UI extension.

---

## Technical Architecture & Highlights

* **Finite State Machine (FSM):** Orchestrates multi-step asynchronous town routines (salvaging, gambling, rift management) with built-in idle fallbacks and edge-case recovery.
* **Process Interop & Memory Patching:** Utilizes managed/unmanaged interop (`DllImport`) for runtime DLL modification and low-level memory interaction.
* **Dynamic Evaluation Engine:** Rule-based filtering system evaluating real-time in-game object attributes (affixes, quality thresholds, item properties).
* **Modular Infrastructure:** Designed with clean boundary separation between state logic, memory patching, and client extensions.

---

## Features & Overview

### Camera Zoom
* Unlocks in-game camera zoom-out capability directly from TurboHUD.
* Fully compatible with standard plugin setups.

### Custom Loot Filter
* **Affix & Quality Evaluation:** Real-time matching based on ancient status, affix names, and item sub-types.
* **Organization:** Group-based filter rules, fast toggle checkboxes, right-click context menus, and drag-and-drop ordering.

### Town Automation (Act 1 / Tristram)
* **Finite State Loop:** Executes sequential routines after Greater Rift completions or game launches.
* **Blood Shard Gambling:** Auto-gambles when shards exceed standard limits.
* **Inventory Management:** Automatically salvages items not whitelisted in your active loot filter.
* **Paragon & Portal Management:** Handles Paragon point distribution and automatic rift re-opening/closing.

### Additional Utilities
* **Auto Pickup:** Automated loot collection pipeline.
* **Pylon Automation:** Auto-activates pylons when Nemesis Bracers are detected.
* **Gem Leveling:** Prioritizes and upgrades lowest-level legendary gems automatically.

---

## Installation

1. Download and extract the latest release of **TurboHUD Lightning Mod**.
2. Copy all files from this repository into the root TurboHUD folder (overwrite existing files when prompted).
3. Ensure `Turbo.cnmgr.dll` is replaced to allow runtime interop patches for camera unlocked features.
4. Launch TurboHUD. When prompted with update windows, close them to retain patch compatibility.

---

Markdown
## FAQ & Support

**Q: Does this work with the free version of TurboHUD Lightning Mod?**  
*A: Yes, replacing `Turbo.cnmgr.dll` as outlined in the installation steps enables full compatibility.*

**Q: Is this the latest development version?**  
*A: This repository hosts the **latest stable release**. Bleeding-edge patches and hotfixes are rolled out dynamically before being merged here into major releases.*

**Q: How do I report urgent bugs or request immediate support?**  
*A: For fast, same-day support and hotfixes, reach out via **Discord** `[https://discord.com/users/534628155474968608]`. GitHub Issues are checked periodically for major stable updates, while live troubleshooting happens on Discord.*

**Q: Can I use this with existing third-party plugins?**  
*A: Yes, PixelHelper runs as a modular extension alongside standard TurboHUD plugins.*
