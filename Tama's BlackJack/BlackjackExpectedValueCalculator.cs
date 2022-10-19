using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tama_s_BlackJack
{
    class BlackjackExpectedValueCalculator
    {
        //現在のデッキ
        private CardManager nowCardDeck;
        //プレイヤーが勝つ確率
        private double _playerWinPercentage;

        public double playerWinPercentage
        {
            get
            {
                return _playerWinPercentage;
            }
        }

        //プレイヤーが引き分ける確率
        private double _playerDrawPercentage;

        public double playerDrawPercentage
        {
            get
            {
                return _playerDrawPercentage;
            }
        }

        //プレイヤーが負ける確率
        private double _playerLosePercentage;

        public double playerLosePercentage
        {
            get
            {
                return _playerLosePercentage;
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public BlackjackExpectedValueCalculator()
        {
            nowCardDeck = null;
            ResetDrawPercentage();
        }

        /// <summary>
        /// デッキをセット
        /// </summary>
        /// <param name="cm">現在のデッキ</param>
        public void SetNowCardDeck(CardManager cm)
        {
            nowCardDeck = cm;
            ResetDrawPercentage();
        }

        /// <summary>
        /// 勝敗確率をリセット
        /// </summary>
        private void ResetDrawPercentage()
        {
            _playerWinPercentage = 0.0;
            _playerDrawPercentage = 0.0;
            _playerLosePercentage = 0.0;
        }

        /// <summary>
        /// 指定したカードを引く確率
        /// </summary>
        /// <param name="cardList">計算するデッキ</param>
        /// <param name="number">計算するカード値</param>
        /// <returns>確率</returns>
        private double GetCardDrawPer(List<int> cardList, int number)
        {
            //合計カード数
            int totalCards = 0;

            for(int i = 0; i < 13; i++)//合計カード数を計算
            {
                totalCards += cardList[i];
            }

            double per = cardList[number] / totalCards;//次そのカードを引く確率を計算

            return per;
        }

        /// <summary>
        /// プレイヤーの勝敗確率を計算する
        /// </summary>
        /// <param name="dealerOpenCard">ディーラーのオープンカード</param>
        /// <param name="playerCardTotal">プレイヤーのカード合計</param>
        private void CalcPlayerWDLPercentage(int dealerOpenCard, int playerCardTotal)
        {
            //勝敗確率をリセット
            ResetDrawPercentage();

            //ディーラーのカード合計をオープンハンドに設定
            int dealerCardTotal = dealerOpenCard;

            //現在のカード残数
            List<int> remainingCard = new List<int>();

            for(int index = 0; index < 13; index++)//カード残数を取得
            {
                remainingCard.Add(nowCardDeck.GetRemainingCard(index));
            }

        }
    }
}
