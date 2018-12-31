using Microsoft.WindowsAPICodePack.Dialogs;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace QuickEvidence.ViewModels
{
	public class MainWindowViewModel : BindableBase
	{
        public MainWindowViewModel()
        {
            FolderPath = Properties.Settings.Default.FolderPath;
            if (!Directory.Exists(FolderPath))
            {
                FolderPath = Directory.GetCurrentDirectory();
                Properties.Settings.Default.Save();
            }
        }

        private string _folderPath = "FolderPath";
        public string FolderPath
        {
            get { return _folderPath; }
            set { SetProperty(ref _folderPath, value); }
        }

        private DelegateCommand _folderSelectCommand;
        public DelegateCommand FolderSelectCommand =>
            _folderSelectCommand ?? (_folderSelectCommand = new DelegateCommand(ExecuteFolderSelectCommand));

        void ExecuteFolderSelectCommand()
        {
            var dlg = new CommonOpenFileDialog("フォルダー選択");
            dlg.IsFolderPicker = true;
            var ret = dlg.ShowDialog();
            if (ret == CommonFileDialogResult.Ok)
            {
                FolderPath = dlg.FileName;
                Properties.Settings.Default.FolderPath = FolderPath;
                Properties.Settings.Default.Save();
            }
        }

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
    }
}
