using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SMClient
{
    public partial class Login : Form
    {
        public Login()
        {
            InitializeComponent();
        }


        private void button1_Click(object sender, EventArgs e)
        {
            /*
             * 
            // Checks both login info boxes have been filled
            if (IDLogin.Text == string.Empty || pwLogin.Text == string.Empty)
            {
                MessageBox.Show("Missing input, Please ensure both fields are completed.");
            }
            else
            {
                try
                {
                    // Looks for account's existence in the DB
                    conn.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter("SELECT COUNT(*) FROM User WHERE Username = '" + IDLogin.Text + "' AND Password = '" + pwLogin.Text + "'", conn);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    userID = IDLogin.Text;
                    if (dataTable.Rows[0][0].ToString() == "1")
                    {
                        if (userID == "1")
                        {
                            // Checks if admin login credentials match and navigates accordingly
                            AdminLoggedIn adminLoggedIn = new AdminLoggedIn();
                            adminLoggedIn.Show();
                            this.Hide();
                            MessageBox.Show("Credentials accepted. Welcome to the admin account.");
                            conn.Close();
                        }
                        else
                        {
                            // If voter login matches and navigates accordingly
                            UserLoggedIn userLoggedIn = new UserLoggedIn();
                            userLoggedIn.Show();
                            this.Hide();
                            MessageBox.Show("Credentials accepted. Welcome to your user account.");
                            conn.Close();
                        }
                    }
                    else
                    {
                        // Lets user retry if they enter incorrect details
                        MessageBox.Show("Incorrect credentials entered. Please try again.");
                    }
                    conn.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Uh oh, invalid input! Please try again." + ex.Message);
                    conn.Close();
                }
            }
            */
        }


    }
}
