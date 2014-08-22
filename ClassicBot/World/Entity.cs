
using System.Collections.Generic;

namespace ClassicBot.World {
    public class Entity {
        #region Variables
        public string Name;
        public Vector3S Location;
        public byte Yaw, Pitch;
        public sbyte PlayerId;
        #endregion

        public Entity(string name, sbyte id, short x, short y, short z, byte yaw, byte pitch) {
            Location = new Vector3S {X = x, Y = y, Z = z};
            Yaw = yaw;
            Pitch = pitch;
            PlayerId = id;
            Name = name;
        }

        /// <summary>
        /// Updates the entity's location.
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="Z"></param>
        public void UpdateLocation(short X, short Y, short Z) {
            Location.X = X;
            Location.Y = Y;
            Location.Z = Z;
        }

        /// <summary>
        /// Updates where the entity is facing.
        /// </summary>
        /// <param name="yaw"></param>
        /// <param name="pitch"></param>
        public void UpdateLook(byte yaw, byte pitch) {
            Yaw = yaw;
            Pitch = pitch;
        }
    }
}
