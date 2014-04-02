using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
}
