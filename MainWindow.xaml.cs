using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WarzoneConnect.Planner;

namespace WarzoneConnect
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            if (!File.Exists(@".\Save.resx"))
            {
                //TODO 是否存在存档，没有就打开WZ
                InitializeComponent();
                Warzone_Intro.Source = new Uri(@"D:\Personal\Documents\Repo\C#\WarzoneConnect\bin\Debug\Resources\Airstrike_3x.mp4");
                ShowDialog();
                Close();

                GameController.Initialize();
                //Application.EnableVisualStyles();
                //Application.SetCompatibleTextRenderingDefault(false);
                // Application.Run(new WarzoneConnect());
                //Application.Run(new WarzoneConnect());
            }
            else
            {
                GameController.Initialize();
            }
            //new OutputTask().RandomProgressBar("加载视频中......", 10);
            // Application.EnableVisualStyles();
            // Application.SetCompatibleTextRenderingDefault(false);
            // Application.Run(new MediaPlayer("test.mp4"));
            //Console.WriteLine("Continue"); //此处要插入巨TM炫酷的加载代码效果，取代Continue
            //new MailAnimationTest();
            //Mail.Init();

            // WAFServer.IncursionSetup("test",2);
            //GameController.Initialize();
            //Task newGame = new Task(() => GameController.Initialize());
            //newGame.Start();
            //Task.WaitAny(newGame);
        }

        private void PlayIntro(object sender, EventArgs e)
        {
            Warzone_Intro.Play();
        }

        private void PrepareStart(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Warzone_Intro.Close();
            Visibility = Visibility.Hidden;
        }
    }
}
