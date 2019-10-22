using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using System.Collections;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraTreeList.Columns;
using DevExpress.XtraGrid.Columns;


namespace Sys.Safety.Reports.Controls
{
    /// <summary>
    /// ͨ�ò���
    /// </summary>
    public partial class frmGenericRef : DevExpress.XtraEditors.XtraForm
    {
        private string strFkCode = string.Empty;
        private string strFkName = string.Empty;
        private string strKeyFieldName = string.Empty;
        private string strDisplayFieldName = string.Empty;
        private string strTreeFkCode = string.Empty;
        private string strTreeFilterField = string.Empty;
        private string strGridFilterField = string.Empty;
        private string _strGridOutFilter = string.Empty;
        private string strCommandParameter = string.Empty;
        private DataTable treeDt = null;
        private DataTable gridDt = null;
        private int lngSelectCount = 0;
        private bool _blnSelectStr = false;
        private string _strSelectValue = string.Empty;
        private string _strSelectDisplay = string.Empty;
        private bool _blnMulti = false;
        private long _selectValue = 0;
        private bool _blnCancelAddData = false;//�Ƿ�ȡ���������ݣ������������
        private IDictionary<object, object> strSelectIDs = new Dictionary<object, object>();//�õ�ѡ�������
        private LookUpEdit _lookup = null;
        private GridLookUpEdit _gridLookup = null;
        private RepositoryItemLookUpEdit _repositoryItemLookup = null;
        private RepositoryItemGridLookUpEdit _repositoryItemGridLookup = null;
        private bool _firstRowIsNull = false;
        private bool _blnOk = false;
        private bool _blnSort = false;//�Ƿ���ѡ������ݽ�������

        /// <summary>
        /// ���������
        /// </summary>
        public PanelControl PointFilter { get; set; }

        /// <summary>
        /// ���ձ���
        /// </summary>
        public string StrFkCode
        {
            get { return strFkCode; }
            set { strFkCode = value; }
        }

        /// <summary>
        /// Grid�ⲿ������˴�
        /// </summary>
        public string StrGridOutFilter
        {
            get { return _strGridOutFilter; }
            set { _strGridOutFilter = value; }
        }

        /// <summary>
        /// ֧��ѡ���¼����
        /// </summary>
        public bool BlnSelectStr
        {
            get { return _blnSelectStr; }
            set { _blnSelectStr = value; }
        }

        /// <summary>
        /// ѡ���¼����
        /// </summary>
        public string StrSelectValue
        {
            get { return _strSelectValue; }
            set { _strSelectValue = value; }
        }

        /// <summary>
        /// ѡ���¼��������
        /// </summary>
        public string StrSelectDisplay
        {
            get { return _strSelectDisplay; }
            set { _strSelectDisplay = value; }
        }

        /// <summary>
        /// ѡ���¼������
        /// </summary>
        public IDictionary<object, object> StrSelectIDs
        {
            get { return strSelectIDs; }
            set { strSelectIDs = value; }
        }

        /// <summary>
        /// �Ƿ�֧�ֶ�ѡ
        /// </summary>
        public bool BlnMulti
        {
            get { return _blnMulti; }
            set { _blnMulti = value; }
        }

        /// <summary>
        /// ��ѡ��ѡ��ֵ��
        /// </summary>
        public long SelectValue
        {
            get { return _selectValue; }
            set { _selectValue = value; }
        }

        /// <summary>
        /// �Ƿ�ȡ���������ݣ������������
        /// </summary>
        public bool BlnCancelAddData
        {
            get { return _blnCancelAddData; }
            set { _blnCancelAddData = value; }
        }

        /// <summary>
        /// �Ƿ�ȷ��
        /// </summary>
        public bool BlnOk
        {
            get { return _blnOk; }
            set { _blnOk = value; }
        }

        /// <summary>
        /// ����GridControl
        /// </summary>
        public DevExpress.XtraGrid.GridControl RefGridControl
        {
            get { return this.gridControl1; }
        }

        /// <summary>
        /// ����GridView
        /// </summary>
        public DevExpress.XtraGrid.Views.Grid.GridView RefGridView
        {
            get { return this.gridView1; }
        }

        public LookUpEdit Lookup
        {
            get { return _lookup; }
            set { _lookup = value; }
        }

        public GridLookUpEdit GridLookup
        {
            get { return _gridLookup; }
            set { _gridLookup = value; }
        }

        public RepositoryItemLookUpEdit RepositoryItemLookup
        {
            get { return _repositoryItemLookup; }
            set { _repositoryItemLookup = value; }
        }

        public RepositoryItemGridLookUpEdit RepositoryItemGridLookup
        {
            get { return _repositoryItemGridLookup; }
            set { _repositoryItemGridLookup = value; }
        }

        public bool FirstRowIsNull
        {
            get { return _firstRowIsNull; }
            set { _firstRowIsNull = value; }
        }

        /// <summary>
        ///�Ƿ���ѡ������ݽ�������(�����ڴӸ߼����õ����Ŀ˳����Ų˵��������Ч) 
        /// </summary>
        public bool BlnSort
        {
            get { return _blnSort; }
            set { _blnSort = value; }
        }


        public frmGenericRef()
        {
            InitializeComponent();
        }

        private void frmGenericRef_Load(object sender, EventArgs e)
        {
            try
            {

                SetToolbarShowState();//���ù�������ʾ״̬

                if (TypeUtil.ToString(strFkCode) == "")
                {
                    ShowMsg("���ձ��벻��Ϊ�գ�", true);
                    dockPanel1.Visibility = DevExpress.XtraBars.Docking.DockVisibility.Hidden;
                    dockPanel1.Width = 0;
                    return;
                }
                bool blnNewData = false;
                Hashtable lookInfo = LookUpUtil.GetlookInfo(strFkCode, ref blnNewData);
                if (lookInfo == null)
                {
                    ShowMsg("���ز���ʧ�ܣ�", true);
                    dockPanel1.Visibility = DevExpress.XtraBars.Docking.DockVisibility.Hidden;
                    dockPanel1.Width = 0;
                    return;
                }

                this.Text = strFkName = TypeUtil.ToString(lookInfo["StrFKName"]);
                strKeyFieldName = TypeUtil.ToString(lookInfo["StrValueMember"]);
                strDisplayFieldName = TypeUtil.ToString(lookInfo["StrDsiplayMember"]);
                strTreeFkCode = TypeUtil.ToString(lookInfo["StrTreeFkCode"]);
                strTreeFilterField = TypeUtil.ToString(lookInfo["StrTreeFilterField"]);
                strGridFilterField = TypeUtil.ToString(lookInfo["StrGridFilterField"]);
                strCommandParameter = TypeUtil.ToString(lookInfo["StrCommandParameter"]);
                if (TypeUtil.ToString(strKeyFieldName) == "" || TypeUtil.ToString(strDisplayFieldName) == "")
                {
                    ShowMsg("���ձ���[" + strFkCode + "]��������", true);
                    return;
                }
                CreateGridColumn(TypeUtil.ToString(lookInfo["StrColumns"]));
                gridDt = lookInfo["dataSource"] as DataTable;
                gridDt = gridDt.Copy();
                if (TypeUtil.ToString(_strGridOutFilter) != "")
                {
                    gridDt.DefaultView.RowFilter = _strGridOutFilter;
                }

                if (this._blnSort)
                {//������Ŀ���ŵ�ʱ���Զ�����֮ǰ�ѱ��ŵ�˳������Ⱥ�˳�� 2015-03-11  
                    this.SetDesginSort();
                }

                // 20170910
                //���ݲ�����������ù��˲��
                if (PointFilter != null)
                {
                    foreach (var item in PointFilter.Controls)
                    {
                        string typeName = item.GetType().Name;
                        if (typeName == "RadioButton")
                        {
                            var control = item as RadioButton;
                            if (control != null && control.Checked)
                            {
                                if (control.Text == "����")
                                {
                                    var dr = gridDt.Select("activity=1");
                                    gridDt = dr.Length != 0 ? dr.CopyToDataTable() : gridDt.Clone();
                                }
                                else if (control.Text == "���Ų��")
                                {
                                    //��ȡ����
                                    foreach (var item2 in PointFilter.Controls)
                                    {
                                        string typeName2 = item2.GetType().Name;
                                        if (typeName2 == "Label")
                                        {
                                            var control2 = item2 as Label;
                                            if (control2.Text != "")
                                            {
                                                var dr = gridDt.Select("point in (" + control2.Text + ")");
                                                gridDt = dr.Length != 0 ? dr.CopyToDataTable() : gridDt.Clone();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                this.gridControl1.DataSource = gridDt;
                if (TypeUtil.ToString(strTreeFkCode) != "" && TypeUtil.ToString(strTreeFkCode.ToLower()) != "null")
                {
                    InitTreeList(strTreeFkCode);
                }
                else
                {
                    dockPanel1.Visibility = DevExpress.XtraBars.Docking.DockVisibility.Hidden;
                    dockPanel1.Width = 0;
                }

                SetGridSelectRow();
                SetGridColumnFormat();
            }
            catch (Exception ex)
            {
                ShowMsg(ex.Message, true);
            }
        }
        
        /// <summary>
        /// ���ù�������ʾ״̬
        /// </summary>
        private void SetToolbarShowState()
        {
            if (!_blnMulti)
            {
                tlbSelAll.Visibility = DevExpress.XtraBars.BarItemVisibility.Never;
                tlbCancelSel.Visibility = DevExpress.XtraBars.BarItemVisibility.Never;
            }
        }

        private void SetGridColumnFormat()
        {
            if (this.gridControl1.DataSource == null)
            {
                return;
            }

            if (TypeUtil.ToString(strCommandParameter) == "" || _blnCancelAddData)
            {
                this.tlbRefresh.Visibility = DevExpress.XtraBars.BarItemVisibility.Never;
                tlbAdd.Visibility = DevExpress.XtraBars.BarItemVisibility.Never;
            }
            else
            {
                this.tlbRefresh.Visibility = DevExpress.XtraBars.BarItemVisibility.Always;
                tlbAdd.Visibility = DevExpress.XtraBars.BarItemVisibility.Always;
            }
        }

        private void InitTreeList(string strTreeFkCode)
        {
            try
            {
                bool blnNewData = false;
                Hashtable lookInfo = LookUpUtil.GetlookInfo(strTreeFkCode, ref blnNewData);
                if (lookInfo == null)
                {
                    throw new Exception("�������ؼ�ʧ�ܣ�");
                }

                dockPanel1.Text = TypeUtil.ToString(lookInfo["StrFKName"]);
                treeList1.KeyFieldName = TypeUtil.ToString(lookInfo["StrValueMember"]).Trim();
                treeList1.ParentFieldName = TypeUtil.ToString(lookInfo["StrParentField"]).Trim();
                CreateTreeColumn(TypeUtil.ToString(lookInfo["StrColumns"]));

                treeDt = lookInfo["dataSource"] as DataTable;
                treeDt = treeDt.Copy();
                DataRow dr = treeDt.NewRow();
                dr[treeList1.KeyFieldName] = 0;
                dr[treeList1.ParentFieldName] = -1;
                dr[strTreeFilterField] = "ȫ��";
                dr[TypeUtil.ToString(lookInfo["StrDsiplayMember"])] = "ȫ��";
                treeDt.Rows.InsertAt(dr, 0);

                treeList1.DataSource = treeDt;
                treeList1.ExpandAll();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void CreateTreeColumn(string strColumns)
        {
            strColumns = strColumns.Trim();
            string[] fieldName = strColumns.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            int filedCount = fieldName.Length;
            int width = 0;
            int visibleIndex = 0;
            TreeListColumn treeCol = null;
            for (int i = 0; i < filedCount; i++)
            {
                string[] subFieldName = fieldName[i].Split(',');
                treeCol = this.treeList1.Columns.Add();
                treeCol.Name = "col" + subFieldName[0];
                treeCol.FieldName = subFieldName[0];
                treeCol.Caption = subFieldName[1];
                treeCol.OptionsColumn.ShowInCustomizationForm = true;
                treeCol.OptionsColumn.AllowFocus = false;
                width = TypeUtil.ToInt(subFieldName[2]);
                if (width > 0)
                {
                    treeCol.Width = width;
                    treeCol.VisibleIndex = visibleIndex++;
                    treeCol.Visible = true;
                }
                else
                {
                    treeCol.VisibleIndex = -1;
                    treeCol.Visible = false;
                }
            }
        }

        private void CreateGridColumn(string strColumns)
        {
            strColumns = strColumns.Trim();
            string[] fieldName = strColumns.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            int filedCount = fieldName.Length;
            int width = 0;
            int visibleIndex = 0;
            GridColumn gridCol = null;
            for (int i = 0; i < filedCount; i++)
            {
                string[] subFieldName = fieldName[i].Split(',');
                gridCol = new GridColumn();
                gridCol.Name = "col" + subFieldName[0];
                gridCol.FieldName = subFieldName[0];
                gridCol.Caption = subFieldName[1];
                gridCol.OptionsColumn.ShowInCustomizationForm = true;
                gridCol.OptionsColumn.AllowFocus = false;
                width = TypeUtil.ToInt(subFieldName[2]);
                if (width > 0)
                {
                    gridCol.Width = width;
                    gridCol.VisibleIndex = visibleIndex++;
                    gridCol.Visible = true;
                }
                else
                {
                    gridCol.VisibleIndex = -1;
                    gridCol.Visible = false;
                }

                this.gridView1.Columns.Add(gridCol);
            }
        }

        private void SetGridSelectRow()
        {
            if (gridDt != null)
            {
                DataColumn dc = new DataColumn("BlnSelect", typeof(bool));
                dc.DefaultValue = false;
                gridDt.Columns.Add(dc);

                this.gridView1.CellValueChanged -= new DevExpress.XtraGrid.Views.Base.CellValueChangedEventHandler(gridView1_CellValueChanged);

                if (_blnSelectStr)
                {
                    SetGridSelectRowBySelectStr();
                    this.gridView1.CellValueChanged += new DevExpress.XtraGrid.Views.Base.CellValueChangedEventHandler(gridView1_CellValueChanged);
                    return;
                }

                object id = 0;
                int lngCount = 0;
                for (int i = 0; i < this.gridView1.RowCount; i++)
                {
                    id = gridView1.GetRowCellValue(i, strKeyFieldName);
                    if (strSelectIDs.ContainsKey(id))
                    {
                        lngCount++;
                        gridView1.SetRowCellValue(i, colSelect, true);
                    }
                    else
                    {
                        gridView1.SetRowCellValue(i, colSelect, false);
                    }
                }

                ShowMsg("���ۼ�ѡ����" + lngCount + "�����ݣ�");

                this.gridView1.CellValueChanged += new DevExpress.XtraGrid.Views.Base.CellValueChangedEventHandler(gridView1_CellValueChanged);
            }
        }

        private void SetGridSelectRowBySelectStr()
        {
            int lngCount = 0;
            string strRows = this._strSelectValue.Replace("'", "");
            string[] temp = strRows.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            int count = temp.Length;
            int rowCount = this.gridView1.RowCount;
            if (rowCount <= 0)
            {
                return;
            }

            IList<object> selectList = new List<object>();
            for (int i = 0; i < count; i++)
            {
                selectList.Add(temp[i]);
            }

            object id = 0;
            object name = "";
            for (int i = 0; i < this.gridView1.RowCount; i++)
            {
                id = gridView1.GetRowCellValue(i, strKeyFieldName);
                name = gridView1.GetRowCellValue(i, strDisplayFieldName);
                if (selectList.Contains(id.ToString()))
                {
                    lngCount++;
                    gridView1.SetRowCellValue(i, colSelect, true);

                    if (!strSelectIDs.ContainsKey(id))
                    {
                        strSelectIDs.Add(id, name);
                    }
                }
                else
                {
                    gridView1.SetRowCellValue(i, colSelect, false);
                }
            }

            ShowMsg("���ۼ�ѡ����" + lngCount + "�����ݣ�");
        }

        private void treeList1_FocusedNodeChanged(object sender, DevExpress.XtraTreeList.FocusedNodeChangedEventArgs e)
        {
            try
            {
                string filter = TypeUtil.ToString(e.Node.GetValue(strTreeFilterField));
                if (filter.Equals("ȫ��"))
                {
                    gridView1.ActiveFilterString = "";
                }
                else
                {
                    string strFilter = "[" + strGridFilterField + "] = '" + filter + "'";
                    if (e.Node.HasChildren)
                    {
                        strFilter = "[" + strGridFilterField + "] like '" + filter + "-%' or [" + strGridFilterField + "] = '" + filter + "'";
                    }
                    gridView1.ActiveFilterString = strFilter;
                }
            }
            catch (Exception ex)
            {
                ShowMsg(ex.Message, true);
            }
        }

        private void txtSearch_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            try
            {
                string strFilterCondition = " like '%" + txtSearch.Text + "%'";
                string strFilter = "";
                int colCount = gridView1.Columns.Count;
                string strColTypeName = string.Empty;
                for (int i = 0; i < colCount; i++)
                {
                    strColTypeName = gridView1.Columns[i].ColumnType.Name.ToLower();
                    if (!gridView1.Columns[i].Visible)
                    {
                        continue;
                    }
                    if (strColTypeName == "boolean")
                    {
                        continue;
                    }
                    try
                    {
                        if (strColTypeName == "datetime")
                        {
                            strFilter += " or substring(" + gridView1.Columns[i].FieldName + ",10)" + strFilterCondition;
                        }
                        else
                        {
                            strFilter += " or [" + gridView1.Columns[i].FieldName + "]" + strFilterCondition;
                        }
                    }
                    catch { }
                }
                if (strFilter.Length > 4)
                {
                    strFilter = strFilter.Substring(4);
                }
                gridView1.ActiveFilterString = strFilter;

                treeList1.FocusedNodeChanged -= new DevExpress.XtraTreeList.FocusedNodeChangedEventHandler(this.treeList1_FocusedNodeChanged);

                DevExpress.XtraTreeList.Nodes.TreeListNode node = treeList1.FindNodeByKeyID(0);
                treeList1.FocusedNode = node;

                treeList1.FocusedNodeChanged += new DevExpress.XtraTreeList.FocusedNodeChangedEventHandler(this.treeList1_FocusedNodeChanged);
            }
            catch (Exception ex)
            {
                ShowMsg(ex.Message, true);
            }
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                txtSearch_ButtonClick(null, null);
            }
        }

        private void chkSelect_EditValueChanged(object sender, System.EventArgs e)
        {
            this.gridView1.CloseEditor();
            this.gridView1.UpdateCurrentRow();
        }

        private void gridView1_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            int rowHandle = e.RowHandle;
            // 20170619
            //int id = TypeUtil.ToInt(gridView1.GetRowCellValue(rowHandle, strKeyFieldName));
            //var id =Convert.ToInt64(gridView1.GetRowCellValue(rowHandle, strKeyFieldName));
            bool blnSelect = TypeUtil.ToBool(gridView1.GetRowCellValue(rowHandle, colSelect));
            SelectCancelRecord(rowHandle, blnSelect);
            if (!_blnMulti)
            {   //��ѡ

                if (blnSelect)
                {
                    //var id = Convert.ToInt64(gridView1.GetRowCellValue(rowHandle, strKeyFieldName));
                    long id;
                    Int64.TryParse(gridView1.GetRowCellValue(rowHandle, strKeyFieldName).ToString(), out id);
                    _selectValue = id;
                    SelectMutualExclusion(id);
                }
                else
                {
                    _selectValue = 0;
                }
            }

            ShowMsg("���ۼ�ѡ����" + strSelectIDs.Count + "�����ݣ�");
        }

        /// <summary>
        /// ѡ�񻥳⴦��
        /// </summary>
        /// <param name="curSelRowHandle">��ǰѡ����</param>
        private void SelectMutualExclusion(long curSelRowHandle)
        {
            //int rowCount = gridView1.RowCount;
            int rowCount = gridDt.Rows.Count;
            this.gridView1.CellValueChanged -= new DevExpress.XtraGrid.Views.Base.CellValueChangedEventHandler(gridView1_CellValueChanged);
            for (int i = 0; i < rowCount; i++)
            {
                //if (curSelRowHandle == TypeUtil.ToInt(gridDt.Rows[i][strKeyFieldName]))
                if (curSelRowHandle == Convert.ToInt64(gridDt.Rows[i][strKeyFieldName].ToString()))
                {
                    continue;
                }

                //ȡ��������¼ѡ��         
                gridDt.Rows[i]["BlnSelect"] = false;
                SelectCancelRecord(i, false);
            }

            this.gridView1.CellValueChanged += new DevExpress.XtraGrid.Views.Base.CellValueChangedEventHandler(gridView1_CellValueChanged);
        }


        private void tlbMultiSelection_CheckedChanged(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            int rowCount = this.gridView1.RowCount;
            bool rowChecked = this.tlbMultiSelection.Checked;
            this.gridView1.CellValueChanged -= new DevExpress.XtraGrid.Views.Base.CellValueChangedEventHandler(gridView1_CellValueChanged);
            for (int i = 0; i < rowCount; i++)
            {
                this.gridView1.SetRowCellValue(i, colSelect, rowChecked);
                SelectCancelRecord(i, rowChecked);
            }
            this.gridView1.CellValueChanged += new DevExpress.XtraGrid.Views.Base.CellValueChangedEventHandler(gridView1_CellValueChanged);
            gridView1.CloseEditor();
            gridView1.UpdateCurrentRow();
            ShowMsg("���ۼ�ѡ����" + strSelectIDs.Count + "�����ݣ�");
        }



        private void tlbSelAll_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            SelectAll(true);
        }

        private void tlbCancelSel_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            SelectAll(false);
        }

        private void SelectAll(bool blnSelect)
        {



            int rowCount = gridView1.RowCount;
            this.gridView1.CellValueChanged -= new DevExpress.XtraGrid.Views.Base.CellValueChangedEventHandler(gridView1_CellValueChanged);
            for (int i = 0; i < rowCount; i++)
            {
                gridView1.SetRowCellValue(i, colSelect, blnSelect);
                SelectCancelRecord(i, blnSelect);
            }

            this.gridView1.CellValueChanged += new DevExpress.XtraGrid.Views.Base.CellValueChangedEventHandler(gridView1_CellValueChanged);
            gridView1.CloseEditor();
            gridView1.UpdateCurrentRow();
            ShowMsg("���ۼ�ѡ����" + strSelectIDs.Count + "�����ݣ�");
        }

        private void SelectCancelRecord(int rowHandle, bool blnSelect)
        {
            object id = gridView1.GetRowCellValue(rowHandle, strKeyFieldName);
            object name = gridView1.GetRowCellValue(rowHandle, strDisplayFieldName);
            if (blnSelect)
            {
                if (!strSelectIDs.ContainsKey(id))
                {
                    lngSelectCount++;
                    strSelectIDs.Add(id, name);
                }
            }
            else
            {
                if (strSelectIDs.ContainsKey(id))
                {
                    strSelectIDs.Remove(id);
                    lngSelectCount--;
                }
            }
        }

        /// <summary>
        /// ��ȡѡ��������ʾ��
        /// </summary>
        private string GetSelectedRowCHSString()
        {
            this.gridView1.CloseEditor();
            int rowCount = this.gridDt.Rows.Count;
            bool selectFlag = false;
            string result = "";
            string returnValue = "";
            for (int i = 0; i < rowCount; i++)
            {
                selectFlag = TypeUtil.ToBool(this.gridDt.Rows[i][colSelect.FieldName]);
                if (selectFlag)
                {
                    returnValue = TypeUtil.ToString(this.gridDt.Rows[i][strDisplayFieldName]);
                    result = result + returnValue + ",";
                }

            }

            if (result != string.Empty)
            {
                result = result.Remove(result.Length - 1);
            }
            return result;
        }

        /// <summary>
        /// ��ȡѡ������ֵ��
        /// </summary>
        private string GetSelectedRowString()
        {
            this.gridView1.CloseEditor();
            int rowCount = this.gridDt.Rows.Count;
            bool selectFlag = false;
            string result = "";
            string returnValue = "";
            for (int i = 0; i < rowCount; i++)
            {
                selectFlag = TypeUtil.ToBool(this.gridDt.Rows[i][colSelect.FieldName]);
                if (selectFlag)
                {
                    returnValue = TypeUtil.ToString(this.gridDt.Rows[i][strKeyFieldName]);
                    result = result + "'" + returnValue + "'" + ",";
                }
            }
            if (result != string.Empty)
            {
                result = result.Remove(result.Length - 1);
            }
            return result;
        }

        private void tlbOK_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (_blnSelectStr)
            {
                _strSelectValue = GetSelectedRowString();
                _strSelectDisplay = GetSelectedRowCHSString();
            }

            this.BlnOk = true;
            this.Close();
        }

        private void tlbCancel_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            this.BlnOk = false;
            this.Close();
        }

        //private void gridControl1_DoubleClick(object sender, EventArgs e)
        //{
        //    int id = TypeUtil.ToInt(gridView1.GetRowCellValue(gridView1.FocusedRowHandle, strKeyFieldName));
        //    object name = gridView1.GetRowCellValue(gridView1.FocusedRowHandle, strDisplayFieldName);
        //    if (id > 0)
        //    {
        //        if (_blnSelectStr)
        //        {
        //            _strSelectValue = "'" + id + "'";
        //            _strSelectDisplay = TypeUtil.ToString(this.gridView1.GetRowCellValue(gridView1.FocusedRowHandle, strDisplayFieldName));
        //        }

        //        _selectValue = id;
        //        strSelectIDs.Clear();
        //        strSelectIDs.Add(id, name);
        //        this.BlnOk = true;
        //        this.Close();
        //    }
        //    else
        //    {
        //        ShowMsg("��ѡ����Ч��¼��");
        //    }
        //}

        private void gridView1_CustomDrawRowIndicator(object sender, DevExpress.XtraGrid.Views.Grid.RowIndicatorCustomDrawEventArgs e)
        {
            int rowhandle = e.RowHandle + 1;
            if (e.Info.IsRowIndicator && rowhandle > 0)
            {
                e.Info.DisplayText = rowhandle.ToString();
            }
        }

        private void gridViewSelect_CustomDrawRowIndicator(object sender, DevExpress.XtraGrid.Views.Grid.RowIndicatorCustomDrawEventArgs e)
        {
            int rowhandle = e.RowHandle + 1;
            if (e.Info.IsRowIndicator && rowhandle > 0)
            {
                e.Info.DisplayText = rowhandle.ToString();
            }
        }

        /// <summary>
        /// ��ʾ��Ϣ
        /// </summary>
        /// <param name="caption">��Ϣ��</param>
        private void ShowMsg(string caption)
        {
            barStaticItemMsg.Caption = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + caption;
        }

        /// <summary>
        /// ��ʾ��Ϣ
        /// </summary>
        /// <param name="caption">��Ϣ��</param>
        private void ShowMsg(string caption, bool isMsg)
        {
            barStaticItemMsg.Caption = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + caption;
            if (isMsg) MessageBox.Show(caption, "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        /// <summary>
        /// ����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tlbAdd_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            //try
            //{
            //    if (strCommandParameter != string.Empty)
            //    {
            //        RequestUtil.ExcuteCommand("Card", strCommandParameter);
            //    }
            //}
            //catch (Exception ex) { ShowMsg(ex.Message, true); }
        }
        /// <summary>
        /// ˢ��
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tlbRefresh_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                if (_blnSelectStr)
                {
                    _strSelectValue = GetSelectedRowString();
                    _strSelectDisplay = GetSelectedRowCHSString();
                }

                if (TypeUtil.ToString(strFkCode) == "")
                {
                    ShowMsg("���ձ��벻��Ϊ�գ�", true);
                    return;
                }

                bool blnNewData = false;
                Hashtable lookInfo = LookUpUtil.GetlookInfo(strFkCode, ref blnNewData);
                if (lookInfo == null)
                {
                    ShowMsg("���ز���ʧ�ܣ�", true);
                    return;
                }

                this.Text = strFkName ="ѡ��"+ TypeUtil.ToString(lookInfo["StrFKName"]);
                strKeyFieldName = TypeUtil.ToString(lookInfo["StrValueMember"]);
                strDisplayFieldName = TypeUtil.ToString(lookInfo["StrDsiplayMember"]);
                strTreeFkCode = TypeUtil.ToString(lookInfo["StrTreeFkCode"]);
                strTreeFilterField = TypeUtil.ToString(lookInfo["StrTreeFilterField"]);
                strGridFilterField = TypeUtil.ToString(lookInfo["StrGridFilterField"]);
                strCommandParameter = TypeUtil.ToString(lookInfo["StrCommandParameter"]);
                if (TypeUtil.ToString(strKeyFieldName) == "" || TypeUtil.ToString(strDisplayFieldName) == "")
                {
                    ShowMsg("���ձ���[" + strFkCode + "]��������", true);
                    return;
                }

                gridDt = lookInfo["dataSource"] as DataTable;
                gridDt = gridDt.Copy();
                if (TypeUtil.ToString(_strGridOutFilter) != "")
                {
                    gridDt.DefaultView.RowFilter = _strGridOutFilter;
                }
                this.gridControl1.DataSource = gridDt;
                if (TypeUtil.ToString(strTreeFkCode) != "")
                {
                    lookInfo = LookUpUtil.GetlookInfo(strTreeFkCode, ref blnNewData);
                    if (lookInfo == null)
                    {
                        throw new Exception("�������ؼ�ʧ�ܣ�");
                    }

                    dockPanel1.Text = TypeUtil.ToString(lookInfo["StrFKName"]);
                    treeList1.KeyFieldName = TypeUtil.ToString(lookInfo["StrValueMember"]).Trim();
                    treeList1.ParentFieldName = TypeUtil.ToString(lookInfo["StrParentField"]).Trim();

                    treeDt = lookInfo["dataSource"] as DataTable;
                    treeDt = treeDt.Copy();
                    DataRow dr = treeDt.NewRow();
                    dr[treeList1.KeyFieldName] = 0;
                    dr[treeList1.ParentFieldName] = -1;
                    dr[strTreeFilterField] = "ȫ��";
                    dr[TypeUtil.ToString(lookInfo["StrDsiplayMember"])] = "ȫ��";
                    treeDt.Rows.InsertAt(dr, 0);

                    treeList1.DataSource = treeDt;
                    treeList1.ExpandAll();
                }

                SetGridSelectRow();
                SetGridColumnFormat();

                txtSearch_ButtonClick(null, null);

                if (_lookup != null)
                {
                    LookUpUtil.RefreshLookUpDataSource(strFkCode, _lookup, _firstRowIsNull);
                }
                else if (_gridLookup != null)
                {
                    LookUpUtil.RefreshGridLookUpDataSource(strFkCode, _gridLookup, _firstRowIsNull);
                }
                else if (_repositoryItemLookup != null)
                {
                    LookUpUtil.RefreshLookUpDataSource(strFkCode, _repositoryItemLookup, _firstRowIsNull);
                }
                else if (_repositoryItemGridLookup != null)
                {
                    LookUpUtil.RefreshGridLookUpDataSource(strFkCode, _repositoryItemGridLookup, _firstRowIsNull);
                }
            }
            catch (Exception ex)
            {
                ShowMsg(ex.Message, true);
            }
        }


        /// <summary>
        /// //������Ŀ���ŵ�ʱ���Զ�����֮ǰ�ѱ��ŵ�˳������Ⱥ�˳��(2015-03-11)
        /// </summary>
        private void SetDesginSort()
        {
            int index = 0;
            string[] str = this._strSelectValue.Split(',');
            foreach (string s in str)
            {
                if (TypeUtil.ToString(s) == "") continue;
                DataRow[] rowss = gridDt.Select(strKeyFieldName + "=" + s + "");
                foreach (DataRow row in rowss)
                {
                    DataRow copyDr = this.gridDt.NewRow();
                    foreach (DataColumn dc in this.gridDt.Columns)
                    {
                        copyDr[dc.ColumnName] = row[dc.ColumnName];
                    }
                    gridDt.Rows.Remove(row);
                    gridDt.Rows.InsertAt(copyDr, index);
                    index++;
                }

            }


        }
        private void frmGenericRef_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
        {

        }




    }
}