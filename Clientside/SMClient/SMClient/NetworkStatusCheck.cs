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
    public partial class NetworkStatusCheck : Form
    {
        public NetworkStatusCheck()
        {
            InitializeComponent();
        }

        public void DisplayNetworkAlert(NetworkAlert networkAlert)
        {
            string message = $"Received network alert: {networkAlert.ErrorType} - {networkAlert.Message} on {networkAlert.Timestamp}";
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<NetworkAlert>(DisplayNetworkAlert), networkAlert);
                label1.Text = message;
            }
            else
            {
                label1.Text = message;
            }
        }

        private void label8_Click(object sender, EventArgs e)
        {
            MainScreen mainScreen = new MainScreen();
            mainScreen.Show();
            this.Hide();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
