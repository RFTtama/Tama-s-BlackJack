using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Tama_s_BlackJack
{
    public partial class Form1 : Form
    {
        private CardManager card;
        public const String LB = "\r\n";
        private CardProperties[ , ] Cards;
        private Random rand = new Random();
        private String datFile = "savedat.dat";
        private StreamReader sr;
        private StreamWriter sw;
        private PlData pData;
        private System.Windows.Forms.Panel[,] Panels;
        private System.Windows.Forms.Label[,] Numbers;
        private System.Windows.Forms.PictureBox[,] Pictures;
        private String tip;
        private int[] total = new int[2];
        private const int deck = 3;
        private const double shafflePer = 0.2;
        private const int maxCards = 8;
        private const int maxMental = 200;
        private const int mainPoint = 1000;
        private bool[] bjFlg;
        private bool[] overFlg;
        private float magn;
        private int mental;
        private int point;
        private int oldPoint;
        private int totalDeal;
        private int[] cardIndex = new int[2];
        private CardProperties hiddenCard;
        private int mentalSize;
        private readonly int[] mentalXY;
        private int mentalBef;
        private System.Windows.Forms.PictureBox[] mentalLabel;

        public Form1()
        {
            InitializeComponent();
            pData = new PlData();
            try
            {
                sr = new StreamReader(datFile);
            }
            catch (FileNotFoundException)
            {
                sw = new StreamWriter(datFile, true);
                sw.Close();
                sr = new StreamReader(datFile);
            }
            while (true)
            {
                String str;
                str = sr.ReadLine();
                if(str == null)
                {
                    break;
                }
                String[] arr = str.Split(',');
                pData.AddPoint(int.Parse(arr[0]));
                pData.AddtScore(int.Parse(arr[1]));
            }
            sr.Close();
            SetStats();
            ButtonLock();
            this.mental = maxMental;
            this.point = 0;
            this.totalDeal = 0;
            this.magn = 1.0f;
            this.mentalXY = new int[2] { MentalPicture.Left, MentalPicture.Top };
            this.mentalBef = 0;
            mentalPosXY = new int[2] { label17.Left, label17.Top };
            mentalPicPosXY = new int[2, 2] { { MentalPicture.Left, MentalPicture.Top },
            { RedMentalBar.Left, RedMentalBar.Top } };
            card = new CardManager();
            Cards = new CardProperties[2, maxCards];
            hiddenCard = new CardProperties();
            bjFlg = new bool[2] { false, false };
            overFlg = new bool[2] { false, false };
            mentalLabel = new System.Windows.Forms.PictureBox[2] { MentalPicture, RedMentalBar };
            Panels = new System.Windows.Forms.Panel[2, maxCards]
            {
                { panel1, panel2, panel3, panel4, panel5, panel6, panel7, panel8 },
                { panel9, panel10, panel11, panel12, panel13, panel14, panel15, panel16}
            };
            Numbers = new System.Windows.Forms.Label[2, maxCards]
            {
                { label1, label2, label3, label4, label5, label6, label7, label8 },
                { label9, label10, label11, label12, label13, label14, label15, label16 }
            };
            Pictures = new System.Windows.Forms.PictureBox[2, maxCards]
            {
                { pictureBox1, pictureBox2, pictureBox3, pictureBox4,pictureBox5, pictureBox6, pictureBox7, pictureBox8 },
                { pictureBox9, pictureBox10, pictureBox11, pictureBox12, pictureBox13, pictureBox14, pictureBox15, pictureBox16 }
            };
            cardIndex[0] = 0;
            cardIndex[1] = 0;
            mentalSize = MentalPicture.Width;

            try
            {
                card.DumpDeck();
                card.SetDeckNum(deck);
                card.CreateDeck();
                card.ShaffleDeck();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void ClearCards()
        {
            for(int i = 0; i < maxCards; i++)
            {
                Cards[0, i] = new CardProperties();
                Cards[1, i] = new CardProperties();
                Numbers[0, i].Text = 0 + "";
                Numbers[1, i].Text = 0 + "";
                Panels[0, i].Visible = false;
                Panels[1, i].Visible = false;
                Pictures[0, i].Image = Properties.Resources.card;
                Pictures[1, i].Image = Properties.Resources.card;
                Numbers[0, i].Visible = true;
                Numbers[1, i].Visible = true;
                if (i < 2)
                {
                    this.cardIndex[i] = 0;
                    this.total[i] = 0;
                    bjFlg[i] = false;
                    overFlg[i] = false;
                }
            }
            PlayerTotalLabel.ForeColor = Color.Black;
            DealerTotalLabel.ForeColor = Color.Black;
            magn = 1.0f;
        }

        private void SetCard(int tag, CardProperties cp, bool wasHidden)
        {
            if (wasHidden)
            {
                cardIndex[tag]--;
                Pictures[0, 1].Image = Properties.Resources.card;
                Numbers[0, 1].Visible = true;
                Panels[0, 1].Visible = true;
            }
            Cards[tag, cardIndex[tag]] = cp;
            if (cp.number == 1)
            {
                Numbers[tag, cardIndex[tag]].Visible = false;
                if (rand.Next(3) == 0)
                {
                    Pictures[tag, cardIndex[tag]].Image = Properties.Resources.ace_nyan;
                }
                else
                {
                    Pictures[tag, cardIndex[tag]].Image = Properties.Resources.tamaAce;
                }
            }
            else if(cp.number == 11)
            {
                Numbers[tag, cardIndex[tag]].Visible = false;
                Pictures[tag, cardIndex[tag]].Image = Properties.Resources.jack;
            }
            else if(cp.number == 12)
            {
                Numbers[tag, cardIndex[tag]].Visible = false;
                Pictures[tag, cardIndex[tag]].Image = Properties.Resources.queen;
            }
            else if(cp.number == 13)
            {
                Numbers[tag, cardIndex[tag]].Visible = false;
                Pictures[tag, cardIndex[tag]].Image = Properties.Resources.king;
            }
            else if(cp.number == 10 && rand.Next(9) == 0)
            {
                Numbers[tag, cardIndex[tag]].Visible = false;
                Pictures[tag, cardIndex[tag]].Image = Properties.Resources.tenrea;
            }
            else
            {
                Numbers[tag, cardIndex[tag]].Text = cp.number + "";
            }
            Panels[tag, cardIndex[tag]].Visible = true;
            this.total[tag] = 0;
            for (int i = 0; i <= cardIndex[tag]; i++)
            {
                if (Cards[tag, i].number == 1)
                {
                    this.total[tag] += 11;
                }
                else if (Cards[tag, i].number > 10)
                {
                    this.total[tag] += 10;
                }
                else
                {
                    this.total[tag] += Cards[tag, i].number;
                }
            }
            for(int i = 0; i <= cardIndex[tag]; i++)
            {
                if(Cards[tag, i].number == 1 && total[tag] > 21)
                {
                    total[tag] -= 10;
                }
            }
            switch (tag)
            {
                case 0:
                    if (total[0] > 21)
                    {
                        DealerTotalLabel.Text = "BUST";
                    }
                    else
                    {
                        DealerTotalLabel.Text = this.total[0] + "";
                    }
                    if (total[0] == 21 && cardIndex[tag] < 2)
                    {
                        DealerTotalLabel.Text = "BJ";
                        bjFlg[0] = true;
                        DealerTotalLabel.ForeColor = Color.Red;
                        if(Pictures[0, 0].Image == Properties.Resources.tenrea)
                        {
                            ColorTimer.Enabled = true;
                        }
                    }
                    break;

                case 1:
                    if (total[1] > 21)
                    {
                        PlayerTotalLabel.Text = "BUST";
                    }
                    else
                    {
                        PlayerTotalLabel.Text = this.total[1] + "";
                    }
                    if (total[1] == 21 && cardIndex[tag] < 2)
                    {
                        PlayerTotalLabel.Text = "BJ";
                        bjFlg[1] = true;
                        PlayerTotalLabel.ForeColor = Color.Red;
                        if (Pictures[1, 0].Image == Properties.Resources.tenrea)
                        {
                            ColorTimer.Enabled = true;
                        }
                        SetCard(0, hiddenCard, true);
                        BattleCards();
                    }
                    break;
            }
            cardIndex[tag]++;
            if(cardIndex[tag] > 8 && total[tag] < 21)
            {
                overFlg[tag] = true;
            }
        }

        private void SetCardTip()
        {
            int cardPer = (int)(card.GetRemainingCardsPer() * 100.0);
            double per;
            int remainingNum;
            this.tip = "残りカード" + cardPer + "%(" + card.GetRemainingDeckCards() + "枚)" + LB + "残り" + (int)(shafflePer * 100.0) + "%でシャッフルが入ります" + LB;
            this.tip += LB + "ディール前" + LB + "~~~~~~~~~~~~~~~~~~";
            for (int i = 0; i < 9; i++)
            {
                per = card.GetCardDrawPer(i) * 100.0;
                remainingNum = card.GetRemainingCard(i);
                this.tip += LB + (i + 1) + "を引く確率 " + per.ToString("F2") + "%(" + remainingNum + "枚)";
            }
            per = 0;
            remainingNum = 0;
            for (int i = 9; i < 13; i++)
            {
                per += card.GetCardDrawPer(i);
                remainingNum += card.GetRemainingCard(i);
            }
            per *= 100.0;
            this.tip += LB + "10を引く確率 " + per.ToString("F2") + "%(" + remainingNum + "枚)";
            this.tip += LB + "~~~~~~~~~~~~~~~~~~";
            toolTip1.SetToolTip(DeckPicture, this.tip);
        }

        private void SetHiddenCard(CardProperties cp)
        {
            hiddenCard = cp;
            Numbers[0, 1].Visible = false;
            Panels[0, 1].Visible = true;
            Pictures[0, 1].Image = Properties.Resources.card_ura;
            DealerTotalLabel.Text += " ?";
            cardIndex[0]++;
        }

        private void HitDealerCard()
        {
            while(total[0] < 17)
            {
                SetCard(0, card.DrawCard(), false);
            }
            BattleCards();
        }

        private void MentalCheck()
        {
            float sizeMagn = (float)this.mental / maxMental;
            sizeMagn *= mentalSize;
            MentalPicture.Width = (int)sizeMagn;
            if(this.mentalBef > this.mental)MentalTimer.Enabled = true;
            float me = maxMental;
            float avgDmg = me / this.totalDeal;
            float Pt = (float)this.point / 100.0f;
            float tScore = (20.0f - avgDmg) * Pt;
            if (tScore < 0) tScore = 0;
            TscoreLabel.Text = "T-Score: " + (int)tScore;
            if (this.mental <= 0)
            {
                ButtonLock();
                DealButton.Enabled = false;
                InformationLabel.Text = "";
                InformationLabel.Text = "ゲームを続けるメンタルが残っていません。 あなたのポイントは" + this.point + "、T-Scoreは" + (int)tScore + "です";
                try
                {
                    StreamWriter sw = new StreamWriter("History.txt", true);
                    sw.WriteLine("Coins: " + this.point + " T-Score " + (int)tScore);
                    sw.Close();
                    sw.Dispose();
                    this.sw = new StreamWriter(datFile, true);
                    this.sw.WriteLine(this.point + "," + (int)tScore);
                    this.sw.Close();
                    this.sw.Dispose();
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                pData.AddPoint(this.point);
                pData.AddtScore((int)tScore);
                SetStats();
            }
        }

        private void SetStats()
        {
            AvgCoinsLabel.Text = pData.GetMaxOfPoint() + "";
            int avg = (int)pData.GetAvgOftScore();
            AvgtScoreLabel.Text = avg.ToString();
            KatagakiLabel.Text = pData.GetEvalOfPoint() + "レベル";
            TotalCoinsLabel.Text = "Total: " + pData.GetTotalOfPoint() + "";
            KatagakiLabel2.Text = pData.GetEvalOftScore() + "並み";
            MaxtScoreLabel.Text = "Max: " + pData.GetMaxOftScore();
            PlayTimesLabel.Text = "Play times: " + pData.GetPlayTimes();
            int po = pData.GetTotalOfPoint();
            if(po < 100000)
            {
                PointEvalPicture.Image = Properties.Resources.coin;
            }else if(po < 500000)
            {
                PointEvalPicture.Image = Properties.Resources.coins;
            }else if(po < 1000000)
            {
                PointEvalPicture.Image = Properties.Resources.bill;
            }else if(po < 10000000)
            {
                PointEvalPicture.Image = Properties.Resources.bills;
            }
            else
            {
                PointEvalPicture.Image = Properties.Resources.kinko;
            }
        }

        private void BattleCards()
        {
            ButtonLock();
            this.mentalBef = this.mental;
            if (overFlg[0] && overFlg[1])
            {
                InformationLabel.Text = "";
                InformationLabel.Text = "お互いがミスフォーチュンです";
            }
            else if (overFlg[0])
            {
                InformationLabel.Text = "";
                InformationLabel.Text = "ディーラーのミスフォーチュンです";
                this.mental -= (int)(10 * magn);
            }
            else if (overFlg[1])
            {
                InformationLabel.Text = "";
                InformationLabel.Text = "プレイヤーのミスフォーチュンです";
                PlusPoint((int)(mainPoint * 2.0));
            }
            else if(total[1] > 21)
            {
                InformationLabel.Text = "";
                InformationLabel.Text = "プレイヤーのバストです";
                this.mental -= (int)(10 * magn);
            }
            else if(total[0] > 21)
            {
                InformationLabel.Text = "";
                InformationLabel.Text = "ディーラーのバストです";
                PlusPoint(mainPoint);
            }
            else if(bjFlg[0] && bjFlg[1])
            {
                InformationLabel.Text = "";
                InformationLabel.Text = "ブラックジャックのプッシュです";
            }else if (bjFlg[0])
            {
                InformationLabel.Text = "";
                InformationLabel.Text = "ディーラーのブラックジャックです";
                this.mental -= (int)(10 * magn);
            }
            else if (bjFlg[1])
            {
                InformationLabel.Text = "";
                InformationLabel.Text = "プレイヤーのブラックジャックです";
                PlusPoint((int)(mainPoint * 1.5));
            }
            else if(total[0] > total[1])
            {
                InformationLabel.Text = "";
                InformationLabel.Text = "ディーラーの勝ちです";
                this.mental -= (int)(10 * magn);
            }else if(total[1] > total[0])
            {
                InformationLabel.Text = "";
                InformationLabel.Text = "プレイヤーの勝ちです";
                PlusPoint(mainPoint);
            }
            else
            {
                InformationLabel.Text = "";
                InformationLabel.Text = "プッシュです";
            }
            MentalCheck();
        }

        private void PlusPoint(int value)
        {
            this.oldPoint = this.point;
            this.point += (int)(value * magn);
            this.PointLabel.ForeColor = Color.Red;
            PointTimer.Enabled = true;
        }

        private void ButtonLock()
        {
            HitPicture.Visible = false;
            StandPicture.Visible = false;
            DoublePicture.Visible = false;
            InsurancePicture.Visible = false;
            SurrenderPicture.Visible = false;
            DealButton.Enabled = true;
        }

        private void ButtonUnlock()
        {
            HitPicture.Visible = true;
            StandPicture.Visible = true;
            DoublePicture.Visible = true;
            InsurancePicture.Visible = true;
            SurrenderPicture.Visible = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ArrowPicture.Visible = false;
            ExplainPanel.Visible = false;
            ArrowTimer.Enabled = false;
            ColorTimer.Enabled = false;
            ButtonUnlock();
            this.totalDeal++;
            DealButton.Enabled = false;
            InsurancePicture.Visible = false;
            if (card.GetRemainingCardsPer() < shafflePer)
            {
                try
                {
                    card.DumpDeck();
                    card.SetDeckNum(deck);
                    card.CreateDeck();
                    card.ShaffleDeck();
                    InformationLabel.Text = "";
                    InformationLabel.Text = "デッキがシャッフルされました";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            ClearCards();
            SetCardTip();
            SetCard(0, card.DrawCard(), false);
            SetHiddenCard(card.DrawCard());
            SetCard(1, card.DrawCard(), false);
            SetCard(1, card.DrawCard(), false);
            SetBustPer();
            if(Cards[0, 0].number == 1 && !bjFlg[1])
            {
                InsurancePicture.Visible = true;
            }
        }

        private void HitPicture_Click(object sender, EventArgs e)
        {
            DoublePicture.Visible = false;
            SurrenderPicture.Visible = false;
            InsurancePicture.Visible = false;
            SetCard(1, card.DrawCard(), false);
            if (total[1] >= 21)
            {
                SetCard(0, hiddenCard, true);
                HitDealerCard();
            }
            SetBustPer();
        }

        private void SetBustPer()
        {
            double per = 0.0;
            String tip;
            int remainingCard = 0;
            for (int i = 0; i < 9; i++)
            {
                if (total[1] + (i + 1) > 21)
                {
                    per += card.GetCardDrawPer(i) * 100.0;
                    remainingCard += card.GetRemainingCard(i);
                }
            }
            for (int i = 9; i < 13; i++)
            {
                if (total[1] + 10 > 21)
                {
                    per += card.GetCardDrawPer(i) * 100.0;
                    remainingCard += card.GetRemainingCard(i);
                }
            }
            per = (int)per - (int)per % 5;
            tip = LB + LB + "バスト確率: 約" + per.ToString("F0") + "%";
            toolTip1.SetToolTip(DeckPicture, this.tip + tip);
        }

        private void StandPicture_Click(object sender, EventArgs e)
        {
            SetCard(0, hiddenCard, true);
            HitDealerCard();
        }

        private void DoublePicture_Click(object sender, EventArgs e)
        {
            this.magn = 2.0f;
            SetCard(1, card.DrawCard(), false);
            SetCard(0, hiddenCard, true);
            HitDealerCard();
        }

        private void InsurancePicture_Click(object sender, EventArgs e)
        {
            if(hiddenCard.number >= 10)
            {
                ButtonLock();
                SetCard(0, hiddenCard, true);
                InformationLabel.Text = "";
                InformationLabel.Text = "インシュアランス成功";
            }
            else
            {
                InsurancePicture.Visible = false;
                this.mental -= 5;
                InformationLabel.Text = "";
                InformationLabel.Text = "インシュアランス失敗";
                MentalCheck();
            }
        }

        private void SurrenderPicture_Click(object sender, EventArgs e)
        {
            ButtonLock();
            SetCard(0, hiddenCard, true);
            if (total[0] > total[1])
            {
                InformationLabel.Text = "";
                InformationLabel.Text = "サレンダー成功";
                this.mental -= 5;
            }
            else
            {
                InformationLabel.Text = "";
                InformationLabel.Text = "サレンダー失敗";
                this.mental -= 10;
            }
            float sizeMagn = (float)this.mental / maxMental;
            sizeMagn *= mentalSize;
            MentalPicture.Width = (int)sizeMagn;
            MentalCheck();
        }

        private void InformationLabel_TextChanged(object sender, EventArgs e)
        {
            if (InformationLabel.Text != "")
            {
                InformationLabel.Visible = true;
                InformationLabel.Left = this.Width;
                InformationTimer.Enabled = true;
            }
        }

        private void InformationTimer_Tick(object sender, EventArgs e)
        {
            InformationLabel.Left -= 2;
            if(InformationLabel.Left + InformationLabel.Width <= 0)
            {
                InformationLabel.Text = "";
                InformationLabel.Visible = false;
                InformationTimer.Enabled = false;
            }
        }

        

        private void timer1_Tick(object sender, EventArgs e)
        {
            if(oldPoint < this.point)
            {
                this.oldPoint += 10;
            }
            else
            {
                this.oldPoint = this.point;
                this.PointLabel.ForeColor = Color.Black;
                PointTimer.Enabled = false;
            }
            PointLabel.Text = this.oldPoint + "";
        }

        private int mentalShakeTimes = 0;

        private void MentalTimer_Tick(object sender, EventArgs e)
        {
            MentalPicture.Top += rand.Next(-2, 3);
            MentalPicture.Left += rand.Next(-2, 3);
            if(mentalShakeTimes >= 10)
            {
                mentalShakeTimes = 0;
                MentalPicture.Left = mentalXY[0];
                MentalPicture.Top = mentalXY[1];
                MentalTimer.Enabled = false;
            }
            mentalShakeTimes++;
        }

        private int colorTag = 0;

        private void ColorTimer_Tick(object sender, EventArgs e)
        {
            Color[] colorTypes = new Color[] { Color.Red, Color.Orange, Color.Yellow, Color.Green, Color.Blue, Color.Purple, Color.Black };
            TscoreLabel.ForeColor = colorTypes[colorTag % colorTypes.Length];
            colorTag++;
        }

        private int arrowTime = 0;

        private void ArrowTimer_Tick(object sender, EventArgs e)
        {
            ArrowPicture.Top += (int)(Math.Sin(arrowTime) * 5.0);
            arrowTime++;
        }

        int[] mentalPosXY;
        int[,] mentalPicPosXY;

        private void MentalHealthTimer_Tick(object sender, EventArgs e)
        {
            float mentalPer = this.mental / 100.0f;
            String mentalInfo;
            int per;
            if(mentalPer >= 0.4f)
            {
                mentalInfo = "正常";
                per = 0;
            }
            else if(mentalPer >= 0.2f)
            {
                mentalInfo = "疲弊";
                per = 10;
            }
            else
            {
                int tiredPer = rand.Next(0, 100);
                if (tiredPer < 20)
                {
                    mentalInfo = "＊＊";
                }
                else if(tiredPer < 40)
                {
                    mentalInfo = "****";
                }
                else if(tiredPer < 60)
                {
                    mentalInfo = "弱衰";
                }
                else
                {
                    mentalInfo = "衰弱";
                }
                per = 40;
            }
            if (rand.Next(0, 100) < per)
            {
                MentalPicture.Top += rand.Next(-1, 2);
                RedMentalBar.Top += rand.Next(-1, 2);
                label17.Top += rand.Next(-1, 2);
            }
            else
            {
                MentalPicture.Top = mentalPicPosXY[0, 1];
                RedMentalBar.Top = mentalPicPosXY[1, 1];
                label17.Top = mentalPosXY[1];
            }
            if (rand.Next(0, 100) < per)
            {
                MentalPicture.Left += rand.Next(-1, 2);
                RedMentalBar.Left += rand.Next(-1, 2);
                label17.Left += rand.Next(-1, 2);
            }
            else
            {
                MentalPicture.Left = mentalPicPosXY[0, 0];
                RedMentalBar.Left = mentalPicPosXY[1, 0];
                label17.Left = mentalPosXY[0];
            }
            for (int i = 0; i < 2; i++)
            {
                toolTip1.SetToolTip(mentalLabel[i], mentalInfo);
            }
        }

        private void statPicture_Click(object sender, EventArgs e)
        {
            StatPanel.Visible = !StatPanel.Visible;
        }
    }
}
