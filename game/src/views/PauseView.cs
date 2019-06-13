using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

using Chaotx.Mgx.Controls.Menus;
using Chaotx.Mgx.Layout;
using Chaotx.Mgx.Views;
using Chaotx.Mgx;

namespace Chaotx.Colorz {
    public class PauseView : FadingView {
        private SessionView parent;
        private SlidingPane slider;
        private FadingPane fader;

        public PauseView(LayoutPane root, SessionView parent) : base(root) {
            FadeInTime = FadeOutTime = 0;
            this.parent = parent;
        }

        protected override void Init() {
            var mniYes = GetItem<MenuItem>("itmYes");
            var mniNo = GetItem<MenuItem>("itmNo");

            slider = GetItem<SlidingPane>("pauseSlider");
            fader = GetItem<FadingPane>("pauseFader");

            slider.SlideIn(GenericPosition.Bottom);
            fader.FadeIn();

            mniYes.IsDisabled = true; // takes away focus
            mniYes.IsDisabled = false;
            mniNo.IsDisabled = true; // takes away focus
            mniNo.IsDisabled = false;

            var colYes = mniYes.TextItem.Color;
            var colNo = mniNo.TextItem.Color;

            mniYes.FocusGain += (s, a) => mniYes.TextItem.Color = Color.Yellow;
            mniNo.FocusGain += (s, a) => mniNo.TextItem.Color = Color.Yellow;
            mniYes.FocusLoss += (s, a) => mniYes.TextItem.Color = colYes;
            mniNo.FocusLoss += (s, a) => mniNo.TextItem.Color = colNo;

            mniNo.Action += (s, a) =>  {
                slider.SlideOut(GenericPosition.Top);
                fader.FadeOut();
            };

            mniYes.Action += (s, a) => {
                parent.StopSession();
                slider.SlideOut(GenericPosition.Top);
                fader.FadeOut();
            };

            slider.SlidedOut += (s, a) => {
                parent.InputDisabled = false;
                Close();
            };
        }

        protected override void HandleInput() {
            base.HandleInput();
            var keyboard = Keyboard.GetState();

            if(slider.State == SlidingPaneState.SlidedIn
            && (keyboard.IsKeyPressed(Keys.Escape)
            || keyboard.IsKeyPressed(Keys.Back))) {
                slider.SlideOut(GenericPosition.Top);
                fader.FadeOut();
            }
        }
    }
}