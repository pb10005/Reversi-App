using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvalEngine
{
    static class Eval
    {
        static int[,] evalBoard = new int[8, 8]
        {
            {89,7,-56,56,56,-56,7,89},
            {-10,-88,-73,18,18,-73,-88,-10},
            {-28,-98,24,-5,-5,24,-98,-28 },
            {65,-100,-4,-21,-21,-4,-100,65},
            {65,-100,-4,-21,-21,-4,-100,65},
            {-28,-98,24,-5,-5,24,-98,-28 },
            {-10,-88,-73,18,18,-73,-88,-10},
            {89,7,-56,56,56,-56,7,89}
        };
        public static int Execute(bool[,] black,bool[,] white)
        {
            var res = 0;
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    if (black[row, col])
                    {
                        res += evalBoard[row, col];
                    }
                    else if (white[row,col])
                    {
                        res -= evalBoard[row, col];
                    }
                }
            }
            return res;
        }
    }

}
