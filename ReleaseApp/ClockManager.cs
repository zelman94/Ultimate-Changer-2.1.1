﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace UltimateChanger
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SYSTEMTIME
    {
        public short wYear;
        public short wMonth;
        public short wDayOfWeek;
        public short wDay;
        public short wHour;
        public short wMinute;
        public short wSecond;
        public short wMilliseconds;
    }

    public class ClockManager
    {
        private SYSTEMTIME systemTime;
        private string time;
        private bool wasChanged;
       
        public ClockManager()
        {
            DateTime now = DateTime.Now;
            wasChanged = false;
            systemTime.wHour = (short)now.Hour;
            systemTime.wMinute = (short)now.Minute;
            systemTime.wMilliseconds = (short)now.Millisecond;
        }

        public void SetTime(short year, short month, short day)
        {
            systemTime.wYear = year;
            systemTime.wMonth = month;
            systemTime.wDay = day;

            if (!TimeZone.CurrentTimeZone.IsDaylightSavingTime(DateTime.Now))
            {
                systemTime.wHour--;
            }
            SetSystemTime(ref systemTime);
        }

        public void UpdateTime()
        {
            if (!wasChanged)
            {
                DateTime now = DateTime.Now;
                systemTime.wHour = (short)now.Hour;
                systemTime.wMinute = (short)now.Minute;
                systemTime.wMilliseconds = (short)now.Millisecond;
            }
        }

        public void DateWasChanged()
        {
            wasChanged = true;
        }

        public void DateWasSet()
        {
            wasChanged = false;
        }

        public void ResetTime()
        {
            // TO DO
            //systemTime.wYear = (short)DateTime.Now.Year;
            //systemTime.wMonth = (short)DateTime.Now.Month;
            //systemTime.wDay = (short)DateTime.Now.Day;
            //systemTime.wHour = (short)DateTime.Now.Hour;
            //systemTime.wMinute = (short)DateTime.Now.Minute;
            //systemTime.wSecond = (short)DateTime.Now.Second;

            SetSystemTime(ref systemTime);
        }

        public void HourUp()
        {
            if (systemTime.wHour == 23)
            {
                systemTime.wHour = 0;
            }
            else
            {
                systemTime.wHour++;
            }
        }
        public void HourDown()
        {
            if (systemTime.wHour == 0)
            {
                systemTime.wHour = 23;
            }
            else
            {
                systemTime.wHour--;
            }
        }
        public void MinuteUp()
        {
            if (systemTime.wMinute == 59)
            {
                systemTime.wMinute = 0;
            }
            else
            {
                systemTime.wMinute++;
            }
        }
        public void MinuteDown()
        {
            if (systemTime.wMinute == 0)
            {
                systemTime.wMinute = 59;
            }
            else
            {
                systemTime.wMinute--;
            }
        }
        public string GetTime()
        {
            string time;
            if (systemTime.wHour < 10)
            {
                time = $"0{systemTime.wHour}";
            }
            else
            {
                time = $"{systemTime.wHour}";
            }
            if (systemTime.wMinute < 10)
            {
                time = $"{time}:0{systemTime.wMinute}";
            }
            else
            {
                time = $"{time}:{systemTime.wMinute}";
            }
            return time;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetSystemTime(ref SYSTEMTIME systemTime);
    }
}
