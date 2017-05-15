using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reversi.Core
{
    /// <summary>
    /// オセロの手を表すクラス
    /// </summary>
    public class ReversiMove
    {
        /// <summary>
        /// 行と列を指定して初期化
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        public ReversiMove(int row,int col)
        {
            Row = row;
            Col = col;
        }
        /// <summary>
        /// 行
        /// </summary>
        public int Row { get; }
        /// <summary>
        /// 列
        /// </summary>
        public int Col { get; }

        /// <summary>
        /// 手を符号なし64ビット整数に変換する
        /// </summary>
        /// <returns></returns>
        public ulong ToUInt64()
        {
            return (ulong)1 << (8 * Row + Col);
        }
    }
}
