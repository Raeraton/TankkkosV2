using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Tankkkos;

namespace TankkkosV2
{
    internal class Enemy : Body
    {

        Model model;

        Terrain terrain;

        public float MovementSpeed = 0.3f;

        GraphicsDevice dev;
        BasicGeometry debugSphere;

        PointLight Sun;
        Matrix LocalTransform => Matrix.CreateScale(0.4f) * Matrix.CreateRotationZ(MathF.PI / 2);

        public Vector3 Position => (verlets[0].Pos + verlets[1].Pos +
            verlets[2].Pos + verlets[3].Pos) * 0.25f;
        public Vector3 Direction => verlets[0].Pos - verlets[3].Pos;
        public Vector3 Right => verlets[1].Pos - verlets[0].Pos;
        public Vector3 Up => 2 * verlets[4].Pos - verlets[0].Pos - verlets[2].Pos;
        public Matrix WorldTransform => Matrix.CreateWorld(Position /*+ Vector3.Normalize(Up) * 0.25f*/,
            Vector3.Normalize(Direction), Vector3.Normalize(Up));

        public Enemy(GraphicsDevice dev, Terrain terrain, Vector3 position, Model model, PointLight sun)
        {

            this.terrain = terrain;
            
            debugSphere = BasicGeometry.CreateSphere(dev);

            float w = 0.5f, l = 1;
            verlets =
            [
                new Verlet( new Vector3( l, 0, -w ) + position ),
                new Verlet( new Vector3( l, 0, w )  + position),
                new Verlet( new Vector3( -l, 0, w ) + position),
                new Verlet( new Vector3( -l, 0, -w ) + position ),
                new Verlet( new Vector3( 0, 1, 0 ) + position),
                new Verlet( new Vector3( 0, -1, 0 ) + position)
            ];
            GenerateFullyConnectedBody();



            this.dev = dev;

            this.Sun = sun;

            this.model = model;
            
        }

        public void Draw(Camera cam)
        {

            foreach (var mesh in model.Meshes)
            {

                var world = mesh.ParentBone.Transform * LocalTransform * WorldTransform;

                foreach (BasicEffect effect in mesh.Effects)
                {
                    /*
                    effect.LightingEnabled = true;
                    effect.PreferPerPixelLighting = true;

                    effect.DirectionalLight1.Enabled = true;
                    effect.DirectionalLight1.Direction = Vector3.Normalize(Sun.Position);
                    effect.DirectionalLight1.DiffuseColor = color;
                    effect.DirectionalLight1.SpecularColor = new Vector3(1, 1, 1);

                    effect.AmbientLightColor = color * 0.5f;

                    */

                    effect.World = world;
                    effect.View = cam.View;
                    effect.Projection = cam.Projection;
                }
                mesh.Draw();
            }



            // Debug
            float debugSphereSize = 0.3f;
            debugSphere.Effect.DiffuseColor = Color.White.ToVector3();
            foreach (var v in verlets)
            {
                debugSphere.Draw(Matrix.CreateScale(debugSphereSize) * Matrix.CreateTranslation(v.Pos),
                    cam.View, cam.Projection);
            }

            debugSphere.Effect.DiffuseColor = Color.Red.ToVector3();
            debugSphere.Draw(Matrix.CreateScale(debugSphereSize) * Matrix.CreateTranslation(Position),
                    cam.View, cam.Projection);

            debugSphere.Effect.DiffuseColor = Color.Red.ToVector3() + Color.Green.ToVector3();
            debugSphere.Draw(Matrix.CreateScale(debugSphereSize) * Matrix.CreateTranslation(Position + Vector3.Normalize(Direction) * 2f),
                    cam.View, cam.Projection);

            debugSphere.Effect.DiffuseColor = Color.Blue.ToVector3() + Color.Green.ToVector3();
            debugSphere.Draw(Matrix.CreateScale(debugSphereSize) * Matrix.CreateTranslation(Position + Vector3.Normalize(Up) * 2f),
                    cam.View, cam.Projection);

            debugSphere.Effect.DiffuseColor = Color.Blue.ToVector3() + Color.Red.ToVector3();
            debugSphere.Draw(Matrix.CreateScale(debugSphereSize) * Matrix.CreateTranslation(Position + Vector3.Normalize(Right) * 2f),
                    cam.View, cam.Projection);



        }



        public void Step()
        {
            ApplyForces();
            for (int i = 0; i < verlets.Length; i++)
                verlets[i].Step();
            ApplyConstraints();

        }

        private void ApplyForces()
        {

            float topVertexHeight = terrain.GetHeightAtPointWorld(verlets[4].Pos.X, verlets[4].Pos.Z);
            float bottomVertexHeight = terrain.GetHeightAtPointWorld(verlets[5].Pos.X, verlets[5].Pos.Z);
            bool inGround = verlets[4].Pos.Y < topVertexHeight && verlets[5].Pos.Y < bottomVertexHeight;
            bool onGround = (verlets[4].Pos.Y < topVertexHeight || verlets[5].Pos.Y < bottomVertexHeight) && !inGround;
                    
            // Gravitáció
            Vector3 g = new Vector3(0, -9.81f, 0);
            for (int i = 0; i < verlets.Length; i++)
                verlets[i].Acc = g;

            // Felhajtó erő
            for (int i = 0; i < 4; i++)
            {
                float height = verlets[i].Pos.Y;
                float terrainHeight = terrain.GetHeightAtPointWorld(verlets[i].Pos.X, verlets[i].Pos.Z);
                if (height < terrainHeight)
                    verlets[i].Acc += Vector3.Up * Math.Min((terrainHeight - height) * 100, 200);
            }


            Vector3 d = Vector3.Normalize(Direction);
            Vector3 r = Vector3.Normalize(Right);
            Vector3 u = Vector3.Normalize(Up);

            for (int i = 0; i < 4; i++)
            {

                float height = verlets[i].Pos.Y;
                float terrainHeight = terrain.GetHeightAtPointWorld(verlets[i].Pos.X, verlets[i].Pos.Z);
                if (height < terrainHeight)
                {
                    verlets[i].Acc += Vector3.Up * Math.Min((terrainHeight - height) * 50, 200);
                    verlets[i].AddSqFriction(Vector3.Up, 10);
                }

                if (onGround && false)
                {
                    verlets[i].AddSqFriction(d, 10f);
                    verlets[i].AddSqFriction(r, 10f);
                    verlets[i].AddSqFriction(u, 10f);
                }
                else
                {
                    verlets[i].AddSqFriction(d, 0.1f);
                    verlets[i].AddSqFriction(r, 1f);
                    verlets[i].AddSqFriction(u, 0.1f);
                }
            }


        }




    }
}
