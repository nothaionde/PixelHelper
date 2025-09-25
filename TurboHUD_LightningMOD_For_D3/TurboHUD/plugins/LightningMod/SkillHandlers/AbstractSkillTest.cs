namespace Turbo.Plugins.LightningMod
{
    public abstract class AbstractSkillTest
    {
        public CombatRole? CombatRoleFilter { get; set; }
        public int? RuneFilter { get; set; }

        public AbstractSkillTest NextTest { get; set; }

        internal abstract SkillTestResult Test(TestContext context);

        public SkillTestResult ResultOnSuccess { get; private set; }
        public SkillTestResult ResultOnFail { get; private set; }

        public SkillTestResult LastEvaluationResult { get; private set; }

        public AbstractSkillTest ThenContinueElseNoCast()
        {
            ResultOnSuccess = SkillTestResult.Continue;
            ResultOnFail = SkillTestResult.NoCast;
            return this;
        }

        public AbstractSkillTest ThenContinueElseCast()
        {
            ResultOnSuccess = SkillTestResult.Continue;
            ResultOnFail = SkillTestResult.Cast;
            return this;
        }

        public AbstractSkillTest ThenCastElseContinue()
        {
            ResultOnSuccess = SkillTestResult.Cast;
            ResultOnFail = SkillTestResult.Continue;
            return this;
        }

        public AbstractSkillTest ThenCastElseNoCast()
        {
            ResultOnSuccess = SkillTestResult.Cast;
            ResultOnFail = SkillTestResult.NoCast;
            return this;
        }

        public AbstractSkillTest ThenNoCastElseCast()
        {
            ResultOnSuccess = SkillTestResult.NoCast;
            ResultOnFail = SkillTestResult.Cast;
            return this;
        }

        public AbstractSkillTest ThenNoCastElseContinue()
        {
            ResultOnSuccess = SkillTestResult.NoCast;
            ResultOnFail = SkillTestResult.Continue;
            return this;
        }

        public AbstractSkillTest ForSpecificCombatRole(CombatRole role)
        {
            CombatRoleFilter = role;
            return this;
        }

        public AbstractSkillTest ForSpecificRune(int rune)
        {
            RuneFilter = rune;
            return this;
        }

        public SkillTestResult Evaluate(TestContext context)
        {
            LastEvaluationResult = SkillTestResult.Continue;

            var skip = false;
            if (CombatRoleFilter != null && CombatRoleFilter.Value != context.Role) skip = true;
            if (RuneFilter != null && RuneFilter.Value != context.Skill.Rune) skip = true;

            if (!skip)
            {
                LastEvaluationResult = Test(context);
            }

            if (LastEvaluationResult == SkillTestResult.Continue && NextTest != null)
            {
                return NextTest.Evaluate(context);
            }

            return LastEvaluationResult;
        }
    }
}