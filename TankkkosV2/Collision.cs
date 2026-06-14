using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Tankkkos;

namespace TankkkosV2
{
    abstract internal class Collision : Body
    {
        public struct CollisionCircle {
            public Vector3 middle;
            public float radius;
        }

        public virtual CollisionCircle[] CollisionCircles { get; }
        
        public void Collide(Collision other)
        {
            var thissCircle = this.CollisionCircles;
            var otherCircle = other.CollisionCircles;

            Vector3 acceleration = Vector3.Zero;
            foreach (var circle1 in thissCircle) {
                foreach (var circle2 in otherCircle) {
                    float distance = (circle1.middle - circle2.middle).Length();
                    if (distance > circle1.radius + circle2.radius) continue;
                    float force = distance / (circle1.radius + circle2.radius);
                    force = 1f - MathF.Pow( force, 2 );
                    acceleration += force * Vector3.Normalize(circle2.middle - circle1.middle);
                }
            }

        }

    }
}
