﻿﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;

using Turbo.Plugins.Default;

 namespace Turbo.Plugins.Prrovoss.Popups
 {
	 /** @author @loremipsus */
     public class AMP_BlackListNotifications : BasePlugin, IInGameTopPainter, INewAreaHandler
     {
         public class UnwelcomePlayer
         {
            public bool Displayed { get; set; }
            public IPlayer Player { get; set; }

            public string Name { get; set; }
            public string Text { get; set; }
            public string Title { get; set; }
            public int Duration { get; set; }
            public TopLabelWithTitleDecorator Decorator { get; set; }

            public UnwelcomePlayer(string name, string text, string title, int duration, TopLabelWithTitleDecorator decorator = null)
            {
                this.Displayed = false;

                this.Name = name;
                this.Text = text;
                this.Title = title;
                this.Duration = duration;
                this.Decorator = decorator;
            }
        }

        public string DafaultText { get; set; }
        public string DefaultTitle { get; set; }
        public int DefaultDuration { get; set; }
        public TopLabelWithTitleDecorator DefaultDecorator { get; set; }
        
        public List<UnwelcomePlayer> UnwelcomePlayersToWatch { get; set; }
        
        public AMP_BlackListNotifications()
        {
            Enabled = true;
        }
        
        public override void Load(IController hud)
        {
            base.Load(hud);
            
            DafaultText = "In the game {0}";
            DefaultTitle = "BLACK LIST";
            DefaultDuration = 15000;
            DefaultDecorator = new TopLabelWithTitleDecorator(Hud)
            {
                BorderBrush = Hud.Render.CreateBrush(255, 255, 0, 0, -1),
                BackgroundBrush = Hud.Render.CreateBrush(200, 0, 0, 0, 0),
                TextFont = Hud.Render.CreateFont("tahoma", 6, 255, 255, 255, 255, true, false, false),
                TitleFont = Hud.Render.CreateFont("tahoma", 6, 255, 255, 0, 0, true, false, false),
            };

            UnwelcomePlayersToWatch = new List<UnwelcomePlayer>();
        }

        public void Add(string name, string text = null, string title = null, int duration = 0, TopLabelWithTitleDecorator decorator = null)
        {
            if (text == null) text = DafaultText;
            if (title == null) title = DefaultTitle;
            if (duration == 0) duration = DefaultDuration;
            if (decorator == null) decorator = DefaultDecorator;
            
            UnwelcomePlayersToWatch.Add(new UnwelcomePlayer(name, text, title, duration, decorator));
        }
        
        private bool checkPlayer(UnwelcomePlayer unwelcomePlayer)
        {
            bool result = false;

            foreach (var player in Hud.Game.Players) 
            {
                if (player.IsMe) continue;
                if (player.BattleTagAbovePortrait == "") continue;
                
                if (unwelcomePlayer.Name.Contains(player.BattleTagAbovePortrait) && !unwelcomePlayer.Displayed)
                {
                    result = true;
                    unwelcomePlayer.Player = player;
                    unwelcomePlayer.Displayed = true;
                    break;
                }
            }
            
            return result;
        }
        
        public void OnNewArea(bool newGame, ISnoArea snoArea)
        {
            if (newGame)
            {
                foreach (UnwelcomePlayer unwelcomePlayer in UnwelcomePlayersToWatch)
                {
                    unwelcomePlayer.Displayed = false;
                }
            }
        }

        public void PaintTopInGame(ClipState clipState)
        {
            if (!Hud.Game.IsInGame) return;
            if (clipState != ClipState.BeforeClip) return;
            if (UnwelcomePlayersToWatch.Count() == 0) return;
            if (Hud.Game.Players.Count() < 2) return;

            foreach (UnwelcomePlayer unwelcomePlayer in UnwelcomePlayersToWatch)
            {
                if(checkPlayer(unwelcomePlayer))
                {
                    Hud.RunOnPlugin<PopupNotifications>(plugin =>
                    {
                        plugin.Show(string.Format(unwelcomePlayer.Text, unwelcomePlayer.Name), unwelcomePlayer.Title, unwelcomePlayer.Duration, "", unwelcomePlayer.Decorator);
                    });
                }
            }
        }
     }
 }