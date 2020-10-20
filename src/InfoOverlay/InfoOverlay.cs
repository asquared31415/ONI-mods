﻿namespace InfoOverlay
{
    public class InfoOverlay : OverlayModes.Mode
    {
        public static readonly HashedString ID = nameof(InfoOverlay);
        public override HashedString ViewMode() => ID;
        public override string GetSoundName() => "Lights";
    }
}