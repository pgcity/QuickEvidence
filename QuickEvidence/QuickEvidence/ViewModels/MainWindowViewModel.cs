using Microsoft.WindowsAPICodePack.Dialogs;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
        public IGetPosition GetPositionIF { get; internal set; }
        public IColorDialog ColorDialogIF { get; internal set; }
        public ITextInputWindow TextInputWindowIF { get; internal set; }

        /// <summary>
        /// DataGrid IF。スクロールとフォーカスの制御
        /// </summary>
        private IDataGrid _dataGridIF;
        public IDataGrid DataGridIF {
            get {
                return _dataGridIF;
            }
            internal set
            {
                _dataGridIF = value;
                if(_dataGridIF != null && SelectedFiles.Count > 0)
                {
                    _dataGridIF.ScrollToItem(SelectedFiles[0]);
                }
            }
        }

        public INavigation NavigationIF { get; internal set; }

        const string APP_NAME = "QuickEvidence";

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainWindowViewModel()
        {
            SelectedToolBarButton = Properties.Settings.Default.SelectedToolBarButton;
            SelectedLineWidthItem = Properties.Settings.Default.SelectedLineWidthItem;
            SelectedColor = Color.FromRgb(
                Properties.Settings.Default.Color_R,
                Properties.Settings.Default.Color_G,
                Properties.Settings.Default.Color_B);
            FolderPath = Properties.Settings.Default.FolderPath;
            if (!Directory.Exists(FolderPath))
            {
                FolderPath = Directory.GetCurrentDirectory();
                Properties.Settings.Default.Save();
            }
            ExpansionRate = Properties.Settings.Default.ExpansionRate;

            UpdateFileList();
            UpdateWindowTitle();

            //過去に選択していたファイルがあれば選択
            if(Properties.Settings.Default.SelectedFiles != null)
            {
                foreach (var file in Properties.Settings.Default.SelectedFiles)
                {
                    var item = (from x in FileItems where x.FullPath == file select x).SingleOrDefault();
                    if (item != null)
                    {
                        item.IsSelected = true;
                    }
                }
                ExecuteFileItemSelectionChangedCommand();
            }
        }

        ///////////////////////////////////////////////
        // プロパティ

        /// <summary>
        /// ドラッグ開始座標
        /// </summary>
        private Point DragStartPosScrollViewer {
            get;set;
        }

        /// <summary>
        /// ドラッグ終了座標
        /// </summary>
        private Point DragEndPosScrollViewer
        {
            get; set;
        }

        /// <summary>
        /// ドラッグ開始座標
        /// </summary>
        private Point DragStartPosViewBox
        {
            get; set;
        }

        /// <summary>
        /// ドラッグ終了座標
        /// </summary>
        private Point DragEndPosViewBox
        {
            get; set;
        }


        ///////////////////////////////////////////////
        // バインディング用プロパティ

        /// <summary>
        /// ウィンドウタイトル
        /// </summary>
        private string _windowTitle = APP_NAME;
        public string WindowTitle
        {
            get { return _windowTitle; }
            set { SetProperty(ref _windowTitle, value); }
        }

        /// <summary>
        /// ファイル一覧
        /// </summary>
        private ObservableCollection<FileItemViewModel> _fileItems = new ObservableCollection<FileItemViewModel>();
        public ObservableCollection<FileItemViewModel> FileItems
        {
            get { return _fileItems; }
            set { SetProperty(ref _fileItems, value); }
        }

        /// <summary>
        /// 選択されたファイル一覧
        /// </summary>
        public IList<FileItemViewModel> SelectedFiles
        {
            get { return (from x in FileItems where x.IsSelected select x).ToList(); }
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
        private RenderTargetBitmap _imageSource;
        public RenderTargetBitmap ImageSource
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
            set {
                SetProperty(ref _selectedToolBarButton, value);
                Properties.Settings.Default.SelectedToolBarButton = value;
                Properties.Settings.Default.Save();

                // カーソル更新
                if (value == "rectangle" || value == "text")
                {
                    ImageMouseCursor = Cursors.Cross;
                }
                else
                {
                    ImageMouseCursor = Cursors.Arrow;
                }
            }
        }

        /// <summary>
        /// 線の太さ
        /// </summary>
        private string _selectedLineWidthItem = "3px";
        public string SelectedLineWidthItem
        {
            get { return _selectedLineWidthItem; }
            set {
                if(value == null)
                {
                    return;
                }
                SetProperty(ref _selectedLineWidthItem, value);
                Properties.Settings.Default.SelectedLineWidthItem = value;
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// 線の太さ（数値）
        /// </summary>
        public int SelectedLineWidth {
            get
            {
                switch (SelectedLineWidthItem)
                {
                    case "5px":
                        return 5;
                    case "3px":
                        return 3;
                    case "1px":
                        return 1;
                }
                return 3;
            }
        }

        /// <summary>
        /// 選択中四角形のマージン
        /// </summary>
        private Thickness _selectingRectangleMargin;
        public Thickness SelectingRectangleMargin
        {
            get { return _selectingRectangleMargin; }
            set { SetProperty(ref _selectingRectangleMargin, value); }
        }

        /// <summary>
        /// 選択中四角形の幅
        /// </summary>
        private int _selectingRectangleWidth = 100;
        public int SelectingRectangleWidth
        {
            get { return _selectingRectangleWidth; }
            set { SetProperty(ref _selectingRectangleWidth, value); }
        }

        /// <summary>
        /// 選択中四角形の高さ
        /// </summary>
        private int _selectingRectangleHeight = 100;
        public int SelectingRectangleHeight
        {
            get { return _selectingRectangleHeight; }
            set { SetProperty(ref _selectingRectangleHeight, value); }
        }

        /// <summary>
        /// 選択中四角形の線の太さ
        /// </summary>
        private int _selectingRectangleLineWidth = 3;
        public int SelectingRectangleLineWidth
        {
            get { return _selectingRectangleLineWidth; }
            set { SetProperty(ref _selectingRectangleLineWidth, value); }
        }

        /// <summary>
        /// 選択中四角形の表示状態
        /// </summary>
        private Visibility _rectangleVisibility = Visibility.Hidden;
        public Visibility RectangleVisibility
        {
            get { return _rectangleVisibility; }
            set { SetProperty(ref _rectangleVisibility, value); }
        }

        /// <summary>
        /// 描画する色
        /// </summary>
        private Color _selectedColor = Color.FromRgb(255, 0, 0);
        public Color SelectedColor
        {
            get { return _selectedColor; }
            set { SetProperty(ref _selectedColor, value); }
        }

        /// <summary>
        /// 編集フラグ
        /// </summary>
        private bool _isModify = false;
        public bool IsModify
        {
            get { return _isModify; }
            set { SetProperty(ref _isModify, value); }
        }

        /// <summary>
        /// 編集フラグ
        /// </summary>
        private bool _isFileNameEditing = false;
        public bool IsFileNameEditing
        {
            get { return _isFileNameEditing; }
            set { SetProperty(ref _isFileNameEditing, value); }
        }

        /// <summary>
        /// 「開く」テキスト
        /// </summary>
        private string _openText =">>";
        public string OpenText
        {
            get { return _openText; }
            set { SetProperty(ref _openText, value); }
        }

        /// <summary>
        /// 画像部分のマウスカーソル
        /// </summary>
        private Cursor _imageMouseCursor = Cursors.Arrow;
        public Cursor ImageMouseCursor
        {
            get { return _imageMouseCursor; }
            set { SetProperty(ref _imageMouseCursor, value); }
        }

        ///////////////////////////////////////////////
        // コマンド

        /// <summary>
        /// Window閉じるときの保存確認
        /// </summary>
        private DelegateCommand<CancelEventArgs> _windowClosingCommand;
        public DelegateCommand<CancelEventArgs> WindowClosingCommand =>
            _windowClosingCommand ?? (_windowClosingCommand = new DelegateCommand<CancelEventArgs>(ExecuteWindowClosingCommand));

        void ExecuteWindowClosingCommand(CancelEventArgs arg)
        {
            if (IsModify)
            {
                switch (MessageBox.Show("変更を保存しますか？", "QuickEvidence", MessageBoxButton.YesNoCancel))
                {
                    case MessageBoxResult.Yes:
                        if (!SaveImage())
                        {
                            arg.Cancel = true;
                            return;
                        }
                        break;
                    case MessageBoxResult.No:
                        break;
                    case MessageBoxResult.Cancel:
                        arg.Cancel = true;
                        return;
                }
            }

            //選択したファイル名を保存
            Properties.Settings.Default.SelectedFiles = new System.Collections.Specialized.StringCollection();
            Properties.Settings.Default.SelectedFiles.AddRange((from x in SelectedFiles select x.FullPath).ToArray());

            //拡大率を保存
            Properties.Settings.Default.ExpansionRate = ExpansionRate;
            Properties.Settings.Default.Save();
        }

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
                Properties.Settings.Default.SelectedFiles?.Clear();
                Properties.Settings.Default.Save();

                foreach (var selectedItem in SelectedFiles)
                {
                    selectedItem.IsSelected = false;
                }
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
            if (SelectedFiles != null && SelectedFiles.Count > 0)
            {
                var itemName = SelectedFiles.Count > 1 ? "選択された " + SelectedFiles.Count + " 個の項目" : SelectedFiles[0].FileName + " ";
                if (MessageBoxResult.No == MessageBox.Show(itemName + "を削除します。\nよろしいですか？",
                    APP_NAME, MessageBoxButton.YesNo))
                {
                    return;
                }
                if (!DeleteSelectedFile())
                {
                    UpdateFileList(); //失敗したらファイルリスト更新
                }
            }
        }

        /// <summary>
        /// 名前の変更
        /// </summary>
        private DelegateCommand _renameCommand;
        public DelegateCommand RenameCommand =>
            _renameCommand ?? (_renameCommand = new DelegateCommand(ExecuteRenameCommand));

        void ExecuteRenameCommand()
        {
            if (SelectedFiles.Count > 1)  //複数選択：連番設定
            {
                EditMultipleFileName();
            }
            else
            {
                IsFileNameEditing = true;
            }
        }

        /// <summary>
        /// ファイルリストでのキー押下
        /// </summary>
        private DelegateCommand<KeyEventArgs> _fileListPreviewKeyDownCommand;
        public DelegateCommand<KeyEventArgs> FileListPreviewKeyDownCommand =>
            _fileListPreviewKeyDownCommand ?? (_fileListPreviewKeyDownCommand = new DelegateCommand<KeyEventArgs>(ExecuteFileListPreviewKeyDownCommand));

        void ExecuteFileListPreviewKeyDownCommand(KeyEventArgs args)
        {
            if (args.Key == Key.Enter && IsFileNameEditing)
            {
                DataGridIF.CommitEdit();
                args.Handled = true;
            }
            if (args.Key == Key.F2 && !IsFileNameEditing)
            {
                ExecuteRenameCommand();
                args.Handled = true;
            }
            if(args.Key == Key.Delete && !IsFileNameEditing)  // Deleteキー：ファイルの削除、ファイル名編集中は右一文字削除に使う
            {
                ExecuteDeleteFileCommand();
                args.Handled = true;
            }
        }

        /// <summary>
        /// ファイルを上に移動
        /// </summary>
        private DelegateCommand _onUpFileCommand;
        public DelegateCommand OnUpFileCommand =>
            _onUpFileCommand ?? (_onUpFileCommand = new DelegateCommand(ExecuteOnUpFileCommand));

        void ExecuteOnUpFileCommand()
        {
            if (!UpFile())
            {
                UpdateFileList(); //失敗したらファイルリスト更新
            }
        }

        /// <summary>
        /// ファイルを下に移動
        /// </summary>
        private DelegateCommand _onDownFileCommand;
        public DelegateCommand OnDownFileCommand =>
            _onDownFileCommand ?? (_onDownFileCommand = new DelegateCommand(ExecuteOnDownFileCommand));

        void ExecuteOnDownFileCommand()
        {
            if (!DownFile())
            {
                UpdateFileList(); //失敗したらファイルリスト更新
            }
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
            _fileSelectionChangedCommand ?? (_fileSelectionChangedCommand = new DelegateCommand(ExecuteFileItemSelectionChangedCommand));

        void ExecuteFileItemSelectionChangedCommand()
        {
            UpdateWindowTitle();
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
            if(arg.Source.GetType() == typeof(System.Windows.Controls.DataGrid))
            {
                return;
            }
            //ドラッグ中は無効
            if(RectangleVisibility == Visibility.Visible)
            {
                arg.Handled = true;
                return;
            }

            if ((Keyboard.GetKeyStates(Key.LeftCtrl) & KeyStates.Down) == KeyStates.Down ||
               (Keyboard.GetKeyStates(Key.RightCtrl) & KeyStates.Down) == KeyStates.Down)
            {
                arg.Handled = true;
                if (arg.Delta > 0)
                {
                    if (ExpansionRate < 500)
                    {
                        ExpansionRate += 10;
                    }
                }
                else
                {
                    if (10 < ExpansionRate)
                    {
                        ExpansionRate -= 10;
                    }
                }
            }
            else if ((Keyboard.GetKeyStates(Key.LeftShift) & KeyStates.Down) == KeyStates.Down ||
               (Keyboard.GetKeyStates(Key.RightShift) & KeyStates.Down) == KeyStates.Down)
            {
                // Shift+ホイールでスクロール
            }else
            {
                arg.Handled = true;
                // ファイル前後移動
                if (arg.Delta > 0)
                {
                    MoveToNextFile(-1);
                }
                else
                {
                    MoveToNextFile(1);
                }
            }
        }

        /// <summary>
        /// マウスカーソル移動時
        /// </summary>
        private DelegateCommand<MouseEventArgs> _mouseMoveCommand;
        public DelegateCommand<MouseEventArgs> MouseMoveCommand =>
            _mouseMoveCommand ?? (_mouseMoveCommand = new DelegateCommand<MouseEventArgs>(ExecuteMouseMoveCommand));

        void ExecuteMouseMoveCommand(MouseEventArgs arg)
        {
            if(arg.LeftButton == MouseButtonState.Pressed)
            {
                if (SelectedToolBarButton == "rectangle")
                {
                    var ptScrollViewer = GetPositionIF.GetPositionFromScrollViewer(arg);
                    var ptViewBox = GetPositionIF.GetPositionFromViewBox(arg);

                    if ((int)ptScrollViewer.X < (int)DragStartPosScrollViewer.X)
                    {
                        SelectingRectangleWidth = (int)(DragStartPosScrollViewer.X - ptScrollViewer.X);
                        SelectingRectangleMargin = new Thickness(ptScrollViewer.X, SelectingRectangleMargin.Top, 0, 0);
                    }
                    else
                    {
                        SelectingRectangleWidth = (int)(ptScrollViewer.X - DragStartPosScrollViewer.X);
                        SelectingRectangleMargin = new Thickness(DragStartPosScrollViewer.X, SelectingRectangleMargin.Top, 0, 0);

                    }
                    if ((int)ptScrollViewer.Y < (int)DragStartPosScrollViewer.Y)
                    {
                        SelectingRectangleHeight = (int)(DragStartPosScrollViewer.Y - ptScrollViewer.Y);
                        SelectingRectangleMargin = new Thickness(SelectingRectangleMargin.Left, ptScrollViewer.Y, 0, 0);
                    }
                    else
                    {
                        SelectingRectangleHeight = (int)(ptScrollViewer.Y - DragStartPosScrollViewer.Y);
                        SelectingRectangleMargin = new Thickness(SelectingRectangleMargin.Left, DragStartPosScrollViewer.Y, 0, 0);
                    }
                }
            }
        }

        /// <summary>
        /// マウス左クリック
        /// </summary>
        private DelegateCommand<MouseEventArgs> _mouseLeftButtonDownCommand;
        public DelegateCommand<MouseEventArgs> MouseLeftButtonDownCommand =>
            _mouseLeftButtonDownCommand ?? (_mouseLeftButtonDownCommand = new DelegateCommand<MouseEventArgs>(ExecuteMouseLeftButtonDownCommand));

        void ExecuteMouseLeftButtonDownCommand(MouseEventArgs arg)
        {
            if(SelectedToolBarButton == "rectangle")
            {
                DragStartPosViewBox = GetPositionIF.GetPositionFromViewBox(arg);
                DragStartPosScrollViewer = GetPositionIF.GetPositionFromScrollViewer(arg);
                SelectingRectangleMargin = new Thickness(DragStartPosScrollViewer.X, DragStartPosScrollViewer.Y, 0, 0);
                SelectingRectangleWidth = 0;
                SelectingRectangleHeight = 0;
                SelectingRectangleLineWidth = SelectedLineWidth;
                RectangleVisibility = Visibility.Visible;
            }
            if(SelectedToolBarButton == "text")
            {
                DragStartPosViewBox = GetPositionIF.GetPositionFromViewBox(arg);
                DragStartPosScrollViewer = GetPositionIF.GetPositionFromScrollViewer(arg);

                var vm = TextInputWindowIF.ShowTextInputWindow();
                if(vm != null)
                {
                    DrawText(vm);
                }
            }
        }

        /// <summary>
        /// マウス左ボタン解除
        /// </summary>
        private DelegateCommand<MouseEventArgs> _mouseLeftButtonUpCommand;
        public DelegateCommand<MouseEventArgs> MouseLeftButtonUpCommand =>
            _mouseLeftButtonUpCommand ?? (_mouseLeftButtonUpCommand = new DelegateCommand<MouseEventArgs>(ExecuteMouseLeftButtonUpCommand));

        void ExecuteMouseLeftButtonUpCommand(MouseEventArgs arg)
        {
            if (SelectedToolBarButton == "rectangle")
            {
                var pt = GetPositionIF.GetPositionFromViewBox(arg);
                if (RectangleVisibility == Visibility.Visible)
                {
                    RectangleVisibility = Visibility.Hidden;
                    DragEndPosViewBox = GetPositionIF.GetPositionFromViewBox(arg);
                    DragEndPosScrollViewer = GetPositionIF.GetPositionFromScrollViewer(arg);
                    DrawRectangle();
                }
            }
        }

        /// <summary>
        /// 色選択
        /// </summary>
        private DelegateCommand _colorPickCommand;
        public DelegateCommand ColorPickCommand =>
            _colorPickCommand ?? (_colorPickCommand = new DelegateCommand(ExecuteColorPickCommand));

        void ExecuteColorPickCommand()
        {
            var color = ColorDialogIF.ShowColorDialog();
            if(color != null)
            {
                SelectedColor = (Color)color;

                Properties.Settings.Default.Color_R = SelectedColor.R;
                Properties.Settings.Default.Color_G = SelectedColor.G;
                Properties.Settings.Default.Color_B = SelectedColor.B;
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// 保存
        /// </summary>
        private DelegateCommand _saveCommand;
        public DelegateCommand SaveCommand =>
            _saveCommand ?? (_saveCommand = new DelegateCommand(ExecuteSaveCommand));

        void ExecuteSaveCommand()
        {
            if (!IsModify)
            {
                return;
            }
            if (SaveImage()){
                IsModify = false;
            }
        }

        /// <summary>
        /// キャンセル
        /// </summary>
        private DelegateCommand _cancelCommand;
        public DelegateCommand CancelCommand =>
            _cancelCommand ?? (_cancelCommand = new DelegateCommand(ExecuteCommandName));

        void ExecuteCommandName()
        {
            if (!IsModify)
            {
                return;
            }
            LoadImage();
            IsModify = false;
        }

        /// <summary>
        /// ViewBoxでの上下キー
        /// </summary>
        private DelegateCommand<KeyEventArgs> _viewBoxKeyDownCommand;
        public DelegateCommand<KeyEventArgs> ViewBoxKeyDownCommand =>
            _viewBoxKeyDownCommand ?? (_viewBoxKeyDownCommand = new DelegateCommand<KeyEventArgs>(ExecuteViewBoxKeyDownCommand));

        void ExecuteViewBoxKeyDownCommand(KeyEventArgs arg)
        {
            if(arg.Key == Key.Up)
            {
                arg.Handled = true;
                MoveToNextFile(-1);
            }
            if (arg.Key == Key.Down)
            {
                arg.Handled = true;
                MoveToNextFile(1);
            }
            if (arg.Key == Key.Home)
            {
                arg.Handled = true;
                if (IsModify)
                {
                    return;
                }
                if (FileItems.Count > 0)
                {
                    SelectSingleItem(FileItems[0]);
                    DataGridIF.ScrollToItem(FileItems[0]);
                }
            }
            if (arg.Key == Key.End)
            {
                arg.Handled = true;
                if (IsModify)
                {
                    return;
                }
                if (FileItems.Count > 0)
                {
                    SelectSingleItem(FileItems[FileItems.Count - 1]);
                    DataGridIF.ScrollToItem(FileItems[FileItems.Count - 1]);
                }
            }
        }

        /// <summary>
        /// F5キー：ファイルリスト更新　ファイル編集中でなければ更新
        /// </summary>
        private DelegateCommand _updateCommand;
        public DelegateCommand UpdateCommand =>
            _updateCommand ?? (_updateCommand = new DelegateCommand(ExecuteUpdateCommand));

        void ExecuteUpdateCommand()
        {
            if (!IsModify)
            {
                UpdateFileList();
            }
        }

        /// <summary>
        /// F1キー：readme.txt表示
        /// </summary>
        private DelegateCommand _helpCommand;
        public DelegateCommand HelpCommand =>
            _helpCommand ?? (_helpCommand = new DelegateCommand(ExecuteHelpCommand));

        void ExecuteHelpCommand()
        {
            if (File.Exists("readme.txt"))
            {
                System.Diagnostics.Process.Start("readme.txt");
            }
        }

        ///////////////////////////////////////////////
        // ロジック

        /// <summary>
        /// ウィンドウタイトルの更新
        /// </summary>
        private void UpdateWindowTitle()
        {
            if (SelectedFiles.Count == 0)
            {
                WindowTitle = APP_NAME;
            }
            else if (SelectedFiles.Count == 1)
            {
                WindowTitle = SelectedFiles[0].FileName + " - " + APP_NAME;
            }
            else
            {
                WindowTitle = SelectedFiles.Count + "個のファイルを選択 - " + APP_NAME;
            }
        }

        /// <summary>
        /// ファイル一覧の更新
        /// </summary>
        void UpdateFileList()
        {
            var tmpSelectedFiles = (from x in SelectedFiles select x.FullPath).ToList();
            FileItems.Clear();

            if (!Directory.Exists(FolderPath))
            {
                return;
            }

            List<string> list = new List<string>();
            try
            {
                list = new List<string>(Directory.GetFiles(FolderPath, "*.*", SearchOption.AllDirectories));
                list.Sort(new PathComparer());
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
                    FolderFullPath = Path.GetDirectoryName(item),
                    IsSelected = false
                });
            }

            //同じパスのファイルを再選択
            if(tmpSelectedFiles != null && FileItems != null)
            {
                foreach(var fullpath in tmpSelectedFiles)
                {
                    var fileItem = (from x in FileItems where x.FullPath == fullpath select x).FirstOrDefault();
                    if(fileItem != null)
                    {
                        fileItem.IsSelected = true;
                    }
                }
            }
        }

        /// <summary>
        /// 比較
        /// </summary>
        private class PathComparer : IComparer<string>
        {
            [System.Runtime.InteropServices.DllImport("shlwapi.dll",
CharSet = System.Runtime.InteropServices.CharSet.Unicode,
ExactSpelling = true)]
            private static extern int StrCmpLogicalW(string x, string y);

            public int Compare(string x, string y)
            {
                string folder1 = Path.GetDirectoryName(x);
                string folder2 = Path.GetDirectoryName(y);
                string file1 = Path.GetFileName(x);
                string file2 = Path.GetFileName(y);

                var cmp1 = StrCmpLogicalW(folder1, folder2);
                var cmp2 = StrCmpLogicalW(file1, file2);

                if (cmp1 != 0)
                {
                    return cmp1;
                }
                return cmp2;
            }
        }

        /// <summary>
        /// 単一アイテムを選択する
        /// </summary>
        /// <param name="item"></param>
        private void SelectSingleItem(FileItemViewModel item)
        {
            foreach(var selectedItem in SelectedFiles)
            {
                selectedItem.IsSelected = false;
            }
            if(item != null)
            {
                item.IsSelected = true;
                DataGridIF.SetCurrentCell(FileItems.IndexOf(item));
            }
        }

        /// <summary>
        /// 前後のファイルに選択を移動する
        /// </summary>
        /// <param name="offset">移動する要素数（マイナスで前に移動）</param>
        private void MoveToNextFile(int offset)
        {
            if (offset == 0)
            {
                return;
            }
            if (IsModify)
            {
                return;
            }
            var baseFile = (offset > 0) ? SelectedFiles.LastOrDefault() : SelectedFiles.FirstOrDefault();
            var nextFile = GetNextFile(baseFile, offset);
            if (nextFile != null)
            {
                SelectSingleItem(nextFile);
                DataGridIF.ScrollToItem(nextFile);
            }
        }

        /// <summary>
        /// 選択されたファイルの削除
        /// </summary>
        private bool DeleteSelectedFile()
        {
            var deleteFiles = new List<FileItemViewModel>(SelectedFiles);
            if (SelectedFiles != null)
            {
                FileItemViewModel nextItem = null; // 削除後に選択するアイテム
                foreach (var deleteFile in deleteFiles)
                {
                    var fullPath = Path.Combine(deleteFile.FolderFullPath, deleteFile.FileName);
                    if (!File.Exists(fullPath))
                    {
                        return false;
                    }
                    try
                    {
                        nextItem = GetNextFile(deleteFile, 1);
                        if(nextItem == null)
                        {
                            nextItem = GetNextFile(deleteFile, -1);
                        }

                        File.Delete(fullPath);
                        FileItems.Remove(deleteFile);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message);
                        return false;
                    }
                }

                if(nextItem != null)
                {
                    SelectSingleItem(nextItem);
                    DataGridIF.ScrollToItem(nextItem);
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// 複数のファイル名を変更
        /// </summary>
        private void EditMultipleFileName()
        {
            var result = NavigationIF.RenameMultipleFiles(EditMultipleFileNameCheck);
            if (result.Result)
            {
                var currentNo = result.StartNo;
                foreach (var file in SelectedFiles)
                {
                    var ext = Path.GetExtension(file.FullPath);
                    if (!file.ChangeFileName(MakeFileName(result.FileName, currentNo, ext)))
                    {
                        return;
                    }
                    currentNo++;
                }
            }
        }

        /// <summary>
        /// 複数のファイル名を変更するためのチェック
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="startNo"></param>
        /// <returns></returns>
        private bool EditMultipleFileNameCheck(string fileName, int startNo, out string resultMessage)
        {
            int currentNo = startNo;
            foreach (var file in SelectedFiles)
            {
                var ext = Path.GetExtension(file.FullPath);
                var newPath = Path.Combine(file.FolderFullPath, MakeFileName(fileName, currentNo, ext));

                if (File.Exists(newPath))
                {
                    resultMessage = "重複するファイル名があるため、ファイル名を変更できません。\n\n" + newPath;
                    return false;
                }
                currentNo++;
            }

            resultMessage = "";
            return true;
        }

        /// <summary>
        /// 連番ファイル名の作成
        /// </summary>
        /// <param name="baseName"></param>
        /// <param name="no"></param>
        /// <param name="ext"></param>
        /// <returns></returns>
        private string MakeFileName(string baseName, int no, string ext)
        {
            return baseName + " (" + no + ")" + ext;
        }

        /// <summary>
        /// ファイルを上に移動
        /// </summary>
        private bool UpFile()
        {
            //選択中のファイルを上から順に並べる
            var fromFiles = (from x in SelectedFiles orderby FileItems.IndexOf(x) select x).ToList();

            var movedFiles = new List<FileItemViewModel>();

            // チェック
            if(fromFiles.Count() == 0)
            {
                return true;
            }
            foreach (var moveFile in fromFiles)
            {
                var destFile = GetNextFile(moveFile , - 1);
                if (destFile == null)
                {
                    return true; //前後のファイルアイテムがないだけなのでエラーにしない
                }

                if (!File.Exists(destFile.FullPath) || !File.Exists(moveFile.FullPath))
                {
                    return false; //ファイルが存在しないのでエラーにする
                }

                if (!CanReplaceFile(destFile, moveFile))
                {
                    return false;
                }
            }

            // 移動
            foreach (var moveFile in fromFiles)
            {
                var prevItem = GetNextFile(moveFile, -1);
                if (prevItem == null)
                {
                    return true; //前後のファイルアイテムがないだけなのでエラーにしない
                }

                if (ReplaceFile(prevItem, moveFile))
                {
                    movedFiles.Add(prevItem);
                }
            }

            // 移動後の選択
            foreach (var item in FileItems)
            {
                item.IsSelected = movedFiles.Contains(item);
            }

            // スクロール
            var scrollItem = movedFiles.First();
            if(scrollItem != null)
            {
                DataGridIF.ScrollToItem(scrollItem);
            }


            return true; //移動にかかわらずファイルは存在するので成功にする
        }

        /// <summary>
        /// ファイルを下に移動
        /// </summary>
        private bool DownFile()
        {
            //選択中のファイルを下から順に並べる
            var fromFiles = (from x in SelectedFiles orderby FileItems.IndexOf(x) descending select x).ToList();

            var movedFiles = new List<FileItemViewModel>();

            // チェック
            if (fromFiles.Count() == 0)
            {
                return true;
            }
            foreach (var fromFile in fromFiles)
            {
                var destFile = GetNextFile(fromFile, 1);
                if (destFile == null)
                {
                    return true;    //前後のファイルアイテムがないだけなのでエラーにしない
                }

                if (!File.Exists(fromFile.FullPath) || !File.Exists(destFile.FullPath))
                {
                    return false; //ファイルが存在しないのでエラーにする
                }

                if (!CanReplaceFile(fromFile, destFile))
                {
                    return false;
                }
            }

            // 移動
            foreach (var moveFile in fromFiles)
            {
                var nextItem = GetNextFile(moveFile, 1);
                if (nextItem == null)
                {
                    return true;    //前後のファイルアイテムがないだけなのでエラーにしない
                }

                if (ReplaceFile(moveFile, nextItem))
                {
                    movedFiles.Add(nextItem);
                }
            }

            // 移動後の選択
            foreach (var item in FileItems)
            {
                item.IsSelected = movedFiles.Contains(item);
            }

            // スクロール
            var scrollItem = movedFiles.First();
            if (scrollItem != null)
            {
                DataGridIF.ScrollToItem(scrollItem);
            }

            return true; //移動にかかわらずファイルは存在するので成功にする
        }

        /// <summary>
        /// 選択しているファイルの前後のアイテムを取得する
        /// </summary>
        /// <param name="offset">選択アイテムに対するオフセット</param>
        /// <returns></returns>
        private FileItemViewModel GetNextFile(FileItemViewModel baseFile, int offset)
        {
            int index = FileItems.IndexOf(baseFile);
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
        /// ファイル名を入れ替え可能か確認する
        /// フォルダと拡張子が同じことが条件。
        /// </summary>
        /// <param name="item1"></param>
        /// <param name="item2"></param>
        /// <returns>移動したらtrue</returns>
        private bool CanReplaceFile(FileItemViewModel item1, FileItemViewModel item2)
        {
            var ext1 = Path.GetExtension(item1.FileName);
            var ext2 = Path.GetExtension(item2.FileName);

            if (item1.FolderFullPath != item2.FolderFullPath)
            {
                return false;
            }
            if (ext1 != ext2 || ext1 == null || ext2 == null)
            {
                return false;
            }

            return true;
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
            if (SelectedFiles.Count == 0 || SelectedFiles.Count > 1)
            {
                ImageSource = null;
                ViewBoxWidth = null;
                ViewBoxHeight = null;
                return;
            }

            // 拡張子チェック
            var ext = Path.GetExtension(SelectedFiles[0].FileName).ToLower();
            var exist = File.Exists(SelectedFiles[0].FullPath);
            if (exist && (ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".bmp"))
            {
                // ビットマップの読み込み
                BitmapImage tmpBitmap = new BitmapImage();

                try
                {
                    using (var stream = File.OpenRead(SelectedFiles[0].FullPath))
                    {
                        tmpBitmap.BeginInit();
                        tmpBitmap.CacheOption = BitmapCacheOption.OnLoad;
                        tmpBitmap.StreamSource = stream;
                        tmpBitmap.EndInit();
                    }
                }
                catch (Exception)
                {
                    ImageSource = null;
                    ViewBoxWidth = null;
                    ViewBoxHeight = null;
                    return;
                }

                // 描画可能なビットマップに変更
                DrawingVisual drawingVisual = new DrawingVisual();
                DrawingContext drawingContext = drawingVisual.RenderOpen();
                drawingContext.DrawImage(tmpBitmap, new Rect(0, 0, tmpBitmap.Width, tmpBitmap.Height));
                drawingContext.Close();

                // 96dpi に固定して読み込み
                RenderTargetBitmap bitmap = new RenderTargetBitmap((int)tmpBitmap.Width, (int)tmpBitmap.Height,
                                                                /*tmpBitmap.DpiX*/96, /*tmpBitmap.DpiY*/96, PixelFormats.Pbgra32);
                bitmap.Render(drawingVisual);
                ImageSource = bitmap;

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
        /// 画像の保存
        /// </summary>
        private bool SaveImage()
        {
            try
            {
                if(SelectedFiles.Count != 1)
                {
                    return false;
                }
                using (var os = new FileStream(SelectedFiles[0].FullPath, FileMode.OpenOrCreate))
                {
                    // 変換したBitmapをエンコードしてFileStreamに保存する。
                    // BitmapEncoder が指定されなかった場合は、PNG形式とする。
                    var ext = Path.GetExtension(SelectedFiles[0].FileName).ToLower();
                    BitmapEncoder encoder = null;
                    switch (ext)
                    {
                        case ".png":
                            encoder = new PngBitmapEncoder();
                            break;
                        case ".jpg":
                        case ".jpeg":
                            encoder = new JpegBitmapEncoder();
                            break;
                        case ".bmp":
                        default:
                            encoder = new BmpBitmapEncoder();
                            break;
                    }
                    encoder.Frames.Add(BitmapFrame.Create(ImageSource));
                    encoder.Save(os);
                    return true;
                }
            }catch(Exception e)
            {
                MessageBox.Show("ファイルの保存でエラーが発生しました\n\n" + e.Message);
                return false;
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

        /// <summary>
        /// 四角形を描画
        /// </summary>
        private void DrawRectangle()
        {
            DrawingVisual drawingVisual = new DrawingVisual();
            DrawingContext drawingContext = drawingVisual.RenderOpen();

            //拡大率で割る
            var startPos = new Point(
                Math.Round(DragStartPosViewBox.X * 100 / ExpansionRate) + 0.5,
                Math.Round(DragStartPosViewBox.Y * 100 / ExpansionRate) + 0.5);
            var endPos = new Point(
                Math.Round(DragEndPosViewBox.X * 100 / ExpansionRate) + 0.5,
                Math.Round(DragEndPosViewBox.Y * 100 / ExpansionRate) + 0.5);
            drawingContext.DrawRectangle(Brushes.Transparent, new Pen(new SolidColorBrush(SelectedColor), SelectedLineWidth), new Rect(startPos, endPos));
            drawingContext.Close();

            ImageSource.Render(drawingVisual);
            IsModify = true;
        }

        /// <summary>
        /// テキストを描画
        /// </summary>
        /// <param name="text"></param>
        private void DrawText(TextInputWindowViewModel vm)
        {
            DrawingVisual drawingVisual = new DrawingVisual();
            DrawingContext drawingContext = drawingVisual.RenderOpen();

            //拡大率で割る
            var textPos = new Point(DragStartPosViewBox.X * 100 / ExpansionRate, DragStartPosViewBox.Y * 100 / ExpansionRate);

            drawingContext.DrawText(
                new FormattedText(
                    vm.Text, 
                    System.Globalization.CultureInfo.CurrentCulture, 
                    FlowDirection.LeftToRight,
                    new Typeface(vm.FontFamily.Source),
                    vm.FontSize,
                    new SolidColorBrush(SelectedColor)),
                textPos);
            drawingContext.Close();

            ImageSource.Render(drawingVisual);
            IsModify = true;
        }
    }
}
