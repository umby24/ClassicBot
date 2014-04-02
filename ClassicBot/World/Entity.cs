using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassicBot.World {
    public class Entity {
        #region Variables
        public string Name;
        public Vector3s Location;
        public byte Yaw, Pitch;
        public sbyte PlayerID;
        #endregion

        public Entity(string _Name, sbyte ID, short X, short Y, short Z, byte _Yaw, byte _Pitch) {
            Location = new Vector3s();
            Location.X = X;
            Location.Y = Y;
            Location.Z = Z;
            Yaw = _Yaw;
            Pitch = _Pitch;
            PlayerID = ID;
            Name = _Name;
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
        /// <param name="_Yaw"></param>
        /// <param name="_Pitch"></param>
        public void UpdateLook(byte _Yaw, byte _Pitch) {
            Yaw = _Yaw;
            Pitch = _Pitch;
        }

        /// <summary>
        /// Returns an entity that matches the given Entity ID.
        /// </summary>
        /// <param name="ID">The Entity ID to search for.</param>
        /// <param name="EList">The list of entities to search within.</param>
        /// <returns></returns>
        public static Entity GetEntitybyID(sbyte ID, List<Entity> EList) {
            foreach(Entity e in EList) {
                if (e.PlayerID == ID) 
                    return e;
            }

            return null;
        }
    }
}
