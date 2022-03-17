using Avalonia;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Modsmith.ViewModels;
using ReactiveUI;
using System;
using System.Diagnostics;

namespace Modsmith.Views
{
    public class LoadProfileWindow : ReactiveWindow<LoadProfileWindowViewModel>
    {
        public LoadProfileWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            this.WhenActivated(d => d(ViewModel!.LoadProfileCommand.Subscribe(Close)));
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void CancelButtonClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void HandleDoubleTap(object sender, RoutedEventArgs e)
        {
            var result = ViewModel.HandleDoubleTap();
            Close(result);
        }
    }
}
