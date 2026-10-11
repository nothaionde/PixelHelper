﻿﻿﻿using System;
using System.Linq;
using System.Collections.Generic;

using Turbo.Plugins.Default;

 namespace Turbo.Plugins.Prrovoss.Popups
 {
	 /** @author @loremipsus */
     public class AMP_EmptyGemSlotNotifications : BasePlugin, IAfterCollectHandler
     {
        public string Text { get; set; }
        public string Title { get; set; }
        public int Duration { get; set; }
        public TopLabelWithTitleDecorator Decorator { get; set; }

        private bool Displayed { get; set; }
        
        public AMP_EmptyGemSlotNotifications()
        {
            Enabled = true;
        }
        
        public override void Load(IController hud)
        {
            base.Load(hud);
            
            Displayed = false;
            
            Text = "В слоте нет легендарного драгоценного камня";
            Title = "ПРЕДУПРЕЖДЕНИЕ";
            Duration = 7000;
            Decorator = new TopLabelWithTitleDecorator(Hud)
            {
                BorderBrush = Hud.Render.CreateBrush(255, 255, 0, 0, -1),
                BackgroundBrush = Hud.Render.CreateBrush(200, 0, 0, 0, 0),
                TextFont = Hud.Render.CreateFont("tahoma", 6, 255, 255, 255, 255, true, false, false),
                TitleFont = Hud.Render.CreateFont("tahoma", 6, 255, 255, 0, 0, true, false, false),
            };
        }
        
        private bool checkAnyGemSlotIsEmpty()
        {
            if (Hud.Game.Me.Hero.Level < 70) return false;
            
            var jewelsLocations = Hud.Game.Items.Where(x => x.Location == ItemLocation.LeftRing || x.Location == ItemLocation.RightRing || x.Location == ItemLocation.Neck);
           
            foreach (var item in jewelsLocations)
            {
                if (item.SocketCount != 1) continue;
                if (item.ItemsInSocket == null) return true;
                var legendaryGem = Array.Find(item.ItemsInSocket, x => x.Quality == ItemQuality.Legendary && x.JewelRank > -1);
                if (legendaryGem == null) return true;
            }

            return false;
        }
        
        public void AfterCollect()
        {
            if (Hud.Game.IsInGame && !Displayed)
            {
                if (checkAnyGemSlotIsEmpty())
                {
                    Hud.RunOnPlugin<PopupNotifications>(plugin =>
                    {
                        plugin.Show(Text, Title, Duration, "", Decorator);
                    });
                }
                
                Displayed = true; // здесь, чтобы не показывало в игре при извлечении камней
            }
            else if (!Hud.Game.IsInGame)
            {
                Displayed = false;
            }
        }
     }
 }