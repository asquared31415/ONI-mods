namespace TrafficVisualizer
{
    public class HotspotOverlayMode : OverlayModes.Mode
    {
        public static HashedString Id = "asquared31415_" + nameof(HotspotOverlayMode);
        public override HashedString ViewMode() => Id;
        public override string GetSoundName() => "Decor";
    }
}
