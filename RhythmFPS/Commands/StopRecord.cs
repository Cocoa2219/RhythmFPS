using System;
using System.Diagnostics.CodeAnalysis;
using CommandSystem;
using Exiled.API.Features;
using RhythmFPS.API;

namespace RhythmFPS.Commands;

[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class StopRecord : ICommand
{
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
    {
        var player = Player.Get((CommandSender)sender);

        if (player == null)
        {
            response = "플레이어가 아닙니다.";
            return false;
        }

        if (!player.SessionVariables.ContainsKey("IsRecording") || player.SessionVariables["IsRecording"] == null)
        {
            response = "녹음 중이 아닙니다.";
            return false;
        }

        var song = (Song)player.SessionVariables["IsRecording"];

        song.StopRecord();
        player.SessionVariables["IsRecording"] = null;
        player.SessionVariables.Remove("IsRecording");

        response = "녹음을 중지합니다.";
        return true;
    }

    public string Command { get; } = "stoprecord";
    public string[] Aliases { get; } = { "sr" };
    public string Description { get; } = "녹음을 중지합니다.";
}