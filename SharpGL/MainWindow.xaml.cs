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
                    if(!(pixel.Equals(Color.FromRgb(255, 255, 255))))
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
                    if (srcPixel.A >= 255 * 0.9)
                    {
                        wbm.SetPixel(i, j, (byte)(pixel.R * alpha + srcPixel.R * beta), (byte)(pixel.G * alpha + srcPixel.G * beta), (byte)(pixel.B * alpha + srcPixel.B * beta));
                    }
                }
            }
        }

        private byte Truncate(double val)
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
        private void PreprocessHead(bool removeBg, bool normalize)
        {
            Color min = new Color();
            Color max = new Color();
            min.R = min.G = min.B = 255;
            max.R = max.G = max.B = 0;
            for(var i = 0; i < head.PixelWidth; i++)
            {
                for(var j = 0; j < head.PixelHeight; j++)
                {
                    Color srcPixel = head.GetPixel(i, j);
                    
                    if(srcPixel.Equals(Color.FromRgb(0, 0, 0)))
                    {
                        if(removeBg)
                        {
                            //Remove Background
                            head.SetPixel(i, j, Color.FromRgb(255, 255, 255));
                        }
                    }
                    else
                    {
                        //Find lowest raw pixel of each channel
                        if(srcPixel.R < min.R)
                        {
                            min.R = srcPixel.R;
                        }
                        if(srcPixel.G < min.G)
                        {
                            min.G = srcPixel.G;
                        }
                        if(srcPixel.B < min.B)
                        {
                            min.B = srcPixel.B;
                        }

                        //Find highest raw pixel of each channel
                        if(srcPixel.R > max.R)
                        {
                            max.R = srcPixel.R;
                        }
                        if(srcPixel.G > max.G)
                        {
                            max.G = srcPixel.G;
                        }
                        if(srcPixel.B > max.B)
                        {
                            max.B = srcPixel.B;
                        }
                        //Adjust Brightness
                        //srcPixel.R = Truncate((double)srcPixel.R * brightness);
                        //srcPixel.G = Truncate((double)srcPixel.G * brightness);
                        //srcPixel.B = Truncate((double)srcPixel.B * brightness);
                        //head.SetPixel(i, j, srcPixel);
                    }
                }
            }

            for(var i = 0; i < head.PixelWidth; i++)
            {
                for(var j = 0; j < head.PixelHeight; j++)
                {
                    Color srcPixel = head.GetPixel(i, j);

                    if(!srcPixel.Equals(Color.FromRgb(255, 255, 255)))
                    {
                        if(normalize)
                        {
                            //Normalized
                            head.SetPixel(i, j, Color.FromRgb(Normalized(srcPixel.R, min.R, max.R, 0, 255),
                                                              Normalized(srcPixel.G, min.G, max.G, 0, 255),
                                                              Normalized(srcPixel.B, min.B, max.B, 0, 255)));
                        }
                    }
                }
            }
        }

        private byte Normalized(byte i, byte min, byte max, byte newMin, byte newMax)
        {
            return (byte)((i-min) * (newMax-newMin) / (max-min) + newMin);
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
         * dan normalized agar untuk adjust brightness. Setelah itu digambar pada canvas. Fungsi ini
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
            PreprocessHead(true, true);
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
