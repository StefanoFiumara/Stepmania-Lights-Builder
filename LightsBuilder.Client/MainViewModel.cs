using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using LightsBuilder.Core.Data;
using Ookii.Dialogs.Wpf;
using TournamentManager.Client.Commands;
using LightsBuilder.Core.Parsers;

namespace LightsBuilder.Client
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public RelayCommand BrowseCommand { get; set; }
        public RelayCommand AddLightChartsCommand { get; set; }
        public RelayCommand RestoreBackupsCommand { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string _songsFolderPath;
        public string SongsFolderPath
        {
            get { return this._songsFolderPath; }
            set
            {
                this._songsFolderPath = value;
                this.OnPropertyChanged();
            }
        }

        private SongsDirectoryParser DirectoryParser { get; set; }

        public MainViewModel()
        {
            this.BrowseCommand = new RelayCommand(this.OnExecuteBrowse);
            this.AddLightChartsCommand = new RelayCommand(this.OnExecuteAddLightCharts, this.HasValidPathSelected);
            this.RestoreBackupsCommand = new RelayCommand(this.RestoreBackups, this.HasValidPathSelected);

            this.DirectoryParser = new SongsDirectoryParser();
        }

        private void RestoreBackups()
        {

            var backupFiles = Directory.GetFiles(this.SongsFolderPath, "*.sm*", SearchOption.AllDirectories).Where(f => f.EndsWith(".backup")).Select(f => new FileInfo(f));

            foreach (var backupFile in backupFiles)
            {
                var correspondingSmFile = new FileInfo(backupFile.FullName.Replace(".backup", string.Empty));

                if (correspondingSmFile.Exists) File.Delete(correspondingSmFile.FullName);

                File.Copy(backupFile.FullName, correspondingSmFile.FullName, true);
            }

            MessageBox.Show("All backups restored");
        }

        private void OnExecuteAddLightCharts()
        {
            var smFiles = this.DirectoryParser.GetSmFiles(this.SongsFolderPath);

            var prompt = MessageBox.Show(
                $"You are about to try to process {smFiles.Count} .sm files to add light charts, continue?", "Confirm", MessageBoxButton.YesNo);

            if (prompt != MessageBoxResult.Yes) return;

            int processedCount = 0;
            foreach (var smFile in smFiles)
            {
                if (smFile.GetChartData(PlayStyle.Lights) != null) continue;

                smFile.AddLightsData();
                smFile.SaveChanges();
                processedCount++;
            }
            
            MessageBox.Show($"{processedCount} Light Charts Added");
        }

        private bool HasValidPathSelected()
        {
            if (string.IsNullOrEmpty(this.SongsFolderPath)) return false;

            var dirInfo = new DirectoryInfo(this.SongsFolderPath);

            return dirInfo.Exists;
        }

        private void OnExecuteBrowse()
        {
            var dialog = new VistaFolderBrowserDialog();

            var result = dialog.ShowDialog();
            if (result == false) return;

            this.SongsFolderPath = dialog.SelectedPath;
        }
    }
}