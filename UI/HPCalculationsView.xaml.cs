using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AdvansysPOC.UI
{
    /// <summary>
    /// Interaction logic for HPCalculationsView.xaml
    /// </summary>
    public partial class HPCalculationsView : Window
    {
        public System.Drawing.Image Image { get; set; }
        public HPCalculationsView()
        {
            InitializeComponent();
        }

        public void AddImage(System.Drawing.Image newImage)
        {
            Image image = new Image
            {
                
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                Stretch = Stretch.None,
            };
            using (var ms = new MemoryStream())
            {
                newImage.Save(ms, ImageFormat.Bmp);
                ms.Seek(0, SeekOrigin.Begin);

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = ms;
                bitmapImage.EndInit();

                image.Source = bitmapImage;
            }
            stackPanel.Children.Add(image);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Images|*.png;";
            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.Image.Save(saveFileDialog.FileName, ImageFormat.Png);
            }
        }
    }
}
