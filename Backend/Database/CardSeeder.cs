using Backend.Models;
using Backend.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Backend.Database
{
    public static class CardSeeder
    {
        public static async Task SeedMissingFractionsAsync(GwintDBContext db)
        {
            var added = false;

            if (!await db.Cards.AnyAsync(c => c.fraction == Fractions.ScoiaTael))
            {
                db.Cards.AddRange(BuildScoiaTaelCards());
                added = true;
            }

            if (!await db.Cards.AnyAsync(c => c.fraction == Fractions.Monsters))
            {
                db.Cards.AddRange(BuildMonsterCards());
                added = true;
            }

            if (added)
            {

                await db.Database.ExecuteSqlRawAsync(@"
                    SELECT setval(
                        pg_get_serial_sequence('public.""Cards""', 'Id'),
                        COALESCE((SELECT MAX(""Id"") FROM public.""Cards""), 0) + 1,
                        false
                    );
                ");

                await db.SaveChangesAsync();
            }
        }

        private static IEnumerable<Card> BuildScoiaTaelCards()
        {
            var f = Fractions.ScoiaTael;

            // Dowódcy (1 z każdą zdolnością scolliatel)
            yield return Commander("Eithné: Queen of Brokilon",                f, Abilities.scolliatel1);
            yield return Commander("Francesca Findabair: The Beautiful",       f, Abilities.scolliatel2);
            yield return Commander("Francesca Findabair: Daisy of the Valley", f, Abilities.scolliatel3);
            yield return Commander("Francesca Findabair: Queen of Dol Blathanna", f, Abilities.scolliatel4);
            yield return Commander("Francesca Findabair: Pureblood Elf",       f, Abilities.scolliatel5);

            // Mistrzowie (hero – odporni na pogodę)
            yield return Champion("Saskia",               f, 15, Place.FirstRow);
            yield return Champion("Iorveth",              f, 10, Place.SecondRow);
            yield return Champion("Isengrim Faoiltiarna", f, 10, Place.FirstRow);
            yield return Champion("Milva",                f, 10, Place.SecondRow);
            yield return Champion("Schirru",              f,  8, Place.ThirdRow);
            yield return Champion("Yaevinn",              f,  6, Place.FirstAndSecondRow, Abilities.zwinnośc);
            yield return Champion("Ciaran aep Easnillen", f,  4, Place.FirstAndSecondRow, Abilities.zwinnośc);

            // Jednostki regularne
            for (int i = 0; i < 3; i++)
                yield return Unit("Elven Skirmisher",          f, 2, Place.SecondRow, Abilities.braterstwo);
            for (int i = 0; i < 5; i++)
                yield return Unit("Mahakam Defender",          f, 5, Place.FirstRow,  Abilities.braterstwo);
            for (int i = 0; i < 5; i++)
                yield return Unit("Mahakam Guard",             f, 5, Place.FirstRow,  Abilities.braterstwo);
            for (int i = 0; i < 3; i++)
                yield return Unit("Dol Blathanna Archer",      f, 4, Place.SecondRow, Abilities.braterstwo);
            for (int i = 0; i < 3; i++)
                yield return Unit("Vrihedd Brigade Recruit",   f, 4, Place.FirstRow,  Abilities.wyzszeMorale);
            for (int i = 0; i < 3; i++)
                yield return Unit("Vrihedd Brigade Veteran",   f, 4, Place.SecondRow, Abilities.wyzszeMorale);
            for (int i = 0; i < 3; i++)
                yield return Unit("Havekar Smuggler",          f, 5, Place.FirstAndSecondRow, Abilities.braterstwo);
            for (int i = 0; i < 3; i++)
                yield return Unit("Havekar Healer",            f, 0, Place.FirstRow,  Abilities.wskrzeszenie);
            yield return Unit("Filavandrel aen Fidháil",       f, 6, Place.SecondRow);
            yield return Unit("Dennis Cranmer",                f, 6, Place.FirstRow,  Abilities.wyzszeMorale);
            yield return Unit("Toruviel",                      f, 2, Place.FirstRow);
            yield return Unit("Riordain",                      f, 1, Place.FirstRow);

            // Karty specjalne (każdej frakcji daje się standardowy zestaw)
            foreach (var s in BuildStandardSpecials(f)) yield return s;
        }

        private static IEnumerable<Card> BuildMonsterCards()
        {
            var f = Fractions.Monsters;

            // Dowódcy
            yield return Commander("Eredin: Bringer of Death",            f, Abilities.monsters1);
            yield return Commander("Eredin: Commander of the Red Riders", f, Abilities.monsters2);
            yield return Commander("Eredin: Destroyer of Worlds",         f, Abilities.monsters3);
            yield return Commander("Eredin: King of the Wild Hunt",       f, Abilities.monsters4);
            yield return Commander("Eredin: Treacherous",                 f, Abilities.monsters5);

            // Mistrzowie
            yield return Champion("Imlerith",    f, 10, Place.FirstRow);
            yield return Champion("Caranthir",   f, 11, Place.SecondRow);
            yield return Champion("Leshen",      f, 10, Place.SecondRow);
            yield return Champion("Toad Prince", f, 10, Place.FirstRow);
            yield return Champion("Draug",       f,  8, Place.FirstRow);
            yield return Champion("Ge'els",      f, 10, Place.SecondRow);
            yield return Champion("Kayran",      f,  8, Place.FirstAndSecondRow);
            yield return Champion("Dagon",       f,  9, Place.FirstRow);

            // Jednostki regularne (Monstery mocno bazują na braterstwie)
            for (int i = 0; i < 3; i++) yield return Unit("Arachas",  f, 4, Place.FirstRow,  Abilities.braterstwo);
            for (int i = 0; i < 3; i++) yield return Unit("Endrega",  f, 2, Place.FirstRow,  Abilities.braterstwo);
            for (int i = 0; i < 3; i++) yield return Unit("Foglet",   f, 4, Place.SecondRow, Abilities.braterstwo);
            for (int i = 0; i < 3; i++) yield return Unit("Harpy",    f, 2, Place.SecondRow, Abilities.braterstwo);
            for (int i = 0; i < 3; i++) yield return Unit("Ghoul",    f, 1, Place.FirstRow,  Abilities.braterstwo);
            for (int i = 0; i < 3; i++) yield return Unit("Nekker",   f, 2, Place.FirstRow,  Abilities.braterstwo);

            yield return Unit("Crone: Brewess",      f, 6, Place.FirstRow);
            yield return Unit("Crone: Whispess",     f, 6, Place.SecondRow);
            yield return Unit("Crone: Weavess",      f, 6, Place.ThirdRow);
            yield return Unit("Forktail",            f, 8, Place.FirstRow);
            yield return Unit("Frightener",          f, 6, Place.FirstRow);
            yield return Unit("Botchling",           f, 4, Place.FirstRow);
            yield return Unit("Cockatrice",          f, 6, Place.SecondRow);
            yield return Unit("Wyvern",              f, 6, Place.SecondRow);
            yield return Unit("Werewolf",            f, 4, Place.FirstRow);
            yield return Unit("Vampire: Bruxa",      f, 4, Place.SecondRow);
            yield return Unit("Vampire: Garkain",    f, 4, Place.FirstRow);
            yield return Unit("Vampire: Ekimmara",   f, 4, Place.FirstRow);
            yield return Unit("Vampire: Fleder",     f, 4, Place.FirstRow);
            yield return Unit("Vampire: Katakan",    f, 4, Place.FirstRow);
            yield return Unit("Fiend",               f, 6, Place.FirstRow);
            yield return Unit("Earth Elemental",     f, 6, Place.ThirdRow);
            yield return Unit("Fire Elemental",      f, 6, Place.ThirdRow);
            yield return Unit("Plague Maiden",       f, 8, Place.SecondRow);
            yield return Unit("Doppler",             f, 4, Place.FirstRow, Abilities.szpieg);

            foreach (var s in BuildStandardSpecials(f)) yield return s;
        }

        private static IEnumerable<Card> BuildStandardSpecials(Fractions f)
        {
            // 3× każdy "pogodowy" + Czyste Niebo + Róg + Manekin + Pożoga
            for (int i = 0; i < 3; i++) yield return Special("Biting Frost",       f, Abilities.trzaskajacyMroz, Place.WithoutRow);
            for (int i = 0; i < 3; i++) yield return Special("Impenetrable Fog",   f, Abilities.gestaMgla,       Place.WithoutRow);
            for (int i = 0; i < 3; i++) yield return Special("Torrential Rain",    f, Abilities.ulewnyDeszcz,    Place.WithoutRow);
            for (int i = 0; i < 3; i++) yield return Special("Clear Weather",      f, Abilities.czysteNiebo,     Place.WithoutRow);
            for (int i = 0; i < 3; i++) yield return Special("Commander's Horn",   f, Abilities.rogDowodcy,      Place.AllRows);
            for (int i = 0; i < 3; i++) yield return Special("Decoy",              f, Abilities.manekinDoCwiczen, Place.AllRows);
            for (int i = 0; i < 3; i++) yield return Special("Scorch",             f, Abilities.pozoga,          Place.WithoutRow);
        }

        private static Card Commander(string name, Fractions f, Abilities ability)
            => new Card(name, f, ability, 0, Place.WithoutRow, isChampion: false, isCommander: true, isSpecial: false);

        private static Card Champion(string name, Fractions f, int strength, Place place, Abilities ability = Abilities.brak)
            => new Card(name, f, ability, strength, place, isChampion: true, isCommander: false, isSpecial: false);

        private static Card Unit(string name, Fractions f, int strength, Place place, Abilities ability = Abilities.brak)
            => new Card(name, f, ability, strength, place, isChampion: false, isCommander: false, isSpecial: false);

        private static Card Special(string name, Fractions f, Abilities ability, Place place)
            => new Card(name, f, ability, 0, place, isChampion: false, isCommander: false, isSpecial: true);
    }
}
