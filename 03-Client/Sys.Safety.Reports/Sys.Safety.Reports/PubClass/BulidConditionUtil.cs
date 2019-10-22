using System;
using System.Collections.Generic;
using System.Text;

namespace Sys.Safety.Reports
{
    public class BulidConditionUtil
    {
        /// <summary>
        /// ��ȡ������
        /// </summary>
        /// <param name="fieldName">�ֶ���</param>
        /// <param name="fieldType">�ֶ�����</param>
        /// <param name="strCondition">������</param>
        /// <returns>string</returns>
        public static string GetConditionString(string fieldName, string fieldType, string strCondition)
        {
            string strReturn = "";

            try
            {
                string strFieldType = fieldType.ToLower();
                if (strFieldType == "varchar" || strFieldType == "nvarchar" || strFieldType == "nchar" || strFieldType == "char")
                {
                    strReturn = GetStringCondition(fieldName, strCondition);
                }
                else if (strFieldType == "money" || strFieldType == "decimal" || strFieldType == "float"
                    || strFieldType == "int" || strFieldType == "smallint" || strFieldType == "bigint")
                {
                    strReturn = GetNumberCondition(fieldName, strCondition);
                }
                else if (strFieldType == "bit")
                {
                    strReturn = GetBooleanCondition(fieldName, strCondition);
                }
                else if (strFieldType == "smalldatetime" || strFieldType == "datetime")
                {
                    strReturn = GetDateTimeCondition(fieldName, strCondition);
                }
                else
                {
                    strReturn = GetStringCondition(fieldName, strCondition);
                }


                if (strReturn != string.Empty)
                {
                    strReturn = " (" + strReturn + " )";
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return strReturn;
        }

        /// <summary>
        /// ��ȡ����������
        /// </summary>
        /// <param name="fieldName">�ֶ���</param>
        /// <param name="strCondition">������</param>
        /// <returns>string</returns>
        public static string GetRefCondition(string fieldName, string strCondition, string fieldType)
        {
            string strReturn = "";
            try
            {
                string str = string.Empty;
                string[] strConditions = { };
                strConditions = strCondition.Split(new string[] { "&&$$" }, StringSplitOptions.RemoveEmptyEntries);

                if (strConditions.Length > 0)
                {
                    string str1 = string.Empty;
                    for (int i = 0; i < strConditions.Length; i++)
                    {
                        if (!fieldType.Contains("int"))//�������int���ͣ�˵��û�н��������ϵ�����ʱ������ַ�����in��ѯ��ʱ����Ҫȥ������
                            str1 = strConditions[i].Substring(5);
                        else
                            str1 = strConditions[i].Substring(5).Replace("'", "");
                        if (i == 0)
                        {
                            str = str1;
                        }
                        else
                        {
                            str += "," + str1;
                        }
                    }
                }

                strReturn = fieldName + " in (" + str + ")";


                if (strReturn != string.Empty)
                {
                    strReturn = " (" + strReturn + " )";
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return strReturn;
        }

        /// <summary>
        /// ��ȡBoolean������
        /// </summary>
        /// <param name="fieldName">�ֶ���</param>
        /// <param name="strCondition">������</param>
        /// <returns>string</returns>
        public static string GetBooleanCondition(string fieldName, string strCondition)
        {
            string strReturn = "";
            try
            {
                string[] strConditions = { };
                strConditions = strCondition.Split(new string[] { "&&$$" }, StringSplitOptions.RemoveEmptyEntries);

                bool blnY = false;//������
                bool blnN = false;//���ڷ�
                if (strConditions.Length > 0)
                {
                    string str1 = string.Empty;
                    for (int i = 0; i < strConditions.Length; i++)
                    {
                        str1 = strConditions[i];
                        if (str1 == "��")
                        {
                            blnY = true;
                        }
                        else if (str1 == "��")
                        {
                            blnN = true;
                        }
                        else
                        {
                            blnY = true;
                            blnN = true;
                        }
                    }
                }

                if (blnY && blnN)
                {
                    strReturn = "";
                }
                else if (blnY)
                {
                    strReturn = fieldName + "=1";
                }
                else if (blnN)
                {
                    strReturn = fieldName + "=0";
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return strReturn;
        }

        /// <summary>
        /// ��ȡ����������
        /// </summary>
        /// <param name="fieldName">�ֶ���</param>
        /// <param name="strCondition">������</param>
        /// <returns>string</returns>
        public static string GetNumberCondition(string fieldName, string strCondition)
        {
            string strReturn = "";
            try
            {
                string[] strConditions = { };
                strConditions = strCondition.Split(new string[] { "&&$$" }, StringSplitOptions.RemoveEmptyEntries);

                string strTemp = string.Empty;
                if (strConditions.Length > 0)
                {
                    string str = string.Empty;
                    string str0 = string.Empty;
                    string str1 = string.Empty;
                    string str2 = string.Empty;
                    for (int i = 0; i < strConditions.Length; i++)
                    {
                        str = strConditions[i];
                        if (str.Contains("&&$"))
                        {
                            string[] strs = str.Split(new string[] { "&&$" }, StringSplitOptions.RemoveEmptyEntries);
                            str0 = strs[0];
                            if (strs.Length > 1) str1 = strs[1];
                            if (strs.Length > 2) str2 = strs[2];
                        }
                        else
                        {
                            str0 = str;
                        }

                        strTemp = string.Empty;
                        switch (str0)
                        {
                            case "��ֵ":
                                strTemp = " is null";
                                break;
                            case "����":
                                strTemp = "={0}";
                                break;
                            case "����":
                                strTemp = ">{0}";
                                break;
                            case "С��":
                                strTemp = "<{0}";
                                break;
                            case "������":
                                strTemp = "<>{0}";
                                break;
                            case "���ڵ���":
                                strTemp = ">={0}";
                                break;
                            case "С�ڵ���":
                                strTemp = "<={0}";
                                break;
                            case "����":
                                strTemp = " between {0} and {1}";
                                break;
                            default:
                                strTemp = "";
                                break;
                        }

                        string strFieldName = fieldName;
                        strFieldName = string.Format("isnull({0},0)", fieldName);
                        if (strTemp != string.Empty)
                        {
                            if (strReturn == string.Empty)
                            {
                                strReturn = strFieldName + string.Format(strTemp, str1, str2);
                            }
                            else
                            {
                                strReturn += " or " + strFieldName + string.Format(strTemp, str1, str2);
                            }
                        }
                    }

                    // 20170704
                    if (!string.IsNullOrEmpty(strReturn.Trim()))
                    {
                        strReturn = "(" + strReturn + ") and (" + fieldName + " REGEXP '(^[0-9]+.[0-9]+$)|(^[0-9]$)') =1";
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return strReturn;
        }

        /// <summary>
        /// ��ȡʱ��������
        /// </summary>
        /// <param name="fieldName">�ֶ���</param>
        /// <param name="strCondition">������</param>
        /// <returns>string</returns>
        public static string GetDateTimeCondition(string fieldName, string strCondition)
        {
            string strReturn = "";
            try
            {
                string[] strConditions = { };
                strConditions = strCondition.Split(new string[] { "&&$$" }, StringSplitOptions.RemoveEmptyEntries);
                string strTemp = string.Empty;
                if (strConditions.Length > 0)
                {
                    string str = string.Empty;
                    string str0 = string.Empty;
                    string str1 = string.Empty;
                    string str2 = string.Empty;
                    bool _blnDynamicPara = false;
                    for (int i = 0; i < strConditions.Length; i++)
                    {
                        str = strConditions[i];
                        if (str.Contains("&&$"))
                        {
                            string[] strs = str.Split(new string[] { "&&$" }, StringSplitOptions.RemoveEmptyEntries);
                            str0 = strs[0];
                            if (strs.Length > 1) str1 = strs[1];
                            if (strs.Length > 2) str2 = strs[2];
                        }
                        else
                        {
                            str0 = str;
                        }
                        //2014-12-11 ȡ������ǿ��ת��
                        //if (str1 != string.Empty && str1.Length > 10)
                        //{
                            //str1 = str1.Remove(10);
                        //}
                        //if (str2 != string.Empty && str2.Length > 10)
                        //{
                            //str2 = str2.Remove(10);
                        //}
                        strTemp = string.Empty;
                        _blnDynamicPara = false;
                        switch (str0)
                        {
                            case "��ֵ":
                                strTemp = " is null";
                                break;
                            case "����":
                                strTemp = "='{0}'";
                                break;
                            case "����":
                                strTemp = ">'{0}'";
                                break;
                            case "С��":
                                strTemp = "<'{0}'";
                                break;
                            case "������":
                                strTemp = "<>'{0}'";
                                break;
                            case "���ڵ���":
                                strTemp = ">='{0}'";
                                break;
                            case "С�ڵ���":
                                strTemp = "<='{0}'";
                                break;
                            case "����":
                                strTemp = " between '{0}' and '{1}'";
                                break;
                            case "��ʼ��":
                                strTemp = " like '{0}%'";
                                break;
                            case "����":
                                strTemp = " like '%{0}%'";
                                break;
                            case "����":
                                strTemp = "  ${Today} ";
                                _blnDynamicPara = true;
                                break;
                            case "����":
                                strTemp = "  ${ThisWeek} ";
                                _blnDynamicPara = true;
                                break;
                            case "����������":
                                strTemp = "  ${ThisWeekToToday} ";
                                _blnDynamicPara = true;
                                break;
                            case "����":
                                strTemp = "  ${ThisMonth} ";
                                _blnDynamicPara = true;
                                break;
                            case "����������":
                                strTemp = "  ${ThisMonthToToday} ";
                                _blnDynamicPara = true;
                                break;
                            case "������":
                                strTemp = "  ${ThisSeason} ";
                                _blnDynamicPara = true;
                                break;
                            case "������������":
                                strTemp = "  ${ThisSeasonToToday} ";
                                _blnDynamicPara = true;
                                break;
                            case "����":
                                strTemp = "  ${ThisYear} ";
                                _blnDynamicPara = true;
                                break;
                            case "����������":
                                strTemp = "  ${ThisYearToToday} ";
                                _blnDynamicPara = true;
                                break;
                            case "����":
                                strTemp = "  ${LastWeek} ";
                                _blnDynamicPara = true;
                                break;
                            case "����":
                                strTemp = "  ${LastMonth} ";
                                _blnDynamicPara = true;
                                break;
                            case "�ϼ���":
                                strTemp = "  ${LastSeason} ";
                                _blnDynamicPara = true;
                                break;
                            case "����":
                                strTemp = "  ${LastYear} ";
                                _blnDynamicPara = true;
                                break;
                            case "���һ��":
                                strTemp = "  ${LastestWeek} ";
                                _blnDynamicPara = true;
                                break;
                            case "���һ��":
                                strTemp = "  ${LastestMonth} ";
                                _blnDynamicPara = true;
                                break;
                            case "���һ����":
                                strTemp = "  ${LastestSeason} ";
                                _blnDynamicPara = true;
                                break;
                            case "���һ��":
                                strTemp = "  ${LastestYear} ";
                                _blnDynamicPara = true;
                                break;
                            default:
                                strTemp = "";
                                break;
                        }

                        string strFieldName = fieldName;
                        if (!_blnDynamicPara)
                        {
                            strTemp = string.Format(strTemp, str1, str2);
                            //2014-12-11 ȡ������ǿ��ת��
                            //strFieldName = string.Format("convert(varchar(10),isnull({0},'1900-01-01'),120)", fieldName);

                        }
                        else
                        {
                            strFieldName = string.Format("isnull({0},'1900-01-01 00:00:00')", fieldName);
                        }
                        if (strTemp != string.Empty)
                        {
                            if (strReturn == string.Empty)
                            {
                                strReturn = strFieldName + strTemp;
                            }
                            else
                            {
                                strReturn += " or " + strFieldName + strTemp;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return strReturn;
        }

        /// <summary>
        /// ��ȡ�ַ���������
        /// </summary>
        /// <param name="fieldName">�ֶ���</param>
        /// <param name="strCondition">������</param>
        /// <returns>string</returns>
        public static string GetStringCondition(string fieldName, string strCondition)
        {
            string strReturn = "";
            try
            {
                string[] strConditions = { };
                strConditions = strCondition.Split(new string[] { "&&$$" }, StringSplitOptions.RemoveEmptyEntries);

                string strTemp = string.Empty;
                if (strConditions.Length > 0)
                {
                    string str = string.Empty;
                    string str0 = string.Empty;
                    string str1 = string.Empty;
                    string str2 = string.Empty;
                    for (int i = 0; i < strConditions.Length; i++)
                    {
                        str = strConditions[i];
                        if (str.Contains("&&$"))
                        {
                            string[] strs = str.Split(new string[] { "&&$" }, StringSplitOptions.RemoveEmptyEntries);
                            str0 = strs[0];
                            if (strs.Length > 1) str1 = strs[1];
                            if (strs.Length > 2) str2 = strs[2];
                        }
                        else
                        {
                            str0 = str;
                        }

                        strTemp = string.Empty;
                        switch (str0)
                        {
                            case "��ֵ":
                                strTemp = " is null";
                                break;
                            case "����":
                                strTemp = "='{0}'";
                                break;
                            case "����":
                                strTemp = ">'{0}'";
                                break;
                            case "С��":
                                strTemp = "<'{0}'";
                                break;
                            case "������":
                                strTemp = "<>'{0}'";
                                break;
                            case "���ڵ���":
                                strTemp = ">='{0}'";
                                break;
                            case "С�ڵ���":
                                strTemp = "<='{0}'";
                                break;
                            case "����":
                                strTemp = " between '{0}' and '{1}'";
                                break;
                            case "��ʼ��":
                                strTemp = " like '{0}%'";
                                break;
                            case "����":
                                strTemp = " like '%{0}%'";
                                break;
                            case "������":
                                strTemp = " not like '%{0}%'";
                                break;
                            default:
                                strTemp = "";
                                break;
                        }

                        string strFieldName = fieldName;
                        strFieldName = string.Format("isnull({0},'')", fieldName);
                        if (strTemp != string.Empty)
                        {
                            if (strReturn == string.Empty)
                            {
                                strReturn = strFieldName + string.Format(strTemp, str1, str2);
                            }
                            else
                            {
                                strReturn += " or " + strFieldName + string.Format(strTemp, str1, str2);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return strReturn;
        }
    }
}
