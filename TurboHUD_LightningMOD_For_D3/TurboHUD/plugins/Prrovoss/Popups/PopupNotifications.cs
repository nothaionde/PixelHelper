using System;
using System.Linq;
using System.Text;

using Turbo.Plugins.Default;

namespace Turbo.Plugins.Prrovoss.Popups
{
	// @modified
    public class PopupNotifications : BasePlugin, IInGameWorldPainter
    {        
        public float RatioX { get; set; }
        public float RatioY { get; set; }
        public float RatioW { get; set; }
        public float RatioH { get; set; }
        public float VerticalGap { get; set; }

        public class Popup : IQueueItem
        {
            public string Text { get; set; }
            public string Title { get; set; }
            public string Hint { get; set; }
            public DateTime QueuedOn { get; private set; }
            public TimeSpan LifeTime { get; private set; }
            public TopLabelWithTitleDecorator Decorator { get; set; }

            // @modified
            public SharpDX.DirectWrite.TextMetrics TextMetrics { get; set; }
            public SharpDX.DirectWrite.TextMetrics TitleMetrics { get; set; }
            public float MaxWidth { get; set; }
			
            // @modified
            public Popup(string text, string title, TimeSpan lifetime, string hint, IController hud, TopLabelWithTitleDecorator decorator = null)
            {
                this.Text = text;
                this.Title = title;
                this.LifeTime = lifetime;
                this.Hint = hint;
                this.QueuedOn = DateTime.Now;

                if (decorator == null) {
                    this.Decorator = new TopLabelWithTitleDecorator(hud)
                    {
                        BorderBrush = hud.Render.CreateBrush(255, 180, 147, 109, -1),
                        BackgroundBrush = hud.Render.CreateBrush(200, 0, 0, 0, 0),
                        //TextFont = hud.Render.CreateFont("tahoma", 8, 255, 255, 255, 255, true, false, false),
                        TextFont = hud.Render.CreateFont("tahoma", 6, 255, 255, 255, 255, true, false, false),
                        TitleFont = hud.Render.CreateFont("tahoma", 6, 255, 180, 147, 109, true, false, false),
                    };
                } else {
                    this.Decorator = decorator;
                }

				this.TextMetrics = this.Decorator.TextFont.GetTextLayout(text).Metrics;
				this.TitleMetrics = this.Decorator.TextFont.GetTextLayout(title).Metrics;
				this.MaxWidth = (this.TextMetrics.Width > this.TitleMetrics.Width) ? this.TextMetrics.Width : this.TitleMetrics.Width;
            }
        }


        public void Show(string text, string title, int duration, string hint = null, TopLabelWithTitleDecorator decorator = null)
        {
            Hud.Queue.AddItem(new Popup(text, title, new TimeSpan(0, 0, 0, 0, duration), hint, Hud, decorator));
        }

        public PopupNotifications()
        {
            Enabled = true;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);

            RatioX = 0.6f;
            RatioY = 0.75f;
            RatioW = 0.18f;
            RatioH = 0.05f;
            VerticalGap = 1.20f;
        }

        /*public void PaintWorld(WorldLayer layer)
        {
            var w = Hud.Window.Size.Height * RatioW;
            var h = Hud.Window.Size.Height * RatioH;
            var x = Hud.Window.Size.Width * RatioX;
            var y = Hud.Window.Size.Height * RatioY;

            foreach (Popup p in Hud.Queue.GetItems<Popup>().Take(13))
            {
                    p.Decorator.Paint(x, y , w, h, p.Text, p.Title, p.Hint);
                    y -= h * VerticalGap;
            }
        }*/
		
		// @modified добавлена реализация растягивания сообщения по ширине под размер текста (не полностью протестированно)
        public void PaintWorld(WorldLayer layer)
        {
            var h = Hud.Window.Size.Height * RatioH;
            var x = Hud.Window.Size.Width;
            var y = Hud.Window.Size.Height * RatioY;

            foreach (Popup p in Hud.Queue.GetItems<Popup>().Take(13))
            {
					var wk = p.MaxWidth + 30;
					var xk = x * RatioX - (wk / 2);
                    p.Decorator.Paint(xk, y , wk, h, p.Text, p.Title, p.Hint);
                    y -= h * VerticalGap;
            }
        }
    }
}