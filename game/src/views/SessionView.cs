using System;
using System.Collections.Generic;
using Chaotx.Mgx.Controls;
using Chaotx.Mgx.Controls.Menus;
using Chaotx.Mgx.Layout;
using Chaotx.Mgx.Views;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Chaotx.Colorz {
    public class SessionView : FadingView {
        public Session Session {get; private set;}
        public int ButtonFade {get;} = 300;

        private GridPane grid;
        private TextItem gameOverText;
        private SlidingPane gridSlider;
        private SlidingPane menuSlider;
        private SlidingPane gameOverSlider;

        private int gridWidth = 3;
        private int gridHeight = 3;
        private GenericPosition[] sides = {
            GenericPosition.Top,
            GenericPosition.Left,
            GenericPosition.Bottom,
            GenericPosition.Right
        };

        struct PositionPair {
            public GenericPosition In {get; set;}
            public GenericPosition Out {get; set;}
        };

        public SessionView(LayoutPane root)
        : base (root) {}

        protected override void Init() {
            grid = GetItem<GridPane>("gridPane");
            menuSlider = GetItem<SlidingPane>("menuSlider");
            gridSlider = GetItem<SlidingPane>("gridSlider");
            gameOverText = GetItem<TextItem>("gameOverText");
            gameOverSlider = GetItem<SlidingPane>("gameOverSlider");

            var mniStart = GetItem<MenuItem>("mniStart");
            var mniExit = GetItem<MenuItem>("mniExit");

            var oldStartCol = mniStart.TextItem.Color;
            var oldExitCol = mniExit.TextItem.Color;

            mniStart.FocusGain += (s, a) => mniStart.TextItem.Color = Color.Yellow;
            mniStart.FocusLoss += (s, a) => mniStart.TextItem.Color = oldStartCol;
            mniExit.FocusGain += (s, a) => mniExit.TextItem.Color = Color.Yellow;
            mniExit.FocusLoss += (s, a) => mniExit.TextItem.Color = oldExitCol;

            mniExit.Action += (s, a) => Close();
            mniStart.Action += (s, a) => {
                CreateSession(gridWidth, gridHeight, 2*ButtonFade + 100);
                mniStart.IsDisabled = true;
                mniExit.IsDisabled = true;
                var pair = RandomPositionPair();
                menuSlider.SlideOut(pair.Out);
                gridSlider.SlideIn(pair.In);
            };

            gridSlider.SlidedIn += (s, a) => Session.EvaluateNextTiles();
            gameOverSlider.SlidedIn += (s, a) => {
                var pair = RandomPositionPair();
                gameOverSlider.SlideOut(pair.Out);
                menuSlider.SlideIn(pair.In);
            };

            menuSlider.SlidedIn += (s, a) => {
                mniStart.IsDisabled = false;
                mniExit.IsDisabled = false;
            };

            CreateSession(gridWidth, gridHeight, 2*ButtonFade + 100);
        }

        private void CreateSession(int w, int h, int t) {
            var faders = new List<FadingPane>();
            Session = new Session(w, h, t);

            for(int x, y = 0, i = 0; y < h; ++y) {
                for(x = 0; x < w; ++x, ++i) {
                    var cell = CreateCell();
                    var mni = cell.Children[0] as MenuItem;
                    var fader = cell.Children[1] as FadingPane;
                    int index = i;

                    bool complete = false;
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

                    faders.Add(fader);
                    grid.Set(x, y, cell);
                }
            }

            Session.NewIndex += (s, a) => {
                faders[a.Index].FadeOut();
                Console.WriteLine("Session says " + a.Index);
            };

            Session.GameOver += (s, a) => {
                var pair = RandomPositionPair();
                gridSlider.SlideOut(pair.Out);
                gameOverSlider.SlideIn(pair.In);
                gameOverText.Text = string.Format("GameOver after {0} turns", a.Turns);
                Console.WriteLine(string.Format("GameOver after {0} turns", a.Turns));
            };
        }

        public override void Update(GameTime gameTime) {
            base.Update(gameTime);

            if(Session.IsTalking)
                Session.Say(gameTime);
        }

        private StackPane CreateCell() {
            var bln = Content.Load<Texture2D>("textures/blank");
            var mni = new MenuItem(bln);
            mni.HGrow = mni.VGrow = 1;
            // mni.ImageItem.KeepAspectRatio = true;
            mni.ImageItem.HAlign = HAlignment.Center;
            mni.ImageItem.VAlign = VAlignment.Center;
            mni.FocusEffect = false;

            var imi = new ImageItem(bln);
            imi.HGrow = imi.VGrow = 1;
            // imi.KeepAspectRatio = true;
            imi.Color = Color.Black;
            imi.HAlign = HAlignment.Center;
            imi.VAlign = VAlignment.Center;
            imi.Alpha = 0.5f;

            var fad = new FadingPane(-1, FadingPaneState.FadedIn);
            fad.FadeInTime = ButtonFade;
            fad.FadeOutTime = ButtonFade;
            fad.HGrow = fad.VGrow = 1;
            fad.Add(imi);

            var stk = new StackPane(mni, fad);
            stk.HGrow = stk.VGrow = 1;
            return stk;
        }

        private Color RandomColor() {
            return Color.FromNonPremultiplied(
                Session.Rng.Next(255) + 1,
                Session.Rng.Next(255) + 1,
                Session.Rng.Next(255) + 1, 255);
        }

        private PositionPair RandomPositionPair() {
            PositionPair pair = new PositionPair();
            var r = Session.Rng.Next(sides.Length);
            pair.In = sides[r];
            pair.Out = sides[(r+sides.Length/2)%sides.Length];
            return pair;
        }
    }
}