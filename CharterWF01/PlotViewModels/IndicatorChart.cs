// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IndiModel.cs" company="Wynand">
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
   using CharterWF.ViewModels;
   using MySqlAccess;
   using OxyPlot;
   using OxyPlot.Axes;
   using OxyPlot.Series;
   using OxyPlot.WindowsForms;
   using System;
   using System.Collections.Generic;
   using System.Collections.ObjectModel;
   using System.Linq;
   using System.Text;
   using System.Threading.Tasks;
   using TechnicalAnalysisEngine;
   using TechnicalAnalysisEngine.Entities;

   /// <summary>
   /// TODO: This class provides
   /// </summary>
   public class IndicatorChart
   {
      public AnnotationAdder annotations;

      public PlotModel PlotModel;
      private static DateTimeAxis XAxis = null;
      private static LinearAxis YAxis = null;
      private LineSeries weeklyRsiSeries;
      private LineSeries monthlyRsiSeries;
      private LineSeries DailyRsiSeries;
      private LineSeries DisplayRsiSeries = null;
      private EodChart EodViewModel;

      public IndicatorChart(MainWindow mainWindow, PlotView indiPlotView)
      {
         this.PlotModel = new PlotModel();
         indiPlotView.Model = this.PlotModel;
         SetupChart();
         annotations = new AnnotationAdder(mainWindow, indiPlotView as ChartPlotView, XAxis, YAxis, "indi");
      }

      public void LoadAnnotations(string share)
      {
         MySqlDbAccess shareDB = new MySqlDbAccess("localhost", "jse_eod", "root", "w51532", annotations);
         this.PlotModel.Annotations.Clear();
         shareDB.GetAnnotations(share, "rsi");
      }

      public void SaveAnnotations(string share)
      {
         MySqlDbAccess shareDB = new MySqlDbAccess("localhost", "jse_eod", "root", "w51532", annotations);
         if (this.annotations.CustomOxyPlotView.IsModified)
         {
            shareDB.UpdateAnnotations(share, this.PlotModel, "rsi", "indi text");
            this.annotations.CustomOxyPlotView.IsModified = false;
         }
      }

      public void SetEodViewModel(EodChart eodViewModel)
      {
         this.EodViewModel = eodViewModel;
      }

      public ViewModels.EodChart EodView
      { get; set; }

      public void SetupChart()
      {
         this.PlotModel.Padding = new OxyThickness(0);

         XAxis = new DateTimeAxis
         {
            Position = AxisPosition.Bottom,
            StringFormat = "MMM yyyy",
            IntervalType = DateTimeIntervalType.Months,
            Minimum = DateTime.Now.Add(TimeSpan.FromDays(-365)).ToOADate(),
         };
         this.PlotModel.Axes.Add(XAxis);

         YAxis = new LinearAxis
         {
            Position = AxisPosition.Right,
            MajorGridlineStyle = LineStyle.Solid,
            MinorGridlineStyle = LineStyle.Dot,
            TickStyle = TickStyle.Outside,
            StringFormat = "00000",
            //Title = "RSI",
            Maximum = 100,
            Minimum = 0
         };

         this.PlotModel.Axes.Add(YAxis);
      }

      public void Clear()
      {
         this.PlotModel.Series.Clear();
         this.PlotModel.InvalidatePlot(true);
      }

      public void QuoteChanged()
      {
         if (this.EodView.DailyCandleStickSeries.Items.Count == 0)
            return;
         RSIResult RsiResult = AnalysisEngine.RSI(EodViewModel.DailyCloseList, 13);

         // build daily RSI list for the full period - 13
         Collection<DataPoint> RsiDailyItems = new Collection<DataPoint>();
         int len = this.EodViewModel.DailyCloseList.ToArray().Length - 1;

         for (int i = 13; i < len - 1; i++)
         {
            RsiDailyItems.Add(new DataPoint(this.EodView.DailyCandleStickSeries.Items[i].X, RsiResult.Values[i - 13]));
         }

         var dailyRsiSeries = new LineSeries
         {
            StrokeThickness = 1,
            ItemsSource = RsiDailyItems,
            Color = OxyColors.Blue,
         };

         this.PlotModel.Series.Clear();
         this.PlotModel.Series.Add(dailyRsiSeries);
         this.PlotModel.InvalidatePlot(true);

      }

      public void Test()
      {
         TechnicalAnalysisEngine.Entities.RSIResult RsiResult = TechnicalAnalysisEngine.AnalysisEngine.RSI(this.EodViewModel.DailyCloseList, 13);

         // build annotation daily RSI list for the full period - 13
         double[] dailyArray = this.EodViewModel.DailyCloseList.ToArray();
         Collection<DataPoint> RsiItemSource = new Collection<DataPoint>();
         int len = dailyArray.Length - 1;

         this.PlotModel.Series.Clear();
         for (int i = 13; i < len - 1; i++)
         {
            RsiItemSource.Add(new DataPoint(this.EodView.DailyCandleStickSeries.Items[i].X, RsiResult.Values[i - 13]));
         }

         var series1 = new LineSeries
         {
            StrokeThickness = 1,
            ItemsSource = RsiItemSource,
            Color = OxyColors.Blue,
         };
         this.PlotModel.Series.Add(series1);

         this.PlotModel.InvalidatePlot(true);
      }

      public void CandleSpanChanged(CandleSpan candleSpan, int NoOfCandlesOnChart, int displayDepthInDays)
      {
         switch (candleSpan)
         {
            case CandleSpan.Day:
               this.PlotModel.Series.Clear();
               this.PlotModel.Series.Add(DailyRsiSeries);
               this.DisplayRsiSeries = DailyRsiSeries;

               XAxis.IntervalType = DateTimeIntervalType.Manual;
               XAxis.MinorIntervalType = DateTimeIntervalType.Manual;
               XAxis.MinorStep = 1;
               XAxis.MajorStep = 30.35;
               XAxis.MajorTickSize = 10;
               XAxis.MinorTickSize = 5;
               XAxis.StringFormat = "dd MMM";
               XAxis.Angle = 0;
               break;

            case CandleSpan.Week:
               this.PlotModel.Series.Clear();
               this.PlotModel.Series.Add(weeklyRsiSeries);
               this.DisplayRsiSeries = weeklyRsiSeries;
               NoOfCandlesOnChart = (int)(displayDepthInDays / 7);
               XAxis.IntervalType = DateTimeIntervalType.Manual;
               XAxis.MinorIntervalType = DateTimeIntervalType.Manual;
               XAxis.MinorStep = 7;
               XAxis.MajorStep = 30.35;
               XAxis.MajorTickSize = 10;
               XAxis.MinorTickSize = 5;
               XAxis.StringFormat = "MMM yyyy";
               XAxis.Angle = 90;
               break;

            case CandleSpan.Month:
               this.PlotModel.Series.Clear();
               this.PlotModel.Series.Add(monthlyRsiSeries);
               this.DisplayRsiSeries = monthlyRsiSeries;
               NoOfCandlesOnChart = (int)(displayDepthInDays / 30);
               XAxis.IntervalType = DateTimeIntervalType.Manual;
               XAxis.MinorIntervalType = DateTimeIntervalType.Manual;
               XAxis.MinorStep = 1;
               XAxis.MajorStep = 30.35;
               XAxis.MajorTickSize = 10;
               XAxis.MinorTickSize = 0;
               XAxis.StringFormat = "MMM yyyy";
               XAxis.Angle = 90;
               break;
         }

      }

      public void DisplayDepthChanged(double Min, double Max)
      {
         XAxis.Minimum = Min;
         XAxis.Maximum = Max;
         this.PlotModel.InvalidatePlot(false);
      }
   }
}
