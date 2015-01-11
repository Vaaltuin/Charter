// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Class1.cs" company="Wynand">
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
namespace CharterWF
{
   using System;
   using System.Collections.Generic;
   using System.Linq;
   using System.Runtime.InteropServices;
   using System.Text;
   using System.Threading.Tasks;
   using System.Windows;
   using System.Windows.Input;

   /// <summary>
   /// TODO: This class provides
   /// </summary>
   public partial class MainWindow : Window
   {
      [DllImport("User32.dll")]
      private static extern bool SetCursorPos(int x, int y);

      private void TestCommand_Executed(object sender, ExecutedRoutedEventArgs e)
      {
         MessageBox.Show("TestCommand_Executed");
      }

      private void TestCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
      {
         e.CanExecute = true;
      }

      private void ExitCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
      {
         e.CanExecute = true;
      }

      private void ExitCommand_Executed(object sender, ExecutedRoutedEventArgs e)
      {
         Application.Current.Shutdown();
      }

      private void GetEODCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
      {
         e.CanExecute = true;
      }

      private void GetEODCommand_Executed(object sender, ExecutedRoutedEventArgs e)
      {
         new EodEntryDialog().Show();
      }

      private void GetDailyCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
      {
         e.CanExecute = true;
      }

      private void GetDailyCommand_Executed(object sender, ExecutedRoutedEventArgs e)
      {
         new DailyDataDialog().Show();
      }

      private void SetTimeFrameCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
      {
         e.CanExecute = true;
      }

      private void ChartLengthCommand_Executed(object sender, ExecutedRoutedEventArgs e)
      {
         switch (e.Parameter as string)
         {
            case "3M": this.EodViewModel.SetDisplayDepthInDays((int)(365 / 4)); break;
            case "6M": this.EodViewModel.SetDisplayDepthInDays((int)(365 / 2)); break;
            case "1Y": this.EodViewModel.SetDisplayDepthInDays(365); break;
            case "2Y": this.EodViewModel.SetDisplayDepthInDays(365 * 2); break;
            case "5Y": this.EodViewModel.SetDisplayDepthInDays(5 * 365); break;
            case "10Y": this.EodViewModel.SetDisplayDepthInDays(10 * 365); break;
         }
         this.EodViewModel.InvalidateEodPlot();
      }

      private void ChartLengthCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
      {
         e.CanExecute = true;
      }

      private void SetModeCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
      {
         e.CanExecute = true;
      }

      private void SetModeCommand_Executed(object sender, ExecutedRoutedEventArgs e)
      {
         switch ((MouseActions)e.Parameter)
         {
            case MouseActions.NewLDT:
               EodOxyPlotChart.MouseAction = MouseActions.NewLDT;
               IndicatorOxyPlotChart.MouseAction = MouseActions.NewLDT;
               EodOxyPlotChart.Cursor = new System.Windows.Forms.Cursor(@"..\..\Images\Cursors\LineDrawing.cur");
               IndicatorOxyPlotChart.MouseAction = MouseActions.NewLDT;
               IndicatorOxyPlotChart.Cursor = new System.Windows.Forms.Cursor(@"..\..\Images\Cursors\LineDrawing.cur");
               PutTextOnAnnotations = true;
               break;

            case MouseActions.MovingAround:
               EodOxyPlotChart.MouseAction = MouseActions.MovingAround;
               IndicatorOxyPlotChart.MouseAction = MouseActions.MovingAround;
               IndicatorOxyPlotChart.Cursor = System.Windows.Forms.Cursors.Arrow;
               EodOxyPlotChart.Cursor = System.Windows.Forms.Cursors.Arrow;
               break;

            case MouseActions.Deleting:
               EodOxyPlotChart.MouseAction = MouseActions.Deleting;
               IndicatorOxyPlotChart.MouseAction = MouseActions.Deleting;
               EodOxyPlotChart.Cursor = new System.Windows.Forms.Cursor(@"..\..\Images\Cursors\Delete.cur");
               IndicatorOxyPlotChart.Cursor = new System.Windows.Forms.Cursor(@"..\..\Images\Cursors\Delete.cur");
               break;

            case MouseActions.NewArrowLine:
               EodOxyPlotChart.MouseAction = MouseActions.NewArrowLine;
               IndicatorOxyPlotChart.MouseAction = MouseActions.NewArrowLine;
               EodOxyPlotChart.Cursor = new System.Windows.Forms.Cursor(@"..\..\Images\Cursors\LineDrawing.cur");
               IndicatorOxyPlotChart.MouseAction = MouseActions.NewArrowLine;
               IndicatorOxyPlotChart.Cursor = new System.Windows.Forms.Cursor(@"..\..\Images\Cursors\LineDrawing.cur");
               PutTextOnAnnotations = false;
               break;

            case MouseActions.NewTextArrowLine:
               EodOxyPlotChart.MouseAction = MouseActions.NewTextArrowLine;
               IndicatorOxyPlotChart.MouseAction = MouseActions.NewTextArrowLine;
               EodOxyPlotChart.Cursor = new System.Windows.Forms.Cursor(@"..\..\Images\Cursors\LineDrawing.cur");
               IndicatorOxyPlotChart.MouseAction = MouseActions.NewArrowLine;
               IndicatorOxyPlotChart.Cursor = new System.Windows.Forms.Cursor(@"..\..\Images\Cursors\LineDrawing.cur");
               PutTextOnAnnotations = true;
               break;

            case MouseActions.NewLine:
               EodOxyPlotChart.MouseAction = MouseActions.NewLine;
               EodOxyPlotChart.Cursor = new System.Windows.Forms.Cursor(@"..\..\Images\Cursors\DrawingHorizontal.cur");
               IndicatorOxyPlotChart.MouseAction = MouseActions.NewLine;
               IndicatorOxyPlotChart.Cursor = new System.Windows.Forms.Cursor(@"..\..\Images\Cursors\DrawingHorizontal.cur");
               PutTextOnAnnotations = false;
               break;

            case MouseActions.NewLineText:
               EodOxyPlotChart.MouseAction = MouseActions.NewLineText;
               EodOxyPlotChart.Cursor = new System.Windows.Forms.Cursor(@"..\..\Images\Cursors\DrawingHorizontal.cur");
               IndicatorOxyPlotChart.MouseAction = MouseActions.NewLineText;
               IndicatorOxyPlotChart.Cursor = new System.Windows.Forms.Cursor(@"..\..\Images\Cursors\DrawingHorizontal.cur");
               PutTextOnAnnotations = true;
               break;

            case MouseActions.NewText:
               EodOxyPlotChart.MouseAction = MouseActions.NewText;
               IndicatorOxyPlotChart.MouseAction = MouseActions.NewText;
               AnnotationTextBox.Focus();
               AnnotationTextBox.SelectAll();

               // move mouse to the AnnotationTextBox box
               Point p = AnnotationTextBox.PointToScreen(new Point(20, 5));
               SetCursorPos((int)p.X, (int)p.Y);

               break;
         }
      }
      private void SetTimeFrameCommand_Executed(object sender, ExecutedRoutedEventArgs e)
      {
         switch (e.Parameter as string)
         {
            case "D": this.EodViewModel.SetCandleSpan(CandleSpan.Day); break;
            case "W": this.EodViewModel.SetCandleSpan(CandleSpan.Week); break;
            case "M": this.EodViewModel.SetCandleSpan(CandleSpan.Month); break;
         }
         this.EodViewModel.InvalidateEodPlot();
      }
   }

   /// <summary>
   /// Custom command to exit as in my document www.tutorial.org
   /// </summary>
   public static class CustomCommands
   {
      public static readonly RoutedUICommand Exit = new RoutedUICommand
         (
            "Exit",
            "Exit",
            typeof(CustomCommands),
            new InputGestureCollection()
               {
                  new KeyGesture(Key.Escape, ModifierKeys.None)
               }
         );

      public static readonly RoutedUICommand Test = new RoutedUICommand
         (
            "Test",
            "Test",
            typeof(CustomCommands),
            new InputGestureCollection()
               {
                  new KeyGesture(Key.D1, ModifierKeys.Control)
               }
         );

      public static readonly RoutedUICommand GetEOD = new RoutedUICommand
         (
            "EOD Data",
            "EOD Data",
            typeof(CustomCommands),
            new InputGestureCollection()
               {
                  new KeyGesture(Key.E, ModifierKeys.Control)
               }
         );

      public static readonly RoutedUICommand GetDailyData = new RoutedUICommand
         (
            "Daily Data",
            "Daily Data",
            typeof(CustomCommands),
            new InputGestureCollection()
               {
                  new KeyGesture(Key.D, ModifierKeys.Control)
               }
         );

      public static readonly RoutedUICommand SetChartLength = new RoutedUICommand
         (
            "Set Timeframe",
            "Set Timeframe",
            typeof(CustomCommands),
            new InputGestureCollection()
               {
                  new KeyGesture(Key.T, ModifierKeys.Control)
               }
         );

      public static readonly RoutedUICommand SetMode = new RoutedUICommand
         (
            "Set Mode",
            "Set Mode",
            typeof(CustomCommands),
            new InputGestureCollection()
               {
                  new KeyGesture(Key.T, ModifierKeys.Control)
               }
         );

      public static readonly RoutedUICommand SetTimeFrame = new RoutedUICommand
         (
            "Set Mode",
            "Set Mode",
            typeof(CustomCommands),
            new InputGestureCollection()
               {
                  new KeyGesture(Key.T, ModifierKeys.Control)
               }
         );
   }
}
