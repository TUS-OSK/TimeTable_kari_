﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using TimeTableOne.Utils;
using TimeTableOne.Data;
using TimeTableOne.View.Pages.EditPage.Controls;
using Windows.UI;
using Windows.UI.Xaml;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media;
using TimeTableOne.Annotations;
using TimeTableOne.Utils.Commands;

namespace TimeTableOne.View.Pages.EditPage
{
    public class EditPageViewModel : INotifyPropertyChanged
    {
        public TableKey _key;
        private TableKey _tableKey;
        private string _tableName = "";
        private string _weekDayText = "";
        private string _detailText = "";
        private string _komidashi = "";
        private string _placeInformation = "";
        private string _tableInformation = "";
        private ScheduleData _scheduleData;

        public ScheduleData ScheduleData
        {
            get { return _scheduleData; }
        }
        public event PropertyChangedEventHandler PropertyChanged = delegate { };


        public EditPageViewModel(TableKey key)
        {
            loadData(key);
        }

        public void loadData(TableKey key)
        {
            _key = key;
            this.TableKey = _key;
            _scheduleData = ApplicationData.Instance.GetSchedule(key.NumberOfDay, key.TableNumber);
            if (_scheduleData == null)//登録されているものがなかった場合
            {
                _scheduleData = ScheduleData.GenerateEmpty();
                ScheduleKey keyModel = ScheduleKey.Generate(key.NumberOfDay, key.TableNumber, _scheduleData);
                //cheduleData.OneNoteId = ""; ノートのIDをここに。
                //授業名は_scheduledata.TableNameでとれる。

                ApplicationData.Instance.Keys.Add(keyModel);
                ApplicationData.Instance.Data.Add(_scheduleData);
            }
            else
            {
                Initialize();
            }
            AllDelete = new AlwaysExecutableDelegateCommand(
            DeleteWithCheck);
            OpenOneNote = new AlwaysExecutableDelegateCommand(
           () =>
           {
               OneNoteControl.OneNoteControler.Current.Open(_scheduleData.TableName);
                
           });
            EditPageUpdateEvents.ColorUpdateEvent += () =>
            {
                this.TableColor = new SolidColorBrush(TableUnitDataHelper.GetCurrentSchedule().ColorData);
          
            };
        }
     

        private void Initialize()
        {
            this.TableNumber = _key.TableNumber;
            this.WeekDayText = _key.dayOfWeek.ToString();
            this.DetailText = _scheduleData.Description;
            this.Komidashi = _scheduleData.TableName;
            this.PlaceInformation = _scheduleData.Place;
            this.TableInformation = _scheduleData.FreeFormText;
            this.TableName = _scheduleData.TableName;
        }


        public void saveData()
        {
            if (tableNameEdited)
            {
                _scheduleData.TableName = TableName;
            }
            if (placeEdited)
            {
                _scheduleData.Place = PlaceInformation;
            }
            if (detailTextEdited)
            {
                _scheduleData.Description = DetailText;
            }
            if (freeTextEdited)
            {
                _scheduleData.FreeFormText = TableInformation;
            }
        }

        private bool tableNameEdited;
        private bool placeEdited;
        private bool freeTextEdited;
        private bool detailTextEdited;
        private SolidColorBrush _tableColor;
        private SolidColorBrush _noteColor;
        private EditPageOneNoteControlViewModel _oneNoteControlData;
        public TableKey TableKey { get; set; }

        int TableNumber { get; set; }

        public string WeekDayText { get; set; }

        public string TableName
        {
            get { return _tableName; }
            set
            {
                _tableName = value;
                PropertyChanged(this, new PropertyChangedEventArgs("TableName"));
            }
        }

        public string DetailText
        {
            get { return _detailText; }
            set
            {
                if (value == "")
                {
                    _detailText = _key.dayOfWeek.ToString() + " " + _key.TableNumber.ToString();
                    detailTextEdited = false;
                }
                else
                {
                    _detailText = value;
                    detailTextEdited = true;
                }
                PropertyChanged(this, new PropertyChangedEventArgs("DetailText"));
            }
        }


        public string Komidashi
        {
            get { return _komidashi; }
            set
            {
                if (value == "")
                {
                    TableName = "";
                    tableNameEdited = false;

                }
                else
                {
                    _komidashi = value;
                    TableName = value;
                    tableNameEdited = true;
                }

                PropertyChanged(this, new PropertyChangedEventArgs("Komidashi"));
                PropertyChanged(this, new PropertyChangedEventArgs("RecLength"));
            }
        }

        public string PlaceInformation
        {
            get { return _placeInformation; }
            set
            {
                if (value == "")
                {
                    placeEdited = false;
                }
                else
                {
                    _placeInformation = value;
                    placeEdited = true;
                }
                PropertyChanged(this, new PropertyChangedEventArgs("PlaceInfomation"));
            }
        }

        public string TableInformation
        {
            get { return _tableInformation; }
            set
            {
                if (value == "")
                {
                    freeTextEdited = false;
                }
                else
                {
                    _tableInformation = value;
                    freeTextEdited = true;
                    _scheduleData.FreeFormText = value;
                    ApplicationData.SaveData();
                }
                PropertyChanged(this, new PropertyChangedEventArgs("PlaceInfomation"));
            }
        }

        public int RecLength
        {
            get
            {
                if (_tableName == "")
                {
                    return 18 * 50;
                }
                else
                {
                    return Komidashi.Length * 50 + 50;
                }

            }
        }

        public AlwaysExecutableDelegateCommand AllDelete { get; set; }

        public AlwaysExecutableDelegateCommand OpenOneNote { get; set; }

        private async void DeleteWithCheck()
        {
            MessageDialog dlg = new MessageDialog("本当に削除しますか？");
            dlg.Commands.Add(new UICommand("はい"));
            dlg.Commands.Add(new UICommand("いいえ"));
            dlg.DefaultCommandIndex = 1;// いいえがデフォ
            var cmd = await dlg.ShowAsync();
            // いいえのとき
            if (cmd == dlg.Commands[1])
            {
                return;
            }
            else if (cmd == dlg.Commands[0])
            {
                this._detailText = "";
                this._komidashi = "";
                this._tableName = "";
                this._placeInformation = "";
                this._tableInformation = "";
                PropertyChanged(this, new PropertyChangedEventArgs("TableName"));
                PropertyChanged(this, new PropertyChangedEventArgs("DetailText"));
                PropertyChanged(this, new PropertyChangedEventArgs("Komidashi"));
                PropertyChanged(this, new PropertyChangedEventArgs("RecLength"));
                PropertyChanged(this, new PropertyChangedEventArgs("PlaceInfomation"));
                PropertyChanged(this, new PropertyChangedEventArgs("TableInfomation"));
                _scheduleData.TableName = TableName;
                _scheduleData.Place = PlaceInformation;
                _scheduleData.Description = DetailText;
                _scheduleData.FreeFormText = TableInformation;
            }

        }
        public SolidColorBrush TableColor
        {
            get { return _tableColor; }
            set
            {
                _tableColor = value;
                PropertyChanged(this, new PropertyChangedEventArgs("TableColor"));
            }
        }


        public EditPageOneNoteControlViewModel OneNoteControlData
        {
            get { return _oneNoteControlData; }
            set { _oneNoteControlData = value; }
        }
    }
}
