using Turbo.Plugins.Default;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using SharpDX.DirectInput;
using System.Threading;
using System.Media;
using Turbo.Plugins.glq;
namespace Turbo.Plugins.flower
{
    public class WitchDoctorTeam : BasePlugin, IInGameWorldPainter, IKeyEventHandler, IInGameTopPainter
    {
        private bool isRightBuild = false;
        private string buildError = string.Empty;
        public TopLabelDecorator nameText { get; set; }
        public TopLabelDecorator stateText { get; set; }
        public IKeyEvent ToggleKeyEvent { get; set; }
        public IKeyEvent WayKeyEvent { get; set; }
        public IKeyEvent RhythmKeyEvent { get; set; }
        public IKeyEvent KillKeyEvent { get; set; }
        public IKeyEvent KeepOnPointKeyEvent { get; set; }
        public IKeyEvent TestingKeyEvent { get; set; }//[alt + *]
        public IKeyEvent OculusRangeKeyEvent { get; set; }//[crtl + ➕]
        public IKeyEvent OculusMoveKeyEvent { get; set; }//手动踩神目[leftControl]
        public IKeyEvent StartLateKeyEvent { get; set; }//开宏延后1秒[key.right]
        public IKeyEvent StartEarlyKeyEvent { get; set; }//开宏提前且延长时间1秒[key.left]
        public bool TurnedOn { get; set; }
        public int HDWay { get; set; }//1:傻瓜方式，2:一轮元素两波

        public bool RhythmSpeak { get; set; }//节奏语音提醒
        public bool KillSpeak { get; set; }//击杀语音提醒
        private bool isTesting = false;//测试模式（城镇里也可以测试）
        private bool keepOnPoint = false;//16秒方式是否一直炸一个点
        private bool isHauntSkill = false;//是否允许放蚀魂
        private bool isLocustSwarmPlus = false;//是否补虫群
        private int oculusRange = 40;//踩神目的码数，默认40码
        private bool isOculusing = false;//是否正在踩神目
        private int tempX = 0;
        private int tempY = 0;
        private int hdNum = 0;//地上有几个魂淡
        private bool isNewRound = true;//是否新的一轮，用于8+8的起手轮
        private bool isRound_8 = false;//8+8要求在火1.0开启
        private bool isRound_16 = false;//16秒错过了物理是否继续
        private bool isSacrifice = true;//是否爆狗

        private double leftSecond = 0;//元素戒时间
        private double startTime = 0;//开宏时机调整
        private int gameTick = 0;

        private int iSpeak = 0;

        private int killEliteNum = -1;
        private int killKeep = 0;
        private int isKeep = 0;
        private int isMark = 0;//标记时间点
        private int markElite = 0;//标记时间点的精英组数
        private int markMonsters = 0;//标记时间点的怪物数量
        private double markPercent = 0;//标记时间点的大秘进度

        public WitchDoctorTeam()
        {
            Enabled = true;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            HDWay = 1;
            RhythmSpeak = true;
            KillSpeak = true;

            ToggleKeyEvent = Hud.Input.CreateKeyEvent(true, Key.F2, false, false, false);
            WayKeyEvent = Hud.Input.CreateKeyEvent(true, Key.NumberPad0, true, false, false);
            KeepOnPointKeyEvent = Hud.Input.CreateKeyEvent(true, Key.NumberPad1, true, false, false);
            TestingKeyEvent = Hud.Input.CreateKeyEvent(true, Key.Multiply, false, true, false);//alt + 小键盘的*
            RhythmKeyEvent = Hud.Input.CreateKeyEvent(true, Key.NumberPad8, true, false, false);
            KillKeyEvent = Hud.Input.CreateKeyEvent(true, Key.NumberPad9, true, false, false);
            OculusRangeKeyEvent = hud.Input.CreateKeyEvent(true, Key.Add, true, false, false);
            OculusMoveKeyEvent = hud.Input.CreateKeyEvent(true, Key.LeftControl, true, false, false);
            StartLateKeyEvent = hud.Input.CreateKeyEvent(true, Key.Right, false, false, false);
            StartEarlyKeyEvent = hud.Input.CreateKeyEvent(true, Key.Left, false, false, false);
            nameText = new TopLabelDecorator(Hud)
            {
                TextFont = Hud.Render.CreateFont("tahoma", 8, 255, 150, 150, 150, true, false, true),
            };
            stateText = new TopLabelDecorator(Hud)
            {
                TextFont = Hud.Render.CreateFont("tahoma", 8, 255, 255, 69, 0, false, false, true),
            };
        }

        public void PaintWorld(WorldLayer layer)
        {
            IPlayer player = Hud.Game.Me;
            IsHunDan(player);

            if (TurnedOn && isRightBuild && string.IsNullOrEmpty(buildError))
            {
                if (isTesting || Hud.Game.SpecialArea == SpecialArea.GreaterRift)
                {
                    if (Hud.Game.SpecialArea == SpecialArea.GreaterRift) isTesting = false;//大秘中关闭测试模式

                    PlayHunDan(player);
                }
            }
        }

        public void PaintTopInGame(ClipState clipState)
        {
            if (clipState == ClipState.BeforeClip && Hud.Game.Me.HeroClassDefinition.HeroClass == HeroClass.WitchDoctor)
            {
                var text = GetMacroName();
                var tip = GetMacroSetting();
                nameText.TextFunc = () => text;
                nameText.HintFunc = () => tip;
                var layout = nameText.TextFont.GetTextLayout(text);
                var x1 = Hud.Window.Size.Width * 0.0022f;
                var y = Hud.Window.Size.Height * (1 - 0.035f);
                var x2 = layout.Metrics.Width + x1 * 2;
                nameText.Paint(x1, y, layout.Metrics.Width, layout.Metrics.Height, HorizontalAlign.Center);

                if (!TurnedOn)
                {
                    switch (Hud.CurrentLanguage)
                    {
                        case Language.zhCN:
                            text = "未启用";
                            break;
                        case Language.zhTW:
                            text = "未啟用";
                            break;
                        default:
                            text = "OFF";
                            break;
                    }
                }
                    
                else
                {
                    if (!isRightBuild)
                    {
                        switch (Hud.CurrentLanguage)
                        {
                            case Language.zhCN:
                                text = "装备或技能不符合";
                                break;
                            case Language.zhTW:
                                text = "裝備或技能不符合";
                                break;
                            default:
                                text = "Build do not meet";
                                break;
                        }
                    }
                        
                    else if (!string.IsNullOrEmpty(buildError))
                        text = buildError;
                    else if (isTesting)
                    {
                        switch (Hud.CurrentLanguage)
                        {
                            case Language.zhCN:
                                text = "测试模式[alt + 小键盘的*]";
                                break;
                            case Language.zhTW:
                                text = "測試模式[alt + 小鍵盤的*]";
                                break;
                            default:
                                text = "Test mode [Alt + NumPad*]";
                                break;
                        }
                    }
                    else
                    {
                        switch (Hud.CurrentLanguage)
                        {
                            case Language.zhCN:
                                text = "已启用" + (Hud.Game.Me.InGreaterRiftRank > 0 ? "" : "[非大秘中不生效]");
                                break;
                            case Language.zhTW:
                                text = "已啟用" + (Hud.Game.Me.InGreaterRiftRank > 0 ? "" : "[非宏偉秘境中不生效]");
                                break;
                            default:
                                text = "ON" + (Hud.Game.Me.InGreaterRiftRank > 0 ? "" : "[Only work in GR]");
                                break;
                        }
                        
                    }
                }
                stateText.TextFunc = () => text;
                stateText.HintFunc = () => GetBuildInfo();
                stateText.Paint(x2, y, stateText.TextFont.GetTextLayout(text).Metrics.Width, stateText.TextFont.GetTextLayout(text).Metrics.Height, HorizontalAlign.Left);
            }
        }

        public void OnKeyEvent(IKeyEvent keyEvent)
        {
            if (keyEvent.IsPressed)
            {
                if (ToggleKeyEvent.Matches(keyEvent))
                {
                    TurnedOn = !TurnedOn;
                    if (!TurnedOn)
                        Hud.Interaction.StopAllContinuousActions();
                }
                else if (WayKeyEvent.Matches(keyEvent))
                {
                    if (HDWay == 1)
                        HDWay = 2;
                    else
                        HDWay = 1;

                    isRound_8 = false;

                    isTesting = false;//切换方式则暂停测试模式
                }
                else if (KeepOnPointKeyEvent.Matches(keyEvent))
                    keepOnPoint = !keepOnPoint;
                else if (RhythmKeyEvent.Matches(keyEvent))
                    RhythmSpeak = !RhythmSpeak;
                else if (KillKeyEvent.Matches(keyEvent))
                    KillSpeak = !KillSpeak;
                else if (TestingKeyEvent.Matches(keyEvent))
                {
                    Hud.Interaction.StopAllContinuousActions();
                    isTesting = !isTesting;
                }
                else if (OculusRangeKeyEvent.Matches(keyEvent))
                {
                    oculusRange += 10;
                    if (oculusRange > 50)
                        oculusRange = 0;
                }
                else if (OculusMoveKeyEvent.Matches(keyEvent))
                {
                    //火元素期间可以设定下轮后面的物理是否爆狗
                    if (leftSecond < 12 && leftSecond > 9 && isSacrifice)
                    {
                        isSacrifice = false;
                        if (RhythmSpeak)
                        {
                            switch (Hud.CurrentLanguage)
                            {
                                case Language.zhCN:
                                    BaseTool.SpeechPrompt(Hud, "留狗", 50);
                                    break;
                                case Language.zhTW:
                                    BaseTool.SpeechPrompt(Hud, "留狗", 50);
                                    break;
                                default:
                                    BaseTool.SpeechPrompt(Hud, "Keep the dog", 50);
                                    break;
                            }
                        }
                            
                    }
                    GoToOculus();
                }
                else if (StartLateKeyEvent.Matches(keyEvent))
                {
                    startTime++;
                    if (startTime > 1)
                        startTime = 0;
                }
                else if (StartEarlyKeyEvent.Matches(keyEvent))
                {
                    startTime--;
                    if (startTime < -1)
                        startTime = 0;
                }
            }
        }
        
        #region tool
        public string GetMacroName()
        {
            var name = "";
            if (HDWay == 1)
            {
                
                switch (Hud.CurrentLanguage)
                {
                    case Language.zhCN:
                        name = "魂弹组队16秒傻瓜宏";
                        break;
                    case Language.zhTW:
                        name = "魂彈組隊16秒傻瓜宏";
                        break;
                    default:
                        name = "Spirit Barrage Witch Doctor team(16s Cycle)";
                        break;
                }
                return name;
                
            }
            else
            {
                switch (Hud.CurrentLanguage)
                {
                    case Language.zhCN:
                        name = "魂弹组队8+8节奏宏";
                        break;
                    case Language.zhTW:
                        name = "魂彈組隊8+8節奏宏";
                        break;
                    default:
                        name = "Spirit Barrage Witch Doctor team(8s+8s Cycle)";
                        break;
                }
                return name;
            }
        }

        private string GetKeepOnePoint()
        {
            if (HDWay == 1)
            {
                string _str = "";
                switch (Hud.CurrentLanguage)
                {
                    case Language.zhCN:
                        _str = "16秒单点埋雷：" + (keepOnPoint ? "是" : "否");
                        break;
                    case Language.zhTW:
                        _str = "16秒單點埋雷：" + (keepOnPoint ? "是" : "否");
                        break;
                    default:
                        _str = "keep On Point: " + (keepOnPoint ? "ON" : "OFF");
                        break;
                }
                return _str;
            }
            else
            {
                return string.Empty;
            }
        }

        private string GetStartTime()
        {
            string _str = "";
            if (startTime > 0)
            {
                switch (Hud.CurrentLanguage)
                {
                    case Language.zhCN:
                        _str = "延后1秒";
                        break;
                    case Language.zhTW:
                        _str = "延後1秒";
                        break;
                    default:
                        _str = "Delay 1s";
                        break;
                }
                return _str;
            }
            else if (startTime < 0)
            {
                switch (Hud.CurrentLanguage)
                {
                    case Language.zhCN:
                        _str = "提前且延长1秒";
                        break;
                    case Language.zhTW:
                        _str = "提前且延長1秒";
                        break;
                    default:
                        _str = "Advance and extend by 1s";
                        break;
                }
                return _str;
            }
            else
            {
                switch (Hud.CurrentLanguage)
                {
                    case Language.zhCN:
                        _str = "正常";
                        break;
                    case Language.zhTW:
                        _str = "正常";
                        break;
                    default:
                        _str = "normal";
                        break;
                }
                return _str;
            }
                
            
        }
        
        public string GetMacroSetting()
        {
            string _str = "";
            switch (Hud.CurrentLanguage)
            {
                case Language.zhCN:
                    _str = $@"Flower宏当前设置：

　　宏名称：{GetMacroName()}
　　{GetKeepOnePoint()}
　　神目范围：{oculusRange}码（多少范围内自动去踩，0码则不自动踩换按Ctrl键手动踩）
　　节奏提醒：{(RhythmSpeak ? "是" : "否")}
　　击杀提醒：{(KillSpeak ? "是" : "否")}
　　启动时间：{GetStartTime()}
　　【鼠标移到右边的红字可看详细说明】>>>>>
";
                    break;
                case Language.zhTW:
                    _str = $@"Flower宏當前設置：

　　宏名稱：{GetMacroName()}
　　{GetKeepOnePoint()}
　　神目範圍：{oculusRange}碼（多少範圍內自動去踩，0碼則不自動踩換按Ctrl鍵手動踩）
　　節奏提醒：{(RhythmSpeak ? "是" : "否")}
　　擊殺提醒：{(KillSpeak ? "是" : "否")}
　　啟動時間：{GetStartTime()}
　　【鼠標移到右邊的紅字可看詳細說明】>>>>>
";
                    break;
                default:
                    _str = $@"current settings:

　　Macro: {GetMacroName()}
　　{GetKeepOnePoint()}
　　Oculus range: {oculusRange} yard(auto move to the Oculus range if the range is not 0 yard, press Ctrl to move manually)
　　Step Speak: {(RhythmSpeak ? "ON" : "OFF")}
　　Kill Speak: {(KillSpeak ? "ON" : "OFF")}
　　Correction Time: {GetStartTime()}
　　【Hover the cursor over the red words on the right to see details】>>>>>
";
                    break;
            }
            return _str;
        }

        public string GetBuildInfo()
        {
            string _str = "";
            switch (Hud.CurrentLanguage)
            {
                case Language.zhCN:
                    _str = @"Flower宏（魂淡组队）使用说明：
【广告】：
　　非赛季的“非联邦”战队征集队友，有意请来YY:2488 5542；
　　另请关注斗鱼“TNT”的直播，斗鱼房间号：597372；
　　关于宏的使用和队伍配合欢迎到YY房间或斗鱼房间咨询

技能要求及推荐键位：
　　[1] 亡者之握：死即是生 或 巫毒狂舞：舞尸化犬 或 召唤僵尸犬：生命连结
　　[2] 牺　牲　：激怒兽群
　　[3] 灵魂行走：撞魂
　　[4] 灵魂弹幕
　　[左]灵魂收割：困魂压迫
　　[右]蚀　魂 或 瘟疫虫群
　　(注：蚀魂 不允许放在左键，其它键位随意调整)

装备要求：
　　特效：剃头师、观死、全能戒指、空虚戒指
　　务必让范围伤为0，巅峰里的范围伤不要点

操作指南：
　　进入大秘后按下快捷键开启，开启后左下角有是否启用提示
　　启动宏后，灵魂收割会自动保持
　　灵行只用于跑神目（血量低于30%自动激活保命)，赶路阶段请自行使用保持减伤
　　强制移动时可打断宏，站定后会根据情况自行进入16秒循环序列，
　　火元素期间按下Ctrl则下个物理就不爆狗
　　（周边有5只怪被上了蚀魂时开始进入循环，不足5只怪需要全部有蚀魂）

调整热键：
　　16秒和8+8切换：Ctrl + (小键盘的0)
　　16秒埋雷点干预：Ctrl + (小键盘的1)
　　踩神目的范围：Ctrl + (小键盘的+)[每次10码,最大50码,默认40码]
　　手动踩神目：[踩神目的范围为0时，按下左边的Ctrl键自动走到指定位置]
　　节奏语音提醒：Ctrl + (小键盘的8)
　　击杀语音提醒：Ctrl + (小键盘的9)
　　宏启动时间调整：键盘的左右键

版本差异：
　　16秒傻瓜宏：爆狗-→连炸(物理2-毒2)-→出圈则停炸-→10秒自爆(冰3时丢亡者)
　　8+8 节奏宏：3雷(火1)-→6秒-→爆狗-→亡者-→3雷(毒1)-→神目-→亡者
　　请队伍根据情况选用版本，两个版本都是只能爆3狗

当前版本：21.12.8.0
作者：Flower
<<<<<【鼠标移到左边的白字可看宏的当前设定】
";
                    break;
                case Language.zhTW:
                    _str = @"Flower宏（魂淡組隊）使用說明：
【廣告】：
　　非賽季的“非聯邦”戰隊征集隊友，有意請來YY:2488 5542；
　　另請關註鬥魚“TNT”的直播，鬥魚房間號：597372；
　　關於宏的使用和隊伍配合歡迎到YY房間或鬥魚房間咨詢

技能要求及推薦鍵位：
　　[1] 亡者之握：死即是生 或 大巫毒儀式：惡靈鬼舞 或 召喚僵屍犬：生命連結
　　[2]  獻  祭 ：屍犬之怒
　　[3] 靈行術：靈肉分離
　　[4] 魂靈彈幕
　　[左]魂靈收割：魂魄凋零
　　[右]蝕魂 或 召喚蟲群
　　(註：蝕魂 不允許放在左鍵，其它鍵位隨意調整)

裝備要求：
　　特效：剃頭師、滅亡凝視、元素嘉年華、空虛之戒
　　務必讓範圍傷為0，巔峰裏的範圍傷不要點

操作指南：
　　進入宏伟密境按下快捷鍵開啟，開啟後左下角有是否啟用提示
　　啟動宏後，魂靈收割會自動保持
　　靈行只用於跑神目（血量低於30%自動激活保命)，趕路階段請自行使用保持減傷
　　強製移動時可打斷宏，站定後會根據情況自行進入16秒循環序列，
　　火元素期間按下Ctrl則下個物理就不爆狗
　　（周邊有5只怪被上了蝕魂時開始進入循環，不足5只怪需要全部有蝕魂）

調整熱鍵：
　　16秒和8+8切換：Ctrl + (小鍵盤的0)
　　16秒埋雷點幹預：Ctrl + (小鍵盤的1)
　　踩神目的範圍：Ctrl + (小鍵盤的+)[每次10碼,最大50碼,默認40碼]
　　手動踩神目：[踩神目的範圍為0時，按下左邊的Ctrl鍵自動走到指定位置]
　　節奏語音提醒：Ctrl + (小鍵盤的8)
　　擊殺語音提醒：Ctrl + (小鍵盤的9)
　　宏啟動時間調整：鍵盤的左右鍵

版本差異：
　　16秒傻瓜宏：爆狗-→連炸(物理2-毒2)-→出圈則停炸-→10秒自爆(冰3時丟亡者)
　　8+8 節奏宏：3雷(火1)-→6秒-→爆狗-→亡者-→3雷(毒1)-→神目-→亡者
　　請隊伍根據情況選用版本，兩個版本都是只能爆3狗

當前版本：21.12.8.0
作者：Flower
<<<<<【鼠標移到左邊的白字可看宏的當前設定】
";
                    break;
                default:
                    _str = @"detailed description:

Skills：
　　[1] Grasp of the Dead:Death Is Life or Big Bad Voodoo:Boogie Man or Summon Zombie Dogs:Life Link
　　[2] Sacrifice:Provoke the Pack
　　[3] Spirit Walk:Severance
　　[4] Spirit Barrage
　　[L] Soul Harvest:Languish
　　[R] Haunt or Locust Swarm
　　(ps:Haunt does not support mouse left button)

Equipments:
　　Powers: The Barber, Gazing Demise, Convention of Elements, Ring of Emptiness

Other:
　　Area damage must be 0.

Operation guide:
　　After entering the Great rift, press the hotkey to Enable the macro.
　　Soul Harvest will auto keep buff
　　Spirit Walk for move to Oculus(auto cast when HP < 30%),Please cast it manually during transfer
　　the macro will interrupted when force move, When you stand, it will re-enter the cycle according to the situation
　　If Ctrl is pressed during the fire, the dog will not be cast in the next physical
　　（When 5 monsters are surrounded by Haunt,if less than 5 monsters then they all need to surrounded by Haunt, it will enter the cycle.）

hotkeys：
　　Switch between 16Ss and 8s+8s cycles: Ctrl + (NumPad0)
　　Keep One Point: Ctrl + (NumPad1)
　　Oculus range: Ctrl + (NumPad+)[10 yards at a time, 50 yards at most, 40 yards by default]
　　Manually move to Oculus：[When Oculus range is 0, Press Ctrl to move to the cursor position]
　　Step Speak: Ctrl + (NumPad8)
　　Kill Speak: Ctrl + (NumPad9)
　　Correction Time: arrow key ← and →

Version difference：
　　16s Cycle：cast Sacrifice → cast Spirit Barrage continuously(Physical2-poison2) → Stop when Oculus was triggered → Self explosion in 10 seconds(Cast Grasp of the Dead when Cold3)
　　8s+8s Cycle：cast Spirit Barrage 3 times(fire1) →Wait 6s →cast Sacrifice → Cast Grasp of the Dead →cast Spirit Barrage 3 times(poison1) → Oculus → Cast Grasp of the Dead
　　Please select the version according to the team situation. Both versions can only explode 3 dogs

version: 21.12.8.0
Creator: Flower
<<<<<【Hover the cursor over the white words on the left to see current settings】
";
                    break;
            }
            return _str;
        }

        private void IsHunDan(IPlayer player)
        {
            //魂谈套路：剃头师、观死、全能、空虚是必须的；
            isRightBuild = player.HeroClassDefinition.HeroClass == HeroClass.WitchDoctor
                && Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.TheBarber.Sno)//BaseTool.FindEquipmentOnPlayer(Hud, player, 3525859087)  //剃头师
                && Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.GazingDemise.Sno)//BaseTool.FindEquipmentOnPlayer(Hud, player, 1003477242)  //观死
                && Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.ConventionOfElements.Sno)//BaseTool.FindEquipmentOnPlayer(Hud, player, 417069418)  //全能
                && Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.RingOfEmptiness.Sno)//BaseTool.FindEquipmentOnPlayer(Hud, player, 1481522298)  //空虚
                && player.Powers.UsedWitchDoctorPowers.SpiritBarrage != null//灵魂弹幕
                && player.Powers.UsedWitchDoctorPowers.SoulHarvest != null//灵魂收割
                && player.Powers.UsedWitchDoctorPowers.SpiritWalk != null//灵魂行走
                && player.Powers.UsedWitchDoctorPowers.Sacrifice != null//牺牲
                && (player.Powers.UsedWitchDoctorPowers.Haunt != null || player.Powers.UsedWitchDoctorPowers.LocustSwarm != null) //蚀魂或者虫群
                //&& (player.Powers.UsedWitchDoctorPowers.GraspOfTheDead != null//留着空余的技能，看自己喜欢来放
                //|| player.Powers.UsedWitchDoctorPowers.BigBadVoodoo != null)//亡者或者大巫毒
            //&& player.Powers.UsedWitchDoctorPowers.LocustSwarm != null
            //&& BaseTool.FindThisSkill(Hud, player, Hud.Sno.SnoPowers.WitchDoctor_SpiritBarrage.Sno)//魂淡 108506
            //&& BaseTool.FindThisSkill(Hud, player, Hud.Sno.SnoPowers.WitchDoctor_SoulHarvest.Sno)//收割 67616
            //&& BaseTool.FindThisSkill(Hud, player, Hud.Sno.SnoPowers.WitchDoctor_SpiritWalk.Sno)//灵行 106237
            //&& BaseTool.FindThisSkill(Hud, player, Hud.Sno.SnoPowers.WitchDoctor_GraspOfTheDead.Sno)//亡者 69182
            //&& BaseTool.FindThisSkill(Hud, player, Hud.Sno.SnoPowers.WitchDoctor_Sacrifice.Sno)//牺牲 102572
            //&& (BaseTool.FindThisSkill(Hud, player, Hud.Sno.SnoPowers.WitchDoctor_Haunt.Sno)//蚀魂 83602
            //|| BaseTool.FindThisSkill(Hud, player, Hud.Sno.SnoPowers.WitchDoctor_LocustSwarm.Sno))//虫群 69867
            ;

            if (isRightBuild && player.Powers.UsedWitchDoctorPowers.Haunt != null && player.Powers.UsedWitchDoctorPowers.Haunt.Key == ActionKey.LeftSkill)
            {
                switch (Hud.CurrentLanguage)
                {
                    case Language.zhCN:
                        buildError = "不允许蚀魂在左键";
                        break;
                    case Language.zhTW:
                        buildError = "不允許蝕魂在左鍵";
                        break;
                    default:
                        buildError = "Haunt does not support mouse left button";
                        break;
                }
            }
                
            else if (Hud.Game.Me.Offense.BonusToCold <= 0)
            {
                switch (Hud.CurrentLanguage)
                {
                    case Language.zhCN:
                        buildError = "冰元素加成不能为0";
                        break;
                    case Language.zhTW:
                        buildError = "冰元素加成不能為0";
                        break;
                    default:
                        buildError = "Cold bonus cannot be 0";
                        break;
                }
            }
            else if (!string.IsNullOrEmpty(buildError))
                buildError = string.Empty;
        }

        private IActor GetHunDan(int range)
        {
            foreach (var actor in Hud.Game.Actors)
            {
                if (actor.NormalizedXyDistanceToMe < range && actor.SnoActor.Sno == ActorSnoEnum._wd_spiritbarragerune_aoe_ghostmodel)
                {
                    return actor;
                }
            }

            return null;
        }
        #endregion

        #region 技能释放
        //保持收割
        private void KeepSoulHarvest(IPlayer player)
        {
            var skill = player.Powers.UsedWitchDoctorPowers.SoulHarvest;
            if (skill != null && !skill.IsOnCooldown)
            {
                int num = BaseTool.CountMonstersByRange(Hud, 18);//18码内怪物数量
                if (skill.BuffIsActive && num > 0)//保持阶段只要有怪就释放
                {
                    BaseTool.KeyPressAndRelease(Hud, skill.Key);
                }
                else if (num > 5)//没有保持的时候5个怪以上释放
                {
                    BaseTool.KeyPressAndRelease(Hud, skill.Key);
                }
            }
        }

        //用灵行保命
        private void DoSpiritWalk(IPlayer player)
        {
            var skill = player.Powers.UsedWitchDoctorPowers.SpiritWalk;
            if (skill == null) return;

            //30以下的血量则使用灵行保命
            if (player.Defense.HealthPct < 30 && !skill.IsOnCooldown)
            {
                BaseTool.KeyPressAndRelease(Hud, skill.Key);
            }
        }

        //丢蚀魂
        private void DoHaunt(IPlayer player, int numNotHuated)
        {
            var skill = player.Powers.UsedWitchDoctorPowers.Haunt;
            if (skill == null || player.Stats.ResourcePctMana < 30) return;
            if (isHauntSkill)
            {
                //BaseTool.DebugInfo("丢蚀魂", 5);

                if (numNotHuated > 0 && !Hud.Interaction.IsContinuousActionStarted(skill.Key))
                    BaseTool.KeyHold(Hud, skill.Key);

                if (numNotHuated == 0 && Hud.Interaction.IsContinuousActionStarted(skill.Key))
                    BaseTool.KeyUnHold(Hud, skill.Key);
            }
            else if (Hud.Interaction.IsContinuousActionStarted(skill.Key))
            {
                BaseTool.KeyUnHold(Hud, skill.Key);
            }
        }

        //丢虫群
        private void DoLocustSwarm(IPlayer player,int numMonster, int numAffected)
        {
            var skill = player.Powers.UsedWitchDoctorPowers.LocustSwarm;
            if (skill == null || player.Stats.ResourcePctMana < 30) return;

            //物理1.0-1.1、毒3.5-3.6、火0-0.1（一共放三个）
            if ((LeftSecondUpdated <= 7 && LeftSecondUpdated >= 6.9)
                || (hdNum > 0 && leftSecond <= 0.5 && leftSecond >= 0.4)
                || (hdNum > 0 && leftSecond <= 12 && leftSecond >= 11.9))
            {
                isHauntSkill = false;//丢虫群的期间不允许丢蚀魂
                BaseTool.KeyPressAndRelease(Hud, skill.Key);
            }

            //如果物理和毒的虫群没有按出来，这里补按一下
            if ((leftSecond <= 6 && leftSecond >= 5)
                || (hdNum > 0 && leftSecond <= 15 && leftSecond >= 14 && !player.Powers.BuffIsActive(Hud.Sno.SnoPowers.OculusRing.Sno, 2)))
            {
                if ((numMonster >= 5 && numAffected < 5) || (numMonster < 5 && numMonster != numAffected))
                {
                    if (!isLocustSwarmPlus)
                    {
                        BaseTool.KeyPressAndRelease(Hud, skill.Key);
                        isLocustSwarmPlus = true;
                    }
                }
            }
            else
            {
                isLocustSwarmPlus = false;
            }
        }

        //放亡者
        private void DoGraspOfTheDead(IPlayer player)
        {
            var skill = player.Powers.UsedWitchDoctorPowers.GraspOfTheDead;
            if (skill == null || skill.IsOnCooldown || hdNum == 0) return;//技能CD中或者没有魂淡圈直接返回

            if (HDWay == 1)
            {
                //火1.0-2.5丢亡者，方便爆炸后产狗
                if (leftSecond < 11 && leftSecond > 9.5)
                {
                    //采用了单点模式的则丢在魂淡上
                    if (keepOnPoint)
                    {
                        var actorHundan = GetHunDan(40);//获取一个魂淡
                        if (actorHundan != null)                            
                            Hud.Interaction.MouseMove(actorHundan.ScreenCoordinate.X, actorHundan.ScreenCoordinate.Y);//鼠标移到魂淡圈的中心点上，然后释放
                    }

                    BaseTool.KeyPressAndRelease(Hud, skill.Key);
                }
            }
            else
            {
                //毒0.0或者火0.0丢亡者（在地上有魂淡的情况下）
                if ((leftSecond < 4 && leftSecond > 3) || (leftSecond < 12 && leftSecond > 11))
                {
                    //这里鼠标不自动移动
                    BaseTool.KeyPressAndRelease(Hud, skill.Key);
                }
            }
        }

        //放巫毒
        private void DoBigBadVoodo(IPlayer player)
        {
            var skill = player.Powers.UsedWitchDoctorPowers.BigBadVoodoo;
            if (skill == null || skill.IsOnCooldown) return;//技能CD中直接返回

            //物理0.0到物理1.0丢巫毒
            if (leftSecond < 8 && leftSecond > 7)
            {
                BaseTool.KeyPressAndRelease(Hud, skill.Key);
            }
        }

        //召唤僵尸犬
        private void DoSummonZombieDog(IPlayer player)
        {
            var skill = player.Powers.UsedWitchDoctorPowers.SummonZombieDog;
            if (skill == null || skill.IsOnCooldown) return;

            //少于2条狗就召
            int dos = BaseTool.CountZombieDog(Hud);
            if (dos < 2)
            {
                BaseTool.KeyPressAndRelease(Hud, skill.Key);
            }

            //爆狗后，必须是3条狗的情况下采用，不看元素戒指
            if (BaseTool.GetBuffCountAssignedPlayer(Hud, player, 102572, 0) >= 3)
            {

            }
        }

        //爆狗
        private void DoSacrifice(IPlayer player)
        {
            var skill = player.Powers.UsedWitchDoctorPowers.Sacrifice;
            if (skill == null) return;

            if (HDWay == 1)
            {
                //物理1.5-4.0共2.5秒爆狗（每爆一只狗都是单独的5秒，重叠时间内才增加层数）【由于主动召狗延长了爆狗时间2->2.5】
                if (isSacrifice && LeftSecondUpdated <= 6.5 && LeftSecondUpdated > 4)
                {
                    isHauntSkill = false;//爆狗期间不允许丢蚀魂

                    //有狗且低于5层则爆，没有则下一步
                    int num = BaseTool.CountZombieDog(Hud);
                    if (num > 0 && CountSacrifice(player) < 5)
                        BaseTool.KeyPressAndRelease(Hud, skill.Key);
                }
            }
            else
            {
                //物理3.0到毒1.0共2秒爆狗（每爆一只狗都是单独的5秒，重叠时间内才增加层数）
                if (isRound_8 && leftSecond < 5 && leftSecond > 3)
                {
                    isHauntSkill = false;

                    //有狗则爆，没有则下一步
                    int num = BaseTool.CountZombieDog(Hud);
                    if (num > 0)
                        BaseTool.KeyPressAndRelease(Hud, skill.Key);
                    else
                        isHauntSkill = true;//没有狗了就上蚀魂
                }
            }
        }

        //丢魂淡
        private void DoSpiritBarrage(IPlayer player)
        {
            var skill = player.Powers.UsedWitchDoctorPowers.SpiritBarrage;
            if (skill == null || player.Stats.ResourcePctMana < 15) return;

            if (HDWay == 1)
            {
                //物理0.0-3.0，如果没被干预，则后面就丢魂淡
                if (leftSecond < 8 && leftSecond > 5 && !isRound_16)
                    isRound_16 = true;

                //物理期间被干预了，则不丢魂淡了
                if (!isRound_16) return;

                //有狗且没满5层牺牲
                if (LeftSecondUpdated < 6 && LeftSecondUpdated > 4.5 && BaseTool.CountZombieDog(Hud) > 0 && CountSacrifice(player) < 5)
                    return;

                if (LeftSecondUpdated < 6 && leftSecond > (startTime < 0 ? 0.5 : 1.5))
                {
                    isHauntSkill = false;//丢魂淡的期间不允许丢蚀魂

                    if (BaseTool.GetOculus(Hud).Count() <= 0)
                    {
                        //地上有魂淡且是单点模式，则鼠标自动移到上一个魂淡的位置
                        if (hdNum > 0 && keepOnPoint)
                        {
                            //TODO:会抢鼠标，改进方便：地上的魂淡圈没有新增之前只移动一次
                            var actorHundan = GetHunDan(40);//获取一个魂淡 
                            if (actorHundan != null)
                                Hud.Interaction.MouseMove(actorHundan.ScreenCoordinate.X, actorHundan.ScreenCoordinate.Y);
                        }

                        BaseTool.KeyPressAndRelease(Hud, skill.Key);//没有神目一直丢
                    }
                    else
                    {
                        int numNew = BaseTool.CountHunDanByTime(Hud, 7, true);
                        if (numNew < 3)
                        {
                            //地上有魂淡且是单点模式，则鼠标自动移到上一个魂淡的位置
                            if (numNew > 0 && keepOnPoint)
                            {
                                var actorHundan = GetHunDan(40);//获取一个魂淡 
                                if (actorHundan != null)
                                    Hud.Interaction.MouseMove(actorHundan.ScreenCoordinate.X, actorHundan.ScreenCoordinate.Y);
                            }

                            BaseTool.KeyPressAndRelease(Hud, skill.Key);//有神目不足3个魂淡，则一直丢到3个为止
                        }
                        else
                        {
                            isHauntSkill = true;//丢完魂淡了可以提前丢蚀魂
                        }
                    }
                }
            }
            else
            {
                //火1.0到火2.5埋3个雷来出神目，一般都是1秒就完成了
                if (leftSecond < 11 && leftSecond > 9.5)
                {
                    isHauntSkill = false;

                    int numNew = BaseTool.CountHunDanByTime(Hud, 7, true);//剩余时间>7秒的魂淡个数
                    //新的一轮丢两个，一个点循环第二轮丢3个
                    if ((isNewRound && numNew < 2) || (!isNewRound && numNew < 3))
                    {
                        isRound_8 = true;
                        BaseTool.KeyPressAndRelease(Hud, skill.Key);
                    }
                    else
                    {
                        isHauntSkill = true;
                        if (isNewRound)
                            isNewRound = false;
                    }
                }

                //毒1.0到毒2.5埋3个神目雷
                if (isRound_8 && leftSecond < 3 && leftSecond > 0.5)
                {
                    isHauntSkill = false;

                    //剩余时间>7秒的魂淡个数
                    int numNew = BaseTool.CountHunDanByTime(Hud, 7, true);
                    if (numNew < 3)
                    {
                        BaseTool.KeyPressAndRelease(Hud, skill.Key);
                    }
                    else
                    {
                        isHauntSkill = true;
                    }
                }
            }
        }

        //自动踩神目
        private void GoToOculus(IPlayer player)
        {
            //有灵行的情况下才去踩神目
            var skill = player.Powers.UsedWitchDoctorPowers.SpiritWalk;
            if (skill == null || oculusRange == 0 || hdNum == 0) return;
            if (!Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.ConventionOfElements.Sno, 7)) return;//不是毒元素时不踩神目
            var spiritbarrages = Hud.Game.Actors.Where(a => a.SnoActor.Sno == ActorSnoEnum._wd_spiritbarragerune_aoe_ghostmodel && a.SummonerAcdDynamicId == Hud.Game.Me.SummonerId && (10 -  ((Hud.Game.CurrentGameTick - a.CreatedAtInGameTick) / 60d) >= 7.5));//大于7.5秒剩余时间的灵魂弹幕才算
            if (spiritbarrages == null) return;
            var spiritbarrage = spiritbarrages.OrderByDescending(a => a.CreatedAtInGameTick).FirstOrDefault();
            if (spiritbarrages.Count() < 3)
            {
                return;//地面没有灵魂弹幕或不足3个时不跑神目
            }
            //var oculusActor = BaseTool.GetNearestOculus(Hud, oculusRange);//指定码数内的神目
            var oculusActor = Hud.Game.Actors.Where(x => x.SnoActor.Sno == ActorSnoEnum._generic_proxy
            && x.GetAttributeValueAsInt(Hud.Sno.Attributes.Power_Buff_1_Visual_Effect_None, Hud.Sno.SnoPowers.OculusRing.Sno) == 1 && x.FloorCoordinate.XYDistanceTo(spiritbarrage.FloorCoordinate) <= oculusRange).OrderByDescending(a => a.FloorCoordinate.XYDistanceTo(spiritbarrage.FloorCoordinate)).FirstOrDefault();//取出离灵魂弹幕oculusRange范围内最远的神目
            if (oculusActor == null)
            {
                isOculusing = false;
            }
                
            if (oculusActor != null)//有神目(402461)
            {
                //8+8模式出神目0.8秒后再去踩
                if (HDWay != 1 && Hud.Game.CurrentGameTick - oculusActor.CreatedAtInGameTick < 48)
                    return;

                if (isOculusing || player.Powers.BuffIsActive(Hud.Sno.SnoPowers.OculusRing.Sno, 2))
                {
                    if (!isOculusing)
                    {
                        isOculusing = true;
                    }
                }
                else
                {
                    //没有踩到，灵行可用，则按下灵行
                    if (!skill.IsOnCooldown)
                    {
                        BaseTool.KeyPressAndRelease(Hud, skill.Key);
                        tempX = Hud.Window.CursorX;
                        tempY = Hud.Window.CursorY;
                    }
                    //灵行中去踩，每6帧点一次强制移动，为避免移动时神目错位，所以移动时不再点击
                    if (skill.BuffIsActive && Hud.Game.Me.AnimationState != AcdAnimationState.Running && Hud.Game.CurrentGameTick - gameTick > 12)
                    {
                        gameTick = Hud.Game.CurrentGameTick;

                        //获取踩的坐标点
                        //var pos = PublicClassPlugin.getNewPointOnLine(Hud, player.FloorCoordinate, oculusActor.FloorCoordinate, 10);
                        var pos = PublicClassPlugin.getNewPointOnLine(Hud, spiritbarrage.FloorCoordinate, oculusActor.FloorCoordinate, (float)(oculusActor.FloorCoordinate.XYDistanceTo(spiritbarrage.FloorCoordinate) + 10));
                        if (pos != null)
                        {
                            //BaseTool.DebugInfo("踩神目", 6);
                            Hud.Interaction.MouseMove(pos.ToScreenCoordinate().X, pos.ToScreenCoordinate().Y);
                            Hud.Interaction.DoAction(ActionKey.Move, false, 30, 10, 10);
                            Hud.Interaction.MouseMove(tempX, tempY);
                        }
                    }
                }
            }
            else
            {
                if (gameTick != 0)
                    gameTick = 0;
            }

            
        }

        //手动踩神目
        private void GoToOculus()
        {
            if (oculusRange == 0)
            {
                var skill = Hud.Game.Me.Powers.UsedWitchDoctorPowers.SpiritWalk;
                if (skill != null)
                    BaseTool.KeyPressAndRelease(Hud, skill.Key);
                //获取鼠标点
                //var mouseCoor = Hud.Window.CreateScreenCoordinate(Hud.Window.CursorX, Hud.Window.CursorY);
                Hud.Interaction.DoAction(ActionKey.Move);
            }
        }

        //走到高密度怪去
        private void GoToMonoster(IPlayer player)
        {
            //没有神目且在火元素期间，2秒内尝试；也许不一定好，走的时间可以丢蚀魂
            if (!isOculusing && leftSecond < 11 && leftSecond > 9)
            {
                var monster = BaseTool.GetDensityMonster(Hud, 15);
                if (monster != null)
                {
                    //获取坐标点
                    var pos = PublicClassPlugin.getNewPointOnLine(Hud, player.FloorCoordinate, monster.FloorCoordinate, 10);
                    if (pos != null)
                    {
                        Hud.Interaction.MouseMove(pos.ToScreenCoordinate().X, pos.ToScreenCoordinate().Y);
                        Hud.Interaction.DoAction(ActionKey.Move);
                    }
                }
            }
        }

        private int CountMonster(IPlayer player)
        {
            if (player.Powers.UsedWitchDoctorPowers.Haunt != null)
                return BaseTool.CountMonstersByRange(Hud, 40);//带了蚀魂返回40码内的
            else
                return BaseTool.CountMonstersByRange(Hud, 20);//没带蚀魂返回20码内的
        }

        private int CountMonsterAffected(IPlayer player)
        {
            if (player.Powers.UsedWitchDoctorPowers.Haunt != null) 
                return Hud.Game.AliveMonsters.Where(m => m.NormalizedXyDistanceToMe <= 40 && (m.Haunted || m.Locust)).Count();//范围内有蚀魂或虫群的怪物
            else
                return Hud.Game.AliveMonsters.Where(m => m.NormalizedXyDistanceToMe <= 20 && m.Locust).Count();//范围内有虫群的怪物
        }

        //牺牲的层数
        private int CountSacrifice(IPlayer player)
        {
            return BaseTool.GetBuffCountAssignedPlayer(Hud, player, 102572, 0);
        }

        //提前或者延后
        private double LeftSecondUpdated
        {
            get
            {
                if (startTime != 0 && leftSecond <= 8)
                {
                    return leftSecond + startTime;
                }

                return leftSecond;
            }
        }
        #endregion

        #region 节凑提醒、精英击杀提醒
        private void Rhythm()
        {
            if (!RhythmSpeak) return;

            if (HDWay == 1)
            {
                if (leftSecond < 8 && leftSecond > 7)
                {
                    if (iSpeak != 1)
                    {
                        switch (Hud.CurrentLanguage)
                        {
                            case Language.zhCN:
                                BaseTool.SpeechPrompt(Hud, "炸圈", 100);
                                break;
                            case Language.zhTW:
                                BaseTool.SpeechPrompt(Hud, "炸圈", 100);
                                break;
                            default:
                                BaseTool.SpeechPrompt(Hud, "Trigger Oculus", 100);
                                break;
                        }
                        
                        iSpeak = 1;
                    }
                }

                if (hdNum >= 3 && leftSecond < 3 && leftSecond > 1)
                {
                    if (iSpeak != 2)
                    {
                        switch (Hud.CurrentLanguage)
                        {
                            case Language.zhCN:
                                BaseTool.SpeechPrompt(Hud, "点炮", 100);
                                break;
                            case Language.zhTW:
                                BaseTool.SpeechPrompt(Hud, "点炮", 100);
                                break;
                            default:
                                BaseTool.SpeechPrompt(Hud, "fire in the hole", 100);
                                break;
                        }
                        iSpeak = 2;
                    }
                }

                if (leftSecond > 10 && iSpeak > 0)
                    iSpeak = 0;//重置节奏提醒
            }
            else
            {
                if (hdNum == 0 && leftSecond < 2)
                {
                    if (iSpeak != 100)
                    {
                        switch (Hud.CurrentLanguage)
                        {
                            case Language.zhCN:
                                BaseTool.SpeechPrompt(Hud, "巫医准备", 2000);
                                break;
                            case Language.zhTW:
                                BaseTool.SpeechPrompt(Hud, "巫医准备", 2000);
                                break;
                            default:
                                BaseTool.SpeechPrompt(Hud, "Witch Doctor to ready", 2000);
                                break;
                        }
                        iSpeak = 100;
                    }
                }

                if (hdNum == 0 && leftSecond < 16 && leftSecond > 13)
                {
                    int n = (int)(leftSecond - 12);
                    if (iSpeak != 333 && n == 3)
                    {
                        BaseTool.SpeechPrompt(Hud, n.ToString(), 10);//3秒
                        iSpeak = 333;
                    }

                    if (iSpeak != 222 && n == 2)
                    {
                        BaseTool.SpeechPrompt(Hud, n.ToString(), 10);//2秒
                        iSpeak = 222;
                    }

                    if (iSpeak != 111 && n == 1)
                    {
                        BaseTool.SpeechPrompt(Hud, n.ToString(), 10);//1秒
                        iSpeak = 111;
                    }
                }

                if (leftSecond < 13 && leftSecond > 12)
                {
                    if (iSpeak != 1)
                    {
                        switch (Hud.CurrentLanguage)
                        {
                            case Language.zhCN:
                                BaseTool.SpeechPrompt(Hud, "第一发", 10);
                                break;
                            case Language.zhTW:
                                BaseTool.SpeechPrompt(Hud, "第一發", 10);
                                break;
                            default:
                                BaseTool.SpeechPrompt(Hud, "First shell", 10);
                                break;
                        }
                        iSpeak = 1;
                    }
                }

                if (hdNum >= 3 && leftSecond < 5 && leftSecond > 4)
                {
                    if (iSpeak != 2)
                    {
                        switch (Hud.CurrentLanguage)
                        {
                            case Language.zhCN:
                                BaseTool.SpeechPrompt(Hud, "第二发", 10);
                                break;
                            case Language.zhTW:
                                BaseTool.SpeechPrompt(Hud, "第二發", 10);
                                break;
                            default:
                                BaseTool.SpeechPrompt(Hud, "Second shell", 10);
                                break;
                        }
                        iSpeak = 2;
                    }
                }

                if (leftSecond < 4 && leftSecond > 2 && iSpeak > 0)
                    iSpeak = 0;//重置节奏提醒
            }
        }

        private void KillElite(IPlayer player)
        {
            if (Hud.Game.SpecialArea != SpecialArea.GreaterRift || !KillSpeak) return;
            //大秘剩余时间
            //var secondsLeft = (Hud.Game.CurrentTimedEventEndTick - Hud.Game.CurrentTimedEventEndTickMod - (double)Hud.Game.CurrentGameTick) / 60.0d;
            if ((killEliteNum != 0 || isKeep != 0) && Hud.Game.RiftPercentage <= 0)
            {
                killEliteNum = 0;
                isKeep = 0;
            }

            if (HDWay == 1)
            {
                if (leftSecond < 13 && leftSecond > 5)//冰3.0到物理3.0，共8秒
                {
                    //标记时间点：火的时候地上有3个魂淡圈，且巫医的攻速大于2.4；此时记录精英数量、40码内怪物数量、大秘进度
                    if (leftSecond > 8 && isMark == 0 && hdNum == 3)
                    //if (leftSecond > 8 && isMark == 0 && hdNum > 0)
                    {
                        markElite = BaseTool.CountElite(Hud, 100);//标记时间点有几组精英
                        markMonsters = Hud.Game.AliveMonsters.Where(m => m.NormalizedXyDistanceToMe <= 60).Count();//标记时间点指定码数内有多少怪物
                        markPercent = Hud.Game.RiftPercentage;//标记时间点的大秘进度
                        isMark = 1;
                    }

                    if (isMark == 1 && leftSecond <= 8)
                    {
                        int monstersCut = markMonsters - Hud.Game.AliveMonsters.Where(m => m.NormalizedXyDistanceToMe <= 60).Count();
                        //跟标记时间点相比进度变化大于1或者怪物数量少了20个
                        if (Hud.Game.RiftPercentage - markPercent > 1 || monstersCut >= 20)
                        {
                            DoKillElite();
                            isMark = 0;
                        }
                    }
                }
                else
                {
                    if (isMark == 1)
                    {
                        isMark = 0;
                        int monstersCut = markMonsters - Hud.Game.AliveMonsters.Where(m => m.NormalizedXyDistanceToMe <= 60).Count();
                    }
                }
            }
            else
            {
                if (leftSecond < 12 && leftSecond > 4)//火0.0到物理2.0，共6秒
                {
                    //标记时间点：火的时候地上有3个魂淡圈，且巫医的攻速大于2.4；此时记录精英数量、40码内怪物数量、大秘进度
                    if (leftSecond > 10 && isMark == 0 && hdNum == 3)
                    //if (leftSecond > 8 && isMark == 0 && hdNum > 0)
                    {
                        markElite = BaseTool.CountElite(Hud, 100);//标记时间点有几组精英
                        markMonsters = Hud.Game.AliveMonsters.Where(m => m.NormalizedXyDistanceToMe <= 60).Count();//标记时间点指定码数内有多少怪物
                        markPercent = Hud.Game.RiftPercentage;//标记时间点的大秘进度
                        isMark = 1;
                    }

                    if (isMark == 1 && leftSecond <= 5)
                    {
                        int monstersCut = markMonsters - Hud.Game.AliveMonsters.Where(m => m.NormalizedXyDistanceToMe <= 60).Count();
                        //跟标记时间点相比进度变化大于1或者怪物数量少了20个
                        if (Hud.Game.RiftPercentage - markPercent > 1 || monstersCut >= 20)
                        {
                            DoKillElite();
                            isMark = 0;
                        }
                    }
                }
                else
                {
                    if (isMark == 1)
                    {
                        isMark = 0;
                        int monstersCut = markMonsters - Hud.Game.AliveMonsters.Where(m => m.NormalizedXyDistanceToMe <= 60).Count();
                    }
                }
            }
        }

        private void DoKillElite()
        {
            var group = markElite - BaseTool.CountElite(Hud, 100);
            if (group > 0)
            {
                //首杀
                if (killEliteNum == 0)
                {
                    SoundPlayer soundPlayer = BaseTool.EnableKillSpeakWav ? Hud.Sound.LoadSoundPlayer(BaseTool.FirstBloodWav) : null;
                    if (soundPlayer == null)
                    {
                        switch (Hud.CurrentLanguage)
                        {
                            case Language.zhCN:
                                Hud.Sound.Speak(BaseTool.FirstBlood);
                                break;
                            case Language.zhTW:
                                Hud.Sound.Speak(BaseTool.FirstBlood);
                                break;
                            default:
                                Hud.Sound.Speak("first blood");
                                break;
                        }
                    }
                    else
                        BaseTool.PlaySoundWav(soundPlayer);
                }

                if (isKeep == 0)
                {
                    isKeep = 1;
                    killKeep = 0;
                }
                killKeep += group;

                //连杀7个以上算超神
                if (killKeep >= 7)
                {
                    SoundPlayer soundPlayer = BaseTool.EnableKillSpeakWav ? Hud.Sound.LoadSoundPlayer(BaseTool.GodLikeWav) : null;
                    if (soundPlayer == null)
                    {
                        //string[] sArray = player.Hero.BattleTag.Split('#');
                        switch (Hud.CurrentLanguage)
                        {
                            case Language.zhCN:
                                Hud.Sound.Speak(BaseTool.GodLike);
                                break;
                            case Language.zhTW:
                                Hud.Sound.Speak(BaseTool.GodLike);
                                break;
                            default:
                                Hud.Sound.Speak("Legendary");
                                break;
                        }
                        
                    }
                    else
                        BaseTool.PlaySoundWav(soundPlayer);
                }
                else
                {
                    //1-6杀单次（一次6杀也是超神）
                    if (group == 1)
                    {
                        SoundPlayer soundPlayer = BaseTool.EnableKillSpeakWav ? Hud.Sound.LoadSoundPlayer(BaseTool.OneKillWav) : null;
                        if (soundPlayer == null)
                        {
                            switch (Hud.CurrentLanguage)
                            {
                                case Language.zhCN:
                                    Hud.Sound.Speak(BaseTool.OneKill);
                                    break;
                                case Language.zhTW:
                                    Hud.Sound.Speak(BaseTool.OneKill);
                                    break;
                                default:
                                    Hud.Sound.Speak("you have slain an enemy");
                                    break;
                            }
                        }
                            
                        else
                            BaseTool.PlaySoundWav(soundPlayer);
                    }
                    else if (group == 2)
                    {
                        SoundPlayer soundPlayer = BaseTool.EnableKillSpeakWav ? Hud.Sound.LoadSoundPlayer(BaseTool.DoubleKillWav) : null;
                        if (soundPlayer == null)
                        {
                            switch (Hud.CurrentLanguage)
                            {
                                case Language.zhCN:
                                    Hud.Sound.Speak(BaseTool.DoubleKill);
                                    break;
                                case Language.zhTW:
                                    Hud.Sound.Speak(BaseTool.DoubleKill);
                                    break;
                                default:
                                    Hud.Sound.Speak("Double Kill");
                                    break;
                            }
                        }
                        else
                            BaseTool.PlaySoundWav(soundPlayer);
                    }
                    else if (group == 3)
                    {
                        SoundPlayer soundPlayer = BaseTool.EnableKillSpeakWav ? Hud.Sound.LoadSoundPlayer(BaseTool.TripleKillWav) : null;
                        if (soundPlayer == null)
                        {
                            switch (Hud.CurrentLanguage)
                            {
                                case Language.zhCN:
                                    Hud.Sound.Speak(BaseTool.TripleKill);
                                    break;
                                case Language.zhTW:
                                    Hud.Sound.Speak(BaseTool.TripleKill);
                                    break;
                                default:
                                    Hud.Sound.Speak("Triple Kill");
                                    break;
                            }
                        }
                        else
                            BaseTool.PlaySoundWav(soundPlayer);
                    }
                    else if (group == 4)
                    {
                        SoundPlayer soundPlayer = BaseTool.EnableKillSpeakWav ? Hud.Sound.LoadSoundPlayer(BaseTool.QuatreKillWav) : null;
                        if (soundPlayer == null)
                        {
                            switch (Hud.CurrentLanguage)
                            {
                                case Language.zhCN:
                                    Hud.Sound.Speak(BaseTool.QuatreKill);
                                    break;
                                case Language.zhTW:
                                    Hud.Sound.Speak(BaseTool.QuatreKill);
                                    break;
                                default:
                                    Hud.Sound.Speak("Quatre Kill");
                                    break;
                            }
                        } 
                        else
                            BaseTool.PlaySoundWav(soundPlayer);
                    }
                    else if (group == 5)
                    {
                        SoundPlayer soundPlayer = BaseTool.EnableKillSpeakWav ? Hud.Sound.LoadSoundPlayer(BaseTool.PentaKillWav) : null;
                        if (soundPlayer == null)
                        {
                            switch (Hud.CurrentLanguage)
                            {
                                case Language.zhCN:
                                    Hud.Sound.Speak(BaseTool.PentaKill);
                                    break;
                                case Language.zhTW:
                                    Hud.Sound.Speak(BaseTool.PentaKill);
                                    break;
                                default:
                                    Hud.Sound.Speak("Penta Kill");
                                    break;
                            }
                        }
                        else
                            BaseTool.PlaySoundWav(soundPlayer);
                    }
                    else if (group >= 6)
                    {
                        SoundPlayer soundPlayer = BaseTool.EnableKillSpeakWav ? Hud.Sound.LoadSoundPlayer(BaseTool.GodLikeWav) : null;
                        if (soundPlayer == null)
                            switch (Hud.CurrentLanguage)
                            {
                                case Language.zhCN:
                                    Hud.Sound.Speak(BaseTool.GodLike);
                                    break;
                                case Language.zhTW:
                                    Hud.Sound.Speak(BaseTool.GodLike);
                                    break;
                                default:
                                    Hud.Sound.Speak("Legendary");
                                    break;
                            }
                        else
                            BaseTool.PlaySoundWav(soundPlayer);
                    }
                }

                if (killEliteNum >= 0)
                    killEliteNum += group;
            }
            else
            {
                isKeep = 0;
            }
        }
        #endregion

        private void PlayHunDan(IPlayer player)
        {
            leftSecond = BaseTool.GetElementTime(Hud, player, 2);//冰元素剩余秒数

            if (HDWay == 1 && leftSecond < 15 && leftSecond > 10 && isRound_16) isRound_16 = false;//冰到火元素期间重置（16秒模式）
            if (HDWay == 2 && leftSecond > 13) isRound_8 = false;//冰0.0到冰3.0重置循环（8+8模式）
            if (HDWay == 1 && leftSecond > 12 && !isSacrifice) isSacrifice = true;//冰元素期间重置爆狗，想要不爆狗则要在火元素的时候按下Ctrl
            
            hdNum = BaseTool.CountHunDan(Hud);//地上的圈数量，不看剩余时间
            KeepSoulHarvest(player);//收割不占抬手，符合条件就释放
            DoSpiritWalk(player);//保命灵行

            if (!BaseTool.IsMoving(Hud))
            {
                GoToOculus(player);//跑神目

                #region 是否开启宏的各种判定
                isHauntSkill = true;//先允许丢蚀魂，符合其他技能条件则改为不允许

                if (hdNum == 0 && !isNewRound)
                    isNewRound = true;

                var numMonster = CountMonster(player);
                var numAffected = CountMonsterAffected(player);

                //测试模式下，手动设置怪物数量
                if (isTesting)
                {
                    numMonster = 50;
                    numAffected = 10;
                }

                //没带虫群只带蚀魂的情况下
                if (player.Powers.UsedWitchDoctorPowers.Haunt == null)
                {
                    if (numMonster == 0)
                        return;

                    if (numAffected == 0)
                        isNewRound = true;
                }
                else
                {
                    if (numMonster == 0                                       //没怪
                        || (numMonster >= 5 && numAffected < 5)               //超过5个怪，但没有5个以上的蚀魂
                        || (numMonster < 5 && numMonster != numAffected))     //低于5个怪，但没有上满蚀魂 
                    {
                        isNewRound = true;
                        DoHaunt(player, numMonster - numAffected);//没进入节奏前，丢蚀魂
                        return;
                    }
                }
                #endregion

                //BaseTool.DebugInfo($"魂淡:{hdNum},怪物:{numAffected}/{numMonster}", 0);

                Rhythm();//节凑提醒
                KillElite(player);//击杀提醒

                if (HDWay == 1)
                {
                    DoBigBadVoodo(player);//放巫毒
                    DoSummonZombieDog(player);//召狗

                    DoLocustSwarm(player, numMonster, numAffected);//虫群
                    if (numAffected > 0)
                    {
                        DoSacrifice(player);//物理1.5到物理3.5，爆狗在丢魂淡之前
                        DoSpiritBarrage(player);//物理2.0到毒2.5，连续丢魂淡
                    }
                    DoGraspOfTheDead(player);//地上有魂淡的时候丢亡者

                    DoHaunt(player, numMonster - numAffected);//符合条件就放蚀魂
                }
                else
                {
                    DoBigBadVoodo(player);//放巫毒
                    DoLocustSwarm(player, numMonster, numAffected);//虫群)
                    if (numAffected > 0)
                    {
                        DoSpiritBarrage(player);//丢魂淡，火1.0丢2-3个，毒1.0丢3个
                        DoSacrifice(player);//物理3.0爆狗
                    }
                    DoGraspOfTheDead(player);//毒0.0或火0.0

                    DoHaunt(player, numMonster - numAffected);//符合条件就放蚀魂
                }
            }
            else
            {
                DoHaunt(player, 0);//强制移动时，若是按下了蚀魂，则抬起
            }
        }
    }
}
