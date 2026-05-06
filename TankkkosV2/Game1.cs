

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Runtime.CompilerServices;
using TankkkosV2;

namespace Tankkkos
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        PointLight sun = new PointLight ( 200f, 100f, 1000f, 1f, 0.4f );

        Water water;
        RenderTarget2D waterReflection;

        Terrain terrain;
        SkyBox skyBox;

        Player player;

        Camera activeCamera = Camera.Main;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            Window.AllowUserResizing = true;
            Window.Title = "Tankkkos xd";

            waterReflection = new RenderTarget2D(GraphicsDevice,
                GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, false,
                SurfaceFormat.Color, DepthFormat.Depth16);

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
                sun, 128, new Vector3(16f, 1f, 16f), 4 );

            skyBox = new SkyBox(GraphicsDevice, Content.Load<Texture2D>("skybox"));

            player = new Player(GraphicsDevice, terrain, new Vector3(0, 100, 0), activeCamera,
                        Content.Load<Model>("tank"), Content.Load<Effect>("Player"), sun );

            water = new(GraphicsDevice, Content.Load<Texture2D>("wave2"),
                Content.Load<Effect>("Water"));

        }


        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            player.Update();
            player.Step();

            terrain.Update(player.Position);

            base.Update(gameTime);
        }

        long secStart = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        uint frameCounter = 0;
        protected override void Draw(GameTime gameTime)
        {
            activeCamera.AspectRatio = GraphicsDevice.Viewport.AspectRatio;
            Window.Title = $"Tankkkos xd - FPS: {frameCounter * 1000 / (DateTimeOffset.Now.ToUnixTimeMilliseconds() - secStart)}     map size: {terrain.mountain.getSize()}";

            // water reflection
            GraphicsDevice.SetRenderTarget(waterReflection);
            GraphicsDevice.Clear(Color.Green);
            var reflectionCam = Camera.Main.GetReflection(Vector3.Up);

            player.Draw(reflectionCam);
            terrain.Draw(reflectionCam, false);
            skyBox.Draw(reflectionCam);

            // draw
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Red);

            player.Draw();

            terrain.Draw(activeCamera, true);

            water.Draw(activeCamera, waterReflection, gameTime, sun.Position);

            skyBox.Draw(activeCamera);

            base.Draw(gameTime);
        }


    }
}
