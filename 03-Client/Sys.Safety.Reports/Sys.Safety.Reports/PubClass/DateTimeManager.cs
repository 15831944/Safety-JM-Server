using System;
using System.Collections.Generic;
using System.Text;

namespace Sys.Safety.Reports
{
    public class DateTimeManager
    {
        /// <summary>
        /// ����
        /// </summary>
        /// <returns></returns>
        public static string GetToday()
        {          
            return GetDateTimeString(DateTime.Now.Date, DateTime.Now.Date);
        }
        /// <summary>
        /// ����
        /// </summary>
        /// <returns></returns>
        public static string GetThisWeek()
        {
            return GetDateTimeString(DateTimeUtil.GetWeekFristDay(DateTime.Now.Date), DateTimeUtil.GetWeekEndDay(DateTime.Now.Date));
        }
        /// <summary>
        /// ����
        /// </summary>
        /// <returns></returns>
        public static string GetThisMonth()
        {
            return GetDateTimeString(DateTimeUtil.GetMonthFristDay(DateTime.Now.Date), DateTimeUtil.GetMonthEndDay(DateTime.Now.Date));
        }
        /// <summary>
        /// ������
        /// </summary>
        /// <returns></returns>
        public static string GetThisSeason()
        {
            return GetDateTimeString(DateTimeUtil.GetQuarterFristDay(DateTime.Now.Date), DateTimeUtil.GetQuarterEndDay(DateTime.Now.Date));
        }
        /// <summary>
        /// ����
        /// </summary>
        /// <returns></returns>
        public static string GetThisYear()
        {
            return GetDateTimeString(DateTimeUtil.GetYearFristDay(DateTime.Now.Date), DateTimeUtil.GetYearEndDay(DateTime.Now.Date));
        }
        /// <summary>
        /// ��������
        /// </summary>
        /// <returns></returns>
        public static string GetThisWeekToToday()
        {
            return GetDateTimeString(DateTimeUtil.GetWeekFristDay (DateTime.Now.Date), DateTime.Now .Date );
        }   
        /// <summary>
        /// ��������
        /// </summary>
        /// <returns></returns>
        public static string GetThisMonthToToday()
        {
            return GetDateTimeString(DateTimeUtil.GetMonthFristDay (DateTime.Now.Date), DateTime.Now.Date);
        }
        /// <summary>
        /// ����������
        /// </summary>
        /// <returns></returns>
        public static string GetThisSeasonToToday()
        {
            return GetDateTimeString(DateTimeUtil.GetQuarterFristDay (DateTime.Now.Date), DateTime.Now.Date);
        }
        /// <summary>
        /// ���굽��
        /// </summary>
        /// <returns></returns>
        public static string GetThisYearToToday()
        {
            return GetDateTimeString(DateTimeUtil.GetYearFristDay(DateTime.Now.Date), DateTime.Now.Date);
        }
        /// <summary>
        /// ����
        /// </summary>
        /// <returns></returns>
        public static string GetLastWeek()
        {
            return GetDateTimeString(DateTimeUtil.GetLastWeekFristDay (DateTime.Now.Date), DateTimeUtil.GetLastWeekEndDay (DateTime.Now.Date));
        }
        /// <summary>
        /// ����
        /// </summary>
        /// <returns></returns>
        public static string GetLastMonth()
        {
            return GetDateTimeString(DateTimeUtil.GetLastWeekFristDay (DateTime.Now.Date), DateTimeUtil.GetLastWeekEndDay (DateTime.Now.Date));
        }
        /// <summary>
        /// �ϼ���
        /// </summary>
        /// <returns></returns>
        public static string GetLastSeason()
        {
            return GetDateTimeString(DateTimeUtil.GetLastQuarterFristDay (DateTime.Now.Date), DateTimeUtil.GetLastQuarterEndDay (DateTime.Now.Date));
        }
        

        /// <summary>
        /// ����
        /// </summary>
        /// <returns></returns>
        public static string GetLastYear()
        {
           
            DateTime datetime = DateTime.Now.AddYears(-1);
            string str = "between '";
            str += FormatDateTime(datetime).Substring(0, 4) + "-01-01 00:00:00'";
            str += " and '" + FormatDateTime(datetime).Substring(0, 4) + "-12-31 23:59:59'";

            return str;
        }

        /// <summary>
        /// ���һ��
        /// </summary>
        /// <returns></returns>
        public static string GetLastestWeek()
        {
            DateTime datetime = DateTime.Now.AddDays(-6);
            string str = "between '";
            str += FormatDateTime(datetime) + " 00:00:00'";
            str += " and '" + FormatDateTime(DateTime.Now) + " 23:59:59'";

            return str;
        }

        /// <summary>
        /// ���һ��
        /// </summary>
        /// <returns></returns>
        public static string GetLastestMonth()
        {
            DateTime datetime = DateTime.Now.AddMonths(-1).AddDays(1);
            string str = "between '";
            str += FormatDateTime(datetime) + " 00:00:00'";
            str += " and '" + FormatDateTime(DateTime.Now) + " 23:59:59'";

            return str;
        }

        /// <summary>
        /// ���һ����
        /// </summary>
        /// <returns></returns>
        public static string GetLastestSeason()
        {
            DateTime datetime = DateTime.Now.AddMonths(-3).AddDays(1);
            string str = "between '";
            str += FormatDateTime(datetime) + " 00:00:00'";
            str += " and '" + FormatDateTime(DateTime.Now) + " 23:59:59'";

            return str;
        }

        /// <summary>
        /// ���һ��
        /// </summary>
        /// <returns></returns>
        public static string GetLastestYear()
        {
            DateTime datetime = DateTime.Now.AddYears(-1).AddDays(1);
            string str = "between '";
            str += FormatDateTime(datetime) + " 00:00:00'";
            str += " and '" + FormatDateTime(DateTime.Now) + " 23:59:59'";

            return str;
        }



        /// <summary>
        /// �������ڻ�ȡ��ѯSQL���ַ���
        /// </summary>
        /// <param name="datFrist">��ʼ����</param>
        /// <param name="datLast">��������</param>
        /// <returns>��ѯ���ڵ��ַ���</returns>
        private static string GetDateTimeString(DateTime datFrist,DateTime datLast)
        {
            string strDateTime = " between '";
            strDateTime += FormatDateTime(datFrist).Substring (0,10) + " 00:00' and '";
            strDateTime += FormatDateTime(datLast).Substring(0, 10) + " 23:59:59'";
            return strDateTime;

            //string strDateTime = " between '";
            //strDateTime +=FormatDateTime(datFrist.Date) + "' and '";
            //strDateTime += datLast .Date .ToShortDateString () + " 23:59:59 '";
            //return strDateTime;
        }

        /// <summary>
        /// ��ʽ������
        /// </summary>
        /// <param name="dateTime">Date����</param>
        /// <returns>���ص��ַ�����ʽΪ ��-��-�� ��(  2007-01-12  )</returns>
        private static string FormatDateTime(DateTime dateTime)
        {
            string strDate = string.Format("{0:yyyy/MM/dd}", dateTime);
            return strDate;
        }

    }
}
