using E1Translator.Core.AIS;
using E1Translator.Core.Config;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace E1Translator.UnitTests
{
    public class UnitTestSetup
    {

        public static Container Container { get; private set; }

        public static Container SetUp(params Assembly[] coreAssemblies)
        {
            var container = new Container();
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            var assemblyList = new List<Assembly> { typeof(UnitTestSetup).Assembly };
            assemblyList.AddRange(coreAssemblies);

            var initializer = new E1ConnectorInitializer(container, assemblyList.ToArray(), new TestConfiguration());
            initializer.UseAisSessionProvider(NSubstitute.Substitute.For<IAisSessionProvider>(), Lifestyle.Scoped)
                .Initialize();

            //container.ConfigureMediator(new[] { Assembly.GetExecutingAssembly() });

            Container = container;

            return container;
        }
    }

    public class TestConfiguration : IAISConfiguration
    {
        public string AisBaseUrl => "N/A";

        public string AisEnvironment => "UNIT_TEST";

        public string AisRole => "TEST";

        public string AisUsername => "UNIT";

        public string AisPassword => "TEST";
    }
}