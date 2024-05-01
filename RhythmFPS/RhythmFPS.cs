using System;
using System.IO;
using Exiled.API.Features;
using HarmonyLib;
using Player = Exiled.Events.Handlers.Player;

namespace RhythmFPS
{
    public class RhythmFPS : Plugin<Config>
    {
        public static RhythmFPS Instance { get; private set; }

        public override string Name { get; } = "RhythmFPS";
        public override string Author { get; } = "Cocoa";
        public override Version Version { get; } = new(1, 0, 0);

        public EventHandler EventHandler { get; private set; }

        public override void OnEnabled()
        {
            base.OnEnabled();

            Instance = this;
            EventHandler = new EventHandler(this);

            Player.Shooting += EventHandler.OnShooting;

            EventHandler.Load();

            CheckForPath();
        }

        private void CheckForPath()
        {
            if (!Directory.Exists(Path.Combine(Paths.Configs, "RhythmFPS")))
            {
                Log.Warn($"{Path.Combine(Paths.Configs, "RhythmFPS")} 폴더가 없습니다. 생성합니다.");
                Directory.CreateDirectory(Path.Combine(Paths.Configs, "RhythmFPS"));
            }
        }

        public override void OnDisabled()
        {
            Player.Shooting -= EventHandler.OnShooting;

            EventHandler = null;
            Instance = null;

            base.OnDisabled();
        }
    }
}