// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PriceLineAnnotation.cs" company="Wynand">
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
   public class PriceLineAnnotation : LineAnnotation, ChartPlotViewSupport
   {
      const double MySqlMin = -1.7976931348623E+308;
      const double MySqlMax = 1.7976931348623E+308;

      public Thumb ActiveThumb;
      public LineType LineType;
      public Point LeftThumbPoint;

      private DataPoint LeftDataPoint;
      private DataPoint RightDataPoint;
      private Point RubberBandLeftPoint;
      private Point RubberBandRightPoint;
      private Point InitialRubberBandLeftPoint;
      private Point InitialRubberBandRightPoint;
      private Point RightThumbPoint;
      private bool LeftThumbOnStartOfRubberBand;
      private bool RightThumbOnEndOfRubberBand;
      private Control Window;
      private GDI32 Gdi;
      private bool IsHighLighted = false;
      private bool BeingEdited = false;
      private bool Creating;
      private MouseEventArgs mouseEventArgs;
      private PlotModel model;
      private OxyPlot.LineStyle SystemLinestyle;
      private int SystemStrokeThickness;
      private OxyColor SystemDrawingColor;

      public PriceLineAnnotation()
      {

      }
      /// <summary>
      /// Creates annotation new horizontal line annotation, sized to just the mouse down spot,
      /// </summary>
      /// <param name="MainWindow"></param>
      /// <param name="model"></param>
      /// <param name="mouseEventArgs"></param>
      public PriceLineAnnotation(MainWindow mainWindow, PlotModel model, MouseEventArgs e, bool addText)
      {
         this.IsHighLighted = false;
         this.Creating = true;
         this.Type = LineAnnotationType.Horizontal;
         this.Y = model.DefaultYAxis.InverseTransform(e.Y);
         this.MinimumX = model.DefaultXAxis.InverseTransform(e.X);
         this.MaximumX = model.DefaultXAxis.InverseTransform(e.X);
         this.StrokeThickness = mainWindow.StrokeThickness;
         this.LineStyle = MainWindow.lineStyle;
         this.Color = MainWindow.drawingColor;
         this.model = model;
         if (addText)
            this.Text = MainWindow.NewAnnotationText;
         this.FontSize = MainWindow.fontSize;
      }

      public void UnSetHighlighted()
      {
        IsHighLighted = false;
      }

      public void MoveRubberband(Control window, GDI32 gdi, PlotModel model, int deltaX, int deltaY, Point mouseDownPoint, MouseEventArgs e)
      {
         if (this.Creating)
         {
            this.Creating = false;
            this.ActiveThumb = Thumb.P2;
            this.LeftThumbPoint = mouseDownPoint;
            double dpX = model.DefaultXAxis.InverseTransform(e.X) - model.DefaultXAxis.InverseTransform(mouseDownPoint.X);
            double dpY = model.DefaultYAxis.InverseTransform(e.Y) - model.DefaultYAxis.InverseTransform(mouseDownPoint.Y);
            this.Slope = dpY / dpX;
            this.Intercept = model.DefaultYAxis.InverseTransform(mouseDownPoint.Y) - this.Slope * model.DefaultXAxis.InverseTransform(mouseDownPoint.X);
         }
         this.DrawRubberBand(window, gdi, deltaX, deltaY, e);
      }

      public void DrawRubberBand(Control window, GDI32 gdi, int deltaX, int deltaY, MouseEventArgs e)
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
            this.LeftDataPoint = new DataPoint(this.RightThumbPoint.X, this.RightThumbPoint.Y);

            if (isShift)
            #region P1Shift
            {
               // sides of P1 line are extended to infinity
               if (isCtrl)
               {
                  // P1 is constrained to either vertical or horizontal
                  if (Math.Abs(mousePos.X - this.LeftDataPoint.X) > Math.Abs(this.LeftDataPoint.Y - mousePos.Y))
                  {
                     // the line is horizontal
                     newRubberBandLeftPoint = new Point(0, mousePos.Y);
                     newRubberBandRightPoint = new Point(window.Width, mousePos.Y);
                     this.LineType = LineType.HorizontalInfinite;
                  }
                  else
                  {
                     // the line is vertical
                     newRubberBandLeftPoint = new Point(mousePos.X, 0);
                     newRubberBandRightPoint = new Point(mousePos.X, window.Height);
                     this.LineType = LineType.VerticalInfinite;
                  }
               }
               else
               {
                  // both sides are extended to infinity
                  newRubberBandRightPoint = new Point(window.Width, mousePos.Y + (int)((double)(window.Width - mousePos.X) * screenSlope));
                  newRubberBandLeftPoint = new Point(0, (int)this.LeftDataPoint.Y - (int)((double)(this.LeftDataPoint.X) * screenSlope));
                  this.LineType = LineType.LinearInfinite;
               }
            }
            #endregion
            else if (isAlt)
            #region P1 Alt
            {
               if (isCtrl)
               {
                  // P1 is constrained to either vertical or horizontal
                  if (Math.Abs(mousePos.X - (int)this.LeftDataPoint.X) > Math.Abs((int)this.LeftDataPoint.Y - mousePos.Y))
                  {
                     // P1 line is horizontal: It may go either to left or right
                     if (mousePos.X < this.LeftDataPoint.X)
                     {
                        // P1 line goes horizontally to the left
                        newRubberBandLeftPoint = new Point(0, mousePos.Y);
                        newRubberBandRightPoint = new Point(mousePos.X, mousePos.Y);
                        this.LineType = LineType.HorizontalFromPointToLeft;
                     }
                     else
                     {
                        // P1 line goes horizontally to the right
                        newRubberBandRightPoint = new Point(window.Width, mousePos.Y);
                        newRubberBandLeftPoint = new Point(mousePos.X, mousePos.Y);
                        this.LineType = LineType.HorizontalFromPointToRight;
                     }
                  }
                  else
                  {
                     // P1 is vertical: it may either go up or down
                     if (mousePos.Y < this.LeftDataPoint.Y)
                     {
                        if (mousePos.Y < this.LeftDataPoint.Y)
                        {
                           // P1 line goes vertically up
                           newRubberBandLeftPoint = new Point(mousePos.X, mousePos.Y);
                           newRubberBandRightPoint = new Point(mousePos.X, 0);
                           this.LineType = LineType.VerticalPointToUp;
                        }
                        else
                        {
                           // P1 line goes vertically
                           newRubberBandLeftPoint = new Point(mousePos.X, mousePos.Y);
                           newRubberBandRightPoint = new Point(mousePos.X, window.Height);
                           this.LineType = LineType.VerticalPointToDown;
                        }
                     }
                  }
               }
               else
               {
                  newRubberBandRightPoint = this.RightThumbPoint;
                  // extend the mouse point to either left or right
                  if (mousePos.X > this.LeftDataPoint.X)
                  {
                     //  extend the P1 line to the right
                     newRubberBandLeftPoint = new Point(window.Width, mousePos.Y + (int)((double)(window.Width - mousePos.X) * screenSlope));
                     this.LineType = LineType.LinearFromPointToRight;
                  }
                  else
                  {
                     //  extend the P1 line to the left
                     newRubberBandLeftPoint = new Point(0, (int)this.LeftDataPoint.Y - (int)((double)((int)this.LeftDataPoint.X) * screenSlope));
                     this.LineType = LineType.LinearFromPointToLeft;
                  }
               }
            }
            #endregion
            else
            #region P1 Point to Point
            {
               if (isCtrl)
               {
                  // the line is constrained to either vertical or horizontal
                  if (Math.Abs(mousePos.X - (int)this.LeftDataPoint.X) > Math.Abs((int)this.LeftDataPoint.Y - mousePos.Y))
                  {
                     // the P1 line is horizontal
                     newRubberBandLeftPoint = new Point(mousePos.X, (int)this.LeftDataPoint.Y);
                     this.LineType = LineType.HorizontalPointToPoint;
                  }
                  else
                  {
                     // the P1 line is vertical
                     newRubberBandLeftPoint = new Point((int)this.LeftDataPoint.X, mousePos.Y);
                     this.LineType = LineType.VerticalPointToPoint;
                  }
               }
               else
               {
                  // The P1 line is point to point
                  newRubberBandLeftPoint = new Point(mousePos.X, mousePos.Y);
                  this.LineType = LineType.LinearPointToPoint;
               }
               newRubberBandRightPoint = new Point((int)this.LeftDataPoint.X, (int)this.LeftDataPoint.Y);
            }
            #endregion
         }
         #endregion
         else if (this.ActiveThumb == Thumb.P2)
         #region P2 Rubberband
         {
            this.LeftDataPoint = new DataPoint(this.LeftThumbPoint.X, this.LeftThumbPoint.Y);
            if (isShift)
            #region P2 Shift Rubberbandings
            {
               if (isShift)
               #region P2Shift
               {
                  // The sides of the P2 are extended to infinity
                  if (isCtrl)
                  {
                     // P2 line is constrained to either vertical or horizontal
                     if (Math.Abs(mousePos.X - this.LeftDataPoint.X) > Math.Abs(mousePos.Y - this.LeftDataPoint.Y))
                     {
                        // The P2 line is horizontal
                        newRubberBandLeftPoint = new Point(0, mousePos.Y);
                        newRubberBandRightPoint = new Point(window.Width, mousePos.Y);
                        this.LineType = LineType.HorizontalInfinite;
                     }
                     else
                     {
                        // The P2 line line is vertical
                        newRubberBandLeftPoint = new Point(mousePos.X, 0);
                        newRubberBandRightPoint = new Point(mousePos.X, window.Height);
                        this.LineType = LineType.VerticalInfinite;
                     }
                  }
                  else
                  {
                     // both sides of the P2 line are extended to infinity
                     newRubberBandLeftPoint = new Point(0, (int)this.LeftDataPoint.Y - (int)((double)(this.LeftDataPoint.X) * screenSlope));
                     newRubberBandRightPoint = new Point(window.Width, mousePos.Y + (int)((double)(window.Width - mousePos.X) * screenSlope));
                     this.LineType = LineType.LinearInfinite;
                  }
               }
               #endregion
            }
            #endregion
            else if (isAlt)
            #region P2 Alt Rubberbanding
            {
               if (isCtrl)
               {
                  // P2 is constrained to either vertical or horizontal
                  if (Math.Abs(mousePos.X - this.LeftDataPoint.X) > Math.Abs(this.LeftDataPoint.Y - mousePos.Y))
                  {
                     // P2 position is horizontally constrained: It may go either to left or right
                     if (mousePos.X < this.LeftDataPoint.X)
                     {
                        // P2 goes horizontally to the left
                        newRubberBandLeftPoint = new Point(0, mousePos.Y);
                        newRubberBandRightPoint = new Point(mousePos.X, mousePos.Y);
                        this.LineType = LineType.HorizontalFromPointToLeft;
                     }
                     else
                     {
                        // P2 goes horizontally to the right
                        newRubberBandRightPoint = new Point(window.Width, mousePos.Y);
                        newRubberBandLeftPoint = new Point(mousePos.X, mousePos.Y);
                        this.LineType = LineType.HorizontalFromPointToRight;
                     }
                  }
                  else
                  {
                     // P2 is vertically constrained: it may either go up or down
                     if (mousePos.Y < this.LeftDataPoint.Y)
                     {
                        // P2 goes vertically up
                        newRubberBandLeftPoint = new Point(mousePos.X, mousePos.Y);
                        newRubberBandRightPoint = new Point(mousePos.X, 0);
                        this.LineType = LineType.VerticalPointToUp;
                     }
                     else
                     {
                        // P2 goes vertically down
                        newRubberBandLeftPoint = new Point(mousePos.X, mousePos.Y);
                        newRubberBandRightPoint = new Point(mousePos.X, window.Height);
                        this.LineType = LineType.VerticalPointToDown;
                     }
                  }
               }
               else
               {
                  // the P2 line is extended from P1 to infinity
                  newRubberBandLeftPoint = this.LeftThumbPoint;
                  // extend the mouse point to either left or right
                  if (mousePos.X > this.LeftDataPoint.X)
                  {
                     //  extend to the right
                     newRubberBandRightPoint = new Point(window.Width, mousePos.Y + (int)((double)(window.Width - mousePos.X) * screenSlope));
                     this.LineType = LineType.LinearFromPointToRight;
                  }
                  else
                  {
                     //  extend to the left
                     newRubberBandRightPoint = new Point(0, mousePos.Y - (int)((double)(window.Width - mousePos.X) * screenSlope));
                     this.LineType = LineType.LinearFromPointToLeft;
                  }
               }

            }
            #endregion
            else
            #region P2 Free Rubberbanding
            {
               if (isCtrl)
               {
                  // the line is constrained to either vertical or horizontal
                  if (Math.Abs(mousePos.X - this.LeftDataPoint.X) > Math.Abs(this.LeftDataPoint.Y - mousePos.Y))
                  {
                     // the line is horizontal
                     newRubberBandRightPoint = new Point(mousePos.X, (int)this.LeftDataPoint.Y);
                     this.LineType = LineType.HorizontalPointToPoint;
                  }
                  else
                  {
                     // the line is vertical
                     newRubberBandRightPoint = new Point((int)this.LeftDataPoint.X, mousePos.Y);
                     this.LineType = LineType.VerticalPointToPoint;
                  }

               }
               else
               {
                  // Point to point
                  newRubberBandRightPoint = new Point(mousePos.X, mousePos.Y);
                  this.LineType = LineType.LinearPointToPoint;
               }
               newRubberBandLeftPoint = new Point((int)this.LeftDataPoint.X, (int)LeftDataPoint.Y);
            }
            #endregion
         }
         #endregion

         // draw the newly calculated rubberband
         gdi.XorDrawLine(window.CreateGraphics(), newRubberBandLeftPoint, newRubberBandRightPoint);
         RubberBandLeftPoint = newRubberBandLeftPoint;
         RubberBandRightPoint = newRubberBandRightPoint;
      }

      /// <summary>
      /// Sets LeftThumbPoint and RightThumbPoint and draws the thumbs on the RUbberband
      /// </summary>
      /// <param name="window"></param>
      /// <param name="gdi"></param>
      /// <param name="mp1"></param>
      /// <param name="mp2"></param>
      /// <param name="mouseEventArgs"></param>
      public void DrawRubberBandAndThumbs(Control window, GDI32 gdi, MouseEventArgs e)
      {
         // The rubberband is drawn from RubberBandLeftPoint to RubberBandRightPoint
         // There are two thumbs per rubberband, one being the anchor point
         // Any side of the LineAnnotation may extend off the window
         // and the annotation may leave the window either on the horizontal or vertical axis.
         // When the line end is not on the window the thumb is drawn 1/4 the way inside the window.

         double screenSlope;
         int xSpan;
         int thumbOffset;

         if (this.Type == LineAnnotationType.Vertical)
         {
            xSpan = this.InitialRubberBandRightPoint.Y - this.InitialRubberBandLeftPoint.Y;
            thumbOffset = xSpan / 4;

            if (this.LeftThumbOnStartOfRubberBand)
            {
               this.LeftThumbPoint = this.InitialRubberBandLeftPoint;
            }
            else
            {
               this.LeftThumbPoint.Y = this.InitialRubberBandLeftPoint.Y + thumbOffset;
               this.LeftThumbPoint.X = this.InitialRubberBandLeftPoint.X;
            }

            if (this.RightThumbOnEndOfRubberBand)
               this.RightThumbPoint = this.InitialRubberBandRightPoint;
            else
            {
               this.RightThumbPoint.Y = this.InitialRubberBandRightPoint.Y - thumbOffset;
               this.RightThumbPoint.X = this.InitialRubberBandRightPoint.X;
            }

            gdi.DrawRectangle(window.CreateGraphics(), new Point(this.LeftThumbPoint.X - 5, this.LeftThumbPoint.Y - 5), new Point(this.LeftThumbPoint.X + 5, this.LeftThumbPoint.Y + 5));
            gdi.DrawRectangle(window.CreateGraphics(), new Point(this.RightThumbPoint.X - 5, this.RightThumbPoint.Y - 5), new Point(this.RightThumbPoint.X + 5, this.RightThumbPoint.Y + 5));
            gdi.XorDrawLine(window.CreateGraphics(), this.InitialRubberBandLeftPoint, this.InitialRubberBandRightPoint);
         }
         else
         {
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

         }
         this.DetectActiveThumb(e);
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

      /// <summary>
      /// Changes the position of the PriceLineAnnotation, using the Rubberband positions
      /// </summary>
      public void ChangePosition()
      {
         var isShift = System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.LeftShift) || System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.RightShift);
         var isCtrl = System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.LeftCtrl) || System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.RightCtrl);
         var isAlt = System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.LeftAlt) || System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.RightAlt);
         this.LeftDataPoint = InverseTransform(new ScreenPoint(RubberBandLeftPoint.X, RubberBandLeftPoint.Y));
         this.RightDataPoint = InverseTransform(new ScreenPoint(RubberBandRightPoint.X, RubberBandRightPoint.Y));
         this.BeingEdited = false;

         switch (this.ActiveThumb)
         {
            case Thumb.Centre:
               #region ChangeCentre
               this.Intercept = this.LeftDataPoint.Y - (this.LeftDataPoint.X * this.Slope);
               switch (this.LineType)
               {
                  case LineType.HorizontalFromPointToRight:
                     this.Y = this.RightDataPoint.Y;
                     this.MinimumX = this.LeftDataPoint.X;
                     this.MaximumX = Double.MaxValue;
                     break;

                  case LineType.HorizontalFromPointToLeft:
                     this.Y = this.RightDataPoint.Y;
                     this.MaximumX = this.RightDataPoint.X;
                     this.MinimumX = Double.MinValue;
                     break;

                  case LineType.VerticalPointToPoint:
                     this.X = this.RightDataPoint.X;
                     this.MaximumY = this.LeftDataPoint.Y;
                     this.MinimumY = this.RightDataPoint.Y;
                     break;

                  case LineType.LinearPointToPoint:
                     this.MaximumX = this.RightDataPoint.X;
                     this.MinimumX = this.LeftDataPoint.X;
                     break;

                  case LineType.HorizontalPointToPoint:
                     this.Y = this.LeftDataPoint.Y;
                     this.MaximumX = this.RightDataPoint.X;
                     this.MinimumX = this.LeftDataPoint.X;
                     break;

                  case LineType.HorizontalInfinite:
                     this.Y = this.LeftDataPoint.Y;
                     this.MaximumX = Double.MaxValue;
                     this.MinimumX = Double.MinValue;
                     break;
               }
               break;

               #endregion
            case Thumb.P1:
               #region ChangeP1
               switch (this.LineType)
               {
                  case LineType.LinearFromPointToRight:
                     this.Type = LineAnnotationType.LinearEquation;
                     this.Slope = (this.RightDataPoint.Y - this.LeftDataPoint.Y) / (this.RightDataPoint.X - this.LeftDataPoint.X);
                     this.Intercept = this.LeftDataPoint.Y - this.Slope * this.LeftDataPoint.X;
                     this.MinimumX = this.LeftDataPoint.X;
                     this.MaximumX = Double.MaxValue;
                     break;

                  case LineType.LinearFromPointToLeft:
                     this.Type = LineAnnotationType.LinearEquation;
                     this.Slope = (this.RightDataPoint.Y - this.LeftDataPoint.Y) / (this.RightDataPoint.X - this.LeftDataPoint.X);
                     this.Intercept = this.LeftDataPoint.Y - this.Slope * this.LeftDataPoint.X;
                     this.MinimumX = Double.MinValue;
                     this.MaximumX = this.RightDataPoint.X;
                     break;

                  case LineType.HorizontalInfinite:
                     this.Type = LineAnnotationType.Horizontal;
                     this.Y = this.LeftDataPoint.Y;
                     this.MinimumX = Double.MinValue;
                     this.MaximumX = Double.MaxValue;
                     break;

                  case LineType.HorizontalFromPointToLeft:
                     this.Type = LineAnnotationType.Horizontal;
                     this.Y = this.LeftDataPoint.Y;
                     this.MinimumX = Double.MinValue;
                     this.MaximumX = this.RightDataPoint.X;
                     break;

                  case LineType.HorizontalFromPointToRight:
                     this.Type = LineAnnotationType.Horizontal;
                     this.Y = this.LeftDataPoint.Y;
                     this.MinimumX = LeftDataPoint.X;
                     this.MaximumX = Double.MaxValue;
                     break;

                  case LineType.HorizontalPointToPoint:
                     this.Type = LineAnnotationType.Horizontal;
                     this.Y = LeftDataPoint.Y;
                     this.MaximumX = this.RightDataPoint.X;
                     this.MinimumX = this.LeftDataPoint.X;
                     break;
                  case LineType.LinearInfinite:
                     this.Type = LineAnnotationType.LinearEquation;
                     this.Slope = (this.RightDataPoint.Y - this.LeftDataPoint.Y) / (this.RightDataPoint.X - this.LeftDataPoint.X);
                     this.Intercept = this.LeftDataPoint.Y - this.Slope * this.LeftDataPoint.X;
                     this.MinimumX = Double.MinValue;
                     this.MaximumX = Double.MaxValue;
                     break;

                  case LineType.LinearPointToPoint:
                     this.Type = LineAnnotationType.LinearEquation;
                     this.Slope = (this.RightDataPoint.Y - this.LeftDataPoint.Y) / (this.RightDataPoint.X - this.LeftDataPoint.X);
                     this.Intercept = this.LeftDataPoint.Y - this.Slope * this.LeftDataPoint.X;
                     this.MinimumX = this.LeftDataPoint.X;
                     this.MaximumX = this.RightDataPoint.X;
                     break;

                  case LineType.VerticalInfinite:
                     this.Type = LineAnnotationType.Vertical;
                     this.X = this.LeftDataPoint.X;
                     this.MinimumY = Double.MinValue;
                     this.MaximumY = Double.MaxValue;
                     break;

                  case LineType.VerticalPointToDown:
                     this.Type = LineAnnotationType.Vertical;
                     this.X = this.LeftDataPoint.X;
                     this.MinimumY = Double.MinValue;
                     this.MaximumY = this.LeftDataPoint.Y;
                     break;

                  case LineType.VerticalPointToPoint:
                     this.Type = LineAnnotationType.Vertical;
                     this.X = this.LeftDataPoint.X;
                     this.MinimumY = this.RightDataPoint.Y;
                     this.MaximumY = this.LeftDataPoint.Y;
                     break;

                  case LineType.VerticalPointToUp:
                     this.Type = LineAnnotationType.Vertical;
                     this.X = this.LeftDataPoint.X;
                     this.MinimumY = this.LeftDataPoint.Y;
                     this.MaximumY = Double.MaxValue;
                     break;
               }
               break;
               #endregion
            case Thumb.P2:
               #region ChangeP2
               switch (this.LineType)
               {
                  case LineType.LinearFromPointToLeft:
                     this.MinimumX = this.RightDataPoint.X;
                     this.MaximumX = Double.MaxValue;
                     this.Slope = (this.RightDataPoint.Y - this.LeftDataPoint.Y) / (this.RightDataPoint.X - this.LeftDataPoint.X);
                     this.Intercept = this.LeftDataPoint.Y - this.Slope * this.LeftDataPoint.X;
                     this.Type = LineAnnotationType.LinearEquation;
                     break;

                  case LineType.LinearFromPointToRight:
                     this.MinimumX = this.LeftDataPoint.X;
                     this.MaximumX = Double.MaxValue;
                     this.Slope = (this.RightDataPoint.Y - this.LeftDataPoint.Y) / (this.RightDataPoint.X - this.LeftDataPoint.X);
                     this.Intercept = this.LeftDataPoint.Y - this.Slope * this.LeftDataPoint.X;
                     this.Type = LineAnnotationType.LinearEquation;
                     break;

                  case LineType.LinearPointToPoint:
                     this.MinimumX = this.LeftDataPoint.X;
                     this.MaximumX = this.RightDataPoint.X;
                     this.Slope = (this.RightDataPoint.Y - this.LeftDataPoint.Y) / (this.RightDataPoint.X - this.LeftDataPoint.X);
                     this.Intercept = this.LeftDataPoint.Y - this.Slope * this.LeftDataPoint.X;
                     this.Type = LineAnnotationType.LinearEquation;
                     break;

                  case LineType.LinearInfinite:
                     this.MinimumX = Double.MinValue;
                     this.MaximumX = Double.MaxValue;
                     this.Slope = (this.RightDataPoint.Y - this.LeftDataPoint.Y) / (this.RightDataPoint.X - this.LeftDataPoint.X);
                     this.Intercept = this.LeftDataPoint.Y - this.Slope * this.LeftDataPoint.X;
                     this.Type = LineAnnotationType.LinearEquation;
                     break;

                  case LineType.HorizontalInfinite:
                     this.Y = this.LeftDataPoint.Y;
                     this.Type = LineAnnotationType.Horizontal;
                     this.MinimumX = Double.MinValue;
                     this.MaximumX = Double.MaxValue;
                     break;

                  case LineType.VerticalInfinite:
                     this.X = this.LeftDataPoint.X;
                     this.MinimumY = Double.MinValue;
                     this.MaximumY = Double.MaxValue;
                     this.Type = LineAnnotationType.Vertical;
                     break;

                  case LineType.HorizontalFromPointToRight:
                     this.MinimumX = this.LeftDataPoint.X;
                     this.MaximumX = double.MaxValue;
                     this.Y = this.LeftDataPoint.Y;
                     this.Type = LineAnnotationType.Horizontal;
                     break;

                  case LineType.HorizontalFromPointToLeft:
                     this.MinimumX = double.MinValue;
                     this.MaximumX = this.RightDataPoint.X;
                     this.Y = this.RightDataPoint.Y;
                     this.Type = LineAnnotationType.Horizontal;
                     break;

                  case LineType.HorizontalPointToPoint:
                     this.Type = LineAnnotationType.Horizontal;
                     this.Y = this.LeftDataPoint.Y;
                     this.MaximumX = this.RightDataPoint.X;
                     this.MinimumX = this.LeftDataPoint.X;
                     break;

                  case LineType.VerticalPointToDown:
                     this.Type = LineAnnotationType.Vertical;
                     this.X = this.LeftDataPoint.X;
                     this.MinimumY = Double.MinValue;
                     this.MaximumY = this.LeftDataPoint.Y;
                     break;

                  case LineType.VerticalPointToPoint:
                     this.Type = LineAnnotationType.Vertical;
                     this.X = this.LeftDataPoint.X;
                     this.MinimumY = this.RightDataPoint.Y;
                     this.MaximumY = this.LeftDataPoint.Y;
                     break;

                  case LineType.VerticalPointToUp:
                     this.Type = LineAnnotationType.Vertical;
                     this.X = this.LeftDataPoint.X;
                     this.MinimumY = this.LeftDataPoint.Y;
                     this.MaximumY = Double.MaxValue;
                     break;
               }
               break;
               #endregion
         }
      }

      public void RestoreSystemTools()
      {
         // restore system tools
         MainWindow mainWindow = (MainWindow.mainWindow as MainWindow);
         mainWindow.LineStyle = this.SystemLinestyle;
         mainWindow.StrokeThickness = this.SystemStrokeThickness;
         mainWindow.DrawingColor = this.SystemDrawingColor;
      }


      /// <summary>
      /// Shows that the line is being edited by drawing a rubberband and and thumbs
      /// and sets the drawing tools to relect those of this line.
      /// If the line was isEditing and must become unselected the system tool values gets written back to them
      /// </summary>
      /// <param name="beingEdited"></param>
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
            //   this.DrawRubberBandAndThumbs(this.Window, this.Gdi, this.mouseEventArgs);
            }
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

      public void SetHighlighted(Control window, GDI32 gdi, MouseEventArgs e, bool select)
      {
         // skip when there is nothing to do
         if (select && this.IsHighLighted)
            // already highlighted
            return;
         if (!select && !this.IsHighLighted)
            // already unselected
            return;
         this.IsHighLighted = select;
         // set/unset highlighting
         this.GetParametersFromAnnotation(window);
         this.Window = window;
         this.Gdi = gdi;
         this.mouseEventArgs = e;
         this.DrawRubberBandAndThumbs(window, gdi, e);
      }

      public void Attach(Control window, GDI32 gdi)
      {
         this.Window = window;
         this.Gdi = gdi;
      }

      private Point CalculateScreenPositionForInfiniteLeftPoint(Control window)
      {
         double x;
         double y;

         double date0 = this.XAxis.InverseTransform(0);
         double price0 = this.Slope * date0 + this.Intercept;
         int screenY0 = (int)this.YAxis.Transform(price0);

         // The line may exit the window on the left or at the bottom or top of the window
         if ((screenY0 >= 0) && (screenY0 <= window.Height))
         {
            // exits at the left
            x = 0;
            y = (int)this.YAxis.Transform(price0);
            this.LeftThumbOnStartOfRubberBand = false;
         }
         else
         {
            // P1 exists either at the top or bottom: this depends on whether the slope is up or down
            if (this.Slope >= 0)
            {
               // The slope is up, so P1 could exit at the bottom or end inside the window
               double priceBottom = this.YAxis.InverseTransform(window.Height);
               double dateBottom = (priceBottom - this.Intercept) / this.Slope;
               int xBottom = (int)this.XAxis.Transform(dateBottom);
               if ((0 <= xBottom) && (xBottom <= window.Width))
               {
                  // it intersects the bottom
                  this.LeftThumbOnStartOfRubberBand = false;
                  x = xBottom;
                  y = window.Height;
               }
               else
               {
                  // P1 is inside the window
                  this.LeftThumbOnStartOfRubberBand = true;
                  x = this.XAxis.Transform(this.MinimumX);
                  y = this.YAxis.Transform((this.Slope * this.MinimumX) + this.Intercept);
               }
            }
            else
            {
               // The slope is down, so P1 could exit at the top or end inside the window
               double priceTop = this.YAxis.InverseTransform(0);
               double dateTop = (priceTop - this.Intercept) / this.Slope;
               int xTop = (int)this.XAxis.Transform(dateTop);
               if ((0 <= xTop) && (xTop <= window.Width))
               {
                  // it intersects the top
                  this.LeftThumbOnStartOfRubberBand = false;
                  x = xTop;
                  y = 0;
               }
               else
               {
                  // P1 is inside the window
                  this.LeftThumbOnStartOfRubberBand = true;
                  x = this.XAxis.Transform(this.MinimumX);
                  y = this.YAxis.Transform((this.Slope * this.MinimumX) + this.Intercept);
               }
            }
         }
         return new Point((int)x, (int)y);
      }

      private Point CalculateScreenPositionForInfiniteRightPoint(Control window)
      {
         double x;
         double y;

         double yIntercept = (this.YAxis.InverseTransform(0) - this.Intercept) / this.Slope;

         double dateWidth = this.XAxis.InverseTransform(window.Width);
         double priceWidth = this.Slope * dateWidth + this.Intercept;
         int screenYWidth = (int)this.YAxis.Transform(priceWidth);

         // The line line may exit the window on the right or earlier
         if ((screenYWidth >= 0) && (screenYWidth <= window.Height))
         {
            // The line exits at the right
            x = window.Width;
            y = (int)this.YAxis.Transform(priceWidth);
            this.RightThumbOnEndOfRubberBand = false;
         }
         else
         {
            // The line exists earlier: calculate where: either at bottom or top
            if (this.Slope >= 0)
            {
               // The line slopes up, so it may intersect the top (0) or end inside the window
               double priceTop = this.YAxis.InverseTransform(0);
               double dateTop = (priceTop - this.Intercept) / this.Slope;
               int xTop = (int)this.XAxis.Transform(dateTop);
               if ((0 <= xTop) && (xTop <= window.Width))
               {
                  // The line exits at the top
                  y = 0;
                  x = xTop;
                  this.RightThumbOnEndOfRubberBand = false;
               }
               else
               {
                  // The right point is inside the window
                  this.RightThumbOnEndOfRubberBand = true;
                  x = this.XAxis.Transform(this.MaximumX);
                  y = this.YAxis.Transform((this.Slope * this.MaximumX) + this.Intercept);
               }
            }
            else
            {
               // The line slopes down, so it may intersect the bottom (Height) or end inside the window
               double priceBottom = this.YAxis.InverseTransform(window.Height);
               double dateBottom = (priceBottom - this.Intercept) / this.Slope;
               int xBottom = (int)this.XAxis.Transform(dateBottom);
               if ((0 <= xBottom) && (xBottom <= window.Width))
               {
                  // The line exits at the bottom
                  y = window.Height;
                  x = xBottom;
                  this.RightThumbOnEndOfRubberBand = false;
               }
               else
               {
                  // The right point is inside the window
                  this.RightThumbOnEndOfRubberBand = true;
                  x = this.XAxis.Transform(this.MaximumX);
                  y = this.YAxis.Transform((this.Slope * this.MaximumX) + this.Intercept);
               }
            }
         }

         return new Point((int)x, (int)y);
      }

      private Point CalculateScreenPositionForASetMinimumX(Control window)
      {
         double x;
         double y;
         double screenMinX = this.XAxis.Transform(this.MinimumX);

         // check if the left point in inside x range
         if (screenMinX < 0)
         {
            // The left point is outside the x range
            // it could exit where x = 0, or at bottom or top
            double price0 = this.Slope * this.XAxis.InverseTransform(0) + this.Intercept;
            int y0 = (int)this.YAxis.Transform(price0);

            if ((y0 >= 0) && (y0 <= window.Height))
            {
               // The line exits at the left
               y = y0;
               x = 0;
            }
            else
            {
               // The line exits at the bottom
               y = window.Height;
               x = 0;
            }
            this.LeftThumbOnStartOfRubberBand = false;
         }
         else
         {
            // The left point is in x range:
            // check if it is in y range
            double screenYatMinX = this.YAxis.Transform(this.MinimumX * this.Slope + this.Intercept);

            // check if left point in inside y range
            if ((screenYatMinX >= 0) && (screenYatMinX <= window.Width))
            {
               this.LeftThumbOnStartOfRubberBand = true;
               double priceY0 = (this.Slope * this.MinimumX) + this.Intercept;
               x = (int)(screenMinX + 0.5);
               y = (int)this.YAxis.Transform(priceY0);
            }
            else
            {
               // The y point is outside the window, so we have to limit SP
               this.LeftThumbOnStartOfRubberBand = false;
               if (screenYatMinX < 0)
               {
                  // it went out on the top
                  double topPrice = this.YAxis.InverseTransform(0);
                  double dayAtTop = (topPrice - this.Intercept) / this.Slope;
                  y = 0;
                  x = this.XAxis.Transform(dayAtTop);
               }
               else
               {
                  // find the price at this point and convert to window
                  x = (int)(this.XAxis.Transform((this.YAxis.Transform(0)) / this.Slope));
                  y = 0;
               }
            }
         }
         return new Point((int)x, (int)y);
      }

      private Point CalculateScreenPositionForASetMaximumX(Control window)
      {
         double x;
         double y;

         double yIntercept = (this.YAxis.InverseTransform(0) - this.Intercept) / this.Slope;

         double screenMaxX = this.XAxis.Transform(this.MaximumX);
         double dateWidth = this.XAxis.InverseTransform(window.Width);
         double priceWidth = this.Slope * dateWidth + this.Intercept;
         int screenYWidth = (int)this.YAxis.Transform(priceWidth);
         double dateYWidth;
         double priceYWidth;

         if (screenMaxX > window.Width)
         {
            // the right point is off the window
            this.RightThumbOnEndOfRubberBand = false;

            // get top or bottom:
            // get price where y = Top (0)
            double priceAtTop = this.YAxis.InverseTransform(0);

            // it could exit where x = Width0, or at bottom or top
            priceWidth = this.Slope * this.XAxis.InverseTransform(window.Width) + this.Intercept;
            int yWidth = (int)this.YAxis.Transform(priceWidth);

            if ((yWidth >= 0) && (yWidth <= window.Height))
            {
               // it exits at the right
               y = yWidth;
               x = window.Width;
            }
            else
            {
               // get the day where the line cuts top
               double dayAtTop = (priceAtTop - this.Intercept) / this.Slope;
               int screenXAtTop = (int)this.XAxis.Transform(dayAtTop);

               if ((screenXAtTop < window.Width) && (screenXAtTop >= 0))
               {
                  // exit at top
                  this.RightThumbOnEndOfRubberBand = false;
                  y = 0;
                  x = screenXAtTop;

               }
               else
               {
                  // exit at bottom
                  priceYWidth = this.YAxis.InverseTransform(window.Width);
                  dateYWidth = (priceYWidth - this.Intercept) / this.Slope;
                  x = this.XAxis.Transform(dateYWidth);
                  x = window.Width;
                  y = (int)this.YAxis.Transform(screenYWidth);
                  this.RightThumbOnEndOfRubberBand = true;
               }
            }
         }
         else
         {
            // the right point is on the window
            this.RightThumbOnEndOfRubberBand = true;

            x = screenMaxX;
            y = this.YAxis.Transform((this.MaximumX * this.Slope) + this.Intercept);
         }
         return new Point((int)x, (int)y);
      }

      /// <summary>
      /// Sets the LineType and InitialRubberBandLeftPoint and InitialRubberBandRightPoint
      /// </summary>
      /// <param name="window"></param>
      /// <param name="mouseEventArgs"></param>
      public void GetParametersFromAnnotation(Control window)
      {
         switch (this.Type)
         {
            case LineAnnotationType.Horizontal:
               #region Horizontal lines
               if (this.MinimumX <= MySqlMin && MaximumX >= MySqlMax)
               {
                  this.LineType = LineType.HorizontalInfinite;
                  this.InitialRubberBandLeftPoint.X = 0;
                  this.InitialRubberBandLeftPoint.Y = (int)this.YAxis.Transform(this.Y);
                  this.InitialRubberBandRightPoint.X = window.Width;
                  this.InitialRubberBandRightPoint.Y = InitialRubberBandLeftPoint.Y;
                  this.LeftThumbPoint.Y = InitialRubberBandLeftPoint.Y;
                  this.RightThumbPoint.Y = InitialRubberBandLeftPoint.Y;
                  this.LeftThumbPoint.X = (InitialRubberBandRightPoint.X - InitialRubberBandLeftPoint.X) / 4;
                  this.RightThumbPoint.X = (InitialRubberBandRightPoint.X - InitialRubberBandLeftPoint.X) * 3 / 4;
                  this.LeftThumbOnStartOfRubberBand = false;
                  this.RightThumbOnEndOfRubberBand = false;
               }
               else if (this.MinimumX > MySqlMin && this.MaximumX >= MySqlMax)
               {
                  this.LineType = LineType.HorizontalFromPointToRight;

                  // The left end may be off the window
                  if (this.XAxis.Transform(this.MinimumX) < 0)
                  {
                     this.InitialRubberBandLeftPoint.X = 0;
                     this.LeftThumbOnStartOfRubberBand = false;
                  }
                  else
                  {
                     // the left is on the window
                     this.InitialRubberBandLeftPoint.X = (int)this.XAxis.Transform(this.MinimumX);
                     this.LeftThumbOnStartOfRubberBand = true;
                  }

                  this.InitialRubberBandLeftPoint.Y = (int)this.YAxis.Transform(this.Y);
                  this.InitialRubberBandRightPoint.X = window.Width;
                  this.InitialRubberBandRightPoint.Y = this.InitialRubberBandLeftPoint.Y;
                  this.LeftThumbPoint.Y = this.InitialRubberBandLeftPoint.Y;
                  this.RightThumbPoint.Y = this.InitialRubberBandLeftPoint.Y;
                  this.LeftThumbPoint.X = this.InitialRubberBandLeftPoint.X;
                  this.RightThumbPoint.X = (this.InitialRubberBandRightPoint.X - this.InitialRubberBandLeftPoint.X) * 3 / 4;
                  this.RightThumbOnEndOfRubberBand = false;
               }
               else if (this.MinimumX <= MySqlMin && this.MaximumX < MySqlMax)
               {
                  this.LineType = LineType.HorizontalFromPointToLeft;
                  this.InitialRubberBandLeftPoint.X = 0;
                  this.InitialRubberBandLeftPoint.Y = (int)this.YAxis.Transform(this.Y);

                  // The right end may be off the window
                  if (this.XAxis.Transform(this.MaximumX) < window.Width)
                  {
                     this.InitialRubberBandRightPoint.X = (int)this.XAxis.Transform(this.MaximumX);
                     this.RightThumbOnEndOfRubberBand = true;
                  }
                  else
                  {
                     // limit to the right of the window
                     this.InitialRubberBandRightPoint.X = window.Width;
                     this.RightThumbOnEndOfRubberBand = false;
                  }

                  this.InitialRubberBandRightPoint.Y = this.InitialRubberBandLeftPoint.Y;
                  this.LeftThumbPoint.Y = this.InitialRubberBandLeftPoint.Y;
                  this.RightThumbPoint.Y = this.InitialRubberBandLeftPoint.Y;
                  this.LeftThumbPoint.X = (this.InitialRubberBandRightPoint.X - this.InitialRubberBandLeftPoint.X) * 3 / 4;
                  this.RightThumbPoint.X = this.InitialRubberBandLeftPoint.X;
                  this.LeftThumbOnStartOfRubberBand = false;
               }
               else
               {
                  // Horizontal point to point
                  this.LineType = LineType.HorizontalPointToPoint;

                  // The left end may be off the window
                  if (this.XAxis.Transform(this.MinimumX) >= 0)
                  {
                     // the left point is on the window
                     this.InitialRubberBandLeftPoint.X = (int)this.XAxis.Transform(this.MinimumX);
                     this.LeftThumbOnStartOfRubberBand = true;
                  }
                  else
                  {
                     // The left point is off the screedn: limit to the left of the window
                     this.InitialRubberBandLeftPoint.X = 0;
                     this.LeftThumbOnStartOfRubberBand = false;
                  }

                  InitialRubberBandLeftPoint.Y = (int)this.YAxis.Transform(this.Y);

                  // The right end may be off the window
                  if (this.XAxis.Transform(this.MaximumX) < window.Width)
                  {
                     // the right is on the window
                     this.InitialRubberBandRightPoint.X = (int)this.XAxis.Transform(this.MaximumX);
                     this.RightThumbOnEndOfRubberBand = true;
                  }
                  else
                  {
                     // The right is off the window: limit to the right of the window
                     this.InitialRubberBandRightPoint.X = window.Width;
                     this.RightThumbOnEndOfRubberBand = false;
                  }

                  this.InitialRubberBandRightPoint.Y = this.InitialRubberBandLeftPoint.Y;
               }
               break;
               #endregion
            case LineAnnotationType.LinearEquation:
               #region LinearInfinite Lines
               if (this.MinimumX <= MySqlMin && this.MaximumX >= MySqlMax)
               {
                  this.LineType = LineType.LinearInfinite;
                  this.InitialRubberBandLeftPoint = this.CalculateScreenPositionForInfiniteLeftPoint(window);
                  this.InitialRubberBandRightPoint = this.CalculateScreenPositionForInfiniteRightPoint(window);
                  this.RightThumbOnEndOfRubberBand = false;
                  this.LeftThumbOnStartOfRubberBand = false;
               }

               else if ((this.MinimumX > MySqlMin) && (this.MaximumX >= MySqlMax))
               {
                  this.LineType = LineType.LinearFromPointToRight;
                  this.InitialRubberBandLeftPoint = this.CalculateScreenPositionForASetMinimumX(window);
                  this.InitialRubberBandRightPoint = this.CalculateScreenPositionForInfiniteRightPoint(window);
                  this.RightThumbOnEndOfRubberBand = false;
                  this.LeftThumbOnStartOfRubberBand = true;
               }
               else if (this.MinimumX <= MySqlMin && this.MaximumX < MySqlMax)
               {
                  this.LineType = LineType.LinearFromPointToLeft;
                  this.InitialRubberBandLeftPoint = this.CalculateScreenPositionForInfiniteLeftPoint(window);
                  this.InitialRubberBandRightPoint = this.CalculateScreenPositionForASetMaximumX(window);
                  this.RightThumbOnEndOfRubberBand = true;
                  this.LeftThumbOnStartOfRubberBand = false;
               }
               else
               {
                  // LinearInfinite point to point line
                  this.LineType = LineType.LinearPointToPoint;
                  this.InitialRubberBandLeftPoint = this.CalculateScreenPositionForASetMinimumX(window);
                  this.InitialRubberBandRightPoint = this.CalculateScreenPositionForASetMaximumX(window);
                  this.RightThumbOnEndOfRubberBand = true;
                  this.LeftThumbOnStartOfRubberBand = true;
               }
               break;
               #endregion
            case LineAnnotationType.Vertical:
               #region Vertical Lines
               if (this.MinimumY > MySqlMin && this.MaximumY < MySqlMax)
               {
                  this.LineType = LineType.VerticalPointToPoint;
                  this.InitialRubberBandLeftPoint.X = (int)this.XAxis.Transform(this.X);
                  this.InitialRubberBandLeftPoint.Y = (int)this.YAxis.Transform(this.MaximumY);
                  this.InitialRubberBandRightPoint.X = InitialRubberBandLeftPoint.X;
                  this.InitialRubberBandRightPoint.Y = (int)this.YAxis.Transform(this.MinimumY);
                  this.RightThumbOnEndOfRubberBand = true;
                  this.LeftThumbOnStartOfRubberBand = true;
               }
               else if (this.MinimumY <= MySqlMin && this.MaximumY >= MySqlMax)
               {
                  this.LineType = LineType.VerticalInfinite;
                  this.InitialRubberBandLeftPoint.X = (int)this.XAxis.Transform(this.X);
                  this.InitialRubberBandLeftPoint.Y = 0;
                  this.InitialRubberBandRightPoint.Y = window.Height;
                  this.InitialRubberBandRightPoint.X = InitialRubberBandLeftPoint.X;
                  this.RightThumbOnEndOfRubberBand = false;
                  this.LeftThumbOnStartOfRubberBand = false;
               }
               else if (this.MinimumY > MySqlMin && this.MaximumY >= MySqlMax)
               {
                  this.LineType = LineType.VerticalPointToUp;
                  this.InitialRubberBandLeftPoint.X = (int)this.XAxis.Transform(this.X);
                  this.InitialRubberBandLeftPoint.Y = (int)this.YAxis.Transform(this.MinimumY);
                  this.InitialRubberBandRightPoint.Y = 0;
                  this.InitialRubberBandRightPoint.X = InitialRubberBandLeftPoint.X;
                  this.RightThumbOnEndOfRubberBand = false;
                  this.LeftThumbOnStartOfRubberBand = true;
               }
               else if (this.MinimumY <= MySqlMin && this.MaximumY < MySqlMax)
               {
                  this.LineType = LineType.VerticalPointToDown;
                  this.InitialRubberBandLeftPoint.X = (int)this.XAxis.Transform(this.X);
                  this.InitialRubberBandLeftPoint.Y = (int)this.YAxis.Transform(this.MaximumY);
                  this.InitialRubberBandRightPoint.Y = window.Height;
                  this.InitialRubberBandRightPoint.X = InitialRubberBandLeftPoint.X;
                  this.RightThumbOnEndOfRubberBand = false;
                  this.LeftThumbOnStartOfRubberBand = true;
               }
               break;
               #endregion
         }
      }

      private Point ToPoint(DataPoint p)
      {
         ScreenPoint ps = this.Transform(p);
         int ix;
         if (ps.X > Int32.MaxValue)
            ix = Int32.MaxValue;
         else if (ps.X < Int32.MinValue)
            ix = Int32.MinValue;
         else
            ix = (int)ps.X;
         int iy;
         if (ps.Y > Int32.MaxValue)
            iy = Int32.MaxValue;
         else if (ps.Y < Int32.MinValue)
            iy = Int32.MinValue;
         else
            iy = (int)ps.Y;
         return new Point(ix, iy);
      }

      private bool IsCloseTo(Point p1, Point p2, int tollerance)
      {
         return ((Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2)) < tollerance * tollerance);
      }
   }
}
