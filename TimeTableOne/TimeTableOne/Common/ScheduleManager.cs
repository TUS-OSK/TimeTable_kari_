﻿

using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using TimeTableOne.Data;
using TimeTableOne.Utils;

namespace TimeTableOne.Common
{
    /// <summary>
    ///     スケジュールに関する便利クラス
    /// </summary>
    public class ScheduleManager
    {
        private static ScheduleManager _instance;
        private TableKey _currentKey;
        private TableKey _nextKeyInToday;
        private ScheduleTimeSpan _currentTimeSpan;
        private int _currentTimeSpanIndex;
        private ScheduleTimeSpan _nextTimeSpanInToday;

        /// <summary>
        ///     Instance of Singleton
        /// </summary>
        public static ScheduleManager Instance
        {
            get
            {
                _instance = _instance ?? new ScheduleManager();
                return _instance;
            }
        }


        private ScheduleManager()
        {
            UpdateCurrentTable();
            TimerManager.Instance.GUITick += Instance_GUITick;
        }
        /// <summary>
        /// タイマーによる現在のテーブルの選択の変更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Instance_GUITick(object sender, EventArgs e)
        {
            UpdateCurrentTable();
        }

        /// <summary>
        ///     現在の時間帯が変わったときに呼び出されるイベント
        /// </summary>
        public event EventHandler<CurrentScheduleKeyChangedEventArgs> CurrentScheduleChanged = delegate { };

        /// <summary>
        ///     現在の時間帯の再チェック
        /// </summary>
        public void UpdateCurrentTable()
        {
            bool isConfirmed = false;
            var now = new DateTime(2015, 1, 1, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
            for (var i = 0; i < ApplicationData.Instance.TimeSpans.Count; i++)
            {
                var span = ApplicationData.Instance.TimeSpans[i];
                if ((now - span.FromTime).TotalMilliseconds > 0 && (span.ToTime - now).TotalMilliseconds > 0)
                {
                    isConfirmed = true;
                    if (CurrentKey == null || (CurrentKey.dayOfWeek != now.DayOfWeek || CurrentKey.TableNumber != i + 1))
                    {
                        var candidateKey=new TableKey(i + 1, ((int) DateTime.Now.DayOfWeek)%7);
                        CurrentKey = candidateKey;
                        CurrentTimeSpan = span;
                        CurrentTimeSpanIndex = i + 1;
                    }
                }
            }
            if (!isConfirmed)
            {
                CurrentKey = null;
                CurrentTimeSpanIndex = 0;
            }
            isConfirmed = false;
            for (var i = 0; i < ApplicationData.Instance.TimeSpans.Count; i++)
            {
                var span = ApplicationData.Instance.TimeSpans[i];
                if ((now - span.FromTime).TotalMilliseconds < 0)
                {
                    isConfirmed = true;
                    if (NextKeyInToday == null || (NextKeyInToday.dayOfWeek != now.DayOfWeek || NextKeyInToday.TableNumber != i + 1))
                    {
                        NextKeyInToday = new TableKey(i + 1, ((int)DateTime.Now.DayOfWeek) % 7);
                        NextTimeSpanInToday = span;
                        NextTimeSpanIndexInToday=i+1;
                    }
                    break;
                }
            }
            if (!isConfirmed)
            {
                NextKeyInToday = null;
                NextTimeSpanIndexInToday = 0;
            }
        }

        public ScheduleTimeSpan CurrentTimeSpan
        {
            get { return _currentTimeSpan; }
            set { _currentTimeSpan = value; }
        }

        public int CurrentTimeSpanIndex
        {
            get { return _currentTimeSpanIndex; }
            set { _currentTimeSpanIndex = value; }
        }

        /// <summary>
        ///     DateTime.Nowが指す今のTableKey
        /// </summary>
        public TableKey CurrentKey
        {
            get { return _currentKey; }
            set
            {
                if (_currentKey != null && _currentKey.Equals(value)) return;
                var lastKey = _currentKey;
                _currentKey = value;
                CurrentScheduleChanged(this, new CurrentScheduleKeyChangedEventArgs(lastKey, value));
            }
        }

        public ScheduleData CurrentSchedule
        {
            get
            {
                if (CurrentKey == null) return null;
                return ApplicationData.Instance.GetSchedule(CurrentKey.NumberOfDay, CurrentKey.TableNumber);
            }
        }

        public ScheduleState CurrentScheduleState
        {
            get
            {
                if (CurrentKey == null) return ScheduleState.Empty;
                var no=ApplicationData.Instance.GetNoClassSchedule(DateTimeUtil.Today(), CurrentKey);
                if (no != null)
                {
                    return ScheduleState.NoClass;
                }
                else
                {
                    //TODO 教室変更の際ここに書く必要あり
                    return ScheduleState.Default;
                }
            }
        }

        public ScheduleState NextScheduleState
        {
            get
            {
                if(NextKeyInToday==null)return ScheduleState.Default;
                var no = ApplicationData.Instance.GetNoClassSchedule(DateTimeUtil.Today(),NextKeyInToday);
                if (no != null)
                {
                    return ScheduleState.NoClass;
                }
                else
                {
                    //TODO 教室変更の際ここに書く必要あり
                    return ScheduleState.Default;
                }
            }
        }


        public ScheduleTimeSpan NextTimeSpanInToday
        {
            get { return _nextTimeSpanInToday; }
            set { _nextTimeSpanInToday = value; }
        }

        public int NextTimeSpanIndexInToday { get; set; }

        public TableKey NextKeyInToday
        {
            get { return _nextKeyInToday; }
            set { _nextKeyInToday = value; }
        }

        public ScheduleData NextScheduleInToday
        {
            get
            {
                if (NextKeyInToday == null) return null;
                return ApplicationData.Instance.GetSchedule(NextKeyInToday.NumberOfDay, NextKeyInToday.TableNumber);
            }
        }

        public ScheduleData[] TodaySchedule
        {
            get
            {
                ScheduleData[] data=new ScheduleData[ApplicationData.Instance.TimeSpans.Count];
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = ApplicationData.Instance.GetSchedule((int) DateTime.Today.DayOfWeek, i + 1);
                }
                return data;
            }
        }

        /// <summary>
        /// 今日を含めた週もしくはそのn週間後のスケジュールの状況を取得します。
        /// </summary>
        /// <param name="weekago"></param>
        /// <returns></returns>
        public ScheduleState[][] GetScheduleStatus(int weekago = 0)
        {
            //返り値の配列の初期化
            ScheduleState[][] resultStatus=new ScheduleState[ApplicationData.Instance.TimeSpans.Count][];
            for (int i = 0; i < resultStatus.Length; i++)
            {
                resultStatus[i] = new ScheduleState[7];
            }
            for (int i = 0; i < 7; i++)
            {
                for (int j = 0; j < ApplicationData.Instance.TimeSpans.Count; j++)
                {
                    int classIndex = j + 1;
                    //classIndex時限目のi日後の予定を取得すればいい。
                    DateTime movedTime = DateTime.Now.AddDays(i + weekago*7).AsDayData();
                    //授業の存在チェック
                    ScheduleData scData = ApplicationData.Instance.GetSchedule((int) movedTime.DayOfWeek, classIndex);
                    if (scData == null||string.IsNullOrWhiteSpace(scData.TableName))
                    {
                        resultStatus[j][i] = ScheduleState.Empty;
                        continue;
                    }
                    else
                    {
                        //休講情報のチェック
                        NoClassSchedule noClassSchedule = ApplicationData.Instance.GetNoClassSchedule(movedTime,
                            new TableKey(classIndex, movedTime.DayOfWeek));
                        if (noClassSchedule != null)
                        {
                            resultStatus[j][i] = ScheduleState.NoClass;
                            continue;
                        }
                        //教室変更情報のチェック
                        ClassRoomChangeSchedule changeSc = ApplicationData.Instance.GetClassRoomChangeSchedule(
                            movedTime, new TableKey(classIndex, movedTime.DayOfWeek));
                        if (changeSc != null)
                        {
                            resultStatus[j][i] = ScheduleState.ChangeRoom;
                            continue;
                        }
                        resultStatus[j][i] = ScheduleState.Default;
                    }
                }
            }
            return resultStatus;
        }

        public IEnumerable<ScheduleTimingUnit> GetTodayKeyTiming(ScheduleTimingType timing,bool withFinishedTiming=false)
        {
            ScheduleState[][] ScheduleStatus = GetScheduleStatus();
            for (int index = 0; index < ApplicationData.Instance.TimeSpans.Count; index++)
            {
                var span = ApplicationData.Instance.TimeSpans[index];
                if (withFinishedTiming || span.FromTime - DateTime.Now.AsTimeData() > new TimeSpan())
                {
                    if (timing.HasFlag(ScheduleTimingType.BeginTimeWithNoClass))
                    {
                        yield return new ScheduleTimingUnit(SpanTimeType.BeginTime, index, span.FromTime);
                    }
                    else if (timing.HasFlag(ScheduleTimingType.BeginTime))
                    {
                        if (ScheduleStatus[index][0] != ScheduleState.Empty &&
                            ScheduleStatus[index][0] != ScheduleState.NoClass)
                            yield return new ScheduleTimingUnit(SpanTimeType.BeginTime, index, span.FromTime);
                    }
                }
                if (withFinishedTiming || span.ToTime - DateTime.Now.AsTimeData() > new TimeSpan())
                {
                    if (timing.HasFlag(ScheduleTimingType.EndTimeWithNoClass))
                    {
                        yield return new ScheduleTimingUnit(SpanTimeType.EndTime, index, span.ToTime);

                    }

                    else if (timing.HasFlag(ScheduleTimingType.EndTime))
                    {
                        if (ScheduleStatus[index][0] != ScheduleState.Empty &&
                            ScheduleStatus[index][0] != ScheduleState.NoClass)
                            yield return new ScheduleTimingUnit(SpanTimeType.EndTime, index, span.ToTime);
                    }
                }
            }
        }
    }

    public class ScheduleTimingUnit
    {
        public ScheduleTimingUnit(SpanTimeType timingType, int classNumber, DateTime time)
        {
            TimingType = timingType;
            ClassNumber = classNumber;
            Time = time;
        }

        public SpanTimeType TimingType { get; private set; }
        public int ClassNumber { get; private set; }

        public DateTime Time { get; private set; }


    }

    


    public enum ScheduleState
    {
        NoClass,Default,ChangeRoom,Empty
    }

    public enum SpanTimeType
    {
        BeginTime,EndTime
    }

    [Flags]
    public enum ScheduleTimingType
    {
        BeginTime=0x01,
        EndTime=0x02,
        BeginTimeWithNoClass=0x04,
        EndTimeWithNoClass=0x08
    }
    public static class ScheduleHelper
    {
        public static ScheduleTimeSpan GetTimeSpan(this ScheduleData data)
        {
            ScheduleKey key=ApplicationData.Instance.GetScheduleKey(data.ScheduleId);
            return ApplicationData.Instance.TimeSpans[key.TableNumber - 1];
        }

        public static int GetTimeSpanIndex(this ScheduleData data)
        {
            ScheduleKey key = ApplicationData.Instance.GetScheduleKey(data.ScheduleId);
            return key.TableNumber;
        }

        public static bool IsNoClass(this ScheduleState state)
        {
            return state == ScheduleState.Empty || state == ScheduleState.NoClass;
        }

        public static bool IsSpecialState(this ScheduleState state)
        {
            return state == ScheduleState.NoClass || state == ScheduleState.ChangeRoom;
        }

        public static string ActualPlaceInNextWeek(this TableKey tableKey)
        {
            var changeSchedule = ApplicationData.Instance.GetClassRoomChangeSchedule(
                DateTimeUtil.NextKeyDay(DateTime.Now, tableKey.dayOfWeek), tableKey);
            if (changeSchedule == null)
            {
                var d = ApplicationData.Instance.GetSchedule(tableKey.NumberOfDay, tableKey.TableNumber);
                if (d != null) return d.Place;
                else
                {
                    return "";
                }
            }
            else
            {
                return changeSchedule.ChangedTo;
            }
        }
    }
    /// <summary>
    ///     現在の時間帯が変わった際にコールされるイベントのイベント引数
    /// </summary>
    public class CurrentScheduleKeyChangedEventArgs : EventArgs
    {
        public CurrentScheduleKeyChangedEventArgs()
        {
        }

        public CurrentScheduleKeyChangedEventArgs(TableKey changedFrom, TableKey changedTo)
        {
            ChangedFrom = changedFrom;
            ChangedTo = changedTo;
        }

        /// <summary>
        ///     もともとのテーブルキー
        /// </summary>
        public TableKey ChangedFrom { get; private set; }

        /// <summary>
        ///     変移先のテーブルキー
        /// </summary>
        public TableKey ChangedTo { get; private set; }
    }
}
