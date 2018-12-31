using Microsoft.WindowsAPICodePack.Dialogs;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;

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
            WindowState = (WindowState == WindowState.Normal)?WindowState.Maximized:WindowState.Normal;
            WindowStyle = (WindowState == WindowState.Normal) ? WindowStyle.SingleBorderWindow : WindowStyle.None;
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
    }
}
