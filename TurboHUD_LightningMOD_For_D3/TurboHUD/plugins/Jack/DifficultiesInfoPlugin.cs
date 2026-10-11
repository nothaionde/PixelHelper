namespace Turbo.Plugins.Jack
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Turbo.Plugins.Default;
    using Turbo.Plugins.Jack.Decorators.TopTables;

    public class DifficultiesInfoPlugin : BasePlugin, IInGameTopPainter
    {
        public Func<int, string> TormentLabelFunc { get; set; } = (pos) => string.Format("T{0}", pos - 3);
        public Dictionary<int, string> LowerDifficultiesLabels { get; set; } = new Dictionary<int, string>() {
            { 0, "Обычный" },
            { 1, "Трудный" },
            { 2, "Эксперт" },
            { 3, "Мастер" },
			{ 4, "И1" },
            { 5, "И2" },
            { 6, "И3" },
            { 7, "И4" },
            { 8, "И5" },
            { 9, "И6" },
            { 10, "И7" },
            { 11, "И8" },
			{ 12, "И9" },
            { 13, "И10" },
            { 14, "И11" },
            { 15, "И12" },
			{ 16, "И13" },
            { 17, "И14" },
            { 18, "И15" },
            { 19, "И16" },			
        };
        public Dictionary<int, string> LineHeaders { get; set; } = new Dictionary<int, string>()
        {
            { 0, "Аналог Великого Портала" },
            { 1, "Здоровье Монстра" },
            { 2, "Урон Монстра" },
            { 3, "+ Опыта" },
            { 4, "+ Поиск золота" },
            { 5, "Выпадение Лег" },
            { 6, "Выпадение Лег (портал)" },
            { 7, "Дыхание Смерти" },
            { 8, "Камень Великого Портала" },
            { 9, "Хорадрим.Ресы в наличие" },
            { 10, "Хорадрим.Леги в наличие" },
            { 11, "Дроп Адского Устройства" },
            { 12, "Дроп Органов Уберов" },
        };

        public TopTable Table { get; private set; }

        // updated by Silkdog569 for T14-T16 data
        private readonly string[,] difficultiesData = new string[,]
        {
            {   "-",        "ВП1",      "ВП4",      "ВП7",      "ВП10",     "ВП13",     "ВП16",     "ВП19",     "ВП22",     "ВП25",     "ВП30",     "ВП35",     "ВП40",     "ВП45",     "ВП50",     "ВП55",     "ВП60",        "ВП65",        "ВП70",          "ВП75",         },
            {   "100%",     "200%",     "320%",     "512%",     "819%",     "1311%",    "2097%",    "3355%",    "5369%",    "8590%",    "18985%",   "41625%",   "91260%",   "200082%",  "438669%",  "961759%",  "2108607%",    "2889383%",    "6334823%",      "13888770%",    },
            {   "100%",     "130",      "189%",     "273%",     "396%",     "575%",     "833%",     "1208%",    "1752%",    "2540%",    "3604%",    "5097%",    "7208%",    "10194%",   "14416%",   "20387%",   "28832%",      "40774%" ,     "57664%",        "64725%",       },
            {   "0%",       "75%",      "100%",     "200%",     "300%",     "400%",     "550%",     "800%",     "1150%",    "1600%",    "1900%",    "2425%",    "3100%",    "4000%",    "5000%",    "6400%",    "8200%",       "10500%",      "13400%",        "17000%",       },
            {   "0%",       "75%",      "100%",     "200%",     "300%",     "400%",     "550%",     "800%",     "1150%",    "1600%",    "1850%",    "2150%",    "2500%",    "2900%",    "3350%",    "3900%",    "4500%",       "5200%",       "6050%",         "7000%",        },
            {   "0%",       "0%",       "0%",       "0%",       "15%",      "32%",      "52%",      "75%",      "101%",     "131%",     "164%",     "205%",     "256%",     "320%",     "400%",     "500%",     "625%",        "781%",        "977%",          "1221%",        },
            {   "25%",      "25%",      "25%",      "25%",      "44%",      "65%",      "90%",      "119%",     "151%",     "189%",     "236%",     "295%",     "369%",     "461%",     "577%",     "721%",     "901%",        "1126%",       "1408%",         "1760%",        },
            {   "15%",      "18%",      "21%",      "25%",      "31%",      "37%",      "44%",      "53%",      "64%",      "75%",      "90%",      "2ые 15%",  "2ые 25%",  "2ые 50%",  "2ые 90%",  "3ые 12%",  "3ые 25%",     "3ые 60%",     "3",             "4 50%",      },
            {   "1",        "2ые 5%",   "2ые 10%",  "2ые 15%",  "2ые 20%",  "2ые 25%",  "2ые 31%",  "2ые 38%",  "2ые 44%",  "2ые 51%",  "2ые 60%",  "2ые 70%",  "2ые 80%",  "2ые 90%",  "2",        "3ые 12%",  "3ые 25%",     "3ые 38%",     "3ые 51%",       "3ые 66%",      },
            {   "3",        "3",        "3",        "3",        "6",        "6",        "6",        "6",        "6",        "6",        "8",        "8",        "8",        "10",       "12",       "14",       "16",          "18",          "20",            "22",           },
            {   "10%",      "10%",      "10%",      "10%",      "10%",      "50%",      "60%",      "75%",      "90%",      "100%",     "2ые 5%",   "2ые 15%",  "2ые 25%",  "2ые 50%",  "2ые 65%",  "2ые 80%",  "2",           "3ые 25%",     "3ые 50%",       "3ые 80%",      },
            {   "-",        "-",        "-",        "-",        "50%",      "75%",      "90%",      "100%",     "2ые 10%",  "2ые 20%",  "2ые 30%",  "2ые 40%",  "2ые 50%",  "2ые 60%",  "2ые 70%",  "2ые 80%",  "2ые 90%",     "2",           "3ые 10%",       "3ые 20%",      },
            {   "-",        "-",        "-",        "-",        "1",        "2ые 10%",  "2ые 25%",  "2ые 50%",  "2ые 75%",  "2ые 90%",  "200%",     "3ые 10%",  "3ые 25%",  "3ые 50%",  "3ые 75%",  "3ые 90%",  "3",           "4 12%",     "4 25%",       "4 50%",      },
        };

        private TopTableHeader HeaderTemplate => new TopTableHeader(Hud, (pos, curPos) => GetColumnHeaderText(pos))
        {
            RatioHeight = 22 / 1080f,
            RatioWidth = 0.075f,
            HighlightFunc = (pos, curPos) => (int)Hud.Game.GameDifficulty == pos,
            TextAlign = HorizontalAlign.Center,
        };

        private TopTableCell[] LineCellsTemplate => Enumerable
            .Range(0, difficultiesData.GetLength(1))
            .Select(x => new TopTableCell(Hud, (line, column, lineSorted, columnSorted) => difficultiesData[line, column]) { TextAlign = HorizontalAlign.Center }).ToArray();

        private TopTableHeader LineHeaderTemplate => new TopTableHeader(Hud, (pos, curPos) => GetLineHeaderText(pos))
        {
            RatioWidth = 62 / 1080f, // define only once on first line, value on other will be ignored
            RatioHeight = 22 / 1080f,
            HighlightFunc = (pos, curPos) => false,
            TextAlign = HorizontalAlign.Right,
            HighlightDecorator = new TopTableCellDecorator(Hud)
            {
                BackgroundBrush = Hud.Render.CreateBrush(255, 0, 0, 0, 0),
                BorderBrush = Hud.Render.CreateBrush(255, 255, 255, 255, -1),
                TextFont = Hud.Render.CreateFont("tahoma", 8, 255, 255, 255, 255, true, false, true),
            },
            CellHighlightDecorator = new TopTableCellDecorator(Hud)
            {
                BackgroundBrush = Hud.Render.CreateBrush(255, 0, 0, 0, 0),
                BorderBrush = Hud.Render.CreateBrush(255, 255, 255, 255, -1),
                TextFont = Hud.Render.CreateFont("tahoma", 8, 255, 255, 255, 255, true, false, true),
            },
        };

        public DifficultiesInfoPlugin()
        {
            Enabled = true;
            Order = int.MaxValue;
        }

        private void InitTable()
        {
            Table = new TopTable(Hud)
            {
                RatioPositionX = 0.5f,
                RatioPositionY = 0.2f,
                HorizontalCenter = true,
                VerticalCenter = false,
                PositionFromRight = false,
                PositionFromBottom = false,
                ShowHeaderLeft = true,
                ShowHeaderTop = true,
                ShowHeaderRight = false,
                ShowHeaderBottom = false,
                DefaultCellDecorator = new TopTableCellDecorator(Hud)
                {
                    BackgroundBrush = Hud.Render.CreateBrush(255, 0, 0, 0, 0),
                    BorderBrush = Hud.Render.CreateBrush(255, 255, 255, 255, -1),
                    TextFont = Hud.Render.CreateFont("tahoma", 8, 255, 255, 255, 255, false, false, true),
                },
                DefaultHighlightDecorator = new TopTableCellDecorator(Hud)
                {
                    BackgroundBrush = Hud.Render.CreateBrush(255, 0, 0, 242, 0),
                    BorderBrush = Hud.Render.CreateBrush(255, 255, 255, 255, -1),
                    TextFont = Hud.Render.CreateFont("tahoma", 8, 255, 255, 255, 255, false, false, true),
                },
                DefaultHeaderDecorator = new TopTableCellDecorator(Hud)
                {
                    //BackgroundBrush = Hud.Render.CreateBrush(0, 0, 0, 0, 0),
                    //BorderBrush = Hud.Render.CreateBrush(255, 255, 255, 255, 1),
                    TextFont = Hud.Render.CreateFont("tahoma", 8, 255, 255, 255, 255, false, false, true),
                }
            };

            var columnCount = difficultiesData.GetLength(1);
            var headers = Enumerable.Range(0, columnCount).Select(x => HeaderTemplate).ToArray();
            Table.DefineColumns(headers);

            var lineCount = difficultiesData.GetLength(0);
            for (var i = 0; i < lineCount; i++)
            {
                Table.AddLine(
                    LineHeaderTemplate,
                    LineCellsTemplate
                );
            }
        }

        private string GetColumnHeaderText(int pos)
        {
            if (LowerDifficultiesLabels.ContainsKey(pos))
                return LowerDifficultiesLabels[pos];

            return TormentLabelFunc?.Invoke(pos) ?? "???";
        }

        private string GetLineHeaderText(int pos)
        {
            if (LineHeaders.ContainsKey(pos))
                return LineHeaders[pos];

            return string.Empty;
        }

        public void PaintTopInGame(ClipState clipState)
        {
            if (!Hud.Game.IsInTown) return;
            if (clipState != ClipState.AfterClip) return;

            var uiRect = Hud.Render.GetUiElement("Root.NormalLayer.minimap_dialog_backgroundScreen.minimap_dialog_pve.BoostWrapper.BoostsDifficultyStackPanel.clock");
            if (!Hud.Window.CursorInsideRect(uiRect.Rectangle.X, uiRect.Rectangle.Y, uiRect.Rectangle.Width, uiRect.Rectangle.Height)) return;
            // if (!Hud.Window.CursorInsideRect(Hud.Window.Size.Width * 0.9f, Hud.Window.Size.Height * 0.02f, Hud.Window.Size.Width * 0.1f, Hud.Window.Size.Height * 0.02f)) return;

            if (Table == null)
                InitTable();
            else
                Table.Paint();
        }
    }
}