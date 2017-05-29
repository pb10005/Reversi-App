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
    public partial class EngineDialog : Form
    {
        public EngineDialog(EngineManager manager)
        {
            InitializeComponent();
            this.manager = manager;
            listBox1.DataSource = manager.EngineMap.Keys.ToList();
        }
        EngineManager manager;
        private void EngineDIalog_Load(object sender, EventArgs e)
        {
        }
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog()
            {
                Filter = "dllファイル|*.dll"
            };
            if (dialog.ShowDialog()==DialogResult.OK)
            {
                try
                {
                    manager.Register(dialog.FileName);
                }
                catch(ArgumentException ex)
                {
                    MessageBox.Show("既に登録されています", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception)
                {
                    MessageBox.Show("登録に失敗しました","エラー",MessageBoxButtons.OK,MessageBoxIcon.Error);
                }
                listBox1.DataSource = manager.EngineMap.Keys.ToList();
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var name = listBox1.SelectedItem.ToString();
            manager.Remove(name);
            listBox1.DataSource = manager.EngineMap.Keys.ToList();
        }
    }
}
