using CommandLine;
using Modsmith.Services;
using Modsmith.Services.Interfaces;
using Modsmith.ViewModels;
using Modsmith.ViewModels.Interface;
using Splat;
using System;

namespace Modsmith
{
    public static class Bootstrapper
    {
        // https://dev.to/ingvarx/avaloniaui-dependency-injection-4aka
        public static void Register(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
        {
            services.RegisterLazySingleton<ISteamService>(() => new SteamService());
            services.RegisterLazySingleton<IProfileManagerService>(() => new ProfileManagerService());

            services.RegisterLazySingleton<IConfigurationService>(() => new ConfigurationService(
                resolver.GetService<IProfileManagerService>())
            );

            services.RegisterLazySingleton<IFirstStartWindowViewModel>(() => new FirstStartWindowViewModel(resolver.GetService<ISteamService>()));
            services.RegisterLazySingleton<IMainWindowViewModel>(() => new MainWindowViewModel(
                resolver.GetService<ILaunchArgumentService>(),
                resolver.GetService<ISteamService>(),
                resolver.GetService<IProfileManagerService>(),
                resolver.GetService<IConfigurationService>()
            ));
        }

        internal static void RegisterLaunchArguments(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver, ParserResult<Utils.LaunchArgumentsInput> launchArguments)
        {
            services.RegisterConstant<ILaunchArgumentService>(new LaunchArgumentService(launchArguments));
        }

    }
}
