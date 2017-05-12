using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThinkingEngineBase
{
    /// <summary>
    /// 思考エンジンのインターフェイス
    /// </summary>
    public interface IThinkingEngine
    {
        /// <summary>
        /// 盤の情報をもとに思考し、次の手を表す符号なし64ビット整数を返す
        /// </summary>
        /// <param name="black"></param>
        /// <param name="white"></param>
        /// <returns></returns>
        ulong Think(ulong black,ulong white);
        /// <summary>
        /// 思考時間の上限を設定する
        /// </summary>
        /// <param name="milliSecond">単位: ミリ秒</param>
        void SetTimeLimit(int milliSecond);
    }
}
