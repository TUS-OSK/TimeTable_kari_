﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using TimeTableOne.Utils;

namespace TimeTableOne.View.Pages.EditPage
{
    class EditPageViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public EditPageViewModel(TableKey key)
        {
            this.TableKey = key;
            this.WeekDayText = key.dayOfWeek.ToString();
            this.TableNumber = key.TableNumber;
            this.DetailText = WeekDayText + " " + TableNumber.ToString();
        }
        public TableKey TableKey { get; set; }
        string WeekDayText { get; set; }
        int TableNumber { get; set; }

        public string DetailText { get; set; }
    }
}