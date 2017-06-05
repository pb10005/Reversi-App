using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvalEngine
{
    /// <summary>
    /// 評価関数
    /// </summary>
    public class Eval
    {
        public Eval()
        {

        }
        /// <summary>
        /// 文字列から評価関数を生成
        /// </summary>
        /// <param name="paramsString"></param>
        /// <returns></returns>
        public static Eval FromParamsString(string paramsString)
        {
            var paramsArray = paramsString.Split(',').Select(y => int.Parse(y)).ToArray();
            var res = new Eval();
            for (int itr = 0; itr < 4; itr++)
            {
                var squareSum = 0.0;　//二乗和
                var sum = 0.0;        //ただの和
                for (int row = 0; row < 4; row++)
                {
                    for (int col = 0; col < 4; col++)
                    {
                        var param = paramsArray[16 * itr + 4 * row + col];
                        sum += param;
                        squareSum += param * param;

                        res.evalBoard[row, col, itr] = param;
                        res.evalBoard[7 - row, col, itr] = param;
                        res.evalBoard[row, 7 - col, itr] = param;
                        res.evalBoard[7 - row, 7 - col, itr] = param;
                    }
                }
                var average = sum / 16.0;　//平均
                var stDev = Math.Sqrt(squareSum / 16.0 - average * average); //標準偏差
                res.stDevs[itr] = stDev;
            }
            
            return res;
        }
        private int[,,] evalBoard = new int[8, 8, 4];
        private double[] stDevs = new double[4];　//各パラメータの標準偏差を格納　意味があるかは不明
        /// <summary>
        /// 盤面を評価
        /// </summary>
        /// <param name="board"></param>
        /// <returns></returns>
        public int Execute(Reversi.Core.ReversiBoard board)
        {
            var black = board.BlackToMat();
            var white = board.WhiteToMat();
            var turn = board.NumOfBlack() + board.NumOfWhite();
            var blackValue = 0;
            var whiteValue = 0;

            var itr = 0;
            if (turn > 16 && turn <= 32)
            {
                itr = 1;
            }
            if (turn > 32 && turn <= 48)
            {
                itr = 2;
            }
            if (turn > 48)
            {
                itr = 3;
            }
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    if (black[row, col])
                    {
                        blackValue += evalBoard[row, col, itr];
                    }
                }
            }
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    if (white[row, col])
                    {
                        whiteValue += evalBoard[row, col, itr];
                    }
                }
            }
            return (blackValue - whiteValue) * 100/(int)stDevs[itr]; //評価値を標準化
        }
    }
}
