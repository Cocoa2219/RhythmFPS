using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CommandSystem;
using Exiled.API.Features;
using RhythmFPS.API;

namespace RhythmFPS.Commands;

[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class StopPlay : ICommand
{
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
    {
        var player = Player.Get((CommandSender)sender);

        if (player == null)
        {
            response = "플레이어가 아닙니다.";
            return false;
        }

        if (Round.IsLobby)
        {
            response = "로비에서는 사용할 수 없습니다.";
            return false;
        }

        if (!int.TryParse(arguments.At(0), out var id))
        {
            response = "정확한 ID를 입력해주세요.";
            return false;
        }

        var song = RhythmFPS.Instance.EventHandler.Songs.FirstOrDefault(x => x.Id == id);

        if (song == null)
        {
            response = "해당 ID의 곡을 찾을 수 없습니다.";
            return false;
        }

        if (player.SessionVariables.ContainsKey("IsRecording") && player.SessionVariables["IsRecording"] != null)
        {
            response = "녹음 중에는 사용할 수 없습니다.";
            return false;
        }

        if (!player.SessionVariables.ContainsKey("IsPlaying") || player.SessionVariables["IsPlaying"] == null)
        {
            response = "재생 중에는 사용할 수 없습니다.";
            return false;
        }

        song.StopPlay();

        response = "재생을 중지합니다.";
        return true;
    }

    public string Command { get; } = "stopplay";
    public string[] Aliases { get; } = { "sp" };
    public string Description { get; } = "재생을 중지합니다.";
    public bool SanitizeResponse { get; } = false;
}