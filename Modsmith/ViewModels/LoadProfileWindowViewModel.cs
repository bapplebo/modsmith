using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Splat;
using Modsmith.Services.Interfaces;
using System.Reactive;

namespace Modsmith.ViewModels
{
    // This helper class exists as styling the font-size for each listbox item as a string seems to not work?
    public class LoadProfileValue
    {
        public string Value { get; set; }
    }
    public class LoadProfileWindowViewModel : ViewModelBase
    {
        public ReactiveCommand<Unit, string> LoadProfileCommand { get; }
        private List<LoadProfileValue> _availableProfiles;
        public List<LoadProfileValue> AvailableProfiles { get => _availableProfiles; set => this.RaiseAndSetIfChanged(ref _availableProfiles, value); }
        private bool _buttonsEnabled;
        public bool ButtonsEnabled { get => _buttonsEnabled; set => this.RaiseAndSetIfChanged(ref _buttonsEnabled, value); }

        private int _selectedIndex;
        public int SelectedIndex { get => _selectedIndex; set => this.RaiseAndSetIfChanged(ref _selectedIndex, value); }

        public LoadProfileWindowViewModel(uint currentAppId)
        {
            // todo - inject this properly once we're done. Need to do it like this so i can see the visual designer, since ctor args seem to break it?
            var profileManagerService = Locator.Current.GetService<IProfileManagerService>();
            var profiles = profileManagerService.GetProfileNamesForGame(currentAppId);
            AvailableProfiles = profiles.Select(profile => new LoadProfileValue { Value = profile }).ToList();

            if (AvailableProfiles.Count == 0)
            {
                ButtonsEnabled = false;
            }
            else
            {
                ButtonsEnabled = true;
            }

            LoadProfileCommand = ReactiveCommand.Create(() =>
            {
                if (_availableProfiles is not null && _availableProfiles[_selectedIndex] is not null)
                {
                    return _availableProfiles[_selectedIndex].Value;
                }
                else
                {
                    throw new Exception("No profiles found, but the button should not be clickable anyhow.");
                }
            });
        }

        public string HandleDoubleTap()
        {
            return _availableProfiles[_selectedIndex].Value;
        }
    }
}
