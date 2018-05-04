using ClassicBot.Common;

namespace ClassicBot.World {
    public class Entity {
        public string Name { get; set; }
        public sbyte PlayerId { get; set; }
        public Vector3S Position { get; set; }
        public byte Yaw { get; set; }
        public byte Pitch { get; set; }

    }
}