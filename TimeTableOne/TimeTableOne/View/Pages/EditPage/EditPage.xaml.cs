﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TimeTableOne.Common;
using TimeTableOne.Utils;
using TimeTableOne.View.Pages.EditPage.Controls;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.Connectivity;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using TimeTableOne.Data;

// 空白ページのアイテム テンプレートについては、http://go.microsoft.com/fwlink/?LinkId=234238 を参照してください

namespace TimeTableOne.View.Pages.EditPage
{
    /// <summary>
    /// グループのタイトル、グループ内のアイテムの一覧、および現在選択されているアイテムの
    /// 詳細を表示するページ。
    /// </summary>
    public sealed partial class EditPage : Page
    {
        private NavigationHelper navigationHelper;

        private CoreDispatcher dispatcher;
        public EditPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            navigationHelper.SaveState += navigationHelper_SaveState;
            YesControl.Visibility = Visibility.Collapsed;
            NoControl.Visibility = Visibility.Collapsed;
            Grid1.Visibility = Visibility.Collapsed;
            dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
            NetworkInformation.NetworkStatusChanged += NetworkInformation_NetworkStatusChanged;
        }

        void NetworkInformation_NetworkStatusChanged(object sender)
        {

            dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                Debug.WriteLine("dispatcher called.");
                EditPageUpdateEvents.ReloadOneNote();
            });
        }

        void NetworkChange_NetworkAddressChanged(object sender, EventArgs e)
        {

        }

        void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }


        /// <summary>
        /// NavigationHelper は、ナビゲーションおよびプロセス継続時間管理を
        /// 支援するために、各ページで使用します。
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// このページには、移動中に渡されるコンテンツを設定します。前のセッションからページを
        /// 再作成する場合は、保存状態も指定されます。
        /// </summary>
        /// <param name="sender">
        /// イベントのソース (通常、<see cref="NavigationHelper"/>)
        /// </param>
        /// <param name="e">このページが最初に要求されたときに
        /// <see cref="Frame.Navigate(Type, Object)"/> に渡されたナビゲーション パラメーターと、
        /// 前のセッションでこのページによって保存された状態の辞書を提供する
        /// イベント データ。ページに初めてアクセスするとき、状態は null になります。</param>
        private async void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            this.DataContext = new EditPageViewModel((TableKey)e.NavigationParameter);
            EditPageUpdateEvents.ReloadOneNoteAction = reloadOneNote;
            EditPageUpdateEvents.ReloadOneNote();
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

        public EditPageViewModel ViewModel
        {
            get { return DataContext as EditPageViewModel; }
        }

        private async void TextBlock_SelectionChanged(object sender, RoutedEventArgs e)
        {
           EditPageUpdateEvents.ReloadOneNote();
        }

        public async void reloadOneNote()
        {
            if (getNetworkStatus())//オンラインかどうか。
            {
            }
            else
            {
                YesControl.Visibility = Visibility.Collapsed;
                NoControl.Visibility = Visibility.Collapsed;
                Grid1.Visibility = Visibility.Visible;
                return;
            }

            if (await OneNoteControl.OneNoteControler.Current.IsExistNotebook(TableUnitDataHelper.GetCurrentSchedule().TableName))
            {
                YesControl.Visibility = Visibility.Collapsed;
                NoControl.Visibility = Visibility.Visible;
            }
            else
            {
                YesControl.Visibility = Visibility.Visible;
                NoControl.Visibility = Visibility.Collapsed;

            }
        }

        private bool getNetworkStatus()
        {
            var data = NetworkInformation.GetInternetConnectionProfile();
            if (data == null) return false;
            return data.IsWlanConnectionProfile || data.IsWwanConnectionProfile || (data.ProfileName=="イーサネット");
        }

    } 
}
