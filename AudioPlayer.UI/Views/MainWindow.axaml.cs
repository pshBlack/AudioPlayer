using Avalonia.Controls;
using Avalonia.Interactivity;
using AudioPlayer.UI.ViewModels;
namespace AudioPlayer.UI.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
    private void OnTrackDoubleTapped(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
            vm.PlayPauseCommand.Execute(null);
    }
}