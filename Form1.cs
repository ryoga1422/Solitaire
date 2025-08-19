using System.IO;
using System.Reflection;
using System.Linq;

namespace Solitaire
{
    public partial class Form1 : Form
    {
        private Deck deck;
        private List<Card> talonPile;
        private List<List<Card>> tableauPiles;
        private List<List<Card>> foundationPiles;
        private Dictionary<string, Image> cardImages;
        private PictureBox draggedCard;
        private Point originalLocation;
        private List<PictureBox> draggedCards;
        private Point dragOffset;
        private int cardSpacingY;
        private int tableauXStart;
        private int tableauYStart;
        private int cardWidth;
        private int horizontalPileSpacing;
        private int foundationsXStart;
        // Add these PictureBox variables
        private PictureBox stockPilePictureBox;
        private PictureBox talonPilePictureBox;
        private PictureBox heartsFoundationBox;
        private PictureBox diamondsFoundationBox;
        private PictureBox clubsFoundationBox;
        private PictureBox spadesFoundationBox;
        private Point mouseDownLocation;
        private PictureBox winScreenBox;
        private PictureBox pictureBoxGiveUp;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            cardImages = new Dictionary<string, Image>();
    
            // Load the card back image and the empty pile image
            cardImages.Add("card_back.png", Image.FromFile("Assets\\Deck1\\card_back.png"));
            cardImages.Add("EmptyPile.png", Image.FromFile("Assets\\EmptyPile.png"));
    
            foreach (Suit suit in Enum.GetValues(typeof(Suit)))
            {
                foreach (Rank rank in Enum.GetValues(typeof(Rank)))
                {
                    string rankString = getRankString(rank);
                    string imageFileName = $"{rankString}_{suit}.png";
                    cardImages.Add(imageFileName, Image.FromFile($"Assets\\Deck1\\{imageFileName}"));
                }
            }

            // Correctly initialize all of your data structures
            deck = new Deck();
            talonPile = new List<Card>();
            tableauPiles = new List<List<Card>>();
            //foundationPiles = new List<List<Card>>();
            foundationPiles = new List<List<Card>> { new List<Card>(), new List<Card>(), new List<Card>(), new List<Card>() };
            draggedCards = new List<PictureBox>();
            // Initialize the tableau piles and deal the cards
            InitializeTableauPiles();
    
            // Call the dynamic layout method to handle all sizing and positioning
            // This is the crucial line to add or uncomment
            UpdateLayout();
        }
        private void InitializeTableauPiles()
        {
            // Initialize the list of lists for the tableau piles
            for (int i = 0; i < 7; i++)
            {
                tableauPiles.Add(new List<Card>());
            }

            // Deal cards from the deck to the tableau piles
            for (int i = 0; i < 7; i++)
            {
                for (int j = i; j < 7; j++)
                {
                    Card dealtCard = deck.DrawCard();
                    if (i == j) // Flip the last card in each pile face up
                    {
                        dealtCard.IsFaceUp = true;
                    }
                    tableauPiles[j].Add(dealtCard);
                }
            }
        }

        private void DealCardsToUI()
        {
            // The core logic is now in a separate method
            UpdateLayout();
        }



        // Helper method to get the correct image file name from a Card object
        private string GetFaceUpImagePath(Card card)
        {
            string rankString = getRankString(card.Rank);
            return $"{rankString}_{card.Suit}.png";
        }
        private string getRankString(Rank rank)
        {
            switch (rank)
            {
                case Rank.Ace:
                    return "A";
                case Rank.Jack:
                    return "J";
                case Rank.Queen:
                    return "Q";
                case Rank.King:
                    return "K";
                default:
                    // For numeric ranks (Two to Ten), convert the enum value to a string.
                    // Since enums start at 0, and our ranks start with Ace (A) at 0,
                    // Two will be 1, so we add 1 to the integer value.
                    return ((int)rank + 1).ToString();
            }
        }
        private void stockPilePictureBox_Click(object sender, EventArgs e)
        {
            // If the deck is not empty, move the top card to the talon pile
            if (deck.Count > 0)
            {
                Card drawnCard = deck.Pop();
                drawnCard.IsFaceUp = true;
                talonPile.Add(drawnCard);
            }
            // If the deck is empty, move all cards from the talon back to the deck
            else if (talonPile.Count > 0)
            {
                // Reverse the talon pile to put cards back in the correct order
                talonPile.Reverse();
                foreach (var card in talonPile)
                {
                    card.IsFaceUp = false;
                    deck.Push(card);
                }
                talonPile.Clear();
            }
            // Update the layout to reflect the changes
            UpdateLayout();
        }


        private void ResetStockpile()
        {
            // Check if there are cards in the talon pile to reset
            if (talonPile.Count > 0)
            {
                // Loop through the talon cards and move them back to the deck
                for (int i = talonPile.Count - 1; i >= 0; i--)
                {
                    talonPile[i].IsFaceUp = false;
                    deck.Add(talonPile[i]); // Requires a public Add() method in your Deck class
                }
    
                talonPile.Clear();
                deck.Shuffle(); // Shuffle the deck again for a new pass
    
                // Update the UI for both piles
                talonPilePictureBox.Image = null;
                stockPilePictureBox.Image = cardImages["card_back.png"];
            }
        }


        //private void Card_MouseDown(object sender, MouseEventArgs e)
        //{
        //    // CRUCIAL FIX: Ignore the event if it's part of a double-click
        //    if (e.Clicks > 1)
        //    {
        //        return;
        //    }

        //    // Check if the left mouse button was pressed
        //    if (e.Button == MouseButtons.Left)
        //    {
        //        // Get the PictureBox that was clicked
        //        PictureBox clickedBox = sender as PictureBox;

        //        if (clickedBox.Tag is Card clickedCard)
        //        {
        //            draggedCard = clickedBox;
        //            originalLocation = clickedBox.Location;
        //            dragOffset = new Point(e.X, e.Y);

        //            // Find the card and its pile in the data structure
        //            List<Card> sourcePile = null;
        //            int cardIndex = -1;

        //            if (talonPile.Any() && talonPile.Last() == clickedCard)
        //            {
        //                // Dragging from the talon pile
        //                sourcePile = talonPile;
        //                cardIndex = talonPile.Count - 1;
        //                Log("Dragging single card: " + clickedCard.Rank + " of " + clickedCard.Suit);
        //                draggedCards.Clear();
        //                draggedCards.Add(clickedBox);
        //            }
        //            else if (tableauPiles.Any(p => p.Contains(clickedCard)))
        //            {
        //                // Dragging from a tableau pile
        //                sourcePile = tableauPiles.FirstOrDefault(p => p.Contains(clickedCard));
        //                cardIndex = sourcePile.IndexOf(clickedCard);
        //                Log("Dragging card from tableau pile: " + clickedCard.Rank + " of " + clickedCard.Suit);
        //                draggedCards.Clear();

        //                for (int i = cardIndex; i < sourcePile.Count; i++)
        //                {
        //                    Card cardInStack = sourcePile[i];
        //                    if (cardInStack.IsFaceUp)
        //                    {
        //                        PictureBox stackCardBox = GamePanel.Controls.OfType<PictureBox>()
        //                            .FirstOrDefault(pb => pb.Tag == cardInStack);

        //                        if (stackCardBox != null)
        //                        {
        //                            draggedCards.Add(stackCardBox);
        //                        }
        //                    }
        //                }
        //            }
        //            else if (foundationPiles.Any(p => p.Contains(clickedCard)))
        //            {
        //                // Dragging from a foundation pile
        //                sourcePile = foundationPiles.FirstOrDefault(p => p.Contains(clickedCard));
        //                cardIndex = sourcePile.Count - 1; // Always the top card
        //                Log("Dragging single card from foundation pile: " + clickedCard.Rank + " of " + clickedCard.Suit);

        //                // Add the single card to the list of dragged cards
        //                draggedCards.Clear();
        //                draggedCards.Add(clickedBox);
        //            }
        //        }
        //    }
        //}
        private void Card_MouseDown(object sender, MouseEventArgs e)
        {
            // Store the location of the mouse down event
            mouseDownLocation = e.Location;
    
            // Clear the dragged cards list for a new drag operation
            draggedCards.Clear();
        }




        //private void Card_MouseMove(object sender, MouseEventArgs e)
        //{
        //    if (draggedCards != null && draggedCards.Any())
        //    {
        //        // Get the new location for the top card
        //        Point newLocation = new Point(
        //            draggedCards[0].Left + e.X - dragOffset.X,
        //            draggedCards[0].Top + e.Y - dragOffset.Y
        //        );
        
        //        // Update the location of the top card
        //        draggedCards[0].Location = newLocation;
        
        //        // Update the location of all subsequent cards in the stack
        //        for (int i = 1; i < draggedCards.Count; i++)
        //        {
        //            draggedCards[i].Location = new Point(
        //                draggedCards[0].Left,
        //                draggedCards[0].Top + (i * cardSpacingY)
        //            );
        //        }
        //    }
        //}
        private void Card_MouseMove(object sender, MouseEventArgs e)
        {
            // Only proceed if the left mouse button is held down
            if (e.Button == MouseButtons.Left)
            {
                // Get the PictureBox that is being dragged
                PictureBox draggedBox = sender as PictureBox;

                // Check if a drag operation should begin
                if (draggedCards.Count == 0 &&
                    (Math.Abs(e.X - mouseDownLocation.X) > SystemInformation.DoubleClickSize.Width ||
                     Math.Abs(e.Y - mouseDownLocation.Y) > SystemInformation.DoubleClickSize.Height))
                {
                    // This is the start of a drag
                    if (draggedBox.Tag is Card draggedCard)
                    {
                        // Find the source pile and set up the draggedCards list
                        if (talonPile.Any() && talonPile.Last() == draggedCard)
                        {
                            Log("Dragging single card: " + draggedCard.Rank + " of " + draggedCard.Suit);
                            draggedCards.Add(draggedBox);
                        }
                        else if (tableauPiles.Any(p => p.Contains(draggedCard)))
                        {
                            List<Card> sourcePile = tableauPiles.FirstOrDefault(p => p.Contains(draggedCard));
                            int cardIndex = sourcePile.IndexOf(draggedCard);
                            Log("Dragging card from tableau pile: " + draggedCard.Rank + " of " + draggedCard.Suit);
                    
                            for (int i = cardIndex; i < sourcePile.Count; i++)
                            {
                                Card cardInStack = sourcePile[i];
                                if (cardInStack.IsFaceUp)
                                {
                                    PictureBox stackCardBox = GamePanel.Controls.OfType<PictureBox>()
                                        .FirstOrDefault(pb => pb.Tag == cardInStack);

                                    if (stackCardBox != null)
                                    {
                                        draggedCards.Add(stackCardBox);
                                    }
                                }
                            }
                        }
                        else if (foundationPiles.Any(p => p.Contains(draggedCard)))
                        {
                            Log("Dragging single card from foundation pile: " + draggedCard.Rank + " of " + draggedCard.Suit);
                            draggedCards.Add(draggedBox);
                        }
                    }
                }
        
                // If a drag is in progress, update the card positions
                if (draggedCards.Count > 0)
                {
                    for (int i = 0; i < draggedCards.Count; i++)
                    {
                        PictureBox currentCardBox = draggedCards[i];
                        Point newLocation = new Point(
                            currentCardBox.Location.X + e.X - mouseDownLocation.X,
                            currentCardBox.Location.Y + e.Y - mouseDownLocation.Y
                        );
                        currentCardBox.Location = newLocation;
                        currentCardBox.BringToFront();
                    }
                }
            }
        }
        private void Card_MouseUp(object sender, MouseEventArgs e)
        {
            if (draggedCards.Count == 0)
            {
                Log("No valid card was dragged, snapping back.");
                return;
            }

            Card topDraggedCard = draggedCards.Last().Tag as Card;
            Card bottomDraggedCard = draggedCards.First().Tag as Card;

            Log("Dragged card: " + topDraggedCard.Rank + " of " + topDraggedCard.Suit);
            Log("Bottom dragged card: " + bottomDraggedCard.Rank + " of " + bottomDraggedCard.Suit);

            Point dropLocation = new Point(
                draggedCards.First().Location.X + e.X,
                draggedCards.First().Location.Y + e.Y
            );

            PictureBox dropTargetBox = null;
            foreach (Control control in GamePanel.Controls)
            {
                if (control is PictureBox pictureBox && pictureBox.Bounds.Contains(dropLocation))
                {
                    if (draggedCards.Contains(pictureBox))
                    {
                        continue;
                    }
                    dropTargetBox = pictureBox;
                    break;
                }
            }

            bool moveSuccessful = false;
            List<Card> oldSourcePile = null;
    
            // Determine the original source pile of the dragged card
            oldSourcePile = talonPile.Contains(bottomDraggedCard) ? talonPile :
                            tableauPiles.FirstOrDefault(p => p.Contains(bottomDraggedCard));

            if (oldSourcePile == null)
            {
                oldSourcePile = foundationPiles.FirstOrDefault(p => p.Contains(bottomDraggedCard));
            }


            if (dropTargetBox != null)
            {
                // Case 1: Dropping on a foundation pile
                if (dropTargetBox == heartsFoundationBox || dropTargetBox == diamondsFoundationBox ||
                    dropTargetBox == clubsFoundationBox || dropTargetBox == spadesFoundationBox)
                {
                    Log("Dropping on a foundation pile.");
                    if (IsValidFoundationMove(topDraggedCard, dropTargetBox))
                    {
                        MoveCardToFoundation(topDraggedCard, dropTargetBox);
                        moveSuccessful = true;
                        Log("Move to foundation successful.");
                    }
                }
                // Case 2: Dropping a King on an empty tableau space
                else if (dropTargetBox.Tag is List<Card> targetPile && bottomDraggedCard.Rank == Rank.King)
                {
                    Log("Dropping a King on an empty tableau pile.");
                    MoveCardToTableau(bottomDraggedCard, targetPile);
                    moveSuccessful = true;
                    Log("Move to empty tableau successful.");
                }
                // Case 3: Dropping on a tableau pile
                else if (dropTargetBox.Tag is Card targetCard)
                {
                    Log("Dropping on a tableau pile.");
                    if (IsValidTableauMove(bottomDraggedCard, targetCard))
                    {
                        List<Card> tableauTargetPile = tableauPiles.FirstOrDefault(pile => pile.Contains(targetCard));
                        if (tableauTargetPile != null)
                        {
                            MoveCardToTableau(bottomDraggedCard, tableauTargetPile);
                            moveSuccessful = true;
                            Log("Move to tableau successful.");
                        }
                    }
                }
            }

            // After attempting the move, if it was successful, remove from the source pile
            if (moveSuccessful)
            {
                // CRUCIAL FIX: Now that we know the move is valid, remove the card(s) from the old pile
                if (oldSourcePile != null)
                {
                    Card cardToRemove = bottomDraggedCard;
                    while(oldSourcePile.Contains(cardToRemove))
                    {
                        oldSourcePile.Remove(cardToRemove);
                    }
                }
            }
            else
            {
                // No change is needed for illegal moves; the card is still in its original pile
                Log("Illegal move. Snapping cards back to original position.");
            }

            // Always clean up the dragged cards and refresh the UI.
            ResetDraggedCards();
            UpdateLayout();
        }
        private void ResetDraggedCards()
        {
            // Remove all dragged cards from the game panel
            foreach (var cardBox in draggedCards)
            {
                GamePanel.Controls.Remove(cardBox);
            }
            // Clear the list
            draggedCards.Clear();
        }

        private PictureBox FindTargetPictureBox(Point dropLocation)
        {
            Log($"Looking for a PictureBox at location: {dropLocation}");

            // Iterate through all controls on the GamePanel in reverse order
            // to find the topmost PictureBox first
            for (int i = GamePanel.Controls.Count - 1; i >= 0; i--)
            {
                Control control = GamePanel.Controls[i];

                if (control is PictureBox pictureBox && pictureBox.Bounds.Contains(dropLocation))
                {
                    // Crucial fix: Immediately skip if the found PictureBox is being dragged
                    if (draggedCards.Contains(pictureBox))
                    {
                        Log($"Found a dragged card at {pictureBox.Location}, skipping it.");
                        continue;
                    }

                    // If the drop target is a Card from a tableau pile, ensure it's the last card
                    if (pictureBox.Tag is Card card)
                    {
                        if (tableauPiles.Any(pile => pile.LastOrDefault() == card))
                        {
                            Log($"Found a valid tableau drop target: {card.Rank} of {card.Suit}");
                            return pictureBox;
                        }
                    }
                    // If it's not a tableau card, it must be one of the static piles
                    else if (pictureBox == talonPilePictureBox ||
                             pictureBox == heartsFoundationBox || pictureBox == diamondsFoundationBox ||
                             pictureBox == clubsFoundationBox || pictureBox == spadesFoundationBox)
                    {
                        Log($"Found a valid foundation/talon drop target.");
                        return pictureBox;
                    }
                    // Check for empty tableau piles
                    else if (pictureBox.Tag is List<Card> targetPile && targetPile.Count == 0)
                    {
                        Log("Found an empty tableau pile.");
                        return pictureBox;
                    }
                }
            }
    
            Log("No valid drop target found.");
            return null;
        }



        private bool IsValidFoundationMove(Card draggedCard, PictureBox targetBox)
        {
            // The target is a foundation pile.
            // Check if the pile is empty
            Card targetCard = targetBox.Tag as Card;

            if (targetCard == null)
            {
                // The foundation pile is empty, only an Ace can be moved.
                return draggedCard.Rank == Rank.Ace;
            }
            else
            {
                // The pile is not empty. The card must be the next rank up and same suit.
                return draggedCard.Suit == targetCard.Suit && (int)draggedCard.Rank == (int)targetCard.Rank + 1;
            }
        }

        private bool IsValidTableauMove(Card topDraggedCard, Card targetCard)
        {
            if (topDraggedCard == null)
            {
                Log("The dragged card is null, invalid move.");
                return false;
            }
            // A valid tableau move must be a different color and one rank lower
            Log("Checking tableau move validity: " + topDraggedCard.Rank + " of " + topDraggedCard.Suit +
                " on " + targetCard.Rank + " of " + targetCard.Suit);
            return IsOppositeColor(topDraggedCard.Suit, targetCard.Suit) && (int)topDraggedCard.Rank == (int)targetCard.Rank - 1;
        }

        // Helper method to check for opposite colors
        private bool IsOppositeColor(Suit suit1, Suit suit2)
        {
            bool isRed1 = (suit1 == Suit.Hearts || suit1 == Suit.Diamonds);
            bool isRed2 = (suit2 == Suit.Hearts || suit2 == Suit.Diamonds);
            return isRed1 != isRed2;
        }

        private PictureBox GetTableauPilePictureBox(int pileIndex)
        {
            if (pileIndex < 0 || pileIndex >= tableauPiles.Count)
            {
                return null; // Handle out-of-bounds index
            }

            if (tableauPiles[pileIndex].Count == 0)
            {
                return null; // Return null if the pile is empty
            }

            Card topCard = tableauPiles[pileIndex].Last();
    
            // Find the PictureBox with this card in its Tag
            foreach (Control control in GamePanel.Controls)
            {
                if (control is PictureBox pictureBox && pictureBox.Tag is Card card)
                {
                    if (card == topCard)
                    {
                        return pictureBox;
                    }
                }
            }
            return null;
        }
        private bool IsValidMoveToFoundation(Card card, Suit foundationSuit)
        {
            // First, find the correct foundation pile based on the suit
            List<Card> foundation = foundationPiles.FirstOrDefault(p => p.Count > 0 && p.Last().Suit == foundationSuit);

            // If the foundation pile is empty
            if (foundation == null || foundation.Count == 0)
            {
                // Only an Ace can be moved to an empty foundation pile
                return card.Rank == Rank.Ace;
            }
            else
            {
                // If the foundation pile is not empty, get the top card
                Card topCard = foundation.Last();

                // The card being moved must be the same suit and one rank higher
                // (e.g., a Two of Hearts on an Ace of Hearts)
                return card.Suit == topCard.Suit && card.Rank == topCard.Rank + 1;
            }
        }
        private void MoveCardToFoundation(Card card, PictureBox targetBox)
        {
            // Find the correct foundation pile data structure
            List<Card> targetFoundationPile = null;
            if (targetBox == heartsFoundationBox) targetFoundationPile = foundationPiles[0];
            else if (targetBox == diamondsFoundationBox) targetFoundationPile = foundationPiles[1];
            else if (targetBox == clubsFoundationBox) targetFoundationPile = foundationPiles[2];
            else if (targetBox == spadesFoundationBox) targetFoundationPile = foundationPiles[3];

            if (targetFoundationPile != null)
            {
                // Add the card to the foundation pile data structure
                targetFoundationPile.Add(card);

                // Find the card's original location and remove it from there.
                // Check if the card is in a tableau pile
                bool cardFoundAndRemoved = false;
                foreach (var tableauPile in tableauPiles)
                {
                    if (tableauPile.Contains(card))
                    {
                        tableauPile.Remove(card);
                        cardFoundAndRemoved = true;

                        // CRUCIAL FIX: Check if there's a card underneath and flip it.
                        if (tableauPile.Any())
                        {
                            Card newTopCard = tableauPile.Last();
                            if (!newTopCard.IsFaceUp)
                            {
                                newTopCard.IsFaceUp = true;
                                Log($"Flipped the top card of a tableau pile: {newTopCard.Rank} of {newTopCard.Suit}");
                            }
                        }
                        break;
                    }
                }

                // If not found in a tableau pile, check if it's in the talon pile
                if (!cardFoundAndRemoved && talonPile.Contains(card))
                {
                    talonPile.Remove(card);
                    cardFoundAndRemoved = true;
                }

                // Remove the dragged card's PictureBox from the GamePanel
                foreach (var draggedCardBox in draggedCards)
                {
                    GamePanel.Controls.Remove(draggedCardBox);
                }

                // Get the image path for the card
                string imagePath = GetFaceUpImagePath(card);

                // Update the target PictureBox's image to the new card
                targetBox.Image = cardImages[imagePath];
                targetBox.Tag = card; // Set the tag to the card, which is critical for future moves

                Log($"Moved {card.Rank} of {card.Suit} to the foundation.");
                UpdateLayout(); // Re-layout all cards to ensure correct Z-order

                // If the card was moved from the talon pile, update the talon pile's display
                if (!talonPile.Any())
                {
                    talonPilePictureBox.Image = null; // Or to an empty pile image
                    talonPilePictureBox.Tag = null;
                }
                else
                {
                    Card newTopCard = talonPile.Last();
                    talonPilePictureBox.Image = cardImages[GetFaceUpImagePath(newTopCard)];
                    talonPilePictureBox.Tag = newTopCard;
                }
            }
        }
        private bool IsValidMoveToTableau(Card card, List<Card> targetPile)
        {
            // If the target pile is empty, only a King can be moved there
            if (targetPile.Count == 0)
            {
                return card.Rank == Rank.King;
            }

            // Otherwise, check if the card is the opposite color and one rank lower
            Card topCard = targetPile.Last();
            return card.IsOppositeColor(topCard) && card.Rank == topCard.Rank - 1;
        }
        private void MoveCardToTableau(Card card, List<Card> targetPile)
        {
            // Remove the card from its original pile
            if (talonPile.Contains(card))
            {
                talonPile.Remove(card);
            }
            else
            {
                // Find the tableau pile the card came from
                List<Card> sourcePile = tableauPiles.FirstOrDefault(pile => pile.Contains(card));
                if (sourcePile != null)
                {
                    // The card being moved is always the first one in the draggedCards list
                    Card firstDraggedCard = (Card)draggedCards.First().Tag;
                    int cardIndex = sourcePile.IndexOf(firstDraggedCard);

                    // Remove the card and any cards on top of it from the pile
                    sourcePile.RemoveRange(cardIndex, sourcePile.Count - cardIndex);
    
                    // Check if there is a face-down card to flip
                    if (sourcePile.Count > 0 && !sourcePile.Last().IsFaceUp)
                    {
                        sourcePile.Last().IsFaceUp = true;
                    }
                }
            }

            // Add the card to the new tableau pile
            targetPile.Add(card);
    
            // Add the rest of the cards in the dragged stack to the new pile
            foreach(var dragged in draggedCards)
            {
                if ((Card)dragged.Tag != card)
                {
                    targetPile.Add((Card)dragged.Tag);
                }
            }
    
            // Call the single layout method to redraw the entire board
            UpdateLayout();
        }      

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            // Re-calculate the layout when the form size changes
            UpdateLayout();
        }
        private void UpdateLayout()
        {
            // Clear all existing PictureBoxes from the main game panel
            GamePanel.Controls.Clear();

            // Constants
            const int numTableauPiles = 7;
            const double cardAspectRatio = 1.4; // Height to width ratio
            horizontalPileSpacing = 10;
    
            // Add these new constants for maximum card size
            const int MAX_CARD_WIDTH = 180; 
            const int MAX_CARD_HEIGHT = (int)(MAX_CARD_WIDTH * cardAspectRatio);

            // Calculate dynamic card size based on available space
            int availableWidth = this.ClientSize.Width - (numTableauPiles + 1) * horizontalPileSpacing;
            int maxCardWidthFromWidth = availableWidth / numTableauPiles;
    
            // Account for the tallest tableau pile to prevent vertical cut-offs
            const int maxTableauHeightInCards = 7; 
            const int topRowHeight = 150; // Or a dynamic value based on cardHeight
            int availableHeight = this.ClientSize.Height - topRowHeight;
            int maxCardHeightFromHeight = availableHeight - (cardSpacingY * (maxTableauHeightInCards - 1));
            int maxCardWidthFromHeight = (int)(maxCardHeightFromHeight / cardAspectRatio);
    
            // Use Math.Min to ensure the card size does not exceed the maximums
            cardWidth = Math.Min(MAX_CARD_WIDTH, Math.Min(maxCardWidthFromWidth, maxCardWidthFromHeight));
            int cardHeight = (int)(cardWidth * cardAspectRatio);
    
            // Assign the spacing to the class-level variable
            cardSpacingY = (int)(cardHeight * 0.2); 

            // Re-create and position the static piles (Stockpile, Talon, Foundations)
            tableauXStart = horizontalPileSpacing;
            tableauYStart = cardHeight + horizontalPileSpacing;
    
            // Upper-left corner: Stockpile and Talon
            stockPilePictureBox = new PictureBox
            {
                Size = new Size(cardWidth, cardHeight),
                Location = new Point(foundationsXStart, horizontalPileSpacing),
                SizeMode = PictureBoxSizeMode.StretchImage,
                Image = deck.Count > 0 ? cardImages["card_back.png"] : cardImages["EmptyPile.png"],
                Tag = deck.Count > 0 ? deck.Last() : null
            };
            stockPilePictureBox.Click += stockPilePictureBox_Click; // This is a crucial fix
            GamePanel.Controls.Add(stockPilePictureBox);
            stockPilePictureBox.BringToFront();
            stockPilePictureBox.BringToFront();

            talonPilePictureBox = new PictureBox
            {
                Size = new Size(cardWidth, cardHeight),
                Location = new Point(foundationsXStart + (cardWidth + horizontalPileSpacing), horizontalPileSpacing),
                SizeMode = PictureBoxSizeMode.StretchImage,
                Image = talonPile.Count > 0 ? cardImages[GetFaceUpImagePath(talonPile.Last())] : cardImages["EmptyPile.png"],
                Tag = talonPile.Count > 0 ? talonPile.Last() : null
            };
            talonPilePictureBox.MouseDown += Card_MouseDown;
            talonPilePictureBox.MouseMove += Card_MouseMove;
            talonPilePictureBox.MouseUp += Card_MouseUp;
            talonPilePictureBox.DoubleClick += Card_DoubleClick; // Add double-click event handler
            talonPilePictureBox.BringToFront();
            GamePanel.Controls.Add(talonPilePictureBox);


            // Upper-right corner: Foundations
            //foundationsXStart = this.ClientSize.Width - (4 * (cardWidth + horizontalPileSpacing));
    
            heartsFoundationBox = new PictureBox
            {
                Size = new Size(cardWidth, cardHeight),
                Location = new Point(foundationsXStart + 2 * (cardWidth + horizontalPileSpacing), horizontalPileSpacing),
                SizeMode = PictureBoxSizeMode.StretchImage,
                Image = foundationPiles[0].Count > 0 ? cardImages[GetFaceUpImagePath(foundationPiles[0].Last())] : cardImages["EmptyPile.png"],
                Tag = foundationPiles[0].Count > 0 ? foundationPiles[0].Last() : null
            };
            heartsFoundationBox.MouseDown += Card_MouseDown;
            heartsFoundationBox.MouseUp += Card_MouseUp;
            heartsFoundationBox.MouseMove += Card_MouseMove;
            GamePanel.Controls.Add(heartsFoundationBox);
            heartsFoundationBox.BringToFront();

            diamondsFoundationBox = new PictureBox
            {
                Size = new Size(cardWidth, cardHeight),
                Location = new Point(foundationsXStart + 3 * (cardWidth + horizontalPileSpacing), horizontalPileSpacing),
                SizeMode = PictureBoxSizeMode.StretchImage,
                Image = foundationPiles[1].Count > 0 ? cardImages[GetFaceUpImagePath(foundationPiles[1].Last())] : cardImages["EmptyPile.png"],
                Tag = foundationPiles[1].Count > 0 ? foundationPiles[1].Last() : null
            };
            diamondsFoundationBox.MouseDown += Card_MouseDown;
            diamondsFoundationBox.MouseUp += Card_MouseUp;
            diamondsFoundationBox.MouseMove += Card_MouseMove;
            GamePanel.Controls.Add(diamondsFoundationBox);
            diamondsFoundationBox.BringToFront();

            clubsFoundationBox = new PictureBox
            {
                Size = new Size(cardWidth, cardHeight),
                Location = new Point(foundationsXStart + 4 * (cardWidth + horizontalPileSpacing), horizontalPileSpacing),
                SizeMode = PictureBoxSizeMode.StretchImage,
                Image = foundationPiles[2].Count > 0 ? cardImages[GetFaceUpImagePath(foundationPiles[2].Last())] : cardImages["EmptyPile.png"],
                Tag = foundationPiles[2].Count > 0 ? foundationPiles[2].Last() : null
            };
            clubsFoundationBox.MouseDown += Card_MouseDown;
            clubsFoundationBox.MouseUp += Card_MouseUp;
            clubsFoundationBox.MouseMove += Card_MouseMove;
            GamePanel.Controls.Add(clubsFoundationBox);
            clubsFoundationBox.BringToFront();
            spadesFoundationBox = new PictureBox
            {
                Size = new Size(cardWidth, cardHeight),
                Location = new Point(foundationsXStart + 5 * (cardWidth + horizontalPileSpacing), horizontalPileSpacing),
                SizeMode = PictureBoxSizeMode.StretchImage,
                Image = foundationPiles[3].Count > 0 ? cardImages[GetFaceUpImagePath(foundationPiles[3].Last())] : cardImages["EmptyPile.png"],
                Tag = foundationPiles[3].Count > 0 ? foundationPiles[3].Last() : null
            };
            spadesFoundationBox.MouseDown += Card_MouseDown;
            spadesFoundationBox.MouseUp += Card_MouseUp;
            spadesFoundationBox.MouseMove += Card_MouseMove;
            GamePanel.Controls.Add(spadesFoundationBox);
            spadesFoundationBox.BringToFront();
            // Draw the "Give Up" button
            if (pictureBoxGiveUp == null)
            {
                pictureBoxGiveUp = new PictureBox();
                pictureBoxGiveUp.Image = Image.FromFile("Assets\\giveUp.png");
                pictureBoxGiveUp.SizeMode = PictureBoxSizeMode.Zoom;
                pictureBoxGiveUp.Size = spadesFoundationBox.Size;
                pictureBoxGiveUp.Click += pictureBoxGiveUp_Click;
            }
    
            pictureBoxGiveUp.Location = new Point(
                spadesFoundationBox.Right + horizontalPileSpacing,
                spadesFoundationBox.Top
            );
    
            if (!GamePanel.Controls.Contains(pictureBoxGiveUp))
            {
                GamePanel.Controls.Add(pictureBoxGiveUp);
            }
            // Now, re-create and position the tableau piles
            for (int i = 0; i < tableauPiles.Count; i++)
            {
                // Check if the tableau pile is empty
                if (tableauPiles[i].Count == 0)
                {
                    PictureBox emptyPileBox = new PictureBox
                    {
                        Size = new Size(cardWidth, cardHeight),
                        Location = new Point(
                            tableauXStart + (i * (cardWidth + horizontalPileSpacing)),
                            tableauYStart
                        ),
                        SizeMode = PictureBoxSizeMode.StretchImage,
                        Image = cardImages["EmptyPile.png"],
                        // Set the tag to the entire pile to be able to drop a King
                        Tag = tableauPiles[i]
                    };
                    emptyPileBox.MouseDown += Card_MouseDown;
                    emptyPileBox.MouseUp += Card_MouseUp;

                    GamePanel.Controls.Add(emptyPileBox);
                }
                else
                {
                    // This loop has been corrected to draw from the bottom up
                    for (int j = tableauPiles[i].Count - 1; j >= 0; j--)
                    {
                        Card card = tableauPiles[i][j];
                        PictureBox cardPictureBox = new PictureBox
                        {
                            Size = new Size(cardWidth, cardHeight),
                            Location = new Point(
                                tableauXStart + (i * (cardWidth + horizontalPileSpacing)),
                                tableauYStart + (cardSpacingY * j)
                            ),
                            SizeMode = PictureBoxSizeMode.StretchImage,
                            Image = card.IsFaceUp ? cardImages[GetFaceUpImagePath(card)] : cardImages["card_back.png"],
                            Tag = card
                        };

                        // Connect the mouse event handlers
                        cardPictureBox.MouseDown += Card_MouseDown;
                        cardPictureBox.MouseMove += Card_MouseMove;
                        cardPictureBox.MouseUp += Card_MouseUp;
                        cardPictureBox.DoubleClick += Card_DoubleClick; // Add double-click event handler
                        GamePanel.Controls.Add(cardPictureBox);
                        //cardPictureBox.BringToFront();
                    }
                }
            }
            // CRUCIAL FIX: Check for a win after every update
            if (IsGameOver())
            {
                ShowWinScreen();
            }
        }
        private void Card_DoubleClick(object sender, EventArgs e)
        {
            Log(Name + " Card_DoubleClick called.");
            // Check if the sender is a PictureBox with a Card tag
            if (sender is PictureBox clickedBox && clickedBox.Tag is Card clickedCard)
            {
                Log("Double-clicked card: " + clickedCard.Rank + " of " + clickedCard.Suit);
                // Find the foundation pile to move the card to
                foreach (var foundationPileBox in new[] { heartsFoundationBox, diamondsFoundationBox, clubsFoundationBox, spadesFoundationBox })
                {
                    // Check if the clicked card can be moved to the foundation pile
                    Log("Checking foundation pile: " + foundationPileBox.Name);
                    if (IsValidFoundationMove(clickedCard, foundationPileBox))
                    {
                        // Find the source pile (talon or tableau)
                        List<Card> sourcePile = talonPile.Contains(clickedCard) ? talonPile :
                                                tableauPiles.FirstOrDefault(p => p.Contains(clickedCard));

                        if (sourcePile != null)
                        {
                            Log("Found source pile for double-click move: " + sourcePile.Count + " cards.");
                            // Check if the card is the topmost card of its pile
                            if (sourcePile.Last() == clickedCard)
                            {
                                
                                // Remove the card from the source pile
                                sourcePile.Remove(clickedCard);
                                Log("Moving card to foundation with double-click: " + clickedCard.Rank + " of " + clickedCard.Suit);
                        
                                // Move the card to the foundation pile
                                MoveCardToFoundation(clickedCard, foundationPileBox);
                                if (tableauPiles.Contains(sourcePile))
                                {
                                    if (sourcePile.Count > 0)
                                    {
                                        sourcePile.Last().IsFaceUp = true;
                                    }
                                }
                                // Since a move was successful, update the UI
                                UpdateLayout();
                                break; // Exit the loop after a successful move
                            }
                        }
                    }
                }
            }
        }
        private void Log(string message)
        {
            try
            {
                // Get the directory of the executing assembly
                string logDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string logFilePath = Path.Combine(logDirectory, "log.txt");

                // Format the log message with a timestamp
                string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";

                // Append the message to the file, creating it if it doesn't exist
                File.AppendAllText(logFilePath, logMessage + Environment.NewLine);
            }
            catch (Exception ex)
            {
                // Handle any potential errors, e.g., permissions issues
                // For now, we can simply show a message box
                MessageBox.Show($"Error writing to log file: {ex.Message}");
            }
        }
        private bool IsGameOver()
        {
            // A game is won when all four foundation piles are full (13 cards each).
            // The sum of the counts of all foundation piles should be 52.
            if (foundationPiles.Sum(p => p.Count) == 52)
            {
                Log("Game over! You won!");
                return true;
            }
            return false;
        }
        private void ShowWinScreen()
        {
            if (winScreenBox == null)
            {
                winScreenBox = new PictureBox();
                winScreenBox.SizeMode = PictureBoxSizeMode.Zoom;
                winScreenBox.Click += (sender, e) => RestartGame();
            }

            // You will need to add an image named "win.png" to your project
            winScreenBox.Image = Image.FromFile("Assets\\win.png");

            // Calculate position and size to cover 80% of the game panel
            int winScreenWidth = (int)(GamePanel.Width * 0.8);
            int winScreenHeight = (int)(GamePanel.Height * 0.8);
            int winScreenX = (GamePanel.Width - winScreenWidth) / 2;
            int winScreenY = (GamePanel.Height - winScreenHeight) / 2;
            winScreenBox.Bounds = new Rectangle(winScreenX, winScreenY, winScreenWidth, winScreenHeight);

            GamePanel.Controls.Add(winScreenBox);
            winScreenBox.BringToFront();
        }

        private void RestartGame()
        {
            // Hide the win screen if it's visible
            if (winScreenBox != null)
            {
                GamePanel.Controls.Remove(winScreenBox);
            }
    
            // Reset PictureBox variables to null to force recreation
            stockPilePictureBox = null;
            talonPilePictureBox = null;
            heartsFoundationBox = null;
            diamondsFoundationBox = null;
            clubsFoundationBox = null;
            spadesFoundationBox = null;
            pictureBoxGiveUp = null;

            // CRUCIAL FIX: Clear the main tableauPiles list
            tableauPiles.Clear();
    
            // Clear all other piles
            talonPile.Clear();
            deck = null;
            deck = new Deck();
            deck.Shuffle();
    
            // Clear the foundation piles
            foreach(var pile in foundationPiles)
            {
                pile.Clear();
            }
    
            // Re-deal cards
            InitializeTableauPiles();
    
            // Update the UI
            UpdateLayout();
        }

        private void pictureBoxGiveUp_Click(object sender, EventArgs e)
        {
            // Display a confirmation dialog
            var confirmResult = MessageBox.Show("Are you sure you want to give up and start a new game?",
                                                 "Confirm Give Up",
                                                 MessageBoxButtons.YesNo,
                                                 MessageBoxIcon.Question);

            // If the user selects "Yes", restart the game
            if (confirmResult == DialogResult.Yes)
            {
                RestartGame();
            }
        }















    }
}
