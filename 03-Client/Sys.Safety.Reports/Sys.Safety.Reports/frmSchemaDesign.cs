using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraBars;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraTreeList;
using DevExpress.XtraTreeList.Nodes;
using Basic.Framework.Web;
using Sys.Safety.DataContract;
using Sys.Safety.Reports.Model;
using Sys.Safety.Reports.PubClass;
using CellValueChangedEventArgs = DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs;

namespace Sys.Safety.Reports
{
    public partial class frmSchemaDesign : XtraForm
    {
        private string _strListName = "";
        private IDictionary<string, int> aliasNumDic = new Dictionary<string, int>();
        private DataTable curNodeDt; //��ǰ���ڵ��Ӧ����


        private GridHitInfo downHitInfo; //����������ȥʱ��GridView�е�����
        private DataTable metadataDt;
        private DataTable metadataFieldDt;
        private readonly ListExModel Model = new ListExModel();
        private readonly IList<string> refCalcColList = new List<string>();
        private readonly IList<string> refContextContextColList = new List<string>();
        private int[] rows; //��ק��������
        private int startRow; // ��ק�ĵ�һ
        private readonly IDictionary<string, DataTable> tableAliasDataDic = new Dictionary<string, DataTable>();
        private readonly DataTable treeDt = new DataTable();
        private GridHitInfo upHitInfo; //������������ʱ��GridView�е�����

        public frmSchemaDesign()
        {
            BlnOk = false;
            BlnListEnter = false;
            ListDisplayExList = null;
            LmdDt = null;
            ListDataExVo = null;
            MetadataId = 0;
            CurListDataId = 0;
            ListDataID = 0;
            ListID = 0;
            InitializeComponent();
        }

        /// <summary>
        ///     �б�ID
        /// </summary>
        public int ListID { get; set; }

        /// <summary>
        ///     �б�����ID
        /// </summary>
        public int ListDataID { get; set; }

        /// <summary>
        ///     �б�����ʹ�õķ���ID
        /// </summary>
        public int CurListDataId { get; set; }

        /// <summary>
        ///     ��ʵ��ID
        /// </summary>
        public int MetadataId { get; set; }

        public ListdataexInfo ListDataExVo { get; set; }

        /// <summary>
        ///     ѡ�����Լ�����������
        /// </summary>
        public DataTable LmdDt { get; set; }

        public IList<ListdisplayexInfo> ListDisplayExList { get; set; }


        /// <summary>
        ///     �б����
        /// </summary>
        public bool BlnListEnter { get; set; }

        public bool BlnOk { get; set; }


        public string StrListName
        {
            get { return _strListName; }
            set { _strListName = value; }
        }

        private void frmSchemaDesign_Load(object sender, EventArgs e)
        {
            try
            {
                //��ʼ����
                InitTree();

                //��ȡԪ��������
                GetMetaDataDesc();

                FillLookup();
                SetShowStyle();

                if (ListDataID <= 0)
                    CreateNewSchema();

                LoadSchema(true);
                SetControlVisible();
                MessageShowUtil.ShowStaticInfo("����ɹ�", barStaticItemMsg);
            }
            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        /// <summary>
        ///     �����·���
        /// </summary>
        private void CreateNewSchema()
        {
            ListDataExVo = new ListdataexInfo();
            ListDataExVo.InfoState = InfoState.AddNew;
        }

        /// <summary>
        ///     ���뷽��
        /// </summary>
        private void LoadSchema(bool blnFormLoad)
        {
            lookUpSchema.EditValueChanged -= lookUpSchema_EditValueChanged;
            lookUpSchema.EditValue = ListDataID;
            lookUpSchema.EditValueChanged += lookUpSchema_EditValueChanged;

            //��ȡ����������б�
            if (ListDataID > 0)
                aliasNumDic = Model.GetAliasNumDic(ListDataID);

            LoadTreeData();

            //��ȡѡ���¼
            if (blnFormLoad)
            {
                if (LmdDt == null)
                    LmdDt = Model.GetListMetaData(ListDataID, ListDataExVo == null ? 0 : ListDataExVo.UserID);

                if (ListDataID > 0)
                {
                    if (ListDataExVo == null)
                        ListDataExVo = Model.GetVO("listdatainfo", ListDataID) as ListdataexInfo;

                    ListDataExVo.InfoState = InfoState.Modified;
                }
            }
            else
            {
                if (ListDataID > 0)
                {
                    ListDataExVo = Model.GetVO("listdatainfo", ListDataID) as ListdataexInfo;
                    ListDataExVo.InfoState = InfoState.Modified;
                }

                LmdDt = Model.GetListMetaData(ListDataID, ListDataExVo.UserID);
            }

            SetSelectedColSrcFieldChs();
            SetRefCalcCol();
            SetRefContextContextCol();
            gcColSelected.DataSource = LmdDt;
            gcAdvanceSet.DataSource = LmdDt;
            gridControlCondition.DataSource = LmdDt;
            LmdDt.DefaultView.RowFilter = "isnull(blnSysProcess,0)=0";

            ShowGroupTitle();
            chkSmlSum.Checked = ListDataExVo.BlnSmlSum;
            lookUpSmlSumType.EditValue = ListDataExVo.LngSmlSumType;

            treeList1.SetFocusedNode(treeList1.Nodes.FirstNode);
            treeList1_Click(null, null);
        }

        /// <summary>
        ///     ��ʼ�����ؼ�
        /// </summary>
        private void InitTree()
        {
            AddTreeDataColumn();
            SetTreeListProperty();
            treeList1.DataSource = treeDt;
            treeDt.DefaultView.RowFilter =
                "isnull(blnPK,0)<>1 or strFieldName='ReceiptTypeID' or strFieldName='OrderTypeID'";
        }

        /// <summary>
        ///     ��ȡԪ��������
        /// </summary>
        private void GetMetaDataDesc()
        {
            metadataDt = ClientCacheModel.GetServerMetaData();
            metadataFieldDt = ClientCacheModel.GetServerMetaDataFields();
        }

        /// <summary>
        ///     ���Ԫ���ݹ���������Դ��
        /// </summary>
        private void AddTreeDataColumn()
        {
            treeDt.Columns.Add(new DataColumn("strId", Type.GetType("System.String"))); //�ڵ�Ψһ��ʶ��
            treeDt.Columns.Add(new DataColumn("metaDataId", Type.GetType("System.Int32"))); //Ԫ����ID lngRelativeFieldID
            treeDt.Columns.Add(new DataColumn("parentId", Type.GetType("System.Int32"))); //Ԫ���ݸ�ID
            treeDt.Columns.Add(new DataColumn("strName", Type.GetType("System.String"))); //Ԫ��������
            treeDt.Columns.Add(new DataColumn("metaDataFieldId", Type.GetType("System.Int32"))); //Ԫ�����ֶ�ID
            treeDt.Columns.Add(new DataColumn("lngParentFieldId", Type.GetType("System.Int32"))); //Ԫ���ݸ��ֶ�ID
            treeDt.Columns.Add(new DataColumn("lngRelativeFieldID", Type.GetType("System.Int32"))); //Ԫ���ݹ����ֶ�ID
            treeDt.Columns.Add(new DataColumn("strTableAlias", Type.GetType("System.String"))); //�����
            treeDt.Columns.Add(new DataColumn("strParentTableAlias", Type.GetType("System.String"))); //�������
            treeDt.Columns.Add(new DataColumn("lngAliasCount", Type.GetType("System.Int32"))); //��������lngAliasCount
            treeDt.Columns.Add(new DataColumn("lngNextAliasCount", Type.GetType("System.Int32"))); //�¼��ڵ��������
            treeDt.Columns.Add(new DataColumn("strFullPath", Type.GetType("System.String"))); //�ֶ�ȫ·������
            treeDt.Columns.Add(new DataColumn("strParentFullPath", Type.GetType("System.String"))); //����ȫ·������
            treeDt.Columns.Add(new DataColumn("strNextFullPath", Type.GetType("System.String"))); //�¼��ڵ�ȫ·������

            treeDt.Columns.Add(new DataColumn("strFieldName", Type.GetType("System.String"))); //�ֶ���
            treeDt.Columns.Add(new DataColumn("strFieldChName", Type.GetType("System.String"))); //�ֶ�������
            treeDt.Columns.Add(new DataColumn("strFieldType", Type.GetType("System.String"))); //�ֶ�����
            treeDt.Columns.Add(new DataColumn("strFkCode", Type.GetType("System.String"))); //���ò��ձ���
            treeDt.Columns.Add(new DataColumn("lngSourceType", Type.GetType("System.Int32"))); //�ֶι���������Դ����
            treeDt.Columns.Add(new DataColumn("lngRelativeID", Type.GetType("System.Int32"))); //����Ԫ����ID
            treeDt.Columns.Add(new DataColumn("blnPK", Type.GetType("System.Boolean"))); //�Ƿ�ΪPK�ֶ�
            treeDt.Columns.Add(new DataColumn("lngDataRightType", Type.GetType("System.Int32"))); //����Ȩ������   
            treeDt.Columns.Add(new DataColumn("strSummaryDisplayFormat", Type.GetType("System.String"))); //��ʾ��ʽ
        }

        /// <summary>
        ///     �������ؼ�����
        /// </summary>
        private void SetTreeListProperty()
        {
            treeList1.KeyFieldName = "strTableAlias";
            treeList1.ParentFieldName = "strParentTableAlias";
            treeList1.OptionsBehavior.AllowIndeterminateCheckState = true;
            treeList1.OptionsView.ShowCheckBoxes = true;
            treeList1.OptionsView.ShowHorzLines = false;
            treeList1.OptionsView.ShowVertLines = false;
        }

        /// <summary>
        ///     ������ؼ�����Դ��
        /// </summary>
        private void AddRelationDataColumn()
        {
            LmdDt.Columns.Add(new DataColumn("strFieldName", Type.GetType("System.String"))); //�ֶ���
            LmdDt.Columns.Add(new DataColumn("strFieldChName", Type.GetType("System.String"))); //�ֶ�������
            LmdDt.Columns.Add(new DataColumn("lngRelativeID", Type.GetType("System.Int32"))); //����Ԫ����ID
        }

        /// <summary>
        ///     ˢ�·�������
        /// </summary>
        private void RefreshSchemaDataSource()
        {
            LmdDt = Model.GetListMetaData(ListDataID, ListDataExVo == null ? 0 : ListDataExVo.UserID);
            SetSelectedColSrcFieldChs();
            SetRefCalcCol();
            SetRefContextContextCol();
            gcColSelected.DataSource = LmdDt;
            gcAdvanceSet.DataSource = LmdDt;
            gridControlCondition.DataSource = LmdDt;
            LmdDt.DefaultView.RowFilter = "isnull(blnSysProcess,0)=0";
        }

        private void SetSelectedColSrcFieldChs()
        {
            if (LmdDt == null) return;

            for (var i = 0; i < LmdDt.Rows.Count; i++)
                if (TypeUtil.ToBool(LmdDt.Rows[i]["isCalcField"]))
                    LmdDt.Rows[i]["strSrcFieldNameCHS"] = TypeUtil.ToString(LmdDt.Rows[i]["strListDisplayFieldNameCHS"]);
                else
                    LmdDt.Rows[i]["strSrcFieldNameCHS"] =
                        GetMetaDataFieldNameCHS(TypeUtil.ToInt(LmdDt.Rows[i]["MetaDataFieldID"]));
        }

        /// <summary>
        ///     ����������
        /// </summary>
        private void SetRefCalcCol()
        {
            if (LmdDt == null) return;

            refCalcColList.Clear();
            var strRefColList = string.Empty;
            for (var i = 0; i < LmdDt.Rows.Count; i++)
                if (TypeUtil.ToBool(LmdDt.Rows[i]["isCalcField"]))
                {
                    //����������
                    strRefColList = TypeUtil.ToString(LmdDt.Rows[i]["strRefColList"]);
                    if (strRefColList == string.Empty) continue;

                    var strs = strRefColList.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var str in strs)
                    {
                        if (refCalcColList.Contains(str))
                            continue;

                        refCalcColList.Add(str);
                    }
                }
        }

        /// <summary>
        ///     ��������������������
        /// </summary>
        private void SetRefContextContextCol()
        {
            if (ListDataExVo == null) return;

            refContextContextColList.Clear();
            if ((TypeUtil.ToString(ListDataExVo.StrConTextCondition) == string.Empty) ||
                (ListDataExVo.StrConTextCondition == null))
                return;

            var strConTextCondition = GetConTextConditionRefColString(ListDataExVo.StrConTextCondition);
            var strs = strConTextCondition.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries);
            foreach (var str in strs)
            {
                if (refContextContextColList.Contains(str))
                    continue;

                refContextContextColList.Add(str);
            }
        }

        private void FillLookup()
        {
            SetOrderMethodData();
            SetSummaryTypeData();
            SetDisplayFormat();
            SetApplyTypeData();
            SetFKTypeData();
            SetLngKeyGroupData();
            SetLngProivtTypeData();
            LoadShcemaList();
        }

        private void SetOrderMethodData()
        {
            var orderMethodDt = new DataTable();
            orderMethodDt.Columns.Add(new DataColumn("ID", Type.GetType("System.Int32")));
            orderMethodDt.Columns.Add(new DataColumn("Name", Type.GetType("System.String")));

            var dr = orderMethodDt.NewRow();
            dr["ID"] = 0;
            dr["Name"] = "";
            orderMethodDt.Rows.Add(dr);

            dr = orderMethodDt.NewRow();
            dr["ID"] = 1;
            dr["Name"] = "����";
            orderMethodDt.Rows.Add(dr);

            dr = orderMethodDt.NewRow();
            dr["ID"] = 2;
            dr["Name"] = "����";
            orderMethodDt.Rows.Add(dr);

            LookUpEditOrderMethod.DataSource = orderMethodDt;
        }

        private void SetSummaryTypeData()
        {
            var summaryTypeDt = new DataTable();
            summaryTypeDt.Columns.Add(new DataColumn("ID", Type.GetType("System.Int32")));
            summaryTypeDt.Columns.Add(new DataColumn("Name", Type.GetType("System.String")));

            IDictionary<int, string> _dic = new Dictionary<int, string>();
            _dic.Add(0, "");
            _dic.Add(1, "����");
            _dic.Add(2, "��Сֵ");
            _dic.Add(3, "���ֵ");
            _dic.Add(4, "����");
            _dic.Add(5, "ƽ��");

            DataRow dr = null;
            foreach (var key in _dic.Keys)
            {
                dr = summaryTypeDt.NewRow();
                dr["ID"] = key;
                dr["Name"] = _dic[key];
                summaryTypeDt.Rows.Add(dr);
            }

            LookUpEditSummaryType.DataSource = summaryTypeDt;
            lookUpSmlSumType.Properties.DataSource = summaryTypeDt;
        }

        private void SetDisplayFormat()
        {
            var displayFormatDt = new DataTable();
            displayFormatDt.Columns.Add("Name", Type.GetType("System.String"));

            IList<string> _list = new List<string>();
            _list.Add("");
            _list.Add("�ַ�");
            _list.Add("����");
            _list.Add("����");
            _list.Add("����");
            _list.Add("����");
            _list.Add("ʱ��");
            _list.Add("����ʱ��");

            DataRow dr = null;
            foreach (var str in _list)
            {
                dr = displayFormatDt.NewRow();
                dr["Name"] = str;
                displayFormatDt.Rows.Add(dr);
            }

            repositoryItemLookUpDisplayFormat.DataSource = displayFormatDt;
        }


        private void SetApplyTypeData()
        {
            var summaryTypeDt = new DataTable();
            summaryTypeDt.Columns.Add(new DataColumn("ID", Type.GetType("System.Int32")));
            summaryTypeDt.Columns.Add(new DataColumn("Name", Type.GetType("System.String")));

            IDictionary<int, string> _dic = new Dictionary<int, string>();
            _dic.Add(0, "");
            _dic.Add(1, "ȫ��");
            _dic.Add(2, "�б�");
            _dic.Add(3, "����");

            DataRow dr = null;
            foreach (var key in _dic.Keys)
            {
                dr = summaryTypeDt.NewRow();
                dr["ID"] = key;
                dr["Name"] = _dic[key];
                summaryTypeDt.Rows.Add(dr);
            }

            LookUpEditApplyType.DataSource = summaryTypeDt;
        }

        private void SetFKTypeData()
        {
            var summaryTypeDt = new DataTable();
            summaryTypeDt.Columns.Add(new DataColumn("ID", Type.GetType("System.Int32")));
            summaryTypeDt.Columns.Add(new DataColumn("Name", Type.GetType("System.String")));

            IDictionary<int, string> _dic = new Dictionary<int, string>();
            _dic.Add(0, "");
            _dic.Add(1, "����");
            _dic.Add(2, "����");

            _dic.Add(11, "����");
            _dic.Add(12, "����+ʱ��");
            _dic.Add(13, "���");
            _dic.Add(14, "��");
            _dic.Add(15, "��");
            _dic.Add(16, "��");

            DataRow dr = null;
            foreach (var key in _dic.Keys)
            {
                dr = summaryTypeDt.NewRow();
                dr["ID"] = key;
                dr["Name"] = _dic[key];
                summaryTypeDt.Rows.Add(dr);
            }

            repLookUpFKType.DataSource = summaryTypeDt;
        }

        private void SetLngKeyGroupData()
        {
            var summaryTypeDt = new DataTable();
            summaryTypeDt.Columns.Add(new DataColumn("ID", Type.GetType("System.Int32")));
            summaryTypeDt.Columns.Add(new DataColumn("Name", Type.GetType("System.String")));

            IDictionary<int, string> _dic = new Dictionary<int, string>();
            _dic.Add(0, "");
            //_dic.Add(1, "����");
            _dic.Add(2, "����");
            _dic.Add(3, "ƽ��");
            //_dic.Add(4, "���");
            //_dic.Add(5, "��С");

            DataRow dr = null;
            foreach (var key in _dic.Keys)
            {
                dr = summaryTypeDt.NewRow();
                dr["ID"] = key;
                dr["Name"] = _dic[key];
                summaryTypeDt.Rows.Add(dr);
            }

            repGridLookUplLngKeyGroup.DataSource = summaryTypeDt;
        }

        private void SetLngProivtTypeData()
        {
            var summaryTypeDt = new DataTable();
            summaryTypeDt.Columns.Add(new DataColumn("ID", Type.GetType("System.Int32")));
            summaryTypeDt.Columns.Add(new DataColumn("Name", Type.GetType("System.String")));

            IDictionary<int, string> _dic = new Dictionary<int, string>();
            _dic.Add(0, "");
            _dic.Add(1, "���� ");
            _dic.Add(2, "����");
            _dic.Add(3, "������");
            _dic.Add(4, "������չ");
            //_dic.Add(4, "���");
            //_dic.Add(5, "��С");

            DataRow dr = null;
            foreach (var key in _dic.Keys)
            {
                dr = summaryTypeDt.NewRow();
                dr["ID"] = key;
                dr["Name"] = _dic[key];
                summaryTypeDt.Rows.Add(dr);
            }

            repGridLookUpLngProivtType.DataSource = summaryTypeDt;
        }

        private void LoadShcemaList()
        {
            var dt = Model.GetSchemaList(ListID);
            lookUpSchema.Properties.DataSource = dt;
            if ((ListDataID <= 0) && (dt != null) && (dt.Rows.Count > 0)) //Ĭ�ϵ�һ������
                ListDataID = TypeUtil.ToInt(dt.Rows[0]["ListDataID"]);
        }

        /// <summary>
        ///     ������ʾ��ʽ(������ʽ)
        /// </summary>
        private void SetShowStyle()
        {
            if (!BlnListEnter)
            {
                tlbOk.Visibility = BarItemVisibility.Never;
                tlbCancel.Visibility = BarItemVisibility.Never;
            }

            var cn = new StyleFormatCondition(FormatConditionEnum.NotEqual, gridViewCondition.Columns["strCondition"],
                null, string.Empty);
            cn.Appearance.ForeColor = Color.Blue; //�����е�����ɫ
            cn.ApplyToRow = true;
            gridViewCondition.FormatConditions.Add(cn);

            var treelistSFC
                = new DevExpress.XtraTreeList.StyleFormatConditions.StyleFormatCondition(FormatConditionEnum.Greater,
                    treeList1.Columns["lngRelativeID"], null, 0);
            treelistSFC.Appearance.Font = new Font("Tahoma", 9, FontStyle.Bold);
            ; //�����е�����           
            treelistSFC.ApplyToRow = true;
            treeList1.FormatConditions.Add(treelistSFC);


            //����û����������
            var cn1 = new StyleFormatCondition(FormatConditionEnum.NotEqual, gvAdvanceSet.Columns["strParaColName"],
                null, string.Empty);
            cn1.Appearance.ForeColor = Color.Blue; //�����е�����ɫ     
            cn1.ApplyToRow = true;
            gvAdvanceSet.FormatConditions.Add(cn1);


            //��ʱ����
            //if (PermissionManager.HavePermission("QrerySchemaSQL"))
            //{
            //    this.layoutGroupDevelop.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Always;
            //}

            //if (PermissionManager.HavePermission("EditContextCondition"))
            //{
            //    this.layoutControlItem20.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Always;
            //}
            //else
            //{
            //    this.layoutControlItem20.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
            //}
        }

        /// <summary>
        ///     ��ʾ����
        /// </summary>
        private void ShowGroupTitle()
        {
            if (LmdDt != null)
                layoutGroupSelected.Text = "��ѡ��Ŀ(" + gvColSelected.RowCount + ")";
        }


        /// <summary>
        ///     ��ʾ��Ϣ
        /// </summary>
        /// <param name="caption">��Ϣ��</param>
        private void ShowMsg(string caption, bool isMsg)
        {
            MessageShowUtil.ShowStaticInfo(caption, barStaticItemMsg);
            if (isMsg)
                MessageShowUtil.ShowInfo(caption);
        }

        /// <summary>
        ///     ��ȡ������
        /// </summary>
        /// <param name="strKey">������</param>
        /// <returns>int</returns>
        private int GetAliasNum(string strKey)
        {
            var num = 0;
            if (aliasNumDic.ContainsKey(strKey))
                num = aliasNumDic[strKey];
            else
                for (var i = 1; i < 1000; i++)
                    if (!aliasNumDic.Values.Contains(i))
                    {
                        num = i;
                        aliasNumDic.Add(strKey, num);
                        break;
                    }

            return num;
        }

        /// <summary>
        ///     �����ı�
        /// </summary>
        private void lookUpSchema_EditValueChanged(object sender, EventArgs e)
        {
            try
            {
                var id = TypeUtil.ToInt(lookUpSchema.EditValue);
                if (id > 0)
                {
                    //���뷽��

                    ClearSchema();

                    ListDataID = id;

                    LoadSchema(false);
                }
            }
            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        /// <summary>
        ///     ѡ��ʵ�壨�����б����Ч��
        /// </summary>
        private void tlbSelectMaster_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                var frm = new frmMetaDataSelect();
                frm.ShowDialog();
                if (0 == frm.MetadataId)
                    return;

                MetadataId = frm.MetadataId;

                ClearList();

                LoadTreeData(); // �������ؼ�����
            }
            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        /// <summary>
        ///     ����б�����
        /// </summary>
        private void ClearList()
        {
            ListID = 0;
            ListDataID = 0;
            treeDt.Rows.Clear();
            if (curNodeDt != null) curNodeDt.Rows.Clear(); //��ǰ���ڵ��Ӧ����
            LmdDt.Rows.Clear();
            LmdDt.Rows.Clear(); //ѡ����
            aliasNumDic.Clear();
            tableAliasDataDic.Clear();
        }

        /// <summary>
        ///     ��շ�������
        /// </summary>
        private void ClearSchema()
        {
            ListDataID = 0;
            treeDt.Rows.Clear();
            if (curNodeDt != null) curNodeDt.Rows.Clear(); //��ǰ���ڵ��Ӧ����
            if (LmdDt != null) LmdDt.Rows.Clear();
            if (LmdDt != null) LmdDt.Rows.Clear(); //ѡ����
            aliasNumDic.Clear();
            tableAliasDataDic.Clear();
            refCalcColList.Clear(); //���������
        }

        /// <summary>
        ///     �������ؼ�����
        /// </summary>
        private void LoadTreeData()
        {
            try
            {
                //��֯��ǰʵ��    
                var drs = metadataDt.Select("ID=" + MetadataId);
                var dr = treeDt.NewRow();
                dr["strId"] = "_0";
                dr["metaDataId"] = 0;
                dr["parentId"] = 0;
                dr["lngRelativeID"] = MetadataId;
                dr["strName"] = drs[0]["strName"];

                dr["lngParentFieldId"] = 0;
                dr["metaDataFieldId"] = 0;
                dr["lngRelativeFieldID"] = 0;

                dr["strParentFullPath"] = "";
                dr["strFullPath"] = "";
                dr["strNextFullPath"] = "0";

                var strKey = "0";
                var aliasNum = GetAliasNum(strKey);
                var tableName = TypeUtil.ToString(drs[0]["strTableName"]);

                var strTableAlias = tableName.Replace('[', 'B').Replace(']', 'E') + aliasNum;
                dr["strTableAlias"] = strTableAlias;
                dr["strParentTableAlias"] = "";
                dr["lngAliasCount"] = 0;
                dr["lngNextAliasCount"] = aliasNum;

                treeDt.Rows.Add(dr);
            }
            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        /// <summary>
        ///     ��֯���ؼ����ݣ�����ʵ������չ����
        /// </summary>
        /// <param name="parentMetaDataId">���ڵ�Ԫ����ID</param>
        /// <param name="lngParentRelativeID">���ڵ����Ԫ����ID</param>
        /// <param name="lngParentFieldId">���ڵ�Ԫ�����ֶ�ID</param>
        /// <param name="topFullPath">���ڵ��ֶ�ȫ·������</param>
        /// <param name="topNextFullPath">���ڵ��¼���ǰ�ڵ�ȫ·������</param>
        /// <param name="strParentTableAlias">�������</param>
        /// <param name="lngSourceType">�й���:1,�޹���:2</param>
        /// <param name="lngParentAliasCount">���ڵ��¼��ڵ������</param>
        private void AddTreeData(int parentMetaDataId, int lngParentRelativeID, int lngParentFieldId, string topFullPath,
            string topNextFullPath,
            string strParentTableAlias, int lngTopNextAliasCount, DataRow fieldDr)
        {
            try
            {
                var lngSourceType = TypeUtil.ToInt(fieldDr["lngSourceType"]); //������Դ����
                var curMetaDataId = TypeUtil.ToInt(fieldDr["MetaDataId"]); //��ǰԪ����ID
                var metaDataFieldId = TypeUtil.ToInt(fieldDr["ID"]);
                var lngRelativeFieldID = TypeUtil.ToInt(fieldDr["lngRelativeFieldID"]);
                var strFieldName = TypeUtil.ToString(fieldDr["strFieldName"]);
                var strFieldChName = TypeUtil.ToString(fieldDr["strFieldChName"]);
                var strFieldType = TypeUtil.ToString(fieldDr["strFieldType"]); //�ֶ�����
                var strFkCode = TypeUtil.ToString(fieldDr["strFkCode"]); //���ò��ձ���
                var lngRelativeID = TypeUtil.ToInt(fieldDr["lngRelativeID"]); //����Ԫ����ID
                var blnPK = TypeUtil.ToBool(fieldDr["blnPK"]); //�Ƿ�ΪPK�ֶ�
                var lngDataRightType = TypeUtil.ToInt(fieldDr["lngDataRightType"]); //����Ȩ������  
                var strSummaryDisplayFormat = TypeUtil.ToString(fieldDr["strDisplayFormat"]); //��ʾ��ʽ

                var dr = treeDt.NewRow();
                var strTableAlias = "";
                if (lngSourceType > 0)
                {
                    //�й���                   
                    //��֯��ǰʵ��    
                    var drs = metadataDt.Select("ID=" + lngRelativeID);
                    if (drs.Length == 1)
                    {
                        dr["metaDataId"] = curMetaDataId;
                        dr["parentId"] = parentMetaDataId;
                        dr["strName"] = drs[0]["strName"];
                        if (strFieldChName != string.Empty)
                            dr["strName"] = strFieldChName;

                        dr["lngParentFieldId"] = lngParentFieldId;
                        dr["metaDataFieldId"] = metaDataFieldId;
                        dr["lngRelativeFieldID"] = lngRelativeFieldID;

                        var strKey = topNextFullPath + "_" + metaDataFieldId;

                        dr["strParentFullPath"] = topFullPath;
                        dr["strFullPath"] = topNextFullPath;
                        dr["strNextFullPath"] = strKey;
                        var aliasNum = GetAliasNum(strKey);
                        var tableName = TypeUtil.ToString(drs[0]["strTableName"]);

                        strTableAlias = tableName.Replace('[', 'B').Replace(']', 'E') + aliasNum;
                        dr["strTableAlias"] = strTableAlias;
                        dr["strParentTableAlias"] = strParentTableAlias;
                        dr["lngAliasCount"] = lngTopNextAliasCount;
                        dr["lngNextAliasCount"] = aliasNum;

                        dr["strId"] = strKey;
                    }
                    else
                    {
                        throw new Exception("Ԫ�������ô�����鿴�ֶ���[" + strFieldChName + "]�Ĺ�����ϵ��");
                    }
                }
                else
                {
                    //�޹���

                    dr["metaDataId"] = curMetaDataId;
                    dr["parentId"] = parentMetaDataId;
                    dr["strName"] = strFieldChName;

                    dr["lngParentFieldId"] = lngParentFieldId;
                    dr["metaDataFieldId"] = metaDataFieldId;
                    dr["lngRelativeFieldID"] = lngRelativeFieldID;

                    dr["strParentFullPath"] = topFullPath;
                    dr["strFullPath"] = topNextFullPath;
                    dr["strNextFullPath"] = "";

                    strTableAlias = strParentTableAlias + metaDataFieldId;
                    dr["strTableAlias"] = "";
                    dr["strParentTableAlias"] = strParentTableAlias;
                    dr["lngAliasCount"] = lngTopNextAliasCount;

                    dr["strId"] = topNextFullPath + "_" + metaDataFieldId;
                }

                dr["strFieldName"] = strFieldName;
                dr["strFieldChName"] = strFieldChName;
                dr["strFieldType"] = strFieldType;
                dr["strFkCode"] = strFkCode;
                dr["lngSourceType"] = lngSourceType;
                dr["lngRelativeID"] = lngRelativeID;
                dr["blnPK"] = blnPK;
                dr["lngDataRightType"] = lngDataRightType;
                dr["strSummaryDisplayFormat"] = strSummaryDisplayFormat;

                treeDt.Rows.Add(dr);
            }
            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        private void treeList1_Click(object sender, EventArgs e)
        {
            try
            {
                var curNode = treeList1.FocusedNode;
                var treeListField = new TreeListField();
                treeListField.metaDataId = TypeUtil.ToInt(curNode.GetValue("metaDataId"));
                treeListField.parentId = TypeUtil.ToInt(curNode.GetValue("parentId"));
                treeListField.lngRelativeID = TypeUtil.ToInt(curNode.GetValue("lngRelativeID"));
                treeListField.metaDataFieldId = TypeUtil.ToInt(curNode.GetValue("metaDataFieldId"));
                treeListField.strName = TypeUtil.ToString(curNode.GetValue("strName"));
                treeListField.lngAliasCount = TypeUtil.ToInt(curNode.GetValue("lngAliasCount"));
                treeListField.lngNextAliasCount = TypeUtil.ToInt(curNode.GetValue("lngNextAliasCount"));
                treeListField.strTableAlias = TypeUtil.ToString(curNode.GetValue("strTableAlias"));
                treeListField.strFullPath = TypeUtil.ToString(curNode.GetValue("strFullPath"));
                treeListField.strParentFullPath = TypeUtil.ToString(curNode.GetValue("strParentFullPath"));
                treeListField.strNextFullPath = TypeUtil.ToString(curNode.GetValue("strNextFullPath"));
                if ((treeListField.strTableAlias == "") || (treeListField.lngRelativeID == 0))
                    return;
                //չ����һ��
                if (!tableAliasDataDic.ContainsKey(treeListField.strTableAlias))
                {
                    //δչ��

                    var fieldDrs = metadataFieldDt.Select("blnHidden<>1 and MetaDataID=" + treeListField.lngRelativeID);
                    foreach (var fieldDr in fieldDrs)
                    {
                        //���ֶ�ID��ȡ��Դ�ֶ�ID��Ϊȡ���ڵ��ֶ�ID��
                        var lngParentFieldId = 0;
                        if (curNode.ParentNode != null)
                            lngParentFieldId = TypeUtil.ToInt(curNode.ParentNode.GetValue("metaDataFieldId"));

                        AddTreeData(treeListField.metaDataId, treeListField.lngRelativeID, treeListField.metaDataFieldId,
                            treeListField.strFullPath, treeListField.strNextFullPath,
                            treeListField.strTableAlias, treeListField.lngNextAliasCount, fieldDr);
                    }
                }

                //��ʾ�����浱ǰ�ڵ���Ŀ����
                DataTable dt;
                if (tableAliasDataDic.ContainsKey(treeListField.strTableAlias))
                {
                    dt = tableAliasDataDic[treeListField.strTableAlias];
                }
                else
                {
                    var strFilter = "MetaDataID=" + treeListField.lngRelativeID;

                    //���˵����ֶ�Ȩ�޵�����blnFieldRight
                    //To

                    // 20170605
                    IDictionary<string, string> colNameAliasDic = new Dictionary<string, string>();
                    colNameAliasDic.Add("ID", "MetaDataFieldID"); //�ֶ�ID
                    colNameAliasDic.Add("MetaDataID", "MetaDataID"); //Ԫ����ID
                    //colNameAliasDic.Add("strFieldName", "strFieldName"); //�ֶ���
                    //colNameAliasDic.Add("strFieldType", "strFieldType"); //�ֶ�����
                    //colNameAliasDic.Add("strFkCode", "strFkCode"); //���ò��ձ���
                    //colNameAliasDic.Add("strFieldChName", "strFieldChName"); //�ֶ�������
                    //colNameAliasDic.Add("lngSourceType", "lngSourceType"); //�ֶι���������Դ����
                    //colNameAliasDic.Add("lngRelativeID", "lngRelativeID"); //����Ԫ����ID
                    //colNameAliasDic.Add("lngRelativeFieldID", "lngRelativeFieldID"); //����Ԫ�����ֶ�ID
                    //colNameAliasDic.Add("blnPK", "blnPK"); //�Ƿ�ΪPK�ֶ�
                    //colNameAliasDic.Add("lngDataRightType", "lngDataRightType"); //����Ȩ������         
                    colNameAliasDic.Add("StrFieldName", "strFieldName"); //�ֶ���
                    colNameAliasDic.Add("StrFieldType", "strFieldType"); //�ֶ�����
                    colNameAliasDic.Add("StrFkCode", "strFkCode"); //���ò��ձ���
                    colNameAliasDic.Add("StrFieldChName", "strFieldChName"); //�ֶ�������
                    colNameAliasDic.Add("LngSourceType", "lngSourceType"); //�ֶι���������Դ����
                    colNameAliasDic.Add("LngRelativeID", "lngRelativeID"); //����Ԫ����ID
                    colNameAliasDic.Add("LngRelativeFieldID", "lngRelativeFieldID"); //����Ԫ�����ֶ�ID
                    colNameAliasDic.Add("BlnPK", "blnPK"); //�Ƿ�ΪPK�ֶ�
                    colNameAliasDic.Add("LngDataRightType", "lngDataRightType"); //����Ȩ������              

                    dt = GetPartialData(metadataFieldDt, strFilter, colNameAliasDic);

                    var dc = new DataColumn("blnSelect", Type.GetType("System.Boolean"));
                    dc.DefaultValue = false;
                    dt.Columns.Add(dc);

                    dc = new DataColumn("lngParentID", Type.GetType("System.Int32")); //��Ԫ����ID
                    dc.DefaultValue = treeListField.metaDataId;
                    dt.Columns.Add(dc);

                    dc = new DataColumn("lngParentFieldID", Type.GetType("System.Int32")); //��Ԫ�����ֶ�ID
                    dc.DefaultValue = treeListField.metaDataFieldId;
                    dt.Columns.Add(dc);

                    dc = new DataColumn("lngAliasCount", Type.GetType("System.Int32")); //�����
                    dc.DefaultValue = treeListField.lngNextAliasCount;
                    dt.Columns.Add(dc);

                    dc = new DataColumn("strTableAlias", Type.GetType("System.String")); //�����
                    dc.DefaultValue = treeListField.strTableAlias;
                    dt.Columns.Add(dc);

                    dc = new DataColumn("strFullPath", Type.GetType("System.String"));
                    dc.DefaultValue = treeListField.strNextFullPath;
                    dt.Columns.Add(dc);

                    dc = new DataColumn("strParentFullPath", Type.GetType("System.String"));
                    dc.DefaultValue = treeListField.strFullPath;
                    dt.Columns.Add(dc);

                    tableAliasDataDic.Add(treeListField.strTableAlias, dt);

                    //�ָ���ѡ��¼
                    strFilter = "strFullPath='" + treeListField.strNextFullPath + "'";
                    strFilter += " and isnull(blnSysProcess,0)=0";
                    var selectedDr = LmdDt.Select(strFilter);
                    DataRow dr;
                    for (var i = 0; i < selectedDr.Length; i++)
                    {
                        dr = selectedDr[i];
                        if (!curNode.HasChildren)
                            break;
                        TreeListNode childNode = null;
                        for (var j = 0; j < curNode.Nodes.Count; j++)
                        {
                            childNode = curNode.Nodes[j];
                            if (TypeUtil.ToInt(dr["MetaDataFieldID"]) ==
                                TypeUtil.ToInt(childNode.GetValue("metaDataFieldId")))
                                childNode.CheckState = CheckState.Checked;
                        }
                    }

                    //����PK�ֶ�
                    //dt.DefaultView.RowFilter = "isnull(blnPK,0)<>1 or strFieldName='ReceiptTypeID' or strFieldName='OrderTypeID'";
                }

                curNodeDt = dt; //��¼��ǰ�ڵ��¼����

                //this.gcCurTable.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        /// <summary>
        ///     ��ȡ���ݣ����ݹ���������
        /// </summary>
        /// <param name="dt">����˱�</param>
        /// <param name="strFilter">��������</param>
        /// <param name="colNameAliasDic">�����������</param>
        /// <returns>DataTable</returns>
        private DataTable GetPartialData(DataTable dt, string strFilter, IDictionary<string, string> colNameAliasDic)
        {
            DataTable returnDt = null;
            try
            {
                dt.DefaultView.RowFilter = strFilter;
                returnDt = dt.DefaultView.ToTable();

                var colName = "";
                var dcc = returnDt.Columns;
                for (var i = 0; i < dcc.Count; i++)
                {
                    colName = dcc[i].ColumnName;
                    if (!colNameAliasDic.ContainsKey(colName))
                    {
                        i--;
                        dcc.Remove(colName);
                    }
                    else
                    {
                        dcc[i].ColumnName = colNameAliasDic[colName];
                    }
                }
            }
            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }

            return returnDt;
        }

        private void treeList1_BeforeCheckNode(object sender, CheckNodeEventArgs e)
        {
            try
            {
                if (e.Node.Level <= 0)
                {
                    ShowMsg("���ڵ㲻��ѡ��", true);
                    e.CanCheck = false;
                    return;
                }

                e.State = e.PrevState == CheckState.Checked ? CheckState.Unchecked : CheckState.Checked;
            }
            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        private void treeList1_AfterCheckNode(object sender, NodeEventArgs e)
        {
            try
            {
                if (e.Node.Level <= 0)
                    return;

                //SetCheckedChildNodes(e.Node, e.Node.CheckState);
                //SetCheckedParentNodes(e.Node, e.Node.CheckState);

                var isSelect = e.Node.CheckState == CheckState.Checked;
                if (!isSelect)
                    if (e.Node.ParentNode != null)
                    {
                        var str = TypeUtil.ToString(e.Node.ParentNode.GetValue("strTableAlias")) + "_" +
                                  TypeUtil.ToString(e.Node.GetValue("strFieldName"));
                        if (refCalcColList.Contains(str)) //���������ã�����ȡ��ѡ��
                        {
                            e.Node.CheckState = CheckState.Checked;
                            ShowMsg("��ǰ�ֶα����ּ�����ʹ�ã�����ȡ��ѡ��", true);
                            return;
                        }

                        if (refContextContextColList.Contains(str)) //�������������ã�����ȡ��ѡ��
                        {
                            e.Node.CheckState = CheckState.Checked;
                            ShowMsg("��ǰ�ֶα�����������ʹ�ã�����ȡ��ѡ��", true);
                            return;
                        }
                    }

                SynchronousSelectCol(e.Node, isSelect);

                if (isSelect)
                {
                    if ((e.Node.ParentNode != null) && (e.Node.ParentNode.Level > 0))
                        AddRelativeCol(e.Node.ParentNode);
                }
                else
                {
                    if ((e.Node.ParentNode != null) && (e.Node.ParentNode.Level > 0))
                        CancelRelativeCol(e.Node.ParentNode);
                }
            }
            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }


        private void SetCheckedChildNodes(TreeListNode node, CheckState check)
        {
            for (var i = 0; i < node.Nodes.Count; i++)
            {
                node.Nodes[i].CheckState = check;
                SetCheckedChildNodes(node.Nodes[i], check);
            }
        }

        private void SetCheckedParentNodes(TreeListNode node, CheckState check)
        {
            if (node.ParentNode != null)
            {
                var b = false;
                CheckState state;
                for (var i = 0; i < node.ParentNode.Nodes.Count; i++)
                {
                    state = node.ParentNode.Nodes[i].CheckState;
                    if (!check.Equals(state))
                    {
                        b = !b;
                        break;
                    }
                }
                node.ParentNode.CheckState = b ? CheckState.Indeterminate : check;
                SetCheckedParentNodes(node.ParentNode, check);
            }
        }


        private void chkSmlSum_CheckedChanged(object sender, EventArgs e)
        {
            var isVisble = chkSmlSum.Checked;
            gcblnSummary.Visible = isVisble;
            if (isVisble)
                gcblnSummary.VisibleIndex = 6;
        }

        private void checkEditSelect_CheckedChanged(object sender, EventArgs e)
        {
            gvAdvanceSet.CloseEditor();
            gvAdvanceSet.UpdateCurrentRow();
        }

        private void CheckEditFreCondition_CheckedChanged(object sender, EventArgs e)
        {
            gvColSelected.CloseEditor();
            gvColSelected.UpdateCurrentRow();
        }

        /// <summary>
        ///     �߼�����
        /// </summary>
        private void gvAdvanceSet_CellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            try
            {
                gvAdvanceSet.CellValueChanged -= gvAdvanceSet_CellValueChanged;

                if ("lngOrderMethod" == e.Column.FieldName)
                {
                    //ѡ���з����ı�

                    var isSelect = TypeUtil.ToBool(gvAdvanceSet.GetRowCellValue(e.RowHandle, "lngOrderMethod"));
                    if (isSelect)
                    {
                        if (TypeUtil.ToInt(gvAdvanceSet.GetRowCellValue(e.RowHandle, "lngOrder")) <= 0)
                        {
                            var selectCount = GetSelectFieldRecordCount(gvAdvanceSet, "lngOrderMethod"); //ѡ���¼��
                            gvAdvanceSet.SetRowCellValue(e.RowHandle, "lngOrder", selectCount);
                        }
                    }
                    else
                    {
                        ReSetSelectFieldIndex(gvAdvanceSet, "lngOrderMethod", "lngOrder",
                            TypeUtil.ToInt(gvAdvanceSet.GetRowCellValue(e.RowHandle, "lngOrder")));
                        gvAdvanceSet.SetRowCellValue(e.RowHandle, "lngOrder", 0);
                    }
                }
                else if ("blnSummary" == e.Column.FieldName)
                {
                    var blnSummary = TypeUtil.ToBool(gvAdvanceSet.GetRowCellValue(e.RowHandle, "blnSummary"));
                    if (blnSummary)
                    {
                        var strFieldType = TypeUtil.ToString(gvAdvanceSet.GetRowCellValue(e.RowHandle, "strFieldType"));
                        if ((strFieldType != "money") && (strFieldType != "decimal") && (strFieldType != "float")
                            && (strFieldType != "int") && (strFieldType != "smallint") && (strFieldType != "bigint") &&
                            (strFieldType != "tinyint"))
                        {
                            gvAdvanceSet.SetRowCellValue(e.RowHandle, "blnSummary", false);
                            ShowMsg("��ֵ����Ŀ����С�ƣ�", true);
                        }
                    }
                }

                gvAdvanceSet.CellValueChanged += gvAdvanceSet_CellValueChanged;
            }
            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        private void gvAdvanceSet_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
        {
            //int rowHandle = e.FocusedRowHandle;
            //string strFieldType = TypeUtil.ToString(this.gvAdvanceSet.GetRowCellValue(rowHandle, "strFieldType"));
            //if (strFieldType != "money" && strFieldType != "decimal" && strFieldType != "float"
            //                && strFieldType != "int" && strFieldType != "smallint" && strFieldType != "bigint")
            //{
            //    this.LookUpEditSummaryType.View.ActiveFilterString = "Name<>'����' and Name<>'ƽ��'";
            //}
            //else
            //{
            //    this.LookUpEditSummaryType.View.ActiveFilterString = "";
            //}
        }

        private void gvColSelected_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
        {
            try
            {
                var rowHandle = e.FocusedRowHandle;
                var MetaDataFieldID = TypeUtil.ToInt(gvColSelected.GetRowCellValue(rowHandle, "MetaDataFieldID"));
                var rows = metadataFieldDt.Select("ID=" + MetaDataFieldID + " and len(strFkCode)>0");
                var rowsa = metadataFieldDt.Select("ID=" + MetaDataFieldID + " and strFieldType like  '%datetime%'");


                if ((rows != null) && (rows.Length > 0))
                    return;
                if ((rowsa != null) && (rowsa.Length > 0))
                    return;

                gcFKType.OptionsColumn.ReadOnly = false;
            }
            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        /// <summary>
        ///     ��ѡ��Ŀ
        /// </summary>
        private void gvColSelected_CellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            try
            {
                gvColSelected.CellValueChanged -= gvColSelected_CellValueChanged;
                if ("blnFreCondition" == e.Column.FieldName)
                {
                    //�Ƿ�Ϊ��������

                    var isFreCondition = TypeUtil.ToBool(gvColSelected.GetRowCellValue(e.RowHandle, "blnFreCondition"));
                    if (isFreCondition)
                    {
                        if (TypeUtil.ToBool(gvColSelected.GetRowCellValue(e.RowHandle, "isCalcField")))
                        {
                            //�����в�������Ϊ��������

                            gvColSelected.SetRowCellValue(e.RowHandle, "blnFreCondition", false);
                            ShowMsg("�����в�������Ϊ����������", true);
                        }
                        else
                        {
                            var selectCount = GetSelectFieldRecordCount(gvColSelected, "blnFreCondition"); //ѡ���¼��
                            if (selectCount > 30)
                            {
                                gvColSelected.SetRowCellValue(e.RowHandle, "blnFreCondition", false);
                                ShowMsg("�����������ܶ���30����", true);
                            }
                            else
                            {
                                gvColSelected.SetRowCellValue(e.RowHandle, "lngFreConIndex", selectCount);
                            }
                        }
                    }
                    else
                    {
                        ReSetSelectFieldIndex(gvColSelected, "blnFreCondition", "lngFreConIndex",
                            TypeUtil.ToInt(gvColSelected.GetRowCellValue(e.RowHandle, "lngFreConIndex")));
                        gvColSelected.SetRowCellValue(e.RowHandle, "lngFreConIndex", 0);
                    }
                }

                gvColSelected.CellValueChanged += gvColSelected_CellValueChanged;
            }
            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        private int GetSelectFieldRecordCount(GridView gv, string selectFieldName)
        {
            var selectCount = 0; //ѡ���¼��
            var rowCount = gv.RowCount;
            for (var i = 0; i < rowCount; i++)
                if (TypeUtil.ToBool(gv.GetRowCellValue(i, selectFieldName)))
                    selectCount++;

            return selectCount;
        }

        private void ReSetSelectFieldIndex(GridView gv, string selectFieldName, string indexFieldName, int curIndex)
        {
            var rowCount = gv.RowCount;
            var index = 0;
            for (var i = 0; i < rowCount; i++)
            {
                index = TypeUtil.ToInt(gv.GetRowCellValue(i, indexFieldName));
                if (TypeUtil.ToBool(gv.GetRowCellValue(i, selectFieldName)) && (index > curIndex))
                    gv.SetRowCellValue(i, indexFieldName, index - 1);
            }
        }

        /// <summary>
        ///     ��ӹ�����
        /// </summary>
        /// <param name="curNode">��ǰ���ڵ�</param>
        private void AddRelativeCol(TreeListNode curNode)
        {
            try
            {
                var treeListField = new TreeListField();
                treeListField.metaDataId = TypeUtil.ToInt(curNode.GetValue("metaDataId"));
                treeListField.parentId = TypeUtil.ToInt(curNode.GetValue("parentId"));
                treeListField.metaDataFieldId = TypeUtil.ToInt(curNode.GetValue("metaDataFieldId"));
                treeListField.lngParentFieldId = TypeUtil.ToInt(curNode.GetValue("lngParentFieldId"));
                treeListField.lngRelativeFieldID = TypeUtil.ToInt(curNode.GetValue("lngRelativeFieldID"));
                treeListField.strName = TypeUtil.ToString(curNode.GetValue("strName"));
                treeListField.lngAliasCount = TypeUtil.ToInt(curNode.GetValue("lngAliasCount"));
                treeListField.strTableAlias = TypeUtil.ToString(curNode.GetValue("strTableAlias"));
                treeListField.strFullPath = TypeUtil.ToString(curNode.GetValue("strFullPath"));
                treeListField.strParentFullPath = TypeUtil.ToString(curNode.GetValue("strParentFullPath"));

                treeListField.strTableAlias = TypeUtil.ToString(curNode.GetValue("strTableAlias")); //����� 
                treeListField.strFieldName = TypeUtil.ToString(curNode.GetValue("strFieldName"));
                treeListField.strFieldChName = TypeUtil.ToString(curNode.GetValue("strFieldChName"));
                treeListField.strFieldType = TypeUtil.ToString(curNode.GetValue("strFieldType"));
                treeListField.strFkCode = TypeUtil.ToString(curNode.GetValue("strFkCode"));
                treeListField.lngSourceType = TypeUtil.ToInt(curNode.GetValue("lngSourceType"));
                treeListField.lngRelativeID = TypeUtil.ToInt(curNode.GetValue("lngRelativeID"));
                treeListField.blnPK = TypeUtil.ToBool(curNode.GetValue("blnPK"));
                treeListField.lngDataRightType = TypeUtil.ToInt(curNode.GetValue("lngDataRightType"));
                treeListField.strSummaryDisplayFormat = TypeUtil.ToString(curNode.GetValue("strSummaryDisplayFormat"));

                if (curNode.ParentNode != null)
                    treeListField.strTableAlias = TypeUtil.ToString(curNode.ParentNode.GetValue("strTableAlias"));

                var strFilter = "strFullPath='" + treeListField.strFullPath + "'";
                strFilter += " and isnull(MetaDataFieldID,0)=" + treeListField.metaDataFieldId;
                if (LmdDt.Select(strFilter).Length <= 0)
                {
                    //��������                

                    //��ӹ������ֶ�
                    var dr = LmdDt.NewRow();
                    dr["ListDataID"] = ListDataID;
                    dr["MetaDataFieldID"] = treeListField.metaDataFieldId;
                    dr["MetaDataID"] = treeListField.metaDataId; //Ԫ����ID
                    dr["strFieldType"] = treeListField.strFieldType; //�ֶ�����
                    dr["strFkCode"] = treeListField.strFkCode; //  ���ò��ձ���
                    dr["strListDisplayFieldNameCHS"] = treeListField.strFieldChName; //�ֶ�������
                    dr["lngSourceType"] = treeListField.lngSourceType; //�ֶι���������Դ����
                    dr["lngRelativeFieldID"] = treeListField.lngRelativeFieldID; //����Ԫ�����ֶ�ID

                    dr["lngParentID"] = treeListField.parentId; //��Ԫ����ID
                    dr["lngParentFieldID"] = treeListField.lngParentFieldId; //��Ԫ�����ֶ�ID
                    dr["lngAliasCount"] = treeListField.lngAliasCount; //�����


                    dr["MetaDataFieldName"] = treeListField.strTableAlias + "_" + treeListField.strFieldName;
                    dr["strTableAlias"] = treeListField.strTableAlias; //����� 
                    dr["strFullPath"] = treeListField.strFullPath;
                    dr["strParentFullPath"] = treeListField.strParentFullPath;
                    dr["strSrcFieldNameCHS"] = GetMetaDataFieldNameCHS(treeListField.metaDataFieldId) + "."
                                               + TypeUtil.ToString(dr["strListDisplayFieldNameCHS"]);

                    dr["strSummaryDisplayFormat"] = treeListField.strSummaryDisplayFormat;

                    dr["blnSysProcess"] = true;
                    dr["lngKeyFieldType"] = 1;
                    dr["blnShow"] = false;

                    LmdDt.Rows.Add(dr);

                    //�����ڵ������Ŀ
                    if ((curNode.ParentNode != null) && (curNode.ParentNode.Level > 0))
                        AddRelativeCol(curNode.ParentNode);
                }
            }
            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        /// <summary>
        ///     ɾ��������
        /// </summary>
        /// <param name="curNode">��ǰ���ڵ�</param>
        private void CancelRelativeCol(TreeListNode curNode)
        {
            try
            {
                var strFullPath = TypeUtil.ToString(curNode.GetValue("strFullPath"));
                var strNextFullPath = TypeUtil.ToString(curNode.GetValue("strNextFullPath"));
                var metaDataFieldId = TypeUtil.ToInt(curNode.GetValue("metaDataFieldId"));

                //�жϵ�ǰ�ڵ��Ƿ��й�����Ŀ��¼
                var strFilter = "blnSysProcess=0 and strFullPath like '" + strNextFullPath + "%'";
                var relatoinDrs = LmdDt.Select(strFilter);

                if (relatoinDrs.Length <= 0)
                {
                    //��ǰ�ڵ�������ѡ����Ŀ ���޹���ѡ����Ŀ ����ȡ����Ŀ�ڵ�

                    strFilter = "strFullPath='" + strFullPath + "'";
                    strFilter += " and isnull(MetaDataFieldID,0)=" + metaDataFieldId;
                    relatoinDrs = LmdDt.Select(strFilter);
                    if (relatoinDrs.Length > 0)
                        if (TypeUtil.ToBool(relatoinDrs[0]["blnSysProcess"])) //���������Ŀ�ֶ�
                            LmdDt.Rows.Remove(relatoinDrs[0]);
                }
            }
            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        /// <summary>
        ///     ͬ��ѡ����Ŀ����
        /// </summary>
        /// <param name="curNode">��ǰ�ڵ�</param>
        /// <param name="isSelect">�Ƿ�ѡ��</param>
        private void SynchronousSelectCol(TreeListNode curNode, bool isSelect)
        {
            try
            {
                var treeListField = new TreeListField();
                treeListField.metaDataId = TypeUtil.ToInt(curNode.GetValue("metaDataId"));
                treeListField.parentId = TypeUtil.ToInt(curNode.GetValue("parentId"));
                treeListField.metaDataFieldId = TypeUtil.ToInt(curNode.GetValue("metaDataFieldId"));
                treeListField.lngParentFieldId = TypeUtil.ToInt(curNode.GetValue("lngParentFieldId"));
                treeListField.lngRelativeFieldID = TypeUtil.ToInt(curNode.GetValue("lngRelativeFieldID"));
                treeListField.strName = TypeUtil.ToString(curNode.GetValue("strName"));
                treeListField.lngAliasCount = TypeUtil.ToInt(curNode.GetValue("lngAliasCount"));
                treeListField.strTableAlias = TypeUtil.ToString(curNode.GetValue("strTableAlias"));
                treeListField.strFullPath = TypeUtil.ToString(curNode.GetValue("strFullPath"));
                treeListField.strParentFullPath = TypeUtil.ToString(curNode.GetValue("strParentFullPath"));

                treeListField.strTableAlias = TypeUtil.ToString(curNode.GetValue("strTableAlias")); //����� 
                treeListField.strFieldName = TypeUtil.ToString(curNode.GetValue("strFieldName"));
                treeListField.strFieldChName = TypeUtil.ToString(curNode.GetValue("strFieldChName"));
                treeListField.strFieldType = TypeUtil.ToString(curNode.GetValue("strFieldType"));
                treeListField.strFkCode = TypeUtil.ToString(curNode.GetValue("strFkCode"));
                treeListField.lngSourceType = TypeUtil.ToInt(curNode.GetValue("lngSourceType"));
                treeListField.lngRelativeID = TypeUtil.ToInt(curNode.GetValue("lngRelativeID"));
                treeListField.blnPK = TypeUtil.ToBool(curNode.GetValue("blnPK"));
                treeListField.lngDataRightType = TypeUtil.ToInt(curNode.GetValue("lngDataRightType"));
                treeListField.strSummaryDisplayFormat = TypeUtil.ToString(curNode.GetValue("strSummaryDisplayFormat"));

                if (curNode.ParentNode != null)
                    treeListField.strTableAlias = TypeUtil.ToString(curNode.ParentNode.GetValue("strTableAlias"));
                var strFilter = "strFullPath='" + treeListField.strFullPath + "'";
                strFilter += " and isnull(MetaDataFieldID,0)=" + treeListField.metaDataFieldId;
                var drs = LmdDt.Select(strFilter);

                if (isSelect)
                {
                    //��ӵ�ǰ��
                    if ((drs != null) && (drs.Length > 0))
                    {
                        drs[0]["blnSysProcess"] = false;
                        drs[0]["blnShow"] = true;
                        drs[0]["strFkCode"] = treeListField.strFkCode;
                        drs[0]["lngSourceType"] = treeListField.lngSourceType;

                        var copyDr = LmdDt.NewRow();
                        foreach (DataColumn dc in LmdDt.Columns)
                            copyDr[dc.ColumnName] = drs[0][dc.ColumnName];
                        LmdDt.Rows.Remove(drs[0]);
                        LmdDt.Rows.Add(copyDr);

                        ShowGroupTitle();
                        return;
                    }

                    //��ӹ������ֶ�
                    var dr = LmdDt.NewRow();
                    dr["ListDataID"] = ListDataID;
                    dr["MetaDataFieldID"] = treeListField.metaDataFieldId;
                    dr["MetaDataID"] = treeListField.metaDataId; //Ԫ����ID
                    dr["strFieldType"] = treeListField.strFieldType; //�ֶ�����
                    dr["strFkCode"] = treeListField.strFkCode; //  ���ò��ձ���
                    dr["strListDisplayFieldNameCHS"] = treeListField.strFieldChName; //�ֶ�������
                    dr["lngSourceType"] = treeListField.lngSourceType; //�ֶι���������Դ����     
                    dr["lngParentID"] = treeListField.parentId; //��Ԫ����ID
                    dr["lngRelativeFieldID"] = treeListField.lngRelativeFieldID; //����Ԫ�����ֶ�ID
                    dr["lngParentFieldID"] = treeListField.lngParentFieldId; //��Ԫ�����ֶ�ID
                    dr["lngAliasCount"] = treeListField.lngAliasCount; //�����
                    dr["MetaDataFieldName"] = treeListField.strTableAlias + "_" + treeListField.strFieldName;
                    dr["strTableAlias"] = treeListField.strTableAlias; //����� 
                    dr["strFullPath"] = treeListField.strFullPath;
                    dr["strParentFullPath"] = treeListField.strParentFullPath;
                    dr["strSrcFieldNameCHS"] = GetMetaDataFieldNameCHS(treeListField.lngParentFieldId) + "."
                                               + treeListField.strFieldChName;

                    dr["strSummaryDisplayFormat"] = treeListField.strSummaryDisplayFormat;

                    dr["blnSysProcess"] = false;
                    dr["blnShow"] = true;

                    dr["strCondition"] = string.Empty;
                    dr["strConditionCHS"] = string.Empty;
                    dr["strParaColName"] = string.Empty;
                    LmdDt.Rows.Add(dr);
                }
                else
                {
                    //ɾ����ǰ��

                    if ((drs != null) && (drs.Length > 0))
                        if (TypeUtil.ToInt(drs[0]["lngKeyFieldType"]) <= 0) //����ǹؼ��ֶΣ�����ɾ��
                        {
                            LmdDt.Rows.Remove(drs[0]);
                        }
                        else
                        {
                            drs[0]["blnSysProcess"] = true;
                            drs[0]["blnShow"] = false;
                            drs[0]["blnFreCondition"] = false;
                            drs[0]["lngSourceType"] = treeListField.lngSourceType;
                        }
                }

                ShowGroupTitle();
            }
            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        private string GetMetaDataFieldNameCHS(int fieldId)
        {
            var strFieldNameChs = "";
            if (fieldId == 0)
            {
                strFieldNameChs = TypeUtil.ToString(treeList1.Nodes.FirstNode.GetValue("strName"));
            }
            else
            {
                var drs = metadataFieldDt.Select("ID=" + fieldId);
                if ((drs != null) && (drs.Length > 0))
                {
                    strFieldNameChs = TypeUtil.ToString(drs[0]["strFieldChName"]);

                    var lmdDrs = LmdDt.Select("MetaDataFieldID=" + fieldId);
                    if ((lmdDrs != null) && (lmdDrs.Length > 0))
                    {
                        var lngParentFieldID = TypeUtil.ToInt(lmdDrs[0]["lngParentFieldID"]);
                        strFieldNameChs = GetMetaDataFieldNameCHS(lngParentFieldID) + "." + strFieldNameChs;
                    }
                }
            }

            return strFieldNameChs;
        }

        /// <summary>
        ///     ����Ԫ���ݻ���
        /// </summary>
        private void btnUpdateMetaDataCache_Click(object sender, EventArgs e)
        {
            try
            {
                ClientCacheModel.UpdateMetaDataCache();
                GetMetaDataDesc();
                MessageShowUtil.ShowStaticInfo("���³ɹ���", barStaticItemMsg);
            }
            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        /// <summary>
        ///     ����SQL
        /// </summary>
        private void btnBuildSql_Click(object sender, EventArgs e)
        {
            memoEditSQL.Text = "";

            try
            {
                var dt = GetListMetaDataTable();
                var strListSQL = Model.GetSQL(dt);
                if ((TypeUtil.ToString(ListDataExVo.StrConTextCondition) != string.Empty) &&
                    (ListDataExVo.StrConTextCondition != null))
                {
                    var strs = ListDataExVo.StrConTextCondition.Split(new[] {"&&&$$$"},
                        StringSplitOptions.RemoveEmptyEntries);
                    if (strs.Length == 2)
                        strListSQL = strListSQL.Replace("where 1=1",
                            "where 1=1 " + " and " + strs[1].Trim().Replace('_', '.'));
                }

                strListSQL = RequestUtil.ProcessDynamicParameters(strListSQL);
                memoEditSQL.Text = strListSQL;

                MessageShowUtil.ShowStaticInfo("���ɳɹ���", barStaticItemMsg);
            }
            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        /// <summary>
        ///     У��SQL
        /// </summary>
        private void btnTestSql_Click(object sender, EventArgs e)
        {
            try
            {
                if (memoEditSQL.Text == string.Empty)
                {
                    MessageShowUtil.ShowInfo("У��SQL����Ϊ�գ�");
                    return;
                }

                //Ϊ���Ч�ʣ�Ԥ������ֻ��ʾ100
                var strsql = memoEditSQL.Text;


                if (Model.GetDbType() == "mysql")
                    strsql = strsql + " LIMIT 0,10";
                if (Model.GetDbType() == "sqlserver")
                    strsql = "select * from (" + strsql.Insert(6, " top 100 ") + ") as A";
                var dt = Model.TestSql(strsql);
                gridControlPreView.DataSource = null;
                gridViewPreView.Columns.Clear();
                gridControlPreView.DataSource = dt;
                gridViewPreView.BestFitColumns();


                MessageShowUtil.ShowStaticInfo("У��ɹ������ؼ�¼��Ϊ" + dt.Rows.Count, barStaticItemMsg);
            }
            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        /// <summary>
        ///     �Ƶ�����
        /// </summary>
        private void btnMoveFirst_Click(object sender, EventArgs e)
        {
            try
            {
                if (gvColSelected.IsFirstRow)
                {
                    MessageShowUtil.ShowInfo("��ǰ���������У�");
                    return;
                }

                var focusedRowHandle = gvColSelected.FocusedRowHandle;
                var focuseDr = gvColSelected.GetFocusedDataRow();
                var copyDr = LmdDt.NewRow();
                foreach (DataColumn dc in LmdDt.Columns)
                    copyDr[dc.ColumnName] = focuseDr[dc.ColumnName];
                LmdDt.Rows.Remove(focuseDr);
                LmdDt.Rows.InsertAt(copyDr, 0);
                gvColSelected.FocusedRowHandle = 0;
            }
            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        /// <summary>
        ///     ����
        /// </summary>
        private void btnMovePrev_Click(object sender, EventArgs e)
        {
            try
            {
                if (gvColSelected.IsFirstRow)
                {
                    MessageShowUtil.ShowInfo("��ǰ���������У�");
                    return;
                }

                var focusedRowHandle = gvColSelected.FocusedRowHandle;
                var focuseDr = gvColSelected.GetFocusedDataRow();
                var copyDr = LmdDt.NewRow();
                foreach (DataColumn dc in LmdDt.Columns)
                    copyDr[dc.ColumnName] = focuseDr[dc.ColumnName];

                var curRowIndex = LmdDt.Rows.IndexOf(focuseDr); //���������м�¼             
                for (var insertRow = curRowIndex - 1; insertRow >= 0; insertRow--)
                    if (!TypeUtil.ToBool(LmdDt.Rows[insertRow]["blnSysProcess"]))
                    {
                        LmdDt.Rows.Remove(focuseDr);
                        LmdDt.Rows.InsertAt(copyDr, insertRow);
                        break;
                    }
                gvColSelected.FocusedRowHandle = focusedRowHandle - 1;
            }
            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        /// <summary>
        ///     ����
        /// </summary>
        private void btnMoveNext_Click(object sender, EventArgs e)
        {
            try
            {
                if (gvColSelected.IsLastRow)
                {
                    MessageShowUtil.ShowInfo("��ǰ������ĩ�У�");
                    return;
                }

                var focusedRowHandle = gvColSelected.FocusedRowHandle;
                var focuseDr = gvColSelected.GetFocusedDataRow();
                var copyDr = LmdDt.NewRow();
                foreach (DataColumn dc in LmdDt.Columns)
                    copyDr[dc.ColumnName] = focuseDr[dc.ColumnName];

                var curRowIndex = LmdDt.Rows.IndexOf(focuseDr); //���������м�¼               
                for (var insertRow = curRowIndex + 1; insertRow < LmdDt.Rows.Count; insertRow++)
                    if (!TypeUtil.ToBool(LmdDt.Rows[insertRow]["blnSysProcess"]))
                    {
                        LmdDt.Rows.Remove(focuseDr);
                        LmdDt.Rows.InsertAt(copyDr, insertRow);
                        break;
                    }
                gvColSelected.FocusedRowHandle = focusedRowHandle + 1;
            }
            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        /// <summary>
        ///     �Ƶ�ĩ��
        /// </summary>
        private void btnMoveLast_Click(object sender, EventArgs e)
        {
            try
            {
                if (gvColSelected.IsLastRow)
                {
                    MessageShowUtil.ShowInfo("��ǰ������ĩ�У�");
                    return;
                }

                var focusedRowHandle = gvColSelected.FocusedRowHandle;
                var focuseDr = gvColSelected.GetFocusedDataRow();
                var copyDr = LmdDt.NewRow();
                foreach (DataColumn dc in LmdDt.Columns)
                    copyDr[dc.ColumnName] = focuseDr[dc.ColumnName];
                LmdDt.Rows.Remove(focuseDr);
                LmdDt.Rows.Add(copyDr);
                gvColSelected.FocusedRowHandle = gvColSelected.RowCount - 1;
            }
            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        /// <summary>
        ///     ɾ����
        /// </summary>
        private void btnDeleteRow_Click(object sender, EventArgs e)
        {
            try
            {
                var rowHandle = gvColSelected.FocusedRowHandle;
                var focuseDr = gvColSelected.GetFocusedDataRow();

                if (focuseDr == null)
                {
                    MessageShowUtil.ShowInfo("��ѡ���м�¼��");
                    return;
                }

                CheckDeleteRow(focuseDr);

                var isCalcField = TypeUtil.ToBool(focuseDr["isCalcField"]);
                if (isCalcField)
                {
                    ShowMsg("�ޱ༭�����е�Ȩ�ޣ�", true);
                    return;
                }

                if (DialogResult.No ==
                    MessageBox.Show("ȷ��Ҫɾ����ǰ����", "��ʾ", MessageBoxButtons.YesNo, MessageBoxIcon.Information))
                    return;

                if (isCalcField)
                {
                    LmdDt.Rows.Remove(focuseDr); //�����У�ֱ��ɾ��
                    ShowGroupTitle();
                    SetRefCalcCol();
                    return;
                }

                //���Ҽ�¼��Ӧ���ڵ㲢ȡ���ڵ�ѡ��״̬����Ӧ�����У������п��Ա���ʱȡ����
                //�����ǰ���ǹ����У�����ɾ��������״̬���ɡ�
                var strFullPath = TypeUtil.ToString(focuseDr["strFullPath"]);
                var metaDataFieldId = TypeUtil.ToInt(focuseDr["metaDataFieldId"]);
                var strId = strFullPath + "_" + metaDataFieldId;
                var curNode = treeList1.FindNodeByFieldValue("strId", strId);
                if (curNode != null)
                    if (curNode.CheckState == CheckState.Checked)
                        curNode.CheckState = CheckState.Unchecked;

                var strFilter = "blnSysProcess=0 and strFullPath like '" + strId + "%'";
                var relatoinDrs = LmdDt.Select(strFilter);
                if ((relatoinDrs != null) && (relatoinDrs.Length > 0))
                {
                    focuseDr["blnSysProcess"] = true;
                    focuseDr["blnShow"] = false;
                }
                else
                {
                    LmdDt.Rows.Remove(focuseDr); //ĩ���ڵ㣬ֱ��ɾ��
                }
            }
            catch (Exception ex)
            {
                ShowMsg(ex.Message, true);
            }
        }

        /// <summary>
        ///     ɾ����У��
        /// </summary>
        /// <param name="dr"></param>
        private void CheckDeleteRow(DataRow dr)
        {
            var str = TypeUtil.ToString(dr["MetaDataFieldName"]);
            if (refCalcColList.Contains(str)) //���������ã�����ɾ��
                throw new Exception("��ǰ�ֶα����ּ�����ʹ�ã�����ɾ����");
            if (refContextContextColList.Contains(str)) //�������������ã�����ɾ��
                throw new Exception("��ǰ�ֶα�����������ʹ�ã�����ɾ����");
        }

        /// <summary>
        ///     ��Ӽ�����
        /// </summary>
        private void btnAddCalcCol_Click(object sender, EventArgs e)
        {
            try
            {
                if (!PermissionManager.HavePermission("SetReportCaclColumn"))
                    return;
                var frm = new frmSchemaAddCalc();
                frm.SelectedDt = LmdDt.Copy();
                frm.ShowDialog();
                if (frm.BlnOk)
                {
                    var dr = LmdDt.NewRow();

                    dr["ListDataID"] = ListDataID;
                    dr["strFieldType"] = frm.Lmd.StrFieldType;
                    dr["MetaDataFieldName"] = frm.Lmd.MetaDataFieldName;
                    dr["strSrcFieldNameCHS"] = frm.StrFieldDispaly;
                    dr["strListDisplayFieldNameCHS"] = frm.StrFieldDispaly;

                    dr["blnSysProcess"] = false;
                    dr["blnShow"] = true;

                    dr["isCalcField"] = true;
                    dr["strFormula"] = frm.Lmd.StrFormula;
                    dr["strRefColList"] = frm.Lmd.StrRefColList;

                    dr["strCondition"] = string.Empty;
                    dr["strConditionCHS"] = string.Empty;
                    dr["strParaColName"] = string.Empty;
                    LmdDt.Rows.Add(dr);

                    SetRefCalcCol();
                    ShowGroupTitle();

                    btnBuildSql_Click(null, null);
                    btnTestSql_Click(null, null);
                }
            }
            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        /// <summary>
        ///     �༭������
        /// </summary>
        private void btnEditCalcCol_Click(object sender, EventArgs e)
        {
            try
            {
                if (!PermissionManager.HavePermission("SetReportCaclColumn"))
                    return;
                var focusedRowHandle = gvColSelected.FocusedRowHandle;
                var focuseDr = gvColSelected.GetFocusedDataRow();
                if (focuseDr == null)
                {
                    ShowMsg("��ѡ��Ҫ�༭�ļ����У�", true);
                    return;
                }
                if (!TypeUtil.ToBool(focuseDr["isCalcField"]))
                {
                    ShowMsg("ѡ��Ĳ��Ǽ����У�", true);
                    return;
                }

                var frm = new frmSchemaAddCalc();
                frm.SelectedDt = LmdDt.Copy();
                frm.BlnEdit = true;
                frm.Lmd.StrFieldType = TypeUtil.ToString(focuseDr["strFieldType"]);
                frm.Lmd.MetaDataFieldName = TypeUtil.ToString(focuseDr["MetaDataFieldName"]);
                frm.StrFieldDispaly = TypeUtil.ToString(focuseDr["strListDisplayFieldNameCHS"]);
                frm.Lmd.StrFormula = TypeUtil.ToString(focuseDr["strFormula"]);
                frm.Lmd.StrRefColList = TypeUtil.ToString(focuseDr["strRefColList"]);
                frm.ShowDialog();
                if (frm.BlnOk)
                {
                    focuseDr["ListDataID"] = ListDataID;
                    focuseDr["strFieldType"] = frm.Lmd.StrFieldType;
                    focuseDr["MetaDataFieldName"] = frm.Lmd.MetaDataFieldName;
                    focuseDr["strSrcFieldNameCHS"] = frm.StrFieldDispaly;
                    focuseDr["strListDisplayFieldNameCHS"] = frm.StrFieldDispaly;

                    focuseDr["blnSysProcess"] = false;
                    focuseDr["blnShow"] = true;

                    focuseDr["isCalcField"] = true;
                    focuseDr["strFormula"] = frm.Lmd.StrFormula;
                    focuseDr["strRefColList"] = frm.Lmd.StrRefColList;

                    focuseDr["strCondition"] = string.Empty;
                    focuseDr["strConditionCHS"] = string.Empty;

                    SetRefCalcCol();
                    btnBuildSql_Click(null, null);
                    btnTestSql_Click(null, null);
                }
            }
            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }


        /// <summary>
        ///     ɾ��������
        /// </summary>
        private void btnDelCalcCol_Click(object sender, EventArgs e)
        {
            try
            {
                if (!PermissionManager.HavePermission("SetReportCaclColumn"))
                    return;
                var focusedRowHandle = gvColSelected.FocusedRowHandle;
                var focuseDr = gvColSelected.GetFocusedDataRow();
                if (focuseDr == null)
                {
                    ShowMsg("��ѡ��Ҫɾ���ļ����У�", true);
                    return;
                }
                if (!TypeUtil.ToBool(focuseDr["isCalcField"]))
                {
                    ShowMsg("ѡ��Ĳ��Ǽ����У�", true);
                    return;
                }

                if (DialogResult.No ==
                    MessageBox.Show("ȷ��Ҫɾ����ǰ����", "��ʾ", MessageBoxButtons.YesNo, MessageBoxIcon.Information))
                    return;

                LmdDt.Rows.Remove(focuseDr);
                ShowGroupTitle();
                SetRefCalcCol();
            }
            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }


        /// <summary>
        ///     ��������������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnContextConditionSet_Click(object sender, EventArgs e)
        {
            try
            {
                if (!PermissionManager.HavePermission("SetReportContenxt"))
                {
                    return;
                }
                var frm = new frmConTextCondition();
                frm.SelectedDt = LmdDt.Copy();
                var strConTextCondition = ListDataExVo.StrConTextCondition == null
                    ? ""
                    : ListDataExVo.StrConTextCondition;
                frm.Lmd.StrFormula = GetConTextConditionString(strConTextCondition);
                frm.ShowDialog();
                if (frm.BlnOk)
                {
                    if (frm.Lmd.StrFormula == string.Empty)
                        ListDataExVo.StrConTextCondition = "";
                    else
                        ListDataExVo.StrConTextCondition = frm.Lmd.StrRefColList + "&&&$$$" + frm.Lmd.StrFormula;

                    SetRefContextContextCol();
                }
            }
            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        /// <summary>
        ///     ��ȡ����������
        /// </summary>
        /// <returns></returns>
        private string GetConTextConditionString(string str)
        {
            if (str == string.Empty)
                return str;

            var strs = str.Split(new[] {"&&&$$$"}, StringSplitOptions.RemoveEmptyEntries);
            if (strs.Length != 2)
                return string.Empty;

            return strs[1].Trim();
        }

        /// <summary>
        ///     ��ȡ���������������д�
        /// </summary>
        /// <returns></returns>
        private string GetConTextConditionRefColString(string str)
        {
            if (str == string.Empty)
                return str;

            var strs = str.Split(new[] {"&&&$$$"}, StringSplitOptions.RemoveEmptyEntries);
            if (strs.Length != 2)
                return string.Empty;

            return strs[0].Trim();
        }

        /// <summary>
        ///     ��ȡ�б�Ԫ��������
        /// </summary>
        /// <returns>DataTable</returns>
        private DataTable GetListMetaDataTable()
        {
            DataTable dt = null;
            DataRow dr = null;

            dt = LmdDt;
            var strFilter = "";

            //���ÿ�����PK�ֶ�
            DataRow[] childDrs = null;
            var strFullPath = string.Empty;
            foreach (var key in tableAliasDataDic.Keys)
            {
                strFilter = "strTableAlias='" + key + "'";
                childDrs = dt.Select(strFilter);
                if (childDrs.Length <= 0)
                    continue;

                strFullPath = TypeUtil.ToString(childDrs[0]["strFullPath"]);
                strFilter = "blnSysProcess=0 and strFullPath like'" + strFullPath + "%'";
                if (dt.Select(strFilter).Length <= 0)
                {
                    //�����ڴ˱��Լ��ӱ���Ŀ�ֶ�

                    //ɾ�������ֶ�
                    strFilter = "strFullPath like'" + strFullPath + "%'";
                    childDrs = dt.Select(strFilter);
                    for (var i = 0; i < childDrs.Length; i++)
                        dt.Rows.Remove(childDrs[i]);

                    continue;
                }

                var keyDt = tableAliasDataDic[key];
                var keyDrs = keyDt.Select("blnPK=1");
                foreach (var keyDr in keyDrs)
                {
                    strFilter = "isnull(MetaDataFieldID,0)=" + TypeUtil.ToInt(keyDr["MetaDataFieldID"]);
                    strFilter += " and isnull(strFullPath,'')='" + TypeUtil.ToString(keyDr["strFullPath"]) + "'";

                    var drs = dt.Select(strFilter);
                    if (drs.Length <= 0)
                    {
                        //�������

                        dr = dt.NewRow();
                        foreach (DataColumn dc in keyDt.Columns)
                            if ("strFieldChName" == dc.ColumnName)
                                dr["strListDisplayFieldNameCHS"] = keyDr[dc.ColumnName];
                            else if (("strFieldName" != dc.ColumnName) && ("lngDataRightType" != dc.ColumnName)
                                        && ("blnPK" != dc.ColumnName) && ("lngRelativeID" != dc.ColumnName) &&
                                        ("blnSelect" != dc.ColumnName))
                                dr[dc.ColumnName] = keyDr[dc.ColumnName];

                        dr["MetaDataFieldName"] = key + "_" + TypeUtil.ToString(keyDr["strFieldName"]);

                        dr["blnSysProcess"] = true;
                        dr["blnShow"] = false;
                        dr["lngKeyFieldType"] = 2;
                        dt.Rows.Add(dr);
                    }
                    else
                    {
                        drs[0]["lngKeyFieldType"] = 2;
                    }
                }
            }

            //���ÿ���������Ȩ���ֶ�
            foreach (var key in tableAliasDataDic.Keys)
            {
                strFilter = "strTableAlias='" + key + "'";
                childDrs = dt.Select(strFilter);
                if (childDrs.Length <= 0)
                    continue;

                var keyDt = tableAliasDataDic[key];
                var keyDrs = keyDt.Select("isnull(lngDataRightType,0)>0");
                foreach (var keyDr in keyDrs)
                {
                    strFilter = "isnull(MetaDataFieldID,0)=" + TypeUtil.ToInt(keyDr["MetaDataFieldID"]);
                    strFilter += " and isnull(strFullPath,'')='" + TypeUtil.ToString(keyDr["strFullPath"]) + "'";

                    var drs = dt.Select(strFilter);
                    if (drs.Length <= 0)
                    {
                        //�������

                        dr = dt.NewRow();
                        foreach (DataColumn dc in keyDt.Columns)
                            if ("strFieldChName" == dc.ColumnName)
                                dr["strListDisplayFieldNameCHS"] = keyDr[dc.ColumnName];
                            else if (("strFieldName" != dc.ColumnName) && ("lngDataRightType" != dc.ColumnName)
                                        && ("blnPK" != dc.ColumnName) && ("lngRelativeID" != dc.ColumnName) &&
                                        ("blnSelect" != dc.ColumnName))
                                dr[dc.ColumnName] = keyDr[dc.ColumnName];

                        dr["MetaDataFieldName"] = key + "_" + TypeUtil.ToString(keyDr["strFieldName"]);

                        dr["blnSysProcess"] = true;
                        dr["blnShow"] = false;

                        dt.Rows.Add(dr);
                    }
                }
            }

            return dt;
        }

        private void gvAdvanceSet_CustomDrawRowIndicator(object sender, RowIndicatorCustomDrawEventArgs e)
        {
            var rowHandle = e.RowHandle + 1;
            if (e.Info.IsRowIndicator && (rowHandle > 0))
                e.Info.DisplayText = rowHandle.ToString();
        }

        private void gvColSelected_CustomDrawRowIndicator(object sender, RowIndicatorCustomDrawEventArgs e)
        {
            var rowHandle = e.RowHandle + 1;
            if (e.Info.IsRowIndicator && (rowHandle > 0))
                e.Info.DisplayText = rowHandle.ToString();
        }

        private void gridViewPreView_CustomDrawRowIndicator(object sender, RowIndicatorCustomDrawEventArgs e)
        {
            var rowHandle = e.RowHandle + 1;
            if (e.Info.IsRowIndicator && (rowHandle > 0))
                e.Info.DisplayText = rowHandle.ToString();
        }

        private void gridViewCondition_CustomDrawRowIndicator(object sender, RowIndicatorCustomDrawEventArgs e)
        {
            var rowHandle = e.RowHandle + 1;
            if (e.Info.IsRowIndicator && (rowHandle > 0))
                e.Info.DisplayText = rowHandle.ToString();
        }

        /// <summary>
        ///     ��������
        /// </summary>
        private void ButtonEditCondition_ButtonClick(object sender, ButtonPressedEventArgs e)
        {
            if (0 != e.Button.Index)
                return;

            var rowHandle = gridViewCondition.FocusedRowHandle;
            var curDr = gridViewCondition.GetDataRow(rowHandle);
            if (curDr == null)
                return;

            try
            {
                if (!PermissionManager.HavePermission("SetReportFixCon"))
                {
                    return;
                }
                var frm = new frmSchemaCondition();
                frm.CurFieldNameCHS = TypeUtil.ToString(curDr["strListDisplayFieldNameCHS"]);
                frm.StrFieldType = TypeUtil.ToString(curDr["strFieldType"]);
                frm.FkCode = TypeUtil.ToString(curDr["strFkCode"]);
                frm.StrCondition = TypeUtil.ToString(curDr["strCondition"]);
                frm.StrConditionCHS = TypeUtil.ToString(curDr["strConditionCHS"]);
                frm.ShowDialog();

                if (frm.BlnOk)
                {
                    curDr["strCondition"] = frm.StrCondition;
                    curDr["strConditionCHS"] = frm.StrConditionCHS;
                    curDr["strConditionShow"] = frm.StrConditionCHS.Replace("&&$$", ";").Replace("&&$", ",");
                }
            }
            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        /// <summary>
        ///     ����������
        /// </summary>
        private void ButtonEditHyperlink_ButtonClick(object sender, ButtonPressedEventArgs e)
        {
            if (0 != e.Button.Index)
                return;

            var rowHandle = gvAdvanceSet.FocusedRowHandle;
            var curDr = gvAdvanceSet.GetDataRow(rowHandle);
            if (curDr == null)
                return;

            try
            {
                //if (TypeUtil.ToBool(curDr["isCalcField"]))
                //{
                //    this.ShowMsg("�����в��ܽ��г��������ã�", true);
                //    return;
                //}

                var strTableAlias = TypeUtil.ToString(curDr["strTableAlias"]);
                var dt = GetListMetaDataTable().Copy();

                var frm = new frmListParameterSet();
                frm.DataSource = dt;
                frm.StrTableAlias = strTableAlias;
                frm.Model = Model;
                frm.StrFieldNameList = TypeUtil.ToString(curDr["strParaColName"]);
                frm.StrHyperlink = TypeUtil.ToString(curDr["strHyperlink"]);
                // frm.StrWebHyperlink = TypeUtil.ToString(curDr["strWebHyperlink"]);
                frm.LngHyperLinkType = TypeUtil.ToInt(curDr["lngHyperLinkType"]);
                frm.BlnLink = true;
                frm.ShowDialog();

                if (frm.BlnOk)
                {
                    curDr["strParaColName"] = frm.StrFieldNameList;
                    curDr["strHyperlink"] = frm.StrHyperlink;
                    // curDr["strWebHyperlink"] = frm.StrWebHyperlink;
                    curDr["lngHyperLinkType"] = frm.LngHyperLinkType;
                }
            }
            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        /// <summary>
        ///     ������ʽ����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSFCSetting_Click(object sender, EventArgs e)
        {
            if (!PermissionManager.HavePermission("SetReportStyle"))
            {
                return;
            }

            var rowHandle = gridViewCondition.FocusedRowHandle;
            var curDr = gridViewCondition.GetDataRow(rowHandle);
            if (curDr == null)
            {
                ShowMsg("��ѡ����Ŀ��", true);
                return;
            }

            if (TypeUtil.ToString(curDr["strFieldType"]) == "bit")
            {
                ShowMsg("�߼�����Ŀ�������ã�", true);
                return;
            }

            try
            {
                if (!PermissionManager.HavePermission("SetReportStyle"))
                    return;
                var frm = new frmListExSFC();
                frm.CurFieldNameCHS = TypeUtil.ToString(curDr["strListDisplayFieldNameCHS"]);
                frm.StrFieldType = TypeUtil.ToString(curDr["strFieldType"]);
                frm.StrCondition = TypeUtil.ToString(curDr["strConditionFormat"]);
                frm.ShowDialog();
                if (frm.BlnOk)
                {
                    curDr["strConditionFormat"] = frm.StrCondition;
                    curDr["strConditionFormatShow"] = frm.StrCondition.Replace("&&$$", ";").Replace("&&$", ",");
                }
            }
            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }


        private void frmSchemaDesign_FormClosing(object sender, FormClosingEventArgs e)
        {
        }

        /// <summary>
        ///     ��갴���¼�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gvColSelected_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                downHitInfo = gvColSelected.CalcHitInfo(new Point(e.X, e.Y)); //����������ȥʱ��GridView�е�����
            }
            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        /// <summary>
        ///     ����ƶ��¼�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gvColSelected_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (e.Button != MouseButtons.Left) return; //�����������Ч
                if ((downHitInfo == null) || (downHitInfo.RowHandle < 0)) return; //�ж�����λ���Ƿ���Ч

                var rowsselect = gvColSelected.GetSelectedRows(); //��ȡ��ѡ�е�index


                var list = new ArrayList();
                foreach (var i in rowsselect)
                {
                    var drs = LmdDt.Select("ID=" + TypeUtil.ToInt(gvColSelected.GetRowCellValue(i, "ID")));
                    var index = LmdDt.Rows.IndexOf(drs[0]); //��ȡ��ק��Ŀ����index�������������У�����Ҫȥ������Դ������
                    list.Add(index);
                }

                rows = new int[list.Count];
                for (var i = 0; i < list.Count; i++)
                    rows[i] = TypeUtil.ToInt(list[i]);


                startRow = rows.Length == 0 ? -1 : rows[0];
                var dt = LmdDt.Clone();

                foreach (var r in rows) // ������ѡ�е�index����ȡֵ��ȥ����ѡ�������ݣ�������ѡȡ�Ķ���
                {
                    var dataSourcerows = r;
                    dt.ImportRow(LmdDt.Rows[dataSourcerows]); //������ѡȡ��������
                }
                gcColSelected.DoDragDrop(dt, DragDropEffects.Move); //��ʼ�ϷŲ���������ק�����ݴ洢����
            }
            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        /// <summary>
        ///     ��ק�����¼�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gcColSelected_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }


        /// <summary>
        ///     ��ק��ɺ��¼�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gcColSelected_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                var gridviewPoint = PointToScreen(gcColSelected.Location); //��ȡ�������Ļ�ϵ�λ�á�
                upHitInfo = gvColSelected.CalcHitInfo(new Point(e.X - gridviewPoint.X, e.Y - gridviewPoint.Y));
                    //������������ʱ��GridView�е����꣨��Ļλ�ü�ȥ gridView ��ʼλ�ã�
                if ((upHitInfo == null) || (upHitInfo.RowHandle < 0))
                    if (upHitInfo.RowHandle == -2147483648)
                        upHitInfo.RowHandle = gvColSelected.RowCount - 1;
                    else
                        return;
                else
                    upHitInfo.RowHandle -= 1;

                var drs = LmdDt.Select("ID=" + TypeUtil.ToInt(gvColSelected.GetRowCellValue(upHitInfo.RowHandle, "ID")));
                var endRow = LmdDt.Rows.IndexOf(drs[0]); //��ȡ��ק��Ŀ����index


                var dt = e.Data.GetData(typeof(DataTable)) as DataTable;
                    //��ȡҪ�ƶ������ݣ�����ק����ĵط�����gridControl1.DoDragDrop(dt, DragDropEffects.Move); ��


                if ((dt != null) && (dt.Rows.Count > 0)) //��ק������Ϊ��
                {
                    int a;
                    var xs = LmdDt.Rows[endRow]; //��ȡ��ק��Ŀ���У�׼��������ֲ
                    if (!rows.Contains(endRow)) //�����ѡ�Ļ���ȷ��������ק���⼸����
                    {
                        gvColSelected.ClearSelection(); //��GirdView��ɾ������ק������
                        var moveValue = 0;
                        foreach (var i in rows)
                        {
                            LmdDt.Rows.Remove(LmdDt.Rows[i - moveValue]); //��GirdView������Դ��ɾ������ק������
                            moveValue++;
                        }

                        if (startRow > endRow)
                            a = LmdDt.Rows.IndexOf(xs); //���������У��������굽���е�����
                        else
                            a = LmdDt.Rows.IndexOf(xs) + 1; //��������ϣ��������굽���е�����
                        var j = 0;
                        DataRow drTemp;
                        foreach (DataRow dr in dt.Rows)
                        {
                            drTemp = LmdDt.NewRow();
                            foreach (DataColumn dc in dr.Table.Columns)
                                drTemp[dc.ColumnName] = dr[dc.ColumnName];
                            LmdDt.Rows.InsertAt(drTemp, a + j); //����ק�������ٴ���ӽ���


                            for (var i = 0; i < gvColSelected.RowCount; i++)
                            {
//�϶�������ԴĿ��ID��gridviewf���бȽ�,������������gridview��ѡ��
                                var id = TypeUtil.ToInt(gvColSelected.GetRowCellValue(i, "ID"));
                                var iddrag = TypeUtil.ToInt(drTemp["ID"]);
                                if (id == iddrag)
                                    gvColSelected.SelectRow(i);
                            }


                            // gvColSelected.SelectRow(a + j);
                            j++;
                        }
                    }


                    //gcColSelected.DataSource = _lmdDt; //���°�
                    //gvColSelected.RefreshData();
                }
            }
            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        private void gvColSelected_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
                btnDeleteRow_Click(null, null);
        }


        /// <summary>
        ///     ģ����ʾ����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnBluerConditionSet_Click(object sender, EventArgs e)
        {
            if (!PermissionManager.HavePermission("SetReportBlurr"))
            {
                return;
            }
            var rowHandle = gridViewCondition.FocusedRowHandle;
            var curDr = gridViewCondition.GetDataRow(rowHandle);
            if (curDr == null)
            {
                ShowMsg("��ѡ����Ŀ��", true);
                return;
            }

            if (TypeUtil.ToString(curDr["strFieldType"]) == "bit")
            {
                ShowMsg("�߼�����Ŀ�������ã�", true);
                return;
            }

            var frm = new frmBluerCondition();
            frm.StrCondition = TypeUtil.ToString(curDr["strBluerCondition"]);
            frm.SelectedDt = LmdDt.Copy();
            frm.ShowDialog();
            if (frm.BlnOk)
                curDr["StrBluerCondition"] = frm.StrCondition;
        }


        /// <summary>
        ///     �Ҽ��˵���Ӽ�����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tlbAddCalcCol_Click(object sender, EventArgs e)
        {
            btnAddCalcCol_Click(null, null);
        }

        /// <summary>
        ///     �Ҽ��༭������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tlbEditCalcCol_Click(object sender, EventArgs e)
        {
            btnEditCalcCol_Click(null, null);
        }


        /// <summary>
        ///     �Ҽ�ɾ��������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tlbDelCalcCol_Click(object sender, EventArgs e)
        {
            btnDelCalcCol_Click(null, null);
        }


        /// <summary>
        ///     �Ҽ�������ʽ����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tlbSFCSetting_Click(object sender, EventArgs e)
        {
            btnSFCSetting_Click(null, null);
        }

        /// <summary>
        ///     �Ҽ���������������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tlbContextConditionSet_Click(object sender, EventArgs e)
        {
            btnContextConditionSet_Click(null, null);
        }

        private void tlbBluerConditionSet_Click(object sender, EventArgs e)
        {
            btnBluerConditionSet_Click(null, null);
        }


        /// <summary>
        ///     ��Ŀ˳�����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tlbColumnSort_Click(object sender, EventArgs e)
        {
            var rowHandle = gridViewCondition.FocusedRowHandle;
            var curDr = gridViewCondition.GetDataRow(rowHandle);
            var strFileName = TypeUtil.ToString(curDr["MetaDataFieldName"]);
            var strListDisplayName = TypeUtil.ToString(curDr["strListDisplayFieldNameCHS"]);
            var rows =
                metadataFieldDt.Select("ID='" + TypeUtil.ToInt(curDr["MetaDataFieldID"]) + "' and blnDesignSort=1");
            if ((rows == null) || (rows.Length == 0))
            {
                MessageShowUtil.ShowMsg("Ԫ����δ���ô��пɱ���,������Ԫ���ݶ��壡");
                return;
            }

            if (TypeUtil.ToString(rows[0]["strFkCode"]) == "")
            {
                MessageShowUtil.ShowMsg("Ԫ����δ���ò��ձ��룬������Ԫ�������ã�");
                return;
            }
            var strFKCode = TypeUtil.ToString(rows[0]["strFkCode"]);
            var frm = new frmDataSortSet();
            frm.MetadataID = MetadataId;
            frm.dtLmd = LmdDt;
            frm.strFileName = strFileName;
            frm.strListDisplayName = strListDisplayName;
            frm.strFKCode = strFKCode;
            //frm.strCondition = TypeUtil.ToString(curDr["strCondition"]);
            //frm.strConditionCHS = TypeUtil.ToString(curDr["strConditionCHS"]);
            frm.ListDataID = ListDataID;

            frm.ShowDialog();
            if (frm.blnOK)
            {
                //curDr["strCondition"] = frm.strCondition;
                //curDr["strConditionCHS"] = frm.strConditionCHS;
                //curDr["strConditionShow"] = frm.strConditionCHS.Replace("&&$$", ";").Replace("&&$", ",");
            }
        }

        private void tlbPivotSetting_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                if (!PermissionManager.HavePermission("OpenReportDesign"))
                {
                    return;
                }

                if ((ListDataExVo == null) || (ListDataExVo.ListDataID == 0))
                {
                    ShowMsg("δ���淽�������ȱ�����ٲ�����", true);
                    return;
                }

                var strFileName = AppDomain.CurrentDomain.BaseDirectory + "Config\\ReportTemple\\" +
                                  ListDataExVo.StrListDataName + ListDataExVo.ListDataID + ".repx";
                //�ָ��ڱ��������������жϣ���Ҫ��֤ 
                //if (!File.Exists(strFileName))
                //{//��������ڣ����ȴ��������ļ�
                //    frmList frm = new frmList();
                //    frm.strReportFileName = _listDataExVo.StrListDataName + _listDataExVo.ListDataID.ToString();
                //    frm.listDisplayExList = Model.GetListDisplayExData(_listDataExVo.ListDataID);                  
                //    frm.SetPivotFormat();
                //}              

                var frmDesign = new frmReportDesign();
                frmDesign.StrFileName = strFileName;
                var strsql = ListDataExVo.StrListSQL.Insert(6, " top 10 ");
                if (Model.GetDbType() == "mysql")
                    strsql = ListDataExVo.StrListSQL + " LIMIT 1,10";
                var dt = Model.GetDataTable(strsql);
                dt = TypeUtil.AddNumberToDataTable(dt);
                frmDesign.dtReportDataSource = dt;
                frmDesign.dtLmd = LmdDt;
                frmDesign.listDataExDTO = ListDataExVo;
                frmDesign.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }


        /// <summary>
        ///     ��ʼ������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tlbInitScheme_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                if (!PermissionManager.HavePermission("InitReportColumn"))
                {
                    return;
                }

                UpdateGridCurrentRow();

                foreach (DataRow row in LmdDt.Rows)
                {
                    row["strConditionShow"] = "";
                    row["blnSummary"] = false;
                    row["lngSummaryType"] = 0;
                    row["strSummaryDisplayFormat"] = "";
                    row["lngHyperLinkType"] = 0;
                    row["strHyperlink"] = "";
                    row["strParaColName"] = "";
                    row["strConditionFormat"] = "";
                    row["strConditionFormatShow"] = "";
                    row["strBluerCondition"] = "";
                    row["blnMerge"] = false;
                    row["lngApplyType"] = 1;
                    row["lngOrder"] = 0;
                    row["lngOrderMethod"] = 0;
                    row["strCondition"] = "";
                    row["strConditionCHS"] = "";
                    row["lngFKType"] = 0;
                    row["blnMainMerge"] = false;
                    row["blnKeyWord"] = false;
                    row["lngKeyGroup"] = 0;
                }
                var rows = LmdDt.Select("isCalcField=1");
                foreach (var row in rows)
                    LmdDt.Rows.Remove(row);
                MessageShowUtil.ShowInfo("��ʼ���ɹ���");
            }
            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        private void SetControlVisible()
        {
            PermissionManager.HavePermission("SaveAsReportScheme", tlbSaveAs);
            PermissionManager.HavePermission("SaveReportScheme", tlbSave);
            PermissionManager.HavePermission("DeleteReportScheme", tlbDelete);
            PermissionManager.HavePermission("SetReportFixCon", ButtonEditCondition);
            PermissionManager.HavePermission("SetReportCaclColumn", btnAddCalcCol);
            PermissionManager.HavePermission("SetReportCaclColumn", btnEditCalcCol);
            PermissionManager.HavePermission("SetReportCaclColumn", btnDelCalcCol);
            PermissionManager.HavePermission("SetReportStyle", btnSFCSetting);
            PermissionManager.HavePermission("SetReportBlurr", btnBluerConditionSet);
            PermissionManager.HavePermission("SetReportContenxt", btnContextConditionSet);
            PermissionManager.HavePermission("OpenReportDesign", tlbPivotSetting);
            PermissionManager.HavePermission("InitReportColumn", tlbInitScheme);
            Text = StrListName + Text;
        }

        #region �������¼�

        /// <summary>
        ///     ��ȡ���ڵķ������б�
        /// </summary>
        /// <returns>IList</returns>
        private IList<string> GetExistSchemaNameList()
        {
            IList<string> existNameList = new List<string>();
            var dt = (DataTable) lookUpSchema.Properties.DataSource;
            if ((dt != null) && (dt.Rows.Count > 0))
                for (var i = 0; i < dt.Rows.Count; i++)
                    existNameList.Add(TypeUtil.ToString(dt.Rows[i]["strListDataName"]));

            return existNameList;
        }

        /// <summary>
        ///     ���淽��
        /// </summary>
        private void tlbSave_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                if (!PermissionManager.HavePermission("SaveReportScheme"))
                {
                    return;
                }


                var schemaName = lookUpSchema.Text;
                if (schemaName == string.Empty)
                {
                    var existNameList = GetExistSchemaNameList();
                    var frm = new frmSchemaName();
                    frm.ExistSchemaNameList = existNameList;
                    frm.ShowDialog();

                    if (frm.SchemaName == string.Empty)
                        return;

                    schemaName = frm.SchemaName;
                }

                UpdateGridCurrentRow(); //����Grid��ǰ��

                //2015-02-03 ������ձ�Ԫ����,�������û����ֶ�
                var dtMetadata = ClientCacheModel.GetServerMetaData(MetadataId);
                DataRow[] rows = null;
                //DataRow[] rows = dtMetadata.Select("blnDay=1");//�ж���Ԫ�����ǲ����ձ�
                //if (rows != null && rows.Length != 0)
                //{
                //    rows = this._lmdDt.Select("lngSummaryType>1");
                //    if (rows != null && rows.Length > 0)
                //    {
                //        MessageShowUtil.ShowInfo("�ձ������û����ֶΣ�");
                //        return;
                //    }
                //}

                var rows2 = LmdDt.Select("blnMainMerge=1");
                var rows3 = LmdDt.Select("strConditionFormat<>''");
                if (rows2.Length > 0 && rows3.Length > 0)
                {
                    MessageShowUtil.ShowInfo("����ͬʱ�������ϲ���������ʽ��");
                    return;
                }

                rows = LmdDt.Select("blnKeyWord=1");
                if (rows.Length > 1)
                {
                    MessageShowUtil.ShowInfo("�ؼ�������ֻ������һ����");
                    return;
                }
                rows = LmdDt.Select("blnKeyWord=1  and  lngKeyGroup>0");
                if (rows.Length > 0)
                {
                    MessageShowUtil.ShowInfo("�ؼ��ֲ������û��ܣ�");
                    return;
                }

                rows = LmdDt.Select("lngProivtType>0");
                if ((rows.Length > 0) && (rows.Length < 3))
                {
                    MessageShowUtil.ShowInfo("��������ͱ�����ֻ��һ����������������������");
                    return;
                }


                //��֯����
                GetListMetaDataTable(); //��ȡListMetaData���ݱ�
                OrgListDataExData(schemaName, 0, LmdDt); //��֯ListDataEx                
                var lmdList = OrgListMetaDataData(LmdDt); //��֯ListMetaData
                var ldList = OrgListDisplayExData(ListDataExVo.UserID > 0 ? 1 : 0, LmdDt); //��֯ListDisplayEx

                //2015-02-13  �ж���Ŀ�����Ƿ����ظ�
                var viewsource = LmdDt.Copy().DefaultView;
                viewsource.RowFilter = "blnSysProcess=0 and blnShow=1";
                var dtsource = viewsource.ToTable(true, "strListDisplayFieldNameCHS");
                if (viewsource.Count != dtsource.Rows.Count)
                {
                    MessageShowUtil.ShowInfo("��Ŀ�������ظ�,���޸ĺ��ٱ��棡");
                    return;
                }

                //��������
                // 20170605
                // 20170624
                //var par = new ListexInfo()
                //{
                //    ListCommandExDTOList = new List<ListcommandexInfo>(),
                //    ListDataExDTOList = new List<ListdataexInfo>(),
                //    LastUpdateDate = "",
                //    Programer = "",
                //    ProgramerNotes = "",
                //    StrDescription = "",
                //    StrGuidCode = "",
                //    StrListCode = "",
                //    StrListDescription = "",
                //    StrListGroupCode = "",
                //    StrListName = "",
                //    StrRightCode = ""
                //};
                //ListDataExVo.ListDataLayoutDTOList = new List<ListdatalayountInfo>();
                //ListDataExVo.ListDisplayExDTOList = new List<ListdisplayexInfo>();
                //ListDataExVo.ListMetaDataDTOList = new List<ListmetadataInfo>();
                //ListDataExVo.ListTempleDTOList = new List<ListtempleInfo>();
                //ListDataExVo.StrConTextCondition = "";
                //ListDataExVo.StrDefaultShowStyle = "";
                
                //ListDataExVo = Model.SaveList(null, new List<ListcommandexInfo>(), ListDataExVo, lmdList,
                //    ldList, LmdDt, ListID <= 0 ? 0 : 1);
                ListDataExVo = Model.SaveList(null, null, ListDataExVo, lmdList,
                    ldList, LmdDt, ListID <= 0 ? 0 : 1);
                ListDataExVo.InfoState = InfoState.Modified;

                ListDataID = ListDataExVo.ListDataID;

                LoadShcemaList(); //ˢ������Դ
                lookUpSchema.EditValueChanged -= lookUpSchema_EditValueChanged;
                lookUpSchema.EditValue = ListDataID;
                lookUpSchema.EditValueChanged += lookUpSchema_EditValueChanged;

                RefreshSchemaDataSource();

                MessageShowUtil.ShowStaticInfo("����ɹ���", barStaticItemMsg);
            }
            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        /// <summary>
        ///     ��淽��
        /// </summary>
        private void tlbSaveAs_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                if (!PermissionManager.HavePermission("SaveAsReportScheme"))
                {
                    return;
                }
                if ((ListDataExVo == null) || (ListDataExVo.InfoState == InfoState.AddNew))
                {
                    ShowMsg("��ǰ����δ���棬������棡", true);
                    return;
                }

                UpdateGridCurrentRow(); //����Grid��ǰ��

                var existNameList = GetExistSchemaNameList();
                var frm = new frmSchemaName();
                frm.ExistSchemaNameList = existNameList;
                frm.ShowDialog();

                if (frm.SchemaName == string.Empty)
                    return;

                ListDataExVo.InfoState = InfoState.AddNew;

                //��֯����
                GetListMetaDataTable(); //��ȡListMetaData���ݱ�
                OrgListDataExData(frm.SchemaName, frm.LngUseRight, LmdDt); //��֯ListDataEx               
                var lmdList = OrgListMetaDataData(LmdDt); //��֯ListMetaData
                var ldList = OrgListDisplayExData(frm.LngUseRight, LmdDt); //��֯ListDisplayEx

                // 20170605
                // 20170624
                //var par = new ListexInfo()
                //{
                //    ListCommandExDTOList = new List<ListcommandexInfo>(),
                //    ListDataExDTOList = new List<ListdataexInfo>(),
                //    LastUpdateDate = "",
                //    Programer = "",
                //    ProgramerNotes = "",
                //    StrDescription = "",
                //    StrGuidCode = "",
                //    StrListCode = "",
                //    StrListDescription = "",
                //    StrListGroupCode = "",
                //    StrListName = "",
                //    StrRightCode = ""
                //};
                //ListDataExVo.ListDataLayoutDTOList = new List<ListdatalayountInfo>();
                //ListDataExVo.ListDisplayExDTOList = new List<ListdisplayexInfo>();
                //ListDataExVo.ListMetaDataDTOList = new List<ListmetadataInfo>();
                //ListDataExVo.ListTempleDTOList = new List<ListtempleInfo>();
                //ListDataExVo.StrConTextCondition = "";
                //ListDataExVo.StrDefaultShowStyle = "";
                //ListDataExVo = Model.SaveList(par, new List<ListcommandexInfo>(), ListDataExVo, lmdList,
                //    ldList, LmdDt, 2);
                ListDataExVo = Model.SaveList(null, null, ListDataExVo, lmdList, ldList, LmdDt, 2);
                ListDataExVo.InfoState = InfoState.Modified;
                ;

                ListDataID = ListDataExVo.ListDataID;

                var dt = (DataTable) lookUpSchema.Properties.DataSource;
                //ˢ������Դ
                var dr = dt.NewRow();
                dr["ListDataID"] = ListDataID;
                dr["strListDataName"] = frm.SchemaName;
                dt.Rows.Add(dr);
                lookUpSchema.Properties.DataSource = dt;

                //����Ϊ��ǰ����
                lookUpSchema.EditValueChanged -= lookUpSchema_EditValueChanged;
                lookUpSchema.EditValue = ListDataID;
                lookUpSchema.EditValueChanged += lookUpSchema_EditValueChanged;

                RefreshSchemaDataSource();

                MessageShowUtil.ShowStaticInfo("���ɹ���", barStaticItemMsg);
            }
            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        /// <summary>
        ///     ����Grid��ǰ��
        /// </summary>
        private void UpdateGridCurrentRow()
        {
            gvAdvanceSet.CloseEditor();
            gvAdvanceSet.UpdateCurrentRow();

            gvColSelected.CloseEditor();
            gvColSelected.UpdateCurrentRow();
        }

        /// <summary>
        ///     ɾ������
        /// </summary>
        private void tlbDelete_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (!PermissionManager.HavePermission("DeleteReportScheme"))
            {
                return;
            }

            if (ListDataID == 0)
            {
                ShowMsg("��ǰ����δ���棬����ɾ����", true);
                return;
            }
            if (CurListDataId == ListDataExVo.ListDataID)
            {
                ShowMsg("�б�����ʹ�õķ�������ɾ����", true);
                return;
            }
            //if (!PermissionManager.HavePermission("DeleteSchema"))
            //{
            //    this.ShowMsg("û��ɾ��������Ȩ�ޣ�����ɾ����", true);
            //    return;
            //}
            //if (this._listDataExVo.BlnDefault && !PermissionManager.HavePermission("DeleteDefaultSchema"))
            //{
            //    this.ShowMsg("û��ɾ��Ĭ�Ϸ�����Ȩ�ޣ�����ɾ����", true);
            //    return;
            //}
            //if (this._listDataExVo.BlnPredefine && !PermissionManager.HavePermission("DeletePredefineSchema"))
            //{
            //    this.ShowMsg("û��ɾ��Ԥ�÷�����Ȩ�ޣ�����ɾ��", true);
            //    return;
            //}

            if (DialogResult.No == MessageShowUtil.ReturnDialogResult("ȷ��Ҫɾ����ǰ������"))
                return;

            try
            {
                Model.DeleteSchema(ListDataExVo.ListID, ListDataExVo.ListDataID);

                ListDataID = 0;

                ClearSchema();

                LoadShcemaList(); //ˢ������Դ

                if (ListDataID <= 0)
                    CreateNewSchema();

                LoadSchema(false);

                MessageShowUtil.ShowInfo("ɾ���ɹ�");
            }
            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        /// <summary>
        ///     ����ΪĬ�Ϸ���
        /// </summary>
        private void tlbSetDefault_ItemClick(object sender, ItemClickEventArgs e)
        {
            if ((ListDataExVo == null) || (ListDataExVo.InfoState == InfoState.AddNew))
            {
                MessageShowUtil.ShowInfo("��ǰ����δ���棬���ȱ��棡");
                return;
            }

            try
            {
                if (ListDataExVo.BlnDefault)
                {
                    MessageShowUtil.ShowInfo("���óɹ���");
                    return;
                }

                ListDataExVo.BlnDefault = true;
                Model.SetDefaultSchema(ListDataExVo);
                LoadShcemaList(); //ˢ������Դ
                MessageShowUtil.ShowInfo("���óɹ���");
            }
            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        /// <summary>
        ///     ����������
        /// </summary>
        private void tlbRename_ItemClick(object sender, ItemClickEventArgs e)
        {
            if ((ListDataExVo == null) || (ListDataExVo.InfoState == InfoState.AddNew))
            {
                MessageShowUtil.ShowInfo("��ǰ����δ���棬���ȱ��棡");
                return;
            }

            try
            {
                var existNameList = GetExistSchemaNameList();
                var frm = new frmSchemaName();
                frm.SchemaName = lookUpSchema.Text.Trim();
                frm.ExistSchemaNameList = existNameList;
                frm.ShowDialog();

                if (frm.SchemaName == string.Empty)
                    return;

                ListDataExVo.StrListDataName = frm.SchemaName;
                Model.SaveListDataEx(ListDataExVo);
                LoadShcemaList(); //ˢ������Դ
                MessageShowUtil.ShowInfo("�����������ɹ���");
            }
            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        /// <summary>
        ///     ��֯ListDataEx
        /// </summary>
        /// <param name="schemaName">������</param>
        /// <param name="lngUseRight">����ʹ��Ȩ��</param>
        private void OrgListDataExData(string schemaName, int lngUseRight, DataTable lmdDt)
        {
            ListDataExVo.ListDataID = ListDataID;
            ListDataExVo.ListID = ListID;
            ListDataExVo.StrListSQL = "11";
            ListDataExVo.StrListSrcSQL = "";
            ListDataExVo.StrListDataName = schemaName;
            ListDataExVo.BlnSmlSum = chkSmlSum.Checked;
            ListDataExVo.LngSmlSumType = TypeUtil.ToInt(lookUpSmlSumType.EditValue);
            if (TypeUtil.ToString(ListDataExVo.StrListDefaultField) == string.Empty)
            {
                DataTable dt = null;
                if (lmdDt != null)
                {
                    dt = lmdDt.Copy();
                    dt.DefaultView.RowFilter = " isnull(lngKeyFieldType,0)=2";
                }
                var frm = new frmListParameterSet();
                frm.DataSource = dt;
                frm.ShowDialog();
                ListDataExVo.StrListDefaultField = frm.StrFieldNameList;

                if (ListDataExVo.StrListDefaultField == string.Empty)
                    throw new Exception("�����뷽��Ĭ�ϲ�����");
            }

            if (ListDataExVo.InfoState == InfoState.AddNew)
            {
                ListDataExVo.StrPivotSetting = "";
                ListDataExVo.StrChartSetting = "";

                ListDataExVo.LngRowIndex = 1;
                if (lookUpSchema.EditValue == null)
                    ListDataExVo.BlnDefault = true;
                else
                    ListDataExVo.BlnDefault = false;
                ListDataExVo.BlnPredefine = false;
                ListDataExVo.UserID = 0;
                if (lngUseRight == 1)
                    ListDataExVo.UserID = TypeUtil.ToInt(RequestUtil.GetParameterValue("userId"));
            }
            else
            {
                if (ListDataExVo.BlnPredefine)
                    throw new Exception("û�б༭Ԥ�Ʒ�����Ȩ�ޣ����ܱ��棡");
            }
        }

        /// <summary>
        ///     ��֯ListMetaData
        /// </summary>
        /// <returns>IList</returns>
        private IList<ListmetadataInfo> OrgListMetaDataData(DataTable lmdDt)
        {
            IList<ListmetadataInfo> list = new List<ListmetadataInfo>();
            ListmetadataInfo item = null;
            DataRow dr;
            var strTableAlias = "";
            for (var i = 0; i < lmdDt.Rows.Count; i++)
            {
                dr = lmdDt.Rows[i];

                strTableAlias = TypeUtil.ToString(dr["strTableAlias"]);

                item = new ListmetadataInfo();
                item.InfoState = InfoState.AddNew;
                item.ID = 0;
                item.ListDataID = ListDataID;
                item.MetaDataID = TypeUtil.ToInt(dr["MetaDataID"]);
                item.MetaDataFieldID = TypeUtil.ToInt(dr["MetaDataFieldID"]);
                item.MetaDataFieldName = TypeUtil.ToString(dr["MetaDataFieldName"]);
                item.LngParentFieldID = TypeUtil.ToInt(dr["lngParentFieldID"]);
                item.LngRelativeFieldID = TypeUtil.ToInt(dr["lngRelativeFieldID"]);
                item.StrTableAlias = strTableAlias;
                item.StrFullPath = TypeUtil.ToString(dr["strFullPath"]);
                item.StrParentFullPath = TypeUtil.ToString(dr["strParentFullPath"]);
                item.LngAliasCount = TypeUtil.ToInt(dr["lngAliasCount"]);
                item.LngSourceType = TypeUtil.ToInt(dr["lngSourceType"]);
                item.LngParentID = TypeUtil.ToInt(dr["lngParentID"]);
                item.StrFieldType = TypeUtil.ToString(dr["strFieldType"]);
                item.StrFkCode = TypeUtil.ToString(dr["strFkCode"]);
                item.IsCalcField = TypeUtil.ToBool(dr["isCalcField"]);
                item.StrFormula = TypeUtil.ToString(dr["strFormula"]);
                item.StrRefColList = TypeUtil.ToString(dr["strRefColList"]);
                item.LngOrder = TypeUtil.ToInt(dr["lngOrder"]);
                item.LngOrderMethod = TypeUtil.ToInt(dr["lngOrderMethod"]);
                item.StrCondition = TypeUtil.ToString(dr["strCondition"]);
                item.StrConditionCHS = TypeUtil.ToString(dr["strConditionCHS"]);
                item.LngKeyFieldType = TypeUtil.ToInt(dr["lngKeyFieldType"]);

                item.BlnFreCondition = TypeUtil.ToBool(dr["blnFreCondition"]);
                item.LngFreConIndex = TypeUtil.ToInt(dr["lngFreConIndex"]);
                item.StrFreCondition = TypeUtil.ToString(dr["strFreCondition"]);
                item.StrFreConditionCHS = TypeUtil.ToString(dr["strFreConditionCHS"]);
                item.BlnReceivePara = TypeUtil.ToBool(dr["blnReceivePara"]);

                item.BlnSysProcess = TypeUtil.ToBool(dr["blnSysProcess"]);
                item.BlnShow = TypeUtil.ToBool(dr["blnShow"]);
                item.BlnPrintFreCondition = TypeUtil.ToBool(dr["blnPrintFreCondition"]);
                list.Add(item);
            }

            return list;
        }

        /// <summary>
        ///     ��֯ListDisplayEx
        /// </summary>
        /// <returns>IList</returns>
        private IList<ListdisplayexInfo> OrgListDisplayExData(int lngUseRight, DataTable lmdDt) //�������������¼�루���԰汾��δ������
        {
            IList<ListdisplayexInfo> list = new List<ListdisplayexInfo>();
            ListdisplayexInfo item = null;
            var lngRowIndex = 0; //��ʾ˳��
            DataRow dr;
            for (var i = 0; i < lmdDt.Rows.Count; i++)
            {
                dr = lmdDt.Rows[i];

                item = new ListdisplayexInfo();
                item.InfoState = InfoState.AddNew;

                item.ListDisplayID = 0;
                item.UserID = TypeUtil.ToInt(RequestUtil.GetParameterValue("userId"));
                item.ListDataID = ListDataID;
                item.StrListDisplayFieldName = TypeUtil.ToString(dr["MetaDataFieldName"]);
                item.StrListDisplayFieldNameCHS = TypeUtil.ToString(dr["strListDisplayFieldNameCHS"]);
                item.BlnSummary = TypeUtil.ToBool(dr["blnSummary"]);
                item.LngListDisplayFieldFormat = 0;
                item.LngDisplayWidth = TypeUtil.ToInt(dr["lngDisplayWidth"]);
                if (TypeUtil.ToBool(dr["blnShow"]))
                    item.LngRowIndex = ++lngRowIndex;
                else
                    item.LngRowIndex = -1;
                item.LngSummaryType = TypeUtil.ToInt(dr["lngSummaryType"]);
                item.StrSummaryDisplayFormat = TypeUtil.ToString(dr["strSummaryDisplayFormat"]);
                item.LngHyperLinkType = TypeUtil.ToInt(dr["lngHyperLinkType"]);
                item.StrHyperlink = TypeUtil.ToString(dr["strHyperlink"]);
                item.StrParaColName = TypeUtil.ToString(dr["strParaColName"]);
                item.StrConditionFormat = TypeUtil.ToString(dr["strConditionFormat"]);
                item.IsCalcField = TypeUtil.ToBool(dr["isCalcField"]);
                item.StrBluerCondition = TypeUtil.ToString(dr["strBluerCondition"]);
                item.BlnMerge = TypeUtil.ToBool(dr["blnMerge"]);
                item.LngApplyType = TypeUtil.ToInt(dr["lngApplyType"]);
                item.LngFKType = TypeUtil.ToInt(dr["lngFKType"]);
                item.BlnMainMerge = TypeUtil.ToBool(dr["blnMainMerge"]);
                item.BlnKeyWord = TypeUtil.ToBool(dr["blnKeyWord"]);
                item.LngKeyGroup = TypeUtil.ToInt(dr["lngKeyGroup"]);
                item.BlnConstant = TypeUtil.ToBool(dr["blnConstant"]);
                //item.LngProivtType = TypeUtil.ToInt(dr["lngProivtType"]);
                item.LngProivtType = TypeUtil.ToBool(dr["lngProivtType"]);
                list.Add(item);
            }

            return list;
        }


        /// <summary>
        ///     ȷ��
        /// </summary>
        private void tlbOk_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                if (BlnListEnter)
                {
                    UpdateGridCurrentRow();
                    var lmdDt = GetListMetaDataTable(); //��ȡListMetaData���ݱ�
                    ListDataExVo.BlnSmlSum = chkSmlSum.Checked;
                    ListDataExVo.LngSmlSumType = TypeUtil.ToInt(lookUpSmlSumType.EditValue);
                    var strListSQL = Model.GetSQL(lmdDt);
                    if ((ListDataExVo.StrConTextCondition != null) &&
                        (TypeUtil.ToString(ListDataExVo.StrConTextCondition) != string.Empty))
                    {
                        var strs = ListDataExVo.StrConTextCondition.Split(new[] {"&&&$$$"},
                            StringSplitOptions.RemoveEmptyEntries);
                        if (strs.Length == 2)
                            strListSQL = strListSQL.Replace("where 1=1",
                                "where 1=1 " + " and " + strs[1].Trim().Replace('_', '.'));
                    }
                    ListDataExVo.StrListSQL = strListSQL;
                    ListDisplayExList = OrgListDisplayExData(ListDataExVo.UserID > 0 ? 1 : 0, lmdDt);
                        //��֯ListDisplayEx                    
                }

                BlnOk = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        /// <summary>
        ///     ȡ��
        /// </summary>
        private void tlbCancel_ItemClick(object sender, ItemClickEventArgs e)
        {
            BlnOk = false;
            Close();
        }

        #endregion

        private void barButtonItem1_ItemClick(object sender, ItemClickEventArgs e)
        {
            
                try
            {
                var rows = metadataFieldDt.Select("MetaDataID=" + MetadataId + " and blnDesignSort=1");
                if ((rows == null) || (rows.Length == 0))
                {
                    MessageShowUtil.ShowMsg("Ԫ������û�����ÿɱ��ŵ���Ŀ�����ܽ��б���");
                    return;
                }
                var strFKCode = TypeUtil.ToString(rows[0]["strFkCode"]);
                var frm = new frmDataSortSet();
                frm.MetadataID = MetadataId;
                frm.dtLmd = LmdDt;
                var strFileName =
                    LmdDt.Select("MetaDataFieldID=" + TypeUtil.ToInt(rows[0]["ID"]))[0]["MetaDataFieldName"].ToString();
                frm.strFileName = strFileName;
                var strListDisplayName =
                    LmdDt.Select("MetaDataFieldID=" + TypeUtil.ToInt(rows[0]["ID"]))[0]["strListDisplayFieldNameCHS"]
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

    /// <summary>
    ///     ���б��ֶνṹ
    /// </summary>
    internal struct TreeListField
    {
        public int metaDataId;
        public int parentId;
        public int lngRelativeID;
        public int metaDataFieldId;
        public int lngParentFieldId;
        public int lngRelativeFieldID;
        public string strName;
        public int lngAliasCount;
        public int lngNextAliasCount;
        public string strTableAlias;
        public string strFullPath;
        public string strParentFullPath;
        public string strNextFullPath;
        public string strFieldName;
        public string strFieldChName;
        public string strFieldType;
        public string strFkCode;
        public int lngSourceType;
        public bool blnPK;
        public int lngDataRightType;
        public string strSummaryDisplayFormat;
    }
}