using System.Collections.Generic;
using System.Linq;
using Turbo.Plugins.Default;

namespace Turbo.Plugins.Prrovoss.Popups
{
    public class BountyAppearedPopup : BasePlugin, IInGameTopPainter, INewAreaHandler
    {
		public class MyBounty
		{
			public bool Displayed { get; set; }
			public uint Sno { get; set; }
			public string Name { get; set; }
            public string Hint { get; set; }
            public string Title { get; set; }
            public int Duration { get; set; }
            public TopLabelWithTitleDecorator Decorator { get; set; }
            
            public MyBounty(uint sno, string name, string hint, string title, int duration, TopLabelWithTitleDecorator decorator = null)
            {
				this.Displayed = false;
				this.Sno = sno;
				this.Hint = hint;
				this.Name = name;
				this.Title = title;
				this.Duration = duration;
				this.Decorator = decorator;
            }
        }
        
        public List<MyBounty> BountiesToPopup { get; set; }
            
        public override void Load(IController hud)
        {
            base.Load(hud);
            BountiesToPopup = new List<MyBounty>();
        }

		private bool BountyCheck(MyBounty bountyToCheck)
		{
			bool returnBool = false;

			foreach (var quest in Hud.Game.Quests.Where(quest => quest.SnoQuest.Type == QuestType.Bounty))
			{
				if ((quest != null) && quest.State != QuestState.completed)
                {
                    if (bountyToCheck.Sno == quest.SnoQuest.Sno && !bountyToCheck.Displayed)
                    {
						returnBool = true;
						bountyToCheck.Displayed = true;
						break;
                    }
                }
			}

			return returnBool;
		}

        public void Add(uint sno, string name, string hint, string title, int duration, TopLabelWithTitleDecorator decorator = null)
        {
            BountiesToPopup.Add(new MyBounty(sno, name, hint, title, duration, decorator));
        }
        
        public void OnNewArea(bool newGame, ISnoArea area)
        {
            if (newGame)
            {
                foreach (MyBounty bounty in BountiesToPopup)
                {
					bounty.Displayed = false;
                }
            }
        }        

        public void PaintTopInGame(ClipState clipState)
        {
            foreach (MyBounty bounty in BountiesToPopup)
            {
				if (BountyCheck(bounty))
                {
                    Hud.RunOnPlugin<PopupNotifications>(plugin =>
                    {
                        plugin.Show(bounty.Name, bounty.Title, bounty.Duration, bounty.Hint, bounty.Decorator);
                    });
				}
            }
        }
        
		public BountyAppearedPopup()
        {
            Enabled = true;
        }
        
    }
}