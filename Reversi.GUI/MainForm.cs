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
        const int waitingTime = 200;
        const string enginePath = "engines.xml";
        #region フラグ
        bool inGame = false;
        bool inPlayback = false;
        bool senteIsCom = false;
        bool goteIsCom = false;
        #endregion

        Block[,] blocks = new Block[8, 8];
        MatchRecord record = MatchRecord.Empty();
        ReversiBoard board = new ReversiBoard();
        Match game = new Match();
        EngineManager manager = new EngineManager();
        ThinkingEngineBase.IThinkingEngine senteEngine;
        ThinkingEngineBase.IThinkingEngine goteEngine;
        
        #region 初期化
        private void Form1_Load(object sender, EventArgs e)
        {
            //マス目を追加
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    Block block = new Block(row, col);
                    block.Click += async (sd, ea) =>
                    {
                        if (!inGame) return;
                        if (game.CurrentPlayer == StoneType.Sente && senteIsCom)
                        {
                            return;
                        }
                        if (game.CurrentPlayer == StoneType.Gote && goteIsCom)
                        {
                            return;
                        }
                        await Add(block.Row, block.Col);
                        await Next();
                    };
                    matchPanel.Controls.Add(block);
                    blocks[row, col] = block;
                }
            }
            //エンジンを読み込み
            if (System.IO.File.Exists(enginePath))
            {
                try
                {
                    manager = EngineManager.FromFile(enginePath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message,"読み込みエラー",MessageBoxButtons.OK,MessageBoxIcon.Error);
                }
            }
            else
            {
                System.IO.File.Create(enginePath).Close();
            }
        }
        private async void Init()
        {
            board = ReversiBoard.InitBoard();
            record = MatchRecord.Empty();
            game.Init();
            game.End += Game_End;
            RefreshTurnLabel();
            RefreshPanel();
            inGame = true;
            inPlayback = false;
            await Next();
        }

        private void Game_End(string message)
        {
            BeginInvoke(new Action(() =>
            {
                if (inGame)
                {
                    inGame = false;
                    MessageBox.Show(message);
                }
            }));
        }
        #endregion

        #region ビューの更新
        private void RefreshTurnLabel()
        {
            BeginInvoke(new Action(() =>
            {
                var text = "";
                var eval = 0;
                if (game.CurrentPlayer == StoneType.Sente)
                {
                    text = "黒";
                    if (goteEngine != default(ThinkingEngineBase.IThinkingEngine))
                    {
                        eval = goteEngine.GetEval();
                    }
                }
                else
                {
                    text = "白";
                    if (senteEngine != default(ThinkingEngineBase.IThinkingEngine))
                    {
                        eval = senteEngine.GetEval();
                    }
                }
                turnLabel.Text = string.Format("{0}手目、{1}の手番、評価値{2}", game.Turn+1, text,eval);
            }
            ));
        }
        private void RefreshPanel()
        {
            BeginInvoke(new Action(() =>
            {
                for (int row = 0; row < 8; row++)
                {
                    for (int col = 0; col < 8; col++)
                    {
                        if (game.CurrentBoard.BlackToMat()[row, col])
                        {
                            blocks[row, col].ToBlack();
                        }
                        else if (game.CurrentBoard.WhiteToMat()[row, col])
                        {
                            blocks[row, col].ToWhite();
                        }
                        else
                        {
                            blocks[row, col].Reset();
                        }
                    }
                }
            }));
        }

        private void RefreshPanelForPlayback()
        {
            BeginInvoke(new Action(() =>
            {
                for (int row = 0; row < 8; row++)
                {
                    for (int col = 0; col < 8; col++)
                    {
                        if (board.BlackToMat()[row, col])
                        {
                            blocks[row, col].ToBlack();
                        }
                        else if (board.WhiteToMat()[row, col])
                        {
                            blocks[row, col].ToWhite();
                        }
                        else
                        {
                            blocks[row, col].Reset();
                        }
                    }
                }
            }));
        }
        #endregion

        #region 対局
        private void newButton_Click(object sender, EventArgs e)
        {
            var dialog = new MatchConfigDialog(manager.EngineMap.Keys.ToList());
            if (dialog.ShowDialog()==DialogResult.OK)
            {
                senteIsCom = dialog.SenteIsCom;
                goteIsCom = dialog.GoteIsCom;
                if (senteIsCom)
                {
                    senteEngine = manager.EngineMap[dialog.SenteName];
                }
                if (goteIsCom)
                {
                    goteEngine = manager.EngineMap[dialog.GoteName];
                }
                Init();
            }
        }
        private async Task Add(int row, int col)
        {
            if (!inGame)
            {
                return;
            }
            if (blocks[row, col].State != StoneType.None)
            {
                return;
            }
            var res = game.Move(row, col);
            if (res == MoveResult.OK)
            {
                if (game.CurrentPlayer == StoneType.Gote)
                {
                    blocks[row, col].ToBlack();
                }
                else
                {
                    blocks[row, col].ToWhite();
                }
                record.Boards.Add(game.CurrentBoard);
            }
            else if (res == MoveResult.End)
            {
                inGame = false;
                BeginInvoke(new Action(() => { MessageBox.Show(game.CurrentBoard.ResultString()); }));
                record.Boards.Add(game.CurrentBoard);
            }
            await Task.Delay(waitingTime); 
            BeginInvoke(new Action(() =>
            {
                RefreshPanel();
                RefreshTurnLabel();
            }));
        }
        private async Task Next()
        {
            if (!inGame)
            {
                return;
            }
            await Task.Delay(waitingTime);
            if (game.CurrentBoard.SearchLegalMoves(game.CurrentPlayer).Count==0)
            {
                game.Pass();
                await Next();
            }
            else if (game.CurrentPlayer == StoneType.Sente && senteIsCom)
            {
                var res = await senteEngine.Think(game.CurrentBoard, StoneType.Sente);
                await Add(res.Row, res.Col);
                await Next();
            }
            else if (game.CurrentPlayer == StoneType.Gote && goteIsCom)
            {
                var res = await goteEngine.Think(game.CurrentBoard, StoneType.Gote);
                await Add(res.Row, res.Col);
                await Next();
            }
        }
        private void surrenderButton_Click(object sender, EventArgs e)
        {
            if (!inGame)
            {
                return;
            }
            game.Surrender();
            inGame = false;
            BeginInvoke(new Action(() =>
            {
                MessageBox.Show(game.CurrentBoard.ResultString());
            }));
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
                    var turnNum = Convert.ToInt32(bindingNavigatorPositionItem.Text);
                    board = record.Boards[turnNum - 1];
                    //RefreshTurnLabel();
                    RefreshPanelForPlayback();
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
                var turnNum = Convert.ToInt32(bindingNavigatorPositionItem.Text);
                board = record.Boards[turnNum - 1];
                RefreshPanelForPlayback();
                //RefreshTurnLabel();
            }
        }

        private void endViewButton_Click(object sender, EventArgs e)
        {
            if (inPlayback)
            {
                bindingNavigator1.BindingSource = null;
                inPlayback = false;
                inGame = false;
                board = ReversiBoard.InitBoard();
                record = MatchRecord.Empty();
                RefreshPanel();
                RefreshTurnLabel();
            }
        }
        #endregion

        private void 終了XToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private async void passButton_Click(object sender, EventArgs e)
        {
            if (!inGame) return;
            game.Pass();
            await Next();
        }

        private void バージョン情報ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var form = new Form()
            {
                Text = "バージョン情報",
                Size = new Size(200, 120),
                FormBorderStyle = FormBorderStyle.FixedToolWindow,
                MaximizeBox = false,
                StartPosition = FormStartPosition.CenterParent
            };
            var label = new Label()
            {
                Location = new Point(10, 10),
                Text = "Reversi\nバージョン: " + VersionInfo.Version
            };
            form.Controls.Add(label);
            form.ShowDialog();
        }

        private void エンジン管理ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EngineDialog dialog = new EngineDialog(manager);
            if (dialog.ShowDialog()==DialogResult.OK)
            {
                manager.SaveToFile(enginePath);
            }
        }
    }
}
