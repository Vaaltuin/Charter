// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CustomOxyPlot.cs" company="Wynand">
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
//  NOTE:
//  This class is used by both EOD and indicator charts
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace CharterWF
{
   using System;
   using System.Drawing;
   using System.Windows.Forms;
   using OxyPlot;
   using OxyPlot.Annotations;
   using WBGraphicUtils;

   public enum ChartTimeFrame { Daily, Weekly, Monthly, Yearly };
   public enum CandleSpan { Refresh, Day, Week, Month };
   public enum MouseActions
   {
      MovingAround, Deleting, Selected, NewArrowLine, NewTextArrowLine, NewLine, NewLineText, NewLDT, Rectangle,
      Ellipse, NewText
   };
   public enum AnnotationType { None, ArrowLine, Line, Text, LDT }
   public enum LineOrientationFreedom { Vertical, Horizontal, Free }
   public enum LineDrawStyle { Free, MouseDownToInfinity, Infinety, Controlled }
   public enum LineType
   {
      LinearInfinite, LinearPointToPoint, LinearFromPointToRight, LinearFromPointToLeft,
      HorizontalInfinite, HorizontalFromPointToRight, HorizontalFromPointToLeft, HorizontalPointToPoint,
      VerticalInfinite, VerticalPointToPoint, VerticalPointToUp, VerticalPointToDown
   };

   /// <summary>
   /// This class creates annotation derived chart where in I can drag/move on the chart with my methods
   /// that are much faster than the slow manipulation of the annotations as provided by OxyPlot.
   /// </summary>
   public class ChartPlotView : OxyPlot.WindowsForms.PlotView
   {
      private enum MovedAnnotationTypes
      {
         None, Arrow, Text, InfiniteLine, LDT, Line
      };

      public bool IsModified = false;
      public MouseActions MouseAction;
      public PriceLineAnnotation SelectedLineAnotation;

      private GDI32 gdi;
      private Point MousePos1 = new Point(0, 0);
      private Point MousePos2 = new Point(0, 0);
      private Point OldMousePoint, MouseDownPoint;
      private Point OldCursorPos;
      private DrawingParams drawingParams = new DrawingParams();
      private ChartPlotViewSupport ActiveAnnotation;

      public ChartPlotView()
      {
         gdi = new GDI32();
      }

      protected override void OnMouseDown(MouseEventArgs e)
      {
         ChartPlotViewSupport annotationUnderCursor = null;
         HitTestArguments args;
         HitTestResult hit = null;
         MainWindow mainWindow = (MainWindow.mainWindow as MainWindow);

         if (e.Button == MouseButtons.Left)
         {
            this.OldMousePoint = this.MouseDownPoint = new Point(e.X, e.Y);
            this.drawingParams.XorP1 = new Point(e.X, e.Y);
            this.drawingParams.XorP2 = this.drawingParams.XorP1;
            DataPoint dp;

            switch (this.MouseAction)
            {
               case MouseActions.NewLDT:
                  LabeledTextAnnotation lta = new LabeledTextAnnotation(mainWindow, this.Model, "LDT1", e);
                  this.ActiveAnnotation = lta;
                  this.Model.Annotations.Add(lta);
                  lta.LastMousePos = new ScreenPoint(e.X, e.Y);
                  this.InvalidatePlot(false);
                  mainWindow.SetMouseActions(MouseActions.MovingAround);
                  break;

               case MouseActions.NewText:
                  dp = new DataPoint(0, 0);
                  dp.X = this.Model.DefaultXAxis.InverseTransform(e.X);
                  dp.Y = this.Model.DefaultYAxis.InverseTransform(e.Y);
                  PriceTextAnnotation ta = new PriceTextAnnotation(mainWindow, this.Model, e, dp);
                  this.ActiveAnnotation = ta;
                  this.Model.Annotations.Add(ta);
                  ta.LastMousePos = new ScreenPoint(e.X, e.Y);
                  this.InvalidatePlot(false);
                  mainWindow.SetMouseActions(MouseActions.MovingAround);
                  break;

               case MouseActions.NewArrowLine:
               case MouseActions.NewTextArrowLine:
                  this.ActiveAnnotation = new PriceArrowAnnotation(mainWindow, this.Model, e, this.MouseAction == MouseActions.NewTextArrowLine);
                  this.Model.Annotations.Add(this.ActiveAnnotation as Annotation);
                  this.InvalidatePlot(false);
                  break;

               case MouseActions.NewLine:
               case MouseActions.NewLineText:
                  this.ActiveAnnotation = new PriceLineAnnotation(mainWindow, this.Model, e, this.MouseAction == MouseActions.NewLineText);
                  this.Model.Annotations.Add(this.ActiveAnnotation as Annotation);
                  this.InvalidatePlot(false);
                  break;

               case MouseActions.MovingAround:
               case MouseActions.Selected:
                  #region Moving

                  // We end up here when we either want to just change selection or to start dragging
                  // We only start draggging when we hit an annotation.
                  // Steps:
                  // Restore system tools if necessary:
                  // We need to first unselect the annotation that is currently being edited in order to correcty restore the system tools,
                  // then select the new annotation to be edited (if necessary)
                  // so go through the annotations, only record the hit and unselect the rest, then select the hit
                  // For each annotaton check whether it is hit, and when hit set beingEdited an set the system tools
                  // unselect the rest, i.e. if no annotation was hit there will be no selection
                  if (this.ActiveAnnotation != null)
                  {
                     if (this.ActiveAnnotation.GetBeingEdited())
                        this.ActiveAnnotation.RestoreSystemTools();
                  }
                  annotationUnderCursor = null;
                  foreach (var annotation in this.Model.Annotations)
                  {
                     args = new HitTestArguments(new ScreenPoint(e.X, e.Y), 10);
                     hit = annotation.HitTest(args);
                     if (hit != null)
                     {
                        annotationUnderCursor = annotation as ChartPlotViewSupport;
                        bool beingEdited = annotationUnderCursor.GetBeingEdited();
                        if (!beingEdited)
                        {
                           // enable beingEdited on this annotation
                           this.ActiveAnnotation = annotationUnderCursor;
                           annotationUnderCursor.DetectActiveThumb(e);
                           (annotation as ChartPlotViewSupport).SetBeingEdited(true);
                        }
                        else
                        {
                           this.ActiveAnnotation = null;
                           (annotation as ChartPlotViewSupport).SetBeingEdited(false);
                        }
                        // break;
                     }
                     else
                        (annotation as ChartPlotViewSupport).SetBeingEdited(false);
                  }

                  return;

                  #endregion
               case MouseActions.Deleting:
                  foreach (var a in this.Model.Annotations)
                  {
                     args = new HitTestArguments(new ScreenPoint(e.X, e.Y), 10);
                     hit = a.HitTest(args);
                     if (hit != null)
                     {
                        this.Model.Annotations.Remove(hit.Element as Annotation);
                        this.IsModified = true;
                        this.Model.InvalidatePlot(false);
                        break;
                     }
                  }
                  break;
            }
            // It is necessary to disable the tracker as it interferes with the XORing during rubberbanding,
            // leaving artifacts on the screen
            // This is easily achieved by not calling the base mouse movement
            return;
         }
         base.OnMouseDown(e);
      }

      public void DoMouseMove(MouseEventArgs e)
      {
         HitTestResult hit = null;

         // manage the cursor lines:
         Graphics graphics = CreateGraphics();
         gdi.InitPenAndBrush(graphics);
         gdi.XorDrawLine1(graphics, new Point(0, this.OldCursorPos.Y), new Point(this.Width, this.OldCursorPos.Y));
         gdi.XorDrawLine1(graphics, new Point(0, e.Y), new Point(Width, e.Y));
         gdi.XorDrawLine1(graphics, new Point(this.OldCursorPos.X, 0), new Point(this.OldCursorPos.X, this.Height));
         gdi.XorDrawLine1(graphics, new Point(e.X, 0), new Point(e.X, this.Height));
         gdi.Dispose(graphics);

         // manage the creation of new Text and LDT which requires action on mouse moves without pressing any mouse button:
         if (e.Button == MouseButtons.None)
         {
            #region PlaceNewText
            if (this.MouseAction == MouseActions.NewText)
            {
               // show the text before it is created
               PriceTextAnnotation.DragText(this, gdi, OldMousePoint, e);
            }
            if (this.MouseAction == MouseActions.NewLDT)
            {
               // show the text before it is created
               LabeledTextAnnotation.DragText(this, gdi, OldMousePoint, e);
            }
            #endregion
            else
            #region HighlightAnnotationUnderCursor
            {
               // set each annotation's higlighting:
               // highlight only the hit, keep isEditing annotations, unhighlight the rest
               foreach (var annotation in this.Model.Annotations)
               {
                  var args = new HitTestArguments(new ScreenPoint(e.X, e.Y), 10);
                  hit = annotation.HitTest(args);
                  if (!(annotation as ChartPlotViewSupport).GetBeingEdited())
                  {
                     (annotation as ChartPlotViewSupport).SetHighlighted(this, gdi, e, hit != null);
                  }
               }
            }
         }
            #endregion
         this.OldCursorPos = new Point(e.X, e.Y);

         if (e.Button == MouseButtons.Left)
         #region RepositionExisting
         {
            this.IsModified = true;
            int deltaX = e.X - this.MouseDownPoint.X;
            int deltaY = e.Y - this.MouseDownPoint.Y;

            if (this.ActiveAnnotation != null)
               this.ActiveAnnotation.MoveRubberband(this, gdi, this.Model, deltaX, deltaY, this.MouseDownPoint, e);

            return; // stop OxyPlot tracker from popping up
         #endregion
         }
         this.OldMousePoint = new Point(e.X, e.Y);
      }

      protected override void OnMouseUp(MouseEventArgs e)
      {
         if (this.ActiveAnnotation != null)
         {
            if (this.OldMousePoint != new Point(e.X, e.Y))
            {
               this.ActiveAnnotation.ChangePosition();
               this.Invalidate(false);
               this.Focus();
               (MainWindow.mainWindow as MainWindow).SelectSelectingMode();
               this.ActiveAnnotation = null;
            }
            else
               (MainWindow.mainWindow as MainWindow).SelectSelectingMode();
         }

         base.OnMouseUp(e);
      }

      new private void InvalidatePlot(bool loadData)
      {
         // this will remove all our drawings, so we have to mark the annotations as not highlighted,
         // and if there was a selection highlight it.
         foreach (var annotation in this.Model.Annotations)
         {
            ChartPlotViewSupport a = annotation as ChartPlotViewSupport;
            a.UnSetHighlighted();
         }
         base.InvalidatePlot(loadData);
      }

      public class DrawingParams
      {
         public DrawingParams()
         {
            XorP1 = new Point(0, 0);
            XorP2 = new Point(0, 0);
            lineOrientationFreedom = LineOrientationFreedom.Free;
            lineDrawStyle = LineDrawStyle.Free;
         }
         public float Slope { get; set; }
         public Point XorP1 { get; set; }
         public Point XorP2 { get; set; }
         public LineOrientationFreedom lineOrientationFreedom { get; set; }
         public LineDrawStyle lineDrawStyle { get; set; }
      }
   }
}
