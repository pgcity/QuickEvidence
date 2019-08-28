using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace QuickEvidence.ViewModels
{
	public class FileItemViewModel : BindableBase
	{
        public FileItemViewModel()
        {

        }

        /// <summary>
        /// 選択状態（VMからの設定専用）
        /// </summary>
        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }

        /// <summary>
        /// ファイル名
        /// </summary>
        private string _fileName;
        public string FileName
        {
            get { return _fileName; }
            set {
                if (_fileName != null)
                {
                    ChangeFileName(value);
                }
                else
                {
                    SetProperty(ref _fileName, value);
                }
            }
        }

        /// <summary>
        /// ファイル名を変更
        /// </summary>
        /// <param name="newName">新しいファイル名</param>
        /// <returns></returns>
        public bool ChangeFileName(string newName)
        {
            var oldFullPath = FullPath;

            if (File.Exists(oldFullPath))
            {
                //旧パスがある場合は変更を試みる。失敗なら変更しない。
                var newFullPath = Path.Combine(FolderFullPath, newName);
                try
                {
                    File.Move(oldFullPath, newFullPath);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                    return false;
                }
            }
            else
            {
                MessageBox.Show("ファイル名の変更でエラーが発生しました。\n選択されたファイルが見つかりません。\n" + FileName);
                return false;
            }

            SetProperty(ref _fileName, newName);
            this.RaisePropertyChanged("FileName");
            return true;
        }

        /// <summary>
        /// フォルダーパス
        /// </summary>
        private string _folderPath;
        public string FolderPath
        {
            get { return _folderPath; }
            set { SetProperty(ref _folderPath, value); }
        }

        /// <summary>
        /// フォルダーフルパス
        /// </summary>
        private string _folderFullPath;
        public string FolderFullPath
        {
            get { return _folderFullPath; }
            set { SetProperty(ref _folderFullPath, value); }
        }

        /// <summary>
        /// フルパス
        /// </summary>
        public string FullPath
        {
            get
            {
                if(FolderFullPath == null)
                {
                    return null;
                }
                if(FileName == null)
                {
                    return FolderFullPath;
                }
                return Path.Combine(FolderFullPath, FileName);
            }
        }

        // コマンド

        /// <summary>
        /// ファイルを開く
        /// </summary>
        private DelegateCommand _onOpenCommand;
        public DelegateCommand OnOpenCommand =>
            _onOpenCommand ?? (_onOpenCommand = new DelegateCommand(ExecuteOnOpenCommand));

        void ExecuteOnOpenCommand()
        {
            try
            {
                System.Diagnostics.Process.Start(FullPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
