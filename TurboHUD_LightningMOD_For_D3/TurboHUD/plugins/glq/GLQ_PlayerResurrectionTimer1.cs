using Turbo.Plugins.Default;
using System.Linq;
namespace Turbo.Plugins.glqz
{
    public class GLQ_PlayerResurrectionTimer1 : BasePlugin, IInGameTopPainter, INewAreaHandler
    {
        private bool debug;
        private bool FalseDead1;
        private bool FalseDead2;
        private bool FalseDead3;
        private bool FalseDead4;
        private bool _FalseDead1;
        private bool _FalseDead2;
        private bool _FalseDead3;
        private bool _FalseDead4;
        private int FalseDeadTime1;
        private int FalseDeadTime2;
        private int FalseDeadTime3;
        private int FalseDeadTime4;
        private bool Dead1;
        private bool Dead2;
        private bool Dead3;
        private bool Dead4;
        private bool DebugDead1;
        private bool DebugDead2;
        private bool DebugDead3;
        private bool DebugDead4;
        private int DeadTimes1;
        private int DeadTimes2;
        private int DeadTimes3;
        private int DeadTimes4;
        private int DebugDeadTimes1;
        private int DebugDeadTimes2;
        private int DebugDeadTimes3;
        private int DebugDeadTimes4;
        private double DeadTime1;
        private double DeadTime2;
        private double DeadTime3;
        private double DeadTime4;
        private double CurrentTime1;
        private double CurrentTime2;
        private double CurrentTime3;
        private double CurrentTime4;
        private double GhostTime1;
        private double GhostTime2;
        private double GhostTime3;
        private double GhostTime4;
        private double CurrentGhostTime1;
        private double CurrentGhostTime2;
        private double CurrentGhostTime3;
        private double CurrentGhostTime4;
        public string DeathSymbol { get; set; }
        public IFont TextFontDeath { get; set; }
        public IFont TextFontDeathTime { get; set; }
        public IFont TextFontGhostTime { get; set; }
        public IFont debugTextFont1 { get; set; }
        public IFont debugTextFont2 { get; set; }
        public IBrush DeadTimeBorderBrush { get; set; }
        public IBrush DeadBorderBrush { get; set; }
        public IBrush GhostBorderBrush { get; set; }
        public double delay { get; set; }
        public GLQ_PlayerResurrectionTimer1()
        {
            Enabled = true;
            debug = false;
            delay = 3d;
        }
        public override void Load(IController hud)
        {
            base.Load(hud);
            Dead1 = false;
            Dead2 = false;
            Dead3 = false;
            Dead4 = false;
            DeathSymbol = " ✟"; //✟  ̸���
            debugTextFont1 = Hud.Render.CreateFont("tahoma", 12, 200, 255, 255, 255, true, false, true);
            debugTextFont2 = Hud.Render.CreateFont("tahoma", 12, 200, 255, 0, 0, true, false, true);
            TextFontDeath = Hud.Render.CreateFont("tahoma", 35, 200, 255, 255, 255, true, false, true);
            TextFontDeathTime = Hud.Render.CreateFont("tahoma", 12, 200, 255, 0, 0, true, false, true);
            TextFontGhostTime = Hud.Render.CreateFont("tahoma", 12, 200, 128, 255, 255, true, false, true);
            DeadTimeBorderBrush = Hud.Render.CreateBrush(200, 255, 0, 0, -2);
            DeadBorderBrush = Hud.Render.CreateBrush(200, 255, 255, 255, -2);
            GhostBorderBrush = Hud.Render.CreateBrush(200, 128, 255, 255, -2);
			
        }
        public void OnNewArea(bool newGame, ISnoArea area)
        {
            if (newGame)
            {
                DeadTimes1 = 0;
                DeadTimes2 = 0;
                DeadTimes3 = 0;
                DeadTimes4 = 0;
                GhostTime1 = 0;
                GhostTime2 = 0;
                GhostTime3 = 0;
                GhostTime4 = 0;
                DebugDeadTimes1 = 0;
                DebugDeadTimes2 = 0;
                DebugDeadTimes3 = 0;
                DebugDeadTimes4 = 0;
            }
        }
        private IQuest riftQuest
        {
            get
            {
                return Hud.Game.Quests.FirstOrDefault(q => q.SnoQuest.Sno == 337492) ?? // rift
                       Hud.Game.Quests.FirstOrDefault(q => q.SnoQuest.Sno == 382695);   // gr
            }
        }
        public void PaintTopInGame(ClipState clipState)
        {
            if (clipState != ClipState.BeforeClip) return;
            //bool inGR = Hud.Game.SpecialArea == SpecialArea.GreaterRift;
            var layout = TextFontDeath.GetTextLayout(DeathSymbol);
            var layoutdebug = TextFontDeath.GetTextLayout(DeathSymbol);
            var secondsLeft = (Hud.Game.CurrentTimedEventEndTick - (double)Hud.Game.CurrentGameTick) / 60.0d;
            if (riftQuest == null || (riftQuest != null && riftQuest.State == QuestState.none))
            {
                DeadTimes1 = 0;
                DeadTimes2 = 0;
                DeadTimes3 = 0;
                DeadTimes4 = 0;
                GhostTime1 = 0;
                GhostTime2 = 0;
                GhostTime3 = 0;
                GhostTime4 = 0;
                DebugDeadTimes1 = 0;
                DebugDeadTimes2 = 0;
                DebugDeadTimes3 = 0;
                DebugDeadTimes4 = 0;
            }
            foreach (var player in Hud.Game.Players)
            {
                int Index = player.Index;
                var portrait = player.PortraitUiElement.Rectangle;
                var x = portrait.Left + portrait.Width * 1.1f;
                var y = portrait.Top + portrait.Height / 4;
                switch (Index)
                {
                    case 0:
                        if (player.IsDead)
                        {
                            if (DebugDead1 == false)
                            {
                                DebugDeadTimes1++;
                                DebugDead1 = true;
                            }
                            if (debug)
                            {
                                layoutdebug = debugTextFont2.GetTextLayout("Index:0 Death" + DebugDeadTimes1 + "times");
                                debugTextFont2.DrawText(layoutdebug, x * 1.2f, y-20);
                            }
                            if (_FalseDead1 == false) FalseDeadTime1 = Hud.Game.CurrentGameTick + (int)(delay * 60);
                            if (player.IsDead && Hud.Game.CurrentGameTick > FalseDeadTime1)
                            {
                                FalseDead1 = true;
                            }
                            else
                            {
                                _FalseDead1 = true;
                            }

                        }
                        else
                        {
                            DebugDead1 = false;
                            if (debug)
                            {
                                layoutdebug = debugTextFont1.GetTextLayout("Index:0Alive,Death" + DebugDeadTimes1 + "times");
                                debugTextFont1.DrawText(layoutdebug, x * 1.2f, y-20);
                            }
                            _FalseDead1 = false;
                            FalseDead1 = false;
                        }
                        if (FalseDead1)
                        {
                            if (debug)
                            {
                                layoutdebug = debugTextFont2.GetTextLayout("Index:0 Death" + DeadTimes1 + "times");
                                debugTextFont2.DrawText(layoutdebug, x * 1.2f, y);
                            }
                            if (Dead1 == false)
                            {
                                DeadTime1 = Hud.Game.CurrentGameTick;
                                if (player.InGreaterRift) DeadTimes1++;
                            }
                            Dead1 = true;
                            CurrentTime1 = GetDeadCountdown(DeadTimes1, secondsLeft, player.InGreaterRift) - (Hud.Game.CurrentGameTick - DeadTime1) / 60.0d;
                            if (CurrentTime1 < 0.5)
                            {
                                y = portrait.Top + portrait.Height / 4;
                                layout = TextFontDeath.GetTextLayout(DeathSymbol);
                                DeadBorderBrush.DrawRectangle(portrait.Left, portrait.Top, portrait.Width, portrait.Height);
                                TextFontDeath.DrawText(layout, x, y);
                                CurrentTime1 = 0;
                            }
                            else
                            {
                                y = portrait.Top + portrait.Height / 3.5f;
                                layout = TextFontDeathTime.GetTextLayout("Мертв: " + CurrentTime1.ToString("f0") + "с");
                                DeadTimeBorderBrush.DrawRectangle(portrait.Left, portrait.Top, portrait.Width, portrait.Height);
                                TextFontDeathTime.DrawText(layout, x, y);
                            }
                        }
                        else
                        {
                            if (debug)
                            {
                                layoutdebug = debugTextFont1.GetTextLayout("Index:0Alive,Death" + DeadTimes1 + "times");
                                debugTextFont1.DrawText(layoutdebug, x * 1.2f, y);
                            }

                            if (Dead1)
                            {
                                GhostTime1 = Hud.Game.CurrentGameTick + 3 * 60;
                            }
                            Dead1 = false;
                            CurrentGhostTime1 = (GhostTime1 - Hud.Game.CurrentGameTick) / 60.0d;
                            if (CurrentGhostTime1 > 0)
                            {
                                y = portrait.Top + portrait.Height / 3.5f;
                                layout = TextFontGhostTime.GetTextLayout("Призрак: " + CurrentGhostTime1.ToString("f0") + "с");
                                if (CurrentGhostTime1 < 1) layout = TextFontGhostTime.GetTextLayout("Призрак: " + CurrentGhostTime1.ToString("f1") + "с");
                                TextFontGhostTime.DrawText(layout, x, y);
                                GhostBorderBrush.DrawRectangle(portrait.Left, portrait.Top, portrait.Width, portrait.Height);
                            }
                            else
                            {
                                CurrentGhostTime1 = 0;
                            }
                        }
                        break;
                    case 1:
                        if (player.IsDead)
                        {
                            if(DebugDead2==false)
                            {
                                DebugDeadTimes2++;
                                DebugDead2 = true;
                            }
                            
                            if (debug)
                            {
                                layoutdebug = debugTextFont2.GetTextLayout("Index:1 Death" + DebugDeadTimes2 + "times");
                                debugTextFont2.DrawText(layoutdebug, x * 1.2f, y-20);
                            }
                            if (_FalseDead2 == false) FalseDeadTime2 = Hud.Game.CurrentGameTick + (int)(delay * 60);
                            if (player.IsDead && Hud.Game.CurrentGameTick > FalseDeadTime2)
                            {
                                FalseDead2 = true;
                            }
                            else
                            {
                                _FalseDead2 = true;
                            }

                        }
                        else
                        {
                            DebugDead2 = false;
                            if (debug)
                            {
                                layoutdebug = debugTextFont1.GetTextLayout("Index:1Alive,Death" + DebugDeadTimes2 + "times");
                                debugTextFont1.DrawText(layoutdebug, x * 1.2f, y-20);
                            }
                            _FalseDead2 = false;
                            FalseDead2 = false;
                        }
                        if (FalseDead2)
                        {
                            if (debug)
                            {
                                layoutdebug = debugTextFont2.GetTextLayout("Index:1 Death" + DeadTimes2 + "times");
                                debugTextFont2.DrawText(layoutdebug, x * 1.2f, y);
                            }

                            if (Dead2 == false)
                            {
                                DeadTime2 = Hud.Game.CurrentGameTick;
                                if (player.InGreaterRift) DeadTimes2++;
                            }
                            Dead2 = true;
                            CurrentTime2 = GetDeadCountdown(DeadTimes2, secondsLeft, player.InGreaterRift) - (Hud.Game.CurrentGameTick - DeadTime2) / 60.0d;
                            if (CurrentTime2 < 0.5)
                            {
                                y = portrait.Top + portrait.Height / 4;
                                layout = TextFontDeath.GetTextLayout(DeathSymbol);
                                DeadBorderBrush.DrawRectangle(portrait.Left, portrait.Top, portrait.Width, portrait.Height);
                                TextFontDeath.DrawText(layout, x, y);
                                CurrentTime2 = 0;
                            }
                            else
                            {
                                y = portrait.Top + portrait.Height / 3.5f;
                                layout = TextFontDeathTime.GetTextLayout("Мертв: " + CurrentTime2.ToString("f0") + "с");
                                DeadTimeBorderBrush.DrawRectangle(portrait.Left, portrait.Top, portrait.Width, portrait.Height);
                                TextFontDeathTime.DrawText(layout, x, y);
                            }
                        }
                        else
                        {
                            if (debug)
                            {
                                layoutdebug = debugTextFont1.GetTextLayout("Index:1Alive,Death" + DeadTimes2 + "times");
                                debugTextFont1.DrawText(layoutdebug, x * 1.2f, y);
                            }

                            if (Dead2)
                            {
                                GhostTime2 = Hud.Game.CurrentGameTick + 3 * 60;
                            }
                            Dead2 = false;
                            CurrentGhostTime2 = (GhostTime2 - Hud.Game.CurrentGameTick) / 60.0d;
                            if (CurrentGhostTime2 > 0)
                            {
                                y = portrait.Top + portrait.Height / 3.5f;
                                layout = TextFontGhostTime.GetTextLayout("Призрак: " + CurrentGhostTime2.ToString("f0") + "с");
                                if (CurrentGhostTime2 < 1) layout = TextFontGhostTime.GetTextLayout("Призрак: " + CurrentGhostTime2.ToString("f1") + "с");
                                TextFontGhostTime.DrawText(layout, x, y);
                                GhostBorderBrush.DrawRectangle(portrait.Left, portrait.Top, portrait.Width, portrait.Height);
                            }
                            else
                            {
                                CurrentGhostTime2 = 0;
                            }
                        }
                        break;
                    case 2:
                        if (player.IsDead)
                        {
                            if (DebugDead3 == false)
                            {
                                DebugDeadTimes3++;
                                DebugDead3 = true;
                            }
                            if (debug)
                            {
                                layoutdebug = debugTextFont2.GetTextLayout("Index:2 Death" + DebugDeadTimes3 + "times");
                                debugTextFont2.DrawText(layoutdebug, x * 1.2f, y-20);
                            }
                            if (_FalseDead3 == false) FalseDeadTime3 = Hud.Game.CurrentGameTick + (int)(delay * 60);
                            if (player.IsDead && Hud.Game.CurrentGameTick > FalseDeadTime3)
                            {
                                FalseDead3 = true;
                            }
                            else
                            {
                                _FalseDead3 = true;
                            }

                        }
                        else
                        {
                            DebugDead3 = false;
                            if (debug)
                            {
                                layoutdebug = debugTextFont1.GetTextLayout("Index:2Alive,Death" + DebugDeadTimes3 + "times");
                                debugTextFont1.DrawText(layoutdebug, x * 1.2f, y-20);
                            }
                            _FalseDead3 = false;
                            FalseDead3 = false;
                        }
                        if (FalseDead3)
                        {
                            if (debug)
                            {
                                layoutdebug = debugTextFont2.GetTextLayout("Index:2 Death" + DeadTimes3 + "times");
                                debugTextFont2.DrawText(layoutdebug, x * 1.2f, y);
                            }

                            if (Dead3 == false)
                            {
                                DeadTime3 = Hud.Game.CurrentGameTick;
                                if (player.InGreaterRift) DeadTimes3++;
                            }
                            Dead3 = true;
                            CurrentTime3 = GetDeadCountdown(DeadTimes3, secondsLeft, player.InGreaterRift) - (Hud.Game.CurrentGameTick - DeadTime3) / 60.0d;
                            if (CurrentTime3 < 0.5)
                            {
                                y = portrait.Top + portrait.Height / 4;
                                layout = TextFontDeath.GetTextLayout(DeathSymbol);
                                DeadBorderBrush.DrawRectangle(portrait.Left, portrait.Top, portrait.Width, portrait.Height);
                                TextFontDeath.DrawText(layout, x, y);
                                CurrentTime3 = 0;
                            }
                            else
                            {
                                y = portrait.Top + portrait.Height / 3.5f;
                                layout = TextFontDeathTime.GetTextLayout("Мертв: " + CurrentTime3.ToString("f0") + "с");
                                DeadTimeBorderBrush.DrawRectangle(portrait.Left, portrait.Top, portrait.Width, portrait.Height);
                                TextFontDeathTime.DrawText(layout, x, y);
                            }
                        }
                        else
                        {
                            if (debug)
                            {
                                layoutdebug = debugTextFont1.GetTextLayout("Index:2Alive,Death" + DeadTimes3 + "times");
                                debugTextFont1.DrawText(layoutdebug, x * 1.2f, y);
                            }

                            if (Dead3)
                            {
                                GhostTime3 = Hud.Game.CurrentGameTick + 3 * 60;
                            }
                            Dead3 = false;
                            CurrentGhostTime3 = (GhostTime3 - Hud.Game.CurrentGameTick) / 60.0d;
                            if (CurrentGhostTime3 > 0)
                            {
                                y = portrait.Top + portrait.Height / 3.5f;
                                layout = TextFontGhostTime.GetTextLayout("Призрак: " + CurrentGhostTime3.ToString("f0") + "с");
                                if (CurrentGhostTime3 < 1) layout = TextFontGhostTime.GetTextLayout("Призрак: " + CurrentGhostTime3.ToString("f1") + "с");
                                TextFontGhostTime.DrawText(layout, x, y);
                                GhostBorderBrush.DrawRectangle(portrait.Left, portrait.Top, portrait.Width, portrait.Height);
                            }
                            else
                            {
                                CurrentGhostTime3 = 0;
                            }
                        }
                        break;
                    case 3:
                        if (player.IsDead)
                        {
                            if (DebugDead4 == false)
                            {
                                DebugDeadTimes4++;
                                DebugDead4 = true;
                            }
                            if (debug)
                            {
                                layoutdebug = debugTextFont2.GetTextLayout("Index:3 Death" + DebugDeadTimes4 + "times");
                                debugTextFont2.DrawText(layoutdebug, x * 1.2f, y-20);
                            }
                            if (_FalseDead4 == false) FalseDeadTime4 = Hud.Game.CurrentGameTick + (int)(delay * 60);
                            if (player.IsDead && Hud.Game.CurrentGameTick > FalseDeadTime4)
                            {
                                FalseDead4 = true;
                            }
                            else
                            {
                                _FalseDead4 = true;
                            }

                        }
                        else
                        {
                            DebugDead4 = false;
                            if (debug)
                            {
                                layoutdebug = debugTextFont1.GetTextLayout("Index:3Alive,Death" + DebugDeadTimes4 + "times");
                                debugTextFont1.DrawText(layoutdebug, x * 1.2f, y-20);
                            }
                            _FalseDead4 = false;
                            FalseDead4 = false;
                        }
                        if (FalseDead4)
                        {
                            if (debug)
                            {
                                layoutdebug = debugTextFont2.GetTextLayout("Index:3 Death" + DeadTimes4 + "times");
                                debugTextFont2.DrawText(layoutdebug, x * 1.2f, y);
                            }

                            if (Dead4 == false)
                            {
                                DeadTime4 = Hud.Game.CurrentGameTick;
                                if (player.InGreaterRift) DeadTimes4++;
                            }
                            Dead4 = true;
                            CurrentTime4 = GetDeadCountdown(DeadTimes4, secondsLeft, player.InGreaterRift) - (Hud.Game.CurrentGameTick - DeadTime4) / 60.0d;
                            if (CurrentTime4 < 0.5)
                            {
                                y = portrait.Top + portrait.Height / 4;
                                layout = TextFontDeath.GetTextLayout(DeathSymbol);
                                DeadBorderBrush.DrawRectangle(portrait.Left, portrait.Top, portrait.Width, portrait.Height);
                                TextFontDeath.DrawText(layout, x, y);
                                CurrentTime4 = 0;
                            }
                            else
                            {
                                y = portrait.Top + portrait.Height / 3.5f;
                                layout = TextFontDeathTime.GetTextLayout("Мертв: " + CurrentTime4.ToString("f0") + "с");
                                DeadTimeBorderBrush.DrawRectangle(portrait.Left, portrait.Top, portrait.Width, portrait.Height);
                                TextFontDeathTime.DrawText(layout, x, y);
                            }
                        }
                        else
                        {
                            if (debug)
                            {
                                layoutdebug = debugTextFont1.GetTextLayout("Index:3Alive,Death" + DeadTimes4 + "times");
                                debugTextFont1.DrawText(layoutdebug, x * 1.2f, y);
                            }

                            if (Dead4)
                            {
                                GhostTime4 = Hud.Game.CurrentGameTick + 3 * 60;
                            }
                            Dead4 = false;
                            CurrentGhostTime4 = (GhostTime4 - Hud.Game.CurrentGameTick) / 60.0d;
                            if (CurrentGhostTime4 > 0)
                            {
                                y = portrait.Top + portrait.Height / 3.5f;
                                layout = TextFontGhostTime.GetTextLayout("Призрак: " + CurrentGhostTime4.ToString("f0") + "с");
                                if (CurrentGhostTime4 < 1) layout = TextFontGhostTime.GetTextLayout("Призрак: " + CurrentGhostTime4.ToString("f1") + "с");
                                TextFontGhostTime.DrawText(layout, x, y);
                                GhostBorderBrush.DrawRectangle(portrait.Left, portrait.Top, portrait.Width, portrait.Height);
                            }
                            else
                            {
                                CurrentGhostTime4 = 0;
                            }
                        }
                        break;
                }
            }
        }
        private double GetDeadCountdown(int DeadTimes,double secondsLeft ,bool inGR)
        {
            double Times = 0;
            if(!inGR) return 0 + 3.5 - delay;
            if (DeadTimes == 1) Times = 0 + 3.5 - delay;
            if (DeadTimes == 2) Times = 5 + 3.5 - delay;
            if (DeadTimes == 3) Times = 10 + 3.5 - delay;
            if (DeadTimes == 4) Times = 15 + 3.5 - delay;
            if (DeadTimes == 5) Times = 20 + 3.5 - delay;
            if (DeadTimes == 6) Times = 25 + 3.5 - delay;
            if (DeadTimes >= 7) Times = 30 + 3.5 - delay;
            if(secondsLeft<=0)
            {
                Times = 0 + 3.5 - delay;
            }
            return Times;
        }
    }
}