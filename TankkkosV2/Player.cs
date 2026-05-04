using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Tankkkos
{
    internal class Player
    {

        Model model;
        Vector3 modelDirection = Vector3.Zero;

        public Camera Camera;

        Terrain terrain;
        public Vector3 Position;

        Effect effect;

        public float CameraDistance = 5;
        public float MovementSpeed = 0.3f;

        GraphicsDevice dev;

        Matrix localTransform => Matrix.Identity
                * Matrix.CreateRotationX( -MathF.PI / 2 )
                * Matrix.CreateScale(1f, 1f, 1f)
                * Matrix.CreateRotationY( -MathF.Atan2( modelDirection.Z, modelDirection.X) );
        Matrix worldTransform => Matrix.CreateTranslation(Position);


        public Player( GraphicsDevice dev, Terrain terrain, Vector3 position, Camera camera, Model model, Effect effect, PointLight sun ) 
        {

            this.terrain = terrain;
            this.Position = position;
            this.Camera = camera;

            this.model = model;


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
            effect.Parameters["World"].SetValue(localTransform * worldTransform);
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
                * worldTransform);
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
                * worldTransform );
            foreach (var part in model.Meshes[2].MeshParts)
            {
                dev.SetVertexBuffer(part.VertexBuffer);
                dev.Indices = part.IndexBuffer;

                effect.CurrentTechnique.Passes[0].Apply();
                dev.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, part.IndexBuffer.IndexCount);
            }


        }

        public void Draw()
        {
            Draw(Camera);
        }

        public void Update() 
        {
            updatePosition();
            updateCamera();
        }

        private void updatePosition() 
        {
            if (Camera == null) return;

            var camDir = Vector3.Normalize(Camera.Direction ) * MovementSpeed;

            var ks = Keyboard.GetState();
            if (ks.IsKeyDown(Keys.W))
                Position += new Vector3( camDir.X, 0, camDir.Z );
            if (ks.IsKeyDown(Keys.S))
                Position -= new Vector3(camDir.X, 0, camDir.Z);
            if (ks.IsKeyDown(Keys.A))
                Position -= new Vector3(-camDir.Z, 0, camDir.X);
            if (ks.IsKeyDown(Keys.D))
                Position += new Vector3(-camDir.Z, 0, camDir.X);
            if( !ks.IsKeyDown(Keys.LeftShift))
                modelDirection = Vector3.Normalize(Camera.Direction );

            Position.Y = terrain.GetHeightAtPointWorld(Position.X, Position.Z) + 1;
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

    }
}
