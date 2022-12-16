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
        public const string LB = "\r\n";                        //LineBreak
        private CardProperties[ , ] Cards;                      //場のカード集
        private Random rand = new Random();                     //ランダムクラス
        private Encryption encryption = new Encryption();       //暗号化クラス
        private RateManager rateMan;                            //レート管理
        private PlData pData;                                   //pDataクラス
        private Panel[,] Panels;                                //パネル管理用配列
        private Label[,] Numbers;                               //数字管理用配列
        private PictureBox[,] Pictures;                         //画像管理用配列
        private string tip;                                     //チップ情報
        private List<string> saveData;                          //セーブデータ保持リスト
        private int[] total;                                    //カードの合計値
        private int playerTotalMin;                             //プレイヤーカードの最低値
        private double bustPer;                                 //プレイヤーのバスト確率
        private int deck;                                       //デッキの数
        private const double shafflePer = 0.2;                  //何%になったらシャッフルするか
        private const int maxCards = 8;                         //場に出せるカードの最大数
        private int maxMental = 200;                            //最大メンタル
        private int mainPoint;                                  //配当
        private bool[] bjFlg;                                   //ブラックジャックかどうか
        private bool[] overFlg;                                 //カードが出せる量を超えているかどうか
        private float betMagn;                                  //メンタル賭け倍率
        private float pointMagn;                                //ポイント賭け倍率
        private float defaultMagn;                              //初期倍率
        private int credits;                                    //メンタル
        private int point;                                      //ポイント
        private int oldPoint;                                   //1ディール前のポイント
        private int totalDeal;                                  //合計ディール数
        private int[] cardIndex;                                //次のカード置き場所
        private CardProperties hiddenCard;                      //隠されたカード
        private int helpPanelDefaultTopPosition;                //ヘルプパネルの初期位置
        private int additionalScore = 0;                        //追加t-score
        private int winStreak = 0;                              //勢い
        private const int reaPer = 10;                          //レア確率
        private Bitmap aceCardPattern;                          //カードの柄(ace)
        private Bitmap tenCardPattern;                          //カードの柄(ten)
        private Bitmap backCardPattern;                         //カードの柄(back)
        private Bitmap jackCardPattern;                         //カードの柄(jack)
        private Bitmap queenCardPattern;                        //カードの柄(queen)
        private Bitmap kingCardPattern;                         //カードの柄(king)
        private int totalSurrender = 0;                         //サレンダー回数
        private bool firstBet = true;                           //最初の賭けかどうか
        private const int TASK_DELAY_TIME = 700;                //タスクの待ち時間

        /// <summary>
        /// Formコンストラクタ
        /// </summary>
        public Form1()
        {
            InitializeComponent();
            this.Width = 775;
            this.Height = 564;
            encryption.fileName = "encryptedData.dat";

            //パネルを初期値に戻す
            HelpParentPanel.Top = this.Height / 2 - HelpParentPanel.Height / 2;
            HelpParentPanel.Left = this.Width / 2 - HelpParentPanel.Width / 2;
            StatPanel.Top = this.Height / 2 - StatPanel.Height / 2;
            StatPanel.Left = this.Width / 2 - StatPanel.Width / 2;
            MemberPanel.Top = this.Height / 2 - MemberPanel.Height / 2;
            MemberPanel.Left = this.Width / 2 - MemberPanel.Width / 2;
            ExplainPanel.Left = 525;
            ExplainPanel.Top = 263;

            HelpPanel.Parent = HelpParentPanel;

            HelpPanel.Left = 0;
            HelpPanel.Top = 0;

            Init();
        }

        /// <summary>
        /// 初期化
        /// </summary>
        private void Init()
        {
            rateMan = new RateManager();
            pData = new PlData();
            pData.SetNowGameMode(1);
            saveData = new List<String>();

            //カード柄の設定
            if (rand.Next(100) >= reaPer)
            {
                aceCardPattern = Properties.Resources.tamaAce;
                backCardPattern = Properties.Resources.card_ura;
                tenCardPattern = Properties.Resources.tenrea;
                jackCardPattern = Properties.Resources.jack;
                queenCardPattern = Properties.Resources.queen;
                kingCardPattern = Properties.Resources.king;
            }
            else
            {
                aceCardPattern = Properties.Resources.tamaAce_handmade;
                backCardPattern = Properties.Resources.card_ura_handmade;
                tenCardPattern = Properties.Resources.tenrea;
                jackCardPattern = Properties.Resources.jack_handmade;
                queenCardPattern = Properties.Resources.queen_handmade;
                kingCardPattern = Properties.Resources.king_handmade;
            }

            try
            {
                saveData = encryption.Decrypt();
            }
            catch (FileNotFoundException)
            {

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.GetType() + "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            for (int i = 0; i < saveData.Count; i++)
            {
                String[] arr = saveData[i].Split(',');
                pData.AddData(int.Parse(arr[0]), int.Parse(arr[1]), int.Parse(arr[2]));
            }
            SetStats();
            ButtonLock();
            this.deck = 3;
            this.credits = maxMental;
            this.point = 0;
            this.totalDeal = 1;
            this.betMagn = 1.0f;
            this.pointMagn = 1.0f;
            this.defaultMagn = 1.0f;
            this.mainPoint = 500;
            this.helpPanelDefaultTopPosition = HelpPanel.Top;
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
            DeckPicture.Image = backCardPattern;
            SetMemberData();
            MentalLabel.Text = this.credits + "";
            ArrowTimer.Enabled = true;

            ClearCards();
            DealerTotalLabel.Text = "0";
            PlayerTotalLabel.Text = "0";
            ArrowPicture.Visible = true;
            PointLabel.Text = "0";
            TscoreLabel.Text = "T-Score:";
            StatPanel.Visible = false;
            MemberPanel.Visible = false;
            HelpParentPanel.Visible = false;
            ExplainPanel.Visible = true;
            BustPerLabel.Text = string.Empty;
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
                DealerTotalLabel.Text = "0";
                PlayerTotalLabel.Text = "0";
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
            this.betMagn = this.defaultMagn;
            this.pointMagn = 1.0f;
        }

        /// <summary>
        /// カードをセットする
        /// </summary>
        /// <param name="tag">タグ: プレイヤーかディーラーか</param>
        /// <param name="cp">カード情報</param>
        /// <param name="wasHidden">隠されていたカードかどうか</param>
        private async Task SetCardAsync(int tag, CardProperties cp, bool wasHidden)
        {
            if (cardIndex[tag] > 8 && total[tag] < 21)
            {
                overFlg[tag] = true;
            }
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
                if (rand.Next(9) == 0)
                {
                    Pictures[tag, cardIndex[tag]].Image = Properties.Resources.ace_nyan;
                }
                else
                {
                    Pictures[tag, cardIndex[tag]].Image = aceCardPattern;
                }
            }
            else if (cp.number == 11)
            {
                Numbers[tag, cardIndex[tag]].Visible = false;
                Pictures[tag, cardIndex[tag]].Image = jackCardPattern;
            }
            else if (cp.number == 12)
            {
                Numbers[tag, cardIndex[tag]].Visible = false;
                Pictures[tag, cardIndex[tag]].Image = queenCardPattern;
            }
            else if (cp.number == 13)
            {
                Numbers[tag, cardIndex[tag]].Visible = false;
                Pictures[tag, cardIndex[tag]].Image = kingCardPattern;
            }
            else if (cp.number == 10 && rand.Next(7) == 0)
            {
                Numbers[tag, cardIndex[tag]].Visible = false;
                Pictures[tag, cardIndex[tag]].Image = tenCardPattern;
            }
            else
            {
                Numbers[tag, cardIndex[tag]].Text = cp.number + "";
            }
            this.Invoke(new Action(() =>
            {
                Panels[tag, cardIndex[tag]].Visible = true;
            }));
            this.total[tag] = 0;
            this.playerTotalMin = 0;
            for (int i = 0; i <= cardIndex[tag]; i++)
            {
                if (Cards[tag, i].number == 1)
                {
                    this.total[tag] += 11;
                    this.playerTotalMin += 1;
                }
                else if (Cards[tag, i].number > 10)
                {
                    this.total[tag] += 10;
                    this.playerTotalMin += 10;
                }
                else
                {
                    this.total[tag] += Cards[tag, i].number;
                    this.playerTotalMin += Cards[tag, i].number;
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
                        if (Pictures[0, 0].Image == tenCardPattern && Pictures[0, 1].Image == Properties.Resources.ace_nyan)
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
                        if (Pictures[0, 0].Image == tenCardPattern && Pictures[0, 1].Image == Properties.Resources.ace_nyan)
                        {
                            ColorTimer.Enabled = true;
                        }
                        await SetCardAsync(0, hiddenCard, true);
                        BattleCards();
                    }
                    break;
            }
            cardIndex[tag]++;
            await Task.Delay(TASK_DELAY_TIME);
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
        private async Task SetHiddenCardAsync(CardProperties cp)
        {
            hiddenCard = cp;
            Numbers[0, 1].Visible = false;
            Panels[0, 1].Visible = true;
            Pictures[0, 1].Image = backCardPattern;
            DealerTotalLabel.Text += " ?";
            cardIndex[0]++;
            await Task.Delay(TASK_DELAY_TIME);
        }

        /// <summary>
        /// ディーラーのカードヒットを行う
        /// </summary>
        private async Task HitDealerCardAsync()
        {
            while(total[0] < 17)
            {
                await SetCardAsync(0, card.DrawCard(), false);
            }
            BattleCards();
        }

        /// <summary>
        /// クレジットをチェックする
        /// </summary>
        private void CheckCredits()
        {
            MentalLabel.Text = this.credits + "";
            float me = maxMental;
            float avgDmg = me / this.totalDeal;
            float Pt = (float)this.point / 100.0f;
            float dmgTolerance = 20.0f * this.defaultMagn;
            float tScore = (dmgTolerance - avgDmg) * Pt;
            if (tScore < 0) tScore = 0;
            tScore += this.additionalScore;
            TscoreLabel.Text = "T-Score: " + (int)tScore;
            if (this.credits <= 0)
            {
                ButtonLock();
                DealButton.Enabled = false;
                InformationLabel.Text = "Coins" + this.point + "、T-Score" + (int)tScore + " +" + additionalScore;
                try
                {
                    /*
                    using (StreamWriter sw = new StreamWriter("History.txt", true))
                    {
                        sw.WriteLine("Coins: " + this.point + " T-Score " + (int)tScore);
                    }
                    */
                    saveData.Add(this.point + "," + (int)tScore + "," + pData.GetNowGameMode());
                    String fullData = String.Empty;
                    for(int i = 0; i < saveData.Count; i++)
                    {
                        fullData += saveData[i] + LB;
                    }
                    encryption.Encrypt(fullData);
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                pData.AddData(this.point, (int)tScore);
                SetStats();
                if (pData.GetNowGameMode() == 2)
                {
                    rateMan.CalcRate(tScore);
                    EnableRankedGameEndAnimation();
                    //SetMemberData();
                }
                DealButton.Text = "Replay";
                DealButton.Enabled = true;
            }
        }

        /// <summary>
        /// 戦績を設定する
        /// </summary>
        private void SetStats()
        {
            String[] modeName = new String[] { "Casual", "T's BJ Member" };
            ModeLabel.Text = modeName[pData.GetNowGameMode() - 1];
            AvgCoinsLabel.Text = pData.GetMaxOfPoint() + "";
            int avg = (int)pData.GetAvgOftScore();
            AvgtScoreLabel.Text = avg.ToString();
            TotalCoinsLabel.Text = "Total: " + pData.GetTotalOfAllPoint() + "";
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
            if (overFlg[0] && overFlg[1])
            {
                InformationLabel.Text = "Misfortune push";
               this. winStreak = 0;
            }
            else if (overFlg[0])
            {
                Slash(true);
                InformationLabel.Text = "Dealer's misfortune";
                this.credits -= (int)(10 * betMagn);
                this.winStreak = 0;
            }
            else if (overFlg[1])
            {
                Slash(false);
                InformationLabel.Text = "Player's misfortune";
                SetAdditionalScore(20, "Super Luck");
                PlusPoint((int)(mainPoint * 2.0));
                if(pointMagn == 2.0)
                {
                    SetAdditionalScore(5, "Smart");
                }
                if(winStreak > 0)
                {
                    SetAdditionalScore(this.winStreak * 3, "Streak bonus");
                }
                this.winStreak++;
            }
            else if(total[1] > 21)
            {
                Slash(true);
                BustTimer.Enabled = true;
                InformationLabel.Text = "Player's bust";
                this.credits -= (int)(10 * betMagn);
                this.winStreak = 0;
            }
            else if(total[0] > 21)
            {
                Slash(false);
                InformationLabel.Text = "Dealer's bust";
                PlusPoint(mainPoint);
                if (pointMagn == 2.0)
                {
                    SetAdditionalScore(5, "Smart");
                }
                if (winStreak > 0)
                {
                    SetAdditionalScore(this.winStreak * 3, "Streak bonus");
                }
                this.winStreak++;
            }
            else if(bjFlg[0] && bjFlg[1])
            {
                InformationLabel.Text = "Blackjack push";
                this.winStreak = 0;
            }else if (bjFlg[0])
            {
                Slash(true);
                InformationLabel.Text = "Dealer's Blackjack";
                this.credits -= (int)(10 * betMagn);
                this.winStreak = 0;
            }
            else if (bjFlg[1])
            {
                Slash(false);
                InformationLabel.Text = "Player's Blackjack";
                PlusPoint((int)(mainPoint * 1.50));
                if (winStreak > 0)
                {
                    SetAdditionalScore(this.winStreak * 3, "Streak bonus");
                }
                this.winStreak++;
            }
            else if(total[0] > total[1])
            {
                Slash(true);
                InformationLabel.Text = "Dealer wins";
                this.credits -= (int)(10 * betMagn);
                this.winStreak = 0;
            }
            else if(total[1] > total[0])
            {
                Slash(false);
                InformationLabel.Text = "Player wins";
                PlusPoint(mainPoint);
                if (pointMagn == 2.0)
                {
                    SetAdditionalScore(5, "Smart");
                }
                if (winStreak > 0)
                {
                    SetAdditionalScore(this.winStreak * 3, "Streak bonus");
                }
                this.winStreak++;
            }
            else
            {
                InformationLabel.Text = "Push";
                this.winStreak = 0;
            }
            CheckCredits();
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
            //if(pData.GetNowGameMode() == 3)this.point += (int)(total[1] * mainPoint / 50);
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
        private void button1_ClickAsync(object sender, EventArgs e)
        {
            button1_click();
        }

        async private void button1_click()
        {
            if (firstBet)
            {
                rateMan.SetRatePenalty();
                firstBet = false;
            }
            if (DealButton.Text == "Replay")
            {
                Init();
                DealButton.Text = "Deal";
                return;
            }
            ResetSlash();
            BustIcon.Visible = true;
            BustIcon.Image = slashResources[0];
            ClearAdditionalText();
            ArrowPicture.Visible = false;
            ExplainPanel.Visible = false;
            ArrowTimer.Enabled = false;
            ColorTimer.Enabled = false;
            TscoreLabel.ForeColor = Color.Black;
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
                        InformationLabel.Text = string.Empty;
                        InformationLabel.Text = "Shuffled decks";
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
                    InformationLabel.Text = string.Empty;
                    InformationLabel.Text = "Place your bets";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            ClearCards();
            SetCardTip();
            await SetCardAsync(0, card.DrawCard(), false);
            await SetCardAsync(1, card.DrawCard(), false);
            await SetHiddenCardAsync(card.DrawCard());
            await SetCardAsync(1, card.DrawCard(), false);
            SetBustPer();
            if (Cards[0, 0].number == 1 && !bjFlg[1])
            {
                InsurancePicture.Visible = true;
            }
        }

        /// <summary>
        /// カードのヒットを行う
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HitPicture_ClickAsync(object sender, EventArgs e)
        {
            HitPicture_Click();
        }

        private async void HitPicture_Click()
        {
            DoublePicture.Visible = false;
            SurrenderPicture.Visible = false;
            InsurancePicture.Visible = false;
            await SetCardAsync(1, card.DrawCard(), false);
            if (total[1] >= 21)
            {
                StandPicture_Click();
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
                if (playerTotalMin + (i + 1) > 21)
                {
                    per += card.GetCardDrawPer(i) * 100.0;
                    remainingCard += card.GetRemainingCard(i);
                }
            }
            for (int i = 9; i < 13; i++)
            {
                if (playerTotalMin + 10 > 21)
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
        private void StandPicture_ClickAsync(object sender, EventArgs e)
        {
            StandPicture_Click();
        }

        private async void StandPicture_Click()
        {
            ButtonLock();
            await SetCardAsync(0, hiddenCard, true);
            await HitDealerCardAsync();
        }

        /// <summary>
        /// ダブルダウンを行う
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DoublePicture_ClickAsync(object sender, EventArgs e)
        {
            DoublePicture_Click();
        }

        private async void DoublePicture_Click()
        {
            this.pointMagn *= 2.0f;
            this.betMagn *= 2.0f;
            await SetCardAsync(1, card.DrawCard(), false);
            await SetCardAsync(0, hiddenCard, true);
            await HitDealerCardAsync();
        }

        /// <summary>
        /// インシュランスを行う
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InsurancePicture_ClickAsync(object sender, EventArgs e)
        {
            InsurancePicture_Click();
        }

        private async void InsurancePicture_Click()
        {
            if (hiddenCard.number >= 10)
            {
                ButtonLock();
                await SetCardAsync(0, hiddenCard, true);
                InformationLabel.Text = "Insurance success";
            }
            else
            {
                InsurancePicture.Visible = false;
                this.credits -= 5;
                InformationLabel.Text = "Insurance failure";
                CheckCredits();
            }
        }

        /// <summary>
        /// サレンダーを行う
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SurrenderPicture_ClickAsync(object sender, EventArgs e)
        {
            SurrenderPicture_Click();
        }

        private async void SurrenderPicture_Click()
        {
            Slash(true);
            ButtonLock();
            await SetCardAsync(0, hiddenCard, true);
            InformationLabel.Text = "You surrendered";
            this.credits -= 5;
            this.winStreak = 0;
            SetAdditionalScore(totalSurrender * -5, "Nope");
            totalSurrender++;
            CheckCredits();
        }

        /// <summary>
        /// 字を動かすやつ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InformationLabel_TextChanged(object sender, EventArgs e)
        {
            if (InformationLabel.Text != string.Empty)
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
            ExplainLabel.Text = "Standard" + LB + LB + "Decks: 3" + LB + "Credits: 200" + LB +
            "Bets: 10";
            this.deck = 3;
            this.maxMental = 200;
            this.mainPoint = 500;
            this.credits = this.maxMental;
            this.defaultMagn = 1.0f;
            this.betMagn = this.defaultMagn;
            pData.SetNowGameMode(1);
            this.BackgroundImage = Properties.Resources.playmat_green1;
            MemberPicture.Visible = false;
            MemberPanel.Visible = false;
            rateMan.enableRatePenalty = false;
            SetStats();
            MentalLabel.Text = this.credits + "";
        }

        /// <summary>
        /// Tower
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TabPicture2_Click(object sender, EventArgs e)
        {
            SetTabRed();
            TabPicture2.Image = Properties.Resources.point2;
            ExplainLabel.Text = "Cat's Tower" + LB + LB + "Decks: 4" + LB + "Credits: 100" + LB +
            "Bets: 10";
            this.deck = 4;
            this.maxMental = 100;
            this.mainPoint = 1000;
            this.credits = this.maxMental;
            this.defaultMagn = 1.0f;
            this.betMagn = this.defaultMagn;
            this.BackgroundImage = Properties.Resources.playmat_green1;
            pData.SetNowGameMode(2);
            MemberPicture.Visible = true;
            MemberPanel.Visible = false;
            rateMan.enableRatePenalty = true;
            SetStats();
            MentalLabel.Text = this.credits + "";
        }

        /// <summary>
        /// モード切り替えボタン
        /// </summary>
        private void SetTabRed()
        {
            TabPicture1.Image = Properties.Resources.point3;
            TabPicture2.Image = Properties.Resources.point3;
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
            HelpPanel.Top = this.helpPanelDefaultTopPosition - HelpScrollBar.Value;
        }

        /*
        private void TabPicture3_Click(object sender, EventArgs e)
        {
            SetTabRed();
            TabPicture3.Image = Properties.Resources.point2;
            ExplainLabel.Text = "Cat's tower " + LB + LB + "勝った際に手札が強いほど" + LB + "もらえるコインが増額";
            this.deck = 3;
            this.maxMental = 200;
            this.mainPoint = 500;
            this.credits = this.maxMental;
            this.defaultMagn = 1.0f;
            this.betMagn = this.defaultMagn;
            pData.SetNowGameMode(3);
            this.BackgroundImage = Properties.Resources.playmat_green1;
            MemberPicture.Visible = true;
            SetStats();
            MentalLabel.Text = this.credits + "";
        }
        */

        /// <summary>
        /// ランクアイコン
        /// </summary>
        private Bitmap[] rankResources =
        {
            Properties.Resources.neko1_1,
            Properties.Resources.neko1_2,
            Properties.Resources.neko1_3,
            Properties.Resources.neko3_1,
            Properties.Resources.neko3_2,
            Properties.Resources.neko3_3,
            Properties.Resources.neko2_1,
            Properties.Resources.neko2_2,
            Properties.Resources.neko2_3
        };

        /// <summary>
        /// ランク名
        /// </summary>
        private string[] rankNames =
        {
            "Normal Cat",
            "Stray Cat",
            "Abandoned Cat"
        };

        /// <summary>
        /// ランクモードの戦績適用
        /// </summary>
        private void SetMemberData()
        {
            MemberLvLabel.Text = rateMan.rate + "";
            int upRate = (rateMan.rate / RateManager.RANK_INTERVAL) * RateManager.RANK_INTERVAL + RateManager.RANK_INTERVAL;
            upRate = upRate - rateMan.rate;
            RankUpLabel.Text = upRate + "";
            int downRate = (rateMan.rate / RateManager.RANK_INTERVAL) * RateManager.RANK_INTERVAL;
            downRate = rateMan.rate - downRate;
            RankDownLabel.Text = downRate + "";
            RankPic.Image = rankResources[rateMan.rate / RateManager.RANK_INTERVAL];
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
            MinusLabel.Text = string.Empty;
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

        private Bitmap[] slashResources =
        {
            Properties.Resources.slash_1,
            Properties.Resources.slash_2,
            Properties.Resources.slash_3
        };

        private Bitmap[] slashRevResources =
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
            if (score == 0) return;
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
            this.AdditionalLabel.Text = string.Empty;
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

        /// <summary>
        /// バストアイコン用
        /// </summary>
        private short bustIconCnt = 0;

        /// <summary>
        /// バストした際の演出
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BustTimer_Tick(object sender, EventArgs e)
        {
            if (bustIconCnt > 2)
            {
                BustTimer.Enabled = false;
                bustIconCnt = 0;
                return;
            }
            BustIcon.Image = slashResources[bustIconCnt];
            bustIconCnt++;
        }

        /// <summary>
        /// ランクリスト用タイマ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RankPreviewTimer_Tick(object sender, EventArgs e)
        {
            RankListFlowPanel.Left--;
            if(RankListFlowPanel.Right <= 0)
            {
                RankListFlowPanel.Left = 0;
            }
        }

        /// <summary>
        /// ランクリストの表示位置調整機能
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MemberPanel_VisibleChanged(object sender, EventArgs e)
        {
            if (RankListFlowPanel.Right <= RankPreviewPanel.Right) return;
            if (MemberPanel.Visible)
            {
                RankListFlowPanel.Left = 0;
                RankPreviewTimer.Enabled = true;
            }
            else
            {
                RankListFlowPanel.Left = 0;
                RankPreviewTimer.Enabled = false;
            }
        }

        /// <summary>
        /// ランクのレート変動アニメーションを有効にする
        /// </summary>
        private void EnableRankedGameEndAnimation()
        {
            rateIncreasing = rateMan.rateBef;
            RankedAnimationTimer.Enabled = true;
            MemberPanel.Visible = true;
        }

        /// <summary>
        /// 増加中のレート値
        /// </summary>
        private int rateIncreasing = 0;

        /// <summary>
        /// レート増加アニメーション処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RankedAnimationTimer_Tick(object sender, EventArgs e)
        {
            if (rateMan.rate - rateIncreasing > 0)//+の際のアニメーション
            {
                RatePlusLabel.ForeColor = Color.Red;//文字色を赤に
                RatePlusLabel.Text = "+" + (rateMan.rate - rateIncreasing);//+の変動値を出力
            }
            else//-の際のアニメーション
            {
                RatePlusLabel.ForeColor = Color.Blue;//文字色を青に
                RatePlusLabel.Text = rateMan.rate - rateIncreasing + "";//-の変動値を出力
            }

            MemberLvLabel.Text = rateIncreasing + "";
            int upRate = (rateIncreasing / RateManager.RANK_INTERVAL) * RateManager.RANK_INTERVAL + RateManager.RANK_INTERVAL;
            upRate = upRate - rateIncreasing;
            RankUpLabel.Text = upRate + "";
            int downRate = (rateIncreasing / RateManager.RANK_INTERVAL) * RateManager.RANK_INTERVAL;
            downRate = rateIncreasing - downRate;
            RankDownLabel.Text = downRate + "";
            RankPic.Image = rankResources[rateIncreasing / RateManager.RANK_INTERVAL];
            if(rateIncreasing > rateMan.rate)
            {
                MemberLvLabel.ForeColor = Color.Blue;
                rateIncreasing--;
            }
            else if(rateIncreasing < rateMan.rate)
            {
                MemberLvLabel.ForeColor = Color.Red;
                rateIncreasing++;
            }
            else//アニメーション終了
            {
                MemberLvLabel.ForeColor = Color.Black;
                RankedAnimationTimer.Enabled = false;
                RatePlusLabel.Text = string.Empty;
                SetMemberData();
            }
        }
    }
}
