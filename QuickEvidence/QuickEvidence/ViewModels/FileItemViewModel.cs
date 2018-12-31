using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
            set { SetProperty(ref _fileName, value); }
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
                return Path.Combine(FolderFullPath, FileName);
            }
        }
    }
}
