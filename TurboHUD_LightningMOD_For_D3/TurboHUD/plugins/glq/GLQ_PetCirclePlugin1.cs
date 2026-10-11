namespace Turbo.Plugins.glq
{
    using SharpDX.Direct2D1;
    using System.Collections.Generic;
    using System.Linq;
    using Turbo.Plugins.Default;

    public class GLQ_PetCirclePlugin1 : BasePlugin, IInGameWorldPainter
    {
        public WorldDecoratorCollection FetishDecorator { get; set; }//鬼娃
        public WorldDecoratorCollection FetishTimerDecorator { get; set; }//鬼娃计时器
        public WorldDecoratorCollection FetishfireShamanDecorator { get; set; }//喷火鬼娃
		public WorldDecoratorCollection FetishfireShamanTimerDecorator { get; set; }//喷火鬼娃计时器
		public WorldDecoratorCollection FetishRangedDecorator { get; set; }//吹箭鬼娃
		public WorldDecoratorCollection FetishRangedTimerDecorator { get; set; }//吹箭鬼娃计时器
		public WorldDecoratorCollection FetishMeleeSycophantsDecorator { get; set; }//被动鬼娃
		public WorldDecoratorCollection FetishMeleeSycophantsTimerDecorator { get; set; }//被动鬼娃计时器
		public WorldDecoratorCollection ZombieDogDecorator { get; set; }//僵尸犬
		public WorldDecoratorCollection GargantuanDecorator { get; set; }//巨尸
		public WorldDecoratorCollection GargantuanTimerDecorator { get; set; }//巨尸计时器
		public WorldDecoratorCollection FetishHexDecorator { get; set; }//妖术
		public WorldDecoratorCollection FetishHexTimerDecorator { get; set; }//妖术计时器
		public WorldDecoratorCollection FetishHexGiantToadDecorator { get; set; }//妖术巨蟾
		public WorldDecoratorCollection FetishHexGiantToadTimerDecorator { get; set; }//妖术巨蟾计时器
		public WorldDecoratorCollection TheGrinReaperDecorator { get; set; }//狞笑死神分身
		public WorldDecoratorCollection TheGrinReaperTimerDecorator { get; set; }//狞笑死神分身计时器
		public WorldDecoratorCollection mysticAllyDecorator { get; set; }//幻身诀
		public WorldDecoratorCollection CallOfTheAncientsDecorator { get; set; }//先祖召唤
		public WorldDecoratorCollection CallOfTheAncientsTimerDecorator { get; set; }//先祖召唤计时器
		public WorldDecoratorCollection CompanionDecorator { get; set; }//邪鸦战宠
		public WorldDecoratorCollection CompanionSpiderDecorator { get; set; }//蜘蛛战宠
		public WorldDecoratorCollection CompanionRuneEDecorator { get; set; }//蝙蝠战宠
		public WorldDecoratorCollection CompanionBoarDecorator { get; set; }//野猪战宠
		public WorldDecoratorCollection CompanionFerretDecorator { get; set; }//雪貂战宠
		public WorldDecoratorCollection CompanionRuneCDecorator { get; set; }//恶狼战宠
		public WorldDecoratorCollection PhalanxDecorator { get; set; }//贴身卫士
		public WorldDecoratorCollection PhalanxTimerDecorator { get; set; }//贴身卫士计时器
		public WorldDecoratorCollection PhalanxArcherDecorator { get; set; }//弓箭卫士
		public WorldDecoratorCollection PhalanxArcherTimerDecorator { get; set; }//弓箭卫士计时器
		public WorldDecoratorCollection PhalanxBlockerTimerDecorator { get; set; }//人墙卫士计时器
		public WorldDecoratorCollection MirrorImageDecorator { get; set; }//镜像术
		public WorldDecoratorCollection MirrorImageTimerDecorator { get; set; }//镜像术计时器
        public WorldDecoratorCollection HydraTimerDecorator { get; set; }//多头蛇计时器
		public WorldDecoratorCollection DemonChainsDecorator { get; set; }//麦西穆斯恶魔
		public WorldDecoratorCollection DemonChainsTimerDecorator { get; set; }//麦西穆斯恶魔计时器
		public WorldDecoratorCollection HauntOfVaxoDecorator { get; set; }//瓦索的阴魂
        public bool Simulacrum { get; set; }
        public WorldDecoratorCollection SimulacrumDecorator { get; set; }//血魂双分体积
        public WorldDecoratorCollection SimulacrumTimer10Decorator { get; set; }//血魂双分计时器
        public WorldDecoratorCollection SimulacrumTimer15Decorator { get; set; }//血魂双分计时器
        public WorldDecoratorCollection SimulacrumTimer20Decorator { get; set; }//血魂双分计时器
        public WorldDecoratorCollection SimulacrumTimer30Decorator { get; set; }//血魂双分计时器
        public WorldDecoratorCollection CommandSkeletonsDecorator { get; set; }//号令骷髅体积
        public WorldDecoratorCollection GolemDecorator { get; set; }//号令傀儡体积
        public WorldDecoratorCollection SkeletonMageDecorator { get; set; }//骷髅法师体积
        public WorldDecoratorCollection ReviveDecorator { get; set; }//亡魂复生体积



        public GLQ_PetCirclePlugin1()
        {
            Enabled = true;
            Simulacrum = true;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
			
            FetishDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud)
                {
                    Radius = -1,
                    Brush = Hud.Render.CreateBrush(255, 255, 173, 91, 2, DashStyle.Solid)
                });
            FetishTimerDecorator = new WorldDecoratorCollection(
                new GroundLabelDecorator(Hud)
                {
                    CountDownFrom = 20,
                    TextFont = Hud.Render.CreateFont("tahoma", 6, 255, 255, 255, 0, true, false, 255, 0, 0, 0, true),
                },
                new GroundTimerDecorator(Hud)
                {
                    CountDownFrom = 20,
                    BackgroundBrushEmpty = Hud.Render.CreateBrush(128, 0, 0, 0, 0),
                    BackgroundBrushFill = Hud.Render.CreateBrush(255, 255, 173, 91, 0),
                    Radius = 10,
                });
				
            FetishfireShamanDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud)
                {
                    Radius = -1,
                    Brush = Hud.Render.CreateBrush(255, 210, 0, 0, 2, DashStyle.Solid)
                });
            FetishfireShamanTimerDecorator = new WorldDecoratorCollection(
                new GroundLabelDecorator(Hud)
                {
                    CountDownFrom = 20,
                    TextFont = Hud.Render.CreateFont("tahoma", 6, 255, 255, 255, 0, true, false, 255, 0, 0, 0, true),
                },
                new GroundTimerDecorator(Hud)
                {
                    CountDownFrom = 20,
                    BackgroundBrushEmpty = Hud.Render.CreateBrush(128, 0, 0, 0, 0),
                    BackgroundBrushFill = Hud.Render.CreateBrush(255, 255, 173, 91, 0),
                    Radius = 10,
                });
					
            FetishRangedDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud)
                {
                    Radius = -1,
                    Brush = Hud.Render.CreateBrush(255, 0, 128, 64, 2, DashStyle.Solid)
                });
            FetishRangedTimerDecorator = new WorldDecoratorCollection(
                new GroundLabelDecorator(Hud)
                {
                    CountDownFrom = 20,
                    TextFont = Hud.Render.CreateFont("tahoma", 6, 255, 255, 255, 0, true, false, 255, 0, 0, 0, true),
                },
                new GroundTimerDecorator(Hud)
                {
                    CountDownFrom = 20,
                    BackgroundBrushEmpty = Hud.Render.CreateBrush(128, 0, 0, 0, 0),
                    BackgroundBrushFill = Hud.Render.CreateBrush(255, 0, 128, 64, 0),
                    Radius = 10,
                });
				
            FetishMeleeSycophantsDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud)
                {
                    Radius = -1,
                    Brush = Hud.Render.CreateBrush(160, 128, 64, 64, 2, DashStyle.Solid)
                });
            FetishMeleeSycophantsTimerDecorator = new WorldDecoratorCollection(
                new GroundLabelDecorator(Hud)
                {
                    CountDownFrom = 60,
                    TextFont = Hud.Render.CreateFont("tahoma", 6, 255, 255, 255, 0, true, false, 255, 0, 0, 0, true),
                },
                new GroundTimerDecorator(Hud)
                {
                    CountDownFrom = 60,
                    BackgroundBrushEmpty = Hud.Render.CreateBrush(128, 0, 0, 0, 0),
                    BackgroundBrushFill = Hud.Render.CreateBrush(255, 128, 64, 64, 0),
                    Radius = 10,
                });

            ZombieDogDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud)
                {
                    Radius = -1,
                    Brush = Hud.Render.CreateBrush(255, 250, 100, 100, 2, DashStyle.Solid)
                });

				
            GargantuanDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud)
                {
                    Radius = -1,
                    Brush = Hud.Render.CreateBrush(255, 0, 136, 255, 3, DashStyle.Solid)
                });
            GargantuanTimerDecorator = new WorldDecoratorCollection(
                new GroundLabelDecorator(Hud)
                {
                    CountDownFrom = 15,
                    TextFont = Hud.Render.CreateFont("tahoma", 7, 255, 255, 255, 0, true, false, 255, 0, 0, 0, true),
                },
                new GroundTimerDecorator(Hud)
                {
                    CountDownFrom = 15,
                    BackgroundBrushEmpty = Hud.Render.CreateBrush(128, 0, 0, 0, 0),
                    BackgroundBrushFill = Hud.Render.CreateBrush(255, 0, 136, 255, 0),
                    Radius = 15,
                });
				
            FetishHexTimerDecorator = new WorldDecoratorCollection(
                new GroundLabelDecorator(Hud)
                {
                    CountDownFrom = 12,
                    TextFont = Hud.Render.CreateFont("tahoma", 7, 255, 255, 255, 0, true, false, 255, 0, 0, 0, true),
                },
                new GroundTimerDecorator(Hud)
                {
                    CountDownFrom = 12,
                    BackgroundBrushEmpty = Hud.Render.CreateBrush(128, 0, 0, 0, 0),
                    BackgroundBrushFill = Hud.Render.CreateBrush(255, 0, 64, 0, 0),
                    Radius = 15,
                });	
				
            FetishHexGiantToadDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud)
                {
                    Radius = -1,
                    Brush = Hud.Render.CreateBrush(255, 128, 128, 0, 3, DashStyle.Solid)
                });
            FetishHexGiantToadTimerDecorator = new WorldDecoratorCollection(
                new GroundLabelDecorator(Hud)
                {
                    CountDownFrom = 10,
                    TextFont = Hud.Render.CreateFont("tahoma", 7, 255, 255, 255, 0, true, false, 255, 0, 0, 0, true),
                },
                new GroundTimerDecorator(Hud)
                {
                    CountDownFrom = 10,
                    BackgroundBrushEmpty = Hud.Render.CreateBrush(128, 0, 0, 0, 0),
                    BackgroundBrushFill = Hud.Render.CreateBrush(255, 128, 128, 0, 0),
                    Radius = 15,
                });
            TheGrinReaperDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud)
                {
                    Radius = -1,
                    Brush = Hud.Render.CreateBrush(255, 0, 255, 255, 2, DashStyle.Solid)
                });
            TheGrinReaperTimerDecorator = new WorldDecoratorCollection(
                new GroundLabelDecorator(Hud)
                {
                    CountDownFrom = 10,
                    TextFont = Hud.Render.CreateFont("tahoma", 7, 255, 255, 255, 0, true, false, 255, 0, 0, 0, true),
                },
                new GroundTimerDecorator(Hud)
                {
                    CountDownFrom = 10,
                    BackgroundBrushEmpty = Hud.Render.CreateBrush(128, 0, 0, 0, 0),
                    BackgroundBrushFill = Hud.Render.CreateBrush(255, 0, 255, 255, 0),
                    Radius = 15,
                });
				
				
				
            mysticAllyDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud)
                {
                    Radius = -1,
                    Brush = Hud.Render.CreateBrush(255, 50, 150, 255, 2, DashStyle.Solid)
                });

            CallOfTheAncientsDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud)
                {
                    Radius = -1,
                    Brush = Hud.Render.CreateBrush(255, 255, 128, 0, 2, DashStyle.Solid)
                });
            CallOfTheAncientsTimerDecorator = new WorldDecoratorCollection(
                new GroundLabelDecorator(Hud)
                {
                    CountDownFrom = 20,
                    TextFont = Hud.Render.CreateFont("tahoma", 7, 255, 255, 255, 0, true, false, 255, 0, 0, 0, true),
                },
                new GroundTimerDecorator(Hud)
                {
                    CountDownFrom = 20,
                    BackgroundBrushEmpty = Hud.Render.CreateBrush(128, 0, 0, 0, 0),
                    BackgroundBrushFill = Hud.Render.CreateBrush(255, 255, 128, 0, 0),
                    Radius = 15,
                });
				
				
            CompanionDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud)
                {
                    Radius = -1,
                    Brush = Hud.Render.CreateBrush(255, 75, 0, 160, 2, DashStyle.Solid)
                });
            CompanionSpiderDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud)
                {
                    Radius = -1,
                    Brush = Hud.Render.CreateBrush(255, 0, 64, 64, 2, DashStyle.Solid)
                });	
            CompanionRuneEDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud)
                {
                    Radius = -1,
                    Brush = Hud.Render.CreateBrush(255, 108, 0, 0, 2, DashStyle.Solid)
                });	
			CompanionBoarDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud)
                {
                    Radius = -1,
                    Brush = Hud.Render.CreateBrush(255, 0, 0, 140, 2, DashStyle.Solid)
                });	
			CompanionFerretDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud)
                {
                    Radius = -1,
                    Brush = Hud.Render.CreateBrush(255, 255, 128, 64, 2, DashStyle.Solid)
                });	
			CompanionRuneCDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud)
                {
                    Radius = -1,
                    Brush = Hud.Render.CreateBrush(255, 0, 64, 128, 2, DashStyle.Solid)
                });	
				
			PhalanxDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud)
                {
                    Radius = -1,
                    Brush = Hud.Render.CreateBrush(255, 0, 128, 128, 2, DashStyle.Solid)
                });	
            PhalanxTimerDecorator = new WorldDecoratorCollection(
                new GroundLabelDecorator(Hud)
                {
                    CountDownFrom = 10,
                    TextFont = Hud.Render.CreateFont("tahoma", 7, 255, 255, 255, 0, true, false, 255, 0, 0, 0, true),
                },
                new GroundTimerDecorator(Hud)
                {
                    CountDownFrom = 10,
                    BackgroundBrushEmpty = Hud.Render.CreateBrush(128, 0, 0, 0, 0),
                    BackgroundBrushFill = Hud.Render.CreateBrush(255, 0, 128, 128, 0),
                    Radius = 15,
                });
			PhalanxArcherDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud)
                {
                    Radius = -1,
                    Brush = Hud.Render.CreateBrush(255, 0, 64, 128, 2, DashStyle.Solid)
                });	
            PhalanxArcherTimerDecorator = new WorldDecoratorCollection(
                new GroundLabelDecorator(Hud)
                {
                    CountDownFrom = 5,
                    TextFont = Hud.Render.CreateFont("tahoma", 7, 255, 255, 255, 0, true, false, 255, 0, 0, 0, true),
                },
                new GroundTimerDecorator(Hud)
                {
                    CountDownFrom = 5,
                    BackgroundBrushEmpty = Hud.Render.CreateBrush(128, 0, 0, 0, 0),
                    BackgroundBrushFill = Hud.Render.CreateBrush(255, 0, 64, 128, 0),
                    Radius = 15,
                });
            PhalanxBlockerTimerDecorator = new WorldDecoratorCollection(
                new GroundLabelDecorator(Hud)
                {
                    CountDownFrom = 5,
                    TextFont = Hud.Render.CreateFont("tahoma", 7, 255, 255, 255, 0, true, false, 255, 0, 0, 0, true),
                },
                new GroundTimerDecorator(Hud)
                {
                    CountDownFrom = 5,
                    BackgroundBrushEmpty = Hud.Render.CreateBrush(128, 0, 0, 0, 0),
                    BackgroundBrushFill = Hud.Render.CreateBrush(255, 128, 64, 0, 0),
                    Radius = 15,
                });
			MirrorImageDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud)
                {
                    Radius = -1,
                    Brush = Hud.Render.CreateBrush(255, 255, 128, 255, 2, DashStyle.Solid)
                });	
            MirrorImageTimerDecorator = new WorldDecoratorCollection(
                new GroundLabelDecorator(Hud)
                {
                    CountDownFrom = 7,
                    TextFont = Hud.Render.CreateFont("tahoma", 7, 255, 255, 255, 0, true, false, 255, 0, 0, 0, true),
                },
                new GroundTimerDecorator(Hud)
                {
                    CountDownFrom = 7,
                    BackgroundBrushEmpty = Hud.Render.CreateBrush(128, 0, 0, 0, 0),
                    BackgroundBrushFill = Hud.Render.CreateBrush(255, 255, 128, 255, 0),
                    Radius = 15,
                });
            HydraTimerDecorator = new WorldDecoratorCollection(
                new GroundLabelDecorator(Hud)
                {
                    CountDownFrom = 15,
                    TextFont = Hud.Render.CreateFont("tahoma", 9, 255, 255, 200, 200, true, false, 128, 0, 0, 0, true),
                },
                new GroundTimerDecorator(Hud)
                {
                    CountDownFrom = 15,
                    BackgroundBrushEmpty = Hud.Render.CreateBrush(128, 0, 0, 0, 0),
                    BackgroundBrushFill = Hud.Render.CreateBrush(230, 255, 50, 50, 0),
                    Radius = 15,
                }
                );
			DemonChainsDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud)
                {
                    Radius = -1,
                    Brush = Hud.Render.CreateBrush(255, 255, 0, 0, 2, DashStyle.Solid)
                });	
            DemonChainsTimerDecorator = new WorldDecoratorCollection(
                new GroundLabelDecorator(Hud)
                {
                    CountDownFrom = 15,
                    TextFont = Hud.Render.CreateFont("tahoma", 7, 255, 255, 255, 0, true, false, 255, 0, 0, 0, true),
                },
                new GroundTimerDecorator(Hud)
                {
                    CountDownFrom = 15,
                    BackgroundBrushEmpty = Hud.Render.CreateBrush(128, 0, 0, 0, 0),
                    BackgroundBrushFill = Hud.Render.CreateBrush(255, 255, 0, 0, 0),
                    Radius = 15,
                });
			HauntOfVaxoDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud)
                {
                    Radius = -1,
                    Brush = Hud.Render.CreateBrush(255, 128, 0, 64, 2, DashStyle.Solid)
                });

            SimulacrumDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud)
                {
                    Radius = -1,
                    Brush = Hud.Render.CreateBrush(255, 255, 0, 0, 2, DashStyle.Solid)
                });
            SimulacrumTimer10Decorator = new WorldDecoratorCollection(
                new GroundLabelDecorator(Hud)
                {
                    CountDownFrom = 10,
                    TextFont = Hud.Render.CreateFont("tahoma", 7, 255, 255, 255, 0, true, false, 255, 0, 0, 0, true),
                },
                new GroundTimerDecorator(Hud)
                {
                    CountDownFrom = 10,
                    BackgroundBrushEmpty = Hud.Render.CreateBrush(128, 0, 0, 0, 0),
                    BackgroundBrushFill = Hud.Render.CreateBrush(255, 255, 0, 0, 0),
                    Radius = 15,
                });

            SimulacrumTimer15Decorator = new WorldDecoratorCollection(
                new GroundLabelDecorator(Hud)
                {
                    CountDownFrom = 15,
                    TextFont = Hud.Render.CreateFont("tahoma", 7, 255, 255, 255, 0, true, false, 255, 0, 0, 0, true),
                },
                new GroundTimerDecorator(Hud)
                {
                    CountDownFrom = 15,
                    BackgroundBrushEmpty = Hud.Render.CreateBrush(128, 0, 0, 0, 0),
                    BackgroundBrushFill = Hud.Render.CreateBrush(255, 255, 0, 0, 0),
                    Radius = 15,
                });
            SimulacrumTimer20Decorator = new WorldDecoratorCollection(
               new GroundLabelDecorator(Hud)
               {
                   CountDownFrom = 20,
                   TextFont = Hud.Render.CreateFont("tahoma", 7, 255, 255, 255, 0, true, false, 255, 0, 0, 0, true),
               },
               new GroundTimerDecorator(Hud)
               {
                   CountDownFrom = 20,
                   BackgroundBrushEmpty = Hud.Render.CreateBrush(128, 0, 0, 0, 0),
                   BackgroundBrushFill = Hud.Render.CreateBrush(255, 255, 0, 0, 0),
                   Radius = 15,
               });
            SimulacrumTimer30Decorator = new WorldDecoratorCollection(
               new GroundLabelDecorator(Hud)
               {
                   CountDownFrom = 30,
                   TextFont = Hud.Render.CreateFont("tahoma", 7, 255, 255, 255, 0, true, false, 255, 0, 0, 0, true),
               },
               new GroundTimerDecorator(Hud)
               {
                   CountDownFrom = 30,
                   BackgroundBrushEmpty = Hud.Render.CreateBrush(128, 0, 0, 0, 0),
                   BackgroundBrushFill = Hud.Render.CreateBrush(255, 255, 0, 0, 0),
                   Radius = 15,
               });
            CommandSkeletonsDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud)
                {
                    Radius = -1,
                    Brush = Hud.Render.CreateBrush(255, 0, 255, 255, 2, DashStyle.Solid)
                });
            GolemDecorator = new WorldDecoratorCollection(
               new GroundCircleDecorator(Hud)
               {
                   Radius = -1,
                   Brush = Hud.Render.CreateBrush(255, 128, 0, 0, 2, DashStyle.Solid)
               });
            SkeletonMageDecorator = new WorldDecoratorCollection(
               new GroundCircleDecorator(Hud)
               {
                   Radius = -1,
                   Brush = Hud.Render.CreateBrush(255, 128, 128, 255, 2, DashStyle.Solid)
               });
            ReviveDecorator = new WorldDecoratorCollection(
               new GroundCircleDecorator(Hud)
               {
                   Radius = -1,
                   Brush = Hud.Render.CreateBrush(255, 100, 0, 0, 2, DashStyle.Solid)
               });

        }
			

        public void PaintWorld(WorldLayer layer)
        {
            if ((Hud.Game.MapMode == MapMode.WaypointMap) || (Hud.Game.MapMode == MapMode.ActMap) || (Hud.Game.MapMode == MapMode.Map)) return;
            var Skills = Hud.Game.Actors;
            foreach (var skill in Skills)
            {
                if (skill.SummonerAcdDynamicId == Hud.Game.Me.SummonerId)
                {
                    switch (skill.SnoActor.Sno)
                    {
                        case (ActorSnoEnum)467053:
                            {
                                var UsedSkills = Hud.Game.Me.Powers.UsedSkills.Where(x => x.SnoPower.Sno == 465350).FirstOrDefault();
                                if (UsedSkills != null && Simulacrum == true)
                                {
                                    if (Hud.Game.Me.Powers.BuffIsActive(476590, 0))//鬼灵面容
                                    {
                                        if (UsedSkills.Rune == 3)
                                        {
                                            //SimulacrumDecorator.Paint(layer, skill, skill.FloorCoordinate, string.Empty); //没有体积
                                            SimulacrumTimer20Decorator.Paint(layer, skill, skill.FloorCoordinate, string.Empty);
                                        }
                                        else
                                        {
                                            //SimulacrumDecorator.Paint(layer, skill, skill.FloorCoordinate, string.Empty); //没有体积
                                            SimulacrumTimer30Decorator.Paint(layer, skill, skill.FloorCoordinate, string.Empty);
                                        }
                                    }
                                    else
                                    {
                                        if (UsedSkills.Rune == 3)
                                        {
                                            //SimulacrumDecorator.Paint(layer, skill, skill.FloorCoordinate, string.Empty); //没有体积
                                            SimulacrumTimer10Decorator.Paint(layer, skill, skill.FloorCoordinate, string.Empty);
                                        }
                                        else
                                        {
                                            //SimulacrumDecorator.Paint(layer, skill, skill.FloorCoordinate, string.Empty); //没有体积
                                            SimulacrumTimer15Decorator.Paint(layer, skill, skill.FloorCoordinate, string.Empty);
                                        }
                                    }
                                        
                                }
                            }
                            break;
                        case (ActorSnoEnum)473147:
                        case (ActorSnoEnum)473428:
                        case (ActorSnoEnum)473426:
                        case (ActorSnoEnum)473420:
                        case (ActorSnoEnum)473417:
                        case (ActorSnoEnum)473418:
                            {
                                CommandSkeletonsDecorator.Paint(layer, skill, skill.FloorCoordinate, string.Empty);
                            }
                            break;
                        case (ActorSnoEnum)471647:
                        case (ActorSnoEnum)471646:
                        case (ActorSnoEnum)465239:
                        case (ActorSnoEnum)471619:
                        case (ActorSnoEnum)460042:
                            {
                                GolemDecorator.Paint(layer, skill, skill.FloorCoordinate, string.Empty);
                            }
                            break;
                        case (ActorSnoEnum)472275://  p6_necro_skeletonMage_A
                        case (ActorSnoEnum)472588://  p6_necro_skeletonMage_B
                        case (ActorSnoEnum)472606://  p6_necro_skeletonMage_C
                        case (ActorSnoEnum)472715://  p6_necro_skeletonMage_D
                        case (ActorSnoEnum)472769://  p6_necro_skeletonMage_E
                            SkeletonMageDecorator.Paint(layer, skill, skill.FloorCoordinate, string.Empty);//由于每次移动时都会重新刷新时间，所以无法计时
                            break;
                        case (ActorSnoEnum)469978://	p6_Necro_Revive_Angel_Corrupt	复生的被腐化的天使
                        case (ActorSnoEnum)476776://	p6_Necro_Revive_armorScavenger	复生的原始食腐兽
                        case (ActorSnoEnum)470491://	p6_Necro_Revive_azmodanBodyguard	复生的巨型移形兽
                        case (ActorSnoEnum)464864://	p6_necro_revive_B_spawn_trailGeo	
                        case (ActorSnoEnum)474380://	p6_Necro_Revive_Bat	复生的邪蝠
                        case (ActorSnoEnum)470880://	p6_Necro_Revive_Beast	复生的凶暴蛮牛怪
                        case (ActorSnoEnum)476407://	p6_Necro_Revive_Beetle	复生的墓穴蟑螂
                        case (ActorSnoEnum)467629://	p6_Necro_Revive_BigRed	复生的施虐狂魔
                        case (ActorSnoEnum)470945://	p6_Necro_Revive_BileCrawler	复生的暴怒爬行者
                        case (ActorSnoEnum)470947://	p6_Necro_Revive_Bloodhawk	复生的血肉猎食者
                        case (ActorSnoEnum)474275://	p6_Necro_Revive_BogBlight	复生的蛆虫育母
                        case (ActorSnoEnum)474283://	p6_Necro_Revive_BogBlight_Maggot	复生的蛆虫
                        case (ActorSnoEnum)477161://	p6_Necro_Revive_BogFamily_brute	复生的尖牙沼泽兽
                        case (ActorSnoEnum)477213://	p6_Necro_Revive_BogFamily_melee	复生的沼泽怪
                        case (ActorSnoEnum)477229://	p6_Necro_Revive_BogFamily_ranged	复生的沼泽诱捕兽
                        case (ActorSnoEnum)477321://	p6_necro_revive_bogFamilyRanged_blowGun	
                        case (ActorSnoEnum)477323://	p6_necro_revive_bogFamilyRanged_quill	
                        case (ActorSnoEnum)470949://	p6_Necro_Revive_Brickhouse	复生的恶魔震地者
                        case (ActorSnoEnum)464883://	p6_necro_revive_C_spawn_trailGeo	
                        case (ActorSnoEnum)470974://	p6_Necro_Revive_CoreEliteDemon	复生的铁甲摧毁者
                        case (ActorSnoEnum)470986://	p6_Necro_Revive_Corpulent	复生的怪诞魔
                        case (ActorSnoEnum)474387://	p6_Necro_Revive_Crab	复生的刀蟹
                        case (ActorSnoEnum)474375://	p6_Necro_Revive_Crab_Mother	复生的血肉收割者
                        case (ActorSnoEnum)471072://	p6_Necro_Revive_creepMob	复生的疫病传染体
                        case (ActorSnoEnum)476437://	p6_Necro_Revive_CrowHound	复生的贪食怪
                        case (ActorSnoEnum)471074://	p6_Necro_Revive_CryptChild	复生的顽魔
                        case (ActorSnoEnum)464935://	p6_necro_revive_D_spawn_trailGeo	
                        case (ActorSnoEnum)476333://	p6_Necro_Revive_Dark_Angel	复生的控尸者
                        case (ActorSnoEnum)477174://	p6_necro_revive_darkAngel_wings	
                        case (ActorSnoEnum)476494://	p6_Necro_Revive_DeathMaiden	复生的死神侍女
                        case (ActorSnoEnum)462220://	p6_necro_revive_default	复生的骷髅
                        case (ActorSnoEnum)471089://	p6_Necro_Revive_demonFlyer	复生的地狱天鬼
                        case (ActorSnoEnum)471129://	p6_Necro_Revive_demonTrooper	复生的恶魔士兵
                        case (ActorSnoEnum)471146://	p6_Necro_Revive_DuneDervish	复生的沙漠魔煞
                        case (ActorSnoEnum)464780://	p6_necro_revive_E_spawn_trailGeo	
                        case (ActorSnoEnum)471151://	p6_Necro_Revive_electricEel	复生的电鳗
                        case (ActorSnoEnum)464974://	p6_necro_revive_F_spawn_trailGeo	
                        case (ActorSnoEnum)463066://	p6_Necro_Revive_FallenChampion	复生的堕落监工
                        case (ActorSnoEnum)462984://	p6_Necro_Revive_FallenGrunt	复生的堕落者
                        case (ActorSnoEnum)463150://	p6_Necro_Revive_FallenHound	复生的堕落恶犬
                        case (ActorSnoEnum)463178://	p6_Necro_Revive_FallenLunatic	复生的堕落癫狂者
                        case (ActorSnoEnum)463169://	p6_Necro_Revive_FallenShaman	复生的堕落萨满
                        case (ActorSnoEnum)471164://	p6_Necro_Revive_fastMummy	复生的背叛者
                        case (ActorSnoEnum)471237://	p6_Necro_Revive_FleshPitFlyer	复生的食腐蝙蝠
                        case (ActorSnoEnum)477393://	p6_Necro_Revive_FloaterAngel	复生的驱魔者
                        case (ActorSnoEnum)471637://	p6_Necro_Revive_Ghost	复生的激怒的厉魂
                        case (ActorSnoEnum)471757://	p6_Necro_Revive_Ghoul	复生的食尸鬼
                        case (ActorSnoEnum)471867://	p6_Necro_Revive_Goatman_Melee	复生的月亮部族战士
                        case (ActorSnoEnum)471860://	p6_Necro_Revive_Goatman_Ranged	复生的月亮部族穿刺者
                        case (ActorSnoEnum)471814://	p6_Necro_Revive_Goatman_Shaman	复生的月亮部族萨满
                        case (ActorSnoEnum)471957://	p6_Necro_Revive_GoatMutant_Melee	复生的鲜血氏族战士
                        case (ActorSnoEnum)471959://	p6_Necro_Revive_GoatMutant_Ranged	复生的鲜血氏族枪兵
                        case (ActorSnoEnum)471977://	p6_Necro_Revive_GoatMutant_Shaman	复生的鲜血氏族巫师
                        case (ActorSnoEnum)471947://	p6_Necro_Revive_Golem	小型复生的傀儡
                        case (ActorSnoEnum)477443://	p6_Necro_Revive_graveDigger	复生的掘墓者
                        case (ActorSnoEnum)467623://	p6_Necro_Revive_HoodedNightmare	复生的奴役梦魇
                        case (ActorSnoEnum)474754://	p6_Necro_Revive_Ice_Porcupine	复生的冰霜豪猪
                        case (ActorSnoEnum)476210://	p6_necro_revive_icePorcupine_actor	
                        case (ActorSnoEnum)476208://	p6_necro_revive_icePorcupine_projectile	
                        case (ActorSnoEnum)472053://	p6_Necro_Revive_LacuniFemale	复生的豹人女猎手
                        case (ActorSnoEnum)472110://	p6_Necro_Revive_LacuniMale	复生的豹人战士
                        case (ActorSnoEnum)472120://	p6_Necro_Revive_Lamprey	复生的尸虫
                        case (ActorSnoEnum)474090://	p6_Necro_Revive_leaperAngel	复生的飞翼刺杀者
                        case (ActorSnoEnum)476349://	p6_Necro_Revive_Mage	复生的顾问
                        case (ActorSnoEnum)467583://	p6_Necro_Revive_MalletDemon	复生的槌地魔
                        case (ActorSnoEnum)467515://	p6_Necro_Revive_MastaBlasta_Rider	复生的统御魔
                        case (ActorSnoEnum)467522://	p6_Necro_Revive_MastaBlasta_Steed	复生的阿玛顿
                        case (ActorSnoEnum)475235://	p6_Necro_Revive_Mermaid_Melee	复生的深渊保卫者
                        case (ActorSnoEnum)475297://	p6_Necro_Revive_Mermaid_Ranged	复生的深渊召唤者
                        case (ActorSnoEnum)475413://	p6_Necro_Revive_MoleMutant_Melee	复生的血魔吞食者
                        case (ActorSnoEnum)475460://	p6_Necro_Revive_MoleMutant_Ranged	复生的血魔抛毒者
                        case (ActorSnoEnum)475490://	p6_Necro_Revive_MoleMutant_Shaman	复生的血魔萨满
                        case (ActorSnoEnum)474129://	p6_Necro_Revive_Monstrosity_Scorpion	复生的折磨之刺
                        case (ActorSnoEnum)476120://	p6_Necro_Revive_Monstrosity_ScorpionBug	复生的圣甲虫
                        case (ActorSnoEnum)462376://	p6_Necro_Revive_MorluMelee	复生的摩鲁入侵者
                        case (ActorSnoEnum)462965://	p6_Necro_Revive_MorluSpellcaster	复生的摩鲁焚化者
                        case (ActorSnoEnum)477449://	p6_Necro_Revive_NightScreamer	复生的尖啸恐蝠
                        case (ActorSnoEnum)477282://	p6_Necro_Revive_portalGuardianMinion_Melee	复生的扫荡冲锋兽
                        case (ActorSnoEnum)477330://	p6_Necro_Revive_portalGuardianMinion_Ranged	复生的扫荡掷击兽
                        case (ActorSnoEnum)477633://	p6_necro_revive_projectile_arrow02	
                        case (ActorSnoEnum)470817://	p6_necro_revive_projectile_impact_large	
                        case (ActorSnoEnum)470762://	p6_necro_revive_projectile_large	
                        case (ActorSnoEnum)470787://	p6_necro_revive_projectile_med	
                        case (ActorSnoEnum)476520://	p6_Necro_Revive_QuillDemon	复生的刺脊魔
                        case (ActorSnoEnum)477481://	p6_necro_revive_quillDemon_projectile	
                        case (ActorSnoEnum)475877://	p6_Necro_Revive_Rat	复生的疫病鼠
                        case (ActorSnoEnum)475895://	p6_Necro_Revive_RatKing	复生的鼠王
                        case (ActorSnoEnum)476525://	p6_Necro_Revive_RavenFlyer	复生的科维亚猎手
                        case (ActorSnoEnum)476997://	p6_Necro_Revive_Rockworm	复生的食腐掘洞者
                        case (ActorSnoEnum)477417://	p6_necro_revive_rockworm_groundBurst_mesh	
                        case (ActorSnoEnum)477036://	p6_Necro_Revive_Sandling	复生的尘土顽魔
                        case (ActorSnoEnum)474326://	p6_Necro_Revive_sandMonster	复生的沙居兽
                        case (ActorSnoEnum)474252://	p6_Necro_Revive_SandShark	复生的沙丘长尾蜥
                        case (ActorSnoEnum)475904://	p6_Necro_Revive_SandWasp	复生的沙漠蜂
                        case (ActorSnoEnum)476196://	p6_necro_revive_sandWasp_actor	
                        case (ActorSnoEnum)476186://	p6_necro_revive_sandWasp_projectile	
                        case (ActorSnoEnum)477044://	p6_Necro_Revive_Scavenger	复生的食腐魔
                        case (ActorSnoEnum)476085://	p6_Necro_Revive_ScorpionBug	复生的毒眼甲虫
                        case (ActorSnoEnum)476462://	p6_Necro_Revive_shadowVermin	复生的死影暴徒
                        case (ActorSnoEnum)476703://	p6_Necro_Revive_Shepherd	复生的重生体
                        case (ActorSnoEnum)466494://	p6_Necro_Revive_Shield_Skeleton	复生的骷髅持盾者
                        case (ActorSnoEnum)466881://	p6_Necro_Revive_Shield_Skeleton_Westmarch	复生的亡魂盾卫
                        case (ActorSnoEnum)466491://	p6_Necro_Revive_Skeleton	复生的骷髅
                        case (ActorSnoEnum)466515://	p6_Necro_Revive_Skeleton_twohander	复生的骷髅处决者
                        case (ActorSnoEnum)466952://	p6_Necro_Revive_Skeleton_Westmarch	复生的亡魂士兵
                        case (ActorSnoEnum)466504://	p6_Necro_Revive_SkeletonArcher	复生的骷髅弓手
                        case (ActorSnoEnum)466953://	p6_Necro_Revive_SkeletonArcher_Westmarch	复生的亡魂弓手
                        case (ActorSnoEnum)466875://	p6_Necro_Revive_skeletonMage	复生的闪电卫士
                        case (ActorSnoEnum)466519://	p6_Necro_Revive_SkeletonSummoner	复生的墓穴卫士
                        case (ActorSnoEnum)477396://	p6_necro_revive_skeletonSummoner_projectile	
                        case (ActorSnoEnum)477373://	p6_Necro_Revive_Slime	复生的强酸黏液
                        case (ActorSnoEnum)476259://	p6_Necro_Revive_snakeMan_Caster	复生的蛇身法师
                        case (ActorSnoEnum)476275://	p6_Necro_Revive_snakeMan_Melee	复生的盘蛇欺诈者
                        case (ActorSnoEnum)477429://	p6_Necro_Revive_sniperAngel	复生的督天灵
                        case (ActorSnoEnum)477048://	p6_Necro_Revive_SoulRipper	复生的灵魂撕裂者
                        case (ActorSnoEnum)476099://	p6_necro_revive_spear	
                        case (ActorSnoEnum)476298://	p6_Necro_Revive_Spider	复生的恐蛛
                        case (ActorSnoEnum)476303://	p6_Necro_Revive_Spiderling	复生的小蜘蛛
                        case (ActorSnoEnum)476312://	p6_Necro_Revive_Squigglet	复生的勾刺潜伏者
                        case (ActorSnoEnum)477133://	p6_necro_revive_squigglet_projectile	
                        case (ActorSnoEnum)476351://	p6_Necro_Revive_Succubus	复生的魅魔
                        case (ActorSnoEnum)477063://	p6_Necro_Revive_Swarm	复生的针刺虫群
                        case (ActorSnoEnum)476704://	p6_Necro_Revive_TempleCultist_Basic	复生的血祖信徒
                        case (ActorSnoEnum)476706://	p6_Necro_Revive_TempleCultist_Caster	复生的血祖祭司
                        case (ActorSnoEnum)476711://	p6_Necro_Revive_TempleCultist_Special	复生的血祖狂徒
                        case (ActorSnoEnum)476697://	p6_Necro_Revive_TempleMonstrosity	复生的邪恶的宿主
                        case (ActorSnoEnum)476510://	p6_Necro_Revive_Tentacle_Goatman_Melee	复生的炼狱魔牛
                        case (ActorSnoEnum)476513://	p6_Necro_Revive_Tentacle_Goatman_Ranged	复生的炼狱魔牛矛兵
                        case (ActorSnoEnum)476515://	p6_Necro_Revive_Tentacle_Goatman_Shaman	复生的炼狱魔牛萨满
                        case (ActorSnoEnum)477090://	p6_Necro_Revive_TentacleBear	复生的可爱的小熊
                        case (ActorSnoEnum)477102://	p6_Necro_Revive_tentacleFlower	复生的闪耀之根
                        case (ActorSnoEnum)477105://	p6_Necro_Revive_TentacleHorse	复生的彩虹独角兽
                        case (ActorSnoEnum)477116://	p6_Necro_Revive_TentacleHorse_Fat	复生的胖嘟嘟的独角兽
                        case (ActorSnoEnum)469798://	p6_Necro_Revive_TerrorDemon	复生的恐惧爪牙
                        case (ActorSnoEnum)470473://	p6_Necro_Revive_ThousandPounder	复生的戈尔格巨魔
                        case (ActorSnoEnum)467245://	p6_Necro_Revive_Triune_Berserker	复生的黑暗狂暴者
                        case (ActorSnoEnum)467275://	p6_Necro_Revive_Triune_Summonable	复生的黑暗地狱犬
                        case (ActorSnoEnum)467248://	p6_Necro_Revive_TriuneCultist	复生的黑暗邪教徒
                        case (ActorSnoEnum)467269://	p6_Necro_Revive_TriuneSummoner	复生的黑暗召唤师
                        case (ActorSnoEnum)467279://	p6_Necro_Revive_TriuneVessel	复生的黑暗人蛹
                        case (ActorSnoEnum)474820://	p6_Necro_Revive_Unburied	复生的无棺尸魔
                        case (ActorSnoEnum)476683://	p6_Necro_Revive_Werewolf	复生的嚎夜者
                        case (ActorSnoEnum)470957://	p6_Necro_Revive_Westmarch_Brute	复生的惩罚者
                        case (ActorSnoEnum)470968://	p6_Necro_Revive_Westmarch_BruteB	复生的处决者
                        case (ActorSnoEnum)477574://	p6_Necro_Revive_westmarchHound	复生的白骨犬
                        case (ActorSnoEnum)477581://	p6_Necro_Revive_westmarchHound_Leader	复生的头狼
                        case (ActorSnoEnum)476828://	p6_Necro_Revive_westmarchRanged	复活的邪天灵
                        case (ActorSnoEnum)467619://	p6_Necro_Revive_WickerMan	复生的树精
                        case (ActorSnoEnum)477065://	p6_Necro_Revive_WitherMoth	复生的枯萎蛾
                        case (ActorSnoEnum)475231://	p6_Necro_Revive_Wolf	复生的远古猎食者
                        case (ActorSnoEnum)467611://	p6_Necro_Revive_WoodWraith	复生的树妖
                        case (ActorSnoEnum)476920://	p6_Necro_Revive_Wraith	复生的叛天灵
                        case (ActorSnoEnum)474909://	p6_Necro_Revive_Yeti	复生的冰川怪兽
                        case (ActorSnoEnum)466214://	p6_Necro_Revive_Zombie	复生的行尸
                        case (ActorSnoEnum)466220://	p6_Necro_Revive_ZombieCrawler	复生的爬行的躯体
                        case (ActorSnoEnum)466253://	p6_Necro_Revive_ZombieFemale	复生的死亡喷吐者
                        case (ActorSnoEnum)466250://	p6_Necro_Revive_ZombieSkinny	复生的活死人
                            ReviveDecorator.Paint(layer, skill, skill.FloorCoordinate, string.Empty);//由于移动时计时器被重置所以无法计时
                            break;
                    }
                }
                    switch (skill.SnoActor.Sno)
				{
                
                    


					case (ActorSnoEnum)89934:
					case (ActorSnoEnum)87189:
					case (ActorSnoEnum)90320:
					case (ActorSnoEnum)409656:
					case (ActorSnoEnum)410238:
					FetishDecorator.Paint(layer, skill, skill.FloorCoordinate, string.Empty);
					FetishTimerDecorator.Paint(layer, skill, skill.FloorCoordinate, string.Empty);
					break;
					case (ActorSnoEnum)90072:
					FetishfireShamanDecorator.Paint(layer, skill, skill.FloorCoordinate, string.Empty);
					FetishfireShamanTimerDecorator.Paint(layer, skill, skill.FloorCoordinate, string.Empty);
					break;
					case (ActorSnoEnum)89933:
					FetishRangedDecorator.Paint(layer, skill, skill.FloorCoordinate, string.Empty);
					FetishRangedTimerDecorator.Paint(layer, skill, skill.FloorCoordinate, string.Empty);
					break;
					case (ActorSnoEnum)409590:
					FetishMeleeSycophantsDecorator.Paint(layer, skill, skill.FloorCoordinate, string.Empty);
					FetishMeleeSycophantsTimerDecorator.Paint(layer, skill, skill.FloorCoordinate, string.Empty);
					break;
					case (ActorSnoEnum)105763:
					case (ActorSnoEnum)103217:
					case (ActorSnoEnum)51353:
					case (ActorSnoEnum)110959:
					case (ActorSnoEnum)103215:
					case (ActorSnoEnum)103235:
					ZombieDogDecorator.Paint(layer, skill, skill.FloorCoordinate, string.Empty);
					break;
					case (ActorSnoEnum)122305:
					case (ActorSnoEnum)179778:
					case (ActorSnoEnum)179780:
					case (ActorSnoEnum)179779:
					case (ActorSnoEnum)179776:
					case (ActorSnoEnum)432690:
					case (ActorSnoEnum)432691:
					case (ActorSnoEnum)432692:
					case (ActorSnoEnum)432693:
					case (ActorSnoEnum)432694:
					GargantuanDecorator.Paint(layer, skill, skill.FloorCoordinate, string.Empty);
					break;
					case (ActorSnoEnum)179772:
					case (ActorSnoEnum)432695:
					GargantuanDecorator.Paint(layer, skill, skill.FloorCoordinate, string.Empty);
					GargantuanTimerDecorator.Paint(layer, skill, skill.FloorCoordinate, string.Empty);
					break;
					case (ActorSnoEnum)107826:
					case (ActorSnoEnum)107829:
					case (ActorSnoEnum)107752:
					case (ActorSnoEnum)107828:
					FetishHexTimerDecorator.Paint(layer, skill, skill.FloorCoordinate, string.Empty);	
					break;
					case (ActorSnoEnum)109906:
					FetishHexGiantToadDecorator.Paint(layer, skill, skill.FloorCoordinate, string.Empty);
					FetishHexGiantToadTimerDecorator.Paint(layer, skill, skill.FloorCoordinate, string.Empty);
					break;
					case (ActorSnoEnum)251680:
					case (ActorSnoEnum)251681:
					TheGrinReaperDecorator.Paint(layer, skill, skill.FloorCoordinate, string.Empty);
					TheGrinReaperTimerDecorator.Paint(layer, skill, skill.FloorCoordinate, string.Empty);
					break;
					case (ActorSnoEnum)169904:    //Monk_male_mysticAlly     
					case (ActorSnoEnum)169905:    //Monk_male_mysticAlly_alabaster     
					case (ActorSnoEnum)169906:    //Monk_male_mysticAlly_crimson     
					case (ActorSnoEnum)169908:    //Monk_male_mysticAlly_golden     
					case (ActorSnoEnum)169907:    //Monk_male_mysticAlly_indigo     
					case (ActorSnoEnum)169909:    //Monk_male_mysticAlly_obsidian 
					case (ActorSnoEnum)123885:    //Monk_female_mysticAlly     
					case (ActorSnoEnum)169891:    //Monk_female_mysticAlly_alabaster     
					case (ActorSnoEnum)168878:    //Monk_female_mysticAlly_crimson     
					case (ActorSnoEnum)169123:    //Monk_female_mysticAlly_golden     
					case (ActorSnoEnum)169890:    //Monk_female_mysticAlly_indigo     
					case (ActorSnoEnum)169077:    //Monk_female_mysticAlly_obsidian 
					mysticAllyDecorator.Paint(layer, skill, skill.FloorCoordinate, string.Empty);
					break;
					case (ActorSnoEnum)90443:
					case (ActorSnoEnum)90535:
					case (ActorSnoEnum)90536:
					CallOfTheAncientsDecorator.Paint(layer, skill, skill.FloorCoordinate, string.Empty);
					CallOfTheAncientsTimerDecorator.Paint(layer, skill, skill.FloorCoordinate, string.Empty);
					break;
					case (ActorSnoEnum)133741:
					CompanionDecorator.Paint(layer, skill, skill.FloorCoordinate, string.Empty);
					break;
					case (ActorSnoEnum)173827:
					CompanionSpiderDecorator.Paint(layer, skill, skill.FloorCoordinate, string.Empty);
					break;
					case (ActorSnoEnum)159144:
					CompanionRuneEDecorator.Paint(layer, skill, skill.FloorCoordinate, string.Empty);
					break;
					case (ActorSnoEnum)181748:
					CompanionBoarDecorator.Paint(layer, skill, skill.FloorCoordinate, string.Empty);
					break;
					case (ActorSnoEnum)178664:
					CompanionFerretDecorator.Paint(layer, skill, skill.FloorCoordinate, string.Empty);
					break;
					case (ActorSnoEnum)159098:
					CompanionRuneCDecorator.Paint(layer, skill, skill.FloorCoordinate, string.Empty);
					break;
					case (ActorSnoEnum)345682:   //x1_Crusader_Phalanx 贴身卫士
					PhalanxDecorator.Paint(layer, skill, skill.FloorCoordinate, string.Empty);
					PhalanxTimerDecorator.Paint(layer, skill, skill.FloorCoordinate, string.Empty);
					break;	
					case (ActorSnoEnum)369795:    //x1_Crusader_PhalanxArcher 弓箭手
					PhalanxArcherDecorator.Paint(layer, skill, skill.FloorCoordinate, string.Empty);
					PhalanxArcherTimerDecorator.Paint(layer, skill, skill.FloorCoordinate, string.Empty);
					break;	
					case (ActorSnoEnum)338807: //x1_Crusader_Phalanx3_blocker 人墙
					PhalanxBlockerTimerDecorator.Paint(layer, skill, skill.FloorCoordinate, string.Empty);
					break;	
					case (ActorSnoEnum)98010:		//Wizard_MirrorImage_Female
					case (ActorSnoEnum)107916:	//Wizard_MirrorImage_Male
					MirrorImageDecorator.Paint(layer, skill, skill.FloorCoordinate, string.Empty);
					MirrorImageTimerDecorator.Paint(layer, skill, skill.FloorCoordinate, string.Empty);
					break;
					case (ActorSnoEnum)81103:
					case (ActorSnoEnum)81239:
					case (ActorSnoEnum)81238:
					case (ActorSnoEnum)83028:
					case (ActorSnoEnum)83959:
					HydraTimerDecorator.Paint(layer, skill, skill.FloorCoordinate, string.Empty);
					break;
					case (ActorSnoEnum)249334:
					DemonChainsDecorator.Paint(layer, skill, skill.FloorCoordinate, string.Empty);
					DemonChainsTimerDecorator.Paint(layer, skill, skill.FloorCoordinate, string.Empty);
					break;
					case (ActorSnoEnum)362949:
					case (ActorSnoEnum)362951:
					case (ActorSnoEnum)362952:
					case (ActorSnoEnum)362953:
					case (ActorSnoEnum)362954:
					case (ActorSnoEnum)362955:
					case (ActorSnoEnum)362956:
					case (ActorSnoEnum)362957:
					case (ActorSnoEnum)362958:
					case (ActorSnoEnum)362959:
					case (ActorSnoEnum)362960:
					case (ActorSnoEnum)362961:
					HauntOfVaxoDecorator.Paint(layer, skill, skill.FloorCoordinate, string.Empty);
					break;
				}
			
            }
        }
    }
}