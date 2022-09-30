using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Tama_s_BlackJack
{
    public partial class Form1 : Form
    {
        private CardManager card;                               //カードデッキの操作クラス
        public const String LB = "\r\n";                        //LineBreak
        private CardProperties[ , ] Cards;                      //場のカード集
        private Random rand = new Random();                     //ランダムクラス
        private Encryption encryption = new Encryption();       //暗号化クラス
        private RateManager rateMan;                            //レート管理
        private PlData pData;                                   //pDataクラス
        private System.Windows.Forms.Panel[,] Panels;           //パネル管理用配列
        private System.Windows.Forms.Label[,] Numbers;          //数字管理用配列
        private System.Windows.Forms.PictureBox[,] Pictures;    //画像管理用配列
        private String tip;                                     //チップ情報
        private List<String> sData;                             //セーブデータ保持リスト
        private int[] total;                                    //カードの合計値
        private int playerMinTotal;                             //プレイヤーカードの最低値
        private double bustPer;                                  //プレイヤーのバスト確率
        private int deck;                                       //デッキの数
        private const double shafflePer = 0.2;                  //何%になったらシャッフルするか
        private const int maxCards = 8;                         //場に出せるカードの最大数
        private int maxMental = 200;                            //最大メンタル
        private int mainPoint;                                  //配当
        private bool[] bjFlg;                                   //ブラックジャックかどうか
        private bool[] overFlg;                                 //カードが出せる量を超えているかどうか
        private float magn;                                     //メンタル賭け倍率
        private float pointMagn;                                //ポイント賭け倍率
        private float defaultMagn;                              //初期倍率
        private int mental;                                     //メンタル
        private int point;                                      //ポイント
        private int oldPoint;                                   //1ディール前のポイント
        private int totalDeal;                                  //合計ディール数
        private int[] cardIndex;                                //次のカード置き場所
        private CardProperties hiddenCard;                      //隠されたカード
        private int mentalBef;                                  //1ディール前のメンタル
        private int helpDefaultTop;                             //ヘルプパネルの初期位置
        private bool reaMode;                                   //レアモードか否か
        private int additionalScore = 0;                        //追加t-score
        private int streak = 0;                                 //勢い

        /// <summary>
        /// Formコンストラクタ
        /// </summary>
        public Form1()
        {
            InitializeComponent();
            this.Width = 775;
            this.Height = 564;
            encryption.fileName = "encryptedData.dat";
            HelpParentPanel.Top = 37;
            HelpParentPanel.Left = 49;
            StatPanel.Top = 66;
            StatPanel.Left = 175;
            MemberPanel.Top = 168;
            MemberPanel.Left = 171;
            rateMan = new RateManager();
            pData = new PlData();
            pData.SetNowGameMode(1);
            sData = new List<String>();
            try
            {
                sData = encryption.Decrypt();
            }
            catch(FileNotFoundException)
            {

            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, ex.GetType() + "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            for(int i = 0; i < sData.Count; i++)
            {
                String[] arr = sData[i].Split(',');
                pData.AddData(int.Parse(arr[0]), int.Parse(arr[1]), int.Parse(arr[2]));
            }
            SetStats();
            ButtonLock();
            this.reaMode = false;
            this.deck = 3;
            this.mental = maxMental;
            this.point = 0;
            this.totalDeal = 1;
            this.magn = 1.0f;
            this.pointMagn = 1.0f;
            this.defaultMagn = 1.0f;
            this.mainPoint = 500;
            this.mentalBef = 0;
            this.helpDefaultTop = HelpPanel.Top;
            this.card = new CardManager();
            this.Cards = new CardProperties[2, maxCards];
            this.hiddenCard = new CardProperties();
            this.bjFlg = new bool[2] { false, false };
            this.overFlg = new bool[2] { false, false };
            this.total = new int[2];
            this.cardIndex = new int[2] { 0, 0 };
            this.Panels = new Panel[2, maxCards]
            {
                { panel1, panel2, panel3, panel4, panel5, panel6, panel7, panel8 },
                { panel9, panel10, panel11, panel12, panel13, panel14, panel15, panel16}
            };
            this.Numbers = new Label[2, maxCards]
            {
                { label1, label2, label3, label4, label5, label6, label7, label8 },
                { label9, label10, label11, label12, label13, label14, label15, label16 }
            };
            this.Pictures = new PictureBox[2, maxCards]
            {
                { pictureBox1, pictureBox2, pictureBox3, pictureBox4,pictureBox5, pictureBox6, pictureBox7, pictureBox8 },
                { pictureBox9, pictureBox10, pictureBox11, pictureBox12, pictureBox13, pictureBox14, pictureBox15, pictureBox16 }
            };
            int reaPer = 20;
            if(rand.Next(0, reaPer) == 0)
            {
                this.reaMode = true;
                DeckPicture.Image = Properties.Resources.card_ura_handmade;
            }
            SetMemberData();
            MentalLabel.Text = this.mental + "";
            ArrowTimer.Enabled = true;
        }

        /// <summary>
        /// 場のカードをクリアする
        /// </summary>
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
            this.magn = this.defaultMagn;
            this.pointMagn = 1.0f;
        }

        /// <summary>
        /// カードをセットする
        /// </summary>
        /// <param name="tag">タグ: プレイヤーかディーラーか</param>
        /// <param name="cp">カード情報</param>
        /// <param name="wasHidden">隠されていたカードかどうか</param>
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
            if (reaMode)
            {
                if (cp.number == 1)
                {
                    Numbers[tag, cardIndex[tag]].Visible = false;
                    if (rand.Next(9) == 0)
                    {
                        Pictures[tag, cardIndex[tag]].Image = Properties.Resources.ace_nyan;
                    }
                    else
                    {
                        Pictures[tag, cardIndex[tag]].Image = Properties.Resources.tamaAce_handmade;
                    }
                }
                else if (cp.number == 11)
                {
                    Numbers[tag, cardIndex[tag]].Visible = false;
                    Pictures[tag, cardIndex[tag]].Image = Properties.Resources.jack_handmade;
                }
                else if (cp.number == 12)
                {
                    Numbers[tag, cardIndex[tag]].Visible = false;
                    Pictures[tag, cardIndex[tag]].Image = Properties.Resources.queen_handmade;
                }
                else if (cp.number == 13)
                {
                    Numbers[tag, cardIndex[tag]].Visible = false;
                    Pictures[tag, cardIndex[tag]].Image = Properties.Resources.king_handmade;
                }
                else if (cp.number == 10 && rand.Next(7) == 0)
                {
                    Numbers[tag, cardIndex[tag]].Visible = false;
                    Pictures[tag, cardIndex[tag]].Image = Properties.Resources.tenrea;
                }
                else
                {
                    Numbers[tag, cardIndex[tag]].Text = cp.number + "";
                }
            }
            else
            {
                if (cp.number == 1)
                {
                    Numbers[tag, cardIndex[tag]].Visible = false;
                    if (rand.Next(9) == 0)
                    {
                        Pictures[tag, cardIndex[tag]].Image = Properties.Resources.ace_nyan;
                    }
                    else
                    {
                        Pictures[tag, cardIndex[tag]].Image = Properties.Resources.tamaAce;
                    }
                }
                else if (cp.number == 11)
                {
                    Numbers[tag, cardIndex[tag]].Visible = false;
                    Pictures[tag, cardIndex[tag]].Image = Properties.Resources.jack;
                }
                else if (cp.number == 12)
                {
                    Numbers[tag, cardIndex[tag]].Visible = false;
                    Pictures[tag, cardIndex[tag]].Image = Properties.Resources.queen;
                }
                else if (cp.number == 13)
                {
                    Numbers[tag, cardIndex[tag]].Visible = false;
                    Pictures[tag, cardIndex[tag]].Image = Properties.Resources.king;
                }
                else if (cp.number == 10 && rand.Next(7) == 0)
                {
                    Numbers[tag, cardIndex[tag]].Visible = false;
                    Pictures[tag, cardIndex[tag]].Image = Properties.Resources.tenrea;
                }
                else
                {
                    this.Invoke(new Action(() =>
                    {
                        Numbers[tag, cardIndex[tag]].Text = cp.number + "";
                    }));
                }
            }
            this.Invoke(new Action(() =>
            {
                Panels[tag, cardIndex[tag]].Visible = true;
            }));
            this.total[tag] = 0;
            this.playerMinTotal = 0;
            for (int i = 0; i <= cardIndex[tag]; i++)
            {
                if (Cards[tag, i].number == 1)
                {
                    this.total[tag] += 11;
                    this.playerMinTotal += 1;
                }
                else if (Cards[tag, i].number > 10)
                {
                    this.total[tag] += 10;
                    this.playerMinTotal += 10;
                }
                else
                {
                    this.total[tag] += Cards[tag, i].number;
                    this.playerMinTotal += Cards[tag, i].number;
                }
            }
            for (int i = 0; i <= cardIndex[tag]; i++)
            {
                if (Cards[tag, i].number == 1 && total[tag] > 21)
                {
                    total[tag] -= 10;
                }
            }
            switch (tag)
            {
                case 0:
                    if (total[0] > 21)
                    {
                        this.Invoke(new Action(() =>
                        {
                            DealerTotalLabel.Text = "BUST";
                        }));
                    }
                    else
                    {
                        this.Invoke(new Action(() =>
                        {
                            DealerTotalLabel.Text = this.total[0] + "";
                        }));
                    }
                    if (total[0] == 21 && cardIndex[tag] < 2)
                    {
                        this.Invoke(new Action(() =>
                        {
                            DealerTotalLabel.Text = "BJ";
                        }));
                        bjFlg[0] = true;
                        DealerTotalLabel.ForeColor = Color.Red;
                        if (Pictures[0, 0].Image == Properties.Resources.tenrea)
                        {
                            ColorTimer.Enabled = true;
                        }
                    }
                    break;

                case 1:
                    if (total[1] > 21)
                    {
                        this.Invoke(new Action(() =>
                        {
                            PlayerTotalLabel.Text = "BUST";
                        }));
                    }
                    else
                    {
                        this.Invoke(new Action(() =>
                        {
                            PlayerTotalLabel.Text = this.total[1] + "";
                        }));
                    }
                    if (total[1] == 21 && cardIndex[tag] < 2)
                    {
                        this.Invoke(new Action(() =>
                        {
                            PlayerTotalLabel.Text = "BJ";
                        }));
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
            if (cardIndex[tag] > 8 && total[tag] < 21)
            {
                overFlg[tag] = true;
            }
        }

        /// <summary>
        /// デッキにカーソルを置いた時に表示される情報を表示する
        /// </summary>
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

        /// <summary>
        /// 隠されたカードを設定する
        /// </summary>
        /// <param name="cp">カード情報</param>
        private void SetHiddenCard(CardProperties cp)
        {
            hiddenCard = cp;
            Numbers[0, 1].Visible = false;
            Panels[0, 1].Visible = true;
            if (reaMode)
            {
                Pictures[0, 1].Image = Properties.Resources.card_ura_handmade;
            }
            else
            {
                Pictures[0, 1].Image = Properties.Resources.card_ura;
            }
            DealerTotalLabel.Text += " ?";
            cardIndex[0]++;
        }

        /// <summary>
        /// ディーラーのカードヒットを行う
        /// </summary>
        private void HitDealerCard()
        {
            while(total[0] < 17)
            {
                SetCard(0, card.DrawCard(), false);
            }
            BattleCards();
        }

        /// <summary>
        /// メンタルをチェックする
        /// </summary>
        private void MentalCheck()
        {
            MentalLabel.Text = this.mental + "";
            float me = maxMental;
            float avgDmg = me / this.totalDeal;
            float Pt = (float)this.point / 100.0f;
            float dmgTolerance = 20.0f * this.defaultMagn;
            float tScore = (dmgTolerance - avgDmg) * Pt;
            if (tScore < 0) tScore = 0;
            tScore += this.additionalScore;
            TscoreLabel.Text = "T-Score: " + (int)tScore;
            if (this.mental <= 0)
            {
                ButtonLock();
                DealButton.Enabled = false;
                InformationLabel.Text = "";
                InformationLabel.Text = "最終ポイント" + this.point + "、T-Score" + (int)tScore + " +" + additionalScore;
                try
                {
                    using (StreamWriter sw = new StreamWriter("History.txt", true))
                    {
                        sw.WriteLine("Coins: " + this.point + " T-Score " + (int)tScore);
                    }
                    sData.Add(this.point + "," + (int)tScore + "," + pData.GetNowGameMode());
                    String fullData = String.Empty;
                    for(int i = 0; i < sData.Count; i++)
                    {
                        fullData += sData[i] + LB;
                    }
                    encryption.Encrypt(fullData);
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                pData.AddData(this.point, (int)tScore);
                SetStats();
                if (pData.GetNowGameMode() == 3)
                {
                    rateMan.CalcRate(tScore);
                    SetMemberData();
                }
            }
        }

        /// <summary>
        /// 戦績を設定する
        /// </summary>
        private void SetStats()
        {
            String[] modeName = new String[] { "Standard", "Casual", "T's BJ Member" };
            ModeLabel.Text = modeName[pData.GetNowGameMode() - 1];
            AvgCoinsLabel.Text = pData.GetMaxOfPoint() + "";
            int avg = (int)pData.GetAvgOftScore();
            AvgtScoreLabel.Text = avg.ToString();
            KatagakiLabel.Text = pData.GetEvalOfPoint() + "レベル";
            TotalCoinsLabel.Text = "Total: " + pData.GetTotalOfAllPoint() + "";
            KatagakiLabel2.Text = pData.GetEvalOftScore() + "並み";
            MaxtScoreLabel.Text = "Max: " + pData.GetMaxOftScore();
            PlayTimesLabel.Text = "Play times: " + pData.GetPlayTimes();
            int po = pData.GetTotalOfAllPoint();
            if(po < 50000)
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

        /// <summary>
        /// カードを対戦させる
        /// </summary>
        private void BattleCards()
        {
            ButtonLock();
            this.totalDeal++;
            this.mentalBef = this.mental;
            if (overFlg[0] && overFlg[1])
            {
                InformationLabel.Text = "";
                InformationLabel.Text = "お互いがミスフォーチュンです";
               this. streak = 0;
            }
            else if (overFlg[0])
            {
                Slash(true);
                InformationLabel.Text = "";
                InformationLabel.Text = "ディーラーのミスフォーチュンです";
                this.mental -= (int)(10 * magn);
                SetPenarty();
                this.streak = 0;
            }
            else if (overFlg[1])
            {
                Slash(false);
                InformationLabel.Text = "";
                InformationLabel.Text = "プレイヤーのミスフォーチュンです";
                SetAdditionalScore(20, "Super Luck");
                PlusPoint((int)(mainPoint * 2.0));
                if(pointMagn == 2.0)
                {
                    SetAdditionalScore(5, "Smart");
                }
                if(streak > 0)
                {
                    SetAdditionalScore(this.streak * 3, "Streak bonus");
                }
                this.streak++;
            }
            else if(total[1] > 21)
            {
                Slash(true);
                InformationLabel.Text = "";
                InformationLabel.Text = "プレイヤーのバストです";
                this.mental -= (int)(10 * magn);
                SetPenarty();
                this.streak = 0;
            }
            else if(total[0] > 21)
            {
                Slash(false);
                InformationLabel.Text = "";
                InformationLabel.Text = "ディーラーのバストです";
                PlusPoint(mainPoint);
                if (pointMagn == 2.0)
                {
                    SetAdditionalScore(5, "Smart");
                }
                if (streak > 0)
                {
                    SetAdditionalScore(this.streak * 3, "Streak bonus");
                }
                this.streak++;
            }
            else if(bjFlg[0] && bjFlg[1])
            {
                InformationLabel.Text = "";
                InformationLabel.Text = "ブラックジャックのプッシュです";
                this.streak = 0;
            }else if (bjFlg[0])
            {
                Slash(true);
                InformationLabel.Text = "";
                InformationLabel.Text = "ディーラーのブラックジャックです";
                this.mental -= (int)(10 * magn);
                SetPenarty();
                this.streak = 0;
            }
            else if (bjFlg[1])
            {
                Slash(false);
                InformationLabel.Text = "";
                InformationLabel.Text = "プレイヤーのブラックジャックです";
                PlusPoint((int)(mainPoint * 1.50));
                if (streak > 0)
                {
                    SetAdditionalScore(this.streak * 3, "Streak bonus");
                }
                this.streak++;
            }
            else if(total[0] > total[1])
            {
                Slash(true);
                InformationLabel.Text = "";
                InformationLabel.Text = "ディーラーの勝ちです";
                this.mental -= (int)(10 * magn);
                SetPenarty();
                this.streak = 0;
            }
            else if(total[1] > total[0])
            {
                Slash(false);
                InformationLabel.Text = "";
                InformationLabel.Text = "プレイヤーの勝ちです";
                PlusPoint(mainPoint);
                if (pointMagn == 2.0)
                {
                    SetAdditionalScore(5, "Smart");
                }
                if (streak > 0)
                {
                    SetAdditionalScore(this.streak * 3, "Streak bonus");
                }
                this.streak++;
            }
            else
            {
                InformationLabel.Text = "";
                InformationLabel.Text = "プッシュです";
                this.streak = 0;
            }
            MentalCheck();
        }

        private void SetPenarty()
        {
            if (pData.GetNowGameMode() == 3)
            {
                PlusPoint((int)((float)-this.point * (0.002f * rateMan.rate)));
            }
        }

        /// <summary>
        /// ポイント(コイン)を追加し、
        /// 演出を行う
        /// </summary>
        /// <param name="value"></param>
        private void PlusPoint(int value)
        {
            this.oldPoint = this.point;
            this.point += (int)(value * this.pointMagn);
            this.PointLabel.ForeColor = Color.Red;
            PointTimer.Enabled = true;
        }

        /// <summary>
        /// アクションボタンをロックする
        /// </summary>
        private void ButtonLock()
        {
            HitPicture.Visible = false;
            StandPicture.Visible = false;
            DoublePicture.Visible = false;
            InsurancePicture.Visible = false;
            SurrenderPicture.Visible = false;
            DealButton.Enabled = true;
        }

        /// <summary>
        /// アクションボタンをアンロックする
        /// </summary>
        private void ButtonUnlock()
        {
            HitPicture.Visible = true;
            StandPicture.Visible = true;
            DoublePicture.Visible = true;
            InsurancePicture.Visible = true;
            SurrenderPicture.Visible = true;
        }

        /// <summary>
        /// Dealボタンが押された時の動作を行う
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            ResetSlash();
            ClearAdditionalText();
            ArrowPicture.Visible = false;
            ExplainPanel.Visible = false;
            ArrowTimer.Enabled = false;
            ColorTimer.Enabled = false;
            ButtonUnlock();
            DealButton.Enabled = false;
            InsurancePicture.Visible = false;
            try
            {
                if (card.GetRemainingCardsPer() < shafflePer)
                {
                    try
                    {
                        card.DumpDeck();
                        card.SetDeckNum(deck);
                        card.CreateDeck();
                        card.ShaffleDeck();
                        InformationLabel.Text = "";
                        InformationLabel.Text = "デッキをシャッフルしました";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
            }
            catch (CardNotInDeckException)
            {
                try
                {
                    card.DumpDeck();
                    card.SetDeckNum(deck);
                    card.CreateDeck();
                    card.ShaffleDeck();
                    InformationLabel.Text = "";
                    InformationLabel.Text = "ディールを開始します";
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

        /// <summary>
        /// カードのヒットを行う
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HitPicture_Click(object sender, EventArgs e)
        {
            DoublePicture.Visible = false;
            SurrenderPicture.Visible = false;
            InsurancePicture.Visible = false;
            SetCard(1, card.DrawCard(), false);
            if (total[1] >= 21)
            {
                SetCard(0, hiddenCard, true);
                BattleCards();
            }
            SetBustPer();
        }

        /// <summary>
        /// 次カードを引いた際にプレイヤーがバストする確率を求めチップに追加する
        /// ディーラーの隠されているカードが分からないように大体の確率で表示される
        /// </summary>
        private void SetBustPer()
        {
            double per = 0.0;
            int remainingCard = 0;
            for (int i = 0; i < 9; i++)
            {
                if (playerMinTotal + (i + 1) > 21)
                {
                    per += card.GetCardDrawPer(i) * 100.0;
                    remainingCard += card.GetRemainingCard(i);
                }
            }
            for (int i = 9; i < 13; i++)
            {
                if (playerMinTotal + 10 > 21)
                {
                    per += card.GetCardDrawPer(i) * 100.0;
                    remainingCard += card.GetRemainingCard(i);
                }
            }
            per = (int)per - (int)per % 5;
            this.bustPer = per;
            MoveBustPer();
            BustPerLabel.Text = per.ToString("F0") + "%";
        }

        /// <summary>
        /// カードのスタンドを行う
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StandPicture_Click(object sender, EventArgs e)
        {
            SetCard(0, hiddenCard, true);
            HitDealerCard();
        }

        /// <summary>
        /// ダブルダウンを行う
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DoublePicture_Click(object sender, EventArgs e)
        {
            this.pointMagn *= 2.0f;
            SetCard(1, card.DrawCard(), false);
            SetCard(0, hiddenCard, true);
            HitDealerCard();
        }

        /// <summary>
        /// インシュランスを行う
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InsurancePicture_Click(object sender, EventArgs e)
        {
            if(hiddenCard.number >= 10)
            {
                ButtonLock();
                SetCard(0, hiddenCard, true);
                InformationLabel.Text = "";
                InformationLabel.Text = "インシュランス成功";
            }
            else
            {
                InsurancePicture.Visible = false;
                this.mental -= 5;
                InformationLabel.Text = "";
                InformationLabel.Text = "インシュランス失敗";
                MentalCheck();
            }
        }

        /// <summary>
        /// サレンダーを行う
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SurrenderPicture_Click(object sender, EventArgs e)
        {
            Slash(true);
            ButtonLock();
            SetCard(0, hiddenCard, true);
            InformationLabel.Text = "";
            InformationLabel.Text = "勝負を降りました";
            this.mental -= 5;
            this.streak = 0;
            SetAdditionalScore(-5, "Nope");
            MentalCheck();
        }

        /// <summary>
        /// 字を動かすやつ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InformationLabel_TextChanged(object sender, EventArgs e)
        {
            if (InformationLabel.Text != "")
            {
                InformationLabel.Visible = true;
            }
            else
            {
                InformationLabel.Visible = false;
            }
        }

        
        /// <summary>
        /// ポイントの演出
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            Task task = Task.Run(() =>
            {
                if (oldPoint < this.point)
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
            });
        }

        private int colorTag = 0;

        /// <summary>
        /// 隠し要素
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ColorTimer_Tick(object sender, EventArgs e)
        {
            Task task = Task.Run(() =>
            {
                Color[] colorTypes = new Color[] { Color.Red, Color.Orange, Color.Yellow, Color.Green, Color.Blue, Color.Purple, Color.Black };
                TscoreLabel.ForeColor = colorTypes[colorTag % colorTypes.Length];
                colorTag++;
            });
        }

        private int arrowTime = 0;

        /// <summary>
        /// 開始直後の矢印操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ArrowTimer_Tick(object sender, EventArgs e)
        {
            Task task = Task.Run(() =>
            {
                ArrowPicture.Top += (int)(Math.Sin(arrowTime) * 5.0);
                arrowTime++;
            });
        }


        /// <summary>
        /// 戦績の表示切り替え
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void statPicture_Click(object sender, EventArgs e)
        {
            StatPanel.Visible = !StatPanel.Visible;
        }

        /// <summary>
        /// スタンダードモード
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TabPicture1_Click(object sender, EventArgs e)
        {
            SetTabRed();
            TabPicture1.Image = Properties.Resources.point2;
            ExplainLabel.Text = "スタンダードモード" + LB + "デッキ数: 3" + LB + "クレジット: 200" + LB +
            "賭けクレジット: 10";
            this.deck = 3;
            this.maxMental = 200;
            this.mainPoint = 500;
            this.mental = this.maxMental;
            this.defaultMagn = 1.0f;
            this.magn = this.defaultMagn;
            pData.SetNowGameMode(1);
            this.BackgroundImage = Properties.Resources.playmat_green1;
            MemberPicture.Visible = false;
            MemberPanel.Visible = false;
            SetStats();
            MentalLabel.Text = this.mental + "";
        }

        /// <summary>
        /// カジュアルモード
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TabPicture2_Click(object sender, EventArgs e)
        {
            SetTabRed();
            TabPicture2.Image = Properties.Resources.point2;
            ExplainLabel.Text = "カジュアルモード" + LB + "デッキ数: 2" + LB + "クレジット: 100" + LB +
            "賭けクレジット: 10";
            this.deck = 2;
            this.maxMental = 100;
            this.mainPoint = 1000;
            this.mental = this.maxMental;
            this.defaultMagn = 1.0f;
            this.magn = this.defaultMagn;
            this.BackgroundImage = Properties.Resources.playmat_green1;
            pData.SetNowGameMode(2);
            MemberPicture.Visible = false;
            MemberPanel.Visible = false;
            SetStats();
            MentalLabel.Text = this.mental + "";
        }

        /// <summary>
        /// モード切り替えボタン
        /// </summary>
        private void SetTabRed()
        {
            TabPicture1.Image = Properties.Resources.point3;
            TabPicture2.Image = Properties.Resources.point3;
            TabPicture3.Image = Properties.Resources.point3;
        }

        /// <summary>
        /// ヘルプ表示切り替え
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HelpLabel_Click(object sender, EventArgs e)
        {
            HelpParentPanel.Visible = !HelpParentPanel.Visible;
        }

        /// <summary>
        /// ヘルプのスクロール
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HelpScrollBar_Scroll_1(object sender, ScrollEventArgs e)
        {
            HelpPanel.Top = this.helpDefaultTop - HelpScrollBar.Value;
        }

        private void TabPicture3_Click(object sender, EventArgs e)
        {
            SetTabRed();
            TabPicture3.Image = Properties.Resources.point2;
            ExplainLabel.Text = "T's BJ 会員モード" + LB + "デッキ数: 3" + LB + LB + "負けペナルティ:"
                + LB + "ポイント没収";
            this.deck = 3;
            this.maxMental = 200;
            this.mainPoint = 500;
            this.mental = this.maxMental;
            this.defaultMagn = 1.0f;
            this.magn = this.defaultMagn;
            pData.SetNowGameMode(3);
            this.BackgroundImage = Properties.Resources.playmat_green1;
            MemberPicture.Visible = true;
            SetStats();
            MentalLabel.Text = this.mental + "";
        }

        private void SetMemberData()
        {
            MemberLvLabel.Text = rateMan.rate + "";
        }

        private void MemberPicture_Click(object sender, EventArgs e)
        {
            MemberPanel.Visible = !MemberPanel.Visible;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void HitPicture_MouseEnter(object sender, EventArgs e)
        {
            if (bustPer > 0.0)
            {
                MinusLabel.Text = "-10";
            }
        }

        private void StandPicture_MouseEnter(object sender, EventArgs e)
        {
            MinusLabel.Text = "-10";
        }

        private void InsurancePicture_MouseEnter(object sender, EventArgs e)
        {
            MinusLabel.Text = "-5";
        }

        private void SurrenderPicture_MouseEnter(object sender, EventArgs e)
        {
            MinusLabel.Text = "-5";
        }

        private void DoublePicture_MouseEnter(object sender, EventArgs e)
        {
            MinusLabel.Text = "-20";
        }

        private void SplitPicture_MouseEnter(object sender, EventArgs e)
        {
            MinusLabel.Text = "-20";
        }

        private void HitPicture_MouseLeave(object sender, EventArgs e)
        {
            MinusLabel.Text = "";
        }

        /// <summary>
        /// スラッシュアニメーションを再生する
        /// </summary>
        private void Slash(bool isPlayer)
        {
            slashCnt = 0;
            SlashPic.Visible = true;
            SlashRevPic.Visible = true;
            SlashPlPic.Visible = true;
            SlashRevPlPic.Visible = true;
            SlashTimer.Enabled = true;
            this.slashIsPlayer = isPlayer;
        }

        /// <summary>
        /// スラッシュアニメーションを非表示
        /// </summary>
        private void ResetSlash()
        {
            SlashPic.Visible = false;
            SlashRevPic.Visible = false;
            SlashPlPic.Visible = false;
            SlashRevPlPic.Visible = false;
            SlashPic.Image = null;
            SlashRevPic.Image = null;
            SlashPlPic.Image = null;
            SlashRevPlPic.Image = null;
        }

        /// <summary>
        /// アニメーションカウント
        /// </summary>
        int slashCnt = 0;

        System.Drawing.Bitmap[] slashResources =
        {
            Properties.Resources.slash_1,
            Properties.Resources.slash_2,
            Properties.Resources.slash_3
        };

        System.Drawing.Bitmap[] slashRevResources =
        {
            Properties.Resources.slash_rev_1,
            Properties.Resources.slash_rev_2,
            Properties.Resources.slash_rev_3
        };

        ///プレイヤーにスラッシュをするか
        private bool slashIsPlayer = false;

        /// <summary>
        /// アニメーションタイマー
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SlashTimer_Tick(object sender, EventArgs e)
        {
            if (slashIsPlayer)
            {
                SlashPlPic.Image = slashResources[slashCnt];
                SlashRevPlPic.Image = slashRevResources[slashCnt];
            }
            else
            {
                SlashPic.Image = slashResources[slashCnt];
                SlashRevPic.Image = slashRevResources[slashCnt];
            }
            slashCnt++;
            if(slashCnt > 2)
            {
                SlashTimer.Enabled = false;
            }
        }

        /// <summary>
        /// AdditionalScoreを加算する
        /// </summary>
        /// <param name="score">加算得点</param>
        /// <param name="text">テキスト</param>
        private void SetAdditionalScore(int score, string text)
        {
            if(score > 0)
            {
                this.AdditionalLabel.Text = "+" + score + " " + text;
            }
            else
            {
                this.AdditionalLabel.Text = score + " " + text;
            }
            this.additionalScore += score;
            if (this.additionalScore < 0) this.additionalScore = 0;
        }

        /// <summary>
        /// AdditionalLabelをクリアする
        /// </summary>
        private void ClearAdditionalText()
        {
            this.AdditionalLabel.Text = "";
        }

        /// <summary>
        /// 前のBustPer
        /// </summary>
        private int bustPerBef = 0;

        /// <summary>
        /// BustPerを変更する
        /// </summary>
        /// <param name="per"></param>
        private void MoveBustPer()
        {
            BustIncreaseTimer.Enabled = true;
        }

        /// <summary>
        /// BustPerの変更処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BustIncreaseTimer_Tick(object sender, EventArgs e)
        {
            if(this.bustPerBef < this.bustPer)
            {
                this.bustPerBef++;
                BustPerLabel.ForeColor = Color.Red;
            }
            else if(this.bustPerBef > this.bustPer)
            {
                this.bustPerBef--;
                BustPerLabel.ForeColor = Color.Blue;
            }
            else if(this.bustPerBef == this.bustPer)
            {
                BustIncreaseTimer.Enabled=false;
                BustPerLabel.ForeColor = Color.Black;
            }
            BustPerLabel.Text = this.bustPerBef.ToString() + "%";
        }

        private void ArrowPicture_Click(object sender, EventArgs e)
        {

        }
    }
}
