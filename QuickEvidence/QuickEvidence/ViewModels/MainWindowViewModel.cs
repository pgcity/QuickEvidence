using Microsoft.WindowsAPICodePack.Dialogs;
using Prism.Commands;
using Prism.Mvvm;
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
        const string APP_NAME = "QuickEvidence";

        public MainWindowViewModel()
        {
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
        /// ファイル一覧
        /// </summary>
        private ObservableCollection<FileItemViewModel> _fileItems = new ObservableCollection<FileItemViewModel>();
        public ObservableCollection<FileItemViewModel> FileItems
        {
            get { return _fileItems; }
            set { SetProperty(ref _fileItems, value); }
        }

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
        private ImageSource _imageSource;
        public ImageSource ImageSource
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
            set { SetProperty(ref _selectedToolBarButton, value); }
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
            arg.Handled = true;
            if ((Keyboard.GetKeyStates(Key.LeftCtrl) & KeyStates.Down) == KeyStates.Down ||
               (Keyboard.GetKeyStates(Key.RightCtrl) & KeyStates.Down) == KeyStates.Down)
            {
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
            else
            {
                // ファイル前後移動
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

            IEnumerable<string> list = new List<string>();
            try
            {
                list = Directory.GetFiles(FolderPath, "*.*", SearchOption.AllDirectories);
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
                // 読み込み
                BitmapImage bmpImage = new BitmapImage();
                FileStream stream = File.OpenRead(SelectedFile.FullPath);
                bmpImage.BeginInit();
                bmpImage.CacheOption = BitmapCacheOption.OnLoad;
                bmpImage.StreamSource = stream;
                bmpImage.EndInit();
                stream.Close();
                ImageSource = bmpImage;

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
    }
}
