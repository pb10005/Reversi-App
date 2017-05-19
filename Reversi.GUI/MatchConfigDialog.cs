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
        public MatchConfigDialog(List<string> engineNames)
        {
            InitializeComponent();
            foreach (var item in engineNames)
            {
                comboBox1.Items.Add(item);
                comboBox2.Items.Add(item);
            }
        }
        public bool SenteIsCom { get; set; }
        public bool GoteIsCom { get; set; }
        public string SenteName { get; private set; }
        public string GoteName { get; private set; }
        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == -1 || comboBox2.SelectedIndex == -1)
            {
                MessageBox.Show("駄目です");
                return;
            }
            if ((string)comboBox1.SelectedItem != "人間")
            {
                SenteIsCom = true;
                SenteName = comboBox1.Text;
            }
            else
            {
                SenteIsCom = false;
            }
            if ((string)comboBox2.SelectedItem != "人間")
            {
                GoteIsCom = true;
                GoteName = comboBox2.Text;
            }
            else
            {
                GoteIsCom = false;
            }
            DialogResult = DialogResult.OK;
            Close();
        }

        private void MatchConfigDialog_Load(object sender, EventArgs e)
        {

        }
    }
}
