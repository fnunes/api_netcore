using Api.Domain.Interfaces.Services.User;
using Api.Service.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Api.CrossCutting.DependencyInjection
{
    public class ConfigureService
    {
        public static void ConfigureDependenciesService(IServiceCollection serviceCollection)
        {
            //AddTransient: Para cada injeção cria uma instância
            //AddScoped: Cria uma instância para cada scope (uma vez por ciclo de vida)
            //AddSingleton: Cria uma instância para toda a aplicação, independente de session (uma vez a nivel de servidor)
            serviceCollection.AddTransient<IUserService, UserService>();
            serviceCollection.AddTransient<ILoginService, LoginService>();
        }
    }
}
