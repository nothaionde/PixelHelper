Pixel Helper For Diablo III

This repository contains my personal plugins and patches for Diablo 3 that I’ve used with TurboHUD Lightning Mod.
Works with the free version!

Installation

Download the latest version of ([TurboHUD Lightning Mod](https://www.turbohud.com.cn/)).

Copy the files from this repository into the root folder of TurboHUD, replacing existing files when asked.

To use with the free version, you also need to replace Turbo.cnmgr.dll.

In recent versions, the developers added a feature that checks if free version is using DllImport. It need to be used to apply ZoomHack and few other 3rd party plugins.

Right now, replacing the DLL is enough. If this changes in the future and I don’t release a patch, reach out to me on Discord (link available in the helper).

When launching TurboHUD, do not update files. Just close the update prompt — TurboHUD will still run.

Features
Camera Zoom

A plugin that unlocks the camera zoom out.

Works directly from TurboHUD.

Fully compatible with other TurboHUD plugins.

Custom Loot Filter

Enter the name of an item and see existing in-game matches in a list.

Choose its ancient quality and secondary affix.

Organize filters into groups for different builds or goals.

Disable filters temporarily with a checkbox.

Manage filters via right-click menu: add, delete, rename.

Drag & drop items between groups.

Full Town Automation

Works only in Act 1 (Tristram).

After completing a Greater Rift or starting a new game, a state machine takes over:

Teleports to Act 1 if “Always Act 1” is enabled.

Checks for blood shards. If above 100 and “Auto Gamble” is enabled → auto gambles.

Salvages items not included in your loot filter.

Assings Paragon points automatically.

Closes Greater Rifts and can auto-open a new one.

Not always reliable in groups: if your party is too slow, the helper will enter idle state.

Auto Pickup

Automatically picks up loot.

Pylon Automation

Auto-clicks pylons if you are wearing Nemesis Bracers.

Legendary Gem Leveling

Automatically upgrades gems.

Prioritizes lowest-level gems first.

FAQ

Q: Does this work with the free version of TurboHUD Lightning Mod?
A: Yes 

Just replace Turbo.cnmgr.dll as described above.

Q: What if the DLL patch stops working in future updates?
A:

Contact me via Discord (link is included in the helper).

I’ll release a fix if possible.

Q: Can I use this with my existing plugins?
A: Yes 

The helper runs alongside all other TurboHUD plugins.

Q: What happens if I accidentally update TurboHUD files on startup?
A:

Simply close the update window.

TurboHUD will still run without updating.

Q: Does the automation work in groups?
A: Yes, but with limitations 

If your party is too slow, the helper will go idle and will not open a new portal.

Gambling and salvaging always work.

Auto Accept:

Presses Accept only after gambling and salvaging are complete.

If these options are unchecked → it will accept immediately.

Q: Why aren’t bugs fixed quickly?
A:

Only a few people use this helper.

If something bothers me (or them), I fix it myself.

Q: Can I build a bot on top of this?
A: Technically, yes

The helper already supports automatic skill usage, which is easy to configure.

The hard part is map logic, which I don’t release (closed source).

Act 1 automation is already bot-like: it doesn’t play the game for you, but reduces downtime in town.

Q: Will there be updates with new features?
A: Yes 

If I receive requests, I add features.

The project is 100% free — no donations, no sales.

Q: Can you get banned for using this?
A: Very unlikely 

Only if Blizzard bans TurboHUD itself (almost impossible).

Plugins are part of TurboHUD, not separate software.

Q: I need help with setup or usage.
A:

I don’t provide support for basic questions (“How do I install TurboHUD?”).

For plugin-related questions (mine or others) — feel free to ask.

But please first check TurboHUD’s built-in interfaces: almost everything is explained there.

Q: Are there any exceptions or issues?
A: Yes 

Everything works, but some old auto-skill plugins from Lightning Mod are included.

These are not my code, so I cannot fix or change them (Lazy).

They do not affect the functionality of Pixel Helper.