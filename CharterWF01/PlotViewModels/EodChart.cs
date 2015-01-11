// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainViewModel.cs" company="Wynand">
//   
//   Permission is hereby granted, free of charge, to any person obtaining annotation
//   copy of this software and associated documentation files (the
//   "Software"), to deal in the Software without restriction, including
//   without limitation the rights to use, copy, modify, merge, publish,
//   distribute, sublicense, and/or sell copies of the Software, and to
//   permit persons to whom the Software is furnished to do so, subject to
//   the following conditions:
//   
//   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
//   OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//   MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
//   IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
//   CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
//   TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
//   SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
// <summary>
//   ...
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace CharterWF.ViewModels
{
   using System;
   using System.Collections.Generic;
   using System.Collections.ObjectModel;
   using System.ComponentModel;
   using System.Globalization;
   using System.Linq;
   using System.Windows.Documents;
   using CharterWF;
   using MySqlAccess;
   using OxyPlot;
   using OxyPlot.Axes;
   using OxyPlot.Series;
   using OxyPlot.WindowsForms;

   /// <summary>
   /// TODO: This class provides
   /// </summary>
   public class EodChart : INotifyPropertyChanged
   {
      public event PropertyChangedEventHandler PropertyChanged;
      public PlotView PlotView;
      public CandleSpan CandleSpan;
      public LineSeries LineSeriesLoadQuotes;
      public Collection<DateValue> RandomItemSource;
      public CandleStickSeries DailyCandleStickSeries;
      public int NoOfCandlesOnChart;
      public int DaysPerCandle = 1;
      public IEnumerable<double> DailyCloseList = null;
      public DateTimeAxis XAxis = null;
      public LinearAxis YAxis = null;
      public AnnotationAdder annotations;

      private CandleStickSeries weeklyCandleStickSeries;
      private CandleStickSeries monthlyCandleStickSeries;
      private CandleStickSeries displayCandleStickSeries = null;
      private DateTime displayStartTime;
      private int displayDepthInDays;
      public PlotModel PlotModel;
      private string ActiveShareCode;

      /// <summary>
      /// Used to notify the indicator of events
      /// </summary>
      private IndicatorChart indiModel;

      public EodChart(MainWindow mainWindow, PlotView eodPlotView, object indiViewModelObject)
      {
         indiModel = indiViewModelObject as IndicatorChart;
         indiModel.EodView = this;
         this.PlotView = eodPlotView;
         eodPlotView.Model = new PlotModel();
         this.PlotModel = eodPlotView.Model;
         this.SetupChart();
         annotations = new AnnotationAdder(mainWindow, eodPlotView as ChartPlotView, XAxis, YAxis, "eod");
      }

      public void InvalidateEodPlot()
      {
         // set scale
         double low = Double.MaxValue;
         double high = Double.MinValue;
         int actualCount;

         if (NoOfCandlesOnChart < displayCandleStickSeries.Items.Count)
            actualCount = NoOfCandlesOnChart;
         else
            actualCount = displayCandleStickSeries.Items.Count;

         // find the high and low of the visible candles
         int end = displayCandleStickSeries.Items.Count;
         // Note! start must be derived from the chart date and not the number of candles
         int start = displayCandleStickSeries.Items.FindIndex((p => p.X.CompareTo(DateTimeAxis.ToDouble(displayStartTime)) > 0));
         for (int idx = start; idx < end; idx++)
         {
            HighLowItem quote = displayCandleStickSeries.Items[idx] as HighLowItem;
            if (low > quote.Low) low = quote.Low;
            if (high < quote.High) high = quote.High;
         }

         displayCandleStickSeries.CandleWidth = 900 / NoOfCandlesOnChart;

         // scale the chart
         YAxis.Maximum = high;
         YAxis.Minimum = low;
         ChartModel.InvalidatePlot(true);
      }

      /// <summary>
      /// Converts between 'OxyPlot.Series.CandleStickSeries' and 'OxyPlot.PlotModel' types
      /// </summary>
      private PlotModel ChartModel
      {
         get
         {
            return this.PlotModel;
         }

         set
         {
            this.PlotModel = value;
            this.OnPropertyChanged("PlotModel");
         }
      }

      public void SetDisplayDepthInDays(int days)
      {
         int extraDays = 80;
         if (days < 400)
            extraDays = 30;

         displayDepthInDays = days + extraDays;
         NoOfCandlesOnChart = (int)(displayDepthInDays / DaysPerCandle);
         displayStartTime = DateTime.Now.AddDays(-displayDepthInDays);
         XAxis.Minimum = DateTimeAxis.ToDouble(displayStartTime);
         XAxis.Maximum = DateTimeAxis.ToDouble(DateTime.Now.AddDays(extraDays));
         indiModel.DisplayDepthChanged(XAxis.ActualMinimum, XAxis.ActualMaximum);
      }

      public void SetCandleSpan(CandleSpan candleSpan)
      {
         if (candleSpan != CandleSpan.Refresh)
            CandleSpan = candleSpan;
         switch (CandleSpan)
         {
            case CandleSpan.Day:
               ChartModel.Series.Clear();
               ChartModel.Series.Add(DailyCandleStickSeries);
               displayCandleStickSeries = DailyCandleStickSeries;
               NoOfCandlesOnChart = (int)displayDepthInDays;
               DaysPerCandle = 1;

               XAxis.IntervalType = DateTimeIntervalType.Manual;
               XAxis.MinorIntervalType = DateTimeIntervalType.Manual;
               XAxis.MinorStep = 1;
               XAxis.MajorStep = 30.35;
               XAxis.MajorTickSize = 10;
               XAxis.MinorTickSize = 5;
               XAxis.StringFormat = "dd MMM";
               XAxis.Angle = 0;
               //ChartModel.PlotMargins = new OxyPlot.OxyThickness(0, 0, 0, 30);
               break;

            case CandleSpan.Week:
               ChartModel.Series.Clear();
               ChartModel.Series.Add(weeklyCandleStickSeries);
               displayCandleStickSeries = weeklyCandleStickSeries;
               DaysPerCandle = 7;
               NoOfCandlesOnChart = (int)(displayDepthInDays / 7);
               XAxis.IntervalType = DateTimeIntervalType.Manual;
               XAxis.MinorIntervalType = DateTimeIntervalType.Manual;
               XAxis.MinorStep = 7;
               XAxis.MajorStep = 30.35;
               XAxis.MajorTickSize = 10;
               XAxis.MinorTickSize = 5;
               XAxis.StringFormat = "MMM yyyy";
               XAxis.Angle = 90;
               //ChartModel.PlotMargins = new OxyPlot.OxyThickness(0, 0, 0, 70);
               break;

            case CandleSpan.Month:
               ChartModel.Series.Clear();
               ChartModel.Series.Add(monthlyCandleStickSeries);
               displayCandleStickSeries = monthlyCandleStickSeries;
               DaysPerCandle = 30;
               NoOfCandlesOnChart = (int)(displayDepthInDays / 30);
               XAxis.IntervalType = DateTimeIntervalType.Manual;
               XAxis.MinorIntervalType = DateTimeIntervalType.Manual;
               XAxis.MinorStep = 1;
               XAxis.MajorStep = 30.35;
               XAxis.MajorTickSize = 10;
               XAxis.MinorTickSize = 0;
               XAxis.StringFormat = "MMM yyyy";
               XAxis.Angle = 90;
               //ChartModel.PlotMargins = new OxyPlot.OxyThickness(0, 0, 0, 70);
               break;
         }
      }

      /// <summary>
      /// Loads the EOD data into the EOD plot,
      /// and then calculates the indicator and loads that into the Indicator Polt
      /// </summary>
      /// <param name="newShareCode"></param>
      public void LoadQuotes(string newShareCode, bool NotifyIndiPlot)
      {
         // update annotations in the charter data base.
         int count = ChartModel.Annotations.Count;

         MySqlDbAccess shareDB = new MySqlDbAccess("localhost", "jse_eod", "root", "w51532", annotations);
         if (this.annotations.CustomOxyPlotView.IsModified)
         {
            shareDB.UpdateAnnotations(ActiveShareCode, ChartModel, "eod", "EOD text");
            this.annotations.CustomOxyPlotView.IsModified = false;
         }
         ChartModel.Annotations.Clear();

         DailyCandleStickSeries = new CandleStickSeries
         {
            //Title = newShareCode,
            CandleWidth = MainWindow.dayCandleWidth,
            IncreasingFill = OxyColors.Blue,
            DecreasingFill = OxyColors.Red,
            TrackerFormatString = "Share: \t{0:}\nDate: \t{1:yyyy-MM-dd}\nHigh: \t{2:0.}\nLow: \t{3:0}\nOpen: \t{4:0.}\nClose: \t{5:0.}"
         };

         List<Quote> DailyPrices = shareDB.GetSharePrices(newShareCode, out DailyCloseList);

         // add the quotes to the CandleStickSerieses:
         DailyPrices.ToList().ForEach(quote => DailyCandleStickSeries.Items.Add(new HighLowItem(DateTimeAxis.ToDouble(quote.Date),
                                                    quote.High, quote.Low, quote.Open, quote.Close)));
         List<Quote> dailyQuoteList = DailyPrices.ToList();

         weeklyCandleStickSeries = new CandleStickSeries
         {
            Title = newShareCode,
            CandleWidth = 30,
            IncreasingFill = OxyColors.Blue,
            DecreasingFill = OxyColors.Red,
            TrackerFormatString = "Share: \t{0:}\nDate: \t{1:yyyy-MM-dd}\nHigh: \t{2:0.}\nLow: \t{3:0}\nOpen: \t{4:0.}\nClose: \t{5:0.}"
         };

         // build the weekly candlestick series
         var ci = CultureInfo.CurrentCulture;
         var cal = ci.Calendar;
         var rule = ci.DateTimeFormat.CalendarWeekRule;
         var firstDayOfWeek = ci.DateTimeFormat.FirstDayOfWeek;

         var years = from quote in dailyQuoteList
                     group quote by quote.Date.Year;

         foreach (var year in years)
         {
            var weeks = from quote in year
                        group quote by cal.GetWeekOfYear(quote.Date, rule, firstDayOfWeek);

            foreach (var week in weeks)
            {
               double weekLow = Double.MaxValue;
               double weekHigh = Double.MinValue;
               double weekClose = 0;
               foreach (var quote in week)
               {
                  if (quote.Low < weekLow) weekLow = quote.Low;
                  if (quote.High > weekHigh) weekHigh = quote.High;
                  weekClose = quote.Close;
               }
               double weekOpen = week.ElementAt(0).Open;
               int candleYear = year.Key;
               int candleWeek = cal.GetWeekOfYear(week.ElementAt(0).Date, rule, firstDayOfWeek);

               DateTime weekDate = FirstDateOfWeek(candleYear, candleWeek).AddDays(2);
               weeklyCandleStickSeries.Items.Add(new HighLowItem(DateTimeAxis.ToDouble(weekDate), weekHigh, weekLow, weekOpen, weekClose));
            }
         }

         // Build the monthly candlestick series
         monthlyCandleStickSeries = new CandleStickSeries
         {
            Title = newShareCode,
            CandleWidth = 100,
            IncreasingFill = OxyColors.Blue,
            DecreasingFill = OxyColors.Red,
            TrackerFormatString = "Share: \t{0:}\nDate: \t{1:yyyy-MM-dd}\nHigh: \t{2:0.}\nLow: \t{3:0}\nOpen: \t{4:0.}\nClose: \t{5:0.}"
         };

         foreach (var year in years)
         {
            var months = from quote in year
                         group quote by quote.Date.Month;

            foreach (var day in months)
            {
               double monthLow = Double.MaxValue;
               double monthHigh = Double.MinValue;
               double monthClose = 0;
               foreach (var quote in day)
               {
                  if (quote.Low < monthLow) monthLow = quote.Low;
                  if (quote.High > monthHigh) monthHigh = quote.High;
                  monthClose = quote.Close;
               }
               double monthOpen = day.ElementAt(0).Open;
               int candleYear = year.Key;
               int candleMonth = day.ElementAt(0).Date.Month;
               DateTime monthDate = new DateTime(candleYear, candleMonth, 15); // middle of month
               monthlyCandleStickSeries.Items.Add(new HighLowItem(DateTimeAxis.ToDouble(monthDate),
                                                    monthHigh, monthLow, monthOpen, monthClose));
            }
         }

         ActiveShareCode = newShareCode;
         shareDB.GetAnnotations(newShareCode, "eod");

         // inform the Indicator plot of the new quote
         if (NotifyIndiPlot)
            indiModel.QuoteChanged();
      }  // end of LoadQuotes

      protected virtual void OnPropertyChanged(string propertyName)
      {
         PropertyChangedEventHandler handler = this.PropertyChanged;
         if (handler != null)
         {
            handler(this, new PropertyChangedEventArgs(propertyName));
         }
      }

      public void SetupChart()
      {
         /*ChartModel.LegendTitle = "Share";
         ChartModel.LegendOrientation = LegendOrientation.Horizontal;
         ChartModel.LegendPlacement = LegendPlacement.Outside;
         ChartModel.LegendPosition = LegendPosition.TopRight;
         ChartModel.LegendBackground = OxyColor.FromAColor(200, OxyColors.White);
         ChartModel.LegendBorder = OxyColors.Black;*/
         PlotView.Model.Padding = new OxyThickness(0);


         XAxis = new DateTimeAxis
         {
            Position = AxisPosition.Bottom,
            MajorGridlineStyle = LineStyle.Solid,
            StringFormat = "dd MMM yyyy",
            IntervalType = DateTimeIntervalType.Weeks,
            MajorTickSize = 10,
            MinorTickSize = 5,
            IsZoomEnabled = true,
            MaximumPadding = 0,
            MinimumPadding = 0,
            TickStyle = TickStyle.Inside
         };
         PlotView.Model.Axes.Add(XAxis);

         YAxis = new LinearAxis
         {
            Position = AxisPosition.Right,

            MajorGridlineStyle = LineStyle.Solid,
            MinorGridlineStyle = LineStyle.Dot,
            TickStyle = TickStyle.Outside,
            StringFormat = "00000",
         };
         PlotView.Model.Axes.Add(YAxis);
      }

      static DateTime FirstDateOfWeek(int year, int weekOfYear)
      {
         DateTime jan1 = new DateTime(year, 1, 1);

         int daysOffset = (int)CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek - (int)jan1.DayOfWeek;

         DateTime firstMonday = jan1.AddDays(daysOffset);

         int firstWeek = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(jan1, CultureInfo.CurrentCulture.DateTimeFormat.CalendarWeekRule, CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek);

         if (firstWeek <= 1)
         {
            weekOfYear -= 1;
         }

         return firstMonday.AddDays(weekOfYear * 7);
      }

   }

   public class DateValue
   {
      public DateTime Date { get; set; }
      public double Value { get; set; }
   }

}