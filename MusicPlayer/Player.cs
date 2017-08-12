using System;
using System.Collections.Generic;
using System.IO;
using WMPLib;
using System.Threading;

namespace MusicPlayer
{
    public class Player
    {
        public static List<string> unplayedSongs = new List<string>();
        public static List<string> playedSongs = new List<string>();
        public static WindowsMediaPlayer currentSong = new WindowsMediaPlayer();

        private static Thread songthread;

        private static Random rand = new Random();

        private static int count = 0;
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
            play:
            songthread?.Abort();
            songthread = new Thread(() => PlaySong(unplayedSongs[SongLocation()])) {Name = $"PlaySongThread" };
            songthread.Start();

            loop:
            CleanAndDisplayName();

            while (true)
            {
                var input = Console.ReadLine();

                if (input == "skip" || input == "Skip")
                {
                    goto play;
                }

                if (input == "mute" || input == "mute")
                {
                    currentSong.settings.mute = !currentSong.settings.mute;
                    goto loop;
                }

                if (input == "pause" || input == "pause")
                {
                    currentSong.controls.pause();
                    goto loop;
                }

                if (input == "play" || input == "Play")
                {
                    currentSong.controls.play();
                    goto loop;
                }

                int s = int.MinValue;
                int.TryParse(input, out s);

                if (s != 0)
                {
                    volume = currentSong.settings.volume = int.Parse(input);
                    goto loop;
                }
            }
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
            Console.WriteLine("Enter Paths to exclude (\"none\" to exclude no paths): ");
            List<string> exclude = new List<string>();

            while (true)
            {
                var input = Console.ReadLine();

                if (input == "none" || input == "done")
                    break;
                else
                    exclude.Add(input);
            }

            for (int i = 0; i < exclude.Count; i++)
            {
                for (int j = unplayedSongs.Count - 1; j >= 0; j--)
                {
                    if (unplayedSongs[j].Contains(exclude[i]))
                        unplayedSongs.RemoveAt(j);
                }
            }
        }

        public static void AddSongPaths()
        {
            Console.WriteLine("Enter Music Folder Path: ");

            enterMusicPath:
            string filePath = Console.ReadLine();

            if (!Directory.Exists(filePath))
            {
                Console.WriteLine("Please Enter a valid file path");
                goto enterMusicPath;
            }

            string[] paths = Directory.GetFiles(filePath, "*.*", SearchOption.AllDirectories);

            for (int i = 0; i < paths.Length; i++)
            {
                if (paths[i] == null)
                    return;

                if (paths[i].EndsWith(".mp3") || paths[i].EndsWith(".mp4") || paths[i].EndsWith("m4a"))
                    unplayedSongs.Add(paths[i]);
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
                songthread.Abort();
                Environment.Exit(exitCode: 0);
            }

            var location = SongLocation();

            currentSong.URL = unplayedSongs[location];
            currentSong.settings.volume = volume;
            currentSong.controls.play();
            CleanAndDisplayName();

            playedSongs.Add(unplayedSongs[location]);
            unplayedSongs.RemoveAt(location);
        }

        public static int SongLocation()
        {
            var location = rand.Next(0, unplayedSongs.Count);
            return (location == unplayedSongs.Count ? location - 1 : location);
        }
    }
}
