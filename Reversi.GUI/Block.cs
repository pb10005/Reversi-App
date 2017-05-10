using System.Windows.Forms;
using Reversi.Core;
namespace Reversi.GUI
{
    /// <summary>
    /// マス目を表すクラス
    /// IDと状態を格納する
    /// </summary>
    public partial class Block : UserControl
    {
        /// <summary>
        /// マス目の初期化
        /// </summary>
        /// <param name="i">ID 棋譜の符号に対応している</param>
        public Block(int row,int col)
        {
            InitializeComponent();
            Row = row;
            Col = col;
        }

        public int Row { get; private set; }
        public int Col { get; private set; }
        public StoneType State { get; private set; }

        /// <summary>
        /// マス目の状態を空にする
        /// </summary>
        public void Reset()
        {
            BackgroundImage = null;
            State = StoneType.None;
        }
        /// <summary>
        /// マス目の状態を黒にする
        /// </summary>
        public void ToBlack()
        {
            BackgroundImage = Properties.Resources.black_stone;
            BackgroundImageLayout = ImageLayout.Stretch;
            State = StoneType.Sente;
        }
        /// <summary>
        /// マス目の状態を白にする
        /// </summary>
        public void ToWhite()
        {
            BackgroundImage = Properties.Resources.white_stone;
            BackgroundImageLayout = ImageLayout.Stretch;
            State = StoneType.Gote;
        }
        
    }
}
