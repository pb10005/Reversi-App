using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reversi.Core
{
    public class Match
    {
        public Match()
        {

        }
        public ReversiBoard CurrentBoard { get; private set; }
        public StoneType CurrentPlayer { get; private set; }
        public int Turn { get; private set; }

        List<bool> passList = new List<bool>();
        /// <summary>
        /// 終了を通知するイベント
        /// </summary>
        public event Action<string> End = delegate { };
        public void Init()
        {
            Turn = 0;
            CurrentBoard = ReversiBoard.InitBoard();
            CurrentPlayer = StoneType.Sente;
            passList = new List<bool>();
        }
        public MoveResult Move(int row,int col)
        {
            try
            {
                CurrentBoard = CurrentBoard.AddStone(row, col, CurrentPlayer);
                passList.Add(false);
                if(System.IO.File.Exists("debug.log"))
                    System.IO.File.AppendAllText("debug.log",string.Format("{0}:{1},{2}\r\n",Turn,row,col));
            }
            catch (ArgumentException)
            {
                return MoveResult.Illegal;
            }
            CurrentPlayer = CurrentPlayer == StoneType.Sente ? StoneType.Gote : StoneType.Sente;
            
            if (CurrentBoard.NumOfBlack()+CurrentBoard.NumOfWhite()>=64)
            {
                //終了
                //End(CurrentBoard.ResultString());
                return MoveResult.End;
            }
            Turn++;
            return MoveResult.OK;
        }
        public void Pass()
        {
            if (passList.Last())
            {
                End(CurrentBoard.ResultString());
            }
            else
            {
                Turn++;
                CurrentPlayer = CurrentPlayer == StoneType.Sente ? StoneType.Gote : StoneType.Sente;
                passList.Add(true);
            }
        }
        public void Surrender()
        {
            var winner = CurrentPlayer == StoneType.Sente ? StoneType.Gote.ToString() : StoneType.Sente.ToString();
            End(winner);
        }
    }
}
