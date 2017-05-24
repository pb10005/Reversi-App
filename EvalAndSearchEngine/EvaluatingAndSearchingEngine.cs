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

    public class EvaluatingAndSearchingEngine:ThinkingEngineBase.IThinkingEngine
    {
        /// <summary>
        /// エンジンの名前
        /// </summary>
        public string Name { get; } = "そこそこ強い";
        public EvaluatingAndSearchingEngine()
        {

        }
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

        List<ReversiBoard>[] moveTree;
        Dictionary<ReversiBoard, int> countMap = new Dictionary<ReversiBoard, int>();
        Dictionary<ReversiBoard, ReversiMove> moveMap = new Dictionary<ReversiBoard, ReversiMove>();
        Dictionary<ReversiBoard, List<ReversiBoard>> childMap = new Dictionary<ReversiBoard, List<ReversiBoard>>();
        //探索の深さ
        int depth=5;
        //探索の広さ
        int breadth=2;
        
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
                    var plyr = (i  + (int)player)%2 == 1 ?StoneType.Sente:StoneType.Gote;
                    var childMove = item.SearchLegalMoves(plyr);
                    var tmpList = new List<ReversiBoard>();
                        if (i==0&&childMove.Count==0)
                        {
                            throw new InvalidOperationException("合法手がありません");
                        }
                        foreach (var move in childMove)
                        {
                            var child = item.AddStone(move.Row, move.Col, plyr);
                            if (i == 0)
                                moveMap[child] = move;
                            tmpList.Add(child);
                        }
                        if ((i+ (int)player)%2 == 1)
                        {
                            tmpList = tmpList.OrderBy(x => -Eval.Execute(x.BlackToMat())+Eval.Execute(x.WhiteToMat())).ToList();
                        }
                        else
                        {
                            tmpList = tmpList.OrderBy(x => -Eval.Execute(x.WhiteToMat())+Eval.Execute(x.BlackToMat())).ToList();
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
                var count = player==StoneType.Sente?Eval.Execute(item.BlackToMat())-Eval.Execute(item.WhiteToMat()):Eval.Execute(item.WhiteToMat())-Eval.Execute(item.BlackToMat());
                countMap[item] = count;
            }
            for (int i = depth-1; i >= 1; i--)
            {
                foreach (var item in moveTree[i])
                {
                    var best = -999999;
                        foreach (var child in childMap[item])
                        {
                            //var count = (i + (int)player)%2 == 1 ? Eval.Execute(child.BlackToMat()) : Eval.Execute(child.WhiteToMat());
                            var count = countMap[child];
                            if (count > best)
                            {
                                best = count;
                            }
                        }
                        
                    countMap[item] = best;
                }
            }
            var bst = -99999;
            var bestMove = default(ReversiMove);
            foreach (var item in moveTree[1])
            {
                if (bst < countMap[item])
                {
                    bst = countMap[item];
                    bestMove = moveMap[item];
                }
            }
                var text = Enumerable.Range(0,moveTree.Count())
                        .Zip(moveTree.Select(x => string.Join(", ", x.Count) + "\n"),
                        (x, y) => x +": "+ y);
                System.IO.File.AppendAllText("debug.log",string.Join("\r\n",text));
                System.IO.File.AppendAllText("debug.log","\r\n");
                return bestMove;
            });

        }

    }
}
