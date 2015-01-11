/* Demonstrates how to read the 'Excel' file that comes from Standardbank.
 * It is in fact an HTML file with a .xls extension so that Excel gets opened when one double clicks on it, and 
 * then Excel preforms a translation.
 * I use the HtmlAgilityPack: One has to get it through the Pakage Manager, and reference it in the References.
 * • The HtmlDocument class contains a collection of HtmlNodes 
 * • These HtmlNodes can be selected with an XPath query 
 * • The HtmlNodes can then be interrogated attribute by attribute with the HtmlAttribute class. 

 */
namespace ImportSbkHistoricalData
{
   using HtmlAgilityPack;
   using System;
   using System.Data;
   using System.IO;
   using System.Linq;
   using System.Windows;
   using System.Windows.Input;

   enum QuoteField { Date, Close, High, Low, Volume, Deals, Value, Move, DY, EY, PE };
   enum TableField { Date, Open, High, Low, Close, Volume };
   /// <summary>
   /// Interaction logic for MainWindow.xaml
   /// </summary>
   public partial class MainWindow : Window
   {
      public MainWindow()
      {
         InitializeComponent();
      }

      private void btnReadFile_Click(object sender, RoutedEventArgs e)
      {
         Cursor = Cursors.Wait;
         var fullFileName = lvFiles.SelectedItem;
         string fileName = System.IO.Path.GetFileName(fullFileName.ToString());

         // get the share code: First get the longName
         string shareLongName =  fileName.Substring(fileName.IndexOf('-')+1);
         shareLongName = shareLongName.Substring(0, shareLongName.IndexOf('-'));
         MySqlDbAccess shareDb = new MySqlDbAccess("localhost", "charter", "root", "w51532");
         string shareCode = shareDb.GetCode(shareLongName);
         if (shareCode == "xxxx")
         {
            MessageBox.Show("There is no code for " + shareLongName);
               return;
         }

         // get the file into a series of characters
         TextReader histroricalDataTextReader = File.OpenText(fullFileName as string);
         //string shareCode = txtCode.Text;

         // load the file into a HtmlDocument
         HtmlDocument historialDataHtmlDoc = new HtmlDocument();
         historialDataHtmlDoc.Load(histroricalDataTextReader);
         histroricalDataTextReader.Close();

         DataTable historicalDataDataTable = new DataTable();

         // build the data table's columns
         historicalDataDataTable.Columns.Add("Date");
         historicalDataDataTable.Columns.Add("Open");
         historicalDataDataTable.Columns.Add("High");
         historicalDataDataTable.Columns.Add("Low");
         historicalDataDataTable.Columns.Add("Close");
         historicalDataDataTable.Columns.Add("Volume");

         // Get the data into the DataTable (select rows with 'td' elements )
         foreach (var row in historialDataHtmlDoc.DocumentNode.SelectNodes("//tr[td]"))
         {
            string[] cols = row.SelectNodes("td").Select(td => td.InnerText).ToArray();
            string closeString = cols[(int)QuoteField.Close];
            historicalDataDataTable.Rows.Add(cols[(int)QuoteField.Date], 0, cols[(int)QuoteField.High], cols[(int)QuoteField.Low], cols[(int)QuoteField.Close], cols[(int)QuoteField.Volume]);
         }

         // insert 'Open' prices by iterating from the back
         string prevOpen = "";
         for (int rowIdx = historicalDataDataTable.Rows.Count - 1; rowIdx >= 0; rowIdx--)
         {
            DataRow row = historicalDataDataTable.Rows[rowIdx];
            if (rowIdx == historicalDataDataTable.Rows.Count - 1)
               prevOpen = row[(int)TableField.Low] as string;
            row[(int)TableField.Open] = prevOpen;
            prevOpen = row[(int)TableField.Close] as string;
         }

         // write data to the Shares database
         shareDb.AddHistoricalData(shareCode, historicalDataDataTable);

         string to = System.IO.Path.Combine(@"c:\users\Wynand\Documents\Shares\JSEData\Keep", fileName);
         File.Move(fullFileName as string, to);
         lvFiles.Items.Clear();
         string[] files = System.IO.Directory.GetFiles(@"c:\users\wynand\downloads", "StatisticsHistory*.*");

         for (int x = 0; x < files.Length; x++)
         {
            lvFiles.Items.Add(files[x]);
         }
         lvFiles.SelectedIndex = 0;
         Cursor = Cursors.Arrow;
      }

      public static void WriteDataToFile(DataTable dataTable, string fileName)
      {
         int i = 0;
         StreamWriter streamWriter = null;

         streamWriter = new StreamWriter(fileName, false);

         foreach (DataRow row in dataTable.Rows)
         {
            object[] array = row.ItemArray;
            // convert date
            DateTime dt;
            DateTime.TryParse(array[0].ToString(), out dt);

            streamWriter.Write(dt.ToString("yyyyMMdd") + ",");
            for (i = 1; i < array.Length - 1; i++)
            {
               streamWriter.Write(array[i].ToString() + ",");
            }
            streamWriter.Write(array[i].ToString());
            streamWriter.WriteLine();
         }

         streamWriter.Close();
      }

      private void Window_Loaded(object sender, RoutedEventArgs e)
      {
         // fill listview
         lvFiles.Items.Clear();
         string[] files = System.IO.Directory.GetFiles(@"c:\users\wynand\downloads", "StatisticsHistory*.*");

         for (int x = 0; x < files.Length; x++)
         {
            lvFiles.Items.Add(files[x]);
         }
      }

      private void txtCode_KeyDown(object sender, KeyEventArgs e)
      {
         if (e.Key == Key.Enter)
            btnReadFile_Click(this, null);
      }
   }
}
