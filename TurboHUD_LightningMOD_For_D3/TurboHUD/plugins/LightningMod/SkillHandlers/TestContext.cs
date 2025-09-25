namespace Turbo.Plugins.LightningMod
{
    public class TestContext
    {
        public IController Hud { get; }
        public CombatRole Role { get; }
        public IPlayerSkill Skill { get; }
        public CastPhase Phase { get; }
        public IWatch LastSimpleCasted { get; }
        public IWatch LastBuffCasted { get; }

        public TestContext(IController hud, CombatRole role, IPlayerSkill skill, CastPhase phase, IWatch lastSimpleCasted, IWatch lastBuffCasted)
        {
            Hud = hud;
            Role = role;
            Skill = skill;
            Phase = phase;
            LastSimpleCasted = lastSimpleCasted;
            LastBuffCasted = lastBuffCasted;
        }
    }
}