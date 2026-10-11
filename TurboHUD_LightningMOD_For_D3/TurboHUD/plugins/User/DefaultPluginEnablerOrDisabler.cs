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
            Hud.TryTogglePlugin("Turbo.Plugins.LM.CloseDialogPlugin1", true);

            Hud.TryTogglePlugin("Turbo.Plugins.Resu.DangerPlugin", true);
            Hud.TryTogglePlugin("Turbo.Plugins.Miqui.LegendaryItemAffixPlugin", true);

            Hud.TryTogglePlugin("Turbo.Plugins.LM.PickUpPlugin1", false);
            Hud.TryTogglePlugin("Turbo.Plugins.LM.OpenChestPlugin1", false);
            Hud.TryTogglePlugin("Turbo.Plugins.LM.EnchantPlugin1", true);
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
            //Hud.TryTogglePlugin("Turbo.Plugins.PixelDrama.SkillDefs.DemonHunterStrafeHungeringArrowPlugin", true);
            Hud.TryTogglePlugin("Turbo.Plugins.PixelDrama.SkillDefs.DemonHunterSmokeScreenPlugin", true);
            Hud.TryTogglePlugin("Turbo.Plugins.PixelDrama.SkillDefs.DemonHunterVengeancePlugin", true);
            Hud.TryTogglePlugin("Turbo.Plugins.PixelDrama.SkillDefs.DemonHunterPreparationPunishmentPlugin", true);
            Hud.TryTogglePlugin("Turbo.Plugins.PixelDrama.SkillDefs.DemonHunterPreparationPlugin", true);
            Hud.TryTogglePlugin("Turbo.Plugins.LightningMod.DemonHunterCompanionPlugin", true);
            Hud.TryTogglePlugin("Turbo.Plugins.LightningMod.DemonHunterEntanglingShotPlugin", true);
            // From LM
            Hud.TryTogglePlugin("Turbo.Plugins.LightningMod.DemonHunterBolasPlugin1", true);
            Hud.TryTogglePlugin("Turbo.Plugins.LM.AutoSkillPlugin", true);
            Hud.TryTogglePlugin("Turbo.Plugins.LightningMod.BaseInventoryManagementPlugin1", true);
            Hud.TryTogglePlugin("Turbo.Plugins.LightningMod.UpgradeRareToLengendaryPlugin1", true);
            //Hud.TryTogglePlugin("Turbo.Plugins.LightningMod.DemonHunterStrafeHungeringArrowPlugin1", true);
            Hud.TryTogglePlugin("Turbo.Plugins.LM.DemonHunterStrafeHungeringArrowPlugin", true);
            Hud.TryTogglePlugin("Turbo.Plugins.LightningMod.EnchantPlugin", true);
            Hud.TryTogglePlugin("Turbo.Plugins.LM.EnchantPlugin", true);
            Hud.TryTogglePlugin("Turbo.Plugins.LightningMod.DrinkHealthPotionPlugin", true);
            // Nec
            Hud.TryTogglePlugin("Turbo.Plugins.PixelDrama.SkillDefs.NecDevourPlugin", true);
            Hud.TryTogglePlugin("Turbo.Plugins.PixelDrama.SkillDefs.NecLandOfTheDeadPlugin", true);
            Hud.TryTogglePlugin("Turbo.Plugins.PixelDrama.SkillDefs.NecLeechPlugin", true);
            Hud.TryTogglePlugin("Turbo.Plugins.PixelDrama.SkillDefs.NecSimulacrumPlugin", true);
            Hud.TryTogglePlugin("Turbo.Plugins.LightningMod.NecBoneArmorPlugin", true);
            // ZB

            Hud.TryTogglePlugin("Turbo.Plugins.LM.BarbarianAncientSpearPlugin1", true);
            Hud.TryTogglePlugin("Turbo.Plugins.LM.BarbarianBandofMightPlugin1", true);
            Hud.TryTogglePlugin("Turbo.Plugins.LM.BarbarianBattleRagePlugin1", true);
            Hud.TryTogglePlugin("Turbo.Plugins.LM.BarbarianCallOfTheAncientsPlugin1", true);
            Hud.TryTogglePlugin("Turbo.Plugins.LM.BarbarianIgnorePainPlugin1", true);
            Hud.TryTogglePlugin("Turbo.Plugins.LM.BarbarianLeapPlugin1", true);
            Hud.TryTogglePlugin("Turbo.Plugins.LM.BarbarianOverPowerPlugin1", true);
            Hud.TryTogglePlugin("Turbo.Plugins.LM.BarbarianRendPlugin1", true);
            Hud.TryTogglePlugin("Turbo.Plugins.LM.BarbarianSeismicSlamPlugin1", true);
            Hud.TryTogglePlugin("Turbo.Plugins.LM.BarbarianSprintPlugin1", true);
            Hud.TryTogglePlugin("Turbo.Plugins.LightningMod.BarbarianSprintPlugin", true);
            Hud.TryTogglePlugin("Turbo.Plugins.LightningMod.BarbarianIgnorePainPlugin", true);
            Hud.TryTogglePlugin("Turbo.Plugins.LightningMod.BarbarianWarCryPlugin", true);
            Hud.TryTogglePlugin("Turbo.Plugins.LightningMod.BarbarianThreateningShoutPlugin", true);
            Hud.TryTogglePlugin("Turbo.Plugins.LM.BarbarianThreateningShoutPlugin1", true);
            Hud.TryTogglePlugin("Turbo.Plugins.LM.BarbarianThreateningShoutPlugin1", true);
            Hud.TryTogglePlugin("Turbo.Plugins.LM.BarbarianWarCryPlugin1", true);
            Hud.TryTogglePlugin("Turbo.Plugins.LM.BarbarianWhirlwindPlugin1", true);
            Hud.TryTogglePlugin("Turbo.Plugins.LM.BarbarianWrathOfTheBerserkerPlugin1", true);
            Hud.TryTogglePlugin("Turbo.Plugins.LM.BarbarianSprintPlugin", true);

            // WD
            Hud.TryTogglePlugin("Turbo.Plugins.LightningMod.WitchDoctorSoulHarvestPlugin", true);
            Hud.TryTogglePlugin("Turbo.Plugins.LightningMod.WitchDoctorSoulHarvestJadeHarvesterPlugin", true);
            Hud.TryTogglePlugin("Turbo.Plugins.LightningMod.WitchDoctorHexPlugin", true);

            // Wizz
            Hud.TryTogglePlugin("Turbo.Plugins.LightningMod.WizardDisintegratePlugin", true);
            Hud.TryTogglePlugin("Turbo.Plugins.LightningMod.WizardStormArmorPlugin", true);
            Hud.TryTogglePlugin("Turbo.Plugins.LightningMod.WizardMagicWeaponPlugin", true);
            Hud.TryTogglePlugin("Turbo.Plugins.LightningMod.WizardDiamondSkinPlugin", true);
            Hud.TryTogglePlugin("Turbo.Plugins.LightningMod.WizardExplosiveBlastPlugin", true);

            // Monk
            Hud.TryTogglePlugin("Turbo.Plugins.LightningMod.MonkEpiphanyPlugin", true);
            Hud.TryTogglePlugin("Turbo.Plugins.LightningMod.MonkMantraOfConvictionPlugin", true);
            Hud.TryTogglePlugin("Turbo.Plugins.LightningMod.MonkMantraOfSalvationPlugin", true);
            Hud.TryTogglePlugin("Turbo.Plugins.LightningMod.MonkMantraOfHealingPlugin", true);
            Hud.TryTogglePlugin("Turbo.Plugins.LightningMod.MonkMysticAllyPlugin", true);
            Hud.TryTogglePlugin("Turbo.Plugins.LightningMod.MonkSerenityPlugin", true);

            Hud.TryTogglePlugin("Turbo.Plugins.PixelDrama.SkillDefs.Monk_CripplingWavePlugin", true);
            Hud.TryTogglePlugin("Turbo.Plugins.PixelDrama.SkillDefs.Monk_FistsOfThunderPlugin", true);
            Hud.TryTogglePlugin("Turbo.Plugins.PixelDrama.SkillDefs.Monk_DeadlyReachPlugin", true);
            Hud.TryTogglePlugin("Turbo.Plugins.PixelDrama.SkillDefs.MonkMantraOfRetribution", true);
        }
    }
}
