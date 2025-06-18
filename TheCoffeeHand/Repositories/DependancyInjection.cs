using Interfracture.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Repositories.Repositories;

namespace Repositories
{
    public static class DependencyInjection
    {
        public static void AddRepositoryLayer(this IServiceCollection services)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        }
    }
}
