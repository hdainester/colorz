using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

using Chaotx.Mgx.Controls.Menus;
using Chaotx.Mgx.Controls;
using Chaotx.Mgx.Layout;
using Chaotx.Mgx.Views;

using System.Collections.Generic;
using System.Linq;
using System;
using Microsoft.Xna.Framework.Input;
using Chaotx.Mgx;

namespace Chaotx.Colorz {
    public enum Difficulty {
        Easy, Medium, Hard
    }

    internal struct PositionPair {
        public GenericPosition In {get; set;}
        public GenericPosition Out {get; set;}
    };

    public class SessionView : FadingView {
        public Session Session {get; private set;}
        public int ButtonFade {get;} = 300;

        internal SlidingPane gridSlider;
        internal SlidingPane menuSlider;
        private SlidingPane gameOverSlider;
        private Difficulty difficulty;
        private Random rng;

        private GenericPosition[] sides = {
            GenericPosition.Top,
            GenericPosition.Left,
            GenericPosition.Bottom,
            GenericPosition.Right
        };

        public SessionView(LayoutPane root)
        : base (root) {
            difficulty = Difficulty.Easy;
            rng = new Random();
        }

        protected override void Init() {
            InitMenu();
            InitGrids();
            InitSlider();
        }

        protected override void HandleInput() {
            base.HandleInput();
            var keyboard = Keyboard.GetState();

            if((keyboard.IsKeyPressed(Keys.Escape)
            || keyboard.IsKeyPressed(Keys.Back))
            && gridSlider.State == SlidingPaneState.SlidedIn) {
                var rootPane = Content.Load<StackPane>("layout/panes/pausepane");
                var pauseView = new PauseView(rootPane, this);
                Manager.Add(pauseView, false);
                InputDisabled = true;
            }
        }

        public override void Update(GameTime gameTime) {
            base.Update(gameTime);

            if(Session != null && Session.IsTalking)
                Session.Say(gameTime);
        }

        private void InitMenu() {
            var mniStart = GetItem<MenuItem>("itmStart");
            var mniExit = GetItem<MenuItem>("itmExit");
            var arrLeft = GetItem<MenuItem>("itmLeft");
            var arrRight = GetItem<MenuItem>("itmRight");

            var diffContent = GetItem<HPane>("diffContent");
            var itmEasy = GetItem<TextItem>("itmEasy");
            var itmMedium = GetItem<TextItem>("itmMedium");
            var itmHard = GetItem<TextItem>("itmHard");
            diffContent.Clear();
            diffContent.Add(itmEasy);

            var oldStartCol = mniStart.TextItem.Color;
            var oldExitCol = mniExit.TextItem.Color;
            var oldLeftCol = arrLeft.TextItem.Color;
            var oldRightCol = arrRight.TextItem.Color;

            mniStart.FocusGain += (s, a) => mniStart.TextItem.Color = Color.Yellow;
            mniStart.FocusLoss += (s, a) => mniStart.TextItem.Color = oldStartCol;
            mniExit.FocusGain += (s, a) => mniExit.TextItem.Color = Color.Yellow;
            mniExit.FocusLoss += (s, a) => mniExit.TextItem.Color = oldExitCol;
            arrLeft.FocusGain += (s, a) => arrLeft.TextItem.Color = Color.Yellow;
            arrLeft.FocusLoss += (s, a) => arrLeft.TextItem.Color = oldLeftCol;
            arrRight.FocusGain += (s, a) => arrRight.TextItem.Color = Color.Yellow;
            arrRight.FocusLoss += (s, a) => arrRight.TextItem.Color = oldLeftCol;
            arrLeft.Disabled += (s, a) => arrLeft.Alpha = 0.5f;
            arrLeft.Enabled += (s, a) => arrLeft.Alpha = 1f;
            arrRight.Disabled += (s, a) => arrRight.Alpha = 0.5f;
            arrRight.Enabled += (s, a) => arrRight.Alpha = 1f;

            arrLeft.IsDisabled = true;
            arrLeft.Action += (s, a) => {
                difficulty -= 1;
                diffContent.Clear();
                diffContent.Add(difficulty == Difficulty.Easy
                    ? itmEasy : difficulty == Difficulty.Medium
                    ? itmMedium : itmHard);
                arrLeft.IsDisabled = difficulty == Difficulty.Easy;
                arrRight.IsDisabled = difficulty == Difficulty.Hard;
            };

            arrRight.Action += (s, a) => {
                difficulty += 1;
                diffContent.Clear();
                diffContent.Add(difficulty == Difficulty.Easy
                    ? itmEasy : difficulty == Difficulty.Medium
                    ? itmMedium : itmHard);
                arrRight.IsDisabled = difficulty == Difficulty.Hard;
                arrLeft.IsDisabled = difficulty == Difficulty.Easy;
            };

            mniExit.Action += (s, a) => Close();
            mniStart.Action += (s, a) => {
                CreateSession(difficulty, 2*ButtonFade + 100);
                var pair = RandomPositionPair();
                menuSlider.SlideOut(pair.Out);
                gridSlider.SlideIn(pair.In);
            };
        }

        private void InitSlider() {
            menuSlider = GetItem<SlidingPane>("menuSlider");
            gridSlider = GetItem<SlidingPane>("gridSlider");
            gameOverSlider = GetItem<SlidingPane>("gameOverSlider");

            gridSlider.SlidedIn += (s, a) =>
                Session.EvaluateNextTiles();

            gameOverSlider.SlidedIn += (s, a) => {
                var pair = RandomPositionPair();
                gameOverSlider.SlideOut(pair.Out);
                menuSlider.SlideIn(pair.In);
            };
        }

        private void InitGrids() {
            var sgrid = GetItem<GridPane>("smallGrid");
            var mgrid = GetItem<GridPane>("mediumGrid");
            var lgrid = GetItem<GridPane>("largeGrid");
            var grids = new GridPane[] {sgrid, mgrid, lgrid};

            foreach(var grid in grids)
            for(int x, y = 0, i = 0; y < grid.GridHeight; ++y)
            for(x = 0; x < grid.GridWidth; ++x, ++i) {
                var cell = grid.Get(x, y) as StackPane;
                var mni = cell.Children[0] as MenuItem;
                var fader = cell.Children[1] as FadingPane;
                var complete = false;
                int index = i;

                mni.ImageItem.Color = RandomColor();
                mni.Action += (s, a) => {
                    if(fader.State != FadingPaneState.FadedIn)
                        return;

                    if(Session.IsListening) {
                        complete = Session.Listen(index);
                        fader.FadeOut();
                    }
                };

                fader.FadedOut += (s, a) => fader.FadeIn();
                fader.FadedIn += (s, a) => {
                    if(complete) {
                        Session.EvaluateNextTiles();
                        complete = false;
                    }
                };
            }
        }

        private void CreateSession(Difficulty d, int t) {
            var gid = d == Difficulty.Easy
                ? "smallGrid": d == Difficulty.Medium
                ? "mediumGrid" : "largeGrid";

            var grid = GetItem<GridPane>(gid);
            var gpane = GetItem<StackPane>("gridPane");
            var gameOverText = GetItem<TextItem>("gameOverText");
            gpane.Clear();
            gpane.Add(grid);

            int w = grid.GridWidth;
            int h = grid.GridHeight;
            Session = new Session(w, h, t);

            Session.NewIndex += (s, a) => {
                int y = a.Index/grid.GridWidth;
                int x = a.Index%grid.GridWidth;
                var fader = (grid.Get(x, y) as StackPane).Children[1] as FadingPane;
                fader.FadeOut();
                Console.WriteLine("Session says " + a.Index);
            };

            Session.GameOver += (s, a) => {
                var pair = RandomPositionPair();
                gridSlider.SlideOut(pair.Out);
                gameOverSlider.SlideIn(pair.In);
                gameOverText.Text = string.Format("GameOver after {0} turn{1}", a.Turns, a.Turns == 1 ? "" : "s");
                Console.WriteLine(string.Format("GameOver after {0} turn{1}", a.Turns, a.Turns == 1 ? "" : "s"));
            };
        }

        private Color RandomColor() {
            return Color.FromNonPremultiplied(
                rng.Next(255) + 1,
                rng.Next(255) + 1,
                rng.Next(255) + 1, 255);
        }

        internal PositionPair RandomPositionPair() {
            PositionPair pair = new PositionPair();
            var r = Session.Rng.Next(sides.Length);
            pair.In = sides[r];
            pair.Out = sides[(r+sides.Length/2)%sides.Length];
            return pair;
        }
    }
}