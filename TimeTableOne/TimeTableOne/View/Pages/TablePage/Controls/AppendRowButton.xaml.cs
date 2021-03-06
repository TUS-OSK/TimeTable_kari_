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

namespace TimeTableOne.View.Pages.TablePage.Controls
{
    public sealed partial class AppendRowButton : UserControl
    {
        public AppendRowButton()
        {
            Loaded += AppendRowButton_Loaded;
            this.InitializeComponent();
        }

        void AppendRowButton_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = new AppendRowButtonViewModel();
        }

        }
}
