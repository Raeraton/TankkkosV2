using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Tankkkos
{
    internal class Player : Body
    {

        bool cntrForward = false, cntrBackward = false, cntrLeft = false, cntrRight = false;


        Model model;
        Vector3 modelDirection = Vector3.Zero;

        public Camera Camera;

        Terrain terrain;

        Effect effect;

        public float CameraDistance = 5;
        public float MovementSpeed = 0.3f;

        GraphicsDevice dev;

        BasicGeometry debugSphere;

        Matrix localTransform => 
                Matrix.CreateRotationX( -MathF.PI / 2 )
                * Matrix.CreateScale(1f, 1f, 1f)
                * Matrix.CreateRotationY( -MathF.Atan2( modelDirection.Z, modelDirection.X) + MathF.PI / 2 );

        public Vector3 Position => (verlets[0].Pos + verlets[1].Pos +
            verlets[2].Pos + verlets[3].Pos) * 0.25f;
        public Vector3 Direction => verlets[0].Pos - verlets[3].Pos;
        public Vector3 Right => verlets[1].Pos - verlets[0].Pos;
        public Vector3 Up => 2 * verlets[4].Pos - verlets[0].Pos - verlets[2].Pos;
        public Matrix WorldTransform => Matrix.CreateWorld(Position + Vector3.Normalize(Up) * 0.25f,
            Vector3.Normalize(Direction), Vector3.Normalize(Up));


        public Player( GraphicsDevice dev, Terrain terrain, Vector3 position, Camera camera, Model model, Effect effect, PointLight sun ) 
        {

            this.terrain = terrain;
            this.Camera = camera;

            this.model = model;


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

            this.effect = effect;
            effect.Parameters["sunPos"].SetValue(sun.Position);
            effect.Parameters["sunShine"].SetValue(sun.Power);

            Update();

        }


        public void Draw(Camera cam)
        {

            effect.Parameters["CamPos"].SetValue(cam.Position);
            effect.Parameters["ViewProj"].SetValue(cam.View * cam.Projection);
            effect.Parameters["Color"].SetValue(new Vector3(0.1f, 0.4f, 0.1f));


            // body
            effect.Parameters["World"].SetValue(localTransform * WorldTransform);
            foreach (var part in model.Meshes[0].MeshParts)
            {
                dev.SetVertexBuffer(part.VertexBuffer);
                dev.Indices = part.IndexBuffer;

                effect.CurrentTechnique.Passes[0].Apply();
                dev.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, part.IndexBuffer.IndexCount);
            }

            // tower
            var modelDir = Vector3.Normalize(modelDirection);
            var camDir = Vector3.Normalize( this.Camera.Direction );
            effect.Parameters["World"].SetValue(
                localTransform *
                Matrix.CreateRotationY(-MathF.Atan2(camDir.Z, camDir.X) + MathF.Atan2(modelDir.Z, modelDir.X) ) *
                Matrix.CreateTranslation(new Vector3(modelDir.X, -0.45f, modelDir.Z) * -0.5f)
                * WorldTransform);
            foreach (var part in model.Meshes[1].MeshParts)
            {
                dev.SetVertexBuffer(part.VertexBuffer);
                dev.Indices = part.IndexBuffer;

                effect.CurrentTechnique.Passes[0].Apply();
                dev.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, part.IndexBuffer.IndexCount);
            }

            //cannon
            effect.Parameters["World"].SetValue(
                localTransform *
                Matrix.CreateTranslation(0f, 0.35f, 0)
                * WorldTransform );
            foreach (var part in model.Meshes[2].MeshParts)
            {
                dev.SetVertexBuffer(part.VertexBuffer);
                dev.Indices = part.IndexBuffer;

                effect.CurrentTechnique.Passes[0].Apply();
                dev.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, part.IndexBuffer.IndexCount);
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

        public void Draw()
        {
            Draw(Camera);
        }

        public void Update() 
        {
            updateInput();
            updateCamera();
        }

        private void updateInput() 
        {
            var ks = Keyboard.GetState();

            cntrForward = ks.IsKeyDown(Keys.W);
            cntrBackward = ks.IsKeyDown(Keys.S);
            cntrLeft = ks.IsKeyDown(Keys.A);
            cntrRight = ks.IsKeyDown(Keys.D);

        }

        private void updateCamera() 
        {
            var ks = Keyboard.GetState();

            if( ks.IsKeyDown( Keys.Q ))
                Camera.Direction = Vector3.Transform(Camera.Direction, Matrix.CreateRotationY( 0.05f ) );

            if (ks.IsKeyDown(Keys.E))
                Camera.Direction = Vector3.Transform(Camera.Direction, Matrix.CreateRotationY(-0.05f));

            Camera.Position = Position - Vector3.Normalize(Camera.Direction) * CameraDistance;


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

                if (onGround)
                {
                    verlets[i].AddSqFriction(d, 10f);
                    verlets[i].AddSqFriction(r, 10f);
                    verlets[i].AddSqFriction(u, 10f);
                }else
                {
                    verlets[i].AddSqFriction(d, 0.1f);
                    verlets[i].AddSqFriction(r, 0.1f);
                    verlets[i].AddSqFriction(u, 0.1f);
                }
            }


            float movementVelocity = 600f;
            if (onGround)
            {
                if (cntrForward)
                {
                    verlets[0].Acc += d * movementVelocity;
                    verlets[1].Acc += d * movementVelocity;
                    verlets[2].Acc += d * movementVelocity;
                    verlets[3].Acc += d * movementVelocity;
                }
                if (cntrBackward)
                {
                    verlets[0].Acc -= d * movementVelocity;
                    verlets[1].Acc -= d * movementVelocity;
                    verlets[2].Acc -= d * movementVelocity;
                    verlets[3].Acc -= d * movementVelocity;
                }
                if (cntrRight)
                {
                    verlets[0].Acc += d * movementVelocity;
                    verlets[1].Acc -= d * movementVelocity;
                    verlets[2].Acc -= d * movementVelocity;
                    verlets[3].Acc += d * movementVelocity;
                }
                if (cntrLeft)
                {
                    verlets[0].Acc -= d * movementVelocity;
                    verlets[1].Acc += d * movementVelocity;
                    verlets[2].Acc += d * movementVelocity;
                    verlets[3].Acc -= d * movementVelocity;
                }
            }


        }

    }
}
