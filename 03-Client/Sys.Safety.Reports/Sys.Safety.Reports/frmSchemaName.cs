using System;
using System.Collections.Generic;
using DevExpress.XtraEditors;

namespace Sys.Safety.Reports
{
    public partial class frmSchemaName : XtraForm
    {
        private string _schemaName = string.Empty;

        public frmSchemaName()
        {
            LngUseRight = 0;
            ExistSchemaNameList = null;
            InitializeComponent();
        }

        /// <summary>
        ///     ���ڵķ������б�
        /// </summary>
        public IList<string> ExistSchemaNameList { get; set; }

        /// <summary>
        ///     ������
        /// </summary>
        public string SchemaName
        {
            get { return _schemaName; }
            set { _schemaName = value; }
        }

        /// <summary>
        ///     ʹ��Ȩ�� 0���� 1 ˽��
        /// </summary>
        public int LngUseRight { get; private set; }

        private void frmSchemaName_Load(object sender, EventArgs e)
        {
            txtSchemaName.Text = _schemaName;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            var str = txtSchemaName.Text.Trim();
            if (str == string.Empty)
            {
                MessageShowUtil.ShowInfo("����������Ϊ�գ�����������!");
                return;
            }
            if (ExistSchemaNameList.Contains(str) && (str != _schemaName))
            {
                MessageShowUtil.ShowInfo("�˷������Ѵ��ڣ�����������!");
                return;
            }

            LngUseRight = TypeUtil.ToInt(radioGroupUseRight.EditValue);
            if (LngUseRight == 0)
            {
                //���湫�з��� ���ж�Ȩ��

                //if (!PermissionManager.HavePermission("AddListPublicSchema"))
                //{
                //    MessageBox.Show("���������з���Ȩ��", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                //    return;
                //}
            }

            _schemaName = str;
            Close();
        }

        private void blnCancel_Click(object sender, EventArgs e)
        {
            _schemaName = "";
            Close();
        }
    }
}