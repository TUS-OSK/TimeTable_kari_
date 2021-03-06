﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeTableOne.Data;

namespace TimeTableOne.Common
{
    public static class WeekStringConverter
    {
        public static string[] EngDayOfWeek = {"Sun","Mon","Tue","Wed","Thu","Fri","Sat"};

        public static string[] JpnDayOfWeek = {"日","月","火","水","木","金","土"};

        public static string getAsString(DayOfWeek week)
        {
            return ApplicationData.Instance.Configuration.IsEnglishTablePageHeader
                ? EngDayOfWeek[(int) week]
                : JpnDayOfWeek[(int) week];
        }

        public static string getAsStringInJpn(DayOfWeek week)
        {
            return JpnDayOfWeek[(int) week];
        }
    }
}
