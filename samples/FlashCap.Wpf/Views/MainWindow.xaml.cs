////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using FlashCap.Wpf.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace FlashCap.Wpf.Views;

public sealed partial class MainWindow : Window
{
    private MainWindowViewModel? viewModel { get; set; }
    public MainWindow()
    {
        InitializeComponent();

        viewModel = DataContext as MainWindowViewModel;
    }

    private void ShowPropertiesClicked(object sender, RoutedEventArgs e)
    {
        viewModel?.ShowProperties();
    }

    private void StartClicked(object sender, RoutedEventArgs e)
    {
        viewModel?.Start();
    }

    private void StopClicked(object sender, RoutedEventArgs e)
    {
        viewModel?.Stop();
    }
}
