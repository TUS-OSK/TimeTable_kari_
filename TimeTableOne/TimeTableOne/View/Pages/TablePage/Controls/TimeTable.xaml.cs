﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// ユーザー コントロールのアイテム テンプレートについては、http://go.microsoft.com/fwlink/?LinkId=234236 を参照してください
using TimeTableOne.Common;

namespace TimeTableOne.View.Pages.TablePage.Controls
{
    public sealed partial class TimeTable : UserControl
    {
        public TimeTableViewModel ViewModel
        {
            get { return DataContext as TimeTableViewModel; }
        }
        
        public TimeTable()
        {
            this.InitializeComponent();
            DataContextChanged += TimeTable_DataContextChanged;
            ScheduleManager.Instance.CurrentScheduleChanged += Instance_CurrentScheduleChanged;
        }

        void Instance_CurrentScheduleChanged(object sender, CurrentScheduleKeyChangedEventArgs e)
        {
            CheckCurrentSchedule();
        }

        void TimeTable_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            CheckCurrentSchedule();
        }

        private void CheckCurrentSchedule()
        {
            if (ViewModel == null) return;
            if (ScheduleManager.Instance.CurrentKey!=null&&ScheduleManager.Instance.CurrentKey.Equals(ViewModel.TableKey))
            {
                VisualStateManager.GoToState(this, "CurrentState", false);
            }
            else
            {
                VisualStateManager.GoToState(this, "NotCurrentState", false);
            }
        }

        private void Button1_Loaded(object sender, RoutedEventArgs e)
        {
            if (Block1.Text == "")
            {
            }
            else
            {
               
            }
        }

        private void SymbolIcon_Loaded(object sender, RoutedEventArgs e)
        {
            if (Block1.Text == "")
            {
            }
            else
            {

            }

        }

    }
}
