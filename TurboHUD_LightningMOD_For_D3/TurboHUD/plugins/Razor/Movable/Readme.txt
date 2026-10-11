Movable Plugin System by Razorfish
https://turbohud.razorfish.dev/2021/05/movable-plugin-system.html
https://turbohud.razorfish.dev/search/label/Movable

[Verified as working with TH version 21.3.31.3, API version 9.2]

This is a TurboHUD plugin that makes it easier to write other standalone plugins where you can drag, resize, and toggle the display on or off in-game and then save those modifications to ICustomizer plugin file(s).
https://i.imgur.com/1kYOATm.gif

FEATURES:
Unified Modification
Each plugin doesn't have to do it's own handling of mouse or key presses, and it doesn't have to define yet another hotkey for toggling stuff on or off, it can just focus on drawing data on the screen. A plugin that implements the IMovable interface will gain the ability to create movable areas that can be dragged around the screen and resized, and all of that code is handled behind the scenes by this library. All of the positions, dimensions, and movement delta values necessary to draw elements on the screen are provided to the Movable plugin via a PaintArea method, which works like PaintTopInGame.

Drag (or Hotkey) to Move
Press F12 to toggle Edit Mode on, which shows outlines of all the movable plugin display areas on the screen. You can drag and drop them freely while in this mode. Alternatively, because clicking around on the screen causes your character to move around or perform actions, you can press E (left) or R (right) to simulate a mouse click. To disable this or change its hotkey, change or unset SimulateLeftMouseClick and SimulateRightMouseClick properties in plugins\Razor\Click\ClickEventHandler.cs

Drag (or Hotkey) to Resize
While in Edit mode, hovering over a resizable plugin area shows little triangle corners on the bottom left and bottom right of the outline. Hovering over them will show what sort of resizing you can do. There are 4 Resize modes that plugins can specify: On (free resize), Fixed Ratio, Horizontal, and Vertical. Not all areas are resizable, just depends on the plugin and whether or not it does the extra size calculations.
https://i.imgur.com/SqbFvA1.gif

Toggle Off or On
While in Edit mode, press Ctrl + X while you have a plugin area selected or hovered over to toggle that area display on or off.

Undo
While in Edit mode, press Ctrl + Z while you have a plugin area selected or hovered over to undo drag or resize actions applied to it.

Save Settings to File
For Tier 3 users, using the save-to-file hotkey (default: Ctrl + S) will generate an ICustomizer plugin file with your modified settings in the correct location. This will allow you to retain the changes you made when you load TurboHUD next time.
Changing the position, size or on/off state of any of the movable areas will automatically generate an ICustomizer file at TurboHUD\logs\MovablePluginConfig.txt. 
In case you forget to use the save-to-file hotkey, this file is there so that you can optionally restore your changes from the last aborted TurboHUD session by copying the txt file from your TurboHUD\logs folder to TurboHUD\plugins\User\MovablePluginConfig.cs.
For Tier 2 users, note that TurboHUD version 20.7.12.0 or newer has a limitation on when log files can be written - only in menus or game difficulties below Torment 1 and not in special areas (such as rifts) - but the framework will wait until you're in a valid logging state before trying to create the config file.
For plugin authors, setting a MovableArea's ConfigFileName property will write its config changes to the specified file instead. Leaving it blank will result it being written to the default "MovablePluginConfig" file.


HOTKEYS (configurable):
All of these are configurable, with the variables mostly located in TurboHUD\plugins\Razor\Movable\MovableController.cs.

ToggleEditMode (F12) - toggle Edit Mode on or off
SimulateLeftMouseClick (E) - because when you click around the screen, D3 moves your character around and interacts with the game world, it's probably easier just to press a hotkey to simulate a mouse click - edit this in TurboHUD\plugins\Razor\Click\ClickEventHandler.cs
SimulateRightMouseClick (R) - because when you click around the screen, D3 moves your character around and interacts with the game world, it's probably easier just to press a hotkey to simulate a mouse click - edit this in TurboHUD\plugins\Razor\Click\ClickEventHandler.cs
ToggleEnable (Ctrl + X) - toggle display on or off
ToggleGrid (Ctrl + G) - toggle placement grid display on or off
HotkeyUndo (Ctrl + Z) - undo movement or resize
HotkeyUndoAll (Ctrl + 0) - undo all movements or resizes (ctrl + ZERO)
HotkeySave (Ctrl + S) - save config file now (this is separate from auto-save, but is still subject to the logging limitations of your TH version)
HotkeyCancel (Esc) - cancel your current move or resize action
HotkeyPickupNext (Right) - cycle forwards through all areas under your cursor and pick up the next one
HotkeyPickupPrev (Left) - cycle backwards through all areas under your cursor and pick up the previous one
https://i.imgur.com/ofOU0U9.gif


VISIBILITY CHECKING
Movable plugins are rendered during the ClipState.BeforeClip step by default, and it will properly disable rendering, click and hotkey events while obscured by (sharing the same space as) most visible game interface elements.


CUSTOM HOTKEY AND CLICK HANDLERS
The Movable system has its own optional handlers for hotkeys and click events that occur while the mouse cursor is hovering over a visible MovableArea. This makes it simple to code actions like "do something when you click on this MovableArea" or "do something when you use a hotkey while hovering over this MovableArea."
IMovableKeyEventHandler
IMovableLeftClickHandler
IMovableRightClickHandler


INSTALLATION:
Extract the folders from the download archive into your TurboHUD folder. The files should go in the following places:

TurboHUD \ plugins \ Razor \ Click \ ClickEventHandler.cs
TurboHUD \ plugins \ Razor \ Click \ ILeftClickHandler.cs
TurboHUD \ plugins \ Razor \ Click \ IRightClickHandler.cs
TurboHUD \ plugins \ Razor \ Hotkey \ HotkeyEventHandler.cs
TurboHUD \ plugins \ Razor \ Hotkey \ IHotkeyEventHandler.cs
TurboHUD \ plugins \ Razor \ Log \ ITextLogger.cs
TurboHUD \ plugins \ Razor \ Log \ TextLogger.cs
TurboHUD \ plugins \ Razor \ Movable \ IMovable.cs
TurboHUD \ plugins \ Razor \ Movable \ IMovableDeleteAreaHandler.cs
TurboHUD \ plugins \ Razor \ Movable \ IMovableKeyEventHandler.cs
TurboHUD \ plugins \ Razor \ Movable \ IMovableLeftClickHandler.cs
TurboHUD \ plugins \ Razor \ Movable \ IMovableRightClickHandler.cs
TurboHUD \ plugins \ Razor \ Movable \ MovableArea.cs
TurboHUD \ plugins \ Razor \ Movable \ MovableController.cs
TurboHUD \ plugins \ Razor \ Movable \ ResizeMode.cs
TurboHUD \ plugins \ Razor \ Util \ UIOverlapHelper.cs

Then (re)start TurboHUD.


INFO FOR PLUGIN AUTHORS
If you would like to make your plugin use the Movable system, I've included a heavily commented example plugin template to explain how to implement it. You can find it at:
TurboHUD\plugins\Razor\Movable\MyMovablePluginTemplate.txt
