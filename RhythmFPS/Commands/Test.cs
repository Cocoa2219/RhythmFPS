using System;
using System.Diagnostics.CodeAnalysis;
using CommandSystem;
using Exiled.API.Features;

namespace RhythmFPS.Commands;

[CommandHandler(typeof(ClientCommandHandler))]
public class Test : ICommand
{
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
    {
        var player = Player.Get((CommandSender)sender);

        if (player == null)
        {
            response = "플레이어가 아닙니다.";
            return false;
        }

        var text = string.Join(" ", arguments);

        player.ShowHint(text, 10f);

        response = "힌트를 띄웁니다.";
        return true;
    }

    public string Command { get; } = "test";
    public string[] Aliases { get; } = { "t" };
    public string Description { get; } = "테스트 명령어입니다.";
}