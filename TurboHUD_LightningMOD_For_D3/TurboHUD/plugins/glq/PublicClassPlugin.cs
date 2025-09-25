using System;
using System.Linq;
using System.Collections.Generic;
using Turbo.Plugins.Default;
using SharpDX.Direct2D1;

namespace Turbo.Plugins.glq
{

    public class PublicClassPlugin : BasePlugin
    {
        private static int secondCounting = 0;
        private static bool speechSwitch = false;
        private static long speechSwitchMsec = 0;
        private static BuffRuleCalculator _ruleCalculator;
        public int CoeIndex { get; set; } = 0;
        public int PartyCoeIndex { get; set; } = 0;
        public PublicClassPlugin()
        {
        }
        public static IEnumerable<BuffRule> GetCurrentRules(HeroClass heroClass)
        {
            for (int i = 1; i <= 7; i++)
            {
                switch (heroClass)
                {
                    case HeroClass.Barbarian:
                        if (i == 1 || i == 4 || i == 7)
                            continue;
                        break;
                    case HeroClass.Crusader:
                        if (i == 1 || i == 2 || i == 7)
                            continue;
                        break;
                    case HeroClass.DemonHunter:
                        if (i == 1 || i == 4 || i == 7)
                            continue;
                        break;
                    case HeroClass.Monk:
                        if (i == 1 || i == 7)
                            continue;
                        break;
                    case HeroClass.Necromancer:
                        if (i == 1 || i == 3 || i == 4 || i == 5)
                            continue;
                        break;
                    case HeroClass.WitchDoctor:
                        if (i == 1 || i == 4 || i == 5)
                            continue;
                        break;
                    case HeroClass.Wizard:
                        if (i == 4 || i == 6 || i == 7)
                            continue;
                        break;
                }
                yield return _ruleCalculator.Rules[i - 1];
            }
        }

        public static double GetHighestElementLeftSecondAssingedPlayer(IController Hud, IPlayer player, int CoeIndex = 0)
        {
            var buff = player.Powers.GetBuff(430674);
            if ((buff == null) || (buff.IconCounts[0] <= 0))
                return 0;

            _ruleCalculator = new BuffRuleCalculator(Hud);
            _ruleCalculator.SizeMultiplier = 0.55f;

            _ruleCalculator.Rules.Add(new BuffRule(430674) { IconIndex = 1, MinimumIconCount = 0, DisableName = true }); // Arcane
            _ruleCalculator.Rules.Add(new BuffRule(430674) { IconIndex = 2, MinimumIconCount = 0, DisableName = true }); // Cold
            _ruleCalculator.Rules.Add(new BuffRule(430674) { IconIndex = 3, MinimumIconCount = 0, DisableName = true }); // Fire
            _ruleCalculator.Rules.Add(new BuffRule(430674) { IconIndex = 4, MinimumIconCount = 0, DisableName = true }); // Holy
            _ruleCalculator.Rules.Add(new BuffRule(430674) { IconIndex = 5, MinimumIconCount = 0, DisableName = true }); // Lightning
            _ruleCalculator.Rules.Add(new BuffRule(430674) { IconIndex = 6, MinimumIconCount = 0, DisableName = true }); // Physical
            _ruleCalculator.Rules.Add(new BuffRule(430674) { IconIndex = 7, MinimumIconCount = 0, DisableName = true }); // Poison

            var classSpecificRules = GetCurrentRules(player.HeroClassDefinition.HeroClass);

            _ruleCalculator.CalculatePaintInfo(player, classSpecificRules);

            if (_ruleCalculator.PaintInfoList.Count == 0)
                return 0;
            if (!_ruleCalculator.PaintInfoList.Any(info => info.TimeLeft > 0))
                return 0;

            var highestElementalBonus = player.Offense.HighestElementalDamageBonus;
            for (int i = 0; i < _ruleCalculator.PaintInfoList.Count; i++)
            {
                var info = _ruleCalculator.PaintInfoList[0];
                if (info.TimeLeft <= 0)
                {
                    _ruleCalculator.PaintInfoList.RemoveAt(0);
                    _ruleCalculator.PaintInfoList.Add(info);
                }
                else
                    break;
            }


            for (int orderIndex = 0; orderIndex < _ruleCalculator.PaintInfoList.Count; orderIndex++)
            {
                var info = _ruleCalculator.PaintInfoList[orderIndex];
                var best = false;
                if (CoeIndex == 0)
                {
                    switch (info.Rule.IconIndex)
                    {
                        case 1:
                            best = player.Offense.BonusToArcane == highestElementalBonus;
                            break;
                        case 2:
                            best = player.Offense.BonusToCold == highestElementalBonus;
                            break;
                        case 3:
                            best = player.Offense.BonusToFire == highestElementalBonus;
                            break;
                        case 4:
                            best = player.Offense.BonusToHoly == highestElementalBonus;
                            break;
                        case 5:
                            best = player.Offense.BonusToLightning == highestElementalBonus;
                            break;
                        case 6:
                            best = player.Offense.BonusToPhysical == highestElementalBonus;
                            break;
                        case 7:
                            best = player.Offense.BonusToPoison == highestElementalBonus;
                            break;
                    }
                }
                else
                {
                    best = info.Rule.IconIndex == CoeIndex;
                }
                if (best)
                {
                    if (orderIndex > 0)
                        info.TimeLeft = (orderIndex - 1) * 4 + _ruleCalculator.PaintInfoList[0].TimeLeft;
                    else if (orderIndex == 0)
                        info.TimeLeft = (_ruleCalculator.PaintInfoList.Count - 1) * 4 + _ruleCalculator.PaintInfoList[0].TimeLeft;

                    return info.TimeLeft;
                }
            }
            return 0;
        }

        public static int GetHighestElement(IController Hud, IPlayer player, int CoeIndex = 0)
        {
            int hi = 0;
            if (CoeIndex > 0)
            {
                return hi = CoeIndex;
            }

            var buff = player.Powers.GetBuff(430674);
            if ((buff != null) && buff.Active)
            {
                //判断哪个元素伤害最高
                var highestElementalBonus = player.Offense.HighestElementalDamageBonus;
                if (player.Offense.BonusToArcane == highestElementalBonus)
                    hi = 1;
                else if (player.Offense.BonusToCold == highestElementalBonus)
                    hi = 2;
                else if (player.Offense.BonusToFire == highestElementalBonus)
                    hi = 3;
                else if (player.Offense.BonusToHoly == highestElementalBonus)
                    hi = 4;
                else if (player.Offense.BonusToLightning == highestElementalBonus)
                    hi = 5;
                else if (player.Offense.BonusToPhysical == highestElementalBonus)
                    hi = 6;
                else if (player.Offense.BonusToPoison == highestElementalBonus)
                    hi = 7;
            }
            return hi;
        }
        public static int GetHighestElementPrevious(IController Hud, IPlayer player, int CoeIndex = 0)
        {
            int hi = 0;
            int pi = 0;

            hi = CoeIndex != 0 ? CoeIndex : GetHighestElement(Hud, player, CoeIndex);
            //根据职业判断最高元素伤害的上一个元素
            switch (player.HeroClassDefinition.HeroClass)
            {
                case HeroClass.Barbarian:
                    switch (hi)
                    {
                        case 2:
                            pi = 6;
                            break;
                        case 3:
                            pi = 2;
                            break;
                        case 5:
                            pi = 3;
                            break;
                        case 6:
                            pi = 5;
                            break;
                    }
                    break;
                case HeroClass.DemonHunter:
                    switch (hi)
                    {
                        case 3:
                        case 6:
                            pi = hi - 1;
                            break;
                        case 2:
                            pi = 6;
                            break;
                        case 5:
                            pi = 3;
                            break;
                    }
                    break;
                case HeroClass.Crusader:
                    switch (hi)
                    {
                        case 3:
                            pi = 6;
                            break;
                        case 4:
                        case 5:
                        case 6:
                            pi = hi - 1;
                            break;
                    }
                    break;
                case HeroClass.Monk:
                    switch (hi)
                    {
                        case 2:
                            pi = 6;
                            break;
                        case 3:
                        case 4:
                        case 5:
                        case 6:
                            pi = hi - 1;
                            break;
                    }
                    break;
                case HeroClass.WitchDoctor:
                    switch (hi)
                    {
                        case 2:
                            pi = 7;
                            break;
                        case 3:
                        case 7:
                            pi = hi - 1;
                            break;
                        case 6:
                            pi = 3;
                            break;
                    }
                    break;
                case HeroClass.Wizard:
                    switch (hi)
                    {
                        case 1:
                            pi = 5;
                            break;
                        case 2:
                        case 3:
                            pi = hi - 1;
                            break;
                        case 5:
                            pi = 3;
                            break;
                    }
                    break;
                case HeroClass.Necromancer:
                    switch (hi)
                    {
                        case 2:
                            pi = 7;
                            break;
                        case 6:
                            pi = 2;
                            break;
                        case 7:
                            pi = 6;
                            break;
                    }
                    break;
            }
            return pi;
        }
        public static bool IsElementReadySoon(IController Hud, double second, IPlayer player, int CoeIndex = 0)
        {
            var buff = player.Powers.GetBuff(430674);
            if (buff == null)
                return false;
            if (!buff.Active)
                return false;

            var pi = GetHighestElementPrevious(Hud, player, CoeIndex);
            //返回是否即将到最高元素伤害档位
            if ((pi < buff.TimeLeftSeconds.Length) && (buff.TimeLeftSeconds[pi] > 0) && (buff.TimeLeftSeconds[pi] < second))
                return true;
            return false;
        }

        public static bool IsElementReady(IController Hud, double second, IPlayer player, int CoeIndex = 0)
        {
            var buff = player.Powers.GetBuff(430674);
            if (buff == null)
                return false;
            if (!buff.Active)
                return false;
            var hi = GetHighestElement(Hud, player, CoeIndex);
            var pi = GetHighestElementPrevious(Hud, player, CoeIndex);
            //返回是否即将到最高元素伤害档位
            if ((pi < buff.TimeLeftSeconds.Length) && (buff.TimeLeftSeconds[pi] > 0) && (buff.TimeLeftSeconds[pi] < second))
                return true;
            //正处于最高元素周期
            if ((hi < buff.TimeLeftSeconds.Length) && (buff.TimeLeftSeconds[hi] > 0))
                return true;
            return false;
        }
        public static bool IsElementOverSoon(IController Hud, double second, IPlayer player, int CoeIndex = 0)
        {
            var buff = player.Powers.GetBuff(430674);
            if (buff == null)
                return false;
            if (!buff.Active)
                return false;

            var hi = GetHighestElement(Hud, player, CoeIndex);

            //正处于最高元素周期
            if ((hi < buff.TimeLeftSeconds.Length) && (buff.TimeLeftSeconds[hi] > 0) && (buff.TimeLeftSeconds[hi] < second))
                return true;
            return false;
        }
        public static double GetHighestElementLeftSecond(IController Hud, IPlayer player = null, int CoeIndex = 0)
        {
            if (player == null)
            {
                player = Hud.Game.Me;
            }
            return GetHighestElementLeftSecondAssingedPlayer(Hud, player, CoeIndex);
        }
        public static bool BuffIsTrueActive(IController Hud, uint sno)
        {
            var buff = Hud.Game.Me.Powers.GetBuff(sno);
            if (buff == null)
                return false;
            if (!buff.Active)
                return false;   //这个active只是表示有该状态存在，比如带了一个具有特效的装备， 对应的buff就是active
            for (int i = 0; i < buff.TimeLeftSeconds.Length; i++)
            {
                if (buff.TimeLeftSeconds[i] > 0)
                    return true;
            }
            return false;
        }

        public static bool BuffIsExist(IController Hud, uint sno)
        {
            var buff = Hud.Game.Me.Powers.GetBuff(sno);
            if (buff == null)
                return false;
            if (!buff.Active)
                return false;
            return true;
        }

        public static bool SpeechPrompt(IController Hud, string prompt, int ms)
        {
            if (Hud.Sound.LastSpeak.TimerTest(ms))
            {
                Hud.Sound.Speak(prompt);
                return true;
            }
            return false;
        }
        public static bool SpeechPrompt(IController Hud, uint sno, string prompt, int ms)
        {
            if (Timer.Delay(Hud, sno, ms))
            {
                Hud.Sound.Speak(prompt);
                return true;
            }
            return false;
        }


        public static double GetBuffLeftTimeAssignedPlayer(IController Hud, IPlayer player, uint sno, int index)
        {
            var buff = player.Powers.GetBuff(sno);
            if (buff != null)
                return buff.TimeLeftSeconds[index];
            return 0;
        }
        public static double GetBuffLeftTime(IController Hud, uint sno, int index)
        {
            var buff = Hud.Game.Me.Powers.GetBuff(sno);
            if (buff != null)
                return buff.TimeLeftSeconds[index];
            return 0;
        }
        public static int GetBuffCountAssignedPlayer(IController Hud, IPlayer player, uint sno, int index)
        {
            var buff = player.Powers.GetBuff(sno);
            if (buff != null)
                return buff.IconCounts[index];
            return 0;
        }
        public static int GetBuffCount(IController Hud, uint sno, int index)
        {
            var buff = Hud.Game.Me.Powers.GetBuff(sno);
            if (buff != null)
                return buff.IconCounts[index];
            return 0;
        }

        public static double GetCooldownTimerOfArchon(IController Hud)
        {
            var usedSkill = Hud.Game.Me.Powers.UsedWizardPowers.Archon;
            if ((usedSkill != null) && usedSkill.IsOnCooldown && (usedSkill.CooldownFinishTick > Hud.Game.CurrentGameTick))
            {
                return (usedSkill.CooldownFinishTick - Hud.Game.CurrentGameTick) / 60.0D;
            }
            return 0;
        }
        public static double GetSkillCooldownTime(IController Hud, IPlayerSkill skill)
        {
            if ((skill != null) && skill.IsOnCooldown && (skill.CooldownFinishTick > Hud.Game.CurrentGameTick))
            {
                return (skill.CooldownFinishTick - Hud.Game.CurrentGameTick) / 60.0D;
            }
            return 0;
        }

        public static void SecondCounting(IController Hud, double second, int limit)
        {
            if ((second > 0) && (second < limit))
            {
                int tmpSecond = (int)Math.Floor(second);
                if (tmpSecond != secondCounting)
                {
                    if (tmpSecond == 0)
                        SpeechPrompt(Hud, "0", 0);
                    else
                        SpeechPrompt(Hud, tmpSecond.ToString(), 0);
                    secondCounting = tmpSecond;
                }
            }
        }

        public static void SpeechPromptSwitch(IController Hud, string prompt, bool sw)
        {
            /*
                        if (speechSwitch != sw)
                        {
                            if (SpeechPrompt(Hud, prompt, 1000)) speechSwitch = sw;
                        }
            */

            if (speechSwitch == sw)
            {
                speechSwitchMsec = Hud.Game.CurrentRealTimeMilliseconds;
            }
            else if ((Hud.Game.CurrentRealTimeMilliseconds - speechSwitchMsec) > 1000)
            {
                speechSwitch = sw;
                SpeechPrompt(Hud, prompt, 0);
            }

        }

        public static bool isThereBossOnScreen(IController Hud)
        {
            return Hud.Game.AliveMonsters.Any(x => x.FloorCoordinate.IsOnScreen() && x.Rarity == ActorRarity.Boss);
        }

        public static bool isOnlyOneEliteMonster(IController Hud, IMonster Monster)
        {
            var monsters = Hud.Game.AliveMonsters.Where(m => m.Rarity == ActorRarity.Champion && !m.Illusion && MonsterAffixIsEqual(m, Monster));
            int count = monsters.Count();
            if (count == 1)
                return true;
            return false;
        }
        public static bool MonsterAffixIsEqual(IMonster MonsterA, IMonster MonsterB)
        {
            string affixSnoListA = null;
            string affixSnoListB = null;
            foreach (ISnoMonsterAffix afx in MonsterA.AffixSnoList)
            {
                affixSnoListA += afx.NameLocalized + ",";
            }
            foreach (ISnoMonsterAffix afx in MonsterB.AffixSnoList)
            {
                affixSnoListB += afx.NameLocalized + ",";
            }
            if (affixSnoListA == affixSnoListB)
                return true;
            return false;
        }

        public static bool FindEquipmentOnPlayer(IController Hud, uint sno)
        {
            var player = Hud.Game.Me;

            //萃取   没有佩戴萃取，则指针为空
            if (((player.CubeSnoItem1 != null) && (player.CubeSnoItem1.Sno == sno)) ||
                ((player.CubeSnoItem2 != null) && (player.CubeSnoItem2.Sno == sno)) ||
                ((player.CubeSnoItem3 != null) && (player.CubeSnoItem3.Sno == sno)) ||
                ((player.CubeSnoItem4 != null) && (player.CubeSnoItem4.Sno == sno))
                )
            {
                return true;
            }

            //佩戴
            var items = Hud.Game.Items;
            foreach (var item in items)
            {
                if ((item.Location >= ItemLocation.Head) && (item.Location <= ItemLocation.Neck))
                    /*
                                    if ((item.Location == ItemLocation.Head) ||
                                        (item.Location == ItemLocation.Torso) ||
                                        (item.Location == ItemLocation.RightHand) ||
                                        (item.Location == ItemLocation.LeftHand) ||
                                        (item.Location == ItemLocation.Hands) ||
                                        (item.Location == ItemLocation.Waist) ||
                                        (item.Location == ItemLocation.Feet) ||
                                        (item.Location == ItemLocation.Shoulders) ||
                                        (item.Location == ItemLocation.Legs) ||
                                        (item.Location == ItemLocation.Bracers) ||
                                        (item.Location == ItemLocation.LeftRing) ||
                                        (item.Location == ItemLocation.RightRing) ||
                                        (item.Location == ItemLocation.Neck))
                    */
                    if (item.SnoItem.Sno == sno)
                        return true;
            }
            return false;
        }
        public static bool FindThisSkill(IController Hud, IPlayer Player, uint sno)
        {
            var skills = Player.Powers.UsedSkills;
            foreach (var skill in skills)
            {
                if (skill.SnoPower.Sno == sno)
                    return true;
            }
            return false;
        }
        public static bool FindThisPassive(IController Hud, IPlayer Player, uint sno)
        {
            var passives = Player.Powers.UsedPassives;
            foreach (var passive in passives)
            {
                if (passive.Sno == sno)
                    return true;
            }
            return false;
        }
        public static bool FindThisSkill(IController Hud, IPlayer Player, uint sno, byte rune)
        {
            var skills = Player.Powers.UsedSkills;
            foreach (var skill in skills)
            {
                if ((skill.SnoPower.Sno == sno) && (skill.Rune == rune))
                    return true;
            }
            return false;
        }
        public static IQuest riftQuest(IController Hud)
        {
            return Hud.Game.Quests.FirstOrDefault(q => q.SnoQuest.Sno == 337492) ?? // rift
                       Hud.Game.Quests.FirstOrDefault(q => q.SnoQuest.Sno == 382695);   // gr
        }
        public static bool IsNephalemRift(IController Hud)
        {
            return riftQuest(Hud) != null && (riftQuest(Hud).QuestStepId == 1 || riftQuest(Hud).QuestStepId == 3 || riftQuest(Hud).QuestStepId == 10);
        }

        public static bool IsGreaterRift(IController Hud)
        {
            return riftQuest(Hud) != null &&
                       (riftQuest(Hud).QuestStepId == 13 || riftQuest(Hud).QuestStepId == 16 || riftQuest(Hud).QuestStepId == 34 ||
                        riftQuest(Hud).QuestStepId == 46);
        }
        public static bool IsGreaterRiftProgress(IController Hud)
        {
            return riftQuest(Hud) != null &&
                       (riftQuest(Hud).QuestStepId == 13 || riftQuest(Hud).QuestStepId == 34 ||
                        riftQuest(Hud).QuestStepId == 46) && !IsGuardianAlive(Hud) && !IsGuardianDead(Hud);
        }
        public static bool IsGuardianAlive(IController Hud)
        {
            if (Hud.Game.AliveMonsters.Any(m => m.Rarity == ActorRarity.Boss))
                return true;
            return riftQuest(Hud) != null && (riftQuest(Hud).QuestStepId == 3 || riftQuest(Hud).QuestStepId == 16);
        }
        public static bool IsGuardianDead(IController Hud)
        {
            if (Hud.Game.Monsters.Any(m => m.Rarity == ActorRarity.Boss && !m.IsAlive))
                return true;
            return riftQuest(Hud) != null && (riftQuest(Hud).QuestStepId == 5 || riftQuest(Hud).QuestStepId == 10 || riftQuest(Hud).QuestStepId == 34 || riftQuest(Hud).QuestStepId == 46);
        }

        public static IWorldCoordinate GetLocationCrossCircle(IController Hud, IWorldCoordinate start, IWorldCoordinate end, float ratio, bool isNearSide)
        {
            //计算两个坐标直线距离指定码数位置
            var a = end.X;
            var b = end.Y;
            var x0 = start.X;
            var y0 = start.Y;
            var k = (b - y0) / (a - x0);
            var jiaodu = Math.Atan(k);

            var x1 = a + ratio * Math.Cos(jiaodu);
            var y1 = b + ratio * Math.Sin(jiaodu);
            var p1 = Hud.Window.CreateWorldCoordinate((float)x1, (float)y1, Hud.Game.Me.FloorCoordinate.Z);

            var x2 = a - ratio * Math.Cos(jiaodu);
            var y2 = b - ratio * Math.Sin(jiaodu);
            var p2 = Hud.Window.CreateWorldCoordinate((float)x2, (float)y2, Hud.Game.Me.FloorCoordinate.Z);

            var loc = p1;
            if (isNearSide)
            {
                if (p1.XYDistanceTo(start) > p2.XYDistanceTo(start))
                    loc = p2;
            }
            else if (p1.XYDistanceTo(start) < p2.XYDistanceTo(start))
                loc = p2;

            return loc;
        }
        public static string Between(string str, string strLeft, string strRight) //取文本中间
        {
            if (str == null || str.Length == 0)
                return "";
            if (strLeft != null)
            {
                int indexLeft = str.IndexOf(strLeft);//左边字符串位置
                if (indexLeft < 0)
                    return "";
                indexLeft = indexLeft + strLeft.Length;//左边字符串长度
                if (strRight != null)
                {
                    int indexRight = str.IndexOf(strRight, indexLeft);//右边字符串位置
                    if (indexRight < 0)
                        return "";
                    return str.Substring(indexLeft, indexRight - indexLeft);//indexRight - indexLeft是取中间字符串长度
                }
                else
                    return str.Substring(indexLeft, str.Length - indexLeft);//取字符串右边
            }
            else//取字符串左边
            {
                if (strRight == null)
                    return "";
                int indexRight = str.IndexOf(strRight);
                if (indexRight <= 0)
                    return "";
                else
                    return str.Substring(0, indexRight);
            }
        }
        public static bool isCasting(IController Hud)
        {
            if (Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.Generic_IdentifyAllWithCast.Sno) ||
                Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.Generic_IdentifyWithCast.Sno) ||
                Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.Generic_IdentifyWithCastLegendary.Sno) ||
                Hud.Game.Me.AnimationState == AcdAnimationState.CastingPortal ||
                Hud.Game.Me.AnimationState == AcdAnimationState.Casting
                )
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static bool ActorTagUIVisible(IController Hud)
        {
            var ActorTagUI = Hud.Render.GetUiElement("Root.NormalLayer.game_dialog_main.RActorTag0");
            return ActorTagUI.Visible;
        }
        public static bool isHoverValidActor(IController Hud, int Distance = 25)
        {
            var actor = Hud.Game.SelectedActor;
            if (actor == null)
                return false;
            if (actor.NormalizedXyDistanceToMe > Distance)
                return false;
            return Hud.Game.SelectedActor.SnoActor.Kind == ActorKind.Shrine
                || Hud.Game.SelectedActor.SnoActor.Kind == ActorKind.Portal
                || Hud.Game.SelectedActor.SnoActor.Kind == ActorKind.Waypoint
                || Hud.Game.SelectedActor.SnoActor.Kind == ActorKind.CursedEvent
                || Hud.Game.SelectedActor.SnoActor.Kind == ActorKind.ChestNormal
                || Hud.Game.SelectedActor.SnoActor.Kind == ActorKind.Chest
                || Hud.Game.SelectedActor.SnoActor.Kind == ActorKind.WeaponRack
                || Hud.Game.SelectedActor.SnoActor.Kind == ActorKind.ArmorRack
                || Hud.Game.SelectedActor.SnoActor.Kind == ActorKind.QuestActivate
                || Hud.Game.SelectedActor.GizmoType == GizmoType.Door
                || Hud.Game.SelectedActor.GizmoType == GizmoType.Headstone
                || Hud.Game.SelectedActor.GizmoType == GizmoType.Portal
                ;
        }
        public static IScreenCoordinate CutScreenCoordinate(IController Hud, IScreenCoordinate ScreenCoordinate)
        {
            var x = ScreenCoordinate.X;
            var y = ScreenCoordinate.Y;
            if (x <= 0)
                x = 1;
            if (y <= 0)
                y = 1;
            if (x >= Hud.Window.Size.Width)
                x = Hud.Window.Size.Width - 1;
            if (y >= Hud.Window.Size.Height)
                x = Hud.Window.Size.Height - 1;
            return Hud.Window.CreateScreenCoordinate(x, y);
        }

        public static bool isMobInSkillRange(IController Hud, IMonster mob, float skillWidth, float skillRange, float heroChestZOffset = 0)//skillWidth=技能的范围宽度（三角形底边），skillRange=技能的最大施法距离，三角形的高度，heroChestZOffset=技能从角色身上发射的位置偏移量
        {
            if(heroChestZOffset == 0)
            {
                switch (Hud.Game.Me.Hero.ClassDefinition.HeroClass)
                {
                    case HeroClass.Monk:
                        heroChestZOffset = 0.3f;
                        break;
                    case HeroClass.Necromancer:
                        heroChestZOffset = 0.3f;
                        break;
                    case HeroClass.Crusader:
                        heroChestZOffset = 4.5f;
                        break;
                    case HeroClass.WitchDoctor:
                        heroChestZOffset = 4.6f;
                        break;
                    case HeroClass.Wizard:
                        heroChestZOffset = 5.3f;
                        break;
                    case HeroClass.Barbarian:
                        heroChestZOffset = 5.5f;
                        break;
                    case HeroClass.DemonHunter:
                        heroChestZOffset = 6f;
                        break;
                }
            }
            var chest = Hud.Game.Me.FloorCoordinate.Offset(0, 0, heroChestZOffset);
            var cursor = Hud.Window.CreateScreenCoordinate(Hud.Window.CursorX, Hud.Window.CursorY).ToWorldCoordinate(chest.Z);
            IWorldCoordinate triangleA, triangleB;
            var target = getNewPointOnLine(Hud, chest, cursor, skillRange);
            getTriangleBaseAngle(Hud, chest, target, skillWidth, out triangleA, out triangleB);
            return IsInTriangle(Hud, triangleA, triangleB, chest, mob.FloorCoordinate, mob.RadiusBottom);
        }
        public static IWorldCoordinate getNewPointOnLine(IController Hud, IWorldCoordinate A, IWorldCoordinate B, float length)
        {
            var AB = (float)Math.Sqrt(Math.Pow(A.X - B.X, 2) + Math.Pow(A.Y - B.Y, 2));//计算AB距离
            var E = Hud.Window.CreateWorldCoordinate(A.X + (length * (B.X - A.X) / AB), A.Y + (length * (B.Y - A.Y) / AB), A.Z);//取出AB延伸线上固定距离坐标
            return E;
        }

        public static void getTriangleBaseAngle(IController Hud, IWorldCoordinate chest, IWorldCoordinate target, float skillWidth, out IWorldCoordinate triangleA, out IWorldCoordinate triangleB)
        {
            var xDist = (float)chest.X - (float)target.X;//获取AB之间x宽度
            var yDist = (float)chest.Y - (float)target.Y;//获取AB之间y宽度
            var tmp = Math.Atan(yDist / xDist);//yy/xx是AB与x轴的夹角的tan值，atan(yy/xx)是夹角度数，角度制
            var dex = (float)skillWidth / 2 * Math.Sin(tmp);//数学方式计算dex，CD俩点与B的x坐标相差的值
            var dey = (float)skillWidth / 2 * Math.Cos(tmp);//数学方式计算dey，CD俩点与B的y坐标相差的值
            var x1 = (float)((float)target.X + dex);
            var y1 = (float)((float)target.Y - dey);
            /*
            计算C的坐标，在B的坐标基础上加上dex和减去dey，dex和dey的加减必须不同， 即 C 的 X 加 dex 那 C 的 Y 必须减 dey
            D点的加减必须和C点的加减不同。即 C 的 X 加 dex 那 D 的 X 必须减 dex
            X坐标 只能减 dex 不能减 dey
            */
            var x2 = (float)((float)target.X - dex);
            var y2 = (float)((float)target.Y + dey);


            triangleA = Hud.Window.CreateWorldCoordinate(x1, y1, target.Z);
            triangleB = Hud.Window.CreateWorldCoordinate(x2, y2, target.Z);
        }

        public static bool IsInTriangle(IController hud, IWorldCoordinate triangleA, IWorldCoordinate triangleB, IWorldCoordinate chest, IWorldCoordinate monster, float monsterHitbox)
        {
            if (chest.XYDistanceTo(triangleB) + monsterHitbox < chest.XYDistanceTo(monster))
            {
                return false; //AA与Monster距离大于三角形边长+怪物半径时肯定不在范围内，直接返回false
            }

            var pt = new IWorldCoordinate[13] { null, null, null, null, null, null, null, null, null, null, null, null, null };
            float detl, dets, pp;
            var jc = 1;
            detl = monsterHitbox * (float)Math.Sin(Math.PI / 6);
            dets = monsterHitbox - (monsterHitbox * (float)Math.Cos(Math.PI / 6));
            pt[1] = hud.Window.CreateWorldCoordinate(monster.X + monsterHitbox, monster.Y, monster.Z);
            pt[2] = hud.Window.CreateWorldCoordinate(monster.X + monsterHitbox - dets, monster.Y + detl, monster.Z);
            pt[3] = hud.Window.CreateWorldCoordinate(monster.X + detl, monster.Y + monsterHitbox - dets, monster.Z);
            pt[4] = hud.Window.CreateWorldCoordinate(monster.X, monster.Y + monsterHitbox, monster.Z);
            pt[5] = hud.Window.CreateWorldCoordinate(monster.X - detl, monster.Y + monsterHitbox - dets, monster.Z);
            pt[6] = hud.Window.CreateWorldCoordinate(monster.X - monsterHitbox + dets, monster.Y + detl, monster.Z);
            pt[7] = hud.Window.CreateWorldCoordinate(monster.X - monsterHitbox, monster.Y, monster.Z);
            pt[8] = hud.Window.CreateWorldCoordinate(monster.X - monsterHitbox + dets, monster.Y - detl, monster.Z);
            pt[9] = hud.Window.CreateWorldCoordinate(monster.X - detl, monster.Y - monsterHitbox + dets, monster.Z);
            pt[10] = hud.Window.CreateWorldCoordinate(monster.X, monster.Y - monsterHitbox, monster.Z);
            pt[11] = hud.Window.CreateWorldCoordinate(monster.X + detl, monster.Y - monsterHitbox + dets, monster.Z);
            pt[12] = hud.Window.CreateWorldCoordinate(monster.X + monsterHitbox - dets, monster.Y - detl, monster.Z);
            pp = foun(monster, triangleA, triangleB, chest);
            if (pp != 5)
            {
                for (var i = 1; i < 13; i++)
                {
                    pp = foun(pt[i], triangleA, triangleB, chest);
                    if (pp == 5)
                    {
                        jc = 0;
                        break;
                    }
                }
            }
            else
            {
                jc = 0;
            }
            if (jc == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static int foun(IWorldCoordinate R, IWorldCoordinate DD, IWorldCoordinate CC, IWorldCoordinate AA)//判断是否在内部
        {
            float dot00, dot01, dot02, dot11, dot12, tmp, u, v;
            var v0 = new dot(); //v0, v1, v2等是AC, AD, AR的向量坐标
            var v1 = new dot();
            var v2 = new dot();

            v0.x = CC.X - AA.X;
            v0.y = CC.Y - AA.Y;
            v1.x = DD.X - AA.X;
            v1.y = DD.Y - AA.Y;
            v2.x = R.X - AA.X;
            v2.y = R.Y - AA.Y;

            dot00 = pt(v0, v0);
            dot01 = pt(v0, v1);
            dot02 = pt(v0, v2);
            dot11 = pt(v1, v1);
            dot12 = pt(v1, v2);
            tmp = 1 / ((dot00 * dot11) - (dot01 * dot01));
            u = ((dot11 * dot02) - (dot01 * dot12)) * tmp;
            if (u < 0 || u > 1)
            {
                return 3;
            }
            v = ((dot00 * dot12) - (dot01 * dot02)) * tmp;
            if (v < 0 || v > 1)
            {
                return 3;
            }

            return 5;
        }

        private static float pt(dot A, dot B)
        {
            float ans;

            ans = (A.x * B.x) + (A.y * B.y) + (A.z * B.z);//A B的点积
            return ans;
        }
        public class dot
        {
            public float x;
            public float y;
            public float z;
        }
    }
}
