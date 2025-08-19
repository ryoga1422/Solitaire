using System.ComponentModel;
using System.Runtime.CompilerServices;


namespace Solitaire
{
    public enum Suit
    {
        Hearts,
        Diamonds,
        Clubs,
        Spades
    }

    public enum Rank
    {
        Ace,
        Two,
        Three,
        Four,
        Five,
        Six,
        Seven,
        Eight,
        Nine,
        Ten,
        Jack,
        Queen,
        King
    }

    public class Card
    {

        public Suit Suit { get; set; }
        public Rank Rank { get; set; }
        

        

        private bool _isFaceUp = false;
        public bool IsRed => Suit == Suit.Hearts || Suit == Suit.Diamonds;
        public bool IsBlack => Suit == Suit.Clubs || Suit == Suit.Spades;
        public bool IsOppositeColor(Card otherCard)
        {
            return (this.IsRed && otherCard.IsBlack) || (this.IsBlack && otherCard.IsRed);
        }

        public bool IsFaceUp
        {
            get => _isFaceUp;
            set
            {
                if (_isFaceUp != value)
                {
                    _isFaceUp = value;
                   
                }
            }
        }

        public string ImagePath
        {
            get
            {
                string imageFile = IsFaceUp ? GetFaceUpImagePath() : "card_back.png";
                // In WinForms, you'll likely load images directly from a subfolder
                // like "Assets\Deck1" within your project's bin directory.
                return $"Assets\\Deck1\\{imageFile}";
            }
        }

        private string GetFaceUpImagePath()
        {
            string rankString = GetRankString(this.Rank);
            return $"{rankString}_{Suit.ToString()}.png";
        }
        
        private static string GetRankString(Rank rank)
        {
            switch (rank)
            {
                case Rank.Ace: return "A";
                case Rank.Jack: return "J";
                case Rank.Queen: return "Q";
                case Rank.King: return "K";
                default: return ((int)rank + 1).ToString();
            }
        }
        
        public Card(Suit suit, Rank rank)
        {
            Suit = suit;
            Rank = rank;
        }
    }
}