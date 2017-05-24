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
        #region フラグ
        bool inGame = false;
        bool inPlayback = false;
        bool senteIsCom = false;
        bool goteIsCom = false;
        bool previousPass = false;
        #endregion

        int turnNum = 0;
        int passNum = 0;
        Block[,] blocks = new Block[8, 8];
        MatchRecord record = MatchRecord.Empty();
        ReversiBoard board = new ReversiBoard();
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
                        if (turnNum%2 ==0 && senteIsCom)
                        {
                            return;
                        }
                        if (turnNum%2 ==1 && goteIsCom)
                        {
                            return;
                        }
                        await Add(block.Row, block.Col);
                        previousPass = false;
                        await Next();
                    };
                    matchPanel.Controls.Add(block);
                    blocks[row, col] = block;
                }
            }
            //エンジンを読み込み
            if (System.IO.File.Exists("engines.txt"))
            {
                manager = EngineManager.FromFile("engines.txt");
            }
            else
            {
                System.IO.File.Create("engines.txt");
            }
        }
        private async void Init()
        {
            turnNum = 0;
            passNum = 0;
            board = ReversiBoard.InitBoard();
            record = MatchRecord.Empty();


            RefreshTurnLabel();
            RefreshPanel();
            inGame = true;
            inPlayback = false;
            await Next();
        }
        #endregion

        #region ビューの更新
        private void RefreshTurnLabel()
        {
            BeginInvoke(new Action(() =>
            {
                var text = "";
                var eval = 0;
                if ((turnNum + 1) % 2 == 1)
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
                turnLabel.Text = string.Format("{0}手目、{1}の手番、評価値{2}", turnNum + 1, text,eval);
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
            turnNum++;
            if (turnNum % 2 == 1)
            {
                try
                {
                    board = board.AddStone(row, col, StoneType.Sente);
                    blocks[row, col].ToBlack(); //仮に打つ
                    record.Boards.Add(board);
                    previousPass = false;
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
                    record.Boards.Add(board);
                }
                catch (ArgumentException)
                {
                    turnNum--;
                }
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
            if (turnNum - passNum >= 60)
            {
                ReportResult();
            }
            await Task.Delay(waitingTime);
            if (turnNum%2 == 0 && senteIsCom)
            {
                try
                {
                    var res = await senteEngine.Think(board, StoneType.Sente);
                    await Add(res.Row, res.Col);
                    previousPass = false;
                }
                catch(InvalidOperationException ex)
                {
                    if (inGame)
                    {
                        if (previousPass)
                        {
                            MessageBox.Show("お互い合法手がないため、終わりです");
                            MessageBox.Show(board.ResultString(), "結果");
                            previousPass = false;
                            inGame = false;
                        }
                        else
                        {
                            MessageBox.Show(ex.Message);
                            turnNum++;
                            passNum++;
                            previousPass = true;
                        }
                    }
                }
                    await Next();
            }
            if (turnNum%2 ==1 && goteIsCom)
            {
                try
                {
                    var res = await goteEngine.Think(board, StoneType.Gote);
                    await Add(res.Row, res.Col);
                }
                catch(InvalidOperationException ex)
                {

                    if (inGame)
                    {
                        if (previousPass)
                        {
                            MessageBox.Show("お互い合法手がないため、終わりです");
                            MessageBox.Show(board.ResultString(), "結果");
                            previousPass = false;
                            inGame = false;
                        }
                        else
                        {
                            MessageBox.Show(ex.Message);
                            turnNum++;
                            passNum++;
                            previousPass = true;
                        }
                    }
                }
                if (turnNum - passNum >= 60)
                {
                    ReportResult();
                }
                else
                {
                    await Next();
                }
            }
        }
        private void surrenderButton_Click(object sender, EventArgs e)
        {
            ReportResult();
        }
        private void ReportResult()
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

        private async void passButton_Click(object sender, EventArgs e)
        {
            turnNum++;
            passNum++;
            previousPass = true;
            await Next();
        }

        private void バージョン情報ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var form = new Form()
            {
                Text = "バージョン情報",
                Size = new Size(200, 120),
                FormBorderStyle = FormBorderStyle.FixedToolWindow,
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
                manager.SaveToFile("engines.txt");
            }
        }
    }
}
