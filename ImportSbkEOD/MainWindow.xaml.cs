namespace ImportSbkEOD
{
   using System;
   using System.Collections.Generic;
   using System.Data;
   using System.IO;
   using System.Linq;
   using System.Windows;

   /// <summary>
   /// Interaction logic for MainWindow.xaml
   /// </summary>
   public partial class MainWindow : Window
   {
      static private DataTable table = new DataTable();

      public MainWindow()
      {
         InitializeComponent();
      }

      private void btnImport_Click(object sender, RoutedEventArgs e)
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

      private void btnSearch_Click(object sender, RoutedEventArgs e)
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
      /// This function imports data from a given folder
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

      void ImportDir(string searchDir, string searchPattern)
      {
         try
         {
            foreach (string directory in Directory.GetDirectories(searchDir))
            {
               foreach (string fileName in Directory.GetFiles(directory, searchPattern))
               {
                  lvFiles.Items.Add(fileName);
                  string date = "2014-07-28";
                  ImportFile(fileName, date);
                  // to delete the file - only once I am happy that everything is working fine (by checking in MySQL)
               }
               ImportDir(directory, searchPattern);
            }
         }
         catch (System.Exception excption)
         {
            Console.WriteLine(excption.Message);
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
   }
}
