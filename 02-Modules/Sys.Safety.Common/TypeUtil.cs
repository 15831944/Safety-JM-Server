using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Drawing;
using System.IO;

namespace Sys.Safety.Reports
{
    public class TypeUtil
    {
        private TypeUtil()
        {

        }
        public static string ToString(object objValue)
        {
            try
            {
                return System.Convert.ToString(objValue);
            }
            catch
            {
                return "";
            }
        }
        public static double ToDouble(object objValue)
        {
            try
            {
                return System.Convert.ToDouble(objValue);
            }
            catch
            {
                return 0;
            }
        }
        public static decimal ToDecimal(object objValue)
        {
            try
            {
                return System.Convert.ToDecimal(objValue);
            }
            catch
            {
                return 0;
            }
        }
        public static decimal ToAmount(object objValue)
        {
            decimal result = ToDecimal(objValue);
            return Math.Round(result, 2);
        }
        public static int ToInt(object objValue)
        {
            try
            {
                return System.Convert.ToInt32(objValue);
            }
            catch
            {
                return 0;
            }
        }
        public static byte ToByte(object objValue)
        {
            try
            {
                return System.Convert.ToByte(objValue);
            }
            catch
            {
                return 0;
            }
        }
        public static DateTime ToDateTime(object objValue)
        {
            try
            {
                if (DateTime.Parse(objValue.ToString()) < DateTime.Parse("1900-1-1"))
                    //return DateTime.Now;
                    return DateTime.Parse("1900-1-1");
                else
                    return DateTime.Parse(objValue.ToString());
            }
            catch
            {
                //return DateTime.Now;
                return DateTime.Parse("1900-1-1");
            }
        }
        public static bool ToBool(object objValue)
        {
            try
            {
                return Convert.ToBoolean(objValue);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// ת�����Ϊ����Ҵ�д
        /// </summary>
        /// <param name="n">Int64 ���</param>
        /// <returns>����Ҵ�д</returns>        
        public static string ToRMB(double dn)
        {
            string[] strN = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
            string[] strC = { "��", "Ҽ", "��", "��", "��", "��", "½", "��", "��", "��" };
            string[] strA = { "", "Բ", "ʰ", "��", "Ǫ", "��", "ʰ", "��", "Ǫ", "��", "ʰ", "��", "Ǫ", "����", "ʰ", "��", "Ǫ", "����", };
            int[] nLoc = { 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0 };

            string strFrom = "";
            string strTo = "";
            string strChar;
            int m, mLast = -1, nCount = 0;

            if (strFrom.Length > strA.GetUpperBound(0) - 1) return "***���У���ô��Ǯ����Ҫ����***";

            if (dn < 0)
            {
                dn *= -1;
                strTo = "��";
            }

            Int64 n1 = (Int64)dn;                   // Ԫ
            strFrom = n1.ToString();

            for (int i = strFrom.Length; i > 0; i--)
            {
                strChar = strFrom.Substring(strFrom.Length - i, 1);
                m = Convert.ToInt32(strChar);
                if (m == 0)
                {
                    // ����Ϊ��ʱ��Ҫ�����м䵥λ,��ֻ����һ��
                    if (nLoc[i] > 0 && nCount == 0 && strFrom.Length > 1)
                    {
                        strTo = strTo + strA[i];
                        nCount++;
                    }
                }
                else
                {
                    // ����
                    if (mLast == 0)
                    {
                        strTo = strTo + strC[0];
                    }

                    // ����ת��Ϊ��д
                    strTo = strTo + strC[m];
                    // ���㵥λ
                    strTo = strTo + strA[i];
                    nCount = 0;
                }
                mLast = m;
            }

            Int64 n2 = ((Int64)(dn * 100)) % 100;   // �Ƿ�
            Int64 n3 = n2 / 10;                     // ��
            Int64 n4 = n2 % 10;                     // ��
            string s2 = "";

            if (n4 > 0)
            {
                s2 = strC[n4] + "��";
                if (n3 > 0)
                {
                    s2 = strC[n3] + "��" + s2;
                }
            }
            else
            {
                if (n3 > 0)
                {
                    s2 = strC[n3] + "��";
                }
            }
            strTo = strTo + s2;

            if (strTo == "") strTo = strC[0];                   // ȫ0��ʾΪ��
            else if (s2 == "") strTo = strTo + "��";            // �޽Ƿ���ʾ��
            return strTo;
        }

        /// <summary>
        /// �Ƿ�����������
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool isNumericType(string type)
        {
            switch (type)
            {
                case "Decimal":
                case "Double":
                case "Int16":
                case "Int32":
                case "Int64":
                case "Single":
                case "UInt16":
                case "UInt32":
                case "UInt64":
                    return true;
                case "Boolean":
                case "Byte":
                case "Char":
                case "DateTime":
                case "SByte":
                case "String":
                case "TimeSpan":
                    return false;
            }
            return false;
        }
        /// <summary>
        /// ת��Ϊָ�����͵�����
        /// </summary>
        /// <param name="objValue"></param>
        /// <param name="toType"></param>
        /// <returns></returns>
        public static object ToRefType(object objValue, System.Type toType)
        {
            if (toType.Equals(typeof(bool)))
            {
                return ToBool(objValue);
            }
            else if (toType.Equals(typeof(DateTime)))
            {
                return ToDateTime(objValue);
            }
            else if (toType.Equals(typeof(int)))
            {
                return ToInt(objValue);
            }
            else if (toType.Equals(typeof(byte)))
            {
                return ToByte(objValue);
            }
            else if (toType.Equals(typeof(decimal)))
            {
                return ToDecimal(objValue);
            }
            else
            {
                return ToString(objValue);
            }
        }
        ///// <summary>
        ///// ��ֵ�����б�ת��ΪDataTable
        ///// ���vosΪ��,�򷵻ؿ�
        ///// </summary>
        ///// <param name="voList"></param>
        ///// <returns></returns>
        //public static DataTable ToDataTable<T>(IList<T> vos)
        //{
        //    if (vos == null || vos.Count == 0)
        //    {
        //        return null;
        //    }
        //    Type voType = vos[0].GetType();
        //    DataTable dt = new DataTable(voType.Name);
        //    for (int i=0; i < vos.Count; i++)
        //    {
        //        voType = vos[i].GetType();          //�������ݱ�                
        //        PropertyInfo[] properties = voType.GetProperties();
        //        IDictionary<string, PropertyInfo> voProperties = new Dictionary<string, PropertyInfo>();
        //        //����������
        //        foreach (PropertyInfo property in properties)
        //        {
        //            if (!IsDuplicateField(dt,property .Name ))
        //            {

        //                DataColumn col = new DataColumn(property.Name);
        //                col.DataType = property.PropertyType;
        //                col.Caption = property.Name;
        //                dt.Columns.Add(col);
        //                voProperties.Add(property.Name, property);
        //            }
        //        }
        //        //��ȡ��¼����
        //        foreach (object obj in vos)
        //        {
        //            if (!IsDuplicateField(dt,obj.ToString ()))
        //            {
        //                DataRow dr = dt.NewRow();
        //                foreach (PropertyInfo pro in voProperties.Values)
        //                {
        //                    dr[pro.Name] = pro.GetValue(obj, null);
        //                }
        //                dt.Rows.Add(dr);
        //            }
        //        }
        //    }
        //    return dt;
        //}

        private static bool IsDuplicateField(DataTable dt, string fieldName)
        {
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                if (dt.Columns[i].ToString() == fieldName)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// ��ֵ�����б�ת��ΪDataTable
        /// ���vosΪ��,�򷵻ؿ�
        /// </summary>
        /// <param name="voList"></param>
        /// <returns></returns>
        public static DataTable ToDataTable<T>(IList<T> vos)
        {
            Type voType = typeof(T);
            //�������ݱ�
            DataTable dt = new DataTable(voType.Name);
            PropertyInfo[] properties = voType.GetProperties();
            IDictionary<string, PropertyInfo> voProperties = new Dictionary<string, PropertyInfo>();
            //����������
            foreach (PropertyInfo property in properties)
            {
                DataColumn col = new DataColumn(property.Name);
                col.DataType = property.PropertyType;
                col.Caption = property.Name;
                dt.Columns.Add(col);
                voProperties.Add(property.Name, property);
            }
            if (vos == null || vos.Count == 0)
            {
                return dt;
            }
            //��ȡ��¼����
            foreach (object obj in vos)
            {
                DataRow dr = dt.NewRow();
                foreach (PropertyInfo pro in voProperties.Values)
                {
                    dr[pro.Name] = pro.GetValue(obj, null);
                }
                dt.Rows.Add(dr);
            }
            return dt;
        }
        /// <summary>
        /// ��IListת��ΪDataTable
        /// </summary>
        /// <param name="vos"></param>
        /// <returns></returns>
        public static DataTable ToDataTable(IList vos)
        {
            if (vos == null || vos.Count == 0)
            {
                return null;
            }
            IList[] vosMore = new IList[vos.Count]; ;
            int i = 0;
            try
            {
                foreach (IList v in vos)
                {
                    vosMore[i++] = v;
                }
                return ToDataTableFromMore(vosMore);

            }
            catch
            {
                return ToDataTableFromOne(vos);
            }

        }
        /// <summary>
        /// ����ʵ��ֵ�����б�ת��ΪDataTable
        /// ���vosΪ��,�򷵻ؿ�
        /// </summary>
        /// <param name="vos"></param>
        /// <returns></returns>
        private static DataTable ToDataTableFromOne(IList vos)
        {
            if (vos == null || vos.Count == 0)
            {
                return null;
            }
            Type voType = vos[0].GetType();
            //�������ݱ�
            DataTable dt = new DataTable(voType.Name);
            PropertyInfo[] properties = voType.GetProperties();
            IDictionary<string, PropertyInfo> voProperties = new Dictionary<string, PropertyInfo>();
            //����������
            foreach (PropertyInfo property in properties)
            {
                DataColumn col = new DataColumn(property.Name);
                col.DataType = property.PropertyType;
                col.Caption = property.Name;
                dt.Columns.Add(col);
                voProperties.Add(property.Name, property);
            }
            //��ȡ��¼����
            foreach (object obj in vos)
            {
                DataRow dr = dt.NewRow();
                foreach (PropertyInfo pro in voProperties.Values)
                {
                    dr[pro.Name] = pro.GetValue(obj, null);
                }
                dt.Rows.Add(dr);
            }
            return dt;
        }
        /// <summary>
        /// ����ʵ��ֵ�����б�ת��ΪDataTable
        /// ���vosΪ��,�򷵻ؿ�
        /// </summary>
        /// <param name="vos"></param>
        /// <returns></returns>
        private static DataTable ToDataTableFromMore(IList[] vos)
        {
            if (vos == null)
            {
                return null;
            }
            Type voType = vos[0].GetType();
            DataTable dt = new DataTable(voType.Name);
            IDictionary<string, PropertyInfo> voProperties = new Dictionary<string, PropertyInfo>();

            for (int i = 0; i < vos[0].Count; i++)
            {
                voType = vos[0][i].GetType();

                //�������ݱ�                
                PropertyInfo[] properties = voType.GetProperties();
                //����������
                foreach (PropertyInfo property in properties)
                {
                    if (!IsDuplicateField(dt, property.Name))
                    {
                        DataColumn col = new DataColumn(property.Name);
                        col.DataType = property.PropertyType;
                        col.Caption = property.Name;
                        dt.Columns.Add(col);
                        voProperties.Add(property.Name, property);
                    }
                }
            }
            //��ȡ��¼����
            foreach (object[] vosRecord in vos)
            {
                DataRow dr = dt.NewRow();
                // ���
                foreach (object voOne in vosRecord)
                {
                    voType = voOne.GetType();
                    PropertyInfo[] properties = voType.GetProperties();
                    // ��������
                    foreach (PropertyInfo property in properties)
                    {
                        dr[property.Name] = property.GetValue(voOne, null);
                    }
                }
                dt.Rows.Add(dr);
            }
            return dt;
        }

        public static byte[] GetThumbnailBtyeArr(Image originalImage, int width, int height, string mode)
        {
            Image bitmap = null;
            Graphics g = null;
            byte[] buffer = new byte[0];
            try
            {
                int towidth = width;
                int toheight = height;
                int x = 0;
                int y = 0;
                int ow = originalImage.Width;
                int oh = originalImage.Height;
                switch (mode)
                {
                    case "HW "://ָ���߿����ţ����ܱ��Σ�   
                        break;
                    case "W "://ָ�����߰�����   
                        toheight = originalImage.Height * width / originalImage.Width;
                        break;
                    case "H "://ָ���ߣ������� 
                        towidth = originalImage.Width * height / originalImage.Height;
                        break;
                    case "Cut "://ָ���߿�ü��������Σ�   
                        if ((double)originalImage.Width / (double)originalImage.Height > (double)towidth / (double)toheight)
                        {
                            oh = originalImage.Height;
                            ow = originalImage.Height * towidth / toheight;
                            y = 0;
                            x = (originalImage.Width - ow) / 2;
                        }
                        else
                        {
                            ow = originalImage.Width;
                            oh = originalImage.Width * height / towidth;
                            x = 0;
                            y = (originalImage.Height - oh) / 2;
                        }
                        break;
                    default:
                        break;
                }

                bitmap = new System.Drawing.Bitmap(towidth, toheight);//�½�һ��bmpͼƬ 
                g = System.Drawing.Graphics.FromImage(bitmap);//�½�һ������ 

                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;//���ø�������ֵ�� 
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;   //���ø�����,���ٶȳ���ƽ���̶� 
                g.Clear(Color.Transparent);   //��ջ�������͸������ɫ��� 
                g.DrawImage(originalImage, new Rectangle(0, 0, towidth, toheight), new Rectangle(x, y, ow, oh), GraphicsUnit.Pixel);//��ָ��λ�ò��Ұ�ָ����С����ԭͼƬ��ָ������  

                Stream stream = new MemoryStream();
                bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Gif);
                if (stream.Length > 0)
                {
                    buffer = new byte[stream.Length];
                    int len = (int)stream.Length;
                    stream.Seek(0, SeekOrigin.Begin);
                    int readLen = stream.Read(buffer, 0, len);
                }
            }
            catch (System.Exception e)
            {
                throw e;
            }
            finally
            {
                originalImage.Dispose();
                bitmap.Dispose();
                g.Dispose();
            }

            return buffer;
        }

        /// <summary>
        /// ��鼶�α���Ϸ���
        /// ������ΪA-Z,a-z,0-9���»���( _ )��λ�úͳ��Ȳ���
        /// ����Ϊ���α���ķָ��� 
        /// </summary>
        /// <param name="strText"></param>
        /// <returns></returns>
        public static bool IsValidLevelCode(string strText)
        {
            Regex regex = new Regex(@"^([A-Za-z0-9_]+([-]{1}[A-Za-z0-9_]+)*)$");
            // Regex regex = new Regex(@"^([\w]+([-]{1}\w+)*)$");
            return regex.IsMatch(strText);
        }

        /// <summary>
        /// ���Ǽ��α���Ϸ���
        /// ������ΪA-Z,a-z,0-9���»���( _ )��λ�úͳ��Ȳ���
        /// </summary>
        /// <param name="strText"></param>
        /// <returns></returns>
        public static bool IsValidCode(string strText)
        {
            Regex regex = new Regex(@"^([A-Za-z0-9_]+)$");
            // Regex regex = new Regex(@"^([\w]+)$");
            return regex.IsMatch(strText);
        }


        /// <summary>
        /// �Ƴ���������ͬ����
        /// </summary>
        /// <param name="list"></param>
        /// <returns>List</returns>
        public static List<int> RemoveSameItem(List<int> list)
        {
            int ItemValue = 0;
            for (int j = 0; j < list.Count; j++)
            {
                ItemValue = list[j];

                for (int m = j + 1; m < list.Count; m++)
                {
                    if (ItemValue == list[m])
                    {
                        list.Remove(list[m]);
                        j = -1; break;
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// ���л�
        /// </summary>
        /// <param name="sPath"></param>
        public static void SerializeSys(object obj, string sPath)
        {
            System.Xml.Serialization.XmlSerializerNamespaces ns = new System.Xml.Serialization.XmlSerializerNamespaces();
            ns.Add("", "");
            System.Xml.Serialization.XmlSerializer xmlSerialization = new System.Xml.Serialization.XmlSerializer(obj.GetType());
            using (Stream stream = new FileStream(sPath, FileMode.Create, FileAccess.Write))
            {
                xmlSerialization.Serialize(stream, obj);
            }
        }
        /// <summary>
        /// �����л�
        /// </summary>
        /// <param name="sPath"></param>
        public static object DeSerializeSys(object obj, string sPath)
        {
            //System.Xml.Serialization.XmlSerializer xmlSerialization = new System.Xml.Serialization.XmlSerializer(typeof(Protocol));
            System.Xml.Serialization.XmlSerializer xmlSerialization = new System.Xml.Serialization.XmlSerializer(obj.GetType());
            if (File.Exists(sPath))
            {
                //���������ļ�
                //DecryptConfigFile();

                Stream s = new FileStream(sPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                obj = xmlSerialization.Deserialize(s);
                //c = xmlSerialization.Deserialize(s) as Config;
                s.Close();

                //EncryptConfigFile();
            }
            return obj;
        }


        /// <summary>
        /// ��DataTable��������һ�������,��Ҫ���ڱ����ӡ��ʱ����Ҫ��ʾ�����
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static DataTable AddNumberToDataTable(DataTable dt)
        {
            DataTable dtCopty = dt.Copy();
            if (dtCopty.Columns.Contains("colNumber"))
                dtCopty.Columns.Remove("colNumber");
            DataColumn dc = new DataColumn();
            dc.ColumnName = "colNumber";
            dc.DataType = typeof(int);
            dtCopty.Columns.Add(dc);

            for (int i = 0; i < dtCopty.Rows.Count; i++)
            {
                dtCopty.Rows[i]["colNumber"] = i + 1;
            }

            return dtCopty;
        }


        /// <summary>
        /// DataTableȥ���ظ�������
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static DataTable DeleteRepeateRow(DataTable dt)
        {
            DataTable dtCopty = dt.Copy();
            ArrayList arraylist = new ArrayList();
            foreach (DataColumn column in dtCopty.Columns)
            {
                string strColumnName = TypeUtil.ToString(column.ColumnName);
                arraylist.Add(strColumnName);
            }
            string[] arrString = (string[])arraylist.ToArray(typeof(string));
            DataView dv = new DataView(dtCopty);//������ͼ
            DataTable dtReturn = dv.ToTable(true, arrString);

            //���´���ֻ���ģ���������쳣�հ౨����Ч(��ΪҪ�ų�һ������е������жϵ�������Щ����û�жϵ�������У����һ�����ֻ��һ�����������޶ϵ�������ɾ��)
            DataRow[] rows = dtReturn.Select("ViewMLLDDDayReport1_wz = '�޶ϵ�����'");
            foreach (DataRow row in rows)
            {
                string strpoint = TypeUtil.ToString(row["ViewMLLDDDayReport1_point"]);
                DataRow[] rowa = dtReturn.Select("ViewMLLDDDayReport1_wz <> '�޶ϵ�����' and ViewMLLDDDayReport1_point='" + strpoint + "'");
                if (rowa.Length > 0)
                {
                    row.Delete();
                }
            }


            foreach (DataRow row in dtReturn.Rows)
            {
                string strwz = "ViewMLLDDDayReport1_wz";
            }


            return dtReturn;




        }



        /// <summary>
        /// �õ�һ���ַ�������һ���ַ��ϴ����ֵ�����λ������
        /// </summary>
        /// <param name="str"></param>
        /// <param name="substr"></param>
        /// <param name="StartPos"></param>
        /// <returns></returns>
        public  static int[] GetSubStrCountInStr(String str, String substr, int StartPos)
        {
            int foundPos = -1;
            int count = 0;
            List<int> foundItems = new List<int>();
            do
            {
                foundPos = str.IndexOf(substr, StartPos);
                if (foundPos > -1)
                {
                    StartPos = foundPos + 1;
                    count++;
                    foundItems.Add(foundPos);
                }
            } while (foundPos > -1 && StartPos < str.Length);

            return ((int[])foundItems.ToArray());
        }

    }
}
