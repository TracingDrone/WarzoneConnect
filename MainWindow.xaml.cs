using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;
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
using WarzoneConnect.Properties;

namespace WarzoneConnect
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        bool isTitleVisible = false;
        public MainWindow()
        {
            if (!File.Exists(@".\Save.resx"))
            {
                //TODO 是否存在存档，没有就打开WZ
                InitializeComponent();

                byte[] tempByte = IntroMedia.stream;
                var fileStream = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "stream.mp4", FileMode.OpenOrCreate);
                fileStream.Write(tempByte ?? throw new InvalidOperationException(), 0, tempByte.Length);
                Warzone_Intro.Source = new Uri(AppDomain.CurrentDomain.BaseDirectory + "stream.mp4");

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

        private void OpenWindow(object sender, EventArgs e)
        {
            Warzone_Intro.Play();
            Dispatcher.BeginInvoke((Action)delegate ()
                {
                    if(!isTitleVisible && Warzone_Intro.Position > new TimeSpan(0, 0, 10))
                    {
                        isTitleVisible = true;
                        for (var i = 1; i <= 100; i++)
                        {
                            Warzone_Title.Opacity = i;
                            Thread.Sleep(10);
                        }
                    }
                });
        }

        private void PrepareStart(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Warzone_Intro.Close();
            Visibility = Visibility.Hidden;
        }

        private void SkipToNext(object sender, MouseButtonEventArgs e)
        {
            if (Warzone_Intro.Position < new TimeSpan(0, 0, 5))
                Warzone_Intro.Position = new TimeSpan(0, 0, 5);
            else if (Warzone_Intro.Position < new TimeSpan(0, 0, 10))
                Warzone_Intro.Position = new TimeSpan(0, 0, 10);
            else
            {
                //TODO 弹窗提示大难临头
                WarzoneConnect_Window.Close();
                return;
            }
                
        }

        private void Replay(object sender, RoutedEventArgs e)
        {
            Warzone_Intro.Position = new TimeSpan(0, 0, 10);
        }
    }
}
