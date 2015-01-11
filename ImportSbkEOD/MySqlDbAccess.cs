// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DbAccessClass1.cs" company="Wynand">
//   
//   Permission is hereby granted, free of charge, to any person obtaining a
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
namespace ImportSbkEOD
{
   using System;
   using System.Collections.Generic;
   using System.Linq;
   using System.Text;
   using System.Threading.Tasks;

   /// <summary>
   /// TODO: This class provides
   /// </summary>
   class MySqlDbAccess
   {
      public bool Connected = false;
      //private MySqlConnection sqlConnection = null;
      public string ConnectingString = "";

      /// <summary>
      /// This constructor also opens the connection to the data base
      /// </summary>
      /// <param name="server"></param>
      /// <param name="dataBaseName"></param>
      /// <param name="user"></param>
      /// <param name="password"></param>
      public MySqlDbAccess(string server, string dataBaseName, string user, string password)
      {
         ConnectingString = @"server=" + server + ";database=" + @dataBaseName + ";userid=" + user + ";password=" + @password + ";";
         /*try
         {
            //sqlConnection = new MySqlConnection(ConnectingString);
            Connected = true;
         }*/
         /*catch (MySqlException err)
         {
            Connected = false;
            Console.WriteLine("Error: " + err.ToString());
         }*/
      }

      /// <summary>
      /// My destructor to ensure that the connection is always closed
      /// </summary>
      ~MySqlDbAccess()
      {
         /*if (sqlConnection != null)
         {
            sqlConnection.Close();
            Connected = false;
         }*/
      }

      public bool AddEod(string name, string date, string open, string high, string low, string close, string volume)
      {
         string sqlCmd = @"CREATE TABLE `jse_eod`.`" + name + @"` (`Date` DATE NOT NULL, `Open` DECIMAL(6,0) NULL,`High` DECIMAL(6,0) NULL, `Low` DECIMAL(6,0) NULL,`Close` DECIMAL(6,0) NULL,`Volume` VARCHAR(10) NULL,PRIMARY KEY (`Date`))";
         //sqlConnection.Open();

         /*MySqlDataReader reader = null;
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
            // UPDATE `jse_eod`.`csb` SET `Open`='12', `High`='123', `Low`='0456', `Close`='1256', `Volume`='01234' WHERE `Date`='2014-08-13';
            //// sqlCmdString = @"UPDATE `jse_eod`.`" + name + "` SET `Open`='" + open + @"', `High`='" + high + @"', `Low`='" + low + @"', `Close`='" + close + @"', `Volume`='" + volume + "' WHERE `Date`='2014-08-13';";
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
         }*/

         return true;
      }

   }
}
