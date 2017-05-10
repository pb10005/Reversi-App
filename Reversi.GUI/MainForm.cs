using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Reversi.Core;

namespace Reversi.GUI
{
    
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }
        bool inGame = false;
        bool inPlayback = false;
        int turnNum = 0;
        Block[,] blocks = new Block[8, 8];
        MatchRecord record = MatchRecord.Empty();
        ReversiBoard board = new ReversiBoard();
        
        #region 初期化
        private void Form1_Load(object sender, EventArgs e)
        {
            //マス目を追加
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    Block block = new Block(row, col);
                    block.Click += (sd, ea) => { Add(block.Row, block.Col); };
                    matchPanel.Controls.Add(block);
                    blocks[row, col] = block;
                }
            }
        }
        private void newButton_Click(object sender, EventArgs e)
        {
            Init();
        }
        private void Init()
        {
            turnNum = 0;
            board = ReversiBoard.InitBoard();
            RefreshTurnLabel();
            RefreshPanel();
            inGame = true;
            inPlayback = false;
        }
        #endregion

        #region ビューの更新
        private void RefreshTurnLabel()
        {
            var text = "";
            if ((turnNum+1)%2==1)
            {
                text = "黒";
            }
            else
            {
                text = "白";
            }
            turnLabel.Text = string.Format("{0}手目、{1}の手番",turnNum+1,text);
        }
        private void RefreshPanel()
        {
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    if (board.BlackToMat()[row,col])
                    {
                        blocks[row, col].ToBlack();
                    }
                    else if (board.WhiteToMat()[row,col])
                    {
                        blocks[row, col].ToWhite();
                    }
                    else
                    {
                        blocks[row, col].Reset();
                    }
                }
            }
        }
        #endregion

        #region 対局
        private async void Add(int row, int col)
        {
            if (!inGame)
            {
                return;
            }
            if (blocks[row, col].State != StoneType.None)
            {
                return;
            }
            turnNum++;
            if (turnNum % 2 == 1)
            {
                try
                {
                    board = board.AddStone(row, col, StoneType.Sente);
                    blocks[row, col].ToBlack(); //仮に打つ
                }
                catch (ArgumentException)
                {
                    turnNum--;
                }
            }
            else
            {
                try
                {
                    board = board.AddStone(row, col, StoneType.Gote);
                    blocks[row, col].ToWhite(); //仮に打つ

                }
                catch (ArgumentException)
                {
                    turnNum--;
                }
            }
            await Task.Delay(500); //500ms待ってから裏返す
            RefreshPanel();
            RefreshTurnLabel();
            record.Boards.Add(board);
            if (turnNum >= 60)
            {
                MessageBox.Show(board.ResultString(), "結果");
            }
        }
        private void surrenderButton_Click(object sender, EventArgs e)
        {
            if (inGame)
            {
                inGame = false;
                MessageBox.Show(board.ResultString(), "結果");
            }
        }
        #endregion

        #region ファイルIO
        private void saveButton_Click(object sender, EventArgs e)
        {
            var dialog = new SaveFileDialog()
            {
                Filter = "REVファイル|*.rev|すべてのファイル|*.*"
            };
            if(dialog.ShowDialog()==DialogResult.OK)
            {
                record.ToFile(dialog.FileName);
            }
        }

        private void openButton_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog()
            {
                Filter = "REVファイル|*.rev|すべてのファイル|*.*"
            };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    record = MatchRecord.FromFile(dialog.FileName);
                    var source = new BindingSource();
                    source.DataSource = record.Boards;
                    bindingNavigator1.BindingSource = source;
                    turnNum = Convert.ToInt32(bindingNavigatorPositionItem.Text);
                    board = record.Boards[turnNum - 1];
                    RefreshTurnLabel();
                    RefreshPanel();
                    inPlayback = true;
                }
                catch
                {
                    MessageBox.Show("ファイルが不正です。");
                }
            }
        }
        #endregion

        #region 閲覧モード
        private void moveButton_Click(object sender, EventArgs e)
        {
            if (inPlayback)
            {
                turnNum = Convert.ToInt32(bindingNavigatorPositionItem.Text);
                board = record.Boards[turnNum - 1];
                RefreshPanel();
                RefreshTurnLabel();
            }
        }

        private void endViewButton_Click(object sender, EventArgs e)
        {
            if (inPlayback)
            {
                Init();
            }
        }
        #endregion

        private void 終了XToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
