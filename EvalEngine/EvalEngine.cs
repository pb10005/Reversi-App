using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reversi.Core;
using ThinkingEngineBase;
using static EvalEngine.Eval;
using System.Diagnostics;
using System.Threading;

namespace EvalEngine
{
    public class ThinkingEngine : IThinkingEngine
    {
        const int max =  10000;
        const int min = -10000;
        Eval evaluator = FromParamsString("-39,-100,-81,-31,-100,-94,68,-59,95,-78,82,11,59,5,73,22,50,-100,1,37,-89,-47,27,-45,-43,-81,54,28,-29,-46,36,52,67,76,-30,51,53,-89,-74,72,-72,-75,41,-32,48,-33,38,47,-7,99,6,13,26,86,-99,8,-77,-80,12,4,85,-59,48,9");
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
                return "アルファベータ";
            }
        }

        public void SetTimeLimit(int milliSecond)
        {
            timeLimit = milliSecond;
        }
        Dictionary<ReversiMove, int> countMap = new Dictionary<ReversiMove, int>();
        //探索の深さ
        const int depth = 4;
        int timeLimit;
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
                    var res = await AlphaBeta(nextBoard, player, depth, int.MinValue,int.MaxValue);
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
        /// アルファベータ法
        /// </summary>
        /// <param name="board"></param>
        /// <param name="player"></param>
        /// <param name="depth"></param>
        /// <param name="alpha"></param>
        /// <param name="beta"></param>
        /// <returns></returns>
        private async Task<int> AlphaBeta(ReversiBoard board, StoneType player, int depth, int alpha, int beta)
        {
            return await Task.Run(async () =>
            {
                if (depth == 0)
                {
                    return evaluator.Execute(board);
                }
                var nextPlayer = player == StoneType.Sente ? StoneType.Gote : StoneType.Sente;
                var children = board.SearchLegalMoves(nextPlayer);
                #region パス
                if (children.Count == 0)
                {
                    var passed = board.SearchLegalMoves(player);
                    if (passed.Count == 0)
                    {
                        //終了なので、勝敗を判定
                        var bl = board.NumOfBlack();
                        var wh = board.NumOfWhite();
                        if (bl > wh)
                        {
                            return max; //先手勝ち
                        }
                        else if (bl < wh)
                        {
                            return min; //後手勝ち
                        }
                        else
                        {
                            return 0; //引き分け
                        }
                    }
                    if (nextPlayer == StoneType.Sente)
                    {
                        foreach (var item in passed)
                        {
                            var nextBoard = board.Pass();
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
                        foreach (var item in passed)
                        {
                            var nextBoard = board.Pass();
                            var alphabeta = await AlphaBeta(nextBoard, StoneType.Gote, depth - 1, alpha, beta);
                            beta = beta > alphabeta ? alphabeta : beta;
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
                        beta = beta > alphabeta ? alphabeta:beta;
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
