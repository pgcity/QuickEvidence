using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage.Streams;

namespace OCRLib
{
    public class OCRCore
    {
        public async Task<string> OCRMainAsync(MemoryStream st)
        {
            return await Task.Run(() =>
            {
                var res = OCR(st);
                res.Wait();
                MessageBox.Show(res.Result.Text);
                return res.Result.Text;
            });
        }
        public async Task<OcrResult> OCR(MemoryStream st)
        {
            var mem = await ConvertToRandomAccessStream(st);
            var bitmap = await LoadImage(mem);
            var ocrEngine = OcrEngine.TryCreateFromUserProfileLanguages();
            var ocrResult = await ocrEngine.RecognizeAsync(bitmap);

            return ocrResult;
        }

        /// <summary>
        /// ファイルパスを指定して SoftwareBitmap を取得
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private async Task<SoftwareBitmap> LoadImage(string path)
        {
            var fs = System.IO.File.OpenRead(path);
            var buf = new byte[fs.Length];
            fs.Read(buf, 0, (int)fs.Length);
            var mem = new MemoryStream(buf);
            mem.Position = 0;

            var stream = await ConvertToRandomAccessStream(mem);
            var bitmap = await LoadImage(stream);
            return bitmap;
        }
        /// <summary>
        /// IRandomAccessStream から SoftwareBitmap を取得
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        private async Task<SoftwareBitmap> LoadImage(IRandomAccessStream stream)
        {
            var decoder = await Windows.Graphics.Imaging.BitmapDecoder.CreateAsync(stream);
            var bitmap = await decoder.GetSoftwareBitmapAsync();
            return bitmap;
        }
        /// <summary>
        /// MemoryStream から IRandomAccessStream へ変換
        /// </summary>
        /// <param name="memoryStream"></param>
        /// <returns></returns>
        public async Task<IRandomAccessStream> ConvertToRandomAccessStream(MemoryStream memoryStream)
        {
            var randomAccessStream = new InMemoryRandomAccessStream();
            var outputStream = randomAccessStream.GetOutputStreamAt(0);
            var dw = new DataWriter(outputStream);
            var task = new Task(() => dw.WriteBytes(memoryStream.ToArray()));
            task.Start();
            task.Wait();
            await dw.StoreAsync();
            await outputStream.FlushAsync();
            return randomAccessStream;
        }
    }
}
