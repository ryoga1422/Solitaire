using System;
using System.Collections.Generic;
using System.Linq;
using Solitaire;
public class Deck
{
    private List<Card> cards;
    private static readonly Random rng = new Random();

    public Deck()
    {
        InitializeDeck();
    }

    private void InitializeDeck()
    {
        cards = new List<Card>();
        foreach (Suit suit in Enum.GetValues(typeof(Suit)))
        {
            foreach (Rank rank in Enum.GetValues(typeof(Rank)))
            {
                cards.Add(new Card(suit, rank));
            }
        }
        Shuffle(); // Shuffle the deck upon creation
    }

    public void Shuffle()
    {
        // Fisher-Yates shuffle algorithm
        int n = cards.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            Card value = cards[k];
            cards[k] = cards[n];
            cards[n] = value;
        }
    }

    public Card DrawCard()
    {
        if (cards.Count == 0)
        {
            throw new InvalidOperationException("The deck is empty.");
        }

        Card cardToDraw = cards[0];
        cards.RemoveAt(0);
        return cardToDraw;
    }
    public int Count
    {
        get { return cards.Count; }
    }
    public void Add(Card card)
    {
        if (card == null)
        {
            throw new ArgumentNullException(nameof(card), "Cannot add a null card to the deck.");
        }
        cards.Add(card);
    }
    public Card Last()
    {
        //Returns the last card in the deck without removing it
        if (cards.Count == 0)
        {
            throw new InvalidOperationException("The deck is empty.");
        }
        return cards[cards.Count - 1];
    }
    public Card Pop()
    {
        if (cards.Count == 0)
        {
            return null; // Return null if the deck is empty
        }

        Card card = cards[cards.Count - 1];
        cards.RemoveAt(cards.Count - 1);
        return card;
    }

    public void Push(Card card)
    {
        cards.Add(card);
    }
}
