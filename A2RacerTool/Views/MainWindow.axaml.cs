using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using A2RacerTool.ViewModels;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia.Models;

namespace A2RacerTool.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async Task<string?>OpenFilePicker(FilePickerOpenOptions options)
    {
        var topLevel = GetTopLevel(this);
        var storageProvider = topLevel?.StorageProvider;
        var files = await storageProvider?.OpenFilePickerAsync(options)!;
        if (files.Count <= 0) return null;
        var path = files[0].Path.LocalPath;
        return path;
    }

    private async Task<string?> OpenFolderPicker(FolderPickerOpenOptions options)
    {
        var topLevel = GetTopLevel(this);
        var storageProvider = topLevel?.StorageProvider;
        var folders = await storageProvider?.OpenFolderPickerAsync(options)!;
        if (folders.Count <= 0) return null;
        var path = folders[0].Path.LocalPath;
        return path;
    }

    private async void RcsPathFilePickerButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var options = new FilePickerOpenOptions
        {
            Title = "Path to rcs.ind",
            AllowMultiple = false,
            SuggestedFileName = "rcs.ind"
            
        };
        var path = await OpenFilePicker(options);
        ((MainWindowViewModel)DataContext!).SetRcsIndPath(path);
    }

    private async void OutPathFilePickerButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var options = new FolderPickerOpenOptions() 
        {
            Title = "Output folder",
            AllowMultiple = true,
            
        };
        var path = await OpenFolderPicker(options);
        ((MainWindowViewModel)DataContext!).SetOutPath(path);
    }
    
    private async void PackPathFilePickerButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var options = new FolderPickerOpenOptions 
        {
            Title = "Path to pack",
            AllowMultiple = false,
            SuggestedFileName = "pack"
            
        };
        var path = await OpenFolderPicker(options);
        ((MainWindowViewModel)DataContext!).SetPackPath(path);
    }
    
    private async void ExtractedPathFilePickerButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var options = new FolderPickerOpenOptions 
        {
            Title = "Path to extracted",
            AllowMultiple = false,
            SuggestedFileName = "extracted"
            
        };
        var path = await OpenFolderPicker(options);
        ((MainWindowViewModel)DataContext!).SetPathToExtracted(path);
    }

    private async void CompileButton_OnClick(object? sender, RoutedEventArgs e)
    {
       var viewModel = (MainWindowViewModel)DataContext!;
       var filesCompiled = await viewModel.Compile();
       if (!filesCompiled) return;
        var msgBoxParams = new MessageBoxCustomParams()
        {
            ContentTitle = "Files packed succesfully!",
            ContentMessage = $"Wrote 2 files to {viewModel.PackPath}",
            ButtonDefinitions = new List<ButtonDefinition>
            {
                new() { Name = "OK", IsDefault = true },
                new() { Name = "Open" }
            },
        };
        var box = MessageBoxManager
            .GetMessageBoxCustom(msgBoxParams);

        var msgBox = await box.ShowAsync();
        if (msgBox != "Open") return;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            if (viewModel.PackPath!= null) Process.Start("xdg-open", viewModel.PackPath);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            if (viewModel.PackPath!= null) Process.Start(@"explorer.exe", viewModel.PackPath);
        }   
    }
    
    private async void ExtractButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var viewModel = (MainWindowViewModel)DataContext!;
        var filesExtracted = await viewModel.Extract(GetTopLevel(this).GetControl<CheckBox>("ConvertModels").IsChecked == true);
        if (filesExtracted <= 0) return;
        var msgBoxParams = new MessageBoxCustomParams()
        {
            ContentTitle = "Files extracted succesfully!",
            ContentMessage = $"Wrote {filesExtracted} files to {viewModel.OutPath}.\nMake sure to copy these files to the game directory.",
            ButtonDefinitions = new List<ButtonDefinition>
            {
                new() { Name = "OK", IsDefault = true },
                new() { Name = "Open" }
            },
        };
        var box = MessageBoxManager
            .GetMessageBoxCustom(msgBoxParams);

        var msgBox = await box.ShowAsync();
        if (msgBox != "Open") return;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            if (viewModel.OutPath != null) Process.Start("xdg-open", viewModel.OutPath);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            if (viewModel.OutPath != null) Process.Start(@"explorer.exe", viewModel.OutPath);
        }

    }
}