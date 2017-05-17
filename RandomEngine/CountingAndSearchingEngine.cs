using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reversi.Core;

namespace ThinkingEngine
{
    public class CountingAndSearchingEngine:ThinkingEngineBase.IThinkingEngine
    {
        public CountingAndSearchingEngine(int depth,int breadth)
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

            if (board.NumOfBlack()+board.NumOfWhite()>=60-depth)
            {
                return await new CountingEngine().Think(board, player);
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
                    var tmpList = new List<ReversiBoard>();
                    foreach (var move in childMove)
                    {
                        var child = item.AddStone(move.Row,move.Col,plyr);
                        moveMap[child] = move;
                        tmpList.Add(child);
                        //var cnt = (i % 2 + (int)player) == 1 ?child.NumOfBlack():child.NumOfWhite();
                        //countMap[item] = cnt;
                    }
                    if ((i%2 + (int) player) ==1)
                    {
                        tmpList = tmpList.OrderBy(x => -x.NumOfBlack()).ToList();
                    }
                    else
                    {
                        tmpList.OrderBy(x => -x.NumOfWhite()).ToList();
                    }
                    if (tmpList.Count > breadth)
                    {
                        for (int j = 0; j < breadth; j++)
                        {
                            var child = tmpList[j];
                            moveTree[i+1].Add(child);
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
                var count = player==StoneType.Sente?item.NumOfBlack():item.NumOfWhite();
                countMap[item] = count;
            }
            for (int i = depth-1; i >= 1; i--)
            {
                foreach (var item in moveTree[i])
                {
                    var best = 0;
                        foreach (var child in childMap[item])
                        {
                            var count = (i % 2 + (int)player) == 1 ? child.NumOfBlack() : child.NumOfWhite();
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
