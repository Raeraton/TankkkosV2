

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Reflection.Metadata.Ecma335;
using Tankkkos;

namespace TankkkosV2
{
    internal class GhostCamera
    {

        public Camera Cam;
        public float Velocity = 1.0f;
        public float RotVel = 0.035f;

        public GhostCamera(Camera cam)
        {
            Cam = cam;
        }


        public void Update()
        {

            var ks = Keyboard.GetState();

            if (ks.IsKeyDown(Keys.W))
                Cam.Position += Vector3.Normalize(Cam.Direction) * Velocity;
            if (ks.IsKeyDown(Keys.S))
                Cam.Position -= Vector3.Normalize(Cam.Direction) * Velocity;

            Cam.Direction = Vector3.Normalize(Cam.Direction);

            if (ks.IsKeyDown(Keys.D))
            {
                var cosFi = MathF.Cos(RotVel);
                var sinFi = MathF.Sin(RotVel);
                var x = Cam.Direction.X * cosFi   -   Cam.Direction.Z * sinFi;
                var z = Cam.Direction.X * sinFi   +   Cam.Direction.Z * cosFi;
                Cam.Direction.X = x;
                Cam.Direction.Z = z;
            }
            if (ks.IsKeyDown(Keys.A)) {
                var cosFi = MathF.Cos(-RotVel);
                var sinFi = MathF.Sin(-RotVel);
                var x = Cam.Direction.X * cosFi - Cam.Direction.Z * sinFi;
                var z = Cam.Direction.X * sinFi + Cam.Direction.Z * cosFi;
                Cam.Direction.X = x;
                Cam.Direction.Z = z;
            }

            if (ks.IsKeyDown(Keys.Q))
            {
                var horizintal_dir = new Vector2 (  Cam.Direction.X, Cam.Direction.Z );
                var vertical_dir = Vector2.Normalize(new Vector2( Cam.Direction.Y, horizintal_dir.Length() ));
                horizintal_dir = Vector2.Normalize(horizintal_dir);
                var cosFi = MathF.Cos(RotVel);
                var sinFi = MathF.Sin(RotVel);
                var y = vertical_dir.X * cosFi - vertical_dir.Y * sinFi;
                var hor = vertical_dir.X * sinFi + vertical_dir.Y * cosFi;
                var x = horizintal_dir.X * hor;
                var z = horizintal_dir.Y * hor;
                Cam.Direction.X = x;
                Cam.Direction.Y = y;
                Cam.Direction.Z = z;
            }
            if (ks.IsKeyDown(Keys.E))
            {
                var horizintal_dir = new Vector2(Cam.Direction.X, Cam.Direction.Z);
                var vertical_dir = Vector2.Normalize(new Vector2(Cam.Direction.Y, horizintal_dir.Length()));
                horizintal_dir = Vector2.Normalize(horizintal_dir);
                var cosFi = MathF.Cos(-RotVel);
                var sinFi = MathF.Sin(-RotVel);
                var y = vertical_dir.X * cosFi - vertical_dir.Y * sinFi;
                var hor = vertical_dir.X * sinFi + vertical_dir.Y * cosFi;
                var x = horizintal_dir.X * hor;
                var z = horizintal_dir.Y * hor;
                Cam.Direction.X = x;
                Cam.Direction.Y = y;
                Cam.Direction.Z = z;
            }

        }



    }
}
