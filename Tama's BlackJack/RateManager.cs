using System;
using System.Collections.Generic;
using System.IO;

namespace Tama_s_BlackJack
{
    class RateManager
    {
        private int _rate;                          //レート
        private const int RANK_NUM = 9;             //区切りの個数
        public const int RANK_INTERVAL = 400;       //区切りの間隔
        private readonly int MaxRate;               //最高レート
        public bool enableRatePenalty = false;      //ランクペナルティ

        /// <summary>
        /// 現在のレート
        /// </summary>
        public int rate
        {
            get
            {
                return this._rate;
            }
        }

        //更新前のレート
        private int _rateBef;

        /// <summary>
        /// 更新前のレート
        /// </summary>
        public int rateBef
        {
            get
            {
                return _rateBef;
            }
        }

        private Encryption encrypt = new Encryption();  //暗号化用
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public RateManager()
        {
            _rateBef = 0;
            MaxRate = RANK_INTERVAL * RANK_NUM - 1;
            encrypt.fileName = "rateData.dat";
            try
            {
                List<String> data = encrypt.Decrypt();
                this._rate = int.Parse(data[data.Count - 1]);
            }
            catch (FileNotFoundException)
            {
                this._rate = 0;
            }
        }

        /// <summary>
        /// 切断時ペナルティを追加
        /// </summary>
        public void SetRatePenalty()
        {
            if (!enableRatePenalty) return;
            //切断時ペナルティ
            int ratePenalty = this.rate;
            ratePenalty -= 100;
            if (ratePenalty < 0) ratePenalty = 0;
            encrypt.Encrypt(ratePenalty.ToString());
        }

        /// <summary>
        /// レートを計算
        /// </summary>
        /// <param name="tScore">tScore</param>
        public void CalcRate(float tScore)
        {
            _rateBef = rate;
            _rate += (int)(tScore - _rate) / 3;
            if(_rate > MaxRate)_rate = MaxRate;
            encrypt.Encrypt(this._rate.ToString());
        }

    }
}
