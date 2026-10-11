//	같은 맵상의 파티원수 및 사망 확인용 플러그인(2인 이상에서 작동)	-----Make by RNN
//	설명1) 	닉네임 표시 색의 의미 :녹색(같은지역), 노랑(다른지역), 회색(다른지역.마을)
//	설명2)	🏴표시 색의 의미 : 녹색(모든멤버가 같은지역일때), 빨강(1명이라도 다른지역일때)
//2021.05.06	투영위치에 인원수 및 All Ready 표시 추가-----Edit by El
//2021.05.14	신단에 인원수 표시 추가	-----Edit by El
//2022.05.28	마을 첨탑에 인원표시 추가	-----Edit by El
//		4인 이하일때 첨탑에 1,2,3 파티참여인원 표시, 4인시 첨탑에  All Ready 표시

using Turbo.Plugins.Default;
using System.Linq;
using System;

namespace Turbo.Plugins.RNN
{
    public class DeathInMap : BasePlugin, IInGameTopPainter, IInGameWorldPainter
    {
	private IFont RedFont { get; set; }
	private IFont GreenFont { get; set; }
	private IFont GrayFont { get; set; }
	private IFont NamesOrangeFont { get; set; }
	private IFont NamesRedFont { get; set; }
	private IFont NamesGreenFont { get; set; }
	private IFont NamesGrayFont { get; set; }
	private IFont NamesYellowFont { get; set; }

	private IFont DefaultFont { get; set; }
	public float XPorc { get; set; }
	public float YPorc { get; set; }
	public bool ShowInTown { get; set; }
	public bool ShowDead { get; set; }
	public bool ShowTown { get; set; }

	public WorldDecoratorCollection NotReadyDecorator { get; set; }
	public WorldDecoratorCollection AllReadyDecorator { get; set; }
	public int InMapPlayer { get; set; }

	public DeathInMap()
	{
		Enabled = true;
	}

	public override void Load(IController hud)
	{
            	base.Load(hud);
	            XPorc = 0.825f;	//0.825f	미니맵 좌상단	0.400f 화면 중앙상단	
            	YPorc = 0.325f;	//0.047f	미니맵 좌상단	0.001f 화면 중앙상단	
		ShowInTown = false;	
		ShowDead = false; 		// Show name players deads of Color red (in Map) or orange (out Map) .
		ShowTown = false; 		// Show name players in Town of Color gray (out Map). 
			
		RedFont = Hud.Render.CreateFont("tahoma", 8f, 250, 250, 0, 0, false, false, 128, 0, 0, 0, true) ;
		GreenFont = Hud.Render.CreateFont("tahoma", 8f, 225, 0, 250, 0, false, false, 128, 0, 0, 0, true) ;
		GrayFont = Hud.Render.CreateFont("tahoma", 7f, 225, 180, 180, 180, false, false, 128, 0, 0, 0, true) ;	
		DefaultFont = GreenFont;
		NamesOrangeFont = Hud.Render.CreateFont("tahoma", 6f, 225, 250, 150, 50, false, false, 128, 0, 0, 0, true) ;
		NamesRedFont = Hud.Render.CreateFont("tahoma", 6f, 250, 250, 0, 0, false, false, 128, 0, 0, 0, true) ;
		NamesGreenFont = Hud.Render.CreateFont("tahoma", 6f, 225, 0, 250, 0, false, false, 128, 0, 0, 0, true) ;
		NamesGrayFont = Hud.Render.CreateFont("tahoma", 6f, 225, 180, 180, 180, false, false, 128, 0, 0, 0, true) ;	
		NamesYellowFont = Hud.Render.CreateFont("tahoma", 6f, 225, 255, 255, 0, false, false, 128, 0, 0, 0, true) ;	

		NotReadyDecorator = new WorldDecoratorCollection(	//vvv NotReady
			new GroundLabelDecorator(Hud)
			{
				BackgroundBrush = Hud.Render.CreateBrush(200, 255, 255, 255, -2),
				TextFont = Hud.Render.CreateFont("tahoma", 12, 255, 255, 0, 0, true, true, false),	//Red font
			});

		AllReadyDecorator = new WorldDecoratorCollection(	//vvv AllReady
			new GroundLabelDecorator(Hud)
			{
				BackgroundBrush = Hud.Render.CreateBrush(160, 0, 255, 0, -1),
				TextFont = Hud.Render.CreateFont("tahoma", 14, 255, 0, 255, 0, true, false, false),	//Green font
			});

	}
 
	public void PaintTopInGame(ClipState clipState)
	{
		if (Hud.Render.UiHidden) return;
		if (clipState != ClipState.BeforeClip) return;	
		if (Hud.Game.IsInGame) 
		{
			if (ShowInTown || !Hud.Game.Me.IsInTown) 
			{
				if ( Hud.Game.NumberOfPlayersInGame > 1 )
				//if ( Hud.Game.NumberOfPlayersInGame >= 1 )	//1인 테스트용
				{
					bool Alive = true; bool EnMapa = true ; int EnMiMapa = 0 ; 
					string PlayerIN = string.Empty, PlayerOUT = string.Empty, PlayerINDead = string.Empty, PlayerOUTDead = string.Empty, PlayerOUTTown = string.Empty;
					foreach (var player in Hud.Game.Players) 
					{
						if (player.IsDead) Alive = false;
						if (player.SnoArea.Sno != Hud.Game.Me.SnoArea.Sno)
						{
							EnMapa = false;
							if (ShowTown && player.IsInTown) { PlayerOUTTown += " " + player.BattleTagAbovePortrait; }
							else if (ShowDead && player.IsDead) { PlayerOUTDead += " " + player.BattleTagAbovePortrait; }
							else { PlayerOUT += " " + player.BattleTagAbovePortrait; }
						}
						else 
						{
							EnMiMapa++ ; 
							if (!player.IsMe) 
							{
								if (ShowDead && player.IsDead) { PlayerINDead += " " + player.BattleTagAbovePortrait; }
								else { PlayerIN += " " + player.BattleTagAbovePortrait; }

							}
						}
					}

					//Закоментирована надпись под миникартой с никами игроков
					InMapPlayer=EnMiMapa;	//vvv
					var xpos = Hud.Window.Size.Width * XPorc;  var ypos = Hud.Window.Size.Height * YPorc;

					DefaultFont = Alive?GreenFont:RedFont ;
					var layout = DefaultFont.GetTextLayout("☠️");
					DefaultFont.DrawText(layout, xpos, ypos );  
					xpos += layout.Metrics.Width * 1.15f;					
						
					DefaultFont = EnMapa?GreenFont:RedFont ; 
					layout = DefaultFont.GetTextLayout("🏴");
					DefaultFont.DrawText(layout, xpos , ypos );  
					xpos += layout.Metrics.Width * 0.85f;
					
					layout =  GrayFont.GetTextLayout(" [" + EnMiMapa + "/" + Hud.Game.NumberOfPlayersInGame + "]");
					GrayFont.DrawText(layout , xpos , ypos + 1 );  
					xpos += layout.Metrics.Width;
						
					if (PlayerIN != string.Empty)	//
					{
						layout = NamesGreenFont.GetTextLayout(PlayerIN);
						NamesGreenFont.DrawText(layout, xpos, ypos + 1 );
						xpos +=  layout.Metrics.Width;
					}
					if (PlayerOUTDead != string.Empty)
					{
						layout = NamesOrangeFont.GetTextLayout(PlayerOUTDead);
						NamesOrangeFont.DrawText(layout, xpos, ypos + 1 );
						xpos +=  layout.Metrics.Width;
					}
					if (PlayerINDead != string.Empty)
					{
						layout = NamesRedFont.GetTextLayout(PlayerINDead);
						NamesRedFont.DrawText(layout, xpos, ypos + 1 );
						xpos +=  layout.Metrics.Width;
					}
					if (PlayerOUT != string.Empty)  
					{
						layout = NamesYellowFont.GetTextLayout(PlayerOUT);
						NamesYellowFont.DrawText(layout, xpos, ypos + 1 );
						xpos +=  layout.Metrics.Width;
					}
					if (PlayerOUTTown != string.Empty)
					{
						NamesGrayFont.DrawText(NamesGrayFont.GetTextLayout(PlayerOUTTown) , xpos, ypos + 1 ); 													
					}  
				}
			}
		}
	}

	//투영위치에 인원수 표시 및 All Ready! 표시
	public void PaintWorld(WorldLayer layer)
	{
		//if (Hud.Game.IsInTown) return;
		if (Hud.Game.NumberOfPlayersInGame < 2) return;

		var markers = Hud.Game.Markers.Where(s => s.FloorCoordinate.IsValid && ( s.IsPoolOfReflection ));	//투영
		foreach (var marker in markers)
		{
			if (!marker.IsUsed)
			{
			    if (marker.IsPoolOfReflection)
			    {
				var poolofreflection = Hud.Game.Actors.Where(x => (uint)x.SnoActor.Sno == 373463);
				foreach (var actor in poolofreflection)
				{
					if(InMapPlayer != Hud.Game.NumberOfPlayersInGame && (marker.FloorCoordinate.XYDistanceTo(Hud.Game.Me.FloorCoordinate) < 100))
					{ NotReadyDecorator.Paint(layer, actor, actor.FloorCoordinate.Offset(0, 0, 2f), "["+InMapPlayer+"/"+Hud.Game.NumberOfPlayersInGame+"]"+"\r\n"+"Жди!"); }
					else if (marker.FloorCoordinate.XYDistanceTo(Hud.Game.Me.FloorCoordinate) < 100 )
					{ AllReadyDecorator.Paint(layer, actor, actor.FloorCoordinate.Offset(0, 0, 2f), "Все Тут!"); }
				}
			    }
			}
		}

		var shrines = Hud.Game.Shrines.Where(x => (x.IsShrine || x.IsPylon) && !x.IsDisabled && !x.IsOperated );	//신단
		foreach (var p_actor in shrines)
		{
			if(InMapPlayer != Hud.Game.NumberOfPlayersInGame && (p_actor.IsOnScreen) && (!p_actor.IsOperated))
			{ NotReadyDecorator.Paint(layer, p_actor, p_actor.FloorCoordinate.Offset(0, 0, 5.7f), "["+InMapPlayer+"/"+Hud.Game.NumberOfPlayersInGame+"]"+"\r\n"+"Жди!"); }
			else if (p_actor.IsOnScreen && (!p_actor.IsOperated))	//!p_actor.IsClickable
			{ AllReadyDecorator.Paint(layer, p_actor, p_actor.FloorCoordinate.Offset(0, 0, 5.2f), "Все Тут!"); }
		}


		//ActorSnoEnum._x1_openworld_lootrunobelisk_b

		//Закоментировано отображение надписи на столпе в городе что все готовы
		var obelisk = Hud.Game.Actors.Where(x => x.SnoActor.Sno == ActorSnoEnum._x1_openworld_lootrunobelisk_b && x.IsOnScreen); 	//첨탑에 인원표시용
		foreach (var o_actor in obelisk)
		{
			//if(InMapPlayer != Hud.Game.NumberOfPlayersInGame && (o_actor.IsOnScreen) && (!o_actor.IsOperated))
			//if(InMapPlayer != Hud.Game.NumberOfPlayersInGame && (o_actor.IsOnScreen) && (!o_actor.IsOperated))
			//{ NotReadyDecorator.Paint(layer, o_actor, o_actor.FloorCoordinate.Offset(0, 0, 5.7f), "["+InMapPlayer+"/"+Hud.Game.NumberOfPlayersInGame+"]"+"\r\n"+"Wait!"); }
			//else if (o_actor.IsOnScreen && (!o_actor.IsOperated))	//!o_actor.IsClickable
			//{ AllReadyDecorator.Paint(layer, o_actor, o_actor.FloorCoordinate.Offset(0, 0, 5.2f), "All Ready!"); }

			if(Hud.Game.NumberOfPlayersInGame == 4 && (o_actor.IsOnScreen) && (!o_actor.IsOperated))
			{ AllReadyDecorator.Paint(layer, o_actor, o_actor.FloorCoordinate.Offset(0, 0, 5.2f), "Все Тут!"); }
			else if (o_actor.IsOnScreen && (!o_actor.IsOperated))	//!o_actor.IsClickable
			{ NotReadyDecorator.Paint(layer, o_actor, o_actor.FloorCoordinate.Offset(0, 0, 5.7f), "["+Hud.Game.NumberOfPlayersInGame+"]"+"\r\n"+"__"); }
		}  
	} 
	//
    }
}