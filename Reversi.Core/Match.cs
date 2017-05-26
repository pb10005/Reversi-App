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

        bool previouslyPassed;
        /// <summary>
        /// 終了を通知するイベント
        /// </summary>
        public event Action<string> End = delegate { };
        public void Init()
        {
            previouslyPassed = false;
            Turn = 0;
            CurrentBoard = ReversiBoard.InitBoard();
            CurrentPlayer = StoneType.Sente;
        }
        public bool Move(int row,int col)
        {
            try
            {
                CurrentBoard = CurrentBoard.AddStone(row, col, CurrentPlayer);
            }
            catch(ArgumentException)
            {
                return false;
            }
            CurrentPlayer = CurrentPlayer == StoneType.Sente ? StoneType.Gote : StoneType.Sente;
            
            if (CurrentBoard.NumOfBlack()+CurrentBoard.NumOfWhite()==64)
            {
                //終了
                End(CurrentBoard.ResultString());
                return false;
            }
            else if (CurrentBoard.SearchLegalMoves(CurrentPlayer).Count==0)
            {
                Pass();
                return false;
            }
            Turn++;
            previouslyPassed = false;
            return true;
        }
        public void Pass()
        {
            if (previouslyPassed)
            {
                End("終了です");
                previouslyPassed = false;
            }
            else
            {
                Turn++;
                CurrentPlayer = CurrentPlayer == StoneType.Sente ? StoneType.Gote : StoneType.Sente;
                previouslyPassed = true;
            }
        }
        public void Surrender()
        {
            var winner = CurrentPlayer == StoneType.Sente ? StoneType.Gote.ToString() : StoneType.Sente.ToString();
            End(winner);
        }
    }
}
