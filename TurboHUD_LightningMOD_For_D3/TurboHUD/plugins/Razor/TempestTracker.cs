using System;
using System.Collections.Generic;
using System.Linq;
using SharpDX.DirectWrite;

using Turbo.Plugins.Default;
using Turbo.Plugins.Razor.Movable;
 
namespace Turbo.Plugins.Razor
{
    //using System.Text;
 
    public class TempestTracker : BasePlugin, IMovable, IInGameTopPainter//, INewAreaHandler //, IInGameWorldPainter
    {
		public bool ShowDecorators { get; set; } = true;
		public bool ShowFlameWaveDecorator { get; set; } = true;
		public bool ShowMeteorDecorator { get; set; } = true;
		public bool ShowSnowballDecorator { get; set; } = true;
		public bool ShowTwisterDecorator { get; set; } = true;
		
		public float BarHeight { get; set; } = 4f; //ratio of countdown timer bar height
		public float ActiveBarHeightMultiplier { get; set; } = 2f;

        public float StartingPositionX { get; set; } //optional, default is to the left of the minimap
        public float StartingPositionY { get; set; } //optional, default is to the left of the minimap
		
		public Dictionary<int, string> Tiers { get; set; }
		public Dictionary<int, IFont> ElementColors { get; set; }
		public Dictionary<int, IBrush> ElementBrushes { get; set; }

        public IFont TextFont { get; set; }
		public IFont EffectFont { get; set; }
		public IFont TimerFont { get; set; }
		public IFont TimerOtherFont { get; set; }
		public IFont OtherFont { get; set; }
		public IBrush TimerHigh { get; set; }
		public IBrush TimerLow { get; set; }
		public IBrush TimerBg { get; set; }
		public IBrush SkillBorderLight { get; set; }
		public IBrush SkillBorderDark { get; set; }
		public IBrush BackgroundBrush { get; set; }
		
		//public WorldDecoratorCollection MeteorImpendingDecorator { get; set; }
		
		private float height_predict;
		private float width_countdown_bar;
		private float height_countdown_bar;
		private Dictionary<uint, int> CycleStartTick = new Dictionary<uint, int>(); //HeroId, cycle start tick //new int[4];
		private int LastSeenPlayerCount = 1;
		//private IScreenCoordinate LastWaveCoordinate;
		private Dictionary<uint, IWorldCoordinate> LastWaveCoordinate = new Dictionary<uint, IWorldCoordinate>();
		
		private bool Debug = false;
 
        public TempestTracker()
        {
            Enabled = false;
        }
 
        public override void Load(IController hud)
        {
            base.Load(hud);
            TextFont = Hud.Render.CreateFont("tahoma", 20f, 255, 200, 200, 200, false, false, 175, 0, 0, 0, true); //180, 147, 109
			TimerFont = Hud.Render.CreateFont("tahoma", 8f, 255, 200, 200, 200, false, false, 175, 0, 0, 0, true);
			TimerOtherFont = Hud.Render.CreateFont("tahoma", 6.5f, 255, 200, 200, 200, false, false, 175, 0, 0, 0, true);
			OtherFont = Hud.Render.CreateFont("tahoma", 8f, 255, 180, 147, 109, false, false, true);

            if (Hud.CurrentLanguage == Language.zhCN)
            {
                Tiers = new Dictionary<int, string>() {
                { 2, "火墙" }, //fire
				{ 3, "雪球" }, //cold
				{ 4, "陨石" }, //poison
				{ 5, "龙卷风" }, //physical
				{ 6, "闪电吐息" }, //lightning
			};
            }
            else if (Hud.CurrentLanguage == Language.zhTW)
            {
                Tiers = new Dictionary<int, string>() {
                { 2, "火墻" }, //fire
				{ 3, "雪球" }, //cold
				{ 4, "隕石" }, //poison
				{ 5, "龍捲風" }, //physical
				{ 6, "閃電吐息" }, //lightning
			};
            }
            else
            {
                Tiers = new Dictionary<int, string>() {
                { 2, "Flame Wave" }, //fire
				{ 3, "Snowball" }, //cold
				{ 4, "Meteors" }, //poison
				{ 5, "Twisters" }, //physical
				{ 6, "Breath" }, //lightning
			};
            }


            
			
			EffectFont = Hud.Render.CreateFont("tahoma", 11f, 255, 255, 0, 0, false, false, 175, 0, 0, 0, true);
			ElementColors = new Dictionary<int, IFont>() {
				{ 2, Hud.Render.CreateFont("tahoma", 11f, 255, 255, 90, 8, false, false, 175, 0, 0, 0, true) }, //fire
				{ 3, Hud.Render.CreateFont("tahoma", 11f, 255, 150, 199, 246, false, false, 175, 0, 0, 0, true) }, //cold
				{ 4, Hud.Render.CreateFont("tahoma", 11f, 255, 64, 225, 16, false, false, 175, 0, 0, 0, true) }, //poison
				{ 5, Hud.Render.CreateFont("tahoma", 11f, 255, 155, 146, 113, false, false, 175, 0, 0, 0, true) }, //physical //155, 146, 113
				{ 6, Hud.Render.CreateFont("tahoma", 11f, 255, 0, 128, 255, false, false, 175, 0, 0, 0, true) }, //lightning
			};
			
			ElementBrushes = new Dictionary<int, IBrush>() {
				{ 2, Hud.Render.CreateBrush(200, 255, 90, 8, 4) },
				{ 3, Hud.Render.CreateBrush(200, 150, 199, 246, 4) }, //Hud.Render.CreateFont("tahoma", 11f, 255, 150, 199, 246, false, false, 175, 0, 0, 0, true) }, //cold
				{ 4, Hud.Render.CreateBrush(200, 64, 225, 16, 4) }, //Hud.Render.CreateFont("tahoma", 11f, 255, 64, 225, 16, false, false, 175, 0, 0, 0, true) }, //poison
				{ 5, Hud.Render.CreateBrush(200, 155, 146, 113, 4) }, //Hud.Render.CreateFont("tahoma", 11f, 255, 200, 200, 200, false, false, 175, 0, 0, 0, true) }, //physical //155, 146, 113
				{ 6, Hud.Render.CreateBrush(200, 0, 128, 255, 4) }, //Hud.Render.CreateFont("tahoma", 11f, 255, 81, 40, 255, false, false, 175, 0, 0, 0, true) }, //lightning
			};
			
			//TimerFont = Hud.Render.CreateFont("tahoma", 7, 200, 255, 255, 255, false, false, 150, 0, 0, 0, true);

			TimerHigh = Hud.Render.CreateBrush(255, 0, 255, 100, 0);
			TimerLow = Hud.Render.CreateBrush(255, 255, 0, 0, 0);
			TimerBg = Hud.Render.CreateBrush(75, 0, 0, 0, 0);
			
			SkillBorderLight = Hud.Render.CreateBrush(200, 95, 95, 95, 1); //235, 227, 164 //138, 135, 109
			SkillBorderDark = Hud.Render.CreateBrush(150, 0, 0, 0, 1);
			
			BackgroundBrush = Hud.Render.CreateBrush(255, 0, 0, 0, 0);
        }
 
		public void OnRegister(MovableController mover)
		{
			//initialize position and dimension elements
			IUiElement ui = Hud.Render.GetUiElement("Root.NormalLayer.eventtext_bkgrnd.eventtext_region.title");
	
			//calculate dimensions
			string maxLengthTier = Tiers.Aggregate((a, b) => a.Value.Length > b.Value.Length ? a : b).Value;
			TextLayout layout = EffectFont.GetTextLayout(maxLengthTier);
			width_countdown_bar = layout.Metrics.Width*1.1f; //Hud.Window.Size.Width * 0.00155f * BarWidth; //layout.Metrics.Width*1.1f;
			height_countdown_bar = Hud.Window.Size.Height * 0.001667f * BarHeight;
			float width = layout.Metrics.Width*1.1f;

			float height = height_countdown_bar*2;
			height += layout.Metrics.Height;
			layout = TextFont.GetTextLayout(maxLengthTier);
			height += layout.Metrics.Height + 2;

			//calculate starting position
			if (StartingPositionX == 0 && StartingPositionY == 0) { //default display position, snap to the left of the minimap
				var uiMinimapRect = Hud.Render.MinimapUiElement.Rectangle;
				StartingPositionX = uiMinimapRect.Left - width_countdown_bar - 15;
				StartingPositionY = uiMinimapRect.Top + (uiMinimapRect.Height - 150) / 2f;
			}

			//MovableAreas.Add(new MovableArea("Countdown") { Enabled = ShowTracker, Rectangle = new RectangleF(ui.Rectangle.X - ProcRuleCalculator.StandardIconSize - Gap, ui.Rectangle.Y, ProcRuleCalculator.StandardIconSize + Gap + Hud.Window.Size.Width * 0.00155f * BarWidth + 12, ProcRuleCalculator.StandardIconSize*4 + Gap*3) });
			//MovableController.CreateArea(IMovable owner, string areaName, RectangleF rect, bool enabledAtStart, bool saveToConfig, bool resizable = false, ClipState clipState = ClipState.BeforeClip)
			mover.CreateArea(
				this,
				"Self", //area name
				new System.Drawing.RectangleF(StartingPositionX, StartingPositionY, width_countdown_bar, height), //position + dimensions
				true, //enabled at start?
				true, //save to config file?
				ResizeMode.Horizontal //resizable?				
			);
			
			mover.CreateArea(
				this,
				"Party", //area name
				new System.Drawing.RectangleF(StartingPositionX, StartingPositionY + height*1.1f, width_countdown_bar, height*1.25f), //position + dimensions
				true, //enabled at start?
				true, //save to config file?
				ResizeMode.Horizontal //resizable
			);
		}

        //public void PaintTopInGame(ClipState clipState)
		public void PaintArea(MovableController mover, MovableArea area, float deltaX = 0, float deltaY = 0)
        {
			if (!Hud.Game.Me.Powers.BuffIsActive(484426))
				return;
			
			//if ((Hud.Game.MapMode == MapMode.WaypointMap) || (Hud.Game.MapMode == MapMode.ActMap) || (Hud.Game.MapMode == MapMode.Map)) return;
			IBuff buff;
			float XPos = area.Rectangle.X + deltaX;
			float YPos = area.Rectangle.Y + deltaY;
			
			if (area.Name == "Self")
			{
				buff = Hud.Game.Me.Powers.GetBuff(484426); //Community_Buff_Weather
				if (buff.Active) //buff is object && 
				{
					TextLayout layout;

					//index = 1 (no tempest effect currently running) // index > 1 (index refers to the buff index of the current effect)
					int index = 1;
					foreach (int effectIndex in Tiers.Keys)
					{
						if (buff.IconCounts[effectIndex] > 0)
						{
							//this effect is currently playing
							index = effectIndex;
							break;
						}
					}
					
					//if (count > 0 || Debug) {
					int count = buff.IconCounts[index > 1 ? 9 : 1];
					double timeLeft = buff.TimeLeftSeconds[index];
						
					//draw count
					layout = TextFont.GetTextLayout(count.ToString());
					TextFont.DrawText(layout, XPos, YPos);

					int currentStackCount = buff.IconCounts[1];
					float currentStackXPos = XPos + layout.Metrics.Width;
					float currentStackYPos = YPos + layout.Metrics.Height;
						
					YPos += layout.Metrics.Height + 2;
						
					//draw timer bar
					float width = area.Rectangle.Width; //width_countdown_bar
					float height = index > 1 ? height_countdown_bar*ActiveBarHeightMultiplier : height_countdown_bar;
					TimerBg.DrawRectangle(XPos, YPos, width, height);
					float timeLeftPct = (float)(timeLeft / (buff.TimeElapsedSeconds[1] + timeLeft)); //1f; //
					if (timeLeftPct > 0) //this may become negative when you get dc'ed/idled out from a game
					{
						//IBrush brush = index > 1 ? ElementBrushes[index] : TimerHigh;
						if (index > 1)
						{
							IBrush brush = ElementBrushes[index];
							brush.StrokeWidth = 0;
							brush.Opacity = 1;
							brush.DrawRectangle(XPos, YPos, width * timeLeftPct, height);
						}
						else
						{
							TimerLow.DrawRectangle(XPos, YPos, width * timeLeftPct, height);
							TimerHigh.Opacity = timeLeftPct;
							TimerHigh.DrawRectangle(XPos, YPos, width * timeLeftPct, height);
						}
					}
					SkillBorderDark.DrawRectangle(XPos - 1, YPos - 1, width + 2, height + 2);
					SkillBorderLight.DrawRectangle(XPos - 2, YPos - 2, width + 4, height + 4);
					SkillBorderDark.DrawRectangle(XPos - 3, YPos - 3, width + 6, height + 6);
					//}
					
					//draw the time
					//System.TimeSpan remaining = System.TimeSpan.FromSeconds(timeLeft);
					if (timeLeft >= 0)
					{
						layout = TimerFont.GetTextLayout(timeLeft.ToString(timeLeft < 1 ? "F1" : "F0") + "s"); //remaining.ToString(remaining.Minutes < 1 ? "%s's'" : "m'm 'ss's'")
						TimerFont.DrawText(layout, XPos + width - layout.Metrics.Width, YPos - layout.Metrics.Height*1.25f - 2);
					}
					
					//draw the stuff below the countdown bar
					//YPos += height + height_countdown_bar;

					if (index > 1)
					{
						//draw the accumulating stack count of the next effect
						layout = TimerFont.GetTextLayout(string.Format(" [{0}]", currentStackCount));
						TimerFont.DrawText(layout, currentStackXPos, currentStackYPos - 2 - layout.Metrics.Height*1.25f);

						//draw the name of the effect currently happening
						float gap = layout.Metrics.Height*0.25f;
						IFont effect = ElementColors[index];
						layout = effect.GetTextLayout(Tiers[index]);
						effect.DrawText(layout, XPos + width*0.5f - layout.Metrics.Width*0.5f, YPos + height + gap);
						effect.DrawText(layout, XPos + width*0.5f - layout.Metrics.Width*0.5f, YPos + height + gap);
						
						YPos += height + height_countdown_bar;
						YPos += layout.Metrics.Height + 3;
					}
				}
			}
			else //if (area.Name == "Others")
			{
				TextLayout layout;

				//draw dummy entries for the party
				if (Debug && Hud.Game.NumberOfPlayersInGame < 2)
				{
					for (int i = 0; i < 3; ++i)
					{
						//YPos += 5; //height_predict + 3;
						
						//bool isInRange = player.HasValidActor && player.CoordinateKnown && Hud.Game.Me.SnoArea.Sno == player.SnoArea.Sno;
						string sCount = "1";
						double timeElapsed = 45;
						double timeLeft = 45;
						
						//if (otherCount > 0) {// && otherCount != currentStackCount) {
						layout = TimerFont.GetTextLayout(string.Format("[{0}]", sCount));
						TimerFont.DrawText(layout, XPos, YPos);
							
						float x = XPos + layout.Metrics.Width + 2;
						float widthTaken = layout.Metrics.Width;
						float width = area.Rectangle.Width; //width_countdown_bar
						
						TextLayout time = null; 
						if (timeLeft >= 0)
						{
							time = TimerOtherFont.GetTextLayout(timeLeft.ToString(timeLeft < 0 ? "F1" : "F0") + "s");
							//TimerOtherFont.DrawText(layout, XPos + width - layout.Metrics.Width, YPos - layout.Metrics.Height);
							widthTaken += time.Metrics.Width;
						}
						
						//int maxNameLength = System.Math.Min(maxLengthTier.Length, player.BattleTagAbovePortrait.Length);
						string battleTagTest = "PlayerWithALongName" + i;
						layout = OtherFont.GetTextLayout(battleTagTest); //.Substring(0, maxNameLength)
						float maxNameSpace = width - widthTaken;
						if (maxNameSpace < 0)
							maxNameSpace = width_countdown_bar;
						if (layout.Metrics.Width > maxNameSpace) //if battletag is too long, truncate it
						{
							float widthPerCharacter = layout.Metrics.Width / battleTagTest.Length;
							int nCharsMax = (int)(maxNameSpace / widthPerCharacter);
							layout = OtherFont.GetTextLayout(battleTagTest.Substring(0, nCharsMax));
						}
						OtherFont.DrawText(layout, x, YPos);
						height_predict = layout.Metrics.Height;

						YPos += layout.Metrics.Height;
							
							//x += layout.Metrics.Width + 2;
							
						//timeLeft = buff.TimeLeftSeconds[1];
						float height = height_countdown_bar*0.5f;
						if (timeElapsed + timeLeft == 90)
						{
							if (time is object)
								TimerOtherFont.DrawText(time, XPos + width - time.Metrics.Width, YPos - time.Metrics.Height);
							
							YPos += 6; //add a little extra clearance for borders
							
							TimerBg.DrawRectangle(XPos, YPos, width, height);
							float timeLeftPct = (float)(timeLeft / (timeElapsed + timeLeft)); //1f; //
							if (timeLeftPct > 0) //this may occur when you get dc'ed/idled out from a game
							{
								TimerLow.DrawRectangle(XPos, YPos, width * timeLeftPct, height);
								
								TimerHigh.Opacity = timeLeftPct;
								TimerHigh.DrawRectangle(XPos, YPos, width * timeLeftPct, height);
							}
							SkillBorderDark.DrawRectangle(XPos - 1, YPos - 1, width + 2, height + 2);
							SkillBorderLight.DrawRectangle(XPos - 2, YPos - 2, width + 4, height + 4);
							SkillBorderDark.DrawRectangle(XPos - 3, YPos - 3, width + 6, height + 6);
						}
						
						YPos += height + 5;
					}
					//return;
				}
				else
				{
					//float XPos = area.Rectangle.X + deltaX;
					//float YPos = area.Rectangle.Y + deltaY;
					
					//draw other players stack counts if they're different from yours
					//YPos += height_predict+3;
					foreach (IPlayer player in Hud.Game.Players.Where(p => !p.IsMe && !p.IsDead))
					{
						//YPos += height_predict + 3;
						
						bool isInRange = player.HasValidActor && player.CoordinateKnown && Hud.Game.Me.SnoArea.Sno == player.SnoArea.Sno;
						string sCount = string.Empty;
						double timeElapsed;
						double timeLeft;
						if (isInRange)
						{
							buff = player.Powers.GetBuff(484426);
							//if (buff == null)
							//	return; //continue; //if the buff isn't present on one person, it wouldn't be present on anyone
							
							timeLeft = buff.TimeLeftSeconds[1];
							timeElapsed = buff.TimeElapsedSeconds[1];
							CycleStartTick[player.HeroId] = Hud.Game.CurrentGameTick - (int)(timeElapsed*60); //save data for out of range drawing
							//int otherCount = buff.IconCounts[1];
							sCount = buff.IconCounts[1].ToString();
						}
						else 
						{
							sCount = "?";
							if (!CycleStartTick.ContainsKey(player.HeroId))
							{
								timeElapsed = 0;
								timeLeft = 0;
							}
							else
							{
								timeElapsed = (double)(Hud.Game.CurrentGameTick - CycleStartTick[player.HeroId]) / 60d;
								if (timeElapsed > 90)
									timeElapsed = timeElapsed % 90;
								timeLeft = 90 - timeElapsed;
							}
						}
						
						//if (otherCount > 0) {// && otherCount != currentStackCount) {
						layout = TimerFont.GetTextLayout(string.Format("[{0}]", sCount));
						TimerFont.DrawText(layout, XPos, YPos);
							
						float x = XPos + layout.Metrics.Width + 2;
						float widthTaken = layout.Metrics.Width;
						float width = area.Rectangle.Width; //width_countdown_bar
							
						TextLayout time = null; 
						if (timeLeft >= 0)
						{
							time = TimerOtherFont.GetTextLayout(timeLeft.ToString(timeLeft < 0 ? "F1" : "F0") + "s");
							//TimerOtherFont.DrawText(layout, XPos + width - layout.Metrics.Width, YPos - layout.Metrics.Height);
							widthTaken += time.Metrics.Width;
						}
						
						//int maxNameLength = System.Math.Min(maxLengthTier.Length, player.BattleTagAbovePortrait.Length);
						layout = OtherFont.GetTextLayout(player.BattleTagAbovePortrait); //.Substring(0, maxNameLength)
						float maxNameSpace = width - widthTaken;
						if (maxNameSpace < 0)
							maxNameSpace = width_countdown_bar;
						if (layout.Metrics.Width > maxNameSpace) //if battletag is too long, truncate it
						{
							float widthPerCharacter = layout.Metrics.Width / player.BattleTagAbovePortrait.Length;
							int nCharsMax = (int)(maxNameSpace / widthPerCharacter);
							layout = OtherFont.GetTextLayout(player.BattleTagAbovePortrait.Substring(0, nCharsMax));
						}
						OtherFont.DrawText(layout, x, YPos);
						height_predict = layout.Metrics.Height;

						YPos += layout.Metrics.Height;
							
							//x += layout.Metrics.Width + 2;
							
						//timeLeft = buff.TimeLeftSeconds[1];
						float height = height_countdown_bar*0.5f;
						if (timeElapsed + timeLeft == 90) //don't show 
						{
							if (time is object)
								TimerOtherFont.DrawText(time, XPos + width - time.Metrics.Width, YPos - time.Metrics.Height);
							
							YPos += 6; //add a little extra clearance for borders
							
							TimerBg.DrawRectangle(XPos, YPos, width, height);
							float timeLeftPct = (float)(timeLeft / (timeElapsed + timeLeft)); //1f; //
							if (timeLeftPct > 0) //this may occur when you get dc'ed/idled out from a game
							{
								TimerLow.DrawRectangle(XPos, YPos, width * timeLeftPct, height);
								
								if (isInRange)
								{
									TimerHigh.Opacity = timeLeftPct;
									TimerHigh.DrawRectangle(XPos, YPos, width * timeLeftPct, height);
								}
							}
							SkillBorderDark.DrawRectangle(XPos - 1, YPos - 1, width + 2, height + 2);
							SkillBorderLight.DrawRectangle(XPos - 2, YPos - 2, width + 4, height + 4);
							SkillBorderDark.DrawRectangle(XPos - 3, YPos - 3, width + 6, height + 6);
						}
						
						YPos += height + 5;
					}
				}
			}
			
			//bookkeeping
			if (LastSeenPlayerCount != Hud.Game.NumberOfPlayersInGame)
			{
				foreach (var key in CycleStartTick.Keys.Where(k => !Hud.Game.Players.Any(p => p.HeroId == k)).ToArray())
					CycleStartTick.Remove(key);
				LastSeenPlayerCount = Hud.Game.NumberOfPlayersInGame;
			}
        }
 
		public void PaintTopInGame(ClipState clipState)
		{
			if (clipState != ClipState.BeforeClip) return;
			
			if (!Hud.Game.Me.Powers.BuffIsActive(484426))
				return;
			
			IBrush brush;
			bool waveSeen = false;
			foreach (IActor actor in Hud.Game.Actors)
			{
				switch (actor.SnoActor.Sno)
				{
					case ActorSnoEnum._p69_community_snowboulder_projectile_roll:
						if (ShowSnowballDecorator)
						{
							//DrawWorldEllipse(float radius, int sectionCount, IWorldCoordinate coordinate)						
							BackgroundBrush.Opacity = 0.4f;
							BackgroundBrush.StrokeWidth = 8f;
							BackgroundBrush.DrawWorldEllipse(actor.RadiusScaled, -1, actor.FloorCoordinate);

							BackgroundBrush.Opacity = 0.2f;
							BackgroundBrush.StrokeWidth = 12f;
							BackgroundBrush.DrawWorldEllipse(actor.RadiusScaled, -1, actor.FloorCoordinate);

							brush = ElementBrushes[3];
							brush.Opacity = 1f;
							brush.StrokeWidth = 6f;
							brush.DrawWorldEllipse(actor.RadiusScaled, -1, actor.FloorCoordinate);
						}
						
						//SkillBorderLight.DrawWorldEllipse(actor.RadiusBottom, -1, actor.FloorCoordinate);
						break;
					case ActorSnoEnum._p4_mysterioushermit_firewave_projectile: //ActorSnoEnum._goatmanlore_corpse //testing
						if (ShowFlameWaveDecorator)
						{
							waveSeen = true;
							//IScreenCoordinate coord = actor.FloorCoordinate.ToScreenCoordinate();
							//if (LastWaveCoordinate is object && (LastWaveCoordinate.X != coord.X || LastWaveCoordinate.Y != coord.Y))
							
							IWorldCoordinate coord = actor.FloorCoordinate;
							//brush.DrawLineWorld(Hud.Game.Me.FloorCoordinate, coord);
							
							if (!LastWaveCoordinate.ContainsKey(actor.AnnId))
							{
								LastWaveCoordinate[actor.AnnId] = Hud.Window.CreateWorldCoordinate(coord); //coord //create a copy to remember the spot //Hud.Game.Me.FloorCoordinate;
								break;
							}

							var startingPoint = LastWaveCoordinate[actor.AnnId];
							if (startingPoint.XYDistanceTo(coord) > 200) //data correction
							{
								LastWaveCoordinate[actor.AnnId] = Hud.Window.CreateWorldCoordinate(coord); //coord //create a copy to remember the spot //Hud.Game.Me.FloorCoordinate;
								break;
							}
							
							//ElementBrushes[2].DrawLineWorld(LastWaveCoordinate, coord);
							if (!startingPoint.Equals(coord))
							{								
								float x1 = startingPoint.X;
								float y1 = startingPoint.Y;
								float x2 = coord.X;
								float y2 = coord.Y;
								//SkillBorderLight.DrawLine(coord.X, coord.Y, Hud.Window.CursorX, Hud.Window.CursorY);

								//orthogonal line math on the world coordinate system from ImpalePlugin by Gigi
								float m = (y2-y1)/(x2-x1);
								float orthm = -1 * (1/m);
								float yoff = y2 - orthm * x2;
								float offset = 16f;
								float x3 = (float)((x2*Math.Pow(orthm, 2) + x2 - Math.Sqrt(Math.Pow(offset, 2)*Math.Pow(orthm, 2) + Math.Pow(offset, 2))) / (Math.Pow(orthm, 2) + 1));
								float x4 = (float)((x2*Math.Pow(orthm, 2) + x2 + Math.Sqrt(Math.Pow(offset, 2)*Math.Pow(orthm, 2) + Math.Pow(offset, 2))) / (Math.Pow(orthm, 2) + 1));
								float y3 = orthm * x3 + yoff;
								float y4 = orthm * x4 + yoff;
								IWorldCoordinate coord3 = Hud.Window.CreateWorldCoordinate(x3, y3, coord.Z);
								IWorldCoordinate coord4 = Hud.Window.CreateWorldCoordinate(x4, y4, coord.Z);
								
								BackgroundBrush.Opacity = 0.4f;
								BackgroundBrush.StrokeWidth = 6f;
								BackgroundBrush.DrawLineWorld(coord3, coord4);

								BackgroundBrush.Opacity = 0.2f;
								BackgroundBrush.StrokeWidth = 10f;
								BackgroundBrush.DrawLineWorld(coord3, coord4);
								
								//SkillBorderLight.DrawLine(x3, y3, x4, y4);
								//SkillBorderLight.DrawLineWorld(x3, y3, LastWaveCoordinate.Z, x4, y4, coord.Z); //this function doesn't work
								brush = ElementBrushes[2];
								brush.Opacity = 1f;
								brush.StrokeWidth = 16f;
								brush.DrawLineWorld(coord3, coord4);
							}
						}
						break;
					case ActorSnoEnum._belial_groundbomb_event_pending:
						if (ShowMeteorDecorator)
						{
							BackgroundBrush.Opacity = 0.4f;
							BackgroundBrush.StrokeWidth = 6f;
							BackgroundBrush.DrawWorldEllipse(9.5f, -1, actor.FloorCoordinate);

							BackgroundBrush.Opacity = 0.2f;
							BackgroundBrush.StrokeWidth = 10f;
							BackgroundBrush.DrawWorldEllipse(9.5f, -1, actor.FloorCoordinate);

							//SkillBorderLight.DrawWorldEllipse(8, -1, actor.FloorCoordinate);
							brush = ElementBrushes[4];
							brush.Opacity = 1f;
							brush.StrokeWidth = 4f;
							brush.DrawWorldEllipse(9.5f, -1, actor.FloorCoordinate);
							//SkillBorderLight.DrawWorldEllipse(10f, -1, actor.FloorCoordinate);
						}
						break;
					case ActorSnoEnum._belial_groundbomb_event_impact:
						if (ShowMeteorDecorator)
						{
							/*BackgroundBrush.Opacity = 0.4f;
							BackgroundBrush.StrokeWidth = 6f;
							BackgroundBrush.DrawWorldEllipse(15f, -1, actor.FloorCoordinate);

							BackgroundBrush.Opacity = 0.2f;
							BackgroundBrush.StrokeWidth = 10f;
							BackgroundBrush.DrawWorldEllipse(15f, -1, actor.FloorCoordinate);*/

							brush = ElementBrushes[4];
							brush.Opacity = 0.2f;
							brush.StrokeWidth = 4f;
							//SkillBorderLight.DrawWorldEllipse(13, -1, actor.FloorCoordinate);
							//SkillBorderLight.DrawWorldEllipse(14, -1, actor.FloorCoordinate);
							brush.DrawWorldEllipse(15f, -1, actor.FloorCoordinate);
							//SkillBorderLight.DrawWorldEllipse(16, -1, actor.FloorCoordinate);
							//SkillBorderLight.DrawWorldEllipse(17, -1, actor.FloorCoordinate);
							//SkillBorderLight.DrawWorldEllipse(18, -1, actor.FloorCoordinate);
						}
						break;
					case ActorSnoEnum._zoltunkulle_energytwister:
						if (ShowTwisterDecorator)
						{
							brush = ElementBrushes[5];
							brush.Opacity = 1f;
							brush.StrokeWidth = 4f;							
							brush.DrawWorldEllipse(10f, -1, actor.FloorCoordinate);
							//Radius = 10,
							//Brush = Hud.Render.CreateBrush(160, 255, 50, 50, 2, DashStyle.Dash)
						}
						break;
				}
			}
			
			if (!waveSeen)
			{
				foreach (var annId in LastWaveCoordinate.Keys.Where(id => !Hud.Game.Actors.Any(a => a.AnnId == id)).ToArray())
					LastWaveCoordinate.Remove(annId);
			}
		}
    }
 
}