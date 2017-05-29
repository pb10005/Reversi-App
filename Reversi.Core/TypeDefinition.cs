using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reversi.Core
{
    /// <summary>
    /// 盤上の石の状態
    /// </summary>
    public enum StoneType
    {
        /// <summary>
        /// 石なし
        /// </summary>
        None = 0,
        /// <summary>
        /// 先手
        /// </summary>
        Sente = 1,
        /// <summary>
        /// 後手
        /// </summary>
        Gote = 2
    }
    public enum MoveResult
    {
        OK = 0,
        End = 1,
        Illegal = 2
    }
    /// <summary>
    /// 対局結果
    /// </summary>
    public enum MatchResult
    {
        /// <summary>
        /// 引き分け
        /// </summary>
        Draw = 0,
        /// <summary>
        /// 先手の勝ち
        /// </summary>
        Sente = 1,
        /// <summary>
        /// 後手の勝ち
        /// </summary>
        Gote = 2,
        /// <summary>
        /// まだ終わっていない
        /// </summary>
        NotYet = 3
    }
}
