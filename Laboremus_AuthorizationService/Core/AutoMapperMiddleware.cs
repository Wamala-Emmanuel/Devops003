using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using IdentityServer4.EntityFramework.Entities;

namespace Laboremus_AuthorizationService.Core
{
    public static class AutoMapperMiddleware
    {
        public static void Configure(IMapperConfigurationExpression config)
        {
            var assemblies = new List<string>
            {
                typeof(Startup).Assembly.GetName().Name
            };
            foreach (var item in assemblies)
            {
                var assembly = Assembly.Load(item);
                var exportedTypes = assembly.GetExportedTypes();

                var external = Assembly.GetAssembly(typeof(ApiResource)).GetExportedTypes();

                var viewModels = exportedTypes
                    .Where(it => it.IsClass && it.Name.EndsWith("ViewModel"))
                    .ToList();

                var viewNames = viewModels.Select(it => it.Name);
                var modelNames = viewNames.Select(it => it.Replace("ViewModel", "")).ToList();

                var externalModels = external.Where(it => modelNames.Contains(it.Name)).ToList();

                var models = exportedTypes
                    .Where(it => it.IsClass && modelNames.Contains(it.Name))
                    .ToList();

                models.AddRange(externalModels);

                models.ForEach(model =>
                {
                    var viewName = $"{model.Name}ViewModel";
                    var viewModel = viewModels.FirstOrDefault(m => m.Name == viewName);
                    if (viewModel == null) return;
                    
                    config.CreateMap(model, viewModel);
                    config.CreateMap(viewModel, model);
                });
            }
        }
    }
}
