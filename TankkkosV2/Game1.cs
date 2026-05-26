

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using TankkkosV2;

namespace Tankkkos
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        PointLight sun = new PointLight ( 200f, 100f, 1000f, 1f, 0.4f );

        Water water;
        RenderTarget2D waterReflection, waterRefraction, heightMap;

        Terrain terrain;
        SkyBox skyBox;

        Player player;

        Camera activeCamera = Camera.Main;

        List<Bullet> my_bullets;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            //IsFixedTimeStep = false;               // disables fixed 60 FPS timing
            //_graphics.SynchronizeWithVerticalRetrace = false; // disables VSync
            //_graphics.ApplyChanges();

            Window.AllowUserResizing = true;
            Window.Title = "Tankkkos xd";

            waterReflection = new RenderTarget2D(GraphicsDevice,
                GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, false,
                SurfaceFormat.Color, DepthFormat.Depth16);
            waterRefraction = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth16);
            heightMap = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height, false, SurfaceFormat.Single, DepthFormat.Depth16);

            Window.ClientSizeChanged += (s, e) =>
            {
                _graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
                _graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
                _graphics.ApplyChanges();
                waterReflection.Dispose();
                waterReflection = new RenderTarget2D(GraphicsDevice,
                    GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, false,
                    SurfaceFormat.Color, DepthFormat.Depth16);
            };

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new Microsoft.Xna.Framework.Graphics.SpriteBatch(GraphicsDevice);

            terrain = new Terrain(GraphicsDevice, Content.Load<Effect>("Terrain"),
                Content.Load<Texture2D>("grass1"), Content.Load<Texture2D>("rock1"), Content.Load<Texture2D>("sand1"),
                sun, 128, new Vector3(16f, 1f, 16f), 3 );

            skyBox = new SkyBox(GraphicsDevice, Content.Load<Texture2D>("skybox"));

            player = new Player(GraphicsDevice, terrain, new Vector3(0, 100, 0), activeCamera,
                        Content.Load<Model>("tank"), Content.Load<Effect>("Player"), sun );

            water = new(GraphicsDevice, Content.Load<Texture2D>("wave2"),
                Content.Load<Effect>("Water"));

            my_bullets = new List<Bullet>();

        }


        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            var delataTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            player.Update();
            player.Step(ref my_bullets);

            my_bullets.RemoveAll(b => {
                if( b.Update(delataTime))
                {
                    // terrain.AddCrater(new Vector2(b.Position.X, b.Position.Z));
                    return true;
                }
                return false;
            });

            terrain.Update(player.Position);

            base.Update(gameTime);
        }

        long secStart = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        long frameCount = 0;
        protected override void Draw(GameTime gameTime)
        {
            activeCamera.AspectRatio = GraphicsDevice.Viewport.AspectRatio;

            long td = DateTimeOffset.Now.ToUnixTimeMilliseconds() - secStart;
            if( td > 1000 )
            {
                Window.Title = $"Tankkkos xd - FPS: {(frameCount)}";
                secStart = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                frameCount = 0;
            }

            frameCount++;


            GraphicsDevice.BlendState = BlendState.Opaque;

            // height map
            GraphicsDevice.SetRenderTarget(heightMap);
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer,
                new Vector4(-100.0f, 0, 0, 0), 1, 0);
            terrain.DrawHeight(Camera.Main);

            // water refraction
            GraphicsDevice.SetRenderTarget(waterRefraction);
            GraphicsDevice.Clear(Color.Black);
            terrain.DrawRefraction(Camera.Main);


            // water reflection
            GraphicsDevice.SetRenderTarget(waterReflection);
            GraphicsDevice.Clear(Color.Green);
            var reflectionCam = Camera.Main.GetReflection(Vector3.Up);

            player.Draw(reflectionCam);
            foreach (var b in my_bullets)
                b.Draw(reflectionCam);
            terrain.Draw(reflectionCam);
            skyBox.Draw(reflectionCam);

            // draw
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Red);

            player.Draw();

            foreach (var b in my_bullets)
                b.Draw(activeCamera);

            terrain.Draw(activeCamera);

            water.Draw(activeCamera, waterReflection, waterRefraction, heightMap, gameTime, sun.Position);

            skyBox.Draw(activeCamera);

            base.Draw(gameTime);
        }


    }
}
