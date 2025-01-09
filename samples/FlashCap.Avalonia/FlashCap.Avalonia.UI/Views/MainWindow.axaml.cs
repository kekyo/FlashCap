////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FlashCap.Avalonia.ViewModels;

namespace FlashCap.Avalonia.Views;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        var vm  = new MainWindowViewModel();
        vm.ParentWindow = this;
        this.DataContext = vm;
        this.InitializeComponent();
    }
        

    private void InitializeComponent() =>
        AvaloniaXamlLoader.Load(this);

    private void TopLevel_OnOpened(object sender, EventArgs e)
    {
        (DataContext as MainWindowViewModel)!.OpenedHandler(sender, e);
    }
}
