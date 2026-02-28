using Backend.Models.Enums;

namespace Backend.Models
{
    public class Card
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Fractions fraction { get; set; }
        public Abilities ability { get; set; }
        public int Strength { get; set; }
        public Place place { get; set; }
        public bool isChampion { get; set; }
        public bool isCommander { get; set; }
        public bool isSpecial { get; set; }

        private Card() { }

        public Card(string Name, Fractions fraction, Abilities ability, int Strength, Place place, bool isChampion, bool isCommander, bool isSpecial) 
        { 
            this.Name = Name;
            this.fraction = fraction;
            this.ability = ability;
            this.Strength = Strength;
            this.place = place;
            this.isChampion = isChampion;
            this.isCommander = isCommander;
            this.isSpecial = isSpecial;
        }
    }
}
