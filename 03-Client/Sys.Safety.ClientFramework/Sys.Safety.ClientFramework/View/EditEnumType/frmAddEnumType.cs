using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using Sys.Safety.ClientFramework.Model;
using Sys.Safety.DataContract;
using Basic.Framework.Web;

namespace Sys.Safety.ClientFramework.View.EnumCode
{
    public partial class frmAddEnumType : DevExpress.XtraEditors.XtraForm
    {

        public int TypeId = 0;
        private int type = 0;//0:���� 1:�޸�        
        private EnumtypeInfo vo = null;
        private EnumtypeInfo CopyVO = new EnumtypeInfo();
        private EnumModel model = new EnumModel();
        private DevExpress.Utils.WaitDialogForm frmWait;
        private int parentId = 0;
        private int Id = 0;
        private int LookUpId = 0;


        public frmAddEnumType(int ETypeId, int Type)
        {
            InitializeComponent();
            TypeId = ETypeId;
            type = Type;
            Id = ETypeId;
        }

        private void frmAddEnumType_Load(object sender, EventArgs e)
        {

            DataTable dt = model.GetEnumtypeDataTable();
            DataRow row = dt.NewRow();
            row["EnumTypeID"] = 0;
            row["StrName"] = " ";
            dt.Rows.InsertAt(row, 0);
            this.LookUpMoveTypeId.Properties.DataSource = dt;
            LoadData();

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
                ShowWaitForm("��������");
                GetData();
                vo = model.SaveEnumType(vo);
                UpdateParentVO();
                TypeId = int.Parse(vo.EnumTypeID);
                type = 1;
                LoadData();
                model.ShowMessageBox("�������ݳɹ�", 3, 0, StaticMsg);

            }
            catch (Exception ex)
            {
                model.ShowMessageBox(ex.Message, 1, 1, StaticMsg);
            }
            finally
            {
                CloseWaitForm();
            }
        }
        /// <summary>
        /// ���ӽڵ���ӳɹ��޸ĸ��ڵ��blnDetailֵ
        /// </summary>
        private void UpdateParentVO()
        {
            EnumtypeInfo typeVO = new EnumtypeInfo();
            typeVO = model.GetEnumTypeByID(Id);
            if (typeVO != null && int.Parse(typeVO.EnumTypeID) > 0)
            {
                typeVO.InfoState = InfoState.Modified;
                typeVO.BlnDetail = false;               
                model.SaveEnumType(typeVO);
            }
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
        /// ��������
        /// </summary>
        private void LoadData()
        {
            if (type > 0)
            {
                vo = model.GetEnumTypeByID(TypeId);
                parentId = int.Parse(vo.ParentID);
                vo.InfoState = InfoState.Modified;
            }
            else
            {
                vo = new EnumtypeInfo();
                parentId = TypeId;
                vo.InfoState = InfoState.AddNew;
            }
            InitData();
            CreateNewVO();
            LookUpId = parentId;
        }
        private void InitData()
        {
            txtEnumTypeCode.Text = vo.StrCode;
            txtEnumTypeName.Text = vo.StrName;
            CheckblnEnumValue1.Checked = (vo.BlnEnumValue1  ? true : false);
            CheckblnEnumValue2.Checked = (vo.BlnEnumValue2  ? true : false);
            CheckblnEnumValue3.Checked = (vo.BlnEnumValue3  ? true : false);
            LookUpMoveTypeId.EditValue = parentId;
        }
        /// <summary>
        /// ��������Ƿ����
        /// </summary>
        /// <returns></returns>
        private bool DataIsChange()
        {
            if (txtEnumTypeCode.Text != CopyVO.StrCode)
                return true;
            if (txtEnumTypeName.Text != CopyVO.StrName)
                return true;
            if (CheckblnEnumValue1.Checked != (CopyVO.BlnEnumValue1  ? true : false))
                return true;
            if (CheckblnEnumValue2.Checked != (CopyVO.BlnEnumValue2  ? true : false))
                return true;
            if (CheckblnEnumValue3.Checked != (CopyVO.BlnEnumValue3  ? true : false))
                return true;
            if (LookUpId != Convert.ToInt32(LookUpMoveTypeId.EditValue))
                return true;
            return false;

        }
        /// <summary>
        /// �ռ�����
        /// </summary>
        private void GetData()
        {
            vo.StrCode = txtEnumTypeCode.Text;
            vo.StrName = txtEnumTypeName.Text;
            vo.ParentID = LookUpMoveTypeId.EditValue.ToString();

            vo.BlnPrefined = false;
            vo.BlnDetail = true;          
            vo.BlnEnumValue1 = (CheckblnEnumValue1.Checked == true ? true : false);
            vo.BlnEnumValue2 = (CheckblnEnumValue2.Checked == true ? true : false);
            vo.BlnEnumValue3 = (CheckblnEnumValue3.Checked == true ? true : false);
        }
        /// <summary>
        /// 
        /// </summary>
        private void CreateNewVO()
        {
            CopyVO.StrCode = txtEnumTypeCode.Text;
            CopyVO.StrName = txtEnumTypeName.Text;
            CopyVO.BlnEnumValue1 = (CheckblnEnumValue1.Checked == true ? true : false);
            CopyVO.BlnEnumValue2 = (CheckblnEnumValue2.Checked == true ? true : false);
            CopyVO.BlnEnumValue3 = (CheckblnEnumValue3.Checked == true ? true : false);

        }
        /// <summary>
        /// �ر�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCanlce_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            this.Close();
        }
        /// <summary>
        /// �رռ�������Ƿ����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frmAddEnumType_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (DataIsChange())
            {
                DialogResult dr = model.ShowMessageBox("��ǰ�����ѱ��޸ģ��Ƿ񱣴棿", 5, 1, StaticMsg);
                if (dr == DialogResult.Yes)
                {
                    btnSave_ItemClick(null, null);
                    e.Cancel = false;
                }
                if (dr == DialogResult.Cancel)
                    e.Cancel = true;
                else e.Cancel = false;
            }
        }
    }
}