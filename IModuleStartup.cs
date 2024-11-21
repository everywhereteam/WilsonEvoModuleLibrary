using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace WilsonEvoModuleLibrary
{
    public enum ModuleEnvironment
    {
        Debug,
        Production
    };
    public interface IModuleStartup
    {
        void ConfigureServices(IServiceCollection services, IConfiguration configuration, ModuleEnvironment environment);
        void Configure(IApplicationBuilder app, IWebHostEnvironment env, ModuleEnvironment environment);
    }
}
