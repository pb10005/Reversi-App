using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvalEngine
{
    public struct EvalBoard
    {
        public EvalBoard(ulong black, ulong white)
        {
            Black = black;
            White = white;
        }
        public ulong Black, White;
    }
}
