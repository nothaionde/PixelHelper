namespace Turbo.Plugins.LightningMod
{
    public class RunningTest : AbstractSkillTest
    {
        public bool Skill { get; set; }
        internal override SkillTestResult Test(TestContext context)
        {
            return context.Hud.Game.Me.AnimationState == AcdAnimationState.Running && Skill || (context.Hud.Game.Me.AnimationState == AcdAnimationState.Running && !Skill && //这些技能默认就是为running状态，为true时不需要额外判断
                !context.Hud.Game.Me.Powers.BuffIsActive(context.Hud.Sno.SnoPowers.Monk_TempestRush.Sno, 0) &&//风雷冲
                !context.Hud.Game.Me.Powers.BuffIsActive(context.Hud.Sno.SnoPowers.DemonHunter_Strafe.Sno, 0) &&//扫射
                !context.Hud.Game.Me.Powers.BuffIsActive(context.Hud.Sno.SnoPowers.Barbarian_Whirlwind.Sno, 0))//旋风斩
                ? ResultOnSuccess : ResultOnFail;
        }
    }

    public static class RunningTestFluent
    {
        public static RunningTest IfRunning(this AbstractSkillTest parent, bool skill = false)
        {
            var test = new RunningTest()
            {
                Skill = skill,//True时引导旋风斩等技能判断为正在移动,false时可旋风斩等技能判断为非移动状态
            };
            parent.NextTest = test;
            return test;
        }
    }
}