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
        public static HPCalculationsView Instance { get; private set; }
        public List<System.Drawing.Image> Images { get; set; }
        public HPCalculationsView(int width = 1000, int height = 450)
        {
            Width = width;
            Height = height;
            InitializeComponent();
            Instance = this;
        }

        public void AddImage(System.Drawing.Image newImage)
        {
            Image image = new Image
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                Stretch = Stretch.None,
                Margin = new Thickness(0,20,0,0)
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
            Images ??= new List<System.Drawing.Image>();
            Images.Add(newImage);
        }

        public void AddImages(List<System.Drawing.Image> newImages)
        {
            for (int i = 0; i < newImages.Count; i++)
            {
                AddImage(newImages[i]);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Images|*.png;";
            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (this.Images.Count == 1)
                {
                    this.Images[0].Save(saveFileDialog.FileName, ImageFormat.Png);
                }
                else if (this.Images.Count > 0)
                {
                    for (int i = 0; i < Images.Count; i++)
                    {
                        this.Images[i].Save(saveFileDialog.FileName.Replace(".png", $"_{i+1}.png"), ImageFormat.Png);
                    }
                }
                System.Windows.Forms.MessageBox.Show("Saved successfully");
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Instance = null;
        }
    }
}
