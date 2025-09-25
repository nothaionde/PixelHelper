namespace Turbo.Plugins.LightningMod
{
    using Turbo.Plugins.Default;
    using Turbo.Plugins.glq;
    using SharpDX.DirectInput;
    using System.Linq;
    using System;
    public class CloseToDPSPlugin : BasePlugin, IAfterCollectHandler, IInGameTopPainter, IKeyEventHandler
    {
        public IKeyEvent ToggleKeyEvent { get; set; }
        public IFont InfoFont { get; private set; }
        private string str_Info;
        private string str_running1;
        private string str_running2;
        private string str_tip1;
        private string str_tip2;
        public bool Running { get; private set; }
        private bool isFuzhuMonk = false;
        private IPlayerSkill skillCripplingWave = null;
        private IPlayerSkill skillDeadlyReach = null;
        private IWatch DelayTimer;
        private IPlayer shawang = null;
        public CloseToDPSPlugin()
        {
            Enabled = true;
        }
        public override void Load(IController hud)
        {
            base.Load(hud);
            ToggleKeyEvent = Hud.Input.CreateKeyEvent(true, Key.F3, false, false, false);
            InfoFont = Hud.Render.CreateFont("tahoma", 8, 255, 200, 200, 0, true, false, 255, 0, 0, 0, true);
            if (Hud.CurrentLanguage == Language.zhCN)
            {
                str_Info = "请先在“游戏选项”“按键绑定”中设置“强制移动”热键";
                //str_running1 = "正在优先贴身推进度DPS";
                str_running1 = "正在自动打拳";
                str_running2 = "正在自动战斗";
                //str_tip1 = "键优先贴身DPS";
                str_tip1 = "键自动打拳";
                str_tip2 = "键自动战斗";
            }
            else if (Hud.CurrentLanguage == Language.zhTW)
            {
                str_Info = "請先在“設定”“按鍵設定”中設置“強制移動”熱鍵";
                //str_running1 = "正在優先貼身推進度DPS";
                str_running1 = "正在自動打拳";
                str_running2 = "正在自動戰鬥";
                //str_tip1 = "鍵優先貼身DPS";
                str_tip1 = "鍵自動打拳";
                str_tip2 = "鍵自動戰鬥";
            }
            else if (Hud.CurrentLanguage == Language.ruRU)
            {
                str_Info = "Назначте клавишу <Только передвижение> в <НАСТРОЙКИ><ГОРЯЧИИ КЛАВИШИ>";
                //str_running1 = "защита ДПС";
                str_running1 = "Автосражение";
                str_running2 = "Автосражение";
                //str_tip1 = "Клавиша для задачи ДПС";
                str_tip1 = "Клавиша для АвтоБоя";
                str_tip2 = "Клавиша для АвтоБоя";
            }
            else
            {
                str_Info = "Plese set <FORCE MOVE> key in <OPTIONS>";
                //str_running1 = "protecting the DPS";
                str_running1 = "Auto attacking";
                str_running2 = "Auto fighting";
                //str_tip1 = " Key priority close to the DPS";
                str_tip1 = " Key to auto attack";
                str_tip2 = " Key to auto fight";
            }
            DelayTimer = Hud.Time.CreateWatch();
        }
        public void OnKeyEvent(IKeyEvent keyEvent)
        {
            if (ToggleKeyEvent.Matches(keyEvent) && keyEvent.IsPressed && isFuzhuMonk)
            {
                Running = !Running;
                if(Running)
                {

                }
                else
                {
                    DelayTimer.Stop();
                }
            }
        }
        private bool BossSkill(IMonster Boss, AnimSnoEnum BossAnime)
        {
            if(Boss.Animation == BossAnime)
            {
                return true;
            }
            return false;

        }
        public void AfterCollect()
        {
            if (!Hud.Window.IsForeground) return;
            if (!Hud.Game.IsInGame) return;
            if (Hud.Game.IsLoading) return;
            isFuzhuMonk = !Hud.Game.Me.IsInTown &&
                Hud.Game.Me.InGreaterRift && 
                Hud.Game.Me.HeroClassDefinition.HeroClass == HeroClass.Monk && Hud.Game.NumberOfPlayersInGame > 1 && Hud.Game.Me.GetSetItemCount(742942) < 6 &&
                Hud.Game.Me.Powers.UsedSkills.Any(x => x.SnoPower.Sno == Hud.Sno.SnoPowers.Monk_InnerSanctuary.Sno);//获取自己是辅助武僧时
            //IPlayer Wizard = null;
            shawang = Hud.Game.Players.Where(p =>
            p.CoordinateKnown && p.Powers.BuffIsActive(Hud.Sno.SnoPowers.BaneOfTheStrickenPrimary.Sno)//带受罚者为杀王的人
            ).FirstOrDefault();
            if (shawang == null)
            {
                shawang = Hud.Game.Players.Where(p =>
            p.CoordinateKnown).OrderByDescending(p => p.Offense.SheetDps)//伤害最高的玩家为杀王的人
            .FirstOrDefault();
            }
            if (!Running || !isFuzhuMonk)
            {
                Running = false;
                return;
            }
            if (!Hud.Interaction.IsHotKeySet(ActionKey.Move)) return;//未设置强制移动时不继续
            if (Hud.Interaction.IsContinuousActionStarted(ActionKey.Move)) return;//按住强制移动时不继续
            
            skillCripplingWave = Hud.Game.Me.Powers.GetUsedSkill(Hud.Sno.SnoPowers.Monk_CripplingWave);
            skillDeadlyReach = Hud.Game.Me.Powers.GetUsedSkill(Hud.Sno.SnoPowers.Monk_DeadlyReach);
            if (Hud.Game.RiftPercentage < 100)
            {
                /*
                if (Wizard == null) return;
                //进度阶段
                if (Wizard?.CentralXyDistanceToMe > 10 + Wizard.RadiusBottom && !Hud.Game.Actors.Any(x => x.SnoActor.Sno == ActorSnoEnum._wizard_meteor_pending_cost))//超出金轮阵范围时靠近白人法师,落奥陨时不跟（法师去踩圈）
                {
                    IWorldCoordinate WizardWorldCoordinate = Wizard.FloorCoordinate;//获取法师坐标
                    Hud.Interaction.MouseMove(Wizard.FloorCoordinate.ToScreenCoordinate().X, Wizard.FloorCoordinate.ToScreenCoordinate().Y);//鼠标点到法师
                    Hud.Interaction.DoAction(ActionKey.Move);//向法师移动
                }
                */
                ActionKey skillkey = skillCripplingWave != null ? skillCripplingWave.Key : skillDeadlyReach.Key;//优先使用断筋诀
                //自动原地打拳
                if (!DelayTimer.IsRunning || (DelayTimer.IsRunning && DelayTimer.ElapsedMilliseconds >= 100))
                {
                    Hud.Interaction.DoAction(skillkey, true);
                    Hud.Interaction.StandStillUp();
                    DelayTimer.Restart();
                }
            }
            else if (shawang != null)
            {
                //满进度时附近有BOSS时，杀王玩家出了金轮阵且没激活金轮阵时靠近杀王玩家
                ActionKey skillkey = skillDeadlyReach != null ? skillDeadlyReach.Key : skillCripplingWave.Key;//优先使用冲心拳
                var Boss = Hud.Game.AliveMonsters.FirstOrDefault(m => m.Rarity == ActorRarity.Boss && m.SummonerAcdDynamicId == 0);
                if (Boss == null)
                {
                    if(DelayTimer.IsRunning)
                    {
                        DelayTimer.Stop();
                    }
                    return;
                }
                IWorldCoordinate ShaWangWorldCoordinate = shawang.FloorCoordinate;//获取杀王玩家坐标
                IWorldCoordinate BossWorldCoordinate = Boss.FloorCoordinate;//获取BOSS坐标
                IWorldCoordinate BossDirectionWorldCoordinate = PublicClassPlugin.GetLocationCrossCircle(Hud, Hud.Game.Me.FloorCoordinate, BossWorldCoordinate, 30, true);//获取BOSS在自己的方向坐标
                IWorldCoordinate DestinationWorldCoordinate = PublicClassPlugin.GetLocationCrossCircle(Hud, ShaWangWorldCoordinate, BossWorldCoordinate, Boss.RadiusBottom, true); //杀王玩家与BOSS中间位置
                if (shawang?.CentralXyDistanceToMe > (10 + shawang.RadiusBottom) && Boss.CentralXyDistanceToMe < shawang.CentralXyDistanceToMe && DestinationWorldCoordinate.XYDistanceTo(Hud.Game.Me.FloorCoordinate) > 3)//杀王玩家超出了金轮阵范围,转向到靠近杀王玩家的BOSS一边
                {
                    Hud.Interaction.MouseMove(DestinationWorldCoordinate.ToScreenCoordinate().X, DestinationWorldCoordinate.ToScreenCoordinate().Y);
                    Hud.Interaction.DoAction(ActionKey.Move);
                }
                else if(skillCripplingWave != null || skillDeadlyReach != null)       
                {
                    if (!DelayTimer.IsRunning|| (DelayTimer.IsRunning && DelayTimer.ElapsedMilliseconds >= 100))//每100毫秒一次
                    {
                        DelayTimer.Restart();
                        if(Boss.SnoMonster.NameEnglish == "Bloodmaw")//血肠
                        {
                            if (!BossSkill(Boss, AnimSnoEnum._x1_westmarchbrute_attack_02_out) && !BossSkill(Boss, AnimSnoEnum._x1_westmarchbrute_b_attack_02_in) && !BossSkill(Boss, AnimSnoEnum._x1_westmarchbrute_idle_01) && !BossSkill(Boss, AnimSnoEnum._x1_westmarchbrute_attack_02_mid) && !BossSkill(Boss, AnimSnoEnum._x1_westmarchbrute_attack_06_out))//不跳的时候
                            {
                                if(Boss.CentralXyDistanceToMe > 30)
                                {//血肠不跳时超出30码则强制移动到血肠身边
                                    Hud.Interaction.MouseMove(BossDirectionWorldCoordinate.ToScreenCoordinate().X, BossDirectionWorldCoordinate.ToScreenCoordinate().Y);
                                    Hud.Interaction.DoAction(ActionKey.Move);
                                }
                                else
                                {//血肠不跳时在30码内鼠标瞄准血肠打拳触发黑人传送至身边
                                    Hud.Interaction.MouseMove(BossWorldCoordinate.ToScreenCoordinate().X, BossWorldCoordinate.ToScreenCoordinate().Y);//鼠标点BOSS
                                    Hud.Interaction.DoAction(skillkey, true);
                                    Hud.Interaction.StandStillUp();
                                }
                            }
                            else
                            {//血肠乱跳时原地打拳，鼠标不动
                                Hud.Interaction.DoAction(skillkey, true);
                                Hud.Interaction.StandStillUp();
                            }
                        }
                        else if(Boss.CentralXyDistanceToMe > 30)//非血肠BOSS超出30码时强制移动到BOSS附近
                        {
                            Hud.Interaction.MouseMove(BossDirectionWorldCoordinate.ToScreenCoordinate().X, BossDirectionWorldCoordinate.ToScreenCoordinate().Y);
                            Hud.Interaction.DoAction(ActionKey.Move);
                        }
                        else
                        {//BOSS在30码内时鼠标瞄准BOSS打拳触发黑人传送至BOSS身边
                            Hud.Interaction.MouseMove(BossWorldCoordinate.ToScreenCoordinate().X, BossWorldCoordinate.ToScreenCoordinate().Y);
                            Hud.Interaction.DoAction(skillkey, true);
                            Hud.Interaction.StandStillUp();
                        }
                        
                    }
                }
            }
        }
        public void PaintTopInGame(ClipState clipState)
        {
            if (clipState != ClipState.AfterClip) return;
            string str_running = (Hud.Game.RiftPercentage < 100) ? str_running1 : str_running2;
            string str_tip = (Hud.Game.RiftPercentage < 100) ? str_tip1 : str_tip2;
            if (isFuzhuMonk)
            {

                if(!Hud.Interaction.IsHotKeySet(ActionKey.Move))
                {
                    InfoFont.DrawText(str_Info, Hud.Game.Me.FloorCoordinate.ToScreenCoordinate().X - InfoFont.GetTextLayout(str_Info).Metrics.Width / 2, Hud.Game.Me.FloorCoordinate.ToScreenCoordinate().Y, true);
                    return;
                }

                if (Running)
                {
                    if (Hud.Game.RiftPercentage == 100)
                    {
                        if (shawang != null)
                        {
                            InfoFont.DrawText(str_running, Hud.Game.Me.FloorCoordinate.ToScreenCoordinate().X - InfoFont.GetTextLayout(str_running).Metrics.Width / 2, Hud.Game.Me.FloorCoordinate.ToScreenCoordinate().Y, true);
                        }
                    }
                    else
                    {
                        InfoFont.DrawText(str_running, Hud.Game.Me.FloorCoordinate.ToScreenCoordinate().X - InfoFont.GetTextLayout(str_running).Metrics.Width / 2, Hud.Game.Me.FloorCoordinate.ToScreenCoordinate().Y, true);
                    }
                }
                else
                {
                    if(Hud.Game.RiftPercentage == 100)
                    {
                        if (shawang != null)
                        {
                            InfoFont.DrawText(ToggleKeyEvent.ToString() + str_tip, Hud.Game.Me.FloorCoordinate.ToScreenCoordinate().X - InfoFont.GetTextLayout(str_tip).Metrics.Width / 2, Hud.Game.Me.FloorCoordinate.ToScreenCoordinate().Y, true);
                        }
                    }
                    else
                    {
                        InfoFont.DrawText(ToggleKeyEvent.ToString() + str_tip, Hud.Game.Me.FloorCoordinate.ToScreenCoordinate().X - InfoFont.GetTextLayout(str_tip).Metrics.Width / 2, Hud.Game.Me.FloorCoordinate.ToScreenCoordinate().Y, true);
                    }
                }
            }
        }
    }
}