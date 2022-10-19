using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tama_s_BlackJack
{
    class BlackjackExpectedValueCalculator
    {
        private CardManager nowCardDeck;
        private float _playerWinPercentage;
        private float _playerDrawPercentage;
        private float _playerLosePercentage;
        private bool cardDeckReset;

        public BlackjackExpectedValueCalculator()
        {
            nowCardDeck = null;
            _playerWinPercentage = 0.0f;
            _playerDrawPercentage = 0.0f;
            _playerLosePercentage = 0.0f;
            cardDeckReset = false;
        }

        public void SetNowCardDeck(CardManager cm)
        {
            nowCardDeck = cm;
            _playerWinPercentage = 0.0f;
            _playerDrawPercentage = 0.0f;
            _playerLosePercentage = 0.0f;
            cardDeckReset = false;
        }

        private void CalcPlayerWDLPercentage(int dealerOpenCard, int playerTotalCard)
        {
            if (cardDeckReset) throw new CardDeckNotResetException();



            cardDeckReset = true;
        }

        public class CardDeckNotResetException : Exception
        {
            public CardDeckNotResetException()
                : base("カードデッキがリセットされていません")
            {

            }
        }
    }
}
