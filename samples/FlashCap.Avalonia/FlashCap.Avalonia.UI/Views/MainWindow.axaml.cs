////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kekyo@mi.kekyo.net)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FlashCap.Avalonia.Views;

public sealed partial class MainWindow : Window
{
    public MainWindow() =>
        this.InitializeComponent();

    private void InitializeComponent() =>
        AvaloniaXamlLoader.Load(this);
}
