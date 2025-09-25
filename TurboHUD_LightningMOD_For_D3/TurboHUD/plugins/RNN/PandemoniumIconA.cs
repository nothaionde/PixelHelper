using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Threading;
using SharpDX.DirectInput;
using Turbo.Plugins.Default;

namespace Turbo.Plugins.RNN
{
    public class PandemoniumIconA : BasePlugin, IInGameTopPainter, ICustomizer, INewAreaHandler, IKeyEventHandler
    {
        public IKeyEvent ToogleKeyEventOn { get; set; }  // Show/Hide.
        public IKeyEvent ToggleKeyEventCircle { get; set; }
        private MapShapeDecorator YardMapShapeDecorator;

        public bool TurnOffWhenNewGameStarts { get; set; }
        private bool PI_On { get; set; } = true;

        private ITexture TextureBG { get; set; } = null;
        private ITexture TextureRS { get; set; } = null;

        private IFont TimeleftFont { get; set; }
        private IFont StackFont { get; set; }
        private IFont StackFontOther { get; set; }
        private IFont NextFont { get; set; }
        private IFont NextFontY { get; set; }
        private IFont NextFontN { get; set; }
        private IFont MonsterCounterFont { get; set; }
        private IFont YardFont { get; set; }

        private int MyIndex { get; set; } = -1;
        private bool Buffon { get; set; } = false;
        private int NewTarget { get; set; } = 0;
        private int Kills { get; set; } = 0;
        private int YardsTarget { get; set; } = 0;

        private readonly Dictionary<int, string> NextBuff = new Dictionary<int, string>()
        {
             {15,"15"}, {30,"30"},  {50,"50"},  {100,"100"}, {150,"150"}, {200,"200"},  {300,"300"},    {400,"400"},    {500,"500"},    {1000,"1000"} , {int.MaxValue,"∞"}
        };

        public float Xpor { get; set; }
        public float Ypor { get; set; }
        public float SizeMultiplier { get; set; }

        public bool ShowIcon { get; set; }
        public bool SoundLost { get; set; }
        public bool SoundTarget { get; set; }
        public string FileSoundLost { get; set; }
        public string FileSoundTarget { get; set; }
        public int MinKillsLost { get; set; }
        public int MinTarget { get; set; }
        public int YardsMinTarget { get; set; }
        public int YardsMaxTarget { get; set; }
        public bool ShowCircle { get; set; }
        public bool ShowMonsterCounter { get; set; }
        public bool ShowBuffOthers { get; set; } = false;

        private SoundPlayer SoundPlayerLost;
        private SoundPlayer SoundPlayerTarget;

        public PandemoniumIconA()
        {
            Enabled = false;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            Order = 30002;

            Xpor = 0.45f;   // Valid values: from 0 to 1 . To set the x coordinate of the icon
            Ypor = 0.45f;   // Valid values: from 0 to 1 . To set the y coordinate of the icon
            SizeMultiplier = 1f; // Size multiplier for icons

            SoundLost = false;      //Notify with a sound when the buff is lost
            SoundTarget = false;    //Notify with a sound when a target (15,30,50,100,150,200,300,400,500,1000) was reached
            FileSoundTarget = "PandemoniumTarget.wav";  // File to be played. It must be in the Sounds\ folder
            FileSoundLost = "PandemoniumLost.wav";  // File to be played. It must be in the Sounds\ folder
            MinTarget = 100;        // The minimum target where you will begin to notify
            MinKillsLost = 100;     // Minimum kills required to notify Buff was lost
            ShowIcon = true;        // Show/Hide icon. Maybe someone just wants sound
            YardsMinTarget = 40;    // Green text when the next target can be achieved by killing the monsters that are at this distance. Control key to switch between YardsMax and YardsMin
            YardsMaxTarget = 120;   // Green text when the next target can be achieved by killing the monsters that are at this distance. Control key to switch between YardsMax and YardsMin
            ShowCircle = true;      // Show circle on the Map, radio YardsMinTarget/YardsMaxTarget.  Control key to switch between YardsMin and YardsMax
            ShowMonsterCounter = false;  // Show Monster Counter
            TurnOffWhenNewGameStarts = false;

            ToggleKeyEventCircle = Hud.Input.CreateKeyEvent(true, Key.R, false, true, false);
            ToogleKeyEventOn = Hud.Input.CreateKeyEvent(true, Key.F8, controlPressed: true, altPressed: false, shiftPressed: false);

            TextureBG = Hud.Texture.BuffFrameTexture;
            TextureRS = Hud.Texture.GetItemTexture(Hud.Sno.SnoItems.P4_Unique_Spear_002);

            YardMapShapeDecorator = new MapShapeDecorator(Hud)
            {
                Brush = Hud.Render.CreateBrush(150, 180, 147, 109, 1),
                ShapePainter = new CircleShapePainter(Hud),
                Radius = 40,
            };
        }

        public void Customize()
        {
            TimeleftFont = Hud.Render.CreateFont("tahoma", 6f * SizeMultiplier, 255, 255, 255, 255, true, false, 255, 0, 0, 0, true);
            StackFont = Hud.Render.CreateFont("tahoma", 8f * SizeMultiplier, 255, 255, 220, 0, true, false, 160, 0, 0, 0, true);
            StackFontOther = Hud.Render.CreateFont("tahoma", 8f * SizeMultiplier, 255, 255, 150, 0, true, false, 160, 0, 0, 0, true);
            NextFont = Hud.Render.CreateFont("tahoma", 4f * SizeMultiplier, 255, 225, 170, 0, true, false, 160, 0, 0, 0, true);
            NextFontY = Hud.Render.CreateFont("tahoma", 4f * SizeMultiplier, 255, 0, 204, 102, true, false, 160, 0, 0, 0, true);
            NextFontN = Hud.Render.CreateFont("tahoma", 4f * SizeMultiplier, 255, 0, 150, 0, true, false, 160, 0, 0, 0, true);
            YardFont = Hud.Render.CreateFont("tahoma", 4f * SizeMultiplier, 255, 200, 200, 200, true, false, 160, 0, 0, 0, true);
            MonsterCounterFont = Hud.Render.CreateFont("tahoma", 4.8f * SizeMultiplier, 255, 200, 200, 200, true, false, 160, 0, 0, 0, true);

            YardsTarget = YardsMinTarget;
            YardMapShapeDecorator.Radius = YardsTarget;
            if (MinTarget < 15)
            { MinTarget = 15; }
        }

        public void OnKeyEvent(IKeyEvent keyEvent)
        {
            if (keyEvent.IsPressed)
            {
                if (ToggleKeyEventCircle.Matches(keyEvent))
                {
                    YardsTarget = (YardsTarget != YardsMaxTarget) ? YardsMaxTarget : YardsMinTarget;
                    YardMapShapeDecorator.Radius = YardsTarget;
                }
                else if (ToogleKeyEventOn.Matches(keyEvent))
                {
                    PI_On = !PI_On;
                    Buffon = false;
                }
            }
        }

        public void OnNewArea(bool newGame, ISnoArea area)
        {
            if (newGame || (MyIndex != Hud.Game.Me.Index))
            {
                MyIndex = Hud.Game.Me.Index;
                Buffon = false;
                if (TurnOffWhenNewGameStarts)
                {
                    PI_On = false;
                }
            }
        }

        public void PaintTopInGame(ClipState clipState)
        {
            if (clipState != ClipState.BeforeClip)
                return;
            if (!Hud.Game.IsInGame)
                return;

            if (PI_On && Hud.Game.Me.Powers.BuffIsActive(483967))  // Jugando en temporada
            {
                var playerc = Hud.Game.Players
                    .Where(p => p.HasValidActor && (p.Powers.GetBuff(483967)?.IconCounts[1] > 0))
                    .OrderByDescending(o => o.Powers.GetBuff(483967).IconCounts[1])
                    .FirstOrDefault();

                if (playerc != null)
                {
                    var iconCount = playerc.Powers.GetBuff(483967).IconCounts[1];
                    var iconCountMe = Hud.Game.Me.Powers.GetBuff(483967).IconCounts[1];
                    if (!Buffon)
                    {
                        Buffon = true;
                        NewTarget = 0;
                    }

                    foreach (var d in NextBuff)
                    {
                        if (d.Key > iconCount)
                        {
                            if (d.Key != NewTarget)
                            {
                                if (SoundTarget && iconCount >= MinTarget && (NewTarget > 0) && (d.Key > NewTarget) && (iconCountMe == iconCount))
                                    PlaySoundTarget();

                                NewTarget = d.Key;
                            }
                            break;
                        }
                    }

                    if (Kills != iconCount)
                        Kills = iconCount;

                    if (ShowIcon)
                    {
                        var x = Hud.Window.Size.Width * Xpor;
                        var y = Hud.Window.Size.Height * Ypor;
                        var iconWidth = TextureBG.Width * 0.60f * SizeMultiplier;
                        var iconHeight = TextureBG.Height * 0.68f * SizeMultiplier;
                        Hud.Texture.Button2TextureOrange.Draw(x, y, iconWidth, iconHeight, 0.7f);
                        Hud.Texture.BackgroundTextureYellow.Draw(x, y, iconWidth, iconHeight, 1f);
                        TextureRS.Draw(x, y, iconWidth, iconHeight, 1f);

                        if (iconCountMe == 0)
                            Hud.Texture.DebuffFrameTexture.Draw(x, y, iconWidth, iconHeight, 1f);
                        else
                            TextureBG.Draw(x, y, iconWidth, iconHeight, 1f);

                        var timeLeft = playerc.Powers.GetBuff(483967).TimeLeftSeconds[1];
                        var layout = TimeleftFont.GetTextLayout(timeLeft.ToString((timeLeft < 1) ? "F1" : "F0"));
                        TimeleftFont.DrawText(layout, x + ((iconWidth - (float)Math.Ceiling(layout.Metrics.Width)) / 2.0f), y + ((iconHeight - (float)Math.Ceiling(layout.Metrics.Height)) / 2.8f));

                        var font = (iconCount > iconCountMe) ? StackFontOther : StackFont;
                        layout = font.GetTextLayout(iconCount.ToString());
                        font.DrawText(layout, x + (iconWidth - (float)Math.Ceiling(layout.Metrics.Width)) - 2, y + (iconHeight - (float)Math.Ceiling(layout.Metrics.Height)) - 2);

                        var monsters = Hud.Game.AliveMonsters
                            .Where(m => m.FloorCoordinate.XYDistanceTo(Hud.Game.Me.FloorCoordinate) <= YardsTarget);

                        var totalMonsterCount = monsters.Count();
                        if (NewTarget != 0)
                        {
                            var normalMonsterCount = monsters
                                .Count(m => m.Rarity == ActorRarity.Normal);

                            font = (iconCount + totalMonsterCount) < NewTarget
                                ? NextFont
                                : (iconCount + normalMonsterCount) < NewTarget
                                    ? NextFontN
                                    : NextFontY;

                            layout = NextFont.GetTextLayout(NextBuff[NewTarget]);
                            font.DrawText(layout, x + 3, y + 2);
                        }

                        if (ShowMonsterCounter)
                        {
                            layout = MonsterCounterFont.GetTextLayout(totalMonsterCount.ToString());
                            MonsterCounterFont.DrawText(layout, x + iconWidth - (float)Math.Ceiling(layout.Metrics.Width) - 3, y + 2);
                        }
                        else
                        {
                            layout = YardFont.GetTextLayout((YardsTarget == YardsMaxTarget) ? "+" : "-");
                            YardFont.DrawText(layout, x + iconWidth - (float)Math.Ceiling(layout.Metrics.Width) - 2, y + 2);
                        }
                    }

                    if (ShowCircle)
                        YardMapShapeDecorator.Paint(null, Hud.Game.Me.FloorCoordinate, null);
                }
                else if (Buffon)
                {
                    Buffon = false;
                    if (SoundLost && (Kills >= MinKillsLost) && !Hud.Game.IsInTown)
                        PlaySoundLost();
                }
            }
        }

        public void PlaySoundLost()
        {
            if (SoundPlayerLost == null)
                SoundPlayerLost = Hud.Sound.LoadSoundPlayer(FileSoundLost);

            ThreadPool.QueueUserWorkItem(state =>
            {
                try
                {
                    SoundPlayerLost.PlaySync();
                }
                catch (Exception) { }
            });
        }

        public void PlaySoundTarget()
        {
            if (SoundPlayerTarget == null)
                SoundPlayerTarget = Hud.Sound.LoadSoundPlayer(FileSoundTarget);

            ThreadPool.QueueUserWorkItem(state =>
            {
                try
                {
                    SoundPlayerTarget.PlaySync();
                }
                catch (Exception) { }
            });
        }
    }
}