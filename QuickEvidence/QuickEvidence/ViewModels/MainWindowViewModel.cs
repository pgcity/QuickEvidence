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
    }
}
