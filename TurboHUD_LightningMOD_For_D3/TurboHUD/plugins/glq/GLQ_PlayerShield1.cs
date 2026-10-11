using Turbo.Plugins.Default;
using System.Globalization;
namespace Turbo.Plugins.glq
{
    public class GLQ_PlayerShield1 : BasePlugin, IInGameTopPainter
    {

        public TopLabelDecorator PlayerShieldCurDecorator { get; set; }
        public TopLabelDecorator PlayerShieldMaxDecorator { get; set; }
        public TopLabelDecorator MeShieldMaxDecorator { get; set; }
        public TopLabelDecorator MeShieldCurDecorator { get; set; }
        public bool OnHealthBallEnabled { get; set; }
        public bool OnHealthBarEnabled { get; set; }
        public GLQ_PlayerShield1()
        {
            Enabled = true;
            OnHealthBallEnabled = true;
            OnHealthBarEnabled = true;
            Order = 100;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);

            MeShieldMaxDecorator = new TopLabelDecorator(Hud)
            {
                BackgroundBrush = Hud.Render.CreateBrush(0, 255, 255, 255, 0),
                BorderBrush = Hud.Render.CreateBrush(255, 0, 0, 0, -1),
                TextFont = Hud.Render.CreateFont("tahoma", 6, 255, 255, 255, 255, false, false, true),
            };
            MeShieldCurDecorator = new TopLabelDecorator(Hud)
            {
                BackgroundBrush = Hud.Render.CreateBrush(70, 0, 220, 220, 0),
                BorderBrush = Hud.Render.CreateBrush(255, 0, 0, 0, -1),
                TextFont = Hud.Render.CreateFont("tahoma", 6, 255, 255, 255, 255, false, false, true),
            };
            PlayerShieldMaxDecorator = new TopLabelDecorator(Hud)
            {
                TextFont = Hud.Render.CreateFont("tahoma", 6, 255, 255, 255, 255, false, false, true),
            };
            PlayerShieldCurDecorator = new TopLabelDecorator(Hud)
            {
                BackgroundBrush = Hud.Render.CreateBrush(100, 0, 255, 255, 0),
                BorderBrush = Hud.Render.CreateBrush(255, 0, 0, 0, 0.3f),
                TextFont = Hud.Render.CreateFont("tahoma", 6, 255, 255, 255, 255, false, false, true),
            };
        }

        public void PaintTopInGame(ClipState clipState)
        {
            if (Hud.Render.UiHidden) return;
            if (clipState != ClipState.BeforeClip) return;
            if(OnHealthBarEnabled)
            {
                foreach (var player in Hud.Game.Players)
                {
                    var pct = Hud.Game.Me.Defense.CurShield / Hud.Game.Me.Defense.HealthMax;
                    if (pct > 1) pct = 1;
                    var portrait = player.PortraitUiElement.Rectangle;
                    var w_max = portrait.Width * 0.67f;
                    var w_cur = portrait.Width * 0.67f * pct;
                    var h = portrait.Height * 0.05f;
                    var x = portrait.Left + portrait.Width * 0.16f;
                    var y = portrait.Top + portrait.Height * 0.725f;
                    PlayerShieldMaxDecorator.HintFunc = () => "Здоровье：" + Hud.Game.Me.Defense.HealthCur.ToString("#,0", CultureInfo.InvariantCulture) + " / " +Hud.Game.Me.Defense.HealthMax.ToString("#,0", CultureInfo.InvariantCulture) + " ("+ Hud.Game.Me.Defense.HealthPct.ToString("#,0", CultureInfo.InvariantCulture) + "%" + ")" + "\n" + "Щит：" + Hud.Game.Me.Defense.CurShield.ToString("#,0", CultureInfo.InvariantCulture);
                    PlayerShieldMaxDecorator.Paint(x, y, w_max, h, HorizontalAlign.Center);
                    if (pct == 0) continue;
                    PlayerShieldCurDecorator.HintFunc = () => "Щит：" + Hud.Game.Me.Defense.CurShield.ToString("#,0", CultureInfo.InvariantCulture);
                    PlayerShieldCurDecorator.Paint(x, y, w_cur, h, HorizontalAlign.Center);
                }
            }
            if (OnHealthBallEnabled)
            {
                var pct = Hud.Game.Me.Defense.CurShield / Hud.Game.Me.Defense.HealthMax;
                if (pct > 1) pct = 1;
                if (pct == 0) return;
                var uiRect = Hud.Render.GetUiElement("Root.NormalLayer.game_dialog_backgroundScreenPC.game_progressBar_healthBall").Rectangle;
                var w = uiRect.Width * 0.05f;
                var h_max = uiRect.Height;
                var h_cur = uiRect.Height * pct;
                var x = uiRect.Left + uiRect.Width / 2;
                var y_max = uiRect.Top;
                var y_cur = uiRect.Top + uiRect.Height * (1 - pct);
                MeShieldMaxDecorator.HintFunc = () => "Щит：" + Hud.Game.Me.Defense.CurShield.ToString("#,0", CultureInfo.InvariantCulture);
                MeShieldCurDecorator.HintFunc = () => "Щит：" + Hud.Game.Me.Defense.CurShield.ToString("#,0", CultureInfo.InvariantCulture);
                MeShieldMaxDecorator.Paint(x + w * 0, y_max, w, h_max, HorizontalAlign.Center);
                MeShieldCurDecorator.Paint(x + w * 0, y_cur, w, h_cur, HorizontalAlign.Center);
            }

        }

    }

}