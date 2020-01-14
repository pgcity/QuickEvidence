using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace QuickEvidence.ViewModels
{
    public class FileRenameWindowViewModel : BindableBase
    {
        public IClose CloseIF { get; internal set; }
        public IFileRenameWindow FileRenameWindowIF { get; internal set; }
        public FileNameCheckFunc CheckFunc { get; internal set; }

        public FileRenameWindowViewModel()
        {

        }

        /// <summary>
        /// 結果
        /// </summary>
        public bool Result
        {
            get; set;
        } = false;

        //////////////////////////////////////////////
        // プロパティ

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
        /// 開始番号
        /// </summary>
        private int _startNo = 1;
        public int StartNo
        {
            get { return _startNo; }
            set { SetProperty(ref _startNo, value); }
        }

        //////////////////////////////////////////////
        // コマンド

        /// <summary>
        /// ロードされた
        /// </summary>
        private DelegateCommand _loadedCommand;
        public DelegateCommand LoadedCommand =>
            _loadedCommand ?? (_loadedCommand = new DelegateCommand(ExecuteLoadedCommand));

        void ExecuteLoadedCommand()
        {
            FileRenameWindowIF.SelectAll();
        }


        /// <summary>
        /// OKボタンが押された
        /// </summary>
        private DelegateCommand _okCommand;
        public DelegateCommand OKCommand =>
            _okCommand ?? (_okCommand = new DelegateCommand(ExecuteOKCommand));

        void ExecuteOKCommand()
        {
            string resultMessage;
            if(CheckFunc(FileName, StartNo, out resultMessage))
            {
                Result = true;
                CloseIF?.Close();
            }
            else
            {
                System.Windows.MessageBox.Show(resultMessage);
            }

        }

        /// <summary>
        /// キャンセルボタンが押された
        /// </summary>
        private DelegateCommand _cancelCommand;
        public DelegateCommand CancelCommand =>
            _cancelCommand ?? (_cancelCommand = new DelegateCommand(ExecuteCancelCommand));

        void ExecuteCancelCommand()
        {
            Result = false;
            CloseIF?.Close();
        }
    }
}
