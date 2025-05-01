using System;
using System.IO;
using System.Threading.Tasks;
using AssetRipper;
using CommunityToolkit.Mvvm.ComponentModel;

namespace A2RacerTool.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty] private string? _pathToRcsInd;
    [ObservableProperty] private string? _outPath;
    [ObservableProperty] private string? _pathToExtracted;
    [ObservableProperty] private string? _packPath;

    public void SetRcsIndPath(string? path)
    {
        PathToRcsInd = path;
    }

    public void SetOutPath(string? path)
    {
        OutPath = path;
    }

    public void SetPackPath(string? path)
    {
        PackPath = path;
    }

    public void SetPathToExtracted(string? path)
    {
        PathToExtracted = path;
    }

    public Task<bool> Compile()
    {
        if (PathToExtracted == null) return Task.FromResult(false);
        if (PackPath == null) return Task.FromResult(false);
        var success = RcsFile.Create(PathToExtracted, PackPath);
        return Task.FromResult(success);
    }

    public async Task<int> Extract(bool convertModels = false)
    {
        if (PathToRcsInd == null) return 0;
        await using var fileStream = new FileStream(PathToRcsInd, FileMode.Open, FileAccess.Read);
        await using var rcsFileStream = new FileStream(PathToRcsInd.Replace(".ind", ".img").Replace(".IND", ".IMG"),
            FileMode.Open, FileAccess.Read);
        using var reader = new BinaryReader(fileStream);
        using var rcsReader = new BinaryReader(rcsFileStream);


        var indFile = new IndFile();
        var rcsFile = new RcsFile("rcs.img");
        indFile.Read(reader);
        rcsFile.Read(rcsReader);

        var list = indFile.GetEntries();
        foreach (var entry in list)
        {
            var rcsEntry = rcsFile.GetEntry(entry);
            var sanitizedName = string.Join("", rcsEntry.GetName().Split(Path.GetInvalidFileNameChars()));
            if (OutPath == null) continue;
            var entryPath = Path.Combine(OutPath, sanitizedName).Trim();

            if (File.Exists(entryPath))
            {
                File.Delete(entryPath);
            }

            await using FileStream entryStream = new(entryPath, FileMode.Create, FileAccess.Write);
            entryStream.Write(rcsEntry.GetData());
        }

        if (!convertModels) return list.Count;
        if (!Directory.Exists(Path.Combine(OutPath, "converted")))
        {
            Directory.CreateDirectory(Path.Combine(OutPath, "converted"));
        }
        foreach (var entry in list)
        {
            if (!entry.GetName().EndsWith(".d3d") && !entry.GetName().EndsWith(".D3D"))
            {
                continue;
            }
            var rcsEntry = rcsFile.GetEntry(entry);
            if (OutPath == null) continue;
            var data = rcsEntry.GetData();
            try
            {
                var d3dFile = data[0] == 0xff ? new VakantieD3dFile(entry.GetName()) : new D3dFile(entry.GetName());
                await using FileStream d3dStream = new(Path.Combine(OutPath, entry.GetName()), FileMode.Open, FileAccess.Read);
                using BinaryReader d3dReader = new(d3dStream);
                d3dFile.Read(d3dReader);
                d3dFile.WriteToObj(Path.Combine(OutPath, "converted", d3dFile.GetFilename() + ".obj"));
            } catch { Console.WriteLine($"{entry.GetName()} could not be converted (possibly empty file???)");}
        }

        return list.Count;
    }
}