using System.Threading.Tasks;
using System.IO;
using System;

namespace Chaotx.Colorz.Desktop {
    public static class Program {
        [STAThread]
        static void Main(string[] args) {
            using (var game = new ColorzGame())
                game.Run();
        }
    }
}
