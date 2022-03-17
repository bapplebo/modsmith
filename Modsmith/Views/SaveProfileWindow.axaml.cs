using Avalonia;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Modsmith.ViewModels;
using ReactiveUI;
using System;

namespace Modsmith.Views
{
    public class SaveProfileWindow : ReactiveWindow<SaveProfileViewModel>
    {
        public SaveProfileWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            this.WhenActivated(d => d(ViewModel!.SaveProfileCommand.Subscribe(Close)));
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void CancelButtonClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
