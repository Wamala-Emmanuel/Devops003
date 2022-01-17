using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Laboremus_AuthorizationService.Core.Helpers
{
    public static class DependencyInjectionMiddleware
    {
        public static IServiceCollection AddDependencyInjection(this IServiceCollection services)
        {
            var skip = new HashSet<string>
            {
                "ServiceBase",
                "GenericRepository"
            };
            var assemblies = new List<string>
            {
                typeof(Startup).Assembly.GetName().Name
            };
            foreach (var item in assemblies)
            {
                var assembly = Assembly.Load(item);

                var types = assembly
                    .GetExportedTypes()
                    .Where(it => it.IsClass && !ShouldSkip(it, skip))
                    .Select(it =>
                    {
                        // get interface class, with similar name
                        var inter = it.GetInterfaces()
                            ?.Where(ot => ot.Name.Contains(it.Name))
                            .FirstOrDefault();
                        // Return a pair
                        return (_class: it, _interface: inter);
                    })
                    //Remove types without interfaces
                    .Where(it => it._interface != null);

                foreach (var (_class, _interface) in types)
                {
                    Console.WriteLine($"Service: {_class.FullName} impl:{_interface.FullName}");
                    services.AddScoped(_interface, _class);
                }
            }

            return services;
        }

        private static bool ShouldSkip(MemberInfo type, IEnumerable<string> exclude)
        {
            var exists = exclude.Any(it => type.Name.Contains(it));
            return exists;
        }
    }
}
