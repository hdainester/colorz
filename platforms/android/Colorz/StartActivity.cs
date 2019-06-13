using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;

namespace Chaotx.Colorz.Android {
    [Activity(Label = "Colorz"
        , MainLauncher = true
        , Icon = "@drawable/icon"
        , Theme = "@style/Theme.Splash"
        , AlwaysRetainTaskState = true
        , LaunchMode = LaunchMode.SingleInstance
        , ScreenOrientation = ScreenOrientation.FullUser
        , ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize | ConfigChanges.ScreenLayout)]
    public class StartActivity : Microsoft.Xna.Framework.AndroidGameActivity {
        protected override void OnCreate(Bundle bundle) {
            base.OnCreate(bundle);
            var g = new ColorzGame();
            SetContentView((View)g.Services.GetService(typeof(View)));
            g.GameFinished += (s, a) => Process.KillProcess(Process.MyPid());
            g.Run();
        }
    }
}

