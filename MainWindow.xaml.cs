using Microsoft.VisualBasic.Devices;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WarzoneConnect.Planner;
using WarzoneConnect.Properties;

namespace WarzoneConnect
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow
    {
        SoundPlayer sp_p1 = new SoundPlayer();
        SoundPlayer sp_p2 = new SoundPlayer();
        SoundPlayer sp_p3 = new SoundPlayer();
        SoundPlayer sp_p3bg = new SoundPlayer(IntroMedia.Into_the_Furnace);


        private bool _isIntroClosing;
        private bool _isMessageReceived;

        public MainWindow()
        {
            if (!File.Exists(@".\Save.resx"))
            {
                InitializeComponent();

                var tempByte = IntroMedia.stream;
                var fileStream = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "stream.mp4",
                    FileMode.OpenOrCreate);
                fileStream.Write(tempByte ?? throw new InvalidOperationException(), 0, tempByte.Length);
                WarzoneIntro.Source = new Uri(AppDomain.CurrentDomain.BaseDirectory + "stream.mp4");

                WarzoneTitle.Source = Imaging.CreateBitmapSourceFromHBitmap(IntroMedia.warzone_connect.GetHbitmap(),
                    IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                PressStart.Source = Imaging.CreateBitmapSourceFromHBitmap(IntroMedia.press_start.GetHbitmap(),
                    IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

                ShowDialog();
                Close();

                if (_isMessageReceived)
                    GameController.Initialize();
            }
            else
            {
                GameController.Initialize();
            }
        }

        private void OpenWindow(object sender, EventArgs e)
        {
            WarzoneIntro.Play();
            new Task(ShowTitleAndSound).Start();
        }

        private void PrepareStart(object sender, CancelEventArgs e)
        {
            if (_isMessageReceived)
            {
                _isIntroClosing = true;
                e.Cancel = true;
                WarzoneIntro.Close();
                Visibility = Visibility.Hidden;
            }
            else
            {
                Application.Current.Shutdown();
            }
        }

        private void SkipToNext(object sender, MouseButtonEventArgs e)
        {
            if (WarzoneIntro.Position < new TimeSpan(0, 0, 5))
                WarzoneIntro.Position = new TimeSpan(0, 0, 5);
            else if (WarzoneIntro.Position < new TimeSpan(0, 0, 10))
                WarzoneIntro.Position = new TimeSpan(0, 0, 10);
            else
            {
                _isMessageReceived = true;
                Thread.Sleep(3000);
                sp_p3bg.Stop();
                Dispatcher.BeginInvoke((Action)delegate { WarzoneIntro.Stop(); });
                new Audio().PlaySystemSound(SystemSounds.Beep); 
                MessageBox.Show("Something`s wrong...\n" +
                                "It seems that the whole thing`s out of control...\n" +
                                "Initializing fail-save system.","Message Intercepted");
                WarzoneConnectWindow.Close();
            }
        }

        private void Replay(object sender, RoutedEventArgs e)
        {
            WarzoneIntro.Position = new TimeSpan(0, 0, 10);
        }

        private void ShowTitleAndSound()
        {
            
            var pos = 0;
            do
            {
                Thread.Sleep(50);
                Dispatcher.BeginInvoke((Action) delegate { pos = WarzoneIntro.Position.Seconds; });
            } while (pos < 10);


            var i = 0.01;

            sp_p3bg.PlayLooping();

            do
            {
                var i1 = i;
                Dispatcher.BeginInvoke((Action) delegate { WarzoneTitle.Opacity = i1; });
                i *= 2;
                Thread.Sleep(80);
                if (!(i > 1)) continue;
                Dispatcher.BeginInvoke((Action) delegate { WarzoneTitle.Opacity = 1; });
                break;
            } while (i < 1);

            void Cleanup()
            {
                sp_p1.Stop();
                sp_p1.Dispose();
                sp_p2.Stop();
                sp_p2.Dispose();
                sp_p3.Stop();
                sp_p3.Dispose();
                sp_p3bg.Stop();
                sp_p3bg.Dispose();
            }

            while (!_isIntroClosing)
            {
                for (var h = 1; h <= 100; h++)
                {
                    var h1 = h;
                    Dispatcher.BeginInvoke((Action) delegate { PressStart.Opacity = h1 * 0.01; });
                    if (_isIntroClosing)
                    {
                        Cleanup();
                        return;
                    }
                    Thread.Sleep(10);
                }

                for (var h = 100 - 1; h >= 0; h--)
                {
                    var h1 = h;
                    Dispatcher.BeginInvoke((Action) delegate { PressStart.Opacity = h1 * 0.01; });
                    if (_isIntroClosing)
                    {
                        Cleanup();
                        return;
                    }
                    Thread.Sleep(10);
                }
            }
            Cleanup();
        }
    }
}