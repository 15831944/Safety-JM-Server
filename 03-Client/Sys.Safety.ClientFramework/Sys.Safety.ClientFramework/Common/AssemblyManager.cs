using Basic.Framework.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Sys.Safety.ClientFramework.CBFCommon
{
    /// <summary>
    /// .net ���������
    /// </summary>
    public class AssemblyManager
    {
        ///// <summary>
        ///// ����ʵ�����Ķ��󣬽���ڴ�й©����  20171013
        ///// </summary>
        //static Dictionary<string, object> assemblyCreateInstanceList = new Dictionary<string, object>();
        ///// <summary>
        ///// ����ʵ�����Ķ��󣬽���ڴ�й©����  20171013
        ///// </summary>
        //static Dictionary<string, Assembly> assemblyCreateList = new Dictionary<string, Assembly>();
        private AssemblyManager()
        {
        }
        /// <summary>
        /// ͨ��������ƴ�������ʵ��
        /// </summary>
        /// <param name="assemblyName">������</param>
        /// <param name="typeName">����</param>
        /// <param name="args">����</param>
        /// <returns>����ʵ��</returns>
        public static object CreateInstance(string assemblyName, string typeName, object[] args, ref bool isReload)
        {
            object returnObject = null;
            System.Reflection.Assembly assemblyCreate = null;


            //if (assemblyCreateInstanceList.ContainsKey(typeName))
            //{
            //    returnObject = assemblyCreateInstanceList[typeName];

            //    assemblyCreate = assemblyCreateList[typeName];

            //    try
            //    {
            //        //����Ѿ������˵�ǰ���壬��ֱ�ӵ��ô������¼��ط������������¼���  20171016
            //        var type = assemblyCreate.GetType(typeName);
            //        var method = type.GetMethod("Reload");
            //        if (args == null)
            //        {
            //            args = new object[1] { "" };
            //        }
            //        method.Invoke(returnObject, args);
            //        isReload = true;
            //    }
            //    catch (Exception ex)
            //    {
            //        Basic.Framework.Logging.LogHelper.Error(ex);
            //    }

            //    return returnObject;//����ѷ��䣬��ֱ�ӷ��ؿ�
            //}
            //else
            //{
            //    isReload = false;
            //}


            try
            {
                if (assemblyCreate == null)
                {
                    assemblyCreate = Assembly.LoadFrom(assemblyName);
                    //assemblyCreateList.Add(typeName, assemblyCreate);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }

            if (returnObject == null)
            {
                if (args != null)
                    returnObject = assemblyCreate.CreateInstance(typeName, true, BindingFlags.CreateInstance, null, args, null, null);
                else
                    returnObject = assemblyCreate.CreateInstance(typeName, true);
                //assemblyCreateInstanceList.Add(typeName, returnObject);
            }
            return returnObject;
        }
        /// <summary>
        /// ͨ��������ƴ�������ʵ�������ö���ķ���
        /// </summary>
        /// <param name="assemblyName">������</param>
        /// <param name="typeName">����</param>
        /// <param name="args">����</param>
        /// <param name="ClassName">��������</param>
        /// <returns>����ʵ��</returns>
        public static object CreateInstance(string assemblyName, string typeName, object[] args, string ClassName)
        {
            //AppDomainSetup info = new AppDomainSetup();
            //info.ApplicationBase = "file:///" + System.Environment.CurrentDirectory;
            //AppDomain dom = AppDomain.CreateDomain("KJ73NDomain", null, info);

            object returnValue = null;
            object instance = null;
            System.Reflection.Assembly assemblyCreate = null;

            //if (assemblyCreateInstanceList.ContainsKey(typeName))
            //{
            //    instance = assemblyCreateInstanceList[typeName];
            //    assemblyCreate = assemblyCreateList[typeName];
            //}

            try
            {
                //foreach (System.Reflection.Assembly tempAssembly in assemblyCreateList)
                //{
                //    if (tempAssembly.Location == assemblyName)
                //    {
                //        assemblyCreate = tempAssembly;
                //    }
                //}
                if (assemblyCreate == null)
                {
                    assemblyCreate = Assembly.LoadFrom(assemblyName);
                    //assemblyCreateList.Add(typeName, assemblyCreate);
                }
            }
            catch (Exception ex)
            {
                Basic.Framework.Logging.LogHelper.Error(ex);
            }
            var type = assemblyCreate.GetType(typeName);
            
            if (instance == null)
            {
                instance = assemblyCreate.CreateInstance(typeName, true);
                //assemblyCreateInstanceList.Add(typeName, instance);
            }

            var method = type.GetMethod(ClassName);

            method.Invoke(instance, args);

            //AppDomain.Unload(dom);

            return returnValue;
        }

    }
}
