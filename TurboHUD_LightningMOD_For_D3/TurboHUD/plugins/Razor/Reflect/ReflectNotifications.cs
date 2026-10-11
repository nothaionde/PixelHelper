/*

Changelog
- March 17, 2024
	- implemented hook to DangerDisplayMenu
	- moved all hooks to a separate (optional) customizer file
	- updated with reflect monster list
	- no longer draws ground circles when full map is shown
- March 16, 2024
	- increased the draw Order so that it draws over blood pools markers
	- darkened the shadow for increased contrast
	- implemented hook for SoundNotifierMenu to toggle ReflectNotifications.PlaySounds on and off
- July 22, 2021
	- new decorator for actively reflecting mobs
- Dec 3, 2020
	- Fixed some spinners not being marked
	- Removed the Movable text notification area - the text now appears at the reflecting monster's feet, so it is easier to identify the locations to avoid targeting with your projectiles

*/

namespace Turbo.Plugins.Razor
{
	using SharpDX;
	using SharpDX.DirectWrite;
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Media;
	using System.Threading;

	using Turbo.Plugins.Default;
	//using Turbo.Plugins.Razor.Drawing;

	public class ReflectNotifications : BasePlugin, IInGameTopPainter//, ICustomizer //, IInGameWorldPainter//, IMovable
	{
		public string ReflectHintText { get; set; } = "Отражение";	// Default "Отражение" - "Reflecting" - Надпись в ногах у монстра

		public bool PlaySounds { get; set; } = false;	// Default true - Воспроизвести звуковой файл при отражение
		public string ReflectHintSound { get; set; } = "Reflecting.wav"; //optional
		public string ReflectHintTTS { get; set; } = " "; //optional. Default "Отражение" - "Reflecting" - Текст в речь (может быть задержка)
		private SoundPlayer ReflectSound;
		private bool SoundPlayed;

		public IFont ReflectHint { get; set; }
		public WorldDecoratorCollection ReflectDecorator { get; set; }
		public WorldDecoratorCollection ReflectDecoratorSpin { get; set; }
		public WorldDecoratorCollection ReflectMapDecorator { get; set; }
		public WorldDecoratorCollection ReflectMapDecoratorSpin { get; set; }
		
		/*public uint[] ReflectSnos = new uint[] {
			3981, //		26066		DuneDervish_B				Dune Dervish	
			3982, //		26067		DuneDervish_C				Vicious Magewraith	
			5191, //		26245		SandMonster_A				Sand Dweller	
			5192, //		57063		SandMonster_B				Rock Giant	
			5193, //		57064		SandMonster_C				Sand Behemoth	
			256022, //		256025		DuneDervish_B_Unique_Uber		Sokahr the Keywarden of Putridness	
			418900, //		418901		p1_LR_sandMonster_A			Sand Dweller	
			116076, //		116075		SandMonster_A_FastWakeup		Sand Dweller	
			116299, //		116305		SandMonster_A_Gauntlet			Sand Dweller	
			//344389, //		344391		X1_LR_Boss_SandMonster			Stonesinger	or sand dweller elite
			297730, //		297726		x1_Malthael_Boss			Malthael	
		};
		
		public HashSet<uint> ReflectMonsters = new HashSet<uint>()
		{
			//tested in patch 2.6.4
			5191, //Sand Dweller - gr - Buff_Visual_Effect	61	1
			5192, //Sand Dweller - adventure mode - Buff_Visual_Effect	61	1
			3981, //Vicious Magewraith - adventure mode
			3982, //Vicious Magewraith - Gr
			256022, //act 2 keywarden - Power_Buff_1_Visual_Effect_None	256026	0	power: Dervish_Whirlwind_Mortar_Prototype / Power_Buff_0_Visual_Effect_None	256026	1	power: Dervish_Whirlwind_Mortar_Prototype
			344389, //stonesinger gr or sand dweller elite
			418900, //stonesinger gr or sand dweller elite
			297730, //malthael
			116076, //		116075		SandMonster_A_FastWakeup		Sand Dweller	
			116299, //		116305		SandMonster_A_Gauntlet			Sand Dweller	

			//old snos from another plugin
			116075, //Sand Dweller - adventure mode
			26245, //Sand Dweller
			116305, //Sand Dweller
			196846, //Sand Dweller
			26066, //Dune Dervish
			26067, //Vicious Magewraith
			57063, //Rock Giant - adventure mode
			57064, //Sand Behemoth - adventure mode
			418901, //Sand Dweller - Gr 
		};*/

		public ReflectNotifications()
		{
			Enabled = true;
			Order = 60006; //higher = drawn above anything with lower order value, try to draw it over other plugins

			//AnimateWhenReflecting = true;			
			//ShowReflectHint = true;
		}

		public override void Load(IController hud)
		{
			base.Load(hud);

			ReflectDecorator = new WorldDecoratorCollection(
				// outline circle
				new GroundCircleDecorator(Hud)
				{
					Brush = Hud.Render.CreateBrush(225, 255, 100, 50, 2, SharpDX.Direct2D1.DashStyle.Dash),
					Radius = -1f,
				},
				new GroundShapeDecorator(Hud)
				{
					Brush = Hud.Render.CreateBrush(225, 255, 100, 50, 6),
					ShadowBrush = Hud.Render.CreateBrush(155, 0, 0, 0, 2),
					Radius = 3.0f,
					ShapePainter = WorldStarShapePainter.NewPentagram(Hud),
				}
			);
			
			ReflectDecoratorSpin = new WorldDecoratorCollection(
				new GroundLabelDecorator(Hud)
				{
					BackgroundBrush = Hud.Render.CreateBrush(0, 0, 0, 0, 0),
					TextFont = Hud.Render.CreateFont("tahoma", 20, 255, 255, 255, 255, true, true, true),
				},
				// outline circle
				new GroundCircleDecorator(Hud)
				{
					Brush = Hud.Render.CreateBrush(225, 255, 100, 50, 2, SharpDX.Direct2D1.DashStyle.Dash),
					Radius = -1f,
				},
				new GroundOrbitDecorator(Hud)
                {
                    Brush = Hud.Render.CreateBrush(255, 255, 100, 50, 0),
					ShadowBrush = Hud.Render.CreateBrush(75, 0, 0, 0, 0),
					//ShadowWidth = 1f,
                    Radius = 6.0f,
					Interval = 45,
					Direction = (x, y) => (x == 0 && y == 0 ? 0 : (float)(Math.Atan2(y, x) / (Math.PI / 180f))),
					ShapePainter = new WorldTriangleShapePainter(Hud) {BorderWidth = 2f},
					RadiusTransformator = new StandardPingRadiusTransformator(Hud, 700) {RadiusMinimumMultiplier = 0.6f},
					//RotationTransformator = new CircularRotationTransformator(Hud, 10)
                }
			);
					
			ReflectMapDecorator = new WorldDecoratorCollection(
				new MapShapeDecorator(Hud)
				{
					ShapePainter = new StarShapePainter(Hud),
					Brush = Hud.Render.CreateBrush(225, 255, 100, 50, 1),
					ShadowBrush = Hud.Render.CreateBrush(150, 0, 0, 0, 1),
					Radius = 7.5f,
				}
			);
			
			ReflectMapDecoratorSpin = new WorldDecoratorCollection(
				new MapShapeDecorator(Hud)
				{
					ShapePainter = new RotatingStarShapePainter(Hud),
					Brush = Hud.Render.CreateBrush(225, 255, 100, 50, 1),
					ShadowBrush = Hud.Render.CreateBrush(150, 0, 0, 0, 1),
					Radius = 7.5f,
				}
			);
			
			ReflectHint = Hud.Render.CreateFont("tahoma", 20f, 255, 255, 100, 50, true, false, 242, 0, 0, 0, true);
			
			if (!string.IsNullOrEmpty(ReflectHintSound)) ReflectSound = Hud.Sound.LoadSoundPlayer(ReflectHintSound);

		}
		
		//public void PaintWorld(WorldLayer layer)
		public void PaintTopInGame(ClipState clipState)
		{
			if (clipState != ClipState.BeforeClip || Hud.Game.IsInTown)
				return;
			
			bool reflecting = false;
			
			foreach (IMonster m in Hud.Game.AliveMonsters)
			{
				bool monster_is_reflector = true;
				bool monster_is_reflecting = false;
				
				//borrowed from DyS's plugin (I think his monster list adds some entries to mine)
				switch (m.SnoActor.Sno)
				{
					// Act 2 Keywarden
					case ActorSnoEnum._dunedervish_b_unique_uber:
						monster_is_reflecting = (m.GetAttributeValueAsUInt(Hud.Sno.Attributes.Power_Buff_0_Visual_Effect_None, Hud.Sno.SnoPowers.Generic_DervishWhirlwindMortarPrototype.Sno) == 1);
						break;

					// Dune Dervishes
					case ActorSnoEnum._dunedervish_a:
					case ActorSnoEnum._dunedervish_a_dervishtwister_unique:
					case ActorSnoEnum._dunedervish_a_dyingmanmine:
					case ActorSnoEnum._dunedervish_a_unique_01:
					case ActorSnoEnum._dunedervish_b:
					case ActorSnoEnum._dunedervish_b_unique_01:
					case ActorSnoEnum._dunedervish_c:
					case ActorSnoEnum._p73_dunedervish_c:
					case ActorSnoEnum._p76_dunedervish_c:
						monster_is_reflecting = (m.GetAttributeValueAsInt(Hud.Sno.Attributes.Power_Buff_0_Visual_Effect_None, Hud.Sno.SnoPowers.Generic_DervishWhirlwind.Sno) == 1);
						break;

					// Sand Dwellers
					case ActorSnoEnum._sandmonster_a:
					case ActorSnoEnum._sandmonster_b:
					case ActorSnoEnum._sandmonster_c:
					case ActorSnoEnum._sandmonster_d:
					case ActorSnoEnum._sandmonster_a_fastwakeup:
					case ActorSnoEnum._sandmonster_a_gauntlet:
					case ActorSnoEnum._sandmonster_a_head_guardian:
					case ActorSnoEnum._sandmonster_a_portalroulette:
					case ActorSnoEnum._sandmonster_b_unique_01:
					case ActorSnoEnum._sandmonster_c_unique_01:
					case ActorSnoEnum._sandmonster_a_eternal_guardian_zoltboss:
					case ActorSnoEnum._sandmonster_ideation_pandext:
					case ActorSnoEnum._x1_pand_ext_ideation_sandmonster_rock_a:
					case ActorSnoEnum._x1_pand_ext_ideation_sandmonster_rock_b2:
					case ActorSnoEnum._p1_lr_sandmonster_a:
					case ActorSnoEnum._p43_d1_sandmonster_1:
					case ActorSnoEnum._p43_d1_sandmonster_2:
					case ActorSnoEnum._p43_d1_sandmonster_3:
					case ActorSnoEnum._p43_d1_sandmonster_4:
					case ActorSnoEnum._p43_d1_sandmonster_1_unique_firewound:
					case ActorSnoEnum._p73_sandmonster_a_gauntlet:
					case ActorSnoEnum._p73_sandmonster_a:
					case ActorSnoEnum._p76_sandmonster_a_gauntlet:
						monster_is_reflecting = (m.GetAttributeValueAsInt(Hud.Sno.Attributes.Buff_Visual_Effect, 61) == 1);
						break;
						
					//Malthael
					case ActorSnoEnum._x1_malthael_boss:
						monster_is_reflecting = (m.GetAttributeValueAsInt(Hud.Sno.Attributes.Power_Buff_0_Visual_Effect_None, 325648) == 1);
						break;


					default:
						monster_is_reflector = false;
						break;
				}
				
				if (monster_is_reflecting)
				{
					reflecting = true;
					if (Hud.Game.MapMode == MapMode.Minimap) ReflectDecoratorSpin.Paint(WorldLayer.Ground, m, m.FloorCoordinate, ReflectHintText); //ReflectMapDecoratorSpin
					ReflectMapDecoratorSpin.Paint(WorldLayer.Map, m, m.FloorCoordinate, ReflectHintText); //ReflectMapDecoratorSpin
				}
				else if (monster_is_reflector)
				{
					if (Hud.Game.MapMode == MapMode.Minimap) ReflectDecorator.Paint(WorldLayer.Ground, m, m.FloorCoordinate, null);
					ReflectMapDecorator.Paint(WorldLayer.Map, m, m.FloorCoordinate, null);
				}
			}
			
			if (PlaySounds)
			{
				if (reflecting)
				{
					if (!SoundPlayed) {
						ThreadPool.QueueUserWorkItem(state => {
							try {
								if (ReflectSound is object)
									ReflectSound.PlaySync();
								else if (!string.IsNullOrEmpty(ReflectHintTTS))
									Hud.Sound.Speak(ReflectHintTTS);
							} catch (Exception) {}
						});
						
						SoundPlayed = true;
					}
				}
				else if (SoundPlayed)
					SoundPlayed = false;
			} 
			
		} // end: paintworld

	}
}