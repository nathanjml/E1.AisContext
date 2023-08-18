using System;
using System.Reflection;
using SimpleInjector;

namespace E1AisSender.Core.Common
{
    public enum Lifestyles {
        Transient,
        Scoped,
        Singleton
    }

    public interface IIocContainer
    {
        void Register<TService, TImplementation>() where TImplementation : class, TService where TService : class;

        void Register<TService>(Func<TService> instanceCreator) where TService : class;
        void Register(Type serviceType, params Assembly[] assemblies);
        void Register(Type concreteType);
        void Register(Type serviceType, Type concreteType);

        void RegisterScoped<TService, TImplementation>() where TImplementation : class, TService where TService : class;
        void RegisterScoped<TService>(Func<TService> instanceCreator) where TService : class;
        void RegisterScoped(Type serviceType, params Assembly[] assemblies);
        void RegisterScoped(Type serviceType, Type concreteType);


        void RegisterSingleton<TService, TImplementation>() where TImplementation : class, TService where TService : class;
        void RegisterSingleton<TService>(Func<TService> instanceCreator) where TService : class;
        void RegisterSingleton(Type serviceType, params Assembly[] assemblies);
        void RegisterSingleton(Type serviceType, Type concreteType);



        TService GetInstance<TService>() where TService : class;
        void OverrideRegistration<TService>(Func<TService> instanceCreator, Lifestyles lifestyle = Lifestyles.Transient) where TService : class;
    }

    public class SimpleInjectorContainerAdapter : IIocContainer
    {
        private readonly Container _container;

        public SimpleInjectorContainerAdapter(Container container)
        {
            _container = container;
        }
        public void Register<TService, TImplementation>() where TImplementation : class, TService where TService : class
        {
            _container.Register<TService, TImplementation>();
        }

        public void Register<TService>(Func<TService> instanceCreator) where TService : class
        {
            _container.Register<TService>(instanceCreator);
        }

        public void Register(Type serviceType, params Assembly[] assemblies)
        {
            _container.Register(serviceType, assemblies);
        }

        public void Register(Type concreteType)
        {
            _container.Register(concreteType);
        }

        public void Register(Type serviceType, Type concreteType)
        {
            _container.Register(serviceType, concreteType);
        }

        public void RegisterScoped<TService, TImplementation>() where TService : class where TImplementation : class, TService
        {
            _container.Register<TService, TImplementation>(Lifestyle.Scoped);
        }

        public void RegisterScoped<TService>(Func<TService> instanceCreator) where TService : class
        {
            _container.Register(instanceCreator, Lifestyle.Scoped);
        }

        public void RegisterScoped(Type serviceType, params Assembly[] assemblies)
        {
            _container.Register(serviceType, assemblies, Lifestyle.Scoped);
        }

        public void RegisterScoped(Type serviceType, Type concreteType)
        {
            _container.Register(serviceType, concreteType, Lifestyle.Scoped);
        }

        public void RegisterSingleton<TService, TImplementation>() where TService : class where TImplementation : class, TService
        {
            _container.Register<TService, TImplementation>(Lifestyle.Singleton);
        }

        public void RegisterSingleton<TService>(Func<TService> instanceCreator) where TService : class
        {
            _container.Register(instanceCreator, Lifestyle.Singleton);

        }

        public void RegisterSingleton(Type serviceType, params Assembly[] assemblies)
        {
            _container.Register(serviceType, assemblies, Lifestyle.Singleton);
        }

        public void RegisterSingleton(Type serviceType, Type concreteType)
        {
            _container.Register(serviceType, concreteType, Lifestyle.Singleton);
        }

        public TService GetInstance<TService>() where TService : class
        {
            return _container.GetInstance<TService>();
        }

        public void OverrideRegistration<TService>(Func<TService> instanceCreator, Lifestyles lifestyle = Lifestyles.Transient) where TService : class
        {
            _container.Options.AllowOverridingRegistrations = true;
            switch (lifestyle)
            {
                case Lifestyles.Transient:
                    Register(instanceCreator);
                    break;
                case Lifestyles.Scoped:
                    RegisterScoped(instanceCreator);
                    break;
                case Lifestyles.Singleton:
                    RegisterSingleton(instanceCreator);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(lifestyle), lifestyle, "Lifestyle not supported");
            }

            _container.Options.AllowOverridingRegistrations = false;
        }
    }
}
