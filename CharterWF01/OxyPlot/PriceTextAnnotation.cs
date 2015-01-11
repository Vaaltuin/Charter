// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PriceTextAnnotation.cs" company="Wynand">
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
   using OxyPlot;
   using OxyPlot.Annotations;
   using System;
   using System.Drawing;
   using System.Windows.Forms;
   using WBGraphicUtils;

   /// <summary>
   /// TODO: This class provides
   /// </summary>
   public class PriceTextAnnotation : TextAnnotation, ChartPlotViewSupport
   {
      public ScreenPoint LastMousePos;

      //private bool Selected;
      private bool IsHighLighted;
      private Control Window;
      private GDI32 Gdi;
      private MouseEventArgs mouseEventArgs;
      private OxyPlot.PlotModel model;
      private Point oldMousePoint;
      private Font font;
      private Size size;
      private bool BeingEdited;
      private OxyColor SystemDrawingColor;

      /// <summary>
      /// Used in moving the text before an annotation is created
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
         //CreateMousePos = new ScreenPoint(e.X, e.Y);
      }

      public PriceTextAnnotation()
      {

      }
      public void UnSetHighlighted()
      {
         IsHighLighted = false;
      }

      public void RestoreSystemTools()
      {
         // restore system tools
         MainWindow mainWindow = (MainWindow.mainWindow as MainWindow);
         mainWindow.DrawingColor = this.SystemDrawingColor;
      }

      public PriceTextAnnotation(MainWindow mainWindow, PlotModel model, MouseEventArgs e, DataPoint p)
      {
         this.IsHighLighted = false;
         this.StrokeThickness = mainWindow.StrokeThickness;
         this.TextColor = MainWindow.drawingColor;
         this.model = model;
         this.Text = MainWindow.NewAnnotationText;
         this.TextPosition = new DataPoint(p.X, p.Y);
         this.FontSize = MainWindow.fontSize;
      }

      public void ChangePosition()
      {
         this.TextPosition = this.InverseTransform(LastMousePos);
      }

      public void DetectActiveThumb(MouseEventArgs e)
      {
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

      public void Attach(Control window, GDI32 gdi)
      {
         this.Window = window;
         this.Gdi = gdi;
      }

      private bool IsCloseTo(Point p1, Point p2, int tollerance)
      {
         return ((Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2)) < tollerance * tollerance);
      }
   }
}
