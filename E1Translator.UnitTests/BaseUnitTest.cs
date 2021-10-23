using SimpleInjector;

namespace E1Translator.UnitTests
{
    public abstract class BaseUnitTest
    {
        protected Container Container => UnitTestSetup.Container;
    }
}
