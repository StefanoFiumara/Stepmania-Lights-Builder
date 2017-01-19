using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using LightsBuilder.Core.Data;
using Ookii.Dialogs.Wpf;
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

        private SongsDirectoryParser DirectoryParser { get; }

        public MainViewModel()
        {
            this.BrowseCommand = new RelayCommand(this.OnExecuteBrowse);
            this.AddLightChartsCommand = new RelayCommand(this.OnExecuteAddLightCharts, this.HasValidPathSelected);
            this.RestoreBackupsCommand = new RelayCommand(this.RestoreBackups, this.HasValidPathSelected);

            this.DirectoryParser = new SongsDirectoryParser();
        }

        private async void RestoreBackups()
        {
            Mouse.OverrideCursor = Cursors.Wait;

            await Task.Run(() =>
            {
                var backupFiles = Directory.GetFiles(this.SongsFolderPath, "*.sm*", SearchOption.AllDirectories).Where(f => f.EndsWith(".backup")).Select(f => new FileInfo(f));

                Parallel.ForEach(backupFiles, file =>
                {
                    var correspondingSmFile = new FileInfo(file.FullName.Replace(".backup", string.Empty));

                    if (correspondingSmFile.Exists) File.Delete(correspondingSmFile.FullName);

                    File.Copy(file.FullName, correspondingSmFile.FullName, true);
                });
            });
            

            Mouse.OverrideCursor = null;
            MessageBox.Show("All backups restored");
        }

        private async void OnExecuteAddLightCharts()
        {
            Mouse.OverrideCursor = Cursors.Wait;

            var smFiles = await Task.Run(() => this.DirectoryParser.FindSmFiles(this.SongsFolderPath).ToList());

            Mouse.OverrideCursor = null;

            var prompt = MessageBox.Show($"You are about to try to process {smFiles.Count} .sm files to add light charts, continue?", 
                                          "Confirm", 
                                          MessageBoxButton.YesNo);

            if (prompt != MessageBoxResult.Yes) return;

            Mouse.OverrideCursor = Cursors.Wait;

            await Task.Run(() =>
            {
                Parallel.ForEach(smFiles, smFile =>
                {
                    if (smFile.GetChartData(PlayStyle.Lights, SongDifficulty.Easy) != null) return;

                    var reference = smFile.GetChartData(PlayStyle.Single, SongDifficulty.Hard)
                                    ?? smFile.GetChartData(PlayStyle.Single, SongDifficulty.Challenge)
                                    ?? smFile.GetChartData(PlayStyle.Single, smFile.GetHighestChartedDifficulty(PlayStyle.Single))
                                    ?? smFile.GetChartData(PlayStyle.Double, SongDifficulty.Hard)
                                    ?? smFile.GetChartData(PlayStyle.Double, SongDifficulty.Challenge)
                                    ?? smFile.GetChartData(PlayStyle.Double, smFile.GetHighestChartedDifficulty(PlayStyle.Double));

                    if (reference == null) return;

                    try
                    {
                        var newChart = SmFileManager.GenerateLightsChart(reference);
                        smFile.AddNewStepchart(newChart);
                        smFile.Dispose();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Lights Builder Exception Caught: {e.Message}");
                    }
                });
            });
            

            Mouse.OverrideCursor = null;
            MessageBox.Show("Light Charts Added");
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