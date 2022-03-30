using System.IO;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace SteamVersionSelector
{
    public class MainViewModel : ObservableObject
    {
        public MainViewModel()
        {
            LoadBlobs();
        }

        private SecondBlob? _secondBlob;
        public SecondBlob? SelectedBlob 
        {
            get => _secondBlob;
            set
            {
                if (SetProperty(ref _secondBlob, value))
                {
                    if (LoadBlobCommand is RelayCommand cmd)
                    {
                        cmd.NotifyCanExecuteChanged();
                    }
                }
            }
        }

        public void LoadBlobs()
        {
            System.IO.DirectoryInfo blobDirectory = new(SourceDirectory);
            var firstBlobs = blobDirectory.GetFiles("firstblob*.*")
                .Select(file => new FirstBlob(file.Name, file.LastWriteTime));

            foreach (var file in blobDirectory.GetFiles("secondblob*.*"))
            {
                var firstBlob = firstBlobs.OrderByDescending(b => b.FileDate)
                    .FirstOrDefault(blob => blob.FileDate <= file.LastWriteTime);
                if (firstBlob == null)
                    break;
                Blobs.Add(new SecondBlob(file.Name, file.LastWriteTime, firstBlob));
            }
        }

        private ICommand? _loadBlobCommand;
        public ICommand LoadBlobCommand => _loadBlobCommand ??= new RelayCommand(() =>
        {
            if (SelectedBlob == null) throw new Exception("wtf");
            File.Copy(Path.Combine(SourceDirectory, SelectedBlob.FileName), Path.Combine(TargetDirectory, "secondblob.bin"), true);

            File.Copy(Path.Combine(SourceDirectory, SelectedBlob.FirstBlob.FileName), Path.Combine(TargetDirectory, "firstblob.bin"), true);
            if (File.Exists(CacheDirectory))
            {
                File.Delete(CacheDirectory);
            }
            var orig = Path.Combine(TargetDirectory, "secondblob.orig");
            if (File.Exists(orig))
            {
                File.Delete(orig);
            }
        }, () => SelectedBlob != null);
        public string SourceDirectory { get; set; } = @"C:\Users\jon\Downloads\small_selection_of_blobs_with_timestamps";
        public string TargetDirectory { get; set; } = @"files\";
        public string CacheDirectory { get; set; } = @"files\cache\";
        public ObservableCollection<SecondBlob> Blobs { get; set; } = new();
    }
    public record FirstBlob(string FileName, DateTime FileDate);
    public record SecondBlob(string FileName, DateTime FileDate, FirstBlob FirstBlob);
}
