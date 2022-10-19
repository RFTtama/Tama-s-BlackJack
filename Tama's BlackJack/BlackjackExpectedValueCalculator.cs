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

        public BlackjackExpectedValueCalculator()
        {
            nowCardDeck = null;
        }

        public void SetNowCardDeck(CardManager cm)
        {
            nowCardDeck = cm;
        }
    }
}
