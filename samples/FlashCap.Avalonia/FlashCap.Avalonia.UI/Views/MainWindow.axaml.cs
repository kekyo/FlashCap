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
using Avalonia.Threading;
using FlashCap.Avalonia.ViewModels;
using System;
using System.Diagnostics;

namespace FlashCap.Avalonia.Views;

public sealed partial class MainWindow : Window
{
    private bool closing;
    private bool readyToClose;

    public MainWindow()
    {
        this.InitializeComponent();
        this.Closing += this.OnClosingAsync;
    }

    private async void OnClosingAsync(object? sender, WindowClosingEventArgs e)
    {
        if (this.readyToClose) return;
        e.Cancel = true;
        if (this.closing) return;
        this.closing = true;
        this.IsEnabled = false;
        try
        {
            // Keep dispatching UI callbacks while the camera is being released.
            if (this.DataContext is MainWindowViewModel model)
                await model.CloseAsync();
        }
        catch (Exception exception)
        {
            Trace.WriteLine(exception);
        }
        finally
        {
            // Even synchronous cleanup must finish the current Closing event first.
            Dispatcher.UIThread.Post(() =>
            {
                this.readyToClose = true;
                this.Close();
            });
        }
    }

    private void InitializeComponent() =>
        AvaloniaXamlLoader.Load(this);
}
