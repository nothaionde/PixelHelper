using System;
using System.Collections.Generic;
using Turbo.Plugins.Default;

namespace Turbo.Plugins.glq
{
 
    public class GLQ_PartyBuffPlugin1 : BasePlugin, IInGameWorldPainter
    {
        public BuffPainter BuffPainter { get; set; }
        public BuffRuleCalculator RuleCalculatorMe { get; private set;}
        public Dictionary<HeroClass, BuffRuleCalculator> RuleCalculators { get; private set; }
        public WorldDecoratorCollection DebugDecorator { get; set; }
		public bool MePortraitPaint { get; set; }
		public bool MeScreenPaint { get; set; }
		public bool OtherPortraitPaint { get; set; }
		public bool OtherScreenPaint { get; set; }
		public bool Tooltips { get; set; }
        public bool Debug { get; set; }
        private float SizeMultiplier { get; set; }
        public float Opacity { get; set; }
        public float PositionOffset { get; set; }
        public float MePositionOffset { get; set; }
        private BuffRuleFactory1 buffRuleFactory1;
		
        public GLQ_PartyBuffPlugin1()
        {
            Enabled = true;
			MeScreenPaint = true;
			MePortraitPaint = true;
			OtherScreenPaint = true;
			OtherPortraitPaint = true;
			Tooltips = true;
        }


        public override void Load(IController hud)
        {   
            base.Load(hud);
            buffRuleFactory1 = new BuffRuleFactory1(hud);
            SizeMultiplier = 0.85f;
            PositionOffset = 0.085f;
            MePositionOffset = 0.04f;
            Debug = false;
            Opacity = 0.85f;
            RuleCalculatorMe = new BuffRuleCalculator(Hud);
            RuleCalculatorMe.SizeMultiplier = SizeMultiplier;
            RuleCalculators = new Dictionary<HeroClass, BuffRuleCalculator>();
            foreach(HeroClass h in Enum.GetValues(typeof(HeroClass))){
                RuleCalculators.Add(h, new BuffRuleCalculator(Hud));
                RuleCalculators[h].SizeMultiplier = SizeMultiplier;
            }

            BuffPainter = new BuffPainter(Hud, true)
            {
                Opacity = Opacity,
                ShowTimeLeftNumbers = true,
                ShowTooltips = Tooltips,
                TimeLeftFont = Hud.Render.CreateFont("tahoma", 9, 255, 255, 255, 255, false, false, 255, 0, 0, 0, true),
                StackFont = Hud.Render.CreateFont("tahoma", 6, 255, 255, 255, 255, false, false, 255, 0, 0, 0, true),
            };

            DebugDecorator = new WorldDecoratorCollection(
                new GroundLabelDecorator(Hud)
                {
                    BackgroundBrush = Hud.Render.CreateBrush(100, 20, 20, 20, 0),
                    TextFont = Hud.Render.CreateFont("tahoma", 6.5f, 255, 255, 255, 255, false, false, false),
                }
            );
		}

        public void PaintWorld(WorldLayer layer)
        {
            if ((Hud.Game.MapMode == MapMode.WaypointMap) || (Hud.Game.MapMode == MapMode.ActMap) || (Hud.Game.MapMode == MapMode.Map)) return;
            var players = Hud.Game.Players;
            foreach(var p in players){
				var portraitRect = p.PortraitUiElement.Rectangle;
                if (p.IsMe){
                    RuleCalculatorMe.CalculatePaintInfo(p);
                    if (RuleCalculatorMe.PaintInfoList.Count != 0)
						if (MeScreenPaint)
						{
						    BuffPainter.PaintHorizontalCenter(
                            RuleCalculatorMe.PaintInfoList,
                            0, 
                            Hud.Window.Size.Height * 0.5f + Hud.Window.Size.Height * MePositionOffset,
                            Hud.Window.Size.Width,
                            RuleCalculatorMe.StandardIconSize,
                            RuleCalculatorMe.StandardIconSpacing
                        );
						}
						if (MePortraitPaint)
						{
                            BuffPainter.PaintHorizontal(
                            RuleCalculatorMe.PaintInfoList,
                            portraitRect.Right, 
                            portraitRect.Top + portraitRect.Height * 0.72f, 
                            RuleCalculatorMe.StandardIconSize,
                            RuleCalculatorMe.StandardIconSpacing
                            );
						}
                }else{
                    if (p.IsOnScreen && p.CoordinateKnown){
                        var ScreenCoordinate = p.FloorCoordinate.ToScreenCoordinate();
                        var PlayerX = ScreenCoordinate.X;
                        var PlayerY = ScreenCoordinate.Y;
                        RuleCalculators[p.HeroClassDefinition.HeroClass].CalculatePaintInfo(p);
                        if (RuleCalculators[p.HeroClassDefinition.HeroClass].PaintInfoList.Count != 0){
							if(OtherScreenPaint)
							{
                            BuffPainter.PaintHorizontalCenter(
                                RuleCalculators[p.HeroClassDefinition.HeroClass].PaintInfoList,
                                PlayerX,
                                PlayerY - Hud.Window.Size.Height * 0.05f + Hud.Window.Size.Height * PositionOffset, 
                                0, 
                                RuleCalculators[p.HeroClassDefinition.HeroClass].StandardIconSize, 
                                RuleCalculators[p.HeroClassDefinition.HeroClass].StandardIconSpacing
								);
							}
							if(OtherPortraitPaint)
							{
                            BuffPainter.PaintHorizontal(
                                RuleCalculators[p.HeroClassDefinition.HeroClass].PaintInfoList, 
                                portraitRect.Right, 
                                portraitRect.Top + portraitRect.Height * 0.72f, 
                                RuleCalculators[p.HeroClassDefinition.HeroClass].StandardIconSize, 
                                RuleCalculators[p.HeroClassDefinition.HeroClass].StandardIconSpacing
                            );
							}
                        }
                    }
                }

                if (Debug)
                    DebugPrint(layer, p);
            }
		}

        private void DebugPrint(WorldLayer layer, IPlayer p){
            string data = "";
            foreach(IBuff b in p.Powers.AllBuffs)
                if (BuffRuleExsitsForPlayer(p, b))
                    data += DataOnBuff(b) + "\n";
            DebugDecorator.Paint(layer, p, p.FloorCoordinate, data);
        }

        private bool BuffRuleExsitsForPlayer(IPlayer p, IBuff b){
            BuffRuleCalculator r = (p.IsMe) ? RuleCalculatorMe : RuleCalculators[p.HeroClassDefinition.HeroClass];
            foreach(BuffRule t in r.Rules)
                if (t.PowerSno == b.SnoPower.Sno)
                    return true;
            return false;
        }

        private string DataOnBuff(IBuff b){
            string res = "";
            res += b.SnoPower.Sno.ToString() + " \t";
            res += "["+string.Join(",", b.IconCounts)+"]" + "\t";
            res += b.Active.ToString() + " \t";
            res += b.SnoPower.NameLocalized + " \t";
            res += b.SnoPower.Code + " \t";
            //res += t.SnoPower.DescriptionEnglish;   
            return res;
        }

        public void DisplayOnAll(params uint[] pwrs){
            foreach(HeroClass h in Enum.GetValues(typeof(HeroClass)))
                AddPower(RuleCalculators[h], pwrs);
            AddPower(RuleCalculatorMe, pwrs);       
        }

        public void DisplayOnMe(params uint[] pwrs){
            AddPower(RuleCalculatorMe, pwrs);
        }

        public void DisplayOnAllClassesExceptMe(params uint[] pwrs){
            foreach(HeroClass h in Enum.GetValues(typeof(HeroClass)))
                AddPower(RuleCalculators[h], pwrs);       
        }

        public void DisplayOnClassExceptMe(HeroClass h, params uint[] pwrs){
            AddPower(RuleCalculators[h], pwrs);
        }

        private void AddPower(BuffRuleCalculator bf, params uint[] pwrs){
            if (pwrs == null) return;
            foreach(uint p in pwrs)
                AddPower(bf, p);
        }

        private void AddPower(BuffRuleCalculator bf, uint pwr){
            var buffRules = buffRuleFactory1.CreateBuffRules(pwr);       //ty jack!
            if (buffRules != null){
                bf.Rules.AddRange(buffRules);
            } 
        }
    }
}