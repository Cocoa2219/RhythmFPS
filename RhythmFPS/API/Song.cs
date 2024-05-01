using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using AudioPlayer.API;
using Exiled.API.Enums;
using Exiled.API.Features;
using PlayerRoles;
using UnityEngine;
using Utf8Json;
using VoiceChat;

namespace RhythmFPS.API;

public class Song(string songPath, string songDir, string configPath, List<string> lyrics, List<long> lyricsTiming, List<long> timing)
{
    public static int IdCounter { get; set; } = 0;
    public int Id { get; } = IdCounter++;
    public string SongPath { get; } = songPath;
    public string SongDirectory { get; } = songDir;
    public string ConfigPath { get; } = configPath;
    public List<string> Lyrics { get; } = lyrics;
    public List<long> LyricsTiming { get; private set; } = lyricsTiming;
    public List<long> Timing { get; } = timing;

    public Player CurrentRecordingPlayer { get; private set; }

    private Stopwatch stopwatch = new();
    private List<long> lyricsTiming = new();
    private int lyricsIndex = 0;

    public void Record(Player player)
    {
        CurrentRecordingPlayer = player;

        AudioController.SpawnDummy(99, "RhythmFPS", "orange", "DJ  Cocoa");

        AudioController.PlayAudioFromFile(SongPath);

        player.Role.Set(RoleTypeId.ClassD, SpawnReason.Respawn, RoleSpawnFlags.All);
        player.SyncEffect(new Effect(EffectType.SoundtrackMute, 0, 1));
        player.SyncEffect(new Effect(EffectType.Ensnared, 0, 1));
        player.SyncEffect(new Effect(EffectType.Scp1853, 0, 255));
        player.Teleport(new Vector3(12f, 992f, -42.8f));
        player.AddItem(ItemType.GunCOM18);
        player.SessionVariables.Add("IsRecording", this);
        stopwatch.Start();
    }

    public void StopRecord()
    {
        CurrentRecordingPlayer.DisableAllEffects();
        CurrentRecordingPlayer.ShowHint("");
        CurrentRecordingPlayer = null;
        stopwatch.Stop();
        stopwatch.Reset();
        AudioController.StopPlayerFromPlaying([99]);
        AudioController.DisconnectDummy();
        lyricsTiming.Clear();
        lyricsIndex = 0;
    }

    internal void NextLyrics(Player player)
    {
        if (player != CurrentRecordingPlayer) return;

        var time = stopwatch.ElapsedMilliseconds;
        lyricsTiming.Add(time);
        stopwatch.Stop();

        player.Broadcast(5, $"{time}ms", Broadcast.BroadcastFlags.Normal, true);

        if (lyricsIndex >= Lyrics.Count)
        {
            player.Broadcast(5, "녹음이 완료되었습니다.");
            player.SessionVariables.Remove("IsRecording");
            stopwatch.Reset();

            LyricsTiming = lyricsTiming;

            var json = JsonSerializer.ToJsonString(new SongSettings(Lyrics.ToArray(), LyricsTiming.ToArray(), Timing.ToArray()));

            File.WriteAllText(ConfigPath, json);

            return;
        }

        ShowLyrics(player, lyricsIndex);
        lyricsIndex++;

        stopwatch.Restart();
    }

    private void ShowLyrics(Player player, int index)
    {
        if (player != CurrentRecordingPlayer) return;

        const string format =
            "\n\n\n\n\n\n\n\n<line-height=45px><cspace=-2px><color=#7d7d7d><size=30>{0}</size></color>\n{1}\n<color=#7d7d7d><size=30>{2}</size></color></cspace></line-height>";

        switch (index)
        {
            case -1:
                player.ShowHint(string.Format(format, "", "", Lyrics[index + 1]), 200f);
                break;
            case 0:
                // ReSharper disable once UselessBinaryOperation
                player.ShowHint(string.Format(format, "", Lyrics[index], Lyrics[index + 1]), 200f);
                break;
            default:
            {
                if (index == Lyrics.Count - 1)
                {
                    player.ShowHint(string.Format(format, Lyrics[index - 1], Lyrics[index], ""), 200f);
                }
                else if (index == Lyrics.Count)
                {
                    player.ShowHint(string.Format(format, Lyrics[index - 1], "", ""), 200f);
                }
                else
                {
                    player.ShowHint(string.Format(format, Lyrics[index - 1], Lyrics[index], Lyrics[index + 1]), 200f);
                }

                break;
            }
        }
    }
}