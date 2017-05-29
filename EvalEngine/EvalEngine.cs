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
    public class ThinkingEngine : IThinkingEngine
    {
        Eval evaluator = FromParamsString("86,17,31,100,2,-85,-78,66,-34,-74,42,48,47,-99,56,16");
        public Eval Evaluator
        {
            set
            {
                if (evaluator == default(Eval))
                {
                    evaluator = value;
                }
                else
                {
                    throw new InvalidOperationException("既に登録されています");
                }
            }
        }
        //合法手のリスト
        List<ReversiMove> legalMoves = new List<ReversiMove>();

        public string Name
        {
            get
            {
                return "Minmax";
            }
        }

        public void SetTimeLimit(int milliSecond)
        {
            throw new NotImplementedException();
        }
        Dictionary<ReversiMove, int> countMap = new Dictionary<ReversiMove, int>();
        //探索の深さ
        int depth = 5;
        //探索の広さ
        int breadth = 6;
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
                if (children.Count == 0)
                {
                    throw new InvalidOperationException("合法手がありません");
                }
                foreach (var item in children)
                {
                    var nextBoard = board.AddStone(item.Row, item.Col, player);
                    var res = await AlphaBeta(nextBoard, player,3,-1000,1000);
                    countMap[item] = res;
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
        public async Task<int> MiniMax(ReversiBoard board, StoneType player, int depth)
        {
            return await Task.Run(async () =>
            {
                if (depth == 0)
                {
                    return evaluator.Execute(board.BlackToMat(), board.WhiteToMat());
                }
                int bestEval = player == StoneType.Sente ? int.MaxValue : int.MinValue;
                var nextPlayer = player == StoneType.Sente ? StoneType.Gote : StoneType.Sente;
                var children = board.SearchLegalMoves(nextPlayer);
                if (children.Count == 0)
                {
                    var passed = board.SearchLegalMoves(player);
                    if (passed.Count == 0)
                    {
                        return evaluator.Execute(board.BlackToMat(), board.WhiteToMat());
                    }
                    foreach (var item in passed)
                    {
                        switch (player)
                        {
                            case StoneType.Sente:
                                var val = await MiniMax(board.AddStone(item.Row, item.Col, StoneType.Sente), StoneType.Sente, depth - 1);
                                if (bestEval < val)
                                {
                                    bestEval = val;
                                }
                                break;
                            case StoneType.Gote:
                                var val2 = await MiniMax(board.AddStone(item.Row, item.Col, StoneType.Gote), StoneType.Gote, depth - 1);
                                if (-bestEval < -val2)
                                {
                                    bestEval = val2;
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
                foreach (var item in children)
                {
                    switch (nextPlayer)
                    {
                        case StoneType.None:
                            break;
                        case StoneType.Sente:
                            var val = await MiniMax(board.AddStone(item.Row, item.Col, StoneType.Sente), StoneType.Sente, depth - 1);
                            if (bestEval < val)
                            {
                                bestEval = val;
                            }
                            break;
                        case StoneType.Gote:
                            var val2 = await MiniMax(board.AddStone(item.Row, item.Col, StoneType.Gote), StoneType.Gote, depth - 1);
                            if (-bestEval < -val2)
                            {
                                bestEval = val2;
                            }
                            break;
                        default:
                            break;
                    }
                }
                return bestEval;
            });
        }

        private async Task<int> AlphaBeta(ReversiBoard board, StoneType player, int depth, int alpha, int beta)
        {
            return await Task.Run(async () =>
            {
                if (depth == 0)
                {
                    return evaluator.Execute(board.BlackToMat(), board.WhiteToMat());
                }
                var nextPlayer = player == StoneType.Sente ? StoneType.Gote : StoneType.Sente;
                var children = board.SearchLegalMoves(nextPlayer);
                #region パス
                if (children.Count == 0)
                {
                    var passed = board.SearchLegalMoves(player);
                    if (passed.Count == 0)
                    {
                        return evaluator.Execute(board.BlackToMat(), board.WhiteToMat());
                    }
                    if (player == StoneType.Sente)
                    {
                        foreach (var item in children)
                        {
                            var nextBoard = board.AddStone(item.Row, item.Col, StoneType.Sente);
                            var alphabeta = await AlphaBeta(nextBoard, StoneType.Sente, depth - 1, alpha, beta);
                            alpha = alpha > alphabeta ? alpha : alphabeta;
                            if (alpha >= beta)
                            {
                                return beta; //枝刈り
                            }
                        }
                        return alpha;
                    }
                    else
                    {
                        foreach (var item in children)
                        {
                            var nextBoard = board.AddStone(item.Row, item.Col, StoneType.Gote);
                            var alphabeta = await AlphaBeta(nextBoard, StoneType.Gote, depth - 1, alpha, beta);
                            beta = -beta < -alphabeta ? alphabeta : beta;
                            if (alpha >= beta)
                            {
                                return alpha; //枝刈り
                            }
                        }
                        return beta;
                    }
                }
                #endregion
                if (nextPlayer == StoneType.Sente)
                {
                    foreach (var item in children)
                    {
                        var nextBoard = board.AddStone(item.Row,item.Col,StoneType.Sente);
                        var alphabeta = await AlphaBeta(nextBoard,StoneType.Sente,depth-1,alpha,beta);
                        alpha = alpha > alphabeta ? alpha : alphabeta;
                        if (alpha >= beta)
                        {
                            return beta; //枝刈り
                        }
                    }
                    return alpha;
                }
                else
                {
                    foreach (var item in children)
                    {
                        var nextBoard = board.AddStone(item.Row, item.Col, StoneType.Gote);
                        var alphabeta = await AlphaBeta(nextBoard, StoneType.Gote, depth - 1, alpha, beta);
                        beta = -beta < -alphabeta ? alphabeta:beta;
                        if (alpha >= beta)
                        {
                            return alpha; //枝刈り
                        }
                    }
                    return beta;
                }
            });
        }
        public int GetEval()
        {
            return best;
        }
    }
}
