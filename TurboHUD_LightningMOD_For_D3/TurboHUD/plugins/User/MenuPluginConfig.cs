/*
	This file contains "menu" plugin priorities and addon settings.
	Change the file extension from .txt to .cs and move this file into the TurboHUD / plugins / User folder
*/

namespace Turbo.Plugins.User
{
	using Turbo.Plugins.Default;
	using Turbo.Plugins.Razor.Menu;

	public class MenuPluginConfig : BasePlugin, ICustomizer
	{
		public MenuPluginConfig() { Enabled = true; }

		public override void Load(IController hud) { base.Load(hud); }

		public void Customize()
		{
			Hud.RunOnPlugin<MenuPlugin>(plugin =>
			{
				//ConfigureDock(string dockId, params string[])
				//ConfigureAddon(string addonId, bool enabled, string config)

				plugin.ConfigureDock("BottomRight", "MenuNephalemRift", "MenuMaterialKeystone", "MenuMaterialGold", "MenuMaterialDeathsBreath", "MenuMaterialBloodShard", "MenuLoot", "MenuLatency", "MenuGreaterRift", "MenuBounties");
				plugin.ConfigureAddon("MenuNephalemRift", true, "");
				plugin.ConfigureAddon("MenuMaterialKeystone", true, "");
				plugin.ConfigureAddon("MenuMaterialGold", true, "");
				plugin.ConfigureAddon("MenuMaterialDeathsBreath", true, "");
				plugin.ConfigureAddon("MenuMaterialBloodShard", true, "");
				plugin.ConfigureAddon("MenuLoot", true, "");
				plugin.ConfigureAddon("MenuLatency", true, "");
				plugin.ConfigureAddon("MenuGreaterRift", true, "");
				plugin.ConfigureAddon("MenuBounties", true, "");

				plugin.ConfigureDock("BottomLeft", "MenuVolume", "MenuBuffs", "MenuUptime", "MenuSpiritBarrage", "MenuDamageTypes", "MenuCrowdControl");
				plugin.ConfigureAddon("MenuVolume", true, "");
				plugin.ConfigureAddon("MenuBuffs", true, "");
				plugin.ConfigureAddon("MenuUptime", true, "");
				plugin.ConfigureAddon("MenuSpiritBarrage", true, "");
				plugin.ConfigureAddon("MenuDamageTypes", true, "");
				plugin.ConfigureAddon("MenuCrowdControl", true, "");

				plugin.ConfigureDock("BottomCenter", "MenuMoveSpeed", "MenuDamageReduction", "MenuDamageDone", "MenuAttackSpeed", "MenuHealth");
				plugin.ConfigureAddon("MenuMoveSpeed", true, "");
				plugin.ConfigureAddon("MenuDamageReduction", true, "");
				plugin.ConfigureAddon("MenuDamageDone", true, "");
				plugin.ConfigureAddon("MenuAttackSpeed", true, "");
				plugin.ConfigureAddon("MenuHealth", true, "");

				plugin.ConfigureDock("MinimapBottom", "MenuMapShrines");
				plugin.ConfigureAddon("MenuMapShrines", true, "");

				plugin.ConfigureDock("TopCenter", "MenuXP", "MenuPools", "MenuParagon");
				plugin.ConfigureAddon("MenuXP", true, "");
				plugin.ConfigureAddon("MenuPools", true, "1 178880641:Haionde:0:0:9,98422069640041:0:38:0;178876070:Haionde:1:0:1,10231865886402:0:38:0;178914173:Haionde:6:1:3,83889287741473:0:38:0;177390378:Haionde:0:1:0,246949251752322:0:38:0;178916434:Haionde:2:1:1,02756433186174:0:38:0;177597858:Haionde:5:1:0:0:37:0;177389952:Haionde:4:1:0,0139470374618856:0:37:0;177390323:Haionde:1:0:0,0474028747417093:0:37:0;179633411:Haionde:0:1:7,64824166099684:0:39:0;179661712:Haionde:5:0:2,86596072163172:0:0:0;179665443:Hainde:1:0:7,81908707672372:0:39:0;179666027:Haionde:6:1:0,560939885111045:0:39:0;179703154:Haionde:1:0:4,94207362192704:0:39:0;179705261:asd:1:1:0:0:39:0;179840878:Haionde:6:1:3,4851616636577:0:39:0;180565911:Haionde:4:1:9,78719527028492:0:40:0;180566006:Haionde:0:1:3,2169075643275:0:40:0;180591358:Haionde:1:0:4,20956176641277:0:40:0;180598854:Haionde:5:1:5,67042309634197:0:40:0;180659402:Haionde:2:1:0:0:40:0");
				plugin.ConfigureAddon("MenuParagon", true, "0");

				plugin.ConfigureDock("TopRight", "MenuToggleAddons", "MenuTogglePlugins");
				plugin.ConfigureAddon("MenuToggleAddons", true, "");
				plugin.ConfigureAddon("MenuTogglePlugins", true, "");
			});
		}
	}
}