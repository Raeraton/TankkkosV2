

using Microsoft.Xna.Framework;

namespace Tankkkos
{
    struct PointLight
    {
        public Vector3 Position;
        public float Strenght; // texture
        public float Power;    // shine

        public PointLight( Vector3 pos, float s, float p)
        {
            Position = pos;
            Strenght = s;
            Power = p;
        }

        public PointLight( float x, float y, float z, float s, float p) {
            Position = new Vector3( x, y, z );
            Strenght = s;
            Power = p;
        }
    }
}
