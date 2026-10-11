using Turbo.Plugins.Default;
using System;
using System.Linq;
using System.Threading;

namespace Turbo.Plugins.RNN
{
    public class SimulacrumsAlertIcon : BasePlugin, IInGameTopPainter, ICustomizer
    {
		private SharpDX.DirectWrite.TextLayout layout { get; set; } = null;
		private MapTextureDecorator SimulacrumMapTexture { get; set; }

		private IBrush SimulacrumBrushDefault { get; set; } = null;
		private IBrush SimulacrumBrushMe { get; set; }
		private IBrush SimulacrumBrushOther { get; set; }
		private ITexture SimulacrumTexture { get; set; } = null;
		private ITexture SimulacrumTextureMe { get; set; }
		private ITexture SimulacrumTextureOther { get; set; }
		private float SizeGroundTexture {get; set; }
		public float SizeGroundTextureMultiplier {get; set; } = 0.45f;

		private IFont FontWhite { get; set; }
		private IFont FontStacks0 { get; set; }
		private IFont FontStacks1 { get; set; }
		private IFont SimulacrumFont { get; set; }

		private float SizeIcon  { get; set; }
		private bool fixLoad  {get; set; } = true;
		private bool AreAlive { get; set; } = false;

		public bool ShowSimulacrums { get; set; }
		public bool OnlyGR { get; set; }
		public bool ShowInTown { get; set; }
		public bool ShowIcon { get; set; }
		public float Xpor  { get; set; }
		public float Ypor  { get; set; }
		public float SizeIconMultiplier {get; set; }
		public float Opacity { get; set; }
		public bool SoundEnabled { get; set; }
		public string FileSound { get; set; }
		public bool HideOnMinimap { get; set; }
		public bool ShowCounterSkillUi { get; set; }

		public SimulacrumsAlertIcon()
		{
			Enabled = true;
		}

        public override void Load(IController hud)
        {
			base.Load(hud);
			Order = 30001;

			OnlyGR = false;				// Show in GreaterRift only
			ShowInTown = false;			// Is independent of the value of OnlyGR. For Testing
			ShowSimulacrums = true;		// Show Simulacrums in Map and Ground
			HideOnMinimap = false;		// Hide or not the simulacrums on the minimap. Will be ignored if ShowSimulacrums = false
			ShowIcon = true;			// Display an icon for your character when the number of Simulacrums is less than 2
			ShowCounterSkillUi = true;
			Xpor = 0.51f;				// To set the x coordinate of the icon
			Ypor = 0.34f;				// To set the y coordinate of the icon
			SizeIconMultiplier = 0.55f;	// Size icon Simulacrum
			Opacity = 0.75f;			// 0f..1f  Opacity for icon
			SoundEnabled = false;		// Play sound when you lose all simulacrum
			FileSound = "notification_8.wav";  // File to be played. It must be in the Sounds\ folder

			SimulacrumTextureMe = Hud.Texture.GetTexture(124715741);
			SimulacrumTextureOther = Hud.Texture.GetTexture(670858621);
			SimulacrumBrushMe = Hud.Render.CreateBrush(255, 255, 0, 0, 3f);
			SimulacrumBrushOther = Hud.Render.CreateBrush(255, 0, 255, 0, 3f);
			SimulacrumFont = Hud.Render.CreateFont("tahoma", 8, 220, 0, 255, 0, false, false, true);

			SimulacrumMapTexture = new MapTextureDecorator(Hud)
			{
				Texture = SimulacrumTextureOther,
				Radius = 0.35f,
				RadiusTransformator = new StandardPingRadiusTransformator(Hud, 700) {  RadiusMinimumMultiplier = 0.75f, }
			};
        }

		public void Customize()
		{
			FontWhite = Hud.Render.CreateFont("tahoma", 11f * SizeIconMultiplier, 255, 255, 255, 255, true, false, 255, 0, 0, 0, true);
			FontStacks0 = Hud.Render.CreateFont("tahoma", 11f * SizeIconMultiplier, 255, 255, 0, 0, true, false, 255, 0, 0, 0, true);
			FontStacks1 = Hud.Render.CreateFont("tahoma", 11f * SizeIconMultiplier, 255, 255, 255, 0, true, false, 255, 0, 0, 0, true);
		}

        public void Play_Sound(string Sonido)
        {
			var playSonido = Hud.Sound.LoadSoundPlayer(Sonido);
			ThreadPool.QueueUserWorkItem(state =>
			{
				try  { playSonido.PlaySync(); }
				catch (Exception)  {   }
			}	);
		}

		public void DrawIcon(float x, float y, float width, float height, IPlayerSkill skill, int num = 0)
		{
			Hud.Texture.GetTexture(skill.SnoPower.NormalIconTextureId).Draw(x, y, width, height, Opacity);
			Hud.Texture.BuffFrameTexture.Draw(x, y, width, height, Opacity);
			if (skill.CooldownFinishTick > Hud.Game.CurrentGameTick)
			{
				double remaining = (skill.CooldownFinishTick - Hud.Game.CurrentGameTick) / 60.0d;
				layout = FontWhite.GetTextLayout( remaining.ToString((remaining < 1)? "F1":"F0") );
			}
			else { layout = FontWhite.GetTextLayout("✔️"); }
			FontWhite.DrawText(layout, x + ((width - (float)Math.Ceiling(layout.Metrics.Width))/4.0f), y + ((height - (float)Math.Ceiling(layout.Metrics.Height))/6.0f));
			layout = FontStacks0.GetTextLayout(num.ToString());
			(num==0?FontStacks0:FontStacks1).DrawText(layout, x + ((SizeIcon - (float)Math.Ceiling(layout.Metrics.Width))/1.15f), y + ((SizeIcon - (float)Math.Ceiling(layout.Metrics.Height))/1.15f));
		}

		public void PaintTopInGame(ClipState clipState)
		{
			if (clipState != ClipState.BeforeClip) return;
			if (!Hud.Game.IsInGame) return;
			if (Hud.Game.Me.IsInTown)
			{
				if (!ShowInTown) return;
			}
			else if (OnlyGR && (Hud.Game.Me.InGreaterRiftRank == 0))  return;

			if (fixLoad)
			{
				SizeGroundTexture = SimulacrumTextureOther.Width * Hud.Window.Size.Height/1200.0f * SizeGroundTextureMultiplier;
				SizeIcon = Hud.Texture.BuffFrameTexture.Width   * Hud.Window.Size.Height/1200.0f * SizeIconMultiplier;
				fixLoad = false;
			}

			var n = 0;
			var actors = Hud.Game.Actors.Where(a => a.SnoActor.Sno == ActorSnoEnum._p6_necro_simulacrum_a || a.SnoActor.Sno == ActorSnoEnum._p6_necro_simulacrum_a_set ); // 467053 , 484304
			if (ShowSimulacrums)
			{
				foreach (var actor in actors)
				{
					if (actor.SummonerAcdDynamicId == Hud.Game.Me.SummonerId)
					{
						n++;
						SimulacrumTexture = SimulacrumTextureMe; SimulacrumBrushDefault = SimulacrumBrushMe;
					}
					else
					{
						SimulacrumTexture = SimulacrumTextureOther; SimulacrumBrushDefault = SimulacrumBrushOther;
					}
					SimulacrumBrushDefault.DrawWorldEllipse(0.80f, -1, actor.FloorCoordinate); // Ground Circle
					SimulacrumTexture.Draw(actor.FloorCoordinate.ToScreenCoordinate().X - SizeGroundTexture/2, actor.FloorCoordinate.ToScreenCoordinate().Y - SizeGroundTexture/2, SizeGroundTexture, SizeGroundTexture, 1f); // Ground texture
					if (!HideOnMinimap)
					{
						SimulacrumMapTexture.Texture = SimulacrumTexture;
						SimulacrumMapTexture.Paint(actor, actor.FloorCoordinate, string.Empty); // Map texture
					}
				}
			}
			else { n = actors.Where(a => a.SummonerAcdDynamicId == Hud.Game.Me.SummonerId).Count(); }

			IPlayerSkill skill = Hud.Game.Me.Powers.UsedNecromancerPowers.Simulacrum;  // 465350 - Simulacrum
			if (skill != null)
			{
				if (ShowCounterSkillUi)
				{
					var ui = Hud.Render.GetPlayerSkillUiElement(skill.Key);
					if (ui != null) SimulacrumFont.DrawText(SimulacrumFont.GetTextLayout(n.ToString()), ui.Rectangle.X + 7, ui.Rectangle.Y + 3);
				}
				if (Hud.Game.Me.Powers.BuffIsActive(484309))	// Amuleto
				{
					var i = ((skill.Rune == 3) || Hud.Game.Me.Powers.BuffIsActive(484301)) ? 2 : 1; // New Set Necromancer , bonus 2 items
					if (n == i)
					{
						if (!AreAlive)	{ AreAlive = true; }
					}
					else if (n < i)
					{
						if (ShowIcon) DrawIcon(Hud.Window.Size.Width * Xpor, Hud.Window.Size.Height * Ypor, SizeIcon, SizeIcon, skill, n);
						if (AreAlive)
						{
							if (!Hud.Game.Me.Powers.BuffIsActive(465350))
							{
								AreAlive = false;
								if (SoundEnabled && !Hud.Game.Me.IsInTown) Play_Sound(FileSound);
							}
						}
					}
					else // (n > i) , no debería llegar nunca aquí , añadido por precaución
					{
						if (ShowIcon) DrawIcon(Hud.Window.Size.Width * Xpor, Hud.Window.Size.Height * Ypor, SizeIcon, SizeIcon, skill, n);
						if (!AreAlive)	{ AreAlive = true; }
					}
				}
			}
        }
    }
}