using System;
using System.Collections.Generic;
using System.IO;

namespace Tama_s_BlackJack
{
    class RateManager
    {
        private int _rate;
        private const int count = 9;
        public readonly int interval = 300;
        private readonly int MaxRate;
        public int rate
        {
            set
            {

            }
            get
            {
                return this._rate;
            }
        }

        private Encryption encrypt = new Encryption();
        
        public RateManager()
        {
            MaxRate = interval * count - 1;
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

        public void CalcRate(float tScore)
        {
            _rate += (int)(tScore - _rate) / 3;
            if(_rate > MaxRate)_rate = MaxRate;
            encrypt.Encrypt(this._rate.ToString());
        }

    }
}
