using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CommandSystem;
using Exiled.API.Features;

namespace RhythmFPS.Commands;

[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class StopRecordTiming : ICommand
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

        song.StopTimingRecord();

        response = "녹음을 중지합니다.";
        return true;
    }

    public string Command { get; } = "stoprecordtiming";
    public string[] Aliases { get; } = { "srt" };
    public string Description { get; } = "박자 녹음을 중지합니다.";
    public bool SanitizeResponse { get; }
}