using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LightsBuilder.Core.Data;
using LightsBuilder.Core.Parsers;

namespace LightsBuilder.CLI
{
    class MainClass
    {

        public static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                string directory = args[0];

                Console.WriteLine(String.Format("----- Generating charts in {0}. -----", directory));

                if (!Directory.Exists((directory)))
                {
                    Console.Write(String.Format("----- Directory {0} does not exist, ignoring... -----", directory));
                }
                else
                {
                    SongsDirectoryParser DirectoryParser = new SongsDirectoryParser();
                    var smFiles = DirectoryParser.FindSmFiles(directory).ToList();

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
                            Console.WriteLine(String.Format("Generating chart for {0}", smFile.SongData.SongName));
                            var newChart = SmFileManager.GenerateLightsChart(reference);
                            smFile.AddNewStepchart(newChart);
                            smFile.Dispose();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"Lights Builder Exception Caught: {e.Message}");
                        }
                    });

                    Console.WriteLine("Done.");
                }
            }
            else
            {
                Console.WriteLine("Please supply a directory to process.");
            }
        }
    }
}
