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
        public static Eval FromParamsString(string paramsString)
        {
            var paramsArray = paramsString.Split(',').Select(x => int.Parse(x)).ToArray();
            var res = new Eval();
            for (int row = 0; row < 4; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    var param = paramsArray[4 * row + col];

                    res.evalBoard[row, col] = param;
                    res.evalBoard[7 - row, col] = param;
                    res.evalBoard[row, 7 - col] = param;
                    res.evalBoard[7 - row, 7 - col] = param;

                }
            }
            return res;
        }
        private int[,] evalBoard = new int[8, 8];
        public int Execute(bool[,] black, bool[,] white)
        {
            var blackValue = 0;
            var whiteValue = 0;
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    if (black[row, col])
                    {
                        blackValue += evalBoard[row, col];
                    }
                }
            }
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    if (white[row, col])
                    {
                        whiteValue += evalBoard[row, col];
                    }
                }
            }
            return blackValue - whiteValue;
        }
    }
    
}
