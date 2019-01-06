using Microsoft.WindowsAPICodePack.Dialogs;
using Prism.Commands;
using Prism.Mvvm;
using QuickEvidence.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace QuickEvidence.ViewModels
{
	public class MainWindowViewModel : BindableBase
	{
        public IGetPosition GetPositionIF { get; internal set; }
        public MainWindow ColorDialogIF { get; internal set; }
        public MainWindow TextInputWindowIF { get; internal set; }
        const string APP_NAME = "QuickEvidence";

        public MainWindowViewModel()
        {
            SelectedToolBarButton = Properties.Settings.Default.SelectedToolBarButton;
            SelectedColor = Color.FromRgb(
                Properties.Settings.Default.Color_R,
                Properties.Settings.Default.Color_G,
                Properties.Settings.Default.Color_B);
            FolderPath = Properties.Settings.Default.FolderPath;
            if (!Directory.Exists(FolderPath))
            {
                FolderPath = Directory.GetCurrentDirectory();
                Properties.Settings.Default.Save();
            }

            UpdateFileList();
        }

        ///////////////////////////////////////////////
        // プロパティ

        /// <summary>
        /// ドラッグ開始座標
        /// </summary>
        private Point DragStartPosScrollViewer {
            get;set;
        }

        /// <summary>
        /// ドラッグ終了座標
        /// </summary>
        private Point DragEndPosScrollViewer
        {
            get; set;
        }

        /// <summary>
        /// ドラッグ開始座標
        /// </summary>
        private Point DragStartPosViewBox
        {
            get; set;
        }

        /// <summary>
        /// ドラッグ終了座標
        /// </summary>
        private Point DragEndPosViewBox
        {
            get; set;
        }


        ///////////////////////////////////////////////
        // バインディング用プロパティ

        /// <summary>
        /// ファイル一覧
        /// </summary>
        private ObservableCollection<FileItemViewModel> _fileItems = new ObservableCollection<FileItemViewModel>();
        public ObservableCollection<FileItemViewModel> FileItems
        {
            get { return _fileItems; }
            set { SetProperty(ref _fileItems, value); }
        }

        /// <summary>
        /// 選択されたファイル
        /// </summary>
        private FileItemViewModel _selectedFile;
        public FileItemViewModel SelectedFile
        {
            get { return _selectedFile; }
            set { SetProperty(ref _selectedFile, value); }
        }

        /// <summary>
        /// フォルダパス
        /// </summary>
        private string _folderPath = "FolderPath";
        public string FolderPath
        {
            get { return _folderPath; }
            set { SetProperty(ref _folderPath, value); }
        }

        /// <summary>
        /// フルスクリーン
        /// </summary>
        /// 
        private ResizeMode _resizeMode = ResizeMode.CanResizeWithGrip;
        public ResizeMode ResizeMode
        {
            get { return _resizeMode; }
            set { SetProperty(ref _resizeMode, value); }
        }

        private WindowState _windowState = WindowState.Normal;
        public WindowState WindowState
        {
            get { return _windowState; }
            set { SetProperty(ref _windowState, value); }
        }

        private WindowStyle _windowStyle = WindowStyle.SingleBorderWindow;
        public WindowStyle WindowStyle
        {
            get { return _windowStyle; }
            set { SetProperty(ref _windowStyle, value); }
        }

        /// <summary>
        /// イメージソース
        /// </summary>
        private RenderTargetBitmap _imageSource;
        public RenderTargetBitmap ImageSource
        {
            get { return _imageSource; }
            set { SetProperty(ref _imageSource, value); }
        }

        /// <summary>
        /// 拡大率(%)
        /// </summary>
        private int _expansionRate = 100;
        public int ExpansionRate
        {
            get { return _expansionRate; }
            set { SetProperty(ref _expansionRate, value / 10 * 10); }
        }

        /// <summary>
        /// ViewBoxの幅（拡大率に合わせて調整する）
        /// </summary>
        private int? _viewBoxWidth;
        public int? ViewBoxWidth
        {
            get { return _viewBoxWidth; }
            set { SetProperty(ref _viewBoxWidth, value); }
        }

        /// <summary>
        /// ViewBoxの高さ（拡大率に合わせて調整する）
        /// </summary>
        private int? _viewBoxHeight;
        public int? ViewBoxHeight
        {
            get { return _viewBoxHeight; }
            set { SetProperty(ref _viewBoxHeight, value); }
        }

        /// <summary>
        /// 選択中のツールバーボタン
        /// </summary>
        private string _selectedToolBarButton;
        public string SelectedToolBarButton
        {
            get { return _selectedToolBarButton; }
            set {
                SetProperty(ref _selectedToolBarButton, value);
                Properties.Settings.Default.SelectedToolBarButton = value;
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// 選択中四角形のマージン
        /// </summary>
        private Thickness _selectingRectangleMargin;
        public Thickness SelectingRectangleMargin
        {
            get { return _selectingRectangleMargin; }
            set { SetProperty(ref _selectingRectangleMargin, value); }
        }

        /// <summary>
        /// 選択中四角形の幅
        /// </summary>
        private int _selectingRectangleWidth = 100;
        public int SelectingRectangleWidth
        {
            get { return _selectingRectangleWidth; }
            set { SetProperty(ref _selectingRectangleWidth, value); }
        }

        /// <summary>
        /// 選択中四角形の高さ
        /// </summary>
        private int _selectingRectangleHeight = 100;
        public int SelectingRectangleHeight
        {
            get { return _selectingRectangleHeight; }
            set { SetProperty(ref _selectingRectangleHeight, value); }
        }

        /// <summary>
        /// 選択中四角形の表示状態
        /// </summary>
        private Visibility _rectangleVisibility = Visibility.Hidden;
        public Visibility RectangleVisibility
        {
            get { return _rectangleVisibility; }
            set { SetProperty(ref _rectangleVisibility, value); }
        }

        /// <summary>
        /// 描画する色
        /// </summary>
        private Color _selectedColor = Color.FromRgb(255, 0, 0);
        public Color SelectedColor
        {
            get { return _selectedColor; }
            set { SetProperty(ref _selectedColor, value); }
        }

        /// <summary>
        /// 編集フラグ
        /// </summary>
        private bool _isModify = false;
        public bool IsModify
        {
            get { return _isModify; }
            set { SetProperty(ref _isModify, value); }
        }

        ///////////////////////////////////////////////
        // コマンド

        /// <summary>
        /// フォルダー選択ダイアログ
        /// </summary>
        private DelegateCommand _folderSelectCommand;
        public DelegateCommand FolderSelectCommand =>
            _folderSelectCommand ?? (_folderSelectCommand = new DelegateCommand(ExecuteFolderSelectCommand));

        void ExecuteFolderSelectCommand()
        {
            var dlg = new CommonOpenFileDialog("フォルダー選択");
            dlg.IsFolderPicker = true;
            dlg.EnsurePathExists = true;
            var ret = dlg.ShowDialog();
            if (ret == CommonFileDialogResult.Ok)
            {
                FolderPath = dlg.FileName;
                Properties.Settings.Default.FolderPath = FolderPath;
                Properties.Settings.Default.Save();

                UpdateFileList();
            }
        }

        /// <summary>
        /// エクスプローラーで開く
        /// </summary>
        private DelegateCommand _openExplorerCommand;
        public DelegateCommand OpenExplorerCommand =>
            _openExplorerCommand ?? (_openExplorerCommand = new DelegateCommand(ExecuteOpenExplorerCommand));

        void ExecuteOpenExplorerCommand()
        {
            if (Directory.Exists(FolderPath))
            {
                System.Diagnostics.Process.Start(FolderPath);
            }
        }

        /// <summary>
        /// ファイル一覧の更新
        /// </summary>
        private DelegateCommand _updateFileListCommand;
        public DelegateCommand UpdateFileListCommand =>
            _updateFileListCommand ?? (_updateFileListCommand = new DelegateCommand(ExecuteUpdateFileListCommand));

        void ExecuteUpdateFileListCommand()
        {
            UpdateFileList();
        }

        /// <summary>
        /// ファイルの削除
        /// </summary>
        private DelegateCommand _deleteFileCommand;
        public DelegateCommand DeleteFileCommand =>
            _deleteFileCommand ?? (_deleteFileCommand = new DelegateCommand(ExecuteDeleteFileCommand));

        void ExecuteDeleteFileCommand()
        {
            if (SelectedFile != null && 
                MessageBoxResult.No == MessageBox.Show(SelectedFile.FileName + " を削除します。\nよろしいですか？",
                APP_NAME, MessageBoxButton.YesNo))
            {
                return;
            }
            DeleteSelectedFile();
        }

        /// <summary>
        /// ファイルを上に移動
        /// </summary>
        private DelegateCommand _onUpFileCommand;
        public DelegateCommand OnUpFileCommand =>
            _onUpFileCommand ?? (_onUpFileCommand = new DelegateCommand(ExecuteOnUpFileCommand));

        void ExecuteOnUpFileCommand()
        {
            UpFile();
        }

        /// <summary>
        /// ファイルを下に移動
        /// </summary>
        private DelegateCommand _onDownFileCommand;
        public DelegateCommand OnDownFileCommand =>
            _onDownFileCommand ?? (_onDownFileCommand = new DelegateCommand(ExecuteOnDownFileCommand));

        void ExecuteOnDownFileCommand()
        {
            DownFile();
        }

        /// <summary>
        /// フルスクリーンの切り替え
        /// </summary>
        private DelegateCommand _fullScreenCommand;
        public DelegateCommand FullScreenCommand =>
            _fullScreenCommand ?? (_fullScreenCommand = new DelegateCommand(ExecuteFullScreenCommand));

        void ExecuteFullScreenCommand()
        {
            ResizeMode = (WindowState != WindowState.Normal) ? ResizeMode.CanResizeWithGrip : ResizeMode.NoResize;
            WindowStyle = (WindowState != WindowState.Normal) ? WindowStyle.SingleBorderWindow : WindowStyle.None;
            WindowState = (WindowState == WindowState.Normal)?WindowState.Maximized:WindowState.Normal;
        }

        /// <summary>
        /// ファイル選択変更
        /// </summary>
        private DelegateCommand _fileSelectionChangedCommand;
        public DelegateCommand FileSelectionChangedCommand =>
            _fileSelectionChangedCommand ?? (_fileSelectionChangedCommand = new DelegateCommand(ExecuteSearchItemSelectionChangedCommand));

        void ExecuteSearchItemSelectionChangedCommand()
        {
            LoadImage();
        }

        /// <summary>
        /// 拡大縮小操作
        /// </summary>
        private DelegateCommand _expansionRateChangedCommand;
        public DelegateCommand ExpansionRateChangedCommand =>
            _expansionRateChangedCommand ?? (_expansionRateChangedCommand = new DelegateCommand(ExecuteExpansionRateChangedCommand));

        void ExecuteExpansionRateChangedCommand()
        {
            UpdateExpansionRate();
        }

        /// <summary>
        /// マウスホイール操作
        /// </summary>
        private DelegateCommand<MouseWheelEventArgs> _mouseWheelCommand;
        public DelegateCommand<MouseWheelEventArgs> MouseWheelCommand =>
            _mouseWheelCommand ?? (_mouseWheelCommand = new DelegateCommand<MouseWheelEventArgs>(ExecuteMouseWheelCommand));

        void ExecuteMouseWheelCommand(MouseWheelEventArgs arg)
        {
            if ((Keyboard.GetKeyStates(Key.LeftCtrl) & KeyStates.Down) == KeyStates.Down ||
               (Keyboard.GetKeyStates(Key.RightCtrl) & KeyStates.Down) == KeyStates.Down)
            {
                arg.Handled = true;
                if (arg.Delta > 0)
                {
                    if (ExpansionRate < 500)
                    {
                        ExpansionRate += 10;
                    }
                }
                else
                {
                    if (0 < ExpansionRate)
                    {
                        ExpansionRate -= 10;
                    }
                }
            }
            else if ((Keyboard.GetKeyStates(Key.LeftShift) & KeyStates.Down) == KeyStates.Down ||
               (Keyboard.GetKeyStates(Key.RightShift) & KeyStates.Down) == KeyStates.Down)
            {
                // Shift+ホイールでスクロール
            }else
            {
                arg.Handled = true;
                // ファイル前後移動
                if (IsModify)
                {
                    return;
                }
                FileItemViewModel nextFile = null;
                if (arg.Delta > 0)
                {
                    nextFile = GetNextFile(-1);
                }
                else
                {
                    nextFile = GetNextFile(1);
                }
                if(nextFile != null)
                {
                    SelectedFile = nextFile;
                }
            }
        }

        /// <summary>
        /// マウスカーソル移動時
        /// </summary>
        private DelegateCommand<MouseEventArgs> _mouseMoveCommand;
        public DelegateCommand<MouseEventArgs> MouseMoveCommand =>
            _mouseMoveCommand ?? (_mouseMoveCommand = new DelegateCommand<MouseEventArgs>(ExecuteMouseMoveCommand));

        void ExecuteMouseMoveCommand(MouseEventArgs arg)
        {
            if(arg.LeftButton == MouseButtonState.Pressed)
            {
                if (SelectedToolBarButton == "rectangle")
                {
                    var ptScrollViewer = GetPositionIF.GetPositionFromScrollViewer(arg);
                    var ptViewBox = GetPositionIF.GetPositionFromViewBox(arg);

                    if ((int)ptScrollViewer.X < (int)DragStartPosScrollViewer.X)
                    {
                        SelectingRectangleWidth = (int)(DragStartPosScrollViewer.X - ptScrollViewer.X);
                        SelectingRectangleMargin = new Thickness(ptScrollViewer.X, SelectingRectangleMargin.Top, 0, 0);
                    }
                    else
                    {
                        SelectingRectangleWidth = (int)(ptScrollViewer.X - DragStartPosScrollViewer.X);
                        SelectingRectangleMargin = new Thickness(DragStartPosScrollViewer.X, SelectingRectangleMargin.Top, 0, 0);

                    }
                    if ((int)ptScrollViewer.Y < (int)DragStartPosScrollViewer.Y)
                    {
                        SelectingRectangleHeight = (int)(DragStartPosScrollViewer.Y - ptScrollViewer.Y);
                        SelectingRectangleMargin = new Thickness(SelectingRectangleMargin.Left, ptScrollViewer.Y, 0, 0);
                    }
                    else
                    {
                        SelectingRectangleHeight = (int)(ptScrollViewer.Y - DragStartPosScrollViewer.Y);
                        SelectingRectangleMargin = new Thickness(SelectingRectangleMargin.Left, DragStartPosScrollViewer.Y, 0, 0);
                    }
                }
            }
        }

        /// <summary>
        /// マウス左クリック
        /// </summary>
        private DelegateCommand<MouseEventArgs> _mouseLeftButtonDownCommand;
        public DelegateCommand<MouseEventArgs> MouseLeftButtonDownCommand =>
            _mouseLeftButtonDownCommand ?? (_mouseLeftButtonDownCommand = new DelegateCommand<MouseEventArgs>(ExecuteMouseLeftButtonDownCommand));

        void ExecuteMouseLeftButtonDownCommand(MouseEventArgs arg)
        {
            if(SelectedToolBarButton == "rectangle")
            {
                DragStartPosViewBox = GetPositionIF.GetPositionFromViewBox(arg);
                DragStartPosScrollViewer = GetPositionIF.GetPositionFromScrollViewer(arg);
                SelectingRectangleMargin = new Thickness(DragStartPosScrollViewer.X, DragStartPosScrollViewer.Y, 0, 0);
                SelectingRectangleWidth = 0;
                SelectingRectangleHeight = 0;
                RectangleVisibility = Visibility.Visible;
            }
            if(SelectedToolBarButton == "text")
            {
                DragStartPosViewBox = GetPositionIF.GetPositionFromViewBox(arg);
                DragStartPosScrollViewer = GetPositionIF.GetPositionFromScrollViewer(arg);

                var text = TextInputWindowIF.ShowTextInputWindow();
                if(text != null)
                {
                    DrawText(text);
                }
            }
        }

        /// <summary>
        /// マウス左ボタン解除
        /// </summary>
        private DelegateCommand<MouseEventArgs> _mouseLeftButtonUpCommand;
        public DelegateCommand<MouseEventArgs> MouseLeftButtonUpCommand =>
            _mouseLeftButtonUpCommand ?? (_mouseLeftButtonUpCommand = new DelegateCommand<MouseEventArgs>(ExecuteMouseLeftButtonUpCommand));

        void ExecuteMouseLeftButtonUpCommand(MouseEventArgs arg)
        {
            if (SelectedToolBarButton == "rectangle")
            {
                var pt = GetPositionIF.GetPositionFromViewBox(arg);
                if (RectangleVisibility == Visibility.Visible)
                {
                    RectangleVisibility = Visibility.Hidden;
                    DragEndPosViewBox = GetPositionIF.GetPositionFromViewBox(arg);
                    DragEndPosScrollViewer = GetPositionIF.GetPositionFromScrollViewer(arg);
                    DrawRectangle();
                }
            }
        }

        /// <summary>
        /// 色選択
        /// </summary>
        private DelegateCommand _colorPickCommand;
        public DelegateCommand ColorPickCommand =>
            _colorPickCommand ?? (_colorPickCommand = new DelegateCommand(ExecuteColorPickCommand));

        void ExecuteColorPickCommand()
        {
            var color = ColorDialogIF.ShowColorDialog();
            if(color != null)
            {
                SelectedColor = (Color)color;

                Properties.Settings.Default.Color_R = SelectedColor.R;
                Properties.Settings.Default.Color_G = SelectedColor.G;
                Properties.Settings.Default.Color_B = SelectedColor.B;
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// 保存
        /// </summary>
        private DelegateCommand _saveCommand;
        public DelegateCommand SaveCommand =>
            _saveCommand ?? (_saveCommand = new DelegateCommand(ExecuteSaveCommand));

        void ExecuteSaveCommand()
        {
            if (!IsModify)
            {
                return;
            }
            SaveImage();
            IsModify = false;
        }

        /// <summary>
        /// キャンセル
        /// </summary>
        private DelegateCommand _cancelCommand;
        public DelegateCommand CancelCommand =>
            _cancelCommand ?? (_cancelCommand = new DelegateCommand(ExecuteCommandName));

        void ExecuteCommandName()
        {
            LoadImage();
            IsModify = false;
        }

        ///////////////////////////////////////////////
        // ロジック

        /// <summary>
        /// ファイル一覧の更新
        /// </summary>
        void UpdateFileList()
        {
            FileItems.Clear();

            if (!Directory.Exists(FolderPath))
            {
                return;
            }

            List<string> list = new List<string>();
            try
            {
                list = new List<string>(Directory.GetFiles(FolderPath, "*.*", SearchOption.AllDirectories));
                list.Sort(new PathComparer());
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            foreach (var item in list)
            {
                FileItems.Add(new FileItemViewModel()
                {
                    FileName = Path.GetFileName(item),
                    FolderPath = Path.GetDirectoryName(item).Replace(FolderPath, "."),
                    FolderFullPath = Path.GetDirectoryName(item)
                });
            }
        }

        /// <summary>
        /// 比較
        /// </summary>
        private class PathComparer : IComparer<string>
        {
            [System.Runtime.InteropServices.DllImport("shlwapi.dll",
CharSet = System.Runtime.InteropServices.CharSet.Unicode,
ExactSpelling = true)]
            private static extern int StrCmpLogicalW(string x, string y);

            public int Compare(string x, string y)
            {
                string folder1 = Path.GetDirectoryName(x);
                string folder2 = Path.GetDirectoryName(y);
                string file1 = Path.GetFileName(x);
                string file2 = Path.GetFileName(y);

                var cmp1 = StrCmpLogicalW(folder1, folder2);
                var cmp2 = StrCmpLogicalW(file1, file2);

                if (cmp1 != 0)
                {
                    return cmp1;
                }
                return cmp2;
            }
        }

        /// <summary>
        /// 選択されたファイルの削除
        /// </summary>
        private void DeleteSelectedFile()
        {
            if (SelectedFile != null)
            {
                var fullPath = Path.Combine(SelectedFile.FolderFullPath, SelectedFile.FileName);
                try
                {
                    File.Delete(fullPath);
                    FileItems.Remove(SelectedFile);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            }
        }

        /// <summary>
        /// ファイルを上に移動
        /// </summary>
        private void UpFile()
        {
            var prevItem = GetNextFile(-1);
            if(prevItem == null)
            {
                return;
            }
            if (ReplaceFile(prevItem, SelectedFile))
            {
                SelectedFile = prevItem;
            }
        }

        /// <summary>
        /// ファイルを下に移動
        /// </summary>
        private void DownFile()
        {
            var nextItem = GetNextFile(1);
            if (nextItem == null)
            {
                return;
            }
            if(ReplaceFile(SelectedFile, nextItem))
            {
                SelectedFile = nextItem;
            }
        }

        /// <summary>
        /// 選択しているファイルの前後のアイテムを取得する
        /// </summary>
        /// <param name="offset">選択アイテムに対するオフセット</param>
        /// <returns></returns>
        private FileItemViewModel GetNextFile(int offset)
        {
            int index = FileItems.IndexOf(SelectedFile);
            if(index < 0)
            {
                return null;
            }

            if(0 <= index + offset && index + offset < FileItems.Count)
            {
                return FileItems[index + offset];
            }

            return null;
        }

        /// <summary>
        /// ファイル名を入れ替える。
        /// フォルダと拡張子が同じことが条件。
        /// </summary>
        /// <param name="item1"></param>
        /// <param name="item2"></param>
        /// <returns>移動したらtrue</returns>
        private bool ReplaceFile(FileItemViewModel item1, FileItemViewModel item2)
        {
            var ext1 = Path.GetExtension(item1.FileName);
            var ext2 = Path.GetExtension(item2.FileName);

            if(item1.FolderFullPath != item2.FolderFullPath)
            {
                return false;
            }
            if(ext1 != ext2 || ext1 == null || ext2 == null)
            {
                return false;
            }

            try
            {
                //ファイルを閉じる
                ImageSource = null;

                //ファイル1を一時ファイル名に変更
                string tempFileName = "";
                string tempFilePath;
                for (int i = 0; ; i++)
                {
                    tempFileName = "_temp_file_" + i;
                    tempFilePath = Path.Combine(item1.FolderFullPath, tempFileName);
                    if (!File.Exists(tempFilePath))
                    {
                        break;
                    }
                }
                File.Move(item1.FullPath, tempFilePath);

                //ファイル2をファイル1の名称に変更
                File.Move(item2.FullPath, item1.FullPath);

                //一時ファイルをファイル2に変更
                File.Move(tempFilePath, item2.FullPath);
                
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 画像を読み込む
        /// </summary>
        private void LoadImage()
        {
            if (SelectedFile == null)
            {
                ImageSource = null;
                ViewBoxWidth = null;
                ViewBoxHeight = null;
                return;
            }

            // 拡張子チェック
            var ext = Path.GetExtension(SelectedFile.FileName).ToLower();

            if(ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".bmp")
            {
                // ビットマップの読み込み
                BitmapImage tmpBitmap = new BitmapImage();
                FileStream stream = File.OpenRead(SelectedFile.FullPath);
                tmpBitmap.BeginInit();
                tmpBitmap.CacheOption = BitmapCacheOption.OnLoad;
                tmpBitmap.StreamSource = stream;
                tmpBitmap.EndInit();
                stream.Close();

                // 描画可能なビットマップに変更
                DrawingVisual drawingVisual = new DrawingVisual();
                DrawingContext drawingContext = drawingVisual.RenderOpen();
                drawingContext.DrawImage(tmpBitmap, new Rect(0, 0, tmpBitmap.Width, tmpBitmap.Height));
                drawingContext.Close();

                RenderTargetBitmap bitmap = new RenderTargetBitmap((int)tmpBitmap.Width, (int)tmpBitmap.Height,
                                                                tmpBitmap.DpiX, tmpBitmap.DpiY, PixelFormats.Pbgra32);
                bitmap.Render(drawingVisual);
                ImageSource = bitmap;

                // 倍率設定
                UpdateExpansionRate();
            }
            else
            {
                ImageSource = null;
                ViewBoxWidth = null;
                ViewBoxHeight = null;
            }
        }

        /// <summary>
        /// 画像の保存
        /// </summary>
        private void SaveImage()
        {
            using (var os = new FileStream(SelectedFile.FullPath, FileMode.OpenOrCreate))
            {
                // 変換したBitmapをエンコードしてFileStreamに保存する。
                // BitmapEncoder が指定されなかった場合は、PNG形式とする。
                var ext = Path.GetExtension(SelectedFile.FileName).ToLower();
                BitmapEncoder encoder = null;
                switch (ext)
                {
                    case ".png":
                        encoder = new PngBitmapEncoder();
                        break;
                    case ".jpg":
                    case ".jpeg":
                        encoder = new JpegBitmapEncoder();
                        break;
                    case ".bmp":
                    default:
                        encoder = new BmpBitmapEncoder();
                        break;
                }
                encoder.Frames.Add(BitmapFrame.Create(ImageSource));
                encoder.Save(os);
            }
        }

        /// <summary>
        /// 拡大率更新
        /// </summary>
        private void UpdateExpansionRate()
        {
            if(ImageSource == null)
            {
                return;
            }
            // 倍率設定
            ViewBoxWidth = (int)ImageSource.Width * ExpansionRate / 100;
            ViewBoxHeight = (int)ImageSource.Height * ExpansionRate / 100;
        }

        /// <summary>
        /// 四角形を描画
        /// </summary>
        private void DrawRectangle()
        {
            DrawingVisual drawingVisual = new DrawingVisual();
            DrawingContext drawingContext = drawingVisual.RenderOpen();

            //拡大率で割る
            var startPos = new Point(DragStartPosViewBox.X * 100 / ExpansionRate, DragStartPosViewBox.Y * 100 / ExpansionRate);
            var endPos = new Point(DragEndPosViewBox.X * 100 / ExpansionRate, DragEndPosViewBox.Y * 100 / ExpansionRate);
            drawingContext.DrawRectangle(Brushes.Transparent, new Pen(new SolidColorBrush(SelectedColor), 2), new Rect(startPos, endPos));
            drawingContext.Close();

            ImageSource.Render(drawingVisual);
            IsModify = true;
        }

        /// <summary>
        /// テキストを描画
        /// </summary>
        /// <param name="text"></param>
        private void DrawText(string text)
        {
            DrawingVisual drawingVisual = new DrawingVisual();
            DrawingContext drawingContext = drawingVisual.RenderOpen();

            //拡大率で割る
            var textPos = new Point(DragStartPosViewBox.X * 100 / ExpansionRate, DragStartPosViewBox.Y * 100 / ExpansionRate);

            drawingContext.DrawText(
                new FormattedText(
                    text, 
                    System.Globalization.CultureInfo.CurrentCulture, 
                    FlowDirection.LeftToRight,
                    new Typeface("メイリオ"),
                    14,
                    new SolidColorBrush(SelectedColor)),
                textPos);
            drawingContext.Close();

            ImageSource.Render(drawingVisual);
            IsModify = true;
        }
    }
}
