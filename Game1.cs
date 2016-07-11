using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Water_Simulation
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private Effect _lightingTextureShader;// Ambient, diffuse, specular lighting shader.

        private Camera _camera;
        private float[] _ambientLightColor;
        private DirectionalLight _dirLight;

        private Water _water;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // Set the size of the window.
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;

            // Position the window on the screen.
            Window.Position = new Point(40, 40);

            // Enable mouse cursor to be visible.
            IsMouseVisible = true;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // Create camera.
            _camera = new Camera(this, new Vector3(1, 20, 1), new Vector3(5,1,5), Vector3.Up,
                MathHelper.PiOver2);

            // Objects to list of game components.
            Components.Add(_camera);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Load shader.
            _lightingTextureShader = Content.Load<Effect>(@"Effects/AmbDiffSpecTextureShader");

            // Create direcitonal light.
            _dirLight = new DirectionalLight(this, Vector3.Up, new Vector3(1.0f, 1.0f, 1.0f));

            // Create water.
            _water = new Water(this, Vector3.Zero, 50, 50, Content.Load<Texture2D>(@"Textures/water_texture_2"));

            // Assign ambient color.
            _ambientLightColor = new float[3]; // Create array.
            for (int i = 0; i < 3; ++i) _ambientLightColor[i] = 0.2f; // Assign all vaules of array.
            
            base.LoadContent();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Calculate time between frames.
            float deltaTime = (float)(gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0);

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // Update camera.
            _camera.update(deltaTime, Keyboard.GetState(), Mouse.GetState());

            // Update water model.
            _water.update(deltaTime);

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _water.draw();

            base.Draw(gameTime);
        }


        public void draw_lightingTextureShader(Matrix mWorld, Texture2D texture, 
            ref VertexPositionNormalTexture[] pnverts, int numTriangles)
        {
            float[] temp = _dirLight.getDirection();

            // Pass information about model to shader.
            _lightingTextureShader.Parameters["World"].SetValue(mWorld);
            _lightingTextureShader.Parameters["ModelTexture"].SetValue(texture);
            // Pass information about camera to shader.
            _lightingTextureShader.Parameters["CamPos"].SetValue(_camera.getPosition());
            _lightingTextureShader.Parameters["View"].SetValue(_camera.view);
            _lightingTextureShader.Parameters["Projection"].SetValue(_camera.proj);

            float[] lightDir = _dirLight.getDirection();
            float[] lightDirClr = _dirLight.getColor();

            // Pass lighting information to shader.
            for(int i = 0; i < 3; ++i)
            {
                _lightingTextureShader.Parameters["LightDir"].GetValueSingleArray()[i] = (lightDir[i]);
                _lightingTextureShader.Parameters["LightColor"].GetValueSingleArray()[i] = lightDirClr[i];
                _lightingTextureShader.Parameters["AmbientColor"].SetValue(_ambientLightColor[i]);
            }

            // Draw model.
            foreach (EffectPass pass in _lightingTextureShader.CurrentTechnique.Passes)
            {
                // Begins pass.
                pass.Apply();

                // Draw primitives.
                GraphicsDevice.DrawUserPrimitives<VertexPositionNormalTexture>
                    (PrimitiveType.TriangleList, pnverts, 0, numTriangles);
            }
        }
    }
}
