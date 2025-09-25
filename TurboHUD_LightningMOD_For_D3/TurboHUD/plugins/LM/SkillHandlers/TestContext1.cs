namespace Turbo.Plugins.LM
{
    public class TestContext1
    {
        public IController Hud { get; }
        public CombatRole Role { get; }
        public IPlayerSkill Skill { get; }
        public CastPhase Phase { get; }
        public IWatch LastSimpleCasted { get; }
        public IWatch LastBuffCasted { get; }

        public TestContext1(IController hud, CombatRole role, IPlayerSkill skill, CastPhase phase, IWatch lastSimpleCasted, IWatch lastBuffCasted)
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