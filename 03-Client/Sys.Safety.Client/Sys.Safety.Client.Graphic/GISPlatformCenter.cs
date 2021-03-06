﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Configuration;
using DevExpress.XtraBars.Ribbon;
using DevExpress.XtraNavBar;
using DevExpress.XtraBars.Helpers;
using DevExpress.XtraSplashScreen;
using DevExpress.XtraBars;
using DevExpress.XtraEditors;
using DevExpress.XtraBars.Ribbon.Gallery;
using DevExpress.XtraRichEdit;
using DevExpress.XtraGrid;
using DevExpress.XtraPrinting;
using DevExpress.Utils.About;
using DevExpress.LookAndFeel;
using DevExpress.Utils;
using Basic.Framework.Logging;
using Sys.Safety.DataContract;
using Sys.Safety.Request.Graphicsbaseinf;
using Sys.Safety.ServiceContract;
using Basic.Framework.Service;
using System.Runtime.InteropServices;
using System.Collections;
using Sys.Safety.Client.Display;
using Sys.Safety.ClientFramework.CBFCommon;
using Sys.Safety.Client.Chart;
using Sys.Safety.Request.Def;

namespace Sys.Safety.Client.Graphic
{
    public partial class GISPlatformCenter : XtraForm
    {

        private IV_DefService _vdefService = ServiceFactory.Create<IV_DefService>();

        private ILargeDataAnalysisCacheClientService largeDataAnalysisCacheClientService = ServiceFactory.Create<ILargeDataAnalysisCacheClientService>();

        private IGraphicsbaseinfService graphicsbaseinfService = ServiceFactory.Create<IGraphicsbaseinfService>();
        public List<Jc_DefInfo> jc_defInfoList = new List<Jc_DefInfo>();
        /// <summary>
        /// 当前鼠标所在X坐标
        /// </summary>
        private int x;
        /// <summary>
        /// 当前鼠标所在Y坐标
        /// </summary>
        private int y;
        /// <summary>
        /// 图形库是否展开
        /// </summary>
        private bool IsGraphicsLibOn = true;
        /// <summary>
        /// 图形操作类
        /// </summary>
        public GraphicOperations GraphOpt = new GraphicOperations();
        /// <summary>
        /// 鼠标ToolTip事件
        /// </summary>
        ToolTip MousetoolTip = new ToolTip();
        /// <summary>
        /// 图形库位置下标变量
        /// </summary>
        int GraphLibindex = 0;
        /// <summary>
        /// 等待窗口
        /// </summary>
        private DevExpress.Utils.WaitDialogForm wdf = null;
        /// <summary>
        /// 是否在主界面中显示
        /// </summary>
        private bool IsInIframe = false;

        /// <summary>
        /// 显示类型
        /// </summary>
        private int ShowType = 1;

        /// <summary>
        /// 记录鼠标上一次移动的时间
        /// </summary>
        private DateTime lastMouseMoveTime = System.DateTime.Now;
        DefaultLookAndFeel defaultLookAndFeel;

        /// <summary>
        /// 获取图形更新线程
        /// </summary>
        private System.Threading.Thread m_GetMapDataThread;
        /// <summary>
        /// 运行标记
        /// </summary>
        public bool _isRun = false;

        public static Axmetamap2dLib.AxMetaMapX2D Mapobj = null;
        [DllImport("user32.dll", EntryPoint = "PostMessage")]
        public static extern int PostMessage(
        IntPtr hwnd,
        int wMsg,
        int wParam,
        int lParam
        );
        Hashtable htCmd = new Hashtable();//保存命令
        Hashtable htParam = new Hashtable();//保存参数
        int nViewCmdIndex = 0;//索引

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="GraphicEdit">是否处于编辑状态(true:是，false:否)</param>
        public GISPlatformCenter()
        {
            //设置所有窗体支持皮肤设置
            DevExpress.Skins.SkinManager.EnableFormSkins();

            defaultLookAndFeel = new DevExpress.LookAndFeel.DefaultLookAndFeel();
            if (!string.IsNullOrEmpty(Program.WindowStypeNow))
            {
                defaultLookAndFeel.LookAndFeel.SetSkinStyle(Program.WindowStypeNow);
            }




            //if (!Basic.Framework.Utils.COMREG.COMUtils.IsRegistered("8ACFF379-3E78-4E73-B75E-ECD908072A02"))
            //{
            //    DevExpress.XtraEditors.XtraMessageBox.Show("检测到GIS控件未注册，请先注册控件！");
            //    this.Close();
            //}
            InitializeComponent();
            //增加服务授权设置  20171026
            mx.SetPirvateKey("{D2D720B4-85C1-4CDF-AB0C-4C1BC04DEB8A}");
            mx.SetRegisterMode(2, "http://" + System.Configuration.ConfigurationManager.AppSettings["ServerIp"].ToString() + ":6789");
            // 解决窗口闪烁的问题  
            SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            //SetStyle(ControlStyles.UserPaint, true);
            //SetStyle(ControlStyles.AllPaintingInWmPaint, true); // 禁止擦除背景.
            //SetStyle(ControlStyles.OptimizedDoubleBuffer, true); // 双缓冲
            Program.main = this;
        }
        public GISPlatformCenter(bool GraphicEdit)
        {
            //设置所有窗体支持皮肤设置
            DevExpress.Skins.SkinManager.EnableFormSkins();

            defaultLookAndFeel = new DevExpress.LookAndFeel.DefaultLookAndFeel();
            if (!string.IsNullOrEmpty(Program.WindowStypeNow))
            {
                defaultLookAndFeel.LookAndFeel.SetSkinStyle(Program.WindowStypeNow);
            }




            //if (!Basic.Framework.Utils.COMREG.COMUtils.IsRegistered("8ACFF379-3E78-4E73-B75E-ECD908072A02"))
            //{
            //    DevExpress.XtraEditors.XtraMessageBox.Show("检测到GIS控件未注册，请先注册控件！");
            //    this.Close();
            //}
            GraphOpt.IsGraphicEdit = GraphicEdit;
            InitializeComponent();
            //增加服务授权设置  20171026
            mx.SetPirvateKey("{D2D720B4-85C1-4CDF-AB0C-4C1BC04DEB8A}");
            mx.SetRegisterMode(2, "http://" + System.Configuration.ConfigurationManager.AppSettings["ServerIp"].ToString() + ":6789");
            // 解决窗口闪烁的问题  
            SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            Program.main = this;
        }
        public GISPlatformCenter(Dictionary<string, string> param)
        {
            //设置所有窗体支持皮肤设置
            DevExpress.Skins.SkinManager.EnableFormSkins();

            defaultLookAndFeel = new DevExpress.LookAndFeel.DefaultLookAndFeel();
            if (!string.IsNullOrEmpty(Program.WindowStypeNow))
            {
                defaultLookAndFeel.LookAndFeel.SetSkinStyle(Program.WindowStypeNow);
            }


            //if (!Basic.Framework.Utils.COMREG.COMUtils.IsRegistered("8ACFF379-3E78-4E73-B75E-ECD908072A02"))
            //{
            //    DevExpress.XtraEditors.XtraMessageBox.Show("检测到GIS控件未注册，请先注册控件！");
            //    this.Close();
            //}
            if (param != null && param.Count > 0)
            {
                if (param["GraphicEdit"].ToString().ToLower() == "true")
                {
                    GraphOpt.IsGraphicEdit = true;
                }
                else
                {
                    GraphOpt.IsGraphicEdit = false;
                }
                if (param["GraphicInIframe"].ToString().ToLower() == "true")
                {
                    IsInIframe = true;
                }
                else
                {
                    IsInIframe = false;
                }

                if (param.ContainsKey("ShowType"))
                {
                    ShowType = int.Parse(param["ShowType"]);
                }
                if (param.ContainsKey("GraphName"))//增加图形名称参数传入  20180914
                {
                    try
                    {
                        GraphOpt.GraphNameNow = param["GraphName"].ToString();
                        GraphicsbaseinfInfo graph = Program.main.GraphOpt.getGraphicDto(GraphOpt.GraphNameNow);
                        if (graph == null)
                        {
                            GraphOpt.GraphNameNow = "";
                        }
                    }
                    catch (Exception ex)
                    {
                        Basic.Framework.Logging.LogHelper.Error(ex);
                    }
                }
            }
            else
            {
                return;
            }
            InitializeComponent();
            //增加服务授权设置  20171026
            mx.SetPirvateKey("{D2D720B4-85C1-4CDF-AB0C-4C1BC04DEB8A}");
            mx.SetRegisterMode(2, "http://" + System.Configuration.ConfigurationManager.AppSettings["ServerIp"].ToString() + ":6789");
            // 解决窗口闪烁的问题  
            SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            Program.main = this;
        }

        /// <summary>初始化皮肤样式</summary>
        private void InitDevControlSkin()
        {
            try
            {
                //DevExpress.UserSkins.BonusSkins.Register();
                BarButtonItem barBI = null;
                foreach (DevExpress.Skins.SkinContainer skin in DevExpress.Skins.SkinManager.Default.Skins)
                {
                    barBI = new BarButtonItem();
                    barBI.Tag = barBI.Name = barBI.Caption = SkinToText(skin.SkinName);
                    barBI.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(SkinItemClick);
                    barSubItem1.AddItem(barBI);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("GISPlatformCenter_InitDevControlSkin" + ex.Message + ex.StackTrace);
            }
        }
        /// <summary>皮肤单击</summary>
        private void SkinItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                string eTag = "";
                eTag = TextToSkin(e.Item.Tag.ToString());
                defaultLookAndFeel.LookAndFeel.SetSkinStyle(eTag);
                e.Item.Hint = eTag;
            }
            catch (Exception ex)
            {
                LogHelper.Error("GISPlatformCenter_SkinItemClick" + ex.Message + ex.StackTrace);
            }
        }
        /// <summary>
        ///  添加测点到地图
        /// </summary>
        /// <param name="PointName"></param>
        /// <param name="GraphBindType"></param>
        /// <param name="left"></param>
        /// <param name="top"></param>
        public void AddPointToMap(string PointName, string GrapUnitName, int GraphBindType, string zoomLevel, string animationState,
            float left, float top, string Width, string Height, string graphType)
        {
            try
            {
                GraphOpt.AddPointToMap(mx, PointName, GrapUnitName, GraphBindType, zoomLevel, animationState, left, top, Width, Height, graphType);
            }
            catch (Exception ex)
            {
                LogHelper.Error("AddPointToMap" + ex.Message + ex.StackTrace);
            }
        }
        /// <summary>
        /// 测点绑定
        /// </summary>
        /// <param name="PointName"></param>
        /// <param name="PointWz"></param>
        /// <param name="PointDevName"></param>
        public void EditPoint(string PointName, string PointWz, string PointDevName, string DisZoomlevel, string animationState, string Width, string Height,
             string TurnToPage, string PointId, string transformDeg)
        {
            try
            {
                GraphOpt.EditPoint(mx, PointName, PointWz, PointDevName, DisZoomlevel, animationState, Width, Height, TurnToPage, PointId, transformDeg);
            }
            catch (Exception ex)
            {
                LogHelper.Error("EditPoint" + ex.Message + ex.StackTrace);
            }
        }
        /// <summary>
        /// 加载图形
        /// </summary>
        /// <param name="GraphName"></param>
        public void LoadMap(string GraphName)
        {
            try
            {
                if (string.IsNullOrEmpty(GraphName))
                {
                    barStaticItem2.Caption = "未打开图形";
                }
                else
                {
                    barStaticItem2.Caption = "当前图形：" + GraphName;
                }

                GraphOpt.LoadMap(mx, GraphName);

                ////将当前打开的图形在页面切换栏目中选中
                //for (int i = 0; i < ribbonGalleryBarItem1.Gallery.Groups.Count; i++)
                //{
                //    for (int j = 0; j < ribbonGalleryBarItem1.Gallery.Groups[i].Items.Count; j++)
                //    {
                //        if (ribbonGalleryBarItem1.Gallery.Groups[i].Items[j].Caption == GraphName)
                //        {
                //            ribbonGalleryBarItem1.Gallery.Groups[i].Items[j].Checked = true;
                //        }
                //    }
                //}

                Mapobj = mx;
            }
            catch (Exception ex)
            {
                LogHelper.Error("LoadMap" + ex.Message + ex.StackTrace);
            }
        }
        /// <summary>
        /// 获取所有图层
        /// </summary>
        /// <returns></returns>
        public List<string> LoadLayers()
        {
            List<string> templist = new List<string>();
            try
            {
                templist = GraphOpt.LoadLayers(mx);
            }
            catch (Exception ex)
            {
                LogHelper.Error("LoadLayers" + ex.Message + ex.StackTrace);
            }
            return templist;
        }
        /// <summary>
        /// 图层显示
        /// </summary>
        public void LayerDisplay(string LayerName)
        {
            try
            {
                GraphOpt.LayerDisplay(mx, LayerName);
            }
            catch (Exception ex)
            {
                LogHelper.Error("LayerDisplay" + ex.Message + ex.StackTrace);
            }
        }
        /// <summary>
        /// 测点显示
        /// </summary>
        /// <param name="Point"></param>
        public void PointDisplay(string Point)
        {
            try
            {
                GraphOpt.PointDisplay(mx, Point);
            }
            catch (Exception ex)
            {
                LogHelper.Error("PointDisplay" + ex.Message + ex.StackTrace);
            }
        }
        /// <summary>
        /// 设备定位
        /// </summary>
        /// <param name="Point"></param>
        public void PointSercah(string Point)
        {
            try
            {
                GraphOpt.PointSercah(mx, Point);
            }
            catch (Exception ex)
            {
                LogHelper.Error("PointSercah" + ex.Message + ex.StackTrace);
            }
        }
        /// <summary>
        /// 图层隐藏
        /// </summary>
        public void LayerHidden(string LayerName)
        {
            try
            {
                GraphOpt.LayerHidden(mx, LayerName);
            }
            catch (Exception ex)
            {
                LogHelper.Error("PointDisplay" + ex.Message + ex.StackTrace);
            }
        }
        /// <summary>
        /// 测点隐藏
        /// </summary>
        /// <param name="Point"></param>
        public void PointHidden(string Point)
        {
            try
            {
                GraphOpt.PointHidden(mx, Point);
            }
            catch (Exception ex)
            {
                LogHelper.Error("PointDisplay" + ex.Message + ex.StackTrace);
            }
        }
        /// <summary>
        /// 获取所有测点的显示/隐藏状态
        /// </summary>
        /// <returns></returns>
        public List<string> GetAllPointDis()
        {
            List<string> templist = new List<string>();
            try
            {
                templist = GraphOpt.getAllGraphPointDis(mx);
            }
            catch (Exception ex)
            {
                LogHelper.Error("GetAllPointDis" + ex.Message + ex.StackTrace);
            }
            return templist;
        }

        /// <summary>
        /// 窗体加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GISPlatformCenter_Load(object sender, EventArgs e)
        {
            try
            {
                IAllSystemPointDefineService allSystemPointDefineService = ServiceFactory.Create<IAllSystemPointDefineService>();
                jc_defInfoList = allSystemPointDefineService.GetAllPointDefineCache().Data;

                #region wcf检查服务端是否正常运行
                //if (ConfigurationManager.AppSettings["ServiceType"].ToString() == "wcf")
                //{
                //    try
                //    {
                //        //ServiceFactory.CreateService<IGraphicsbaseinfService>().GetAll();//调用服务端接口，看能否正常调用来判断服务端是否开启

                //    }
                //    catch
                //    {
                //        //MessageBox.Show("连接服务端异常，请配置服务器！");

                //        WcfManage wcfmag = new WcfManage();
                //        wcfmag.ShowDialog();
                //        //退出应用程序
                //        System.Environment.Exit(0);
                //    }
                //}
                #endregion

                //wdf = new WaitDialogForm("正在加载数据...", "请等待...");

                InitDevControlSkin();//加载皮肤

                //设置窗体高度和宽度
                this.Width = Screen.GetWorkingArea(this).Width;
                this.Height = Screen.GetWorkingArea(this).Height;
                navBarControl1.Height = 200;
                mx.Height = this.Height - 290;

                this.Left = 0;
                this.Top = 0;
                if (!GraphOpt.IsGraphicEdit)//如果是非编辑状态
                {
                    //隐藏图形库
                    //navBarControl1.Width = 0;
                    navBarControl1.Height = 0;
                    mx.Height = this.Height - 100;
                    //隐藏筛选条件
                    //    navBarControl2.Width = 0;
                    //隐藏图形基本操作、命令操作功能
                    //BasicOptMenu.Visible = false;
                    //CommandPage.Visible = false;
                    barSubItem4.Visibility = BarItemVisibility.Never;
                    barSubItem2.Visibility = BarItemVisibility.Never;
                }
                else
                {
                    navBarControl2.Width = 0;
                }
                //判断是否为嵌入方式，如果是嵌入方式，则隐藏ribboncontrol1
                if (IsInIframe)
                {
                    //ribbonControl1.Visible = false;
                    //ribbonStatusBar1.Visible = false;
                }



                //设置不显示gis控件的滚动条
                mx.SetData("showProgress", "false");

                #region 从数据库读取所有图形文件
                GraphOpt.LoadGraphicsInfo();
                #endregion

                #region 加载图元库
                GraphLibindex = 0;
                XtraScrollableControl pal = new XtraScrollableControl();
                pal.Dock = DockStyle.Fill;
                pal.AutoScroll = true;
                pal.Controls.AddRange(GraphLibLoad(0));
                pal.Controls.AddRange(GraphLibLoad(3));
                GraphLibindex = 0;
                pal.Controls.AddRange(GraphLibTextLoad(0));
                pal.Controls.AddRange(GraphLibTextLoad(3));
                navBarGroupControlContainer4.Controls.Add(pal);

                GraphLibindex = 0;
                pal = new XtraScrollableControl();
                pal.Dock = DockStyle.Fill;
                pal.AutoScroll = true;
                pal.Controls.AddRange(GraphLibLoad(1));
                GraphLibindex = 0;
                pal.Controls.AddRange(GraphLibTextLoad(1));
                navBarGroupControlContainer1.Controls.Add(pal);

                GraphLibindex = 0;
                pal = new XtraScrollableControl();
                pal.Dock = DockStyle.Fill;
                pal.AutoScroll = true;
                pal.Controls.AddRange(GraphLibLoad(2));
                GraphLibindex = 0;
                pal.Controls.AddRange(GraphLibTextLoad(2));
                navBarGroupControlContainer3.Controls.Add(pal);

                //navBarControl2.Height = Screen.GetWorkingArea(this).Height - 400;
                DevTypeNvigation dev = null;
                if (GraphOpt.IsGraphicEdit)
                {
                    dev = new DevTypeNvigation(true);
                }
                else
                {
                    dev = new DevTypeNvigation(false);
                }
                //pal = new XtraScrollableControl();
                //pal.Dock = DockStyle.Fill;
                //pal.AutoScroll = true;
                //pal.Controls.Add(dev);
                navBarGroupControlContainer2.Controls.Add(dev);

                #endregion

                //隐藏悬浮拖动图元
                pictureBox1.Visible = false;
                //进入时隐藏菜单
                //ribbonControl1.Minimized = true;

                #region//加载页面跳转
                LoadPageChg();
                #endregion

                //if (wdf != null)
                //{
                //    wdf.Close();
                //}

                if (string.IsNullOrEmpty(GraphOpt.GraphNameNow))//增加传入GraphName参数判断，如果传了图形名称，此处不再默认加载  20180914
                {
                    if (ShowType == 1)
                    {
                        //获取通风系统默认图
                        var graphicsInfo = Program.main.GraphOpt.GetSystemtDefaultGraphics(0);
                        if (graphicsInfo != null && !string.IsNullOrWhiteSpace(graphicsInfo.GraphName))
                        {
                            //LoadMap(graphicsInfo.GraphName);

                            //更新服务端标记，统一通过线程来加载  20180319
                            var request = new SetSaveFlagRequest() { Flag = true };
                            var response = graphicsbaseinfService.SetSaveFlag(request);
                            GraphOpt.GraphNameNow = graphicsInfo.GraphName;
                        }
                    }
                    else
                    {
                        IList<GraphicsbaseinfInfo> AllGraphicDto = GraphOpt.getAllGraphicDto();

                        if (!GraphOpt.IsGraphicEdit)//如果是非编辑状态
                        {
                            if (AllGraphicDto.Count > 0)
                            {
                                //LoadMap(AllGraphicDto[0].GraphName);

                                //更新服务端标记，统一通过线程来加载  20180319
                                var request = new SetSaveFlagRequest() { Flag = true };
                                var response = graphicsbaseinfService.SetSaveFlag(request);
                                GraphOpt.GraphNameNow = AllGraphicDto[0].GraphName;//修改,通过线程加载  20170621

                            }

                        }
                    }
                }

                _isRun = true;
                m_GetMapDataThread = new Thread(RefMapData);
                //object o = mx;
                m_GetMapDataThread.Start();

                barStaticItem1.Caption = "启动成功";
            }
            catch (Exception ex)
            {
                if (wdf != null)
                {
                    wdf.Close();
                }
                LogHelper.Error("GISPlatformCenter_Load" + ex.Message + ex.StackTrace);
            }
        }
        /// <summary>
        /// 加载页面跳转
        /// </summary>
        public void LoadPageChg()
        {
            //try
            //{
            //    this.ribbonGalleryBarItem1.Gallery.Groups.Clear();//清除所有页面跳转页面

            //    List<string> tempPages = GraphOpt.GraphPagesLoad();
            //    DevExpress.XtraBars.Ribbon.GalleryItem[] ItemList = new GalleryItem[tempPages.Count];
            //    for (int i = 0; i < tempPages.Count; i++)
            //    {
            //        GalleryItem tempItem = new GalleryItem();
            //        tempItem.Image = Sys.Safety.Client.Graphic.Properties.Resources.img;
            //        if (tempPages[i].ToString().Length <= 15)
            //        {
            //            tempItem.Caption = tempPages[i].ToString();
            //        }
            //        else
            //        {
            //            tempItem.Caption = tempPages[i].ToString().Substring(0, 15) + "...";
            //        }
            //        tempItem.Value = tempPages[i].ToString();
            //        ItemList[i] = tempItem;
            //    }
            //    DevExpress.XtraBars.Ribbon.GalleryItemGroup galleryItemGroup3 = new DevExpress.XtraBars.Ribbon.GalleryItemGroup();
            //    galleryItemGroup3.Items.AddRange(ItemList);
            //    this.ribbonGalleryBarItem1.Gallery.Groups.AddRange(new DevExpress.XtraBars.Ribbon.GalleryItemGroup[] {
            //galleryItemGroup3});
            //}
            //catch (Exception ex)
            //{
            //    LogHelper.Error("LoadPageChg" + ex.Message + ex.StackTrace);
            //}
        }
        /// <summary>
        /// 线程中调用
        /// </summary>
        public void LoadPageChg1()
        {
            //try
            //{
            //    this.ribbonGalleryBarItem1.Gallery.Groups.Clear();//清除所有页面跳转页面

            //    List<string> tempPages = GraphOpt.GraphPagesLoad();
            //    DevExpress.XtraBars.Ribbon.GalleryItem[] ItemList = new GalleryItem[tempPages.Count];
            //    for (int i = 0; i < tempPages.Count; i++)
            //    {
            //        GalleryItem tempItem = new GalleryItem();
            //        tempItem.Image = Sys.Safety.Client.Graphic.Properties.Resources.img;
            //        if (tempPages[i].ToString().Length <= 15)
            //        {
            //            tempItem.Caption = tempPages[i].ToString();
            //        }
            //        else
            //        {
            //            tempItem.Caption = tempPages[i].ToString().Substring(0, 15) + "...";
            //        }
            //        tempItem.Value = tempPages[i].ToString();
            //        ItemList[i] = tempItem;
            //    }
            //    DevExpress.XtraBars.Ribbon.GalleryItemGroup galleryItemGroup3 = new DevExpress.XtraBars.Ribbon.GalleryItemGroup();
            //    galleryItemGroup3.Items.AddRange(ItemList);
            //    this.ribbonGalleryBarItem1.Gallery.Groups.AddRange(new DevExpress.XtraBars.Ribbon.GalleryItemGroup[] {
            //galleryItemGroup3});
            //}
            //catch (Exception ex)
            //{
            //    LogHelper.Error("LoadPageChg" + ex.Message + ex.StackTrace);
            //}
        }
        /// <summary>
        /// 页面跳转单击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ribbonGalleryBarItem1_GalleryItemClick(object sender, GalleryItemClickEventArgs e)
        {
            try
            {
                //if (!GraphOpt.IsGraphicEdit)
                //{
                //    if (DevExpress.XtraEditors.XtraMessageBox.Show("正在预览，要停止吗?", "提示", MessageBoxButtons.YesNo) == DialogResult.No)
                //    {
                //        return;
                //    }
                //}

                //isExit = true;
                //GraphOpt.IsGraphicEdit = true;

                GalleryItem tempGall = e.Item;
                LoadMap(tempGall.Value.ToString());
                GraphOpt.IsTopologyInit = false;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ribbonGalleryBarItem1_GalleryItemClick" + ex.Message + ex.StackTrace);
            }
        }
        /// <summary>
        /// 加载图形库
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private PictureBox[] GraphLibLoad(int type)
        {

            IList<string> GraphImgList = new List<string>();
            string graphPath = "";
            PictureBox[] pb = null;
            try
            {
                GraphImgList = GraphOpt.GraphicsLibLoad(type);
                pb = new PictureBox[GraphImgList.Count];
                switch (type)
                {
                    case 0:
                        graphPath = "Text";
                        break;
                    case 1:
                        graphPath = "Topology";
                        break;
                    case 2:
                        graphPath = "Svg";
                        break;
                    case 3:
                        graphPath = "Gif";
                        break;
                }

                int baseX = 20;
                if (GraphLibindex > 0)
                {
                    baseX = (GraphLibindex - 1) * 90 + 20;
                }
                int baseY = 0;
                int baseCount = navBarControl1.Width / 90;
                for (int i = 0; i < GraphImgList.Count; i++)
                {
                    pb[i] = new System.Windows.Forms.PictureBox();
                    pb[i].BorderStyle = BorderStyle.FixedSingle;
                    pb[i].Name = GraphImgList[i];
                    pb[i].Tag = type;
                    pb[i].Width = 60;
                    pb[i].Height = 60;
                    if (GraphLibindex % baseCount == 0)
                    {
                        baseX = 20;
                        pb[i].Location = new Point(baseX, (int)(GraphLibindex / baseCount) * (100) + 5);
                        pb[i].Image = Image.FromFile(Application.StartupPath + @"\\mx\\" + graphPath + "\\" + GraphImgList[i] + ".png");
                    }
                    else
                    {
                        baseX += 90;
                        pb[i].Location = new Point(baseX, (int)(GraphLibindex / baseCount) * (100) + 5);
                        pb[i].Image = Image.FromFile(Application.StartupPath + @"\\mx\\" + graphPath + "\\" + GraphImgList[i] + ".png");
                    }
                    pb[i].SizeMode = PictureBoxSizeMode.Zoom;
                    pb[i].MouseMove += new MouseEventHandler(onMouseMove);

                    GraphLibindex++;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("GraphLibLoad" + ex.Message + ex.StackTrace);
            }
            return pb;
        }
        private Label[] GraphLibTextLoad(int type)
        {

            IList<string> GraphImgList = new List<string>();
            string graphPath = "";
            Label[] pb = null;
            try
            {
                GraphImgList = GraphOpt.GraphicsLibLoad(type);
                switch (type)
                {
                    case 0:
                        graphPath = "Text";
                        break;
                    case 1:
                        graphPath = "Topology";
                        break;
                    case 2:
                        graphPath = "Svg";
                        break;
                    case 3:
                        graphPath = "Gif";
                        break;
                }
                pb = new Label[GraphImgList.Count];

                int baseX = 20;
                int baseY = 0;
                int baseCount = navBarControl1.Width / 90;
                if (GraphLibindex > 0)
                {
                    baseX = (GraphLibindex - 1) * 90 + 20;
                }

                for (int i = 0; i < GraphImgList.Count; i++)
                {
                    pb[i] = new System.Windows.Forms.Label();

                    string labeltext = string.Empty;

                    if (GraphImgList[i].Contains("&"))
                    {
                        labeltext = GraphImgList[i].Substring(0, GraphImgList[i].IndexOf('&'));
                        //pb[i].Text = GraphImgList[i].Substring(0, GraphImgList[i].IndexOf('&'));
                    }
                    else
                    {
                        labeltext = GraphImgList[i];
                        //pb[i].Text = GraphImgList[i];
                    }
                    pb[i].Text = labeltext;
                    pb[i].Width = 90;

                    //如果图元名称过长，则分多行显示
                    if (labeltext.Length > 7)
                    {
                        int rows = labeltext.Length / 7 + 1;
                        pb[i].Height = 18 * rows;
                        pb[i].AutoSize = false;
                    }



                    if (GraphLibindex % baseCount == 0)
                    {
                        baseX = 20;
                        pb[i].Location = new Point(baseX, (int)(GraphLibindex / baseCount) * (100) + 70);

                    }
                    else
                    {
                        baseX += 90;
                        pb[i].Location = new Point(baseX, (int)(GraphLibindex / baseCount) * (100) + 70);

                    }


                    GraphLibindex++;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("GraphLibTextLoad" + ex.Message + ex.StackTrace);
            }
            return pb;
        }


        /// <summary>
        /// 图元鼠标移动事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void onMouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                PictureBox pic = (PictureBox)sender;
                pictureBox1.Width = 60;
                pictureBox1.Height = 60;
                pictureBox1.Image = pic.Image;
                pictureBox1.Visible = true;
                x = e.Location.X;
                y = e.Location.Y;
                pictureBox1.Left = this.PointToClient(Control.MousePosition).X - 10;
                pictureBox1.Top = this.PointToClient(Control.MousePosition).Y - 10;
                pictureBox1.Name = pic.Name;
                pictureBox1.Tag = pic.Tag;



                MousetoolTip.AutoPopDelay = 3000;
                MousetoolTip.InitialDelay = 1000;
                MousetoolTip.ReshowDelay = 200;

                if (IsGraphicsLibOn)
                {
                    MousetoolTip.SetToolTip(this.pictureBox1, "拖动到图上添加设备");
                }
                else
                {
                    MousetoolTip.SetToolTip(this.pictureBox1, "拖动到图上并单击添加设备");
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("onMouseMove" + ex.Message + ex.StackTrace);
            }
        }
        /// <summary>
        /// 悬浮拖动图元鼠标移动事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                PictureBox pic1 = (PictureBox)(sender);
                if (e.Button == MouseButtons.Left)
                {
                    if (IsGraphicsLibOn)
                    {


                        //大于300毫秒或有组件显示或隐藏才进行重绘
                        TimeSpan mouseMoveTimeStep = System.DateTime.Now - lastMouseMoveTime;
                        if (mouseMoveTimeStep.TotalMilliseconds < 30) { return; }
                        lastMouseMoveTime = System.DateTime.Now;

                        (pic1).Left -= x - e.X;
                        (pic1).Top -= y - e.Y;

                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("pictureBox1_MouseMove" + ex.Message + ex.StackTrace);
            }
        }
        /// <summary>
        /// 悬浮拖动图元鼠标弹起事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                PictureBox pic = (PictureBox)(sender);


                Image img = pic.Image;
                string Width = img.PhysicalDimension.Width.ToString();
                string Height = img.PhysicalDimension.Height.ToString();

                pic.Visible = false;
                float left = 0;
                float top = 0;
                string zoomLevel = "1$22";
                string graphType = GraphOpt.getGraphicNowType().ToString();
                switch (pic.Tag.ToString())//位置调整
                {
                    case "0":
                        // left = (pic).Left - navBarControl1.Width + 35;
                        left = (pic).Left + 35;
                        //top = (pic).Top - ribbonControl1.Height + 55;
                        top = (pic).Top + 55;
                        break;
                    case "1":
                        //left = (pic).Left - navBarControl1.Width;
                        left = (pic).Left;
                        //top = (pic).Top - ribbonControl1.Height + 5;
                        top = (pic).Top + 5;
                        break;
                    case "2"://SVG图元
                        //left = (pic).Left - navBarControl1.Width;
                        left = (pic).Left;
                        //top = (pic).Top - ribbonControl1.Height + 5;
                        top = (pic).Top + 5;
                        zoomLevel = "1$22";//最小显示级别、最大显示级别
                        break;
                    case "3":
                        //left = (pic).Left - navBarControl1.Width + 35;
                        left = (pic).Left + 35;
                        //top = (pic).Top - ribbonControl1.Height + 35;
                        top = (pic).Top + 35;
                        break;
                }
                if (left >= 0 && top >= 0)//鼠标必须在地图控件内
                {
                    //拓扑图形页面只能加拓扑图元判断
                    if (GraphOpt.getGraphicNowType() == 1)
                    {
                        if (pic.Tag.ToString() != "1")
                        {
                            //DevExpress.XtraEditors.XtraMessageBox.Show("请选择‘拓扑图形库’中的图元！");
                            barStaticItem1.Caption = "请选择‘拓扑图形库’中的图元！";
                            pic.Visible = false;
                            return;
                        }
                    }
                    else if (GraphOpt.getGraphicNowType() == -1)
                    {
                        //DevExpress.XtraEditors.XtraMessageBox.Show("当前未打开图形，请先打开图形！");
                        barStaticItem1.Caption = "当前未打开图形，请先打开图形！";
                        pic.Visible = false;
                        return;
                    }
                    if (GraphOpt.IsGraphicEdit == false)
                    {
                        //DevExpress.XtraEditors.XtraMessageBox.Show("当前处于运行状态，不能进行图元编辑，请切换到编辑状态，再进行图元编辑！");
                        barStaticItem1.Caption = "当前处于运行状态，不能进行图元编辑，请切换到编辑状态，再进行图元编辑！";
                        pic.Visible = false;
                        return;
                    }

                    AddPointToMap("000000", pic.Name, int.Parse(pic.Tag.ToString()), zoomLevel, "-1", left, top, Width, Height, graphType);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("pictureBox1_MouseUp" + ex.Message + ex.StackTrace);
            }
        }
        /// <summary>
        /// 图形库控件SizeChange事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void navBarControl1_SizeChanged(object sender, EventArgs e)
        {
            //if (navBarControl1.Width < 50)//未展开
            //{
            //    IsGraphicsLibOn = false;
            //    //pictureBox1.Visible = false;
            //}
            //else
            //{
            //    IsGraphicsLibOn = true;
            //}
        }
        /// <summary>
        /// 基础图形库点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void navBarGroupControlContainer4_MouseDown(object sender, MouseEventArgs e)
        {
            pictureBox1.Visible = false;
        }
        /// <summary>
        /// 图库控件单击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void navBarControl1_MouseDown(object sender, MouseEventArgs e)
        {
            pictureBox1.Visible = false;
        }
        /// <summary>
        /// 菜单控制单击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ribbonControl1_MouseDown(object sender, MouseEventArgs e)
        {
            pictureBox1.Visible = false;
        }
        /// <summary>
        /// 图形控件命令响应方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mx_OnViewCallOutCommand(object sender, Axmetamap2dLib.IMetaMapX2DEvents_OnViewCallOutCommandEvent e)
        {
            String strCmd = e.p_sCmd;
            String strParam = e.p_sParam;
            nViewCmdIndex = nViewCmdIndex + 1;
            if (nViewCmdIndex >= Int32.MaxValue - 1)
            {
                nViewCmdIndex = 1;
            }
            htCmd.Add(nViewCmdIndex, strCmd);
            htParam.Add(nViewCmdIndex, strParam);

            PostMessage(this.Handle, 0x0401, nViewCmdIndex, 0);

        }
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case 0x0401:
                    int nIndex = (int)m.WParam;
                    if (htCmd.Contains(nIndex) && htParam.Contains(nIndex))
                    {
                        ProcessViewCallOutCommand(htCmd[nIndex].ToString(), htParam[nIndex].ToString());
                        htCmd.Remove(nIndex);
                        htParam.Remove(nIndex);
                    }
                    break;
                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        private void ProcessViewCallOutCommand(String strCmd, String strParam)
        {
            string Param = "", PointId;

            switch (strCmd)
            {
                case "MessagePub"://测试交互代码
                    DevExpress.XtraEditors.XtraMessageBox.Show(strParam);
                    break;
                case "PointEdit"://图形测点编辑
                    barStaticItem1.Caption = "测点编辑";
                    try
                    {
                        Param = strParam;
                        string Point = strParam.ToString().Split('|')[0];
                        string UnitName = strParam.ToString().Split('|')[1];
                        string BindType = strParam.ToString().Split('|')[2];
                        string ZoomLevel = strParam.ToString().Split('|')[3];
                        string animationState = strParam.ToString().Split('|')[4];
                        string transformDeg = "";
                        string TurnToPage = "";
                        //判断当前图元类型
                        if (UnitName.Contains("&"))//静态图元
                        {
                            if (BindType == "1" || BindType == "2")
                            {
                                string Width = strParam.ToString().Split('|')[5];
                                string Height = strParam.ToString().Split('|')[6];
                                TurnToPage = strParam.ToString().Split('|')[7];
                                transformDeg = strParam.ToString().Split('|')[9];
                                PointEdit pointedit = new PointEdit(Point, Width, Height, TurnToPage, transformDeg);
                                pointedit.Show();
                            }
                            else
                            {
                                TurnToPage = strParam.ToString().Split('|')[5];
                                transformDeg = strParam.ToString().Split('|')[7];
                                PointEdit pointedit = new PointEdit(Point, TurnToPage, transformDeg);
                                pointedit.Show();
                            }
                        }
                        else//动态图元
                        {
                            if (BindType == "1" || BindType == "2")
                            {
                                string Width = strParam.ToString().Split('|')[5];
                                string Height = strParam.ToString().Split('|')[6];
                                TurnToPage = strParam.ToString().Split('|')[7];
                                PointId = strParam.ToString().Split('|')[8];
                                transformDeg = strParam.ToString().Split('|')[9];
                                PointDefEdit pointedit = new PointDefEdit(Point, UnitName, BindType, ZoomLevel, animationState, Width, Height, TurnToPage, PointId, transformDeg);
                                pointedit.Show();
                            }
                            else
                            {
                                TurnToPage = strParam.ToString().Split('|')[5];
                                PointId = strParam.ToString().Split('|')[6];
                                transformDeg = strParam.ToString().Split('|')[7];
                                //如果是视频测点，则采用视频修改页面；如果是大数据分析模型，则采用大数据模型绑定界面；监控、广播、人员定位采用监控的修改页面
                                if (UnitName.Contains("摄像机"))
                                {
                                    VideoPointDefEdit pointedit = new VideoPointDefEdit(Point, UnitName, BindType, ZoomLevel, animationState, TurnToPage, PointId);
                                    pointedit.Show();
                                }
                                else if (UnitName.Contains("大数据分析"))
                                {
                                    AnalysisModelPointEdit pointedit = new AnalysisModelPointEdit(Point, UnitName, BindType, ZoomLevel, animationState, TurnToPage, PointId);
                                    pointedit.Show();
                                }
                                else if (UnitName.Contains("识别器"))
                                {
                                    PersonPointDefEdit pointedit = new PersonPointDefEdit(Point, UnitName, BindType, ZoomLevel, animationState, TurnToPage, PointId);
                                    pointedit.Show();
                                }
                                else if (UnitName.Contains("广播"))
                                {
                                    AudioPointEdit pointedit = new AudioPointEdit(Point, UnitName, BindType, ZoomLevel, animationState, TurnToPage, PointId);
                                    pointedit.Show();
                                }
                                else
                                {
                                    PointDefEdit pointedit = new PointDefEdit(Point, UnitName, BindType, ZoomLevel, animationState, TurnToPage, PointId, transformDeg);
                                    pointedit.Show();
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //DevExpress.XtraEditors.XtraMessageBox.Show("测点编辑失败，详细请查看错误日志！");
                        barStaticItem1.Caption = "编辑失败，详细请查看错误日志！";
                        LogHelper.Error("mx_OnViewCallOutCommand_PointEdit" + ex.Message + ex.StackTrace);
                    }
                    break;
                case "PointDel"://图形测点删除
                    barStaticItem1.Caption = "测点删除";
                    try
                    {
                        Param = strParam;
                        GraphOpt.DelPoint(mx, Param);
                    }
                    catch (Exception ex)
                    {
                        //DevExpress.XtraEditors.XtraMessageBox.Show("删除失败，详细请查看错误日志！");
                        barStaticItem1.Caption = "删除失败，详细请查看错误日志！";
                        LogHelper.Error("mx_OnViewCallOutCommand_PointDel" + ex.Message + ex.StackTrace);
                    }
                    break;
                case "PointsSave"://图形测点保存
                    barStaticItem1.Caption = "图形保存";
                    try
                    {
                        Param = strParam;
                        GraphOpt.PointsSave(Param);
                        barStaticItem1.Caption = "保存成功！";
                        //DevExpress.XtraEditors.XtraMessageBox.Show("保存成功！");                       
                    }
                    catch (Exception ex)
                    {
                        //DevExpress.XtraEditors.XtraMessageBox.Show("保存失败，详细请查看错误日志！");
                        barStaticItem1.Caption = "保存失败，详细请查看错误日志！";
                        LogHelper.Error("mx_OnViewCallOutCommand_PointsSave" + ex.Message + ex.StackTrace);
                    }
                    break;
                case "RoutesSave"://保存测点连线                    
                    try
                    {
                        Param = strParam;
                        if (string.IsNullOrEmpty(Param))
                        {
                            return;
                        }
                        GraphOpt.RoutesSave(Param);
                    }
                    catch (Exception ex)
                    {
                        //DevExpress.XtraEditors.XtraMessageBox.Show("保存测点连线失败，详细请查看错误日志！");
                        barStaticItem1.Caption = "保存失败，详细请查看错误日志！";
                        LogHelper.Error("mx_OnViewCallOutCommand_RoutesSave" + ex.Message + ex.StackTrace);
                    }
                    break;
                case "LoadPoint"://加载图形绑定的测点信息                     
                    try
                    {
                        GraphOpt.LoadMapPointsInfo(mx, GraphOpt.GraphNameNow);
                    }
                    catch (Exception ex)
                    {
                        //DevExpress.XtraEditors.XtraMessageBox.Show("加载图形绑定的测点信息失败，详细请查看错误日志！");
                        barStaticItem1.Caption = "加载图形的测点信息失败，详细请查看错误日志！";
                        LogHelper.Error("mx_OnViewCallOutCommand_LoadPoint" + ex.Message + ex.StackTrace);
                    }
                    break;
                case "SetMapEditState"://设置图形的可编辑状态                    
                    try
                    {
                        GraphOpt.setGraphEditState(mx, GraphOpt.IsGraphicEdit);
                    }
                    catch (Exception ex)
                    {
                        //DevExpress.XtraEditors.XtraMessageBox.Show("设置图形的可编辑状态失败，详细请查看错误日志！");
                        barStaticItem1.Caption = "设置图形的编辑状态失败，详细请查看错误日志！";
                        LogHelper.Error("mx_OnViewCallOutCommand_SetMapEditState" + ex.Message + ex.StackTrace);
                    }
                    break;
                case "setMapEditSave"://设置图形是否保存
                    try
                    {
                        Param = strParam;
                        GraphOpt.setGraphEditSave(bool.Parse(Param));
                    }
                    catch (Exception ex)
                    {
                        //DevExpress.XtraEditors.XtraMessageBox.Show("设置图形是否保存失败，详细请查看错误日志！");
                        barStaticItem1.Caption = "设置图形是否保存失败，详细请查看错误日志！";
                        LogHelper.Error("mx_OnViewCallOutCommand_setMapEditSave" + ex.Message + ex.StackTrace);
                    }
                    break;
                case "SetMapTopologyInit"://拓扑图初始化所有井上设备                    
                    try
                    {
                        if (GraphOpt.IsTopologyInit)
                        {
                            GraphOpt.SetMapTopologyInit(mx);
                        }
                    }
                    catch (Exception ex)
                    {
                        //DevExpress.XtraEditors.XtraMessageBox.Show("拓扑图加载井上设备失败，详细请查看错误日志！");
                        barStaticItem1.Caption = "拓扑图加载井上设备失败，详细请查看错误日志！";
                        LogHelper.Error("mx_OnViewCallOutCommand_SetMapTopologyInit" + ex.Message + ex.StackTrace);
                    }
                    break;
                case "pageToImg":
                    barStaticItem1.Caption = "导出图片";
                    saveFileDialog1.FileName = GraphOpt.GraphNameNow;
                    if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                    {
                        GraphOpt.mapToImage(mx, saveFileDialog1.FileName);
                    }
                    break;
                case "layerDis":
                    barStaticItem1.Caption = "图层显示/隐藏";
                    LayerDisHid layerdis = new LayerDisHid();
                    layerdis.Show();
                    break;
                case "pointDis":
                    barStaticItem1.Caption = "设备显示/隐藏";
                    PointDisHid pointdishid = new PointDisHid();
                    pointdishid.Show();
                    break;
                case "pointSercah":
                    barStaticItem1.Caption = "设备查找";
                    PointSerach pointsercah = new PointSerach();
                    pointsercah.Show();
                    break;
                case "PageChange":
                    Param = strParam;
                    LoadMap(Param);
                    GraphOpt.IsTopologyInit = false;
                    break;
                case "AddMapRightMenu":
                    //添加右键菜单
                    string pageListstr = "";
                    IList<GraphicsbaseinfInfo> AllGraphicDto = GraphOpt.getAllGraphicDto();
                    if (AllGraphicDto.Count > 0)
                    {
                        for (int i = 0; i < AllGraphicDto.Count; i++)
                        {
                            pageListstr += AllGraphicDto[i].GraphName + "|";
                        }
                        if (pageListstr.Contains("|"))
                        {
                            pageListstr = pageListstr.Substring(0, pageListstr.Length - 1);
                        }
                        GraphOpt.AddMapRightMenu(mx, pageListstr);
                    }
                    //设置隐藏联动信息查看菜单   20180319
                    GraphOpt.RemoveRichPointRconMenu(mx, "联动信息查看");
                    break;
                case "DoRefPointSsz":
                    try
                    {
                        GraphOpt.DoRefPointSsz(mx);
                    }
                    catch (Exception ex)
                    {
                        //DevExpress.XtraEditors.XtraMessageBox.Show("实时刷新数据失败，详细请查看错误日志！");
                        barStaticItem1.Caption = "实时刷新数据失败，详细请查看错误日志！";
                        LogHelper.Error("mx_OnViewCallOutCommand_DoRefPointSsz" + ex.Message + ex.StackTrace);
                    }
                    break;
                case "PointDblClick":
                    string Point_Now = strParam.ToString().Split('|')[0];
                    string UnitName_Now = strParam.ToString().Split('|')[1];

                    GraphicspointsinfInfo PointDto = GraphOpt.getPointDto(Point_Now);
                    if (!string.IsNullOrEmpty(PointDto.Bz4))
                    {
                        GraphOpt.LoadMap(mx, PointDto.Bz4);
                    }

                    if (UnitName_Now.Contains("摄像头"))
                    {
                        //CFVideoInf videoForm = new CFVideoInf(Point_Now);
                        //videoForm.Show();
                    }
                    break;
                case "ShowDetailInRightMenu":
                    string point_Now = strParam.ToString().Split('|')[1];
                    if (point_Now.Contains("￣"))
                    {
                        point_Now = point_Now.Substring(0, point_Now.IndexOf('￣'));
                    }
                    string menu_Now = strParam.ToString().Split('|')[0];
                    Dictionary<string, string> point1 = new Dictionary<string, string>();
                    #region 右键菜单链接
                    switch (menu_Now)
                    {
                        case "详细信息查看":
                            try
                            {
                                string point = point_Now;
                                //先根据测点号获取是否为监控、识别器、广播测点；如果没有则判断是否为视频测点;再判断是否为分析模型
                                var tempjc_defInfo = jc_defInfoList.FirstOrDefault(a => a.Point == point);
                                if (tempjc_defInfo != null)
                                {
                                    if (tempjc_defInfo.DevPropertyID == 0)//分站
                                    {
                                        FzShowForm fzshow = new FzShowForm(point);
                                        fzshow.ShowDialog();
                                    }
                                    else if (tempjc_defInfo.DevPropertyID == 3)//控制量
                                    {
                                        KzRealForm kzshow = new KzRealForm(point);
                                        kzshow.ShowDialog();
                                    }
                                    else if (tempjc_defInfo.DevPropertyID == 1)//模拟量
                                    {
                                        AnalogRealForm analogshow = new AnalogRealForm(point);
                                        analogshow.ShowDialog();
                                    }
                                    else if (tempjc_defInfo.DevPropertyID == 2)//开关量
                                    {
                                        SwitchRealForm switchshow = new SwitchRealForm(point);
                                        switchshow.ShowDialog();
                                    }
                                    else if (tempjc_defInfo.DevPropertyID == 7)//识别器
                                    {
                                        RecognizerRealForm recognizershow = new RecognizerRealForm(point);
                                        recognizershow.ShowDialog();
                                    }
                                }
                                else
                                {
                                    var vdefino = _vdefService.GetDefByIP(new DefIPRequest { IPAddress = point }).Data;
                                    if (vdefino != null)
                                    {
                                        frmVideoPreview videoform = new frmVideoPreview(vdefino.Id);
                                        videoform.ShowDialog();
                                    }
                                    else
                                    {
                                        var analysisconfigs = largeDataAnalysisCacheClientService.GetAllLargeDataAnalysisConfigCache(new Sys.Safety.Request.LargeDataAnalysisCacheClientGetAllRequest()).Data;
                                        var analysisconfig = analysisconfigs.FirstOrDefault(o => o.Name == point);
                                        if (analysisconfig != null)
                                        {
                                            AnalysisResultsInRealtime analysisform = new AnalysisResultsInRealtime(analysisconfig.Id);
                                            analysisform.ShowDialog();
                                        }
                                    }

                                }
                            }
                            catch (Exception ex)
                            {
                                OprFuction.SaveErrorLogs("历史运行记录", ex);
                            }
                            break;
                        case "运行记录查询":
                            try
                            {
                                string point = point_Now;
                                if (!string.IsNullOrEmpty(point))
                                {
                                    point1.Add("SourceIsList", "true");
                                    point1.Add("Key_viewsbrunlogreport1_point", "等于&&$" + point);
                                    point1.Add("Display_viewsbrunlogreport1_point", "等于&&$" + point);
                                }
                                point1.Add("ListID", "27");
                                //RequestUtil.ExcuteCommand("Requestsbrunlogreport", point1, false);
                                Sys.Safety.Reports.frmList listReport = new Sys.Safety.Reports.frmList(point1);
                                listReport.Show();
                            }
                            catch (Exception ex)
                            {
                                OprFuction.SaveErrorLogs("历史运行记录", ex);
                            }
                            break;
                        case "模拟量实时曲线":
                            try
                            {
                                string point = point_Now;
                                if (!string.IsNullOrEmpty(point))
                                {
                                    Jc_DefInfo tempjc_defInfo = jc_defInfoList.Find(a => a.Point == point);
                                    if (tempjc_defInfo.DevPropertyID == 1)
                                    {
                                        point = tempjc_defInfo.PointID;
                                        point1.Add("PointID", point);
                                        //RequestUtil.ExcuteCommand("RequestMnl_SSZChart", point1, false);
                                        Mnl_SSZChart mnl_SSZChart = new Mnl_SSZChart(point1);
                                        mnl_SSZChart.Show();
                                    }
                                    else
                                    {
                                        OprFuction.MessageBoxShow(0, "请选择模拟量测点");
                                    }
                                }
                                else
                                {
                                    //RequestUtil.ExcuteCommand("RequestMnl_SSZChart", null, false);
                                    Mnl_SSZChart mnl_SSZChart = new Mnl_SSZChart();
                                    mnl_SSZChart.Show();
                                }
                            }
                            catch (Exception ex)
                            {
                                OprFuction.SaveErrorLogs("模拟量实时曲线", ex);
                            }
                            break;
                        case "模拟量历史曲线（5分钟）":
                            try
                            {
                                string point = point_Now;
                                if (!string.IsNullOrEmpty(point))
                                {
                                    Jc_DefInfo tempjc_defInfo = jc_defInfoList.Find(a => a.Point == point);
                                    if (tempjc_defInfo.DevPropertyID == 1)
                                    {
                                        point = tempjc_defInfo.PointID;
                                        point1.Add("PointID", point);
                                        //RequestUtil.ExcuteCommand("RequestMnl_FiveMiniteLine", point1, false);
                                        Mnl_FiveMiniteLine mnl_FiveMiniteLine = new Mnl_FiveMiniteLine(point1);
                                        mnl_FiveMiniteLine.Show();
                                    }
                                    else
                                    {
                                        OprFuction.MessageBoxShow(0, "请选择模拟量测点");
                                    }
                                }
                                else
                                {
                                    //RequestUtil.ExcuteCommand("RequestMnl_FiveMiniteLine", null, false);
                                    Mnl_FiveMiniteLine mnl_FiveMiniteLine = new Mnl_FiveMiniteLine();
                                    mnl_FiveMiniteLine.Show();
                                }
                            }
                            catch (Exception ex)
                            {
                                OprFuction.SaveErrorLogs("模拟量历史曲线5分钟", ex);
                            }
                            break;
                        case "模拟量历史曲线（密采）":
                            try
                            {
                                string point = point_Now;
                                if (!string.IsNullOrEmpty(point))
                                {
                                    Jc_DefInfo tempjc_defInfo = jc_defInfoList.Find(a => a.Point == point);
                                    if (tempjc_defInfo.DevPropertyID == 1)
                                    {
                                        point = tempjc_defInfo.PointID;
                                        point1.Add("PointID", point);
                                        //RequestUtil.ExcuteCommand("RequestMnl_McLine", point1, false);
                                        Mnl_McLine mnl_McLine = new Mnl_McLine(point1);
                                        mnl_McLine.Show();
                                    }
                                    else
                                    {
                                        OprFuction.MessageBoxShow(0, "请选择模拟量测点");
                                    }
                                }
                                else
                                {
                                    //RequestUtil.ExcuteCommand("RequestMnl_McLine", null, false);
                                    Mnl_McLine mnl_McLine = new Mnl_McLine();
                                    mnl_McLine.Show();
                                }
                            }
                            catch (Exception ex)
                            {
                                OprFuction.SaveErrorLogs("模拟量历史曲线密采", ex);
                            }
                            break;
                        case "模拟量小时曲线":
                            try
                            {
                                string point = point_Now;
                                if (!string.IsNullOrEmpty(point))
                                {
                                    Jc_DefInfo tempjc_defInfo = jc_defInfoList.Find(a => a.Point == point);
                                    if (tempjc_defInfo.DevPropertyID == 1)
                                    {
                                        point = tempjc_defInfo.PointID;
                                        point1.Add("PointID", point);
                                        //RequestUtil.ExcuteCommand("RequestMnl_DayZdzLine", point1, false);
                                        Mnl_DayZdzLine mnl_DayZdzLine = new Mnl_DayZdzLine(point1);
                                        mnl_DayZdzLine.Show();
                                    }
                                    else
                                    {
                                        OprFuction.MessageBoxShow(0, "请选择模拟量测点");
                                    }
                                }
                                else
                                {
                                    //RequestUtil.ExcuteCommand("RequestMnl_DayZdzLine", null, false);
                                    Mnl_DayZdzLine mnl_DayZdzLine = new Mnl_DayZdzLine();
                                    mnl_DayZdzLine.Show();
                                }
                            }
                            catch (Exception ex)
                            {
                                OprFuction.SaveErrorLogs("模拟量小时曲线", ex);
                            }
                            break;
                        case "开关量状态图显示":
                            try
                            {
                                string point = point_Now;
                                if (!string.IsNullOrEmpty(point))
                                {
                                    Jc_DefInfo tempjc_defInfo = jc_defInfoList.Find(a => a.Point == point);
                                    if (tempjc_defInfo.DevPropertyID == 2)
                                    {
                                        point = tempjc_defInfo.PointID;
                                        point1.Add("PointID", point);
                                        //RequestUtil.ExcuteCommand("RequestKgl_StateLine", point1, false);
                                        Kgl_StateLine kgl_StateLine = new Kgl_StateLine(point1);
                                        kgl_StateLine.Show();
                                    }
                                    else
                                    {
                                        OprFuction.MessageBoxShow(0, "请选择开关量测点");
                                    }
                                }
                                else
                                {
                                    //RequestUtil.ExcuteCommand("RequestKgl_StateLine", null, false);
                                    Kgl_StateLine kgl_StateLine = new Kgl_StateLine();
                                    kgl_StateLine.Show();
                                }
                            }
                            catch (Exception ex)
                            {
                                OprFuction.SaveErrorLogs("开关量状态图", ex);
                            }
                            break;
                        case "开关量柱状图显示":
                            try
                            {
                                string point = point_Now;
                                if (!string.IsNullOrEmpty(point))
                                {
                                    Jc_DefInfo tempjc_defInfo = jc_defInfoList.Find(a => a.Point == point);
                                    if (tempjc_defInfo.DevPropertyID == 2)
                                    {
                                        point = tempjc_defInfo.PointID;
                                        point1.Add("PointID", point);
                                        //RequestUtil.ExcuteCommand("RequestKgl_StateBar", point1, false);
                                        Kgl_StateBar kgl_StateBar = new Kgl_StateBar(point1);
                                        kgl_StateBar.Show();
                                    }
                                    else
                                    {
                                        OprFuction.MessageBoxShow(0, "请选择开关量测点");
                                    }
                                }
                                else
                                {
                                    //RequestUtil.ExcuteCommand("RequestKgl_StateBar", null, false);
                                    Kgl_StateBar kgl_StateBar = new Kgl_StateBar();
                                    kgl_StateBar.Show();
                                }
                            }
                            catch (Exception ex)
                            {
                                OprFuction.SaveErrorLogs("开关量柱状图", ex);
                            }
                            break;
                        case "模拟量密采记录查询":
                            try
                            {
                                string point = point_Now;
                                if (!string.IsNullOrEmpty(point))
                                {
                                    Jc_DefInfo tempjc_defInfo = jc_defInfoList.Find(a => a.Point == point);
                                    if (tempjc_defInfo.DevPropertyID == 1)
                                    {
                                        point1.Add("SourceIsList", "true");
                                        point1.Add("Key_viewmcrunlogreport1_point", "等于&&$" + point);
                                        point1.Add("Display_viewmcrunlogreport1_point", "等于&&$" + point);
                                    }
                                    else
                                    {
                                        OprFuction.MessageBoxShow(0, "请选择模拟量测点");
                                        return;
                                    }
                                }
                                point1.Add("ListID", "28");
                                //RequestUtil.ExcuteCommand("RequestMCRungLogReport", point1, false);
                                Sys.Safety.Reports.frmList listReport = new Sys.Safety.Reports.frmList(point1);
                                listReport.Show();
                            }
                            catch (Exception ex)
                            {
                                OprFuction.SaveErrorLogs("模拟量密采记录", ex);
                            }
                            break;
                        case "开关量状态变化查询":
                            try
                            {
                                string point = point_Now;
                                if (!string.IsNullOrEmpty(point))
                                {
                                    Jc_DefInfo tempjc_defInfo = jc_defInfoList.Find(a => a.Point == point);
                                    if (tempjc_defInfo.DevPropertyID == 2)
                                    {
                                        //point = tempjc_defInfo.PointID;
                                        point1.Add("SourceIsList", "true");
                                        point1.Add("Key_ViewJC_KGStateMonth1_point", "等于&&$" + point);
                                        point1.Add("Display_ViewJC_KGStateMonth1_point", "等于&&$" + point);
                                    }
                                    else
                                    {
                                        OprFuction.MessageBoxShow(0, "请选择开关量测点");
                                        return;
                                    }
                                }
                                point1.Add("ListID", "17");
                                //RequestUtil.ExcuteCommand("RequestKGLStateRBReport", point1, false);
                                Sys.Safety.Reports.frmList listReport = new Sys.Safety.Reports.frmList(point1);
                                listReport.Show();
                            }
                            catch (Exception ex)
                            {
                                OprFuction.SaveErrorLogs("开关量状态变动记录", ex);
                            }
                            break;
                        case "模开同屏曲线":
                            try
                            {
                                //RequestUtil.ExcuteCommand("RequestMnlAndKgl_LineWithScreen", null, false);
                                MnlAndKgl_LineWithScreen mnlAndKgl_LineWithScreen = new MnlAndKgl_LineWithScreen();
                                mnlAndKgl_LineWithScreen.Show();
                            }
                            catch (Exception ex)
                            {
                                OprFuction.SaveErrorLogs("调用模开同屏曲线异常", ex);
                            }
                            break;
                    }
                    #endregion
                    break;
                case "LoadBgSVG":
                    GraphOpt.OpenSVG(mx, GraphOpt.GraphNameNow);
                    break;
            }
        }
        /// <summary>
        /// 窗体关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GISPlatformCenter_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!GraphOpt.IsGraphicEditSave)
            {
                if (DevExpress.XtraEditors.XtraMessageBox.Show("当前图形未保存，确定要退出吗?", "提示", MessageBoxButtons.YesNo) == DialogResult.No)
                {
                    e.Cancel = true;
                }
                else
                {
                    ExitGISPlatform();
                }
            }
            else
            {
                ExitGISPlatform();
            }
        }
        private void ExitGISPlatform()
        {
            if (GraphOpt.refPointssz._isRun)//如果实时刷新线程正在运行，退出实时刷新线程
            {
                GraphOpt.refPointssz.Stop();
            }
            wdf = new WaitDialogForm("正在退出...", "请等待...");
            Thread.Sleep(1000);

            if (wdf != null)
            {
                wdf.Close();
            }
        }

        /// <summary>
        /// 打开图形
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bbiShowPreview_ItemClick(object sender, ItemClickEventArgs e)
        {
            barStaticItem1.Caption = "打开图形";
            if (!GraphOpt.IsGraphicEdit)
            {
                if (DevExpress.XtraEditors.XtraMessageBox.Show("正在预览，要停止吗?", "提示", MessageBoxButtons.YesNo) == DialogResult.No)
                {
                    return;
                }
            }

            GraphOpt.IsGraphicEdit = true;

            GraphicsOpen GraphicsOpen = new GraphicsOpen();
            GraphicsOpen.ShowDialog();
            GraphOpt.IsTopologyInit = false;
        }
        /// <summary>
        /// 缩小图形
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void barButtonItem5_ItemClick(object sender, ItemClickEventArgs e)
        {
            barStaticItem1.Caption = "缩小图形";
            GraphOpt.ZoomIn(mx, "1");
        }
        /// <summary>
        /// 放大图形
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void barButtonItem4_ItemClick(object sender, ItemClickEventArgs e)
        {
            barStaticItem1.Caption = "放大图形";
            GraphOpt.ZoomOut(mx, "1");
        }
        /// <summary>
        /// 图形缩放
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void barButtonItem6_ItemClick(object sender, ItemClickEventArgs e)
        {
            barStaticItem1.Caption = "图形缩放";
            GraphOpt.zoomExtent(mx);
        }
        /// <summary>
        /// 新建图形
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void barButtonItem2_ItemClick(object sender, ItemClickEventArgs e)
        {
            barStaticItem1.Caption = "新建图形";
            if (!GraphOpt.IsGraphicEdit)
            {
                if (DevExpress.XtraEditors.XtraMessageBox.Show("正在预览，要停止吗?", "提示", MessageBoxButtons.YesNo) == DialogResult.No)
                {
                    return;
                }
            }

            GraphOpt.IsGraphicEdit = true;


            GraphicsAdd graphAdd = new GraphicsAdd();
            graphAdd.ShowDialog();
        }
        /// <summary>
        /// 图形编辑
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void barButtonItem8_ItemClick(object sender, ItemClickEventArgs e)
        {
            barStaticItem1.Caption = "图形编辑";
            var TypeNow = GraphOpt.getGraphicNowType();
            if (TypeNow >= 0)
            {
                GraphicsAdd graphAdd = new GraphicsAdd();
                graphAdd.Text = "图形编辑";
                graphAdd.ShowDialog();
            }
            else if (GraphOpt.getGraphicNowType() == -1)
            {
                //DevExpress.XtraEditors.XtraMessageBox.Show("当前未打开图形，请先打开图形！");
                barStaticItem1.Caption = "当前未打开图形，请先打开图形！";
            }

        }
        /// <summary>
        /// 刷新图形
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void barButtonItem3_ItemClick(object sender, ItemClickEventArgs e)
        {
            barStaticItem1.Caption = "图形刷新";
            LoadMap(GraphOpt.GraphNameNow);
        }
        /// <summary>
        /// 删除图形
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void barButtonItem9_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (DevExpress.XtraEditors.XtraMessageBox.Show("删除图形后将不能恢复，确定要删除吗?", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                try
                {
                    barStaticItem1.Caption = "删除图形";
                    if (GraphOpt.GraphNameNow.Length > 0)
                    {

                        //数据库删除
                        GraphOpt.GraphDelete(GraphOpt.GraphNameNow);
                        //停止实时刷新线程
                        if (GraphOpt.refPointssz._isRun)
                        {
                            GraphOpt.refPointssz.Stop();//停止实时刷新线程
                        }

                        if (GraphOpt.GraphNameNow.Contains(".dwg") || GraphOpt.GraphNameNow.Contains(".dxf"))
                        {
                            //删除本地文件
                            var dwgFileName = Application.StartupPath + "\\mx\\dwg\\" + GraphOpt.GraphNameNow;
                            File.Delete(dwgFileName);
                        }

                        //重新加载页面跳转
                        LoadPageChg();

                        LoadMap("");
                        GraphOpt.GraphNameNow = "";
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Error("barButtonItem9_ItemClick" + ex.Message + ex.StackTrace);
                }
            }
        }
        /// <summary>
        /// 保存图形
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void barButtonItem1_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                barStaticItem1.Caption = "保存图形";
                if (GraphOpt.getGraphicNowType() != -1)
                {
                    GraphOpt.DoSaveAllPoints(mx);
                }
                else
                {
                    //DevExpress.XtraEditors.XtraMessageBox.Show("当前未打开图形，请先打开图形！");
                    barStaticItem1.Caption = "当前未打开图形，请先打开图形！";
                }
                GraphOpt.IsTopologyInit = false;
            }
            catch (Exception ex)
            {
                LogHelper.Error("barButtonItem1_ItemClick" + ex.Message + ex.StackTrace);
            }
        }
        /// <summary>
        /// 图层显示/隐藏
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void barButtonItem22_ItemClick(object sender, ItemClickEventArgs e)
        {
            barStaticItem1.Caption = "图层显示/隐藏";
            LayerDisHid layerdis = new LayerDisHid();
            layerdis.Show();
        }
        /// <summary>
        /// 连S型线
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void barButtonItem7_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                barStaticItem1.Caption = "连S型线";
                //只有拓扑图才支持此命令
                if (GraphOpt.getGraphicNowType() == 1)
                {
                    GraphOpt.DoMapTopologyTransDef(mx, 0);
                }
                else
                {
                    //DevExpress.XtraEditors.XtraMessageBox.Show("此操作只支持拓扑图！");
                    barStaticItem1.Caption = "此操作只支持拓扑图！";
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("barButtonItem7_ItemClick" + ex.Message + ex.StackTrace);
            }
        }
        /// <summary>
        /// 连L型线
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void barButtonItem25_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                barStaticItem1.Caption = "连L型线";
                //只有拓扑图才支持此命令
                if (GraphOpt.getGraphicNowType() == 1)
                {
                    GraphOpt.DoMapTopologyTransDef(mx, 1);
                }
                else
                {
                    //DevExpress.XtraEditors.XtraMessageBox.Show("此操作只支持拓扑图！");
                    barStaticItem1.Caption = "此操作只支持拓扑图！";
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("barButtonItem25_ItemClick" + ex.Message + ex.StackTrace);
            }
        }
        /// <summary>
        /// 结束连线命令
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void barButtonItem26_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                barStaticItem1.Caption = "结束拓扑连线";
                //只有拓扑图才支持此命令
                if (GraphOpt.getGraphicNowType() == 1)
                {
                    GraphOpt.EndMapTopologyTransDef(mx);
                }
                else
                {
                    //DevExpress.XtraEditors.XtraMessageBox.Show("此操作只支持拓扑图！");
                    barStaticItem1.Caption = "此操作只支持拓扑图！";
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("barButtonItem26_ItemClick" + ex.Message + ex.StackTrace);
            }
        }
        /// <summary>
        /// 巷道连接命令
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void barButtonItem27_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                barStaticItem1.Caption = "巷道连线";
                //只有动态图才支持
                if (GraphOpt.getGraphicNowType() == 0)
                {
                    GraphOpt.DoMapTrajectoryDef(mx);
                }
                else
                {
                    //DevExpress.XtraEditors.XtraMessageBox.Show("此操作只支持动态图！");
                    barStaticItem1.Caption = "此操作只支持拓扑图！";
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("barButtonItem27_ItemClick" + ex.Message + ex.StackTrace);
            }
        }
        /// <summary>
        /// 实时刷新设备状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void barButtonItem28_ItemClick_1(object sender, ItemClickEventArgs e)
        {
            try
            {
                barStaticItem1.Caption = "图形预览";
                if (!GraphOpt.IsGraphicEditSave)
                {
                    //DevExpress.XtraEditors.XtraMessageBox.Show("图形尚未保存，请先保存再进行图形预览！");
                    barStaticItem1.Caption = "图形尚未保存，请先保存再进行图形预览！";
                    return;
                }

                GraphOpt.IsGraphicEdit = false;
                LoadMap(GraphOpt.GraphNameNow);
            }
            catch (Exception ex)
            {
                LogHelper.Error("barButtonItem28_ItemClick_1" + ex.Message + ex.StackTrace);
            }
        }
        /// <summary>
        /// 返回编辑状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void barButtonItem29_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                barStaticItem1.Caption = "结束预览";
                GraphOpt.IsGraphicEdit = true;
                LoadMap(GraphOpt.GraphNameNow);
            }
            catch (Exception ex)
            {
                LogHelper.Error("barButtonItem29_ItemClick" + ex.Message + ex.StackTrace);
            }
        }

        /// <summary>
        /// 设备显示隐藏
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void barButtonItem23_ItemClick(object sender, ItemClickEventArgs e)
        {
            barStaticItem1.Caption = "设备显示/隐藏";
            PointDisHid pointdishid = new PointDisHid();
            pointdishid.Show();
        }
        /// <summary>
        /// 根据定义信息自动生成拓扑图
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void barButtonItem31_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                barStaticItem1.Caption = "自动生成拓扑图";
                if (!GraphOpt.IsGraphicEdit)
                {
                    //DevExpress.XtraEditors.XtraMessageBox.Show("当前图形正在预览，请先停止预览！");
                    barStaticItem1.Caption = "当前图形正在预览，请先停止预览！";
                    return;
                }

                //只有拓扑图才支持此命令
                if (GraphOpt.getGraphicNowType() == 1)
                {
                    GraphOpt.AutoDragTopologyTrans(mx);
                }
                else
                {
                    //DevExpress.XtraEditors.XtraMessageBox.Show("此操作只支持拓扑图！");
                    barStaticItem1.Caption = "此操作只支持拓扑图！";
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("barButtonItem31_ItemClick" + ex.Message + ex.StackTrace);
            }
        }
        /// <summary>
        /// 结束巷道连线命令
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void barButtonItem32_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                barStaticItem1.Caption = "结束巷道连线";
                //只有动态图才支持
                if (GraphOpt.getGraphicNowType() == 0)
                {
                    GraphOpt.EndMapTrajectoryDef(mx);
                }
                else
                {
                    //DevExpress.XtraEditors.XtraMessageBox.Show("此操作只支持动态图！");
                    barStaticItem1.Caption = "此操作只支持拓扑图！";
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("barButtonItem32_ItemClick" + ex.Message + ex.StackTrace);
            }
        }
        /// <summary>
        /// 打印图形
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void barButtonItem11_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                barStaticItem1.Caption = "打印图形";
                GraphOpt.mapPrint(mx);
            }
            catch (Exception ex)
            {
                LogHelper.Error("barButtonItem11_ItemClick" + ex.Message + ex.StackTrace);
            }
        }
        /// <summary>
        /// 导出图片
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void barButtonItem20_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                barStaticItem1.Caption = "导出图片";
                saveFileDialog1.FileName = GraphOpt.GraphNameNow;
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    GraphOpt.mapToImage(mx, saveFileDialog1.FileName);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("barButtonItem20_ItemClick" + ex.Message + ex.StackTrace);
            }
        }
        /// <summary>
        /// 切换图形
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void barButtonItem21_ItemClick(object sender, ItemClickEventArgs e)
        {
            //if (!GraphOpt.IsGraphicEdit)
            //{
            //    if (DevExpress.XtraEditors.XtraMessageBox.Show("正在预览，要停止吗?", "提示", MessageBoxButtons.YesNo) == DialogResult.No)
            //    {
            //        return;
            //    }
            //}

            //GraphOpt.IsGraphicEdit = true;
            //isExit = true;

            GraphicsOpen GraphicsOpen = new GraphicsOpen();
            GraphicsOpen.ShowDialog();
            GraphOpt.IsTopologyInit = false;
        }
        /// <summary>
        /// 关闭事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GISPlatformCenter_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {

                //更新服务端标记
                var request = new SetSaveFlagRequest() { Flag = true };
                var response = graphicsbaseinfService.SetSaveFlag(request);

                _isRun = false;
                //释放控件
                //mx.Dispose();
                //释放控件
                navBarGroupControlContainer4.Dispose();
                navBarGroupControlContainer4 = null;
            }
            catch (Exception ex)
            {
                _isRun = false;
                //释放控件
                //mx.Dispose();
                //释放控件
                navBarGroupControlContainer4.Dispose();
                navBarGroupControlContainer4 = null;

                LogHelper.Error("GISPlatformCenter_GISPlatformCenter_FormClosed" + ex.Message + ex.StackTrace);
            }
        }
        /// <summary>
        /// 设备定位
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void barButtonItem10_ItemClick(object sender, ItemClickEventArgs e)
        {
            barStaticItem1.Caption = "设备查找";
            PointSerach pointsercah = new PointSerach();
            pointsercah.ShowDialog();
        }

        /// <summary>
        /// 皮肤显示中文转换
        /// </summary>
        /// <param name="skinName"></param>
        /// <returns></returns>
        public string SkinToText(string skinName)
        {
            string rvalue = "";
            switch (skinName)
            {
                case "DevExpress Style":
                    rvalue = "灰色风格";
                    break;
                case "DevExpress Dark Style":
                    rvalue = "夜间深色风格";
                    break;
                case "VS2010":
                    rvalue = "深蓝色风格";
                    break;
                case "Seven Classic":
                    rvalue = "win7风格";
                    break;
                case "Office 2010 Blue":
                    rvalue = "蓝色风格(默认)";
                    break;
                case "Office 2010 Black":
                    rvalue = "黑色风格";
                    break;
                case "Office 2010 Silver":
                    rvalue = "银光白风格";
                    break;
                case "Office 2013":
                    rvalue = "蓝白组合风格";
                    break;
                case "Office 2013 Dark Gray":
                    rvalue = "灰黑色风格";
                    break;
                case "Office 2013 Light Gray":
                    rvalue = "白灰色风格";
                    break;
                case "Visual Studio 2013 Blue":
                    rvalue = "深蓝灰组合风格";
                    break;
                case "Visual Studio 2013 Light":
                    rvalue = "深蓝白组合风格";
                    break;
                case "Visual Studio 2013 Dark":
                    rvalue = "黑水晶风格";
                    break;
                default:
                    rvalue = "蓝色风格(默认)";
                    break;
            }
            return rvalue;
        }
        /// <summary>
        /// 中文显示转皮肤
        /// </summary>
        /// <param name="TextName"></param>
        /// <returns></returns>
        public string TextToSkin(string TextName)
        {
            string rvalue = "";
            switch (TextName)
            {
                case "灰色风格":
                    rvalue = "DevExpress Style";
                    break;
                case "夜间深色风格":
                    rvalue = "DevExpress Dark Style";
                    break;
                case "深蓝色风格":
                    rvalue = "VS2010";
                    break;
                case "win7风格":
                    rvalue = "Seven Classic";
                    break;
                case "蓝色风格(默认)":
                    rvalue = "Visual Studio 2013 Blue";
                    break;
                case "黑色风格":
                    rvalue = "Office 2010 Black";
                    break;
                case "银光白风格":
                    rvalue = "Office 2010 Silver";
                    break;
                case "蓝白组合风格":
                    rvalue = "Office 2013";
                    break;
                case "灰黑色风格":
                    rvalue = "Office 2013 Dark Gray";
                    break;
                case "白灰色风格":
                    rvalue = "Office 2013 Light Gray";
                    break;
                case "深蓝灰组合风格":
                    rvalue = "Visual Studio 2013 Blue";
                    break;
                case "深蓝白组合风格":
                    rvalue = "Visual Studio 2013 Light";
                    break;
                case "黑水晶风格":
                    rvalue = "Visual Studio 2013 Dark";
                    break;
                default:
                    rvalue = "Visual Studio 2013 Blue";
                    break;
            }
            return rvalue;

        }
        /// <summary>
        /// 根据图形更新标记，刷新图形(此方法调用IGraphicsbaseinfService服务，需将服务独立，不然会与线程外的其它调用冲突)
        /// </summary>
        public void RefMapData()
        {
            while (_isRun)
            {

                try
                {
                    //如果编辑工具改变了图形，则重新加载图形
                    var response = graphicsbaseinfService.GetSaveFlag();
                    if (response.Data)
                    {
                        #region 从数据库读取所有图形文件
                        GraphOpt.LoadGraphicsInfo1();
                        #endregion

                        #region//加载页面跳转
                        LoadPageChg1();
                        #endregion

                        if (!string.IsNullOrEmpty(GraphOpt.GraphNameNow))
                        {
                            LoadMap(GraphOpt.GraphNameNow);
                        }

                        var setRequest = new SetSaveFlagRequest() { Flag = false };
                        var setResponse = graphicsbaseinfService.SetSaveFlag(setRequest);
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Error("GISPlatformCenter_RefMapData" + ex.Message + ex.StackTrace);
                }
                Thread.Sleep(3000);
            }
        }
        private void barButtonItem12_ItemClick(object sender, ItemClickEventArgs e)
        {
            GraphDrawing drawFrom = new GraphDrawing();
            drawFrom.Show();
        }

        private void barButtonItem13_ItemClick(object sender, ItemClickEventArgs e)
        {
            string str = "[{\"pointX\":1825.377736,\"pointY\":1679.765708,\"radius\":1021.3509371813967}]|circle";
            GraphDrawingRefresh refresh = new GraphDrawingRefresh(str);
            refresh.Show();
        }

        private void barButtonItem14_ItemClick(object sender, ItemClickEventArgs e)
        {
            string str = "[{\"pointX\":1594.279622,\"pointY\":2204.250099},{\"pointX\":2926.704685,\"pointY\":2453.402753},{\"pointX\":3724.71536,\"pointY\":1781.773859},{\"pointX\":2247.853976,\"pointY\":662.39237},{\"pointX\":2341.737585,\"pointY\":662.39237},{\"pointX\":2237.021252,\"pointY\":1088.479518}]|polygon";
            GraphDrawingRefresh refresh = new GraphDrawingRefresh(str);
            refresh.Show();
        }

        private void barButtonItem15_ItemClick(object sender, ItemClickEventArgs e)
        {
            string str = "[{\"pointX\":1825.377736,\"pointY\":1679.765708,\"radius\":1021.3509371813967}]|circle";
            GraphDrawing refresh = new GraphDrawing(str);
            refresh.Show();
        }

        private void barButtonItem16_ItemClick(object sender, ItemClickEventArgs e)
        {
            string str = "[{\"pointX\":1594.279622,\"pointY\":2204.250099},{\"pointX\":2926.704685,\"pointY\":2453.402753},{\"pointX\":3724.71536,\"pointY\":1781.773859},{\"pointX\":2247.853976,\"pointY\":662.39237},{\"pointX\":2341.737585,\"pointY\":662.39237},{\"pointX\":2237.021252,\"pointY\":1088.479518}]|polygon";
            GraphDrawing refresh = new GraphDrawing(str);
            refresh.Show();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string str = "[{\"pointX\":1594.279622,\"pointY\":2204.250099},{\"pointX\":2926.704685,\"pointY\":2453.402753},{\"pointX\":3724.71536,\"pointY\":1781.773859},{\"pointX\":2247.853976,\"pointY\":662.39237},{\"pointX\":2341.737585,\"pointY\":662.39237},{\"pointX\":2237.021252,\"pointY\":1088.479518}]|polygon";
            GraphDrawingRefresh refresh = new GraphDrawingRefresh(str);
            refresh.Show();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            string str = "[{\"pointX\":1594.279622,\"pointY\":2204.250099},{\"pointX\":2926.704685,\"pointY\":2453.402753},{\"pointX\":3724.71536,\"pointY\":1781.773859},{\"pointX\":2247.853976,\"pointY\":662.39237},{\"pointX\":2341.737585,\"pointY\":662.39237},{\"pointX\":2237.021252,\"pointY\":1088.479518}]|polygon";
            GraphDrawingRefresh refresh = new GraphDrawingRefresh(str);
            refresh.Show();
        }
        /// <summary>
        /// 测量命令
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void barButtonItem12_ItemClick_1(object sender, ItemClickEventArgs e)
        {
            try
            {
                barStaticItem1.Caption = "测量命令";
                GraphOpt.MeasureCommand(mx);
            }
            catch (Exception ex)
            {
                LogHelper.Error("barButtonItem12_ItemClick_1" + ex.Message + ex.StackTrace);
            }
        }

        private void navBarControl2_Resize(object sender, EventArgs e)
        {
            mx.Width = this.Width - navBarControl2.Width - 20;
        }

    }
}
