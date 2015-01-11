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
namespace ImportSbkHistoricalData
{
   //using MySql.Data.MySqlClient;
   using System;
   using System.Collections.Generic;
   using System.Data;
   using System.Linq;
   using System.Text;
   using System.Threading.Tasks;

   /// <summary>
   /// TODO: This class provides
   /// </summary>
   class MySqlDbAccess
   {
      public bool Connected = false;
     // private MySqlConnection sqlConnection = null;
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
         }
         catch (MySqlException err)
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

      /// <summary>
      /// This function fetches the shares code from its long code.
      /// </summary>
      /// <param name="longcode"></param>
      /// <returns></returns>
      public string GetCode(string longcode)
      {
         string code = "xxxx";

         /*MySqlDataReader reader = null;
         try
         {
            sqlConnection.Open();
            String sqlCmd = "SELECT * FROM shares where instrument = '" + longcode + "'";
            MySqlCommand cmd = new MySqlCommand(sqlCmd, sqlConnection);
            reader = cmd.ExecuteReader(); //execure the reader
            while (reader.Read())
            {
               code = reader.GetString(0);
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
         }*/

         return code;
      }

      public void AddHistoricalData(string code, DataTable dataTable)
      {

         //sqlConnection.Open();

         String sqlInsertCmdString;

         /*try
         {
            foreach (DataRow row in dataTable.Rows)
            {
               object[] array = row.ItemArray;

               // convert date
               DateTime quoteDateTime;
               DateTime.TryParse(array[0].ToString(), out quoteDateTime);

               string dateString = quoteDateTime.ToString("yyyy-MM-dd");
               sqlInsertCmdString = @"INSERT INTO `jse_eod`.`" + code + "` (`Date`, `Open`, `High`, `Low`, `Close`, `Volume`) VALUES ('" +
                  dateString + "', '" +
                  array[1].ToString() + "', '" +
                  array[2].ToString() + "', '" +
                  array[3].ToString() + "', '" +
                  array[4].ToString() + "', '" +
                  array[5].ToString() + "')";
               //MySqlCommand sqlInsertCmd = new MySqlCommand(sqlInsertCmdString, sqlConnection);
               try
               {
                  sqlInsertCmd.ExecuteNonQuery();
               }
               catch (MySqlException err)
               {
                  int x = err.ToString().IndexOf("doesn't exist");
                  if (x != -1)
                  {
                     // create and then insert
                     string sqlCreateCmdStr = @"CREATE TABLE `jse_eod`.`" + code + @"` (`Date` DATE NOT NULL, `Open` DECIMAL(6,0) NULL,`High` DECIMAL(6,0) NULL, `Low` DECIMAL(6,0) NULL,`Close` DECIMAL(6,0) NULL,`Volume` VARCHAR(10) NULL,PRIMARY KEY (`Date`))";
                     MySqlCommand sqlCreateCmd = new MySqlCommand(sqlCreateCmdStr, sqlConnection);
                     sqlCreateCmd.ExecuteNonQuery();
                     //AddHistoricalData(name, dataTable);
                     /*foreach (DataRow row1 in dataTable.Rows)
                     {
                        array = row1.ItemArray;

                        // convert date
                        DateTime.TryParse(array[0].ToString(), out quoteDateTime);

                        dateString = quoteDateTime.ToString("yyyy-MM-dd");
                        sqlInsertCmdString = @"INSERT INTO `shares`.`" + code + "` (`Date`, `Open`, `High`, `Low`, `Close`, `Volume`) VALUES ('" +
                           dateString + "', '" +
                           array[1].ToString() + "', '" +
                           array[2].ToString() + "', '" +
                           array[3].ToString() + "', '" +
                           array[4].ToString() + "', '" +
                           array[5].ToString() + "')";
                        sqlInsertCmd = new MySqlCommand(sqlInsertCmdString, sqlConnection);
                        try
                        {
                           sqlInsertCmd.ExecuteNonQuery();
                        }
                        catch (MySqlException err1)
                        {
                           // hopefully the remainders due to a duplicate
                        }
                     }
                     // hopefully the remainders due to a duplicate
                  }
               }
            }
         }*/
      }

      public bool AddEod(string name, string date, string open, string high, string low, string close, string volume)
      {
         string sqlCmd = @"CREATE TABLE `shares`.`" + name + @"` (`Date` DATE NOT NULL, `Open` DECIMAL(6,0) NULL,`High` DECIMAL(6,0) NULL, `Low` DECIMAL(6,0) NULL,`Close` DECIMAL(6,0) NULL,`Volume` VARCHAR(10) NULL,PRIMARY KEY (`Date`))";
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
            sqlCmdString = @"INSERT INTO `shares`.`" + name + "` (`Date`, `Open`, `High`, `Low`, `Close`, `Volume`) VALUES ('" +
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


      /// <summary>
      /// Creates a new end of day data table if it does not yet exist.
      /// </summary>
      /// <param name="name"></param>
      /// <returns></returns>
      public bool CreateEodTable(string name)
      {
         string sqlCmd = @"CREATE TABLE `shares`.`" + name + @"` (`Date` DATE NOT NULL, `Open` DECIMAL(6,0) NULL,`High` DECIMAL(6,0) NULL, `Low` DECIMAL(6,0) NULL,`Close` DECIMAL(6,0) NULL,`Volume` VARCHAR(10) NULL,PRIMARY KEY (`Date`))";
         /*sqlConnection.Open();

         MySqlDataReader reader = null;
         try
         {
            String cmdText = @"SELECT * FROM " + name;
            MySqlCommand cmd = new MySqlCommand(cmdText, sqlConnection);
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
            if (sqlConnection != null)
            {
               sqlConnection.Close();
            }
         }*/

         return true;
      }

      public void Read()
      {
         String str = @"server=localhost;database=yourDBname;userid=root;password=yourDBpassword;";
         /*MySqlConnection con = null;
         MySqlDataReader reader = null;
         try
         {
            con = new MySqlConnection(str);
            con.Open(); //open the connection
            //We will need to SELECT all or some columns in the table
            //via this command
            String cmdText = "SELECT * FROM myTable";
            MySqlCommand cmd = new MySqlCommand(cmdText, con);
            reader = cmd.ExecuteReader(); //execure the reader
            // The Read() method points to the next record It return false if there are no more records else returns true.
            while (reader.Read())
            {
               /*reader.GetString(0) will get the value of the first column of the table myTable because
                  we selected all columns using SELECT * (all); the first loop of the while loop is the first
                  row; the next loop will be the second row and so on...*/
           /*    Console.WriteLine(reader.GetString(0));
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
            if (con != null)
            {
               con.Close(); //close the connection
            }
         } //remember to close the connection after accessing the database
         */
      }

      public void PreparedStatements()
      {
         String str = @"server=localhost;database=yourDBname;userid=root;password=yourDBpassword;";
         /*MySqlConnection con = null;
         try
         {
            con = new MySqlConnection(str);
            con.Open(); //open the connection
            //This is the mysql command that we will query into the db.
            //It uses Prepared statements and the Placeholder is @name.
            //Using prepared statements is faster and secure.
            String cmdText = "INSERT INTO myTable(name) VALUES(@name)";
            MySqlCommand cmd = new MySqlCommand(cmdText, con);
            cmd.Prepare();
            //we will bound a value to the placeholder
            cmd.Parameters.AddWithValue("@name", "your value here");
            cmd.ExecuteNonQuery(); //execute the mysql command
         }
         catch (MySqlException err)
         {
            Console.WriteLine("Error: " + err.ToString());
         }
         finally
         {
            if (con != null)
            {
               con.Close(); //close the connection
            }
         } //remember to close the connection after accessing the database
         */
      }

      /// <summary>
      /// For now just example code of how to read
      /// </summary>
      /// <param name="command"></param>
      public void Reader(string command)
      {
         /*sqlConnection.Open();
         MySqlCommand cmd = new MySqlCommand(command, sqlConnection);
         MySqlDataReader reader = null;
         reader = cmd.ExecuteReader(); //execure the reader
         //The Read() method points to the next record. It return false if there are no more records else returns true.
         while (reader.Read())
         {
            Console.WriteLine(reader.GetString(0) + "," + reader.GetString(1));
         }
         sqlConnection.Close();*/
      }
   }
}
