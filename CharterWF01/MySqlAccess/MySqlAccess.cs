// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DbAccessClass1.cs" company="Wynand">
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
namespace MySqlAccess
{
   using CharterWF;
   using MySql.Data.MySqlClient;
   using OxyPlot;
   using OxyPlot.Annotations;
   using System;
   using System.Collections.Generic;
   using System.Windows.Controls;

   enum AnnotationColumn
   {
      ShareCode, ID, Chart, Type, StartX, StartY, EndX, EndY, Color, Intercept, MinimumX, MaximumX, Slope,
      Text, Font, FontSize, LineStyle, StrokeThickeness, LineAnnotationStyle, X, Y, MinimumY, MaximumY, LineType
   }

   /// <summary>
   /// TODO: This class provides
   /// </summary>
   class MySqlDbAccess
   {
      public bool Connected = false;
      public string ConnectingString = "";

      private MySqlConnection sqlConnection = null;
      private AnnotationAdder annotations;

      #region Contructors
      /// <summary>
      /// This constructor also opens the connection to the data base
      /// </summary>
      /// <param name="server"></param>
      /// <param name="dataBaseName"></param>
      /// <param name="user"></param>
      /// <param name="password"></param>
      public MySqlDbAccess(string server, string dataBaseName, string user, string password, AnnotationAdder annotations)
      {
         this.annotations = annotations;
         ConnectingString = @"server=" + server + ";database=" + @dataBaseName + ";userid=" + user + ";password=" + @password + ";";
         try
         {
            sqlConnection = new MySqlConnection(ConnectingString);
            Connected = true;
         }
         catch (MySqlException err)
         {
            Connected = false;
            Console.WriteLine("Error: " + err.ToString());
         }
      }

      public MySqlDbAccess(string server, string dataBaseName, string user, string password)
      {
         ConnectingString = @"server=" + server + ";database=" + @dataBaseName + ";userid=" + user + ";password=" + @password + ";";
         try
         {
            sqlConnection = new MySqlConnection(ConnectingString);
            Connected = true;
         }
         catch (MySqlException err)
         {
            Connected = false;
            Console.WriteLine("Error: " + err.ToString());
         }
      }

      /// <summary>
      /// My destructor to ensure that the connection is always closed
      /// </summary>
      ~MySqlDbAccess()
      {
         if (sqlConnection != null)
         {
            sqlConnection.Close();
            Connected = false;
         }
      }

      #endregion

      public void GetAnnotations(string shareCode, string chart)
      {
         MySqlDataReader reader = null;

         try
         {
            sqlConnection.Open();

            // Get all rows for this share and chart
            string sqlCmd = @"SELECT * FROM `charter`.`annotations` WHERE ShareCode = '" + shareCode + "' AND chart = '" + chart + "'";
            MySqlCommand cmd = new MySqlCommand(sqlCmd, sqlConnection);
            reader = cmd.ExecuteReader();
            while (reader.Read())
            {
               AnnotationType type = (AnnotationType)(reader.GetInt16((int)AnnotationColumn.Type));

               {
                  switch (type)
                  {
                     case AnnotationType.ArrowLine: annotations.AddArrowLineAnnotation(new ArrowLineAnnotationArgs(reader)); break;
                     case AnnotationType.Text: annotations.AddTextAnnotation(new TextAnnotationArgs(reader)); break;
                     case AnnotationType.Line: annotations.AddLineAnnotation(new LineAnnotationArgs(reader)); break;
                     case AnnotationType.LDT: annotations.AddLDTAnnotation(new LDTAnnotationArgs(reader)); break;
                  }
               }
            }
         }
         catch
         {

         }
      }

      public void UpdateAnnotations(string shareCode, PlotModel plotModel, string chart, string annotText)
      {
         if (shareCode == null) return;
         try
         {
            sqlConnection.Open();

            string sqlCmdString = @"DELETE FROM `charter`.`annotations` WHERE ShareCode = '" + shareCode + "' AND `Chart` = '" + chart + "'";
            MySqlCommand cmd = new MySqlCommand(sqlCmdString, sqlConnection);
            cmd.ExecuteNonQuery();

            // insert the new annotations
            int count = plotModel.Annotations.Count;
            for (int annotationIdx = 0; annotationIdx < count; annotationIdx++)
            {
               OxyPlot.Annotations.Annotation annotation = plotModel.Annotations[annotationIdx];

               if (annotation is PriceArrowAnnotation)
               {
                  PriceArrowAnnotation aa = annotation as PriceArrowAnnotation;
                  string text = aa.Text;
                  sqlCmdString = @"INSERT INTO `charter`.`annotations` (`ShareCode`, `Id`, `Type`, `StartX`, `StartY`, `EndX`, `EndY`, `Color`, `Text`, `Font`, `FontSize`, `Linestyle`, `StrokeThickness`, `Chart`) VALUES ('" +
                    shareCode + "', '" +
                    annotationIdx + "', '" +
                    (int)AnnotationType.ArrowLine + "', '" +
                    aa.StartPoint.X + "', '" +
                    aa.StartPoint.Y + "', '" +
                    aa.EndPoint.X + "', '" +
                    aa.EndPoint.Y + "', '" +
                    aa.Color.ToUint() + "', '" +
                    (aa.Text == null ? "" : aa.Text) + "', '" +
                    (aa.Font == null ? "" : aa.Font) + "', '" +
                    (double.IsNaN(aa.FontSize) ? 0 : aa.FontSize) + "', '" +
                    (int)aa.LineStyle + "', '" +
                    aa.StrokeThickness + "', '" +
                    chart +
                    "')";
                  cmd = new MySqlCommand(sqlCmdString, sqlConnection);
                  cmd.ExecuteNonQuery();
               }
               else if (annotation is PriceLineAnnotation)
               {
                  PriceLineAnnotation la = annotation as PriceLineAnnotation;

                  sqlCmdString = @"INSERT INTO `charter`.`annotations` (`ShareCode`, `Id`, `Chart`, `Type`, `MinimumX`, `MaximumX`, `MinimumY`, `MaximumY`, `Intercept`, `Slope`, `Color`, `Text`, `Font`, `FontSize`, `LineStyle`, `StrokeThickness`, `LineAnnotationStyle`, `Y`, `X`, `LineType`) VALUES ('" +
                                shareCode + "', '" +
                                (annotationIdx) + "', '" +
                                chart + "', '" +
                                (int)AnnotationType.Line + "', '" +                       // type
                                MySqlValue(la.MinimumX) + "', '" +                        // MinimumX
                                MySqlValue(la.MaximumX) + "', '" +                        // MaximumX
                                MySqlValue(la.MinimumY) + "', '" +                        // MinimumX
                                MySqlValue(la.MaximumY) + "', '" +                        // MaximumX
                                la.Intercept + "', '" +                                   // Intercept
                                la.Slope + "', '" +                                       // Slope
                                la.Color.ToUint() + "', '" +                              // Color
                                (la.Text == null ? "" : la.Text) + "', '" +
                                (la.Font == null ? "" : la.Font) + "', '" +
                                (double.IsNaN(la.FontSize) ? 0 : la.FontSize) + "', '" +
                                (int)la.LineStyle + "', '" +
                                (la.StrokeThickness) + "', '" +
                                (int)la.LineStyle +  "', '" +
                                la.Y + "', '" +
                                la.X + "', '" +
                               (int)la.Type +
                                "')";
                  cmd = new MySqlCommand(sqlCmdString, sqlConnection);
                  cmd.ExecuteNonQuery();
               }
               else if (annotation is PriceTextAnnotation)
               {
                  PriceTextAnnotation aa = annotation as PriceTextAnnotation;
                  OxyPlot.DataPoint textPoint = aa.TextPosition;
                  sqlCmdString = @"INSERT INTO `charter`.`annotations` (`ShareCode`, `Id`, `Type`, `StartX`, `StartY`, `Color`, `Chart`, `Text`, `Font`, `FontSize`) VALUES ('" +
                    shareCode + "', '" +
                    annotationIdx + "', '" +
                    (int)AnnotationType.Text + "', '" +
                    textPoint.X + "', '" +
                    textPoint.Y + "', '" +
                    aa.TextColor.ToUint() + "', '" +
                    chart + "', '" +
                    aa.Text + "', '" +
                    aa.Font.ToString() + "', '" +
                    aa.FontSize +
                    "')";
                  cmd = new MySqlCommand(sqlCmdString, sqlConnection);
                  cmd.ExecuteNonQuery();
               }
               else if (annotation is LabeledTextAnnotation)
               {
                  OxyPlot.Annotations.LabeledTextAnnotation ldt = annotation as OxyPlot.Annotations.LabeledTextAnnotation;
                  sqlCmdString = @"INSERT INTO `charter`.`annotations` (`ShareCode`, `Id`, `Chart`, `Type`, `StartX`, `StartY`, `EndX`, `EndY`, `Color`, `Text`, `LineStyle`, `StrokeThickness`) VALUES ('" +
                    shareCode + "', '" +
                    annotationIdx + "', '" +
                    chart + "', '" +
                    (int)AnnotationType.LDT + "', '" +
                    ldt.StartPoint.X + "', '" +
                    ldt.StartPoint.Y + "', '" +
                    ldt.EndPoint.X + "', '" +
                    ldt.EndPoint.Y + "', '" +
                    ldt.Color.ToUint() + "', '" +
                    ldt.Text + "', '" +
                    (int)ldt.LineStyle + "', '" +
                    ldt.StrokeThickness +
                    "')";
                  cmd = new MySqlCommand(sqlCmdString, sqlConnection);
                  cmd.ExecuteNonQuery();
               }
            }
         }
         catch
         {
            // to update
         }
         if (sqlConnection != null)
         {
            sqlConnection.Close();
         }

      }

      public void FillListViewWithShares(ListView listView)
      {
         MySqlDataReader reader = null;

         try
         {
            sqlConnection.Open();
            string sqlCmd = @"select table_name from information_schema.tables where table_schema = 'jse_eod'";
            MySqlCommand cmd = new MySqlCommand(sqlCmd, sqlConnection);
            reader = cmd.ExecuteReader();
            while (reader.Read())
            {
               string s = reader.GetString(0);
               listView.Items.Add(s);
            }
         }
         catch
         {

         }
      }

      public void WriteDailyEOD(EOD rina, EOD wynand)
      {
         sqlConnection.Open();

         DateTime date = DateTime.Now;
         string dateStr = date.ToString("yyyy-MM-dd HH:mm:ss");
         string sqlCmdString = @"INSERT INTO `portfolio`.`daily` (`Date`, `RinaTotalCash`, `RinaAvailableTradingFunds`, `RinaMoneyMarketCashBalance`, `RinaPortfolioValue`, `RinaProfit`, " +
                                                                         "`WynandTotalCash`, `WynandAvailableTradingFunds`, `WynandMoneyMarketCashBalance`, `WynandPortfolioValue`, `WynandProfit` ) VALUES ('" +
                dateStr + "', '" + rina.TotalCash.ToString() + "', '" + rina.AvailableTradingFunds.ToString() + "', '" + rina.MoneyMarketCashBalance.ToString() + "', '" + rina.PortfolioValue.ToString() + "', '" + rina.Profit.ToString() + "', '" +
                   wynand.TotalCash.ToString() + "', '" + wynand.AvailableTradingFunds.ToString() + "', '" + wynand.MoneyMarketCashBalance.ToString() + "', '" + wynand.PortfolioValue.ToString() + "', '" + wynand.Profit.ToString() + "')";
         MySqlCommand cmd = new MySqlCommand(sqlCmdString, sqlConnection);
         try
         {
            cmd.ExecuteNonQuery();
         }
         catch
         {
            // to update
         }

      }

      public static double MySqlValue(double d)
      {
         double r = d;
         if (r < -1.7976931348623E+308) r = -1.7976931348623E+308;
         if (r > 1.7976931348623E+308) r = 1.7976931348623E+308;

         return r;
      }

      /// <summary>
      /// This function gets both annotation List<Quote> and an IEnumerable<double> of the Close price
      /// </summary>
      /// <param name="share"></param>
      /// <param name="IEnumerableClosePriceList"></param>
      /// <returns></returns>
      public List<Quote> GetSharePrices(string share, out IEnumerable<double> IEnumerableClosePriceList)
      {
         var quoteList = new List<Quote>();
         var closeList = new List<double>();

         MySqlDataReader reader = null;
         try
         {
            sqlConnection.Open();
            String sqlCmd = "SELECT * FROM " + share;
            MySqlCommand cmd = new MySqlCommand(sqlCmd, sqlConnection);
            reader = cmd.ExecuteReader(); //execure the reader
            while (reader.Read())
            {
               Quote quote = new Quote();
               DateTime parsedDate;
               DateTime.TryParse(reader.GetString(0) as string, out parsedDate);
               quote.Date = parsedDate;
               quote.Open = Convert.ToDouble(reader.GetString(1));
               quote.High = Convert.ToDouble(reader.GetString(2));
               quote.Low = Convert.ToDouble(reader.GetString(3));
               quote.Close = Convert.ToDouble(reader.GetString(4));
               quote.Volume = Convert.ToDouble(reader.GetString(5));
               quoteList.Add(quote);

               closeList.Add(quote.Close);
            }
         }
         catch (MySqlException err)
         {
            Console.WriteLine("Error: " + err.ToString());
         }
         finally
         {
            if (reader != null)
            {
               reader.Close();
            }
            if (sqlConnection != null)
            {
               sqlConnection.Close();
            }
         }

         IEnumerableClosePriceList = closeList;
         return quoteList;
      }

      public bool AddEod(string name, string date, string open, string high, string low, string close, string volume)
      {
         string sqlCmd = @"CREATE TABLE `jse_eod`.`" + name + @"` (`Date` DATE NOT NULL, `Open` DECIMAL(6,0) NULL,`High` DECIMAL(6,0) NULL, `Low` DECIMAL(6,0) NULL,`Close` DECIMAL(6,0) NULL,`Volume` VARCHAR(10) NULL,PRIMARY KEY (`Date`))";
         sqlConnection.Open();

         MySqlDataReader reader = null;
         String sqlCmdString;
         try
         {
            sqlCmdString = @"SELECT * FROM " + name;
            MySqlCommand cmd = new MySqlCommand(sqlCmdString, sqlConnection);
            reader = cmd.ExecuteReader(); //execure the reader
         }
         catch (MySqlException err)
         {
            MySqlCommand cmd1 = new MySqlCommand(sqlCmd, sqlConnection);
            cmd1.ExecuteNonQuery();
         }
         finally
         {
            if (reader != null)
            {
               reader.Close();
            }
            sqlCmdString = @"INSERT INTO `jse_eod`.`" + name + "` (`Date`, `Open`, `High`, `Low`, `Close`, `Volume`) VALUES ('" +
                   date + "', '" + open + "', '" + high + "', '" + low + "', '" + close + "', '" + volume + "')";
            MySqlCommand cmd = new MySqlCommand(sqlCmdString, sqlConnection);
            try
            {
               cmd.ExecuteNonQuery();
            }
            catch
            {
               // to update
            }
            if (sqlConnection != null)
            {
               sqlConnection.Close();
            }
         }

         return true;
      }
   }

   #region ArgClasses
   /*
 * The annotations are stored in the `Charter` schema of the MySql database:
 * The table holds all the annotation types for all shares
 * Each annotation type uses jsut the columns required by it. Some colum names thus don't tie up with the annotation names
 *                
 * charter.annotations columns:
 * -----------------------------------------------------------------------
 * 0  string(5) Sharecode      ArrowLine  LinearInfinite  Horizontal  Text  NewHorizontalLine
 * 1  int       id
 * 2  string(5) chart
 * -----------------------------------------------------------------------
 * 3  int        Type           1          2              3          4      5
 * 4  double     StartX         startX                    textX            MinimumX
 * 5  double     StartY         startY                    textY            MaximumX
 * 6  double     EndX           endX                                       intercept
 * 7  double     EndY           endY                                       slope
 * 8  double     Color          Color                                      Color
 * 9  double     Intercept                  intercept                      
 * 10 double     MinimumX                   minimumX                       
 * 11 double     Slope                      slope
 * 12 string(45) Text                                     text             Text
 * 13 string(15) Font                                     Font             Font
 * 14 int        FontSize                                 FontSize         FontSize
 * 15 int        LineType                                                  LineStyle
 * 16 int        StrokeThickness                                                 StrokeThickness
 * 17 int        LineAnnotationStyle                                       LineAnnotationStyle
 * 
*/
   public class ArrowLineAnnotationArgs
   {
      public ArrowLineAnnotationArgs(MySqlDataReader reader)
      {
         double x;
         double y;

         if (!reader.IsDBNull((int)AnnotationColumn.StartX))
            x = MySqlDbAccess.MySqlValue(reader.GetDouble((int)AnnotationColumn.StartX));
         else
            x = 0;

         if (!reader.IsDBNull((int)AnnotationColumn.StartY))
            y = MySqlDbAccess.MySqlValue(reader.GetDouble((int)AnnotationColumn.StartY));
         else
            y = 0;
         StartPoint = new DataPoint(x, y);

         if (!reader.IsDBNull((int)AnnotationColumn.EndX))
            x = MySqlDbAccess.MySqlValue(reader.GetDouble((int)AnnotationColumn.EndX));
         else
            x = 0;

         if (!reader.IsDBNull((int)AnnotationColumn.EndY))
            y = MySqlDbAccess.MySqlValue(reader.GetDouble((int)AnnotationColumn.EndY));
         else
            y = 0;
         EndPoint = new DataPoint(x, y);

         if (!reader.IsDBNull((int)AnnotationColumn.Color))
            Color = OxyColor.FromUInt32((uint)reader.GetDouble((int)AnnotationColumn.Color));
         else
            Color = OxyColors.Blue;

         if (!reader.IsDBNull((int)AnnotationColumn.Text))
            Text = reader.GetString((int)AnnotationColumn.Text);
         else
            Text = null;

         if (!reader.IsDBNull((int)AnnotationColumn.Font))
            Font = reader.GetString((int)AnnotationColumn.Font);
         else
            Font = null;

         if (!reader.IsDBNull((int)AnnotationColumn.FontSize))
            FontSize = reader.GetInt32((int)AnnotationColumn.FontSize);
         else
            FontSize = Double.NaN;

         if (!reader.IsDBNull((int)AnnotationColumn.LineStyle))
            LineStyle = (LineStyle)reader.GetInt32((int)AnnotationColumn.LineStyle);
         else
            LineStyle = LineStyle.Dash;

         if (!reader.IsDBNull((int)AnnotationColumn.StrokeThickeness))
            StrokeThickness = reader.GetInt32((int)AnnotationColumn.StrokeThickeness);
         else
            StrokeThickness = 1;
      }

      public DataPoint StartPoint { get; set; }
      public DataPoint EndPoint { get; set; }
      public OxyColor Color { get; set; }
      public string Text { get; set; }
      public string Font { get; set; }
      public double FontSize { get; set; }
      public LineStyle LineStyle { get; set; }
      public int StrokeThickness { get; set; }
   }

   public class LDTAnnotationArgs
   {
      public LDTAnnotationArgs(MySqlDataReader reader)
      {
         if (!reader.IsDBNull((int)AnnotationColumn.Color))
            Color = OxyColor.FromUInt32((uint)reader.GetDouble((int)AnnotationColumn.Color));
         else
            Color = OxyColors.Blue;

         if (!reader.IsDBNull((int)AnnotationColumn.StartX))
            StartX = MySqlDbAccess.MySqlValue(reader.GetDouble((int)AnnotationColumn.StartX));
         else
            StartX = Double.MinValue;

         if (!reader.IsDBNull((int)AnnotationColumn.StartY))
            StartY = MySqlDbAccess.MySqlValue(reader.GetDouble((int)AnnotationColumn.StartY));
         else
            StartY = Double.MinValue;

         if (!reader.IsDBNull((int)AnnotationColumn.EndX))
            EndX = MySqlDbAccess.MySqlValue(reader.GetDouble((int)AnnotationColumn.EndX));
         else
            EndX = Double.MinValue;

         if (!reader.IsDBNull((int)AnnotationColumn.EndY))
            EndY = MySqlDbAccess.MySqlValue(reader.GetDouble((int)AnnotationColumn.EndY));
         else
            EndY = Double.MaxValue;

         if (!reader.IsDBNull((int)AnnotationColumn.Text))
            Text = reader.GetString((int)AnnotationColumn.Text);
         else
            Text = null;

         if (!reader.IsDBNull((int)AnnotationColumn.Font))
            Font = reader.GetString((int)(AnnotationColumn.Font));
         else
            Font = null;
         if (!reader.IsDBNull((int)AnnotationColumn.FontSize))
            FontSize = reader.GetInt32((int)AnnotationColumn.FontSize);
         else
            FontSize = Double.NaN;

         if (!reader.IsDBNull((int)AnnotationColumn.LineStyle))
            LineStyle = (LineStyle)reader.GetInt32((int)AnnotationColumn.LineStyle);
         else
            LineStyle = LineStyle.Dash;

         if (!reader.IsDBNull((int)AnnotationColumn.StrokeThickeness))
            StrokeThickness = reader.GetInt32((int)AnnotationColumn.StrokeThickeness);
         else
            StrokeThickness = 1;

         StartPoint = new DataPoint(StartX, StartY);
         EndPoint = new DataPoint(EndX, EndY);
      }

      public double StartX { get; set; }
      public double StartY { get; set; }
      public DataPoint StartPoint { get; set; }
      public DataPoint EndPoint { get; set; }
      public double EndX { get; set; }
      public double EndY { get; set; }
      public OxyColor Color { get; set; }
      public string Text { get; set; }
      public string Font { get; set; }
      public double FontSize { get; set; }
      public LineStyle LineStyle { get; set; }
      public int StrokeThickness { get; set; }
   }

   public class LineAnnotationArgs
   {
      public LineAnnotationArgs(MySqlDataReader reader)
      {
         if (!reader.IsDBNull((int)AnnotationColumn.Color))
            this.Color = OxyColor.FromUInt32((uint)reader.GetDouble((int)AnnotationColumn.Color));
         else
            this.Color = OxyColors.Blue;

         if (!reader.IsDBNull((int)AnnotationColumn.Intercept))
            this.Intercept = reader.GetDouble((int)AnnotationColumn.Intercept);
         else
            this.Intercept = 0.0;

         if (!reader.IsDBNull((int)AnnotationColumn.MinimumX))
            this.MinimumX = MySqlDbAccess.MySqlValue(reader.GetDouble((int)AnnotationColumn.MinimumX));
         else
            this.MinimumX = Double.MinValue;

         if (!reader.IsDBNull((int)AnnotationColumn.MaximumX))
            this.MaximumX = MySqlDbAccess.MySqlValue(reader.GetDouble((int)AnnotationColumn.MaximumX));
         else
            this.MaximumX = Double.MaxValue;

         if (!reader.IsDBNull((int)AnnotationColumn.Slope))
            this.Slope = reader.GetDouble((int)AnnotationColumn.Slope);
         else
            this.Slope = 0.0;

         if (!reader.IsDBNull((int)AnnotationColumn.Text))
            this.Text = reader.GetString((int)AnnotationColumn.Text);
         else
            this.Text = null;

         if (!reader.IsDBNull(14))
            this.Font = reader.GetString(14);
         else
            this.Font = null;

         if (!reader.IsDBNull((int)AnnotationColumn.FontSize))
            this.FontSize = reader.GetInt32((int)AnnotationColumn.FontSize);
         else
            this.FontSize = Double.NaN;

         if (!reader.IsDBNull((int)AnnotationColumn.LineStyle))
            this.LineStyle = (LineStyle)reader.GetInt32((int)AnnotationColumn.LineStyle);
         else
            this.LineStyle = LineStyle.Dash;

         if (!reader.IsDBNull((int)AnnotationColumn.StrokeThickeness))
            this.StrokeThickness = reader.GetInt32((int)AnnotationColumn.StrokeThickeness);
         else
            this.StrokeThickness = 1;

         if (!reader.IsDBNull((int)AnnotationColumn.LineType))
            this.Type = (LineAnnotationType)reader.GetDouble((int)AnnotationColumn.LineType);
         else
            this.Type = LineAnnotationType.LinearEquation;

         if (!reader.IsDBNull((int)AnnotationColumn.X))
            this.X = reader.GetDouble((int)AnnotationColumn.X);
         else
            this.X = 0.0;

         if (!reader.IsDBNull((int)AnnotationColumn.Y))
            this.Y = reader.GetDouble((int)AnnotationColumn.Y);
         else
            this.Y = 0.0;

         if (!reader.IsDBNull((int)AnnotationColumn.MinimumY))
            this.MinimumY = MySqlDbAccess.MySqlValue(reader.GetDouble((int)AnnotationColumn.MinimumY));
         else
            this.MinimumY = Double.MinValue;

         if (!reader.IsDBNull((int)AnnotationColumn.MaximumY))
            this.MaximumY = MySqlDbAccess.MySqlValue(reader.GetDouble((int)AnnotationColumn.MaximumY));
         else
            this.MaximumY = Double.MaxValue;

      }

      public double MinimumX { get; set; }
      public double MaximumX { get; set; }
      public double MinimumY { get; set; }
      public double MaximumY { get; set; }
      public double Intercept { get; set; }
      public double Slope { get; set; }
      public OxyColor Color { get; set; }
      public string Text { get; set; }
      public string Font { get; set; }
      public double FontSize { get; set; }
      public LineStyle LineStyle { get; set; }
      public int StrokeThickness { get; set; }
      public LineAnnotationType Type { get; set; }
      public double X { get; set; }
      public double Y { get; set; }
   }

   public class TextAnnotationArgs
   {
      public TextAnnotationArgs(MySqlDataReader reader)
      {
         double x;
         double y;

         if (!reader.IsDBNull((int)AnnotationColumn.StartX))
            x = MySqlDbAccess.MySqlValue(reader.GetDouble((int)AnnotationColumn.StartX));
         else
            x = 0;

         if (!reader.IsDBNull((int)AnnotationColumn.StartY))
            y = MySqlDbAccess.MySqlValue(reader.GetDouble((int)AnnotationColumn.StartY));
         else
            y = 0;

         Position = new DataPoint(x, y);
         if (!reader.IsDBNull((int)AnnotationColumn.Color))
            Color = OxyColor.FromUInt32((uint)reader.GetDouble((int)AnnotationColumn.Color));
         else
            Color = OxyColors.Blue;

         if (!reader.IsDBNull((int)AnnotationColumn.Text))
            Text = reader.GetString((int)AnnotationColumn.Text);

         if (!reader.IsDBNull((int)AnnotationColumn.Font))
            Font = reader.GetString((int)AnnotationColumn.Font);
         else
            Font = "Arial";

         if (!reader.IsDBNull((int)AnnotationColumn.FontSize))
            FontSize = reader.GetInt32((int)AnnotationColumn.FontSize);
         else
            FontSize = 12;
      }

      public OxyColor Color { get; set; }
      public string Text { get; set; }
      public string Font { get; set; }
      public int FontSize { get; set; }
      public LineStyle LineStyle { get; set; }
      public int StrokeThickness { get; set; }
      public DataPoint Position { get; set; }
   }
   #endregion
}
