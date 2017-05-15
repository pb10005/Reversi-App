using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Reversi.GUI
{
    public partial class MatchConfigDialog : Form
    {
        public MatchConfigDialog()
        {
            InitializeComponent();
        }
        public bool SenteIsCom { get; set; }
        public bool GoteIsCom { get; set; }
        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == -1 || comboBox2.SelectedIndex == -1)
            {
                MessageBox.Show("駄目です");
                return;
            }
            if ((string)comboBox1.SelectedItem == "COM")
            {
                SenteIsCom = true;
            }
            else
            {
                SenteIsCom = false;
            }
            if ((string)comboBox2.SelectedItem == "COM")
            {
                GoteIsCom = true;
            }
            else
            {
                GoteIsCom = false;
            }
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
