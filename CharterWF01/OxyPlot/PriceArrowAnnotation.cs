// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PriceArrowAnnotation.cs" company="Wynand">
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
   public class PriceArrowAnnotation : ArrowAnnotation, ChartPlotViewSupport
   {
      public Thumb MovingStyle;
      public Thumb ActiveThumb;
      public Point LeftThumbPoint;
      private Point RightThumbPoint;
      private Point RubberBandLeftPoint;
      private Point RubberBandRightPoint;
      private DataPoint LeftDataPoint;
      private DataPoint RightDataPoint;
      private Point InitialRubberBandLeftPoint;
      private Point InitialRubberBandRightPoint;
      private bool RightThumbOnEndOfRubberBand;
      private bool LeftThumbOnStartOfRubberBand;
      private Control Window;
      private GDI32 Gdi;
      private bool IsHighLighted = false;
      private bool FirstMoveOnCreate = false;
      private bool Selected;
      private bool BeingEdited = false;
      private OxyPlot.LineStyle SystemLinestyle;
      private int SystemStrokeThickness;
      private OxyColor SystemDrawingColor;
      private MouseEventArgs mouseEventArgs;

      public PriceArrowAnnotation()
      {

      }

      public PriceArrowAnnotation(MainWindow mainWindow, PlotModel model, MouseEventArgs e, bool addText)
      {
         this.IsHighLighted = false;
         this.FirstMoveOnCreate = true;
         this.StrokeThickness = mainWindow.StrokeThickness;
         this.LineStyle = MainWindow.lineStyle;
         this.Color = MainWindow.drawingColor;
         if (addText)
            this.Text = MainWindow.NewAnnotationText;
         this.FontSize = MainWindow.fontSize;
      }

      public void SetBeingEdited(bool beingEdited)
      {
         MainWindow mainWindow = (MainWindow.mainWindow as MainWindow);
         if (this.BeingEdited == beingEdited)
            return;
         this.BeingEdited = beingEdited;
         if (beingEdited)
         {
            // save toolbar settings and load with that of the annotation
            this.SystemLinestyle = mainWindow.LineStyle;
            mainWindow.LineStyle = this.LineStyle;
            this.SystemStrokeThickness = mainWindow.StrokeThickness;
            mainWindow.StrokeThickness = (int)this.StrokeThickness;
            this.SystemDrawingColor = mainWindow.DrawingColor;
            mainWindow.DrawingColor = this.Color;
         }
         else
         {
            // remove highlight
            if (this.IsHighLighted)
            {
               // remove the highlighing
               this.DrawRubberBandAndThumbs(this.Window, this.Gdi, this.mouseEventArgs);
            }
         }
      }

      public bool GetBeingEdited()
      {
         return this.Selected;
      }
      public void UnSetHighlighted()
      {
         IsHighLighted = false;
      }

      public void RestoreSystemTools()
      {

      }

      public void DrawRubberBand(System.Windows.Forms.Control window, GDI32 gdi, int deltaX, int deltaY, MouseEventArgs e)
      {
         Point newRubberBandLeftPoint = new Point(0, 0);
         Point newRubberBandRightPoint = new Point(0, 0);
         Point mousePos = new Point(e.X, e.Y);

         var isShift = System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.LeftShift) || System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.RightShift);
         var isCtrl = System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.LeftCtrl) || System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.RightCtrl);
         var isAlt = System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.LeftAlt) || System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.RightAlt);

         // remove the old rubberband by xor redrawing it.
         gdi.XorDrawLine(window.CreateGraphics(), RubberBandLeftPoint, RubberBandRightPoint);

         double screenSlope = (double)(mousePos.Y - this.LeftDataPoint.Y) / (double)(mousePos.X - this.LeftDataPoint.X);

         if (this.ActiveThumb == Thumb.Centre)
         #region Centre Rubberband
         {
            newRubberBandLeftPoint = new Point(InitialRubberBandLeftPoint.X + deltaX, InitialRubberBandLeftPoint.Y + deltaY);
            newRubberBandRightPoint = new Point(InitialRubberBandRightPoint.X + deltaX, InitialRubberBandRightPoint.Y + deltaY);
         }
         #endregion
         else if (this.ActiveThumb == Thumb.P1)
         #region P1 Rubberband
         {
            this.RightDataPoint = new DataPoint(this.RightThumbPoint.X, this.RightThumbPoint.Y);

            if (isCtrl)
            {
               // P1 is constrained to either vertical or horizontal
               if (Math.Abs(mousePos.X - this.LeftDataPoint.X) > Math.Abs(this.LeftDataPoint.Y - mousePos.Y))
               {
                  // the line is horizontal
                  newRubberBandLeftPoint = new Point(mousePos.X, (int)this.LeftDataPoint.Y);
                  newRubberBandRightPoint = new Point(window.Width, mousePos.Y);
               }
               else
               {
                  // the line is vertical
                  newRubberBandLeftPoint = new Point((int)this.LeftDataPoint.X, mousePos.Y);
                  newRubberBandRightPoint = new Point(mousePos.X, window.Height);
               }
            }
            else
            {
               // Point to point
               newRubberBandLeftPoint = new Point(mousePos.X, mousePos.Y);
            }
            newRubberBandRightPoint = new Point((int)this.RightDataPoint.X, (int)RightDataPoint.Y);
         }
         #endregion
         else if (this.ActiveThumb == Thumb.P2)
         #region P2 Rubberband
         {
            this.LeftDataPoint = new DataPoint(this.LeftThumbPoint.X, this.LeftThumbPoint.Y);
            if (isCtrl)
            {
               // the line is constrained to either vertical or horizontal
               if (Math.Abs(mousePos.X - this.LeftDataPoint.X) > Math.Abs(this.LeftDataPoint.Y - mousePos.Y))
               {
                  // the line is horizontal
                  newRubberBandRightPoint = new Point(mousePos.X, (int)this.LeftDataPoint.Y);
               }
               else
               {
                  // the line is vertical
                  newRubberBandRightPoint = new Point((int)this.LeftDataPoint.X, mousePos.Y);
               }

            }
            else
            {
               // Point to point
               newRubberBandRightPoint = new Point(mousePos.X, mousePos.Y);
            }
            newRubberBandLeftPoint = new Point((int)this.LeftDataPoint.X, (int)LeftDataPoint.Y);
         }
         #endregion

         // draw the newly calculated rubberband
         gdi.XorDrawLine(window.CreateGraphics(), newRubberBandLeftPoint, newRubberBandRightPoint);
         RubberBandLeftPoint = newRubberBandLeftPoint;
         RubberBandRightPoint = newRubberBandRightPoint;
      }

      public void MoveRubberband(Control window, GDI32 gdi, PlotModel model, int deltaX, int deltaY, Point mouseDownPoint, MouseEventArgs e)
      {
         if (this.FirstMoveOnCreate)
         {
            this.FirstMoveOnCreate = false;
            this.ActiveThumb = Thumb.P2;
            this.LeftThumbPoint = mouseDownPoint;
            double dpX = model.DefaultXAxis.InverseTransform(e.X) - model.DefaultXAxis.InverseTransform(mouseDownPoint.X);
            double dpY = model.DefaultYAxis.InverseTransform(e.Y) - model.DefaultYAxis.InverseTransform(mouseDownPoint.Y);
         }
         this.DrawRubberBand(window, gdi, deltaX, deltaY, e);

      }

      public void SetHighlighted(Control window, GDI32 gdi, MouseEventArgs e, bool select)
      {
         // skip when there is nothing to so
         if (select && this.IsHighLighted)
            // already highlighted
            return;
         if (!select && !this.IsHighLighted)
            // already unselected
            return;
         this.IsHighLighted = select;
         // set/unset highlighting
         this.GetParametersFromAnnotation(window);
         this.DrawRubberBandAndThumbs(window, gdi, e);
         this.mouseEventArgs = e;
         this.Window = window;
         this.Gdi = gdi;
      }
      public bool GetHighLighted()
      {
         return this.IsHighLighted;
      }
      public void Attach(Control window, GDI32 gdi)
      {
         this.Window = window;
         this.Gdi = gdi;
      }

      public void DrawRubberBandAndThumbs(System.Windows.Forms.Control window, GDI32 gdi, MouseEventArgs e)
      {
         // The rubberband is drawn from RubberBandLeftPoint to RubberBandRightPoint
         // There are two thumbs per rubberband, one being the anchor point
         // Any side of the LineAnnotation may extend off the window
         // and the annotation may leave the window either on the horizontal or vertical axis.
         // When the line end is not on the window the thumb is drawn 1/4 the way inside the window.

         double screenSlope;
         int xSpan;
         int thumbOffset;

         xSpan = this.InitialRubberBandRightPoint.X - this.InitialRubberBandLeftPoint.X;
         thumbOffset = xSpan / 4;

         if (this.LeftThumbOnStartOfRubberBand)
         {
            this.LeftThumbPoint = this.InitialRubberBandLeftPoint;
         }
         else
         {
            this.LeftThumbPoint.X = this.InitialRubberBandLeftPoint.X + thumbOffset;
            screenSlope = (double)(this.InitialRubberBandRightPoint.Y - this.InitialRubberBandLeftPoint.Y) / (double)(this.InitialRubberBandRightPoint.X - this.InitialRubberBandLeftPoint.X);
            this.LeftThumbPoint.Y = (int)(this.InitialRubberBandLeftPoint.Y + screenSlope * thumbOffset);
         }

         if (this.RightThumbOnEndOfRubberBand)
            this.RightThumbPoint = this.InitialRubberBandRightPoint;
         else
         {
            this.RightThumbPoint.X = this.InitialRubberBandRightPoint.X - thumbOffset;
            screenSlope = (double)(this.InitialRubberBandRightPoint.Y - this.InitialRubberBandLeftPoint.Y) / (double)(this.InitialRubberBandRightPoint.X - this.InitialRubberBandLeftPoint.X);
            this.RightThumbPoint.Y = (int)(InitialRubberBandRightPoint.Y - screenSlope * thumbOffset);
         }

         gdi.DrawRectangle(window.CreateGraphics(), new Point(this.LeftThumbPoint.X - 5, this.LeftThumbPoint.Y - 5), new Point(this.LeftThumbPoint.X + 5, this.LeftThumbPoint.Y + 5));
         gdi.DrawRectangle(window.CreateGraphics(), new Point(RightThumbPoint.X - 5, this.RightThumbPoint.Y - 5), new Point(RightThumbPoint.X + 5, this.RightThumbPoint.Y + 5));
         gdi.XorDrawLine(window.CreateGraphics(), this.InitialRubberBandLeftPoint, this.InitialRubberBandRightPoint);

         if (IsCloseTo(new Point(e.X, e.Y), this.LeftThumbPoint, 15))
         {
            this.ActiveThumb = Thumb.P1;
            this.LeftDataPoint = new DataPoint(this.RightThumbPoint.X, this.RightThumbPoint.Y);
         }
         else if (IsCloseTo(new Point(e.X, e.Y), this.RightThumbPoint, 15))
         {
            this.ActiveThumb = Thumb.P2;
            this.LeftDataPoint = new DataPoint(this.LeftThumbPoint.X, this.RightThumbPoint.Y);
         }
         else
            this.ActiveThumb = Thumb.Centre;
         return;
         #region LinearInfinite
         #endregion
      }

      public void DetectActiveThumb(MouseEventArgs e)
      {
         if (IsCloseTo(new Point(e.X, e.Y), this.LeftThumbPoint, 15))
         {
            this.ActiveThumb = Thumb.P1;
            this.LeftDataPoint = new DataPoint(this.RightThumbPoint.X, this.RightThumbPoint.Y);
         }
         else if (IsCloseTo(new Point(e.X, e.Y), this.RightThumbPoint, 15))
         {
            this.ActiveThumb = Thumb.P2;
            this.LeftDataPoint = new DataPoint(this.LeftThumbPoint.X, this.RightThumbPoint.Y);
         }
         else
            this.ActiveThumb = Thumb.Centre;
      }

      private bool IsCloseTo(Point p1, Point p2, int tollerance)
      {
         return ((Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2)) < tollerance * tollerance);
      }

      public void ChangePosition()
      {
         this.Selected = false;
         this.StartPoint = this.InverseTransform(new ScreenPoint(this.RubberBandLeftPoint.X, this.RubberBandLeftPoint.Y));
         this.EndPoint = this.InverseTransform(new ScreenPoint(this.RubberBandRightPoint.X, this.RubberBandRightPoint.Y));
      }
      public void GetParametersFromAnnotation(Control window)
      {
         this.InitialRubberBandLeftPoint.X = (int)this.XAxis.Transform(this.StartPoint.X);
         this.InitialRubberBandLeftPoint.Y = (int)this.YAxis.Transform(this.StartPoint.Y);
         this.InitialRubberBandRightPoint.X = (int)this.XAxis.Transform(this.EndPoint.X);
         this.InitialRubberBandRightPoint.Y = (int)this.YAxis.Transform(this.EndPoint.Y);
         this.RightThumbOnEndOfRubberBand = true;
         this.LeftThumbOnStartOfRubberBand = true;
      }

      public Thumb GetMoveStyle(MouseEventArgs e)
      {
         if (IsClose(new Point(e.X, e.Y), Transform(StartPoint), 10))
         {
            return Thumb.P1;
         }
         else if (IsClose(new Point(e.X, e.Y), Transform(EndPoint), 10))
         {
            return Thumb.P2;
         }
         else
            return Thumb.Centre;
      }

      public void DrawSelectors(System.Windows.Forms.Control control, GDI32 gdi, ref Point mp1, ref Point mp2)
      {
         if ((mp1.X == 0) && (mp1.Y == 0))
         {
            mp1 = ToPoint(StartPoint);
            gdi.DrawRectangle(control.CreateGraphics(), new Point(mp1.X - 5, mp1.Y - 5), new Point(mp1.X + 5, mp1.Y + 5));
         }
         if ((mp2.X == 0) && (mp2.Y == 0))
         {
            mp2 = ToPoint(this.EndPoint);
            gdi.DrawRectangle(control.CreateGraphics(), new Point(mp2.X - 5, mp2.Y - 5), new Point(mp2.X + 5, mp2.Y + 5));
         }
         gdi.XorDrawLine(control.CreateGraphics(), ToPoint(StartPoint), ToPoint(EndPoint));
      }

      public void ChangePosition(System.Windows.Forms.Control control, GDI32 gdi, int deltaX, int deltaY, MouseEventArgs e)
      {
         Point aaP1;
         Point aaP2;

         if (MovingStyle == Thumb.Centre)
         {
            aaP1 = ToPoint(StartPoint);
            aaP2 = ToPoint(EndPoint);
            gdi.XorDrawLine(control.CreateGraphics(), new Point(aaP1.X + deltaX, aaP1.Y + deltaY), new Point(aaP2.X + deltaX, aaP2.Y + deltaY));
            StartPoint = InverseTransform(new ScreenPoint(aaP1.X + deltaX, aaP1.Y + deltaY));
            EndPoint = InverseTransform(new ScreenPoint(aaP2.X + deltaX, aaP2.Y + deltaY));

         }
         else if (MovingStyle == Thumb.P1)
         {
            gdi.XorDrawLine(control.CreateGraphics(), new Point(e.X, e.Y), ToPoint(EndPoint));
            StartPoint = InverseTransform(new ScreenPoint(e.X, e.Y));
         }
         else if (MovingStyle == Thumb.P2)
         {
            gdi.XorDrawLine(control.CreateGraphics(), ToPoint(StartPoint), new Point(e.X, e.Y));
            EndPoint = InverseTransform(new ScreenPoint(e.X, e.Y));
         }
      }

      public static void MoveNew(System.Windows.Forms.Control control, GDI32 gdi, Point mouseDownPoint, Point oldMousePoint, Point ep)
      {

         gdi.XorDrawLine(control.CreateGraphics(), mouseDownPoint, ep);
         gdi.XorDrawLine(control.CreateGraphics(), mouseDownPoint, oldMousePoint);

      }

      private Point ToPoint(DataPoint p)
      {
         ScreenPoint ps = XAxis.Transform(p.X, p.Y, YAxis);
         return new Point((int)ps.X, (int)ps.Y);
      }

      private bool IsClose(Point p1, ScreenPoint p2, int tollerance)
      {
         return ((Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2)) < tollerance * tollerance);
      }

   }
}
