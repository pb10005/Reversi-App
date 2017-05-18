using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reversi.Core;

namespace ThinkingEngine
{
    public static class Eval
    {
        static int[,] evalBoard = new int[8, 8]
        {
            {20,1,3,2,2,3,1,20 },
            {1,-1,-1,-1,-1,-1,-1,1 },
            {3,-1,1,1,1,1,-1,3 },
            {2,-1,1,1,1,1,-1,2 },
            {2,-1,1,1,1,1,-1,2},
            {3,-1,1,1,1,1,-1,3 },
            {1,-1,-1,-1,-1,-1,-1,1 },
            {20,1,3,2,2,3,0,20 }
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

    public class EvaluatingAndSearchingEngine:ThinkingEngineBase.IThinkingEngine
    {
        public EvaluatingAndSearchingEngine(int depth,int breadth)
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
        //オセロの局面
        Reversi.Core.ReversiBoard board;
        //先手または後手
        Reversi.Core.StoneType player;
        //合法手のリスト
        List<ReversiMove> legalMoves = new List<ReversiMove>();

        List<ReversiBoard>[] moveTree;
        Dictionary<ReversiBoard, int> countMap = new Dictionary<ReversiBoard, int>();
        Dictionary<ReversiBoard, ReversiMove> moveMap = new Dictionary<ReversiBoard, ReversiMove>();
        Dictionary<ReversiBoard, List<ReversiBoard>> childMap = new Dictionary<ReversiBoard, List<ReversiBoard>>();
        //探索の深さ
        int depth;
        //探索の広さ
        int breadth;
        
        /// <summary>
        /// 盤の情報をもとに思考し、次の手を返す
        /// </summary>
        /// <param name="board"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        public async Task<ReversiMove> Think(ReversiBoard board, StoneType player)
        {
            return await Task<ReversiMove>.Run(async () =>
            {
                
            if (board.NumOfBlack()+board.NumOfWhite()>=58)
            {
                
                return await new CountingEngine().Think(board, player);
            }
                else if (board.NumOfBlack() + board.NumOfWhite() >= 60 - depth)
                {
                    depth = 60 - board.NumOfBlack() - board.NumOfWhite() - 1;
                }
                moveTree = new List<ReversiBoard>[depth+1];
            moveTree[0] = new List<ReversiBoard>() { board };
            for (int i = 0; i < depth; i++)
            {
                moveTree[i+1] = new List<ReversiBoard>();
                foreach (var item in moveTree[i])
                {
                    childMap.Add(item,new List<ReversiBoard>());
                    var plyr = (i % 2 + (int)player) == 1 ?StoneType.Sente:StoneType.Gote;
                    var childMove = item.SearchLegalMoves(plyr);
                        if (childMove.Count==0)
                        {
                            throw new InvalidOperationException("合法手がありません");
                        }
                    var tmpList = new List<ReversiBoard>();
                    foreach (var move in childMove)
                    {
                        var child = item.AddStone(move.Row,move.Col,plyr);
                        moveMap[child] = move;
                        tmpList.Add(child);
                    }
                        if ((i % 2 + (int)player) == 1)
                        {
                            tmpList = tmpList.OrderBy(x => -Eval.Execute(x.WhiteToMat())).ToList();
                        }
                        else
                        {
                            tmpList.OrderBy(x => -Eval.Execute(x.WhiteToMat())).ToList();
                        }
                        if (tmpList.Count > breadth&&i!=0)
                        {
                            for (int j = 0; j < breadth; j++)
                            {
                                var child = tmpList[j];
                                moveTree[i + 1].Add(child);
                                childMap[item].Add(child);

                            }
                        }
                        else
                        {
                            foreach (var child in tmpList)
                            {
                                moveTree[i + 1].Add(child);
                                childMap[item].Add(child);
                            }
                        }
                    
                }
            }
            foreach (var item in moveTree[depth])
            {
                var count = player==StoneType.Sente?Eval.Execute(item.BlackToMat()):Eval.Execute(item.WhiteToMat());
                countMap[item] = count;
            }
            for (int i = depth-1; i >= 1; i--)
            {
                foreach (var item in moveTree[i])
                {
                    var best = -100;
                        foreach (var child in childMap[item])
                        {
                            var count = (i % 2 + (int)player) == 1 ? Eval.Execute(child.BlackToMat()) : Eval.Execute(child.WhiteToMat());
                            if (count > best)
                            {
                                best = count;
                            }
                        }
                        
                    countMap[item] = best;
                }
            }
            var bst = 0;
            var bestMove = default(ReversiMove);
            foreach (var item in moveTree[1])
            {
                if (bst < countMap[item])
                {
                    bst = countMap[item];
                    bestMove = moveMap[item];
                }
            }
            return bestMove;
            });

        }

    }
}
