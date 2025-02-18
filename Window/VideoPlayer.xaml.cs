using System;
using System.Windows;
using System.Windows.Controls;


namespace Camera
{
    /// <summary>
    /// VideoPlayer.xaml 的交互逻辑
    /// </summary>
    public partial class VideoPlayer : Window
    {
        public int PASSED {  get; set; }
        public VideoPlayer(string path)
        {
            InitializeComponent();
            // 获取屏幕的工作区大小（不包括任务栏等）
            var screenWidth = SystemParameters.WorkArea.Width;
            var screenHeight = SystemParameters.WorkArea.Height;

            // 设置窗口大小为屏幕的三分之二
            this.Width = screenWidth * 2 / 3;
            this.Height = screenHeight * 2 / 3;

            // 如果需要居中显示，可以设置窗口位置
            this.Left = (screenWidth - this.Width) / 2;
            this.Top = (screenHeight - this.Height) / 2;
            mediaElement.Source = new Uri(path, UriKind.Absolute);
            // 添加元素加载完成事件 -- 自动开始播放
            mediaElement.Loaded += new RoutedEventHandler(media_Loaded);

            // 添加媒体播放结束事件 -- 重新播放
            mediaElement.MediaEnded += new RoutedEventHandler(media_MediaEnded);

            // 添加元素卸载完成事件 -- 停止播放
            mediaElement.Unloaded += new RoutedEventHandler(media_Unloaded);

            PASSED = 2;
        }
        private void media_Loaded(object sender, RoutedEventArgs e)
        {
            try {
                
                (sender as MediaElement).Play(); }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }
      
        private void media_Unloaded(object sender, RoutedEventArgs e)
        {
            (sender as MediaElement).Stop();
        }
        private void media_MediaEnded(object sender, RoutedEventArgs e)
        {
       
            (sender as MediaElement).Stop();
           
            MessageShow message = new MessageShow();
            message.ShowDialog();
            int re = message.Result;

            switch (re) { 
                case 0:
                    mediaElement.Stop();
                    PASSED = 0;
                    this.Close();

                    break;
                case 1:
                    mediaElement.Stop();
                    PASSED = 1;
                    this.Close();
                    break;
                default:
                    this.Close();
                    break;
               
            }

           
        }

       
    }
}
