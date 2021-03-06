﻿using System;
using System.Runtime.InteropServices;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using TimeTableOne.Common;

// 基本ページのアイテム テンプレートについては、http://go.microsoft.com/fwlink/?LinkId=234237 を参照してください
using TimeTableOne.Data;
using TimeTableOne.Utils;
using TimeTableOne.Utils.Commands;
using TimeTableOne.View.Pages.TablePage.Controls;

namespace TimeTableOne.View.Pages.TablePage
{
    /// <summary>
    /// 多くのアプリケーションに共通の特性を指定する基本ページ。
    /// </summary>
    public sealed partial class TablePage : Page
    {

        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        private bool _isTableTitleEditing;

        /// <summary>
        /// これは厳密に型指定されたビュー モデルに変更できます。
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// NavigationHelper は、ナビゲーションおよびプロセス継続時間管理を
        /// 支援するために、各ページで使用します。
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }


        public TablePage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;
            VisualStateManager.GoToState(this, "BasicState", false);
        }

        /// <summary>
        /// このページには、移動中に渡されるコンテンツを設定します。前のセッションからページを
        /// 再作成する場合は、保存状態も指定されます。
        /// </summary>
        /// <param name="sender">
        /// イベントのソース (通常、<see cref="NavigationHelper"/>)>
        /// </param>
        /// <param name="e">このページが最初に要求されたときに
        /// <see cref="Frame.Navigate(Type, Object)"/> に渡されたナビゲーション パラメーターと、
        /// 前のセッションでこのページによって保存された状態の辞書を提供する
        /// セッション。ページに初めてアクセスするとき、状態は null になります。</param>
        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        /// <summary>
        /// アプリケーションが中断される場合、またはページがナビゲーション キャッシュから破棄される場合、
        /// このページに関連付けられた状態を保存します。値は、
        /// <see cref="SuspensionManager.SessionState"/> のシリアル化の要件に準拠する必要があります。
        /// </summary>
        /// <param name="sender">イベントのソース (通常、<see cref="NavigationHelper"/>)</param>
        /// <param name="e">シリアル化可能な状態で作成される空のディクショナリを提供するイベント データ
        ///。</param>
        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper の登録

        /// このセクションに示したメソッドは、NavigationHelper がページの
        /// ナビゲーション メソッドに応答できるようにするためにのみ使用します。
        /// 
        /// ページ固有のロジックは、
        /// <see cref="GridCS.Common.NavigationHelper.LoadState"/>
        /// および <see cref="GridCS.Common.NavigationHelper.SaveState"/> のイベント ハンドラーに配置する必要があります。
        /// LoadState メソッドでは、前のセッションで保存されたページの状態に加え、
        /// ナビゲーション パラメーターを使用できます。

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = new TablePageViewModel();
            RemoveButton.Command = RemoveCommand = new DeleteRowCommand(RemoveRow);
            BasicTableCommands.AddRowCommand = AppendCommand = new AlwaysExecutableDelegateCommand(AppendRow);
        }

        public DeleteRowCommand RemoveCommand { get; set; }

        public CommandBase AppendCommand { get; set; }

        public TablePageViewModel ViewModel
        {
            get { return DataContext as TablePageViewModel; }
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            BottomCommandBar.IsOpen = !BottomCommandBar.IsOpen;
        }

        private void ToggleColumn(object sender, RoutedEventArgs e)
        {
            var config = ApplicationData.Instance.Configuration;
            config.TableTypeSetting = this.MoveNext(config.TableTypeSetting);
            ApplicationData.SaveData();
            ViewModel.TimeTableDataContext = new TimeTableGridViewModel();
            this.DataContext = new TablePageViewModel();
        }

        private TableType MoveNext(TableType t)
        {
            return (TableType)((((int)t) + 1) % Enum.GetValues(typeof(TableType)).Length);
        }

        private void AppendRow(object sender, RoutedEventArgs e)
        {
            AppendRow();
        }

        private void AppendRow()
        {
            var config = ApplicationData.Instance.Configuration;
            config.TableCount++;
            ApplicationData.SaveData();
            ViewModel.TimeTableDataContext = new TimeTableGridViewModel();
            RemoveCommand.NotifyCanExecuteChanged();
        }

        private async void RemoveRow()
        {
            MessageDialog dlg = new MessageDialog("行を削除するとその行に含まれる時間割情報も削除されます。\n本当に削除してもよろしいですか？");
            dlg.Commands.Add(new UICommand("はい"));
            dlg.Commands.Add(new UICommand("いいえ"));
            dlg.DefaultCommandIndex = 1;
            IUICommand cmd = await dlg.ShowAsync();
            if (cmd == dlg.Commands[0])
            {
                var config = ApplicationData.Instance.Configuration;
                config.TableCount--;
                ApplicationData.Instance.CheckAndRemoveRow();
                ApplicationData.SaveData();
                ViewModel.TimeTableDataContext = new TimeTableGridViewModel();
                RemoveCommand.NotifyCanExecuteChanged();
            }
            else if (cmd == dlg.Commands[1])
            {
                return;
            }
        }

        private void PageTitle_OnPointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (_isTableTitleEditing) return;
            VisualStateManager.GoToState(this, "MouseOn", true);
        }

        private void PageTitle_OnPointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (_isTableTitleEditing) return;
            VisualStateManager.GoToState(this, "BasicState", true);
        }

        private void PageTitle_OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            _isTableTitleEditing = true;
            VisualStateManager.GoToState(this, "Editing", true);
        }

        private void TextBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            _isTableTitleEditing = false;
            VisualStateManager.GoToState(this, "BasicState", true);
        }

        private void BackButton_OnClick(object sender, RoutedEventArgs e)
        {
            PageUtil.MovePage(MainStaticPages.DayPage);
        }

        private void ToggleHeaderLanguage(object sender, RoutedEventArgs e)
        {
            var config = ApplicationData.Instance.Configuration;
            config.IsEnglishTablePageHeader = !config.IsEnglishTablePageHeader;
            ApplicationData.SaveData();
            ViewModel.UpdateHeader();
        }

        private void pageRoot_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.DataContext = new TablePageViewModel();
        }


        private void Button1_OnClick(object sender, RoutedEventArgs e)
        {
            TablePageUpdateEvents.OnEditTimeDisplay();
            button1.Visibility = Visibility.Collapsed;
            button2.Visibility = Visibility.Visible;
        }

        private void Button2_OnClick(object sender, RoutedEventArgs e)
        {
            if (TablePageUpdateEvents.OnCommitTimeDisplay())
            {
                button2.Visibility = Visibility.Collapsed;
                button1.Visibility = Visibility.Visible;
            }
        }
    }
}
