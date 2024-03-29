﻿using System;
using System.Collections.Generic;

/// <summary>
/// 汎用トランプカード管理クラス
/// </summary>
public class CardManager
{
    private const int cardNum = 52;                     //合計カード数 変更するな
    private int deckNum;                                //合計デッキ数
    private bool cardsInDeck;                           //デッキにカードが入っているか
    private List<int> cards = new List<int>();          //カードリスト
    private List<int> cardsPattern = new List<int>();   //柄リスト
    private int[] cardHistory = new int[13];            //カードを引いた履歴
    private Random rand = new Random();                 //Randインスタンス
    private int nextCard;                               //カードカウンタ 次のカードを示す

    public CardManager()//コンストラクタ
    {
        this.deckNum = 0;
        this.nextCard = 0;
        this.cardsInDeck = false;
        //コンストラクタではデッキをクリアしないため注意
    }

    /// <summary>
    /// デッキ数を指定する
    /// デッキにカードが存在する場合Exceptionをthrowする
    /// </summary>
    /// <param name="deck"></param>
    public void SetDeckNum(int deck)
    {
        //カードがデッキに存在するためデッキ数を変更できない
        if (cardsInDeck)
        {
            throw new CardsInDeckException();
        }

        //deckNumにデッキ数を設定
        this.deckNum = deck;
    }

    /// <summary>
    /// デッキを生成する
    /// 生成されたデッキは1-13の数字で初期化される
    /// </summary>
    public void CreateDeck()
    {
        //デッキ数が0以下である
        if (deckNum <= 0)
        {
            throw new DeckNumUnderZeroException();
        }
        //デッキの上書き不可
        if (this.cardsInDeck == true)
        {
            throw new CardsInDeckException();
        }

        //カードをクリア
        cards.Clear();

        //カードパターンをクリア
        cardsPattern.Clear();

        for (int i = 0; i < cardNum * deckNum; i++)
        {
            cards.Add(i % 13 + 1);
            cardsPattern.Add((int)(i / 13) % 4);
            //デッキにカードを1～13の順に挿入
            //13枚ごとにカードの柄を変える
        }

        //次のカードをデッキの一番上に指定
        this.nextCard = 0;

        //カードを引いた履歴をクリア
        for (int i = 0; i < 13; i++)
        {
            this.cardHistory[i] = 0;
        }

        //デッキにカードが入ってる
        this.cardsInDeck = true;
    }

    /// <summary>
    /// デッキをシャッフルする
    /// </summary>
    public void ShaffleDeck()
    {
        //デッキにカードが存在しない
        if (this.cardsInDeck == false)
        {
            throw new CardNotInDeckException();
        }

        //念のため2回行う
        for (int cnt = 0; cnt < 2; cnt++)
        {
            //現在の残りデッキ位置を設定
            int startCard = this.nextCard;

            //最初のカードから最後のカードまで行う
            for (int i = startCard; i < cards.Count; i++)
            {
                //最初のカードから順番にランダムなカードと入れ替える
                this.Change(i, rand.Next(startCard, cards.Count));
            }
        }
    }

    /// <summary>
    /// デッキをクリアする
    /// </summary>
    public void DumpDeck()
    {
        //デッキ数を0に
        this.deckNum = 0;
        //カードを削除
        cards.Clear();
        //カードの柄を削除
        cardsPattern.Clear();
        //次のカードは最初からに
        this.nextCard = 0;
        //カード履歴をリセット
        for (int i = 0; i < 13; i++)
        {
            this.cardHistory[i] = 0;
        }
        //デッキにカードが入っていない
        this.cardsInDeck = false;
    }

    /// <summary>
    /// カードを引く
    /// カードがデッキからなくなっている場合はnullを返す
    /// </summary>
    /// <returns>カード番号</returns>
    public CardProperties DrawCard()
    {
        //デッキにカードが存在しない
        if (this.nextCard >= this.deckNum * cardNum)
        {
            cardsInDeck = false;
            return null;
        }
        //カード情報インスタンスを生成
        CardProperties cp = new CardProperties();
        //出たカードを引いた枚数を足す
        this.cardHistory[this.cards[this.nextCard] - 1]++;
        //次のカード指定を1つずらす
        this.nextCard++;
        //カード情報にカードの数字を指定する
        cp.number = this.cards[this.nextCard - 1];
        //カード情報にカードの柄を指定する
        cp.pattern = this.cardsPattern[this.nextCard - 1];

        return cp;
    }

    /// <summary>
    /// 指定したカードを並び替える
    /// private
    /// </summary>
    /// <param name="cardA">カードA</param>
    /// <param name="cardB">カードB</param>
    private void Change(int cardA, int cardB)
    {
        //引数がカードの枚数を超えている
        if (cardA >= cards.Count || cardB >= cards.Count)
        {
            throw new CardArgOutOfRangeException();
        }
        //AとBのカードを入れ替え
        int mem;
        mem = cards[cardA];
        cards[cardA] = cards[cardB];
        cards[cardB] = mem;
        mem = cardsPattern[cardA];
        cardsPattern[cardA] = cardsPattern[cardB];
        cardsPattern[cardB] = mem;
    }

    /// <summary>
    /// 指定したカードを引いた枚数を取得する
    /// </summary>
    /// <param name="index">カード番号 - 1</param>
    /// <returns></returns>
    public int GetCardHistory(int index)
    {
        //引数がカードの種類総数を超えている
        if (index >= 13)
        {
            throw new CardArgOutOfRangeException();
        }
        return this.cardHistory[index];
    }

    /// <summary>
    /// 指定したカードの残り枚数を取得する
    /// </summary>
    /// <param name="index">カード番号 - 1</param>
    /// <returns></returns>
    public int GetRemainingCard(int index)
    {
        //引数がカードの種類総数を超えている
        if (index >= 13)
        {
            throw new CardArgOutOfRangeException();
        }
        //最大カード数に引いたカード数を引く
        int remain = this.deckNum * 4 - this.cardHistory[index];
        return remain;
    }

    /// <summary>
    /// 次のそのカードを引く確率を取得する
    /// </summary>
    /// <param name="index">カードの番号 - 1</param>
    /// <returns>確率(double)</returns>
    public double GetCardDrawPer(int index)
    {
        //デッキに存在している残りのカード総数
        int remainingCardsInDeck = 0;
        //指定したカードの残りカード数
        int remainingCards;

        //デッキにカードが存在しない
        if (cardsInDeck == false)
        {
            throw new CardNotInDeckException();
        }

        //引数がカードの種類総数を超えている
        if (index >= 13)
        {
            throw new CardArgOutOfRangeException();
        }

        //デッキに入っているカードの総数を計算
        for (int i = 0; i < 13; i++)
        {
            //デッキのカード総数からひいたカード総数を引く
            remainingCardsInDeck += (4 * deckNum) - this.cardHistory[i];
        }

        //カード総数が0なら
        if (remainingCardsInDeck <= 0)
        {
            //指定したカードを引く確率は0
            return 0.0;
        }

        //指定したカードの残り総数を計算する
        remainingCards = (4 * deckNum) - this.cardHistory[index];

        //次に指定したカードを引く確率を計算する
        double per = (double)remainingCards / remainingCardsInDeck;

        return per;
    }

    /// <summary>
    /// デッキの残り%を取得する
    /// </summary>
    /// <returns></returns>
    public double GetRemainingCardsPer()
    {
        //デッキにカードが存在しない
        if (cardsInDeck == false)
        {
            throw new CardNotInDeckException();
        }

        //デッキの初期カード総数を計算する
        int total = deckNum * cardNum;

        //デッキの残っているカード総数を算出し、割合を計算する
        return (double)(total - nextCard) / total;
    }

    /// <summary>
    /// デッキの残り枚数を取得する
    /// </summary>
    /// <returns></returns>
    public int GetRemainingDeckCards()
    {
        //デッキにカードが存在しない
        if (cardsInDeck == false)
        {
            throw new CardNotInDeckException();
        }

        //デッキの初期カード総数を計算する
        int total = deckNum * cardNum;

        //残りのカード総数を算出する
        return total - nextCard;
    }

    /// <summary>
    /// カードの残数をCardNumクラスで返す
    /// </summary>
    /// <returns>CardNum</returns>
    public CardNum GetCardNum()
    {
        if(cardsInDeck == false)
        {
            throw new CardNotInDeckException();
        }
        CardNum card = new CardNum();
        for(int i = 0; i < 13; i++)
        {
            card.card[i] = GetRemainingCard(i);
        }
        return card;
    }
}

/// <summary>
/// カードの情報を保存しておくためのクラス
/// number: カードの番号
/// pattern: カードの柄番号
/// 必要に応じてインスタンスを生成すること
/// </summary>
public class CardProperties
{
    public int number;
    public int pattern;
    public CardProperties()
    {
        this.number = -1;
        this.pattern = -1;
    }

    /// <summary>
    /// セットされたカードのパターン(柄)を取得する
    /// 不明なパターンの場合nullを返す
    /// </summary>
    /// <returns></returns>
    public String GetCardPattern()
    {
        switch (pattern)
        {
            case 0:
                return "Heart";

            case 1:
                return "Spade";

            case 2:
                return "Clover";

            case 3:
                return "Diamond";

            default:
                return null;
        }
    }
}

/// <summary>
/// カードの枚数のみを保存するクラス
/// </summary>
public class CardNum
{
    public int[] card = new int[13]
    {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
    };
}


/// <summary>
/// エクセプション
/// </summary>
public class CardsInDeckException: Exception
    {
        public CardsInDeckException()
            : base("デッキにカードが存在しています")
        {

        }
    }

    public class CardNotInDeckException: Exception
    {
        public CardNotInDeckException()
            : base("デッキにカードがありません")
        {

        }
    }

    public class DeckNumUnderZeroException: Exception
    {
        public DeckNumUnderZeroException()
            : base("デッキ数が0以下です")
        {

        }
    }

public class CardArgOutOfRangeException : Exception
{
    public CardArgOutOfRangeException()
        : base("カード引数がカード枚数を超えています")
    {

    }
}