using Modsmith.Models;
using Modsmith.Services.Interfaces;
using Modsmith.Utils;
using Modsmith.ViewModels.Interface;
using Newtonsoft.Json;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Reactive.Linq;

namespace Modsmith.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, IMainWindowViewModel
    {
        public ICommand LaunchGameCommand { get; }
        public ICommand RefreshModListCommand { get; }
        public ICommand MoveModUp { get; }
        public ICommand MoveModDown { get; }
        public ICommand LoadProfileCommand { get; }
        public ICommand SaveProfileCommand { get; }
        public Interaction<SaveProfileViewModel, string> ShowSaveProfileDialog { get; }
        public Interaction<LoadProfileWindowViewModel, string> ShowLoadProfileDialog { get; }

        private bool _filteredView;
        public bool FilteredView { get => _filteredView; set => this.RaiseAndSetIfChanged(ref _filteredView, value); }

        private string _modFilterText;
        public string ModFilterText { get => _modFilterText; set => this.RaiseAndSetIfChanged(ref _modFilterText, value); }

        private bool buttonsEnabled;
        public bool ButtonsEnabled { get => buttonsEnabled; set => this.RaiseAndSetIfChanged(ref buttonsEnabled, value); }

        // todo - add a dictionary to handle the mods that we'll display. we might be able to merge it into Mods, though.

        private ObservableCollection<Mod> _mods;
        private ObservableCollection<Mod> _fullModList;

        public ObservableCollection<Mod> Mods { get => _mods; set => this.RaiseAndSetIfChanged(ref _mods, value); }

        private string? _currentGame;

        public string CurrentGame { get => _currentGame; set => this.RaiseAndSetIfChanged(ref _currentGame, value); }

        // todo - figure out if i can share this or not
        private readonly Encoding utf8WithoutBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
        private int selectedListBoxModIndex;
        public int SelectedListBoxModIndex
        {
            get => selectedListBoxModIndex;
            set => this.RaiseAndSetIfChanged(ref selectedListBoxModIndex, value);
        }

        private readonly ILaunchArgumentService _launchArgumentService;
        private readonly ISteamService _steamService;
        private readonly IProfileManagerService _profileManagerService;
        private readonly IConfigurationService _configurationService;
        private uint _currentAppId;
        private string _selectedProfile;

        public MainWindowViewModel(
                ILaunchArgumentService launchArgumentService,
                ISteamService steamService,
                IProfileManagerService profileManagerService,
                IConfigurationService configurationService
            )
        {
            ShowSaveProfileDialog = new Interaction<SaveProfileViewModel, string>();
            ShowLoadProfileDialog = new Interaction<LoadProfileWindowViewModel, string>();

            _launchArgumentService = launchArgumentService;
            _steamService = steamService;
            _profileManagerService = profileManagerService;
            _configurationService = configurationService;

            var currentAppId = _launchArgumentService.GetLaunchArgAppId();
            // It's unlikely that this will be mutated in the future, but keep an eye on it, just in case. We plan to just reboot the app if the user changes
            // Creating a mutatable CurrentGameService or something might be handy in the future. Or I can just pass the argument through.
            _currentAppId = currentAppId;
            _currentGame = GameUtils.AppIdToPrettyName(currentAppId);
            ButtonsEnabled = true;

            var profiles = _profileManagerService.GetProfileNamesForGame(_currentAppId);

            _fullModList = new ObservableCollection<Mod>();
            Mods = new ObservableCollection<Mod>();

            LoadMods(currentAppId).ContinueWith(t =>
            {
                try
                {
                    var lastUsedProfile = _configurationService.GetLastUsedProfileForGame(_currentAppId);
                    // Check the mods that exist in the profile. We do this rather than pass the profile in as mods may have been deleted / updated since the last profile refresh
                    SelectModsInProfile(lastUsedProfile);
                }
                catch
                {
                    // Use better error handling if there's no profile
                    Debug.WriteLine("no profiles exist");
                }

            });

            // todo - remove

            // todo - neatly handle when there's no mods
            LaunchGameCommand = ReactiveCommand.Create(() =>
            {
                ButtonsEnabled = false;
                LaunchGame();
                ButtonsEnabled = true;
            });

            RefreshModListCommand = ReactiveCommand.Create(async () =>
            {
                ButtonsEnabled = false;
                await LoadMods();
                ButtonsEnabled = true;
            });

            MoveModUp = ReactiveCommand.Create(() =>
            {
                if (selectedListBoxModIndex == 0)
                {
                    return;
                }

                // Store it in a temp, as our index seems to be decrementing
                var tmpSelectedIndex = selectedListBoxModIndex;
                (Mods[selectedListBoxModIndex], Mods[selectedListBoxModIndex - 1]) = (Mods[selectedListBoxModIndex - 1], Mods[selectedListBoxModIndex]);
                SelectedListBoxModIndex = tmpSelectedIndex - 1;
            });

            MoveModDown = ReactiveCommand.Create(() =>
            {
                if (selectedListBoxModIndex == Mods.Count - 1)
                {
                    return;
                }

                // Store it in a temp, as our index seems to be decrementing
                var tmpSelectedIndex = selectedListBoxModIndex;
                (Mods[selectedListBoxModIndex], Mods[selectedListBoxModIndex + 1]) = (Mods[selectedListBoxModIndex + 1], Mods[selectedListBoxModIndex]);
                SelectedListBoxModIndex = tmpSelectedIndex + 1;
            });

            SaveProfileCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                await OnSaveProfile();
            });

            LoadProfileCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                await OnLoadProfile();
            });

            this.WhenAnyValue(window => window.ModFilterText).Subscribe(HandleFilterChange);
            // .Throttle(TimeSpan.FromSeconds(0.1))

        }

        public void InitWithAppId(uint appId)
        {
            Debug.WriteLine("InitWithAppId");
            Debug.WriteLine(appId);
            if (appId == 779340)
            {
                // set property, not the private field
                CurrentGame = "Total War: Three Kingdoms";
            }

            if (appId == 594570)
            {
                CurrentGame = "Total War: Warhammer II";
            }
        }

        private void WriteUserScript(List<Mod> selectedMods)
        {
            var scriptDirectory = DirectoryPathUtils.GetTotalWarScriptPath(_currentAppId);
            var scriptPath = Path.Combine(scriptDirectory, "user.script.txt");

            // Clear the old configuration
            File.Delete(scriptPath);

            if (selectedMods.Count > 0)
            {
                using var writer = new StreamWriter(scriptPath, append: false, utf8WithoutBom);
                foreach (var mod in selectedMods)
                {
                    writer.WriteLine($"mod \"{mod.Filename}\";");
                }
                writer.Close();
            }
        }

        private async Task LoadMods(uint appId = (uint)GameUtils.Game.THREE_KINGDOMS)
        {
            // CopyFilesFromWorkshopDirectoryToData()
            // todo - store all of these in local variable, or make functions to retrieve them - also put in ConfigurationService
            var config = JsonConvert.DeserializeObject<ModsmithConfiguration>(File.ReadAllText(DirectoryPathUtils.GetConfigurationFilePath()));
            var gameConfig = config.gameConfig.games.Find(game => game.appId == appId);
            var steamRootIndex = gameConfig.directoryPath.IndexOf("common");
            var steamContent = @$"{gameConfig.directoryPath[..steamRootIndex]}workshop\content\{appId}";

            //Debug.WriteLine(steamContent);
            if (!Directory.Exists(steamContent))
            {
                return;
            }

            // Workshop ID List
            var workshopIdSubdirectoryList = new List<string>(Directory.EnumerateDirectories(steamContent, "*.*", SearchOption.AllDirectories));

            var mods = new List<Mod>();

            foreach (var workshopIdSubdirectory in workshopIdSubdirectoryList)
            {
                var subdirectoryId = workshopIdSubdirectory.Substring(workshopIdSubdirectory.LastIndexOf(@"\") + 1);
                var files = new List<string>(Directory.EnumerateFiles(workshopIdSubdirectory));
                if (files.Count < 1)
                {
                    try
                    {
                        Debug.WriteLine("Delete this folder since it's useless to us now.");
                        Directory.Delete(workshopIdSubdirectory);
                    }
                    catch
                    {
                        Debug.WriteLine($"Error deleting folder {workshopIdSubdirectory}");
                    }
                }
                else
                {
                    foreach (var file in files)
                    {
                        if (file.EndsWith(".bin"))
                        {
                            continue;
                        }

                        // CopyFileIfNewer()
                        var modFilename = file.Substring(file.LastIndexOf(@"\") + 1);
                        var existingDataFile = @$"{gameConfig.directoryPath}\{modFilename}";
                        // Debug.WriteLine(existingDataFile);
                        using (File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            //File.Copy(file, existingDataFile);
                        }

                        if (file.EndsWith(".pack"))
                        {
                            mods.Add(new Mod
                            {
                                Filename = modFilename,
                                SteamId = subdirectoryId
                            });
                        }
                    }
                }
            }

            var allIds = workshopIdSubdirectoryList.Select(subdirectory =>
            {
                var stringId = subdirectory.Substring(subdirectory.LastIndexOf(@"\") + 1);
                return Convert.ToUInt32(stringId);
            });

            var modListWithWorkshopInfo = await _steamService.BulkFetchWorkshopInfo(allIds.ToList());
            var modListWithFileName = mods.Join(modListWithWorkshopInfo, mod => mod.SteamId, workshopInfo => workshopInfo.SteamId, (mod, workshopInfo) =>
            {
                workshopInfo.Filename = mod.Filename;
                return workshopInfo;
            });

            // todo - unpack the pack file to get the mod type (normal, movie etc)

            Mods = new ObservableCollection<Mod>(modListWithFileName);
            _fullModList = SystemExtension.Clone(Mods);
        }

        private void SelectModsInProfile(Profile profile)
        {
            // todo - can probably join these in one for loop, but cbb right now
            // Unselect all of the mods
            foreach (var mod in Mods)
            {
                mod.Checked = false;
            }

            // Check the ones that should be checked 
            foreach (var profileMod in profile.Mods)
            {
                try
                {
                    var mod = Mods.First(mod => mod.SteamId == profileMod.SteamId);
                    if (mod is not null)
                    {
                        mod.Checked = true;
                    }
                }
                catch (Exception ex)
                {
                    // Pass over exceptions
                    Debug.WriteLine(ex.ToString());
                }
            }

            for (var i = profile.Mods.Count - 1; i >= 0; i--)
            {
                var mod = profile.Mods[i];
                // Move the checked mods to the top, ensuring order is preserved. Do this by starting at the bottom, then inserting at the front.
                var modIndex = Mods.ToList().FindIndex(m => m.SteamId == mod.SteamId);
                Mods.RemoveAt(modIndex);
                Mods.Insert(0, mod);
            }

            // This is stupid but it works. Do it properly in the future: https://stackoverflow.com/questions/1427471/observablecollection-not-noticing-when-item-in-it-changes-even-with-inotifyprop
            var temp = SystemExtension.Clone(Mods);
            Mods.Clear();
            Mods = temp;
            _fullModList = temp;
        }

        private async Task OnSaveProfile()
        {
            var saveProfile = new SaveProfileViewModel();
            var newProfileName = await ShowSaveProfileDialog.Handle(saveProfile);
            if (!string.IsNullOrEmpty(newProfileName))
            {
                _profileManagerService.SaveProfile(_currentAppId, newProfileName, GetSelectedMods());
            }
        }

        private async Task OnLoadProfile()
        {
            ButtonsEnabled = false;
            var loadProfileWindow = new LoadProfileWindowViewModel(_currentAppId);
            var result = await ShowLoadProfileDialog.Handle(loadProfileWindow);
            if (!string.IsNullOrEmpty(result))
            {
                var profile = _profileManagerService.LoadProfile(_currentAppId, result);
                if (profile is not null)
                {
                    SelectModsInProfile(profile);
                    _selectedProfile = profile.Name;
                }
            }

            ButtonsEnabled = true;
        }

        private List<Mod> GetSelectedMods()
        {
            return _mods.Where(mod => mod.Checked).ToList();
        }

        private void LaunchGame()
        {
            // Parse our mods and put them into user.script.txt
            var selectedMods = GetSelectedMods();

            // Skip mod launcher by running the game in the background, then launching the .exe
            WriteUserScript(selectedMods);

            const string LastUsedModsProfileName = "Last used mods";
            _profileManagerService.SaveProfile(_currentAppId, LastUsedModsProfileName, selectedMods);

            if (_selectedProfile != LastUsedModsProfileName && !string.IsNullOrEmpty(_selectedProfile))
            {
                _configurationService.SetLastUsedProfileForGame(_selectedProfile, _currentAppId);
            }
            else
            {
                _configurationService.SetLastUsedProfileForGame(LastUsedModsProfileName, _currentAppId);
            }

            // todo - Create helper functions for this
            var configuration = _configurationService.LoadConfiguration();

            var workingDirectory = Directory.GetCurrentDirectory();
            var gameConfig = configuration.gameConfig.games.First(game => game.appId == _currentAppId);
            var gameDirectory = gameConfig.directoryPath;
            var exeName = gameConfig.exe;

            //Console.WriteLine(workingDirectory);
            using (var writer = new StreamWriter(@$"{workingDirectory}\steam_appid.txt", append: false, utf8WithoutBom))
            {
                writer.WriteLine(_currentAppId);
                writer.Close();
            }

            var launchGameProcess = new Process();
            launchGameProcess.StartInfo.Arguments = "";
            launchGameProcess.StartInfo.FileName = Path.Combine(gameDirectory, exeName);
            launchGameProcess.StartInfo.WorkingDirectory = gameDirectory;
            launchGameProcess.Start();
        }

        private void HandleFilterChange(string text)
        {
            if (string.IsNullOrEmpty(text?.Trim()))
            {
                FilteredView = false;
                Mods = _fullModList;
            }
            else
            {
                FilteredView = true;
                var extractedValues = FuzzySharp.Process
                    .ExtractAll(text, _fullModList.Select(mod => mod.Name), cutoff: 40)
                    .OrderByDescending(term => term.Score);

                var filteredModListTerms = extractedValues
                    .Select(extractedTerm => extractedTerm.Value)
                    .ToList();

                if (filteredModListTerms is not null)
                {
                    var filteredModList = new ObservableCollection<Mod>();
                    foreach (var term in filteredModListTerms)
                    {
                        var mod = _fullModList.First(mod => mod.Name == term);
                        filteredModList.Add(mod);
                    }
                    Mods = new ObservableCollection<Mod>(filteredModList);
                    Debug.WriteLine(filteredModList);
                }
            }
        }
    }
}
