using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Turbo.Plugins.Default;

namespace Turbo.Plugins.flower
{
    public class BaseTool
    {
        public static bool EnableKillSpeak = true;//精英击杀提醒
        public static bool EnableKillSpeakWav = true;//精英击杀语音文件启用
        public static string FirstBlood = "第一滴血";//首杀
        public static string FirstBloodWav = "FirstBlood.wav";//首杀个性语音
        public static string OneKill = "不错，击杀成功";//击杀一组
        public static string OneKillWav = "OneKill.wav";//击杀一组个性语音
        public static string DoubleKill = "漂亮，双杀";//双杀
        public static string DoubleKillWav = "DoubleKill.wav";//双杀个性语音
        public static string TripleKill = "三杀，要上天的节奏";//三杀
        public static string TripleKillWav = "TripleKill.wav";//三杀个性语音
        public static string QuatreKill = "四杀，牛逼逼逼";//四杀
        public static string QuatreKillWav = "QuatreKill.wav";//四杀个性语音
        public static string PentaKill = "五杀，快要成神了";//五杀
        public static string PentaKillWav = "PentaKill.wav";//五杀个性语音
        public static string GodLike = "超神，神的名字是";//超神（6杀以上）
        public static string GodLikeWav = "GodLike.wav";//超神个性语音

        private static string debug_info = string.Empty;//用于测试，作用在DebugPlugin
        private static Dictionary<int, string> debug_dict = new Dictionary<int, string>();

        private static BuffRuleCalculator _ruleCalculator;

        public static string DebugInfo()
        {
            if (debug_dict.Count() == 0)
                return debug_info;
            else
            {
                debug_info = string.Empty;
                foreach(KeyValuePair<int, string> kv in debug_dict)
                {
                    debug_info += kv.Value + ", ";
                }
                return debug_info;
            }
        }

        public static void DebugInfo(string info)
        {
            debug_info = info;
        }

        public static void DebugInfo(string info, int i)
        {
            debug_dict[i] = info;
        }

        public static void DebugInfoAdd(string info, string dim = "，")
        {
            debug_info += info + dim;
        }

        public static void DebugInfoAddLine(string info)
        {
            debug_info += info + "\n";
        }

        public static void DebugInfoAddLine(string info, int i)
        {
            debug_dict[i] = info + "\n";
        }

        public static void DebugInfoClear()
        {
            debug_info = string.Empty;
            debug_dict.Clear();
        }
        
        //在屏幕的指定高度显示文字
        public static void DebugTrace(IController Hud, string trace, float height)
        {
            var len = trace.Length;
            //if (len > 80) len = 80;
            var x = Hud.Window.Size.Width * 0.18f;
            var y = Hud.Window.Size.Height * height;
            var TextFont = Hud.Render.CreateFont("tahoma", 9, 255, 255, 255, 0, false, false, true);
            TextFont.DrawText(trace, x, y);
        }

        //在固定位置（屏幕的左下角）显示信息
        public static void ShowInfo(IController Hud, string info)
        {
            var infoFont = Hud.Render.CreateFont("tahoma", 8, 255, 255, 255, 0, true, false, 255, 0, 0, 0, true);
            var layout = infoFont.GetTextLayout(info);
            var x = Hud.Window.Size.Width * 0.07f;
            var y = Hud.Window.Size.Height * 0.97f;
            infoFont.DrawText(layout, x, y);
        }

        //在屏幕中间显示错误信息
        public static void ShowError(IController Hud, string info)
        {
            var infoFont = Hud.Render.CreateFont("tahoma", 11, 255, 255, 255, 0, true, false, 255, 0, 0, 0, true);
            var layout = infoFont.GetTextLayout(info);
            var meWoldCoordinate = Hud.Game.Me.FloorCoordinate;
            infoFont.DrawText(layout, meWoldCoordinate.ToScreenCoordinate().X - layout.Metrics.Width / 2, meWoldCoordinate.ToScreenCoordinate().Y / 2);
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

        //播放Wav文件
        public static void PlaySoundWav(SoundPlayer soundPlayer)
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                try
                {
                    soundPlayer.PlaySync();
                }
                catch (Exception)
                {
                }
            });
        }

        //是否有某个装备（萃取或佩戴）
        public static bool FindEquipmentOnPlayer(IController Hud, IPlayer player, uint sno)
        {
            if (player == null) return false;

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
                if ((item.Location >= ItemLocation.Head) && (item.Location <= ItemLocation.Neck))
                    if (item.SnoItem.Sno == sno) return true;
            }
            return false;
        }

        //是否带有某技能
        public static bool FindThisSkill(IController Hud, IPlayer player, uint sno)
        {
            var skills = player.Powers.UsedSkills;
            foreach (var skill in skills)
            {
                if (skill.SnoPower.Sno == sno) return true;
            }
            return false;
        }

        //是否带了某被动
        public static bool FindThisPassive(IController Hud, uint sno)
        {
            var passives = Hud.Game.Me.Powers.UsedPassives;
            foreach (var passive in passives)
            {
                if (passive.Sno == sno) return true;
            }
            return false;
        }

        public static IPlayerSkill FindTheSkill(IController Hud, IPlayer player, uint sno)
        {
            var skills = player.Powers.UsedSkills;
            foreach (var skill in skills)
            {
                if (skill.SnoPower.Sno == sno) return skill;
            }
            return null;
        }

        //指定范围内的怪物数量
        public static int CountMonstersByRange(IController Hud, int range)
        {
            return Hud.Game.AliveMonsters.Where(m => m.NormalizedXyDistanceToMe < range).Count();
        }

        //获取指定范围内的精英数量
        public static int CountElite(IController Hud, int range)
        {
            int elitePackCount = 0;
            var Allmonsters = Hud.Game.AliveMonsters;
            var monsters = Allmonsters.Where(m => ((m.SummonerAcdDynamicId == 0 && m.IsElite) || !m.IsElite) && m.FloorCoordinate.XYDistanceTo(Hud.Game.Me.FloorCoordinate) <= range);
            Dictionary<IMonster, string> eliteGroup = new Dictionary<IMonster, string>();
            foreach (var monster in monsters)
            {
                if (monster.Rarity == ActorRarity.Unique || monster.Rarity == ActorRarity.Boss)
                {
                    elitePackCount++;
                }

                if (monster.Rarity == ActorRarity.Champion)
                {
                    eliteGroup.Add(monster, string.Join(", ", monster.AffixSnoList));
                }

                if (monster.Rarity == ActorRarity.Rare)
                {
                    elitePackCount++;
                }
            }

            Dictionary<IMonster, string> eliteGroup1 = eliteGroup.OrderBy(p => p.Value).ToDictionary(p => p.Key, o => o.Value);
            string preStr = null;
            foreach (var elite in eliteGroup1)
            {
                if (elite.Key.Rarity == ActorRarity.Champion)
                {
                    if (preStr != elite.Value)
                    {
                        elitePackCount++;
                    }
                    preStr = elite.Value;
                }
            }

            return elitePackCount;
        }

        //获取高密度怪中的一个怪
        public static IMonster GetDensityMonster(IController Hud, int range)
        {
            return Hud.Game.AliveMonsters.Where(m => m.IsOnScreen).OrderByDescending(m => m.GetMonsterDensity(range)).FirstOrDefault();
        }

        public static IEnumerable<BuffRule> GetCurrentRules(HeroClass heroClass)
        {
            for (int i = 1; i <= 7; i++)
            {
                switch (heroClass)
                {
                    case HeroClass.Barbarian: if (i == 1 || i == 4 || i == 7) continue; break;
                    case HeroClass.Crusader: if (i == 1 || i == 2 || i == 7) continue; break;
                    case HeroClass.DemonHunter: if (i == 1 || i == 4 || i == 7) continue; break;
                    case HeroClass.Monk: if (i == 1 || i == 7) continue; break;
                    case HeroClass.Necromancer: if (i == 1 || i == 3 || i == 4 || i == 5) continue; break;
                    case HeroClass.WitchDoctor: if (i == 1 || i == 4 || i == 5) continue; break;
                    case HeroClass.Wizard: if (i == 4 || i == 6 || i == 7) continue; break;
                }
                yield return _ruleCalculator.Rules[i - 1];
            }
        }

        /// <summary>
        /// 获取到达指定元素的时间，默认则为最高元素
        /// </summary>
        /// <param name="Hud"></param>
        /// <param name="player"></param>
        /// <param name="CoeIndex"></param>
        /// <returns></returns>
        public static double GetElementTime(IController Hud, IPlayer player, int CoeIndex = 0)
        {
            var buff = player.Powers.GetBuff(430674);
            if ((buff == null) || (buff.IconCounts[0] <= 0)) return 0;

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

            if (_ruleCalculator.PaintInfoList.Count == 0) return 0;
            if (!_ruleCalculator.PaintInfoList.Any(info => info.TimeLeft > 0)) return 0;

            var highestElementalBonus = player.Offense.HighestElementalDamageBonus;
            for (int i = 0; i < _ruleCalculator.PaintInfoList.Count; i++)
            {
                var info = _ruleCalculator.PaintInfoList[0];
                if (info.TimeLeft <= 0)
                {
                    _ruleCalculator.PaintInfoList.RemoveAt(0);
                    _ruleCalculator.PaintInfoList.Add(info);
                }
                else break;
            }


            for (int orderIndex = 0; orderIndex < _ruleCalculator.PaintInfoList.Count; orderIndex++)
            {
                var info = _ruleCalculator.PaintInfoList[orderIndex];
                var best = false;
                if (CoeIndex == 0)
                {
                    switch (info.Rule.IconIndex)
                    {
                        case 1: best = player.Offense.BonusToArcane == highestElementalBonus; break;
                        case 2: best = player.Offense.BonusToCold == highestElementalBonus; break;
                        case 3: best = player.Offense.BonusToFire == highestElementalBonus; break;
                        case 4: best = player.Offense.BonusToHoly == highestElementalBonus; break;
                        case 5: best = player.Offense.BonusToLightning == highestElementalBonus; break;
                        case 6: best = player.Offense.BonusToPhysical == highestElementalBonus; break;
                        case 7: best = player.Offense.BonusToPoison == highestElementalBonus; break;
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
                if (player.Offense.BonusToArcane == highestElementalBonus) hi = 1;
                else if (player.Offense.BonusToCold == highestElementalBonus) hi = 2;
                else if (player.Offense.BonusToFire == highestElementalBonus) hi = 3;
                else if (player.Offense.BonusToHoly == highestElementalBonus) hi = 4;
                else if (player.Offense.BonusToLightning == highestElementalBonus) hi = 5;
                else if (player.Offense.BonusToPhysical == highestElementalBonus) hi = 6;
                else if (player.Offense.BonusToPoison == highestElementalBonus) hi = 7;
            }
            return hi;
        }

        public static IEnumerable<IActor> GetOculus(IController Hud)
        {
            return Hud.Game.Actors.Where(x => x.SnoActor.Sno == ActorSnoEnum._generic_proxy && x.GetAttributeValueAsInt(Hud.Sno.Attributes.Power_Buff_1_Visual_Effect_None, Hud.Sno.SnoPowers.OculusRing.Sno) == 1);
        }

        //距离指定位置距离最近的神目圈
        public static IActor GetNearestOculusByTarget(IController Hud, IWorldCoordinate target, int range)
        {
            if (target == null)
                return null;
            IActor nearActor = null;
            int minRange = range;
            var actors = Hud.Game.Actors.Where(x => x.SnoActor.Sno == ActorSnoEnum._generic_proxy 
            && x.GetAttributeValueAsInt(Hud.Sno.Attributes.Power_Buff_1_Visual_Effect_None, Hud.Sno.SnoPowers.OculusRing.Sno) == 1);
            foreach (var actor in actors)
            {
                var dis = actor.FloorCoordinate.XYDistanceTo(target);
                if (dis > range)
                    continue;
                if (dis <= minRange)
                {
                    minRange = (int)dis;
                    nearActor = actor;
                }
            }
            return nearActor;
        }

        public static IActor GetNearestOculus(IController Hud, int range)
        {
            return GetNearestOculusByTarget(Hud, Hud.Game.Me.FloorCoordinate, range);
        }
        
        public static IWorldCoordinate GetLocationCrossCircle(IController Hud, IWorldCoordinate start, IWorldCoordinate end, int ratio, bool isNearSide, int ZXiuzheng, int jdXiuZheng)
        {
            if (start == null || end == null)
                return null;
            //计算距离目标的圈的远点坐标
            var a = end.X;
            var b = end.Y;
            var x0 = start.X;
            var y0 = start.Y;
            var k = (b - y0) / (a - x0);
            var jiaodu = Math.Atan(k) + jdXiuZheng;

            var x1 = a + ratio * Math.Cos(jiaodu);
            var y1 = b + ratio * Math.Sin(jiaodu);
            var p1 = Hud.Window.CreateWorldCoordinate((float)x1, (float)y1, Hud.Game.Me.FloorCoordinate.Z + ZXiuzheng);

            var x2 = a - ratio * Math.Cos(jiaodu);
            var y2 = b - ratio * Math.Sin(jiaodu);
            var p2 = Hud.Window.CreateWorldCoordinate((float)x2, (float)y2, Hud.Game.Me.FloorCoordinate.Z + ZXiuzheng);

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

        public static void KeyPressAndRelease(IController Hud, ActionKey key)
        {
            Hud.Interaction.DoActionAutoShift(key);
        }

        public static void KeyHold(IController Hud, ActionKey key)
        {
            if (key == ActionKey.LeftSkill)
                Hud.Interaction.StartContinuousAction(key, true);
            else
                Hud.Interaction.StartContinuousAction(key, false);
        }

        public static void KeyUnHold(IController Hud, ActionKey key)
        {
            Hud.Interaction.StopContinuousAction(key);
            if (key == ActionKey.LeftSkill)
                Hud.Interaction.StandStillUp();
        }

        public static bool IsMoving(IController Hud)
        {
            return Hud.Interaction.IsContinuousActionStarted(ActionKey.Move);
        }

        public static int CountZombieDog(IController Hud)
        {
            int count = 0;
            foreach (var actor in Hud.Game.Actors.Where(a => a.SummonerAcdDynamicId == Hud.Game.Me.SummonerId))
            {
                if (actor.SnoActor.Sno == ActorSnoEnum._wd_zombiedog ||
                    actor.SnoActor.Sno == ActorSnoEnum._wd_zombiedog_cast_spirit ||
                    actor.SnoActor.Sno == ActorSnoEnum._wd_zombiedogrune_fire ||
                    actor.SnoActor.Sno == ActorSnoEnum._wd_zombiedogrune_fire_castspirit ||
                    actor.SnoActor.Sno == ActorSnoEnum._wd_zombiedogrune_fire_swipes_02 ||
                    actor.SnoActor.Sno == ActorSnoEnum._wd_zombiedogrune_healthglobe ||
                    actor.SnoActor.Sno == ActorSnoEnum._wd_zombiedogrune_healthglobe_castspirit ||
                    actor.SnoActor.Sno == ActorSnoEnum._wd_zombiedogrune_healthlink ||
                    actor.SnoActor.Sno == ActorSnoEnum._wd_zombiedogrune_healthlink_attract ||
                    actor.SnoActor.Sno == ActorSnoEnum._wd_zombiedogrune_lifesteal ||
                    actor.SnoActor.Sno == ActorSnoEnum._wd_zombiedogrune_lifesteal_castspirit ||
                    actor.SnoActor.Sno == ActorSnoEnum._wd_zombiedogrune_poison ||
                    actor.SnoActor.Sno == ActorSnoEnum._wd_zombiedogrune_poison_castspirit ||
                    actor.SnoActor.Sno == ActorSnoEnum._wd_zombiedogrune_poison_swipes_02)
                {
                    count++;
                }
            }
            return count;
        }
        public static int CountHunDan(IController Hud)
        {
            return Hud.Game.Actors.Where(m => m.SnoActor.Sno == ActorSnoEnum._wd_spiritbarragerune_aoe_ghostmodel && m.SummonerAcdDynamicId == Hud.Game.Me.SummonerId).Count();
        }

        public static int CountHunDanByTime(IController Hud, int leftTime, bool isMoreThan)
        {
            if (isMoreThan)
            {
                //剩余时间超过指定的leftTime
                return Hud.Game.Actors.Where(m => m.SnoActor.Sno == ActorSnoEnum._wd_spiritbarragerune_aoe_ghostmodel
                && 10 - (Hud.Game.CurrentGameTick - m.CreatedAtInGameTick) / 60 >= leftTime).Count();
            }
            else
            {
                //剩余时间小于指定的leftTime
                return Hud.Game.Actors.Where(m => m.SnoActor.Sno == ActorSnoEnum._wd_spiritbarragerune_aoe_ghostmodel
                && 10 - (Hud.Game.CurrentGameTick - m.CreatedAtInGameTick) / 60 <= leftTime).Count();
            }
        }

        public static int GetBuffCountAssignedPlayer(IController Hud, IPlayer player, uint sno, int index)
        {
            var buff = player.Powers.GetBuff(sno);
            if (buff != null) return buff.IconCounts[index];
            return 0;
        }

        public static double GetBuffLeftTimeAssignedPlayer(IController Hud, IPlayer player, uint sno, int index)
        {
            var buff = player.Powers.GetBuff(sno);
            if (buff != null) return buff.TimeLeftSeconds[index];
            return 0;
        }
    }
}
