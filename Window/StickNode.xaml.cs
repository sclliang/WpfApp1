using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Camera
{
    /// <summary>
    /// StickNode.xaml 的交互逻辑
    /// </summary>
    public partial class StickNode : Window
    {
        public int X = 0;
        public StickNode(string msg)
        {
            InitializeComponent();

            X = SharedData.CommonMessage;
            SharedData.CommonMessage = SharedData.CommonMessage + 1;
            INFO.Text = msg;
            // 设置窗口初始位置（屏幕顶部居中）
            this.WindowStartupLocation = WindowStartupLocation.Manual;
            this.Left = (SystemParameters.PrimaryScreenWidth - this.Width) / 2;
            this.Top = -this.Height; // 初始位置在屏幕上方
            // 启动动画
            StartAnimation();
        }
        private void StartAnimation()
        {
            // 滑入动画
            DoubleAnimation slideInAnimation = new DoubleAnimation
            {
                To = X * (this.Height), // 滑动到屏幕顶部
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            this.BeginAnimation(TopProperty, slideInAnimation);

            // 2 秒后关闭窗口
            var closeTimer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3)
            };
            closeTimer.Tick += (s, e) =>
            {
                closeTimer.Stop();
                CloseWindowWithAnimation();
            };
            closeTimer.Start();
        }
        private void CloseWindowWithAnimation()
        {
            // 滑出动画
            DoubleAnimation slideOutAnimation = new DoubleAnimation
            {
                To = -this.Height, // 滑动到屏幕上方
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };

            slideOutAnimation.Completed += (s, e) => this.Close(); // 动画完成后关闭窗口
            this.BeginAnimation(TopProperty, slideOutAnimation);
            SharedData.CommonMessage = SharedData.CommonMessage - 1;
        }
    }
    public static class SharedData
    {
        public static int CommonMessage {  get; set; }
    }

}
