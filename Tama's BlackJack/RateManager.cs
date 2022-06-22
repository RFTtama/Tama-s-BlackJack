using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tama_s_BlackJack
{
    class RateManager
    {
        private int _rate;
        private const int maxRate = 200;
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
            encrypt.fileName = "rateData.dat";
            try
            {
                List<String> data = encrypt.Decrypt();
                this._rate = int.Parse(data[data.Count - 1]);
            }
            catch (FileNotFoundException)
            {
                this._rate = 1;
            }
        }

        public void CalcRate(float tScore)
        {
            int score = ((int)tScore / 200) - 2;
            if(score > 5)
            {
                score = 5;
            }
            if(this._rate + score > maxRate)
            {
                this._rate = maxRate;
            }
            else if(this._rate + score >= 1)
            {
                this._rate += score;
            }
            else
            {
                this._rate = 1;
            }
            encrypt.Encrypt(this._rate.ToString());
        }

        public float GetDifficultyMagn()
        {
            float magn;
            magn = this._rate * 0.1f;
            return magn;
        }
    }
}
