﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using TimeTableOne.Utils;
using TimeTableOne.View.Pages.EditPage.Controls;

namespace TimeTableOne.View.Pages.EditPage
{
    class EditPageViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public EditPageViewModel(TableKey key)
        {
            this.TableKey = key;
            leftData = new EditPageLeftViewModel(key);
            rightData = new EditPageRightViewModel();
        }
        public TableKey TableKey { get; set; }

        public EditPageLeftViewModel leftData { get; set; }

        public EditPageRightViewModel rightData { get; set; }
    }
}
