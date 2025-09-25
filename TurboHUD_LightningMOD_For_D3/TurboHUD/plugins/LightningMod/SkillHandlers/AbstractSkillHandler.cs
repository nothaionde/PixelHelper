namespace Turbo.Plugins.LightningMod
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using Turbo.Plugins.Default;

    public enum CombatRole { Default = 0, Support = 1 }

    public enum SkillTestResult { Continue, NoCast, Cast }

    public enum CastType { SimpleSkill, BuffSkill, RangedChannelingSkill }

    public abstract class AbstractSkillHandler : BasePlugin, ISkillHandler, ICustomizer
    {
        public void Customize()
        {
            Hud.GetPlugin<DemonHunterEntanglingShotPlugin>().Enabled = Hud.GetPlugin<DemonHunterEvasiveFirePlugin>().Enabled;
            Hud.GetPlugin<DemonHunterBolasPlugin>().Enabled = Hud.GetPlugin<DemonHunterEvasiveFirePlugin>().Enabled;
            Hud.GetPlugin<DemonHunterHungeringArrowPlugin>().Enabled = Hud.GetPlugin<DemonHunterEvasiveFirePlugin>().Enabled;
            Hud.GetPlugin<NecNayrsBlackDeath_BoneSpikesPlugin>().Enabled = Hud.GetPlugin<NecNayrsBlackDeath_BoneArmorPlugin>().Enabled;
            Hud.GetPlugin<BarbarianLeapPlugin>().Enabled = Hud.GetPlugin<BarbarianBandofMightPlugin>().Enabled;
            Hud.GetPlugin<MonkFistsOfThunderPlugin>().Enabled = Hud.GetPlugin<MonkDeadlyReachPlugin>().Enabled;
            Hud.GetPlugin<WizardArchonArcaneBlastColdPlugin>().Enabled = Hud.GetPlugin<WizardArchonArcaneBlastPlugin>().Enabled;
            Hud.GetPlugin<WizardArchonArcaneBlastFirePlugin>().Enabled = Hud.GetPlugin<WizardArchonArcaneBlastPlugin>().Enabled;
            Hud.GetPlugin<WizardArchonArcaneBlastLightningPlugin>().Enabled = Hud.GetPlugin<WizardArchonArcaneBlastPlugin>().Enabled;
            Hud.GetPlugin<WizardTriumvirate_SpectralBladePlugin>().Enabled = Hud.GetPlugin<WizardTriumvirate_ShockPulsePlugin>().Enabled;
            Hud.GetPlugin<WizardTriumvirate_MagicMissilePlugin>().Enabled = Hud.GetPlugin<WizardTriumvirate_ShockPulsePlugin>().Enabled;
            Hud.GetPlugin<WizardTriumvirate_ElectrocutePlugin>().Enabled = Hud.GetPlugin<WizardTriumvirate_ShockPulsePlugin>().Enabled;
            Hud.GetPlugin<NecNayrsBlackDeath_BoneSpearPlugin>().Enabled = Hud.GetPlugin<NecNayrsBlackDeath_BoneArmorPlugin>().Enabled;
            Hud.GetPlugin<NecNayrsBlackDeath_SiphonBloodPlugin>().Enabled = Hud.GetPlugin<NecNayrsBlackDeath_BoneArmorPlugin>().Enabled;
            Hud.GetPlugin<NecNayrsBlackDeath_SkeletalMagePlugin>().Enabled = Hud.GetPlugin<NecNayrsBlackDeath_BoneArmorPlugin>().Enabled;
            Hud.GetPlugin<NecNayrsBlackDeath_DeathNovaPlugin>().Enabled = Hud.GetPlugin<NecNayrsBlackDeath_BoneArmorPlugin>().Enabled;
            Hud.GetPlugin<NecNayrsBlackDeath_CommandSkeletonsPlugin>().Enabled = Hud.GetPlugin<NecNayrsBlackDeath_BoneArmorPlugin>().Enabled;
            Hud.GetPlugin<NecNayrsBlackDeath_RevivePlugin>().Enabled = Hud.GetPlugin<NecNayrsBlackDeath_BoneArmorPlugin>().Enabled;

        }
        public ISnoPower AssignedSnoPower { get; protected set; }
        public int? Rune { get; protected set; }
        public CastType SkillType { get; }

        public HashSet<CastPhase> SupportedPhases { get; }
        public List<AbstractSkillTest> CastRules { get; }
        public CombatRole CombatRole { get; protected set; }

        private static IWatch _lastBuffCasted;
        private static IWatch _lastSimpleCasted;
        public static bool DisableAllSkillHandlers { get; set; }
        protected AbstractSkillHandler(CastType skillType, params CastPhase[] supportedPhases)
        {
            SkillType = skillType;
            SupportedPhases = new HashSet<CastPhase>(supportedPhases);

            CastRules = new List<AbstractSkillTest>();

            Enabled = true;
        }

        public AbstractSkillTest CreateCastRule()
        {
            var root = new CustomTrueTest()
            {
                TestFunc = ctx => true,
            };

            root.ThenContinueElseNoCast();

            CastRules.Add(root);
            return root;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);

            if (_lastBuffCasted == null)
                _lastBuffCasted = Hud.Time.CreateWatch();

            if (_lastSimpleCasted == null)
                _lastSimpleCasted = Hud.Time.CreateWatch();
        }

        public void HandleCastPhase(IPlayerSkill skill, CastPhase phase)
        {
            if (!Hud.Window.IsForeground
                || (!Hud.Render.MinimapUiElement.Visible && skill.Key != ActionKey.Heal)
                || Hud.Render.ActMapUiElement.LastVisibleSystemTick > DateTime.Now.Ticks - (500 * 10000)
                || Hud.Render.WorldMapUiElement.LastVisibleSystemTick > DateTime.Now.Ticks - (500 * 10000)
                || skill.Player.AnimationState == AcdAnimationState.Transform
                || skill.Player.IsDead
                || skill.Player.Powers.BuffIsActive(Hud.Sno.SnoPowers.Generic_AxeOperateGizmo.Sno)//悬赏任务读条
                || skill.Player.Powers.BuffIsActive(Hud.Sno.SnoPowers.Generic_ActorGhostedBuff.Sno)
                || (DisableAllSkillHandlers && skill.SnoPower != Hud.Sno.SnoPowers.Generic_DrinkHealthPotion)
                || Hud.GetPlugin<OpenGreatRiftPlugin>().Running == true
                || (skill.Player.Powers.BuffIsActive(Hud.Sno.SnoPowers.ShukranisTriumph.Sno) && skill.Player.Powers.BuffIsActive(Hud.Sno.SnoPowers.WitchDoctor_SpiritWalk.Sno) && skill.Player.AnimationState != AcdAnimationState.Attacking) //舒克拉尼的胜利且处于灵行时
                )
                
            {
                return;
            }

            if (skill.Key == ActionKey.LeftSkill)
            {
                if (!Hud.Window.CursorInsideGroundRect())
                    return;
            }
            else if (!Hud.Window.CursorInsideGameWindow())
            {
                return;
            }

            var canCast = false;
            if (!skill.Player.IsDeadSafeCheck)
            {
                var context = new TestContext(Hud, CombatRole, skill, phase, _lastSimpleCasted, _lastBuffCasted);

                foreach (var test in CastRules)
                {
                    var result = test.Evaluate(context);
                    if (result == SkillTestResult.Cast)
                    {
                        canCast = true;
                        break;
                    }
                }
            }

            if (canCast)
            {
                skill.LastUsed.Restart();
                switch (SkillType)
                {
                    case CastType.SimpleSkill:
                        _lastSimpleCasted.Restart();
                        Hud.Interaction.DoActionAutoShift(skill.Key);
                        //Hud.Debug("cast " + skill.SnoPower.NameLocalized);
                        break;
                    case CastType.BuffSkill:
                        _lastBuffCasted.Restart();
                        Hud.Interaction.DoActionAutoShift(skill.Key);
                        //Hud.Debug("cast " + skill.SnoPower.NameLocalized);
                        break;
                    case CastType.RangedChannelingSkill:
                        Hud.Interaction.StartContinuousAction(skill.Key, skill.Key == ActionKey.LeftSkill);
                        //Hud.Debug("start channeling " + skill.SnoPower.NameLocalized);
                        break;
                }
            }
            else
            {
                switch (SkillType)
                {
                    case CastType.SimpleSkill:
                        break;
                    case CastType.BuffSkill:
                        break;
                    case CastType.RangedChannelingSkill:
                        if(Hud.Interaction.IsContinuousActionStarted(skill.Key))
                        {
                            Hud.Interaction.StopContinuousAction(skill.Key);
                            if (skill.Key == ActionKey.LeftSkill)
                                Hud.Interaction.StandStillUp();
                        }
                        skill.LastReleased.Restart();
                        //Hud.Debug("stop channeling " + skill.SnoPower.NameLocalized);
                        break;
                }
            }
        }

        public static int NextRandom(int min, int max)
        {
            lock (_random)
            {
                return _random.Next(min, max + 1);
            }
        }

        public static int ChangeRnd(IController hud, string id, int min, int max, int changeAfterMilliseconds)
        {
            _randomPool.TryGetValue(id, out var node);

            if (node == null)
            {
                node = new RndNode()
                {
                    LastChangedOn = hud.Time.CreateAndStartWatch(),
                    LastValue = NextRandom(min, max),
                    Id = id
                };

                _randomPool.Add(id, node);
            }

            if (node.LastChangedOn.ElapsedMilliseconds >= changeAfterMilliseconds)
            {
                node.LastChangedOn.Restart();
                node.LastValue = NextRandom(min, max);
            }

            return node.LastValue;
        }

        public virtual void CalculatePriority(IPlayerSkill skill, IMonster monster, ref double priority)
        {
        }

        private class RndNode
        {
            public string Id;
            public IWatch LastChangedOn;
            public int LastValue;
        }

        private static readonly Dictionary<string, RndNode> _randomPool = new Dictionary<string, RndNode>();
        private static readonly Random _random = new Random(Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture).GetHashCode());
    }
}