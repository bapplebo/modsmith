using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Modsmith.Services.Interfaces;
using Modsmith.Utils;
using Modsmith.ViewModels;
using ReactiveUI;
using Splat;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Modsmith.Views
{
    public class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            this.WhenActivated(d => d(ViewModel!.ShowSaveProfileDialog.RegisterHandler(DoShowSaveProfile)));
            this.WhenActivated(d => d(ViewModel!.ShowLoadProfileDialog.RegisterHandler(DoShowLoadProfile)));
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void WindowClosing(object sender, CancelEventArgs e)
        {
            // cleanup the app_id
            var workingDirectory = Directory.GetCurrentDirectory();
            var steamAppIdFilePath = @$"{workingDirectory}\steam_appid.txt";
            if (File.Exists(steamAppIdFilePath))
            {
                Debug.WriteLine("Deleting: " + steamAppIdFilePath);
                File.Delete(steamAppIdFilePath);
            }

            // pull the launch argument stuff from the ether so I can get the app id
            var launchArgumentService = Locator.Current.GetService<ILaunchArgumentService>();
            var launchArgumentAppId = launchArgumentService.GetLaunchArgAppId();
            var userScriptPath = Path.Combine(DirectoryPathUtils.GetTotalWarScriptPath(launchArgumentAppId), "user.script.txt");
            if (File.Exists(userScriptPath))
            {
                Debug.WriteLine("Deleting mod list at: " + userScriptPath);
                File.Delete(userScriptPath);
            }
        }

        private async Task DoShowSaveProfile(InteractionContext<SaveProfileViewModel, string?> interaction)
        {
            var dialog = new SaveProfileWindow
            {
                DataContext = interaction.Input
            };

            var result = await dialog.ShowDialog<string>(this);
            interaction.SetOutput(result);
        }

        private async Task DoShowLoadProfile(InteractionContext<LoadProfileWindowViewModel, string?> interaction)
        {
            var dialog = new LoadProfileWindow
            {
                DataContext = interaction.Input
            };

            var result = await dialog.ShowDialog<string>(this);
            interaction.SetOutput(result);
        }
    }
}
