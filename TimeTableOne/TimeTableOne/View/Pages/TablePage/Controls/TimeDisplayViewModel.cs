﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using TimeTableOne.Data;
using TimeTableOne.Utils;
using TimeTableOne.Utils.Commands;

namespace TimeTableOne.View.Pages.TablePage.Controls
{
    public class TimeDisplayViewModel:BasicViewModel
    {
        protected TimeDisplayViewModel()
        {
            TimeRegions=new ObservableCollection<TimeDisplayUnitViewModel>();
        }

        public ObservableCollection<TimeDisplayUnitViewModel> TimeRegions { get; set; }

        public static TimeDisplayViewModel GenerateViewModel()
        {

            TimeDisplayViewModel vm=new TimeDisplayViewModel();
            //モデルデータからVMを生成します。
            for (int index = 0; index < ApplicationData.Instance.TimeSpans.Count; index++)
            {
                var scheduleTimeSpan = ApplicationData.Instance.TimeSpans[index];
                vm.TimeRegions.Add(new TimeDisplayUnitViewModel()
                {
                    FromTime = scheduleTimeSpan.FromTime,
                    ToTime = scheduleTimeSpan.ToTime,
                    TimeIndex = index+1,
                    TargetModelSpan = scheduleTimeSpan
                });
            }
            return vm;
    }

    }

    public class TimeDisplayUnitViewModel:BasicViewModel
    {
        private string _fromTimeToEdit;
        private string _toTimeToEdit;
        private DateTime _fromTime;
        private DateTime _toTime;

        public TimeDisplayUnitViewModel()
        {
            
        }
        public int TimeIndex { get; set; }

        public DateTime FromTime
        {
            get { return _fromTime; }
            set
            {
                if (value.Equals(_fromTime)) return;
                this.FromTimeToEdit = string.Format("{0}:{1:D2}", value.Hour, value.Minute);
                _fromTime = value;
                OnPropertyChanged();
            }
        }

        public DateTime ToTime
        {
            get { return _toTime; }
            set
            {
                if (value.Equals(_toTime)) return;
                this.ToTimeToEdit = string.Format("{0}:{1:D2}", value.Hour, value.Minute);
                _toTime = value;
                OnPropertyChanged();
            }
        }

        public string FromTimeToEdit
        {
            get { return _fromTimeToEdit; }
            set
            {
                if (value == _fromTimeToEdit) return;
                _fromTime = ToDate(value);
                _fromTimeToEdit = value;
                OnPropertyChanged();
                OnPropertyChanged("FromTime");
            }
        }

        public string ToTimeToEdit
        {
            get { return _toTimeToEdit; }
            set
            {
                if (value == _toTimeToEdit) return;
                _toTime = ToDate(value);
                _toTimeToEdit = value;
                OnPropertyChanged();
                OnPropertyChanged("ToTime");
            }
        }

        //TODO もっといい感じに。
        private static DateTime ToDate(string str)
        {
            string hour="";
            string minute="";
            string[] splitted = str.Split(':');
            if (splitted.Length == 2)
            {
                hour = splitted[0];
                minute = splitted[1];     
            }
            if (splitted.Length == 1)
            {
                if (str.Length == 3)
                {
                    hour = str[0].ToString();
                    minute = str[1].ToString() + str[2].ToString();
                }
                if (str.Length == 4)
                {
                    hour = str[0].ToString() + str[1].ToString();
                    minute = str[2].ToString() + str[3].ToString();
                }
            }
            try
            {
                return new DateTime(2015, 1, 1, int.Parse(hour), int.Parse(minute), 0);
            }
            catch (Exception)
            {
                return new DateTime();
                
            }
          
        }

        public async Task CommitChange()
        {
            //if ((FromTime-ToTime).TotalMilliseconds > 0)
            //{
            //    //時間の順番がおかしい時
            //    FromTime = TargetModelSpan.FromTime;
            //    ToTime = TargetModelSpan.ToTime;
            //    MessageDialog dialog=new MessageDialog("開始時間は終了時間より前である必要があります。","エラー");
            //    await dialog.ShowAsync();
            //    return false;
            //}
            ////次の時間が先になっていないかチェック
            //int index = ApplicationData.Instance.TimeSpans.IndexOf(TargetModelSpan);
            //if (index == -1)
            //{
            //    throw new InvalidDataContractException();
            //}
            //    if (index != ApplicationData.Instance.TimeSpans.Count - 1)
            //    {
            //        var next = ApplicationData.Instance.TimeSpans[index + 1];
            //        if ((next.FromTime - TargetModelSpan.ToTime).TotalMilliseconds < 0)
            //        {
            //            FromTime = TargetModelSpan.FromTime;
            //            ToTime = TargetModelSpan.ToTime;
            //            MessageDialog dialog = new MessageDialog("終了時間は次の時間の開始時間よりも前である必要があります。", "エラー");
            //            await dialog.ShowAsync();
            //            return false;
            //        }
            //    }
           
                TargetModelSpan.FromTime = FromTime;
                TargetModelSpan.ToTime = ToTime;
                ApplicationData.SaveData();         
        }

        public async Task<ScheduleTimeSpan> GetTargetModelSpan()
        {
            return ScheduleTimeSpan.GenerateFromHourMinute(FromTime.Hour,FromTime.Minute,ToTime.Hour,ToTime.Minute);
        }


        public ScheduleTimeSpan TargetModelSpan { get; set; }

        public static BasicViewModel FromData(int i)
        {
            if (ApplicationData.Instance.TimeSpans.Count != 0)
            {
                var lastData = ApplicationData.Instance.TimeSpans[ApplicationData.Instance.TimeSpans.Count - 1];
                for (int j = 0;
                    j < ApplicationData.Instance.Configuration.TableCount - ApplicationData.Instance.TimeSpans.Count;
                    j++)
                {
                    DateTime fromTime = lastData.ToTime.AddMinutes(TimeSpanAnlyzer.IdealSpanStride());
                    DateTime toTime = fromTime.AddMinutes(TimeSpanAnlyzer.IdealSpanLength());
                     ApplicationData.Instance.TimeSpans.Add(ScheduleTimeSpan.GenerateFromHourMinute(
                        fromTime.Hour,fromTime.Minute,toTime.Hour,toTime.Minute));
                }
            }
            else
            {
                GenerateDefaultSpans(ApplicationData.Instance.TimeSpans);
            }
            var model= ApplicationData.Instance.TimeSpans[i];
            TimeDisplayUnitViewModel vm=new TimeDisplayUnitViewModel();
            vm.TargetModelSpan = model;
            vm.FromTime = model.FromTime;
            vm.ToTime = model.ToTime;
            vm.TimeIndex = i + 1;
            return vm;
        }


        private static void GenerateDefaultSpans(List<ScheduleTimeSpan> spans)
        {//データがない際は以下のデータをデフォルトとして書きだします。
            spans.Add(ScheduleTimeSpan.GenerateFromHourMinute(8, 50, 10, 20));
            spans.Add(ScheduleTimeSpan.GenerateFromHourMinute(10, 30, 12, 0));
            spans.Add(ScheduleTimeSpan.GenerateFromHourMinute(12, 50, 14, 20));
            spans.Add(ScheduleTimeSpan.GenerateFromHourMinute(14, 30, 16, 0));
            spans.Add(ScheduleTimeSpan.GenerateFromHourMinute(16, 10, 17, 40));
            spans.Add(ScheduleTimeSpan.GenerateFromHourMinute(18, 00, 19, 30));
            spans.Add(ScheduleTimeSpan.GenerateFromHourMinute(19, 40, 21, 10));
        }
    }

    public class TimeDisplayUnitViewModelInDesign : TimeDisplayUnitViewModel
    {
        public TimeDisplayUnitViewModelInDesign()
        {
            this.TimeIndex = 1;
            this.FromTime=new DateTime(2015,1,1,8,50,0);
            this.ToTime=new DateTime(2015,1,1,10,20,0);
        }
    }

    public class TimeDisplayViewModelInDesign:TimeDisplayViewModel
    {

        private static DateTime genFromTime(int h, int m)
        {
            return new DateTime(2015,1,1,h,m,0);
        }
        public TimeDisplayViewModelInDesign()
        {
            TimeRegions.Add(new TimeDisplayUnitViewModel(){TimeIndex = 1,FromTime = genFromTime(8,50),ToTime = genFromTime(10,20)});
            TimeRegions.Add(new TimeDisplayUnitViewModel() { TimeIndex = 2, FromTime = genFromTime(10, 30), ToTime = genFromTime(12, 00) });
            TimeRegions.Add(new TimeDisplayUnitViewModel() { TimeIndex = 3, FromTime = genFromTime(12, 50), ToTime = genFromTime(14, 20) });
            TimeRegions.Add(new TimeDisplayUnitViewModel() { TimeIndex = 4, FromTime = genFromTime(14, 30), ToTime = genFromTime(16, 00) });
            TimeRegions.Add(new TimeDisplayUnitViewModel() { TimeIndex = 5, FromTime = genFromTime(16, 10), ToTime = genFromTime(17, 40) });
            TimeRegions.Add(new TimeDisplayUnitViewModel() { TimeIndex = 6, FromTime = genFromTime(18, 00), ToTime = genFromTime(19, 30) });
            TimeRegions.Add(new TimeDisplayUnitViewModel() { TimeIndex = 7, FromTime = genFromTime(19, 40), ToTime = genFromTime(21, 10) });
        }
    }
    public class TimeDisplayUnitTimeConverter : IValueConverter
    {
        public TimeDisplayUnitTimeConverter()
        {
            
        }
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (!(value is DateTime))
            {
                return "";
            }
            DateTime time = (DateTime) value;
            return string.Format("{0}:{1:D2}",time.Hour,time.Minute);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return "";
        }
    }
}
