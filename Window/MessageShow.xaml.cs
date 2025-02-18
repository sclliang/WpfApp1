using System.Windows;


namespace Camera
{
    /// <summary>
    /// MessageShow.xaml 的交互逻辑
    /// </summary>
    public partial class MessageShow : Window
    {
        public int Result { get; private set; }

        public MessageShow()
        {
            InitializeComponent();
            Result = 3;
            PA.Focus();
        }

        private void PassButton_Click(object sender, RoutedEventArgs e)
        {
            Result = 0;
            this.Close();
        }

        private void FailButton_Click(object sender, RoutedEventArgs e)
        {
            Result = 1; this.Close();
        }

        private void RetryButton_Click(object sender, RoutedEventArgs e)
        {
            Result = 2; this.Close();
        }


    }
}
