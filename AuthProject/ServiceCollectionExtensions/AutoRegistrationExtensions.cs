using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Force;
using Microsoft.Extensions.DependencyInjection;

namespace AuthProject.ServiceCollectionExtensions
{
    public static class AutoRegistrationExtensions
    {
        private static IEnumerable<Assembly> Assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();

        private static Type[] BaseTypes = {typeof(IHandler<,>)};

        public static IServiceCollection AutoRegistration(this IServiceCollection serviceCollection)
        {
            return serviceCollection.Scan(x => x.FromAssemblies(Assemblies)
                .AddClasses(xx => xx.AssignableToAny(BaseTypes))
                .AsSelf()
                .AsImplementedInterfaces()
                .WithScopedLifetime());
        }
    }
}