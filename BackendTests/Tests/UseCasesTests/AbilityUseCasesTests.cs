using Backend.Models;
using Backend.Models.Enums;
using Backend.UseCases;
using Xunit;

namespace Backend.Tests.UseCasesTests
{
    public class AbilityUseCasesTests
    {
        private readonly AbilityUseCases abilityUseCases;

        public AbilityUseCasesTests()
        {
            abilityUseCases = new AbilityUseCases();
        }
    }
}
