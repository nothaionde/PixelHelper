/*

handles all the click events and hover tooltips for labels

*/

namespace Turbo.Plugins.Razor.Label
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	//using System.Globalization;
	using System.Linq;
	//using System.Text; //StringBuilder
	//using System.Windows.Forms; //Keys
	//using SharpDX.DirectInput; //Key
	//using SharpDX.DirectWrite; //TextLayout
	//using System.IO; //File.WriteAllText, tier 3
	using System.Windows.Forms; //MouseButtons
	//using System.Threading;
	//using Gma.System.MouseKeyHook;
	//using Indieteur.GlobalHooks;

	using Turbo.Plugins.Default;
	//using Turbo.Plugins.Razor.Click;
	using Turbo.Plugins.Razor.Util;

	public class LabelController : BasePlugin, IBeforeRenderHandler, IInGameTopPainter, IMouseClickHandler, IMouseClickBlocker //, ILeftClickHandler, IRightClickHandler, ILeftBlockHandler //, IMessageFilter
	{
		//public Func<bool> LeftClickBlockCondition { get; private set; }

		public IBrush BackgroundBrush { get; set; }
		public IBrush SkillBorderLight { get; set; }
		public IBrush SkillBorderDark { get; set; }
		
		public ILabelDecorator HoveredLabel { get; set; } //set by LabelDecorator
		public ILabelDecorator HintLabel { get; set; }
		public ILabelDecorator ClickLabel { get; set; }
		
		public List<Action> Queue { get; private set; } = new List<Action>();
		//public List<Action> QueueBefore { get; private set; } = new List<Action>();
		//public List<Action> QueueInventory { get; private set; } = new List<Action>();
		//public List<Action> QueueAfter { get; private set; } = new List<Action>();
		
		private ILabelDecorator LastClickLabel; //to cover the brief moments that ClickLabel = null when it really isn't for blocking mouse click events
		
		private bool clicked = false;
		
		public LabelController() //: base()
		{
			Enabled = true;
			//Order = -10001; //run this before some stuff
			Order = int.MaxValue; //try to draw it last
		}
		
		public override void Load(IController hud)
		{
			base.Load(hud);

			BackgroundBrush = Hud.Render.CreateBrush(175, 0, 0, 0, 0);
			SkillBorderLight = Hud.Render.CreateBrush(200, 95, 95, 95, 1); //235, 227, 164 //138, 135, 109
			SkillBorderDark = Hud.Render.CreateBrush(150, 0, 0, 0, 1);
			
			//LabelDecorator.GlobalHook = Hook.GlobalEvents();
			
			//ThreadPool.QueueUserWorkItem(state => {
			//	Hud.Sound.Speak("register");
			/*LabelDecorator.GlobalHook.MouseDownExt += async (sender, e) =>
			{
				//Console.Write(e.KeyChar);
				//if (e.KeyChar == 'q') quit();
				if (Hud.Window.IsForeground)
				{
					Hud.Sound.Speak("click");
					clicked = true;

					if (ClickLabel is object)
					{
						//suppress the left mouse button click only if there is a click handler bound to the hovered label
						if (e.Button == System.Windows.Forms.MouseButtons.Left) 
							e.Handled = true;
					}
				}
			};*/
			//});
			
			//If it is not created, we instantiate it and subscribe to the available events.
			/*LabelDecorator.MouseHook = new GlobalMouseHook();
			LabelDecorator.MouseHook.OnButtonDown += (sender, e) =>
			{
				//Console.Write(e.KeyChar);
				//if (e.KeyChar == 'q') quit();
				if (Hud.Window.IsForeground)
				{
					Hud.Sound.Speak("click");
					clicked = true;

					if (ClickLabel is object)
					{
						//suppress the left mouse button click only if there is a click handler bound to the hovered label
						if (e.Button == GHMouseButtons.Left) //System.Windows.Forms.MouseButtons.Left) 
							e.Handled = true;
					}
				}
			}; //GlobalMouseHook_OnButtonDown;
			*/
			//LabelDecorator.MouseHook.OnButtonUp += GlobalMouseHook_OnButtonUp;
			//globalMouseHook.OnMouseMove += GlobalMouseHook_OnMouseMove;
			//globalMouseHook.OnMouseWheelScroll += GlobalMouseHook_OnMouseWheelScroll;
			
/* 			LabelDecorator.Thread = new GlobalHookThread((thread) => {//(hook) => {
				thread.GlobalHook.MouseDownExt += (sender, e) =>
				{
					//Console.Write(e.KeyChar);
					//if (e.KeyChar == 'q') quit();
					if (Hud.Window.IsForeground)
					{
						//Hud.Sound.Speak("click");
						clicked = true;

						if (ClickLabel is object)
						{
							//suppress the left mouse button click only if there is a click handler bound to the hovered label
							if (e.Button == System.Windows.Forms.MouseButtons.Left) //GHMouseButtons.Left) //
							{
								e.Handled = true;
								//Hud.Sound.Speak("supress");
							}
						}
						
						//suppress the left mouse button click only if there is a click handler bound to the hovered label
						// if (e.Button == System.Windows.Forms.MouseButtons.Left && thread.IsBlockingLeftClick.Any(f => f()))
						// {
							// e.Handled = true;
							// Hud.Sound.Speak("supress");
						// }
					}
				};
			});
			
			LabelDecorator.Thread.IsBlockingLeftClick.Add(() => ClickLabel is object);
 */
			//BlockingFunc = () => ClickLabel is object;
			
			//prevent left clicks when trying to click on labels with click functions, LastClickLabel is to fill in the gap between when new ClickLabel is determined across the render loop (may be momentarily null when click event happens)
			//LeftClickBlockCondition = () => ClickLabel is object || LastClickLabel is object;
		}
		
		public void BeforeRender()
		{
			/*if (BlockingFunc is object)
			{
				ClickEventHandler handler = Hud.GetPlugin<ClickEventHandler>();
				handler.BlockLeftClick.Add(BlockingFunc);
				BlockingFunc = null;
			}*/
			
			if (LabelDecorator.Overlay == null)
				LabelDecorator.Overlay = Hud.GetPlugin<UIOverlapHelper>();
			if (LabelDecorator.Controller == null)
				LabelDecorator.Controller = this;
			
			if (HoveredLabel is object)
			{
				if (clicked && ClickLabel is object)
				{
					//var handler = FindClickHandler(HoveredLabel);
					//if (handler is object)
					//	handler.OnClick(handler);
					//else
					//	Hud.Sound.Speak("no handler found");
					ClickLabel.OnClick(ClickLabel);
				}
				
				HoveredLabel = null;
			}
			HintLabel = null;
			
			LastClickLabel = ClickLabel;
			ClickLabel = null;
			
			clicked = false;
		}
		
		public void PaintTopInGame(ClipState clipState)
		{
			/*if (clipState == ClipState.BeforeClip)
			{
				while (QueueBefore.Count > 0)
				{
					int i = QueueBefore.Count - 1;
					QueueBefore[i].Invoke();
					QueueBefore.RemoveAt(i);
				}
			}
			else if (clipState == ClipState.Inventory)
			{
				while (QueueInventory.Count > 0)
				{
					int i = QueueInventory.Count - 1;
					QueueInventory[i].Invoke();
					QueueInventory.RemoveAt(i);
				}
			}
			else*/ if (clipState == ClipState.AfterClip)
			{
				/*while (QueueAfter.Count > 0)
				{
					int i = QueueAfter.Count - 1;
					QueueAfter[i].Invoke();
					QueueAfter.RemoveAt(i);
				}*/
				while (Queue.Count > 0)
				{
					int i = Queue.Count - 1;
					Queue[i].Invoke();
					Queue.RemoveAt(i);
				}
				
				if (HintLabel is object && LabelDecorator.IsVisible(HintLabel))
				{
					float x = Hud.Window.CursorX + 30;
					float y = Hud.Window.CursorY - 30 - HintLabel.Height;
					float w = HintLabel.Width;
					float h = HintLabel.Height;
					
					//edge cases
					if (x + w > Hud.Window.Size.Width)
						x = Hud.Window.CursorX - w - 30;
					if (y < 0)
						y = Hud.Window.CursorY + 30;
					else if (y + h > Hud.Window.Size.Height)
						y = Hud.Window.Size.Height - h;
					
					//draw tooltip bg
					BackgroundBrush?.DrawRectangle(x-3, y-3, w+6, h+6);
					
					//save dimensions for the borders afterwards
					HintLabel.Paint(x, y);
				
					//draw tooltip border
					if (SkillBorderDark is object && SkillBorderLight is object)
					{
						SkillBorderDark.DrawRectangle(x - 1, y - 1, w + 2, h + 2);
						SkillBorderLight.DrawRectangle(x - 2, y - 2, w + 4, h + 4);
						SkillBorderDark.DrawRectangle(x - 3, y - 3, w + 6, h + 6);
					}
				}
				/*if (HoveredLabel is object)
				{
					var hint = FindHint(HoveredLabel);
					if (hint is object && LabelDecorator.IsVisible(hint))
					{
						float x = Hud.Window.CursorX + 30;
						float y = Hud.Window.CursorY - 30 - hint.Height;
						float w = hint.Width;
						float h = hint.Height;
						
						//edge cases
						if (x + w > Hud.Window.Size.Width)
							x = Hud.Window.CursorX - w - 30;
						if (y < 0)
							y = Hud.Window.CursorY + 30;
						else if (y + h > Hud.Window.Size.Height)
							y = Hud.Window.Size.Height - h;
						
						//draw tooltip bg
						BackgroundBrush?.DrawRectangle(x, y, w, h);
						
						//save dimensions for the borders afterwards
						hint.Paint(x, y);
					
						//draw tooltip border
						if (SkillBorderDark is object && SkillBorderLight is object)
						{
							SkillBorderDark.DrawRectangle(x - 1, y - 1, w + 2, h + 2);
							SkillBorderLight.DrawRectangle(x - 2, y - 2, w + 4, h + 4);
							SkillBorderDark.DrawRectangle(x - 3, y - 3, w + 6, h + 6);
						}
					}
				}*/
			}
		}

		public bool MouseDown(MouseButtons button)
		{
			return true;
		}
        public bool MouseUp(MouseButtons button)
		{
			if (button == MouseButtons.Left)
				clicked = true;

			return true;
		}

		public bool MouseClickShouldBeBlocked(MouseButtons button, int x, int y)
		{
			return (button == MouseButtons.Left) && (ClickLabel is object || LastClickLabel is object);
		}
	
		public ILabelDecorator FindClickHandler(ILabelDecorator decorator)
		{
			if (decorator is ILabelDecoratorCollection)
			{
				ILabelDecoratorCollection mdc = (ILabelDecoratorCollection)decorator;
				if (mdc.HoveredLabel is object)
				{
					var nestedButton = FindClickHandler(mdc.HoveredLabel);
					if (nestedButton is object)
						return nestedButton;
				}
			}
			
			if (decorator.OnClick is object)
				return decorator;
			
			return null;
		}
		
		public ILabelDecorator FindHint(ILabelDecorator decorator)
		{
			if (decorator is ILabelDecoratorCollection)
			{
				ILabelDecoratorCollection nestedDecorator = (ILabelDecoratorCollection)decorator;
				if (nestedDecorator.HoveredLabel is object)
				{
					ILabelDecorator nestedHint = FindHint(nestedDecorator.HoveredLabel);
					if (nestedHint is object)
						return nestedHint;
				}
			}
			
			return decorator.Hint;
		}
	}
}