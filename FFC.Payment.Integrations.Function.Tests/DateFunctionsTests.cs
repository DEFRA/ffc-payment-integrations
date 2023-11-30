using FFC.Payment.Integrations.Function.Helpers;
using Moq;
using Xunit;

namespace FFC.Payment.Integrations.Function.Tests
{
    public class DateFunctionsTests
    {
        private readonly IDateFunctions _dateFunctions;

        public DateFunctionsTests()
        {
            _dateFunctions = new DateFunctions();
        }

        [Fact]
        public void DateFunctions_UtcNow_Returns_Date_String()
        {
            var res = _dateFunctions.GetUtcNow();
            Assert.Equal("System.String", res.GetType().FullName);
        }
   }
}