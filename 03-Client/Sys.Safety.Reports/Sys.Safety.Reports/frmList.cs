using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using DevExpress.Data;
using DevExpress.Utils;
using DevExpress.XtraBars;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Design;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraLayout.Utils;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports.UI;
using Basic.Framework.Logging;
using Basic.Framework.Web;
using Sys.Safety.DataContract;
using Sys.Safety.Reports.Controls;
using Sys.Safety.Reports.Model;
using Sys.Safety.Reports.PubClass;
using BorderSide = DevExpress.XtraPrinting.BorderSide;
using Sys.Safety.DataContract.Custom;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using Font = System.Drawing.Font;
using NPOI.HSSF.Util;
using NPOI.SS.Util;
using Sys.Safety.ClientFramework.CBFCommon;
using Sys.Safety.ServiceContract;
using Basic.Framework.Service;

namespace Sys.Safety.Reports
{
    public partial class frmList : XtraForm
    {
        private static readonly ResourceManager res = new ResourceManager(typeof(frmList));
        //2015-10-26   ���ڴ洢ÿ����ѯ�������õı���
        private IDictionary<string, string> _dicConditionOldTable = new Dictionary<string, string>();
        private FreQryCondition _freQryCondition;
        private List<string> _listdate; //�ձ����ڴ�
        private DataTable _lmdDt; //��ǰ����ѡ����
        private string _strFreQryCondition = string.Empty;
        private string _strFreQryConditionByChs = "";
        private readonly string _strReceiveParaCondition = string.Empty;
        private string _strSortCondtion = string.Empty; //2016-10-18 ,�洢������
        private bool blnFirstOpenLoadData;
        private bool blnListFreCondition;
        private bool blnRefreshed;
        private int CurrentPageNumber;
        private readonly IDictionary<string, string> DicBluerCondition = new Dictionary<string, string>();
        private IDictionary<string, string> DicReceivePara = new Dictionary<string, string>();
        private BarButtonItem DoubleButtonItem;
        private DataTable dt;
        private DateTime executeBeginTime = DateTime.MinValue; //���ڼ�¼���ֲ���ִ�п�ʼʱ��
        private string extendEntity = "";
        private readonly IList<RepositoryItemHyperLinkEdit> hypers = new List<RepositoryItemHyperLinkEdit>();
        private IDictionary<int, bool> IsHaveFieldRightDic;
        private ListdataexInfo listDataExVo;
        public IList<ListdisplayexInfo> listDisplayExList;
        private ListexInfo listExvo;
        private int listId;

        private IList<ListdatalayountInfo> listlayount;
        private ListtempleInfo listTemplevo;
        private DataTable metadataFieldDt;
        private readonly ListExModel Model = new ListExModel();
        private int PerPageRecord;
        private string realEntity = "";
        private ResourceManager rm = new ResourceManager(typeof(frmList));
        private string strListStyle = "";

        private string strOldStrListSql = "";
        private string strRight = "";
        private string strSortFileName = "";
        private string strSortText = "";
        private int TotalPage;
        private int TotalRecord;
        private int userId;
        private string voType = null;
        private WaitDialogForm wdf;
        private XAppearances xapp;

        IConfigService _configService = ServiceFactory.Create<IConfigService>();

        /// <summary>
        ///     �б�Ĺ��췽��
        /// </summary>
        public frmList()
        {
            ListDataID = 0;
            InitializeComponent();
        }

        public frmList(int listid)
        {
            ListDataID = 0;
            InitializeComponent();
            listId = listid;
        }

        public frmList(int listid, IDictionary<string, string> dicReceivePara)
        {
            ListDataID = 0;
            InitializeComponent();
            listId = listid;
            DicReceivePara = dicReceivePara;
        }

        public frmList(Dictionary<string, string> dicMeun)
        {
            ListDataID = 0;
            InitializeComponent();
            listId = TypeUtil.ToInt(dicMeun["ListID"]);
            if (dicMeun.Count >= 4) //2015-08-07   ������ݵ����ĸ�����������Ϊ�Ǵ�ʵʱ�������
                DicReceivePara = dicMeun;
        }

        public void Reload(Dictionary<string, string> dicMeun)
        {
            ListDataID = 0;
            listId = TypeUtil.ToInt(dicMeun["ListID"]);
            if (dicMeun.Count >= 4)
                DicReceivePara = dicMeun;

            //object sender1 = null;
            //var e1 = new EventArgs();
            //frmList_Load(sender1, e1);
        }

        /// <summary>
        ///     ���û��ȡ�б��ListDataID��
        /// </summary>
        public int ListDataID { get; set; }

        private void frmList_Load(object sender, EventArgs e)
        {
            try
            {
                //��Ȩ���
                RunningInfo runinfo = _configService.GetRunningInfo().Data;
                if (runinfo.AuthorizationExpires)
                {
                    XtraMessageBox.Show("��⵽������Ȩ�ѵ��ڣ��뼰ʱ��ϵ����Ա������Ȩ��");
                    //return;
                }


                OpenWaitDialog("�б��������ڼ�����");

                //���ô���߶ȺͿ��
                Width = Convert.ToInt32(Screen.GetWorkingArea(this).Width * 0.9);
                Height = Convert.ToInt32(Screen.GetWorkingArea(this).Height * 0.9);
                Left = Convert.ToInt32(Screen.GetWorkingArea(this).Width * 0.1 / 2);
                Top = Convert.ToInt32(Screen.GetWorkingArea(this).Height * 0.1 / 2);

                //���������  20180405
                layoutControlItem1.Width = 300;
                layoutControlItem2.Width = 300;
                layoutItemGrid.Height = Height - 110;

                SetExecuteBeginTime();

                GetSysSetting(); //��ȡϵͳ����      
                GetListEx(); //��ȡ�б�ͷ


                SetTitleInfo(); //���ñ�����Ϣ 
                InitListConfig(); //��ȡ����
                GetListDataEx(); //��ȡ�б�������
                GetListDisplay(); //��ȡ�б���ʾ����
                SetListFormat(); //���ݷ��������б��ʽ   
                ReceiveParaCondition(); //��ȡ���ղ���
                SetControlVisible();
                if (blnFirstOpenLoadData)
                    RefreshListData();
                buttonContent.TabIndex = 0; //���б�󽹵��Զ�����ѯ��

                gridView.LayoutChanged();

                //��̬���ز�����layout
                var rows = metadataFieldDt.Select("MetaDataID=" + listExvo.MainMetaDataID + " and blnDesignSort=1");
                if (rows.Length == 0)
                {
                    layoutControlItem1.Visibility = LayoutVisibility.Never;
                }
                else
                {
                    //��ʼ�����ȥ�ظ�����
                    DataTable dtRemoveDuplication = new DataTable();
                    dtRemoveDuplication.Columns.Add("Num");
                    dtRemoveDuplication.Columns.Add("QcfItem");

                    var dr = dtRemoveDuplication.NewRow();
                    dr["Num"] = "1";
                    dr["QcfItem"] = "ȫ��";
                    dtRemoveDuplication.Rows.Add(dr);

                    var dr2 = dtRemoveDuplication.NewRow();
                    dr2["Num"] = "2";
                    dr2["QcfItem"] = "ȥ�ظ���ɾ��ǰ��";
                    dtRemoveDuplication.Rows.Add(dr2);

                    var dr3 = dtRemoveDuplication.NewRow();
                    dr3["Num"] = "3";
                    dr3["QcfItem"] = "ȥ�ظ���ɾ����";
                    dtRemoveDuplication.Rows.Add(dr3);

                    lookUpEditRemoveDuplication.Properties.DataSource = dtRemoveDuplication;
                    lookUpEditRemoveDuplication.EditValue = "1";

                    InitializeArrangeTime();

                    //��ʼ�����Ż
                    var bindData = new List<Kvp>
                    {
                        new Kvp {Id = "1", Text = "����"},
                        new Kvp {Id = "2", Text = "�洢���"}
                    };
                    lookUpEditArrangeActivity.Properties.DataSource = bindData;
                    lookUpEditArrangeActivity.EditValue = "1";

                    layoutControlItem1.Visibility = LayoutVisibility.Always;
                }

                frmListEx_SizeChanged(this, null);

                //ǰһ���һ����ʾ����
                var controls = panelFreQry.Controls;
                bool existQueryTime = false;
                foreach (Control item in controls)
                {
                    // 20180408
                    var type = item.GetType();
                    var typeName = type.Name;
                    if (typeName.Contains("DateTime") && !typeName.Contains("Year") && !typeName.Contains("Month"))
                    {
                        existQueryTime = true;
                        break;
                    }

                    //var hash = item.Tag as Hashtable;
                    //if (hash == null)
                    //{
                    //    continue;
                    //}

                    //if (!hash.ContainsKey("_fieldName"))
                    //{
                    //    continue;
                    //}

                    //var fieldName = hash["_fieldName"].ToString();
                    //var fieldNameSplit = fieldName.Split('_');
                    //var split2 = fieldNameSplit[fieldNameSplit.Length - 1].ToLower();
                    //if (split2 != "datsearch")
                    //{
                    //    continue;
                    //}

                    //existQueryTime = true;
                    //break;
                }

                if (existQueryTime)
                {
                    BeforeDay.Visibility = BarItemVisibility.Always;
                    AfterDay.Visibility = BarItemVisibility.Always;
                    simpleButton1.Visible = true;
                    simpleButton2.Visible = true;
                }
                else
                {
                    BeforeDay.Visibility = BarItemVisibility.Never;
                    AfterDay.Visibility = BarItemVisibility.Never;
                    simpleButton1.Visible = false;
                    simpleButton2.Visible = false;
                }



                MessageShowUtil.ShowStaticInfo("���سɹ�������ʱ��Ϊ" + GetExecuteTimeString(), barStaticItemMsg);
            }
            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
            finally
            {
                CloseWaitDialog();
            }
        }

        /// <summary>
        /// //��ʼ������ʱ��
        /// </summary>
        private void InitializeArrangeTime()
        {
            listlayount = Model.GetListDataLayountData(ListDataID);
            //listlayount = listlayount.OrderByDescending(a => a.StrDate).ToList();
            lookUpEditArrangeTime.Properties.DataSource = listlayount;
            lookUpEditArrangeTime.Properties.DisplayMember = "StrDate";  // ָ����ʾ�ֶ�
            lookUpEditArrangeTime.Properties.ValueMember = "ListDataLayoutID";    // ָ��ֵ�ֶ�
            if (listlayount.Count != 0)
            {
                lookUpEditArrangeTime.EditValue = listlayount[0].ListDataLayoutID;
            }
        }

        /// <summary>
        ///     ����ִ�п�ʼʱ��
        /// </summary>
        protected void SetExecuteBeginTime()
        {
            executeBeginTime = DateTime.Now;
        }

        /// <summary>
        ///     ��ȡִ��ʱ���ַ���
        /// </summary>
        /// <returns></returns>
        protected string GetExecuteTimeString()
        {
            var strTime = string.Empty;
            var ts = DateTime.Now - executeBeginTime;
            if (ts.Minutes > 0)
                strTime += ts.Minutes + "��";
            if (ts.Seconds > 0)
                strTime += ts.Seconds + "��";
            if (ts.Milliseconds > 0)
                strTime += ts.Milliseconds + "����";

            strTime += "��";

            return strTime;
        }

        /// <summary>
        ///     ��ȡ���ղ���
        /// </summary>
        private void ReceiveParaCondition()
        {
            if (_lmdDt == null)
                _lmdDt = Model.GetListMetaData(ListDataID, listDataExVo.UserID);
            //if (!TypeUtil.ToBool(RequestUtil.GetParameterValue("SourceIsList")))
            //{
            //    return;
            //}
            if ((DicReceivePara == null) || !DicReceivePara.ContainsKey("SourceIsList") ||
                !TypeUtil.ToBool(DicReceivePara["SourceIsList"]))
                return;
            if (_lmdDt != null)
            {
                var count = _lmdDt.Rows.Count;
                var strFieldName = "";
                var strValue = "";
                var strDisplay = "";
                DataRow curDr = null;
                for (var i = 0; i < count; i++)
                {
                    curDr = _lmdDt.Rows[i];
                    if (!TypeUtil.ToBool(curDr["blnReceivePara"]))
                        continue;


                    strFieldName = TypeUtil.ToString(curDr["MetaDataFieldName"]);

                    strValue = DicReceivePara.ContainsKey("Key_" + strFieldName)
                        ? TypeUtil.ToString(DicReceivePara["Key_" + strFieldName])
                        : "";
                    strDisplay = DicReceivePara.ContainsKey("Display_" + strFieldName)
                        ? TypeUtil.ToString(DicReceivePara["Display_" + strFieldName])
                        : "";
                    if ((strValue != string.Empty) && (strValue != null))
                    {
                        //2017-01-24 ������Ǵӳ����ӽ�������Ĭ��Ҫ�����б�����
                        blnFirstOpenLoadData = true;
                        //strReceivePara += strValue;
                        //strReceivePara += " and " + strFieldName.Replace("_", ".") + "='" + strValue + "'";
                        if (TypeUtil.ToBool(curDr["blnFreCondition"]))
                        {
                            //��������

                            curDr["strFreCondition"] = strValue;
                            curDr["strFreConditionCHS"] = strDisplay;
                            if (TypeUtil.ToString(curDr["strFkCode"]).Length > 0)
                            {
                                //2017-01-24   ����б����б��ʱ���ǲ������͵ģ���Ҫ��������������ID�Ѻ�����ʾ����
                                var blnNewData = false;
                                var lookup = LookUpUtil.GetlookInfo(TypeUtil.ToString(curDr["strFkCode"]),
                                    ref blnNewData);
                                var dtsource = lookup["dataSource"] as DataTable;
                                var strValueMember = TypeUtil.ToString(lookup["StrValueMember"]);
                                var strValueID = strValue.Substring(strValue.IndexOf("$") + 1);
                                var rowslookup = dtsource.Select(strValueMember + "='" + strValueID + "'");
                                if (rowslookup.Length > 0)
                                    curDr["strFreConditionCHS"] = rowslookup[0][lookup["StrDsiplayMember"].ToString()];
                            }
                        }
                        else
                        {
                            curDr["strCondition"] = strValue;
                            curDr["strConditionCHS"] = strDisplay;
                        }
                    }
                }

                var strListSQL = Model.GetSQL(_lmdDt);
                listDataExVo.StrListSQL = strListSQL;
                _freQryCondition.CreateControl(_lmdDt);
                //2015-10-27   �����ͨ���б����б�ʽ�����󣬼�����������Ҫ��������Ԫ������գ��������������б������������Ҳ����
                var rows = _lmdDt.Select("len(strFreCondition)>0 or len(strConditionCHS)>0");
                foreach (var row in rows)
                    row["strFreCondition"] = row["strFreConditionCHS"] = "";
                SetFrmQryConditionSize();
                if (_freQryCondition != null)
                    _strFreQryCondition = _freQryCondition.GetFreQryCondition();
            }

            //_strReceiveParaCondition = strReceivePara;
        }

        private void SetControlVisible()
        {
            PermissionManager.HavePermission("OpenReportScheme", tlbSchema);
            PermissionManager.HavePermission("SaveReportFreQryCon", tlbSaveFreQryCon);
            PermissionManager.HavePermission("SaveReportColumnWidth", tlbSaveWidth);
            //PermissionManager.HavePermission("ExportReportData", tlbExportExcel);
            //PermissionManager.HavePermission("ExportReportData", tlbExeclPDF);
            //PermissionManager.HavePermission("ExportReportData", txtExportTXT);
            //PermissionManager.HavePermission("ExportReportData", txtExportCSV);
            //PermissionManager.HavePermission("ExportReportData", txtExportHTML);
            //PermissionManager.HavePermission("PrintReport", tlbPrint);
            PermissionManager.HavePermission("OpenReportDesign", tlbPivotSetting);

            // 20170629
            //var RowsblnKeyWord = _lmdDt.Select("blnKeyWord=1");
            //if ((RowsblnKeyWord == null) || (RowsblnKeyWord.Length == 0))
            //    chkblnKeyWord.Visibility = BarItemVisibility.Never;
        }

        /// <summary>
        ///     ��ȡϵͳ����
        /// </summary>
        private void GetSysSetting()
        {
            userId = TypeUtil.ToInt(RequestUtil.GetParameterValue("userId"));

            var listRowCount = TypeUtil.ToInt(RequestUtil.GetParameterValue("ListRowCount"));
            blnFirstOpenLoadData = TypeUtil.ToBool(RequestUtil.GetParameterValue("blnFirstOpenLoadData"));

            // 20170623
            //blnListFreCondition = true; // TypeUtil.ToBool(RequestUtil.GetParameterValue("blnListFreCondition"));
            blnListFreCondition = TypeUtil.ToBool(RequestUtil.GetParameterValue("blnListFreCondition"));


            listRowCount = listRowCount > 0 ? listRowCount : 500;
            if (listRowCount > 0)
            {
                //tlbSetPerPageNumber.EditValueChanged -= tlbSetPerPageNumber_EditValueChanged;
                //tlbSetPerPageNumber.EditValue = listRowCount;
                //tlbSetPerPageNumber.EditValueChanged += tlbSetPerPageNumber_EditValueChanged;

                comboBoxEdit1.SelectedValueChanged -= tlbSetPerPageNumber_EditValueChanged;
                comboBoxEdit1.Text = listRowCount.ToString();
                comboBoxEdit1.EditValueChanged += tlbSetPerPageNumber_EditValueChanged;
            }
            if (blnFirstOpenLoadData)
                blnRefreshed = true;

            //2014-11-05 �жϲ���Ա�Ƿ���Ե��������ť����ҪȨ�޷����������ٵ��ô˷���
            //if (!PermissionManager.HavePermission("EditSchema"))
            //{
            //    this.tlbSchema.Visibility = BarItemVisibility.Never;
            //}
        }

        /// <summary>
        ///     ��ȡ�б�ʵ��
        /// </summary>
        private void GetListEx()
        {
            listExvo = Model.GetListEx(listId);
            if ((listExvo == null) || (listExvo.ListID <= 0))
                throw new Exception("�����б�δ�ҵ�!");


            metadataFieldDt = ClientCacheModel.GetServerMetaDataFields();
        }

        /// <summary>
        ///     ���ñ�����Ϣ
        /// </summary>
        private void SetTitleInfo()
        {
            lblTile.Text = listExvo.StrListName + "       ";
            if (Parent == null)
                Text = listExvo.StrListName;

            //if (this.Parent != null)
            //{//�������ر�������,�������ڵ������õ�tablepage��ʽ��������Ҫ��ֵ
            //    Control c = this.Parent;
            //    string str = c.GetType().Name;
            //    if (str.ToLower() == "xtratabpage")
            //        c.Text = this.Text;
            //}
        }

        /// <summary>
        ///     ��ʼ������б�
        ///     �б�ķ���
        ///     �����б���ʾ��ʽ����
        ///     �б�ķ��
        ///     �б�Ĺ�����
        /// </summary>
        private void InitListConfig()
        {
            InitGroupListToolbar();
            InitListSchema();
            SetListShowStyleData();
            InitListToolbarConfig();
        }

        /// <summary>
        ///     ��ʼ������б�
        /// </summary>
        private void InitGroupListToolbar()
        {
            if (string.IsNullOrEmpty(listExvo.StrListGroupCode))
            {
                tlbGroupList.Visibility = BarItemVisibility.Never;
                return;
            }

            var strArr = listExvo.StrListGroupCode.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var strSql = "";
            foreach (var str in strArr)
                if (strSql == "")
                    strSql =
                        string.Format(
                            @"select ListID,strListCode,strListName,strListGroupCode,strRightCode from BFT_ListEx where blnList=1 and  ' ' + strListGroupCode + ' ' like '% {0} %' and ListID<>" +
                            listExvo.ListID, str);
                else
                    strSql +=
                        string.Format(
                            @" union select ListID,strListCode,strListName,strListGroupCode,strRightCode from BFT_ListEx where blnList=1 and  ' ' + strListGroupCode + ' ' like '% {0} %' and ListID<>" +
                            listExvo.ListID, str);

            var groupListDt = Model.GetDataTable(strSql);
            BarButtonItem tlbItem;
            var tlbId = 101;
            foreach (DataRow dr in groupListDt.Rows)
            {
                tlbItem = new BarButtonItem();
                tlbItem.Caption = TypeUtil.ToString(dr["strListName"]);
                tlbItem.Id = tlbId++;
                tlbItem.Name = "tlbLinkList" + TypeUtil.ToString(dr["strListCode"]);
                tlbItem.Tag = dr;
                tlbItem.ItemClick += tlbItem_ItemClick;
                tlbGroupList.AddItem(tlbItem);
            }

            if (groupListDt.Rows.Count <= 0)
                tlbGroupList.Visibility = BarItemVisibility.Never;
        }

        /// <summary>
        ///     �����б��
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tlbItem_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                var tlbItem = e.Item as BarButtonItem;
                var dr = tlbItem.Tag as DataRow;
                var strParameters = "Entity=" + TypeUtil.ToString(dr["strListCode"]).Trim() + "&ListID=" +
                                    TypeUtil.ToInt(dr["ListID"]);

                var frm = new frmList(TypeUtil.ToInt(TypeUtil.ToInt(dr["ListID"])));
                frm.MdiParent = MdiParent;
                frm.Show();
                //��Ҫʵ��
                //RequestUtil.ExcuteCommand("ListEx", strParameters, null, TypeUtil.ToString(dr["strRightCode"]).Trim());
            }
            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        /// <summary>
        ///     ��ʼ���б�������
        /// </summary>
        private void InitListSchema()
        {
            var dt = Model.GetSchemaList(listExvo.ListID);
            lookupSchema.Properties.DataSource = dt;
            if (dt.Rows.Count > 0) //Ĭ�ϵ�һ������
            {
                ListDataID = TypeUtil.ToInt(dt.Rows[0]["ListDataID"]);

                lookupSchema.EditValueChanged -= lookupSchema_EditValueChanged;
                lookupSchema.EditValue = ListDataID;
                lookupSchema.EditValueChanged += lookupSchema_EditValueChanged;
            }
        }

        /// <summary>
        ///     �����б���ʾ��ʽ����
        /// </summary>
        public void SetListShowStyleData()
        {
            try
            {
                var listShowStyleDt = new DataTable();
                listShowStyleDt.Columns.Add(new DataColumn("Code", Type.GetType("System.String")));
                listShowStyleDt.Columns.Add(new DataColumn("Name", Type.GetType("System.String")));

                var dr = listShowStyleDt.NewRow();
                dr["Code"] = "list";
                dr["Name"] = "�б�";
                listShowStyleDt.Rows.Add(dr);

                if (listExvo.BlnPivot)
                {
                    dr = listShowStyleDt.NewRow();
                    dr["Code"] = "pivot";
                    dr["Name"] = "����";
                    listShowStyleDt.Rows.Add(dr);
                }
                if (listExvo.BlnChart)
                {
                    dr = listShowStyleDt.NewRow();
                    dr["Code"] = "chart";
                    dr["Name"] = "ͼ��";
                    listShowStyleDt.Rows.Add(dr);
                }

                lookupShowStyle.Properties.DataSource = listShowStyleDt;

                lookupShowStyle.EditValueChanged -= lookupShowStyle_EditValueChanged;
                lookupShowStyle.EditValue = "list"; //Ĭ��Ϊ�б�


                lookupShowStyle.EditValueChanged += lookupShowStyle_EditValueChanged;
            }
            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        /// <summary>
        ///     �б����ʾ���
        /// </summary>
        private void InitGridStyleConfig()
        {
            if (gridControl.DataSource == null)
                return;
            xapp = new XAppearances(AppDomain.CurrentDomain.BaseDirectory + "Config\\" + "listStyle.xml");
            BarCheckItem item;
            foreach (var o in xapp.FormatNames)
            {
                item = new BarCheckItem(barManager, false);
                item.Caption = o.ToString();
                item.Name = o.ToString();
                item.ItemClick += item_ItemClick;
                menuListStyle.AddItems(new BarItem[] { item });
            }
            var strListStyle = RequestUtil.GetParameterValue("ListStyle");
            SetListStyle(strListStyle);
        }

        /// <summary>
        ///     ��ʼ���б�Ĺ�����
        /// </summary>
        private void InitListToolbarConfig()
        {
            tlbPivotSetting.Visibility = BarItemVisibility.Never;
            tlbChartSetting.Visibility = BarItemVisibility.Never;


            IList<ListcommandexInfo> listCommands = Model.GetListCommandExDTOs(listExvo.ListID);
            var bar = barManager.Bars["ToolBar"];
            if (listCommands != null)
            {
                //�����ж���ʾToolBar  20180407
                if (listCommands.Count > 0)
                {
                    ToolBar.Visible = true;
                }
                foreach (var listCommand in listCommands)
                {
                    var button = new BarButtonItem();
                    button.Caption = listCommand.StrListCommandName;
                    button.Hint = listCommand.StrListCommandTip;
                    button.Tag = listCommand.StrRequestCode + "&" + listCommand.RequestId; //��Ҫȷ�����ٿ���
                    if (listCommand.BlnDblClick)
                        DoubleButtonItem = button;
                    if (TypeUtil.ToString(listCommand.StrListIconName) != "")
                    {
                        button.Glyph = res.GetBitmap(listCommand.StrListIconName);
                        if (button.Glyph == null)
                        {
                            var strPath = AppDomain.CurrentDomain.BaseDirectory + "Image\\ListReportImage\\" +
                                          listCommand.StrListIconName;
                            if (File.Exists(strPath))
                                button.Glyph = Image.FromFile(strPath);
                        }
                    }

                    if (realEntity == string.Empty)
                    {
                        //string strPara = (string)commandItem.Tag;
                        //if (strPara != string.Empty)
                        //{
                        //    string[] strs = strPara.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
                        //    for (int i = 0; i < strs.Length; i++)
                        //    {
                        //        if (strs[i].Contains("RealEntity"))
                        //        {
                        //            this.realEntity = strs[i].Substring(11);
                        //            break;
                        //        }
                        //    }
                        //}
                    }

                    button.PaintStyle = BarItemPaintStyle.CaptionGlyph;
                    button.ItemClick += button_ItemClick;

                    bar.AddItem(button);
                    barManager.Items.Add(button);
                    if (listCommand.Parameters != "")
                        PermissionManager.HavePermission(listCommand.Parameters, button);
                }

                foreach (var listCommand in listCommands)
                {
                    if (extendEntity != string.Empty)
                        continue;

                    var strPara = listCommand.Parameters;
                    if (strPara != string.Empty)
                    {
                        var strs = strPara.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
                        for (var i = 0; i < strs.Length; i++)
                            if (strs[i].Contains("Entity"))
                            {
                                extendEntity = strs[i].Substring(7);
                                break;
                            }
                    }
                }
            }

            if (realEntity == string.Empty)
                realEntity = extendEntity;
        }

        private void SetGridColumnFormat(GridColumn gridColumn, string strDisplayFormat, string strCoinDecimalNum,
            string strWeightDecimalNum)
        {
            if (strDisplayFormat == "����")
            {
                gridColumn.DisplayFormat.FormatType = FormatType.Numeric;
                gridColumn.DisplayFormat.FormatString = "n0";
                gridColumn.SummaryItem.DisplayFormat = "{0:n0}";
            }
            else if (strDisplayFormat == "����")
            {
                gridColumn.DisplayFormat.FormatType = FormatType.Numeric;
                gridColumn.DisplayFormat.FormatString = "n" + strCoinDecimalNum;
                gridColumn.SummaryItem.DisplayFormat = "{0:n" + strCoinDecimalNum + "}";
            }
            else if (strDisplayFormat == "����")
            {
                gridColumn.DisplayFormat.FormatType = FormatType.Numeric;
                gridColumn.DisplayFormat.FormatString = "n" + strWeightDecimalNum;
                gridColumn.SummaryItem.DisplayFormat = "{0:n" + strWeightDecimalNum + "}";
            }
            else if (strDisplayFormat == "����")
            {
                gridColumn.DisplayFormat.FormatType = FormatType.DateTime;
                gridColumn.DisplayFormat.FormatString = "yyyy-MM-dd";
            }
            else if (strDisplayFormat == "ʱ��")
            {
                gridColumn.DisplayFormat.FormatType = FormatType.DateTime;
                gridColumn.DisplayFormat.FormatString = "HH:mm:ss";
            }
            else if (strDisplayFormat == "����ʱ��")
            {
                gridColumn.DisplayFormat.FormatType = FormatType.DateTime;
                gridColumn.DisplayFormat.FormatString = "yyyy-MM-dd HH:mm:ss";
            }
            if (gridColumn.SummaryItem.SummaryType == SummaryItemType.Count)
                gridColumn.SummaryItem.DisplayFormat = "{0:n0}";
        }

        private void SetGridColumnFormat()
        {
            //if (this.gridControl.DataSource == null)
            //{
            //    return;
            //}

            if (_lmdDt == null)
                return;


            foreach (GridGroupSummaryItem groupSumItem in gridView.GroupSummary)
                if (groupSumItem.DisplayFormat == string.Empty)
                    groupSumItem.DisplayFormat = "{0:" +
                                                 gridView.Columns[groupSumItem.FieldName].DisplayFormat.FormatString +
                                                 "}";
        }

        /// <summary>
        ///     ������ʽ����
        /// </summary>
        private void SetGridColumnConditionFormat(GridColumn gridColumn, string strConditionFormat)
        {
            var sfcType = (SFCDataTypeEnum)TypeUtil.ToInt(strConditionFormat.Substring(strConditionFormat.Length - 1));
            var strTemp = strConditionFormat.Remove(strConditionFormat.Length - 1);
            var strConditions = strTemp.Split(new[] { "&&$$" }, StringSplitOptions.RemoveEmptyEntries);
            if (strConditions.Length > 0)
            {
                var str = string.Empty;
                var strOper = string.Empty;
                var strValue1 = string.Empty;
                var strValue2 = string.Empty;
                var blnApplyRow = false;
                StyleFormatCondition gridSFC = null;
                var color = Color.White;
                var fontColor = Color.Black;
                bool bBold = false;
                bool bItalic = false;
                bool bUnderline = false;
                bool bStrikeout = false;

                // 20170801
                for (var i = 0; i < strConditions.Length; i++)
                {
                    str = strConditions[i];
                    if (str.Contains("&&$"))
                    {
                        // 20170906
                        //var strs = str.Split(new[] { "&&$" }, StringSplitOptions.RemoveEmptyEntries);
                        var strs = str.Split(new[] { "&&$" }, StringSplitOptions.None);
                        strOper = strs[0];
                        blnApplyRow = TypeUtil.ToBool(strs[1]);
                        strValue1 = "";
                        strValue2 = "";

                        //if (strs.Length > 3)
                        //    strValue1 = strs[2];
                        //if (strs.Length > 4)
                        //    strValue2 = strs[3];
                        strValue1 = strs[2];
                        if (!(strs[3] == "##*"))
                        {
                            strValue2 = strs[3];
                        }
                        else
                        {
                            strValue2 = "";
                        }

                        var ss = strs[4].Split(';');
                        Model.GetColorByString(ss, ref color);

                        var fontss = strs[5].Split(';');
                        Model.GetColorByString(fontss, ref fontColor);

                        bBold = TypeUtil.ToBool(strs[6]);

                        if (strs.Length >= 8)
                        {
                            bItalic = Convert.ToBoolean(strs[7]);
                        }
                        if (strs.Length >= 9)
                        {
                            bUnderline = Convert.ToBoolean(strs[8]);
                        }
                        if (strs.Length >= 10)
                        {
                            bStrikeout = Convert.ToBoolean(strs[9]);
                        }
                    }

                    gridSFC = new StyleFormatCondition();
                    //���ô���/б��/�»���/ɾ����
                    if (bBold || bItalic || bUnderline || bStrikeout)
                    {
                        gridSFC.Appearance.Options.UseFont = true;
                        FontStyle fs = FontStyle.Regular;
                        if (bBold)
                        {
                            fs |= FontStyle.Bold;
                        }
                        if (bItalic)
                        {
                            fs |= FontStyle.Italic;
                        }
                        if (bUnderline)
                        {
                            fs |= FontStyle.Underline;
                        }
                        if (bStrikeout)
                        {
                            fs |= FontStyle.Strikeout;
                        }
                        gridSFC.Appearance.Font = new Font(gridSFC.Appearance.Font, fs);
                    }

                    gridSFC.Appearance.Options.UseBackColor = true; //����ɫ
                    gridSFC.Appearance.BackColor = color;
                    gridSFC.Appearance.Options.UseForeColor = true; //������ɫ
                    gridSFC.Appearance.ForeColor = fontColor;

                    gridSFC.Column = gridColumn;
                    gridSFC.ApplyToRow = blnApplyRow;
                    gridSFC.Condition = FormatConditionEnum.Expression;

                    var strCaclOper = GetCaclOper(strOper, gridColumn.FieldName);
                    gridSFC.Expression = string.Format(gridColumn.FieldName + strCaclOper, strValue1, strValue2);
                    gridView.FormatConditions.Add(gridSFC);
                }
            }
        }


        /// <summary>
        ///     ģ��������ʽ����
        /// </summary>
        private void SetGridColumnBluerFormat(string strFileName, string strBluerCondition)
        {
            var strConditions = strBluerCondition.Split(new[] { "&&$$" }, StringSplitOptions.RemoveEmptyEntries);
            if (strConditions.Length > 0)
                foreach (var s in strConditions)
                {
                    var ss = s.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                    if (ss.Length > 0)
                        DicBluerCondition.Add(strFileName, strBluerCondition);
                }
        }


        /// <summary>
        ///     �õ����Ŵ�
        /// </summary>
        /// <param name="strListDisplayFieldName"></param>
        private void SetGridColumnSort(string strListDisplayFieldName)
        {
            //�����б�Ԫ��������û�������˱���˳��(���ھ����õĹ̶�����)
            if (_lmdDt == null) return;
            var rows =
                _lmdDt.Select("MetaDataID=" + listExvo.MainMetaDataID + " and MetaDataFieldName='" +
                              strListDisplayFieldName + "' and len(strCondition)>0");
            if (rows.Length > 0)
            {
                var MetaDataFileID = TypeUtil.ToInt(rows[0]["MetaDataFieldID"]);
                var rowm =
                    metadataFieldDt.Select("MetaDataID=" + listExvo.MainMetaDataID + " and ID=" + MetaDataFileID +
                                           " and blnDesignSort=1");
                if (rowm.Length > 0)
                {
                    //�ж�Ԫ�����������û������Ϊ�ɱ���
                    strSortText = TypeUtil.ToString(rows[0]["strCondition"]);
                    strSortText = strSortText.Substring(5);
                    strSortFileName = strListDisplayFieldName;
                }
            }
        }

        /// <summary>
        ///     ��ȡ�б�����ʵ��
        /// </summary>
        private void GetListDataEx()
        {
            //��ȡ�б�������
            listDataExVo = Model.GetListDataExData(ListDataID);
            strOldStrListSql = listDataExVo.StrListSQL;
            if ((listDataExVo == null) || (listDataExVo.ListDataID <= 0))
                throw new Exception("��ǰ�б�û�з���������з�������!");

            listTemplevo = Model.GetListTemple(listDataExVo.ListDataID);

            if ((listDataExVo != null) && (listDataExVo.StrDefaultShowStyle != string.Empty) &&
                (listDataExVo.StrDefaultShowStyle != null))
            {
                lookupShowStyle.EditValueChanged -= lookupShowStyle_EditValueChanged;
                lookupShowStyle.EditValue = listDataExVo.StrDefaultShowStyle; //Ĭ��Ϊ�б�
                lookupShowStyle.EditValueChanged += lookupShowStyle_EditValueChanged;
            }

            InitFreQryCondition(); //��ʼ����ѯ����


            if (blnListFreCondition ||
                ((DicReceivePara != null) && DicReceivePara.ContainsKey("SourceIsList") &&
                 TypeUtil.ToBool(DicReceivePara["SourceIsList"])))
                chkItemFreQryCondition.Checked = true;
        }

        /// <summary>
        ///     ��ȡ�б���ʾ����
        /// </summary>
        private void GetListDisplay()
        {
            listDisplayExList = Model.GetListDisplayExData(listDataExVo.ListDataID);

            //�Ƿ�����ֶ�Ȩ���ֵ�
            if (IsHaveFieldRightDic == null)
                IsHaveFieldRightDic = new Dictionary<int, bool>();
            else
                IsHaveFieldRightDic.Clear();

            var fieldRightDt = Model.GetMetaDataFieldsByListDataID(listDataExVo.ListDataID);
            var fieldId = 0;
            for (var i = 0; i < fieldRightDt.Rows.Count; i++)
            {
                fieldId = TypeUtil.ToInt(fieldRightDt.Rows[i]["ID"]);
                if (IsHaveFieldRightDic.ContainsKey(fieldId))
                    continue;

                IsHaveFieldRightDic.Add(fieldId, TypeUtil.ToBool(fieldRightDt.Rows[i]["blnFieldRight"]));
            }
        }

        /// <summary>
        ///     �Ƿ����ֶ�Ȩ��
        /// </summary>
        /// <returns>true/false</returns>
        private bool IsHaveFieldRight(bool isCalcField, string strFieldFullName)
        {
            var blnExist = true;
            if (IsHaveFieldRightDic == null)
                return true;

            var fieldId = 0;
            var drs = _lmdDt.Select("MetaDataFieldName='" + strFieldFullName + "'");
            if ((drs != null) && (drs.Length > 0))
            {
                fieldId = TypeUtil.ToInt(drs[0]["MetaDataFieldID"]);
                if (isCalcField)
                {
                    //���������⴦��
                    var strRefColList = TypeUtil.ToString(drs[0]["StrRefColList"]);
                    var strColumns = strRefColList.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var strColumn in strColumns)
                    {
                        drs = _lmdDt.Select("MetaDataFieldName='" + strColumn + "'");
                        if ((drs != null) || (drs.Length > 0))
                        {
                            fieldId = TypeUtil.ToInt(drs[0]["MetaDataFieldID"]);
                            if ((fieldId == 0) ||
                                (IsHaveFieldRightDic.ContainsKey(fieldId) && IsHaveFieldRightDic[fieldId] &&
                                 !ClientCacheModel.IsHaveFieldRigth(fieldId)))
                            {
                                blnExist = false;
                                break;
                            }
                        }
                    }
                    return blnExist;
                }
            }

            //if (fieldId == 0 || (ClientCacheData.IsFieldRightById(fieldId) && !ClientCacheData.IsHaveFieldRigth(fieldId)))
            if ((fieldId == 0) ||
                (IsHaveFieldRightDic.ContainsKey(fieldId) && IsHaveFieldRightDic[fieldId] &&
                 !ClientCacheModel.IsHaveFieldRigth(fieldId)))
                blnExist = false;

            return blnExist;
        }


        /// <summary>
        ///     �����б��ʽ
        /// </summary>
        private void SetListFormat()
        {
            var strShowStyle = TypeUtil.ToString(lookupShowStyle.EditValue);
            if (strShowStyle == "pivot")
            {
                //����              

                SetPivotFormat();
                tlbPivotSetting.Visibility = BarItemVisibility.Always;
                tlbChartSetting.Visibility = BarItemVisibility.Never;

                layoutItemGrid.Visibility = LayoutVisibility.Never;
                layoutItemReport.Visibility = LayoutVisibility.Always;
                layoutItemChart.Visibility = LayoutVisibility.Never;
                //tlbSetPerPageNumber.Visibility = BarItemVisibility.Never;
                comboBoxEdit1.Visible = false;
                labelControl3.Visible = false;
            }
            else if (strShowStyle == "chart")
            {
                //ͼ��
                if (listDataExVo.StrChartSetting == string.Empty)
                    MessageShowUtil.ShowInfo("ͼ����δ���ã����ʼ������");

                SetChartFormat();
                tlbPivotSetting.Visibility = BarItemVisibility.Never;
                tlbChartSetting.Visibility = BarItemVisibility.Always;

                layoutItemGrid.Visibility = LayoutVisibility.Never;
                layoutItemReport.Visibility = LayoutVisibility.Never;
                layoutItemChart.Visibility = LayoutVisibility.Always;
            }
            else
            {
                //�б�ʽ
                SetGridFormat();
                tlbPivotSetting.Visibility = BarItemVisibility.Never;
                tlbChartSetting.Visibility = BarItemVisibility.Never;
                layoutItemGrid.Visibility = LayoutVisibility.Always;
                layoutItemReport.Visibility = LayoutVisibility.Never;
                layoutItemChart.Visibility = LayoutVisibility.Never;
                //tlbSetPerPageNumber.Visibility = BarItemVisibility.Always;
                comboBoxEdit1.Visible = true;
                labelControl3.Visible = true;
            }
        }

        /// <summary>
        ///     ����Grid��ʽ
        /// </summary>
        private void SetGridFormat()
        {
            gridView.OptionsView.ShowGroupPanel = false;
            gridView.Columns.Clear();
            hypers.Clear();
            gridView.GroupSummary.Clear();
            gridView.FormatConditions.Clear();
            DicBluerCondition.Clear();
            strSortText = "";
            GridColumn col = null;

            var fieldName = "";
            var strCoinDecimalNum = RequestUtil.GetParameterValue("CoinDecBit");
            var strWeightDecimalNum = RequestUtil.GetParameterValue("WeightBit");
            var summaryItem = new GridGroupSummaryItemCollection(gridView);

            foreach (var listDisplay in listDisplayExList)
            {
                #region �����б����ʾ��Ϣ

                fieldName = listDisplay.StrListDisplayFieldName;

                //�ж��ֶ�Ȩ��
                //if (!IsHaveFieldRight(listDisplay.IsCalcField, fieldName))
                //    continue;
                col = new GridColumn();
                col.FieldName = fieldName;
                col.Caption = listDisplay.StrListDisplayFieldNameCHS;
                col.Width = listDisplay.LngDisplayWidth;
                col.OptionsColumn.ReadOnly = true;
                col.OptionsColumn.ShowInCustomizationForm = listDisplay.LngRowIndex > -1;
                if (listDisplay.LngHyperLinkType > 0)
                {
                    var hyperlink = new RepositoryItemHyperLinkEdit();
                    hyperlink.Name = "hyperlink";
                    hyperlink.SingleClick = true;
                    hyperlink.OpenLink += hyperlink_OpenLink;
                    col.Tag = listDisplay;
                    col.ColumnEdit = hyperlink;
                    col.OptionsColumn.ReadOnly = false;
                    hypers.Add(hyperlink);
                }
                else
                {
                    col.OptionsColumn.AllowEdit = false;
                }

                if (listDisplay.BlnMerge)
                {
                    if (!gridView.OptionsView.AllowCellMerge)
                        gridView.OptionsView.AllowCellMerge = true;
                    col.OptionsColumn.AllowMerge = DefaultBoolean.True;
                }
                else
                {
                    col.OptionsColumn.AllowMerge = DefaultBoolean.False;
                }

                //���õ�Ԫ��ɻ�����ʾ
                if (col.ColumnEdit == null)
                {
                    var rime = new DevExpress.XtraEditors.Repository.RepositoryItemMemoEdit();
                    col.ColumnEdit = rime;
                }

                gridView.Columns.Add(col);

                gridView.Columns[fieldName].Visible = listDisplay.LngRowIndex > -1;

                #endregion

                if (listDataExVo.BlnSmlSum && listDisplay.BlnSummary)
                {
                    if (listDataExVo.LngSmlSumType <= 0)
                    {
                        gridView.Columns[fieldName].SummaryItem.SummaryType = SummaryItemType.Sum;
                        gridView.Columns[fieldName].SummaryItem.DisplayFormat = "{0:n2}";
                    }
                    else
                    {
                        gridView.Columns[fieldName].SummaryItem.SummaryType =
                            (SummaryItemType)(listDataExVo.LngSmlSumType - 1);
                        gridView.Columns[fieldName].SummaryItem.DisplayFormat = "{0:n2}";
                    }

                    summaryItem.Add(gridView.Columns[fieldName].SummaryItem.SummaryType, fieldName,
                        gridView.Columns[fieldName], gridView.Columns[fieldName].SummaryItem.DisplayFormat);
                    gridView.GroupSummary.Assign(summaryItem);
                    gridView.GroupSummary.Add(new GridGroupSummaryItem(SummaryItemType.Count, col.FieldName, null,
                        "(��¼: ������ {0:n0})"));
                }


                //�����и�ʽ
                if (!string.IsNullOrEmpty(listDisplay.StrSummaryDisplayFormat))
                    SetGridColumnFormat(gridView.Columns[fieldName],
                        TypeUtil.ToString(listDisplay.StrSummaryDisplayFormat), strCoinDecimalNum, strWeightDecimalNum);

                //������ʽ����
                if (!string.IsNullOrEmpty(listDisplay.StrConditionFormat) && (listDisplay.LngApplyType != 3))
                    SetGridColumnConditionFormat(gridView.Columns[fieldName], listDisplay.StrConditionFormat);

                //ģ����ѯ����
                if (!string.IsNullOrEmpty(listDisplay.StrBluerCondition))
                    SetGridColumnBluerFormat(fieldName, listDisplay.StrBluerCondition);

                //���ñ���˳��
                SetGridColumnSort(listDisplay.StrListDisplayFieldName);
            }

            if (listDataExVo.BlnSmlSum)
                gridView.OptionsView.ShowFooter = true;

            SetGridColumnFormat();
        }

        /// <summary>
        ///     ��ʼ���б�����
        /// </summary>
        private void InitListData()
        {
            try
            {
                //PerPageRecord = TypeUtil.ToInt(tlbSetPerPageNumber.EditValue);
                PerPageRecord = TypeUtil.ToInt(comboBoxEdit1.Text);

                // 20171227
                //if ((PerPageRecord == 0) || (PerPageRecord > 10000)) PerPageRecord = 10000;
                if ((PerPageRecord == 0) || (PerPageRecord > 50000)) PerPageRecord = 50000;

                var strListSql = listDataExVo.StrListSQL.Replace("where 1=1",
                    "where 1=1 " + _strFreQryCondition + _strReceiveParaCondition + _strSortCondtion);
                GetDistinctSql(ref strListSql);
                TotalRecord = Model.GetTotalRecord(strListSql);


                TotalPage = TotalRecord / PerPageRecord;
                if (TotalRecord % PerPageRecord > 0) TotalPage++;
                CurrentPageNumber = 1;

                if (TotalPage <= 1)
                    dt = Model.GetDataTable(strListSql);
                else
                    dt = Model.GetPageData(strListSql, 1, PerPageRecord);
                BandingData();
                SetListPageInfo();
            }
            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        private string GetSortWhere()
        {
            //var strSortWhere = (listlayount != null) && (listlayount.Count > 0)
            //    ? " and " + listlayount[0].StrConTextCondition
            //    : "";
            string strSortWhere = "";
            if (listlayount != null && listlayount.Count > 0)
            {
                var listDataLay = listlayount.FirstOrDefault(a => a.ListDataLayoutID.ToString() == lookUpEditArrangeTime.EditValue.ToString());
                if (listDataLay != null) strSortWhere = " and " + listDataLay.StrConTextCondition;
            }
            return strSortWhere;
        }

        /// <summary>
        ///     ˢ���б��е�����
        /// </summary>
        private void RefreshListData()
        {
            try
            {
                OpenWaitDialog("����ˢ������");

                SetExecuteBeginTime();

                if (_freQryCondition != null)
                {
                    _strFreQryCondition = _freQryCondition.GetFreQryCondition();
                    _strFreQryConditionByChs = _freQryCondition.GetFreQryCondtionByChs();
                    _listdate = _freQryCondition.GetDayDate();
                    _dicConditionOldTable = _freQryCondition.GetConditionOldTable();
                }

                //���û��ѡ���ѯ����,����ʾ��ѡ���ѯ����
                var dtMetadata = ClientCacheModel.GetServerMetaData(listExvo.MainMetaDataID);
                var rows = dtMetadata.Select("blnDay=1");
                //�ж���Ԫ���ǲ����ձ�������ձ�����Ĭ����Ϊ�����治���ж��ٸ������ֶΣ�����ֻ���˵�������ݣ�������ʱ�����ж����ĸ�Ԫ�����ֶ���Ϊ�ձ����������ֶ�
                if ((rows != null) && (rows.Length > 0))
                    if ((_strFreQryCondition.Contains("1900-") || _strFreQryCondition.Contains("9999-")) &&
                        _strFreQryCondition.ToLower().Contains("datsearch"))
                    {
                        MessageShowUtil.ShowMsg("��ѡ���ѯʱ�䣡", true, barStaticItemMsg);
                        return;
                    }
                if ((_listdate != null) && (_listdate.Count > 92))
                {
                    //2016-10-24  ,����if�жϣ���Ϊ��ű����±�������ձ���������������ձ�,���Բ���_listdate��ֵ
                    var dtMetadataA = ClientCacheModel.GetServerMetaData(listExvo.MainMetaDataID);
                    var rowsA = dtMetadataA.Select("blnDay=1");
                    if ((rowsA == null) || (rowsA.Length == 0)) return;
                    var strDayType = TypeUtil.ToString(rowsA[0]["strDayType"]);
                    if (strDayType != "")
                    {
                        MessageShowUtil.ShowMsg("���ֻ�ܲ�ѯ���������ݣ�", true, barStaticItemMsg);
                        return;
                    }
                }

                if (listExvo.ListID == 68 || listExvo.ListID == 34 || listExvo.ListID == 19)//�������ϵ硢�����쳣ʱ������
                {
                    if (_listdate.Count > 1)
                    {
                        DateTime stime, etime;
                        DateTime.TryParse(_listdate[0].ToString().Substring(0, 4) + "-" + _listdate[0].ToString().Substring(4, 2) + "-" + _listdate[0].ToString().Substring(6, 2), out stime);
                        DateTime.TryParse(_listdate[_listdate.Count - 1].ToString().Substring(0, 4) + "-" + _listdate[_listdate.Count - 1].ToString().Substring(4, 2) + "-" + _listdate[_listdate.Count - 1].ToString().Substring(6, 2), out etime);
                        TimeSpan ts = etime - stime;
                        if (ts.TotalDays > 7)
                        {
                            XtraMessageBox.Show("���ֻ�ܲ�ѯ7������ݣ�");
                            return;
                        }
                    }
                }

                SetDayTableSql();

                // 20180306
                // 20180227
                // 20170830
                if ((_listdate != null) && (_listdate.Count > 0))
                {
                    //��if���ڲ����ţ���Ϊ�������ǰ��콨����

                    if (_strFreQryCondition.ToLower().IndexOf("datsearch") > 0)
                    {
                        var strdate =
                            _strFreQryCondition.Substring(_strFreQryCondition.ToLower().IndexOf("datsearch") + 10, 55);
                        _strFreQryCondition = _strFreQryCondition.Replace(strdate, " <> '1900-1-1'");
                    }
                }

                if (radioButtonArrangePoint.Checked)
                {
                    _strSortCondtion = GetSortWhere();
                }
                else
                {
                    _strSortCondtion = "";
                }

                //2015-10-26   ����ѯ����������sql������󣬶��Ƿŵ��������ǰ��,����ǰ��ѯЧ��(�ܲɼ�¼���ű������������ֵñȽϴ�)

                #region

                var StrListSQL = strOldStrListSql;

                foreach (var str in _dicConditionOldTable.Keys)
                {
                    rows = dtMetadata.Select("strTableName='" + str + "'");
                    if (rows.Length > 0) //�������Ĳ�ѯ��������Ԫ���ݵ�����Ű��������ڹ�����֮ǰ����Ϊ������Ԫ���ݿ����ж������һ������ܹ������ö�٣���������ƣ���ѹ�����ö�ٱ�ȫ����������
                        StrListSQL = StrListSQL.Replace("from " + str,
                            "from " + str + " where " + _dicConditionOldTable[str]);
                }
                if (StrListSQL.Length > 0)
                    listDataExVo.StrListSQL = StrListSQL;

                #endregion

                if (PerPageRecord == 0)
                {
                    InitListData();
                    MessageShowUtil.ShowStaticInfo("��������ִ��ʱ��Ϊ" + GetExecuteTimeString(), barStaticItemMsg);
                    return;
                }

                //���������� 
                var strListSql = listDataExVo.StrListSQL.Replace("where 1=1",
                    "where 1=1 " + _strFreQryCondition + _strReceiveParaCondition + _strSortCondtion);
                GetDistinctSql(ref strListSql);
                TotalRecord = Model.GetTotalRecord(strListSql);

                //������ҳ��
                TotalPage = TotalRecord / PerPageRecord;
                if (TotalRecord % PerPageRecord > 0) TotalPage++;

                var rowHandle = gridView.FocusedRowHandle; //�õ���ǰ��ѡ�����
                tlbGoToPage_ItemClick(null, null); //ִ��ˢ��
                var totalRow = gridView.RowCount; //�õ���ǰҳ��������              

                if (totalRow > 0)
                    if (rowHandle <= totalRow)
                        gridView.FocusedRowHandle = rowHandle;
                    else
                        gridView.FocusedRowHandle = totalRow - 1;
                if ((CurrentPageNumber > 0) && (totalRow == 0))
                    tlbGoToPage_ItemClick(null, null); //ִ��ˢ��

                MessageShowUtil.ShowStaticInfo("��������ִ��ʱ��Ϊ" + GetExecuteTimeString(), barStaticItemMsg);
            }
            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
            finally
            {
                CloseWaitDialog();
            }
        }

        /// <summary>
        ///     ��ʼ�����ò�ѯ����
        /// </summary>
        private void InitFreQryCondition()
        {
            var strFreWhere = ""; //���ò�ѯ����
            if (_lmdDt == null)
                _lmdDt = Model.GetListMetaData(ListDataID, listDataExVo.UserID);
            if (_lmdDt != null)
            {
                var dt = _lmdDt.Copy();
                dt.DefaultView.RowFilter = "blnFreCondition=1 and isnull(strFreCondition,'')<>''";
                dt.DefaultView.Sort = "lngFreConIndex asc";
                dt = dt.DefaultView.ToTable();
                var count = dt.Rows.Count;
                var strFieldName = "";
                var strFieldType = "";
                var strFkCode = "";
                var strFreCondition = "";
                DataRow curDr = null;
                for (var i = 0; i < count; i++)
                {
                    curDr = dt.Rows[i];
                    strFieldName = TypeUtil.ToString(curDr["MetaDataFieldName"]);
                    strFieldType = TypeUtil.ToString(curDr["strFieldType"]);
                    strFkCode = TypeUtil.ToString(curDr["strFkCode"]);
                    strFreCondition = TypeUtil.ToString(curDr["strFreCondition"]);

                    var str = "";
                    if (strFreCondition != string.Empty)
                    {
                        var strfilename = strFieldName;
                        var index = strfilename.LastIndexOf("_");
                        if (index > -1)
                            strfilename = "" + strfilename.Remove(index, 1).Insert(index, ".");

                        if (strFkCode != string.Empty)
                            str = BulidConditionUtil.GetRefCondition(strfilename, strFreCondition, strFieldType);
                        else
                            str = BulidConditionUtil.GetConditionString(strfilename, strFieldType, strFreCondition);

                        //������Ŀ�б������»��ߣ�����Ҫȡ���һ���»���  2014-11-26 
                        //str = BulidConditionUtil.GetConditionString(strFieldName.Replace("_", "."), strFieldType, strFreCondition);

                        if (str != string.Empty)
                            strFreWhere += " and " + str;
                    }
                }
            }

            _strFreQryCondition = strFreWhere;
        }

        /// <summary>
        ///     �����б���к�
        /// </summary>
        private void gridView_CustomDrawRowIndicator(object sender, RowIndicatorCustomDrawEventArgs e)
        {
            var rowHandle = e.RowHandle + 1;
            if (e.Info.IsRowIndicator && (rowHandle > 0))
                e.Info.DisplayText = rowHandle.ToString();
        }

        /// <summary>
        ///     ������
        /// </summary>
        private void BandingData()
        {
            try
            {
                if (dt != null)
                {
                    //�޸�����Ϊ��1900-01-01��ΪNull


                    var rowCount = dt.Rows.Count;

                    if (listExvo.StrListCode == "MLLKDDay")
                    {
                        // dt = TypeUtil.DeleteRepeateRow(dt);
                    }

                    // ģ����ʾ���罫���>1000����ʾΪ*
                    foreach (var s in DicBluerCondition.Keys)
                    {
                        var ss = DicBluerCondition[s].Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        DataRow[] rows = null;
                        try
                        {
                            rows = dt.Select(ss[1]);
                            foreach (var row in rows)
                                try
                                {
                                    row[s] = ss[0];
                                }
                                catch
                                {
                                    //2017-04-07  ,�����ű���ϼ�����ʾʱ������
                                    row[s] = DBNull.Value;
                                }
                        }
                        catch (Exception ex)
                        {
                            MessageShowUtil.ShowInfo("ģ�����������������ڷ�������鿴!ԭ��Ϊ" + ex.Message);
                            break;
                        }
                    }

                    // 20170830
                    //���ݱ��ŵĲ��������򣬴�BFT_ListDataLayount���ȡ�����Ϣ
                    if (radioButtonArrangePoint.Checked)
                    {
                        if ((listlayount != null) && (listlayount.Count > 0))
                        {
                            var listDataLay = listlayount.FirstOrDefault(a => a.StrDate == lookUpEditArrangeTime.Text);
                            if (listDataLay != null)
                            {
                                strSortText = TypeUtil.ToString(listDataLay.StrCondition);
                                strSortText = strSortText.Substring(5);
                                strSortFileName = listDataLay.StrFileName;
                                var t = dt.Clone();
                                t.Clear();
                                var str = strSortText.Split(',');
                                foreach (var s in str)
                                {
                                    var rowss = dt.Select(strSortFileName + "=" + s + "");
                                    foreach (var row in rowss)
                                        t.Rows.Add(row.ItemArray);
                                }
                                dt = t;
                            }
                        }
                    }


                    #region //���ư�װλ�ñ仯����,�����ѡ�˺��԰�װλ�ñ仯�Ž������´���

                    if (chkblnKeyWord.Checked)
                    {
                        var RowsblnKeyWord = _lmdDt.Select("blnKeyWord=1");
                        var RowsGroupBy = _lmdDt.Select("lngKeyGroup>0");
                        var d = dt.Clone();
                        if (RowsblnKeyWord.Length > 0)
                        {
                            IDictionary<object, object> dics = new Dictionary<object, object>();
                            foreach (DataRow row in dt.Rows)
                            {
                                //ѭ������Դ������������ͬ��KeyWord(��Point�ֶ�)����key��
                                var strFileNameJia = "";
                                var strFileNames = "";
                                foreach (var rowkey in RowsblnKeyWord)
                                {
                                    var strFileName = TypeUtil.ToString(rowkey["MetaDataFieldName"]);
                                    var strvalue = TypeUtil.ToString(row[strFileName]);
                                    strFileNames += strFileName;
                                    strFileNameJia += strvalue;
                                }
                                if (!dics.ContainsKey(strFileNameJia))
                                    dics.Add(strFileNameJia, strFileNames);
                            }

                            foreach (var o in dics.Keys)
                            {
                                //������ͬ��keyword���õ����keyword���м��Ͻ��м���
                                var rowselect = dt.Select(dics[o] + "='" + o + "'");
                                if (rowselect.Length > 1)
                                {
                                    //�������keywordɸѡ�������������ϼ�¼,��ʾ���˰�װλ�û����豸���͵ı仯������Ҫ������
                                    for (var i = 1; i < rowselect.Length; i++)
                                        foreach (var rowkeygroup in RowsGroupBy)
                                        {
                                            //�õ���һ�е�ֵ�������ʱ��Ѽ�����д����һ��ȥ
                                            var strValueOneRow =
                                                rowselect[0][rowkeygroup["MetaDataFieldName"].ToString()];
                                            //�õ��ϲ����������(1����,2����,3ƽ��,4���,5��С)
                                            var lngkeygrouptype = TypeUtil.ToInt(rowkeygroup["lngKeyGroup"]);
                                            //�õ���ʱ�е�ֵ
                                            var strValue = rowselect[i][rowkeygroup["MetaDataFieldName"].ToString()];
                                            var strFileType = TypeUtil.ToString(rowkeygroup["strFieldType"]);
                                            if (lngkeygrouptype == 2)
                                                if (strFileType.Contains("char"))
                                                {
                                                    //������ַ������ͣ���ֱ��ת��Ϊ���ڽ����ж�
                                                    var time1 = TypeUtil.ToDateTime(strValueOneRow);
                                                    var time2 = TypeUtil.ToDateTime(strValue);
                                                    var span1 = new TimeSpan(time1.Hour, time1.Minute, time1.Second);
                                                    var span2 = new TimeSpan(time2.Hour, time2.Minute, time2.Second);
                                                    var sum = span1 + span2;
                                                    rowselect[0][rowkeygroup["MetaDataFieldName"].ToString()] =
                                                        sum.ToString();
                                                }
                                                else
                                                {
                                                    rowselect[0][rowkeygroup["MetaDataFieldName"].ToString()] =
                                                        TypeUtil.ToDecimal(strValueOneRow) +
                                                        TypeUtil.ToDecimal(strValue);
                                                }
                                            if (lngkeygrouptype == 3)
                                            {
                                                rowselect[0][rowkeygroup["MetaDataFieldName"].ToString()] =
                                                    TypeUtil.ToDecimal(strValueOneRow) + TypeUtil.ToDecimal(strValue);
                                                if (i == rowselect.Length - 1)
                                                    rowselect[0][rowkeygroup["MetaDataFieldName"].ToString()] =
                                                        Math.Round(
                                                            TypeUtil.ToDecimal(
                                                                rowselect[0][rowkeygroup["MetaDataFieldName"].ToString()
                                                                ]) / rowselect.Length, 2, MidpointRounding.AwayFromZero);
                                            }
                                        }
                                    d.Rows.Add(rowselect[0].ItemArray);
                                }
                                else
                                {
                                    foreach (var row in rowselect)
                                        d.Rows.Add(row.ItemArray);
                                }
                            }
                            dt = d;
                        }
                    }

                    #endregion
                }

                BandingDataToControl();
                if (dt != null)
                    //barStaticItemRowCount.Caption = "����" + TotalRecord + "����¼��";
                    labelControl2.Text = "��" + TotalRecord + "����¼";
            }
            catch (OutOfMemoryException)
            {
                throw new Exception("��ʾ����������,�ڴ����,�����ÿҳ��ʾ����");
            }
            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        /// <summary>
        ///     ������
        /// </summary>
        private void BandingDataToControl()
        {
            try
            {
                if (dt != null)
                    dt = TypeUtil.AddNumberToDataTable(dt); //������Դ�����һ������У����ڱ�����ʾ��ʱ���������
                var strShowStyle = TypeUtil.ToString(lookupShowStyle.EditValue);
                if (strShowStyle == "pivot")
                {
                    //����                     
                    //DataSet dataset = new DataSet("ywdataset");
                    //dataset.Tables.Clear();
                    //dataset.Tables.Add(dt);   
                    if (dt == null) return;
                    dt.TableName = "dtMaster";
                    ReportDataBinding();

                    //������������
                    var lblConditon = rp.FindControl("lblConditon", true) as XRLabel;
                    if (lblConditon != null)
                        lblConditon.Text = _strFreQryConditionByChs;

                    //���ر�ע
                    var xrrich = rp.FindControl("txtDesc", true) as XRRichText;
                    if (xrrich != null)
                    {
                        //xrrich.Text = listDataExVo.StrListSrcSQL;
                        DateTime dtRemark = GetRemarkTime();
                        var listDataRemarkInfo = Model.GetListDataRemarkByTimeListDataId(dtRemark, listDataExVo.ListDataID);

                        if (listDataRemarkInfo == null)
                        {
                            xrrich.Text = "";
                        }
                        else
                        {
                            xrrich.Text = listDataRemarkInfo.Remark;
                        }
                    }

                    documentViewer1.DocumentSource = rp;
                    documentViewer1.PrintingSystem = rp.PrintingSystem;

                    rp.CreateDocument();
                }
                else if (strShowStyle == "chart")
                {
                    //ͼ��

                    chartControl.DataSource = dt;
                }
                else
                {
                    //�б�ʽ
                    SwitchingValueStateChangeDataAmend();
                    gridControl.DataSource = null;
                    gridControl.DataSource = dt;

                    InitGridStyleConfig();
                }
                if (dt != null)
                    //barStaticItemRowCount.Caption = "����" + TotalRecord + "����¼��";
                    labelControl2.Text = "��" + TotalRecord + "����¼";
            }
            catch (OutOfMemoryException)
            {
                throw new Exception("��ʾ����������,�ڴ����,�����ÿҳ��ʾ����");
            }
            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        private void hyperlink_OpenLink(object sender, OpenLinkEventArgs e)
        {
            try
            {
                if (
                    string.IsNullOrEmpty(
                        TypeUtil.ToString(gridView.GetRowCellValue(gridView.FocusedRowHandle, gridView.FocusedColumn))))
                    return;
                ListdisplayexInfo listDisplayEx = null;

                if (gridView.FocusedColumn.Tag != null)
                    listDisplayEx = (ListdisplayexInfo)gridView.FocusedColumn.Tag;
                if ((listDisplayEx == null) || (listDisplayEx.LngHyperLinkType <= 0))
                    return;


                var rowHandle = gridView.FocusedRowHandle;
                var lngHyperLinkType = listDisplayEx.LngHyperLinkType;
                var strRequest = listDisplayEx.StrHyperlink;
                var strParaColName = listDisplayEx.StrParaColName;

                var fieldName = "";
                var fieldValue = "";
                string[] strParaArr;
                if (lngHyperLinkType == 1)
                {
                    //��Ƭ
                    strParaArr = strParaColName.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    var genericEntityName = GetGenericEntityName(rowHandle);
                    if (genericEntityName != string.Empty)
                        strRequest = strRequest.Replace("GenericEntity", genericEntityName);
                    foreach (var strPara in strParaArr)
                    {
                        fieldName = strPara.Remove(0, strPara.IndexOf('_') + 1);
                        fieldValue = TypeUtil.ToString(gridView.GetRowCellValue(rowHandle, gridView.Columns[strPara]));
                        strRequest = strRequest.Replace("${" + fieldName + "}", fieldValue);
                    }

                    //��Ƭ
                    var cmd = strRequest.Split(';');
                    if (cmd.Length == 2)
                        strRequest = cmd[1].Replace(",", "");
                }
                else if (lngHyperLinkType == 2)
                {
                    //�б�
                    strParaArr = strParaColName.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
                    var strProvide = string.Empty;
                    var strReceive = string.Empty;
                    var lngProvideType = 0;
                    IDictionary<string, string> paraDic = new Dictionary<string, string>();
                    paraDic.Add("SourceIsList", "true");
                    var strKey = "";
                    var strDisplay = "";
                    foreach (var strPara in strParaArr)
                    {
                        strReceive = strPara.Remove(strPara.LastIndexOf('='));
                        strProvide = strPara.Substring(strPara.LastIndexOf('=') + 1);
                        if ((strProvide.Length > 2) && ('$' == strProvide[strProvide.Length - 2]))
                        {
                            //������ʷ����

                            lngProvideType = TypeUtil.ToInt(strProvide.Substring(strProvide.Length - 1));
                            strProvide = strProvide.Remove(strProvide.Length - 2);
                        }
                        if (1 == lngProvideType)
                        {
                            //��������

                            GetFreQryConditionByField(strProvide, ref strKey, ref strDisplay);
                        }
                        else
                        {
                            //Ĭ����������Ϊ����
                            fieldValue =
                                TypeUtil.ToString(gridView.GetRowCellValue(rowHandle, gridView.Columns[strProvide]));
                            strKey = "����&&$" + fieldValue;
                            var drs = _lmdDt.Select("MetaDataFieldName='" + strProvide + "'");
                            if ((drs.Length > 0) && (TypeUtil.ToString(drs[0]["strFkCode"]) != string.Empty))
                                strDisplay = "����&&$(" + fieldValue + ")";
                            else
                                strDisplay = strKey;
                        }

                        if (strKey != string.Empty)
                        {
                            if (!paraDic.ContainsKey("Key_" + strReceive))
                                paraDic.Add("Key_" + strReceive, strKey);
                            if (!paraDic.ContainsKey("Display_" + strReceive))
                                paraDic.Add("Display_" + strReceive, strDisplay);
                        }
                    }

                    var cmd = strRequest.Split(';');
                    if (cmd.Length == 2)
                    {
                        strRequest = cmd[1].Replace(",", "");
                        var paras = strRequest.Split("&".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        for (var i = 0; i < paras.Length; i++)
                        {
                            var keyValues = paras[i].Split("=".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            if (keyValues.Length == 2)
                            {
                                var key = keyValues[0];
                                var value = keyValues[1];
                                if (!paraDic.ContainsKey(key))
                                    paraDic.Add(key, value);
                            }
                        }


                        var frm = new frmList(TypeUtil.ToInt(paraDic["ListID"]), paraDic);
                        frm.MdiParent = MdiParent;
                        frm.Show();

                        //RequestUtil.ExcuteCommand(cmd[0], paraDic);
                    }
                }
                else if (lngHyperLinkType == 3)
                {
                    //����

                    strParaArr = strParaColName.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var strPara in strParaArr)
                        strRequest = TypeUtil.ToString(gridView.GetRowCellValue(rowHandle, gridView.Columns[strPara]));
                    // RequestUtil.ExcuteCommand("Card", strRequest);
                }
                else
                {
                    //���� �磺��ַ

                    strParaArr = strParaColName.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var strPara in strParaArr)
                    {
                        fieldName = strPara.Remove(0, strPara.IndexOf('_') + 1);
                        fieldValue = TypeUtil.ToString(gridView.GetRowCellValue(rowHandle, gridView.Columns[strPara]));
                        strRequest = strRequest.Replace("${" + fieldName + "}", fieldValue);
                    }
                    Process.Start(strRequest);
                }
            }

            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        private string GetGenericEntityName(int rowHandle)
        {
            var str = string.Empty;
            if ((gridView.Columns["GenericEntity"] != null) && (rowHandle >= 0))
                str = TypeUtil.ToString(gridView.GetRowCellValue(rowHandle, "GenericEntity"));
            return str;
        }

        /// <summary>
        ///     ��������ť���¼�
        /// </summary>
        private void button_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                gridView.CloseEditor();
                var rowHandle = gridView.FocusedRowHandle;
                var dic = new Dictionary<string, string>();
                var strbuttonTag = e.Item.Tag.ToString();
                var str = strbuttonTag.Split('&');

                var strRequestCode = str[0];
                if (strRequestCode == "")
                {
                    MessageShowUtil.ShowInfo("δ������������,�������ã�");
                    return;
                }

                var dtRequest = ClientCacheModel.GetServerRequest();
                var row = dtRequest.Select("RequestCode='" + strRequestCode + "'");
                if (row.Length == 0)
                {
                    MessageShowUtil.ShowInfo("δ�ҵ���������,��ȷ���������Ƿ���ȷ");
                    return;
                }
                var strRequest = TypeUtil.ToString(row[0]["MenuParams"]);
                if ((strRequest == "") && (str[1] != "add"))
                {
                    MessageShowUtil.ShowInfo("������δ����MenuParams�ֶ�,�������ã�");
                    return;
                }
                var fieldName = "";
                var fieldValue = "";
                var strParaArr = listDataExVo.StrListDefaultField.Split(new[] { ',' },
                    StringSplitOptions.RemoveEmptyEntries);
                foreach (var strPara in strParaArr)
                {
                    fieldName = strPara.Remove(0, strPara.LastIndexOf('_') + 1);
                    fieldValue = TypeUtil.ToString(gridView.GetRowCellValue(rowHandle, gridView.Columns[strPara]));
                    strRequest = strRequest.Replace("${" + fieldName + "}", fieldValue);
                }
                var strpar = strRequest.Split('&');
                foreach (var s in strpar)
                {
                    if (s == "") continue;
                    var sp = s.Split('=');
                    dic.Add(sp[0], sp[1]);
                }

                if ((str[1].ToLower() == "add") || (str[1].ToLower() == "modfiy") || str[1].ToLower() == "delete")
                {
                    //��������������޸ģ��򿪴��� 
                    if ((str[1].ToLower() == "modfiy") && (rowHandle < 0))
                    {
                        MessageShowUtil.ShowInfo("��ѡ����޸ĵļ�¼��");
                        return;
                    }

                    if ((str[1].ToLower() == "delete") && (rowHandle < 0))
                    {
                        MessageShowUtil.ShowInfo("��ѡ���ɾ���ļ�¼��");
                        return;
                    }

                    Sys.Safety.ClientFramework.CBFCommon.RequestUtil.OnRefreshReportEvent += RequestUtil_OnRefreshReportEvent;
                    Sys.Safety.ClientFramework.CBFCommon.RequestUtil.ExcuteCommand(strRequestCode, dic, false);
                    RefreshListData();
                }
                else
                {
                    //if (str[1].ToLower() == "delete")
                    //    DeleteListRows(row[0]);
                    //else
                    //    ButtonClieckExtend();
                }
            }
            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        private void RequestUtil_OnRefreshReportEvent(object sender, object e)
        {
            RefreshListData();
            Sys.Safety.ClientFramework.CBFCommon.RequestUtil.OnRefreshReportEvent -= RequestUtil_OnRefreshReportEvent;
        }

        public virtual void ButtonClieckExtend()
        {
            throw new Exception("�÷���δʵ��,����������ʵ�֣�");
        }

        private void DeleteListRows(DataRow row)
        {
            var strRowIDs = string.Empty;
            try
            {
                var strAssembly = TypeUtil.ToString(row["MenuFile"]);
                if (strAssembly == "")
                {
                    MessageShowUtil.ShowInfo("������δ����MenuFilep�ֶ�,�������ã�");
                    return;
                }
                var strClassName = TypeUtil.ToString(row["MenuNamespace"]);
                if (strClassName == "")
                {
                    MessageShowUtil.ShowInfo("������δ����MenuNamespace�ֶ�,�������ã�");
                    return;
                }

                var strKeyFileName = TypeUtil.ToString(row["MenuParams"]);
                strKeyFileName = strKeyFileName.Substring(0, strKeyFileName.IndexOf("="));

                strRowIDs = GetSelectListRows(strKeyFileName);
                if (strRowIDs == string.Empty)
                {
                    MessageShowUtil.ShowInfo("��ѡ���ɾ���ļ�¼��");
                    return;
                }

                var result = MessageShowUtil.ReturnDialogResult("ȷ��Ҫɾ����ѡ��¼��");
                if (result == DialogResult.No)
                    return;

                var dtoTypeName = strClassName + "," + strAssembly;
                var dtoType = Type.GetType(dtoTypeName);
                Model.DeteteRows(strRowIDs, dtoType);
                RefreshListData();
                MessageShowUtil.ShowInfo("�����ɹ�");
            }

            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex.InnerException);
            }
        }

        private string GetEntityIDFieldName()
        {
            var strEntityIDField = ""; //ʵ����������
            var strParaArr = listDataExVo.StrListDefaultField.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var strPara in strParaArr)
            {
                var fieldName = strPara.Remove(0, strPara.LastIndexOf('_') + 1);
                if (fieldName.ToLower() == realEntity.ToLower() + "id")
                {
                    strEntityIDField = strPara;
                    break;
                }
            }

            return strEntityIDField;
        }

        private string GetSelectListRows(string strFieldName)
        {
            var strColFieldName = "";
            var fieldName = "";
            var colCount = gridView.Columns.Count;
            for (var i = 0; i < colCount; i++)
            {
                fieldName = gridView.Columns[i].FieldName;
                fieldName = fieldName.Remove(0, fieldName.LastIndexOf('_') + 1);
                if (fieldName.ToLower() == strFieldName.ToLower())
                {
                    strColFieldName = gridView.Columns[i].FieldName;
                    break;
                }
            }

            if (strColFieldName == "")
            {
                MessageShowUtil.ShowInfo("������[" + strFieldName + "]�ֶ�!");
                return string.Empty;
                ;
            }


            var strRowIDs = string.Empty;
            var ArraySelectRows = gridView.GetSelectedRows();
            for (var i = 0; i < ArraySelectRows.Length; i++)
                strRowIDs = strRowIDs + TypeUtil.ToString(gridView.GetRowCellValue(ArraySelectRows[i], strColFieldName)) +
                            ",";
            if (strRowIDs != "")
                strRowIDs = strRowIDs.Remove(strRowIDs.Length - 1);

            return strRowIDs;
        }

        /// <summary>
        ///     �б�˫��
        /// </summary>
        private void gridView_DoubleClick(object sender, EventArgs e)
        {
            if (DoubleButtonItem == null)
            {
                if (gridView.FocusedRowHandle < 0)
                {
                    return;
                }
                //ȡ��ѡ������Ϣ���жϲ�������ϸ����  20170731
                string nodeName = gridView.GetRowCellValue(gridView.FocusedRowHandle, gridView.FocusedColumn).ToString();
                if (nodeName.Length > 20)//����30���ַ���������ϸ����
                {
                    CellDetail cellDetail = new CellDetail(nodeName);
                    cellDetail.ShowDialog();
                }
                return;
            }
            var args = new ItemClickEventArgs(DoubleButtonItem, DoubleButtonItem.Links[0]);
            button_ItemClick(null, args);
        }

        /// <summary>
        ///     ˢ������
        /// </summary>
        private void tlbRefresh_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                RefreshListData();
                blnRefreshed = true;
            }
            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        private string GetSqlByDeleteNull(string strsql)
        {
            var sql = strsql.Replace("\r\n", " ");
            var blnFor = true;
            while (blnFor)
            {
                sql = sql.Replace("  ", " ");
                if (!sql.Contains("  "))
                    blnFor = false;
            }
            return sql;
        }

        /// <summary>
        ///     ����Ԫ���ݱ�(BFT_MetaData)�е��ֶ�strDayTableName����������ı�
        /// </summary>
        /// <param name="dtTable"></param>
        /// <returns></returns>
        public DataTable GetDayTable(DataTable dtTable)
        {
            var dtTableCopy = dtTable.Clone();
            foreach (DataRow row in dtTable.Rows)
            {
                //�����б��õ�Ԫ���ݣ�Ȼ��ȥ��MeataData����strDayTableName�ֶΣ���Ҫ������strDayTableNameҲ����ͼ�����
                var strViewName = TypeUtil.ToString(row["strTableName"]);
                var strViewSrcTableName = TypeUtil.ToString(row["strDayTableName"]).ToLower();
                if (strViewSrcTableName.Contains(";"))
                {
                    var strs = strViewSrcTableName.Split(';');
                    foreach (var ss in strs)
                    {
                        if (ss == "") continue;
                        var s = ss.Split(':');

                        // 20180309
                        //foreach (var stable in s[1].Split(','))
                        //{
                        //    var r = dtTableCopy.NewRow();
                        //    r["strName"] = "";
                        //    r["strDayTableName"] = stable;
                        //    r["strTableName"] = s[0];
                        //    dtTableCopy.Rows.Add(r);
                        //}
                        var r = dtTableCopy.NewRow();
                        r["strName"] = "";
                        r["strDayTableName"] = s[1];
                        r["strTableName"] = s[0];
                        dtTableCopy.Rows.Add(r);
                    }
                }
            }

            return dtTableCopy;
        }

        /// <summary>
        /// �޸�view_mdef��ͼ���谴ʱ����˵���ͼ���ձ���ͼ
        /// </summary>
        private void SetDayTableSql()
        {
            string sAllUpdateSql = "";
            var dtMetadata = ClientCacheModel.GetServerMetaData(listExvo.MainMetaDataID);

            #region �޸�view_mdef��ͼ

            string sDate2 = "";
            if (radioButtonActivityPoint.Checked || radioButtonArrangePoint.Checked && lookUpEditArrangeActivity.EditValue == "1")
            {
                sDate2 = "between '9999-12-31 23:59:59' and '9999-12-31 23:59:59'";
            }
            else if (radioButtonStorePoint.Checked || radioButtonArrangePoint.Checked && lookUpEditArrangeActivity.EditValue == "2")
            {
                if (_strFreQryCondition.ToLower().IndexOf("datsearch") > 0)     //�в�ѯʱ��
                {
                    sDate2 = _strFreQryCondition.Substring(_strFreQryCondition.ToLower().IndexOf("datsearch") + 10, 55);
                    sDate2 = sDate2.Split(new string[] { "and" }, StringSplitOptions.None)[0] + "and" + " '9999-12-31 23:59:59'";
                }
                else        //û�в�ѯʱ��
                {
                    sDate2 = "between '1900-01-01 00:00:00' and '9999-12-31 23:59:59'";
                }
            }

            var defTableName = "KJ_DeviceDefInfo";
            var strKeyFieldPropName = dtMetadata.Rows[0]["strKeyFieldPropName"];
            if (strKeyFieldPropName != null && strKeyFieldPropName.ToString() != "")
            {
                int outValue;
                var suc = int.TryParse(strKeyFieldPropName.ToString(), out outValue);
                if (!suc)
                {
                    defTableName = strKeyFieldPropName.ToString();
                }
            }

            string sAllPointView =
                "SELECT `fzh`, `kh`, `devid`, `wzid`, `point`, `pointid`, `pointid` AS `bid`, `bz6`, `bz7`, `bz8` FROM `" + defTableName + "` WHERE (( `" + defTableName + "`.`DeleteTime` = '1900-01-01 00:00:00' ) OR ( `" + defTableName + "`.`DeleteTime` " +
                sDate2 + " ))";
            string sRemoveDuplicationBefore =
                "SELECT `fzh`, `kh`, `devid`, `wzid`, `point`, `pointid`, `pointid` AS `bid`, `bz6`, `bz7`, `bz8` FROM " + defTableName + " AS c WHERE c.pointid IN ( SELECT ( SELECT pointid FROM " + defTableName + " AS b WHERE b.point = temp.point AND activity = 0 ORDER BY b.DeleteTime DESC LIMIT 1 ) AS point FROM viewjc_mdefsubquerybef AS temp ) UNION ALL SELECT `fzh`, `kh`, `devid`, `wzid`, `point`, `pointid`, `pointid` AS `bid`, `bz6`, `bz7`, `bz8` FROM " + defTableName + " WHERE activity = 1 AND point NOT IN ( SELECT point FROM viewjc_mdefsubquerybef )";
            string sRemoveDuplicationAfter =
                "SELECT `fzh`, `kh`, `devid`, `wzid`, `point`, `pointid`, `pointid` AS `bid`, `bz6`, `bz7`, `bz8` FROM " + defTableName + " AS c WHERE c.pointid IN ( SELECT ( SELECT pointid FROM " + defTableName + " AS b WHERE b.point = temp.point AND activity = 0 ORDER BY b.DeleteTime DESC LIMIT 1 ) AS point FROM viewjc_mdefsubqueryaft AS temp ) UNION ALL SELECT `fzh`, `kh`, `devid`, `wzid`, `point`, `pointid`, `pointid` AS `bid`, `bz6`, `bz7`, `bz8` FROM " + defTableName + " WHERE activity = 1";
            string sDefSubSqlBef =
                "SELECT DISTINCT point FROM " + defTableName + " AS a WHERE a.activity = 0 AND a.`DeleteTime` " + sDate2;
            string sDefSubSqlAft =
                "SELECT DISTINCT point FROM " + defTableName + " AS a WHERE a.activity = 0 AND point NOT IN ( SELECT point FROM " + defTableName + " WHERE activity = 1 ) AND a.`DeleteTime` " + sDate2;

            string sDefMainUseSql = "";
            string sDefSubUseSqlBef = "";
            string sDefSubUseSqlAft = "";
            if (radioButtonStorePoint.Checked)
            {
                if (lookUpEditRemoveDuplication.EditValue == "2")
                {
                    sDefMainUseSql = sRemoveDuplicationBefore;
                    sDefSubUseSqlBef = sDefSubSqlBef;
                }
                else if (lookUpEditRemoveDuplication.EditValue == "3")
                {
                    sDefMainUseSql = sRemoveDuplicationAfter;
                    sDefSubUseSqlAft = sDefSubSqlAft;
                }
                else
                {
                    sDefMainUseSql = sAllPointView;
                }
            }
            else
            {
                sDefMainUseSql = sAllPointView;
            }

            if (Model.GetDbType() == "sqlserver")
            {
                sDefMainUseSql = "go\r\n alter view viewjc_mdef \r\n as\r\n " + sDefMainUseSql;
                if (!string.IsNullOrEmpty(sDefSubUseSqlBef))
                {
                    sDefSubUseSqlBef = "go\r\n alter view viewjc_mdefsubquerybef \r\n as\r\n " + sDefSubUseSqlBef;
                }
                if (!string.IsNullOrEmpty(sDefSubUseSqlAft))
                {
                    sDefSubUseSqlAft = "go\r\n alter view viewjc_mdefsubqueryaft \r\n as\r\n " + sDefSubUseSqlAft;
                }
            }
            if (Model.GetDbType() == "mysql")
            {
                sDefMainUseSql = "alter view viewjc_mdef \r\n as\r\n " + sDefMainUseSql;
                if (!string.IsNullOrEmpty(sDefSubUseSqlBef))
                {
                    sDefSubUseSqlBef = ";alter view viewjc_mdefsubquerybef \r\n as\r\n " + sDefSubUseSqlBef;
                }
                if (!string.IsNullOrEmpty(sDefSubUseSqlAft))
                {
                    sDefSubUseSqlAft = ";alter view viewjc_mdefsubqueryaft \r\n as\r\n " + sDefSubUseSqlAft;
                }
            }

            sAllUpdateSql += sDefMainUseSql + sDefSubUseSqlBef + sDefSubUseSqlAft;

            #endregion

            #region �޸��ձ���ͼ
            var strWhereBySumReport = "";

            if (_listdate != null)      //���û��ѡ������,��ô����������֯sql
            {
                var rows = dtMetadata.Select("blnDay=1");
                //�ж���Ԫ���ǲ����ձ�������ձ�����Ĭ����Ϊ�����治���ж��ٸ������ֶΣ�����ֻ���˵�������ݣ�������ʱ�����ж����ĸ�Ԫ�����ֶ���Ϊ�ձ����������ֶ�
                if (rows != null && rows.Length != 0)
                {
                    var strDayType = TypeUtil.ToString(rows[0]["strDayType"]);
                    if (strDayType.ToLower() == "yyyymm")
                    {
                        //������±�,Ҫ�����±�ĸ�ʽ����֯sql
                        var _listdatecopy = new List<string>();
                        foreach (var strdate in _listdate)
                        {
                            var s = strdate.Substring(0, 6);
                            if (!_listdatecopy.Contains(s))
                                _listdatecopy.Add(s);
                        }
                        _listdate = _listdatecopy;
                    }

                    var strupdatesql = "";
                    if (TypeUtil.ToString(rows[0]["strSrcType"]) == "V")
                    {
                        //������ձ����ҽ���������ͼ,����Ҫ��̬�޸���ͼ��sql//                
                        var dtTable = GetDayTable(dtMetadata);

                        // 20170916
                        if (dtTable.Rows.Count != 0)
                        {
                            var strDayTable = "";
                            foreach (DataRow row in dtTable.Rows)
                                strDayTable += "'" + row["strTableName"] + "',";
                            strDayTable = strDayTable.Substring(0, strDayTable.Length - 1);
                            DataTable dt = null;
                            //�õ���ͼ�Ĵ���sql 
                            if (Model.GetDbType() == "sqlserver") //��Ҫ��취֧��sqlͬʱ��������ͼ�Ľű�
                                dt =
                                    Model.GetDataTable(
                                        "select name as TABLE_NAME,text as Text from sys.views  left join syscomments on sys.views.object_id=syscomments.id where name in(" +
                                        strDayTable + ")");
                            if (Model.GetDbType() == "mysql")
                                dt =
                                    Model.GetDataTable(
                                        "SELECT TABLE_NAME,VIEW_DEFINITION as Text FROM information_schema.VIEWS where TABLE_NAME in(" +
                                        strDayTable + ") and TABLE_SCHEMA='" + Model.GetDBName() + "'");

                            foreach (DataRow row in dtTable.Rows)
                            {
                                var strViewName = row["strTableName"].ToString();
                                var strViewSrcTableName = row["strDayTableName"].ToString();
                                var rowscript = dt.Select("TABLE_NAME='" + strViewName + "'");
                                var strsql = "";
                                for (var i = 0; i < rowscript.Length; i++)
                                {
                                    //dt�������Ǵ�����ͼ�ű���ѭ���õ��ű�

                                    var strvalue = TypeUtil.ToString(rowscript[i]["Text"]);
                                    if (Model.GetDbType() == "sqlserver")
                                        strvalue = strvalue.Substring(strvalue.ToLower().IndexOf("as") + 2);

                                    strsql += strvalue.ToLower();
                                    if (strsql.ToLower().Contains("union "))
                                    {
                                        strsql = strsql.Substring(0, strsql.IndexOf("union"));
                                        break;
                                    }
                                }

                                if (Model.GetDbType() == "sqlserver")
                                    strupdatesql += "go\r\n alter view " + strViewName + " \r\n as\r\n ";
                                if (Model.GetDbType() == "mysql")
                                    strupdatesql += ";alter view " + strViewName + " \r\n as\r\n ";
                                var k = 0;

                                if (strDayType == "")
                                    //2016-10-21 �����ڳ�ű����գ��£���û�зֱ�����jc_ll_dmonthmax��ͼ��Ҫ��ʱ��where��������Ҫͳһ�޸�jc_ll_dmonth��ʱ�䣬�൱���������ձ�
                                    strupdatesql = strupdatesql + strsql + "\r\n union all\r\n ";
                                else
                                    foreach (var s in _listdate)
                                    {
                                        var strSrcTableNames = strViewSrcTableName.Split(',');

                                        // 20180312
                                        //�ж��Ƿ���ڱ�
                                        //if ((_listdate.Count > 1) && !Model.blnExistsTable(strViewSrcTableName + s))
                                        //    //������ڶδ���1�죬��ѡ������������ݿ��в����ڣ���ֱ�������˱�
                                        //    continue;
                                        var lisTables = new List<string>();
                                        foreach (var item in strSrcTableNames)
                                        {
                                            lisTables.Add(item + s);
                                        }

                                        if ((_listdate.Count > 1) && !Model.IfExistTables(lisTables))
                                            continue;

                                        foreach (var strSrcTableName in strSrcTableNames)
                                        {
                                            var strname = strsql.Substring(
                                                strsql.IndexOf(strSrcTableName) + strSrcTableName.Length, strDayType.Length);
                                            if (TypeUtil.ToInt(strname) > 0)
                                            {
                                                var strDataBaseTable = strSrcTableName + s;
                                                if ((_listdate.Count == 1) && !Model.blnExistsTable(strDataBaseTable))
                                                {
                                                    gridControl.DataSource = null;
                                                    throw new Exception("�������������ݣ�");
                                                }
                                                strsql = strsql.Replace(strSrcTableName + strname, strSrcTableName + s + "");
                                            }
                                            else
                                            {
                                                strsql = strsql.Replace(strSrcTableName, strSrcTableName + s + "");
                                            }
                                        }

                                        strupdatesql = strupdatesql + strsql + "\r\n union all\r\n ";
                                    }

                                if (strupdatesql.Contains("union all"))
                                {
                                    strupdatesql = strupdatesql.Substring(0, strupdatesql.Length - 12);
                                    if (_strFreQryCondition.ToLower().IndexOf("datsearch") > 0)
                                    {
                                        //�õ���ѯ���ڵ������ַ���
                                        //var strdate = _strFreQryCondition.Substring(_strFreQryCondition.ToLower().IndexOf("datsearch") + 10, 55);
                                        var strdate = _freQryCondition.GetFreQryCondition().Substring(_strFreQryCondition.ToLower().IndexOf("datsearch") + 10, 55);
                                        var strsqldate = "";
                                        var strsqlCreateView = strupdatesql.ToLower().Substring(0, strupdatesql.ToLower().IndexOf("as") + 2) + "\r\n";
                                        var strselectsql = strupdatesql.ToLower().Substring(strupdatesql.ToLower().IndexOf("as") + 2);
                                        strselectsql = GetSqlByDeleteNull(strselectsql);

                                        var strSqlArray = TypeUtil.GetSubStrCountInStr(strselectsql, "between", 0);
                                        foreach (var startindex in strSqlArray)
                                        {
                                            strsqldate = strselectsql.Substring(startindex, 55);
                                            strupdatesql = strsqlCreateView + strselectsql.Replace(strsqldate, strdate);
                                        }
                                    }

                                    #region

                                    ////2015-09-08  �����豸״̬ѡ���Ӱ���ۼƴ������ۼ�����

                                    //if ((_strFreQryCondition.ToLower().IndexOf("state in") > 0) ||
                                    //    (strupdatesql.Replace("`", "").ToLower().IndexOf(".state") > 0) ||
                                    //    (strupdatesql.Replace("`", "").ToLower().IndexOf(", state") > 0))
                                    //{
                                    //    //��if�����豸״̬�ֶ�,�����豸״̬Ҫ��group by��where����,����b����typeΪ10���������state�ֶ���21��24���֣���ô����û�ֻѡ��21��state����ô��������Ҳֻ��ֻ��21������,ͬʱ��������Ҳ�����
                                    //    strupdatesql = strupdatesql.Replace("`", "");
                                    //    var strUIDevStatus = " state <> 12345"; //����û�û��ѡ����ƾ�豸״̬,��ô��һ��Ĭ��ֵ�����ں���֯sql
                                    //    var endindex = 0;
                                    //    if (_strFreQryCondition.ToLower().Contains("state in"))
                                    //    {
                                    //        //�������������豸״̬������ȡ����
                                    //        var strUIstaus =
                                    //            _strFreQryCondition.ToLower()
                                    //                .Substring(_strFreQryCondition.ToLower().IndexOf("state in"));
                                    //        endindex = strUIstaus.IndexOf(")");
                                    //        strUIDevStatus = strUIstaus.Substring(0, endindex + 1);
                                    //    }

                                    //    var strselectsql = strupdatesql.ToLower()
                                    //        .Substring(strupdatesql.ToLower().IndexOf("as") + 2);
                                    //    strselectsql = GetSqlByDeleteNull(strselectsql);
                                    //    if (strupdatesql.Contains("state in") || strupdatesql.Contains("state =") ||
                                    //        strupdatesql.Contains("state <> 12345"))
                                    //    {
                                    //        var strsqlstaus = "";
                                    //        if (strupdatesql.Contains("state in"))
                                    //        {
                                    //            //�õ���ͼ������豸״̬����
                                    //            strsqlstaus =
                                    //                strselectsql.ToLower().Substring(strselectsql.ToLower().IndexOf("state in"));
                                    //            endindex = strsqlstaus.IndexOf(")");
                                    //        }
                                    //        if (strupdatesql.Contains("state ="))
                                    //        {
                                    //            //���û�ֻѡ��һ���豸״̬��ʱ�򣬴���ͼ��ʱ��mysql�Զ����in��Ϊ=������Ҫ����������ҷ���������
                                    //            strsqlstaus =
                                    //                strselectsql.ToLower().Substring(strselectsql.ToLower().IndexOf("state ="));
                                    //            endindex = strsqlstaus.IndexOf(")") - 1;
                                    //        }
                                    //        if (strupdatesql.Contains("state <> 12345"))
                                    //        {
                                    //            //���û�һֱû��ѡ��������ʱ��
                                    //            strsqlstaus =
                                    //                strselectsql.ToLower()
                                    //                    .Substring(strselectsql.ToLower().IndexOf("state <> 12345"));
                                    //            endindex = strsqlstaus.IndexOf(")") - 1;
                                    //        }
                                    //        var strsqlDevstatus = strsqlstaus.Substring(0, endindex + 1);

                                    //        var strsqlCreateView =
                                    //            strupdatesql.ToLower().Substring(0, strupdatesql.ToLower().IndexOf("as") + 2) +
                                    //            "\r\n";
                                    //        strupdatesql = strsqlCreateView + strselectsql.Replace(strsqlDevstatus, strUIDevStatus);
                                    //        //���������豸״̬�����ý����ϳ��������豸״̬�滻

                                    //        if ((listExvo.StrDescription == "���ܱ�") && (strWhereBySumReport == ""))
                                    //            strWhereBySumReport = _strFreQryCondition.Replace(strUIDevStatus, "state in(12345)");
                                    //    }
                                    //}
                                    //else
                                    //    ExecMoreSql(strupdatesql);

                                    #endregion
                                }
                                else
                                {
                                    throw new Exception("�������������ݣ�");
                                }
                            }
                        }

                        sAllUpdateSql += strupdatesql;
                    }
                }
            }

            #endregion

            if (sAllUpdateSql.Length > 0)
                ExecMoreSql(sAllUpdateSql);
            if (strWhereBySumReport != "")
                _strFreQryCondition = strWhereBySumReport;
        }

        /// <summary>
        ///     ִ�ж���sql���
        /// </summary>
        private void ExecMoreSql(string strupdatesql)
        {
            var strsqls = strupdatesql.Replace("go", "��").Split('��');
            foreach (var strsql in strsqls)
                if (Convert.ToString(strsql).Length > 10)
                    Model.ExecuteSQL(strsql);
        }

        private void tlbPrint_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                if (!PermissionManager.HavePermission("PrintReport"))
                {
                    return;
                }

                var strShowStyle = TypeUtil.ToString(lookupShowStyle.EditValue);
                if (strShowStyle == "pivot")
                    PrintPivot();
                else if (strShowStyle == "chart")
                    PrintChart();
                else
                    lookupShowStyle.EditValue = "pivot";
            }
            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        /// <summary>
        ///     ��ӡGrid����
        /// </summary>
        private void PrintGrid()
        {
            gridView.OptionsPrint.PrintHeader = true;
            gridView.OptionsPrint.PrintFooter = true;

            gridView.PrintInitialize += gridView_PrintInitialize;
            gridView.ShowRibbonPrintPreview();
            gridView.PrintInitialize -= gridView_PrintInitialize;
        }

        private void gridView_PrintInitialize(object sender, PrintInitializeEventArgs e)
        {
            //�����ӡ��ʱ����˱�ͷ����ô������ʱ��Ҳ����
            //PageHeaderFooter phf = e.Link.PageHeaderFooter as PageHeaderFooter;
            //phf.Header.Content.Clear();
            //phf.Header.Content.AddRange(new string[] { "", this.lblTile.Text, "" });
            //phf.Header.LineAlignment = BrickAlignment.Center;
        }

        /// <summary>
        ///     ��ӡ����������
        /// </summary>
        private void PrintPivot()
        {
            rp.PrintDialog();
            //rp.ShowRibbonPreview();
        }

        /// <summary>
        ///     ��ӡͼ������
        /// </summary>
        private void PrintChart()
        {
            chartControl.ShowPrintPreview();
        }

        private void tlbDesign_Popup(object sender, EventArgs e)
        {
            try
            {
                if (xapp == null) return;
                tlbDesign.Reset();
                BarCheckItem item;
                foreach (var o in xapp.FormatNames)
                {
                    item = new BarCheckItem(barManager, false);
                    item.Caption = o.ToString();
                    item.Name = o.ToString();
                    item.ItemClick += item_ItemClick;
                    menuListStyle.AddItems(new BarItem[] { item });
                }
                SetListStyle(strListStyle);
            }

            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        private void SetListStyle(string strStyleName)
        {
            try
            {
                if (string.IsNullOrEmpty(strStyleName))
                    strStyleName = "����";
                var itemCount = menuListStyle.ItemLinks.Count;
                for (var i = 0; i < itemCount; i++)
                    if (menuListStyle.ItemLinks[i].Item.Caption == strStyleName)
                    {
                        ((BarCheckItem)menuListStyle.ItemLinks[i].Item).Checked = true;
                        xapp.LoadScheme(strStyleName, gridView);
                        strListStyle = strStyleName;
                        break;
                    }
                for (var i = 0; i < itemCount; i++)
                    if (((BarCheckItem)menuListStyle.ItemLinks[i].Item).Checked &&
                        (menuListStyle.ItemLinks[i].Item.Caption != strStyleName))
                    {
                        ((BarCheckItem)menuListStyle.ItemLinks[i].Item).Checked = false;
                        break;
                    }
            }
            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        private void item_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                if (((BarCheckItem)e.Item).Checked)
                {
                    xapp.LoadScheme(e.Item.Caption, gridView);
                    strListStyle = e.Item.Caption;
                    var itemCount = menuListStyle.ItemLinks.Count;
                    for (var i = 0; i < itemCount; i++)
                        if (((BarCheckItem)menuListStyle.ItemLinks[i].Item).Checked &&
                            (menuListStyle.ItemLinks[i].Item.Caption != e.Item.Caption))
                        {
                            ((BarCheckItem)menuListStyle.ItemLinks[i].Item).Checked = false;
                            break;
                        }
                }
            }

            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        /// <summary>
        ///     ����ȫѡ
        /// </summary>
        private void tlbMultiSelection_CheckedChanged(object sender, ItemClickEventArgs e)
        {
            var rowCount = gridView.RowCount;
            var rowChecked = false;
            rowChecked = tlbMultiSelection.Checked;
            for (var i = 0; i < rowCount; i++)
                gridView.SetRowCellValue(i, "MultiSelect", rowChecked);
        }

        /// <summary>
        ///     ���û�����ÿҳ����ʾ�������ı�ʱ��ִ��ˢ��
        /// </summary>
        private void tlbSetPerPageNumber_EditValueChanged(object sender, EventArgs e)
        {
            try
            {
                wdf = new WaitDialogForm("�����л���", "��ȴ�....");
                SetExecuteBeginTime();
                if (blnRefreshed)
                {
                    //PerPageRecord = TypeUtil.ToInt(tlbSetPerPageNumber.EditValue);
                    PerPageRecord = TypeUtil.ToInt(comboBoxEdit1.Text);

                    InitListData();
                }

                MessageShowUtil.ShowStaticInfo("��������ִ��ʱ��Ϊ" + GetExecuteTimeString(), barStaticItemMsg);
            }
            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
            finally
            {
                wdf.Close();
            }
        }

        /// <summary>
        ///     ���һҳ��ť
        /// </summary>
        private void tlbGoToFirstPage_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                SetExecuteBeginTime();
                if (lookupShowStyle.EditValue == "pivot")
                {
                    SetReportPage(0);
                    return;
                }


                if (PerPageRecord == 0 || TotalPage == 0)
                    return;

                CurrentPageNumber = 1;

                var strListSql = listDataExVo.StrListSQL.Replace("where 1=1",
                    "where 1=1 " + _strFreQryCondition + _strReceiveParaCondition + _strSortCondtion);
                GetDistinctSql(ref strListSql);
                dt = Model.GetPageData(strListSql, 1, PerPageRecord);


                BandingData();

                SetListPageInfo();

                MessageShowUtil.ShowStaticInfo("���ص�һҳ��ִ��ʱ��Ϊ" + GetExecuteTimeString(), barStaticItemMsg);
            }

            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        /// <summary>
        ///     ����һҳ��ť
        /// </summary>
        private void tlbGoToPreviousPage_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                SetExecuteBeginTime();
                if (lookupShowStyle.EditValue == "pivot")
                {
                    SetReportPage(documentViewer1.SelectedPageIndex - 1);
                    return;
                }


                if (PerPageRecord == 0)
                    return;

                var StartRecord = (CurrentPageNumber - 1) * PerPageRecord;
                if (StartRecord > PerPageRecord - 1)
                {
                    CurrentPageNumber--;

                    var strListSql = listDataExVo.StrListSQL.Replace("where 1=1",
                        "where 1=1 " + _strFreQryCondition + _strReceiveParaCondition + _strSortCondtion);
                    GetDistinctSql(ref strListSql);
                    dt = Model.GetPageData(strListSql, CurrentPageNumber, PerPageRecord);


                    BandingData();

                    SetListPageInfo();
                }
                else
                {
                    MessageShowUtil.ShowInfo("�ѵ���һҳ");
                }


                MessageShowUtil.ShowStaticInfo("������һҳ��ִ��ʱ��Ϊ" + GetExecuteTimeString(), barStaticItemMsg);
            }

            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        /// <summary>
        ///     ����һҳ��ť
        /// </summary>
        private void tlbGoToNextPage_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                SetExecuteBeginTime();

                if (lookupShowStyle.EditValue == "pivot")
                {
                    SetReportPage(documentViewer1.SelectedPageIndex + 1);
                    return;
                }

                if (PerPageRecord == 0)
                    return;

                var StartRecord = CurrentPageNumber * PerPageRecord;
                if (StartRecord < TotalRecord)
                {
                    CurrentPageNumber++;

                    var strListSql = listDataExVo.StrListSQL.Replace("where 1=1",
                        "where 1=1 " + _strFreQryCondition + _strReceiveParaCondition + _strSortCondtion);
                    GetDistinctSql(ref strListSql);
                    dt = Model.GetPageData(strListSql, CurrentPageNumber, PerPageRecord);

                    BandingData();
                    SetListPageInfo();
                }
                else
                {
                    MessageShowUtil.ShowInfo("�ѵ����һҳ");
                }

                MessageShowUtil.ShowStaticInfo("������һҳ��ִ��ʱ��Ϊ" + GetExecuteTimeString(), barStaticItemMsg);
            }

            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        /// <summary>
        ///     �����һҳ��ť
        /// </summary>
        private void tlbGoToLastPage_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                SetExecuteBeginTime();

                if (lookupShowStyle.EditValue == "pivot")
                {
                    SetReportPage(rp.Pages.Count - 1);
                    return;
                }

                if (PerPageRecord == 0 || TotalPage == 0)
                    return;

                var StartRecord = (TotalPage - 1) * PerPageRecord;
                if (StartRecord < TotalRecord)
                {
                    CurrentPageNumber = TotalPage;

                    var strListSql = listDataExVo.StrListSQL.Replace("where 1=1",
                        "where 1=1 " + _strFreQryCondition + _strReceiveParaCondition + _strSortCondtion);
                    GetDistinctSql(ref strListSql);
                    dt = Model.GetPageData(strListSql, CurrentPageNumber, PerPageRecord);

                    BandingData();

                    SetListPageInfo();
                }
                else
                {
                    MessageShowUtil.ShowInfo("�ѵ����һҳ");
                }

                MessageShowUtil.ShowStaticInfo("�������һҳ��ִ��ʱ��Ϊ" + GetExecuteTimeString(), barStaticItemMsg);
            }

            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        /// <summary>
        ///     ��GOTO��ť
        /// </summary>
        /// <param name="sender"></param>
        private void tlbGoToPage_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                SetExecuteBeginTime();

                // 20170622
                //if (tlbSetCurrentPage.EditValue == null)
                //{
                //    return;
                //}
                if (textEdit1.EditValue == null)
                {
                    return;
                }


                //var currPage = tlbSetCurrentPage.EditValue.ToString().Split('/');
                var currPage = textEdit1.EditValue.ToString().Split('/');
                CurrentPageNumber = TypeUtil.ToInt(currPage[0]);
                if (CurrentPageNumber > TotalPage)
                    CurrentPageNumber = TotalPage;
                else if ((0 == CurrentPageNumber) && (TotalPage > 0))
                    CurrentPageNumber = 1;

                var strListSql = listDataExVo.StrListSQL.Replace("where 1=1",
                    "where 1=1 " + _strFreQryCondition + _strReceiveParaCondition + _strSortCondtion);
                GetDistinctSql(ref strListSql);
                if (TotalPage <= 1)
                    dt = Model.GetDataTable(strListSql);
                else
                    dt = Model.GetPageData(strListSql, CurrentPageNumber, PerPageRecord);
                BandingData();
                SetListPageInfo();
                if (lookupShowStyle.EditValue == "pivot")
                    SetReportPage(TypeUtil.ToInt(currPage[0]) - 1);
                MessageShowUtil.ShowStaticInfo("����ִ��ʱ��Ϊ" + GetExecuteTimeString(), barStaticItemMsg);
            }

            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        /// <summary>
        ///     ������ʾ����ҳ���ı�ʱ
        /// </summary>
        private void SetListPageInfo()
        {
            try
            {
                if (lookupShowStyle.EditValue == "pivot")
                {
                    SetReportPage(0);
                    return;
                }
                if (PerPageRecord == 0)
                {
                    TotalPage = 1;
                }
                else
                {
                    TotalPage = TotalRecord / PerPageRecord;
                    if (TotalRecord % PerPageRecord > 0)
                        TotalPage = TotalPage + 1;
                }
                CurrentPageText.Mask.EditMask = @"([1-9]|[1-9]\d+)/" + TotalPage;
                //tlbSetCurrentPage.EditValue = CurrentPageNumber + "/" + TotalPage;
                textEdit1.Text = CurrentPageNumber + "/" + TotalPage;
            }

            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        /// <summary>
        ///     ���û��ı�ҳ��ʱ
        /// </summary>
        private void tlbSetCurrentPage_EditValueChanged(object sender, EventArgs e)
        {
            try
            {
                //var currPageNumber = tlbSetCurrentPage.EditValue.ToString().Split('/');
                var currPageNumber = textEdit1.Text.ToString().Split('/');
                if (lookupShowStyle.EditValue == "pivot")
                {
                    if (Convert.ToInt32(currPageNumber[0]) > rp.Pages.Count)
                        //tlbSetCurrentPage.EditValue = rp.Pages.Count + "/" + rp.Pages.Count;
                        textEdit1.Text = rp.Pages.Count + "/" + rp.Pages.Count;
                    return;
                }

                if (Convert.ToInt32(currPageNumber[0]) > TotalPage)
                    //tlbSetCurrentPage.EditValue = TotalPage + "/" + TotalPage;
                    textEdit1.Text = TotalPage + "/" + TotalPage;
            }

            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        /// <summary>
        ///     ����ҳ
        /// </summary>
        private void SetReportPage(int TypeID)
        {
            if (dt == null) return;
            if (TypeID == -1)
            {
                MessageShowUtil.ShowInfo("�ѵ���һҳ");
                return;
            }
            if (TypeID == rp.Pages.Count)
            {
                MessageShowUtil.ShowInfo("�ѵ����һҳ");
                return;
            }

            documentViewer1.SelectedPageIndex = TypeID;


            CurrentPageText.Mask.EditMask = @"([1-9]|[1-9]\d+)/" + rp.Pages.Count;
            //tlbSetCurrentPage.EditValue = documentViewer1.SelectedPageIndex + 1 + "/" + rp.Pages.Count;

            textEdit1.Text = documentViewer1.SelectedPageIndex + 1 + "/" + rp.Pages.Count;
        }

        /// <summary>
        ///     ����
        /// </summary>
        private void tlbSchema_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                if (!PermissionManager.HavePermission("OpenReportScheme"))
                {
                    return;
                }
                var frm = new frmSchemaDesign();
                frm.ListID = listId;
                frm.ListDataID = ListDataID;
                frm.CurListDataId = ListDataID;
                frm.MetadataId = listExvo.MainMetaDataID;
                frm.LmdDt = _lmdDt;
                frm.BlnListEnter = true;
                frm.StrListName = listExvo.StrListName;
                frm.ShowDialog();

                if (frm.BlnOk)
                {
                    //�л�����

                    var blnNew = false;
                    if (ListDataID != frm.ListDataID)
                    {
                        blnNew = true;
                        ListDataID = frm.ListDataID;
                    }
                    listDataExVo = frm.ListDataExVo;
                    _lmdDt = frm.LmdDt;
                    if (frm.ListDisplayExList != null)
                        listDisplayExList = frm.ListDisplayExList;


                    if ((listDataExVo != null) && (listDataExVo.StrDefaultShowStyle != string.Empty))
                        listTemplevo = Model.GetListTemple(listDataExVo.ListDataID);

                    SwitchSchema(blnNew);
                }

                lookupSchema.Properties.DataSource = Model.GetSchemaList(listExvo.ListID);
                lookupSchema.EditValueChanged -= lookupSchema_EditValueChanged;
                lookupSchema.EditValue = ListDataID;
                lookupSchema.EditValueChanged += lookupSchema_EditValueChanged;
            }

            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        /// <summary>
        ///     ����ѡ��
        /// </summary>
        private void lookupSchema_EditValueChanged(object sender, EventArgs e)
        {
            try
            {
                var id = TypeUtil.ToInt(lookupSchema.EditValue);
                if (id > 0)
                {
                    //���뷽��

                    ListDataID = id;
                    _lmdDt = null;
                    GetListDataEx(); //��ȡ�б�������
                    GetListDisplay(); //��ȡ�б���ʾ����                   
                    SwitchSchema(true);
                }

                InitializeArrangeTime();
            }

            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        /// <summary>
        ///     �л�����
        /// </summary>
        /// <param name="blnNew">�Ƿ�Ϊ�·���</param>
        private void SwitchSchema(bool blnNew)
        {
            try
            {
                OpenWaitDialog("�����л��б�����");

                SetExecuteBeginTime();

                SetListFormat(); //���ݷ��������б��ʽ  
                // if (!blnNew || blnFirstOpenLoadData)               
                //{


                // 20170628
                //if (blnRefreshed)
                //{
                //    InitListData();
                //}
                ////}
                //else
                //{
                //    //ClearOldSchemaData

                //    if ((dt != null) && blnNew)
                //        dt.Rows.Clear();

                //    blnRefreshed = false;
                //}

                if (chkItemFreQryCondition.Checked && (_freQryCondition != null))
                {
                    _freQryCondition.CreateControl(_lmdDt);

                    SetFrmQryConditionSize();
                }

                RefreshListData();
            }

            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
            finally
            {
                CloseWaitDialog();
            }

            MessageShowUtil.ShowStaticInfo("�б��л���ʱ��Ϊ" + GetExecuteTimeString(), barStaticItemMsg);
        }

        /// <summary>
        ///     �б���ʽ�л�
        /// </summary>
        private void lookupShowStyle_EditValueChanged(object sender, EventArgs e)
        {
            try
            {
                wdf = new WaitDialogForm("�����л���...", "��ȴ�");
                var strShowStyle = string.Empty;
                if (lookupShowStyle.EditValue != null)
                {
                    SetListFormat();
                    BandingDataToControl();
                    SetListPageInfo();
                }
            }
            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
            finally
            {
                wdf.Close();
            }
        }

        /// <summary>
        ///     ��������
        /// </summary>
        private void tlbPivotSetting_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                if (!PermissionManager.HavePermission("OpenReportDesign"))
                {
                    return;
                }

                if (listDataExVo == null)
                {
                    MessageShowUtil.ShowInfo("��ѡ���б���");
                    return;
                }

                // 20170622
                if (dt == null)
                {
                    MessageShowUtil.ShowInfo("���Ȳ�ѯ���ݡ�");
                    return;
                }

                if (TypeUtil.ToString(lookupShowStyle.EditValue) == "pivot")
                {
                    var strFileName = AppDomain.CurrentDomain.BaseDirectory + "Config\\ReportTemple\\" +
                                      listDataExVo.StrListDataName + ListDataID + ".repx";
                    var frm = new frmReportDesign();
                    frm.StrFileName = strFileName;
                    frm.dtReportDataSource = dt;
                    frm.dtLmd = _lmdDt;
                    frm.listDataExDTO = listDataExVo;
                    frm.ShowDialog();
                    if (frm.blnOK)
                    {
                        //��������˱���ģ�壬��ô��Ҫ���¼���ģ��
                        listTemplevo = Model.GetListTemple(listDataExVo.ListDataID);
                        lookupShowStyle_EditValueChanged(null, null);
                    }
                }

                //��Ҫʵ��
                //frmListExPivotConfig frm = new frmListExPivotConfig();
                //frm.ListDataExVo = this.listDataExVo;
                //if (this._lmdDt != null)
                //{
                //    frm.LmdDt = this._lmdDt.Copy();
                //}
                //frm.ShowDialog();

                //SetPivotFormat();
            }

            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        /// <summary>
        ///     ͼ������
        /// </summary>
        private void tlbChartSetting_ItemClick(object sender, ItemClickEventArgs e)
        {
            int a;
            try
            {
                if (listDataExVo == null)
                    MessageShowUtil.ShowInfo("��ѡ���б���");

                //��Ҫʵ��
            }
            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        #region ͼ������

        public void SetChartFormat()
        {
            //��Ҫʵ��
        }

        #endregion

        /// <summary>
        ///     ֧�ָ���
        /// </summary>
        private void chkItemAllowCopy_CheckedChanged(object sender, ItemClickEventArgs e)
        {
            //Dictionary<string, string> point1 = new Dictionary<string, string>();
            //try
            //{
            //    string point = "001A010";
            //    if (!string.IsNullOrEmpty(point))
            //    {
            //        point1.Add("SourceIsList", "true");
            //        point1.Add("Key_viewsbrunlogreport1_point", "����&&$" + point);
            //        point1.Add("Display_viewsbrunlogreport1_point", "����&&$" + point);
            //    }
            //    point1.Add("ListID", "27");
            //    frmList frm = new frmList(point1);
            //    frm.ShowDialog();
            //}
            //catch (Exception ex)
            //{

            //}


            var blnAllowCopy = chkItemAllowCopy.Checked;

            var colCount = gridView.Columns.Count;
            for (var i = 0; i < colCount; i++)
            {
                if (gridView.Columns[i].Tag != null)
                    continue;

                gridView.Columns[i].OptionsColumn.AllowEdit = blnAllowCopy;
            }
        }

        /// <summary>
        ///     ���ò�ѯ����
        /// </summary>
        private void chkItemFreQryCondition_CheckedChanged(object sender, ItemClickEventArgs e)
        {
            if (chkItemFreQryCondition.Checked)
            {
                layoutControlGroupFreQry.Visibility = LayoutVisibility.Always;

                if (_freQryCondition == null)
                {
                    _freQryCondition = new FreQryCondition(panelFreQry);
                    _freQryCondition.PointFilter = panelControlPointFilter;
                    _freQryCondition.CalcControlWidth(2);//20190119
                    _freQryCondition.CreateControl(_lmdDt);
                }

                SetFrmQryConditionSize();
            }
            else
            {
                layoutControlGroupFreQry.Visibility = LayoutVisibility.Never;
            }
        }

        /// <summary>
        ///     ���泣�ò�ѯ����
        /// </summary>
        private void tlbSaveFreQryCon_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                if (!PermissionManager.HavePermission("SaveReportFreQryCon"))
                {
                    return;
                }

                var strFieldName = "";
                var strKey = "";
                var strDisplay = "";
                var controlCount = panelFreQry.Controls.Count;
                IList<ListmetadataInfo> lmdList = new List<ListmetadataInfo>();
                ListmetadataInfo lmd = null;
                DataRow[] drs = null;
                DataRow dr = null;
                for (var i = 0; i < controlCount; i++)
                {
                    strKey = "";
                    strDisplay = "";
                    strFieldName = "";
                    var hash = (Hashtable)panelFreQry.Controls[i].Tag;
                    if (hash.ContainsKey("_fieldName"))
                        strFieldName = TypeUtil.ToString(hash["_fieldName"]);
                    if (strFieldName == string.Empty)
                        continue;

                    if (panelFreQry.Controls[i] is UCRef)
                    {
                        if (TypeUtil.ToString(hash["_strKeyValue"]) != string.Empty)
                        {
                            strKey += "����&&$" + TypeUtil.ToString(hash["_strKeyValue"]);
                            strDisplay += "����&&$" + TypeUtil.ToString(hash["_strDisplayValue"]);
                        }
                    }
                    else //UCDateTimeOne || UCText || UCNumber
                    {
                        strKey = TypeUtil.ToString(hash["_strKeyValue"]);
                    }


                    drs = _lmdDt.Select("MetaDataFieldName='" + strFieldName + "'");
                    if (drs.Length != 1)
                        continue;

                    dr = drs[0];
                    lmd = new ListmetadataInfo();
                    lmd.InfoState = InfoState.Modified;
                    lmd.ID = TypeUtil.ToInt(dr["ID"]);
                    lmd.ListDataID = ListDataID;
                    lmd.MetaDataID = TypeUtil.ToInt(dr["MetaDataID"]);
                    lmd.MetaDataFieldID = TypeUtil.ToInt(dr["MetaDataFieldID"]);
                    lmd.MetaDataFieldName = TypeUtil.ToString(dr["MetaDataFieldName"]);
                    lmd.LngParentFieldID = TypeUtil.ToInt(dr["lngParentFieldID"]);
                    lmd.LngRelativeFieldID = TypeUtil.ToInt(dr["lngRelativeFieldID"]);
                    lmd.StrTableAlias = TypeUtil.ToString(dr["strTableAlias"]);
                    lmd.StrFullPath = TypeUtil.ToString(dr["strFullPath"]);
                    lmd.StrParentFullPath = TypeUtil.ToString(dr["strParentFullPath"]);
                    lmd.LngAliasCount = TypeUtil.ToInt(dr["lngAliasCount"]);
                    lmd.LngSourceType = TypeUtil.ToInt(dr["lngSourceType"]);
                    lmd.LngParentID = TypeUtil.ToInt(dr["lngParentID"]);
                    lmd.StrFieldType = TypeUtil.ToString(dr["strFieldType"]);
                    lmd.StrFkCode = TypeUtil.ToString(dr["strFkCode"]);
                    lmd.IsCalcField = TypeUtil.ToBool(dr["isCalcField"]);
                    lmd.StrFormula = TypeUtil.ToString(dr["strFormula"]);
                    lmd.StrRefColList = TypeUtil.ToString(dr["strRefColList"]);
                    lmd.LngOrder = TypeUtil.ToInt(dr["lngOrder"]);
                    lmd.LngOrderMethod = TypeUtil.ToInt(dr["lngOrderMethod"]);
                    lmd.StrCondition = TypeUtil.ToString(dr["strCondition"]);
                    lmd.StrConditionCHS = TypeUtil.ToString(dr["strConditionCHS"]);
                    lmd.LngKeyFieldType = TypeUtil.ToInt(dr["lngKeyFieldType"]);

                    lmd.BlnFreCondition = TypeUtil.ToBool(dr["blnFreCondition"]);
                    lmd.LngFreConIndex = TypeUtil.ToInt(dr["lngFreConIndex"]);
                    lmd.StrFreCondition = strKey;
                    lmd.StrFreConditionCHS = strDisplay;

                    lmd.BlnSysProcess = TypeUtil.ToBool(dr["blnSysProcess"]);
                    lmd.BlnShow = TypeUtil.ToBool(dr["blnShow"]);

                    lmdList.Add(lmd);
                }

                if (lmdList.Count > 0)
                    Model.SaveListMetaData(lmdList);

                MessageShowUtil.ShowInfo("�����б��������ɹ�");
            }
            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        private void tlbSaveWidth_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                if (!PermissionManager.HavePermission("SaveReportColumnWidth"))
                {
                    return;
                }

                gridView.CloseEditor();
                gridView.UpdateColumnsCustomization();

                IList<ListdisplayexInfo> ldedto = Model.GetListDisplayExData(ListDataID);
                var lngRowIndex = 0;
                foreach (GridColumn column in gridView.VisibleColumns)
                    foreach (var dto in ldedto)
                        if (column.FieldName == dto.StrListDisplayFieldName)
                        {
                            dto.InfoState = InfoState.Modified;
                            if (column.VisibleIndex >= 0)
                            {
                                dto.LngRowIndex = lngRowIndex + 1;
                                lngRowIndex += 1;
                            }
                            else
                            {
                                dto.LngRowIndex = -1;
                            }
                            dto.LngDisplayWidth = column.Width;
                        }
                Model.SaveListDisplayEx(ldedto);


                IList<ListmetadataInfo> lmddto = Model.GetListMetaDataData(ListDataID);

                foreach (GridColumn column in gridView.Columns)
                    foreach (var dto in lmddto)
                        if (column.FieldName == dto.MetaDataFieldName)
                        {
                            dto.InfoState = InfoState.Modified;
                            if (column.VisibleIndex >= 0)
                                dto.BlnShow = true;
                            else
                                dto.BlnShow = false;
                        }
                Model.SaveListMetaData(lmddto);
                _lmdDt = Model.GetListMetaData(ListDataID, listDataExVo.UserID);
                MessageShowUtil.ShowInfo("�����б���Ŀ��ȳɹ�");
            }
            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        /// <summary>
        ///     ��ȡ���ò�ѯ���������ֶ���
        /// </summary>
        private string GetFreQryConditionByField(string strParaFieldName)
        {
            var strConn = "";
            try
            {
                var strFieldType = "";
                var strFieldName = "";
                var strFkCode = "";
                var strKey = "";
                var strDisplay = "";
                var controlCount = panelFreQry.Controls.Count;
                for (var i = 0; i < controlCount; i++)
                {
                    strFieldName = "";
                    var hash = (Hashtable)panelFreQry.Controls[i].Tag;
                    if (hash.ContainsKey("_fieldName"))
                        strFieldName = TypeUtil.ToString(hash["_fieldName"]);
                    if (strFieldName != strParaFieldName)
                        continue;

                    if (hash.ContainsKey("_fieldType"))
                        strFieldType = TypeUtil.ToString(hash["_fieldType"]);
                    if (strFieldType == string.Empty)
                        break;

                    if (panelFreQry.Controls[i] is UCRef)
                    {
                        if (TypeUtil.ToString(hash["_strKeyValue"]) != string.Empty)
                        {
                            strFkCode = TypeUtil.ToString(hash["_fkCode"]);
                            strKey += "����&&$" + TypeUtil.ToString(hash["_strKeyValue"]);
                            strDisplay += "����&&$" + TypeUtil.ToString(hash["_strDisplayValue"]);
                        }
                    }
                    else //UCDateTimeOne || UCText || UCNumber
                    {
                        strKey = TypeUtil.ToString(hash["_strKeyValue"]);
                    }

                    var str = "";
                    if (strKey != string.Empty)
                    {
                        if (strFkCode != string.Empty)
                            str = BulidConditionUtil.GetRefCondition(strFieldName.Replace("_", "."), strKey,
                                strFieldType);
                        else
                            str = BulidConditionUtil.GetConditionString(strFieldName.Replace("_", "."), strFieldType,
                                strKey);
                        if (str != string.Empty)
                            strConn += " and " + str;
                    }

                    break;
                }
            }
            catch (Exception)
            {
            }

            return strConn;
        }

        /// <summary>
        ///     ��ȡ���ò�ѯ���������ֶ���
        /// </summary>
        private void GetFreQryConditionByField(string strParaFieldName, ref string strKey, ref string strDisplay)
        {
            try
            {
                var strFieldName = "";
                var controlCount = panelFreQry.Controls.Count;
                for (var i = 0; i < controlCount; i++)
                {
                    strFieldName = "";
                    var hash = (Hashtable)panelFreQry.Controls[i].Tag;
                    if (hash.ContainsKey("_fieldName"))
                        strFieldName = TypeUtil.ToString(hash["_fieldName"]);
                    if (strFieldName != strParaFieldName)
                        continue;

                    if (panelFreQry.Controls[i] is UCRef)
                    {
                        if (TypeUtil.ToString(hash["_strKeyValue"]) != string.Empty)
                        {
                            strKey += "����&&$" + TypeUtil.ToString(hash["_strKeyValue"]);
                            strDisplay += "����&&$" + TypeUtil.ToString(hash["_strDisplayValue"]);
                        }
                    }
                    else //UCDateTimeOne || UCText || UCNumber
                    {
                        strKey = TypeUtil.ToString(hash["_strKeyValue"]);
                        strDisplay = strKey;
                    }

                    break;
                }
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        ///     ����ΪĬ����ʾ��ʽ
        /// </summary>
        private void tlbSetDefaultStyle_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                if (lookupShowStyle.EditValue != null)
                {
                    listDataExVo.StrDefaultShowStyle = TypeUtil.ToString(lookupShowStyle.EditValue);
                    listDataExVo.InfoState = InfoState.Modified;
                    Model.SaveListDataEx(listDataExVo);
                }
                MessageShowUtil.ShowInfo("����ΪĬ����ʾ��ʽ�ɹ�");
            }
            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        private string GetListStyle()
        {
            var itemCount = menuListStyle.ItemLinks.Count;
            for (var i = 0; i < itemCount; i++)
                if (((BarCheckItem)menuListStyle.ItemLinks[i].Item).Checked)
                    return menuListStyle.ItemLinks[i].Item.Caption;
            return "";
        }

        private void frmListEx_SizeChanged(object sender, EventArgs e)
        {
            var intX = Width - lookupSchema.Width - 5;
            var intY = lookupSchema.Location.Y;
            lookupSchema.Location = new Point(intX, intY);
            intX = intX - lookupShowStyle.Width - 5;
            lookupShowStyle.Location = new Point(intX, intY);
            intX = intX - buttonContent.Width - 5;
            buttonContent.Location = new Point(intX, intY);

            if (chkItemFreQryCondition.Checked && (_freQryCondition != null))
            {
                _freQryCondition.SetControlLocation();
                SetFrmQryConditionSize();
            }

            layoutControlItem1.Height = 35;
        }

        private void SetFrmQryConditionSize()
        {
            if (panelFreQry.Controls.Count > 3)
            {
                var height = 75; //2015-12-22 ֮ǰ��85(�������,��Ҫͬʱ����2015-12-22,����һ����������Ҫ�޸�
                var rows = panelFreQry.Controls.Count / 3;
                //var rows = panelFreQry.Controls.Count;
                if (panelFreQry.Controls.Count % 3 != 0)
                    rows++;

                height += 30 * (rows - 1); //2015-12-22 ֮ǰ��37 * (rows - 1)
                layoutControlGroupFreQry.Size = new Size(layoutControlGroupFreQry.Size.Width, height);
            }
            else
            {
                layoutControlGroupFreQry.Size = new Size(layoutControlGroupFreQry.Size.Width, 120); //2015-02-06 ֮ǰ��75
            }
            layoutControlItem4.Height = 40;
            layoutControlItem2.Width = 235;
            if (!ToolBar.Visible)
            {
                layoutItemGrid.Height = this.Height - 40 - layoutControlGroupFreQry.Height - 65;
            }
            else
            {
                layoutItemGrid.Height = this.Height - 70 - layoutControlGroupFreQry.Height - 65;
            }
            //layoutControlItemPointFilter.Width = 150;            
        }

        private void frmListEx_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
            }
            catch (SocketException)
            {
            }
            catch (Exception ex)
            {
            }
        }

        private void tlbAdvancedSearch_ItemClick(object sender, ItemClickEventArgs e)
        {
            gridView.ShowFilterEditor(gridView.FocusedColumn);
        }

        private void tlbSimpleSearch_CheckedChanged(object sender, ItemClickEventArgs e)
        {
            gridView.OptionsView.ShowAutoFilterRow = tlbSimpleSearch.Checked;
        }

        private void gridView_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
        {
            gridView.Appearance.FocusedCell.BackColor = Color.White;
        }

        private void tlbFind_ItemClick()
        {
            try
            {
                var strContext = buttonContent.Text.Trim();
                if (string.IsNullOrEmpty(strContext))
                {
                    gridView.ActiveFilterString = "";
                    return;
                }

                var str = strContext.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                var strFilterCondition = " ";
                var strFilter = "";
                foreach (var s in str)
                {
                    strFilterCondition = " like '%" + s + "%'";
                    foreach (GridColumn column in gridView.VisibleColumns)
                        if ((column.ColumnType.Name.ToLower() != "boolean") &&
                            (column.ColumnType.Name.ToLower() != "int"))
                            strFilter += " or [" + column.FieldName + "]" + strFilterCondition;
                }

                if (strFilter.Length > 2)
                    strFilter = strFilter.Substring(4);
                gridView.ActiveFilterString = strFilter;
            }

            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        /// <summary>
        ///     �����ַ������ַ����г��ֵĴ���
        /// </summary>
        /// <param name="Char">Ҫ�����ֵ��ַ�</param>
        /// <param name="String">Ҫ�����ַ���</param>
        /// <returns>int</returns>
        public int GetCharInStringCount(string Char, string str1)
        {
            var str = str1.Replace(Char, "");
            return (str1.Length - str.Length) / Char.Length;
        }

        private void buttonContent_ButtonClick(object sender, ButtonPressedEventArgs e)
        {
            buttonContent.Text = "";
            gridView.ActiveFilterString = "";
        }

        private void buttonContent_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                tlbFind_ItemClick();

                //�����Զ�ѡ��������
                if (tlbFilterAutoSelect.Checked)
                {
                    var rowCount = gridView.RowCount;
                    if (rowCount != 1)
                    {
                    }
                    for (var i = 0; i < rowCount; i++)
                        gridView.SetRowCellValue(i, "MultiSelect", true);

                    buttonContent.SelectAll();
                }
            }
        }

        private void gridView_RowCellStyle(object sender, RowCellStyleEventArgs e)
        {
            //AppearanceDefault appError = new AppearanceDefault(Color.White, Color.LightCoral, Color.Empty, Color.Red, System.Drawing.Drawing2D.LinearGradientMode.ForwardDiagonal);
            //AppearanceHelper.Apply(e.Appearance, appError);
        }

        private void tlbCancelCondtion_ItemClick(object sender, ItemClickEventArgs e)
        {
            gridView.FormatConditions.Clear();
        }

        private void buttonContent_EditValueChanged(object sender, EventArgs e)
        {
            //�����Զ�ѡ�������У������ˣ��س��ٹ��ˡ� ��������⣺ɨ��ǹ¼��ÿ���ַ�������̫����
            if (tlbFilterAutoSelect.Checked)
                return;

            tlbFind_ItemClick();
        }

        /// <summary>
        ///     �򿪵ȴ��Ի���
        /// </summary>
        private void OpenWaitDialog(string caption)
        {
            CloseWaitDialog();
            wdf = new WaitDialogForm(caption + "...", "��ȴ�...");
            Cursor = Cursors.WaitCursor;
        }

        /// <summary>
        ///     �رյȴ��Ի���
        /// </summary>
        private void CloseWaitDialog()
        {
            if (wdf != null)
                wdf.Close();

            Cursor = Cursors.Default;
        }

        private void ShowRunTimeMessage(DateTime datstart)
        {
            var span = DateTime.Now - datstart;
            var seconds = span.Seconds + span.Milliseconds / 1000.0M;
            MessageBox.Show("�˰�ťִ���� " + decimal.Round(seconds, 1) + " ��ʱ��!", "��ʾ", MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void tlbExportExcel_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (!PermissionManager.HavePermission("ExportReportData"))
            {
                return;
            }

            var strFileType = ".xls";
            Export(1, strFileType);
        }

        private void tlbExeclPDF_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (!PermissionManager.HavePermission("ExportReportData"))
            {
                return;
            }

            var strFileType = ".pdf";
            Export(2, strFileType);
        }

        private void txtExportTXT_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (!PermissionManager.HavePermission("ExportReportData"))
            {
                return;
            }

            var strFileType = ".txt";
            Export(3, strFileType);
        }

        private void txtExportCSV_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (!PermissionManager.HavePermission("ExportReportData"))
            {
                return;
            }

            var strFileType = ".csv";
            Export(4, strFileType);
        }

        private void txtExportHTML_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (!PermissionManager.HavePermission("ExportReportData"))
            {
                return;
            }

            var strFileType = ".html";
            Export(5, strFileType);
        }

        /// <summary>
        ///     ����ͨ�÷���
        /// </summary>
        /// <param name="LngTypeID">�ļ�����ID������Ĭ��ѡ������</param>
        /// <param name="strFileType">�ļ��������ƣ����ڸ��ļ�������׺��</param>
        private void Export(int LngTypeID, string strFileType)
        {
            try
            {
                var saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter =
                    "Excel�ļ�(*.xls)|*.xls|PDF�ļ�(*.pdf)|*.pdf|TXT�ļ�(*.txt)|*.txt|CSV�ļ�(*.csv)|*.csv|HTML�ļ�(*.html)|*.html";
                saveFileDialog.Title = "�����ļ�";
                saveFileDialog.RestoreDirectory = true;
                saveFileDialog.FileName = Text;
                saveFileDialog.FilterIndex = LngTypeID;
                var strShowStyle = TypeUtil.ToString(lookupShowStyle.EditValue);


                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    if (strShowStyle == "pivot")
                    {
                        //������
                        if (LngTypeID == 1)
                            rp.ExportToXls(saveFileDialog.FileName);
                        if (LngTypeID == 2)
                            rp.ExportToPdf(saveFileDialog.FileName);
                        if (LngTypeID == 3)
                            rp.ExportToText(saveFileDialog.FileName);
                        if (LngTypeID == 4)
                            rp.ExportToCsv(saveFileDialog.FileName);
                        if (LngTypeID == 5)
                            rp.ExportToHtml(saveFileDialog.FileName);
                    }
                    else if (strShowStyle == "chart")
                    {
                        //ͼ����
                    }
                    else
                    {
                        gridView.OptionsPrint.PrintHeader = true; //���ú��б���������ӡ����
                        gridView.OptionsPrint.PrintFooter = true; //���ú��б���footer����С�ƣ�
                        //gridView.OptionsPrint.UsePrintStyles = false;//���ú�Ϊ�����б���ɫ��ʽ

                        //�б�ʽ
                        if (LngTypeID == 1)
                            gridView.ExportToXls(saveFileDialog.FileName, false);
                        if (LngTypeID == 2)
                            gridView.ExportToPdf(saveFileDialog.FileName);
                        if (LngTypeID == 3)
                            gridView.ExportToText(saveFileDialog.FileName);
                        if (LngTypeID == 4)
                            gridView.ExportToCsv(saveFileDialog.FileName);
                        if (LngTypeID == 5)
                            gridView.ExportToHtml(saveFileDialog.FileName);
                    }

                    if (DialogResult.Yes ==
                        MessageBox.Show("�Ƿ������򿪴��ļ�?", "��ʾ", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                        Process.Start(saveFileDialog.FileName);
                }
            }
            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        private void barSubItemExport_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                if (!PermissionManager.HavePermission("ExportReportData"))
                    return;
            }
            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        private void tlbSet_ItemClick(object sender, ItemClickEventArgs e)
        {
            var frm = new frmSet();
            frm.ShowDialog();
        }

        private void gridView_CellMerge(object sender, CellMergeEventArgs e)
        {
            // 20170918
            ////����ϲ����⡣gridview�ϲ��¼���ָ��һ��Ϊ�ϲ�������
            //var strFileName = e.Column.FieldName;
            //var rowsmain = _lmdDt.Select("blnMerge=1 and blnMainMerge=1");      //���ϲ�
            //if (rowsmain.Length == 0) return;
            //var rowsmainA = _lmdDt.Select("blnMerge=1 and blnMainMerge=1 and MetaDataFieldName='" + strFileName + "'");

            //if ((rowsmainA == null) || (rowsmainA.Length == 0))     //��ǰ�ϲ��������ϲ�
            //{
            //    //����ϲ��Ĳ������ϲ��ֶΣ�����Ҫ���������ϲ��ֶΣ��������ϲ��ֶν��кϲ�
            //    var strValue = "";
            //    var strValuePrev = "";
            //    var strValueFileName = "";
            //    var strValueFileNamePrev = "";
            //    foreach (var row in rowsmain)
            //    {
            //        var strMainMergeFileName = TypeUtil.ToString(row["MetaDataFieldName"]);
            //        strValue += gridView.GetRowCellValue(e.RowHandle1, strMainMergeFileName) + "&";
            //        strValuePrev += gridView.GetRowCellValue(e.RowHandle2, strMainMergeFileName) + "&";
            //        strValueFileName += gridView.GetRowCellValue(e.RowHandle1, strFileName) + "&";
            //        strValueFileNamePrev += gridView.GetRowCellValue(e.RowHandle2, strFileName) + "&";
            //    }
            //    if ((strValue == strValuePrev) && (strValueFileName == strValueFileNamePrev))
            //    {
            //        //������������������ϲ��ֶ�ֵ��Ȳ��Ҳ������ϲ���ֵҲ��ȣ���źϲ�������ȡ���ϲ�
            //        e.Merge = true;
            //        e.Handled = true;
            //    }
            //    else
            //    {
            //        e.Merge = false;
            //        e.Handled = true;
            //    }
            //}

            ////���ԭ����Ҫ�ϲ��򲻴���
            var strFileName = e.Column.FieldName;       //��ǰ���ֶ�
            string strValueFileName = gridView.GetRowCellValue(e.RowHandle1, strFileName).ToString();
            string strValueFileNamePrev = gridView.GetRowCellValue(e.RowHandle2, strFileName).ToString();
            if (strValueFileName != strValueFileNamePrev)
            {
                return;
            }

            var rowsmain = _lmdDt.Select("blnMerge=1 and blnMainMerge=1");      //���ϲ�
            bool bSplit = false;
            for (int i = 0; i < rowsmain.Length; i++)
            {
                var strMainMergeFileName = TypeUtil.ToString(rowsmain[i]["MetaDataFieldName"]);     //���ϲ��ֶ�
                string strValue = gridView.GetRowCellValue(e.RowHandle1, strMainMergeFileName).ToString();
                string strValuePrev = gridView.GetRowCellValue(e.RowHandle2, strMainMergeFileName).ToString();
                if (strValue != strValuePrev)
                {
                    bSplit = true;
                    break;
                }
            }
            if (bSplit)
            {
                e.Merge = false;
                e.Handled = true;
            }
        }

        /// <summary>
        ///     ��ע¼��(¼����ڶ�Ӧ��������ʾ)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tlbDescInput_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                if (lookupShowStyle.EditValue != null)
                {
                    // 20170821
                    if (dt == null)
                    {
                        MessageShowUtil.ShowMsg("���Ȳ�ѯ���ݡ�");
                        return;
                    }

                    DateTime dtRemark = GetRemarkTime();
                    var listDataRemarkInfo = Model.GetListDataRemarkByTimeListDataId(dtRemark, listDataExVo.ListDataID);

                    var frm = new frmInputDesc();
                    //frm.StrDesc = listDataExVo.StrListSrcSQL;

                    if (listDataRemarkInfo == null)
                    {
                        frm.StrDesc = "";
                    }
                    else
                    {
                        frm.StrDesc = listDataRemarkInfo.Remark;
                    }

                    frm.ShowDialog();
                    if (frm.BlnOK)
                    {
                        //listDataExVo.StrListSrcSQL = frm.StrDesc;
                        //listDataExVo.InfoState = InfoState.Modified;
                        //Model.SaveListDataEx(listDataExVo);
                        if (listDataRemarkInfo == null)
                        {
                            Model.AddListdataremark(listDataExVo.ListDataID, dtRemark, frm.StrDesc);
                        }
                        else
                        {
                            Model.UpdateListdataremarkByTimeListDataId(listDataExVo.ListDataID, dtRemark, frm.StrDesc);
                        }

                        listTemplevo = Model.GetListTemple(listDataExVo.ListDataID);
                        lookupShowStyle_EditValueChanged(null, null);
                        //MessageShowUtil.ShowInfo("��ע����ɹ�");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        /// <summary>
        ///     ���԰�װλ�ù�ѡ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chkblnKeyWord_CheckedChanged(object sender, ItemClickEventArgs e)
        {
            if (chkblnKeyWord.Checked)
            {
                var RowsblnKeyWord = _lmdDt.Select("blnKeyWord=1");
                if ((RowsblnKeyWord == null) || (RowsblnKeyWord.Length == 0))
                    MessageShowUtil.ShowMsg("������δ���ùؼ��֣������ý���Ч");
            }
        }

        private void tlbColumnSortSet_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                var rows = metadataFieldDt.Select("MetaDataID=" + listExvo.MainMetaDataID + " and blnDesignSort=1");
                if ((rows == null) || (rows.Length == 0))
                {
                    MessageShowUtil.ShowMsg("Ԫ������û�����ÿɱ��ŵ���Ŀ�����ܽ��б���");
                    return;
                }
                var strFKCode = TypeUtil.ToString(rows[0]["strFkCode"]);
                var frm = new frmDataSortSet();
                frm.MetadataID = listExvo.MainMetaDataID;
                frm.dtLmd = _lmdDt;
                var strFileName =
                    _lmdDt.Select("MetaDataFieldID=" + TypeUtil.ToInt(rows[0]["ID"]))[0]["MetaDataFieldName"].ToString();
                frm.strFileName = strFileName;
                var strListDisplayName =
                    _lmdDt.Select("MetaDataFieldID=" + TypeUtil.ToInt(rows[0]["ID"]))[0]["strListDisplayFieldNameCHS"]
                        .ToString();
                frm.strListDisplayName = strListDisplayName;
                frm.strFKCode = strFKCode;
                frm.ListDataID = ListDataID;
                frm.ShowDialog();
                if (frm.blnOK)
                {
                }
            }
            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        private void GetDistinctSql(ref string strListSql)
        {
            if (listExvo.StrListCode == "MLLKDDay")
                strListSql = strListSql.Insert(6, " distinct ");
        }

        #region ��������

        private XtraReportTemple rp;
        private int ReportWidth;
        public string strReportFileName = ""; //����ģ���ļ����ƣ������Ҫ���ڴӷ����������༭����ģ���ã����÷������ô˱���Ϊ"",Ϊ�յ�ʱ����Ҫ��ֵ


        public void SetPivotFormat()
        {
            rp = new XtraReportTemple();
            if (strReportFileName == "") //���ڿ�˵���Ǵ��б�����������������ڿ�˵���Ǵӱ��������������
                strReportFileName = listDataExVo.StrListDataName + ListDataID;
            var strReportTemple = AppDomain.CurrentDomain.BaseDirectory + "Config\\ReportTemple\\" + strReportFileName +
                                  ".repx";
            var strReportDir = AppDomain.CurrentDomain.BaseDirectory + "Config\\ReportTemple";
            if (!Directory.Exists(strReportDir))
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "Config\\ReportTemple");
            if (File.Exists(strReportTemple) && (0 == 1)) //2014-12-08��������ģ��ȫ�������ݿ����ȡ��������ʱ�����IF���һֱ��ִ��
            {
                //������ڣ�ֱ�Ӽ���ģ�壬û�д򿪺����
                rp.LoadLayout(strReportTemple);
                rp.Bands["Detail"].FormattingRules.Clear();
                //2014-12-01����ģ�������������ʽ��ʽ,��Ϊ��������أ�������������þ���Ч����ȥ��ȡ����ģ���ļ�
                foreach (var dto in listDisplayExList)
                {
                    //����������ʽ
                    if (!string.IsNullOrEmpty(dto.StrConditionFormat) && (dto.LngApplyType != 2))
                        SetReportColumnConditionFormat(dto.StrListDisplayFieldName, dto.StrConditionFormat);

                    //ģ����ѯ����
                    if (!string.IsNullOrEmpty(dto.StrBluerCondition))
                        SetGridColumnBluerFormat(dto.StrListDisplayFieldName, dto.StrBluerCondition);

                    //���ñ���˳��
                    SetGridColumnSort(dto.StrListDisplayFieldName);
                }
            }
            //������ݿ����д���ģ���ļ�����ô��ȥ���ݿ��������������ڱ���
            if ((listTemplevo != null) && (listTemplevo.ListTempleID > 0))
            {
                var fileData = listTemplevo.BloImage;
                var buffer = fileData.GetUpperBound(0) + 1;
                var fs = new FileStream(strReportTemple, FileMode.Create, FileAccess.Write);
                fs.Write(fileData, 0, buffer);
                fs.Close();

                rp.LoadLayout(strReportTemple);
                rp.Bands["Detail"].FormattingRules.Clear();
                DicBluerCondition.Clear();
                foreach (var dto in listDisplayExList)
                {
                    //����������ʽ
                    if (!string.IsNullOrEmpty(dto.StrConditionFormat) && (dto.LngApplyType != 2))
                        SetReportColumnConditionFormat(dto.StrListDisplayFieldName, dto.StrConditionFormat);

                    //ģ����ѯ����
                    if (!string.IsNullOrEmpty(dto.StrBluerCondition))
                        SetGridColumnBluerFormat(dto.StrListDisplayFieldName, dto.StrBluerCondition);
                    //���ñ���˳��
                    SetGridColumnSort(dto.StrListDisplayFieldName);


                    //���úϲ�

                    var xrtable = rp.FindControl("TableReportDetail", true) as XRTable;
                    foreach (XRTableRow row in xrtable.Rows)
                        foreach (XRTableCell cell in row.Cells)
                        {
                            if (cell.DataBindings["Text"] == null) continue;
                            if (cell.DataBindings["Text"].DataMember == dto.StrListDisplayFieldName)
                                if (dto.BlnMerge)
                                    cell.ProcessDuplicates = ValueSuppressType.MergeByValue;
                                else
                                    cell.ProcessDuplicates = ValueSuppressType.Leave;
                        }
                }
            }
            else
            {
                //����ListDisplay��õ�����Ŀ��
                ReportWidth = 0;
                foreach (var dto in listDisplayExList)
                {
                    if (dto.LngRowIndex < 0) continue;
                    ReportWidth += dto.LngDisplayWidth;
                }

                CreateReportHead();
                CreatePageHeader();
                CreateReportDetail();
                rp.PaperKind = PaperKind.Custom;
                rp.PageWidth = ReportWidth + rp.Margins.Left + rp.Margins.Right;
                rp.SaveLayout(strReportTemple);
            }
        }

        private void CreateReportHead()
        {
            var lable = (XRLabel)rp.FindControl("lblTitle", true); //���ַ�ʽ�����ڽ����ϰѿؼ��Ϸ���
            if (listExvo == null)
                listExvo = Model.GetListEx(listId);
            lable.Text = listExvo.StrListName;
            lable.LocationFloat = new PointFloat((ReportWidth - lable.WidthF) / 2F, lable.LocationF.Y);

            //���ַ�ʽ�Ƕ�̬�����ؼ���Ŀǰ��������ô����
            //XRLabel lable = new XRLabel();
            //lable.Font = new System.Drawing.Font("Times New Roman", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            //lable.Name = "lblTitle";
            //lable.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            //lable.SizeF = new System.Drawing.SizeF(ReportWidth, 35.20833F);
            //lable.StylePriority.UseFont = false;
            //lable.StylePriority.UseTextAlignment = false;
            //lable.Text = this.Text;
            //lable.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
            //rp.Bands["ReportHeader"].Controls.Add(lable);
        }


        private void CreatePageHeader()
        {
            //������ѯ�������
            //rp.Bands["PageHeader"].Controls.Clear();
            //��д�ͷſؼ��ķ�����ֱ��clear�ᵼ�¾����Դһֱ����  20180422
            while (rp.Bands["PageHeader"].Controls.Count > 0)
            {
                if (rp.Bands["PageHeader"].Controls[0] != null)
                    rp.Bands["PageHeader"].Controls[0].Dispose();
            }
            var lblConditon = new XRLabel();
            lblConditon.Text = "";
            lblConditon.Name = "lblConditon";
            lblConditon.Size = new Size(ReportWidth, 40);
            lblConditon.TextAlignment = TextAlignment.MiddleLeft;
            lblConditon.Font = new Font("����", 12.5F, FontStyle.Regular, GraphicsUnit.Point, 134);
            rp.Bands["PageHeader"].Controls.Add(lblConditon);


            //��������
            var xrtable = new XRTable();
            xrtable.Name = "TablePageHeader";
            xrtable.SizeF = new SizeF(ReportWidth, 40F);
            xrtable.Borders = BorderSide.All;
            xrtable.LocationF = new PointF(lblConditon.LocationF.X, lblConditon.LocationF.Y + lblConditon.SizeF.Height);

            var row = new XRTableRow();
            xrtable.Rows.Add(row);

            XRTableCell xrcell = null;

            xrcell = new XRTableCell();
            xrcell.Weight = 50 / 250D;
            xrcell.Font = new Font("����", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 134);
            xrcell.TextAlignment = TextAlignment.MiddleCenter;
            xrcell.Text = "���";
            row.Cells.Add(xrcell);


            foreach (var dto in listDisplayExList)
            {
                var lngRowIndex = dto.LngRowIndex;
                if (lngRowIndex < 0) continue;
                var strListDisplayFieldNameCHS = dto.StrListDisplayFieldNameCHS;
                ;
                var lngwidth = dto.LngDisplayWidth;
                xrcell = new XRTableCell();
                xrcell.Weight = lngwidth / 250D;
                xrcell.Font = new Font("����", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 134);
                xrcell.TextAlignment = TextAlignment.MiddleCenter;
                xrcell.Text = strListDisplayFieldNameCHS;
                xrcell.Tag = dto.StrListDisplayFieldName;
                row.Cells.Add(xrcell);
            }

            //rp.Bands["PageHeader"].Controls.Clear();
            rp.Bands["PageHeader"].Controls.Add(xrtable);
        }

        private void CreateReportDetail()
        {
            ///��̬����һ�����
            var xrtable = new XRTable();
            xrtable.Name = "TableReportDetail";
            xrtable.SizeF = new SizeF(ReportWidth, 25F);
            xrtable.Borders = BorderSide.All;
            xrtable.Borders = BorderSide.Left | BorderSide.Bottom | BorderSide.Right;


            //��̬����һ��
            var row = new XRTableRow();
            xrtable.Rows.Add(row);
            XRTableCell xrcell = null;

            //���һ�������
            xrcell = new XRTableCell();
            xrcell.Weight = 50 / 250D;
            xrcell.Font = new Font("����", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 134);
            xrcell.TextAlignment = TextAlignment.MiddleCenter;
            xrcell.DataBindings.Add("Text", dt, "colNumber");
            row.Cells.Add(xrcell);

            ///�������ݿ�Display�����Զ�������
            foreach (var dto in listDisplayExList)
            {
                var lngRowIndex = dto.LngRowIndex;
                if (lngRowIndex < 0) continue;
                var strFileName = dto.StrListDisplayFieldName;
                var lngwidth = dto.LngDisplayWidth;
                xrcell = new XRTableCell();
                xrcell.Weight = lngwidth / 250D; //Dev�˰汾�Ǹ���Ȩ����������ʾ��С��������Width����   
                xrcell.Font = new Font("����", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 134);
                xrcell.TextAlignment = TextAlignment.MiddleCenter;

                if (dto.StrSummaryDisplayFormat == "")
                    xrcell.DataBindings.Add("Text", dt, strFileName);
                if (dto.StrSummaryDisplayFormat == "����")
                {
                    xrcell.DataBindings.Add("Text", dt, strFileName, "{0:c2}");
                    xrcell.TextAlignment = TextAlignment.MiddleRight;
                }
                if (dto.StrSummaryDisplayFormat == "����")
                {
                    xrcell.DataBindings.Add("Text", dt, strFileName, "{0:n2}");
                    xrcell.TextAlignment = TextAlignment.MiddleRight;
                }
                if (dto.StrSummaryDisplayFormat == "����")
                {
                    xrcell.DataBindings.Add("Text", dt, strFileName, "{0:yyyy-MM-dd}");
                    xrcell.TextAlignment = TextAlignment.MiddleRight;
                }
                if (dto.StrSummaryDisplayFormat == "ʱ��")
                {
                    xrcell.DataBindings.Add("Text", dt, strFileName, "{0:HH:mm:ss}");
                    xrcell.TextAlignment = TextAlignment.MiddleRight;
                }
                if (dto.StrSummaryDisplayFormat == "����ʱ��")
                {
                    xrcell.DataBindings.Add("Text", dt, strFileName, "{0:yyyy-MM-dd HH:mm:ss}");
                    xrcell.TextAlignment = TextAlignment.MiddleRight;
                }
                xrcell.Tag = dto.StrListDisplayFieldName;
                xrcell.ProcessDuplicates = dto.BlnMerge ? ValueSuppressType.MergeByValue : ValueSuppressType.Leave;
                row.Cells.Add(xrcell);
                //������ʽ����
                if (!string.IsNullOrEmpty(dto.StrConditionFormat) && (dto.LngApplyType != 2))
                    SetReportColumnConditionFormat(strFileName, dto.StrConditionFormat);
                //ģ����ѯ����
                if (!string.IsNullOrEmpty(dto.StrBluerCondition))
                    SetGridColumnBluerFormat(dto.StrListDisplayFieldName, dto.StrBluerCondition);

                //���ñ���˳��
                SetGridColumnSort(dto.StrListDisplayFieldName);
            }

            //rp.Bands["Detail"].Controls.Clear();
            //��д�ͷſؼ��ķ�����ֱ��clear�ᵼ�¾����Դһֱ����  20180422
            while (rp.Bands["Detail"].Controls.Count > 0)
            {
                if (rp.Bands["Detail"].Controls[0] != null)
                    rp.Bands["Detail"].Controls[0].Dispose();
            }
            rp.Bands["Detail"].Controls.Add(xrtable);
        }


        private void ReportDataBinding()
        {
            // 20170918
            //#region 2015-02-04   �������ϲ�����
            //var rowsmain = _lmdDt.Select("blnMerge=1 and blnMainMerge=1");
            //var rowsdetail = _lmdDt.Select("blnMerge=1 and blnMainMerge=0");
            //if (rowsdetail.Length > 0)
            //{
            //    var strMerge = " ";
            //    var strMainMergeValue = "";
            //    var strMainMergeValuePrev = "";
            //    for (var i = 0; i < dt.Rows.Count; i++)
            //    {
            //        if (i == 0) continue;
            //        strMainMergeValue = "";
            //        strMainMergeValuePrev = "";
            //        foreach (var row in rowsmain)
            //        {
            //            var strMainMergeFileName = TypeUtil.ToString(row["MetaDataFieldName"]);
            //            strMainMergeValue += TypeUtil.ToString(dt.Rows[i][strMainMergeFileName]) + "&";
            //            strMainMergeValuePrev += TypeUtil.ToString(dt.Rows[i - 1][strMainMergeFileName]) + "&";
            //        }
            //        foreach (var row in rowsdetail)
            //        {
            //            var strMergeFileName = row["MetaDataFieldName"].ToString();
            //            var Value = TypeUtil.ToString(dt.Rows[i][strMergeFileName]);
            //            var ValuePrev = TypeUtil.ToString(dt.Rows[i - 1][strMergeFileName]);
            //            if (Value == ValuePrev)
            //                if (strMainMergeValue != strMainMergeValuePrev)
            //                {
            //                    dt.Rows[i][strMergeFileName] = dt.Rows[i][strMergeFileName] + strMerge;
            //                    if (strMerge == " ")
            //                        strMerge = "  ";
            //                    else
            //                        strMerge = " ";
            //                }
            //        }
            //    }
            //}
            //#endregion

            var rpDt = dt.Clone();

            for (int i = 0; i < rpDt.Columns.Count; i++)
            {
                rpDt.Columns[i].DataType = typeof(string);
            }

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                var row = rpDt.NewRow();
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    string col = dt.Columns[j].ToString();
                    row[col] = dt.Rows[i][col].ToString();
                }
                rpDt.Rows.Add(row);
            }

            var rowsMainMerge = _lmdDt.Select("blnMerge=1 and blnMainMerge=1");     //���ϲ�

            if (rowsMainMerge.Length > 0)        //�����ϲ��вŴ���
            {
                //string sSplitSign = " ";        //�ָ��־
                bool bIfAddBlankLast = false;       //��һ���Ƿ�����˿ո�
                for (int i = 0; i < rpDt.Rows.Count - 1; i++)
                {
                    //�жϸ�������һ�е����ϲ����Ƿ��зָ���
                    bool bSsplit = false;       //�Ƿ���Ҫ�ָ�
                    for (int j = 0; j < rowsMainMerge.Length; j++)
                    {
                        string sZhbzd = rowsMainMerge[j]["MetaDataFieldName"].ToString();
                        string sFirstRow = rpDt.Rows[i][sZhbzd].ToString();
                        string sSecondRow = rpDt.Rows[i + 1][sZhbzd].ToString();
                        if (sFirstRow != sSecondRow)
                        {
                            bSsplit = true;
                            break;
                        }
                    }

                    //������Ҫ�ָ���
                    if (bSsplit)
                    {
                        //��ǰ��֮����мӿո�
                        for (int j = i; j < rpDt.Rows.Count - 1; j++)
                        {
                            for (int k = 0; k < rpDt.Columns.Count; k++)
                            {
                                string sCol = rpDt.Columns[k].ToString();
                                if (bIfAddBlankLast)
                                {
                                    string val = rpDt.Rows[j + 1][sCol].ToString();
                                    rpDt.Rows[j + 1][sCol] = val.Substring(0, val.Length - 1);
                                }
                                else
                                {
                                    rpDt.Rows[j + 1][sCol] += " ";
                                }
                            }
                        }
                        bIfAddBlankLast = !bIfAddBlankLast;
                    }
                }
            }

            rp.DataSource = rpDt;
        }

        private void SetReportColumnConditionFormat(string strFileName, string strConditionFormat)
        {
            var sfcType = (SFCDataTypeEnum)TypeUtil.ToInt(strConditionFormat.Substring(strConditionFormat.Length - 1));
            var strTemp = strConditionFormat.Remove(strConditionFormat.Length - 1);
            var strConditions = strTemp.Split(new[] { "&&$$" }, StringSplitOptions.RemoveEmptyEntries);
            if (strConditions.Length > 0)
            {
                var str = string.Empty;
                var strOper = string.Empty;
                var strValue1 = string.Empty;
                var strValue2 = string.Empty;
                var blnApplyRow = false;
                FormattingRule rule = null;
                var color = Color.White;
                var fontColor = Color.Black;

                for (var i = 0; i < strConditions.Length; i++)
                {
                    str = strConditions[i];
                    if (str.Contains("&&$"))
                    {
                        var strs = str.Split(new[] { "&&$" }, StringSplitOptions.RemoveEmptyEntries);
                        strOper = strs[0];
                        blnApplyRow = TypeUtil.ToBool(strs[1]);
                        strValue1 = "";
                        strValue2 = "";
                        if (strs.Length > 3)
                            strValue1 = strs[2];
                        if (strs.Length > 4)
                            strValue2 = strs[3];

                        var ss = strs.Length > 5 ? strs[4].Split(';') : strs[3].Split(';');
                        Model.GetColorByString(ss, ref color);

                        var fontss = strs.Length > 5 ? strs[5].Split(';') : strs[4].Split(';');
                        Model.GetColorByString(fontss, ref fontColor);
                    }

                    var strCaclOper = GetCaclOper(strOper, strFileName);
                    rule = new FormattingRule();
                    rp.FormattingRuleSheet.Add(rule);
                    rule.DataSource = rp.DataSource;
                    rule.Formatting.BackColor = color;
                    rule.Formatting.ForeColor = fontColor;
                    rule.Condition = string.Format(strFileName + strCaclOper, strValue1, strValue2);
                    rp.Bands["Detail"].FormattingRules.Add(rule);
                }
            }
        }


        /// <summary>
        ///     �������õ�������ʽ������֯�������ʽ��
        /// </summary>
        /// <param name="strOper">��������</param>
        /// <param name="strFileName">��������Ҫ�����ڽ�������֮��������</param>
        /// <returns></returns>
        private string GetCaclOper(string strOper, string strFileName)
        {
            var strCaclOper = "";
            switch (strOper)
            {
                case "����":
                    strCaclOper = " = '{0}'";
                    break;
                case "������":
                    strCaclOper = " <> '{0}'";
                    break;
                case "����":
                    strCaclOper = " > {0}";
                    break;
                case "���ڵ���":
                    strCaclOper = " >= {0}";
                    break;
                case "С��":
                    strCaclOper = " < {0}";
                    break;
                case "С�ڵ���":
                    strCaclOper = " <= {0}";
                    break;
                case "����":
                    strCaclOper = " >= {0}  and  " + strFileName + " <= {1}";
                    break;
                case "������":
                    strCaclOper = " <= {0}  and  " + strFileName + " >= {1}";
                    break;
                default:
                    strCaclOper = "";
                    break;
            }
            return strCaclOper;
        }


        public string GetFieldCaption(string str)
        {
            var a = str.LastIndexOf('(');
            if (a > 0)
                str = str.Remove(a);
            return str;
        }

        #endregion

        /// <summary>
        /// ��ȡ��עʱ��
        /// </summary>
        /// <returns></returns>
        private DateTime GetRemarkTime()
        {
            DateTime dtRemark;
            if (_strFreQryConditionByChs == "" || listId != 9)
            {
                dtRemark = new DateTime(1900, 01, 01);
            }
            else
            {
                string sQueryTime =
                    _strFreQryConditionByChs.Substring(5, _strFreQryConditionByChs.Length - 5).Split('��')[0];
                dtRemark = Convert.ToDateTime(sQueryTime);
            }
            return dtRemark;
        }

        private void radioButtonArrangePoint_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonArrangePoint.Checked)
            {
                lookUpEditArrangeTime.Enabled = true;
                lookUpEditArrangeActivity.Enabled = true;
            }
            else
            {
                lookUpEditArrangeTime.Enabled = false;
                lookUpEditArrangeActivity.Enabled = false;
            }
        }

        private void lookUpEditArrangeTime_EditValueChanged(object sender, EventArgs e)
        {
            var listDataLay = listlayount.FirstOrDefault(a => a.ListDataLayoutID.ToString() == lookUpEditArrangeTime.EditValue.ToString());
            if (listDataLay != null)
            {
                string point = listDataLay.StrConTextCondition.Split(new string[] { ".point in (" }, StringSplitOptions.None)[1];
                string point2 = point.Substring(0, point.Length - 3);
                labelArrangePoint.Text = point2;
            }
        }

        private void radioButtonStorePoint_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonStorePoint.Checked)
            {
                lookUpEditRemoveDuplication.Enabled = true;
            }
            else
            {
                lookUpEditRemoveDuplication.Enabled = false;
            }
        }

        private void BeforeDay_ItemClick(object sender, ItemClickEventArgs e)
        {
            NearQuery(-1);
            tlbRefresh_ItemClick(this, null);
        }

        private void AfterDay_ItemClick(object sender, ItemClickEventArgs e)
        {
            NearQuery(1);
            tlbRefresh_ItemClick(this, null);
        }

        /// <summary>
        /// �ٽ�ʱ���ѯ
        /// </summary>
        /// <param name="day"></param>
        private void NearQuery(int day)
        {
            try
            {
                var controls = panelFreQry.Controls;
                foreach (Control item in controls)
                {
                    //var hash = item.Tag as Hashtable;
                    //if (hash == null)
                    //{
                    //    continue;
                    //}

                    //if (!hash.ContainsKey("_fieldName"))
                    //{
                    //    continue;
                    //}

                    //var fieldName = hash["_fieldName"].ToString();
                    //var fieldNameSplit = fieldName.Split('_');
                    //var split2 = fieldNameSplit[fieldNameSplit.Length - 1].ToLower();
                    //if (split2 != "datsearch")
                    //{
                    //    continue;
                    //}

                    // 20180408
                    var type = item.GetType();
                    var typeNameCustom = type.Name;
                    if (typeNameCustom.Contains("DateTime") && !typeNameCustom.Contains("Year") && !typeNameCustom.Contains("Month"))
                    {
                        var subcontrols = item.Controls[0].Controls;
                        List<DateEdit> allDe = new List<DateEdit>();
                        foreach (Control item2 in subcontrols)
                        {
                            var typeName = item2.GetType();

                            if (typeName.Name == "DateEdit")
                            {
                                var de = item2 as DateEdit;
                                if (de != null && de.DateTime != new DateTime(1, 1, 1))
                                {
                                    //de.DateTime = de.DateTime.AddDays(day);
                                    allDe.Add(de);
                                }
                            }
                        }
                        var allDeOrder = allDe.OrderBy(a => a.DateTime).ToList();
                        if (day < 0)
                        {
                            for (int i = 0; i < allDeOrder.Count; i++)
                            {
                                allDeOrder[i].DateTime = allDeOrder[i].DateTime.AddDays(day);
                            }
                        }
                        else
                        {
                            for (int i = allDeOrder.Count - 1; i >= 0; i--)
                            {
                                allDeOrder[i].DateTime = allDeOrder[i].DateTime.AddDays(day);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageShowUtil.ShowErrow(e);
            }
        }

        private void barButtonItemExportExcelAll_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel�ļ�(*.xls)|*.xls",
                    Title = "�����ļ�",
                    RestoreDirectory = true,
                    FileName = Text
                };

                var res = saveFileDialog.ShowDialog();
                if (res != DialogResult.OK)
                {
                    return;
                }

                OpenWaitDialog("���������ļ�");

                SetDayTableSql();
                var strListSql = listDataExVo.StrListSQL.Replace("where 1=1",
                    "where 1=1 " + _strFreQryCondition + _strReceiveParaCondition + _strSortCondtion);
                var dtExport = Model.GetDataTable(strListSql);      //��������
                var displayExListNeed = listDisplayExList.Where(a => a.LngRowIndex != -1).OrderBy(a => a.LngRowIndex).ToList();      //��������

                if (/*_listdate == null ||*/ dtExport == null || dtExport.Rows.Count == 0 || displayExListNeed.Count == 0)
                {
                    MessageShowUtil.ShowMsg("�����ݵ�����");
                    return;
                }

                HSSFWorkbook hssfworkbook = new HSSFWorkbook();
                Sheet sheet1 = hssfworkbook.CreateSheet("sheet1");      //����Sheet1

                //����
                Row rowTitle = sheet1.CreateRow(0);
                rowTitle.Height = 500;
                var cellTitle = rowTitle.CreateCell(0);
                cellTitle.SetCellValue(Text);

                var fontTitle = hssfworkbook.CreateFont();
                fontTitle.FontName = "����";      //����
                fontTitle.FontHeightInPoints = 20;      //�ֺ�
                //font1.Color = HSSFColor.RED.index;        //��ɫ
                fontTitle.Boldweight = (short)FontBoldWeight.BOLD;      //����
                //font1.IsItalic = true;        //б��
                //font1.Underline = (byte)FontUnderlineType.DOUBLE;     //���˫�»���
                CellStyle styleTitle = hssfworkbook.CreateCellStyle();
                styleTitle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.CENTER;        //����
                //styleTitle.VerticalAlignment = VerticalAlignment.CENTER;        //��ֱ���� 
                //styleTitle.WrapText = true;     //�Զ�����
                styleTitle.SetFont(fontTitle);
                cellTitle.CellStyle = styleTitle;

                var columnCount = displayExListNeed.Count;
                sheet1.AddMergedRegion(new CellRangeAddress(0, 0, 0, columnCount - 1));     //CellRangeAddress(��ʼ��,��ֹ��,��ʼ��,��ֹ��);

                //��ͷ
                var fontColumnHead = hssfworkbook.CreateFont();
                fontColumnHead.FontName = "����";      //����
                fontColumnHead.FontHeightInPoints = 10;      //�ֺ�
                //font1.Color = HSSFColor.RED.index;        //��ɫ
                fontColumnHead.Boldweight = (short)FontBoldWeight.BOLD;     //����
                //font1.IsItalic = true;        //б��
                //font1.Underline = (byte)FontUnderlineType.DOUBLE;     //���˫�»���
                CellStyle styleColumnHead = hssfworkbook.CreateCellStyle();
                styleColumnHead.Alignment = NPOI.SS.UserModel.HorizontalAlignment.CENTER;        //����
                //styleTitle.VerticalAlignment = VerticalAlignment.CENTER;        //��ֱ���� 
                //styleTitle.WrapText = true;     //�Զ�����
                styleColumnHead.SetFont(fontColumnHead);

                Row rowColumnHead = sheet1.CreateRow(1);
                for (int i = 0; i < displayExListNeed.Count; i++)
                {
                    sheet1.SetColumnWidth(i, displayExListNeed[i].LngDisplayWidth * 40);

                    var cellColumnHead = rowColumnHead.CreateCell(i);
                    var columnHeadText = displayExListNeed[i].StrListDisplayFieldNameCHS;
                    cellColumnHead.SetCellValue(columnHeadText);
                    cellColumnHead.CellStyle = styleColumnHead;
                }

                //�б�����
                var fontDetail = hssfworkbook.CreateFont();
                fontDetail.FontName = "����";      //����
                fontDetail.FontHeightInPoints = 10;      //�ֺ�
                //font1.Color = HSSFColor.RED.index;        //��ɫ
                //fontColumnHead.Boldweight = 700;     //����
                //font1.IsItalic = true;        //б��
                //font1.Underline = (byte)FontUnderlineType.DOUBLE;     //���˫�»���
                CellStyle styleDetail = hssfworkbook.CreateCellStyle();
                styleDetail.SetFont(fontDetail);
                styleDetail.WrapText = true;     //�Զ�����
                styleDetail.VerticalAlignment = VerticalAlignment.CENTER;        //��ֱ���� 

                for (int i = 0; i < dtExport.Rows.Count; i++)
                {
                    Row rowDetail = sheet1.CreateRow(i + 2);
                    for (int j = 0; j < displayExListNeed.Count; j++)
                    {
                        var cellDetail = rowDetail.CreateCell(j);
                        var detailFieldName = displayExListNeed[j].StrListDisplayFieldName;
                        var detailText = dtExport.Rows[i][detailFieldName].ToString();
                        cellDetail.SetCellValue(detailText);
                        cellDetail.CellStyle = styleDetail;
                    }
                }

                FileStream file = new FileStream(saveFileDialog.FileName, FileMode.Create);
                hssfworkbook.Write(file);
                file.Close();
            }
            catch (Exception exception)
            {
                MessageShowUtil.ShowErrow(exception);
            }
            finally
            {
                CloseWaitDialog();
            }
        }

        /// <summary>
        /// ������״̬�䶯�������������¼�������ͳ���ʱ�䣩
        /// </summary>
        private void SwitchingValueStateChangeDataAmend()
        {
            try
            {
                var listCode = listExvo.StrListCode;
                if (listCode != "KGLStateRBReport")
                {
                    return;
                }

                var ifMergeRecords = CbfSettingRequest.IfMergeRecords();
                if (!ifMergeRecords)
                {
                    return;
                }

                var dtHandled = dt.Copy();
                dtHandled.DefaultView.Sort = "ViewJC_KGStateMonth1_point asc,ViewJC_KGStateMonth1_stime asc";
                dtHandled = dtHandled.DefaultView.ToTable();

                //ɾδ�仯״̬
                string currentPoint = null;
                string currentState = null;
                for (int i = dtHandled.Rows.Count - 1; i >= 0; i--)
                {
                    var point = dtHandled.Rows[i]["ViewJC_KGStateMonth1_point"].ToString();
                    var state = dtHandled.Rows[i]["ViewJC_KGStateMonth1_state"].ToString();

                    if (point == currentPoint && state == currentState)
                    {
                        var rightEndTime = dtHandled.Rows[i + 1]["ViewJC_KGStateMonth1_etime"];
                        dtHandled.Rows[i]["ViewJC_KGStateMonth1_etime"] = rightEndTime;
                        var startTime = dtHandled.Rows[i]["ViewJC_KGStateMonth1_stime"];
                        var duration = Convert.ToDateTime(rightEndTime) - Convert.ToDateTime(startTime);
                        dtHandled.Rows[i]["ViewJC_KGStateMonth1_duration"] = duration.ToString();
                        dtHandled.Rows.RemoveAt(i + 1);
                    }

                    currentPoint = point;
                    currentState = state;
                }

                //�����ܴ�������ʱ��
                var dtDis = dtHandled.DefaultView.ToTable(true, "ViewJC_KGStateMonth1_point", "ViewJC_KGStateMonth1_state");

                foreach (DataRow item in dtDis.Rows)
                {
                    var drs = dtHandled.Select("ViewJC_KGStateMonth1_point='" + item["ViewJC_KGStateMonth1_point"] + "' and ViewJC_KGStateMonth1_state='" + item["ViewJC_KGStateMonth1_state"] + "'");
                    var count = drs.Length;

                    TimeSpan sumTime = new TimeSpan(0);
                    foreach (var item2 in drs)
                    {
                        sumTime += TimeSpan.Parse(item2["ViewJC_KGStateMonth1_duration"].ToString());
                    }

                    foreach (DataRow item3 in dtHandled.Rows)
                    {
                        if (item3["ViewJC_KGStateMonth1_point"].ToString() == item["ViewJC_KGStateMonth1_point"].ToString() && item3["ViewJC_KGStateMonth1_state"].ToString() == item["ViewJC_KGStateMonth1_state"].ToString())
                        {
                            item3["ViewJC_KGStateMonth1_sumtime"] = sumTime.ToString();
                            item3["ViewJC_KGStateMonth1_sumcount"] = count.ToString();
                        }
                    }
                }

                //��������
                var orderInfo = _lmdDt.Select("lngOrder<>0");
                var orderInfoSort = orderInfo.OrderBy(a => Convert.ToInt32(a["lngOrder"])).ToList();
                var orderText = "";
                foreach (var item in orderInfoSort)
                {
                    var lngOrderMethod = item["lngOrderMethod"].ToString();
                    var order = lngOrderMethod == "1" ? "asc" : "desc";
                    orderText += item["MetaDataFieldName"] + " " + order + ",";
                }
                if (!string.IsNullOrEmpty(orderText))
                {
                    orderText = orderText.Substring(0, orderText.Length - 1);
                }
                dtHandled.DefaultView.Sort = orderText;

                dt = dtHandled.DefaultView.ToTable();
            }
            catch (Exception e)
            {
                dt = null;
                MessageShowUtil.ShowMsg("�������������������Ա��ϵ��");
                LogHelper.Error(e.ToString());
            }
        }

        private void tlbRefresh1_Click(object sender, EventArgs e)
        {
            try
            {
                RefreshListData();
                blnRefreshed = true;
            }
            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            NearQuery(-1);
            tlbRefresh_ItemClick(this, null);
        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            NearQuery(1);
            tlbRefresh_ItemClick(this, null);
        }

        private void simpleButton3_Click(object sender, EventArgs e)
        {
            if (!PermissionManager.HavePermission("ExportReportData"))
            {
                return;
            }

            var strFileType = ".xls";
            Export(1, strFileType);
        }

        private void simpleButton4_Click(object sender, EventArgs e)
        {

            if (simpleButton4.Text == "Ԥ��")
            {
                simpleButton4.Text = "ȡ��";
                lookupShowStyle.EditValue = "pivot";
            }
            else
            {
                simpleButton4.Text = "Ԥ��";
                lookupShowStyle.EditValue = "list";
            }

            //���������  20180405
            layoutControlItem1.Width = 300;
            layoutControlItem2.Width = 300;

        }

        private void tlbRefresh1_Click_1(object sender, EventArgs e)
        {

        }

        private void simpleButton9_Click(object sender, EventArgs e)
        {
            try
            {
                SetExecuteBeginTime();
                if (lookupShowStyle.EditValue == "pivot")
                {
                    SetReportPage(0);
                    return;
                }


                if (PerPageRecord == 0 || TotalPage == 0)
                    return;

                CurrentPageNumber = 1;

                var strListSql = listDataExVo.StrListSQL.Replace("where 1=1",
                    "where 1=1 " + _strFreQryCondition + _strReceiveParaCondition + _strSortCondtion);
                GetDistinctSql(ref strListSql);
                dt = Model.GetPageData(strListSql, 1, PerPageRecord);


                BandingData();

                SetListPageInfo();

                MessageShowUtil.ShowStaticInfo("���ص�һҳ��ִ��ʱ��Ϊ" + GetExecuteTimeString(), barStaticItemMsg);
            }

            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        private void simpleButton5_Click(object sender, EventArgs e)
        {
            try
            {
                SetExecuteBeginTime();
                if (lookupShowStyle.EditValue == "pivot")
                {
                    SetReportPage(documentViewer1.SelectedPageIndex - 1);
                    return;
                }


                if (PerPageRecord == 0)
                    return;

                var StartRecord = (CurrentPageNumber - 1) * PerPageRecord;
                if (StartRecord > PerPageRecord - 1)
                {
                    CurrentPageNumber--;

                    var strListSql = listDataExVo.StrListSQL.Replace("where 1=1",
                        "where 1=1 " + _strFreQryCondition + _strReceiveParaCondition + _strSortCondtion);
                    GetDistinctSql(ref strListSql);
                    dt = Model.GetPageData(strListSql, CurrentPageNumber, PerPageRecord);


                    BandingData();

                    SetListPageInfo();
                }
                else
                {
                    MessageShowUtil.ShowInfo("�ѵ���һҳ");
                }


                MessageShowUtil.ShowStaticInfo("������һҳ��ִ��ʱ��Ϊ" + GetExecuteTimeString(), barStaticItemMsg);
            }

            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        private void simpleButton6_Click(object sender, EventArgs e)
        {
            try
            {
                SetExecuteBeginTime();

                // 20170622
                //if (tlbSetCurrentPage.EditValue == null)
                if (textEdit1.Text == null)
                {
                    return;
                }

                //var currPage = tlbSetCurrentPage.EditValue.ToString().Split('/');
                var currPage = textEdit1.Text.ToString().Split('/');

                CurrentPageNumber = TypeUtil.ToInt(currPage[0]);
                if (CurrentPageNumber > TotalPage)
                    CurrentPageNumber = TotalPage;
                else if ((0 == CurrentPageNumber) && (TotalPage > 0))
                    CurrentPageNumber = 1;

                var strListSql = listDataExVo.StrListSQL.Replace("where 1=1",
                    "where 1=1 " + _strFreQryCondition + _strReceiveParaCondition + _strSortCondtion);
                GetDistinctSql(ref strListSql);
                if (TotalPage <= 1)
                    dt = Model.GetDataTable(strListSql);
                else
                    dt = Model.GetPageData(strListSql, CurrentPageNumber, PerPageRecord);
                BandingData();
                SetListPageInfo();
                if (lookupShowStyle.EditValue == "pivot")
                    SetReportPage(TypeUtil.ToInt(currPage[0]) - 1);
                MessageShowUtil.ShowStaticInfo("����ִ��ʱ��Ϊ" + GetExecuteTimeString(), barStaticItemMsg);
            }

            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        private void simpleButton7_Click(object sender, EventArgs e)
        {
            try
            {
                SetExecuteBeginTime();

                if (lookupShowStyle.EditValue == "pivot")
                {
                    SetReportPage(documentViewer1.SelectedPageIndex + 1);
                    return;
                }

                if (PerPageRecord == 0)
                    return;

                var StartRecord = CurrentPageNumber * PerPageRecord;
                if (StartRecord < TotalRecord)
                {
                    CurrentPageNumber++;

                    var strListSql = listDataExVo.StrListSQL.Replace("where 1=1",
                        "where 1=1 " + _strFreQryCondition + _strReceiveParaCondition + _strSortCondtion);
                    GetDistinctSql(ref strListSql);
                    dt = Model.GetPageData(strListSql, CurrentPageNumber, PerPageRecord);

                    BandingData();
                    SetListPageInfo();
                }
                else
                {
                    MessageShowUtil.ShowInfo("�ѵ����һҳ");
                }

                MessageShowUtil.ShowStaticInfo("������һҳ��ִ��ʱ��Ϊ" + GetExecuteTimeString(), barStaticItemMsg);
            }

            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        private void simpleButton8_Click(object sender, EventArgs e)
        {
            try
            {
                SetExecuteBeginTime();

                if (lookupShowStyle.EditValue == "pivot")
                {
                    SetReportPage(rp.Pages.Count - 1);
                    return;
                }

                if (PerPageRecord == 0 || TotalPage == 0)
                    return;

                var StartRecord = (TotalPage - 1) * PerPageRecord;
                if (StartRecord < TotalRecord)
                {
                    CurrentPageNumber = TotalPage;

                    var strListSql = listDataExVo.StrListSQL.Replace("where 1=1",
                        "where 1=1 " + _strFreQryCondition + _strReceiveParaCondition + _strSortCondtion);
                    GetDistinctSql(ref strListSql);
                    dt = Model.GetPageData(strListSql, CurrentPageNumber, PerPageRecord);

                    BandingData();

                    SetListPageInfo();
                }
                else
                {
                    MessageShowUtil.ShowInfo("�ѵ����һҳ");
                }

                MessageShowUtil.ShowStaticInfo("�������һҳ��ִ��ʱ��Ϊ" + GetExecuteTimeString(), barStaticItemMsg);
            }

            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        private void simpleButton10_Click(object sender, EventArgs e)
        {
            var strShowStyle = TypeUtil.ToString(lookupShowStyle.EditValue);
            if (strShowStyle == "pivot")
            {
                PrintPivot();
            }
            else
            {
                MessageShowUtil.ShowInfo("���Ƚ���Ԥ�����ٽ��д�ӡ������");
            }
        }

        private void simpleButton11_Click(object sender, EventArgs e)
        {
            try
            {
                if (lookupShowStyle.EditValue != null)
                {
                    // 20170821
                    if (dt == null)
                    {
                        MessageShowUtil.ShowMsg("���Ȳ�ѯ���ݡ�");
                        return;
                    }

                    DateTime dtRemark = GetRemarkTime();
                    var listDataRemarkInfo = Model.GetListDataRemarkByTimeListDataId(dtRemark, listDataExVo.ListDataID);

                    var frm = new frmInputDesc();
                    //frm.StrDesc = listDataExVo.StrListSrcSQL;

                    if (listDataRemarkInfo == null)
                    {
                        frm.StrDesc = "";
                    }
                    else
                    {
                        frm.StrDesc = listDataRemarkInfo.Remark;
                    }

                    frm.ShowDialog();
                    if (frm.BlnOK)
                    {
                        //listDataExVo.StrListSrcSQL = frm.StrDesc;
                        //listDataExVo.InfoState = InfoState.Modified;
                        //Model.SaveListDataEx(listDataExVo);
                        if (listDataRemarkInfo == null)
                        {
                            Model.AddListdataremark(listDataExVo.ListDataID, dtRemark, frm.StrDesc);
                        }
                        else
                        {
                            Model.UpdateListdataremarkByTimeListDataId(listDataExVo.ListDataID, dtRemark, frm.StrDesc);
                        }

                        listTemplevo = Model.GetListTemple(listDataExVo.ListDataID);
                        lookupShowStyle_EditValueChanged(null, null);
                        //MessageShowUtil.ShowInfo("��ע����ɹ�");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        private void simpleButton12_Click(object sender, EventArgs e)
        {
            try
            {
                var rows = metadataFieldDt.Select("MetaDataID=" + listExvo.MainMetaDataID + " and blnDesignSort=1");
                if ((rows == null) || (rows.Length == 0))
                {
                    MessageShowUtil.ShowMsg("Ԫ������û�����ÿɱ��ŵ���Ŀ�����ܽ��б���");
                    return;
                }
                var strFKCode = TypeUtil.ToString(rows[0]["strFkCode"]);
                var frm = new frmDataSortSet();
                frm.MetadataID = listExvo.MainMetaDataID;
                frm.dtLmd = _lmdDt;
                var strFileName =
                    _lmdDt.Select("MetaDataFieldID=" + TypeUtil.ToInt(rows[0]["ID"]))[0]["MetaDataFieldName"].ToString();
                frm.strFileName = strFileName;
                var strListDisplayName =
                    _lmdDt.Select("MetaDataFieldID=" + TypeUtil.ToInt(rows[0]["ID"]))[0]["strListDisplayFieldNameCHS"]
                        .ToString();
                frm.strListDisplayName = strListDisplayName;
                frm.strFKCode = strFKCode;
                frm.ListDataID = ListDataID;
                frm.ShowDialog();
                if (frm.blnOK)
                {
                }
            }
            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }
    }

    public class FreQryCondition
    {
        private readonly Panel _panel;
        private readonly int controlHeight = 20;
        private int controlWidth;
        private readonly IDictionary<string, string> dicConditionOldTable = new Dictionary<string, string>();
        private readonly int intXSpacing = 5;
        private int intYSpacing = 15;
        private List<string> listsdate;
        private int panelWidth;

        public PanelControl PointFilter { get; set; }

        /// <summary>
        ///     ���캯��
        /// </summary>
        /// <param name="panel">�������</param>
        public FreQryCondition(Panel panel)
        {
            _panel = panel;

            if (_panel != null)
                panelWidth = _panel.Width;
        }

        /// <summary>
        ///     ��ѯ���
        /// </summary>
        public Panel QryPanel
        {
            get { return _panel; }
        }

        public void CalcControlWidth(int cols)
        {
            controlWidth = (panelWidth - (cols + 1) * intXSpacing) / cols;
            // controlWidth = panelWidth - 5;
        }

        public void CreateControl(DataTable lmdDt)
        {
            if (lmdDt == null) return;
            if (_panel.Controls.Count > 0)
            {
                //_panel.Controls.Clear();
                //��д�ͷſؼ��ķ�����ֱ��clear�ᵼ�¾����Դһֱ����  20180422
                while (_panel.Controls.Count > 0)
                {
                    if (_panel.Controls[0] != null)
                        _panel.Controls[0].Dispose();
                }
            }
            var dt = lmdDt.Copy();
            dt.DefaultView.RowFilter = "blnFreCondition=1";
            dt.DefaultView.Sort = "lngFreConIndex asc";
            dt = dt.DefaultView.ToTable();
            var count = dt.Rows.Count;
            int row;
            int col;
            var strFieldName = "";
            var strCurFieldNameCHS = "";
            var strFieldType = "";
            var strFkCode = "";
            var strValPara = "";
            var strDisplayPara = "";
            var blnPrintFreCondition = false;
            var lngFKType = 0;
            DataRow curDr = null;
            for (var i = 0; i < count; i++)
            {
                row = (i + 1) / 2;
                col = (i + 1) % 2;//20190119   /3
                //row = i;
                //col = 1;

                if (col == 0)
                    col = 2;//20190119   /3
                else
                    row = row + 1;

                curDr = dt.Rows[i];
                strFieldName = TypeUtil.ToString(curDr["MetaDataFieldName"]);
                strCurFieldNameCHS = TypeUtil.ToString(curDr["strListDisplayFieldNameCHS"]);
                strFieldType = TypeUtil.ToString(curDr["strFieldType"]);
                strFkCode = TypeUtil.ToString(curDr["strFkCode"]);
                strValPara = TypeUtil.ToString(curDr["strFreCondition"]);
                strDisplayPara = TypeUtil.ToString(curDr["strFreConditionCHS"]);
                blnPrintFreCondition = TypeUtil.ToBool(curDr["blnPrintFreCondition"]);
                lngFKType = TypeUtil.ToInt(curDr["lngFKType"]);
                if (strFkCode != string.Empty)
                {
                    CreateRefControl(strCurFieldNameCHS, strFieldName, row, col, strValPara, strDisplayPara, strFkCode,
                        blnPrintFreCondition, strFieldType, lngFKType);
                    continue;
                }

                strFieldType = strFieldType.ToLower();
                if ((strFieldType == "varchar") || (strFieldType == "nvarchar") || (strFieldType == "nchar") ||
                    (strFieldType == "char") || (strFieldType == "ntext"))
                    CreateTextControl(strCurFieldNameCHS, strFieldName, row, col, strValPara, blnPrintFreCondition);
                else if ((strFieldType == "money") || (strFieldType == "decimal") || (strFieldType == "float") ||
                         (strFieldType == "double")
                         || (strFieldType == "int") || (strFieldType == "smallint") || (strFieldType == "bigint") ||
                         (strFieldType == "tinyint"))
                    CreateNumberControl(strCurFieldNameCHS, strFieldName, row, col, strValPara, blnPrintFreCondition);
                else if (strFieldType == "bit")
                    CreateBooleanControl(strCurFieldNameCHS, strFieldName, row, col, strValPara, blnPrintFreCondition);
                else if ((strFieldType == "smalldatetime") || (strFieldType == "datetime"))
                    CreateDateTimeControl(strCurFieldNameCHS, strFieldName, row, col, strValPara, blnPrintFreCondition,
                        lngFKType);
            }
        }

        /// <summary>
        ///     ���ÿؼ�����
        /// </summary>
        public void SetControlLocation()
        {
            int row;
            int col;
            panelWidth = _panel.Width;
            CalcControlWidth(2);//20190119
            for (var i = 0; i < _panel.Controls.Count; i++)
            {
                row = (i + 1) / 2;
                col = (i + 1) % 2;//20190119
                //row = i;
                //col = 1;
                if (col == 0)
                    col = 2;//20190119
                else
                    row = row + 1;
                _panel.Controls[i].Width = controlWidth;
                _panel.Controls[i].Location = GetLocation(row, col);
            }
        }

        /// <summary>
        ///     ��ȡ���ò�ѯ����
        /// </summary>
        public string GetFreQryCondition()
        {
            dicConditionOldTable.Clear();
            var strFreWhere = ""; //���ò�ѯ����
            var strFieldName = "";
            var strFieldType = "";
            var strFreCondition = "";
            var strFkCode = "";
            var controlCount = _panel.Controls.Count;
            if (controlCount > 0)
                for (var i = 0; i < controlCount; i++)
                {
                    strFieldName = "";
                    strFieldType = "";
                    strFreCondition = "";
                    strFkCode = "";
                    var hash = (Hashtable)_panel.Controls[i].Tag;
                    if (hash.ContainsKey("_fieldName"))
                        strFieldName = TypeUtil.ToString(hash["_fieldName"]);
                    if (strFieldName == string.Empty)
                        continue;
                    if (hash.ContainsKey("_fieldType"))
                        strFieldType = TypeUtil.ToString(hash["_fieldType"]);
                    if (strFieldType == string.Empty)
                        continue;

                    if (_panel.Controls[i] is UCRef || _panel.Controls[i] is UCGridLookUp)
                    {
                        if (TypeUtil.ToString(hash["_strKeyValue"]) == string.Empty)
                            continue;

                        strFkCode = TypeUtil.ToString(hash["_fkCode"]);
                        if (!TypeUtil.ToString(hash["_fieldType"]).Contains("int"))
                            strFreCondition += "����&&$" + "'" +
                                               TypeUtil.ToString(hash["_strDisplayValue"]).Replace(",", "','") + "'";
                        else
                            strFreCondition += "����&&$" + TypeUtil.ToString(hash["_strKeyValue"]);
                    }
                    else //UCDateTimeOne || UCText || UCNumber
                    {
                        strFreCondition = TypeUtil.ToString(hash["_strKeyValue"]);
                    }

                    var str = "";
                    if (strFreCondition != string.Empty)
                    {
                        var strfilename = strFieldName;
                        var index = strfilename.LastIndexOf("_");
                        if (index > -1)
                            strfilename = "" + strfilename.Remove(index, 1).Insert(index, ".");


                        if (strFkCode != string.Empty)
                            str = BulidConditionUtil.GetRefCondition(strfilename, strFreCondition, strFieldType);
                        else
                            str = BulidConditionUtil.GetConditionString(strfilename, strFieldType, strFreCondition);
                        if (str != string.Empty)
                        {
                            strFreWhere += " and " + str;

                            if (!str.ToLower().Contains("datsearch") && !str.Contains("state in"))
                            {
                                var strTableName = strfilename.Substring(0, strfilename.IndexOf(".") - 1);

                                // 20170705
                                //var strContion = str.Substring(0, str.IndexOf(".") - 1) +
                                //                 str.Substring(str.IndexOf("."));
                                int repIndex = strfilename.IndexOf(".");
                                string repStr = strfilename.Substring(0, repIndex);
                                var strContion = str.Replace(repStr, strTableName);

                                if (dicConditionOldTable.ContainsKey(strTableName))
                                    dicConditionOldTable[strTableName] += " and " + strContion;
                                else
                                    dicConditionOldTable.Add(strTableName, strContion);
                            }
                        }
                    }

                    if ((strFieldType == "smalldatetime") || (strFieldType == "datetime"))
                        listsdate = hash["_date"] as List<string>;
                }

            return strFreWhere;
        }

        /// <summary>
        ///     �õ���ѯ���������Ĵ������ڴ�ӡ��ʱ����ʾ��ѯ����
        /// </summary>
        /// <returns></returns>
        public string GetFreQryCondtionByChs()
        {
            var strFreWhere = ""; //���ò�ѯ����
            var strFieldName = "";
            var strFieldType = "";
            var strFreCondition = "";
            var strFkCode = "";
            var strFieldNameByChs = "";
            var controlCount = _panel.Controls.Count;
            var blnPrintFreCondition = false;

            if (controlCount > 0)
                for (var i = 0; i < controlCount; i++)
                {
                    strFieldName = "";
                    strFieldType = "";
                    strFreCondition = "";
                    strFkCode = "";
                    var hash = (Hashtable)_panel.Controls[i].Tag;
                    if (hash.ContainsKey("_fieldName"))
                        strFieldName = TypeUtil.ToString(hash["_fieldName"]);
                    if (strFieldName == string.Empty)
                        continue;
                    if (hash.ContainsKey("_fieldType"))
                        strFieldType = TypeUtil.ToString(hash["_fieldType"]);
                    if (strFieldType == string.Empty)
                        continue;
                    if (hash.ContainsKey("_strTitleName"))
                        strFieldNameByChs = TypeUtil.ToString(hash["_strTitleName"]);
                    if (strFieldNameByChs == string.Empty)
                        continue;

                    if (hash.ContainsKey("_blnPrintFreCondition"))
                        blnPrintFreCondition = TypeUtil.ToBool(hash["_blnPrintFreCondition"]);
                    if (!blnPrintFreCondition)
                        continue;


                    if (_panel.Controls[i] is UCRef)
                    {
                        if (TypeUtil.ToString(hash["_strKeyValue"]) == string.Empty)
                            continue;

                        strFkCode = TypeUtil.ToString(hash["_fkCode"]);
                        strFreCondition += "����&&$" + TypeUtil.ToString(hash["_strDisplayValue"]);
                    }
                    else //UCDateTimeOne || UCText || UCNumber
                    {
                        strFreCondition = TypeUtil.ToString(hash["_strKeyValue"]);
                    }

                    var str = "";
                    if (strFreCondition != string.Empty)
                    {
                        var strfilename = strFieldName;
                        var index = strfilename.LastIndexOf("_");
                        if (index > -1)
                            strfilename = "" + strfilename.Remove(index, 1).Insert(index, ".");


                        if (strFkCode != string.Empty)
                            str = BulidConditionUtil.GetRefCondition(strfilename, strFreCondition, strFieldType);
                        else
                            str = BulidConditionUtil.GetConditionString(strfilename, strFieldType, strFreCondition);
                        if (str != string.Empty)
                            if (strFieldType == "datetime")
                            {
                                var strChsValue = TypeUtil.ToString(hash["_strValueChs"]);
                                strFreWhere += strFieldNameByChs + ":" + strChsValue + "  ";
                            }
                            else
                                strFreWhere += strFieldNameByChs + ":" + strFreCondition.Replace("&&$", "") + "     ";
                    }
                }

            return strFreWhere;
        }

        /// <summary>
        ///     �õ��ձ����ڶε����ڼ���
        /// </summary>
        /// <returns></returns>
        public List<string> GetDayDate()
        {
            return listsdate;
        }

        public IDictionary<string, string> GetConditionOldTable()
        {
            return dicConditionOldTable;
        }

        /// <summary>
        ///     ���������Ϳؼ�
        /// </summary>
        private void CreateRefControl(string titleName, string fieldName, int row, int col, string strValPara,
            string strDisplayPara, string fkCode, bool blnPrintFreCondition, string strFieldType, int RefType)
        {
            try
            {
                if (RefType <= 1)
                {
                    var control = new UCRef(titleName);
                    var hash = new Hashtable();
                    hash.Add("_fieldName", fieldName);
                    hash.Add("_fieldType", strFieldType);
                    hash.Add("_fkCode", fkCode);
                    hash.Add("_strKeyValue", strValPara);
                    hash.Add("_strDisplayValue", strDisplayPara);
                    hash.Add("_strTitleName", titleName);
                    hash.Add("_blnPrintFreCondition", blnPrintFreCondition);
                    control.Tag = hash;
                    control.FkCode = fkCode;
                    control.Width = controlWidth;
                    control.Location = GetLocation(row, col);
                    _panel.Controls.Add(control);
                    control.StrKeyValue = strValPara;
                    control.StrDisplayValue = strDisplayPara;
                    if (fkCode.Contains("AllPoint"))
                    {
                        control.PointFilter = PointFilter;
                    }
                }
                if (RefType == 2)
                {
                    //���Ϊ2��ʾ������
                    var control = new UCGridLookUp(titleName, fkCode);
                    var hash = new Hashtable();
                    hash.Add("_fieldName", fieldName);
                    hash.Add("_fieldType", strFieldType);
                    hash.Add("_fkCode", fkCode);
                    hash.Add("_strKeyValue", strValPara);
                    hash.Add("_strDisplayValue", strDisplayPara);
                    hash.Add("_strTitleName", titleName);
                    hash.Add("_blnPrintFreCondition", blnPrintFreCondition);
                    control.Tag = hash;
                    control.FkCode = fkCode;
                    control.Width = controlWidth;
                    control.Location = GetLocation(row, col);
                    _panel.Controls.Add(control);
                    control.StrKeyValue = strValPara;
                    control.StrDisplayValue = strDisplayPara;
                }
            }
            catch (Exception e)
            {
                MessageShowUtil.ShowInfo("�������տؼ�����\n" + e.Message);
            }
        }

        /// <summary>
        ///     ������ֵ�Ϳؼ�
        /// </summary>
        private void CreateNumberControl(string titleName, string fieldName, int row, int col, string strValPara,
            bool blnPrintFreCondition)
        {
            try
            {
                var control = new UCNumber(titleName);
                var hash = new Hashtable();
                hash.Add("_fieldName", fieldName);
                hash.Add("_fieldType", "decimal");
                hash.Add("_strKeyValue", strValPara);
                hash.Add("_strTitleName", titleName);
                hash.Add("_blnPrintFreCondition", blnPrintFreCondition);
                control.HeaderValue = strValPara;
                control.Tag = hash;
                control.Width = controlWidth;
                control.Location = GetLocation(row, col);
                _panel.Controls.Add(control);
            }
            catch (Exception e)
            {
                MessageShowUtil.ShowInfo("�������տؼ�����\n" + e.Message);
            }
        }

        /// <summary>
        ///     ���������Ϳؼ�
        /// </summary>
        private void CreateDateTimeControl(string titleName, string fieldName, int row, int col, string strValPara,
            bool blnPrintFreCondition, int RefType)
        {
            try
            {
                if (RefType <= 11)
                {
                    var control = new UCDateTimeTwo(titleName);
                    var hash = new Hashtable();
                    hash.Add("_fieldName", fieldName);
                    hash.Add("_fieldType", "datetime");
                    strValPara = " " + "&&$" + DateTime.Now.ToShortDateString() + "&&$" +
                                 DateTime.Now.ToShortDateString();
                    hash.Add("_strKeyValue", strValPara);
                    hash.Add("_strTitleName", titleName);
                    hash.Add("_blnPrintFreCondition", blnPrintFreCondition);
                    hash.Add("_date", null);
                    hash.Add("_strValueChs", ""); //��ѯ��������
                    control.Tag = hash;
                    control.Width = controlWidth;
                    control.Location = GetLocation(row, col);
                    _panel.Controls.Add(control);

                    control.HeaderValue = strValPara;
                }
                if (RefType == 12)
                {
                    //������ ʱ�ָ�ʽ
                    var control = new UCDateTimeShiFen(titleName);
                    //UCDateTimeClass control = new UCDateTimeClass(titleName);
                    //UCDateTimeTwo control = new UCDateTimeTwo(titleName);
                    var hash = new Hashtable();
                    hash.Add("_fieldName", fieldName);
                    hash.Add("_fieldType", "datetime");
                    strValPara = " " + "&&$" + DateTime.Now.ToShortDateString() + " 00:00" + "&&$" +
                                 DateTime.Now.ToShortDateString() + " 23:59";
                    hash.Add("_strKeyValue", strValPara);
                    hash.Add("_strTitleName", titleName);
                    hash.Add("_blnPrintFreCondition", blnPrintFreCondition);
                    hash.Add("_date", null);
                    hash.Add("_strValueChs", ""); //��ѯ��������
                    control.Tag = hash;
                    control.Width = controlWidth;
                    control.Location = GetLocation(row, col);
                    _panel.Controls.Add(control);

                    control.HeaderValue = strValPara;
                }
                if (RefType == 13)
                {
                    //��θ�ʽ
                    var control = new UCDateTimeClass(titleName);
                    var hash = new Hashtable();
                    hash.Add("_fieldName", fieldName);
                    hash.Add("_fieldType", "datetime");
                    strValPara = " " + "&&$" + DateTime.Now.ToShortDateString();
                    hash.Add("_strKeyValue", strValPara);
                    hash.Add("_strTitleName", titleName);
                    hash.Add("_blnPrintFreCondition", blnPrintFreCondition);
                    hash.Add("_date", null);
                    hash.Add("_strValueChs", ""); //��ѯ��������
                    control.Tag = hash;
                    control.Width = controlWidth;
                    control.Location = GetLocation(row, col);
                    _panel.Controls.Add(control);

                    control.HeaderValue = strValPara;
                }
                if (RefType == 14)
                {
                    //�¸�ʽ
                    var control = new UCDateTimeMonth(titleName);
                    var hash = new Hashtable();
                    hash.Add("_fieldName", fieldName);
                    hash.Add("_fieldType", "datetime");
                    strValPara = " " + "&&$" + DateTime.Now.ToShortDateString() + "&&$" +
                                 DateTime.Now.ToShortDateString();
                    hash.Add("_strKeyValue", strValPara);
                    hash.Add("_strTitleName", titleName);
                    hash.Add("_blnPrintFreCondition", blnPrintFreCondition);
                    hash.Add("_date", null);
                    hash.Add("_strValueChs", ""); //��ѯ��������
                    control.Tag = hash;
                    control.Width = controlWidth;
                    control.Location = GetLocation(row, col);
                    _panel.Controls.Add(control);

                    control.HeaderValue = strValPara;
                }
                if (RefType == 15)
                {
                    //����ʽ
                    //UCDateTimeYear control = new UCDateTimeYear(titleName);
                    //Hashtable hash = new Hashtable();
                    //hash.Add("_fieldName", fieldName);
                    //hash.Add("_fieldType", "datetime");
                    //strValPara = " " + "&&$" + DateTime.Now.ToShortDateString() + "&&$" + DateTime.Now.ToShortDateString();
                    //hash.Add("_strKeyValue", strValPara);
                    //hash.Add("_strTitleName", titleName);
                    //hash.Add("_blnPrintFreCondition", blnPrintFreCondition);
                    //hash.Add("_date", null);
                    //hash.Add("_strValueChs", "");//��ѯ��������
                    //control.Tag = hash;
                    //control.Width = controlWidth;
                    //control.Location = GetLocation(row, col);
                    //this._panel.Controls.Add(control);

                    //control.HeaderValue = strValPara;
                }
                if (RefType == 16)
                {
                    //���ʽ
                    var control = new UCDateTimeYear(titleName);
                    var hash = new Hashtable();
                    hash.Add("_fieldName", fieldName);
                    hash.Add("_fieldType", "datetime");
                    strValPara = " " + "&&$" + DateTime.Now.ToShortDateString() + "&&$" +
                                 DateTime.Now.ToShortDateString();
                    hash.Add("_strKeyValue", strValPara);
                    hash.Add("_strTitleName", titleName);
                    hash.Add("_blnPrintFreCondition", blnPrintFreCondition);
                    hash.Add("_date", null);
                    hash.Add("_strValueChs", ""); //��ѯ��������
                    control.Tag = hash;
                    control.Width = controlWidth;
                    control.Location = GetLocation(row, col);
                    _panel.Controls.Add(control);

                    control.HeaderValue = strValPara;
                }
            }
            catch (Exception e)
            {
                MessageShowUtil.ShowInfo("�������տؼ�����\n" + e.Message);
            }
        }

        /// <summary>
        ///     �����߼��Ϳؼ�
        /// </summary>
        private void CreateBooleanControl(string titleName, string fieldName, int row, int col, string strValPara,
            bool blnPrintFreCondition)
        {
            try
            {
                var control = new UCBoolean(titleName);
                var hash = new Hashtable();
                hash.Add("_fieldName", fieldName);
                hash.Add("_fieldType", "bit");
                hash.Add("_strKeyValue", strValPara);
                hash.Add("_strTitleName", titleName);
                hash.Add("_blnPrintFreCondition", blnPrintFreCondition);
                control.Tag = hash;
                control.Width = controlWidth;
                control.StrKeyValue = strValPara;
                control.Location = GetLocation(row, col);
                _panel.Controls.Add(control);
            }
            catch (Exception e)
            {
                MessageBox.Show("�����߼��Ϳؼ�����\n" + e.Message);
            }
        }

        /// <summary>
        ///     �����ַ��Ϳؼ�
        /// </summary>
        private void CreateTextControl(string titleName, string fieldName, int row, int col, string strValPara,
            bool blnPrintFreCondition)
        {
            try
            {
                var control = new UCTextTwo(titleName);
                var hash = new Hashtable();
                hash.Add("_fieldName", fieldName);
                hash.Add("_fieldType", "varchar");
                hash.Add("_strKeyValue", strValPara);
                hash.Add("_strTitleName", titleName);
                hash.Add("_blnPrintFreCondition", blnPrintFreCondition);
                control.Tag = hash;
                control.Width = controlWidth;
                control.Location = GetLocation(row, col);
                _panel.Controls.Add(control);

                control.HeaderValue = strValPara;
            }
            catch (Exception e)
            {
                MessageShowUtil.ShowInfo("�������տؼ�����\n" + e.Message);
            }
        }

        /// <summary>
        ///     ��ȡ�ؼ�����
        /// </summary>
        /// <param name="row">����</param>
        /// <param name="col">����</param>
        /// <returns>Point</returns>
        private Point GetLocation(int row, int col)
        {
            int intX = 5, intY = 5;
            intYSpacing = 10; //2015-12-22  ��Ĭ��ֵ��15���ָ�Ϊ10���Լ����������еľ���
            intX = intX + (col - 1) * (controlWidth + intXSpacing);
            intY = intY + (row - 1) * (controlHeight + intYSpacing);
            return new Point(intX, intY);
        }
    }

    public class ListTitleEventArgs
    {
        /// <summary>
        ///     �б����
        /// </summary>
        public string StrTitle = "";
    }
}