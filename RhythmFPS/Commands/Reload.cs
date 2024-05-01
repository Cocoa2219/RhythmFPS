using System;
using System.Diagnostics.CodeAnalysis;
using CommandSystem;
using Exiled.Permissions.Extensions;
using PlayerRoles;
using RemoteAdmin;

namespace RhythmFPS.Commands;

[CommandHandler(typeof(RemoteAdminCommandHandler))]
[CommandHandler(typeof(GameConsoleCommandHandler))]
public class Reload : ICommand
{
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
    {
        if (sender is PlayerCommandSender && !sender.CheckPermission("rhythmfps.reload"))
        {
            response = "당신은 이 명령어를 실행할 권한이 없습니다.";
            return false;
        }

        RhythmFPS.Instance.EventHandler.Load();

        response = "곡들을 리로드했습니다. 자세한 내용은 로그를 확인하세요.";
        return true;
    }

    public string Command { get; } = "reloadsongs";
    public string[] Aliases { get; } = { "rls" };
    public string Description { get; } = "곡들을 리로드합니다.";
}