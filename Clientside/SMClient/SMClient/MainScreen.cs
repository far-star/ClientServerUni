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
    }
}
