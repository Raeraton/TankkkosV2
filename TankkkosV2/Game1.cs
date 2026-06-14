

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SharpDX.Direct2D1.Effects;
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
        GhostCamera ghostCamera;
        bool player_camera = true;

        List<Bullet> my_bullets;

        List<Enemy> Enemies;
        Model EnemyModel;

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
                sun, 128, new Vector3(16f, 1f, 16f), 4 );

            skyBox = new SkyBox(GraphicsDevice, Content.Load<Texture2D>("skybox"));

            player = new Player(GraphicsDevice, terrain, new Vector3(0, 100, 0), activeCamera,
                        Content.Load<Model>("tank"), sun );

            water = new(GraphicsDevice, Content.Load<Texture2D>("wave2"),
                Content.Load<Effect>("Water"));

            my_bullets = new List<Bullet>();
            Enemies = new List<Enemy>();
            EnemyModel = Content.Load<Model>("tank");

            ghostCamera = new GhostCamera( new Camera() );

        }


        uint updateCount = 0xffffffff;
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if(updateCount++ > 3000) 
            {
                updateCount = 0;
                Enemies.Add(new Enemy(GraphicsDevice, terrain, player.Position + Vector3.Up * 10, EnemyModel, sun));
            }

            var delataTime = (float)gameTime.ElapsedGameTime.TotalSeconds;


            //input
            if (Keyboard.GetState().IsKeyDown(Keys.Tab)) player_camera = !player_camera;
            if (player_camera)
            {
                player.Update();
                ghostCamera.Cam.Position = player.Position;
                ghostCamera.Cam.Direction = player.Direction;
                activeCamera = player.Camera;
            }
            else
            {
                ghostCamera.Update();
                activeCamera = ghostCamera.Cam;
            }


            List<Collision> collisions = new List<Collision>();
            foreach(var enemy in Enemies)
            {
                collisions.Add(enemy);
            }
            collisions.Add(player);

            player.Step(ref my_bullets, collisions);

            foreach (var enemy in Enemies)
            {
                enemy.Step(player.Position, collisions);
            }


            my_bullets.RemoveAll(b => {
                if( b.Update(delataTime))
                {
                    terrain.AddCrater(b.Position, b.Radius);
                    foreach( var e in Enemies)
                    {
                        Vector3 diff = b.Position - e.Position;
                        float dist = diff.Length();
                        if( dist < b.Radius) {
                            for(int i=0;  i<e.verlets.Length; i++)
                            {
                                e.verlets[i].pPos = e.verlets[i].Pos + diff * (b.Radius - dist) * 0.45f;
                            }
                        }
                    }
                    return true;
                }
                return false;
            });

            terrain.UpdateTerrain(player.Position);
            terrain.Update(player.Position);

            base.Update(gameTime);
        }

        long secStart = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        long frameCount = 0;
        long showFrameCount = 0;
        protected override void Draw(GameTime gameTime)
        {
            activeCamera.AspectRatio = GraphicsDevice.Viewport.AspectRatio;

            long td = DateTimeOffset.Now.ToUnixTimeMilliseconds() - secStart;
            if( td > 1000 )
            {
                showFrameCount = frameCount;
                secStart = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                frameCount = 0;
            }

            Window.Title = $"Tankkkos xd - FPS: {(showFrameCount)}";

            frameCount++;


            GraphicsDevice.BlendState = BlendState.Opaque;

            // height map
            GraphicsDevice.SetRenderTarget(heightMap);
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer,
                new Vector4(-100.0f, 0, 0, 0), 1, 0);
            terrain.DrawHeight(activeCamera);

            // water refraction
            GraphicsDevice.SetRenderTarget(waterRefraction);
            GraphicsDevice.Clear(Color.Black);
            terrain.DrawRefraction(activeCamera);


            // water reflection
            GraphicsDevice.SetRenderTarget(waterReflection);
            GraphicsDevice.Clear(Color.Green);
            var reflectionCam = activeCamera.GetReflection(Vector3.Up);

            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            player.Draw(reflectionCam);
            foreach (var b in my_bullets)
                b.Draw(reflectionCam);
            terrain.Draw(reflectionCam);
            skyBox.Draw(reflectionCam);

            // draw
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Red);

            player.Draw(activeCamera);

            foreach (var b in my_bullets)
                b.Draw(activeCamera);

            foreach(var enemy in Enemies)
            {
                enemy.Draw(activeCamera);
            }

            terrain.Draw(activeCamera);

            water.Draw(activeCamera, waterReflection, waterRefraction, heightMap, gameTime, sun.Position);

            skyBox.Draw(activeCamera);

            base.Draw(gameTime);
        }


    }
}
