using System;
using System.Data;
using DevExpress.XtraEditors;
using Sys.Safety.DataContract;

namespace Sys.Safety.Reports
{
    public partial class frmConTextCondition : XtraForm
    {
        private ListmetadataInfo _lmd = new ListmetadataInfo();
        private string _strFieldDispaly = "";

        public frmConTextCondition()
        {
            BlnOk = false;
            SelectedDt = null;
            BlnEdit = false;
            InitializeComponent();
        }

        /// <summary>
        ///     �༭״̬
        /// </summary>
        public bool BlnEdit { get; set; }

        /// <summary>
        ///     ѡ����Ŀ����Դ
        /// </summary>
        public DataTable SelectedDt { get; set; }

        public ListmetadataInfo Lmd
        {
            get { return _lmd; }
            set { _lmd = value; }
        }

        public string StrFieldDispaly
        {
            get { return _strFieldDispaly; }
            set { _strFieldDispaly = value; }
        }

        public bool BlnOk { get; set; }

        /// <summary>
        ///     Load
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frmConTextCondition_Load(object sender, EventArgs e)
        {
            try
            {
                memoEditFormula.Text = _lmd.StrFormula;
                listBoxControl1.ValueMember = "MetaDataFieldName";
                listBoxControl1.DisplayMember = "strListDisplayFieldNameCHS";
                SelectedDt.DefaultView.RowFilter = "isnull(blnSysProcess,0)=0 and isnull(isCalcField,0)=0";
                listBoxControl1.DataSource = SelectedDt;
                GetDt();
            }
            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        /// <summary>
        ///     ΪlistBoxControl2��ֵ
        /// </summary>
        private void GetDt()
        {
            var dt = new DataTable();
            dt.Columns.Add("Name", Type.GetType("System.String"));
            dt.Columns.Add("ID", Type.GetType("System.String"));
            var rows = dt.NewRow();
            rows["Name"] = "��ǰ��¼����Ա";
            rows["ID"] = "=${userId}";
            dt.Rows.Add(rows);

            //rows = dt.NewRow();
            //rows["Name"] = "��ǰ��¼����";
            //rows["ID"] = "=${orgId}";
            //dt.Rows.Add(rows);

            rows = dt.NewRow();
            rows["Name"] = "��ǰ����Ա��Ͻ����";
            rows["ID"] = " in (${manageOrgsId})";
            dt.Rows.Add(rows);
            listBoxControl2.DataSource = dt;
        }

        /// <summary>
        ///     ˫����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listBoxControl1_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                var str = TypeUtil.ToString(listBoxControl1.SelectedValue);
                memoEditFormula.Text += str;
            }
            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        /// <summary>
        ///     ���
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClear_Click(object sender, EventArgs e)
        {
            memoEditFormula.Text = "";
        }

        /// <summary>
        ///     ȷ��
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOk_Click(object sender, EventArgs e)
        {
            try
            {
                _lmd.StrFormula = memoEditFormula.Text.Trim(); //���㹫ʽ
                _lmd.StrRefColList = ""; //������Ŀ�б�
                //if (_lmd.StrFormula == string.Empty)
                //{
                //    this.ShowMsg("���ʽ����Ϊ�գ�����������");
                //    this.memoEditFormula.Focus();
                //    return;
                //}

                var rowCount = SelectedDt.Rows.Count;
                for (var i = 0; i < rowCount; i++)
                {
                    var str = TypeUtil.ToString(SelectedDt.Rows[i]["MetaDataFieldName"]);
                    if (_lmd.StrFormula.Contains(str))
                        _lmd.StrRefColList = _lmd.StrRefColList + str + ",";
                }
                if (_lmd.StrRefColList != string.Empty)
                    _lmd.StrRefColList = _lmd.StrRefColList.Remove(_lmd.StrRefColList.Length - 1);
            }
            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }


            BlnOk = true;
            Close();
        }

        /// <summary>
        ///     ȡ��
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            BlnOk = false;
            Close();
        }


        private void listBoxControl2_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                var str = TypeUtil.ToString(listBoxControl2.SelectedValue);
                memoEditFormula.Text += str;
            }
            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
            finally
            {
            }
        }
    }
}