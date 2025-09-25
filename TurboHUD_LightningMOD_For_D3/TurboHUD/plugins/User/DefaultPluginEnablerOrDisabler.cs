namespace Turbo.Plugins.User
{
    using Turbo.Plugins.Default;
    using Turbo.Plugins.Razor.Util;

    public class DefaultPluginEnablerOrDisabler : BasePlugin, ICustomizer
    {
        public DefaultPluginEnablerOrDisabler()
        {
            Enabled = true;
            Order = int.MaxValue;
        }

        public void Customize()
        {
            Hud.TryTogglePlugin("Turbo.Plugins.PixelDrama.Helper", true);
            Hud.TryTogglePlugin("Turbo.Plugins.LM.CloseDialogPlugin1", true);
            Hud.TryTogglePlugin("Turbo.Plugins.LM.CloseGRVictoryScreenPlugin1", true);
            Hud.TryTogglePlugin("Turbo.Plugins.LM.CloseQuestDialogPlugin1", true);
            Hud.TryTogglePlugin("Turbo.Plugins.LightningMod.CloseDialogPlugin", true);
            Hud.TryTogglePlugin("Turbo.Plugins.LightningMod.CloseQuestDialogPlugin", true);
            Hud.TryTogglePlugin("Turbo.Plugins.Resu.DangerPlugin", true);
            Hud.TryTogglePlugin("Turbo.Plugins.Miqui.LegendaryItemAffixPlugin", true);

            Hud.TryTogglePlugin("Turbo.Plugins.LM.PickUpPlugin1", false);
            Hud.TryTogglePlugin("Turbo.Plugins.LM.OpenChestPlugin1", false);
            Hud.TryTogglePlugin("Turbo.Plugins.LightningMod.EnchantPlugin", false);
            Hud.TryTogglePlugin("Turbo.Plugins.LightningMod.PickUpPlugin", false);
            Hud.TryTogglePlugin("Turbo.Plugins.LightningMod.OpenChestPlugin", false);


            // AutoCast
            // Movable in town
            Hud.TryTogglePlugin("Turbo.Plugins.PixelDrama.SkillDefs.CrusaiderMovable", true);
            Hud.TryTogglePlugin("Turbo.Plugins.PixelDrama.SkillDefs.DemonHunterSmokeScreenMovable", true);
            // Crusaider
            Hud.TryTogglePlugin("Turbo.Plugins.PixelDrama.SkillDefs.CrusaderSteedChargePlugin", true);
            // DH
            Hud.TryTogglePlugin("Turbo.Plugins.PixelDrama.SkillDefs.DemonHunterMarkOfTheDeadPlugin", true);
            Hud.TryTogglePlugin("Turbo.Plugins.PixelDrama.SkillDefs.DemonHunterSentryPlugin", true);
            Hud.TryTogglePlugin("Turbo.Plugins.PixelDrama.SkillDefs.DemonHunterMultishotPlugin", true);
            Hud.TryTogglePlugin("Turbo.Plugins.PixelDrama.SkillDefs.DemonHunterStrafeHungeringArrowPlugin", true);
            // From LM
            Hud.TryTogglePlugin("Turbo.Plugins.LightningMod.DemonHunterBolasPlugin1", true);
            Hud.TryTogglePlugin("Turbo.Plugins.LM.AutoSkillPlugin", true);
            Hud.TryTogglePlugin("Turbo.Plugins.LightningMod.BaseInventoryManagementPlugin1", true);
            Hud.TryTogglePlugin("Turbo.Plugins.LightningMod.UpgradeRareToLengendaryPlugin1", true);
            Hud.TryTogglePlugin("Turbo.Plugins.LightningMod.DemonHunterStrafeHungeringArrowPlugin1", true);
            Hud.TryTogglePlugin("Turbo.Plugins.LM.DemonHunterStrafeHungeringArrowPlugin", true);
            // Nec
            Hud.TryTogglePlugin("Turbo.Plugins.PixelDrama.SkillDefs.NecDevourPlugin", true);
            Hud.TryTogglePlugin("Turbo.Plugins.PixelDrama.SkillDefs.NecLandOfTheDeadPlugin", true);
            Hud.TryTogglePlugin("Turbo.Plugins.PixelDrama.SkillDefs.NecLeechPlugin", true);
        }
    }
}
