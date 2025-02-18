using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Windows.Devices.Enumeration;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.UI.Xaml.Controls;


namespace Camera
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private MediaCapture _mediaCapture;
        private bool _isRecording;
        private DispatcherTimer timer;
        public bool IsCapture = false;
        public int RecordTime = 4;
        string filePath = "";
        int cameraCount = 0;
        public int CamRe = 0;
        public enum CAPMODE
        {
            ALL,
            PHOTO,
            VIDEO,
        }

        public CAPMODE CAP;

        public MainWindow()
        {
            InitializeComponent();
            string[] args = Environment.GetCommandLineArgs();

            if (args.Length > 1)
            {
                cameraCount = int.Parse(args[1]);
            }
            string[] files = Directory.GetFiles(".\\", "*.*", SearchOption.TopDirectoryOnly);
            foreach (string file in files)
            {
                string extension = System.IO.Path.GetExtension(file).ToLower(); // 获取文件的扩展名并转换为小写

                if (extension == ".jpg" | extension == ".png" | extension == ".mp4")
                {
                    File.Delete(file); // 删除符合条件的文件
                }
            }
            OnInitializeCameraClick(null, null);
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(3000);
            timer.Tick += Timer_Tick;
            timer.Start();

            if (File.Exists("config.ini"))
            {
                ReadINI readINI = new ReadINI("config.ini");

                String CapMode = readINI.GetValue("config", "Capture");

                if (CapMode == "ALL")
                {
                    CAP = CAPMODE.ALL;
                }
                else if (CapMode == "Photo")
                {
                    CAP = CAPMODE.PHOTO;
                }
                else
                {
                    CAP = CAPMODE.VIDEO;
                }
            }
            else
            {
                CAP = CAPMODE.ALL;
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!IsCapture)
            {
                if (CAP == CAPMODE.PHOTO || CAP == CAPMODE.ALL)
                {
                    timer.Interval = TimeSpan.FromMilliseconds(1000);
                    IsCapture = true;
                    OnCapturePhotoClick(null, null);
                    timer.Stop();
                }
                else
                {
                    timer.Interval = TimeSpan.FromMilliseconds(1000);
                    IsCapture = true;
                    OnStartRecordingClick(null, null);
                }
            }
            if (_isRecording)
            {
                RecordTime--;
                if (RecordTime == 0)
                {
                    timer.Stop();
                    OnStartRecordingClick(null, null);
                }
            }
        }

        private async void OnInitializeCameraClick(object sender, RoutedEventArgs e)
        {
            await InitializeCameraAsync();
        }

        private async void OnCapturePhotoClick(object sender, RoutedEventArgs e)
        {
            await CapturePhotoAsync();
            ShowPhoto photo = new ShowPhoto(filePath);
            try
            {
                
                photo.ShowDialog();
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message);
                return;
            }

            int result = photo.PASSED;
            if (result == 0)
            {
                if (CAP == CAPMODE.PHOTO)
                {
                    File.WriteAllText("result.log", "PASS");
                    this.Close();
                    return;
                }

                OnStartRecordingClick(null, null);
            }
            else if (result == 2)
            {
                OnCapturePhotoClick(null, null);
                return;
            }
            else
            {
                this.Close();
            }
        }

        private async void OnStartRecordingClick(object sender, RoutedEventArgs e)
        {
            if (!_isRecording)
            {
                StickNode node = new StickNode("正在录制视频");
                node.Show();
                timer.Start();
                VICON.Source = new BitmapImage(
                    new Uri("pack://application:,,,/Resources/stop.png")
                );
                await StartRecordingAsync();
            }
            else
            {
                VICON.Source = new BitmapImage(
                    new Uri("pack://application:,,,/Resources/record_fill.png")
                );
                await StopRecordingAsync();
                VideoPlayer player = new VideoPlayer(filePath);
                player.ShowDialog();
                int result = player.PASSED;
                if (result == 0)
                {
                    File.WriteAllText("result.log", "PASS");
                }
                else if (result == 2)
                {
                    RecordTime = 4;
                    OnStartRecordingClick(null, null);
                    return;
                }
                this.Close();
            }
        }

        public async Task InitializeCameraAsync()
        {
            if (_mediaCapture == null)
            {
                // 获取所有可用的视频捕获设备
                var videoDevices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
                var selectedDevice = videoDevices[cameraCount];
                _mediaCapture = new MediaCapture();
                // 使用选定的设备初始化 MediaCapture
                var settings = new MediaCaptureInitializationSettings
                {
                    VideoDeviceId = selectedDevice.Id,

                };

                await _mediaCapture.InitializeAsync(settings);

                // 获取视频捕获设备支持的分辨率
                var videoDeviceController = _mediaCapture.VideoDeviceController;
                var availableMediaStreamProperties = videoDeviceController
                    .GetAvailableMediaStreamProperties(MediaStreamType.VideoPreview)
                    .OfType<VideoEncodingProperties>()
                    .ToList();
                // 选择最高分辨率
                var highestResolutionList = availableMediaStreamProperties
                    .OrderByDescending(x => x.Width * x.Height)
                    .GroupBy(x => new
                    {
                        x.Width,
                        x.Height,
                        x.FrameRate.Numerator,
                    }) // 按宽度和高度分组
                    .Select(g => g.First())
                    .ToList();

                //遍历分辨率列表并设置分辨率
                foreach (var Resolution in highestResolutionList)
                {
                    await videoDeviceController.SetMediaStreamPropertiesAsync(
                        MediaStreamType.VideoPreview,
                        Resolution
                    );
                    // 设置拍照分辨率
                    await videoDeviceController.SetMediaStreamPropertiesAsync(
                        MediaStreamType.Photo,
                        Resolution
                    );
                    await videoDeviceController.SetMediaStreamPropertiesAsync(
                       MediaStreamType.VideoRecord,
                       Resolution
                   );

                    var mediaStreamProperties = videoDeviceController.GetMediaStreamProperties(
                        MediaStreamType.VideoPreview
                    );
                    // 解析分辨率
                    if (mediaStreamProperties is VideoEncodingProperties videoProperties)
                    {
                        uint width = videoProperties.Width;
                        uint height = videoProperties.Height;
                        //如果分辨率设置成功则退出循环,反之则换一个分辨率重新设置
                        if (width == Resolution.Width && height == Resolution.Height)
                            break;
                    }
                }

                // 创建 UWP 的 CaptureElement
                var captureElement = new CaptureElement();
                captureElement.Source = _mediaCapture;
                //前置相机预览镜像显示
                if (videoDevices.Count == 1 || (videoDevices.Count > 1 && cameraCount == 1))
                {
                    // 设置 RenderTransformOrigin 为 (0.5, 0.5) 来使变换围绕中心点
                    captureElement.RenderTransformOrigin = new Windows.Foundation.Point(0.5, 0.5);
                    // 应用镜像效果，使用 ScaleTransform 来替代 ScaleTransform3D
                    var scaleTransform = new Windows.UI.Xaml.Media.ScaleTransform
                    {
                        ScaleX = -1, // 水平镜像
                        ScaleY = 1, // 不进行垂直翻转
                    };

                    captureElement.RenderTransform = scaleTransform;
                }

                // 将 CaptureElement 设置为 WindowsXamlHost 的子控件
                UwpCaptureElementHost.Child = captureElement;

                // 开始预览
                await _mediaCapture.StartPreviewAsync();
            }
        }

        public async Task CapturePhotoAsync()
        {

            if (_mediaCapture == null)
            {
                throw new InvalidOperationException("Camera not initialized.");
            }

            // 获取当前工作目录
            string currentDirectory = Directory.GetCurrentDirectory();

            // 生成当前时间戳作为文件名（例如：20230213_151234.jpg）
            string fileName = DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".jpg";

            // 创建文件的完整路径
            filePath = Path.Combine(currentDirectory, fileName);
            if (!File.Exists(filePath))
            {
                using (File.Create(filePath)) { }
            }
           
            var imageProperties = ImageEncodingProperties.CreateJpeg();
            var file = await StorageFile.GetFileFromPathAsync(filePath);
         
            await _mediaCapture.CapturePhotoToStorageFileAsync(imageProperties, file);

        }

        public async Task StartRecordingAsync()
        {
            if (_mediaCapture == null)
            {
                throw new InvalidOperationException("Camera not initialized.");
            }
            // 获取当前工作目录
            string currentDirectory = Directory.GetCurrentDirectory();

            // 生成当前时间戳作为文件名（例如：20230213_151234.jpg）
            string fileName = DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".mp4";
            // 创建文件的完整路径
            filePath = Path.Combine(currentDirectory, fileName);
            if (!File.Exists(filePath))
            {
                using (File.Create(filePath)) { }
            }

            var videoProperties = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.Auto);

           

            var file = await StorageFile.GetFileFromPathAsync(filePath);

            await _mediaCapture.StartRecordToStorageFileAsync(videoProperties, file);

            _isRecording = true;
        }

        public async Task StopRecordingAsync()
        {
            if (_isRecording)
            {
                await _mediaCapture.StopRecordAsync();
                _isRecording = false;
            }
        }

        public async Task CleanupCameraAsync()
        {
            if (_mediaCapture != null)
            {
                if (_isRecording)
                {
                    await StopRecordingAsync();
                }

                // 如果正在预览,停止预览
                await _mediaCapture.StopPreviewAsync();

                _mediaCapture.Dispose();
                _mediaCapture = null;
            }
        }

        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            await CleanupCameraAsync();
        }
    }

    internal class ReadINI
    {
        private Dictionary<string, Dictionary<string, string>> sections;
        private byte[] key = Encoding.UTF8.GetBytes("0123456789ABCDEF"); //加密密码
        private byte[] iv = Encoding.UTF8.GetBytes("FEDCBA9876543210"); //解密密码

        public ReadINI(string filePath)
        {
            if (File.Exists("temp.ini"))
            {
                File.Delete("temp.ini");
            }
            DecryptIniFile(filePath, "temp.ini", key, iv);
            sections = new Dictionary<string, Dictionary<string, string>>();
            Load("temp.ini");
        }

        public void Load(string filePath)
        {
            sections.Clear();

            string currentSection = "";
            string[] lines = File.ReadAllLines(filePath, Encoding.UTF8);

            foreach (string line in lines)
            {
                string trimmedLine = line.Trim();

                if (
                    trimmedLine.StartsWith(";")
                    || trimmedLine.StartsWith("#")
                    || string.IsNullOrEmpty(trimmedLine)
                )
                {
                    // Skip comment or empty lines
                    continue;
                }
                else if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
                {
                    // Section line
                    currentSection = trimmedLine.Substring(1, trimmedLine.Length - 2);
                    sections[currentSection] = new Dictionary<string, string>();
                }
                else if (currentSection != "")
                {
                    // Key-value pair
                    int separatorIndex = trimmedLine.IndexOf('=');
                    if (separatorIndex != -1)
                    {
                        string key = trimmedLine.Substring(0, separatorIndex).Trim();
                        string value = trimmedLine.Substring(separatorIndex + 1).Trim();

                        sections[currentSection][key] = value;
                    }
                }
            }
            if (File.Exists("temp.ini"))
            {
                File.Delete("temp.ini");
            }
        }

        public string GetValue(string section, string key, string defaultValue = "")
        {
            if (sections.ContainsKey(section) && sections[section].ContainsKey(key))
            {
                return sections[section][key];
            }

            return defaultValue;
        }

        public Dictionary<string, string> GetSection(string section)
        {
            if (sections.ContainsKey(section))
            {
                return sections[section];
            }

            return new Dictionary<string, string>();
        }

        static void EncryptIniFile(string inputFile, string outputFile, byte[] key, byte[] iv)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;

                using (FileStream fsInput = new FileStream(inputFile, FileMode.Open))
                using (FileStream fsEncrypted = new FileStream(outputFile, FileMode.Create))
                using (ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV))
                using (
                    CryptoStream cryptoStream = new CryptoStream(
                        fsEncrypted,
                        encryptor,
                        CryptoStreamMode.Write
                    )
                )
                {
                    fsInput.CopyTo(cryptoStream);
                }
            }
        }

        static void DecryptIniFile(string inputFile, string outputFile, byte[] key, byte[] iv)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;

                using (FileStream fsInput = new FileStream(inputFile, FileMode.Open))
                using (FileStream fsDecrypted = new FileStream(outputFile, FileMode.Create))
                using (ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV))
                using (
                    CryptoStream cryptoStream = new CryptoStream(
                        fsInput,
                        decryptor,
                        CryptoStreamMode.Read
                    )
                )
                {
                    cryptoStream.CopyTo(fsDecrypted);
                }
            }
        }
    }
}
