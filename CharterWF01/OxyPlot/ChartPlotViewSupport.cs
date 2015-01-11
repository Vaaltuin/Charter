
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChartPlotViewSupport.cs" company="Wynand">
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
//   This interface is used by my annotations to implement "multiple" inheritance
//   it provides a bridge between functions required by CharPlotView and my annotations.
//   The interface ensures that all annotations correctly implemements the fuctions so that they can
//   be safely called by ChartPlotView.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace CharterWF
{
   using System.Drawing;
   using System.Windows.Forms;
   using OxyPlot;
   using WBGraphicUtils;

   interface ChartPlotViewSupport
   {
      void MoveRubberband(Control window, GDI32 gdi, PlotModel model, int deltaX, int deltaY, Point mouseDownPoint, MouseEventArgs e);
      void ChangePosition();
      void UnSetHighlighted();
      void SetHighlighted(Control window, GDI32 gdi, MouseEventArgs e, bool select);

      /// <summary>
      /// Shows that the annotation is being edited by drawing a rubberband and and thumbs
      /// and sets the mainwindow drawing tools to reflect those of this annotation.
      /// If the annotation was being edited and now becomes nopt edited , then the system tool values gets written back to them
      /// </summary>
      /// <param name="beingEdited"></param>
      void SetBeingEdited(bool beingEdited);
      /// <summary>
      /// Restores the main window system tools
      /// </summary>
      void RestoreSystemTools();
      bool GetBeingEdited();
      void DetectActiveThumb(MouseEventArgs e);
   }
}
