using Avalonia.Controls;
using Modsmith.Models;
using Modsmith.Services.Interfaces;
using Modsmith.Utils;
using Modsmith.ViewModels.Interface;
using Newtonsoft.Json;
using ReactiveUI;
using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Input;

namespace Modsmith.ViewModels
{
    class FirstStartWindowViewModel : ViewModelBase, IFirstStartWindowViewModel
    {
        public ICommand OnClickCloseCommand { get; }

        private readonly ISteamService _steamService;

        private string mainText = "Searching for supported games...";
        public string MainText { get => mainText; set => this.RaiseAndSetIfChanged(ref mainText, value); }

        public FirstStartWindowViewModel(ISteamService steamService)
        {
            _steamService = steamService;
            _steamService.Test();
            SteamClient.Init(480);

            OnClickCloseCommand = ReactiveCommand.Create<Window>(this.CloseWindow);

            if (!_steamService.IsSteamRunning())
            {
                MainText = "Modsmith currently only supports Steam and Steam needs to be running.";
            }

            BootstrapConfigurationFiles();
            CreateConfigurationFile();
            // todo - show message and confirm window when we're done looking for games. shouldn't take too long, anyway
            SteamClient.Shutdown();
            MainText = "First-time configuration complete. Please exit and restart Modsmith to use the mod manager.";
        }

        private void CloseWindow(Window window)
        {
            if (window != null)
            {
                window.Close();
            }
        }

        private void BootstrapConfigurationFiles()
        {
            // %APPDATA%/modsmith

            // todo - put in a static class
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "modsmith");
            Directory.CreateDirectory(path);
            // Create a directory to hold user profiles
            Directory.CreateDirectory(Path.Combine(path, "profiles"));
        }

        private void CreateConfigurationFile()
        {
            var newConfig = new ModsmithConfiguration
            {
                gameConfig = new GameConfig
                {
                    games = new List<Game>()
                }
            };

            var validAppIds = new List<uint> { (uint)GameUtils.Game.THREE_KINGDOMS, (uint)GameUtils.Game.WARHAMMER_2 };
            foreach (var appId in validAppIds)
            {
                var installDir = SteamApps.AppInstallDir(appId);
                newConfig.gameConfig.games.Add(new Game
                {
                    appId = appId,
                    directoryPath = installDir,
                    exe = DirectoryPathUtils.GetExecutableFromAppId(appId)
                });
            }

            var jsonFile = JsonConvert.SerializeObject(newConfig, Formatting.Indented);
            File.WriteAllText(DirectoryPathUtils.GetConfigurationFilePath(), jsonFile);
        }
    }
}
