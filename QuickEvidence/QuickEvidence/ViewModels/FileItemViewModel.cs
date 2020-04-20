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

        public FileItemViewModel(string filePath, string baseFolder, bool isSelected = false)
        {
            FileName = Path.GetFileName(filePath);
            FolderPath = Path.GetDirectoryName(filePath).Replace(baseFolder, ".");
            FolderFullPath = Path.GetDirectoryName(filePath);
            IsSelected = isSelected;
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
                // ファイル名チェック&メッセージ
                if (!FileNameCheck(newName))
                {
                    MessageBox.Show("ファイル名には次の文字は使えません:\n\\ / : * ? \" < > |");
                    return false;
                }

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

        /// <summary>
        /// ファイル名の禁則文字の有無チェック
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>true:問題なし, false:問題あり</returns>
        public static bool FileNameCheck(string fileName)
        {
            var exclusionChar = new char[] { '\\', '/', ':', '*', '?', '\"', '<', '>', '|' };
            if (0 < (from x in exclusionChar where fileName.Contains(x) select x).Count())
            {
                return false;
            }
            return true;
        }
    }
}
