namespace Turbo.Plugins.LM
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using Turbo.Plugins.Default;

    public enum CombatRole { Default = 0, Support = 1 }

    public enum SkillTestResult { Continue, NoCast, Cast }

    public enum CastType { SimpleSkill, BuffSkill, RangedChannelingSkill }

    public abstract class AbstractSkillHandler1 : BasePlugin, ISkillHandler, ICustomizer
    {
        public void Customize()
        {
            Hud.GetPlugin<DemonHunterBolasPlugin1>().Enabled = Hud.GetPlugin<DemonHunterEvasiveFirePlugin1>().Enabled;
            Hud.GetPlugin<DemonHunterHungeringArrowPlugin1>().Enabled = Hud.GetPlugin<DemonHunterEvasiveFirePlugin1>().Enabled;
            Hud.GetPlugin<NecNayrsBlackDeath_BoneSpikesPlugin1>().Enabled = Hud.GetPlugin<NecNayrsBlackDeath_BoneArmorPlugin1>().Enabled;
            Hud.GetPlugin<BarbarianLeapPlugin1>().Enabled = Hud.GetPlugin<BarbarianBandofMightPlugin1>().Enabled;																										   


        }
        public ISnoPower AssignedSnoPower { get; protected set; }
        public int? Rune { get; protected set; }
        public CastType SkillType { get; }

        public HashSet<CastPhase> SupportedPhases { get; }
        public List<AbstractSkillTest1> CastRules { get; }
        public CombatRole CombatRole { get; protected set; }

        private static IWatch _lastBuffCasted;
        private static IWatch _lastSimpleCasted;
        public static bool DisableAllSkillHandlers { get; set; }
        protected AbstractSkillHandler1(CastType skillType, params CastPhase[] supportedPhases)
        {
            SkillType = skillType;
            SupportedPhases = new HashSet<CastPhase>(supportedPhases);

            CastRules = new List<AbstractSkillTest1>();

            Enabled = true;
        }

        public AbstractSkillTest1 CreateCastRule()
        {
            var root = new CustomTrueTest1()
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
                || Hud.GetPlugin<OpenGreatRiftPlugin1>().Running == true
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
                var context = new TestContext1(Hud, CombatRole, skill, phase, _lastSimpleCasted, _lastBuffCasted);

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
                        Hud.Interaction.StopContinuousAction(skill.Key);
                        if (skill.Key == ActionKey.LeftSkill) Hud.Interaction.StandStillUp();
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