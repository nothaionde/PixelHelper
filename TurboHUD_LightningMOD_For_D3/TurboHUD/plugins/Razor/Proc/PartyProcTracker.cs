//using SharpDX;
using SharpDX.DirectInput;
using SharpDX.DirectWrite;
using SharpDX.Direct2D1;
using System;
using System.Collections.Generic;
using System.Drawing; //RectangleF
using System.Globalization;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;

using Turbo.Plugins.Default;
using Turbo.Plugins.Razor.Movable;

namespace Turbo.Plugins.Razor.Proc
{
	public class PartyProcTracker : BasePlugin, IAfterCollectHandler, INewAreaHandler, IMovable, IInGameTopPainter//, IMonsterKilledHandler //, IInGameWorldPainter
    {
		public bool ShowTracker { get; set; } = true; //show/hide the countdown bars but keep recording the data to share with other plugins regardless
		public bool SoundsEnabled { get; set; } = false; //global on/off switch for playing proc sound alerts
		public bool ShowExtraLives { get; set; } = true;
		
		//public bool TakeScreenshot { get; set; } = false; //take a screenshot whenever procs happen
		//public string SubFolderName { get; set; } = "capture_proc";
		
		//shortcuts for defining strings that are used in many ProcRule definitions
		public string FileProcYou { get; set; } = "ProcYou.wav";
		public string TTS_SayYouHaveProcced { get; set; } = "Сработала пасивка";
		public string TTS_SayHasProcced { get; set; } = "<name> пасивка";
		
		public string SoundFileDeathMe { get; set; }
		public string SoundFileDeathOther { get; set; }
		public string TTS_DeathMe { get; set; } = "Ты умер";
		public string TTS_DeathOther { get; set; } = "<name> умер";
		
		public string LifeSymbol { get; set; } = "🖤"; //💔❤♥♡💗💖💕🖤
		public string LifeLostSymbol { get; set; } = "❤";
		public IFont LifeFont { get; set; }
		public IFont LifeLostFont { get; set; }
		
		public ProcRule AncestorsGraceRule { get; set; } //set this to null in Customize() or comment out AncestorsGraceRule = in Load() if you don't want to track this
		public ProcRule SkeletonKingsPauldronsRule { get; set; } //set this to null in Customize() or comment out SkeletonKingsPauldronsRule = in Load() if you don't want to track this
		public ProcRule EnchantressRule { get; set; }
		public ProcRule ScoundrelRule { get; set; }
		public ProcRule TemplarRule { get; set; }

		public float Gap { get; set; } = 10f; //spacing inbetween each timer bar
		public float BarWidth { get; set; } = 45f; //a ratio of the screen width for the width of the proc timer bars
		public float BarHeight { get; set; } = 10f; //a ratio of the screen height for the height of the proc timer bars
		public float IconSizeMultiplier { get; set; } = 0.55f; //a ratio of the full size of the proc icons
		
		public IFont TimeLeftFont { get; set; }
		public IFont PlayerFont { get; set; }
		public IBrush TimerHigh { get; set; }
		public IBrush TimerLow { get; set; }
		public IBrush TimerBg { get; set; }
		public IBrush SkillBorderLight { get; set; }
		public IBrush SkillBorderDark { get; set; }
		
		public Dictionary<uint, PlayerProcInfo> Snapshots { get; private set; } = new Dictionary<uint, PlayerProcInfo>();
		
		public IBrush ShadowBrush { get; set; }
		public IBrush BorderBrush { get; set; }
		public IBrush ProcBrushDefault { get; set; }
		public static Dictionary<HeroClass, int[]> HeroColors { get; set; } = new Dictionary<HeroClass, int[]>() 
		{
			{HeroClass.Barbarian, new int[3] {255, 128, 64}}, //255, 67, 0
			{HeroClass.Crusader, new int[3] {0, 200, 250}}, //0, 200, 250
			{HeroClass.DemonHunter, new int[3] {0, 100, 255}}, //0, 0, 255
			{HeroClass.Monk, new int[3] {252, 239, 0}}, //255, 255, 0
			{HeroClass.Necromancer, new int[3] {252, 235, 191}}, //0, 190, 190 //240, 240, 240
			{HeroClass.WitchDoctor, new int[3] {163, 244, 65}},
			{HeroClass.Wizard, new int[3] {153, 51, 255}}
		};
		public Dictionary<HeroClass, IBrush> ProcBrush { get; set; }
		
		private IPlugin[] NotifyPlugins;
		private Dictionary<uint, int> PotentiallyDead = new Dictionary<uint, int>();
		//private Dictionary<HeroClass, ProcRule[]> ProcRules; // = new Dictionary<HeroClass, ProcRule[]>();
		private ActorSnoEnum[] SimulacrumSno = new ActorSnoEnum[] {
			ActorSnoEnum._p6_necro_simulacrum_a_set, //2.6.9+ haunted visions
			ActorSnoEnum._p6_necro_simulacrum_a, //old haunted visions
			ActorSnoEnum._p6_necro_simulacrum_male, //shouldn't need these
			ActorSnoEnum._p6_necro_simulacrum_female, //shouldn't need these
			ActorSnoEnum._p6_necro_simulacrum_norune //shouldn't need these
		};
		private Dictionary<HeroClass, List<ProcRule>> ProcRules;
		private float texture_width;
		private float texture_height;
		private float bar_width;
		private float bar_height;
		private int buffer_delay = 30; //in game ticks
		private bool added_special_rules = false;
		//private Dictionary<uint, uint> SimState = new Dictionary<uint, uint>();
		
        public PartyProcTracker()
        {
            Enabled = true;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
			
			//Snapshots = new Dictionary<uint, ProcInfo>();

			ProcBrush = new Dictionary<HeroClass, IBrush>();
			foreach(KeyValuePair<HeroClass, int[]> entry in HeroColors)
				ProcBrush.Add(entry.Key, Hud.Render.CreateBrush(200, entry.Value[0], entry.Value[1], entry.Value[2], 0));
			
			ShadowBrush = Hud.Render.CreateBrush(255, 0, 0, 0, 0); //change the opacity dynamically
			BorderBrush = Hud.Render.CreateBrush(255, 255, 0, 0, 2);
			
			ProcBrushDefault = Hud.Render.CreateBrush(200, 255, 0, 0, 0);

			TimeLeftFont = Hud.Render.CreateFont("tahoma", 7, 255, 255, 255, 255, false, false, 255, 0, 0, 0, true);
			PlayerFont = Hud.Render.CreateFont("tahoma", 8, 255, 255, 255, 255, false, false, 255, 0, 0, 0, true);
			LifeFont = Hud.Render.CreateFont("tahoma", 7f, 255, 242, 15, 15, false, false, 175, 0, 0, 0, true);
			LifeLostFont = Hud.Render.CreateFont("tahoma", 7f, 255, 242, 15, 15, false, false, 175, 0, 0, 0, true);

			AncestorsGraceRule = new ProcRule(Hud.Sno.SnoPowers.AncestorsGrace.Sno) { IconIndex = 0, IsAvailable = CheckAncestorsGrace, IsProcced = CheckAncestorsGraceProc, TTSMe = "Ancestors grace destroyed", TTSOther = "<name> lost Ancestor's Grace" };
			SkeletonKingsPauldronsRule = new ProcRule(334883) { IconIndex = 1, IsExtraLife = false, TTSMe = "Pauldrons", TTSOther = "<name> Pauldrons", UseItemTexture = Hud.Sno.SnoItems.Unique_Shoulder_103_x1 };
			EnchantressRule = new ProcRule(Hud.Sno.SnoPowers.Generic_EnchantressCheatDeathPassive.Sno) { IconIndex = 1, IsExtraLife = true, TTSMe = "Follower Prock", TTSOther = "<name> Follower Prock" };
			ScoundrelRule = new ProcRule(Hud.Sno.SnoPowers.Generic_ScoundrelCheatDeathPassive.Sno) { IconIndex = 1, IsExtraLife = true, TTSMe = "Follower Prock", TTSOther = "<name> Follower Prock" };
			TemplarRule = new ProcRule(Hud.Sno.SnoPowers.Generic_TemplarCheatDeathPassive.Sno) { IconIndex = 1, IsExtraLife = true, TTSMe = "Follower Prock", TTSOther = "<name> Follower Prock" };
			
			ProcRules = new Dictionary<HeroClass, List<ProcRule>>()
			{
				{ HeroClass.Wizard, new List<ProcRule>() { 
										new ProcRule(Hud.Sno.SnoPowers.Wizard_Passive_UnstableAnomaly.Sno) { IconIndex = 1, SoundFileMe = FileProcYou, TTSOther = TTS_SayHasProcced }, // Wizard_Passive_UnstableAnomaly (Cooldown)
										new ProcRule(485318) { IconIndex = 1, TTSMe = "Firebird", TTSOther = "<name> procked Firebird" }, //Firebird 2pc //359580
									} }, 
				{ HeroClass.Monk, new List<ProcRule>() { 
									new ProcRule(Hud.Sno.SnoPowers.Monk_Passive_NearDeathExperience.Sno) { IconIndex = 1, SoundFileMe = FileProcYou, TTSOther = TTS_SayHasProcced },
									//new ProcRule(Hud.Sno.SnoPowers.Monk_SweepingWind.Sno) { IconIndex = 0, IsExtraLife = false, SoundFileMe = FileProcYou, TTSOther = TTS_SayHasProcced }, //testing only
									//new ProcRule(Hud.Sno.SnoPowers.Monk_Epiphany.Sno) { IconIndex = 0, IsExtraLife = false, SoundFileMe = FileProcYou, TTSOther = TTS_SayHasProcced } //testing only
								} },
				{ HeroClass.Barbarian, new List<ProcRule>() { 
											new ProcRule(Hud.Sno.SnoPowers.Barbarian_Passive_NervesOfSteel.Sno) { IconIndex = 1, SoundFileMe = FileProcYou, TTSOther = TTS_SayHasProcced },
											//new ProcRule(Hud.Sno.SnoPowers.AncestorsGrace.Sno) { IconIndex = 0, IsAvailable = CheckAncestorsGrace, IsProcced = CheckAncestorsGraceProc, TTSMe = "Ancestors grace destroyed", TTSOther = "<name> lost Ancestor's Grace" },
										} },
				{ HeroClass.Necromancer, new List<ProcRule>() { 
											new ProcRule(Hud.Sno.SnoPowers.Necromancer_Passive_FinalService.Sno) { IconIndex = 1, SoundFileMe = FileProcYou, TTSOther = TTS_SayHasProcced }, 
											new ProcRule(Hud.Sno.SnoPowers.Necromancer_Simulacrum.Sno) { IconIndex = 1, IsAvailable = CheckSimulacrumSacrifice, IsProcced = CheckSimulacrumProc, UseCooldownTime = Hud.Sno.SnoPowers.Necromancer_Simulacrum.Sno, /*SoundFileMe = "Sim.wav"*/TTSMe = "Simulacrum lost", TTSOther = "<name> lost Sim" },
										} },
				{ HeroClass.Crusader, new List<ProcRule>() { 
										new ProcRule(Hud.Sno.SnoPowers.Crusader_Passive_Indestructible.Sno) { IconIndex = 1, SoundFileMe = FileProcYou, TTSOther = TTS_SayHasProcced },
										new ProcRule(Hud.Sno.SnoPowers.Crusader_AkaratsChampion.Sno) { IconIndex = 11, IsAvailable = CheckAkaratProphet, IsProcced = CheckAkaratProphetProc, /*UseCooldownTime = Hud.Sno.SnoPowers.Crusader_AkaratsChampion.Sno,*/ TTSMe = "Prophet", TTSOther = "<name> propheted" }, //Prophet
									} },
				{ HeroClass.DemonHunter, new List<ProcRule>() { 
											new ProcRule(Hud.Sno.SnoPowers.DemonHunter_Passive_Awareness.Sno) { IconIndex = 1, SoundFileMe = FileProcYou, TTSOther = TTS_SayHasProcced }, //Awareness
											new ProcRule(Hud.Sno.SnoPowers.BeckonSail.Sno) { IconIndex = 1, TTSMe = "Beckon Sail", TTSOther = "<name> procked Beckon Sail" }, 
											//new ProcRule(Hud.Sno.SnoPowers.DemonHunter_Vengeance.Sno) { IconIndex = 0, SoundFileMe = FileProcYou, TTSOther = TTS_SayHasProcced }
										} },
				{ HeroClass.WitchDoctor, new List<ProcRule>() { 
											new ProcRule(Hud.Sno.SnoPowers.WitchDoctor_Passive_SpiritVessel.Sno) { IconIndex = 1, SoundFileMe = FileProcYou, TTSOther = TTS_SayHasProcced },
										} },
				{ HeroClass.None, new List<ProcRule>() },
			};
			
			TimerHigh = Hud.Render.CreateBrush(255, 0, 255, 100, 0);
			TimerLow = Hud.Render.CreateBrush(255, 255, 0, 0, 0);
			TimerBg = Hud.Render.CreateBrush(100, 0, 0, 0, 0);
			
			SkillBorderLight = Hud.Render.CreateBrush(200, 255, 50, 50, 1); //95, 95, 95 //235, 227, 164 //138, 135, 109
			SkillBorderDark = Hud.Render.CreateBrush(150, 0, 0, 0, 1);
        }
		
		public void OnNewArea(bool newGame, ISnoArea area)
		{
			if (newGame) {
				Reset();
			}
		}
		
		public void AfterCollect()
		{
			if (!Hud.Game.IsInGame)
				return;
			
			if (!added_special_rules)
			{
				added_special_rules = true;
				
				foreach (List<ProcRule> rules in ProcRules.Values) //player.HeroClassDefinition.HeroClass
				{
					if (AncestorsGraceRule is object)
						rules.Add(AncestorsGraceRule);
					if (SkeletonKingsPauldronsRule is object)
						rules.Add(SkeletonKingsPauldronsRule);
					if (EnchantressRule is object)
						rules.Add(EnchantressRule);
					if (ScoundrelRule is object)
						rules.Add(ScoundrelRule);
					if (TemplarRule is object)
						rules.Add(TemplarRule);
				}
			}

			int current = Hud.Game.CurrentGameTick;
			
			if (PotentiallyDead.Any())
			{
				foreach (uint id in PotentiallyDead.Keys) //.ToArray()
				{
					if (current - PotentiallyDead[id] > buffer_delay && Snapshots.ContainsKey(id)) //most likely dead
					{
						//PotentiallyDead.Remove(id);
						Snapshots.Remove(id);
						
						if (SoundsEnabled)
						{
							var player = Hud.Game.Players.FirstOrDefault(p => p.HeroId == id);
							if (player is object)
							{
								if (player.IsMe)
								{
									if (!string.IsNullOrEmpty(SoundFileDeathMe))
									{
										var sound = Hud.Sound.LoadSoundPlayer(SoundFileDeathMe);
										ThreadPool.QueueUserWorkItem(state =>
										{
										   try { sound.PlaySync(); }
										   catch (Exception) {}
										});
									}
									else if (!string.IsNullOrEmpty(TTS_DeathMe))
									{
										Hud.Sound.Speak(TTS_DeathMe);
									}
								}
								else
								{
									if (!string.IsNullOrEmpty(SoundFileDeathOther))
									{
										var sound = Hud.Sound.LoadSoundPlayer(SoundFileDeathOther);
										ThreadPool.QueueUserWorkItem(state =>
										{
										   try { sound.PlaySync(); }
										   catch (Exception) {}
										});
									}
									else if (!string.IsNullOrEmpty(TTS_DeathOther))
									{
										Hud.Sound.Speak(TTS_DeathOther.Replace("<name>", player.BattleTagAbovePortrait));
									}
								}
							}
						}
					}
				}
			}
			
			foreach (IPlayer player in Hud.Game.Players)
			{
				if (player.IsDeadSafeCheck)
				{
					if (!PotentiallyDead.ContainsKey(player.HeroId))
						PotentiallyDead.Add(player.HeroId, current);
				}
				else
				{
					if (PotentiallyDead.ContainsKey(player.HeroId))
						PotentiallyDead.Remove(player.HeroId);
					
					if (player.IsMe || (player.HasValidActor && player.CoordinateKnown)) // && player.SnoArea == Hud.Game.Me.SnoArea
					{
						PlayerProcInfo info = null;
						if (!Snapshots.ContainsKey(player.HeroId))
						{
							info = new PlayerProcInfo(player);
							Snapshots.Add(player.HeroId, info);
						}
						else
						{
							info = Snapshots[player.HeroId];
						}
						
						info.LivesRemaining = 0;
						info.LivesSpent = 0;
						//info.Debug = string.Empty;
						
						foreach (ProcRule rule in ProcRules[info.HeroClass]) //player.HeroClassDefinition.HeroClass
						{
							if (rule.IsAvailable is object)
							{
								if (!rule.IsAvailable(player))
									continue;
							}
							else if (!player.Powers.BuffIsActive(rule.Sno))
								continue;
								
							//count lives
							//if (player.Powers.BuffIsActive(rule.Sno))
							//{
							if (rule.IsProcced is object ? rule.IsProcced(player) : player.Powers.BuffIsActive(rule.Sno, rule.IconIndex) != rule.CheckInactive) //(rule.CheckInactive ? !player.Powers.BuffIsActive(rule.Sno, rule.IconIndex) : player.Powers.BuffIsActive(rule.Sno, rule.IconIndex)))
							{
								IBuff buff = player.Powers.GetBuff(rule.Sno);
								
								//sanity check
								if (buff == null)
									continue;

								if (rule.IsExtraLife)
									++info.LivesSpent;
								
								//already have a record of this proc
								if (info.Procs.ContainsKey(rule.Sno))
								{
									var proc = info.Procs[rule.Sno];
									bool expired = current > proc.FinishTick;
									
									if (rule.UseCooldownTime > 0)
									{
										var skill = player.Powers.UsedSkills.FirstOrDefault(pwr => pwr.SnoPower.Sno == rule.UseCooldownTime);
										if (skill is object)
										{
											if (skill.IsOnCooldown)
											{
												proc.StartTick = skill.CooldownStartTick;
												proc.FinishTick = skill.CooldownFinishTick;
											}
										}
									}
									else
									{
										var index = rule.IconIndex;
											
										if (rule.UseBuff is object)
										{
											buff = player.Powers.GetBuff(rule.UseBuff.PowerSno);
											if (rule.UseBuff.IconIndex.HasValue)
												index = rule.UseBuff.IconIndex.Value;
										}
										
										proc.StartTick = current - (int)(buff.TimeElapsedSeconds[index]*60);
										proc.FinishTick = current + (int)(buff.TimeLeftSeconds[index]*60); //resync buff info
									}
									
									//update timestamp
									proc.LastSeenTick = current;
									
									//the data in the record already expired and the current time remaining is more than a fraction of a second
									if (expired && (proc.FinishTick - proc.StartTick > 15)) //if (current > proc.FinishTick && buff.TimeLeftSeconds[rule.IconIndex] > 0.3)
										OnProcStart(proc, player); //new proc notification
								}
								else
								{
									ProcInfo pinfo = null;
									
									if (rule.ShowTimeLeft)
									{
										if (rule.UseCooldownTime > 0)
										{
											var skill = player.Powers.UsedSkills.FirstOrDefault(pwr => pwr.SnoPower.Sno == rule.UseCooldownTime);
											if (skill is object)
											{
												if (skill.IsOnCooldown)
												{
													pinfo = new ProcInfo(rule)
													{
														StartTick = skill.CooldownStartTick, //Hud.Game.CurrentGameTick,
														FinishTick = skill.CooldownFinishTick,
														LastSeenTick = current,
														Texture = rule.UseItemTexture is object ? Hud.Texture.GetItemTexture(rule.UseItemTexture) : Hud.Texture.GetTexture(rule.UseTextureId > 0 ? rule.UseTextureId : buff.SnoPower.NormalIconTextureId),
													};
												}
											}
										}
										else 
										{
											var index = rule.IconIndex;
											
											if (rule.UseBuff is object)
											{
												buff = player.Powers.GetBuff(rule.UseBuff.PowerSno);
												if (rule.UseBuff.IconIndex.HasValue)
													index = rule.UseBuff.IconIndex.Value;
											}
											
											pinfo = new ProcInfo(rule)
											{
												StartTick = current - (int)(buff.TimeElapsedSeconds[index]*60),
												FinishTick = current + (int)(buff.TimeLeftSeconds[index]*60),
												LastSeenTick = current,
												Texture = rule.UseItemTexture is object ? Hud.Texture.GetItemTexture(rule.UseItemTexture) : Hud.Texture.GetTexture(rule.UseTextureId > 0 ? rule.UseTextureId : buff.SnoPower.NormalIconTextureId),
											};
										}
									}
									else
									{
										pinfo = new ProcInfo(rule)
										{
											LastSeenTick = current,
											Texture = rule.UseItemTexture is object ? Hud.Texture.GetItemTexture(rule.UseItemTexture) : Hud.Texture.GetTexture(rule.UseTextureId > 0 ? rule.UseTextureId : buff.SnoPower.NormalIconTextureId),
											//TextureId = buff.SnoPower.NormalIconTextureId,
										};
									}
									
									if (pinfo is object)
									{
										info.Procs.Add(rule.Sno, pinfo);
										OnProcStart(pinfo, player);
										//++info.ProcCount;
									}
								}
							}
							else //if buff is active
							{
								if (rule.IsExtraLife)
								{
									++info.LivesRemaining;
									//info.Debug += " " + rule.Sno;
								}

								if (info.Procs.ContainsKey(rule.Sno))
								{
									//buff ended or haven't seen that buff active for over (0.5s)
									var proc = info.Procs[rule.Sno];
									if ((proc.FinishTick > 0 && current > proc.FinishTick) || current > proc.LastSeenTick + buffer_delay)
									{
										info.Procs.Remove(rule.Sno);
										OnProcFinish(proc, player);
									}
								}
							}
						}
					}
					else if (Snapshots.ContainsKey(player.HeroId))
					{
						PlayerProcInfo info = Snapshots[player.HeroId];
						
						//proc cleanup
						//foreach (uint sno in info.Procs.Keys.Where(k => info.Procs[k].FinishTick > 0 && current > info.Procs[k].FinishTick + 10).ToArray()) //add a little margin of error
						foreach (KeyValuePair<uint, ProcInfo> pair in info.Procs.Where(kvp => kvp.Value.FinishTick > 0 && current > kvp.Value.FinishTick + 10).ToArray()) //add a little margin of error
						{
							//if (current > info.Procs[sno].FinishTick + 10) 
							OnProcFinish(pair.Value, player);
							info.Procs.Remove(pair.Key);
						}
					}
				}
			}
			
			//bookkeeping
			//var keys = Snapshots.Keys.Where(k => !Hud.Game.Players.Any(p => p.HeroId == Snapshots[k].HeroId));
			//if (keys is object)
			//{
			foreach (uint key in Snapshots.Keys.Where(k => !Hud.Game.Players.Any(p => p.HeroId == Snapshots[k].HeroId)).ToArray())
				Snapshots.Remove(key);
			//}
		}

		//initialize position and dimension elements
		public void OnRegister(MovableController mover)
		{
			bar_height = (float)(int)(Hud.Window.Size.Height * 0.001667f * BarHeight); //8
			bar_width = (float)(int)(Hud.Window.Size.Width * 0.00155f * BarWidth); //55
			
			ITexture texture = Hud.Texture.GetTexture(Hud.Sno.SnoPowers.Monk_Passive_NearDeathExperience.NormalIconTextureId);
			texture_width = (float)(int)(texture.Width * IconSizeMultiplier);
			texture_height = (float)(int)(texture.Height * IconSizeMultiplier);
			
			float height = Math.Max(bar_height + Gap, texture_height);
			
			IUiElement ui = Hud.Render.GetUiElement("Root.NormalLayer.eventtext_bkgrnd.eventtext_region.title");
			//MovableAreas.Add(new MovableArea("Countdown") { Enabled = ShowTracker, Rectangle = new RectangleF(ui.Rectangle.X - ProcRuleCalculator.StandardIconSize - Gap, ui.Rectangle.Y, ProcRuleCalculator.StandardIconSize + Gap + Hud.Window.Size.Width * 0.00155f * BarWidth + 12, ProcRuleCalculator.StandardIconSize*4 + Gap*3) });
			mover.CreateArea(
				this,
				"Bars", //area name
				new RectangleF(ui.Rectangle.X, ui.Rectangle.Y + Gap, bar_width + texture_width + Gap*2, height*4 + 2*3), //position + dimensions
				ShowTracker, //enabled at start?
				true, //save to config file?
				ResizeMode.Horizontal //resizable?
			);
			
			/*mover.CreateArea(
				this,
				"ExtraLives", //area name
				new RectangleF(ui.Rectangle.X, ui.Rectangle.Y - Gap, bar_width + texture_width + Gap*2, height*4 + 2*3), //position + dimensions
				ShowTracker, //enabled at start?
				true, //save to config file?
				ResizeMode.Horizontal //resizable?
			);*/
		}

		public void PaintArea(MovableController mover, MovableArea area, float deltaX = 0, float deltaY = 0)
        {
			var x = area.Rectangle.X + deltaX;
			var y = area.Rectangle.Y + deltaY;

			var h = bar_height; //((int)(Hud.Window.Size.Height * 0.001667f * BarHeight)
			var w = area.Rectangle.Width - texture_width - Gap*2; //55
			
			int current = Hud.Game.CurrentGameTick;
			
			//foreach (PlayerProcInfo info in Snapshots.Values)
			foreach (IPlayer player in Hud.Game.Players)
			{
				//if (PotentiallyDead.ContainsKey(info.HeroId))
				if (PotentiallyDead.ContainsKey(player.HeroId))
					continue;
				
				if (!Snapshots.ContainsKey(player.HeroId))
					continue;
				
				//pull up all the valid procs in descending order
				PlayerProcInfo info = Snapshots[player.HeroId];
				var procs = info.Procs.Values.Where(pinfo => pinfo.FinishTick > current || !pinfo.Rule.ShowTimeLeft).OrderByDescending(pinfo => pinfo.FinishTick);
				if (procs.Any()) //procs is object && 
				{
					//draw the name
					//TextLayout layout = PlayerFont.GetTextLayout(info.PlayerName); //info.PlayerName //info.HeroId.ToString()
					TextLayout layout = PlayerFont.GetTextLayout(player.BattleTagAbovePortrait);
					PlayerFont.DrawText(layout, x - layout.Metrics.Width - Gap, y + texture_height*0.5f - layout.Metrics.Height*0.5f);
					
					float height = Math.Max(h + Gap, texture_height);
					
					//draw the bars
					foreach (ProcInfo proc in procs)
					{
						TimerBg.DrawRectangle(x + texture_width, y, w + Gap*2, texture_height);
						
						//float width = area.Rectangle.Width; //width_countdown_bar
						//float height = index > 1 ? height_countdown_bar*ActiveBarHeightMultiplier : height_countdown_bar;
						float timeLeft = 0;
						if (proc.Rule.ShowTimeLeft)
						{
							float x2 = x + texture_width + Gap;
							float y2 = y + texture_height*0.5f - h*0.5f;
							TimerBg.DrawRectangle(x2, y2, w, h);
							timeLeft = (float)(proc.FinishTick - current) / 60f;
							float timeTotal = (float)(proc.FinishTick - proc.StartTick) / 60f;
							float timeLeftPct = (float)(timeLeft / timeTotal); //1f; //
							if (timeLeftPct > 0) //this may become negative when you get dc'ed/idled out from a game
							{
								//TimerLow.DrawRectangle(x, y, w * timeLeftPct, h);
								//TimerHigh.Opacity = timeLeftPct;
								ProcBrush[info.HeroClass].DrawRectangle(x2, y2, w * timeLeftPct, h);
								
								//draw countdown text
								layout = TimeLeftFont.GetTextLayout(timeLeft.ToString(timeLeft > 1 ? "F0" : "F1"));
								TimeLeftFont.DrawText(layout, x2 + w*0.5f - layout.Metrics.Width*0.5f, y2 + h*0.5f - layout.Metrics.Height*0.5f);

							}
							SkillBorderDark.DrawRectangle(x2 - 1, y2 - 1, w + 2, h + 2);
							SkillBorderLight.DrawRectangle(x2 - 2, y2 - 2, w + 4, h + 4);
							SkillBorderDark.DrawRectangle(x2 - 3, y2 - 3, w + 6, h + 6);
						}

						//draw icon
						//ITexture texture = Hud.Texture.GetTexture(proc.TextureId);
						if (proc.Texture is object)
						{
							float tWidth = proc.Texture.Width * (texture_height/proc.Texture.Height);
							float tHeight = texture_height;
							float opacity = timeLeft == 0 ? 0.9f : (timeLeft >= 1 || (timeLeft < 1 && (Hud.Game.CurrentRealTimeMilliseconds / 200) % 2 == 0) ? 0.9f : 0.2f);
							proc.Texture.Draw(x, y, tWidth, tHeight, opacity);
							Hud.Texture.DebuffFrameTexture.Draw(x, y, tWidth, tHeight, 0.85f);
							SkillBorderLight.DrawRectangle(x, y, tWidth, tHeight);
						}
						
						y += height + 2; //h + Gap;
					}
				}
			}
        }
		
		public void PaintTopInGame(ClipState clipState)
		{
			if (clipState != ClipState.BeforeClip) return;
			
			if (ShowExtraLives)
			{
				foreach (IPlayer player in Hud.Game.Players)
				{
					string output = GetExtraLivesString(player);
					
					if (!string.IsNullOrEmpty(output))
					{
						TextLayout layout = LifeFont.GetTextLayout(output); //"test " + player.Powers.BuffIsActive(Hud.Sno.SnoPowers.Monk_Passive_NearDeathExperience.Sno) + " " + 
						var rect = Hud.Render.GetUiElement("Root.NormalLayer.portraits.stack.party_stack.portrait_" + player.PortraitIndex + ".icon").Rectangle; //player.PortraitUiElement.Rectangle;
						LifeFont.DrawText(layout, rect.X + rect.Width*0.5f - layout.Metrics.Width*0.5f, rect.Bottom - layout.Metrics.Height*0.15f);
					}
				}
			}
		}
		
		public string GetExtraLivesString(IPlayer player)
		{
			if (!Snapshots.ContainsKey(player.HeroId))
				return string.Empty;

			PlayerProcInfo info = Snapshots[player.HeroId];
			string output = string.Empty;
			
			if (player.IsDeadSafeCheck)
			{
				for (int i = 0; i < (info.LivesRemaining + info.LivesSpent); ++i)
					output += LifeLostSymbol;
			}
			else
			{
				for (int i = 0; i < info.LivesRemaining; ++i)
					output += LifeSymbol;
				for (int i = 0; i < info.LivesSpent; ++i)
					output += LifeLostSymbol;
			}
			
			return output;
			//return info.Debug;
		}
		
		public PlayerProcInfo GetProcInfo(IPlayer player)
		{
			if (!Snapshots.ContainsKey(player.HeroId))
				return null;

			return Snapshots[player.HeroId];
		}
		
		//returns -1 if there were no extra lives to begin with, or the player was never seen
		public int ExtraLivesRemaining(IPlayer player)
		{
			if (!Snapshots.ContainsKey(player.HeroId))
				return -1;
			
			PlayerProcInfo info = Snapshots[player.HeroId];
			
			if (info.LivesSpent + info.LivesRemaining > 0)
				return info.LivesRemaining;
			
			return -1;
		}
		
		//does the player have Simulacrum with Sacrifice rune effect?
		public bool CheckSimulacrumSacrifice(IPlayer player)
		{
			//check for Self-Sacrifice rune and 2pc Carnival set and active buff
			return player.Powers.UsedSkills.Any(skill => skill.SnoPower.Sno == Hud.Sno.SnoPowers.Necromancer_Simulacrum.Sno && (skill.Rune == 4 || player.Powers.BuffIsActive(484301))); //&& player.Powers.BuffIsActive(Hud.Sno.SnoPowers.Necromancer_Simulacrum.Sno, 1);
		}
		
		public bool CheckSimulacrumProc(IPlayer player)
		{
			var info = Snapshots[player.HeroId];
			var simSno = Hud.Sno.SnoPowers.Necromancer_Simulacrum.Sno;
			
			if (!player.Powers.BuffIsActive(simSno)) //BuffIsActive(Hud.Sno.SnoPowers.Necromancer_Simulacrum.Sno, 1)
			{
				if (info.Data.ContainsKey(simSno))
				{
					//are the sims supposed to be summoned?
					if (info.Data[simSno] > 0)
					{
						//are the sims not around? //sanity check
						if (!Hud.Game.Actors.Any(a => a.SummonerAcdDynamicId == player.SummonerId && SimulacrumSno.Contains(a.SnoActor.Sno)))
						{
							var skill = player.Powers.UsedSkills.FirstOrDefault(s => s.SnoPower.Sno == simSno);
							if (skill.IsOnCooldown)
							{
								info.Data[simSno] = 2; //notified
								return true; //draw cooldown bar
							}
							else if (info.Data[simSno] == 1) //didn't notify yet
							{
								//change data value so that this step is not repeated
								info.Data[simSno] = 0;
								
								//notify but don't draw a cooldown bar
								ProcRule rule = ProcRules[HeroClass.Necromancer].FirstOrDefault(r => r.Sno == simSno);
								if (rule is object)
								{
									OnProcStart(new ProcInfo(rule), player);
									//Hud.Sound.Speak("custom notify "+ Hud.Game.Actors.Count(a => a.SummonerAcdDynamicId == Hud.Game.Me.SummonerId && (a.SnoActor.Sno == ActorSnoEnum._p6_necro_simulacrum_a_set || a.SnoActor.Sno == ActorSnoEnum._p6_necro_simulacrum_a))); //testing
								}
							}
						}
					}
					else
					{
						//not on proc, but the buff is down so have to adjust extra life numbers to compensate, otherwise it will flag this ability as being active
						info.LivesSpent++;
						info.LivesRemaining--;
					}
				}
				else
				{
					info.Data.Add(simSno, 0);
				}
			}
			else if (!info.Data.ContainsKey(simSno))
			{
				info.Data.Add(simSno, 1);
			}
			else if (info.Data[simSno] != 1)
			{
				info.Data[simSno] = 1;
			}
			
			return false;
		}
		
		public bool CheckAkaratProphet(IPlayer player)
		{
			//check for Prophet rune or Akkhan's Addendum and active buff
			return player.Powers.UsedSkills.Any(skill => skill.SnoPower.Sno == Hud.Sno.SnoPowers.Crusader_AkaratsChampion.Sno && (skill.Rune == 3 || player.Powers.BuffIsActive(Hud.Sno.SnoPowers.AkkhansAddendum.Sno))); //&& player.Powers.BuffIsActive(Hud.Sno.SnoPowers.Crusader_AkaratsChampion.Sno, 1);
		}
		
		public bool CheckAkaratProphetProc(IPlayer player)
		{
			if (player.Powers.BuffIsActive(Hud.Sno.SnoPowers.Crusader_AkaratsChampion.Sno, 11))
				return true;
			
			if (!player.Powers.BuffIsActive(Hud.Sno.SnoPowers.Crusader_AkaratsChampion.Sno, 1))
			{
				//extra lives from buffs that need to be active need a counter adjustment to show up as inactive
				var info = Snapshots[player.HeroId];
				info.LivesSpent++;
				info.LivesRemaining--;
			}
			
			return false;
		}
		
		public bool CheckAncestorsGrace(IPlayer player)
		{
			PlayerProcInfo info = Snapshots[player.HeroId];
			
			//check for Prophet rune or Akkhan's Addendum and active buff
			if (player.Powers.BuffIsActive(Hud.Sno.SnoPowers.AncestorsGrace.Sno))
			{
				//remember the item id
				var necklace = Hud.Game.Items.FirstOrDefault(x => x.Location == ItemLocation.Neck);
				if (necklace is object)
					info.Data[Hud.Sno.SnoPowers.AncestorsGrace.Sno] = necklace.Seed;

				return true;
			}
			
			if (info.Data.ContainsKey(Hud.Sno.SnoPowers.AncestorsGrace.Sno))
			{
				//check if the item still exists, if so, remove the data entry, otherwise, leave it for proc checks
				if (Hud.Game.Items.Any(x => x.Seed == info.Data[Hud.Sno.SnoPowers.AncestorsGrace.Sno]))
					info.Data.Remove(Hud.Sno.SnoPowers.AncestorsGrace.Sno);
			}
			
			return false;
		}
		
		public bool CheckAncestorsGraceProc(IPlayer player)
		{
			if (!player.Powers.BuffIsActive(Hud.Sno.SnoPowers.AncestorsGrace.Sno))
			{
				PlayerProcInfo info = Snapshots[player.HeroId];
				if (info.Data.ContainsKey(Hud.Sno.SnoPowers.AncestorsGrace.Sno)) //it was worn
				{
					if (!Hud.Game.Items.Any(x => x.Seed == info.Data[Hud.Sno.SnoPowers.AncestorsGrace.Sno]))
					{
						info.Data.Remove(Hud.Sno.SnoPowers.AncestorsGrace.Sno);
						
						//custom notify
						ProcRule rule = ProcRules[HeroClass.Necromancer].FirstOrDefault(r => r.Sno == Hud.Sno.SnoPowers.Necromancer_Simulacrum.Sno);
						if (rule is object)
							OnProcStart(new ProcInfo(rule), player);

						return true;
					}
				}
				
				return true;
			}
			
			return false;
		}
		
		//private void PlaySounds(ProcRule rule, IPlayer player)
		private void OnProcStart(ProcInfo proc, IPlayer player)
		{
			ProcRule rule = proc.Rule;
			PlayerProcInfo info = Snapshots[player.HeroId];
			if (info.ProcCount.ContainsKey(rule.Sno))
				info.ProcCount[rule.Sno]++;
			else
				info.ProcCount[rule.Sno] = 1;
			
			/*if (TakeScreenshot)
			{
				try
                {
                    var fileName = string.Format("proc_{0}_{1}_{2}_{3}_{4}.jpg", rule.Sno, player.BattleTagAbovePortrait, player.HeroId.ToString("D", CultureInfo.InvariantCulture), player.HeroName, Hud.Time.Now.ToString("yyyyMMddHHmmssfff", CultureInfo.InvariantCulture));
                    Hud.Render.CaptureScreenToFile(SubFolderName, fileName);
                }
                catch (Exception) {}
			}*/
			
			//find all plugins that want to be notified
			if (NotifyPlugins == null)
				NotifyPlugins = Hud.AllPlugins.Where(p => p is IProcHandler).ToArray();
			
			//pass them the player proc info
			foreach (var plugin in NotifyPlugins)
			{
				if (plugin.Enabled)
					((IProcHandler)plugin).OnProcStart(proc, player);
			}
			/*double timeElapsed = Hud.Game.CurrentGameTick - info.StartTick;
			double timeLeft = info.FinishTick - Hud.Game.CurrentGameTick;
			foreach (IPlugin plugin in Hud.AllPlugins.Where(p => p.Enabled && p is IProcHandler))
				((IProcHandler)plugin).OnProcStart(rule.Sno, player, timeElapsed, timeLeft);*/
			
			//play sounds
			if (!SoundsEnabled)
				return;

			//Hud.Sound.Speak("test");

			//adapted from RNN's ProcPlayers
			if (rule.Beep)
				Console.Beep(250, 200); 
			else if (player.IsMe)
			{
				if (!string.IsNullOrEmpty(rule.SoundFileMe))
				{
					var sound = Hud.Sound.LoadSoundPlayer(rule.SoundFileMe);
					ThreadPool.QueueUserWorkItem(state =>
					{
					   try { sound.PlaySync(); }
					   catch (Exception) {}
					});
				}
				else if (!string.IsNullOrEmpty(rule.TTSMe))
				{
					Hud.Sound.Speak(rule.TTSMe);
				}
			}
			else
			{
				if (!string.IsNullOrEmpty(rule.SoundFileOther))
				{
					var sound = Hud.Sound.LoadSoundPlayer(rule.SoundFileOther);
					ThreadPool.QueueUserWorkItem(state =>
					{
					   try { sound.PlaySync(); }
					   catch (Exception) {}
					});
				}
				else if (!string.IsNullOrEmpty(rule.TTSOther))
				{
					Hud.Sound.Speak(rule.TTSOther.Replace("<name>", player.BattleTagAbovePortrait));
				}
			}
		}
		
		private void OnProcFinish(ProcInfo proc, IPlayer player)
		{
			if (NotifyPlugins == null)
				NotifyPlugins = Hud.AllPlugins.Where(p => p is IProcHandler).ToArray();
			
			//pass them the player proc info
			foreach (var plugin in NotifyPlugins)
			{
				if (plugin.Enabled)
					((IProcHandler)plugin).OnProcFinish(proc, player);
			}
			
			/*foreach (IPlugin plugin in Hud.AllPlugins.Where(p => p.Enabled && p is IProcHandler))
				((IProcHandler)plugin).OnProcFinish(ruleSno, player);*/
		}
		
		private void Reset()
		{
			Snapshots.Clear();
			PotentiallyDead.Clear();
		}
    }
	

}