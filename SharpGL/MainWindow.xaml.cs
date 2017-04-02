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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SharpGL;
using System.IO;

namespace SharpGL
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            SetBodyKind(1);

            //0 = (239, 202, 40)
            //1 = (243, 221, 187)
            //2 = (223, 190, 143)
            //3 = (196, 157, 89)
            //4 = (156, 103, 58)
            //5 = (78, 59, 39)
            colorCategory = new Color[6];
            colorCategory[0].R = 239; colorCategory[0].G = 202; colorCategory[0].B = 40;
            colorCategory[1].R = 243; colorCategory[1].G = 221; colorCategory[1].B = 187;
            colorCategory[2].R = 223; colorCategory[2].G = 190; colorCategory[2].B = 143;
            colorCategory[3].R = 196; colorCategory[3].G = 157; colorCategory[3].B = 89;
            colorCategory[4].R = 156; colorCategory[4].G = 103; colorCategory[4].B = 58;
            colorCategory[5].R = 78; colorCategory[5].G = 59; colorCategory[5].B = 39;

            skinColorCategory = 3;

            path = "http://fashionsense.azurewebsites.net/web/asset/";
        }


        //Utility
        private BitmapImage ConvertWriteableBitmapToBitmapImage(WriteableBitmap wbm)
        {
            BitmapImage bmImage = new BitmapImage();
            using(MemoryStream stream = new MemoryStream())
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(wbm));
                encoder.Save(stream);
                bmImage.BeginInit();
                bmImage.CacheOption = BitmapCacheOption.OnLoad;
                bmImage.StreamSource = stream;
                bmImage.EndInit();
                bmImage.Freeze();
            }
            return bmImage;
        }

        private Color GetSkinColorFromHead()
        {
            Color avgPixel = new Color();
            int[] arr = new int[3]; //arr[0] = r, arr[1] = g, arr[2] = b
            int size = 0;

            for (var i = 0; i < head.PixelWidth; i++)
            {
                for (var j = 0; j < head.PixelHeight; j++)
                {
                    Color pixel = head.GetPixel(i, j);
                    if(!(pixel.R == 255 && pixel.G == 255 && pixel.B == 255))
                    {
                        arr[0] += pixel.R;
                        arr[1] += pixel.G;
                        arr[2] += pixel.B;
                        size++;
                    }
                }
            }
            avgPixel.R = (byte)Math.Round((double)(arr[0] / (size * 1.0)));
            avgPixel.G = (byte)Math.Round((double)(arr[1] / (size * 1.0)));
            avgPixel.B = (byte)Math.Round((double)(arr[2] / (size * 1.0)));

            return avgPixel;
        }

        private void SetSkinColorToMannequin(WriteableBitmap wbm, Color pixel, double initVal = 0.7)
        {
            double alpha = initVal, beta = 1.0 - alpha;
            for (var i = 0; i < wbm.PixelWidth; i++)
            {
                for (var j = 0; j < wbm.PixelHeight; j++)
                {
                    Color srcPixel = wbm.GetPixel(i, j);
                    if (srcPixel.A != 0)
                    {
                        wbm.SetPixel(i, j, (byte)(pixel.R * alpha + srcPixel.R * beta), (byte)(pixel.G * alpha + srcPixel.G * beta), (byte)(pixel.B * alpha + srcPixel.B * beta));
                    }
                }
            }
        }

        private byte Truncate(int val)
        {
            if(val < 0)
            {
                return 0;
            }
            else if(val > 255)
            {
                return 255;
            }
            else
            {
                return (byte)val;
            }
        }

        private void SetSkinColorCategory(Color pixel)
        {
            int smallest = 765;
            int smallestIndex = 0;
            int current = 0;

            for (var i = 0; i < colorCategory.Length; i++)
            {
                current = Math.Abs(colorCategory[i].R - pixel.R) + Math.Abs(colorCategory[i].G - pixel.G) + Math.Abs(colorCategory[i].B - pixel.B);
                if (current < smallest)
                {
                    smallest = current;
                    smallestIndex = i;
                }
            }
            skinColorCategory = smallestIndex;
        }


        //Head Section
        private void PreprocessHead(int brightness)
        {
            if (brightness > 200)
            {
                brightness = 200;
            }
            for (var i = 0; i < head.PixelWidth; i++)
            {
                for(var j = 0; j < head.PixelHeight; j++)
                {
                    Color srcPixel = head.GetPixel(i, j);
                    
                    if(srcPixel.R == 0 && srcPixel.G == 0 && srcPixel.B == 0)
                    {
                        //Remove Background
                        srcPixel.R = 255;
                        srcPixel.G = 255;
                        srcPixel.B = 255;
                        //srcPixel.A = 0;

                        head.SetPixel(i, j, srcPixel);
                    }
                    else
                    {
                        //Adjust Brightness
                        srcPixel.R = Truncate((int)srcPixel.R + brightness);
                        srcPixel.G = Truncate((int)srcPixel.G + brightness);
                        srcPixel.B = Truncate((int)srcPixel.B + brightness);
                        head.SetPixel(i, j, srcPixel);
                    }
                }
            }
        }

        private void DrawHead()
        {
            Head.Source = head;
            Head.Width = 38;
            Head.Height = 50;
            Canvas.SetLeft(Head, 109);
            Canvas.SetTop(Head, 12);
            Canvas.SetZIndex(Head, 104);
        }


        //Upper Mannequin Section
        private void SetUpperMannequin()
        {
            bitmapUpperMannequin = new BitmapImage();
            bitmapUpperMannequin.BeginInit();
            bitmapUpperMannequin.UriSource = new Uri(@"" + path + bodyKind + "bodyAtas.png", UriKind.Absolute);
            bitmapUpperMannequin.EndInit();

            bitmapUpperMannequin.DownloadCompleted += BitmapUpperMannequin_DownloadCompleted;
        }

        private void BitmapUpperMannequin_DownloadCompleted(object sender, EventArgs e)
        {
            CroppedBitmap croppedBitmapUpperMannequin = new CroppedBitmap(bitmapUpperMannequin, new Int32Rect(45, 15, 510, 430));
            upperMannequin = new WriteableBitmap(croppedBitmapUpperMannequin);
            SetSkinColorToMannequin(upperMannequin, GetSkinColorFromHead());
            DrawUpperMannequin();
        }

        private void DrawUpperMannequin()
        {
            UpperMannequin.Source = upperMannequin;
            UpperMannequin.Width = 255;
            UpperMannequin.Height = 215;
            Canvas.SetLeft(UpperMannequin, 0);
            Canvas.SetTop(UpperMannequin, 0);
            Canvas.SetZIndex(UpperMannequin, 101);
        }


        //Lower Mannequin Section
        private void SetLowerMannequin()
        {
            bitmapLowerMannequin = new BitmapImage();
            bitmapLowerMannequin.BeginInit();
            bitmapLowerMannequin.UriSource = new Uri(@"" + path + bodyKind + "bodyBawah.png", UriKind.Absolute);
            bitmapLowerMannequin.EndInit();

            bitmapLowerMannequin.DownloadCompleted += BitmapLowerMannequin_DownloadCompleted;
        }

        private void BitmapLowerMannequin_DownloadCompleted(object sender, EventArgs e)
        {
            CroppedBitmap croppedBitmapLowerMannequin = new CroppedBitmap(bitmapLowerMannequin, new Int32Rect(45, 15, 510, 530));
            lowerMannequin = new WriteableBitmap(croppedBitmapLowerMannequin);
            SetSkinColorToMannequin(lowerMannequin, GetSkinColorFromHead());
            DrawLowerMannequin();
        }

        private void DrawLowerMannequin()
        {
            LowerMannequin.Source = lowerMannequin;
            LowerMannequin.Width = 255;
            LowerMannequin.Height = 265;
            Canvas.SetLeft(LowerMannequin, 0);
            Canvas.SetTop(LowerMannequin, 125);
            Canvas.SetZIndex(LowerMannequin, 100);
        }


        //Upper Clothes Section
        private void BitmapUpperClothes_DownloadCompleted(object sender, EventArgs e)
        {
            CroppedBitmap croppedBitmapUpperClothes = new CroppedBitmap(bitmapUpperClothes, new Int32Rect(45, 15, 510, 430));
            upperClothes = new WriteableBitmap(croppedBitmapUpperClothes);
            DrawUpperClothes();
        }

        private void DrawUpperClothes()
        {
            UpperClothes.Source = upperClothes;
            UpperClothes.Width = 255;
            UpperClothes.Height = 215;
            Canvas.SetLeft(UpperClothes, 0);
            Canvas.SetTop(UpperClothes, 0);
            Canvas.SetZIndex(UpperClothes, 103);
        }


        //Lower Clothes Section
        private void BitmapLowerClothes_DownloadCompleted(object sender, EventArgs e)
        {
            CroppedBitmap croppedBitmapLowerClothes = new CroppedBitmap(bitmapLowerClothes, new Int32Rect(45, 15, 510, 530));
            lowerClothes = new WriteableBitmap(croppedBitmapLowerClothes);
            DrawLowerClothes();
        }

        private void DrawLowerClothes()
        {
            LowerClothes.Source = lowerClothes;
            LowerClothes.Width = 255;
            LowerClothes.Height = 265;
            Canvas.SetLeft(LowerClothes, 0);
            Canvas.SetTop(LowerClothes, 125);
            Canvas.SetZIndex(LowerClothes, 102);
        }


        //Public Function

        /**
         * Mengubah bentuk tubuh.
         * @param {int bodyKind} Ket: (KEKAR = 0, NORMAL = 1, KURUS = 2, AGAKGEMUK = 3, GEMUK = 4)
         * Default Constructor jika tidak memanggil fungsi ini, bodyKind = NORMAL.
         * Setelah memanggil fungsi ini, panggil SetMannequin(), SetUpperClothes(), SetLowerClothes()
         * untuk melihat perubahan. fungsi SetHead() tidak terpengaruh oleh fungsi ini.
         */
        public void SetBodyKind(int bodyKind)
        {
            this.bodyKind = bodyKind.ToString() + "/";
        }

        /**
         * Mengubah kepala dari gambar base64 sekaligus menggambarnya pada canvas.
         * @param {string imgBase64} String berisikan gambar base64
         * Fungsi ini sekaligus melakukan praproses terhadap gambar berupa menghilangkan background
         * dan menambahkan brightness. Setelah itu digambar pada canvas. Fungsi ini
         * juga menentukan skinColorCategory, panggil fungsi GetSkinColorCategory()
         * setelah memanggil fungsi ini untuk mendapatkan kategori.
         */
        public void SetHead(string imgBase64)
        {
            byte[] binaryData = Convert.FromBase64String(imgBase64);
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = new MemoryStream(binaryData);
            bitmap.EndInit();

            head = new WriteableBitmap(bitmap);
            PreprocessHead(35);
            SetSkinColorCategory(GetSkinColorFromHead());
            DrawHead();
        }

        /**
         * Mengubah mannequin sekaligus menggambarnya pada canvas.
         * @param null
         * Fungsi ini sangat tergantung pada SetHead(). Apabila memanggil fungsi ini tanpa
         * memanggil SetHead(), maka akan error karena warna mannequin akan disesuaikan dengan
         * warna kulit kepala. Ukuran mannequin juga tergantung dari bodyKind pada SetBodyKind().
         */
        public void SetMannequin()
        {
            SetLowerMannequin();
            SetUpperMannequin();
        }

        /**
         * Mengubah pakaian bagian atas (baju) sekaligus menggambarnya pada canvas.
         * @param {string img} String nama gambar beserta formatnya
         * Ukuran baju tergantung dari bodyKind pada SetBodyKind().
         */
        public void SetUpperClothes(string img)
        {
            bitmapUpperClothes = new BitmapImage();
            bitmapUpperClothes.BeginInit();
            bitmapUpperClothes.UriSource = new Uri(@"" + path + bodyKind + img, UriKind.Absolute);
            bitmapUpperClothes.EndInit();

            bitmapUpperClothes.DownloadCompleted += BitmapUpperClothes_DownloadCompleted;
        }

        /**
         * Mengubah pakaian bagian bawah (celana) sekaligus menggambarnya pada canvas.
         * @param {string img} String nama gambar beserta formatnya
         * Ukuran celana tergantung dari bodyKind pada SetBodyKind().
         */
        public void SetLowerClothes(string img)
        {
            bitmapLowerClothes = new BitmapImage();
            bitmapLowerClothes.BeginInit();
            bitmapLowerClothes.UriSource = new Uri(@"" + path + bodyKind + img, UriKind.Absolute);
            bitmapLowerClothes.EndInit();

            bitmapLowerClothes.DownloadCompleted += BitmapLowerClothes_DownloadCompleted;
        }

        /**
         * Mendapatkan skinColorCategory
         * @param null
         * @return {int skinColorCategory} Range kategori: 0 - 5 (paling cerah - paling gelap)
         * Fungsi ini sangat tergantung pada SetHead(). Apabila memanggil fungsi ini tanpa
         * memanggil SetHead(), maka akan error karena kategori ditentukan dari warna kulit kepala.
         */
        public int GetSkinColorCategory()
        {
            return skinColorCategory;
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Sample
            CanvasBorder.BorderThickness = new Thickness(1);
            var bgImage64 = "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";

            SetBodyKind(1);
            SetHead(bgImage64);
            SetMannequin();
            SetUpperClothes("baju0.png");
            SetLowerClothes("celana0.png");
            Console.WriteLine(GetSkinColorCategory());
        }


        string path;
        string bodyKind;
        int skinColorCategory;
        Color[] colorCategory;

        WriteableBitmap head;
        WriteableBitmap lowerClothes;
        WriteableBitmap lowerMannequin;
        WriteableBitmap upperClothes;
        WriteableBitmap upperMannequin;

        BitmapImage bitmapLowerClothes;
        BitmapImage bitmapLowerMannequin;
        BitmapImage bitmapUpperClothes;
        BitmapImage bitmapUpperMannequin;
    }
}
