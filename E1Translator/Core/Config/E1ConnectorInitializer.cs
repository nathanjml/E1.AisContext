using E1Translator.Core.AIS;
using E1Translator.Core.Extensions;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using TurnerTablet.Core.Scaffolding.Features.Ais;
using UnstableSort.Crudless.Mediator;

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
        }

        public void Initialize()
        {
            var assemblies = _configAssemblies.ToList();
            assemblies.Add(GetType().Assembly);

            _container.Register<IAISConfiguration>(() => _configuration);
            _container.Register<IAisSessionProvider, DefaultAisSessionProvider>(Lifestyle.Scoped);
            _container.Register(typeof(AisAppStackRequestHandler<>), assemblies);
            _container.Register(typeof(AisContext<,>));

            _container.Register(typeof(IRequestHandler<,>), GetType().Assembly);
            _container.Register(typeof(IRequestHandler<>), GetType().Assembly);
            _container.Register(typeof(IValidator<>), GetType().Assembly);
            //_container.RegisterConditional(typeof(IRequestHandler<,>), typeof(CloseAppRequestHandler), c => !c.Handled);
            //_container.RegisterConditional(typeof(IRequestHandler<,>), typeof(AisAppStackRequestHandler<>), c => !c.Handled);
            _container.ResolveUnregisteredType += Container_ResolveUnregisteredType;
        }

        private void Container_ResolveUnregisteredType(object sender, UnregisteredTypeEventArgs e)
        {
            if(e.UnregisteredServiceType == typeof(IHttpClientFactory))
            {
                var serviceProvider = new ServiceCollection().AddHttpClient().BuildServiceProvider();
                e.Register(() => serviceProvider.GetService<IHttpClientFactory>()!);
            }
        }

        public E1ConnectorInitializer UseAisSessionProvider(IAisSessionProvider aisSessionProvider, Lifestyle? lifestyle)
        {
            _container.OverrideAisProvider(aisSessionProvider, lifestyle ?? Lifestyle.Transient);
            return this;
        }

        public E1ConnectorInitializer RegisterDataServiceRequestHandler<TAisResponse>()
        {
            _container.Register(typeof(IRequestHandler<DataServiceRequest<TAisResponse>
                , AisResponse<TAisResponse>>), typeof(AisDataServiceRequestHandler<TAisResponse>));

            return this;
        }

        public E1ConnectorInitializer RegisterDataServiceValidator<TAisResponse>()
        {
            _container.Register(typeof(IValidator<DataServiceRequest<TAisResponse>>)
                , typeof(AisDataServiceRequestValidator<TAisResponse>));

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
