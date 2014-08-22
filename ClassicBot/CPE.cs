namespace ClassicBot {
    
    public enum CPEExtensions {
        ClickDistance,
        CustomBlocks,
        HeldBlock,
        EmoteFix,
        TextHotKey,
        ExtPlayerList,
        EnvColors,
        SelectionCuboid,
        BlockPermissions,
        ChangeModel,
        EnvMapAppearance,
        EnvWeatherType,
        HackControl,
        MessageTypes
    }

    public enum HotkeyModifier {
        None = 0,
        Ctrl,
        Shift,
        Alt
    }

    public struct TextHotKeyEntry {
        public string Label;
        public string Action;
        public int Keycode;
        public HotkeyModifier Modifier;
    }

    public struct ExtPlayerListEntry {
        public short NameId;
        public string PlayerName;
        public string ListName;
        public string GroupName;
        public byte GroupRank;
    }
}
