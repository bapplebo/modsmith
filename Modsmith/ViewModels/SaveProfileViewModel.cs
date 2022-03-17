using ReactiveUI;
using System;
using System.Diagnostics;
using System.Reactive;

namespace Modsmith.ViewModels
{
    public class SaveProfileViewModel : ViewModelBase
    {
        public ReactiveCommand<Unit, string> SaveProfileCommand { get; }

        private bool _buttonsEnabled;
        public bool ButtonsEnabled { get => _buttonsEnabled; set => this.RaiseAndSetIfChanged(ref _buttonsEnabled, value); }

        private string _newProfileName;
        public string NewProfileName { get => _newProfileName; set => this.RaiseAndSetIfChanged(ref _newProfileName, value); }


        public SaveProfileViewModel()
        {
            ButtonsEnabled = true;

            SaveProfileCommand = ReactiveCommand.Create(() =>
            {
                if (!string.IsNullOrEmpty(_newProfileName?.Trim()))
                {
                    return _newProfileName;
                }
                else
                {
                    throw new Exception("Button should be disabled, so returning should be impossible");
                }
            });

            this.WhenAnyValue(window => window.NewProfileName).Subscribe(value =>
            {
                if (string.IsNullOrEmpty(value?.Trim()))
                    ButtonsEnabled = false;
                else
                    ButtonsEnabled = true;
            });
        }
    }
}
