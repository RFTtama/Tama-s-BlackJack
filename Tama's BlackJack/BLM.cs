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

        /// <summary>
        /// 期待値取得(未実装)
        /// </summary>
        /// <param name="firstCard"></param>
        /// <returns></returns>
        public float GetExpectedValue(int firstCard)
        {
            CardNum card = cardManager.GetCardNum();
            return 0.0f;
        }

        public BLM(CardManager cm) => cardManager = cm;
    }
}
