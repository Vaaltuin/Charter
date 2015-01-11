namespace CharterWF
{
   using MySqlAccess;
   using OxyPlot;
   using OxyPlot.Series;
   using System.Media;
   using System.Windows;
   using System.Windows.Controls;
   using System.Windows.Input;
   using System.Windows.Media;
   using System;
   using System.ComponentModel;
   using WBGraphicUtils;

   /// <summary>
   /// Interaction logic for MainWindow.xaml
   /// </summary>
   public partial class MainWindow
   {
      public static ChartTimeFrame chartTimeFrame = ChartTimeFrame.Daily;
      public static int monthCandleWidth = 50;
      public static int weekCandleWidth = 12;
      public static int dayCandleWidth = 3;
      public static string currentShare;
      public static int fontSize;
      public static LineStyle lineStyle;
      public static string fontName;
      public static bool PutTextOnAnnotations = false;
      public static OxyColor drawingColor = OxyColors.Blue;
      public static Control mainWindow;

      private ViewModels.IndicatorChart IndiViewModel;
      private ViewModels.EodChart EodViewModel;
      private static string annotationText;
      private int strokeThickness;

      public MainWindow()
      {
         InitializeComponent();
         mainWindow = this;
      }

      public OxyColor DrawingColor
      {
         get
         {
            return drawingColor;
         }
         set
         {
            drawingColor = value;
            ColorPicker.SelectedColor = Color.FromArgb(drawingColor.A, drawingColor.R, drawingColor.G, drawingColor.B);
         }
      }

      public int StrokeThickness
      {
         get { return strokeThickness; }
         set
         {
            strokeThickness = value;
            LineWidthCombo.SelectedIndex = value - 1;
         }
      }

      public LineStyle LineStyle
      {
         get { return lineStyle; }
         set
         {
            lineStyle = value;
            LineStyleCombo.SelectedIndex = (int)value;
         }
      }

      public static string NewAnnotationText { get { return (annotationText); } }

      /// <summary>
      /// This function set the mouse action for all charts
      /// </summary>
      /// <param name="action"></param>
      public void SetMouseActions(MouseActions action)
      {
         (this.IndiViewModel.PlotModel.PlotView as ChartPlotView).MouseAction = action;
         (this.EodViewModel.PlotModel.PlotView as ChartPlotView).MouseAction = action;
      }

      public void SelectSelectingMode()
      {
         EodOxyPlotChart.MouseAction = MouseActions.MovingAround;
         EodOxyPlotChart.Cursor = System.Windows.Forms.Cursors.Arrow;
         IndicatorOxyPlotChart.MouseAction = MouseActions.MovingAround;
         IndicatorOxyPlotChart.Cursor = System.Windows.Forms.Cursors.Arrow;
         SelectButton.IsChecked = true;
      }

      private void Window_Loaded(object sender, RoutedEventArgs e)
      {
         drawingColor = OxyColor.FromUInt32(Properties.Settings.Default.DrawingColor);
         ColorPicker.SelectedColor = Color.FromArgb(drawingColor.A, drawingColor.R, drawingColor.G, drawingColor.B);
         StrokeThickness = Properties.Settings.Default.LineWidth;
         LineWidthCombo.SelectedIndex = StrokeThickness - 1;
         lineStyle = (LineStyle)Properties.Settings.Default.LineStyle;
         this.LineStyleCombo.SelectedIndex = Properties.Settings.Default.LineStyle;
         this.AnnotationTextBox.Text = Properties.Settings.Default.AnnotationTextBox;
         fontSize = Properties.Settings.Default.FontSize;
         FontSizes.Text = fontSize.ToString();
         fontName = Properties.Settings.Default.Font;
         FontCombo.Text = fontName.ToString();

         annotationText = AnnotationTextBox.Text;
         this.IndiViewModel = new ViewModels.IndicatorChart(this, IndicatorOxyPlotChart);
         this.EodViewModel = new ViewModels.EodChart(this, this.EodOxyPlotChart, this.IndiViewModel);
         this.IndiViewModel.SetEodViewModel(this.EodViewModel);
         this.DataContext = this;

         currentShare = ShareTextBox.Text as string;
         ShareTextBox.Text = currentShare;
         this.EodViewModel.SetDisplayDepthInDays(365);
         this.EodViewModel.LoadQuotes(MainWindow.currentShare, false);
         this.IndiViewModel.LoadAnnotations(MainWindow.currentShare);
         this.EodViewModel.SetCandleSpan(CandleSpan.Day);
         this.EodViewModel.InvalidateEodPlot();
         this.IndiViewModel.QuoteChanged();
         this.EodViewModel.SetDisplayDepthInDays(365);
         MySqlDbAccess databaseConnection = new MySqlDbAccess("localhost", "jse_eod", "root", "w51532", EodViewModel.annotations);
         databaseConnection.FillListViewWithShares(SharesListView);
      }

      private void ShareKeyDown(object sender, KeyEventArgs e)
      {
         if (e.Key == Key.Enter)
         {
            int newIdx = SharesListView.Items.IndexOf(ShareTextBox.Text);
            if (newIdx == -1)
            {
               SystemSounds.Beep.Play();
               return;
            }

            currentShare = ShareTextBox.Text;
            this.IndiViewModel.SaveAnnotations(MainWindow.currentShare);
            SharesListView.SelectedIndex = newIdx;
            GetNewShare();
         }
      }

      private void lvShares_SelectionChanged(object sender, SelectionChangedEventArgs e)
      {
         this.IndiViewModel.SaveAnnotations(MainWindow.currentShare);
         MainWindow.currentShare = (SharesListView.Items[SharesListView.SelectedIndex] as string).ToLower();
         GetNewShare();
         ShareTextBox.Text = SharesListView.Items[SharesListView.SelectedIndex] as string;
      }

      private void GetNewShare()
      {
         this.EodViewModel.LoadQuotes(MainWindow.currentShare, true); // this also updates currentShare
         this.EodViewModel.SetCandleSpan(CandleSpan.Refresh);
         this.EodViewModel.InvalidateEodPlot();
         this.IndiViewModel.LoadAnnotations(MainWindow.currentShare);
      }

      /// <summary>
      /// This event gets fired frequently as opposed to the one in CustomOxyPlot that
      /// only gets fired once the mouse has stopped moving
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="mouseEventArgs"></param>
      private void ChartsMouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
      {
         // this routine gets send to both EOD and Indi charts
         ChartPlotView plotView = (sender as ChartPlotView);
         plotView.DoMouseMove(e);
         this.PriceTextBlock.Text = plotView.Model.DefaultYAxis.InverseTransform(e.Y).ToString("0");
         
         DateTime d = DateTime.FromOADate(this.EodOxyPlotChart.Model.DefaultXAxis.InverseTransform(e.X));
         this.DateTextBlock.Text = d.ToString("yyyy:MM:dd");
      }

      /// <summary>
      /// Catches the keys from the main window, preventing the Up and Down key from leaving the plot
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="mouseEventArgs"></param>
      private void OnMainKeyDown(object sender, KeyEventArgs e)
      {
         if ((e.Key == Key.Up) || (e.Key == Key.Down))
         {
            // this stops the focus jumping from the plot to the rest of the form
            e.Handled = true;
         }
      }

      private void AnnotationText_LostFocus(object sender, RoutedEventArgs e)
      {
         annotationText = (sender as TextBox).Text;
      }

      private void FontSizes_SelectionChanged(object sender, SelectionChangedEventArgs e)
      {
         if (FontSizes.SelectedValue == null) return;
         var size = (FontSizes.SelectedValue as ComboBoxItem).Content;
         if (size == null)
            fontSize = 12;
         else
            fontSize = Convert.ToInt32(size);
      }

      private void Font_SelectionChanged(object sender, SelectionChangedEventArgs e)
      {
         if (FontCombo.SelectedValue == null) return;
         fontName = (FontCombo.SelectedValue as FontFamily).Source;
      }

      private void LineStyle_SelectionChanged(object sender, SelectionChangedEventArgs e)
      {
         lineStyle = (OxyPlot.LineStyle)LineStyleCombo.SelectedIndex;
         if ((this.EodOxyPlotChart != null) && (this.EodOxyPlotChart.SelectedLineAnotation != null))
         {
            this.EodOxyPlotChart.SelectedLineAnotation.LineStyle = lineStyle;
            this.EodOxyPlotChart.IsModified = true;
            this.EodOxyPlotChart.Invalidate();
         }
      }

      private void LineWidth_SelectionChanged(object sender, SelectionChangedEventArgs e)
      {
         StrokeThickness = LineWidthCombo.SelectedIndex + 1;
         if ((this.EodOxyPlotChart != null) && (this.EodOxyPlotChart.SelectedLineAnotation != null))
         {
            this.EodOxyPlotChart.SelectedLineAnotation.StrokeThickness = StrokeThickness;
            this.EodOxyPlotChart.IsModified = true;
            this.EodOxyPlotChart.Invalidate();
         }
      }

      private void ColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
      {
         drawingColor = OxyColor.FromRgb(ColorPicker.SelectedColor.R, ColorPicker.SelectedColor.G, ColorPicker.SelectedColor.B);
         if (this.EodOxyPlotChart.SelectedLineAnotation != null)
         {
            this.EodOxyPlotChart.SelectedLineAnotation.Color = drawingColor;
            this.EodOxyPlotChart.IsModified = true;
            this.EodOxyPlotChart.Invalidate();
         }
      }

      private void Window_Closing(object sender, CancelEventArgs e)
      {
         Properties.Settings.Default.DrawingColor = drawingColor.ToUint();
         Properties.Settings.Default.LineStyle = (int)lineStyle;
         Properties.Settings.Default.LineWidth = StrokeThickness;
         Properties.Settings.Default.AnnotationTextBox = AnnotationTextBox.Text;
         Properties.Settings.Default.Font = fontName;
         Properties.Settings.Default.FontSize = fontSize;
         Properties.Settings.Default.Save();
      }

      private void TestButton_Click(object sender, RoutedEventArgs e)
      {

         foreach (var a in this.EodOxyPlotChart.Model.Annotations)
         {

         }
      }

   }
}
