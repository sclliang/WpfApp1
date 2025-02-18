using System;
using System.Windows;
using System.Windows.Media.Imaging;


namespace Camera
{
    /// <summary>
    /// ShowPhoto.xaml 的交互逻辑
    /// </summary>
    public partial class ShowPhoto : Window
    {
        public int PASSED {  get; set; }
        public ShowPhoto( string path)
        {

            try
            {
                InitializeComponent();
            }
            catch(Exception ex) {
                MessageBox.Show(ex.Message);
                PASSED = 2;
                Close();
                return;
            }
            IMGBOX.Source = new BitmapImage(new Uri(path));
            PASSED = 2;
            // 获取屏幕的工作区大小（不包括任务栏等）
            var screenWidth = SystemParameters.WorkArea.Width;
            var screenHeight = SystemParameters.WorkArea.Height;

            // 设置窗口大小为屏幕的三分之二
            this.Width = screenWidth * 2 / 3;
            this.Height = screenHeight * 2 / 3;

            // 如果需要居中显示，可以设置窗口位置
            this.Left = (screenWidth - this.Width) / 2;
            this.Top = (screenHeight - this.Height) / 2;
            PASSBTN.Focus();
        }

        private void PASSBTN_Click(object sender, RoutedEventArgs e)
        {
            PASSED = 0;
            this.Close();
        }

        private void FAILBTN_Click(object sender, RoutedEventArgs e)
        {
            PASSED = 1;
            this.Close();
        }

        private void RETRYBTN_Click(object sender, RoutedEventArgs e)
        {
            PASSED = 2;
            this.Close();
        }
    }
}
