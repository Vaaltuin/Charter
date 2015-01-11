// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Quote.cs" company="Wynand">
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
//   Gets the median.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace CharterWF
{
   using System;
   using System.Collections.Generic;
   using System.Linq;
   using System.Text;
   using System.Threading.Tasks;

   /// <summary>
   /// Class for measurement
   /// </summary>
   public class Quote
   {
      /// <summary>
      /// Gets or sets DateTime field
      /// </summary>
      public DateTime Date { get; set; }

      /// <summary>
      /// Gets or sets the DetectorId field
      /// </summary>
      public double Open { get; set; }

      /// <summary>
      /// Gets or sets the Value field
      /// </summary>
      public double High { get; set; }

      /// <summary>
      /// Gets or sets the Value Low
      /// </summary>
      public double Low { get; set; }

      /// <summary>
      /// Gets or sets the Value Close
      /// </summary>
      public double Close { get; set; }

      /// <summary>
      /// Gets or sets the Value Volume
      /// </summary>
      public double Volume { get; set; }
   }
}
