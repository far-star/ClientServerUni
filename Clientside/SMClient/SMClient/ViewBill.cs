using SMClient.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SMClient
{
    public partial class ViewBill : Form
    {
        public ViewBill()
        {
            InitializeComponent();
        }

        private void label8_Click(object sender, EventArgs e)
        {
            MainScreen mainScreen = new MainScreen();
            mainScreen.Show();
            this.Hide();
        }

        public void DisplayBillDetails(BillResponse billResponse)
        {
            string message = $"Received bill: {billResponse.Amount} for meter {billResponse.MeterId} on {billResponse.BillDate}";
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<BillResponse>(DisplayBillDetails), billResponse);
                label24.Text = message;

            }
            else
            {
                label24.Text = message;
 
            }
        }


    }
}
