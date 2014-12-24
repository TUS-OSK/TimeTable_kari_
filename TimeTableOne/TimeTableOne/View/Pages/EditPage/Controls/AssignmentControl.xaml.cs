﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// ユーザー コントロールのアイテム テンプレートについては、http://go.microsoft.com/fwlink/?LinkId=234236 を参照してください

namespace TimeTableOne.View.Pages.EditPage.Controls
{
    public sealed partial class AssignmentControl : UserControl
    {
        public AssignmentControl()
        {
            this.InitializeComponent();
            Loaded += AssignmentControl_Loaded;

        }

        void AssignmentControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = new AssignmentControlViewModel();
        }


        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            //Expand Popup
            AddAssignmentPopup.IsOpen = !AddAssignmentPopup.IsOpen;
        }
    }
}