using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SMClient.Models;
using SMClient.Services;

namespace SMClient
{
    public partial class MainScreen : Form
    {
        public MainScreen()
        {
            InitializeComponent();

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }



        private void button2_Click(object sender, EventArgs e)
        {
            ViewBill viewBill = new ViewBill();
            viewBill.Show();
            this.Hide();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            NetworkStatusCheck networkStatusCheck = new NetworkStatusCheck();
            networkStatusCheck.Show();
            this.Hide();
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        public void DisplayMessage(string message)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<string>(DisplayMessage), message);
                label5.Text = message;

            }
            else
            {
                label5.Text = message;
            }
        }

        public void DisplayBill(BillResponse billResponse)
        {
            if (this.InvokeRequired)
            {
                string message = $"Received bill: {billResponse.Amount} for meter {billResponse.MeterId} on {billResponse.BillDate}";
                DisplayMessage(message);
                label6.Text = message;

            }
            else
            {
                label6.Text = billResponse.ToString();
            }


        }


    }


}
