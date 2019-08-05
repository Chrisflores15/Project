using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Data;

namespace ProjectTemplate
{
	[WebService(Namespace = "http://tempuri.org/")]
	[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
	[System.ComponentModel.ToolboxItem(false)]
	[System.Web.Script.Services.ScriptService]

	public class ProjectServices : System.Web.Services.WebService
	{
		private string dbID = "runtime";
		private string dbPass = "!!Cis440";
		private string dbName = "runtime";

        private string getConString() {
			return "SERVER=107.180.1.16; PORT=3306; DATABASE=" + dbName+"; UID=" + dbID + "; PASSWORD=" + dbPass;
		}

        [WebMethod(EnableSession = true)]
        public CurrentLogon LogOn(string uid, string pass)
        {
            //LOGIC: pass the parameters into the database to see if an account
            //with these credentials exist.  If it does, then return true.  If
            //it doesn't, then return false

            CurrentLogon currentLogon = new CurrentLogon();
            currentLogon.success = false;
            currentLogon.adminFlag = false;

            string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;

            string sqlSelect = "SELECT employeeId, adminFlag FROM Account WHERE employeeId=@idValue and AcctPassword=@passValue";

            //set up our connection object to be ready to use our connection string
            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            //set up our command object to use our connection, and our query
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            //tell our command to replace the @parameters with real values
            //we decode them because they came to us via the web so they were encoded
            //for transmission (funky characters escaped, mostly)
            sqlCommand.Parameters.AddWithValue("@idValue", HttpUtility.UrlDecode(uid));
            sqlCommand.Parameters.AddWithValue("@passValue", HttpUtility.UrlDecode(pass));

            //a data adapter acts like a bridge between our command object and 
            //the data we are trying to get back and put in a table object
            MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
            //here's the table we want to fill with the results from our query
            DataTable sqlDt = new DataTable();
            //here we go filling it!
            sqlDa.Fill(sqlDt);
            //check to see if any rows were returned.  If they were, it means it's 
            //a legit account
            if (sqlDt.Rows.Count > 0)
            {
                //flip our flag to true so we return a value that lets them know they're logged in
                currentLogon.success = true;
                currentLogon.employeeId = sqlDt.Rows[0]["EmployeeId"].ToString();
                if (sqlDt.Rows[0]["AdminFlag"].ToString().ToUpper() == "X")
                {
                    currentLogon.adminFlag = true;
                }

            }
            
            return currentLogon;
        }

        [WebMethod(EnableSession = true)]
        public PendingAccountRequest[] GetAccountRequests()
        {//LOGIC: get all account requests and return them!
            DataTable sqlDt = new DataTable("accountrequests");

            string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
            
            string sqlSelect = "SELECT EmployeeId, AcctPassword, FirstName, LastName, Email, RequestDt FROM runtime.AccountRequest WHERE AccountApproval IS NULL ORDER BY RequestDt";

            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
            sqlDa.Fill(sqlDt);

            List<PendingAccountRequest> accountRequests = new List<PendingAccountRequest>();
            for (int i = 0; i < sqlDt.Rows.Count; i++)
            {
                accountRequests.Add(new PendingAccountRequest
                {
                    employeeId = sqlDt.Rows[i]["EmployeeId"].ToString(),
                    acctPassword = sqlDt.Rows[i]["AcctPassword"].ToString(),
                    firstName = sqlDt.Rows[i]["FirstName"].ToString(),
                    lastName = sqlDt.Rows[i]["LastName"].ToString(),
                    email = sqlDt.Rows[i]["Email"].ToString(),
                    requestDt = sqlDt.Rows[i]["RequestDt"].ToString()
                });
            }
            //convert the list of accounts to an array and return!
            return accountRequests.ToArray();
        }

        [WebMethod(EnableSession = true)]
        public PendingAccountRequest GetAccountRequestDetails(string employeeId)
        {
            PendingAccountRequest pendingAccount = new PendingAccountRequest();
            
            string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;

            string sql = "SELECT * FROM AccountRequest WHERE EmployeeId=@idValue";

            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            sqlConnection.Open();

            MySqlCommand sqlCommand = new MySqlCommand(sql, sqlConnection);

            sqlCommand.Parameters.AddWithValue("@idValue", HttpUtility.UrlDecode(employeeId));

            try
            {
                MySqlDataReader dr;
                dr = sqlCommand.ExecuteReader();
                while (dr.Read())
                {
                    pendingAccount.employeeId = dr.GetString("EmployeeID");
                    pendingAccount.acctPassword = dr.GetString("AcctPassword");
                    pendingAccount.email = dr.GetString("Email");
                    pendingAccount.firstName = dr.GetString("FirstName");
                    pendingAccount.lastName = dr.GetString("LastName");
                }
                dr.Close();
            }
            catch (Exception)
            {
                
            }
            finally
            {
                if (sqlConnection.State == ConnectionState.Open)
                {
                    sqlConnection.Close();
                }
            }
            return pendingAccount;
        }

        [WebMethod(EnableSession = true)]
        public string DenyAccountRequest(string employeeId)
        {
            string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
            MySqlConnection conn = new MySqlConnection(connStr);
            string sql = "DELETE FROM AccountRequest WHERE EmployeeId=@idValue";

            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@idValue", HttpUtility.UrlDecode(employeeId));

            try
            {
                conn.Open();

                cmd.ExecuteNonQuery();

                return "account denied success";
            }
            catch (Exception)
            {
                return "account denied failed";
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }

        [WebMethod(EnableSession = true)]
        public string ApproveAccountRequest(string employeeId)
        {
            string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
            MySqlConnection conn = new MySqlConnection(connStr);
            string sql = "INSERT INTO Account(EmployeeId, AcctPassword, FirstName, LastName, Email) SELECT EmployeeId, AcctPassword, FirstName, LastName, Email FROM AccountRequest WHERE AccountRequest.EmployeeId=@idValue";
            string sql2 = "DELETE FROM AccountRequest WHERE EmployeeId=@idValue";

            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlCommand cmd2 = new MySqlCommand(sql2, conn);
            cmd.Parameters.AddWithValue("@idValue", HttpUtility.UrlDecode(employeeId));
            cmd2.Parameters.AddWithValue("@idValue", HttpUtility.UrlDecode(employeeId));

            try
            {
                conn.Open();

                cmd.ExecuteNonQuery();
                cmd2.ExecuteNonQuery();

                return "account approval success";
            }
            catch (Exception)
            {
                return "account approval failed";
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }

        [WebMethod(EnableSession = true)]
        public string AccountRequest(string employeeId, string password, string fName, string lName, string email)
        {
            string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
            MySqlConnection conn = new MySqlConnection(connStr);
            string sql = "INSERT INTO AccountRequest (EmployeeId, AcctPassword, FirstName, LastName, Email) VALUES (@idValue, @pwValue, @fnValue, @lnValue, @emValue);";

            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@idValue", HttpUtility.UrlDecode(employeeId));
            cmd.Parameters.AddWithValue("@pwValue", HttpUtility.UrlDecode(password));
            cmd.Parameters.AddWithValue("@fnValue", HttpUtility.UrlDecode(fName));
            cmd.Parameters.AddWithValue("@lnValue", HttpUtility.UrlDecode(lName));
            cmd.Parameters.AddWithValue("@emValue", HttpUtility.UrlDecode(email));

            try
            {
                conn.Open();

                cmd.ExecuteNonQuery();

                return "account request success";
            }
            catch (Exception)
            {
                return "account request failed";
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }

        [WebMethod(EnableSession = true)]
        public string EditProfile(string employeeId, string password, string fName, string lName, string email)
        {
            string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
            MySqlConnection conn = new MySqlConnection(connStr);
            string sql = "UPDATE Account SET EmployeeId=@idValue, AcctPassword=@pwValue, FirstName=@fnValue, LastName=@lnValue, Email=@emValue WHERE EmployeeId=@idValue;";

            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@idValue", HttpUtility.UrlDecode(employeeId));
            cmd.Parameters.AddWithValue("@pwValue", HttpUtility.UrlDecode(password));
            cmd.Parameters.AddWithValue("@fnValue", HttpUtility.UrlDecode(fName));
            cmd.Parameters.AddWithValue("@lnValue", HttpUtility.UrlDecode(lName));
            cmd.Parameters.AddWithValue("@emValue", HttpUtility.UrlDecode(email));

            try
            {
                conn.Open();

                cmd.ExecuteNonQuery();

                return "edit profile success";
            }
            catch (Exception)
            {
                return "edit profile failed";
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }
        
        [WebMethod(EnableSession = true)]
        public Account GetAccount(string employeeId)
        {
            Account currentAccount = new Account();

            string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;

            string sql = "SELECT * FROM Account WHERE EmployeeId=@idValue";

            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            sqlConnection.Open();

            MySqlCommand sqlCommand = new MySqlCommand(sql, sqlConnection);

            sqlCommand.Parameters.AddWithValue("@idValue", HttpUtility.UrlDecode(employeeId));

            try
            {
                MySqlDataReader dr;
                dr = sqlCommand.ExecuteReader();
                while (dr.Read())
                {
                    currentAccount.employeeId = dr.GetString("EmployeeID");
                    currentAccount.password = dr.GetString("AcctPassword");
                    currentAccount.firstName = dr.GetString("FirstName");
                    currentAccount.lastName = dr.GetString("LastName");
                    currentAccount.email = dr.GetString("Email");
                    currentAccount.adminFlag = Convert.ToBoolean(dr.GetString("AdminFlag"));
                    currentAccount.disableFlag = Convert.ToBoolean(dr.GetString("DisableFlag"));
                    currentAccount.disableCount = Convert.ToInt32(dr.GetString("DisableCount"));
                }
                dr.Close();
            }
            catch (Exception)
            {

            }
            finally
            {
                if (sqlConnection.State == ConnectionState.Open)
                {
                    sqlConnection.Close();
                }
            }
            return currentAccount;
        }

        [WebMethod(EnableSession = true)]
        public Rankings ViewRankings(string test)
        {
            Rankings currentRankings  = new Rankings();

            string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;

            string sql = "SELECT Department, COUNT(Department) AS 'numOfResp' ,CAST(AVG (FeedbackRating4) AS DECIMAL (12,2)) AS 'AvgRating'FROM Feedback GROUP BY Department ORDER BY count('numOfResp') DESC LIMIT 5;";

            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            sqlConnection.Open();

            MySqlCommand sqlCommand = new MySqlCommand(sql, sqlConnection);

            MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
            DataTable sqlDt = new DataTable();
            sqlDa.Fill(sqlDt);

            currentRankings.department1 = sqlDt.Rows[0]["Department"].ToString();
            currentRankings.department2 = sqlDt.Rows[1]["Department"].ToString();
            currentRankings.department3 = sqlDt.Rows[2]["Department"].ToString();
            currentRankings.department4 = sqlDt.Rows[3]["Department"].ToString();
            currentRankings.department5 = sqlDt.Rows[4]["Department"].ToString();

            currentRankings.response1 = sqlDt.Rows[0]["numOfResp"].ToString();
            currentRankings.response2 = sqlDt.Rows[1]["numOfResp"].ToString();
            currentRankings.response3 = sqlDt.Rows[2]["numOfResp"].ToString();
            currentRankings.response4 = sqlDt.Rows[3]["numOfResp"].ToString();
            currentRankings.response5 = sqlDt.Rows[4]["numOfResp"].ToString();

            currentRankings.rating1 = sqlDt.Rows[0]["avgRating"].ToString();
            currentRankings.rating2 = sqlDt.Rows[1]["avgRating"].ToString();
            currentRankings.rating3 = sqlDt.Rows[2]["avgRating"].ToString();
            currentRankings.rating4 = sqlDt.Rows[3]["avgRating"].ToString();
            currentRankings.rating5 = sqlDt.Rows[4]["avgRating"].ToString();

            return currentRankings;
        }

        [WebMethod(EnableSession = true)]
        public Account[] GetActiveAccounts()
        {//LOGIC: get all active accounts and return them!
            DataTable sqlDt = new DataTable("activeAccounts");

            string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;

            string sqlSelect = "SELECT * FROM runtime.Account where DisableFlag IS null;";

            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
            sqlDa.Fill(sqlDt);

            List<Account> activeAccount = new List<Account>();
            for (int i = 0; i < sqlDt.Rows.Count; i++)
            {
                activeAccount.Add(new Account
                {
                    employeeId = sqlDt.Rows[i]["EmployeeId"].ToString(),
                    password = sqlDt.Rows[i]["AcctPassword"].ToString(),
                    firstName = sqlDt.Rows[i]["FirstName"].ToString(),
                    lastName = sqlDt.Rows[i]["LastName"].ToString(),
                    email = sqlDt.Rows[i]["Email"].ToString(),
                    
                });
            }
            //convert the list of accounts to an array and return
            return activeAccount.ToArray();
        }

        [WebMethod(EnableSession = true)]
        public Account GetActiveAccountDetails(string employeeId)
        {
            Account accountDetails = new Account();

            string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;

            string sql = "SELECT * FROM Account WHERE EmployeeId=@idValue";

            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            sqlConnection.Open();

            MySqlCommand sqlCommand = new MySqlCommand(sql, sqlConnection);

            sqlCommand.Parameters.AddWithValue("@idValue", HttpUtility.UrlDecode(employeeId));

            bool adminUser = false;

            try
            {
                MySqlDataReader dr;
                dr = sqlCommand.ExecuteReader();
                while (dr.Read())
                {            
                    accountDetails.employeeId = dr.GetString("EmployeeID");
                    accountDetails.password = dr.GetString("AcctPassword");
                    accountDetails.email = dr.GetString("Email");
                    accountDetails.firstName = dr.GetString("FirstName");
                    accountDetails.lastName = dr.GetString("LastName");
                    accountDetails.adminFlag = adminUser;

                }

                if (dr.GetString("AdminFlag").ToUpper() == "X")
                {
                    adminUser = true;
                    accountDetails.adminFlag = adminUser;
                }

                dr.Close();
            }
            catch (Exception)
            {

            }
            finally
            {
                if (sqlConnection.State == ConnectionState.Open)
                {
                    sqlConnection.Close();
                }
            }

            return accountDetails;
        }

        [WebMethod(EnableSession = true)]
        public string DisableAccount(string employeeId, string disableFlag)
        {
            string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
            MySqlConnection conn = new MySqlConnection(connStr);
            string sql = "UPDATE Account SET DisableFlag=@dValue WHERE EmployeeId=@idValue;";

            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@idValue", HttpUtility.UrlDecode(employeeId));
            cmd.Parameters.AddWithValue("@dValue", HttpUtility.UrlDecode(disableFlag));

            try
            {
                conn.Open();

                cmd.ExecuteNonQuery();

                return "disable success";
            }
            catch (Exception)
            {
                return "disable failed";
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }

        [WebMethod(EnableSession = true)]
        public Account[] GetDisabledAccounts()
        {//LOGIC: get all active accounts and return them!
            DataTable sqlDt = new DataTable("disabledAccounts");

            string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;

            string sqlSelect = "SELECT * FROM runtime.Account where DisableFlag IS NOT null;";

            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
            sqlDa.Fill(sqlDt);

            List<Account> disabledAccount = new List<Account>();
            for (int i = 0; i < sqlDt.Rows.Count; i++)
            {
                disabledAccount.Add(new Account
                {
                    employeeId = sqlDt.Rows[i]["EmployeeId"].ToString(),
                    password = sqlDt.Rows[i]["AcctPassword"].ToString(),
                    firstName = sqlDt.Rows[i]["FirstName"].ToString(),
                    lastName = sqlDt.Rows[i]["LastName"].ToString(),
                    email = sqlDt.Rows[i]["Email"].ToString(),
                });
            }
            //convert the list of accounts to an array and return
            return disabledAccount.ToArray();
        }

        [WebMethod(EnableSession = true)]
        public Account GetDisabledAccountDetails(string employeeId)
        {
            Account accountDetails = new Account();

            string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;

            string sql = "SELECT * FROM Account WHERE EmployeeId=@idValue";

            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            sqlConnection.Open();

            MySqlCommand sqlCommand = new MySqlCommand(sql, sqlConnection);

            sqlCommand.Parameters.AddWithValue("@idValue", HttpUtility.UrlDecode(employeeId));

            try
            {
                MySqlDataReader dr;
                dr = sqlCommand.ExecuteReader();
                while (dr.Read())
                {
                    accountDetails.employeeId = dr.GetString("EmployeeID");
                    accountDetails.password = dr.GetString("AcctPassword");
                    accountDetails.email = dr.GetString("Email");
                    accountDetails.firstName = dr.GetString("FirstName");
                    accountDetails.lastName = dr.GetString("LastName");
                }
                dr.Close();
            }
            catch (Exception)
            {

            }
            finally
            {
                if (sqlConnection.State == ConnectionState.Open)
                {
                    sqlConnection.Close();
                }
            }

            return accountDetails;
        }

        [WebMethod(EnableSession = true)]
        public string EnableAccount(string employeeId)
        {
            string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
            MySqlConnection conn = new MySqlConnection(connStr);
            string sql = "UPDATE Account SET DisableFlag=NULL WHERE EmployeeId=@idValue;";

            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@idValue", HttpUtility.UrlDecode(employeeId));

            try
            {
                conn.Open();

                cmd.ExecuteNonQuery();

                return "enable account success";
            }
            catch (Exception)
            {
                return "enable account failed";
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }

        [WebMethod(EnableSession = true)]
        public string DisableAdmin(string employeeId)
        {
            string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
            MySqlConnection conn = new MySqlConnection(connStr);
            string sql = "UPDATE Account SET AdminFlag=NULL WHERE EmployeeId=@idValue;";

            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@idValue", HttpUtility.UrlDecode(employeeId));

            try
            {
                conn.Open();

                cmd.ExecuteNonQuery();

                return "disable admin success";
            }
            catch (Exception)
            {
                return "disable failed";
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }

        [WebMethod(EnableSession = true)]
        public string MakeAdmin(string employeeId, string enableFlag)
        {
            string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
            MySqlConnection conn = new MySqlConnection(connStr);
            string sql = "UPDATE Account SET AdminFlag=@dValue WHERE EmployeeId=@idValue;";

            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@idValue", HttpUtility.UrlDecode(employeeId));
            cmd.Parameters.AddWithValue("@dValue", HttpUtility.UrlDecode(enableFlag));

            try
            {
                conn.Open();

                cmd.ExecuteNonQuery();

                return "enable admin success";
            }
            catch (Exception)
            {
                return "enable admin failed";
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }

        [WebMethod(EnableSession = true)]
        public string InsertFeedBack(string EmployeeId,string department, string FB1,string FBR1, string FB2,string FBR2,string FB3,string FBR3,string FB4,string FBR4)
        {
            string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
            MySqlConnection conn = new MySqlConnection(connStr);
            string sql = "INSERT INTO Feedback (EmployeeId, Department, FeedbackText1, FeedbackRating1, FeedbackText2,FeedbackRating2,FeedbackText3,FeedbackRating3,FeedbackText4,FeedbackRating4) VALUES (@idValue, @depValue, @FBT1Value, @FBR1Value, @FBT2Value,@FBR2Value,@FBT3Value,@FBR3Value,@FBT4Value,@FBR4Value);"; 
            
            MySqlCommand cmd = new MySqlCommand(sql, conn);
           
            cmd.Parameters.AddWithValue("@idValue", HttpUtility.UrlDecode(EmployeeId));
            cmd.Parameters.AddWithValue("@depValue", HttpUtility.UrlDecode(department));
            cmd.Parameters.AddWithValue("@FBT1Value", HttpUtility.UrlDecode(FB1));
            cmd.Parameters.AddWithValue("@FBR1Value", HttpUtility.UrlDecode(FBR1));
            cmd.Parameters.AddWithValue("@FBT2Value", HttpUtility.UrlDecode(FB2));
            cmd.Parameters.AddWithValue("@FBR2Value", HttpUtility.UrlDecode(FBR2));
            cmd.Parameters.AddWithValue("@FBT3Value", HttpUtility.UrlDecode(FB3));
            cmd.Parameters.AddWithValue("@FBR3Value", HttpUtility.UrlDecode(FBR3));
            cmd.Parameters.AddWithValue("@FBT4Value", HttpUtility.UrlDecode(FB4));
            cmd.Parameters.AddWithValue("@FBR4Value", HttpUtility.UrlDecode(FBR4));

            try
            {
                conn.Open();

                cmd.ExecuteNonQuery();
                

                return "Feedback saved successfully";
            }
            catch (Exception)
            {
                return "Feedback cannot be saved";
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }

        [WebMethod(EnableSession = true)]
        public Feedback[] GetAllFeedback()
        {//LOGIC: get all account requests and return them!
            DataTable sqlDt = new DataTable("allFeedback");

            string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;

            string sqlSelect = "SELECT *, CAST(FeedbackDate AS DATE) 'FeedbackDate2', CAST((FeedbackRating1 + FeedbackRating2 + FeedbackRating3 + FeedbackRating4)/4 AS DECIMAL(3,2)) 'OverallRating' FROM runtime.Feedback ORDER BY FeedbackDate DESC";

            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
            sqlDa.Fill(sqlDt);

            List<Feedback> allFeedback = new List<Feedback>();
            for (int i = 0; i < sqlDt.Rows.Count; i++)
            {
                string dateFormatter = sqlDt.Rows[i]["FeedbackDate2"].ToString();
                string[] dateFormatterSplit = dateFormatter.Split(Convert.ToChar(" "));

                allFeedback.Add(new Feedback
                {
                    feedbackRecordId = Convert.ToInt32(sqlDt.Rows[i]["RecordId"]),
                    employeeId = sqlDt.Rows[i]["EmployeeId"].ToString(),                    
                    feedbackDate = dateFormatterSplit[0],
                    department = sqlDt.Rows[i]["Department"].ToString(),
                    feedbackText1 = sqlDt.Rows[i]["FeedbackText1"].ToString(),
                    feedbackRating1 = Convert.ToInt32(sqlDt.Rows[i]["FeedbackRating1"]),
                    feedbackText2 = sqlDt.Rows[i]["FeedbackText2"].ToString(),
                    feedbackRating2 = Convert.ToInt32(sqlDt.Rows[i]["FeedbackRating2"]),
                    feedbackText3 = sqlDt.Rows[i]["FeedbackText3"].ToString(),
                    feedbackRating3 = Convert.ToInt32(sqlDt.Rows[i]["FeedbackRating3"]),
                    feedbackText4 = sqlDt.Rows[i]["FeedbackText4"].ToString(),
                    feedbackRating4 = Convert.ToInt32(sqlDt.Rows[i]["FeedbackRating4"]),
                    overallRating = Convert.ToDouble(sqlDt.Rows[i]["OverallRating"])
                });
            }
            //convert the list of accounts to an array and return
            return allFeedback.ToArray();
        }


        [WebMethod(EnableSession = true)]
        public Feedback[] GetFilteredFeedback(string departmentValue)
        {//LOGIC: get all account requests and return them!
            DataTable sqlDt = new DataTable("filteredFeedback");

            string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;

            string sqlSelect = "SELECT *, CAST(FeedbackDate AS DATE) 'FeedbackDate2', CAST((FeedbackRating1 + FeedbackRating2 + FeedbackRating3 + FeedbackRating4)/4 AS DECIMAL(3,2)) 'OverallRating' FROM runtime.Feedback WHERE Department = @department ORDER BY FeedbackDate DESC";

            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            sqlCommand.Parameters.AddWithValue("@department", HttpUtility.UrlDecode(departmentValue));

            MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
            sqlDa.Fill(sqlDt);

            List<Feedback> filteredFeedback = new List<Feedback>();
            for (int i = 0; i < sqlDt.Rows.Count; i++)
            {
                string dateFormatter = sqlDt.Rows[i]["FeedbackDate2"].ToString();
                string[] dateFormatterSplit = dateFormatter.Split(Convert.ToChar(" "));

                filteredFeedback.Add(new Feedback
                {
                    feedbackRecordId = Convert.ToInt32(sqlDt.Rows[i]["RecordId"]),
                    employeeId = sqlDt.Rows[i]["EmployeeId"].ToString(),
                    feedbackDate = dateFormatterSplit[0],
                    department = sqlDt.Rows[i]["Department"].ToString(),
                    feedbackText1 = sqlDt.Rows[i]["FeedbackText1"].ToString(),
                    feedbackRating1 = Convert.ToInt32(sqlDt.Rows[i]["FeedbackRating1"]),
                    feedbackText2 = sqlDt.Rows[i]["FeedbackText2"].ToString(),
                    feedbackRating2 = Convert.ToInt32(sqlDt.Rows[i]["FeedbackRating2"]),
                    feedbackText3 = sqlDt.Rows[i]["FeedbackText3"].ToString(),
                    feedbackRating3 = Convert.ToInt32(sqlDt.Rows[i]["FeedbackRating3"]),
                    feedbackText4 = sqlDt.Rows[i]["FeedbackText4"].ToString(),
                    feedbackRating4 = Convert.ToInt32(sqlDt.Rows[i]["FeedbackRating4"]),
                    overallRating = Convert.ToDouble(sqlDt.Rows[i]["OverallRating"])
                });
            }
            //convert the list of accounts to an array and return
            return filteredFeedback.ToArray();
        }


        [WebMethod(EnableSession = true)]
        public Feedback GetFeedbackDetail(string recordIdValue)
        {
            DataTable sqlDt = new DataTable("allFeedback");

            string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;

            string sqlSelect = "SELECT *, CAST(FeedbackDate AS DATE) 'FeedbackDate2', CAST((FeedbackRating1 + FeedbackRating2 + FeedbackRating3 + FeedbackRating4)/4 AS DECIMAL(3,2)) 'OverallRating' FROM runtime.Feedback WHERE RecordId = @RecordId ORDER BY FeedbackDate DESC;";

            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            sqlCommand.Parameters.AddWithValue("@recordId", HttpUtility.UrlDecode(recordIdValue));

            MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
            sqlDa.Fill(sqlDt);

            Feedback feedbackDetail = new Feedback();

            string dateFormatter = sqlDt.Rows[0]["FeedbackDate2"].ToString();
            string[] dateFormatterSplit = dateFormatter.Split(Convert.ToChar(" "));

            feedbackDetail.feedbackRecordId = Convert.ToInt32(sqlDt.Rows[0]["RecordId"]);
            feedbackDetail.employeeId = sqlDt.Rows[0]["EmployeeId"].ToString();
            feedbackDetail.feedbackDate = dateFormatterSplit[0];
            feedbackDetail.department = sqlDt.Rows[0]["Department"].ToString();
            feedbackDetail.feedbackText1 = sqlDt.Rows[0]["FeedbackText1"].ToString();
            feedbackDetail.feedbackRating1 = Convert.ToInt32(sqlDt.Rows[0]["FeedbackRating1"]);
            feedbackDetail.feedbackText2 = sqlDt.Rows[0]["FeedbackText2"].ToString();
            feedbackDetail.feedbackRating2 = Convert.ToInt32(sqlDt.Rows[0]["FeedbackRating2"]);
            feedbackDetail.feedbackText3 = sqlDt.Rows[0]["FeedbackText3"].ToString();
            feedbackDetail.feedbackRating3 = Convert.ToInt32(sqlDt.Rows[0]["FeedbackRating3"]);
            feedbackDetail.feedbackText4 = sqlDt.Rows[0]["FeedbackText4"].ToString();
            feedbackDetail.feedbackRating4 = Convert.ToInt32(sqlDt.Rows[0]["FeedbackRating4"]);
            feedbackDetail.overallRating = Convert.ToDouble(sqlDt.Rows[0]["OverallRating"]);

            return feedbackDetail;
        }
    }
}
