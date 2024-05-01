using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CommandSystem;

namespace RhythmFPS.Commands;

[CommandHandler(typeof(RemoteAdminCommandHandler))]
[CommandHandler(typeof(GameConsoleCommandHandler))]
public class List : ICommand
{
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
    {
        response = RhythmFPS.Instance.EventHandler.Songs.Aggregate("\n<b><color=white>등록된 곡 목록</color></b>\n\n", (current, song) => current + $"<b><color=white>{song.Id}</color></b> - <b><color=yellow>{song.SongDirectory}</color></b>\n");
        return true;
    }

    public string Command { get; } = "slist";
    public string[] Aliases { get; } = { "sl" };
    public string Description { get; } = "등록된 곡 목록을 보여줍니다.";
}