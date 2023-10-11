using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Calculate
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void textBoxMain_TextChanged(object sender, EventArgs e)
        {
            try {  labelAnswer.Text = new StringCalculator().Converting(textBoxMain.Text).ToString();  }
            catch (Exception ex) { labelAnswer.Text = ex.Message; }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

            //textBoxMain.Text = "((10+50)*10)/10-5*2+(10-1)+1";//"10/10+510-65*4";
            textBoxMain.Text = "2+(2.+6)";
        }

        private void labelAnswer_Click(object sender, EventArgs e)
        {
        }
    }
}
