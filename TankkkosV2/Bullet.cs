using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Tankkkos
{
    internal class Bullet
    {

        static Vector3 G = new Vector3(0, -9.81f, 0);

        ulong id;
        public Vector3 Position;
        Vector3 Velocity;

        public float Radius = 3.0f;

        Terrain terrain;

        BasicGeometry sphere;

        public Bullet( GraphicsDevice dev, Random random, Terrain terrain, Vector3 pos, Vector3 vel) {

            this.terrain = terrain;

            id = (ulong)random.NextInt64();
            Position = pos;
            Velocity = vel;

            sphere = BasicGeometry.CreateSphere(dev);
            sphere.Effect.DiffuseColor = new Vector3(0, 0, 0);
        
        }

        public bool Update(float deltaTime) {

            Position += Velocity * deltaTime;

            Velocity += G * deltaTime;

            return TestForCollission();

        }

        bool TestForCollission() {
            return Position.Y < terrain.GetHeightAtPointWorld(Position.X, Position.Z);
        }

        public void Draw(Camera cam) {
            sphere.Draw(Matrix.CreateScale(0.4f) * Matrix.CreateTranslation(Position), cam.View, cam.Projection);
        }

    }
}
