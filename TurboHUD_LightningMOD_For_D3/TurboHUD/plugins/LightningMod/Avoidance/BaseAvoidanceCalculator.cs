namespace Turbo.Plugins.LightningMod
{
    using System;
    using System.Linq;
    using Turbo.Plugins.Default;

    public class AvoidanceCalculator : BasePlugin, IAvoidanceCalculator
    {
        public AvoidanceCalculator()
        {
            Enabled = true;
        }

        public bool Calculate()
        {
            if (Hud.Game.Me.Powers.CantMove)
                return false;
            if (Hud.Game.Me.AvoidablesInRange.Count == 0)
                return false;
            if (Hud.Game.SpecialArea == SpecialArea.UberFight)
            {
                if (!Hud.Game.AliveMonsters.Any())
                    return false;
            }

            var any = 0.0d;
            var normal = 0;
            var bigHit = 0;
            var bigDot = 0;

            foreach (var avoid in Hud.Game.Me.AvoidablesInRange)
            {
                var weight = avoid.AvoidableDefinition.Weight;
                if (Hud.Game.ActorQuery.NearestBoss != null)
                {
                    switch (Hud.Game.ActorQuery.NearestBoss.SnoActor.Sno)
                    {
                        case ActorSnoEnum._bigred_izual: // Izual
                        case ActorSnoEnum._izualbossworld: // Izual
                        case ActorSnoEnum._x1_lr_boss_bigred_izual: // Cold Snap
                            if (avoid.AvoidableDefinition.Type == AvoidableType.IceBalls)
                                weight = AvoidableWeight.Low;
                            break;
                    }
                }

                if ((avoid.AvoidableDefinition.Type == AvoidableType.Arcane) || (avoid.AvoidableDefinition.Type == AvoidableType.ArcaneSpawn))
                {
                    if (avoid.CentralXyDistanceToMe <= 8)
                        return true;
                    foreach (var player in Hud.Game.Players)
                    {
                        if (!player.IsDeadSafeCheck && (player.Defense.HealthPct < 80))
                            return true;
                    }
                }

                if (avoid.AvoidableDefinition.InstantDeath)
                {
                    if (Hud.Game.Me.HeroIsHardcore)
                    {
                        if (Hud.Game.Me.Defense.HealthPct < 99)
                            return true;
                    }
                    else
                    {
                        if ((Hud.Game.Me.Defense.HealthPct < 85) && Hud.Game.Me.Powers.HealthPotionSkill.IsOnCooldown)
                            return true;
                        if (Hud.Game.Me.Defense.HealthPct < 70)
                            return true;
                    }

                    bigHit++;
                }
                else if (weight == AvoidableWeight.BigHit)
                {
                    bigHit++;
                }
                else if (weight == AvoidableWeight.BigDot)
                {
                    bigDot++;
                }
                else
                {
                    normal++;
                }

                any += (weight == AvoidableWeight.Low) ? 0.5d : 1.0d;
            }

            if (any < 1.0d)
                return false;

            var extraTough = 0;
            if (!Hud.Game.Me.HeroIsHardcore)
            {
                if (!Hud.Game.Me.Powers.HealthPotionSkill.IsOnCooldown && (Hud.Game.Me.Defense.HealthPct >= 90))
                    extraTough++;
                if (!Hud.Game.Me.HeroClassDefinition.IsRanged)
                {
                    extraTough++;
                    if (Hud.Game.Me.Defense.CurShield > 0)
                        extraTough++;
                }

                if ((Hud.Game.SpecialArea != SpecialArea.GreaterRift) && (Hud.Game.SpecialArea != SpecialArea.UberFight))
                {
                    if (Hud.Game.Me.Defense.EhpCur >= 50 * 1000 * 1000)
                        extraTough++;
                    if (Hud.Game.Me.Defense.EhpCur >= 60 * 1000 * 1000)
                        extraTough++;
                    if (Hud.Game.Me.Defense.EhpCur >= 70 * 1000 * 1000)
                        extraTough++;
                }
            }

            if (Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.Monk_InnerSanctuary.Sno))
                extraTough += 2;
            if (Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.Barbarian_IgnorePain.Sno))
                extraTough += 2;

            extraTough += Convert.ToInt32(Hud.Game.Me.Defense.LifeRegen / 200000);

            if (Hud.Game.Me.Powers.HealthPotionSkill.IsOnCooldown)
                extraTough -= 2;

            if (extraTough > 0)
            {
                bigHit -= extraTough / 2;
                bigDot -= extraTough;
                any -= extraTough;
                normal -= extraTough;
            }

            if ((bigHit >= 1) && (any >= 2))
                return true;
            if ((bigDot >= 2) && (any >= 3))
                return true;
            if ((bigHit >= 1) && (Hud.Game.Me.Defense.HealthPct < 70))
                return true;
            if ((bigHit >= 2) && ((Hud.Game.Me.Defense.HealthPct < 80) || Hud.Game.Me.Powers.HealthPotionSkill.IsOnCooldown))
                return true;
            if ((bigHit >= 3) && ((Hud.Game.Me.Defense.HealthPct < 90) || Hud.Game.Me.Powers.HealthPotionSkill.IsOnCooldown))
                return true;

            if ((Hud.Game.Me.Defense.HealthPct >= 80) && (normal <= 2) && (bigHit <= 0) && (bigDot <= 1))
                return false;
            if ((Hud.Game.Me.Defense.HealthPct >= 40) && (any <= 2))
                return false;
            if ((Hud.Game.Me.Defense.HealthPct >= 60) && (any <= 4))
                return false;
            if ((Hud.Game.Me.Defense.HealthPct >= 80) && (any <= 6))
                return false;
            if ((Hud.Game.Me.Defense.HealthPct >= 90) && (any <= 8))
                return false;
#pragma warning disable IDE0046 // Convert to conditional expression
            if (Hud.Game.Me.Defense.HealthPct >= 99)
#pragma warning restore IDE0046 // Convert to conditional expression
                return false;

            return true;
        }
    }
}