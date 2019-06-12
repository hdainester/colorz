using System;
using Chaotx.Mgx.Layout;
using Chaotx.Mgx.Views;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Chaotx.Colorz {
    public class ColorzGame : Game {
        private SpriteBatch spriteBatch;
        private GraphicsDeviceManager graphics;
        private ViewManager viewManager;
        private Session session;

        public ColorzGame() {
            Content.RootDirectory = "../content";
            IsMouseVisible = true;
            graphics = new GraphicsDeviceManager(this);
            viewManager = new ViewManager(Content, graphics);
            session = new Session();

            // graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            // graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            // graphics.HardwareModeSwitch = false;
            // graphics.IsFullScreen = true;
        }

        protected override void LoadContent() {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            var homePane = Content.Load<StackPane>("layout/panes/homepane");
            viewManager.Add(new SessionView(homePane));
            base.LoadContent();
        }

        protected override void Update(GameTime gameTime) {
            if(viewManager.Views.Count == 0) Exit();
            viewManager.Update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(Color.Black);
            viewManager.Draw(spriteBatch);
            base.Draw(gameTime);
        }
    }
}