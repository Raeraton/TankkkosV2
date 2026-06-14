
using Microsoft.Xna.Framework;
using System;
using Tankkkos;

namespace TankkkosV2
{
    internal abstract class Collision : Body
    {
        public struct CollisionCircle {
            public Vector3 middle=Vector3.Zero;
            public float radius=1f;
            public CollisionCircle() { }
            public CollisionCircle(Vector3 middle, float radius) {
                this.middle = middle;
                this.radius = radius;
            }
        }

        public abstract CollisionCircle[] CollisionCircles { get; }
        
        public void Collide(Collision other)
        {
            var thissCircle = this.CollisionCircles;
            var otherCircle = other.CollisionCircles;

            Vector3 acceleration = Vector3.Zero;
            foreach (var circle1 in thissCircle) {
                foreach (var circle2 in otherCircle) {
                    float distance = Vector3.Distance(circle1.middle, circle2.middle);
                    float radiusSum = circle1.radius + circle2.radius;
                    if (distance < radiusSum)
                    {
                        Vector3 direction = Vector3.Normalize(circle1.middle - circle2.middle);
                        float penetrationDepth = radiusSum - distance;
                        acceleration += direction * penetrationDepth * 1000f;
                    }
                }
            }

            for (int i = 0; i < this.verlets.Length; i++) {
                this.verlets[i].Acc += acceleration;
            }

        }

    }
}
