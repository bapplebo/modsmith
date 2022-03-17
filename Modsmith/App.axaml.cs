using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CommandLine;
using Modsmith.Utils;
using Modsmith.ViewModels.Interface;
using Modsmith.Views;
using Splat;
using Steamworks;
using System.Diagnostics;
using System.IO;

namespace Modsmith
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                if (!IsFirstStart())
                {
                    var dataContext = Locator.Current.GetService<IFirstStartWindowViewModel>();
                    desktop.MainWindow = new FirstStartWindow
                    {
                        DataContext = dataContext
                    };
                }
                else
                {
                    uint appId = Parser.Default.ParseArguments<LaunchArgumentsInput>(desktop.Args).MapResult(result =>
                    {
                        Debug.WriteLine($"Running with appid: {result.AppId}");
                        return result.AppId;
                    }, err =>
                    {
                        Debug.WriteLine("Failed to map the result, defaulting appId to Warhammer 2");
                        return (uint)GameUtils.Game.WARHAMMER_2;
                    });

                    if (appId == 0)
                    {
                        appId = (uint)GameUtils.Game.WARHAMMER_2;
                    }

                    try
                    {
                        SteamClient.Init(appId);
                    }
                    catch
                    {
                        Debug.WriteLine($"Couldn't init with appId {appId}, maybe Steam is closed?");
                        throw;
                    }

                    var dataContext = Locator.Current.GetService<IMainWindowViewModel>();
                    desktop.MainWindow = new MainWindow
                    {
                        DataContext = dataContext
                    };
                }
            }

            base.OnFrameworkInitializationCompleted();
        }

        private static bool IsFirstStart()
        {
            return File.Exists(DirectoryPathUtils.GetConfigurationFilePath());
        }
    }
}
