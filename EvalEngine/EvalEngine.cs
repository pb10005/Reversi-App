using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reversi.Core;
using ThinkingEngineBase;
using static EvalEngine.Eval;

namespace EvalEngine
{
    public class EvalEngine : IThinkingEngine
    {
        int best=0;
        //オセロの局面
        Reversi.Core.ReversiBoard board;
        //先手または後手
        Reversi.Core.StoneType player;
        //合法手のリスト
        List<ReversiMove> legalMoves = new List<ReversiMove>();
        public string Name
        {
            get
            {
                return "評価のみ";
            }
        }

        public int GetEval()
        {
            return best;
        }

        public void SetTimeLimit(int milliSecond)
        {
            throw new NotImplementedException();
        }

        public async Task<ReversiMove> Think(ReversiBoard board, StoneType player)
        {
            return await Task.Run(() =>
            {
                this.board = board;
                this.player = player;
                legalMoves = board.SearchLegalMoves(player); //合法手
                if (legalMoves.Count == 0)
                {
                    throw new InvalidOperationException("合法手がありません");
                }
                else
                {
                    if (player == StoneType.Sente)
                    {
                        best = -999999;
                    }
                    else
                    {
                        best = 999999;
                    }
                    var bestMove = default(ReversiMove);
                    foreach (var item in board.SearchLegalMoves(player))
                    {
                        var child = board.AddStone(item.Row, item.Col, player);
                        var eval =Execute(child.BlackToMat(), child.WhiteToMat());
                        switch (player)
                        {
                            case StoneType.None:
                                break;
                            case StoneType.Sente:
                                if (best < eval)
                                {
                                    best = eval;
                                    bestMove = item;
                                }
                                break;
                            case StoneType.Gote:
                                if (best > eval)
                                {
                                    best = eval;
                                    bestMove = item;
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    return bestMove;
                }
            });
        }
    }
}
