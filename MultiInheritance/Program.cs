using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiInheritance
{
   class Program
   {
      static void Main(string[] args)
      {
         Line line = new Line();
         Arrow arrow = new Arrow();

         Console.WriteLine("Line: " + line.Var1);
         Console.WriteLine("Arrow name: " + arrow.name());
         Support support1 = arrow;
         Support support2 = line;
         Console.WriteLine("Support1 name: " + support1.name());
         Console.WriteLine("Support2 name: " + support2.name());
         Console.WriteLine("Hit any key");
         Console.ReadKey();
      }
   }

   class Annotations
   {
      private string text = "Annotations";

      public string Text
      {
         get { return text; }
      }
   }

   class Line : Annotations, Support
   {
      public string Var1 = "gert";
      public string name()
      {
         return "Line";
      }
   }

   class Arrow : Annotations, Support
   {
      public string VarArrow = "gert";
      public string name()
      {
         return "Arrow";
      }
   }

   interface Support
   {
      string name();
   }
}
