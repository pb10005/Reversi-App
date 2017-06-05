using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Reversi.Core;
using ThinkingEngineBase;

namespace EvalEngine
{
    /// <summary>
    /// アルファベータ法による探索
    /// </summary>
    public class ThinkingEngine : IThinkingEngine
    {
        const int max =  10000;
        const int min = -10000;
        const int depth = 5;　//探索の深さ

        int timeLimit;
        StoneType currentPlayer;
        int best = 0;

        Eval evaluator = Eval.FromParamsString("62,-26,-100,-48,1,-100,93,-91,41,-9,7,-12,12,8,14,-30,-10,-54,24,25,-42,-81,-2,-12,-94,-70,4,26,-13,-100,32,-6,66,74,-57,31,-34,0,-100,46,-62,-65,35,61,71,-43,33,-30,36,74,83,3,100,100,-57,-26,-2,-39,-8,81,51,-72,22,11");
        /// <summary>
        /// 評価関数のsetter
        /// </summary>
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

        /// <summary>
        /// 思考エンジンの名前
        /// </summary>
        public string Name => "アルファベータ";

        /// <summary>
        /// 制限時間を設定する
        /// </summary>
        /// <param name="milliSecond"></param>
        public void SetTimeLimit(int milliSecond)
        {
            timeLimit = milliSecond;
        }
        Dictionary<ReversiMove, int> countMap = 
                        new Dictionary<ReversiMove, int>();
        

        /// <summary>
        /// 盤の情報をもとに思考し、次の手を返す
        /// </summary>
        /// <param name="board"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        public async Task<ReversiMove> Think(
                                    ReversiBoard board
                                    , StoneType player)
        {
            currentPlayer = player;
            countMap = new Dictionary<ReversiMove, int>();
            return await Task.Run(() =>
            {
                var children = board.SearchLegalMoves(player);
                if (children.Count == 0)
                {
                    throw new InvalidOperationException("合法手がありません");
                }
                Parallel.ForEach(children, item =>
                {
                    //並列化でCPUをめいっぱい使える
                    //それなりに速くなる
                    countMap[item] = AlphaBeta(
                                  board.AddStone(item.Row, item.Col, player)
                                , player
                                , depth
                                , int.MinValue
                                , int.MaxValue)
                                .Result;
                });
                if (player == StoneType.Sente)
                {
                    var max = countMap
                        .FirstOrDefault(x =>
                                    x.Value == countMap.Values.Max());
                    best = max.Value;
                    return max.Key;
                }
                else
                {
                    var min = countMap
                        .FirstOrDefault(x =>
                                    x.Value == countMap.Values.Min());
                    best = min.Value;
                    return min.Key;
                }
            });
        }

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
                    //探索の終端
                    return evaluator.Execute(board);
                }
                var nextPlayer = player == StoneType.Sente ? 
                                    StoneType.Gote : StoneType.Sente;
                var children = board.SearchLegalMoves(nextPlayer);
                #region パス
                if (children.Count == 0)
                {
                    if (board.SearchLegalMoves(player).Count == 0)
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
                        var alphabeta = await AlphaBeta(
                                                  board.Pass()
                                                , StoneType.Sente
                                                , depth - 1
                                                , alpha
                                                , beta);
                        alpha = alpha > alphabeta ? alpha : alphabeta;
                        if (alpha >= beta)
                        {
                            return beta; //枝刈り
                        }
                        return alpha;
                    }
                    else
                    {
                        var alphabeta = await AlphaBeta(
                                                  board.Pass()
                                                , StoneType.Gote
                                                , depth - 1
                                                , alpha
                                                , beta);
                        beta = beta > alphabeta ? alphabeta : beta;
                        if (alpha >= beta)
                        {
                            return alpha; //枝刈り
                        }
                        return beta;
                    }
                }
                #endregion
                if (nextPlayer == StoneType.Sente)
                {
                    foreach (var item in children)
                    {
                        var alphabeta = await AlphaBeta(
                                                  board.AddStone(
                                                      item.Row
                                                    , item.Col
                                                    , StoneType.Sente)
                                                , StoneType.Sente
                                                , depth-1
                                                , alpha
                                                , beta);
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
                        var alphabeta = await AlphaBeta(
                                                board.AddStone(
                                                      item.Row
                                                    , item.Col
                                                    , StoneType.Gote)
                                                , StoneType.Gote
                                                , depth - 1
                                                , alpha
                                                , beta);
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
        /// <summary>
        /// 現局面の評価値を返す
        /// </summary>
        /// <returns></returns>
        public int GetEval()
        {
            return best;
        }
    }
}
