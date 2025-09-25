namespace Turbo.Plugins.LM
{
    public abstract class AbstractSkillTest1
    {
        public CombatRole? CombatRoleFilter { get; set; }
        public int? RuneFilter { get; set; }

        public AbstractSkillTest1 NextTest { get; set; }

        internal abstract SkillTestResult Test(TestContext1 context);

        public SkillTestResult ResultOnSuccess { get; private set; }
        public SkillTestResult ResultOnFail { get; private set; }

        public SkillTestResult LastEvaluationResult { get; private set; }

        public AbstractSkillTest1 ThenContinueElseNoCast()
        {
            ResultOnSuccess = SkillTestResult.Continue;
            ResultOnFail = SkillTestResult.NoCast;
            return this;
        }

        public AbstractSkillTest1 ThenContinueElseCast()
        {
            ResultOnSuccess = SkillTestResult.Continue;
            ResultOnFail = SkillTestResult.Cast;
            return this;
        }

        public AbstractSkillTest1 ThenCastElseContinue()
        {
            ResultOnSuccess = SkillTestResult.Cast;
            ResultOnFail = SkillTestResult.Continue;
            return this;
        }

        public AbstractSkillTest1 ThenCastElseNoCast()
        {
            ResultOnSuccess = SkillTestResult.Cast;
            ResultOnFail = SkillTestResult.NoCast;
            return this;
        }

        public AbstractSkillTest1 ThenNoCastElseCast()
        {
            ResultOnSuccess = SkillTestResult.NoCast;
            ResultOnFail = SkillTestResult.Cast;
            return this;
        }

        public AbstractSkillTest1 ThenNoCastElseContinue()
        {
            ResultOnSuccess = SkillTestResult.NoCast;
            ResultOnFail = SkillTestResult.Continue;
            return this;
        }

        public AbstractSkillTest1 ForSpecificCombatRole(CombatRole role)
        {
            CombatRoleFilter = role;
            return this;
        }

        public AbstractSkillTest1 ForSpecificRune(int rune)
        {
            RuneFilter = rune;
            return this;
        }

        public SkillTestResult Evaluate(TestContext1 context)
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