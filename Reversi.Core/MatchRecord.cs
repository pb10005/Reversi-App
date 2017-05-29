using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Reversi.Core
{
    /// <summary>
    /// 対局記録を表すクラス
    /// </summary>
    public class MatchRecord
    {
        public readonly List<ReversiBoard> Boards = 
                                new List<ReversiBoard>();

        #region 初期化
        private MatchRecord()
        {
            //通常のインスタンスの生成を隠蔽
        }
        /// <summary>
        /// 空の記録を生成
        /// </summary>
        /// <returns></returns>
        public static MatchRecord Empty()
        {
            return new MatchRecord();
        }
        /// <summary>
        /// ファイルのパスを指定して記録を生成
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static MatchRecord FromFile(string path)
        {
            var record = File.ReadAllText(path);
            return FromRecordString(record);
        }
        /// <summary>
        /// 文字列から記録を生成
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public static MatchRecord FromRecordString(string record)
        {
            MatchRecord res = new MatchRecord();
            var array = record.Split('\n');
            foreach (var bits in array)
            {
                if (bits=="")
                {
                    break;
                }
                var bitArray = bits.Split('|');
                var bl = Convert.ToUInt64(bitArray[0],2);
                var wh = Convert.ToUInt64(bitArray[1],2);
                var board = new ReversiBoard(bl,wh);
                res.Boards.Add(board);
            }
            return res;
        }
        #endregion

        #region 保存
        public void ToFile(string path)
        {
            var res = "";
            foreach (var item in Boards)
            {
                res += string.Format("{0}|{1}\n", item.BlackToBitString(), item.WhiteToBitString());
            }
            File.WriteAllText(path, res);
        }
        #endregion
    }
}
