# Pixel Helper for Diablo III

Pixel Helper is a collection of personal plugins and patches for Diablo III, designed to work with TurboHUD Lightning Mod.

**Works with the free version of TurboHUD Lightning Mod.**

## Installation

1. Download the latest version of [TurboHUD Lightning Mod](https://www.turbohud.com.cn/).
2. Copy the files from this repository into the TurboHUD root directory, replacing existing files when prompted.
3. To use Pixel Helper with the free version, replace `Turbo.cnmgr.dll` with the patched version included in this repository.

Recent versions of TurboHUD Lightning Mod introduced a check related to `DllImport` usage in the free version. The patched DLL is required for Zoom Hack and some other third-party plugins to work.

Currently, replacing the DLL is sufficient. If this changes in a future update, I may release another patch if possible.

When launching TurboHUD, close the update prompt without proceeding with the update.

Once Pixel Helper is loaded, its Win32 UI window will appear. The status indicator in the bottom-right corner shows whether the helper is enabled or suspended.

## Features

### Camera Zoom

Unlocks the camera zoom-out limit.

- Works directly through TurboHUD.
- Compatible with other TurboHUD plugins.

### Custom Loot Filter

Configure loot filters based on item names and properties.

- Enter an item name and browse matching in-game items.
- Filter by ancient quality and secondary affixes.
- Organize filters into groups for different builds or goals.
- Temporarily disable filters using a checkbox.
- Add, delete, and rename filters through the right-click menu.
- Drag and drop items between groups.

### Full Town Automation

Automates routine tasks in Act I (Tristram).

After completing a Greater Rift or starting a new game, a state machine handles the following tasks:

- Teleports to Act I if **Always Act 1** is enabled.
- Checks your Blood Shards and automatically gambles when you have more than 100, provided **Auto Gamble** is enabled.
- Salvages items that are not included in your loot filter.
- Automatically allocates Paragon points according to the helper's built-in rules.
- Closes Greater Rifts and can automatically open a new one.

Group play has some limitations. If your party takes too long, the helper may enter an idle state and stop opening new portals. Gambling and salvaging do not depend on the party's progress, although they still depend on your character's state.

### Auto Pickup

Automatically picks up nearby loot within a limited radius.

### Pylon Automation

Automatically activates pylons when you are wearing Nemesis Bracers.

### Legendary Gem Leveling

Automatically upgrades Legendary Gems, prioritizing the lowest-level gems first. If a gem is placed in the top-left slot of your inventory, the helper will prioritize that gem whenever it can be upgraded.

### Skill Automation

Pixel Helper includes build-specific skill automation for several Diablo III builds, reducing the amount of manual input required during gameplay.

- **zDH:** Press Spin and focus on moving your mouse. The helper handles the rest of the gameplay.
- **Crusader:** Enable TurboHUD's built-in auto-cast for Akarat's Champion, Iron Skin, Laws of Hope, and Laws of Valor. Do **not** enable auto-cast for Steed Charge (horse). Pixel Helper handles its timing and waits for the other skills to be cast when they are available.
- **Monk:** Detects generator-based builds using Shenlong's Spirit and automatically maintains the Combination Strike passive by casting the required generator. You only need to use your main damage skill. Mantras are also cast automatically according to the helper's built-in conditions.
- **zNec (Necromancer):** Dash requires manual input. Curses are already automated based on enemy packs, but you can still cast them manually when you want more control—for example, when fighting trash mobs.
- **zBarb (Barbarian):** Pixeling and Charge require manual input; the remaining skills are automated.

Some builds may support further automation. For example, Monk's main damage skill could potentially be automated too, depending on the desired targeting and casting conditions.

Additional automation is available for other builds and skills as well, including maintaining Wizard buffs. These are minor quality-of-life features, so they aren't all listed here.

## FAQ

**Q: Does Pixel Helper work with the free version of TurboHUD Lightning Mod?**

Yes. Replace `Turbo.cnmgr.dll` as described in the Installation section.

**Q: What if the DLL patch stops working after a TurboHUD update?**

Contact me through [Discord](https://discord.com/users/534628155474968608). 

I'll release a fix if possible.

**Q: Can I use Pixel Helper alongside my existing plugins?**

Yes. Pixel Helper is designed to run alongside other TurboHUD plugins.

**Q: What happens if I accidentally update TurboHUD files on startup?**

Close the update prompt without proceeding with the update. TurboHUD will continue running without updating its files.

**Q: Does town automation work in groups?**

Yes, but with limitations.

If your party takes too long, the helper may enter an idle state and stop opening new portals. Gambling and salvaging are independent of the party's progress.

**Q: What does Auto Accept do?**

It presses Accept after gambling and salvaging are complete.

If both options are disabled, Accept is pressed immediately.

**Q: Why aren't bugs fixed quickly?**

This is a personal project used by a relatively small number of people. I fix issues that affect me or other users when I consider them worth fixing.

**Q: Can I build a bot on top of Pixel Helper?**

Technically, yes.

**Q: Will there be updates or new features?**

Yes. I add features when I receive requests and decide to implement them.

The project is completely free. No donations, no sales.

**Q: Can I get banned for using TurboHUD or Pixel Helper?**

Using third-party software may violate Blizzard's rules and can carry a risk of account sanctions. I cannot guarantee that using TurboHUD or its plugins is safe.

**Q: Can you help me with setup or usage?**

I don't provide basic TurboHUD support, such as instructions on how to install TurboHUD itself.

Questions about Pixel Helper and other plugins are welcome. Please check TurboHUD's built-in interfaces first, as most features are explained there.

**Q: Are there any known issues or exceptions?**

Some older auto-skill plugins from Lightning Mod are included. They are not my code, and I don't plan to maintain or modify them. They do not affect Pixel Helper's functionality.