using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QuickEvidence.ViewModels
{
	public class MainWindowViewModel : BindableBase
	{
        public MainWindowViewModel()
        {

        }

        private string _folderPath = "FolderPath";
        public string FolderPath
        {
            get { return _folderPath; }
            set { SetProperty(ref _folderPath, value); }
        }
    }
}
