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
        /// ファイル名
        /// </summary>
        private string _fileName;
        public string FileName
        {
            get { return _fileName; }
            set {
                var oldFullPath = FullPath;
                if (_fileName != null && File.Exists(oldFullPath))
                {
                    //旧パスがある場合は変更を試みる。失敗なら変更しない。
                    var newFullPath = Path.Combine(FolderFullPath, value);
                    try
                    {
                        File.Move(oldFullPath, newFullPath);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message);
                        return;
                    }
                }
                SetProperty(ref _fileName, value);

            }
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
    }
}
