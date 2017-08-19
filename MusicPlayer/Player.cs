using System;
using System.Collections.Generic;
using System.IO;
using WMPLib;
using System.Threading;
using System.Linq;

namespace MusicPlayer
{
    public class Player
    {
        private static List<string> unplayedSongs = new List<string>();
        private static WindowsMediaPlayer currentSong = new WindowsMediaPlayer();

        private static Thread songthread;

        private static Random rand = new Random();
        
        private static int volume = 1;
        private static bool repeat = true;

        static void Main(string[] args)
        {
            AddSongPaths();
            AddExcludePaths();

            Console.WriteLine("Would you like song repeats? (Y/N)");
            var input = Console.ReadLine();

            if (input.ToLower() == "n")
                repeat = false;

            PlaySong();

            Console.ReadLine();
        }

        private static void PlaySong()
        {
            StartPlaySongThread();
            CleanAndDisplayName();

            while (true)
            {
                var input = Console.ReadLine();

                if (string.Equals(input, "skip", StringComparison.InvariantCultureIgnoreCase))
                {
                    StartPlaySongThread();
                    CleanAndDisplayName();
                }
                else if (string.Equals(input, "mute", StringComparison.InvariantCultureIgnoreCase))
                {
                    currentSong.settings.mute = !currentSong.settings.mute;
                    CleanAndDisplayName();
                }
                else if (string.Equals(input, "pause", StringComparison.InvariantCultureIgnoreCase))
                {
                    currentSong.controls.pause();
                    CleanAndDisplayName();
                }
                else if (string.Equals(input, "mute", StringComparison.InvariantCultureIgnoreCase))
                {
                    currentSong.controls.play();
                    CleanAndDisplayName();
                }
                else if(string.Equals(input, "exit", StringComparison.InvariantCultureIgnoreCase))
                {
                    Environment.Exit(0);
                }
                else if(int.TryParse(input, out int s))
                {
                    volume = currentSong.settings.volume = s;
                    CleanAndDisplayName();
                }
                else
                {
                    CleanAndDisplayName();
                    Console.WriteLine("Invalid Input please try again");
                }
            }
        }

        public static void StartPlaySongThread()
        {
            songthread?.Abort();
            songthread = new Thread(() => PlaySong(unplayedSongs[SongLocation()])) { Name = $"PlaySongThread" };
            songthread.Start();
        }

        private static void CleanAndDisplayName()
        {
            Console.Clear();

            PrintControls();
            Console.WriteLine($"Currently Playing: {currentSong.currentMedia?.name}");

            if (currentSong.currentMedia == null)
                CleanAndDisplayName();
        }

        private static void PrintControls()
        {
            var controlls = "===========\nCommands:\nPause\nPlay\nSkip\nMute\n===========\nType a number for Volume Control: 1 - 10\n===========";
            Console.WriteLine(controlls);
        }

        private static void AddExcludePaths()
        {
            Console.WriteLine("Enter Paths to exclude (\"none\" to exclude no paths, \"done\" to finish excludeing paths): ");
            List<string> exclude = new List<string>();

            while (true)
            {
                var input = Console.ReadLine();

                if (string.Equals(input, "none", StringComparison.InvariantCultureIgnoreCase) || string.Equals(input, "done", StringComparison.InvariantCultureIgnoreCase))
                    break;
                else
                    exclude.Add(input);
            }

            unplayedSongs.RemoveAll(s => exclude.Any(e => s.Contains(e)));
        }

        public static void AddSongPaths()
        {
            string filePath = "";

            do
            {
                Console.WriteLine("Please inter a valid file path: ");
                filePath = Console.ReadLine();
            } while (!Directory.Exists(filePath));

            string[] paths = Directory.GetFiles(filePath, "*.*", SearchOption.AllDirectories);

            foreach (var path in paths)
            {
                if (string.IsNullOrEmpty(path))
                    return;

                if (path.EndsWith(".mp3") || path.EndsWith(".mp4") || path.EndsWith("m4a"))
                    unplayedSongs.Add(path);
            }
        }

        public static void PlaySong(string path)
        {
            currentSong.URL = path;
            currentSong.settings.volume = 1;
            currentSong.controls.play();

            while (true)
            {
                Thread.Sleep(2000);
                
                if((currentSong.controls.currentPosition >= currentSong.currentMedia.duration) || (currentSong.controls.currentPosition <= 0.5))
                {
                    if(repeat)
                    {
                        PlaySongWithRepeats();
                    }
                    else
                    {
                        PlaySongWithoutRepeats();
                    }
                }
            }
        }

        public static void PlaySongWithRepeats()
        {
            currentSong.URL = unplayedSongs[SongLocation()];
            currentSong.settings.volume = volume;
            currentSong.controls.play();
            CleanAndDisplayName();
        }

        public static void PlaySongWithoutRepeats()
        {
            if(unplayedSongs.Count == 0)
            {
                Console.Clear();
                Console.WriteLine("Song list compleated exiting");

                Thread.Sleep(10000);

                songthread.Abort();
                Environment.Exit(exitCode: 0);
            }

            var location = SongLocation();

            currentSong.URL = unplayedSongs[location];
            currentSong.settings.volume = volume;
            currentSong.controls.play();
            CleanAndDisplayName();
            
            unplayedSongs.RemoveAt(location);
        }

        public static int SongLocation()
        {
            return rand.Next(0, unplayedSongs.Count);
        }
    }
}
