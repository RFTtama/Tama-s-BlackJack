using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tama_s_BlackJack
{
    public class BLM
    {
        private CardManager cardManager;

        public float GetExpectedValue(int firstCard)
        {
            CardNum card = cardManager.GetCardNum();
            return 0.0f;
        }

        public BLM(CardManager cm) => cardManager = cm;
    }
}
