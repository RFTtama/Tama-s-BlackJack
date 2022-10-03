using System;
using System.Collections.Generic;

namespace Tama_s_BlackJack
{
    class PlData
    {
        private List<int> point;        //ポイントデータ保存リスト
        private List<int> tScore;       //スコアデータ保存リスト
        private List<int> gameMode;     //ゲームモード保存リスト
        private const int modeNum = 3;  //ゲームモードの数
        private int nowGameMode;        //現在のゲームモード
        private int[] dataNum;          //データの個数

        /// <summary>
        /// データを削除する
        /// </summary>
        public void Dump()
        {
            point.Clear();
            tScore.Clear();
            gameMode.Clear();
            dataNum = new int[modeNum];
            nowGameMode = 0;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PlData()
        {
            point = new List<int>();
            tScore = new List<int>();
            gameMode = new List<int>();
            dataNum = new int[modeNum];
            nowGameMode = 0;
        }

        /// <summary>
        /// 現在のゲームモードを設定する
        /// 引数が範囲外の場合例外を返す
        /// </summary>
        /// <param name="mode">現在のモード</param>
        public void SetNowGameMode(int mode)
        {
            if (mode > modeNum || mode < 1)
            {
                throw new IndexOutOfRangeException();
            }
            this.nowGameMode = mode;
        }

        /// <summary>
        /// 現在のゲームモードを取得する
        /// </summary>
        /// <returns>ゲームモード</returns>
        public int GetNowGameMode()
        {
            return this.nowGameMode;
        }

        /// <summary>
        /// データを追加する
        /// </summary>
        /// <param name="po">ポイント</param>
        /// <param name="sco">tスコア</param>
        /// <param name="mod">モード</param>
        public void AddData(int po, int sco, int mod)
        {
            if(mod > modeNum || mod < 1)
            {
                throw new IndexOutOfRangeException();
            }
            point.Add(po);
            tScore.Add(sco);
            gameMode.Add(mod);
            dataNum[mod - 1]++;
        }

        /// <summary>
        /// データを追加する
        /// ゲームモードはnowGameModeを使用する
        /// </summary>
        /// <param name="po">ポイント</param>
        /// <param name="sco">スコア</param>
        public void AddData(int po, int sco)
        {
            point.Add(po);
            tScore.Add(sco);
            gameMode.Add(this.GetNowGameMode());
            dataNum[this.GetNowGameMode() - 1]++;
        }

        /// <summary>
        /// 現在のゲームモードでの合計ポイントを取得する
        /// </summary>
        /// <returns>合計ポイント</returns>
        public int GetTotalOfPoint()
        {
            int total = 0;
            for (int i = 0; i < point.Count; i++)
            {
                if (gameMode[i] == this.GetNowGameMode())
                {
                    total += point[i];
                }
            }
            return total;
        }

        /// <summary>
        /// 指定したゲームモードの合計ポイントを取得する
        /// </summary>
        /// <param name="mode">ゲームモード</param>
        /// <returns>合計ポイント</returns>
        public int GetTotalOfPoint(int mode)
        {
            if (mode > modeNum || mode < 1)
            {
                throw new IndexOutOfRangeException();
            }
            int total = 0;
            for (int i = 0; i < point.Count; i++)
            {
                if (gameMode[i] == mode)
                {
                    total += point[i];
                }
            }
            return total;
        }

        /// <summary>
        /// 全モードの合計ポイントを取得する
        /// </summary>
        /// <returns>合計ポイント</returns>
        public int GetTotalOfAllPoint()
        {
            int total = 0;
            for(int i = 0; i < point.Count; i++)
            {
                total += point[i];
            }
            return total;
        }

        /// <summary>
        /// 現在のゲームモードの平均ポイントを取得する
        /// </summary>
        /// <returns>平均ポイント</returns>
        public float GetAvgOfPoint()
        {
            float avg = 0.0f;
            int total;
            if (dataNum[this.GetNowGameMode() - 1] == 0)
            {
                return 0.0f;
            }
            total = this.GetTotalOfPoint();
            avg = total / dataNum[this.GetNowGameMode() - 1];
            return avg;
        }

        /// <summary>
        /// 指定したゲームモードの平均ポイントを取得する
        /// </summary>
        /// <param name="mode">ゲームモード</param>
        /// <returns>平均ポイント</returns>
        public float GetAvgOfPoint(int mode)
        {
            if (mode > modeNum || mode < 1)
            {
                throw new IndexOutOfRangeException();
            }
            float avg = 0.0f;
            int total;
            if(dataNum[mode - 1] == 0)
            {
                return 0.0f;
            }
            total = this.GetTotalOfPoint(mode);
            avg = total / dataNum[mode - 1];
            return avg;
        }

        /// <summary>
        /// 現在のゲームモードの最大ポイントを取得する
        /// </summary>
        /// <returns>最大ポイント</returns>
        public int GetMaxOfPoint()
        {
            int max = 0;
            for (int i = 0; i < point.Count; i++)
            {
                if (point[i] > max && gameMode[i] == this.GetNowGameMode())
                {
                    max = point[i];
                }
            }
            return max;
        }

        public int GetMaxOfPoint(int mode)
        {
            if (mode > modeNum || mode < 1)
            {
                throw new IndexOutOfRangeException();
            }
            int max = 0;
            for (int i = 0; i < point.Count; i++)
            {
                if (point[i] > max && gameMode[i] == mode)
                {
                    max = point[i];
                }
            }
            return max;
        }

        public int GetTotalOftScore()
        {
            int total = 0;
            for (int i = 0; i < tScore.Count; i++)
            {
                if (gameMode[i] == this.GetNowGameMode())
                {
                    total += tScore[i];
                }
            }
            return total;
        }

        public int GetTotalOftScore(int mode)
        {
            if (mode > modeNum || mode < 1)
            {
                throw new IndexOutOfRangeException();
            }
            int total = 0;
            for (int i = 0; i < tScore.Count; i++)
            {
                if (gameMode[i] == mode)
                {
                    total += tScore[i];
                }
            }
            return total;
        }

        public int GetMaxOftScore()
        {
            int max = 0;
            for (int i = 0; i < tScore.Count; i++)
            {
                if (tScore[i] > max && gameMode[i] == this.GetNowGameMode())
                {
                    max = tScore[i];
                }
            }
            return max;
        }

        public int GetMaxOftScore(int mode)
        {
            if (mode > modeNum || mode < 1)
            {
                throw new IndexOutOfRangeException();
            }
            int max = 0;
            for(int i = 0; i < tScore.Count; i++)
            {
                if (tScore[i] > max && gameMode[i] == mode)
                {
                    max = tScore[i];
                }
            }
            return max;
        }

        public float GetAvgOftScore()
        {
            float avg = 0.0f;
            int total = 0;
            if (dataNum[this.GetNowGameMode() - 1] == 0)
            {
                return 0.0f;
            }
            total = this.GetTotalOftScore();
            avg = total / dataNum[this.GetNowGameMode() - 1];
            return avg;
        }

        public float GetAvgOftScore(int mode)
        {
            if (mode > modeNum || mode < 1)
            {
                throw new IndexOutOfRangeException();
            }
            float avg = 0.0f;
            int total = 0;
            if(dataNum[mode - 1] == 0)
            {
                return 0.0f;
            }
            total = this.GetTotalOftScore(mode);
            avg = total / dataNum[mode - 1];
            return avg;
        }


        public int GetPlayTimes()
        {
            return dataNum[this.GetNowGameMode() - 1];
        }

        public int GetPlayTimes(int mode)
        {
            if (mode > modeNum || mode < 1)
            {
                throw new IndexOutOfRangeException();
            }
            return dataNum[mode - 1];
        }

        public class IndexOutOfRangeException: Exception
        {
            public IndexOutOfRangeException()
            : base("引数が範囲を超えています")
            {

            }
        }
    }
}
