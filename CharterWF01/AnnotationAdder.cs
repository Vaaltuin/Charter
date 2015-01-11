// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Annotation.cs" company="Wynand">
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
   using CharterWF.ViewModels;
   using MySqlAccess;
   using OxyPlot;
   using OxyPlot.Annotations;
   using OxyPlot.Axes;
   using System;
   using System.Drawing;
   using System.Windows.Input;

   public enum Thumb { P1, P2, Centre };

   /// <summary>
   /// TODO: This class provides
   /// </summary>
   public class AnnotationAdder
   {
      public string Chart;
      public ChartPlotView CustomOxyPlotView;
      public MainWindow MainWindow;

      private DateTimeAxis XAxis;
      private LinearAxis YAxis;

      public AnnotationAdder(MainWindow win, ChartPlotView plot, DateTimeAxis xAxis, LinearAxis yAxis, string chart)
      {
         this.CustomOxyPlotView = plot;
         this.XAxis = xAxis;
         this.YAxis = yAxis;
         this.MainWindow = win;
         this.Chart = chart;
      }

      #region ArrowLines
      public PriceArrowAnnotation NewArrowLine(Point startScreenPoint, Point endScreenPoint)
      {
         DataPoint endPoint = XAxis.InverseTransform(endScreenPoint.X, endScreenPoint.Y, this.YAxis);
         DataPoint startPoint = XAxis.InverseTransform(startScreenPoint.X, startScreenPoint.Y, YAxis);
         OxyColor color;
         color = MainWindow.drawingColor;
         PriceArrowAnnotation PriceArrowAnnotation;

         if (MainWindow.PutTextOnAnnotations)
         {
            PriceArrowAnnotation = new PriceArrowAnnotation
            {
               StartPoint = startPoint,
               EndPoint = endPoint,
               Color = color,
               LineStyle = (LineStyle)MainWindow.lineStyle,
               StrokeThickness = (MainWindow.mainWindow as MainWindow).StrokeThickness,
               Text = MainWindow.NewAnnotationText,
               Font = MainWindow.fontName,
               FontSize = MainWindow.fontSize
            };
         }
         else
         {
            PriceArrowAnnotation = new PriceArrowAnnotation
            {
               StartPoint = startPoint,
               EndPoint = endPoint,
               Color = color,
               LineStyle = (LineStyle)MainWindow.lineStyle,
               StrokeThickness = (MainWindow.mainWindow as MainWindow).StrokeThickness
            };
         }
         CustomOxyPlotView.Model.Annotations.Add(PriceArrowAnnotation);

         return PriceArrowAnnotation;
      }

      public PriceArrowAnnotation AddArrowLineAnnotation(ArrowLineAnnotationArgs args)
      {
         PriceArrowAnnotation PriceArrowAnnotation = new PriceArrowAnnotation
         {
            StartPoint = args.StartPoint,
            EndPoint = args.EndPoint,
            StrokeThickness = args.StrokeThickness,
            LineStyle = args.LineStyle,
            Color = args.Color,
            Text = args.Text,
            Font = args.Font,
            FontSize = args.FontSize,
         };
         CustomOxyPlotView.Model.Annotations.Add(PriceArrowAnnotation);
         return PriceArrowAnnotation;
      }
      #endregion

      #region Lines

      public LabeledTextAnnotation AddLDTAnnotation(LDTAnnotationArgs args)
      {
         LabeledTextAnnotation ldtAnnotation = new LabeledTextAnnotation()
         {
            Color = args.Color,
            Text = args.Text,
            Font = args.Font,
            FontSize = args.FontSize,
            LineStyle = args.LineStyle,
            StrokeThickness = args.StrokeThickness,
            StartPoint = args.StartPoint,
            EndPoint = args.EndPoint
         };
         CustomOxyPlotView.Model.Annotations.Add(ldtAnnotation);
         return ldtAnnotation;

      }

      public PriceLineAnnotation AddLineAnnotation(LineAnnotationArgs args)
      {
         PriceLineAnnotation PriceLineAnnotation = new PriceLineAnnotation()
         {
            MinimumX = args.MinimumX,
            MaximumX = args.MaximumX,
            MinimumY = args.MinimumY,
            MaximumY = args.MaximumY,
            Intercept = args.Intercept,
            Slope = args.Slope,
            Color = args.Color,
            Text = args.Text,
            Font = args.Font,
            FontSize = args.FontSize,
            LineStyle = args.LineStyle,
            StrokeThickness = args.StrokeThickness,
            Type = args.Type,
            Y = args.Y,
            X = args.X
         };
         CustomOxyPlotView.Model.Annotations.Add(PriceLineAnnotation);
         return PriceLineAnnotation;
      }

      #endregion

      #region Text
      public PriceTextAnnotation AddTextAnnotation(TextAnnotationArgs args)
      {
         PriceTextAnnotation newTextAnnotation = new PriceTextAnnotation()
         {
            TextPosition = args.Position,
            FontSize = args.FontSize,
            Font = args.Font,
            Text = args.Text
         };
         CustomOxyPlotView.Model.Annotations.Add(newTextAnnotation);
         return newTextAnnotation;
      }

      #endregion

   }
}
