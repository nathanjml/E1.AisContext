using E1Translator.Core.AIS;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Text;

namespace E1Translator.Core.Extensions
{
    public static class ContainerExtensions
    {
        public static Container OverrideAisProvider(this Container container,  IAisSessionProvider aisSessionProvider, Lifestyle lifestyle)
        {
            container.Options.AllowOverridingRegistrations = true;
            container.Register<IAisSessionProvider>(() => aisSessionProvider, lifestyle);
            container.Options.AllowOverridingRegistrations = false;

            return container;
        }
    }
}
