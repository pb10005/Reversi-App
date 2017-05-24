using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reversi.Core;

namespace EvalAndSearchEngine
{
    public static class Eval
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
        public static int Execute(bool[,] board)
        {
            var res = 0;
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    if (board[row,col])
                    {
                        res += evalBoard[row, col];
                    }
                }
            }
            return res;
        }
    }

    public class SuperCounting:ThinkingEngineBase.IThinkingEngine
    {
        /// <summary>
        /// エンジンの名前
        /// </summary>
        public string Name { get; } = "Super Greedy";
        public SuperCounting()
        {

        }
        public SuperCounting(int depth,int breadth)
        {
            this.depth = depth;
            this.breadth = breadth;
        }
        /// <summary>
        /// 思考時間の上限を設定する
        /// </summary>
        /// <param name="milliSecond"></param>
        public void SetTimeLimit(int milliSecond)
        {
            //今は実装しない
        }

        Dictionary<ReversiMove, int> countMap = new Dictionary<ReversiMove, int>();
        //探索の深さ
        int depth=5;
        //探索の広さ
        int breadth=6;
        StoneType currentPlayer;
        
        /// <summary>
        /// 盤の情報をもとに思考し、次の手を返す
        /// </summary>
        /// <param name="board"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        public async Task<ReversiMove> Think(ReversiBoard board, StoneType player)
        {
            currentPlayer = player;
            countMap = new Dictionary<ReversiMove, int>();
            return await Task.Run(async () =>
            {
                var children = board.SearchLegalMoves(player);
                if (children.Count==0)
                {
                    throw new InvalidOperationException("合法手がありません");
                }
                foreach (var item in children)
                {
                    var nextBoard = board.AddStone(item.Row, item.Col, player);
                    var res = MiniMax(nextBoard,player,4);
                    countMap[item] = await res;
                }
                if (player == StoneType.Sente)
                {
                    var max = countMap.FirstOrDefault(x => x.Value == countMap.Values.Max());
                    best = max.Value;
                    return max.Key;
                }
                else
                {
                    var min = countMap.FirstOrDefault(x => x.Value == countMap.Values.Min());
                    best = min.Value;
                    return min.Key;
                }
                
            });

        }
        int best = 0;
        /// <summary>
        /// ミニマックス法
        /// </summary>
        /// <param name="board"></param>
        /// <param name="player"></param>
        /// <param name="depth"></param>
        /// <returns></returns>
        public async Task<int> MiniMax(ReversiBoard board,StoneType player, int depth)
        {
            return await Task.Run(async () =>
            {
                if (depth == 0)
                {
                    return board.NumOfBlack() - board.NumOfWhite();
                }
                int bestEval = int.MinValue;
                foreach (var item in board.SearchLegalMoves(player))
                {
                    switch (player)
                    {
                        case StoneType.None:
                            break;
                        case StoneType.Sente:
                            var val = await MiniMax(board.AddStone(item.Row, item.Col, StoneType.Sente), StoneType.Gote, depth - 1);
                            if (bestEval < val)
                            {
                                bestEval = val;
                            }
                            break;
                        case StoneType.Gote:
                            var val2 = await MiniMax(board.AddStone(item.Row, item.Col, StoneType.Gote), StoneType.Sente, depth - 1);
                            if (bestEval < -val2)
                            {
                                bestEval = -val2;
                            }
                            break;
                        default:
                            break;
                    }
                }
                return bestEval;
            });
        }
        public int GetEval()
        {
            return best;
        }
    }
}
