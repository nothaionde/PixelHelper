namespace Turbo.Plugins.glq
{
    using System.Globalization;
    using System.Linq;
    using Turbo.Plugins.Default;
    using Turbo.Plugins.Jack.Decorators.TopTables;

    public class GLQ_PlayerInfoPlugin1 : BasePlugin, INewAreaHandler, IInGameTopPainter
    {
        public TopTable Table { get; set; }

        private int lastPlayerCount = -1;
        private int hoveredPlayerIndex = -1;

        public GLQ_PlayerInfoPlugin1()
        {
            Enabled = true;
            Order = int.MaxValue;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);

            Table = new TopTable(Hud)
            {
                RatioPositionX = 0.5f,
                RatioPositionY = 0.03f,
                HorizontalCenter = true,
                VerticalCenter = false,
                PositionFromRight = false,
                PositionFromBottom = false,
                ShowHeaderLeft = false,
                ShowHeaderTop = true,
                ShowHeaderRight = false,
                ShowHeaderBottom = false,
                DefaultCellDecorator = new TopTableCellDecorator(Hud)
                {
                    BackgroundBrush = Hud.Render.CreateBrush(255, 0, 0, 0, 0),
                    BorderBrush = Hud.Render.CreateBrush(255, 70, 56, 42, 1.5f),
                    TextFont = Hud.Render.CreateFont("tahoma", 7, 255, 255, 255, 255, false, false, false),
                },
                DefaultHighlightDecorator = new TopTableCellDecorator(Hud)
                {
                    BackgroundBrush = Hud.Render.CreateBrush(255, 0, 0, 242, 0),
                    BorderBrush = Hud.Render.CreateBrush(255, 70, 56, 42, 1.5f),
                    TextFont = Hud.Render.CreateFont("tahoma", 7, 255, 255, 255, 255, false, false, false),
                },
                DefaultHeaderDecorator = new TopTableCellDecorator(Hud)
                {
                    BackgroundBrush = Hud.Render.CreateBrush(255, 0, 0, 0, 0),
                    BorderBrush = Hud.Render.CreateBrush(255, 70, 56, 42, 1.5f),
                    TextFont = Hud.Render.CreateFont("tahoma", 7, 255, 255, 255, 255, false, false, true),
                }
            };
            DefineColumns();
        }

        public void OnNewArea(bool newGame, ISnoArea area)
        {
            if (!newGame) return;

            lastPlayerCount = -1;
        }

        private void DefineTable()
        {
            Table.Reset(true); // keep columns

            foreach (var player in Hud.Game.Players.OrderBy(p => p.PortraitIndex))
            {
                if (player == null) continue;

                Table.AddLine(
                    new TopTableHeader(Hud, (pos, curPos) => "")
                    {
                        RatioWidth = 120 / 1080f,
                        RatioHeight = 28 / 1080f,
                        HighlightFunc = (pos, curPos) => hoveredPlayerIndex == pos,
                        HighlightDecorator = new TopTableCellDecorator(Hud)
                        {
                            BackgroundBrush = Hud.Render.CreateBrush(255, 255, 255, 128, 0),
                            BorderBrush = Hud.Render.CreateBrush(255, 70, 56, 42, 1.5f),
                            TextFont = Hud.Render.CreateFont("tahoma", 7, 255, 0, 0, 0, true, false, false),
                        },
                        CellHighlightDecorator = new TopTableCellDecorator(Hud)
                        {
                            BackgroundBrush = Hud.Render.CreateBrush(255, 255, 0, 255, 0),
                            BorderBrush = Hud.Render.CreateBrush(255, 70, 56, 42, 1.5f),
                            TextFont = Hud.Render.CreateFont("tahoma", 7, 255, 255, 255, 255, true, false, true),
                        },
                    },
                    new TopTableCell(Hud, (l, c, ls, cs) => GetCellText(l, c)),
                    new TopTableCell(Hud, (l, c, ls, cs) => GetCellText(l, c)),
                    new TopTableCell(Hud, (l, c, ls, cs) => GetCellText(l, c)),
                    new TopTableCell(Hud, (l, c, ls, cs) => GetCellText(l, c)),
                    new TopTableCell(Hud, (l, c, ls, cs) => GetCellText(l, c)),
                    new TopTableCell(Hud, (l, c, ls, cs) => GetCellText(l, c)),
                    new TopTableCell(Hud, (l, c, ls, cs) => GetCellText(l, c)),
                    new TopTableCell(Hud, (l, c, ls, cs) => GetCellText(l, c)),
                    new TopTableCell(Hud, (l, c, ls, cs) => GetCellText(l, c)),
                    new TopTableCell(Hud, (l, c, ls, cs) => GetCellText(l, c)),
                    new TopTableCell(Hud, (l, c, ls, cs) => GetCellText(l, c))
                    );
            }
        }

        private string GetCellText(int line, int column)
        {
            var player = Hud.Game.Players.FirstOrDefault(p => p.PortraitIndex == line);
            if (player == null) return string.Empty;

            switch (column)
            {
                case 0:
                    return player.BattleTagAbovePortrait;
                case 1:
                    return ValueToString((long)player.Defense.EhpCur, ValueFormat.LongNumber);
                case 2:
                    return ValueToString((long)player.Offense.SheetDps, ValueFormat.LongNumber);
                case 3:
                    return player.Stats.MainStat.ToString();
                case 4:
                    return player.Offense.AttackSpeedPets.ToString("F2", CultureInfo.InvariantCulture) + "/сек";
                case 5:
                    return player.Offense.CriticalHitChance.ToString("F2", CultureInfo.InvariantCulture) + "%";
                case 6:
                    return player.Offense.CritDamage.ToString("F2", CultureInfo.InvariantCulture) + "%";
                case 7:
                    return (player.Stats.CooldownReduction * 100).ToString("F1", CultureInfo.InvariantCulture) + "%";
                case 8:
                    return (player.Stats.ResourceCostReduction * 100).ToString("F1", CultureInfo.InvariantCulture) + "%";
                case 9:
                    return player.Offense.AreaDamageBonus.ToString() + "%";
                case 10:
                    return player.HighestSoloRiftLevel.ToString() + " УР";
                default:
                    return string.Empty;
            }
        }

        private void DefineColumns()
        {
            Table.DefineColumns(
                new TopTableHeader(Hud, (pos, curPos) => "Имя Игрока")
                {
                    RatioHeight = 28 / 1080f,
                    RatioWidth = 120 / 1080f,
                },
                new TopTableHeader(Hud, (pos, curPos) => "ХпТек")
                {
                    RatioWidth = 0.07f,
                },
                new TopTableHeader(Hud, (pos, curPos) => "ДПС")
                {
                    RatioWidth = 0.07f,
                },
                new TopTableHeader(Hud, (pos, curPos) => "МСтат")
                {
                    RatioWidth = 0.06f,
                },
                new TopTableHeader(Hud, (pos, curPos) => "АтСкор")
                {
                    RatioWidth = 0.06f,
                },
                new TopTableHeader(Hud, (pos, curPos) => "КШ")
                {
                    RatioWidth = 0.08f,
                },
                new TopTableHeader(Hud, (pos, curPos) => "КДМГ")
                {
                    RatioWidth = 0.07f,
                },
                new TopTableHeader(Hud, (pos, curPos) => "КДР")
                {
                    RatioWidth = 0.045f,
                },
                new TopTableHeader(Hud, (pos, curPos) => "РКР")
                {
                    RatioWidth = 0.045f,
                },
                new TopTableHeader(Hud, (pos, curPos) => "Область")
                {
                    RatioWidth = 0.06f,
                },
                new TopTableHeader(Hud, (pos, curPos) => "СолоВП")
                {
                    RatioWidth = 0.06f,
                }
            );
        }

        public void PaintTopInGame(ClipState clipState)
        {
            if (clipState != ClipState.BeforeClip) return;
            if (Hud.Inventory.StashMainUiElement.Visible) return;
            var myPortrait = Hud.Game.Me.PortraitUiElement.Rectangle;
            if (Hud.Window.CursorX > myPortrait.Right)
            {
                lastPlayerCount = -1;
                return; // cursor is too much to the right, no need to go further
            }

            if (lastPlayerCount != Hud.Game.Players.Count())
            {
                lastPlayerCount = Hud.Game.Players.Count();
                DefineTable();
                return; // no need to lose more time within this frame
            }

            var displayTable = false;
            foreach (var player in Hud.Game.Players.OrderBy(p => p.PortraitIndex))
            {
                if (player == null) continue;
                var portrait = player.PortraitUiElement.Rectangle;
                if (!Hud.Window.CursorInsideRect(portrait.X, portrait.Y, portrait.Width, portrait.Height)) continue;

                hoveredPlayerIndex = (Hud.Game.NumberOfPlayersInGame > 1) ? player.PortraitIndex : -1;
                displayTable = true;
            }

            if (displayTable && Table != null)
                Table.Paint();
        }
    }
}