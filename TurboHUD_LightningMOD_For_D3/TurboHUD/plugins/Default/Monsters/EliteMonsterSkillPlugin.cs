using System.Threading;
using System;

namespace Turbo.Plugins.Default
{
    public class EliteMonsterSkillPlugin : BasePlugin, IInGameWorldPainter
    {
        public double WallWidth { get; set; } = 1.5d;
        public double WallLength { get; set; } = 18d;
        public IBrush WallBrush { get; set; }
        private double toRad { get; set; } = Math.PI / 180d;
        public bool Wall { get; set; } = false;
        public bool Wormhole { get; set; } = false; 
        public WorldDecoratorCollection FrozenBallDecorator { get; set; }
        public WorldDecoratorCollection MoltenDecorator { get; set; }
        public WorldDecoratorCollection MoltenExplosionDecorator { get; set; }
        public WorldDecoratorCollection DesecratorDecorator { get; set; }
        public WorldDecoratorCollection ThunderstormDecorator { get; set; }
        public WorldDecoratorCollection PlaguedDecorator { get; set; }
        public WorldDecoratorCollection GhomDecorator { get; set; }
        public WorldDecoratorCollection ArcaneDecorator { get; set; }
        public WorldDecoratorCollection ArcaneSpawnDecorator { get; set; }
        public WorldDecoratorCollection FrozenPulseDecorator { get; set; }
        public WorldDecoratorCollection WormholeDecorator { get; set; }//虫洞
        public WorldDecoratorCollection InWormholeDecorator { get; set; }//虫洞
        public WorldDecoratorCollection PoisonEnchantedDecorator { get; set; }//强毒
        public WorldDecoratorCollection OrbiterDecorator { get; set; }//电球
        private string str_InWormhole;
        public EliteMonsterSkillPlugin()
        {
            Enabled = true;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            if (Hud.CurrentLanguage == Language.zhCN)
            {
                str_InWormhole = "踩到虫洞";
            }
            else if (Hud.CurrentLanguage == Language.zhTW)
            {
                str_InWormhole = "踩到蟲洞";
            }
            else if (Hud.CurrentLanguage == Language.ruRU)
            {
                str_InWormhole = "В Червоточине";
            }
            else
            {
                str_InWormhole = "In Wormhole";
            }
            WallBrush = Hud.Render.CreateBrush(255, 170, 80, 40, 5);
            FrozenBallDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud)
                {
                    Brush = Hud.Render.CreateBrush(128, 200, 200, 255, 3, SharpDX.Direct2D1.DashStyle.Dash),
                    Radius = 15.8f,
                },
                new GroundLabelDecorator(Hud)
                {
                    CountDownFrom = 3,
                    TextFont = Hud.Render.CreateFont("tahoma", 9, 255, 255, 255, 255, true, false, 128, 0, 0, 0, true),
                },
                new GroundTimerDecorator(Hud)
                {
                    CountDownFrom = 3,
                    BackgroundBrushEmpty = Hud.Render.CreateBrush(128, 0, 0, 0, 0),
                    BackgroundBrushFill = Hud.Render.CreateBrush(160, 100, 100, 240, 0),
                    Radius = 30,
                }
                );
            MoltenDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud)
                {
                    Brush = Hud.Render.CreateBrush(160, 255, 50, 50, 3, SharpDX.Direct2D1.DashStyle.Dash),
                    Radius = 13f,
                }
                );
            MoltenExplosionDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud)
                {
                    Brush = Hud.Render.CreateBrush(160, 255, 50, 50, 3, SharpDX.Direct2D1.DashStyle.Dash),
                    Radius = 13f,
                },
                new GroundLabelDecorator(Hud)
                {
                    CountDownFrom = 3,
                    TextFont = Hud.Render.CreateFont("tahoma", 9, 255, 255, 255, 255, true, false, 128, 0, 0, 0, true),
                },
                new GroundTimerDecorator(Hud)
                {
                    CountDownFrom = 3,
                    BackgroundBrushEmpty = Hud.Render.CreateBrush(128, 0, 0, 0, 0),
                    BackgroundBrushFill = Hud.Render.CreateBrush(200, 255, 32, 32, 0),
                    Radius = 30,
                }
                );
            DesecratorDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud)
                {
                    Brush = Hud.Render.CreateBrush(160, 255, 50, 50, 3, SharpDX.Direct2D1.DashStyle.Dash),
                    Radius = 8f,
                }
                );
            ThunderstormDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud)
                {
                    Brush = Hud.Render.CreateBrush(16, 200, 200, 255, 0),
                    Radius = 16f,
                },
                new GroundCircleDecorator(Hud)
                {
                    Brush = Hud.Render.CreateBrush(128, 200, 200, 255, 3, SharpDX.Direct2D1.DashStyle.Dash),
                    Radius = 16f,
                }
                );
            PlaguedDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud)
                {
                    Brush = Hud.Render.CreateBrush(128, 160, 255, 160, 3, SharpDX.Direct2D1.DashStyle.Dash),
                    Radius = 12f,
                }
                );
            GhomDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud)
                {
                    Brush = Hud.Render.CreateBrush(128, 160, 255, 160, 3, SharpDX.Direct2D1.DashStyle.Dash),
                    Radius = 20f,
                }
                );
            ArcaneDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud)
                {
                    Brush = Hud.Render.CreateBrush(128, 255, 60, 255, 3, SharpDX.Direct2D1.DashStyle.Dash),
                    Radius = 6f,
                }
                );
            ArcaneSpawnDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud)
                {
                    Brush = Hud.Render.CreateBrush(128, 255, 60, 255, 3, SharpDX.Direct2D1.DashStyle.Dash),
                    Radius = 6f,
                },
                new GroundLabelDecorator(Hud)
                {
                    CountDownFrom = 2,
                    TextFont = Hud.Render.CreateFont("tahoma", 9, 255, 255, 255, 255, true, false, 128, 0, 0, 0, true),
                },
                new GroundTimerDecorator(Hud)
                {
                    CountDownFrom = 2,
                    BackgroundBrushEmpty = Hud.Render.CreateBrush(128, 0, 0, 0, 0),
                    BackgroundBrushFill = Hud.Render.CreateBrush(200, 255, 32, 255, 0),
                    Radius = 30,
                }
                );
            FrozenPulseDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud)
                {
                    Brush = Hud.Render.CreateBrush(128, 200, 200, 255, 3, SharpDX.Direct2D1.DashStyle.Dash),
                    Radius = 14f,
                }
                );
            WormholeDecorator = new WorldDecoratorCollection(
            new GroundCircleDecorator(Hud)
            {
                Radius = 3.8f,
                Brush = Hud.Render.CreateBrush(200, 255, 0, 255, 5, SharpDX.Direct2D1.DashStyle.Dash)
            });
            InWormholeDecorator = new WorldDecoratorCollection(
            new GroundCircleDecorator(Hud)
            {
                Radius = 3.8f,
                Brush = Hud.Render.CreateBrush(200, 255, 0, 255, 5, SharpDX.Direct2D1.DashStyle.Dash)
            },
            new GroundLabelDecorator(Hud)
            {
                BackgroundBrush = Hud.Render.CreateBrush(160, 255, 255, 255, 0),
                TextFont = Hud.Render.CreateFont("tahoma", 9, 255, 255, 0, 255, true, false, false)
            });
            PoisonEnchantedDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud)
                {
                    Radius = 3f,
                    Brush = Hud.Render.CreateBrush(200, 117, 217, 117, 2, SharpDX.Direct2D1.DashStyle.Dash)
                },
                new GroundLabelDecorator(Hud)
                {
                    CountDownFrom = 5,
                    TextFont = Hud.Render.CreateFont("tahoma", 7, 255, 117, 217, 117, true, false, 255, 0, 0, 0, true),
                },
                new GroundTimerDecorator(Hud)
                {
                    CountDownFrom = 5,
                    BackgroundBrushEmpty = Hud.Render.CreateBrush(128, 0, 0, 0, 0),
                    BackgroundBrushFill = Hud.Render.CreateBrush(160, 117, 217, 117, 0),
                    Radius = 15,
                });
            OrbiterDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud)
                {
                    Radius = 2f,
                    Brush = Hud.Render.CreateBrush(200, 53, 146, 255, 2, SharpDX.Direct2D1.DashStyle.Dash)
                },
                new GroundLabelDecorator(Hud)
                {
                    CountDownFrom = 15,
                    TextFont = Hud.Render.CreateFont("tahoma", 7, 255, 53, 146, 255, true, false, 255, 0, 0, 0, true),
                },
                new GroundTimerDecorator(Hud)
                {
                    CountDownFrom = 15,
                    BackgroundBrushEmpty = Hud.Render.CreateBrush(128, 0, 0, 0, 0),
                    BackgroundBrushFill = Hud.Render.CreateBrush(160, 53, 146, 255, 0),
                    Radius = 15,
                });
            OrbiterDecorator.Enabled = false;
            PoisonEnchantedDecorator.Enabled = false;
            GhomDecorator.Enabled = false;
        }

        public void PaintWorld(WorldLayer layer)
        {
            foreach (var actor in Hud.Game.Actors)
            {
                switch (actor.SnoActor.Sno)
                {
                    case ActorSnoEnum._monsteraffix_frozen_iceclusters:
                        FrozenBallDecorator.Paint(layer, actor, actor.FloorCoordinate, null);
                        break;
                    case ActorSnoEnum._monsteraffix_molten_deathstart_proxy:
                        MoltenExplosionDecorator.Paint(layer, actor, actor.FloorCoordinate, null);
                        break;
                    case ActorSnoEnum._monsteraffix_molten_deathexplosion_proxy:
                    case ActorSnoEnum._monsteraffix_molten_firering:
                        // case 247987:
                        MoltenDecorator.Paint(layer, actor, actor.FloorCoordinate, null);
                        break;
                    case ActorSnoEnum._monsteraffix_desecrator_damage_aoe:
                        DesecratorDecorator.Paint(layer, actor, actor.FloorCoordinate, null);
                        break;
                    case ActorSnoEnum._x1_monsteraffix_thunderstorm_impact:
                        ThunderstormDecorator.Paint(layer, actor, actor.FloorCoordinate, null);
                        break;
                    case ActorSnoEnum._monsteraffix_plagued_endcloud:
                    case ActorSnoEnum._creepmobarm:
                        PlaguedDecorator.Paint(layer, actor, actor.FloorCoordinate, null);
                        break;
                    case ActorSnoEnum._gluttony_gascloud_proxy:
                        GhomDecorator.Paint(layer, actor, actor.FloorCoordinate, null);
                        break;
                    case ActorSnoEnum._monsteraffix_arcaneenchanted_petsweep:
                    case ActorSnoEnum._monsteraffix_arcaneenchanted_petsweep_reverse:
                        ArcaneDecorator.Paint(layer, actor, actor.FloorCoordinate, null);
                        break;
                    case ActorSnoEnum._arcaneenchanteddummy_spawn:
                        ArcaneSpawnDecorator.Paint(layer, actor, actor.FloorCoordinate, null);
                        break;
                    case ActorSnoEnum._x1_monsteraffix_frozenpulse_monster:
                        FrozenPulseDecorator.Paint(layer, actor, actor.FloorCoordinate, null);
                        break;
                    case ActorSnoEnum._x1_monsteraffix_teleportmines:
                        if(Wormhole)
                        {
                            if (Hud.Game.Me.FloorCoordinate.XYDistanceTo(actor.FloorCoordinate) > 3.8f + Hud.Game.Me.RadiusBottom)
                            {
                                WormholeDecorator.Paint(layer, actor, actor.FloorCoordinate, string.Empty);
                            }
                            else
                            {
                                InWormholeDecorator.Paint(layer, actor, actor.FloorCoordinate, str_InWormhole);
                            }
                        }
                        break;
                    case ActorSnoEnum._monsteraffix_waller_model:
                        if (Wall)
                        {
                            DrawWall(WallBrush, actor.FloorCoordinate, Direction(actor));
                        }
                        break;
                    case ActorSnoEnum._x1_monsteraffix_corpsebomber_projectile:
                        PoisonEnchantedDecorator.Paint(layer, actor, actor.FloorCoordinate, string.Empty);
                        break;
                    case ActorSnoEnum._x1_monsteraffix_orbiter_projectile:
                        OrbiterDecorator.Paint(layer, actor, actor.FloorCoordinate, string.Empty);
                        break;
                }
            }
        }
        private float Direction(IActor actor)
        {
            var diffX = (double)(actor.FloorCoordinate.X - actor.CollisionCoordinate.X);
            var diffY = (double)(actor.FloorCoordinate.Y - actor.CollisionCoordinate.Y);

            if (diffX == 0 && diffY == 0)
                return -45f;

            return (float)(Math.Atan2(diffY, diffX) / toRad) - 45f;
        }

        private void DrawWall(IBrush cBrush, IWorldCoordinate worldCoord, float rotation)
        {
            if (cBrush == null)
                return;

            var radius = ((float)Math.Sqrt(WallLength * WallLength + WallWidth * WallWidth)) * 0.5f;
            if (radius <= 0f)
                return;

            var revAngle = rotation * toRad + Math.Atan2(WallWidth, WallLength);
            var revX = radius * (float)Math.Cos(revAngle);
            var revY = radius * (float)Math.Sin(revAngle);
            var wCoord_1 = worldCoord.Offset(revX, revY, 0);
            var wCoord_3 = worldCoord.Offset(-revX, -revY, 0);

            revAngle = rotation * toRad - Math.Atan2(WallWidth, WallLength);
            revX = radius * (float)Math.Cos(revAngle);
            revY = radius * (float)Math.Sin(revAngle);
            var wCoord_2 = worldCoord.Offset(revX, revY, 0);
            var wCoord_4 = worldCoord.Offset(-revX, -revY, 0);

            cBrush.DrawLineWorld(wCoord_1, wCoord_2);
            cBrush.DrawLineWorld(wCoord_2, wCoord_3);
            cBrush.DrawLineWorld(wCoord_3, wCoord_4);
            cBrush.DrawLineWorld(wCoord_4, wCoord_1);
        }
    }
}