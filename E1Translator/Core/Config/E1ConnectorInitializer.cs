using E1Translator.Core.AIS;
using FluentValidation;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using E1AisSender.Core.Common;
using TurnerTablet.Core.Scaffolding.Features.Ais;
using UnstableSort.Crudless.Mediator;

namespace E1Translator.Core.Config
{
    public class E1ConnectorInitializer
    {
        private readonly IIocContainer _container;
        private readonly Assembly[] _configAssemblies;
        private readonly IAISConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public E1ConnectorInitializer(IIocContainer container, Assembly[] configAssemblies, IAISConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _container = container;
            _configAssemblies = configAssemblies;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        public void Initialize()
        {
            var assemblies = _configAssemblies.ToList();
            assemblies.Add(GetType().Assembly);

            _container.Register<IAISConfiguration>(() => _configuration);
            _container.RegisterScoped<IAisSessionProvider, DefaultAisSessionProvider>();
            _container.Register(typeof(AisAppStackRequestHandler<>), assemblies.ToArray());
            _container.Register(typeof(AisContext<,>));

            _container.Register(typeof(IRequestHandler<,>), GetType().Assembly);
            _container.Register(typeof(IRequestHandler<>), GetType().Assembly);
            _container.Register(typeof(IValidator<>), GetType().Assembly);
            //_container.RegisterConditional(typeof(IRequestHandler<,>), typeof(CloseAppRequestHandler), c => !c.Handled);
            //_container.RegisterConditional(typeof(IRequestHandler<,>), typeof(AisAppStackRequestHandler<>), c => !c.Handled);
            _container.Register(() => _httpClientFactory);
        }

        public E1ConnectorInitializer UseAisSessionProvider(IAisSessionProvider aisSessionProvider, Lifestyles? lifestyle)
        {
            _container.OverrideRegistration(() => aisSessionProvider, lifestyle ?? Lifestyles.Transient);
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
        public static E1ConnectorInitializer CreateInitializer(IIocContainer container, IAISConfiguration configuration, IHttpClientFactory httpClientFactory, Assembly[] configAssemblies)
        {
            return new E1ConnectorInitializer(container, configAssemblies, configuration, httpClientFactory);
        }

        //public static E1ConnectorInitializer CreateInitializer(IAISConfiguration configuration, IHttpClientFactory httpClientFactory, Assembly[] configAssemblies)
        //{
        //    return new E1ConnectorInitializer(configAssemblies, configuration, httpClientFactory);
        //}
    }
}
