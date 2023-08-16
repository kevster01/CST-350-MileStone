using static System.Net.Mime.MediaTypeNames;
using System;
using CST350_Milestone1.Models;
using System.Data.SqlClient;
using System.Data;

namespace CST350_Milestone1.Services
{
    /* Security Data Access Object (DAO): This class is used to perform data access work.
     * Adding SQL Server:
     * 1. View->SQL Object Explorer. ( ctrl + s ) SSOE tab.
     * 2. Sql-> localdb-> Right Click Databases-> Add New Database-> [DatabaseName]-> Ok.
     * 3. SSOE tab-> Expand [DatabaseName]-> Right Click Tables-> Add New Table.
     * 4. dbo.Table tab-> Blue T-SQL Tab rename [Tables] and add columns->Update, Update Database.
     * 5. Right click [DatabaseName], select Properties-> Copy connections string path and set = to connectionString variable.
     *  
     */
    public class SecurityDAO
    {
        //string variable for long connection string, may need to remove some spaces after pasting from properties

        string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=Test;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

        /* Method for verifying valid login. This bool returns true if username and password match. 
         * Method searches for a record by username and is used by SecurityService class.
         *          
        */
        public bool FindByUsernameAndPassword(UserModel user)
        {
            //assume nothing is found
            bool success = false;

            //use prepared statements for security. @username and @password are defined below
            string sqlStatement = "SELECT * FROM dbo.MileStoneUsers WHERE username = @username and password = @password";

            //connect to SQL database
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(sqlStatement, connection);

                //define the values of the two placeholders in the sqlstatement string
                command.Parameters.Add("@USERNAME", System.Data.SqlDbType.VarChar, 50).Value = user.UserName;
                command.Parameters.Add("@PASSWORD", System.Data.SqlDbType.VarChar, 50).Value = user.Password;

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.HasRows)
                        success = true;
                }catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                };
            }
            return success;
        }

        public bool RegisterNewUser(UserModel user)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "INSERT INTO dbo.MileStoneUsers (FIRST_NAME, LAST_NAME, SEX, AGE, STATE, EMAIL, USERNAME, PASSWORD) " +
                                   "VALUES (@FIRST_NAME, @LAST_NAME, @SEX, @AGE, @STATE, @EMAIL, @USERNAME, @PASSWORD)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                       
                        command.Parameters.AddWithValue("@FIRST_NAME", user.FirstName);
                        command.Parameters.AddWithValue("@LAST_NAME", user.LastName);
                        command.Parameters.AddWithValue("@SEX", user.Sex);
                        command.Parameters.AddWithValue("@AGE", user.Age);
                        command.Parameters.AddWithValue("@STATE", user.State);
                        command.Parameters.AddWithValue("@EMAIL", user.EmailAddress);
                        command.Parameters.AddWithValue("@USERNAME", user.UserName);
                        command.Parameters.AddWithValue("@PASSWORD", user.Password);

                        int rowsAffected = command.ExecuteNonQuery();

                        return rowsAffected > 0; // Registration successful if rows were affected
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
                return false;
            }
        }

        //This method adds a new user to the database
   
    }
      
}
