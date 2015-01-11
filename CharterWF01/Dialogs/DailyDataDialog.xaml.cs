namespace CharterWF
{
   using MySqlAccess;
   using System;
   using System.Windows;

   /// <summary>
   /// Extracts the daily data from the selection I make of the SBK webpage.
   /// To insert into my database
   /// </summary>
   public partial class DailyDataDialog : Window
   {
      public DailyDataDialog()
      {
         InitializeComponent();
      }

      private EOD GetClibBoard()
      {
         EOD Result = new EOD();
         if (Clipboard.ContainsText(TextDataFormat.Html))
         {
            String html = Clipboard.GetText(TextDataFormat.Html);

            int cashIdx = html.IndexOf("Cash Balance:");
            int idx = html.IndexOf("EN\">R", cashIdx) + 6;
            int end = html.IndexOf('<', idx);
            string str = html.Substring(idx, end - idx);
            str = str.Replace(",", "");
            Result.MoneyMarketCashBalance = Convert.ToDouble(str);

            // get Profit/Loss
            idx = html.IndexOf("Today", idx);
            idx = html.IndexOf(">R", idx) + 3;
            end = html.IndexOf('<', idx);
            str = html.Substring(idx, end - idx);
            str = str.Replace(",", "");
            Result.Profit = Convert.ToDouble(str);

            // get Total Cash
            idx = html.IndexOf("Total", idx);
            idx = html.IndexOf("EN\">R", idx) + 6;
            end = html.IndexOf('<', idx);
            str = html.Substring(idx, end - idx);
            str = str.Replace(",", "");
            Result.TotalCash = Convert.ToDouble(str);

            // get PORTFOLIO VALUE:
            idx = html.IndexOf("PORTFOLIO", idx);
            idx = html.IndexOf(">R", idx) + 3;
            end = html.IndexOf('<', idx);
            str = html.Substring(idx, end - idx);
            str = str.Replace(",", "");
            Result.PortfolioValue = Convert.ToDouble(str);

            // get Available trading funds:
            idx = html.IndexOf("Available ", idx);
            str = html.Substring(idx, 100);
            idx = html.IndexOf(">R", idx) + 3;
            end = html.IndexOf('<', idx);
            str = html.Substring(idx, end - idx);
            str = str.Replace(",", "");
            Result.AvailableTradingFunds = Convert.ToDouble(str);
         }
         return Result;
             
      }

      private void TestButton_Click(object sender, RoutedEventArgs e)
      {
         EOD rina = GetClibBoard();
         String html = "";
         if (Clipboard.ContainsText(TextDataFormat.Html))
         {
            html = Clipboard.GetText(TextDataFormat.Html);

            int cashIdx = html.IndexOf("Money Market Cash Balance:");
            int idx = html.IndexOf("EN\">R", cashIdx) + 6;
            string moneymarketCashBalanceStr = html.Substring(idx, 10);
            double moneyMarketCashBalance = Convert.ToDouble(moneymarketCashBalanceStr);

            // get Profit/Loss
            idx = html.IndexOf("Today", idx);
            idx = html.IndexOf(">R", idx) + 3;
            int end = html.IndexOf('<', idx);
            string str = html.Substring(idx, end - idx);
            double profit = Convert.ToDouble(str);

            // get Total Cash
            idx = html.IndexOf("Total", idx);
            idx = html.IndexOf("EN\">R", idx) + 6;
            end = html.IndexOf('<', idx);
            str = html.Substring(idx, end - idx);
            double totalCash = Convert.ToDouble(str);

            // get PORTFOLIO VALUE:
            idx = html.IndexOf("PORTFOLIO", idx);
            idx = html.IndexOf(">R", idx) + 3;
            end = html.IndexOf('<', idx);
            str = html.Substring(idx, end - idx);
            double portfolioValue = Convert.ToDouble(str);

            // get Available trading funds:
            idx = html.IndexOf("Available ", idx);
            str = html.Substring(idx, 100);
            idx = html.IndexOf(">R", idx) + 3;
            end = html.IndexOf('<', idx);
            str = html.Substring(idx, end - idx);
            double availableTradingFunds = Convert.ToDouble(str);
         }
      }

      private void ImportRina(object sender, RoutedEventArgs e)
      {
         EOD rina = GetClibBoard();
         RinaATF.Text = rina.AvailableTradingFunds.ToString();
         RinaDPL.Text = rina.Profit.ToString();
         RinaMMCB.Text = rina.MoneyMarketCashBalance.ToString();
         RinaPV.Text = rina.PortfolioValue.ToString();
         RinaTC.Text = rina.TotalCash.ToString();
      }

      private void ImportWynand(object sender, RoutedEventArgs e)
      {
         EOD eod = GetClibBoard();
         WynandATF.Text = eod.AvailableTradingFunds.ToString();
         WynandDPL.Text = eod.Profit.ToString();
         WynandMMCB.Text = eod.MoneyMarketCashBalance.ToString();
         WynandPV.Text = eod.PortfolioValue.ToString();
         WynandTC.Text = eod.TotalCash.ToString();
      }

      private void MMCB_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
      {
         double r;
         double w;
         try { r = Convert.ToDouble(RinaMMCB.Text); }
         catch { r = 0; }
         try { w = Convert.ToDouble(WynandMMCB.Text); }
         catch { w = 0; }
         TotalMMCB.Text = (r + w).ToString();
      }

      private void PL_Changed(object sender, System.Windows.Controls.TextChangedEventArgs e)
      {
         double r;
         double w;
         try { r = Convert.ToDouble(RinaDPL.Text); }
         catch { r = 0; }
         try { w = Convert.ToDouble(WynandDPL.Text); }
         catch { w = 0; }
         TotalDPL.Text = (r + w).ToString();
      }

      private void TC_Changed(object sender, System.Windows.Controls.TextChangedEventArgs e)
      {
         double r;
         double w;
         try { r = Convert.ToDouble(RinaTC.Text); }
         catch { r = 0; }
         try { w = Convert.ToDouble(WynandTC.Text); }
         catch { w = 0; }
         TotalTC.Text = (r + w).ToString();
      }

      private void PV_Changed(object sender, System.Windows.Controls.TextChangedEventArgs e)
      {
         double r;
         double w;
         try { r = Convert.ToDouble(RinaPV.Text); }
         catch { r = 0; }
         try { w = Convert.ToDouble(WynandPV.Text); }
         catch { w = 0; }
         TotalPV.Text = (r + w).ToString();
      }

      private void ATF_Changed(object sender, System.Windows.Controls.TextChangedEventArgs e)
      {
         double r;
         double w;
         try { r = Convert.ToDouble(RinaATF.Text); }
         catch { r = 0; }
         try { w = Convert.ToDouble(WynandATF.Text); }
         catch { w = 0; }
         TotalATF.Text = (r + w).ToString();
      }

      private void WriteToDatabase(object sender, RoutedEventArgs e)
      {
         EOD rina = new EOD();
         rina.TotalCash = Convert.ToDouble(RinaTC.Text);
         rina.AvailableTradingFunds = Convert.ToDouble(RinaATF.Text);
         rina.Profit = Convert.ToDouble(RinaDPL.Text);
         rina.MoneyMarketCashBalance = Convert.ToDouble(RinaMMCB.Text);
         rina.PortfolioValue = Convert.ToDouble(RinaPV.Text);

         EOD wynand = new EOD();
         wynand.TotalCash = Convert.ToDouble(WynandTC.Text);
         wynand.AvailableTradingFunds = Convert.ToDouble(WynandATF.Text);
         wynand.Profit = Convert.ToDouble(WynandDPL.Text);
         wynand.MoneyMarketCashBalance = Convert.ToDouble(WynandMMCB.Text);
         wynand.PortfolioValue = Convert.ToDouble(WynandPV.Text);

         MySqlDbAccess dailyDB = new MySqlDbAccess("localhost", "portfolio", "root", "w51532");
         dailyDB.WriteDailyEOD(rina, wynand);
      }
   }

   class EOD
   {
      /// <summary>
      /// Gets or sets DateTime field
      /// </summary>
    //  public DateTime Date { get; set; }
      public double TotalCash { get; set; }
      public double AvailableTradingFunds { get; set; }
      public double MoneyMarketCashBalance { get; set; }
      public double PortfolioValue { get; set; }
      public double Profit { get; set; }
   }
}
