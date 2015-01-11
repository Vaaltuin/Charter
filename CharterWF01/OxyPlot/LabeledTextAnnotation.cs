// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LabeledTextAnnotation.cs" company="OxyPlot">
//   The MIT License (MIT)
//   
//   Copyright (c) 2014 OxyPlot contributors
//   
//   Permission is hereby granted, free of charge, to any person obtaining annotation
//   copy of this software and associated documentation files (the
//   "Software"), to deal in the Software without restriction, including
//   without limitation the rights to use, copy, modify, merge, publish,
//   distribute, sublicense, and/or sell copies of the Software, and to
//   permit persons to whom the Software is furnished to do so, subject to
//   the following conditions:
//   
//   The above copyright notice and this permission notice shall be included
//   in all copies or substantial portions of the Software.
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
//   Represents an annotation that shows an down arrow above the candle, and text next to
//  it that holds the dividend value, with optional currency and conversion 
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using CharterWF;
using System.Drawing;
using System.Windows.Forms;
using WBGraphicUtils;
namespace OxyPlot.Annotations
{
   /// <summary>
   /// Represents an annotation that shows an arrow.
   /// </summary>
   public class LabeledTextAnnotation : TextualAnnotation, ChartPlotViewSupport
   {
      public ScreenPoint LastMousePos;

      /// <summary>
      /// The end point in screen coordinates.
      /// </summary>
      private ScreenPoint screenEndPoint;

      /// <summary>
      /// The start point in screen coordinates.
      /// </summary>
      private ScreenPoint screenStartPoint;
      private DataPoint startPoint;
      private DataPoint endPoint;
      private bool IsHighLighted;
      private OxyPlot.PlotModel model;
      private System.Drawing.Font font;
      private Size size;
      private Point oldMousePoint;
      private bool BeingEdited;
      private OxyColor SystemDrawingColor;
      private Control Window;
      private GDI32 Gdi;
      private MouseEventArgs mouseEventArgs;

      /// <summary>
      /// Initializes annotation new instance of the <see cref="LabeledTextAnnotation" /> class.
      /// </summary>
      public LabeledTextAnnotation()
      {
         this.HeadLength = 10;
         this.HeadWidth = 3;
         this.Color = OxyColors.Blue;
         this.StrokeThickness = 2;
         this.LineStyle = LineStyle.Solid;
         this.LineJoin = OxyPenLineJoin.Miter;
         this.Label = "LDT1";
      }

      public LabeledTextAnnotation(MainWindow mainWindow, PlotModel model, string label, MouseEventArgs e)
      {
         this.IsHighLighted = false;
         this.HeadLength = 10;
         this.HeadWidth = 3;
         this.Color = OxyColors.Blue;
         this.LineStyle = LineStyle.Solid;
         this.LineJoin = OxyPenLineJoin.Miter;
         this.StrokeThickness = mainWindow.StrokeThickness;
         this.TextColor = MainWindow.drawingColor;
         this.model = model;
         this.Text = MainWindow.NewAnnotationText;
         this.Label = label;

         DataPoint p1 = new DataPoint(0, 0);
         p1.X = model.DefaultXAxis.InverseTransform(e.X);
         p1.Y = model.DefaultYAxis.InverseTransform(e.Y);

         this.TextPosition = new DataPoint(p1.X, p1.Y);
         this.FontSize = MainWindow.fontSize;
         this.StartPoint = p1;

         this.EndPoint = new DataPoint(p1.X, p1.Y - 60);
      }

      /// <summary>
      /// Used in moving the labeledtext before the annotation is created
      /// </summary>
      /// <param name="window"></param>
      /// <param name="gdi"></param>
      /// <param name="OldMousePoint"></param>
      /// <param name="e"></param>
      public static void DragText(Control window, GDI32 gdi, Point OldMousePoint, MouseEventArgs e)
      {
         Font font;
         Size size;

         NativeTextRenderer tr = new NativeTextRenderer(window.CreateGraphics());
         font = new Font(MainWindow.fontName, MainWindow.fontSize);
         size = tr.MeasureString(MainWindow.NewAnnotationText, font);

         // remove the old
         // remove the text
         tr.XorDrawString(MainWindow.NewAnnotationText, font, new Point(OldMousePoint.X - size.Width / 2, OldMousePoint.Y - size.Height));
         Point p1 = new Point((int)OldMousePoint.X - size.Width / 2, (int)OldMousePoint.Y - size.Height - 4);
         Point p2 = new Point((int)OldMousePoint.X + size.Width / 2, (int)OldMousePoint.Y + 4);
         // remove the rectangle:
         gdi.XorDrawLine(window.CreateGraphics(), p1, new Point(p2.X, p1.Y));
         gdi.XorDrawLine(window.CreateGraphics(), p1, new Point(p1.X, p2.Y));
         gdi.XorDrawLine(window.CreateGraphics(), new Point(p1.X, p2.Y), p2);
         gdi.XorDrawLine(window.CreateGraphics(), new Point(p2.X, p1.Y), p2);
         p1 = new Point((int)e.X - size.Width / 2, (int)e.Y - size.Height - 4);
         p2 = new Point((int)e.X + size.Width / 2, (int)e.Y + 4);

         // show the new position:
         // show the text
         tr.XorDrawString(MainWindow.NewAnnotationText, font, new Point(e.X - size.Width / 2, e.Y - size.Height));
         //  the rectangle
         gdi.XorDrawLine(window.CreateGraphics(), p1, new Point(p2.X, p1.Y));
         gdi.XorDrawLine(window.CreateGraphics(), p1, new Point(p1.X, p2.Y));
         gdi.XorDrawLine(window.CreateGraphics(), new Point(p1.X, p2.Y), p2);
         gdi.XorDrawLine(window.CreateGraphics(), new Point(p2.X, p1.Y), p2);
         tr.Dispose();
      }

      public void ChangePosition()
      {
         this.TextPosition = this.InverseTransform(LastMousePos);
         this.StartPoint = this.TextPosition;
         this.EndPoint = new DataPoint(this.TextPosition.X, this.TextPosition.Y - 60);
      }

      public void DetectActiveThumb(MouseEventArgs e)
      {
      }

      public void SetBeingEdited(bool beingEdited)
      {
         this.BeingEdited = beingEdited;
         if (beingEdited)
         {
            // save toolbar settings and load with that of the annotation
            MainWindow mainWindow = (MainWindow.mainWindow as MainWindow);
            this.SystemDrawingColor = mainWindow.DrawingColor;
            mainWindow.DrawingColor = this.TextColor;
         }
      }

      public void UnSetHighlighted()
      {
         this.IsHighLighted = false;
      }

      public void RestoreSystemTools()
      {
         // restore system tools
         MainWindow mainWindow = (MainWindow.mainWindow as MainWindow);
         mainWindow.DrawingColor = this.SystemDrawingColor;
      }

      public bool GetBeingEdited()
      {
         return this.BeingEdited;
      }

      public bool GetHighLighted()
      {
         return this.IsHighLighted;
      }

      public void SetHighlighted(Control window, GDI32 gdi, MouseEventArgs e, bool highlight)
      {
         // skip when there is nothing to do
         if (highlight && this.IsHighLighted)
            // already highlighted
            return;
         if (this.BeingEdited) return;
         if (!highlight && !this.IsHighLighted)
            // already unselected
            return;
         this.IsHighLighted = highlight;
         // set/unset highlighting
         //this.GetParametersFromAnnotation(window);
         this.Window = window;
         this.Gdi = gdi;
         this.mouseEventArgs = e;

         NativeTextRenderer tr = new NativeTextRenderer(window.CreateGraphics());
         this.font = new Font(MainWindow.fontName, MainWindow.fontSize);
         this.size = tr.MeasureString(MainWindow.NewAnnotationText, font);
         ScreenPoint sp = this.Transform(this.TextPosition);
         tr.XorDrawString(MainWindow.NewAnnotationText, font, new Point((int)sp.X - size.Width / 2, (int)sp.Y - size.Height));
         Point p1 = new Point((int)sp.X - size.Width / 2, (int)sp.Y - size.Height - 4);
         Point p2 = new Point((int)sp.X + size.Width / 2, (int)sp.Y + 4);
         // xor draw the rectangle
         gdi.XorDrawLine(window.CreateGraphics(), p1, new Point(p2.X, p1.Y));
         gdi.XorDrawLine(window.CreateGraphics(), p1, new Point(p1.X, p2.Y));
         gdi.XorDrawLine(window.CreateGraphics(), new Point(p1.X, p2.Y), p2);
         gdi.XorDrawLine(window.CreateGraphics(), new Point(p2.X, p1.Y), p2);
         tr.Dispose();
      }

      public void MoveRubberband(Control window, GDI32 gdi, PlotModel model, int deltaX, int deltaY, Point mouseDownPoint, MouseEventArgs e)
      {
         this.LastMousePos = new ScreenPoint(e.X, e.Y);

         NativeTextRenderer tr = new NativeTextRenderer(window.CreateGraphics());
         font = new Font(MainWindow.fontName, MainWindow.fontSize);
         size = tr.MeasureString(MainWindow.NewAnnotationText, font);

         // remove the old
         // remove the text
         tr.XorDrawString(MainWindow.NewAnnotationText, font, new Point(oldMousePoint.X - size.Width / 2, oldMousePoint.Y - size.Height));
         Point p1 = new Point((int)oldMousePoint.X - size.Width / 2, (int)oldMousePoint.Y - size.Height - 4);
         Point p2 = new Point((int)oldMousePoint.X + size.Width / 2, (int)oldMousePoint.Y + 4);
         // remove the rectangle:
         gdi.XorDrawLine(window.CreateGraphics(), p1, new Point(p2.X, p1.Y));
         gdi.XorDrawLine(window.CreateGraphics(), p1, new Point(p1.X, p2.Y));
         gdi.XorDrawLine(window.CreateGraphics(), new Point(p1.X, p2.Y), p2);
         gdi.XorDrawLine(window.CreateGraphics(), new Point(p2.X, p1.Y), p2);
         p1 = new Point((int)e.X - size.Width / 2, (int)e.Y - size.Height - 4);
         p2 = new Point((int)e.X + size.Width / 2, (int)e.Y + 4);

         // show the new position:
         // show the text
         tr.XorDrawString(MainWindow.NewAnnotationText, font, new Point(e.X - size.Width / 2, e.Y - size.Height));
         //  the rectangle
         gdi.XorDrawLine(window.CreateGraphics(), p1, new Point(p2.X, p1.Y));
         gdi.XorDrawLine(window.CreateGraphics(), p1, new Point(p1.X, p2.Y));
         gdi.XorDrawLine(window.CreateGraphics(), new Point(p1.X, p2.Y), p2);
         gdi.XorDrawLine(window.CreateGraphics(), new Point(p2.X, p1.Y), p2);
         tr.Dispose();
         oldMousePoint = new Point(e.X, e.Y);
      }

      public void Add(System.Windows.Forms.Control control, GDI32 gdi, int deltaX, int deltaY)
      {
         Point p1 = ToPoint(this.StartPoint);
         Point p2 = ToPoint(this.EndPoint);
         gdi.XorDrawLine(control.CreateGraphics(), new Point(p1.X + deltaX, p1.Y + deltaY), new Point(p2.X + deltaX, p2.Y + deltaY));
         this.StartPoint = InverseTransform(new ScreenPoint(p1.X + deltaX, p1.Y + deltaY));
         this.EndPoint = InverseTransform(new ScreenPoint(p2.X + deltaX, p2.Y + deltaY));
      }

      private Point ToPoint(DataPoint p)
      {
         ScreenPoint ps = XAxis.Transform(p.X, p.Y, YAxis);
         return new Point((int)ps.X, (int)ps.Y);
      }

      /// <summary>
      /// Gets or sets the arrow direction.
      /// </summary>
      /// <remarks>Setting this property overrides the <see cref="StartPoint" /> property.</remarks>
      public ScreenVector ArrowDirection { get; set; }

      /// <summary>
      /// Gets or sets the color of the arrow.
      /// </summary>
      public OxyColor Color { get; set; }

      /// <summary>
      /// Gets or sets the end point of the arrow, defaults to 60.
      /// </summary>
      public DataPoint EndPoint {
         get
         {
            return endPoint;
         }
         set
         {
            endPoint = value;
         }
      }

      public string Label { get; set; }

      /// <summary>
      /// Gets or sets the length of the head (relative to the stroke thickness) (the default value is 10).
      /// </summary>
      /// <value>The length of the head.</value>
      public double HeadLength { get; set; }

      /// <summary>
      /// Gets or sets the width of the head (relative to the stroke thickness) (the default value is 3).
      /// </summary>
      /// <value>The width of the head.</value>
      public double HeadWidth { get; set; }

      /// <summary>
      /// Gets or sets the line join type.
      /// </summary>
      /// <value>The line join type.</value>
      public OxyPenLineJoin LineJoin { get; set; }

      /// <summary>
      /// Gets or sets the line style.
      /// </summary>
      /// <value>The line style.</value>
      public LineStyle LineStyle { get; set; }

      /// <summary>
      /// Gets or sets the start point of the arrow.
      /// </summary>
      /// <remarks>This property is overridden by the ArrowDirection property, if set.</remarks>
      public DataPoint StartPoint {
         get
         {
            return this.startPoint;
         }
         set
         {
            this.startPoint = value;
            this.endPoint = new DataPoint(this.startPoint.X, this.startPoint.Y - 100);
         }
      }

      /// <summary>
      /// Gets or sets the stroke thickness (the default value is 2).
      /// </summary>
      /// <value>The stroke thickness.</value>
      public double StrokeThickness { get; set; }

      /// <summary>
      /// Gets or sets the 'veeness' of the arrow head (relative to thickness) (the default value is 0).
      /// </summary>
      /// <value>The 'veeness'.</value>
      public double Veeness { get; set; }

      /// <summary>
      /// Renders the LDT annotation.
      /// </summary>
      /// <param name="rc">The render context.</param>
      /// <param name="model">The plot model.</param>
      public override void Render(IRenderContext rc, PlotModel model)
      {
         base.Render(rc, model);

         this.screenEndPoint = this.Transform(this.EndPoint);

         if (this.ArrowDirection.LengthSquared > 0)
         {
            this.screenStartPoint = this.screenEndPoint - this.ArrowDirection;
         }
         else
         {
            this.screenStartPoint = this.Transform(this.StartPoint);
         }

         var d = this.screenEndPoint - this.screenStartPoint;
         d.Normalize();
         var n = new ScreenVector(d.Y, -d.X);

         var p1 = this.screenEndPoint - (d * this.HeadLength * this.StrokeThickness);
         var p2 = p1 + (n * this.HeadWidth * this.StrokeThickness);
         var p3 = p1 - (n * this.HeadWidth * this.StrokeThickness);
         var p4 = p1 + (d * this.Veeness * this.StrokeThickness);

         var clippingRectangle = this.GetClippingRect();
         const double MinimumSegmentLength = 4;

         var dashArray = this.LineStyle.GetDashArray();

         rc.DrawClippedLine(
             clippingRectangle,
             new[] { this.screenStartPoint, p4 },
             MinimumSegmentLength * MinimumSegmentLength,
             this.GetSelectableColor(this.Color),
             this.StrokeThickness,
             dashArray,
             this.LineJoin,
             false);

         rc.DrawClippedPolygon(
             clippingRectangle,
             new[] { p3, this.screenEndPoint, p2, p4 },
             MinimumSegmentLength * MinimumSegmentLength,
             this.GetSelectableColor(this.Color),
             OxyColors.Undefined);

         if (!string.IsNullOrEmpty(this.Text))
         {
            var ha = this.TextHorizontalAlignment;
            var va = VerticalAlignment.Bottom;
            var textPoint = this.GetActualTextPosition(() => this.screenStartPoint);
            var ldtPoit = new ScreenPoint(textPoint.X, textPoint.Y - this.ActualFontSize - 2);
            rc.DrawClippedText(
                clippingRectangle,
                ldtPoit,
                this.Label,
                this.ActualTextColor,
                this.ActualFont,
                this.ActualFontSize,
                this.ActualFontWeight,
                this.TextRotation,
                ha,
                va);
            rc.DrawClippedText(
                clippingRectangle,
                textPoint,
                this.Text,
                this.ActualTextColor,
                this.ActualFont,
                this.ActualFontSize,
                this.ActualFontWeight,
                this.TextRotation,
                ha,
                va);
         }
      }

      /// <summary>
      /// When overridden in annotation derived class, tests if the plot element is hit by the specified point.
      /// </summary>
      /// <param name="args">The hit test arguments.</param>
      /// <returns>
      /// The result of the hit test.
      /// </returns>
      protected override HitTestResult HitTestOverride(HitTestArguments args)
      {
         if ((args.Point - this.screenStartPoint).Length < args.Tolerance)
         {
            return new HitTestResult(this, this.screenStartPoint, null, 1);
         }

         if ((args.Point - this.screenEndPoint).Length < args.Tolerance)
         {
            return new HitTestResult(this, this.screenEndPoint, null, 2);
         }

         var p = ScreenPointHelper.FindPointOnLine(args.Point, this.screenStartPoint, this.screenEndPoint);
         if ((p - args.Point).Length < args.Tolerance)
         {
            return new HitTestResult(this, p);
         }

         return null;
      }
   }
}