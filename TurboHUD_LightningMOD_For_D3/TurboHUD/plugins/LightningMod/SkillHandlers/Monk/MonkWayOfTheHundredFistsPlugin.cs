using Turbo.Plugins.glq;

namespace Turbo.Plugins.LightningMod
{
    public class MonkWayOfTheHundredFistsPlugin : AbstractSkillHandler, ISkillHandler
    {
        public MonkWayOfTheHundredFistsPlugin()
            : base(CastType.SimpleSkill, CastPhase.AutoCast, CastPhase.UseWpStart, CastPhase.Move, CastPhase.Attack, CastPhase.AttackIdle)
        {
            Enabled = false;
        }
        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.Monk_WayOfTheHundredFists;
            CreateCastRule()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfTrue(ctx =>
                {
                    if (ctx.Skill.Player.GetSetItemCount(755275) >= 6 && ctx.Skill.Player.GetSetItemCount(563257) >= 2)
                    {
                        return true;
                    }
                    return false;
                }).ThenContinueElseNoCast()
                .IfTrue(ctx =>
                {
                    if(Hud.Game.Me.Animation.ToString().Contains("_rapidstrikes_"))//正在百烈拳
                    {
                        return false;
                    }
                    if (Hud.Game.Me.Animation.ToString().Contains("_debilitatingblows_"))//正在伏魔破
                    {
                        return true;
                    }
                    return false;
                }).ThenContinueElseNoCast()
                .IfTrue(ctx =>
                {
                    var rune = ctx.Skill.Rune;
                    var buff = ctx.Skill.Player.Powers.GetBuff(Hud.Sno.SnoPowers.Monk_WayOfTheHundredFists.Sno);
                    if (ctx.Skill.Player.Powers.BuffIsActive(Hud.Sno.SnoPowers.Monk_Passive_CombinationStrike.Sno))//融会贯通
                    {
                        if (PublicClassPlugin.GetBuffLeftTime(ctx.Hud, Hud.Sno.SnoPowers.Monk_Passive_CombinationStrike.Sno, 5) < 1)
                        {
                            return true;
                        }
                    }
                    if (rune == 2)//Blazing Fists奔熾拳
                    {
                        
                        
                        if (buff != null)
                        {
                            if (buff.IconCounts[1] < 3 || buff.TimeLeftSeconds[1] < 1)
                            {
                                return true;
                            }
                        }
                        else
                        {
                            return true;
                        }
                    }
                    if(rune == 3)//Assimilation聚勁擊
                    {
                        if (buff != null)
                        {
                            if (buff.TimeLeftSeconds[4] < 1)
                            {
                                return true;
                            }
                        }
                        else
                        {
                            return true;
                        }
                    }
                    return false;
                }).ThenCastElseContinue()
                
                ;
        }
    }
}