using E1Translator.Core.AIS;
using E1Translator.Core.Extensions;
using SimpleInjector;
using System.Linq;
using System.Reflection;
using TurnerTablet.Core.Scaffolding.Features.Ais;

namespace E1Translator.Core.Config
{
    public class E1ConnectorInitializer
    {
        private readonly Container _container;
        private readonly Assembly[] _configAssemblies;
        private readonly IAISConfiguration _configuration;

        public E1ConnectorInitializer(Container container, Assembly[] configAssemblies, IAISConfiguration configuration)
        {
            _container = container;
            _configAssemblies = configAssemblies;
            _configuration = configuration;

            _container.Register<IAisSessionProvider, DefaultAisSessionProvider>();
        }

        public void Initialize()
        {
            var assemblies = _configAssemblies.ToList();
            assemblies.Add(GetType().Assembly);

            _container.Register<IAISConfiguration>(() => _configuration);
            _container.Register(typeof(AisAppStackRequestHandler<>), assemblies);
            _container.Register(typeof(AisContext<,>));

            //void RegisterDataServiceRequestHandler<TAisResponse>() =>
            //    container.Register(typeof(IRequestHandler<DataServiceRequest<TAisResponse>, AisResponse<TAisResponse>>),
            //        typeof(AisDataServiceRequestHandler<TAisResponse>));

            //void RegisterDataServiceValidator<TAisResponse>() =>
            //    container.Register(typeof(IValidator<DataServiceRequest<TAisResponse>>),
            //        typeof(AisDataServiceRequestValidator<TAisResponse>));
        }

        public E1ConnectorInitializer UseAisSessionProvider(IAisSessionProvider aisSessionProvider, Lifestyle? lifestyle)
        {
            _container.OverrideAisProvider(aisSessionProvider, lifestyle ?? Lifestyle.Transient);
            return this;
        }
    }

    public static class E1Connector
    {
        public static E1ConnectorInitializer CreateInitializer(Container container, IAISConfiguration configuration, Assembly[] configAssemblies)
        {
            return new E1ConnectorInitializer(container, configAssemblies, configuration);
        }
    }
}
