#region Using declarations
using NinjaTrader.Data;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.NinjaScript.BarsTypes;
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.NinjaScript.Indicators;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using System.Xml.Serialization;
#endregion

namespace NinjaTrader.NinjaScript.Indicators
{
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

        private VolumetricBarsType volumetricBars;
        private int volumetricBarsIndex = -1;
        private SharpDX.Direct2D1.RenderTarget lastRenderTarget;
        private SharpDX.DirectWrite.TextFormat textFormat;

        private Brush _bullBarNegDeltaBrush;
        private Brush _bearBarPosDeltaColorBrush;
        private Brush _deltaBarOutlineColorBrush;
        private SharpDX.Direct2D1.SolidColorBrush _tableBgBrush;
        private SharpDX.Direct2D1.SolidColorBrush _tableLabelBrush;
        private SharpDX.Direct2D1.SolidColorBrush _positiveTextBrush;
        private SharpDX.Direct2D1.SolidColorBrush _negativeTextBrush;
        private SharpDX.Direct2D1.SolidColorBrush _volumeTextBrush;
        private SharpDX.Direct2D1.SolidColorBrush _minDeltaTextBrush;
        private SharpDX.Direct2D1.SolidColorBrush _maxDeltaTextBrush;

        // Fields to track previous property values
        private float previousTextSize;
        private Brush previousBullBarNegDeltaColor;
        private Brush previousBearBarPosDeltaColor;
        private Brush previousDeltaBarOutlineColor;
        private Brush previousTableBgColor;
        private Brush previousTableLabelColor;
        private Brush previousPositiveTextColor;
        private Brush previousNegativeTextColor;
        private Brush previousVolumeTextColor;
        private Brush previousMinDeltaTextColor;
        private Brush previousMaxDeltaTextColor;

        #region Properties

        #region General Properties

        [NinjaScriptProperty]
        [Display(Name = "Version", Description = "Order Flow Stats Version", Order = 0, GroupName = GROUP_NAME_GENERAL)]
        [ReadOnly(true)]
        public string Version => "1.0.0";

        #endregion

        #region  Order Flow Stats Properties

        [Range(1, int.MaxValue), NinjaScriptProperty]
        [Display(Name = "Volumetric Period", Description = "Volumetric data series period", GroupName = GROUP_NAME_ORDER_FLOW_STATS, Order = 0)]
        public int VolumetricPeriod { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Volumetric Bars Type", Description = "The type of bars for the volumetric data series", GroupName = GROUP_NAME_ORDER_FLOW_STATS, Order = 1)]
        public BarsPeriodType VolumetricBarsType { get; set; }

        [Range(1, int.MaxValue), NinjaScriptProperty]
        [Display(Name = "Ticks Per Level", Description = "The ticks per level", GroupName = GROUP_NAME_ORDER_FLOW_STATS, Order = 2)]
        public int TicksPerLevel { get; set; }

        [Range(6, 14), NinjaScriptProperty]
        [Display(Name = "Text Size", Description = "Font size for the table text (6 to 14)", GroupName = GROUP_NAME_ORDER_FLOW_STATS, Order = 3)]
        public float TextSize { get; set; }

        #endregion

        #region Data Bar Properties

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Bull Bar Neg Delta Color", Description = "The color for a bullish bar and negative delta.", GroupName = GROUP_NAME_DATA_BAR, Order = 0)]
        public Brush BullBarNegDeltaColor { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Bear Bar Pos Delta Color", Description = "The color for a bearish bar and positive delta.", GroupName = GROUP_NAME_DATA_BAR, Order = 1)]
        public Brush BearBarPosDeltaColor { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Bull Bear Bar Outline Color", Description = "The bar outline color for the bull bear delta bar.", GroupName = GROUP_NAME_DATA_BAR, Order = 2)]
        public Brush DeltaBarOutlineColor { get; set; }

        #endregion

        #region Table Properties

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Table Display Mode", Description = "How to display the table", GroupName = GROUP_NAME_TABLE, Order = 0)]
        public TableDisplayModeType TableDisplayMode { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
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

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Positive Text Color", Description = "The positive text color.", GroupName = GROUP_NAME_TABLE, Order = 4)]
        public Brush PositiveTextColor { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Negative Text Color", Description = "The negative text color.", GroupName = GROUP_NAME_TABLE, Order = 5)]
        public Brush NegativeTextColor { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Volume Text Color", Description = "The volume text color.", GroupName = GROUP_NAME_TABLE, Order = 6)]
        public Brush VolumeTextColor { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Min Delta Text Color", Description = "The min delta text color.", GroupName = GROUP_NAME_TABLE, Order = 7)]
        public Brush MinDeltaTextColor { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Max Delta Text Color", Description = "The max delta text color.", GroupName = GROUP_NAME_TABLE, Order = 8)]
        public Brush MaxDeltaTextColor { get; set; }

        #endregion

        #region Serialization

        [Browsable(false)] public string BullBarNegDeltaColorSerialize { get => Serialize.BrushToString(BullBarNegDeltaColor); set => BullBarNegDeltaColor = Serialize.StringToBrush(value); }
        [Browsable(false)] public string BearBarPosDeltaColorSerialize { get => Serialize.BrushToString(BearBarPosDeltaColor); set => BearBarPosDeltaColor = Serialize.StringToBrush(value); }
        [Browsable(false)] public string DeltaBarOutlineColorSerialize { get => Serialize.BrushToString(DeltaBarOutlineColor); set => DeltaBarOutlineColor = Serialize.StringToBrush(value); }
        [Browsable(false)] public string TableBgColorSerialize { get => Serialize.BrushToString(TableBgColor); set => TableBgColor = Serialize.StringToBrush(value); }
        [Browsable(false)] public string TableLabelColorSerialize { get => Serialize.BrushToString(TableLabelColor); set => TableLabelColor = Serialize.StringToBrush(value); }
        [Browsable(false)] public string PositiveTextColorSerialize { get => Serialize.BrushToString(PositiveTextColor); set => PositiveTextColor = Serialize.StringToBrush(value); }
        [Browsable(false)] public string NegativeTextColorSerialize { get => Serialize.BrushToString(NegativeTextColor); set => NegativeTextColor = Serialize.StringToBrush(value); }
        [Browsable(false)] public string VolumeTextColorSerialize { get => Serialize.BrushToString(VolumeTextColor); set => VolumeTextColor = Serialize.StringToBrush(value); }
        [Browsable(false)] public string MinDeltaTextColorSerialize { get => Serialize.BrushToString(MinDeltaTextColor); set => MinDeltaTextColor = Serialize.StringToBrush(value); }
        [Browsable(false)] public string MaxDeltaTextColorSerialize { get => Serialize.BrushToString(MaxDeltaTextColor); set => MaxDeltaTextColor = Serialize.StringToBrush(value); }

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
                VolumetricBarsType = BarsPeriodType.Minute;
                TicksPerLevel = 5;
                TextSize = 11;
                TableDisplayMode = TableDisplayModeType.PerBar;
                MaxTableColumns = 30;

                BullBarNegDeltaColor = Brushes.DarkTurquoise;
                BearBarPosDeltaColor = Brushes.DarkOrange;
                DeltaBarOutlineColor = Brushes.SlateGray;
                TableBgColor = Brushes.Black;
                TableLabelColor = Brushes.LightGray;
                PositiveTextColor = Brushes.DarkGreen;
                NegativeTextColor = Brushes.DarkRed;
                VolumeTextColor = Brushes.LightGray;
                MinDeltaTextColor = Brushes.DarkOrange;
                MaxDeltaTextColor = Brushes.DarkTurquoise;

                previousTextSize = TextSize;
                previousBullBarNegDeltaColor = BullBarNegDeltaColor;
                previousBearBarPosDeltaColor = BearBarPosDeltaColor;
                previousDeltaBarOutlineColor = DeltaBarOutlineColor;
                previousTableBgColor = TableBgColor;
                previousTableLabelColor = TableLabelColor;
                previousPositiveTextColor = PositiveTextColor;
                previousNegativeTextColor = NegativeTextColor;
                previousVolumeTextColor = VolumeTextColor;
                previousMinDeltaTextColor = MinDeltaTextColor;
                previousMaxDeltaTextColor = MaxDeltaTextColor;
            }
            else if (State == State.Configure)
            {
                AddVolumetric(Instrument.FullName, VolumetricBarsType, VolumetricPeriod, VolumetricDeltaType.BidAsk, TicksPerLevel);
            }
            else if (State == State.DataLoaded)
            {
                volumetricBarsIndex = 1;
                volumetricBars = BarsArray[volumetricBarsIndex].BarsType as VolumetricBarsType;
                textFormat = new SharpDX.DirectWrite.TextFormat(Core.Globals.DirectWriteFactory, "Arial", TextSize);

                _bullBarNegDeltaBrush = BullBarNegDeltaColor;
                _bearBarPosDeltaColorBrush = BearBarPosDeltaColor;
                _deltaBarOutlineColorBrush = DeltaBarOutlineColor;
            }
            else if (State == State.Finalized)
            {
                if (textFormat != null) { textFormat.Dispose(); textFormat = null; }
                DisposeBrushes();
            }
        }

        protected override void OnBarUpdate()
        {
            if (BarsInProgress != 0 || volumetricBars == null || CurrentBar < 1)
                return;

            // Update bar brushes if colors have changed
            if (!ReferenceEquals(BullBarNegDeltaColor, previousBullBarNegDeltaColor))
            {
                _bullBarNegDeltaBrush = BullBarNegDeltaColor;
                previousBullBarNegDeltaColor = BullBarNegDeltaColor;
            }
            if (!ReferenceEquals(BearBarPosDeltaColor, previousBearBarPosDeltaColor))
            {
                _bearBarPosDeltaColorBrush = BearBarPosDeltaColor;
                previousBearBarPosDeltaColor = BearBarPosDeltaColor;
            }
            if (!ReferenceEquals(DeltaBarOutlineColor, previousDeltaBarOutlineColor))
            {
                _deltaBarOutlineColorBrush = DeltaBarOutlineColor;
                previousDeltaBarOutlineColor = DeltaBarOutlineColor;
            }

            bool isPositiveBar = Close[0] > Open[0];
            bool isDeltaNegative = volumetricBars.Volumes[CurrentBar].BarDelta < 0;

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

            double pocPrice = 0;
            volumetricBars.Volumes[CurrentBar].GetMaximumVolume(null, out pocPrice);

            TimeSpan halfDuration = Bars.IsFirstBarOfSession
                ? TimeSpan.FromMinutes(VolumetricPeriod / 2.0)
                : TimeSpan.FromTicks((Time[0] - Time[1]).Ticks / 2);
            double scaleFactor = 0.6;
            halfDuration = TimeSpan.FromTicks((long)(halfDuration.Ticks * scaleFactor));

            DateTime startTime = Time[0] - halfDuration;
            DateTime endTime = Time[0] + halfDuration;

            var line = Draw.Line(this, "Line" + CurrentBar, false, startTime, pocPrice, endTime, pocPrice, Brushes.Yellow, DashStyleHelper.Solid, 2);
            if (line != null)
            {
                line.ZOrder = -1;
            }
        }

        protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
        {
            base.OnRender(chartControl, chartScale);

            if (volumetricBars == null || Bars == null || TableDisplayMode == TableDisplayModeType.None || RenderTarget == null)
                return;

            // Check for changes in TextSize
            if (TextSize != previousTextSize)
            {
                if (textFormat != null) textFormat.Dispose();
                textFormat = new SharpDX.DirectWrite.TextFormat(Core.Globals.DirectWriteFactory, "Arial", TextSize);
                previousTextSize = TextSize;
            }

            // Check for changes in table-related colors or RenderTarget
            bool brushesNeedUpdate = lastRenderTarget != RenderTarget ||
                !ReferenceEquals(TableBgColor, previousTableBgColor) ||
                !ReferenceEquals(TableLabelColor, previousTableLabelColor) ||
                !ReferenceEquals(PositiveTextColor, previousPositiveTextColor) ||
                !ReferenceEquals(NegativeTextColor, previousNegativeTextColor) ||
                !ReferenceEquals(VolumeTextColor, previousVolumeTextColor) ||
                !ReferenceEquals(MinDeltaTextColor, previousMinDeltaTextColor) ||
                !ReferenceEquals(MaxDeltaTextColor, previousMaxDeltaTextColor);

            if (brushesNeedUpdate)
            {
                DisposeBrushes();
                _tableBgBrush = new SharpDX.Direct2D1.SolidColorBrush(RenderTarget, ToDxColor(TableBgColor));
                _tableLabelBrush = new SharpDX.Direct2D1.SolidColorBrush(RenderTarget, ToDxColor(TableLabelColor));
                _positiveTextBrush = new SharpDX.Direct2D1.SolidColorBrush(RenderTarget, ToDxColor(PositiveTextColor));
                _negativeTextBrush = new SharpDX.Direct2D1.SolidColorBrush(RenderTarget, ToDxColor(NegativeTextColor));
                _volumeTextBrush = new SharpDX.Direct2D1.SolidColorBrush(RenderTarget, ToDxColor(VolumeTextColor));
                _minDeltaTextBrush = new SharpDX.Direct2D1.SolidColorBrush(RenderTarget, ToDxColor(MinDeltaTextColor));
                _maxDeltaTextBrush = new SharpDX.Direct2D1.SolidColorBrush(RenderTarget, ToDxColor(MaxDeltaTextColor));

                // Update previous values
                previousTableBgColor = TableBgColor;
                previousTableLabelColor = TableLabelColor;
                previousPositiveTextColor = PositiveTextColor;
                previousNegativeTextColor = NegativeTextColor;
                previousVolumeTextColor = VolumeTextColor;
                previousMinDeltaTextColor = MinDeltaTextColor;
                previousMaxDeltaTextColor = MaxDeltaTextColor;
                lastRenderTarget = RenderTarget;
            }

            // Table layout parameters
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
                float maxTextWidth = CalculateShowLastWidth(chartControl);
                tableWidth = labelColumnWidth + maxTextWidth + 10f;
            }
            else
            {
                tableWidth = Math.Max(labelColumnWidth + barWidth, chartControl.GetXByBarIndex(ChartBars, ChartBars.ToIndex) + barWidth - labelX);
            }

            RenderTableBackgroundAndLabels(chartControl, tableTop, tableWidth, labelX, dataStartX, rowHeight, numRows, tablePadding);

            if (TableDisplayMode == TableDisplayModeType.ShowLast)
                RenderTableShowLast(chartControl, dataStartX, tableTop, rowHeight);
            else if (TableDisplayMode == TableDisplayModeType.PerBar)
                RenderTablePerBar(chartControl, dataStartX, tableTop, rowHeight, maxVisibleBars);
        }

        private void RenderTableBackgroundAndLabels(ChartControl chartControl, float tableTop, float tableWidth, float labelX, float dataStartX, float rowHeight, int numRows, float tablePadding)
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
                RenderTarget.DrawText(labels[i], textFormat, labelRect, _tableLabelBrush);
            }
        }

        private void RenderTableShowLast(ChartControl chartControl, float dataStartX, float tableTop, float rowHeight)
        {
            int idx = ChartBars.ToIndex;
            if (idx >= 0 && idx < Bars.Count)
            {
                float x = dataStartX;

                double volume = volumetricBars.Volumes[idx].TotalVolume;
                double cumulativeDelta = volumetricBars.Volumes[idx].CumulativeDelta;
                double delta = volumetricBars.Volumes[idx].BarDelta;
                double minDelta = volumetricBars.Volumes[idx].MinSeenDelta;
                double maxDelta = volumetricBars.Volumes[idx].MaxSeenDelta;

                string[] texts = new[]
                {
                volume.ToString("N0"),
                cumulativeDelta.ToString("N0"),
                delta.ToString("N0"),
                minDelta.ToString("N0"),
                maxDelta.ToString("N0")
            };
                SharpDX.Direct2D1.SolidColorBrush[] brushes = new[]
                {
                _volumeTextBrush,
                cumulativeDelta >= 0 ? _positiveTextBrush : _negativeTextBrush,
                delta >= 0 ? _positiveTextBrush : _negativeTextBrush,
                _minDeltaTextBrush,
                _maxDeltaTextBrush
            };

                for (int i = 0; i < texts.Length; i++)
                {
                    SharpDX.RectangleF rect = new SharpDX.RectangleF(x, tableTop + i * rowHeight, 1000f, rowHeight); // Large width to ensure text fits
                    RenderTarget.DrawText(texts[i], textFormat, rect, brushes[i]);
                }
            }
        }

        private float CalculateShowLastWidth(ChartControl chartControl)
        {
            float maxTextWidth = 0f;
            int idx = ChartBars.ToIndex;
            if (idx >= 0 && idx < Bars.Count)
            {
                double volume = volumetricBars.Volumes[idx].TotalVolume;
                double cumulativeDelta = volumetricBars.Volumes[idx].CumulativeDelta;
                double delta = volumetricBars.Volumes[idx].BarDelta;
                double minDelta = volumetricBars.Volumes[idx].MinSeenDelta;
                double maxDelta = volumetricBars.Volumes[idx].MaxSeenDelta;

                string[] texts = new[]
                {
                    volume.ToString("N0"),
                    cumulativeDelta.ToString("N0"),
                    delta.ToString("N0"),
                    minDelta.ToString("N0"),
                    maxDelta.ToString("N0")
                };

                // Measure the width of each text and find the maximum
                for (int i = 0; i < texts.Length; i++)
                {
                    SharpDX.DirectWrite.TextLayout textLayout = new SharpDX.DirectWrite.TextLayout(
                        Core.Globals.DirectWriteFactory, texts[i], textFormat, 1000f, 16f);
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
            double volume = volumetricBars.Volumes[idx].TotalVolume;
            double cumulativeDelta = volumetricBars.Volumes[idx].CumulativeDelta;
            double delta = volumetricBars.Volumes[idx].BarDelta;
            double minDelta = volumetricBars.Volumes[idx].MinSeenDelta;
            double maxDelta = volumetricBars.Volumes[idx].MaxSeenDelta;

            string[] texts = new[]
            {
                volume.ToString("N0"),
                cumulativeDelta.ToString("N0"),
                delta.ToString("N0"),
                minDelta.ToString("N0"),
                maxDelta.ToString("N0")
            };
            SharpDX.Direct2D1.SolidColorBrush[] brushes = new[]
            {
                _volumeTextBrush,
                cumulativeDelta >= 0 ? _positiveTextBrush : _negativeTextBrush,
                delta >= 0 ? _positiveTextBrush : _negativeTextBrush,
                _minDeltaTextBrush,
                _maxDeltaTextBrush
            };

            for (int i = 0; i < texts.Length; i++)
            {
                SharpDX.RectangleF rect = new SharpDX.RectangleF(x, tableTop + i * rowHeight, width, rowHeight);
                RenderTarget.DrawText(texts[i], textFormat, rect, brushes[i]);
            }
        }

        private void DisposeBrushes()
        {
            _tableBgBrush?.Dispose(); _tableBgBrush = null;
            _tableLabelBrush?.Dispose(); _tableLabelBrush = null;
            _positiveTextBrush?.Dispose(); _positiveTextBrush = null;
            _negativeTextBrush?.Dispose(); _negativeTextBrush = null;
            _volumeTextBrush?.Dispose(); _volumeTextBrush = null;
            _minDeltaTextBrush?.Dispose(); _minDeltaTextBrush = null;
            _maxDeltaTextBrush?.Dispose(); _maxDeltaTextBrush = null;
        }
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private OrderFlowStats[] cacheOrderFlowStats;
		public OrderFlowStats OrderFlowStats(int volumetricPeriod, BarsPeriodType volumetricBarsType, int ticksPerLevel, float textSize, Brush bullBarNegDeltaColor, Brush bearBarPosDeltaColor, Brush deltaBarOutlineColor, TableDisplayModeType tableDisplayMode, int maxTableColumns, Brush tableBgColor, Brush tableLabelColor, Brush positiveTextColor, Brush negativeTextColor, Brush volumeTextColor, Brush minDeltaTextColor, Brush maxDeltaTextColor)
		{
			return OrderFlowStats(Input, volumetricPeriod, volumetricBarsType, ticksPerLevel, textSize, bullBarNegDeltaColor, bearBarPosDeltaColor, deltaBarOutlineColor, tableDisplayMode, maxTableColumns, tableBgColor, tableLabelColor, positiveTextColor, negativeTextColor, volumeTextColor, minDeltaTextColor, maxDeltaTextColor);
		}

		public OrderFlowStats OrderFlowStats(ISeries<double> input, int volumetricPeriod, BarsPeriodType volumetricBarsType, int ticksPerLevel, float textSize, Brush bullBarNegDeltaColor, Brush bearBarPosDeltaColor, Brush deltaBarOutlineColor, TableDisplayModeType tableDisplayMode, int maxTableColumns, Brush tableBgColor, Brush tableLabelColor, Brush positiveTextColor, Brush negativeTextColor, Brush volumeTextColor, Brush minDeltaTextColor, Brush maxDeltaTextColor)
		{
			if (cacheOrderFlowStats != null)
				for (int idx = 0; idx < cacheOrderFlowStats.Length; idx++)
					if (cacheOrderFlowStats[idx] != null && cacheOrderFlowStats[idx].VolumetricPeriod == volumetricPeriod && cacheOrderFlowStats[idx].VolumetricBarsType == volumetricBarsType && cacheOrderFlowStats[idx].TicksPerLevel == ticksPerLevel && cacheOrderFlowStats[idx].TextSize == textSize && cacheOrderFlowStats[idx].BullBarNegDeltaColor == bullBarNegDeltaColor && cacheOrderFlowStats[idx].BearBarPosDeltaColor == bearBarPosDeltaColor && cacheOrderFlowStats[idx].DeltaBarOutlineColor == deltaBarOutlineColor && cacheOrderFlowStats[idx].TableDisplayMode == tableDisplayMode && cacheOrderFlowStats[idx].MaxTableColumns == maxTableColumns && cacheOrderFlowStats[idx].TableBgColor == tableBgColor && cacheOrderFlowStats[idx].TableLabelColor == tableLabelColor && cacheOrderFlowStats[idx].PositiveTextColor == positiveTextColor && cacheOrderFlowStats[idx].NegativeTextColor == negativeTextColor && cacheOrderFlowStats[idx].VolumeTextColor == volumeTextColor && cacheOrderFlowStats[idx].MinDeltaTextColor == minDeltaTextColor && cacheOrderFlowStats[idx].MaxDeltaTextColor == maxDeltaTextColor && cacheOrderFlowStats[idx].EqualsInput(input))
						return cacheOrderFlowStats[idx];
			return CacheIndicator<OrderFlowStats>(new OrderFlowStats(){ VolumetricPeriod = volumetricPeriod, VolumetricBarsType = volumetricBarsType, TicksPerLevel = ticksPerLevel, TextSize = textSize, BullBarNegDeltaColor = bullBarNegDeltaColor, BearBarPosDeltaColor = bearBarPosDeltaColor, DeltaBarOutlineColor = deltaBarOutlineColor, TableDisplayMode = tableDisplayMode, MaxTableColumns = maxTableColumns, TableBgColor = tableBgColor, TableLabelColor = tableLabelColor, PositiveTextColor = positiveTextColor, NegativeTextColor = negativeTextColor, VolumeTextColor = volumeTextColor, MinDeltaTextColor = minDeltaTextColor, MaxDeltaTextColor = maxDeltaTextColor }, input, ref cacheOrderFlowStats);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.OrderFlowStats OrderFlowStats(int volumetricPeriod, BarsPeriodType volumetricBarsType, int ticksPerLevel, float textSize, Brush bullBarNegDeltaColor, Brush bearBarPosDeltaColor, Brush deltaBarOutlineColor, TableDisplayModeType tableDisplayMode, int maxTableColumns, Brush tableBgColor, Brush tableLabelColor, Brush positiveTextColor, Brush negativeTextColor, Brush volumeTextColor, Brush minDeltaTextColor, Brush maxDeltaTextColor)
		{
			return indicator.OrderFlowStats(Input, volumetricPeriod, volumetricBarsType, ticksPerLevel, textSize, bullBarNegDeltaColor, bearBarPosDeltaColor, deltaBarOutlineColor, tableDisplayMode, maxTableColumns, tableBgColor, tableLabelColor, positiveTextColor, negativeTextColor, volumeTextColor, minDeltaTextColor, maxDeltaTextColor);
		}

		public Indicators.OrderFlowStats OrderFlowStats(ISeries<double> input , int volumetricPeriod, BarsPeriodType volumetricBarsType, int ticksPerLevel, float textSize, Brush bullBarNegDeltaColor, Brush bearBarPosDeltaColor, Brush deltaBarOutlineColor, TableDisplayModeType tableDisplayMode, int maxTableColumns, Brush tableBgColor, Brush tableLabelColor, Brush positiveTextColor, Brush negativeTextColor, Brush volumeTextColor, Brush minDeltaTextColor, Brush maxDeltaTextColor)
		{
			return indicator.OrderFlowStats(input, volumetricPeriod, volumetricBarsType, ticksPerLevel, textSize, bullBarNegDeltaColor, bearBarPosDeltaColor, deltaBarOutlineColor, tableDisplayMode, maxTableColumns, tableBgColor, tableLabelColor, positiveTextColor, negativeTextColor, volumeTextColor, minDeltaTextColor, maxDeltaTextColor);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.OrderFlowStats OrderFlowStats(int volumetricPeriod, BarsPeriodType volumetricBarsType, int ticksPerLevel, float textSize, Brush bullBarNegDeltaColor, Brush bearBarPosDeltaColor, Brush deltaBarOutlineColor, TableDisplayModeType tableDisplayMode, int maxTableColumns, Brush tableBgColor, Brush tableLabelColor, Brush positiveTextColor, Brush negativeTextColor, Brush volumeTextColor, Brush minDeltaTextColor, Brush maxDeltaTextColor)
		{
			return indicator.OrderFlowStats(Input, volumetricPeriod, volumetricBarsType, ticksPerLevel, textSize, bullBarNegDeltaColor, bearBarPosDeltaColor, deltaBarOutlineColor, tableDisplayMode, maxTableColumns, tableBgColor, tableLabelColor, positiveTextColor, negativeTextColor, volumeTextColor, minDeltaTextColor, maxDeltaTextColor);
		}

		public Indicators.OrderFlowStats OrderFlowStats(ISeries<double> input , int volumetricPeriod, BarsPeriodType volumetricBarsType, int ticksPerLevel, float textSize, Brush bullBarNegDeltaColor, Brush bearBarPosDeltaColor, Brush deltaBarOutlineColor, TableDisplayModeType tableDisplayMode, int maxTableColumns, Brush tableBgColor, Brush tableLabelColor, Brush positiveTextColor, Brush negativeTextColor, Brush volumeTextColor, Brush minDeltaTextColor, Brush maxDeltaTextColor)
		{
			return indicator.OrderFlowStats(input, volumetricPeriod, volumetricBarsType, ticksPerLevel, textSize, bullBarNegDeltaColor, bearBarPosDeltaColor, deltaBarOutlineColor, tableDisplayMode, maxTableColumns, tableBgColor, tableLabelColor, positiveTextColor, negativeTextColor, volumeTextColor, minDeltaTextColor, maxDeltaTextColor);
		}
	}
}

#endregion
