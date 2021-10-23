using NUnit.Framework;

namespace E1Translator.UnitTests
{
    [SetUpFixture]
    public class OneTimeSetup
    {
        [OneTimeSetUp]
        public static void OneTimeSetupRoutine()
        {
            UnitTestSetup.SetUp();
        }
    }
}
