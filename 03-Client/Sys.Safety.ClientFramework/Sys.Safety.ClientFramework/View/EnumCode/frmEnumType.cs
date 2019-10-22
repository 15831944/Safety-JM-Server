using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraBars;
using Sys.Safety.ClientFramework.Model;
using DevExpress.XtraTreeList.Nodes;
using Sys.Safety.DataContract;
using Basic.Framework.Common;
using Basic.Framework.Web;

namespace Sys.Safety.ClientFramework.View.EnumCode
{
    public partial class frmEnumType : DevExpress.XtraEditors.XtraForm
    {

        private int EnumTypeID = 0;
        private int EnumCodeID = 0;
        private EnumModel Model = new EnumModel();
        private EnumcodeInfo enumCodeVO = null;
        private int GridIsChange = 0;
        private IList<int> delDetailItems = new List<int>();
        private DevExpress.Utils.WaitDialogForm frmWait;
        private int TypeID = 0;//�ڵ�ID(EnumTypeID)


        public frmEnumType()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Load
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frmEnumType_Load(object sender, EventArgs e)
        {


            gridViewEnumType.IndicatorWidth = 35;
            enumCodeVO = new EnumcodeInfo();
            LoadDate();

            //gridViewEnumType.OptionsView.NewItemRowPosition = NewItemRowPosition.None;
        }

        /// <summary>
        /// ʵ�ֵȴ�����
        /// </summary>
        /// <param name="caption"></param>
        private void ShowWaitForm(string caption)
        {
            CloseWaitForm();
            if (frmWait == null || frmWait.IsDisposed)
            {
                frmWait = new DevExpress.Utils.WaitDialogForm(caption + "��...", "��ȴ�...");
            }
            this.Cursor = Cursors.WaitCursor;
        }
        /// <summary>
        /// ���صȴ�����
        /// </summary>
        private void CloseWaitForm()
        {
            if (frmWait != null)
                frmWait.Close();
            this.Cursor = Cursors.Default;
        }
        /// <summary>
        /// ����TreeList
        /// </summary>
        private void LoadTreeList()
        {
            treeList.FocusedNodeChanged -= new DevExpress.XtraTreeList.FocusedNodeChangedEventHandler(treeList_FocusedNodeChanged);
            treeList.DataSource = Model.EnumTypeList();
            treeList.ExpandAll();
            TreeListNode currNode = treeList.FindNodeByFieldValue("EnumTypeID", TypeID);
            if (currNode != null) treeList.SetFocusedNode(currNode);
            else treeList.SetFocusedNode(null);
            treeList.FocusedNodeChanged += new DevExpress.XtraTreeList.FocusedNodeChangedEventHandler(treeList_FocusedNodeChanged);
        }
        /// <summary>
        /// ����DataTable
        /// </summary>
        private void CreateDataTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("EnumCodeID", Type.GetType("System.Int32"));
            dt.Columns.Add("EnumTypeID", Type.GetType("System.Int32"));
            dt.Columns.Add("StrEnumDisplay", Type.GetType("System.String"));
            dt.Columns.Add("LngEnumValue", Type.GetType("System.Int32"));
            dt.Columns.Add("LngEnumValue1", Type.GetType("System.Int32"));
            dt.Columns.Add("LngRowIndex", Type.GetType("System.Int32"));
            dt.Columns.Add("LngEnumValue2", Type.GetType("System.Decimal"));
            dt.Columns.Add("LngEnumValue3", Type.GetType("System.String"));
            dt.Columns.Add("LngEnumValue4", Type.GetType("System.String"));
            dt.Columns.Add("StrDescription", Type.GetType("System.String"));
            dt.Columns.Add("BlnDefault", Type.GetType("System.Boolean"));
            dt.Columns.Add("BlnPredefined", Type.GetType("System.Boolean"));
            dt.Columns.Add("BlnEnable", Type.GetType("System.Boolean"));
            dt.Columns.Add("SelectCheck", Type.GetType("System.Boolean"));
            dt.Columns.Add("DetailDelete", Type.GetType("System.String"));
        }
        /// <summary>
        /// ��������
        /// </summary>
        private void LoadDate()
        {
            CreateDataTable();
            GridIsChange = 0;
            LoadTreeList();
            gridViewEnumType.FocusedRowChanged -= new DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventHandler(gridViewEnumType_FocusedRowChanged);
            gridControl1.DataSource = Model.GetEnumCodeByEnumTypeID(TypeID);
            SetLngEnumValue();
            gridViewEnumType.FocusedRowChanged += new DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventHandler(gridViewEnumType_FocusedRowChanged);
        }
        private void SetLngEnumValue()
        {
            DataTable dt = Model.GetEnumCodeByEnumTypeID(TypeID);
            if (dt != null && dt.Rows.Count > 0)
            {
                EnumtypeInfo type = new EnumtypeInfo();
                type = Model.GetEnumTypeByID(TypeID);
                if (type != null && int.Parse(type.EnumTypeID) > 0)
                {
                    if (type.BlnEnumValue1  ? true : false)
                        DatelngEnumValue1.ReadOnly = true;
                    else
                        DatelngEnumValue1.ReadOnly = false;
                    if (type.BlnEnumValue2  ? true : false)
                        CalcLngEnumValue2.ReadOnly = true;
                    else
                        CalcLngEnumValue2.ReadOnly = false;
                    if (type.BlnEnumValue3  ? true : false)
                        TextLngEnumValue3.ReadOnly = true;
                    else
                        TextLngEnumValue3.ReadOnly = false;
                }
            }
        }
        /// <summary>
        /// ��������Ƿ�ı�
        /// </summary>
        /// <returns></returns>
        private bool VOIsChanged()
        {
            if (GridIsChange == 1)
                return true;
            return false;
        }
        /// <summary>
        /// ��������
        /// </summary>
        private void SaveData()
        {
            gridViewEnumType.CloseEditor();
            gridViewEnumType.UpdateCurrentRow();
            EnumcodeInfo enumcode = null;
            int Id;
            for (int i = 0; i < gridViewEnumType.RowCount; i++)
            {
                enumcode = new EnumcodeInfo();
                Id = TypeConvert.ToInt(gridViewEnumType.GetRowCellValue(i, "EnumCodeID"));
                if (Id == 0)
                {
                    enumcode.InfoState = InfoState.AddNew;

                }
                else
                {
                    enumcode.InfoState = InfoState.Modified;

                }
                enumcode.EnumCodeID = Id.ToString();
                enumcode.EnumTypeID = TypeID.ToString();
                enumcode.BlnEnable = TypeConvert.ToBool(gridViewEnumType.GetRowCellValue(i, "BlnEnable")) ? "1" : "0";
                enumcode.BlnDefault = TypeConvert.ToBool(gridViewEnumType.GetRowCellValue(i, "BlnDefault")) ? "1" : "0";
                enumcode.BlnPredefined = TypeConvert.ToBool(gridViewEnumType.GetRowCellValue(i, "BlnPredefined")) ? "1" : "0";
                enumcode.LngEnumValue = TypeConvert.ToInt(gridViewEnumType.GetRowCellValue(i, "LngEnumValue"));
                enumcode.LngEnumValue1 = TypeConvert.ToDateTime(gridViewEnumType.GetRowCellValue(i, "LngEnumValue1"));
                enumcode.LngEnumValue2 = TypeConvert.ToDecimal(gridViewEnumType.GetRowCellValue(i, "LngEnumValue2"));
                enumcode.LngEnumValue3 = TypeConvert.ToString(gridViewEnumType.GetRowCellValue(i, "LngEnumValue3"));
                enumcode.LngEnumValue4 = TypeConvert.ToString(gridViewEnumType.GetRowCellValue(i, "LngEnumValue4"));
                enumcode.LngRowIndex = TypeConvert.ToInt(gridViewEnumType.GetRowCellValue(i, "LngRowIndex"));
                enumcode.StrEnumDisplay = TypeConvert.ToString(gridViewEnumType.GetRowCellValue(i, "StrEnumDisplay"));
                enumcode.StrDescription = TypeConvert.ToString(gridViewEnumType.GetRowCellValue(i, "StrDescription"));
                Model.SaveEnumCode(enumcode);
            }
        }
        /// <summary>
        /// ��������ö��ֵ��Ȼ���ۼ�
        /// </summary>
        /// <param name="IsRead"></param>
        private int GetMaxEnumValue()
        {
            List<int> list = new List<int>();
            for (int i = 0; i < gridViewEnumType.RowCount; i++)
            {

                int lngEnumValue = TypeConvert.ToInt(gridViewEnumType.GetRowCellValue(i, "LngEnumValue"));
                list.Add(lngEnumValue);
            }
            list.Sort();
            int MaxNum = 0;
            if (list.Count > 0)
                MaxNum = list[list.Count - 1];
            return MaxNum;
        }
        /// <summary>
        /// ���浥�ݷ���
        /// </summary>
        /// <param name="Caption"></param>
        private void Save(string Caption)
        {
            ShowWaitForm(Caption);
            SaveData();
            foreach (int id in delDetailItems)
            {
                EnumcodeInfo code = Model.GetEnumCodeByID(id);
                if (code != null && long.Parse(code.EnumCodeID) > 0)
                    Model.DeleteEnumCode(code);
            }

            Model.UpdateCache();
            LoadDate();
            CloseWaitForm();
            StaticMsg.Caption = DateTime.Now.ToString() + Caption + "�ɹ�,�������ķ�����Ч����";
        }
        /// <summary>
        /// ״̬����ʾ��Ϣ
        /// </summary>
        /// <param name="Caption"></param>
        private void StaticCaption(string Caption)
        {
            StaticMsg.Caption = DateTime.Now.ToString() + "  " + Caption;
        }
        /// <summary>
        /// �վ����ݣ�����ɾ��ö������
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private EnumtypeInfo GetData()
        {
            TreeListNode node = null;
            EnumtypeInfo type = new EnumtypeInfo();
            node = treeList.FindNodeByKeyID(TypeID.ToString());
            type.InfoState = InfoState.Delete;
            if (node != null)
            {
                type.ID = node.GetValue("ID").ToString();
                type.EnumTypeID = node.GetValue("EnumTypeID").ToString();
                type.StrCode = TypeConvert.ToString(node.GetValue("StrCode"));
                type.StrName = TypeConvert.ToString(node.GetValue("StrName"));
            }
            return type;

        }
        private void lngEnumValueExist()
        {
            try
            {
                gridViewEnumType.CloseEditor();
                gridViewEnumType.UpdateCurrentRow();
                for (int i = 0; i < gridViewEnumType.RowCount; i++)
                {
                    for (int j = i + 1; j < gridViewEnumType.RowCount; j++)
                    {
                        string strEnumDisplayi = TypeConvert.ToString(gridViewEnumType.GetRowCellValue(i, "StrEnumDisplay"));
                        string strEnumDisplayj = TypeConvert.ToString(gridViewEnumType.GetRowCellValue(j, "StrEnumDisplay"));
                        {
                            if (i != j && strEnumDisplayi == strEnumDisplayj)
                            {

                                throw new Exception("��ǰö����ʾֵ���ظ������������룡");
                                StaticMsg.Caption = DateTime.Now.ToString() + "��ǰö����ʾֵ���ظ������������룡";
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Model.ShowMessageBox(ex.Message, 3, 1, StaticMsg);
            }
            finally { }
        }
        /// <summary>
        /// ������ݺϷ���
        /// </summary>
        /// <returns></returns>
        private bool Validate()
        {
            gridViewEnumType.CloseEditor();
            gridViewEnumType.UpdateCurrentRow();
            int rowHand = gridViewEnumType.FocusedRowHandle;

            for (int i = 0; i < gridViewEnumType.RowCount; i++)
            {
                string strEnumDisplayi = TypeConvert.ToString(gridViewEnumType.GetRowCellValue(i, "StrEnumDisplay"));
                int lngEnumValuei = TypeConvert.ToInt(gridViewEnumType.GetRowCellValue(i, "LngEnumValue"));
                if (lngEnumValuei < 0)
                {
                    Model.ShowMessageBox("ö��ֵ����Ϊ��", 3, 1, StaticMsg);
                    return false;
                }
                if (strEnumDisplayi == "")
                {
                    Model.ShowMessageBox("ö����ʾֵ����Ϊ��", 3, 1, StaticMsg);
                    return false;
                }
            }
            for (int i = 0; i < gridViewEnumType.RowCount; i++)
            {
                for (int j = i + 1; j < gridViewEnumType.RowCount; j++)
                {
                    string strEnumDisplayi = TypeConvert.ToString(gridViewEnumType.GetRowCellValue(i, "LngEnumValue"));
                    string strEnumDisplayj = TypeConvert.ToString(gridViewEnumType.GetRowCellValue(j, "LngEnumValue"));
                    {
                        if (i != j && strEnumDisplayi == strEnumDisplayj)
                        {

                            Model.ShowMessageBox("����¼����ͬ��ö��ֵ�����������룡", 3, 1, StaticMsg);
                            return false;
                        }
                    }
                }

            }
            return true;

        }
        /// <summary>
        /// ����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSave_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {

            try
            {
                if (Validate())
                    Save("��������");
            }
            catch (Exception ex)
            {
                DevExpress.XtraEditors.XtraMessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Information);
                StaticCaption(ex.Message);
            }
            finally { CloseWaitForm(); }
        }
        /// <summary>
        /// �ر�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancle_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            this.Close();
        }
        /// <summary>
        /// �ر� �ж������Ƿ����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frmEnumType_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (VOIsChanged())
                {
                    DialogResult dr = DevExpress.XtraEditors.XtraMessageBox.Show("��ǰ�����ѱ��޸ģ��Ƿ񱣴棿", "��ʾ", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information);
                    if (dr == DialogResult.Yes)
                    {
                        if (Validate())
                        {
                            Save("��������");
                            e.Cancel = false;
                        }
                    }
                    else if (dr == DialogResult.Cancel)
                        e.Cancel = true;
                    else e.Cancel = false;
                }
                else e.Cancel = false;

            }
            catch (Exception ex)
            {
                DevExpress.XtraEditors.XtraMessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Information);
                StaticCaption(ex.Message);
            }
            finally { }
        }
        /// <summary>
        /// Grid�ı�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gridViewEnumType_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            try
            {

                gridViewEnumType.CellValueChanged -= new DevExpress.XtraGrid.Views.Base.CellValueChangedEventHandler(gridViewEnumType_CellValueChanged);

                GridIsChange = 1;
                int Id = TypeConvert.ToInt(gridViewEnumType.GetRowCellValue(gridViewEnumType.FocusedRowHandle, "EnumCodeID"));
                string EnumValue = TypeConvert.ToString(gridViewEnumType.GetRowCellValue(gridViewEnumType.FocusedRowHandle, "LngEnumValue"));
                if (Id == 0 && EnumValue == "")
                {
                    int lngEnumValue = GetMaxEnumValue() + 1;//�õ���ǰ���͵����ö��ֵȻ���ۼ�
                    gridViewEnumType.SetRowCellValue(gridViewEnumType.FocusedRowHandle, "LngEnumValue", lngEnumValue);//��ö��ֵ��ֵ
                    gridViewEnumType.SetRowCellValue(gridViewEnumType.FocusedRowHandle, "BlnEnable", true);//��ö��ֵ��ֵ
                }
                lngEnumValueExist();
                gridViewEnumType.CellValueChanged += new DevExpress.XtraGrid.Views.Base.CellValueChangedEventHandler(gridViewEnumType_CellValueChanged);


            }
            catch (Exception ex)
            {
                DevExpress.XtraEditors.XtraMessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Information);
                StaticCaption(ex.Message);
            }
            finally { }
        }
        /// <summary>
        /// �к�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gridViewEnumType_CustomDrawRowIndicator(object sender, DevExpress.XtraGrid.Views.Grid.RowIndicatorCustomDrawEventArgs e)
        {
            if (e.Info.IsRowIndicator && e.RowHandle >= 0)//���Grid��������ʾ
            {
                e.Info.DisplayText = (e.RowHandle + 1).ToString();//��Ϊ���ݱ�����������Ǵ�0��ʼ��Ϊ�˷��㣬���Լ�1
            }
        }
        /// <summary>
        /// �ڵ�ı�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeList_FocusedNodeChanged(object sender, DevExpress.XtraTreeList.FocusedNodeChangedEventArgs e)
        {
            try
            {
                if (VOIsChanged())
                {
                    DialogResult dr = DevExpress.XtraEditors.XtraMessageBox.Show("��ǰ�����ѱ��޸ģ��Ƿ񱣴棿", "��ʾ", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                    if (dr == DialogResult.Yes)
                    {
                        Save("��������");
                    }

                }
                TypeID = TypeConvert.ToInt(e.Node.GetValue("EnumTypeID"));
                gridViewEnumType.FocusedRowChanged -= new DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventHandler(gridViewEnumType_FocusedRowChanged);
                gridViewEnumType.RowCountChanged -= new EventHandler(gridViewEnumType_RowCountChanged);
                gridControl1.DataSource = Model.GetEnumCodeByEnumTypeID(TypeID);
                SetLngEnumValue();
                gridViewEnumType.RowCountChanged += new EventHandler(gridViewEnumType_RowCountChanged);
                TreeListNode currNode = treeList.FindNodeByFieldValue("EnumTypeID", TypeID);
                if (currNode != null) treeList.SetFocusedNode(currNode);
                else treeList.SetFocusedNode(null);
                GetMaxEnumValue();
                gridViewEnumType.FocusedRowChanged += new DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventHandler(gridViewEnumType_FocusedRowChanged);
            }
            catch (Exception ex)
            {
                DevExpress.XtraEditors.XtraMessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Information);
                StaticCaption(ex.Message);
            }
            finally { GridIsChange = 0; }
        }
        /// <summary>
        /// Gridɾ��
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDetailDelete_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            try
            {
                int rowHandle = gridViewEnumType.FocusedRowHandle;
                if (rowHandle < 0)
                    return;

                bool blnPredefined = TypeConvert.ToBool(gridViewEnumType.GetRowCellValue(rowHandle, "BlnPredefined"));
                if (blnPredefined)
                {
                    DevExpress.XtraEditors.XtraMessageBox.Show("����ɾ��Ԥ��������", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                int id = TypeConvert.ToInt(gridViewEnumType.GetRowCellValue(rowHandle, "EnumCodeID"));
                enumCodeVO = Model.GetEnumCodeByID(id);
                if (enumCodeVO != null &&long.Parse( enumCodeVO.EnumCodeID) > 0)
                {
                    int lngEnumValue = TypeConvert.ToInt(gridViewEnumType.GetRowCellValue(rowHandle, "LngEnumValue"));
                    int ETypeId = TypeConvert.ToInt(gridViewEnumType.GetRowCellValue(rowHandle, "EnumTypeID"));

                }
                DialogResult dr = DevExpress.XtraEditors.XtraMessageBox.Show("�Ƿ�ɾ����ǰ���ݣ�", "ѯ��", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dr == DialogResult.Yes)
                {
                    int Id = TypeConvert.ToInt(gridViewEnumType.GetRowCellValue(rowHandle, "EnumCodeID"));
                    if (Id > 0)
                        this.delDetailItems.Add(Id);
                    gridViewEnumType.FocusedRowChanged -= new DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventHandler(gridViewEnumType_FocusedRowChanged);
                    gridViewEnumType.DeleteSelectedRows();
                    GetMaxEnumValue();
                    gridViewEnumType.FocusedRowChanged += new DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventHandler(gridViewEnumType_FocusedRowChanged);
                    GridIsChange = 1;
                }
            }
            catch (Exception ex)
            {
                DevExpress.XtraEditors.XtraMessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Information);
                StaticCaption(ex.Message);
            }
            finally { }
        }
        private void gridViewEnumType_FocusedColumnChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedColumnChangedEventArgs e)
        {

            try
            {
                int rowHandle = gridViewEnumType.FocusedRowHandle;
                if (rowHandle < 0)//��һ������༭
                {
                    //gridViewEnumType.Columns[gridViewEnumType.FocusedColumn.FieldName].OptionsColumn.ReadOnly = false;
                    //gridViewEnumType.Columns[gridViewEnumType.FocusedColumn.FieldName].OptionsColumn.AllowEdit = true;
                    //return;
                }

                int Id = TypeConvert.ToInt(gridViewEnumType.GetRowCellValue(rowHandle, "EnumCodeID"));
                enumCodeVO = Model.GetEnumCodeByID(Id);
                if (enumCodeVO == null || long.Parse(enumCodeVO.EnumCodeID) <= 0 || enumCodeVO.BlnPredefined=="0")
                {
                    //gridViewEnumType.Columns[gridViewEnumType.FocusedColumn.FieldName].OptionsColumn.ReadOnly = false;
                    //gridViewEnumType.Columns[gridViewEnumType.FocusedColumn.FieldName].OptionsColumn.AllowEdit = true;
                    //return;
                }
                bool blnPredefined = TypeConvert.ToBool(gridViewEnumType.GetRowCellValue(gridViewEnumType.FocusedRowHandle, "BlnPredefined"));
                if (blnPredefined)
                {
                    gridViewEnumType.Columns[gridViewEnumType.FocusedColumn.FieldName].OptionsColumn.ReadOnly = true;
                    gridViewEnumType.Columns[gridViewEnumType.FocusedColumn.FieldName].OptionsColumn.AllowEdit = false;
                    StaticCaption("�����޸�Ԥ������");
                }
                else
                {
                    gridViewEnumType.Columns[gridViewEnumType.FocusedColumn.FieldName].OptionsColumn.ReadOnly = false;
                    gridViewEnumType.Columns[gridViewEnumType.FocusedColumn.FieldName].OptionsColumn.AllowEdit = true;
                }


            }
            catch (Exception ex)
            {
                DevExpress.XtraEditors.XtraMessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Information);
                StaticCaption(ex.Message);
            }
            finally { }
        }
        private void gridViewEnumType_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {

            try
            {
                int rowHandle = gridViewEnumType.FocusedRowHandle;
                if (rowHandle < 0)
                {
                    //gridViewEnumType.Columns[gridViewEnumType.FocusedColumn.FieldName].OptionsColumn.ReadOnly = false;
                    //gridViewEnumType.Columns[gridViewEnumType.FocusedColumn.FieldName].OptionsColumn.AllowEdit = true;
                    //return;
                }

                int Id = TypeConvert.ToInt(gridViewEnumType.GetRowCellValue(rowHandle, "EnumCodeID"));
                enumCodeVO = Model.GetEnumCodeByID(Id);
                if (enumCodeVO == null || long.Parse(enumCodeVO.EnumCodeID) <= 0 || enumCodeVO.BlnPredefined=="0")
                {
                    //gridViewEnumType.Columns[gridViewEnumType.FocusedColumn.FieldName].OptionsColumn.ReadOnly = false;
                    //gridViewEnumType.Columns[gridViewEnumType.FocusedColumn.FieldName].OptionsColumn.AllowEdit = true;
                    //return;
                }
                bool blnPredefined = TypeConvert.ToBool(gridViewEnumType.GetRowCellValue(gridViewEnumType.FocusedRowHandle, "BlnPredefined"));
                if (blnPredefined)
                {
                    gridViewEnumType.Columns[gridViewEnumType.FocusedColumn.FieldName].OptionsColumn.ReadOnly = true;
                    gridViewEnumType.Columns[gridViewEnumType.FocusedColumn.FieldName].OptionsColumn.AllowEdit = false;
                    StaticCaption("�����޸�Ԥ������");
                }
                else
                {
                    gridViewEnumType.Columns[gridViewEnumType.FocusedColumn.FieldName].OptionsColumn.ReadOnly = false;
                    gridViewEnumType.Columns[gridViewEnumType.FocusedColumn.FieldName].OptionsColumn.AllowEdit = true;
                }

            }
            catch (Exception ex)
            {
                DevExpress.XtraEditors.XtraMessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Information);
                StaticCaption(ex.Message);
            }
            finally { }
        }
        /// <summary>
        /// ����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAdd_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {

            try
            {

                frmAddEnumType frm = new frmAddEnumType(TypeID, 0);
                frm.ShowDialog();
                TypeID = frm.TypeId;
                LoadTreeList();
            }
            catch (Exception ex)
            {
                Model.ShowMessageBox(ex.Message, 3, 1, StaticMsg);
            }
            finally { }
        }
        /// <summary>
        /// ɾ��
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDelete_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {


                TreeListNode node = treeList.FindNodeByKeyID(TypeID.ToString());
                if (node == null)
                {
                    Model.ShowMessageBox("��ѡ��ö������", 3, 1, StaticMsg);
                    return;
                }

                EnumtypeInfo type = Model.GetEnumTypeByID(TypeID);
                if (type != null && long.Parse(type.EnumTypeID) > 0)
                {
                    if (type.BlnPrefined)
                    {
                        Model.ShowMessageBox("����ɾ��Ԥ��ö������", 3, 1, StaticMsg);
                        return;
                    }
                }


                DialogResult dr = DevExpress.XtraEditors.XtraMessageBox.Show("ȷ��Ҫɾ����ö������", "��ʾ", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (dr == DialogResult.Yes)
                {
                    Model.SaveEnumType(GetData());

                    LoadTreeList();
                    Model.ShowMessageBox("ɾ�����ݳɹ�", 3, 1, StaticMsg);
                }

            }
            catch (Exception ex)
            {
                Model.ShowMessageBox(ex.Message, 3, 1, StaticMsg);
            }
            finally { }
        }
        /// <summary>
        /// ���˫���޸Ľڵ�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeList_MouseDoubleClick(object sender, MouseEventArgs e)
        {

        }
        /// <summary>
        /// �޸�ö������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnUpdate_ItemClick(object sender, ItemClickEventArgs e)
        {

            try
            {
                TreeListNode node = treeList.FindNodeByKeyID(TypeID.ToString());
                if (node == null)
                {
                    Model.ShowMessageBox("��ѡ��ö������", 3, 1, StaticMsg);
                    return;
                }
                frmAddEnumType frm = new frmAddEnumType(TypeID, 1);
                frm.ShowDialog();
                LoadTreeList();
            }
            catch (Exception ex)
            {
                Model.ShowMessageBox(ex.Message, 3, 1, StaticMsg);
            }
            finally { }
        }

        private void gridViewEnumType_RowCountChanged(object sender, EventArgs e)
        {

        }


        /// <summary>
        /// ѡ�е�ǰĬ��ֵ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void blnDefaultCheck_CheckedChanged(object sender, EventArgs e)
        {
            gridViewEnumType.CloseEditor();
            bool blnChecked = TypeConvert.ToBool(gridViewEnumType.GetRowCellValue(gridViewEnumType.FocusedRowHandle, "BlnDefault"));
            for (int i = 0; i < gridViewEnumType.RowCount; i++)
            {
                if (i != gridViewEnumType.FocusedRowHandle)
                    gridViewEnumType.SetRowCellValue(i, "BlnDefault", false); ;

            }
        }






        private void treeList_Click(object sender, EventArgs e)
        {
            try
            {
                TypeID = TypeConvert.ToInt(treeList.FocusedNode.GetValue("EnumTypeID"));
            }
            catch
            { }
        }

        private void treeList_GetSelectImage(object sender, DevExpress.XtraTreeList.GetSelectImageEventArgs e)
        {
            if (e.FocusedNode)
                e.NodeImageIndex = 2;
            else
                e.NodeImageIndex = 1;
        }
    }
}