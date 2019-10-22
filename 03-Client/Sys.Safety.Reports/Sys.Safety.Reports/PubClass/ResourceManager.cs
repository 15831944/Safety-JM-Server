using System;
using System.Collections.Generic;
using System.Text;
using System.Resources;
using System.IO;
using System.Drawing;


namespace Sys.Safety.Reports
{
    public class ResourceManager
    {
       

        private System.Resources.ResourceManager res;
        /// <summary>
        /// ���캯��
        /// </summary>
        /// <param name="type">��Դ�ļ�������</param>
        public ResourceManager(System.Type type)
        {
            if (type != null)
            {
                try
                {
                    string temp = type.Assembly.ManifestModule.Name;
                    if (temp.LastIndexOf(".") > 0)
                    {
                        temp = temp.Remove(temp.LastIndexOf("."));
                    }
                    res = new System.Resources.ResourceManager(temp + ".Properties.Resources", type.Assembly);
                }
                catch(Exception e)
                {
                   throw new Exception("δ�ҵ���Դ�ļ�"+type.Namespace+",����Ϊ"+e.ToString());
                }
            }
            else
            {
                throw new Exception("��������["+type+"]Ϊ��");
            }
        }
        /// <summary>
        /// ��ȡ���ʻ�����ִ�
        /// </summary>
        /// <param name="name">��Ҫ���ʻ����ִ�</param>
        /// <returns>���ʻ���Ľ��,���û�ҵ��ڵ�,���ؽڵ���</returns>
        public string GetString(string name)
        {
            return GetString(name,false);
        }
        /// <summary>
        /// ��ȡ���ʻ�����ִ�
        /// </summary>
        /// <param name="name">��Ҫ���ʻ����ִ�</param>
        /// <param name="isThrowException">û�ҵ��ڵ�ʱ�Ƿ��׳��쳣</param>
        /// <returns>���ʻ���Ľ��</returns>
        public string GetString(string name,bool isThrowException)
        {
            bool isHave = true;
            string result = name;
            if (this.res != null)
            {
                try
                {
                    result = res.GetString(name);
                    if (result == null)
                    {
                        isHave = false;
                    }
                }
                catch(Exception e)
                {
                    throw new Exception("δ�ҵ���Դ�ڵ�["+name+"]:"+e.ToString());
                    isHave = false;
                }
                if (!isHave && isThrowException)
                {
                    throw new Exception(string.Format("δ�ҵ���Դ�ڵ�[{0}]!", name));
                }
            }
            return result;
        }
        /// <summary>
        /// ��ȡ���ʻ�����ִ�
        /// </summary>
        /// <param name="name">��Ҫ���ʻ����ִ�</param>
        /// <param name="arg0">�ִ��е��滻����</param>
        /// <returns>���ʻ���Ľ��,���û�ҵ��ڵ�,���ؽڵ���</returns>
        public string GetString(string name, string arg0)
        {
            return GetString(name,arg0,false);
        }
        /// <summary>
        /// ��ȡ���ʻ�����ִ�
        /// </summary>
        /// <param name="name">��Ҫ���ʻ����ִ�</param>
        /// <param name="arg0">�ִ��е��滻����</param>
        /// <param name="isThrowException">û�ҵ��ڵ�ʱ�Ƿ��׳��쳣</param>
        /// <returns>���ʻ���Ľ��</returns>
        public string GetString(string name, string arg0, bool isThrowException)
        {
			string result = GetString(name);
			try
			{
				result = string.Format(result, arg0);
			}
            catch (Exception e)
            {
                if (isThrowException)
                {
                    throw new Exception(string.Format("δ�ҵ���Դ�ڵ�["+name+"]:"+e));
                }
            }
			return result;
        }
        /// <summary>
        /// ��ȡ���ʻ�����ִ�
        /// </summary>
        /// <param name="name">��Ҫ���ʻ����ִ�</param>
        /// <param name="arg0">�ִ��е��滻����0</param>
        /// <param name="arg1">�ִ��е��滻����1</param>
        /// <returns>���ʻ���Ľ��,���û�ҵ��ڵ�,���ؽڵ���</returns>
        public string GetString(string name, string arg0,string arg1)
        {
            return GetString(name, arg0, arg1, false);
        }
        /// <summary>
        /// ��ȡ���ʻ�����ִ�
        /// </summary>
        /// <param name="name">��Ҫ���ʻ����ִ�</param>
        /// <param name="arg0">�ִ��е��滻����0</param>
        /// <param name="arg1">�ִ��е��滻����1</param>
        /// <param name="isThrowException">û�ҵ��ڵ�ʱ�Ƿ��׳��쳣</param>
        /// <returns>���ʻ���Ľ��</returns>
        public string GetString(string name, string arg0, string arg1, bool isThrowException)
        {
			string result = GetString(name);
			try
			{
				result = string.Format(result, arg0, arg1);
			}
            catch (Exception e)
            {
                if (isThrowException)
                {
                    throw new Exception(string.Format("δ�ҵ���Դ�ڵ�[{0}]:{1}", name, e));
                }
            }
			return result;
        }
        /// <summary>
        /// ��ȡ���ʻ�����ִ�
        /// </summary>
        /// <param name="name">��Ҫ���ʻ����ִ�</param>
        /// <param name="arg0">�ִ��е��滻����0</param>
        /// <param name="arg1">�ִ��е��滻����1</param>
        /// <param name="arg2">�ִ��е��滻����2</param>
        /// <returns>���ʻ���Ľ��,���û�ҵ��ڵ�,���ؽڵ���</returns>
        public string GetString(string name, string arg0, string arg1, string arg2)
        {
            return GetString(name, arg0, arg1, arg2, false);
        }
        /// <summary>
        /// ��ȡ���ʻ�����ִ�
        /// </summary>
        /// <param name="name">��Ҫ���ʻ����ִ�</param>
        /// <param name="arg0">�ִ��е��滻����0</param>
        /// <param name="arg1">�ִ��е��滻����1</param>
        /// <param name="arg2">�ִ��е��滻����2</param>
        /// <param name="isThrowException">û�ҵ��ڵ�ʱ�Ƿ��׳��쳣</param>
        /// <returns>���ʻ���Ľ��</returns>
        public string GetString(string name, string arg0, string arg1, string arg2, bool isThrowException)
        {
			string result = GetString(name);
			try
			{
				result = string.Format(result, arg0, arg1, arg2);
			}
            catch (Exception e)
            {
                if (isThrowException)
                {
                    throw new Exception(string.Format("δ�ҵ���Դ�ڵ�[{0}]:{1}", name, e));
                }
            }
			return result;
        }
        /// <summary>
        /// ��ȡ���ʻ�����ִ�
        /// </summary>
        /// <param name="name">��Ҫ���ʻ����ִ�</param>
        /// <param name="args">�ִ��е��滻����</param>
        /// <returns>���ʻ���Ľ��,���û�ҵ��ڵ�,���ؽڵ���</returns>
        public string GetString(string name, object[] args)
        {
            return GetString(name, args, false);
        }
        /// <summary>
        /// ��ȡ���ʻ�����ִ�
        /// </summary>
        /// <param name="name">��Ҫ���ʻ����ִ�</param>
        /// <param name="args">�ִ��е��滻����</param>
        /// <param name="isThrowException">û�ҵ��ڵ�ʱ�Ƿ��׳��쳣</param>
        /// <returns>���ʻ���Ľ��</returns>
        public string GetString(string name, object[] args, bool isThrowException)
        {
			string result = GetString(name);
			try
			{
				result = string.Format(result, args);
			}
            catch (Exception e)
            {
                if (isThrowException)
                {
                    throw new Exception(string.Format("δ�ҵ���Դ�ڵ�[{0}]:{1}", name, e));
                }
            }
            return result;
        }
        /// <summary>
        /// ��ȡͼƬ��Դ
        /// </summary>
        /// <param name="name">��Դ��</param>
        /// <returns>��Դ,���û�ҵ�,�򷵻ؿ�</returns>
        public Bitmap GetBitmap(string name)
        {
            return GetBitmap(name, false);
        }
        /// <summary>
        /// ��ȡͼƬ��Դ
        /// </summary>
        /// <param name="name">��Դ��</param>
        /// <param name="isThrowException">û�ҵ��ڵ�ʱ�Ƿ��׳��쳣</param>
        /// <returns>��Դ</returns>
        public Bitmap GetBitmap(string name, bool isThrowException)
        {
            Bitmap result = null;
            try
            {
                result = (System.Drawing.Bitmap)res.GetObject(name);
            }
            catch (Exception e)
            {
                if (isThrowException)
                {
                    throw new Exception(string.Format("δ�ҵ���Դ�ڵ�[{0}]:{1}", name, e));
                }
            }
            return result;
        }
    }
}
