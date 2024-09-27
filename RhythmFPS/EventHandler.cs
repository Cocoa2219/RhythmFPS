using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using PlayerRoles;
using PlayerRoles.PlayableScps.Scp096;
using PlayerStatsSystem;
using RhythmFPS.API;
using Utf8Json;
using Scp096Role = Exiled.API.Features.Roles.Scp096Role;

namespace RhythmFPS;

public class EventHandler(RhythmFPS plugin)
{
    public RhythmFPS Plugin { get; } = plugin;

    private static string ConfigPath => Path.Combine(Paths.Configs, "RhythmFPS");

    internal List<Song> Songs { get; } = [];

    internal void Load()
    {
        if (!Directory.Exists(ConfigPath))
        {
            return;
        }

        var directories = Directory.GetDirectories(ConfigPath);

        Song.IdCounter = 0;

        Songs.Clear();

        foreach (var directory in directories)
        {
            var files = Directory.GetFiles(directory);

            if (!files.Any(x => x.EndsWith(".ogg")))
            {
                Log.Warn($"{directory} 폴더에 .ogg 파일이 없습니다. 스킵합니다.");
                continue;
            }

            if (files.Count(x => x.EndsWith(".ogg")) > 1)
            {
                Log.Warn($"{directory} 폴더에 .ogg 파일이 2개 이상 있습니다. 스킵합니다.");
                continue;
            }

            if (!files.Any(x => x.EndsWith(".json")))
            {
                Log.Warn($"{directory} 폴더에 .json 파일이 없습니다. 스킵합니다.");
                continue;
            }

            if (files.Count(x => x.EndsWith(".json")) > 1)
            {
                Log.Warn($"{directory} 폴더에 .json 파일이 2개 이상 있습니다. 스킵합니다.");
                continue;
            }

            SongSettings songSettings;

            try
            {
                songSettings = JsonSerializer.Deserialize<SongSettings>(File.ReadAllText(files.First(x => x.EndsWith(".json"))));
            }
            catch (Exception e)
            {
                Log.Warn("설정 파일을 읽는 도중 오류가 발생했거나 형식에 맞지 않습니다. 스킵합니다.");
                continue;
            }

            var fileName = Path.GetFileName(files.First(x => x.EndsWith(".ogg")));
            var lastDirName = new DirectoryInfo(directory).Name;
            var songDir = $"{lastDirName}/{fileName}";

            Songs.Add(new Song
            {
                SongPath = Path.Combine(directory, files.First(x => x.EndsWith(".ogg"))),
                SongDirectory = songDir,
                ConfigPath = files.First(x => x.EndsWith(".json")),
                Lyrics = songSettings.Lyrics.ToList(),
                LyricsTiming = songSettings.LyricsTiming.ToList(),
                Timing = songSettings.Timing.ToList()
            });

            Log.Info($"{files.First(x => x.EndsWith(".ogg"))}을(를) 성공적으로 로드했습니다.");
        }

        Log.Info($"{Songs.Count}곡을 로드했습니다.");
    }

    internal void OnShooting(ShootingEventArgs ev)
    {
        if (ev.Player.SessionVariables.TryGetValue("IsRecording", out var songObj))
        {
            ev.Firearm.Ammo = ev.Firearm.MaxAmmo;

            var song = (Song)songObj;

            song.NextLyrics(ev.Player);
        } else if (ev.Player.SessionVariables.TryGetValue("IsRecordingTiming", out songObj))
        {
            ev.Firearm.Ammo = ev.Firearm.MaxAmmo;

            var song = (Song)songObj;

            song.NextTiming(ev.Player);
        }
    }
}