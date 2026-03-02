using Backend.Models;

namespace Backend.UseCases
{
    public class CardUseCases
    {
        public int GetEffectiveStrength(Card card)
        {
            if (card.isSpecial) return 0; // Karty specjalne (pogoda itp.) nie mają siły
            return card.Strength;
        }

        // Sprawdza, czy karta może być zagrana w danym rzędzie
        public bool CanBePlacedInRow(Card card, int rowIdx)
        {
            return (int)card.place == rowIdx;
        }
    }
}
