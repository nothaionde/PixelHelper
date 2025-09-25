using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text; //StringBuilder
using System.Windows.Forms; //Keys
using SharpDX.DirectInput; //Key
using SharpDX.DirectWrite; //TextLayout

using Turbo.Plugins.Default;
using Turbo.Plugins.Razor.Click;

namespace Turbo.Plugins.Razor.Movable
{
    public class MovableController : BasePlugin, IInGameTopPainter, IKeyEventHandler, IAfterCollectHandler, ILeftClickHandler //, ICustomizer, IInGameWorldPainter
    {
		//public bool AllowDragAndDrop { get; set; } = true; //only while outlines shown
		public bool AutoSaveConfigChanges { get; set; } = true; //whenever the floater position or enable/disable status changes, update the config file written to /logs folder
		public string ConfigFileName { get; set; } = "MovablePluginConfig";

		public IKeyEvent TogglePickup { get; set; } //optional
		public IKeyEvent ToggleEnable { get; set; } //optional
		public IKeyEvent ToggleEditMode { get; set; } //optional
		public IKeyEvent ToggleDragAndDrop { get; set; } //optional
		public IKeyEvent ToggleGrid { get; set; } //optional
		public IKeyEvent HotkeyCancel { get; set; } //optional
		public IKeyEvent HotkeySave { get; set; } //optional
		public IKeyEvent HotkeyUndo { get; set; } //optional
		public IKeyEvent HotkeyUndoAll { get; set; } //optional
		public IKeyEvent HotkeyPickupNext { get; set; } //optional
		public IKeyEvent HotkeyPickupPrev { get; set; } //optional
		
		public string ArrowBottomLeft { get; set; } = "↙";
		public string ArrowBottomRight { get; set; } = "↘"; //
		public string CornerBottomLeft { get; set; } = "◣";
		public string CornerBottomRight { get; set; } = "◢";
		public string ArrowHorizontal { get; set; } = "↔";
		public string ArrowVertical { get; set; } = "↕";
		
		//public string CornerTopLeft { get; set; } = "◤";
		//public string CornerTopRight { get; set; } = "◥";
		
		public bool EditMode { get; set; } = false; //starting state, can be toggled on and off with a hotkey
		public bool ShowGrid { get; set; } = true; //while in Edit Mode only
		public int GridSize { get; set; } = 10;
		public IBrush GridBrush { get; set; }
		public IFont EnabledFont { get; set; }
		public IFont EnabledSelectedFont { get; set; }
		public IFont EnabledUnselectedFont { get; set; }
		public IBrush EnabledBrush { get; set; }
		public IBrush EnabledSelectedBrush { get; set; }
		public IFont DisabledFont { get; set; }
		public IFont DisabledSelectedFont { get; set; }
		public IFont DisabledUnselectedFont { get; set; }
		public IBrush DisabledBrush { get; set; }
		public IBrush DisabledSelectedBrush { get; set; }
		public IBrush TemporaryBrush { get; set; }
		public IBrush TemporarySelectedBrush { get; set; }
		public IFont SelectedFont { get; set; }
		public IFont EnabledDragFont { get; set; }
		public IFont DisabledDragFont { get; set; }
		
		public string HoverHint { get; set; } = "{0}: Pick Up / Put Down\n{1}: Enable / Disable\n{2}: Cancel"; //set to null or empty string to disable
		public string DragAndDropHint { get; set; } = "Drag and Drop";
		
		//public List<IMovable> Floaters { get; private set; } = new List<IMovable>();
		//public List<IMovable> MovablePlugins { get; private set; } //= new List<IMovable>();
		public Dictionary<IMovable, List<MovableArea>> MovablePlugins { get; private set; } //= new List<IMovable>();
		//public Dictionary<string, Tuple<float, float, bool>> FloatersConfig { get; set; } = new Dictionary<string, Tuple<float, float, bool>>(); //so that a config file can fail without exceptions if a floater-implemented plugin or addon is removed
		public List<Tuple<string, int, float, float, float, float, bool>> Config { get; set; } = new List<Tuple<string, int, float, float, float, float, bool>>(); //so that a config file can fail without exceptions if a floater-implemented plugin or addon is removed
		
		//public IMovable CursorPlugin { get; set; }
		public MovableArea CursorPluginArea { get; set; }
		public MovableArea HoveredPluginArea { get; set; }
		public MovableArea ResizePluginArea { get; set; }
		
		private IMovable[] Lookup;
		private List<MovableArea> DeletionQueue = new List<MovableArea>();
		private float PickedUpAtX = 0;
		private float PickedUpAtY = 0;
		private float ResizeHoverSize;
		private bool HoveredResizeRight = false;
		private bool HoveredResizeLeft = false;
		private bool LButtonPressed = false;
		private bool ConfigChanged = false;
		private bool QueueConfigSave = false;
		private StringBuilder TextBuilder;
		
        public MovableController() : base()
        {
            Enabled = true;
			Order = -10001; //run this before the other floater stuff
        }
		
        public override void Load(IController hud)
        {
            base.Load(hud);
			
			TextBuilder = new StringBuilder();
			
			TogglePickup = Hud.Input.CreateKeyEvent(true, Key.C, true, false, false); //Ctrl + C
			ToggleEnable = Hud.Input.CreateKeyEvent(true, Key.X, true, false, false); //Ctrl + X
			ToggleEditMode = Hud.Input.CreateKeyEvent(true, Key.F12, true, false, false); //F12
			ToggleGrid = Hud.Input.CreateKeyEvent(true, Key.G, true, false, false); //Ctrl + G
			HotkeyCancel = Hud.Input.CreateKeyEvent(true, Key.Escape, false, false, false); //Esc
			HotkeySave = Hud.Input.CreateKeyEvent(true, Key.S, true, false, false); //Ctrl + S
			HotkeyUndo = Hud.Input.CreateKeyEvent(true, Key.Z, true, false, false); //Ctrl + Z
			HotkeyUndoAll = Hud.Input.CreateKeyEvent(true, Key.D0, true, false, false); //Ctrl + Z
			HotkeyPickupNext = Hud.Input.CreateKeyEvent(true, Key.Right, false, false, false); //Left Arrow
			HotkeyPickupPrev = Hud.Input.CreateKeyEvent(true, Key.Left, false, false, false); //Right Arrow
			
			GridBrush = Hud.Render.CreateBrush(15, 175, 175, 175, 1);
			EnabledBrush = Hud.Render.CreateBrush(255, 200, 200, 255, 2, SharpDX.Direct2D1.DashStyle.Dash);
			EnabledSelectedBrush = Hud.Render.CreateBrush(255, 200, 200, 255, 2);
			EnabledFont = Hud.Render.CreateFont("tahoma", 7f, 255, 200, 200, 255, false, false, 175, 0, 0, 0, true); //, 255, 0, 0, 0, true
			EnabledUnselectedFont = Hud.Render.CreateFont("tahoma", 7f, 150, 200, 200, 255, false, false, false); //, 255, 0, 0, 0, true
			EnabledSelectedFont = Hud.Render.CreateFont("tahoma", 7f, 255, 255, 255, 255, false, false, 100, 122, 147, 255, true);
			DisabledBrush = Hud.Render.CreateBrush(255, 255, 0, 0, 2, SharpDX.Direct2D1.DashStyle.Dash);
			DisabledSelectedBrush = Hud.Render.CreateBrush(255, 255, 0, 0, 2);
			DisabledFont = Hud.Render.CreateFont("tahoma", 7f, 255, 255, 50, 50, false, false, 255, 0, 0, 0, true); //, 255, 0, 0, 0, true
			DisabledUnselectedFont = Hud.Render.CreateFont("tahoma", 7f, 150, 255, 50, 50, false, false, false); //, 255, 0, 0, 0, true
			DisabledSelectedFont = Hud.Render.CreateFont("tahoma", 7f, 255, 255, 255, 255, false, false, 175, 255, 50, 50, true);
			SelectedFont = Hud.Render.CreateFont("tahoma", 14f, 255, 255, 255, 255, true, false, 175, 0, 0, 0, true);
			EnabledDragFont = Hud.Render.CreateFont("tahoma", 10f, 255, 200, 200, 255, false, false, false); //, 175, 0, 0, 0, true);
			DisabledDragFont = Hud.Render.CreateFont("tahoma", 10f, 255, 255, 50, 50, false, false, false); //, 255, 0, 0, 0, true);
			//SelectedBrush = Hud.Render.CreateBrush(100, 200, 200, 200, 4, SharpDX.Direct2D1.DashStyle.Dash);
			TemporaryBrush = Hud.Render.CreateBrush(255, 69, 41, 255, 2, SharpDX.Direct2D1.DashStyle.Dash);
			TemporarySelectedBrush = Hud.Render.CreateBrush(255, 69, 41, 255, 2);
			
			ResizeHoverSize = SelectedFont.MaxHeight*0.12f;
			
			//LButtonPressed = Hud.Input.IsKeyDown(Keys.LButton);
        }
		
		public void OnKeyEvent(IKeyEvent keyEvent)
		{
			if (!Hud.Window.IsForeground) return; //only process the key event if hud is actively displayed //this check is now baked into hud but sometimes I get weird behavior when alt-tabbed anyway
			//check if chat window is open, if so, ignore key input
			//IUiElement ui = Hud.Render.GetUiElement("Root.NormalLayer.chatentry_dialog_backgroundScreen.chatentry_content.chat_editline");
			if (Hud.Render.GetUiElement("Root.NormalLayer.chatentry_dialog_backgroundScreen.chatentry_content.chat_editline").Visible) //chatPrompt is visible
				return;

			if (keyEvent.IsPressed) //Ctrl + C
			{
				if (ToggleEditMode is object && ToggleEditMode.Matches(keyEvent))
				{
					EditMode = !EditMode;
					
					if (!EditMode) //CursorPluginArea is object
					{
						//CursorPluginArea = null;
						//ResizePluginArea = null;
						PutDown();
						HoveredPluginArea = null;
					}
				}
				else if (HotkeySave is object && HotkeySave.Matches(keyEvent))
				{
					ConfigChanged = true;
				}
				else if (TogglePickup is object && TogglePickup.Matches(keyEvent)) //if (keyEvent.Key == Key.C && keyEvent.ControlPressed) //Ctrl + C
				{
					if (LButtonPressed) //if (CursorPluginArea is object)
						OnLeftMouseUp();
					else
						OnLeftMouseDown();
				}
				else
				{
					if (!EditMode)
						return;
					
					MovableArea selected = CursorPluginArea ?? HoveredPluginArea;
					
					//pass along the key event if the MovableArea is currently selected while in Modify Mode
					if (selected is object && selected.Owner is IMovableKeyEventHandler)
						((IMovableKeyEventHandler)selected.Owner).OnKeyEvent(this, keyEvent, selected); //, CursorPluginArea == selected

					//
					if (ToggleEnable is object && ToggleEnable.Matches(keyEvent)) 
					{
						if (selected is object)
							ToggleArea(selected);
					}
					else if (ToggleGrid is object && ToggleGrid.Matches(keyEvent)) 
					{
						ShowGrid = !ShowGrid;
					}
					else if (HotkeyPickupNext is object && HotkeyPickupNext.Matches(keyEvent))
					{
						if (selected == null)
							return;
						
						if (CursorPluginArea == null) // && HoveredPluginArea is object
						{
							PickUp(HoveredPluginArea);
							/*CursorPluginArea = HoveredPluginArea;
							ResizePluginArea = null;
							PickedUpAtX = Hud.Window.CursorX;
							PickedUpAtY = Hud.Window.CursorY;
							LButtonPressed = true;*/
						}
						
						//only start searching after finding the currently selected plugin
						int index = Array.IndexOf(Lookup, selected.Owner);
						if (index < 0)
							return;
						
						//try to find it in the same plugin first
						List<MovableArea> areas = MovablePlugins[Lookup[index]];
						for (int i = areas.IndexOf(selected) + 1; i < areas.Count && CursorPluginArea == selected; ++i)
						{
							MovableArea area = areas[i];
							if (IsHovered(area))
							{
								/*CursorPluginArea = area;
								HoveredPluginArea = area;
								PickedUpAtX = Hud.Window.CursorX;
								PickedUpAtY = Hud.Window.CursorY;*/
								PickUp(area);
								break;
							}
						}
						
						if (CursorPluginArea == selected)
						{
							for (int i = index+1; i < Lookup.Length && CursorPluginArea == selected; ++i)
							{
								areas = MovablePlugins[Lookup[i]];
								for (int j = 0; j < areas.Count; ++j) //j = areas.IndexOf(selected)
								{
									MovableArea area = areas[j];
									if (IsHovered(area))
									{
										/*CursorPluginArea = area;
										HoveredPluginArea = area;
										PickedUpAtX = Hud.Window.CursorX;
										PickedUpAtY = Hud.Window.CursorY;*/
										PickUp(area);
										break;
									}
								}
								
								//if (CursorPluginArea != selected) //found next
								//	return;
							}
						}
						
						if (CursorPluginArea == selected)
						{
							for (int i = 0; i <= index && CursorPluginArea == selected; ++i)
							{
								areas = MovablePlugins[Lookup[i]];
								for (int j = 0; j < areas.Count; ++j)
								{
									MovableArea area = areas[j];
									
									if (area == selected)
										break;
									if (IsHovered(area))
									{
										/*CursorPluginArea = area;
										HoveredPluginArea = area;
										PickedUpAtX = Hud.Window.CursorX;
										PickedUpAtY = Hud.Window.CursorY;*/
										PickUp(area);
										break;
									}
								}
							}
						}
					}
					else if (HotkeyPickupPrev is object && HotkeyPickupPrev.Matches(keyEvent))
					{
						if (selected == null)
							return;
						
						if (CursorPluginArea == null) // && HoveredPluginArea is object
						{
							PickUp(HoveredPluginArea);
							/*CursorPluginArea = HoveredPluginArea;
							ResizePluginArea = null;
							PickedUpAtX = Hud.Window.CursorX;
							PickedUpAtY = Hud.Window.CursorY;
							LButtonPressed = true;*/
						}
						
						//MovableArea selected = CursorPluginArea; //CursorPluginArea ?? HoveredPluginArea;
						//if (selected == null)
						//	return;
						
						//only start searching after finding the currently selected plugin
						int index = Array.IndexOf(Lookup, selected.Owner);
						if (index < 0)
							return;
						
						//try to find it in the same plugin first
						List<MovableArea> areas = MovablePlugins[Lookup[index]];
						for (int i = areas.IndexOf(selected) - 1; i > -1 && CursorPluginArea == selected; --i)
						{
							MovableArea area = areas[i];
							if (IsHovered(area))
							{
								/*CursorPluginArea = area;
								HoveredPluginArea = area;
								PickedUpAtX = Hud.Window.CursorX;
								PickedUpAtY = Hud.Window.CursorY;*/
								PickUp(area);
								break;
							}
						}
						
						if (CursorPluginArea == selected)
						{
							for (int i = index-1; i > -1 && CursorPluginArea == selected; --i)
							{
								areas = MovablePlugins[Lookup[i]];
								for (int j = areas.Count-1; j > -1; --j) //j = areas.IndexOf(selected)
								{
									MovableArea area = areas[j];
									if (IsHovered(area))
									{
										/*CursorPluginArea = area;
										HoveredPluginArea = area;
										PickedUpAtX = Hud.Window.CursorX;
										PickedUpAtY = Hud.Window.CursorY;*/
										PickUp(area);
										break;
									}
								}
							}
						}
						
						if (CursorPluginArea == selected)
						{
							for (int i = Lookup.Length-1; i >= index && CursorPluginArea == selected; --i)
							{
								areas = MovablePlugins[Lookup[i]];
								for (int j = areas.Count-1; j > -1; --j)
								{
									MovableArea area = areas[j];
									
									if (area == selected)
										break;
									if (IsHovered(area))
									{
										/*CursorPluginArea = area;
										HoveredPluginArea = area;
										PickedUpAtX = Hud.Window.CursorX;
										PickedUpAtY = Hud.Window.CursorY;*/
										PickUp(area);
										break;
									}
								}
							}
						}
					}
					else if (HotkeyCancel is object && HotkeyCancel.Matches(keyEvent))
					{
						//CursorPluginArea = null;
						//ResizePluginArea = null;
						//LButtonPressed = false;
						PutDown();
					}
					else if (HotkeyUndo is object && HotkeyUndo.Matches(keyEvent))
					{
						if (selected is object)
						{
							selected.Undo();
							PutDown();
						}
					}
					else if (HotkeyUndoAll is object && HotkeyUndoAll.Matches(keyEvent))
					{
						if (selected is object)
						{
							selected.Reset();
							PutDown();
						}
					}
				}
			}
		}
		
		public void AfterCollect()
		{
			if (!Hud.Game.IsInGame)
			{
				if (CursorPluginArea is object)
				{
					//PutDown(false);
					//CursorPlugin = null;
					CursorPluginArea = null;
					ConfigChanged = false;
				}
				
				if (ResizePluginArea is object)
					ResizePluginArea = null;
				
				//output a config file into logs folder so that user can preserve these position settings
				if ((ConfigChanged || QueueConfigSave) && MovablePlugins is object)
				{
					ConfigChanged = false;
					QueueConfigSave = false;
					Hud.TextLog.Log(ConfigFileName, ConfigToString(), false, false);
				}
				
				return;
			}

			//initialization
			//have to wait until you are in a game for the first time, otherwise OnRegister calls that reference existing UI element positions may return 0 because those haven't been initialized yet
			if (MovablePlugins == null)
			{
				MovablePlugins = new Dictionary<IMovable, List<MovableArea>>();
				
				//find all plugins that this controller will manage
				foreach (IPlugin p in Hud.AllPlugins.Where(p => p is IMovable)) //p.Enabled && 
				{
					//add to the master list
					//List<MovableArea> areas = new List<MovableArea>();
					IMovable plugin = (IMovable)p;
					MovablePlugins.Add(plugin, new List<MovableArea>()); //areas
					
					//tell the plugin to define all the MovableAreas (with MovableController.CreateArea calls)
					plugin.OnRegister(this);
				}
				
				Lookup = MovablePlugins.Keys.ToArray();
			}
			
			//if a MovableArea removal was requested, do it now
			if (DeletionQueue.Count > 0)
			{
				foreach (MovableArea area in DeletionQueue)
					MovablePlugins[area.Owner]?.Remove(area);
					
				DeletionQueue.Clear();
			}
			
			//loading any new config settings introduced after initialization
			if (Config.Count > 0)
			{
				
				foreach (Tuple<string, int, float, float, float, float, bool> settings in Config.ToArray())
				{
					IMovable m = MovablePlugins.Keys.FirstOrDefault(p => p.GetType().Name == settings.Item1); //Hud.AllPlugins.FirstOrDefault(p => p.GetType().Name == settings.Item1);
					if (m is object)
					{
						var areas = MovablePlugins[m];
						if (settings.Item2 < areas.Count)
							areas[settings.Item2].SetConfig(settings.Item3, settings.Item4, settings.Item5, settings.Item6, settings.Item7);
					}
					
					Config.Remove(settings);
				}
			}
			
			//hover check
			if (Hud.Window.IsForeground && ResizePluginArea == null && CursorPluginArea == null)
			{
				//bool hovered = false;
				HoveredPluginArea = null;
				HoveredResizeRight = false;
				HoveredResizeLeft = false;
				foreach (IMovable m in MovablePlugins.Keys)
				{
					if (!m.Enabled)
						continue;
					
					List<MovableArea> areas = MovablePlugins[m];
					areas.RemoveAll(a => 
{ 
						if (!a.Enabled && a.DeleteOnDisable)
						{
							if (CursorPluginArea == a)
								CursorPluginArea = null;
							else if (ResizePluginArea == a)
								ResizePluginArea = null;
							
							if (HoveredPluginArea == a)
								HoveredPluginArea = null;
							
							return true;
						}
						
						return false;
					}); //cleanup
					
					foreach (MovableArea area in MovablePlugins[m]) //for (int i = 0; i < areas.Count; ++i)
					{
						//MovableArea area = areas[i];						
						if (IsHovered(area))
						{
							//hovered = true;
							HoveredPluginArea = area;

							if (area.ResizeMode != ResizeMode.Off)
							{
								if (Hud.Window.CursorInsideRect(area.Rectangle.Right - ResizeHoverSize, area.Rectangle.Bottom - ResizeHoverSize, ResizeHoverSize, ResizeHoverSize)) //check bottom right
								{
									HoveredResizeRight = true;
									//ResizePluginArea = area;
								}
								else if (Hud.Window.CursorInsideRect(area.Rectangle.Left, area.Rectangle.Bottom - ResizeHoverSize, ResizeHoverSize, ResizeHoverSize)) //check bottom left
								{
									HoveredResizeLeft = true;
									//ResizePluginArea = area;
								}
							}
							
							break;
						}
					}
					
					//found
					if (HoveredPluginArea is object) //hovered
						break;
				}
			}
			
			//output a config file into logs folder so that user can preserve these position settings
			if (ConfigChanged)
			{
				ConfigChanged = false;
				Hud.TextLog.Log(ConfigFileName, ConfigToString(), false, false);
				
				//for TH version 20.7.12.0+
				if (Hud.Game.SpecialArea != SpecialArea.None || (int)Hud.Game.GameDifficulty > 3) //torment 1
					QueueConfigSave = true;
			}
			else if (QueueConfigSave && Hud.Game.SpecialArea == SpecialArea.None && (int)Hud.Game.GameDifficulty < 4)
			{
				QueueConfigSave = false;
				Hud.TextLog.Log(ConfigFileName, ConfigToString(), false, false);
			}
		}
		
        public void PaintTopInGame(ClipState clipState)
        {
			/*if (clipState == ClipState.AfterClip)
			{
				TextLayout test = DisabledFont.GetTextLayout(LButtonPressed.ToString()); //LButtonPressed //(HoveredPluginArea is object) //(CursorPluginArea is object) //(CursorPluginArea == null && HoveredPluginArea is object)
				DisabledFont.DrawText(test, Hud.Window.Size.Width*0.5f, Hud.Window.Size.Height*0.5f);
			}*/
			//wait until initialization
			if (MovablePlugins == null)
				return;
			
			bool isAnyHighlighted = CursorPluginArea is object || ResizePluginArea is object || HoveredPluginArea is object;
			
			foreach (IMovable m in MovablePlugins.Keys)
			{
				if (!m.Enabled)
					continue;
			
				string pluginName = m.GetType().Name;				
				List<MovableArea> areas = MovablePlugins[m];
				
				for (int i = 0; i < areas.Count; ++i)
				{
					MovableArea area = areas[i];

					bool isAreaOnCursor = area == CursorPluginArea; //f == CursorPlugin && area = CursorPluginArea;					
					bool highlighted = isAreaOnCursor || ResizePluginArea == area || HoveredPluginArea == area;

					//draw on top if highlighted
					if (highlighted)
					{
						if (clipState != ClipState.AfterClip)
							continue;
					}
					else if (area.ClipState != clipState)
						continue;

					if (area.Enabled && Hud.Game.MapMode == MapMode.Minimap)
					{
						if (isAreaOnCursor)
							m.PaintArea(this, area, Hud.Window.CursorX - PickedUpAtX, Hud.Window.CursorY - PickedUpAtY);
						else
							m.PaintArea(this, area, 0, 0);
					}
					
					if (EditMode)
					{
						//draw grid
						if (ShowGrid && clipState == ClipState.BeforeClip)
						{
							GridBrush.Opacity = 0.1f;
							for (int j = GridSize; j < Hud.Window.Size.Width; j += GridSize)
							{
								GridBrush.DrawLine(j, 0, j, Hud.Window.Size.Height);
								GridBrush.DrawLine(0, j, Hud.Window.Size.Width, j);
							}
						}
						
						RectangleF rect = area.Rectangle;
						float x = rect.X; //(isAreaOnCursor ? rect.X + Hud.Window.CursorX - PickedUpAtX : rect.X);
						float y = rect.Y; //(isAreaOnCursor ? rect.Y + Hud.Window.CursorY - PickedUpAtY : rect.Y);
						float w = rect.Width;
						float h = rect.Height;
						
						if (isAreaOnCursor)
						{
							x += Hud.Window.CursorX - PickedUpAtX;
							y += Hud.Window.CursorY - PickedUpAtY;
							//SelectedBrush.DrawRectangle(x, y, rect.Width, rect.Height);
						}
						else if (ResizePluginArea == area)
						{
							float deltaX = Hud.Window.CursorX - PickedUpAtX;
							float deltaY = Hud.Window.CursorY - PickedUpAtY;
							
							//correct the deltas based on the resize mode
							switch(area.ResizeMode)
							{
								case ResizeMode.FixedRatio:
									deltaY = deltaX * h/w;
									break;
								case ResizeMode.Horizontal:
									deltaY = 0;
									break;
								case ResizeMode.Vertical:
									deltaX = 0;
									break;
							}
							
							if (HoveredResizeRight)
							{
								if (w + deltaX < ResizeHoverSize)
								{
									deltaX = ResizeHoverSize - w;
									deltaY = deltaX * h/w;
								}
								
								if (h + deltaY < ResizeHoverSize)
								{
									deltaY = ResizeHoverSize - h;
									deltaX = deltaY * w/h;
								}

								w += deltaX;
								h += deltaY;
							}	
							else if (HoveredResizeLeft)
							{
								if (w - deltaX < ResizeHoverSize)
								{
									deltaX = w - ResizeHoverSize;
									deltaY = deltaX * h/w;
								}
								if (h - deltaY < ResizeHoverSize)
								{
									deltaY = h - ResizeHoverSize;
									deltaX = deltaY * w/h;
								}

								x += deltaX;
								w -= deltaX;
								h -= deltaY;
							}
								
							//debug							
							//TextLayout test = DisabledFont.GetTextLayout(string.Format("Δ({0},{1})\n=({2},{3})", deltaX, deltaY, Hud.Window.CursorX - PickedUpAtX, Hud.Window.CursorY - PickedUpAtY));
							//DisabledFont.DrawText(test, Hud.Window.CursorX, Hud.Window.CursorY - test.Metrics.Height);
						}

						IBrush brush = area.DeleteOnDisable ?
							(highlighted ? TemporarySelectedBrush : TemporaryBrush) :
							(area.Enabled ? (highlighted ? EnabledSelectedBrush : EnabledBrush) : (highlighted ? DisabledSelectedBrush : DisabledBrush )); //area.Enabled ? EnabledBrush : DisabledBrush;
						IFont font = area.Enabled ? (isAreaOnCursor ? EnabledSelectedFont : (isAnyHighlighted && !highlighted ? EnabledUnselectedFont : EnabledFont)) : (isAreaOnCursor ? DisabledSelectedFont : (isAnyHighlighted && !highlighted ? DisabledUnselectedFont : DisabledFont));
						IFont drag = area.Enabled ? EnabledDragFont : DisabledDragFont;
						
						TextLayout layout = font.GetTextLayout(pluginName + "." + area.Name);
						font.DrawText(layout, x, y - layout.Metrics.Height - 2);
						brush.DrawRectangle(x, y, w, h);
						
						if (area.ResizeMode != ResizeMode.Off && highlighted)
						{
							//DisabledSelectedBrush.DrawRectangle(x + w - ResizeHoverSize, y + h - ResizeHoverSize, ResizeHoverSize, ResizeHoverSize);
							//DisabledSelectedBrush.DrawRectangle(x, y + h - ResizeHoverSize, ResizeHoverSize, ResizeHoverSize);
							float strokeCorrection = brush.StrokeWidth; // + 1;
							
							TextLayout arrow = drag.GetTextLayout(CornerBottomRight);
							drag.DrawText(arrow, x + w - arrow.Metrics.Width + strokeCorrection, y + h - arrow.Metrics.Height*0.85f + strokeCorrection); // + brush.StrokeWidth
							
							arrow = drag.GetTextLayout(CornerBottomLeft);
							drag.DrawText(arrow, x - strokeCorrection, y + h - arrow.Metrics.Height*0.85f + strokeCorrection); // + brush.StrokeWidth
							
							//draw resize arrows
							if (HoveredResizeRight) 
							{
								arrow = SelectedFont.GetTextLayout(area.ResizeMode == ResizeMode.Horizontal ? ArrowHorizontal : (area.ResizeMode == ResizeMode.Vertical ? ArrowVertical : ArrowBottomRight));
								SelectedFont.DrawText(arrow, Hud.Window.CursorX - arrow.Metrics.Width, Hud.Window.CursorY - arrow.Metrics.Height);
							}
							else if (HoveredResizeLeft)
							{
								arrow = SelectedFont.GetTextLayout(area.ResizeMode == ResizeMode.Horizontal ? ArrowHorizontal : (area.ResizeMode == ResizeMode.Vertical ? ArrowVertical : ArrowBottomLeft));
								SelectedFont.DrawText(arrow, Hud.Window.CursorX, Hud.Window.CursorY - arrow.Metrics.Height);
							}
						}
					}
				}
			}
			
        }
		
		public void OnLeftMouseDown()
		{
			LButtonPressed = true; //CursorPluginArea == null; //

			//if (!AllowDragAndDrop) return;
			if (!EditMode)
				return;
			
			if (CursorPluginArea == null && ResizePluginArea == null && HoveredPluginArea is object)
			{
				PickedUpAtX = Hud.Window.CursorX;
				PickedUpAtY = Hud.Window.CursorY;

				if (HoveredResizeLeft || HoveredResizeRight)
				{
					//drag resize
					ResizePluginArea = HoveredPluginArea;
				}
				else
				{
					//CursorPlugin = HoveredPlugin;
					CursorPluginArea = HoveredPluginArea;
					//PickedUpAtX = Hud.Window.CursorX;
					//PickedUpAtY = Hud.Window.CursorY;
					//PickUp();
				}
			}
		}
		
		public void OnLeftMouseUp()
		{
			LButtonPressed = false;

			//if something is already on the cursor, let it be placed even if not in Modify mode
			//if (!EditMode)
			//	return;
			
			if (CursorPluginArea is object)
			{
				CursorPluginArea.Move(Hud.Window.CursorX - PickedUpAtX, Hud.Window.CursorY - PickedUpAtY);
				CursorPluginArea = null;
				//PutDown(true);

				if (AutoSaveConfigChanges)
					ConfigChanged = true;
			}
			else if (ResizePluginArea is object)
			{
				float deltaX = Hud.Window.CursorX - PickedUpAtX;
				float deltaY = Hud.Window.CursorY - PickedUpAtY;
				
				//correct the deltas based on the resize mode
				float w = ResizePluginArea.Rectangle.Width;
				float h = ResizePluginArea.Rectangle.Height;
				switch(ResizePluginArea.ResizeMode)
				{
					case ResizeMode.FixedRatio:
						deltaY = deltaX * h/w;
						break;
					case ResizeMode.Horizontal:
						deltaY = 0;
						break;
					case ResizeMode.Vertical:
						deltaX = 0;
						break;
				}
				
				//enforce minimum dimensions
				float x = ResizePluginArea.Rectangle.X;
				if (HoveredResizeRight)
				{
					if (w + deltaX < ResizeHoverSize)
					{
						deltaX = ResizeHoverSize - w;
						deltaY = deltaX * h/w;
					}
					
					if (h + deltaY < ResizeHoverSize)
					{
						deltaY = ResizeHoverSize - h;
						deltaX = deltaY * w/h;
					}

					w += deltaX;
					h += deltaY;
					
					ResizePluginArea.Rectangle = new RectangleF(x, ResizePluginArea.Rectangle.Y, w, h);
				}
				else if (HoveredResizeLeft)
				{								
					//w -= Hud.Window.CursorX - PickedUpAtX;
					if (w - deltaX < ResizeHoverSize)
					{
						deltaX = w - ResizeHoverSize;
						deltaY = deltaX * h/w;
					}
					if (h - deltaY < ResizeHoverSize)
					{
						//h = ResizeHoverSize;
						deltaY = h - ResizeHoverSize;
						deltaX = deltaY * w/h;
					}

					x += deltaX;
					w -= deltaX;
					h -= deltaY;
					
					ResizePluginArea.Rectangle = new RectangleF(x, ResizePluginArea.Rectangle.Y, w, h);
				}
				
				/*if (HoveredResizeRight)
					//ResizePluginArea.SetRectangle(ResizePluginArea.Rectangle.X, ResizePluginArea.Rectangle.Y, ResizePluginArea.Rectangle.Width + Hud.Window.CursorX - PickedUpAtX, ResizePluginArea.Rectangle.Height + Hud.Window.CursorY - PickedUpAtY);
					ResizePluginArea.Rectangle = new RectangleF(ResizePluginArea.Rectangle.X, ResizePluginArea.Rectangle.Y, w, h);
				else if (HoveredResizeLeft)
					//ResizePluginArea.SetRectangle(ResizePluginArea.Rectangle.X + Hud.Window.CursorX - PickedUpAtX, ResizePluginArea.Rectangle.Y, ResizePluginArea.Rectangle.Width - (Hud.Window.CursorX - PickedUpAtX), ResizePluginArea.Rectangle.Height + Hud.Window.CursorY - PickedUpAtY);
					ResizePluginArea.Rectangle = new RectangleF(ResizePluginArea.Rectangle.X + deltaX, ResizePluginArea.Rectangle.Y, ResizePluginArea.Rectangle.Width - deltaX, ResizePluginArea.Rectangle.Height + deltaY);*/				
				ResizePluginArea = null;
				
				if (AutoSaveConfigChanges)
					ConfigChanged = true;
			}
		}
		
		//cursor bookkeeping
		//requires area != null
		public void PickUp(MovableArea area = null)
		{
			if (area == null)
			{
				if (HoveredPluginArea == null)
					return;
				
				area = HoveredPluginArea;
			}
			
			CursorPluginArea = area;
			ResizePluginArea = null;
			PickedUpAtX = Hud.Window.CursorX;
			PickedUpAtY = Hud.Window.CursorY;
			LButtonPressed = true;
		}
		
		//cursor bookkeeping, does not apply changes like OnLeftMouseUp does
		public void PutDown()
		{
			CursorPluginArea = null;
			ResizePluginArea = null;
			//LButtonPressed = false;
		}
		
		public void SnapTo(float x, float y)
		{
			if (CursorPluginArea is object)
			{
				float deltaX = CursorPluginArea.Rectangle.X - PickedUpAtX;
				float deltaY = CursorPluginArea.Rectangle.Y - PickedUpAtY;
	
				PickedUpAtX = x + deltaX;
				PickedUpAtY = y + deltaY;
			}
		}
		
		public void Configure(string s, int index, float x, float y, float w, float h, bool enabled = true)
		{
			Config.Add(new Tuple<string, int, float, float, float, float, bool>(s, index, x, y, w, h, enabled));
		}

		public void ToggleArea(MovableArea area)
		{
			area.Enabled = !area.Enabled;
			
			if (AutoSaveConfigChanges && area.SaveToConfig)
				ConfigChanged = true;
		}
		
		public bool IsHovered(MovableArea area)
		{
			return Hud.Window.CursorInsideRect(area.Rectangle.X, area.Rectangle.Y, area.Rectangle.Width, area.Rectangle.Height);
		}
		
		public MovableArea CreateArea(IMovable owner, string areaName, RectangleF rect, bool enabledAtStart, bool saveToConfig, ResizeMode resize = ResizeMode.Off, ClipState clipState = ClipState.BeforeClip)
		{
			if (MovablePlugins.ContainsKey(owner))
			{
				MovableArea area = new MovableArea(areaName) 
				{ 
					Owner = owner, 
					Enabled = enabledAtStart, 
					Rectangle = rect, 
					SaveToConfig = saveToConfig, 
					ResizeMode = resize, 
					ClipState = clipState
				};
				MovablePlugins[owner].Add(area);
				return area;
			}
			
			return null;
			//return MovableAreas.Count - 1;
		}
		
		public void DeleteArea(MovableArea area)
		{
			area.DeleteOnDisable = true;
			area.Enabled = false;
			
			/*if (CursorPluginArea == area)
				CursorPluginArea = null;
			else if (ResizePluginArea == area)
				ResizePluginArea = null;
			
			if (HoveredPluginArea == area)
				HoveredPluginArea = null;*/
		}
		
		public string ConfigToString()
		{
			TextBuilder.Clear();
			TextBuilder.Append("/*\n\tThis file contains \"movable\" plugin positions and enabled/disabled state settings.\n\tChange the file extension from .txt to .cs and move this file into the TurboHUD/plugins/Razor/Movable folder\n*/\n\n");
			TextBuilder.Append("using Turbo.Plugins.Default;\n\n");
			TextBuilder.Append("namespace Turbo.Plugins.Razor.Movable\n{\n");
			//TextBuilder.AppendFormat("\tpublic class {0} : BasePlugin, ICustomizer\n\t{\n", ConfigFileName);
			TextBuilder.AppendFormat("\tpublic class {0} : BasePlugin, ICustomizer\n", ConfigFileName);
			TextBuilder.Append("\t{\n");
			TextBuilder.AppendFormat("\t\tpublic {0}() ", ConfigFileName);
			TextBuilder.Append("{ Enabled = true; }\n\n");
			TextBuilder.Append("\t\tpublic override void Load(IController hud) { base.Load(hud); }\n\n");
			TextBuilder.Append("\t\tpublic void Customize()\n\t\t{\n");
			//TextBuilder.AppendFormat("\t\t\tHud.RunOnPlugin<{0}>(plugin =>\n\t\t\t{\n", this.GetType().Name);
			TextBuilder.AppendFormat("\t\t\tHud.RunOnPlugin<{0}>(plugin =>\n", this.GetType().Name);
			TextBuilder.Append("\t\t\t{\n");
			//TextBuilder.Append(string.Format("\t\t\tHud.RunOnPlugin<{0}>(plugin =>\n\t\t\t{\n", this.GetType().Name)); //"\t\t\tHud.RunOnPlugin<MovableController>(plugin =>\n\t\t\t{\n"
			TextBuilder.Append("\t\t\t\t//Configure(string pluginName, int areaIndex, float x, float y, float w, float h, bool enabled = true)\n");
			
			foreach (IMovable f in MovablePlugins.Keys)
			{
				string pluginName = f.GetType().Name;
				List<MovableArea> areas = MovablePlugins[f];
				for (int i = 0; i < areas.Count; ++i)
				{
					MovableArea area = areas[i];
					
					if (area.SaveToConfig)
					{
						//TextBuilder.AppendFormat("\t\t\t\t//{0}\n", area.Name);
						TextBuilder.AppendFormat("\t\t\t\tplugin.Configure(\"{0}\", {1}, {2}f, {3}f, {4}f, {5}f, {6}); //{7}\n", pluginName, i, (int)area.Rectangle.X, (int)area.Rectangle.Y, (int)area.Rectangle.Width, (int)area.Rectangle.Height, area.Enabled.ToString().ToLower(), area.Name);
					}
				}
			}

			TextBuilder.Append("\t\t\t});\n\t\t}\n\t}\n}");
			
			return TextBuilder.ToString();
		}
		
		/*public IPlugin GetPlugin(string pluginName)
		{
			return Hud.AllPlugins.FirstOrDefault(p => p.GetType().Name == pluginName);
		}*/
    }
}