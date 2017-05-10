using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reversi.Core
{
    /// <summary>
    /// 黒と白それぞれについて、駒の有無を1と0で表すと
    /// 盤全体を64ビット整数で表現できる。
    /// 参考: Wikipedia オセロにおけるビットボード
    ///     　Wikipedia ビット演算
    /// </summary>
    public class ReversiBoard
    {
        ulong black = 0;
        ulong white = 0;
        StoneType player;
        /// <summary>
        /// オセロの初期状態を返す
        /// </summary>
        /// <returns></returns>
        public static ReversiBoard InitBoard()
        {
            var res = new ReversiBoard()
            {
                //オセロの初期状態
                black = 0x0000000810000000,
                white = 0x0000001008000000
            };
            return res;
        }
        /// <summary>
        /// 黒のbitboardを、boolの2次元配列に変換して返す
        /// </summary>
        /// <returns></returns>
        public bool[,] BlackToMat()
        {
            var res = new bool[8, 8];
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    ulong mask = (ulong)1 << (8 * row + col);
                    if ((black & mask) == 0)
                    {
                        res[row, col] = false;
                    }
                    else
                    {
                        res[row, col] = true;
                    }
                }
            }
            return res;
        }
        /// <summary>
        /// 白のbitboardを、boolの2次元配列に変換して返す
        /// </summary>
        /// <returns></returns>
        public bool[,] WhiteToMat()
        {
            var res = new bool[8, 8];
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    ulong mask = (ulong)1 << (8 * row + col);
                    if ((white & mask) == 0)
                    {
                        res[row, col] = false;
                    }
                    else
                    {
                        res[row, col] = true;
                    }
                }
            }
            return res;
        }
        /// <summary>
        /// 石を打つ
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        public ReversiBoard AddStone(int row,int col,StoneType player)
        {
            this.player = player;
            ulong pl = 0;
            ulong opp = 0;
            switch (this.player)
            {
                case StoneType.None:
                    break;
                case StoneType.Sente:
                    pl = black;
                    opp = white;
                    break;
                case StoneType.Gote:
                    pl = white;
                    opp = black;
                    break;
                default:
                    break;
            }
            ulong mov = (ulong) 1 << (8 * row + col);
            var rev = Reverse(pl, opp, mov, Right) 
                        |Reverse(pl, opp, mov, Left)
                        |Reverse(pl, opp, mov, Up) 
                        |Reverse(pl, opp, mov, Down)
                        |Reverse(pl, opp, mov, UpRight)
                        |Reverse(pl, opp, mov, UpLeft) 
                        |Reverse(pl, opp, mov, DownRight)
                        |Reverse(pl, opp, mov, DownLeft);
            if (rev != 0)
            {
                pl  ^= mov | rev;
                opp ^= rev;
                ReversiBoard board = new ReversiBoard();
                switch (player)
                {
                    case StoneType.Sente:
                        board.black = pl;
                        board.white = opp;
                        break;
                    case StoneType.Gote:
                        board.black = opp;
                        board.white = pl;
                        break;
                    default:
                        break;
                }
                return board;
            }
            else
            {
                throw new ArgumentException("非合法手です");
            }
        }
        private ulong Reverse(ulong player,ulong opposite,ulong mov,Func<ulong,ulong>transfer)
        {
            ulong rev = 0;
            if (((player | opposite) & mov) != 0)
                return rev;

            ulong mask = transfer(mov);
            while (mask != 0 && (mask & opposite) != 0)
            { 
                rev |= mask;
                mask = transfer(mask);
            }
            if ((mask & player) == 0)
                return 0;
            else
                return rev; // 反転パターン
        }
        private ulong Right(ulong m)
        {
            //右端から左端に進むことを避けるためにフィルターをかける
            return (m >> 1) & 0x7f7f7f7f7f7f7f7f;
        }
        private ulong Left(ulong m)
        {
            //左端から右端に進むことを避けるためにフィルターをかける
            return (m << 1) & 0xfefefefefefefefe;
        }
        private ulong Up(ulong m)
        {
            //上端から下端に進むことを避けるためにフィルターをかける
            return (m << 8) & 0xffffffffffffff00;
        }
        private ulong Down(ulong m)
        {
            //下端から上端に進むことを避けるためにフィルターをかける
            return (m >> 8) & 0x00ffffffffffffff;
        }
        private ulong UpRight(ulong m)
        {
            return Up(Right(m));
        }
        private ulong UpLeft(ulong m)
        {
            return Up(Left(m));
        }
        private ulong DownRight(ulong m)
        {
            return Down(Right(m));
        }
        private ulong DownLeft(ulong m)
        {
            return Down(Left(m));
        }
    }
}
