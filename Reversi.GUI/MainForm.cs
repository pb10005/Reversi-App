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
        int turnNum = 0;
        Block[,] blocks = new Block[8, 8];
        ReversiBoard board = new ReversiBoard();
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
        private void Add(int row,int col)
        {
            if (!inGame)
            {
                return;
            }
            if (blocks[row,col].State != StoneType.None)
            {
                return;
            }
            turnNum++;
            if (turnNum % 2 == 1)
            {
                try
                {
                    board = board.AddStone(row, col, StoneType.Sente);
                }
                catch
                {
                    turnNum--;
                }
            }
            else
            {
                try
                {
                    board = board.AddStone(row, col, StoneType.Gote);

                }
                catch
                {
                    turnNum--;
                }
            }
            RefreshPanel();
        }
        private void Init()
        {
            board = ReversiBoard.InitBoard();
            RefreshPanel();
            inGame = true;
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

        private void newButton_Click(object sender, EventArgs e)
        {
            Init();
        }
    }
}
