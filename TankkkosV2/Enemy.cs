using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Tankkkos;

namespace TankkkosV2
{
    internal class Enemy : Collision
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

        public override CollisionCircle[] CollisionCircles => [
            new CollisionCircle(Position+Vector3.Normalize(Up)*0.5f+Vector3.Normalize(Direction)*0.5f, 1f),
            new CollisionCircle(Position+Vector3.Normalize(Up)*0.5f-Vector3.Normalize(Direction)*0.5f, 1f)
        ];

        float[] sunDirFactors;
        Vector3[] colors;

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
                new Verlet( new Vector3( l, 0.6f, -w ) + position),
                new Verlet( new Vector3( l, 0.6f, w ) + position),
                new Verlet( new Vector3( -l, 0.6f, w ) + position),
                new Verlet( new Vector3( -l, 0.6f, -w ) + position)
            ];
            GenerateFullyConnectedBody();



            this.dev = dev;

            this.Sun = sun;

            this.model = model;


            // model bullshit
            ///////////////////////////////////
            this.model = model;
            sunDirFactors = new float[model.Meshes.Count];
            colors = new Vector3[model.Meshes.Count];
            for (int i = 0; i < model.Meshes.Count; i++)
            {
                sunDirFactors[i] = -1.0f;
                colors[i] = new Vector3(1, 1, 1);
            }

            sunDirFactors[0] = 1.0f;
            colors[0] = new Vector3(0.5f, 0.5f, 0.2f); // cso
            colors[1] = new Vector3(0.5f, 0.5f, 0.2f); // test

            sunDirFactors[2] = 1.0f; // lanctalp
            sunDirFactors[3] = 1.0f;
            colors[2] = new Vector3(0.2f);
            colors[3] = new Vector3(0.2f);

            colors[4] = new Vector3(0.5f, 0.5f, 0.2f); // torony

            sunDirFactors[5] = 1.0f; // felfuggesztes
            sunDirFactors[6] = 1.0f;
            colors[5] = new Vector3(0.4f);
            colors[6] = new Vector3(0.4f);

            /////////////////////////////////////


        }

        public void Draw(Camera cam)
        {

            int index = 0;
            foreach (var mesh in model.Meshes)
            {
                float sunDirFact = sunDirFactors[index];
                Vector3 color = colors[index];

                var world = mesh.ParentBone.Transform * LocalTransform * WorldTransform;

                index++;

                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.LightingEnabled = true;
                    effect.PreferPerPixelLighting = true;

                    effect.DirectionalLight1.Enabled = true;
                    effect.DirectionalLight1.Direction = sunDirFact * Vector3.Normalize(Sun.Position);
                    effect.DirectionalLight1.DiffuseColor = color;
                    effect.DirectionalLight1.SpecularColor = new Vector3(1, 1, 1);

                    effect.AmbientLightColor = color * 0.5f;

                    effect.World = world;
                    effect.View = cam.View;
                    effect.Projection = cam.Projection;
                }
                mesh.Draw();
            }



            // Debug
            /*
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

            foreach (var c in CollisionCircles)
            {
                debugSphere.Effect.DiffuseColor = Color.Yellow.ToVector3();
                debugSphere.Draw(Matrix.CreateScale(c.radius) * Matrix.CreateTranslation(c.middle),
                    cam.View, cam.Projection);
            }
            */


        }



        public void Step(Vector3 playerPos, List<Collision> collisions)
        {
            ApplyForces(playerPos, collisions);
            for (int i = 0; i < verlets.Length; i++)
                verlets[i].Step();
            ApplyConstraints();

        }

        private void ApplyForces( Vector3 playerPos, List<Collision> collisions )
        {

            // Gravitáció
            Vector3 g = new Vector3(0, -9.81f, 0);
            for (int i = 0; i < verlets.Length; i++)
                verlets[i].Acc = g;

            Vector3 d = Vector3.Normalize(Direction);
            Vector3 r = Vector3.Normalize(Right);
            Vector3 u = Vector3.Normalize(Up);


            // ground
            for (uint i = 0; i < verlets.Length; i++)
            {
                float terrainHeight = terrain.GetHeightAtPointWorld(verlets[i].Pos.X, verlets[i].Pos.Z);
                if (verlets[i].Pos.Y <= terrainHeight)
                {
                    Vector3 terrainNormal = terrain.GetNormalAtPoint(verlets[i].Pos.X, verlets[i].Pos.Z);
                    verlets[i].pPos.Y = verlets[i].Pos.Y - (terrainHeight - verlets[i].Pos.Y - 0.1f);
                }
            }


            // drag
            bool[] verletOnGround = { false, false, false, false };
            for (uint i = 0; i < verlets.Length; i++)
            {

                float terrainHeight = terrain.GetHeightAtPointWorld(verlets[i].Pos.X, verlets[i].Pos.Z);

                if (verlets[i].Pos.Y > terrainHeight)
                {  // air resistanse
                    verlets[i].AddSqFriction(d, 0.05f);
                    verlets[i].AddSqFriction(r, 0.05f);
                    verlets[i].AddSqFriction(u, 0.05f);
                }
                else
                {
                    verlets[i].AddSqFriction(d, 2f);
                    verlets[i].AddSqFriction(r, 10f);
                    verlets[i].AddSqFriction(u, 0.05f);
                    if (i < 4) verletOnGround[i] = true;
                }

            }

            

            float movementVelocity = 40f;

            Vector3 left = Vector3.Normalize(-r);
            Vector3 right = Vector3.Normalize(r);
            Vector3 playerDir = Vector3.Normalize(Position - playerPos);
            float leftThrotle = 1f - Vector3.Dot(left, playerDir);
            float rightThrotle = 1f - Vector3.Dot(right, playerDir);

            Vector3[] accs = { Vector3.Zero, Vector3.Zero, Vector3.Zero, Vector3.Zero };
            LeftSideThrotle(ref accs, verletOnGround, d, leftThrotle * movementVelocity);
            RightSideThrotle(ref accs, verletOnGround, d, rightThrotle * movementVelocity);

            for(int i = 0; i < 4; i++)
            {
                verlets[i].Acc += accs[i];
            }

            foreach (var c in collisions)
            {
                if (c == this) continue;
                Collide(c);
            }


        }

        void LeftSideThrotle(ref Vector3[] accs, bool[] verletOnGround, Vector3 dir, float force)
        {
            if(verletOnGround[1]) accs[1] += dir * force;
            if (verletOnGround[2]) accs[2] += dir * force;
        }
        void RightSideThrotle(ref Vector3[] accs, bool[] verletOnGround, Vector3 dir, float force)
        {
            if (verletOnGround[0]) accs[0] += dir * force;
            if (verletOnGround[3]) accs[3] += dir * force;
        }




    }
}
