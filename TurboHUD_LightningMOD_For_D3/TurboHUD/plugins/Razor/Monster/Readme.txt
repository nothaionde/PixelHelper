Monster Markers plugin by Razorfish
https://turbohud.razorfish.dev/2021/08/monster-markers.html

This plugin allows you to define custom conditions for which text markers or decorators (both ground and map) are drawn on monsters. They stack horizontally like ground labels do, but the relatively high drawing priority allows it to draw above all world decorators.

It is inspired by Stone.ReflectDamageMonstersPlugin, Jack.DangerousAffixMonsterPlugin, Razor.RunStats.MenuUptimeTracker, and Default.Decorator.GroundLabelDecorator.

I've included a few example marker definitions to start with in the MonsterMarkersConfig.cs file, but you can remove, edit, or add to them however you like:
- snowflake symbol if monsters are frozen (and commented out, I included the code you would need to make that show only if the Buriza ethereal item is equipped)
- skull and crossbones with a dashed circle around monsters in Execute range if any party members have an execute skill or power equipped
- lightning bolt on monsters that can trigger your Halo of Karini, if you have HoK equipped
- small reticle on monsters that are the target of Command Skeletons, if you have Command Skeletons on your action bar
- (commented out/disabled) arm flex symbol on monsters that don't have Strongarms applied, if someone in your party has the Strongarms power equipped

[Tested with TurboHUD verson 21.7.22.20, API version 9.2]


CUSTOMIZATION
You can define your own conditions, markers and decorators in MonsterMarkersConfig.cs in a similar fashion to UptimeRule from Razor.RunStats.MenuUptimeTracker
Each MonsterMarker rule has a few useful configuration options:
	MonsterMarker.IsRelevant - only run IsMarked() if this function returns true
	MonsterMarker.IsMarked - only draw markers if this function returns true
	MonsterMarker.Symbol - optional, text to draw if IsMarked returns true
	MonsterMarker.Font - optional, text font (color, size, opacity, shadow) to draw with
	MonsterMarker.Decorator - optional, WorldDecoratorCollection to draw if IsMarked returns true
	MonsterMarker.DecoratorPosition - optional, a function that returns a custom position relative to its input IMonster
The default DecoratorPosition function is set to IMonster.FloorCoordinate, but it can be redefined in a customizer call.
The default SymbolPosition function is set to IMonster.FloorCoordinate.ToScreenCoordinate(), but it can be redefined in a customizer call.
The DefaultMarker rule is what is drawn for a monster if no other MonsterMarker rules match it when the MarkAllMonsters variable is set to true.


INSTALLATION
Put the folders in the archive into your TurboHUD directory. The files should be in the following locations when you're done:

TurboHUD \ plugins \ Razor \ Monster \ MonsterMarkers.cs
TurboHUD \ plugins \ Razor \ Monster \ MonsterMarkersConfig.cs
TurboHUD \ plugins \ Razor \ Util \ UtilExtensions.cs

You probably already have UtilExtensions.cs if you have the Menu Plugin System installed. It is okay to either skip or overwrite it with the version included in this download because they are the same.

Then (re)start TurboHUD.