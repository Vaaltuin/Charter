namespace CharterWF
{
   using MySqlAccess;
   using System;
   using System.Collections.Generic;
   using System.IO;
   using System.Linq;
   using System.Text;
   using System.Threading.Tasks;
   using System.Windows;
   using System.Windows.Controls;
   using System.Windows.Data;
   using System.Windows.Documents;
   using System.Windows.Input;
   using System.Windows.Media;
   using System.Windows.Media.Imaging;
   using System.Windows.Shapes;

   /// <summary>
   /// Interaction logic for EodEntry.xaml
   /// </summary>
   public partial class EodEntryDialog : Window
   {
      public EodEntryDialog()
      {
         InitializeComponent();
      }

      private void ImportButton_Clicked(object sender, RoutedEventArgs e)
      {
         // Take all the files matching the txtSearchCriteria in the cmbFolderPath and import them,
         string fullFileName = System.IO.Path.Combine(cboFolderPath.Text, txtSearchCriteria.Text);
         var folder = new DirectoryInfo(cboFolderPath.Text);

         if (folder.Exists)
         {
            var files = folder.GetFiles(txtSearchCriteria.Text);

            files.ToList().ForEach(file =>
            {
               ImportFile(System.IO.Path.Combine(cboFolderPath.Text, file.ToString()), "date");
               string name = System.IO.Path.GetFileName(file.ToString());
               string to = System.IO.Path.Combine(@"c:\users\Wynand\Documents\Shares\JSEData\Keep", name);
               File.Move(System.IO.Path.Combine(cboFolderPath.Text, file.ToString()), to);
            });
         }

      }

      void ImportFile(string filename, string date)
      {
         var quoteList = new List<string>();
         date = filename.Substring(filename.Length - 17, 8);
         date = date.Insert(4, "-");
         date = date.Insert(7, "-");
         MySqlDbAccess shareDb = new MySqlDbAccess("localhost", "jse_eod", "root", "w51532");

         // Read the file and convert it line by line.
         using (var file = new StreamReader(filename))
         {
            string csvLine;
            bool isHeader = true;
            bool firstDataStyle = false;
            while ((csvLine = file.ReadLine()) != null)
            {
               if (isHeader)
               {
                  isHeader = false;
                  var delimiters = new char[] { ',' }; // split on commas
                  var shareQuotes = csvLine.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                  firstDataStyle = (shareQuotes[0] == "Name");
               }
               else
               {
                  var delimiters = new char[] { ',' }; // split on commas
                  var shareQuotes = csvLine.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

                  if (firstDataStyle)
                     shareDb.AddEod(shareQuotes[1], date, trim(shareQuotes[2]), trim(shareQuotes[3]), trim(shareQuotes[4]), trim(shareQuotes[5]), shareQuotes[6]);
                  else
                     shareDb.AddEod(shareQuotes[0], date, trim(shareQuotes[1]), trim(shareQuotes[2]), trim(shareQuotes[3]), trim(shareQuotes[4]), shareQuotes[5]);
                  // 
                  Console.Write("*");
               }
            }

            file.Close();
         }

      }

      private string trim(string price)
      {
         return price.Substring(0, price.Length - 1);
      }

      private double ExtractDouble(string str)
      {
         string valueStr = str.Substring(0, str.Length - 1);
         return Convert.ToDouble(valueStr);
      }

      private void ClearlvFiles(object sender, RoutedEventArgs e)
      {
         lvFiles.Items.Clear();
      }

      private void SearchButton_Clicked(object sender, RoutedEventArgs e)
      {
         lvFiles.Items.Clear();
         //lstFilesFound.Items.Clear();
         txtSearchCriteria.IsEnabled = false;
         cboFolderPath.IsEnabled = false;
         btnSearch.Content = "Searching...";
         // this.Cursor = Cursors.WaitCursor;
         //Application.DoEvents();
         SearchInDirectory(cboFolderPath.Text, txtSearchCriteria.Text, false);
         btnSearch.Content = "Search";
         //this.Cursor = Cursors.Default;
         txtSearchCriteria.IsEnabled = true;
         cboFolderPath.IsEnabled = true;
      }

      /// <summary>
      /// This function imports data from annotation given folder
      /// </summary>
      /// <param name="searchDir"></param>
      /// <param name="searchCriteria">Can contain wildcards</param>
      /// <param name="seachSubFolders">When true the search is or all subfolders</param>
      void SearchInDirectory(string searchDir, string searchCriteria, bool seachSubFolders)
      {
         if (seachSubFolders)
         {
            try
            {
               foreach (string directory in Directory.GetDirectories(searchDir))
               {
                  foreach (string fileName in Directory.GetFiles(directory, searchCriteria))
                  {
                     lvFiles.Items.Add(fileName);
                  }
                  SearchInDirectory(directory, searchCriteria, seachSubFolders);
               }
            }
            catch (System.Exception excption)
            {
               Console.WriteLine(excption.Message);
            }
         }
         else
         {
            try
            {
               foreach (string fileName in Directory.GetFiles(searchDir, searchCriteria))
               {
                  lvFiles.Items.Add(fileName);
               }
            }
            catch (System.Exception excption)
            {
               Console.WriteLine(excption.Message);
            }
         }
      }
   }
}
