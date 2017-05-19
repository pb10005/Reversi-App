using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThinkingEngineBase
{
    /// <summary>
    /// 思考エンジンのインターフェイス
    /// 思考エンジンはこのインターフェイスを実装する
    /// </summary>
    public interface IThinkingEngine
    {
        /// <summary>
        /// 盤の情報をもとに思考し、次の一手を返す
        /// </summary>
        /// <param name="board"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        Task<Reversi.Core.ReversiMove> Think(Reversi.Core.ReversiBoard board,Reversi.Core.StoneType player);
        /// <summary>
        /// 思考時間の上限を設定する
        /// </summary>
        /// <param name="milliSecond">単位: ミリ秒</param>
        void SetTimeLimit(int milliSecond);
        /// <summary>
        /// エンジンの名前
        /// </summary>
        string Name { get; }
    }
}
