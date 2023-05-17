using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace devnull
{
    /// <summary>
    /// This program copy files from targets to destinations in zip xformat. 
    /// 
    /// 1.Create file with source pathes e.g : 
    /// 
    ///   C:\\silvia_source_default
    ///   
    ///   inside you entered in spearate line e.g :
    ///   
    ///   F:\file.txt
    ///   F:\directory_with_images
    ///   
    ///   That's list of source files that program will zip and copy.
    ///   
    /// 2.Now let's say you connected two pendrive that are F:\\ and G:\\
    /// 3.You put file silvia_target_default.txt in F:\\ and G:\\
    /// 4.Now if you run program it will copy your F:\file.txt and directory_with_images as zipped
    ///   archived into F:\\ and G:\\
    ///       
    /// In simple words program recognize source files and target files and do copy files listed in 
    /// source files int volumes when he found target files exists.
    /// 
    /// </summary>
    public class Program
    {
        public const string programVersion = "1.0.0";

        static void Main(string[] args)
        {
            Object thisLock = new object();
            List<string> logList = new List<string>();
            List<Task> backups = new List<Task>();

            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            // remote locations e.g shares
            var virtualDrives = config.GetSection("virtual_drives").Get<List<string>>(); // Microsoft.Extensions.Configuration.Binder

            // channel say what config files will be recognized by app e.g if scome is : xyz only 
            // silvia.source[xyz] will be considered during app run

            string stream = ((args.Length == 0) ? "default" : args[0]);

            // write standard info on console
            Tools.writePreamble(stream, Program.programVersion);

            // setup standard source and target configuration that must match to setup backup stream.
            // silvia.source[xyz] and devbcp.target[xyz] setup stream so files will be transferred.
            // silvia.source[xyz1] and devbcp.target[xyz2] do not as xyz1 ident differ from xyz2
            string SOURCE = "silvia_source_" + stream + ".txt";
            string TARGET = "silvia_target_" + stream + ".txt";

            List<string> volumes = Tools.loadVolumes(DriveInfo.GetDrives());

            volumes.AddRange(Tools.loadVirtualVolumes(virtualDrives));

            List<string> sourcePaths = new List<string>();
            List<string> targetPaths = new List<string>();

            Tools.loadSourceAndTarges(volumes, sourcePaths, targetPaths, SOURCE, TARGET);

            if (targetPaths.Count == 0)
            {
                Tools.LogLine("[E]:ERROR - target not found.", ConsoleColor.Red);
            }
            else if (sourcePaths.Count == 0)
            {
                Tools.LogLine("[E]:ERROR - source not found.", ConsoleColor.Red);
            }
            else
            {
                Tools.LogLine("");

                if (!Tools.userApprovedBackup())
                {
                    Tools.LogLine("[I]:User didn't approved backup, exiting.", ConsoleColor.Red);
                    Tools.LogLine("");
                    return;
                }
                else
                {
                    Tools.LogLine("[I]:User approved backup, processing.", ConsoleColor.Green);
                    Tools.LogLine("");
                }              

                CancellationTokenSource tokenSource = new CancellationTokenSource();
                var progress = Task.Run(() => { Tools.progressBar("backup progress", 0, 100, tokenSource.Token); }, tokenSource.Token);

                foreach (var sourcePath in sourcePaths) 
                {
                     string sourceConfig = Path.Combine(sourcePath, SOURCE);
                     string[] sourceConfigLines = File.ReadAllLines(sourceConfig);
                    
                     foreach (string configLine in sourceConfigLines)
                     {
                         foreach (var targetPath in targetPaths)
                         {                                
                             try
                             {
                                 DateTime currentUtc = DateTime.UtcNow;
                                 DateTime current = DateTime.Now;

                                 string zipFileName = Path.GetFileName(configLine) + "_" + dateToString(currentUtc) + "utc_" + dateToString(current) + "-utc" + ".zip";

                                 Tools.LogLine("", logList, thisLock);
                                 Tools.LogLine($"[I]:backup from source ( {configLine} ) to target ( {Path.Combine(targetPath, zipFileName)} )", logList, thisLock);
                                 
                                 if (File.GetAttributes(configLine).HasFlag(FileAttributes.Directory))
                                 {
                                     Tools.LogLine("[I]:compressing directory.", logList, thisLock);
                                     
                                     ZipFile.CreateFromDirectory(configLine, Path.Combine(targetPath, Path.Combine(targetPath, zipFileName)));
                                 }
                                 else
                                 {
                                    Tools.LogLine("[I]:compressing file.", logList, thisLock);
                                    backups.Add(Task.Run(() =>
                                     {
                                        using (var zip = ZipFile.Open(Path.Combine(targetPath, Path.Combine(targetPath, zipFileName)), ZipArchiveMode.Create))
                                                   zip.CreateEntryFromFile(configLine, Path.GetFileName(configLine));
                                     }));
                                 }

                                 Tools.LogLine("", logList, thisLock);
                                 Tools.LogLine("[I]:SUCCESS - backup completed.", logList, thisLock);
                             }
                             catch (Exception ex)
                             {
                                 Tools.LogLine("[E]:ERROR - looks like backup failed.", logList, thisLock);
                                 Tools.LogLine("[E]:" + ex.Message + "|" + ex.StackTrace, logList, thisLock);
                             }
                         }
                     }
                }

                
                // wait for all backups to complete.
                Task.WaitAll(backups.ToArray());

                // cancel the load indicator task.
                tokenSource.Cancel();
                
                // wait for load indicator to finish.
                Task.WaitAll(new Task[] { progress });

                Tools.LogLine("");

                // display logs from backups.
                foreach (var message in logList)
                {
                    if (message.StartsWith("[E]"))
                    {
                        Tools.LogLine(message, ConsoleColor.Red);
                    }
                    else
                    {
                        Tools.LogLine(message, ConsoleColor.Green);
                    }
                    
                }

                Tools.LogLine("");
                Tools.LogLine("[I]:Exiting.", ConsoleColor.DarkYellow);
                Tools.LogLine("");
            }
        }

        public static string dateToString(DateTime dt)
        {
            string year = dt.Year.ToString();
            string month = dt.Month.ToString("D2");
            string day = dt.Day.ToString("D2");
            string hour = dt.Hour.ToString("D2");
            string minute = dt.Minute.ToString("D2");
            string second = dt.Second.ToString("D2");
            return day + month + year + hour + minute + second;
        }
    }
}



