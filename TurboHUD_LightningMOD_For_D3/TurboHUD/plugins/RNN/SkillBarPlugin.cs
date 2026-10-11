// Inspired by the glq plugin PlayerSkillBarPlugin. Link: 
// https://www.ownedcore.com/forums/diablo-3/turbohud/turbohud-community-plugins/631088-v7-3-international-glq-playerskillbarplugin.html

using Turbo.Plugins.Default;
using System;
using System.Linq;
using System.Collections.Generic;
using SharpDX.DirectInput;

namespace Turbo.Plugins.RNN
{
    public class SkillBarPlugin : BasePlugin, IInGameTopPainter, ICustomizer, IKeyEventHandler, INewAreaHandler
    {
		private Dictionary<uint,uint> ReplaceSno { get; set; } = new Dictionary<uint,uint>  			
		{			
				{375049, 375050},	// Mantra Salvación		(Monk)
				{375082, 375083},	// Mantra Reprensión	(Monk)
				{373143, 373154},	// Mantra Curación		(Monk)
				{375088, 375089},	// Mantra Convicción	(Monk)
				{362102, 362118},	// Aliado Místico		(Monk)

				{342281, 342284},	// Ley Valor		(Crus)
				{342280, 342286},	// Ley Justicia		(Crus)
				{342279, 342299},	// Ley Esperanza	(Crus)

				{392883, 134872},	// Archon Override Timeleft (Wiz) Cold
				{167355, 134872},	// Archon Override Timeleft (Wiz) Arcane
				{130831, 434964},				
		};
		
		private Dictionary<uint,int> FixedIndex { get; set; } = new Dictionary<uint,int>  // Default Skill Index. Por defecto se muestran el primer indice con timeleft > 0 , aquí se puede fijar un indice concreto para la habilidad
		{						
			{130738, 3},	// Marcado Para Morir (DH)				
							
			{ 79528, 0},   	// Tolerancia Dolor	(Barb)
			{ 96296, 9},	// Torbellino		(Barb)

			{462255, 3},	// Parasitar (Necro)
				
			{317076, 6},	// Santuario Interior	(Monk)
				
			{375050, 1},	// Mantra Salvación		(Monk)
			{375083, 1},	// Mantra Reprensión	(Monk)
			{373154, 1},	// Mantra Curación		(Monk)
			{375089, 2},	// Mantra Convicción	(Monk)

			{342284, 4},	// Ley Valor		(Crus)
			{342286, 4},	// Ley Justicia 	(Crus)
			{342299, 4},	// Ley Esperanza	(Crus)	
			{325216, 0},	// Justicia 		(Crus) //  o excluir 7
			{286510, 1},	// Fulminar 		(Crus) //  o excluir 14
			{239042, 4},	// Ataque de barrido(Crus) //  o excluir 0
			{266951, 2},	// Escudo Bendito	(Crus) //  o excluir 7				
			{291804, 0},	// Piel Férrea		(Crus) //  o excluir 3		

			//{134872, 2},	// Arconte		(Wiz)
				
			{117402, 4},	// Trance Vudú			(WD) // o excluir 1,3,2
			{108506, 3},	// Rafaga de espiritus	(WD) // o excluir el 1
			{ 67616, 0},	// Cosecha de Almas		(WD) // o excluir el 4
		};
		private List<uint> ExcludeAll { get; set; } = new List<uint>	 // Ignore Skill index (all)
		{
			 93395,		// Rayo de Hielo	(Wiz)  // o excluir 6
			  1765,		// Electrocutar		(Wiz)  // o excluir 1
			 91549,		// Desintegrar		(Wiz)  // o excluir 10
			134456,		// Rafaga			(Wiz)  // o excluir 2,8,9
				
			462089,		//Mago esqueletico (Necro)
				
			 77649,  	// Multishot		(DH)
			129216,		// Abrojos			(DH)
			131325,		// Flecha Elemental	(DH)
			129217,		// Centinela		(DH)
			
			362102, 	// Aliado Místico   (Monk)				
			362118,		// Aliado Místico R (Monk)
			 95940,		// Puños del trueno (Monk)
			121442,		// Vuelo del Dragón (Monk)
				
			266766,		// Martillo Bendito (Crus)
			273941,		// Consagración		(Crus), index 0 y 8  pero no parecen útiles	

			105963,		// Murciélagos		(WD) o excluir index 4
		};		
		private Dictionary<uint,List<int>> ExcludeThis { get; set; } = new Dictionary<uint,List<int>>	// Ignore skill Index (list)	
		{ 							
			{460757, new List<int>{5,1} },		// Devorar 			(Necro)  
			{453563, new List<int>{8} },		// Abosorver sangre (Necro) 
						
			{159169, new List<int>{2} },		// Arrollar			(Barb) ( active index = 0 , 1 ) 
			
			{ 97110, new List<int>{0,2} },		// Senda cien puños	(Monk) ( active index = 1 y 4 )
			{312736, new List<int>{0} },		// Embestida 		(monk) ( active index = 3 ,2 , 4)
			{ 83602, new List<int>{5} },		// Posesión 		(WD)
		};	
		
		private SharpDX.DirectWrite.TextLayout layout { get; set; } = null;	 

		private float SizeIconWidth { get; set; } = 0; 
		private float SizeIconHeight { get; set; } = 0; 
		private bool fixLoad  {get; set; } = true;		
		private IFont FontTimeLeft { get; set; }
		private IFont FontStacks { get; set; }
		private IFont FontCharges { get; set; }
		private IFont FontCooldown { get; set; }
		private Dictionary<HeroClass,bool> HeroEnabled { get; set; }

		public List<uint> ListSkills { get; set; }

        public IKeyEvent ToggleKeyEvent1 { get; set; }  // Show/Hide
		public IKeyEvent ToggleKeyEvent2 { get; set; }  // All/Custom Skill
        private bool ShowBar { get; set; } = true;
		
	
		public float OffsetX { get; set; }
		public float OffsetY { get; set; }
		public float SizeMultiplier  { get; set; }
		public float Opacity { get; set; }
		public bool ShowPassives { get; set; }
		public bool ShowStacks { get; set; }
		public int GreaterThanStacks { get; set; }
		public bool NGStartEnabled { get; set; }
		
        public bool Wizard { get; set; }
        public bool WitchDoctor { get; set; }
        public bool Barbarian { get; set; }
        public bool DemonHunter { get; set; }
        public bool Crusader { get; set; }
        public bool Monk { get; set; }
        public bool Necromancer { get; set; }		
        public bool MyCharacter { get; set; }		
		
        public bool AllSkills { get; set; }
		
        public SkillBarPlugin()
        {
            Enabled = true;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
			Order = 31001;

            ToggleKeyEvent1 = Hud.Input.CreateKeyEvent(true, Key.F8, controlPressed: false, altPressed: true, shiftPressed: false);
			ToggleKeyEvent2 = Hud.Input.CreateKeyEvent(true, Key.F8, controlPressed: true, altPressed: false, shiftPressed: false);
			
			OffsetX = 0.047f;		// 0f..1f  To set the x coordinate of the icon
			OffsetY = 0.01f;		// 0f..1f  To set the y coordinate of the icon				
			SizeMultiplier = 0.65f;	// 0f..1f  Size multiplier for icons
			Opacity = 0.80f;		// 0f..1f  Opacity for icon texture
			ShowStacks = false;		// Show also stacks 
			GreaterThanStacks = 0;	// Recommended values: 0 or 1.  Applicable when ShowStacks = true
			NGStartEnabled = true;	// start enabled/disabled in every New Game
			ShowPassives = true;	// Show the passives below if the mouse is over the last icon
			
            Barbarian = true;		// Show or hide Bar for this character
            Wizard = true;
            WitchDoctor = true;
            Monk = true;
            Crusader = true;
            Necromancer = true;
            DemonHunter = true;			
			MyCharacter = false;	// Show or hide Bar for Me
			
            AllSkills = true;	// Show All Skill or only skills included in ListSkills (consult interfaces\controllers\sno\ISnoPowerList.cs , interfaces\data\player\powers\*)

			ListSkills = new List<uint>
			{
				365311, 302846,				// DH:		Companion, Vengeance
				79528, 79607,				// Barb:	Ignore Pain, Wrath of the Berserker
				134872,						// Wiz:		Archon 
				117402,						// WD:		Big Bad Voodoo
				317076, 96215,				// Monk:	Inner sanctuary, Serenity
				269032,						// Crus:	Akkarat's champion
				465839, 465350 , 460358,	// Necro:	Land of the dead, Simulacrum, Army of the dead	
			};
		}

        public void OnKeyEvent(IKeyEvent keyEvent)
        {
            if (keyEvent.IsPressed)
			{
				if (ToggleKeyEvent1.Matches(keyEvent))  
				{ 
					ShowBar = !ShowBar; 
				}
				else if (ToggleKeyEvent2.Matches(keyEvent))  
				{ 
					if (ShowBar) { AllSkills = !AllSkills; }
				}
			}
        }
        public void OnNewArea(bool newGame, ISnoArea area)
        {
            if (newGame) { ShowBar = NGStartEnabled; }
        }
		
		public void Customize()
		{
			FontTimeLeft  = Hud.Render.CreateFont("Arial", 9 * SizeMultiplier, 255, 0, 255, 0, true, false, 255, 0, 0, 0, true);	
			FontStacks = Hud.Render.CreateFont("Arial", 8 * SizeMultiplier, 255, 255, 255, 0, true, false, 255, 0, 0, 0, true);	
			FontCharges = Hud.Render.CreateFont("Arial", 8 * SizeMultiplier, 255, 200, 200, 200, true, false, 255, 0, 0, 0, true);	
			FontCooldown = Hud.Render.CreateFont("Arial", 12f * SizeMultiplier, 255, 255, 255, 255, true, false, 255, 0, 0, 0, true);
			

			HeroEnabled = new Dictionary<HeroClass,bool> 
			{
				{ HeroClass.DemonHunter,	DemonHunter },
				{ HeroClass.Barbarian,		Barbarian },
				{ HeroClass.Wizard,			Wizard },
				{ HeroClass.WitchDoctor,	WitchDoctor },
				{ HeroClass.Monk,			Monk },				
				{ HeroClass.Crusader,		Crusader },
				{ HeroClass.Necromancer,	Necromancer },
				{ HeroClass.None,			MyCharacter },
			};
		}
		
        public void PaintTopInGame(ClipState clipState)
        {
            if (Hud.Render.UiHidden)   return;
            if ((Hud.Game.MapMode == MapMode.WaypointMap) || (Hud.Game.MapMode == MapMode.ActMap) || (Hud.Game.MapMode == MapMode.Map))  return;
			if (!ShowBar || !Hud.Game.IsInGame) return;
			if (fixLoad)
			{
				SizeIconWidth = Hud.Texture.BuffFrameTexture.Width   * Hud.Window.Size.Height/1200.0f * SizeMultiplier;
				SizeIconHeight = Hud.Texture.BuffFrameTexture.Height * Hud.Window.Size.Height/1200.0f * SizeMultiplier;
				fixLoad = false;
			}			
			var players = Hud.Game.Players.Where( p => HeroEnabled[ p.IsMe? HeroClass.None : p.HeroClassDefinition.HeroClass ] );
			foreach(var player in players)
			{
				var x = player.PortraitUiElement.Rectangle.X + Hud.Window.Size.Width * OffsetX;
				var y = player.PortraitUiElement.Rectangle.Y + Hud.Window.Size.Height * OffsetY;
				
				var skills = AllSkills? player.Powers.CurrentSkills.OrderBy(s => s.Key.ToString().Replace("Skill", "")) : player.Powers.UsedSkills.Where(s => ListSkills.Contains(s.SnoPower.Sno));

				var sc = skills.Count();
				foreach (var skill in skills)
				{
					if (clipState == ClipState.BeforeClip) 
					{
						ISnoPower sSnoPower = AllSkills? skill.CurrentSnoPower : skill.SnoPower;					
						Hud.Texture.GetTexture(sSnoPower.NormalIconTextureId).Draw(x, y, SizeIconWidth, SizeIconHeight, Opacity);
						if (skill.CooldownFinishTick > Hud.Game.CurrentGameTick) 
						{
							double remaining = (skill.CooldownFinishTick - Hud.Game.CurrentGameTick)/60.0d;						
							layout = FontCooldown.GetTextLayout( remaining.ToString( (remaining < 1)? "F1":"F0" ) );
							FontCooldown.DrawText(layout, x + ((SizeIconWidth - (float)Math.Ceiling(layout.Metrics.Width))/2.0f), y + ((SizeIconHeight - (float)Math.Ceiling(layout.Metrics.Height))/1.14f) );
						}
						if (skill.Charges > 0) 
						{ 
							layout = FontCharges.GetTextLayout( skill.Charges.ToString());
							FontCharges.DrawText(layout, x + ((SizeIconWidth - (float)Math.Ceiling(layout.Metrics.Width))/7f), y + ((SizeIconHeight - (float)Math.Ceiling(layout.Metrics.Height))/1.10f));
						}					
						if (player.HasValidActor) 						
						{
							Hud.Texture.BuffFrameTexture.Draw(x, y, SizeIconWidth, SizeIconHeight, Opacity);
							IBuff buff = null ;
							if (ReplaceSno.TryGetValue(sSnoPower.Sno, out var sno))
							{
								buff = player.Powers.AllBuffs.FirstOrDefault(b => b.SnoPower.Sno == sno );			
							}
							else { buff = Hud.Game.Me.Powers.AllBuffs.FirstOrDefault(b => b.SnoPower.Sno == sSnoPower.Sno ); } 		//else { buff = skill.Buff; }
							if ( (buff != null) && (buff.Active) )
							{
								if (!FixedIndex.TryGetValue(buff.SnoPower.Sno, out var index)) 
								{ 
									index = -1;
									if (!ExcludeAll.Contains(buff.SnoPower.Sno))  
									{		
										bool skip = ExcludeThis.ContainsKey(buff.SnoPower.Sno) ? true:false;
										for (int i = 0; i < buff.TimeLeftSeconds.Length ;i++) 
										{
											if (buff.TimeLeftSeconds[i] > 0) 
											{
												if (skip && ExcludeThis[buff.SnoPower.Sno].Contains(i)) continue;				
												index = i;		
												break;		
											}
										}
									}						
								}
								if (index != -1)
								{
									if ( ShowStacks && (buff.IconCounts[index] > GreaterThanStacks) )
									{
										layout = FontStacks.GetTextLayout(buff.IconCounts[index].ToString());
										FontStacks.DrawText(layout, x + ((SizeIconWidth - (float)Math.Ceiling(layout.Metrics.Width))/6f), y + ((SizeIconHeight - (float)Math.Ceiling(layout.Metrics.Height))/10.0f));
									}
									double timeleft = buff.TimeLeftSeconds[index];								
									if (timeleft > 0)
									{  
											
										if (timeleft < 1.0f) 
										{ 
											layout = FontTimeLeft.GetTextLayout(String.Format("{0:N1}", timeleft)); 
										}
										else 
										{
											timeleft =  (int) (timeleft + 0.80);  // Redondeará a X si es menor  a X.20
											layout = FontTimeLeft.GetTextLayout( (ShowStacks || timeleft<60)? String.Format("{0:0}", timeleft):String.Format("{0:0}:{1:00}",(int)(timeleft/60), timeleft%60));
										}
										FontTimeLeft.DrawText(layout, x + ((SizeIconWidth - (float)Math.Ceiling(layout.Metrics.Width))/(ShowStacks?1.25f:2.0f)), y + ((SizeIconHeight - (float)Math.Ceiling(layout.Metrics.Height))/7.0f) );	
									}
								}
							}
						}
						else { Hud.Texture.DebuffFrameTexture.Draw(x, y, SizeIconWidth, SizeIconHeight, Opacity); }											
						if (Hud.Window.CursorInsideRect(x, y, SizeIconWidth, SizeIconHeight)) 
						{ 
							Hud.Render.SetHint(sSnoPower.NameLocalized + "\n" + skill.RuneNameLocalized); 
							if ( ShowPassives && (sc == 1) /*&& Hud.Window.CursorInsideRect(x, y, 5, 5)*/) 
							{
								var k = 1;	var xp = player.PortraitUiElement.Rectangle.X + Hud.Window.Size.Width * OffsetX; var yp = y + SizeIconHeight;
								foreach (var power in player.Powers.UsedPassives)
								{
									if (power == null) continue;
									Hud.Texture.GetTexture(power.NormalIconTextureId).Draw(xp, yp, SizeIconWidth, SizeIconHeight, 1.0f);								
									// if (++k > 5) break;	// bug antiguo que ya se corrigió
									xp += SizeIconWidth;
								}
							}
						}												
					}
					else if (clipState == ClipState.Inventory)	
					{
						if (Hud.Window.CursorInsideRect(x, y, SizeIconWidth, SizeIconHeight)) { Hud.Render.SetHint(null); }
					}						
					x += SizeIconWidth;	sc--;			
				}
			}
        }
    }
}