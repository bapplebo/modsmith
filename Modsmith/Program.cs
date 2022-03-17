using Avalonia;
using Avalonia.ReactiveUI;
using CommandLine;
using Modsmith.Utils;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.FontAwesome;
using Splat;
using System;
using System.Diagnostics;

namespace Modsmith
{
    internal class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            var launchArguments = Parser.Default.ParseArguments<LaunchArgumentsInput>(args);

            Bootstrapper.RegisterLaunchArguments(Locator.CurrentMutable, Locator.Current, launchArguments);
            Bootstrapper.Register(Locator.CurrentMutable, Locator.Current);

            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace()
                .WithIcons(container => container.Register<FontAwesomeIconProvider>())
                .UseReactiveUI();
    }
}
