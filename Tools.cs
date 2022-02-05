using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace devnull
{
    public class Tools
    {
        public static bool userApprovedBackup()
        {
            Console.Write("Perform backup ? [y/n] : ");

            string userAnswer = Console.ReadLine().Trim();

            Tools.LogLine("");

            if (userAnswer.Trim() != "y" && userAnswer.Trim() != "n")
            {                
                Tools.LogLine("[I]:Answer not recognized, please give y - for performin backup ot give n - if you cancel doing that backup ?", ConsoleColor.Cyan);
                return userApprovedBackup();                
            }

            if (userAnswer == "y")
            {
                return true;
            }
            else return false;
        }

        public static void loadSourceAndTarges(List<string> volumes, List<string> sourcePaths, List<string> targetPaths)
        {
            foreach (var v in volumes)
            {
                string sourceFullPath = Path.Combine(v, SOURCE);
                if (File.Exists(sourceFullPath))
                {
                    Tools.LogLine("source found:" + sourceFullPath, ConsoleColor.Green);
                    sourcePaths.Add(v);
                }

                string targetFullpath = Path.Combine(v, TARGET);
                if (File.Exists(targetFullpath))
                {
                    Tools.LogLine("target found:" + targetFullpath, ConsoleColor.Green);
                    targetPaths.Add(v);
                }
            }
        }
        public static void progressBar(string stepDescription, int progress, int total, CancellationToken ctoken)
        {            
            bool finish = false;

            while (true)
            {                
                Thread.Sleep(10);

                progress += 1;

                if (ctoken.IsCancellationRequested)
                {
                    finish = true;                    
                }

                int totalChunks = 30;

                //draw empty progress bar
                
                Console.CursorLeft = 0;
                Console.Write("["); //start
                Console.CursorLeft = totalChunks + 1;
                Console.Write("]"); //end
                Console.CursorLeft = 1;

                double pctComplete = Convert.ToDouble(progress) / total;
                int numChunksComplete = Convert.ToInt16(totalChunks * pctComplete);

                //draw completed chunks
                Console.BackgroundColor = ConsoleColor.Green;
                Console.Write("".PadRight(numChunksComplete));

                //draw incomplete chunks
                Console.BackgroundColor = ConsoleColor.Gray;
                Console.Write("".PadRight(totalChunks - numChunksComplete));

                //draw totals
                Console.CursorLeft = totalChunks + 5;
                Console.BackgroundColor = ConsoleColor.Black;

                //string output = progress.ToString() + " of " + total.ToString();
                //Console.Write(output.PadRight(15) + stepDescription); //pad the output so when changing from 3 to 4 digits we avoid text shifting
                Console.Write("backup in progress, please wait ...".PadRight(15)); //pad the output so when changing from 3 to 4 digits we avoid text shifting
                //Console.WriteLine("work in progress, please wait...");

                if (finish && progress == 100)
                {
                    break;
                }

                if (progress >= total)
                {
                    progress = 0;
                }
            }            
        }

        public static List<string> loadVirtualVolumes(List<string> virtualDrives)
        {
            List<string> volumes = new List<string>();

            if (virtualDrives != null)
            {
                foreach (var virtualDrive in virtualDrives)
                {
                    if (!virtualDrive.StartsWith("OFF|"))
                    {
                        volumes.Add(virtualDrive);
                    }
                }
            }

            return volumes;
        }

        public static List<string> loadVolumes(DriveInfo [] localDrives)
        {
            List<string> volumes = new List<string>();

            foreach (var drive in localDrives)
            {
                if (drive.DriveType == DriveType.Fixed || drive.DriveType == DriveType.Removable)
                {
                    volumes.Add(drive.Name);
                }
            }

            return volumes;
        }

        public static void LogLine(string text, ConsoleColor color)
        {
            Console.ForegroundColor = color;

            LogLine(text);

            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void LogLine(string text, ConsoleColor color, List<string> logList, Object thisLock)
        {
            Console.ForegroundColor = color;

            LogLine(text, logList, thisLock);

            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void LogLine(string text, List<string> logList, Object thisLock)
        {
            lock (thisLock)
            {
                TextWriter tmp = Console.Out;

                using (StreamWriter sw = new StreamWriter("Logs.txt", true))
                {
                    Console.SetOut(sw);
                    Console.WriteLine(text);
                    Console.SetOut(tmp);
                    sw.Close();
                }

                logList.Add(text);
            }            
        }

        public static void LogLine(string text)
        {
            TextWriter tmp = Console.Out;

            using (StreamWriter sw = new StreamWriter("Logs.txt", true))
            {
                Console.SetOut(sw);
                Console.WriteLine(text);
                Console.SetOut(tmp);
                sw.Close();
            }

            Console.WriteLine(text);
        }

        public static string Repc(char c, int num)
        {
            var str = new StringBuilder();
            while (num-- > 0) str.Append(c);
            return str.ToString();
        }

        public static void writePreamble(string stream, string programVersion)
        {
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            LogLine("");
            LogLine(Repc('-', 100), ConsoleColor.Green);
            LogLine(Repc('-', 100), ConsoleColor.Green);
            LogLine("");

            LogLine($"................................................",ConsoleColor.Green);
            LogLine($"..######..####.##.......##.....##.####....###...",ConsoleColor.Green);
            LogLine($".##....##..##..##.......##.....##..##....##.##..",ConsoleColor.Green);
            LogLine($".##........##..##.......##.....##..##...##...##.",ConsoleColor.Green);
            LogLine($"..######...##..##.......##.....##..##..##.....##",ConsoleColor.Green);
            LogLine($".......##..##..##........##...##...##..#########",ConsoleColor.Green);
            LogLine($".##....##..##..##.........##.##....##..##.....##",ConsoleColor.Green);
            LogLine($"..######..####.########....###....####.##.....##",ConsoleColor.Green);
            LogLine($"................................................", ConsoleColor.Green);

            LogLine("");
            LogLine($"---- Run for stream : [{stream}]");
            LogLine($"---- date : { DateTime.Now.ToString() }");
            LogLine($"---- date utc : { DateTime.UtcNow.ToString() }");
            LogLine($"---- version : { programVersion }", ConsoleColor.DarkYellow);
            LogLine("");

            LogLine(Repc('-', 100), ConsoleColor.Green);
            LogLine(Repc('-', 100), ConsoleColor.Green);
            
            LogLine("");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
