

using Microsoft.Xna.Framework;
using System;

namespace TankkkosV2
{
    internal struct Krater
    {
        public Vector2 Position;
        public float Radius;

        public Krater( Vector2 position, float radius ) {
            Position = position;
            Radius = radius;
        }

        public float GetImpackAtPoint( Vector2 coord)
        {
            var diff = Position - coord;
            float len = diff.Length();
            if (len > Radius) return 0;

            return - ( 1f - MathF.Pow(len/Radius, 2f) ) * Radius / 2f;
        }

    }
}
