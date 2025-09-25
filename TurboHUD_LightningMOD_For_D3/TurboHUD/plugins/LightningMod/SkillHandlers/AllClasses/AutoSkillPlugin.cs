using System.Collections.Generic;
using System;
using System.Linq;
using System.Drawing;
using Turbo.Plugins.Default;
using Turbo.Plugins.glq;
using System.Runtime.InteropServices;

namespace Turbo.Plugins.LightningMod
{
    public class AutoSkillPlugin : BasePlugin, IInGameTopPainter, IAfterCollectHandler
    {
        private IWatch[] Hover = new IWatch[7]; //由OnMouseOver和OnMouseOut处理
        private bool[] HoverRepeat = { false, false, false, false, false, false, false}; //由OnMouseOver和OnMouseOut处理
        public int HoverPinDelay { get; set; } = 300; //ms
        public int RepeatPinDelay { get; set; } = 0;
        private IWatch[] timer = new IWatch[7];
        private IBrush Brush;
        private IPlayerSkill[] Skill = new IPlayerSkill[6];
        private RectangleF[] Rect = new RectangleF[7];
        private bool[] EnabledSkill = { false, false, false, false, false, false, false};
        private string str_Enabled;
        private string str_Disabled;
        private IUiElement ChatUI;
        private bool isChanneling = false;
        private IPlayerSkill HealthPotionSkill = null;
        public readonly HashSet<string> ChannelingSkills = new HashSet<string>
        {
            "Whirlwind","Strafe","Firebats","Ray of Frost","Arcane Torrent","Disintegrate","Tempest Rush","Siphon Blood"
        };
        public AutoSkillPlugin()
        {
            Enabled = true;
        }
        public override void Load(IController hud)
        {
            base.Load(hud);
            Brush = Hud.Render.CreateBrush(255, 0, 0, 0, 2);
            ChatUI = Hud.Render.GetUiElement("Root.NormalLayer.chatentry_dialog_backgroundScreen.chatentry_content.chat_editline");
            timer[0] = Hud.Time.CreateWatch();
            timer[1] = Hud.Time.CreateWatch();
            timer[2] = Hud.Time.CreateWatch();
            timer[3] = Hud.Time.CreateWatch();
            timer[4] = Hud.Time.CreateWatch();
            timer[5] = Hud.Time.CreateWatch();
            timer[6] = Hud.Time.CreateWatch();
            Hover[0] = Hud.Time.CreateWatch();
            Hover[1] = Hud.Time.CreateWatch();
            Hover[2] = Hud.Time.CreateWatch();
            Hover[3] = Hud.Time.CreateWatch();
            Hover[4] = Hud.Time.CreateWatch();
            Hover[5] = Hud.Time.CreateWatch();
            Hover[6] = Hud.Time.CreateWatch();
            if (Hud.CurrentLanguage == Language.zhCN)
            {
                str_Enabled = "已开启：该技能持续施放";
                str_Disabled = "已关闭：该技能持续施放";

            }
            else if (Hud.CurrentLanguage == Language.zhTW)
            {
                str_Enabled = "已開啟：該技能持續施放";
                str_Disabled = "已關閉：該技能持續施放";
            }
            else if (Hud.CurrentLanguage == Language.ruRU)
            {
                str_Enabled = "Включить: это умение на автовыполнение";
                str_Disabled = "Выключить: это умение на автовыполнение";
            }
            else
            {
                str_Enabled = "Enabled: this skill is auto cast";
                str_Disabled = "Disabled: this skill is auto cast";
            }
        }
        private bool OnMouseOver(int delay, int i, int repeat = 0)
        {
            if (!Hover[i].IsRunning)
            {
                Hover[i].Start();
                HoverRepeat[i] = false;
            }
            else if ((!HoverRepeat[i] && Hover[i].TimerTest(delay)) || (repeat > 0 && HoverRepeat[i] && Hover[i].TimerTest(repeat)))
            {
                Hover[i].Restart();
                HoverRepeat[i] = true;
                return true;
            }

            return false;
        }
        private void OnMouseOut(int i)
        {
            Hover[i].Stop();
            Hover[i].Reset();
            HoverRepeat[i] = false;
        }
        private void setUI(int i,IPlayerSkill skill)
        {
            if (skill != null)
            {
                float h = 0.25f;
                float w = 0.25f;
                IUiElement ui = Hud.Render.GetPlayerSkillUiElement(i < 6 ? skill.Key : ActionKey.Heal);
                Rect[i].X = ui.Rectangle.Right - (ui.Rectangle.Width * w);
                Rect[i].Y = ui.Rectangle.Top;
                Rect[i].Width = ui.Rectangle.Width * w;
                Rect[i].Height = ui.Rectangle.Height * h;
            }
            else
            {
                Rect[i].X = 0;
                Rect[i].Y = 0;
                Rect[i].Width = 0;
                Rect[i].Height = 0;
                EnabledSkill[i] = false;
                timer[i].Stop();
            }
        }
        public void PaintTopInGame(ClipState clipState)
        {
            int n = 0;
            foreach (var skill in Hud.Game.Me.Powers.SkillSlots)
            {
                if(skill != null)
                {
                    int i = skill.Key.GetHashCode();
                    Skill[i] = skill;
                    setUI(i, skill);
                }
                else
                {
                    Skill[n] = null;
                    setUI(n, null);
                }
                n++;
            }
            HealthPotionSkill = Hud.Game.Me.Powers.HealthPotionSkill;
            if(HealthPotionSkill != null)
            {
                setUI(6, HealthPotionSkill);
            }
            else
            {
                setUI(6, null);
            }
            int j = 0;
            foreach (var rect in Rect)
            {
                if(rect != null && rect.X != 0)
                {
                    var enableTex = Hud.Texture.GetTexture(2560062517);
                    var disableTex = Hud.Texture.GetTexture(1156241668);
                    //Brush.DrawRectangle(rect);//temp
                    if (EnabledSkill[j])
                    {
                        enableTex.Draw(rect.Left - rect.Width * 2.5f, rect.Top - rect.Height * 2.5f, rect.Width * 6, rect.Height * 6);
                    }
                    else
                    {
                        disableTex.Draw(rect.Left - rect.Width * 2.5f, rect.Top - rect.Height * 2.5f, rect.Width * 6, rect.Height * 6);
                    }
                    if (Hud.Window.CursorInsideRect(rect.X, rect.Y, rect.Width, rect.Height))
                    {
                        if (EnabledSkill[j])
                        {
                            Hud.Render.SetHint(str_Enabled);
                        }
                        else
                        {
                            Hud.Render.SetHint(str_Disabled);
                        }
                        if (OnMouseOver(HoverPinDelay, j, RepeatPinDelay)) //悬停鼠标
                        {
                            if (EnabledSkill[j]) //关闭
                            {
                                EnabledSkill[j] = false;
                            }
                            else //启用
                            {
                                EnabledSkill[j] = true;
                            }
                        }
                    }
                    else
                        OnMouseOut(j);
                }
                j = j + 1;
            }
        }
        public void AfterCollect()
        {
            bool isSoulShardActive = Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.Generic_P72ItemPassiveSoulshard015.Sno) //恐惧碎片：每个冷却的技能+5%攻速和暴击
                || Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.Generic_P72ItemPassiveSoulshard016.Sno);//恐惧碎片：有3个冷却的技能+50%电伤和火伤
            if (Hud.Game.IsLoading
                ||Hud.Game.IsPaused
                ||Hud.Game.IsInTown
                ||!Hud.Game.IsInGame
                || !Hud.Window.IsForeground
                || (!Hud.Render.MinimapUiElement.Visible)
                || ChatUI.Visible
                || Hud.Render.ActMapUiElement.LastVisibleSystemTick > DateTime.Now.Ticks - (500 * 10000)
                || Hud.Render.WorldMapUiElement.LastVisibleSystemTick > DateTime.Now.Ticks - (500 * 10000)
                || Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.Generic_IdentifyAllWithCast.Sno)
                || Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.Generic_IdentifyWithCast.Sno)
                || Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.Generic_IdentifyWithCastLegendary.Sno)
                || Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.Generic_AxeOperateGizmo.Sno)//悬赏任务读条
                || Hud.Game.Me.AnimationState == AcdAnimationState.Transform
                || Hud.Game.Me.AnimationState == AcdAnimationState.CastingPortal
                || Hud.Game.Me.IsDead
                || Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.Generic_ActorGhostedBuff.Sno)
                || (Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.Crusader_SteedCharge.Sno) && !isSoulShardActive)
                || Hud.Render.IsAnyBlockingUiElementVisible
                )
            {
                foreach(var _time in timer)
                {
                    if(_time.IsRunning) _time.Stop();
                }
                return;
            }
            bool isSteedChargeOnCooldown = Hud.Game.Me.Powers.UsedSkills.FirstOrDefault(s => s.SnoPower == Hud.Sno.SnoPowers.Crusader_SteedCharge)?.IsOnCooldown == true;
            int i = 0;
            foreach (var skill in Skill)
            {
                i = i + 1;
                if (timer[i - 1].IsRunning && timer[i - 1].ElapsedMilliseconds < 100)
                {
                    continue;
                }
                if (!EnabledSkill[i - 1])
                {
                    if (ChannelingSkills.Contains(Skill[i - 1]?.SnoPower?.NameEnglish) == true)//引导技能弹起
                    {
                        if (Hud.Interaction.IsContinuousActionStarted(Skill[i - 1].Key) == true && isChanneling == true)
                        {
                            Hud.Interaction.StopContinuousAction(Skill[i - 1].Key);
                            isChanneling = false;
                        }
                    }
                    continue;
                }
                if (Skill[i - 1] == null || Skill[i - 1].IsOnCooldown)
                {
                    continue;
                }
                if (Skill[i - 1].GetResourceRequirement() > Hud.Game.Me.Stats.ResourceCurPri)//能量不足时不施放
                {
                    continue;
                }
                if (Skill[i - 1].CalculateCooldown((float)Skill[i - 1].SnoPower.BaseCoolDownByRune.FirstOrDefault()) <= 0 && Hud.Interaction.IsHotKeySet(ActionKey.Move) && Hud.Interaction.IsContinuousActionStarted(ActionKey.Move))//无CD技能强制移动时不施放
                {
                    if (ChannelingSkills.Contains(Skill[i - 1].SnoPower.NameEnglish) == true &&//引导技能
                           isChanneling == true)//强制移动时停止引导
                    {
                        Hud.Interaction.StopContinuousAction(Skill[i - 1].Key);
                        isChanneling = false;
                    }
                    continue;
                }
                if (Skill[i - 1].Key == ActionKey.LeftSkill || Skill[i - 1].Key == ActionKey.RightSkill)
                {
                    if (!Hud.Window.CursorInsideGroundRect())//左右键技能不能点到技能栏位置后不施放
                    {
                        continue;
                    }
                }
                if (Skill[i - 1].Key == ActionKey.LeftSkill && PublicClassPlugin.isHoverValidActor(Hud))//鼠标悬停在门塔宝箱过图点等交互物时不点击，避免误触
                {
                    continue;
                }
                if (isSoulShardActive)
                {
                    if (Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.Crusader_SteedCharge.Sno) && (isSteedChargeOnCooldown || skill == Hud.Game.Me.Powers.UsedCrusaderPowers.SteedCharge))
                    {
                        continue;
                    }
                }
                timer[i - 1].Restart();
                if(ChannelingSkills.Contains(Skill[i - 1].SnoPower.NameEnglish) == true)//引导技能
                {
                    if (Hud.Interaction.IsContinuousActionStarted(Skill[i - 1].Key) == false && !Hud.Interaction.IsContinuousActionStarted(ActionKey.Move))
                    {
                        //按下引导技能
                        isChanneling = true;
                        Hud.Interaction.StartContinuousAction(Skill[i - 1].Key, Skill[i - 1].Key == ActionKey.LeftSkill);
                        if (Skill[i - 1].Key == ActionKey.LeftSkill)
                        {
                            Hud.Interaction.StandStillUp();
                        }
                    }
                    
                }else
                {
                    Hud.Interaction.DoActionAutoShift(Skill[i - 1].Key);
                }
            }

            if (!timer[6].IsRunning || (timer[6].IsRunning && timer[6].ElapsedMilliseconds >= 100))//自动持续喝药，触发祭坛buff效果
            {
                if (EnabledSkill[6] && HealthPotionSkill != null && !HealthPotionSkill.IsOnCooldown)
                {
                    if(Hud.Game.Me.Powers.BuffIsActive(488004) || Hud.Game.Me.Powers.BuffIsActive(488036) || Hud.Game.Me.Powers.BuffIsActive(488037) || Hud.Game.Me.Defense.HealthPct < 100)
                    {
                        Hud.Interaction.DoAction(ActionKey.Heal);
                    }
                }
            }
        }
    }
}