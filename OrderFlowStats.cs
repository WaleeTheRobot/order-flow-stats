#region Using declarations
using NinjaTrader.Data;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.NinjaScript.BarsTypes;
using NinjaTrader.NinjaScript.Indicators;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using System.Xml.Serialization;
#endregion

namespace NinjaTrader.NinjaScript.Indicators
{
    public enum BarTypeOptions
    {
        Minute,
        Range,
        Second,
        Tick,
        Volume
    }

    public enum TableDisplayModeType
    {
        None,
        ShowLast,
        PerBar
    }

    public class OrderFlowStats : Indicator
    {
        public const string GROUP_NAME_GENERAL = "1. General";
        public const string GROUP_NAME_ORDER_FLOW_STATS = "2. Order Flow Stats";
        public const string GROUP_NAME_DATA_BAR = "3. Data Bar";
        public const string GROUP_NAME_TABLE = "4. Table";

        private VolumetricBarsType _volumetricBars;
        private int _volumetricBarsIndex = -1;
        private SharpDX.Direct2D1.RenderTarget _lastRenderTarget;
        private SharpDX.DirectWrite.TextFormat _textFormat;
        private SharpDX.DirectWrite.TextFormat _textFormatBold;

        private Brush _bullBarNegDeltaBrush;
        private Brush _bearBarPosDeltaColorBrush;
        private Brush _deltaBarOutlineColorBrush;
        private SharpDX.Direct2D1.SolidColorBrush _neutralBrush;
        private SharpDX.Direct2D1.SolidColorBrush _deltaTextBrush;
        private SharpDX.Direct2D1.SolidColorBrush _increasingPosDeltaBrush;
        private SharpDX.Direct2D1.SolidColorBrush _decreasingPosDeltaBrush;
        private SharpDX.Direct2D1.SolidColorBrush _decreasingNegDeltaBrush;
        private SharpDX.Direct2D1.SolidColorBrush _increasingNegDeltaBrush;
        private SharpDX.Direct2D1.SolidColorBrush _increasingMaxDeltaBrush;
        private SharpDX.Direct2D1.SolidColorBrush _decreasingMaxDeltaBrush;
        private SharpDX.Direct2D1.SolidColorBrush _decreasingMinDeltaBrush;
        private SharpDX.Direct2D1.SolidColorBrush _increasingMinDeltaBrush;
        private SharpDX.Direct2D1.SolidColorBrush _tableBgBrush;
        private SharpDX.Direct2D1.SolidColorBrush _tableLabelBrush;
        private SharpDX.Direct2D1.SolidColorBrush _positiveTextBrush;
        private SharpDX.Direct2D1.SolidColorBrush _negativeTextBrush;
        private SharpDX.Direct2D1.SolidColorBrush _volumeTextBrush;
        private SharpDX.Direct2D1.SolidColorBrush _minDeltaTextBrush;
        private SharpDX.Direct2D1.SolidColorBrush _maxDeltaTextBrush;
        private SharpDX.Direct2D1.SolidColorBrush _pocBrush;

        #region Properties

        #region General Properties

        [NinjaScriptProperty]
        [Display(Name = "Version", Description = "Order Flow Stats Version", Order = 0, GroupName = GROUP_NAME_GENERAL)]
        [ReadOnly(true)]
        public string Version => "1.0.0";

        #endregion

        #region Order Flow Stats Properties

        [Range(1, int.MaxValue), NinjaScriptProperty]
        [Display(Name = "Volumetric Period", Description = "Volumetric data series period", GroupName = GROUP_NAME_ORDER_FLOW_STATS, Order = 0)]
        public int VolumetricPeriod { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Volumetric Bars Type", Description = "The type of bars for the volumetric data series", GroupName = GROUP_NAME_ORDER_FLOW_STATS, Order = 1)]
        public BarTypeOptions VolumetricBarsType { get; set; }

        [Range(1, int.MaxValue), NinjaScriptProperty]
        [Display(Name = "Ticks Per Level", Description = "The ticks per level", GroupName = GROUP_NAME_ORDER_FLOW_STATS, Order = 2)]
        public int TicksPerLevel { get; set; }

        [Range(6, 14), NinjaScriptProperty]
        [Display(Name = "Text Size", Description = "Font size for the table text (6 to 14)", GroupName = GROUP_NAME_ORDER_FLOW_STATS, Order = 3)]
        public float TextSize { get; set; }

        #endregion

        #region Data Bar Properties

        [NinjaScriptProperty]
        [Display(Name = "Show Price Delta Bar", Description = "Show the bar color change for price delta divergence.", GroupName = GROUP_NAME_DATA_BAR, Order = 0)]
        public bool ShowPriceDeltaBar { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Bull Bar Neg Delta Color", Description = "The color for a bullish bar and negative delta.", GroupName = GROUP_NAME_DATA_BAR, Order = 1)]
        public Brush BullBarNegDeltaColor { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Bear Bar Pos Delta Color", Description = "The color for a bearish bar and positive delta.", GroupName = GROUP_NAME_DATA_BAR, Order = 2)]
        public Brush BearBarPosDeltaColor { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Bull Bear Bar Outline Color", Description = "The bar outline color for the bull bear delta bar.", GroupName = GROUP_NAME_DATA_BAR, Order = 3)]
        public Brush DeltaBarOutlineColor { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Show Point of Control", Description = "Show point of control.", GroupName = GROUP_NAME_DATA_BAR, Order = 4)]
        public bool ShowPoc { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Point of Control Color", Description = "The point of control color.", GroupName = GROUP_NAME_DATA_BAR, Order = 5)]
        public Brush PocColor { get; set; }

        #endregion

        #region Table Properties

        [NinjaScriptProperty]
        [Display(Name = "Table Display Mode", Description = "How to display the table", GroupName = GROUP_NAME_TABLE, Order = 0)]
        public TableDisplayModeType TableDisplayMode { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Max Table Columns", Description = "Max table columns to show", GroupName = GROUP_NAME_TABLE, Order = 1)]
        public int MaxTableColumns { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Table Background Color", Description = "The table background color.", GroupName = GROUP_NAME_TABLE, Order = 2)]
        public Brush TableBgColor { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Table Label Color", Description = "The table label color.", GroupName = GROUP_NAME_TABLE, Order = 3)]
        public Brush TableLabelColor { get; set; }

        [Range(1, int.MaxValue), NinjaScriptProperty]
        [Display(Name = "Neutral Threshold", Description = "The neutral threshold.", GroupName = GROUP_NAME_TABLE, Order = 4)]
        public int NeutralThreshold { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Neutral Text Color", Description = "The neutral text color.", GroupName = GROUP_NAME_TABLE, Order = 5)]
        public Brush NeutralTextColor { get; set; }

        [Range(1, int.MaxValue), NinjaScriptProperty]
        [Display(Name = "Delta Threshold", Description = "The delta, min delta or max delta threshold to consider for increasing or decreasing comparison.", GroupName = GROUP_NAME_TABLE, Order = 6)]
        public int DeltaThreshold { get; set; }

        [Range(1, int.MaxValue), NinjaScriptProperty]
        [Display(Name = "Delta Change (%)", Description = "The delta, min delta and max delta percentage change to consider for increasing or decreasing comparison.", GroupName = GROUP_NAME_TABLE, Order = 7)]
        public double DeltaChangePercentage { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Delta Text Color", Description = "The delta text color.", GroupName = GROUP_NAME_TABLE, Order = 8)]
        public Brush DeltaTextColor { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Increasing Pos Delta Text Color", Description = "The increasing positive delta text color.", GroupName = GROUP_NAME_TABLE, Order = 9)]
        public Brush IncreasingPosDeltaTextColor { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Decreasing Pos Delta Text Color", Description = "The decreasing positive delta text color.", GroupName = GROUP_NAME_TABLE, Order = 10)]
        public Brush DecreasingPosDeltaTextColor { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Decreasing Neg Delta Text Color", Description = "The decreasing negative delta text color.", GroupName = GROUP_NAME_TABLE, Order = 11)]
        public Brush DecreasingNegDeltaTextColor { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Increasing Neg Delta Text Color", Description = "The increasing negative delta text color.", GroupName = GROUP_NAME_TABLE, Order = 12)]
        public Brush IncreasingNegDeltaTextColor { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Increasing Max Delta Text Color", Description = "The increasing max delta text color.", GroupName = GROUP_NAME_TABLE, Order = 13)]
        public Brush IncreasingMaxDeltaTextColor { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Decreasing Max Delta Text Color", Description = "The decreasing max delta text color.", GroupName = GROUP_NAME_TABLE, Order = 14)]
        public Brush DecreasingMaxDeltaTextColor { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Decreasing Min Delta Text Color", Description = "The decreasing min delta text color.", GroupName = GROUP_NAME_TABLE, Order = 15)]
        public Brush DecreasingMinDeltaTextColor { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Increasing Min Delta Text Color", Description = "The increasing min delta text color.", GroupName = GROUP_NAME_TABLE, Order = 16)]
        public Brush IncreasingMinDeltaTextColor { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Positive Text Color", Description = "The positive text color.", GroupName = GROUP_NAME_TABLE, Order = 17)]
        public Brush PositiveTextColor { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Negative Text Color", Description = "The negative text color.", GroupName = GROUP_NAME_TABLE, Order = 18)]
        public Brush NegativeTextColor { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Volume Text Color", Description = "The volume text color.", GroupName = GROUP_NAME_TABLE, Order = 19)]
        public Brush VolumeTextColor { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Min Delta Text Color", Description = "The min delta text color.", GroupName = GROUP_NAME_TABLE, Order = 20)]
        public Brush MinDeltaTextColor { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Max Delta Text Color", Description = "The max delta text color.", GroupName = GROUP_NAME_TABLE, Order = 21)]
        public Brush MaxDeltaTextColor { get; set; }

        #endregion

        #region Serialization

        [Browsable(false)] public string BullBarNegDeltaColorSerialize { get => Serialize.BrushToString(BullBarNegDeltaColor); set => BullBarNegDeltaColor = Serialize.StringToBrush(value); }
        [Browsable(false)] public string BearBarPosDeltaColorSerialize { get => Serialize.BrushToString(BearBarPosDeltaColor); set => BearBarPosDeltaColor = Serialize.StringToBrush(value); }
        [Browsable(false)] public string DeltaBarOutlineColorSerialize { get => Serialize.BrushToString(DeltaBarOutlineColor); set => DeltaBarOutlineColor = Serialize.StringToBrush(value); }
        [Browsable(false)] public string TableBgColorSerialize { get => Serialize.BrushToString(TableBgColor); set => TableBgColor = Serialize.StringToBrush(value); }
        [Browsable(false)] public string TableLabelColorSerialize { get => Serialize.BrushToString(TableLabelColor); set => TableLabelColor = Serialize.StringToBrush(value); }
        [Browsable(false)] public string NeutralTextColorSerialize { get => Serialize.BrushToString(NeutralTextColor); set => NeutralTextColor = Serialize.StringToBrush(value); }
        [Browsable(false)] public string IncreasingMaxDeltaTextColorSerialize { get => Serialize.BrushToString(IncreasingMaxDeltaTextColor); set => IncreasingMaxDeltaTextColor = Serialize.StringToBrush(value); }
        [Browsable(false)] public string DecreasingMaxDeltaTextColorSerialize { get => Serialize.BrushToString(DecreasingMaxDeltaTextColor); set => DecreasingMaxDeltaTextColor = Serialize.StringToBrush(value); }
        [Browsable(false)] public string DecreasingMinDeltaTextColorSerialize { get => Serialize.BrushToString(DecreasingMinDeltaTextColor); set => DecreasingMinDeltaTextColor = Serialize.StringToBrush(value); }
        [Browsable(false)] public string IncreasingMinDeltaTextColorSerialize { get => Serialize.BrushToString(IncreasingMinDeltaTextColor); set => IncreasingMinDeltaTextColor = Serialize.StringToBrush(value); }
        [Browsable(false)] public string DeltaTextColorSerialize { get => Serialize.BrushToString(DeltaTextColor); set => DeltaTextColor = Serialize.StringToBrush(value); }
        [Browsable(false)] public string IncreasingPosDeltaTextColorSerialize { get => Serialize.BrushToString(IncreasingPosDeltaTextColor); set => IncreasingPosDeltaTextColor = Serialize.StringToBrush(value); }
        [Browsable(false)] public string DecreasingPosDeltaTextColorSerialize { get => Serialize.BrushToString(DecreasingPosDeltaTextColor); set => DecreasingPosDeltaTextColor = Serialize.StringToBrush(value); }
        [Browsable(false)] public string DecreasingNegDeltaTextColorSerialize { get => Serialize.BrushToString(DecreasingNegDeltaTextColor); set => DecreasingNegDeltaTextColor = Serialize.StringToBrush(value); }
        [Browsable(false)] public string IncreasingNegDeltaTextColorSerialize { get => Serialize.BrushToString(IncreasingNegDeltaTextColor); set => IncreasingNegDeltaTextColor = Serialize.StringToBrush(value); }
        [Browsable(false)] public string PositiveTextColorSerialize { get => Serialize.BrushToString(PositiveTextColor); set => PositiveTextColor = Serialize.StringToBrush(value); }
        [Browsable(false)] public string NegativeTextColorSerialize { get => Serialize.BrushToString(NegativeTextColor); set => NegativeTextColor = Serialize.StringToBrush(value); }
        [Browsable(false)] public string VolumeTextColorSerialize { get => Serialize.BrushToString(VolumeTextColor); set => VolumeTextColor = Serialize.StringToBrush(value); }
        [Browsable(false)] public string MinDeltaTextColorSerialize { get => Serialize.BrushToString(MinDeltaTextColor); set => MinDeltaTextColor = Serialize.StringToBrush(value); }
        [Browsable(false)] public string MaxDeltaTextColorSerialize { get => Serialize.BrushToString(MaxDeltaTextColor); set => MaxDeltaTextColor = Serialize.StringToBrush(value); }
        [Browsable(false)] public string PocColorSerialize { get => Serialize.BrushToString(PocColor); set => PocColor = Serialize.StringToBrush(value); }

        #endregion

        #endregion

        private static SharpDX.Color ToDxColor(System.Windows.Media.Brush brush)
        {
            var solid = brush as System.Windows.Media.SolidColorBrush;
            if (solid == null) return SharpDX.Color.White;
            System.Windows.Media.Color wpfColor = solid.Color;
            return new SharpDX.Color(wpfColor.R, wpfColor.G, wpfColor.B, wpfColor.A);
        }

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Displays Order Flow statistics on non-volumetric charts.";
                Name = "_OrderFlowStats";
                Calculate = Calculate.OnEachTick;
                IsOverlay = true;
                DisplayInDataBox = true;
                DrawOnPricePanel = true;
                DrawHorizontalGridLines = true;
                DrawVerticalGridLines = true;
                PaintPriceMarkers = true;
                ScaleJustification = NinjaTrader.Gui.Chart.ScaleJustification.Right;
                IsSuspendedWhileInactive = true;

                VolumetricPeriod = 5;
                VolumetricBarsType = BarTypeOptions.Minute;
                TicksPerLevel = 5;
                TextSize = 11;
                TableDisplayMode = TableDisplayModeType.PerBar;
                MaxTableColumns = 30;

                ShowPriceDeltaBar = true;
                ShowPoc = true;
                BullBarNegDeltaColor = Brushes.DarkTurquoise;
                BearBarPosDeltaColor = Brushes.DarkOrange;
                DeltaBarOutlineColor = Brushes.LightGray;

                NeutralThreshold = 20;
                NeutralTextColor = Brushes.DimGray;
                DeltaThreshold = 100;
                DeltaChangePercentage = 75;
                IncreasingPosDeltaTextColor = Brushes.LimeGreen;
                DecreasingPosDeltaTextColor = Brushes.DarkGoldenrod;
                DecreasingNegDeltaTextColor = Brushes.MediumVioletRed;
                IncreasingNegDeltaTextColor = Brushes.DarkGoldenrod;
                IncreasingMaxDeltaTextColor = Brushes.LimeGreen;
                DecreasingMaxDeltaTextColor = Brushes.DarkGoldenrod;
                DecreasingMinDeltaTextColor = Brushes.MediumVioletRed;
                IncreasingMinDeltaTextColor = Brushes.DarkGoldenrod;

                TableBgColor = Brushes.Black;
                TableLabelColor = Brushes.LightGray;
                PositiveTextColor = Brushes.DarkGreen;
                NegativeTextColor = Brushes.DarkRed;
                VolumeTextColor = Brushes.LightGray;
                DeltaTextColor = Brushes.LightGray;
                MinDeltaTextColor = Brushes.LightGray;
                MaxDeltaTextColor = Brushes.LightGray;
                PocColor = Brushes.Yellow;
            }
            else if (State == State.Configure)
            {
                BarsPeriodType barsPeriodType;

                switch (VolumetricBarsType)
                {
                    case BarTypeOptions.Minute:
                        barsPeriodType = BarsPeriodType.Minute;
                        break;
                    case BarTypeOptions.Range:
                        barsPeriodType = BarsPeriodType.Range;
                        break;
                    case BarTypeOptions.Second:
                        barsPeriodType = BarsPeriodType.Second;
                        break;
                    case BarTypeOptions.Tick:
                        barsPeriodType = BarsPeriodType.Tick;
                        break;
                    case BarTypeOptions.Volume:
                        barsPeriodType = BarsPeriodType.Volume;
                        break;
                    default:
                        barsPeriodType = BarsPeriodType.Minute;
                        break;
                }

                AddVolumetric(Instrument.FullName, barsPeriodType, VolumetricPeriod, VolumetricDeltaType.BidAsk, TicksPerLevel);
            }
            else if (State == State.DataLoaded)
            {
                _volumetricBarsIndex = 1;
                _volumetricBars = BarsArray[_volumetricBarsIndex].BarsType as VolumetricBarsType;
                _textFormat = new SharpDX.DirectWrite.TextFormat(Core.Globals.DirectWriteFactory, "Arial", TextSize);
                _textFormatBold = new SharpDX.DirectWrite.TextFormat(Core.Globals.DirectWriteFactory, "Arial", SharpDX.DirectWrite.FontWeight.Bold, SharpDX.DirectWrite.FontStyle.Normal, TextSize + 1);

                _bullBarNegDeltaBrush = BullBarNegDeltaColor;
                _bearBarPosDeltaColorBrush = BearBarPosDeltaColor;
                _deltaBarOutlineColorBrush = DeltaBarOutlineColor;
            }
            else if (State == State.Finalized)
            {
                if (_textFormat != null) { _textFormat.Dispose(); _textFormat = null; }
                if (_textFormatBold != null) { _textFormatBold.Dispose(); _textFormatBold = null; }
                DisposeBrushes();
            }
        }

        protected override void OnBarUpdate()
        {
            if (BarsInProgress != 0 || _volumetricBars == null || CurrentBar < 1)
                return;

            if (ShowPriceDeltaBar)
            {
                bool isPositiveBar = Close[0] > Open[0];
                bool isDeltaNegative = _volumetricBars.Volumes[CurrentBar].BarDelta < 0;

                if (isPositiveBar && isDeltaNegative)
                {
                    BarBrush = _bullBarNegDeltaBrush;
                    CandleOutlineBrush = _deltaBarOutlineColorBrush;
                }
                else if (!isPositiveBar && !isDeltaNegative)
                {
                    BarBrush = _bearBarPosDeltaColorBrush;
                    CandleOutlineBrush = _deltaBarOutlineColorBrush;
                }
                else
                {
                    BarBrush = null;
                    CandleOutlineBrush = null;
                }
            }
        }

        protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
        {
            base.OnRender(chartControl, chartScale);

            if (_volumetricBars == null || Bars == null || RenderTarget == null)
                return;

            if (_lastRenderTarget != RenderTarget)
            {
                DisposeBrushes();
                _neutralBrush = new SharpDX.Direct2D1.SolidColorBrush(RenderTarget, ToDxColor(NeutralTextColor));
                _deltaTextBrush = new SharpDX.Direct2D1.SolidColorBrush(RenderTarget, ToDxColor(DeltaTextColor));
                _increasingPosDeltaBrush = new SharpDX.Direct2D1.SolidColorBrush(RenderTarget, ToDxColor(IncreasingPosDeltaTextColor));
                _decreasingPosDeltaBrush = new SharpDX.Direct2D1.SolidColorBrush(RenderTarget, ToDxColor(DecreasingPosDeltaTextColor));
                _decreasingNegDeltaBrush = new SharpDX.Direct2D1.SolidColorBrush(RenderTarget, ToDxColor(DecreasingNegDeltaTextColor));
                _increasingNegDeltaBrush = new SharpDX.Direct2D1.SolidColorBrush(RenderTarget, ToDxColor(IncreasingNegDeltaTextColor));
                _increasingMaxDeltaBrush = new SharpDX.Direct2D1.SolidColorBrush(RenderTarget, ToDxColor(IncreasingMaxDeltaTextColor));
                _decreasingMaxDeltaBrush = new SharpDX.Direct2D1.SolidColorBrush(RenderTarget, ToDxColor(DecreasingMaxDeltaTextColor));
                _decreasingMinDeltaBrush = new SharpDX.Direct2D1.SolidColorBrush(RenderTarget, ToDxColor(DecreasingMinDeltaTextColor));
                _increasingMinDeltaBrush = new SharpDX.Direct2D1.SolidColorBrush(RenderTarget, ToDxColor(IncreasingMinDeltaTextColor));
                _tableBgBrush = new SharpDX.Direct2D1.SolidColorBrush(RenderTarget, ToDxColor(TableBgColor));
                _tableLabelBrush = new SharpDX.Direct2D1.SolidColorBrush(RenderTarget, ToDxColor(TableLabelColor));
                _positiveTextBrush = new SharpDX.Direct2D1.SolidColorBrush(RenderTarget, ToDxColor(PositiveTextColor));
                _negativeTextBrush = new SharpDX.Direct2D1.SolidColorBrush(RenderTarget, ToDxColor(NegativeTextColor));
                _volumeTextBrush = new SharpDX.Direct2D1.SolidColorBrush(RenderTarget, ToDxColor(VolumeTextColor));
                _minDeltaTextBrush = new SharpDX.Direct2D1.SolidColorBrush(RenderTarget, ToDxColor(MinDeltaTextColor));
                _maxDeltaTextBrush = new SharpDX.Direct2D1.SolidColorBrush(RenderTarget, ToDxColor(MaxDeltaTextColor));
                _pocBrush = new SharpDX.Direct2D1.SolidColorBrush(RenderTarget, ToDxColor(PocColor));
                _lastRenderTarget = RenderTarget;
            }

            RenderPocLines(chartControl, chartScale);

            if (TableDisplayMode == TableDisplayModeType.None) return;

            float rowHeight = 16f;
            int numRows = 5;
            float tableMargin = 25f;
            float labelColumnWidth = 70f;
            float tablePadding = 5f;
            float barWidth = (float)chartControl.BarWidth * 2.0f + 20f;
            int maxVisibleBars = MaxTableColumns;

            ChartPanel panel = chartControl.ChartPanels[0];
            float tableBottom = panel.Y + panel.H - tableMargin;
            float tableTop = tableBottom - (numRows * rowHeight);
            float labelX = chartControl.CanvasLeft + 5f;
            float dataStartX = labelX + labelColumnWidth;

            float tableWidth;
            if (TableDisplayMode == TableDisplayModeType.ShowLast)
            {
                float maxTextWidth = CalculateShowLastWidth();
                tableWidth = labelColumnWidth + maxTextWidth + 10f;
            }
            else
            {
                tableWidth = Math.Max(labelColumnWidth + barWidth, chartControl.GetXByBarIndex(ChartBars, ChartBars.ToIndex) + barWidth - labelX);
            }

            RenderTableBackgroundAndLabels(tableTop, tableWidth, labelX, dataStartX, rowHeight, numRows, tablePadding);

            if (TableDisplayMode == TableDisplayModeType.ShowLast)
                RenderTableShowLast(dataStartX, tableTop, rowHeight);
            else if (TableDisplayMode == TableDisplayModeType.PerBar)
                RenderTablePerBar(chartControl, dataStartX, tableTop, rowHeight, maxVisibleBars);
        }

        private void RenderTableBackgroundAndLabels(float tableTop, float tableWidth, float labelX, float dataStartX, float rowHeight, int numRows, float tablePadding)
        {
            SharpDX.RectangleF backgroundRect = new SharpDX.RectangleF(
                labelX - tablePadding,
                tableTop - tablePadding,
                tableWidth + 2 * tablePadding,
                numRows * rowHeight + 2 * tablePadding
            );
            RenderTarget.FillRectangle(backgroundRect, _tableBgBrush);

            string[] labels = { "Volume", "Cum. Delta", "Delta", "Min Delta", "Max Delta" };
            for (int i = 0; i < numRows; i++)
            {
                SharpDX.RectangleF labelRect = new SharpDX.RectangleF(labelX, tableTop + i * rowHeight, dataStartX - labelX, rowHeight);
                RenderTarget.DrawText(labels[i], _textFormat, labelRect, _tableLabelBrush);
            }
        }

        private void RenderTableShowLast(float dataStartX, float tableTop, float rowHeight)
        {
            int idx = ChartBars.ToIndex;
            if (idx < 1 || idx >= Bars.Count) return;

            float x = dataStartX;
            double volume = _volumetricBars.Volumes[idx].TotalVolume;
            double cumulativeDelta = _volumetricBars.Volumes[idx].CumulativeDelta;
            double delta = _volumetricBars.Volumes[idx].BarDelta;
            double minDelta = _volumetricBars.Volumes[idx].MinSeenDelta;
            double maxDelta = _volumetricBars.Volumes[idx].MaxSeenDelta;
            double prevDelta = _volumetricBars.Volumes[idx - 1].BarDelta;
            double prevMinDelta = _volumetricBars.Volumes[idx - 1].MinSeenDelta;
            double prevMaxDelta = _volumetricBars.Volumes[idx - 1].MaxSeenDelta;

            string[] texts = { volume.ToString("N0"), cumulativeDelta.ToString("N0"), delta.ToString("N0"), minDelta.ToString("N0"), maxDelta.ToString("N0") };
            SharpDX.Direct2D1.SolidColorBrush[] brushes = GetBrushes(delta, minDelta, maxDelta, prevDelta, prevMinDelta, prevMaxDelta, cumulativeDelta);
            bool[] isBold = GetBoldFlags(delta, minDelta, maxDelta, prevDelta, prevMinDelta, prevMaxDelta);

            for (int i = 0; i < texts.Length; i++)
            {
                SharpDX.RectangleF rect = new SharpDX.RectangleF(x, tableTop + i * rowHeight, 1000f, rowHeight);
                RenderTarget.DrawText(texts[i], isBold[i] ? _textFormatBold : _textFormat, rect, brushes[i]);
            }
        }

        private float CalculateShowLastWidth()
        {
            float maxTextWidth = 0f;
            int idx = ChartBars.ToIndex;
            if (idx >= 0 && idx < Bars.Count)
            {
                double volume = _volumetricBars.Volumes[idx].TotalVolume;
                double cumulativeDelta = _volumetricBars.Volumes[idx].CumulativeDelta;
                double delta = _volumetricBars.Volumes[idx].BarDelta;
                double minDelta = _volumetricBars.Volumes[idx].MinSeenDelta;
                double maxDelta = _volumetricBars.Volumes[idx].MaxSeenDelta;

                string[] texts = new[]
                {
                    volume.ToString("N0"),
                    cumulativeDelta.ToString("N0"),
                    delta.ToString("N0"),
                    minDelta.ToString("N0"),
                    maxDelta.ToString("N0")
                };

                for (int i = 0; i < texts.Length; i++)
                {
                    SharpDX.DirectWrite.TextLayout textLayout = new SharpDX.DirectWrite.TextLayout(
                        Core.Globals.DirectWriteFactory, texts[i], _textFormat, 1000f, 16f);
                    float textWidth = textLayout.Metrics.Width;
                    maxTextWidth = Math.Max(maxTextWidth, textWidth);
                    textLayout.Dispose();
                }
            }
            return maxTextWidth;
        }

        private void RenderTablePerBar(ChartControl chartControl, float dataStartX, float tableTop, float rowHeight, int maxVisibleBars)
        {
            int startIdx = Math.Max(ChartBars.FromIndex, ChartBars.ToIndex - maxVisibleBars + 1);
            for (int idx = startIdx; idx <= ChartBars.ToIndex; idx++)
            {
                if (idx < 0 || idx >= Bars.Count) continue;

                float barCenterX = chartControl.GetXByBarIndex(ChartBars, idx);
                float actualBarWidth = (float)chartControl.BarWidth;
                float barWidth = actualBarWidth * 2.0f + 20f;
                float x = barCenterX - (barWidth / 2) + (barWidth - actualBarWidth) / 2;
                if (x < dataStartX) x = dataStartX;

                RenderBarData(idx, x, barWidth, tableTop, rowHeight);
            }
        }

        private void RenderBarData(int idx, float x, float width, float tableTop, float rowHeight)
        {
            double volume = _volumetricBars.Volumes[idx].TotalVolume;
            double cumulativeDelta = _volumetricBars.Volumes[idx].CumulativeDelta;
            double delta = _volumetricBars.Volumes[idx].BarDelta;
            double minDelta = _volumetricBars.Volumes[idx].MinSeenDelta;
            double maxDelta = _volumetricBars.Volumes[idx].MaxSeenDelta;
            double prevDelta = idx > 0 ? _volumetricBars.Volumes[idx - 1].BarDelta : 0;
            double prevMinDelta = idx > 0 ? _volumetricBars.Volumes[idx - 1].MinSeenDelta : 0;
            double prevMaxDelta = idx > 0 ? _volumetricBars.Volumes[idx - 1].MaxSeenDelta : 0;

            string[] texts = { volume.ToString("N0"), cumulativeDelta.ToString("N0"), delta.ToString("N0"), minDelta.ToString("N0"), maxDelta.ToString("N0") };
            SharpDX.Direct2D1.SolidColorBrush[] brushes = GetBrushes(delta, minDelta, maxDelta, prevDelta, prevMinDelta, prevMaxDelta, cumulativeDelta);
            bool[] isBold = GetBoldFlags(delta, minDelta, maxDelta, prevDelta, prevMinDelta, prevMaxDelta);

            for (int i = 0; i < texts.Length; i++)
            {
                SharpDX.RectangleF rect = new SharpDX.RectangleF(x, tableTop + i * rowHeight, width, rowHeight);
                RenderTarget.DrawText(texts[i], isBold[i] ? _textFormatBold : _textFormat, rect, brushes[i]);
            }
        }

        private SharpDX.Direct2D1.SolidColorBrush[] GetBrushes(double delta, double minDelta, double maxDelta, double prevDelta, double prevMinDelta, double prevMaxDelta, double cumulativeDelta)
        {
            SharpDX.Direct2D1.SolidColorBrush[] brushes = new SharpDX.Direct2D1.SolidColorBrush[5];

            // Volume
            brushes[0] = _volumeTextBrush;

            // Cumulative Delta
            brushes[1] = cumulativeDelta >= 0 ? _positiveTextBrush : _negativeTextBrush;

            // Delta
            if (delta >= -NeutralThreshold && delta <= NeutralThreshold)
            {
                brushes[2] = _neutralBrush;
            }
            else if (prevDelta > DeltaThreshold && delta > DeltaThreshold)
            {
                double changePercent = (delta - prevDelta) / prevDelta * 100;
                if (changePercent > DeltaChangePercentage)
                {
                    brushes[2] = _increasingPosDeltaBrush;
                }
                else if (changePercent < -DeltaChangePercentage)
                {
                    brushes[2] = _decreasingPosDeltaBrush;
                }
                else
                {
                    brushes[2] = _positiveTextBrush;
                }
            }
            else if (prevDelta < -DeltaThreshold && delta < -DeltaThreshold)
            {
                double changePercent = (prevDelta - delta) / Math.Abs(prevDelta) * 100;
                if (changePercent > DeltaChangePercentage)
                {
                    brushes[2] = _decreasingNegDeltaBrush;
                }
                else if (changePercent < -DeltaChangePercentage)
                {
                    brushes[2] = _increasingNegDeltaBrush;
                }
                else
                {
                    brushes[2] = _negativeTextBrush;
                }
            }
            else
            {
                brushes[2] = delta >= 0 ? _positiveTextBrush : _negativeTextBrush;
            }

            // Min Delta
            if (minDelta >= -NeutralThreshold)
            {
                brushes[3] = _neutralBrush;
            }
            else if (prevMinDelta < -DeltaThreshold && minDelta < -DeltaThreshold)
            {
                double changePercent = (prevMinDelta - minDelta) / Math.Abs(prevMinDelta) * 100;
                if (changePercent > DeltaChangePercentage)
                {
                    brushes[3] = _decreasingMinDeltaBrush;
                }
                else if (changePercent < -DeltaChangePercentage)
                {
                    brushes[3] = _increasingMinDeltaBrush;
                }
                else
                {
                    brushes[3] = _minDeltaTextBrush;
                }
            }
            else
            {
                brushes[3] = _minDeltaTextBrush;
            }

            // Max Delta
            if (maxDelta <= NeutralThreshold)
            {
                brushes[4] = _neutralBrush;
            }
            else if (prevMaxDelta > DeltaThreshold && maxDelta > DeltaThreshold)
            {
                double changePercent = (maxDelta - prevMaxDelta) / prevMaxDelta * 100;
                if (changePercent > DeltaChangePercentage)
                {
                    brushes[4] = _increasingMaxDeltaBrush;
                }
                else if (changePercent < -DeltaChangePercentage)
                {
                    brushes[4] = _decreasingMaxDeltaBrush;
                }
                else
                {
                    brushes[4] = _maxDeltaTextBrush;
                }
            }
            else
            {
                brushes[4] = _maxDeltaTextBrush;
            }

            return brushes;
        }

        private bool[] GetBoldFlags(double delta, double minDelta, double maxDelta, double prevDelta, double prevMinDelta, double prevMaxDelta)
        {
            bool[] isBold = new bool[5] { false, false, false, false, false };

            // Delta
            if (prevDelta > DeltaThreshold && delta > DeltaThreshold)
            {
                double changePercent = Math.Abs((delta - prevDelta) / prevDelta) * 100;
                isBold[2] = changePercent > DeltaChangePercentage;
            }
            else if (prevDelta < -DeltaThreshold && delta < -DeltaThreshold)
            {
                double changePercent = Math.Abs((delta - prevDelta) / prevDelta) * 100;
                isBold[2] = changePercent > DeltaChangePercentage;
            }

            // Min Delta
            if (prevMinDelta < -DeltaThreshold && minDelta < -DeltaThreshold)
            {
                double changePercent = Math.Abs((minDelta - prevMinDelta) / prevMinDelta) * 100;
                isBold[3] = changePercent > DeltaChangePercentage;
            }

            // Max Delta
            if (prevMaxDelta > DeltaThreshold && maxDelta > DeltaThreshold)
            {
                double changePercent = Math.Abs((maxDelta - prevMaxDelta) / prevMaxDelta) * 100;
                isBold[4] = changePercent > DeltaChangePercentage;
            }

            return isBold;
        }

        private void RenderPocLines(ChartControl chartControl, ChartScale chartScale)
        {
            if (!ShowPoc) return;

            for (int idx = ChartBars.FromIndex; idx <= ChartBars.ToIndex; idx++)
            {
                if (idx < 0 || idx >= Bars.Count) continue;

                float barCenterX = chartControl.GetXByBarIndex(ChartBars, idx);
                float lineWidth = (float)chartControl.BarWidth * 2f;
                float halfWidth = lineWidth / 2f;
                float xStart = barCenterX - halfWidth;
                float xEnd = barCenterX + halfWidth;

                double pocPrice = 0;
                _volumetricBars.Volumes[idx].GetMaximumVolume(null, out pocPrice);
                float y = chartScale.GetYByValue(pocPrice);

                SharpDX.Vector2 point1 = new SharpDX.Vector2(xStart, y);
                SharpDX.Vector2 point2 = new SharpDX.Vector2(xEnd, y);
                RenderTarget.DrawLine(point1, point2, _pocBrush, 2);
            }
        }

        private void DisposeBrushes()
        {
            _neutralBrush?.Dispose(); _neutralBrush = null;
            _deltaTextBrush?.Dispose(); _deltaTextBrush = null;
            _increasingNegDeltaBrush?.Dispose(); _increasingNegDeltaBrush = null;
            _decreasingNegDeltaBrush?.Dispose(); _decreasingNegDeltaBrush = null;
            _decreasingPosDeltaBrush?.Dispose(); _decreasingPosDeltaBrush = null;
            _increasingPosDeltaBrush?.Dispose(); _increasingPosDeltaBrush = null;
            _increasingMinDeltaBrush?.Dispose(); _increasingMinDeltaBrush = null;
            _decreasingMinDeltaBrush?.Dispose(); _decreasingMinDeltaBrush = null;
            _decreasingMaxDeltaBrush?.Dispose(); _decreasingMaxDeltaBrush = null;
            _increasingMaxDeltaBrush?.Dispose(); _increasingMaxDeltaBrush = null;
            _tableBgBrush?.Dispose(); _tableBgBrush = null;
            _tableLabelBrush?.Dispose(); _tableLabelBrush = null;
            _positiveTextBrush?.Dispose(); _positiveTextBrush = null;
            _negativeTextBrush?.Dispose(); _negativeTextBrush = null;
            _volumeTextBrush?.Dispose(); _volumeTextBrush = null;
            _minDeltaTextBrush?.Dispose(); _minDeltaTextBrush = null;
            _maxDeltaTextBrush?.Dispose(); _maxDeltaTextBrush = null;
            _pocBrush?.Dispose(); _pocBrush = null;
        }
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private OrderFlowStats[] cacheOrderFlowStats;
		public OrderFlowStats OrderFlowStats(int volumetricPeriod, BarTypeOptions volumetricBarsType, int ticksPerLevel, float textSize, bool showPriceDeltaBar, Brush bullBarNegDeltaColor, Brush bearBarPosDeltaColor, Brush deltaBarOutlineColor, bool showPoc, Brush pocColor, TableDisplayModeType tableDisplayMode, int maxTableColumns, Brush tableBgColor, Brush tableLabelColor, int neutralThreshold, Brush neutralTextColor, int deltaThreshold, double deltaChangePercentage, Brush deltaTextColor, Brush increasingPosDeltaTextColor, Brush decreasingPosDeltaTextColor, Brush decreasingNegDeltaTextColor, Brush increasingNegDeltaTextColor, Brush increasingMaxDeltaTextColor, Brush decreasingMaxDeltaTextColor, Brush decreasingMinDeltaTextColor, Brush increasingMinDeltaTextColor, Brush positiveTextColor, Brush negativeTextColor, Brush volumeTextColor, Brush minDeltaTextColor, Brush maxDeltaTextColor)
		{
			return OrderFlowStats(Input, volumetricPeriod, volumetricBarsType, ticksPerLevel, textSize, showPriceDeltaBar, bullBarNegDeltaColor, bearBarPosDeltaColor, deltaBarOutlineColor, showPoc, pocColor, tableDisplayMode, maxTableColumns, tableBgColor, tableLabelColor, neutralThreshold, neutralTextColor, deltaThreshold, deltaChangePercentage, deltaTextColor, increasingPosDeltaTextColor, decreasingPosDeltaTextColor, decreasingNegDeltaTextColor, increasingNegDeltaTextColor, increasingMaxDeltaTextColor, decreasingMaxDeltaTextColor, decreasingMinDeltaTextColor, increasingMinDeltaTextColor, positiveTextColor, negativeTextColor, volumeTextColor, minDeltaTextColor, maxDeltaTextColor);
		}

		public OrderFlowStats OrderFlowStats(ISeries<double> input, int volumetricPeriod, BarTypeOptions volumetricBarsType, int ticksPerLevel, float textSize, bool showPriceDeltaBar, Brush bullBarNegDeltaColor, Brush bearBarPosDeltaColor, Brush deltaBarOutlineColor, bool showPoc, Brush pocColor, TableDisplayModeType tableDisplayMode, int maxTableColumns, Brush tableBgColor, Brush tableLabelColor, int neutralThreshold, Brush neutralTextColor, int deltaThreshold, double deltaChangePercentage, Brush deltaTextColor, Brush increasingPosDeltaTextColor, Brush decreasingPosDeltaTextColor, Brush decreasingNegDeltaTextColor, Brush increasingNegDeltaTextColor, Brush increasingMaxDeltaTextColor, Brush decreasingMaxDeltaTextColor, Brush decreasingMinDeltaTextColor, Brush increasingMinDeltaTextColor, Brush positiveTextColor, Brush negativeTextColor, Brush volumeTextColor, Brush minDeltaTextColor, Brush maxDeltaTextColor)
		{
			if (cacheOrderFlowStats != null)
				for (int idx = 0; idx < cacheOrderFlowStats.Length; idx++)
					if (cacheOrderFlowStats[idx] != null && cacheOrderFlowStats[idx].VolumetricPeriod == volumetricPeriod && cacheOrderFlowStats[idx].VolumetricBarsType == volumetricBarsType && cacheOrderFlowStats[idx].TicksPerLevel == ticksPerLevel && cacheOrderFlowStats[idx].TextSize == textSize && cacheOrderFlowStats[idx].ShowPriceDeltaBar == showPriceDeltaBar && cacheOrderFlowStats[idx].BullBarNegDeltaColor == bullBarNegDeltaColor && cacheOrderFlowStats[idx].BearBarPosDeltaColor == bearBarPosDeltaColor && cacheOrderFlowStats[idx].DeltaBarOutlineColor == deltaBarOutlineColor && cacheOrderFlowStats[idx].ShowPoc == showPoc && cacheOrderFlowStats[idx].PocColor == pocColor && cacheOrderFlowStats[idx].TableDisplayMode == tableDisplayMode && cacheOrderFlowStats[idx].MaxTableColumns == maxTableColumns && cacheOrderFlowStats[idx].TableBgColor == tableBgColor && cacheOrderFlowStats[idx].TableLabelColor == tableLabelColor && cacheOrderFlowStats[idx].NeutralThreshold == neutralThreshold && cacheOrderFlowStats[idx].NeutralTextColor == neutralTextColor && cacheOrderFlowStats[idx].DeltaThreshold == deltaThreshold && cacheOrderFlowStats[idx].DeltaChangePercentage == deltaChangePercentage && cacheOrderFlowStats[idx].DeltaTextColor == deltaTextColor && cacheOrderFlowStats[idx].IncreasingPosDeltaTextColor == increasingPosDeltaTextColor && cacheOrderFlowStats[idx].DecreasingPosDeltaTextColor == decreasingPosDeltaTextColor && cacheOrderFlowStats[idx].DecreasingNegDeltaTextColor == decreasingNegDeltaTextColor && cacheOrderFlowStats[idx].IncreasingNegDeltaTextColor == increasingNegDeltaTextColor && cacheOrderFlowStats[idx].IncreasingMaxDeltaTextColor == increasingMaxDeltaTextColor && cacheOrderFlowStats[idx].DecreasingMaxDeltaTextColor == decreasingMaxDeltaTextColor && cacheOrderFlowStats[idx].DecreasingMinDeltaTextColor == decreasingMinDeltaTextColor && cacheOrderFlowStats[idx].IncreasingMinDeltaTextColor == increasingMinDeltaTextColor && cacheOrderFlowStats[idx].PositiveTextColor == positiveTextColor && cacheOrderFlowStats[idx].NegativeTextColor == negativeTextColor && cacheOrderFlowStats[idx].VolumeTextColor == volumeTextColor && cacheOrderFlowStats[idx].MinDeltaTextColor == minDeltaTextColor && cacheOrderFlowStats[idx].MaxDeltaTextColor == maxDeltaTextColor && cacheOrderFlowStats[idx].EqualsInput(input))
						return cacheOrderFlowStats[idx];
			return CacheIndicator<OrderFlowStats>(new OrderFlowStats(){ VolumetricPeriod = volumetricPeriod, VolumetricBarsType = volumetricBarsType, TicksPerLevel = ticksPerLevel, TextSize = textSize, ShowPriceDeltaBar = showPriceDeltaBar, BullBarNegDeltaColor = bullBarNegDeltaColor, BearBarPosDeltaColor = bearBarPosDeltaColor, DeltaBarOutlineColor = deltaBarOutlineColor, ShowPoc = showPoc, PocColor = pocColor, TableDisplayMode = tableDisplayMode, MaxTableColumns = maxTableColumns, TableBgColor = tableBgColor, TableLabelColor = tableLabelColor, NeutralThreshold = neutralThreshold, NeutralTextColor = neutralTextColor, DeltaThreshold = deltaThreshold, DeltaChangePercentage = deltaChangePercentage, DeltaTextColor = deltaTextColor, IncreasingPosDeltaTextColor = increasingPosDeltaTextColor, DecreasingPosDeltaTextColor = decreasingPosDeltaTextColor, DecreasingNegDeltaTextColor = decreasingNegDeltaTextColor, IncreasingNegDeltaTextColor = increasingNegDeltaTextColor, IncreasingMaxDeltaTextColor = increasingMaxDeltaTextColor, DecreasingMaxDeltaTextColor = decreasingMaxDeltaTextColor, DecreasingMinDeltaTextColor = decreasingMinDeltaTextColor, IncreasingMinDeltaTextColor = increasingMinDeltaTextColor, PositiveTextColor = positiveTextColor, NegativeTextColor = negativeTextColor, VolumeTextColor = volumeTextColor, MinDeltaTextColor = minDeltaTextColor, MaxDeltaTextColor = maxDeltaTextColor }, input, ref cacheOrderFlowStats);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.OrderFlowStats OrderFlowStats(int volumetricPeriod, BarTypeOptions volumetricBarsType, int ticksPerLevel, float textSize, bool showPriceDeltaBar, Brush bullBarNegDeltaColor, Brush bearBarPosDeltaColor, Brush deltaBarOutlineColor, bool showPoc, Brush pocColor, TableDisplayModeType tableDisplayMode, int maxTableColumns, Brush tableBgColor, Brush tableLabelColor, int neutralThreshold, Brush neutralTextColor, int deltaThreshold, double deltaChangePercentage, Brush deltaTextColor, Brush increasingPosDeltaTextColor, Brush decreasingPosDeltaTextColor, Brush decreasingNegDeltaTextColor, Brush increasingNegDeltaTextColor, Brush increasingMaxDeltaTextColor, Brush decreasingMaxDeltaTextColor, Brush decreasingMinDeltaTextColor, Brush increasingMinDeltaTextColor, Brush positiveTextColor, Brush negativeTextColor, Brush volumeTextColor, Brush minDeltaTextColor, Brush maxDeltaTextColor)
		{
			return indicator.OrderFlowStats(Input, volumetricPeriod, volumetricBarsType, ticksPerLevel, textSize, showPriceDeltaBar, bullBarNegDeltaColor, bearBarPosDeltaColor, deltaBarOutlineColor, showPoc, pocColor, tableDisplayMode, maxTableColumns, tableBgColor, tableLabelColor, neutralThreshold, neutralTextColor, deltaThreshold, deltaChangePercentage, deltaTextColor, increasingPosDeltaTextColor, decreasingPosDeltaTextColor, decreasingNegDeltaTextColor, increasingNegDeltaTextColor, increasingMaxDeltaTextColor, decreasingMaxDeltaTextColor, decreasingMinDeltaTextColor, increasingMinDeltaTextColor, positiveTextColor, negativeTextColor, volumeTextColor, minDeltaTextColor, maxDeltaTextColor);
		}

		public Indicators.OrderFlowStats OrderFlowStats(ISeries<double> input , int volumetricPeriod, BarTypeOptions volumetricBarsType, int ticksPerLevel, float textSize, bool showPriceDeltaBar, Brush bullBarNegDeltaColor, Brush bearBarPosDeltaColor, Brush deltaBarOutlineColor, bool showPoc, Brush pocColor, TableDisplayModeType tableDisplayMode, int maxTableColumns, Brush tableBgColor, Brush tableLabelColor, int neutralThreshold, Brush neutralTextColor, int deltaThreshold, double deltaChangePercentage, Brush deltaTextColor, Brush increasingPosDeltaTextColor, Brush decreasingPosDeltaTextColor, Brush decreasingNegDeltaTextColor, Brush increasingNegDeltaTextColor, Brush increasingMaxDeltaTextColor, Brush decreasingMaxDeltaTextColor, Brush decreasingMinDeltaTextColor, Brush increasingMinDeltaTextColor, Brush positiveTextColor, Brush negativeTextColor, Brush volumeTextColor, Brush minDeltaTextColor, Brush maxDeltaTextColor)
		{
			return indicator.OrderFlowStats(input, volumetricPeriod, volumetricBarsType, ticksPerLevel, textSize, showPriceDeltaBar, bullBarNegDeltaColor, bearBarPosDeltaColor, deltaBarOutlineColor, showPoc, pocColor, tableDisplayMode, maxTableColumns, tableBgColor, tableLabelColor, neutralThreshold, neutralTextColor, deltaThreshold, deltaChangePercentage, deltaTextColor, increasingPosDeltaTextColor, decreasingPosDeltaTextColor, decreasingNegDeltaTextColor, increasingNegDeltaTextColor, increasingMaxDeltaTextColor, decreasingMaxDeltaTextColor, decreasingMinDeltaTextColor, increasingMinDeltaTextColor, positiveTextColor, negativeTextColor, volumeTextColor, minDeltaTextColor, maxDeltaTextColor);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.OrderFlowStats OrderFlowStats(int volumetricPeriod, BarTypeOptions volumetricBarsType, int ticksPerLevel, float textSize, bool showPriceDeltaBar, Brush bullBarNegDeltaColor, Brush bearBarPosDeltaColor, Brush deltaBarOutlineColor, bool showPoc, Brush pocColor, TableDisplayModeType tableDisplayMode, int maxTableColumns, Brush tableBgColor, Brush tableLabelColor, int neutralThreshold, Brush neutralTextColor, int deltaThreshold, double deltaChangePercentage, Brush deltaTextColor, Brush increasingPosDeltaTextColor, Brush decreasingPosDeltaTextColor, Brush decreasingNegDeltaTextColor, Brush increasingNegDeltaTextColor, Brush increasingMaxDeltaTextColor, Brush decreasingMaxDeltaTextColor, Brush decreasingMinDeltaTextColor, Brush increasingMinDeltaTextColor, Brush positiveTextColor, Brush negativeTextColor, Brush volumeTextColor, Brush minDeltaTextColor, Brush maxDeltaTextColor)
		{
			return indicator.OrderFlowStats(Input, volumetricPeriod, volumetricBarsType, ticksPerLevel, textSize, showPriceDeltaBar, bullBarNegDeltaColor, bearBarPosDeltaColor, deltaBarOutlineColor, showPoc, pocColor, tableDisplayMode, maxTableColumns, tableBgColor, tableLabelColor, neutralThreshold, neutralTextColor, deltaThreshold, deltaChangePercentage, deltaTextColor, increasingPosDeltaTextColor, decreasingPosDeltaTextColor, decreasingNegDeltaTextColor, increasingNegDeltaTextColor, increasingMaxDeltaTextColor, decreasingMaxDeltaTextColor, decreasingMinDeltaTextColor, increasingMinDeltaTextColor, positiveTextColor, negativeTextColor, volumeTextColor, minDeltaTextColor, maxDeltaTextColor);
		}

		public Indicators.OrderFlowStats OrderFlowStats(ISeries<double> input , int volumetricPeriod, BarTypeOptions volumetricBarsType, int ticksPerLevel, float textSize, bool showPriceDeltaBar, Brush bullBarNegDeltaColor, Brush bearBarPosDeltaColor, Brush deltaBarOutlineColor, bool showPoc, Brush pocColor, TableDisplayModeType tableDisplayMode, int maxTableColumns, Brush tableBgColor, Brush tableLabelColor, int neutralThreshold, Brush neutralTextColor, int deltaThreshold, double deltaChangePercentage, Brush deltaTextColor, Brush increasingPosDeltaTextColor, Brush decreasingPosDeltaTextColor, Brush decreasingNegDeltaTextColor, Brush increasingNegDeltaTextColor, Brush increasingMaxDeltaTextColor, Brush decreasingMaxDeltaTextColor, Brush decreasingMinDeltaTextColor, Brush increasingMinDeltaTextColor, Brush positiveTextColor, Brush negativeTextColor, Brush volumeTextColor, Brush minDeltaTextColor, Brush maxDeltaTextColor)
		{
			return indicator.OrderFlowStats(input, volumetricPeriod, volumetricBarsType, ticksPerLevel, textSize, showPriceDeltaBar, bullBarNegDeltaColor, bearBarPosDeltaColor, deltaBarOutlineColor, showPoc, pocColor, tableDisplayMode, maxTableColumns, tableBgColor, tableLabelColor, neutralThreshold, neutralTextColor, deltaThreshold, deltaChangePercentage, deltaTextColor, increasingPosDeltaTextColor, decreasingPosDeltaTextColor, decreasingNegDeltaTextColor, increasingNegDeltaTextColor, increasingMaxDeltaTextColor, decreasingMaxDeltaTextColor, decreasingMinDeltaTextColor, increasingMinDeltaTextColor, positiveTextColor, negativeTextColor, volumeTextColor, minDeltaTextColor, maxDeltaTextColor);
		}
	}
}

#endregion
