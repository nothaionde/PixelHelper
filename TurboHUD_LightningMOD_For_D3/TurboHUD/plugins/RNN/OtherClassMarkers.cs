using Turbo.Plugins.Default;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Turbo.Plugins.RNN
{
    public class OtherClassMarkers : BasePlugin, ICustomizer, IInGameWorldPainter, INewAreaHandler
    {
		public Dictionary<HeroClass, int[] > HeroBrush { get; set; } = new Dictionary<HeroClass, int[]> ();
		
		private Dictionary<HeroClass, WorldDecoratorCollection> DecoratorbyClass { get; set; } = new Dictionary<HeroClass, WorldDecoratorCollection>();

		private Dictionary<uint, uint> HeroIdTexture { get; set; } = new Dictionary<uint, uint>();
		
		private Dictionary<ActorSnoEnum, uint> ActorSnoTexture { get; set; } = new Dictionary<ActorSnoEnum, uint>  
		{			
				{(ActorSnoEnum) 75207, 3785199803},	{(ActorSnoEnum) 74706, 2939779782}, //dh
				{(ActorSnoEnum)  6544,   44435619},	{(ActorSnoEnum)  6526,  876580014}, //wiz
				{(ActorSnoEnum)  6485, 3925954876},	{(ActorSnoEnum)  6481, 1603231623}, //wd
				{(ActorSnoEnum)  3301, 3921484788},	{(ActorSnoEnum)  3285, 1030273087}, //barb
				{(ActorSnoEnum)  4721, 2227317895},	{(ActorSnoEnum)  4717, 2918463890}, //monk
				{(ActorSnoEnum)238284, 3742271755},	{(ActorSnoEnum)238286, 3435775766}, //crus
				{(ActorSnoEnum)454021, 3285997023},	{(ActorSnoEnum)454402,  473831658} //necro	
		};		

		private int MyIndex { get; set; } = -1;

		private ITexture HeroTexture { get; set;}

		private IBrush SancBrushOther { get; set; }
		private IBrush SancBrushMe { get; set; }
		private IFont TextFont { get; set; }
		private IBrush BrushShield { get; set; }
		private IBrush BrushDead { get; set; }	
		
		public bool NoGR {get; set;} 

		public bool ShowInTown {get; set;}
		
        public bool MyCircle {get; set;}
        public bool MyCircleColorOverride {get; set;}
		public bool MyCircleCenter {get; set;}
		
		public bool CircleMapOthers {get; set;}
		public bool CircleGroundOthers {get; set;}
		public bool CircleGroundCenterOthers {get; set;}
		
		public bool SancIpOthers {get; set;}
		public bool MySancIP  {get; set;}
		
		public float CircleGroundRadius {get; set;}
		
		public bool NamesGroundOthers {get; set;}
		public bool NamesMapOthers {get; set;}
		
		public bool AvatarGroundOthers {get; set;}
		public bool AvatarMapOthers {get; set;}
		
		public bool MyCircleImmune {get; set;} = true;			// Show other circle when I'm immune
		public bool AvatarLeaderMapOthers {get; set;} = false;
		
        public OtherClassMarkers()
        {
            Enabled = true;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
			Order = 300950;
			
			ShowInTown = true;
			NoGR = false;

			MyCircle = true;
            MyCircleColorOverride = true;	// If true my color will always be white
			MyCircleCenter = true;			// Circles Red&White For Me
		
			CircleMapOthers = true;
			CircleGroundOthers = true;
			CircleGroundRadius = 4f;		// Radius of the outer circle in the Ground. Assign positive values, or -1 to show the player's Hitbox	
			CircleGroundCenterOthers = true;

			MySancIP = true;  		// To know if I have ip and if I am inside a sanctuary. Only if MyCircleCenter is true
			SancIpOthers = false;	// To know if another player have ip && is inside a sanctuary
			
			NamesGroundOthers = true;
			NamesMapOthers = true;
			
			AvatarGroundOthers = false;
			AvatarMapOthers = true;
			
			AvatarLeaderMapOthers = true;	// Show frame for Leader on map (only if AvatarMapOthers = true)		
			
			//HeroClass.Barbarian , HeroClass.Crusader , HeroClass.DemonHunter, HeroClass.Monk , HeroClass.WitchDoctor, HeroClass.Wizard, HeroClass.Necromancer
			HeroBrush[HeroClass.Barbarian] =	new int[5] {255,237, 20, 20, 4}; 	// new int[5] { opacity , Red, Green , Blue ,  stroke }
            HeroBrush[HeroClass.Crusader] =		new int[5] {255,255,204,  0, 4};
            HeroBrush[HeroClass.DemonHunter] = 	new int[5] {255,  0,168,255, 4};
            HeroBrush[HeroClass.Monk] = 		new int[5] {255, 35,225, 6, 4};
            HeroBrush[HeroClass.WitchDoctor] =	new int[5] {255,255,132,  0, 4};
            HeroBrush[HeroClass.Wizard] = 		new int[5] {255, 220, 150,255, 4};
			HeroBrush[HeroClass.Necromancer] =	new int[5] {255, 64,192,176, 4};
			HeroBrush[HeroClass.None] = 		new int[5] {255,255,255,255, 4};  // Color Override 
			
			SancBrushOther = Hud.Render.CreateBrush(255, 255 , 255, 255, 3);		
			SancBrushMe = Hud.Render.CreateBrush(255, 64, 128, 255, 3);  //SancBrushMe = Hud.Render.CreateBrush(255, 0 , 255, 0, 3);  //64, 128, 255
			TextFont = Hud.Render.CreateFont("tahoma", 6, 255, 255, 255, 255, false, false, true);

			BrushShield = Hud.Render.CreateBrush(255, 255, 255, 255, 2f, SharpDX.Direct2D1.DashStyle.Dash);
			BrushDead = Hud.Render.CreateBrush(255, 255, 100, 100, -1);	
			
			IUiElement ui; string text;
			for (var i = 0; i < 4; i++)
			{
				text = "Root.NormalLayer.portraits.stack.party_stack.portrait_" + i + ".leaderIcon";
				ui = Hud.Render.GetUiElement(text);
				if (ui == null) Hud.Render.RegisterUiElement(text, ui, null);				
			}
        }

		public void OnNewArea(bool newGame, ISnoArea area)  
		{
			if (newGame || (MyIndex != Hud.Game.Me.Index) )  
			{
				MyIndex = Hud.Game.Me.Index;
				
				if (!MyCircleColorOverride)
				{
					DecoratorbyClass[HeroClass.None].Decorators.Clear();
					int[] c = HeroBrush[Hud.Game.Me.HeroClassDefinition.HeroClass]; 
					addDecoratorbyClass(HeroClass.None, c[0],c[1],c[2],c[3],c[4]);
				}	
			}
		}
		
        public void Customize()
        {
			Hud.TogglePlugin<OtherPlayersPlugin>(false);  // disables default plugin OtherPlayersPlugin
			if (AvatarMapOthers) Hud.TogglePlugin<HeadStonePlugin>(false);  // disables default plugin HeadStonePlugin
			
			foreach (HeroClass heroClass in Enum.GetValues(typeof(HeroClass)))
			{
				if ((uint)heroClass == (uint.MaxValue - 1)) continue;		//igual a if (heroClass == HeroClass.All) continue;	 , pero de esta forma es también compatible con ros-bot
				DecoratorbyClass.Add( heroClass, new WorldDecoratorCollection() );
				int[] c =  HeroBrush[(heroClass == HeroClass.None && !MyCircleColorOverride && Hud.Game?.IsInGame == true)?Hud.Game.Me.HeroClassDefinition.HeroClass:heroClass];
				addDecoratorbyClass(heroClass, c[0],c[1],c[2],c[3],c[4]);			
			}
        }
		
        public void addDecoratorbyClass(HeroClass hero, int o,int r,int g, int b, int t) 
		{
			var groundLabelBackgroundBrush = Hud.Render.CreateBrush(255, 0, 0, 0, 0);
			
			if ((CircleGroundOthers && (hero != HeroClass.None)) || ( MyCircle && (hero == HeroClass.None)))
				DecoratorbyClass[hero].Decorators.Add(
					new GroundCircleDecorator(Hud)
					{
						Brush = Hud.Render.CreateBrush(o,r,g,b,t),
						Radius = CircleGroundRadius,
					}
				);
			if 	(hero != HeroClass.None) 
			{
				if (CircleMapOthers) 
					DecoratorbyClass[hero].Decorators.Add(
					new MapShapeDecorator(Hud)
					{
						Brush = Hud.Render.CreateBrush(o,r,g,b,t),
						ShapePainter = new CircleShapePainter(Hud),
						Radius = 3f
					}
					);	
				if (CircleGroundCenterOthers) 	
					DecoratorbyClass[hero].Decorators.Add(
					new GroundCircleDecorator(Hud)
					{
						Brush = Hud.Render.CreateBrush(o,r,g,b, 19),
						Radius = 0.6f,
					}
					);
				if (NamesMapOthers)
					DecoratorbyClass[hero].Decorators.Add(
						new MapLabelDecorator(Hud)
						{
							LabelFont = Hud.Render.CreateFont("tahoma", 6f, o,r,g,b, false, false, 128, 0, 0, 0, true),
							Up = false,
							RadiusOffset = 7f,
						}
					);
				if (NamesGroundOthers)
					DecoratorbyClass[hero].Decorators.Add(
						new GroundLabelDecorator(Hud)
						{
							BackgroundBrush = groundLabelBackgroundBrush,
							BorderBrush = Hud.Render.CreateBrush(o,r,g,b, 1),
							TextFont = Hud.Render.CreateFont("tahoma", 6f, o,r,g,b, false, false, 128, 0, 0, 0, true),
							OffsetY = (AvatarGroundOthers)? 23f:0f,
						}
					);	
			}	
			else if (MyCircleCenter) 
			{
				DecoratorbyClass[hero].Decorators.Add(
					new GroundCircleDecorator(Hud) 
					{
						Brush = Hud.Render.CreateBrush(255, 255, 0, 0, 6.0f),
						Radius = 0.3f	
					}
				);
				DecoratorbyClass[hero].Decorators.Add(						
					new GroundCircleDecorator(Hud) 
					{
						Brush = Hud.Render.CreateBrush(255,255,255, 255, 4.0f),
						Radius = 0.7f	
					}
				);
			}	
		}
				
        public void PaintWorld(WorldLayer layer)
        {
			if (layer != WorldLayer.Ground) return;
			if (!Hud.Game.IsInGame) return;
			if (NoGR && Hud.Game.Me.InGreaterRiftRank > 0) return;					
			if (Hud.Game.IsInTown && !ShowInTown) return;
			
            var players = Hud.Game.Players.Where(p => p.CoordinateKnown);
            foreach (var pl in players)   
			{
				if ( (Hud.Game.Me.SnoArea.Sno != pl.SnoArea.Sno) && ((Hud.Game.Me.SnoArea.HostAreaSno == 288482) || (Hud.Game.Me.SnoArea.Sno == 288482)) ) continue;
				if ( Hud.Game.Me.IsInTown ^ pl.IsInTown) continue;
				
                if (DecoratorbyClass.TryGetValue(pl.IsMe?HeroClass.None:pl.HeroClassDefinition.HeroClass, out var decorator))
				{                    
					decorator.Paint(layer, pl, pl.FloorCoordinate, pl.BattleTagAbovePortrait /*"Player" + pl.Index*/);
					decorator.Paint(WorldLayer.Map, pl, pl.FloorCoordinate, pl.BattleTagAbovePortrait /*"Player" + pl.Index*/);
				}								

				if (pl.IsMe)
				{
					if (MySancIP && MyCircleCenter) 
					{
						if (pl.Powers.BuffIsActive(317076, 1)) { SancBrushMe.DrawEllipse(pl.FloorCoordinate.ToScreenCoordinate().X  , pl.FloorCoordinate.ToScreenCoordinate().Y , 14, 10);	}
						if (pl.Powers.BuffIsActive(79528))
						{
							var l = TextFont.GetTextLayout("Ip");
							TextFont.DrawText(l,pl. FloorCoordinate.ToScreenCoordinate().X - l.Metrics.Width / 2 ,pl.FloorCoordinate.ToScreenCoordinate().Y - l.Metrics.Height / 2);	
						}	
					}
					if ( MyCircleImmune && (pl.Powers.BuffIsActive(266254) || pl.Powers.BuffIsActive(439438) || pl.Powers.BuffIsActive(224639) || pl.Powers.BuffIsActive(30290)) ) // Shield, TP, Ghost , buff
						BrushShield.DrawWorldEllipse(1.4f, -1, pl.FloorCoordinate);					
				}
				else 
				{
					if (!HeroIdTexture.TryGetValue(pl.HeroId, out var TextureSno))
					{
						if (pl.HasValidActor) 
						{
							TextureSno = ActorSnoTexture[pl.SnoActor.Sno];
							HeroIdTexture.Add(pl.HeroId,TextureSno); 
						}
						else { TextureSno = ActorSnoTexture[pl.HeroClassDefinition.MaleActorSno]; }
					}				
					HeroTexture = Hud.Texture.GetTexture(TextureSno);	
					
					if (AvatarMapOthers)
					{
						Hud.Render.GetMinimapCoordinates(pl.FloorCoordinate.X, pl.FloorCoordinate.Y, out float mapX, out float mapY);										
						float PlayersHeadOpacity = Hud.Game.AliveMonsters.Any(m => m.Rarity != ActorRarity.Normal && m.SummonerAcdDynamicId == 0 && (m.FloorCoordinate.XYDistanceTo(pl.FloorCoordinate) < 25))? 0.20f: 1.0f;					
						HeroTexture.Draw(mapX - HeroTexture.Width / 20, mapY - HeroTexture.Height / 17, HeroTexture.Width / 10 , HeroTexture.Height / 10, PlayersHeadOpacity);

						if (pl.IsDeadSafeCheck)
						{
							BrushDead.DrawEllipse(mapX, mapY - HeroTexture.Height / 100, HeroTexture.Width/ 25, HeroTexture.Width/ 20);
							BrushDead.DrawLine(mapX - HeroTexture.Width / 25, mapY - HeroTexture.Height / 100, mapX + HeroTexture.Width / 25, mapY - HeroTexture.Height / 100);
							BrushDead.DrawLine(mapX, mapY - HeroTexture.Height / 20 , mapX , mapY + HeroTexture.Height / 30);		
						}
					
						if (AvatarLeaderMapOthers && Hud.Render.GetUiElement("Root.NormalLayer.portraits.stack.party_stack.portrait_" + pl.PortraitIndex + ".leaderIcon").Visible) 							
							Hud.Texture.BuffFrameTexture.Draw(mapX - HeroTexture.Width / 20, mapY - HeroTexture.Height / 16, HeroTexture.Width / 10 , HeroTexture.Height / 10, 1.0f);						 						
					}
					if (pl.IsOnScreen)
					{
						if (AvatarGroundOthers)  
							HeroTexture.Draw(pl.FloorCoordinate.ToScreenCoordinate().X - HeroTexture.Width/15, pl.FloorCoordinate.ToScreenCoordinate().Y - HeroTexture.Height/13, HeroTexture.Width/7.5f, HeroTexture.Height/7.5f, 1f);
						
						if (SancIpOthers) 	
						{
							if (pl.Powers.BuffIsActive(317076, 1))	SancBrushOther.DrawEllipse(pl.FloorCoordinate.ToScreenCoordinate().X  , pl.FloorCoordinate.ToScreenCoordinate().Y , 20, 16);
							if (pl.Powers.BuffIsActive(79528)) 
							{
								var l = TextFont.GetTextLayout("Ip");
								TextFont.DrawText(l,pl. FloorCoordinate.ToScreenCoordinate().X - l.Metrics.Width / 2 ,pl.FloorCoordinate.ToScreenCoordinate().Y );	
							}
						}
					}					
				}
            }		
        }
    }
}