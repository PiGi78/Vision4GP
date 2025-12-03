using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Vision4GP.Core.FileSystem;
using Microsoft.Extensions.Options;

namespace Vision4GP.Microfocus
{

    /// <summary>
    /// Extensions for configuration management
    /// </summary>
    public static class ConfigurationExtensions
    {


        /// <summary>
        /// Adds Microfocus specific services to the service collection
        /// </summary>
        /// <param name="services">Collection where to add Microfocus</param>
        /// <param name="dataConverter">Data converter to use, the default if not specified</param>
        /// <returns>Services with Microfocus</returns>
        public static IServiceCollection AddMicrofocus(this IServiceCollection services, IDataConverter? dataConverter = null)
        {
            services.AddOptions<MicrofocusSettings>().BindConfiguration(nameof(MicrofocusSettings));
            services.AddScoped<IDataConverter>((sp) =>
            {
                if (dataConverter != null)
                {
                    return dataConverter;
                }
                return new MicrofocusDataConverter();
            });
            services.AddSingleton<MicrofocusFileSystem>();
            return services;
        }


        public static IServiceCollection AddMicrofocus(this IServiceCollection services, MicrofocusSettings settings, IDataConverter? dataConverter = null)
        {
            var settingsOption = Options.Create(settings);
            services.AddSingleton<IOptions<MicrofocusSettings>>(settingsOption);
            services.AddScoped<IDataConverter>((sp) =>
            {
                if (dataConverter != null)
                {
                    return dataConverter;
                }
                return new MicrofocusDataConverter();
            });
            services.AddSingleton<MicrofocusFileSystem>();
            return services;
        }

    }
}
