using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using AudioPlayer.API;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Roles;
using PlayerRoles;
using UnityEngine;
using Utf8Json;
using VoiceChat;

namespace RhythmFPS.API;

public class Song
{
    public static int IdCounter { get; set; } = 0;
    public int Id { get; } = IdCounter++;
    public string SongPath { get; set; }
    public string SongDirectory { get; set; }
    public string ConfigPath { get; set; }
    public List<string> Lyrics { get; set; }
    public List<long> LyricsTiming { get; set; }
    public List<long> Timing { get; set; }

    public Player CurrentRecordingPlayer { get; private set; }
    public Player CurrentPlayingPlayer { get; private set; }
    public Player CurrentTimingRecordingPlayer { get; private set; }

    private Stopwatch stopwatch = new();
    private List<long> lyricsTiming = new();
    private List<long> timing = new();
    private int lyricsIndex = 0;

    public void Record(Player player)
    {
        CurrentRecordingPlayer = player;

        AudioController.SpawnDummy(99, "RhythmFPS", "orange", "DJ Cocoa");

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
            player.Broadcast(5, "녹음이 완료되었습니다.", Broadcast.BroadcastFlags.Normal, true);
            player.SessionVariables.Remove("IsRecording");
            stopwatch.Reset();

            LyricsTiming = lyricsTiming;

            var json = JsonSerializer.ToJsonString(new SongSettings(Lyrics.ToArray(), LyricsTiming.ToArray(), Timing.ToArray()));

            File.WriteAllText(ConfigPath, json);

            StopRecord();

            return;
        }

        ShowLyrics(player, lyricsIndex);
        lyricsIndex++;

        stopwatch.Restart();
    }

    private void ShowLyrics(Player player, int index)
    {
        const string format =
            "\n\n\n\n\n\n\n\n<line-height=35px><cspace=0.1em><color=#7d7d7d><size=20>{0}</size></color>\n{1}\n<color=#7d7d7d><size=20>{2}</size></color></cspace></line-height>";

        switch (index)
        {
            case -1:
                player.ShowHint(string.Format(format, "", "", RemoveColorTag(Lyrics[index + 1])), 200f);
                break;
            case 0:
                player.ShowHint(string.Format(format, "", Lyrics[index], RemoveColorTag(Lyrics[1])), 200f);
                break;
            default:
            {
                if (index == Lyrics.Count - 1)
                {
                    player.ShowHint(string.Format(format, RemoveColorTag(Lyrics[index - 1]), Lyrics[index], ""), 200f);
                }
                else if (index == Lyrics.Count)
                {
                    player.ShowHint(string.Format(format, RemoveColorTag(Lyrics[index - 1]), "", ""), 200f);
                }
                else
                {
                    player.ShowHint(string.Format(format, RemoveColorTag(Lyrics[index - 1]), Lyrics[index], RemoveColorTag(Lyrics[index + 1])), 200f);
                }

                break;
            }
        }
    }

    public void PlayLyrics(Player player)
    {
        CurrentPlayingPlayer = player;

        AudioController.SpawnDummy(99, "RhythmFPS", "orange", "DJ Cocoa");

        AudioController.PlayAudioFromFile(SongPath);

        player.Role.Set(RoleTypeId.ClassD, SpawnReason.Respawn, RoleSpawnFlags.All);
        player.SyncEffect(new Effect(EffectType.SoundtrackMute, 0, 1));
        player.SyncEffect(new Effect(EffectType.Ensnared, 0, 1));
        player.SyncEffect(new Effect(EffectType.Scp1853, 0, 255));
        player.Teleport(new Vector3(12f, 992f, -42.8f));
        player.AddItem(ItemType.GunCOM18);
        player.SessionVariables.Add("IsPlaying", this);

        MEC.Timing.RunCoroutine(PlayLyricsCoroutine(player), "PlayLyrics_" + player.UserId);
    }

    private IEnumerator<float> PlayLyricsCoroutine(Player player)
    {
        ShowLyrics(player, -1);

        for (var i = 0; i < LyricsTiming.Count; i++)
        {
            yield return MEC.Timing.WaitForSeconds(LyricsTiming[i] / 1000f);

            ShowLyrics(player, i);
        }

        StopPlay();
    }

    public void StopPlay()
    {
        MEC.Timing.KillCoroutines("PlayLyrics_" + CurrentPlayingPlayer.UserId);

        CurrentPlayingPlayer.SessionVariables.Remove("IsPlaying");
        CurrentPlayingPlayer.DisableAllEffects();
        CurrentPlayingPlayer.ShowHint("");
        CurrentPlayingPlayer = null;
        AudioController.StopPlayerFromPlaying([99]);
        AudioController.DisconnectDummy();
    }

    public void RecordTiming(Player player)
    {
        CurrentTimingRecordingPlayer = player;

        AudioController.SpawnDummy(99, "RhythmFPS", "orange", "DJ Cocoa");

        AudioController.PlayAudioFromFile(SongPath);

        player.Role.Set(RoleTypeId.ClassD, SpawnReason.Respawn, RoleSpawnFlags.All);
        player.SyncEffect(new Effect(EffectType.SoundtrackMute, 0, 1));
        player.SyncEffect(new Effect(EffectType.Ensnared, 0, 1));
        player.SyncEffect(new Effect(EffectType.Scp1853, 0, 255));
        player.Teleport(new Vector3(12f, 992f, -42.8f));
        player.AddItem(ItemType.GunCOM18);
        player.SessionVariables.Add("IsRecordingTiming", this);

        stopwatch.Start();

        MEC.Timing.RunCoroutine(PlayLyricsCoroutine(player), "PlayLyrics_" + player.UserId);
    }

    internal void NextTiming(Player player)
    {
        if (player != CurrentTimingRecordingPlayer) return;

        var time = stopwatch.ElapsedMilliseconds;
        timing.Add(time);
        stopwatch.Restart();

        player.Broadcast(5, $"{time}ms", Broadcast.BroadcastFlags.Normal, true);
    }

    public void StopTimingRecord()
    {
        Timing = timing;

        CurrentTimingRecordingPlayer.SessionVariables.Remove("IsRecordingTiming");

        var json = JsonSerializer.ToJsonString(new SongSettings(Lyrics.ToArray(), LyricsTiming.ToArray(), Timing.ToArray()));

        File.WriteAllText(ConfigPath, json);

        CurrentTimingRecordingPlayer.DisableAllEffects();
        CurrentTimingRecordingPlayer.ShowHint("");
        CurrentTimingRecordingPlayer = null;
        stopwatch.Stop();
        stopwatch.Reset();
        AudioController.StopPlayerFromPlaying([99]);
        AudioController.DisconnectDummy();
    }

    public readonly Regex OpeningTagRegex = new(@"<[^>]*>", RegexOptions.Compiled);
    public readonly Regex ClosingTagRegex = new(@"</[^>]*>", RegexOptions.Compiled);

    public string RemoveColorTag(string input)
    {
        return ClosingTagRegex.Replace(OpeningTagRegex.Replace(input, ""), "");
    }
}