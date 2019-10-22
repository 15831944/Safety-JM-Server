using System;
using System.Data;
using DevExpress.XtraBars;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraLayout.Utils;
using Sys.Safety.Reports.Controls;
using Sys.Safety.Reports.Model;

namespace Sys.Safety.Reports
{
    public partial class frmListParameterSet : XtraForm
    {
        private string _strFieldNameList = string.Empty;
        private string _strHyperlink = string.Empty;

        private string _strKeyValue = "";
        private string _strTableAlias = "";
        private string _strWebHyperlink = string.Empty;

        public frmListParameterSet()
        {
            DataSource = null;
            BlnOk = false;
            BlnLink = false;
            LngHyperLinkType = 0;
            Model = null;
            InitializeComponent();
        }

        public ListExModel Model { get; set; }

        /// <summary>
        ///     �����ֶ����б�
        /// </summary>
        public string StrFieldNameList
        {
            set { _strFieldNameList = value; }
            get { return _strFieldNameList; }
        }

        /// <summary>
        ///     �������� 1��Ƭ 2�б� 3����
        /// </summary>
        public int LngHyperLinkType { get; set; }

        /// <summary>
        ///     ��Ƭ�򵥾�(ͨ�����������)
        /// </summary>
        public string StrTableAlias
        {
            get { return _strTableAlias; }
            set { _strTableAlias = value; }
        }

        /// <summary>
        ///     ���ӵ�ַ
        /// </summary>
        public string StrHyperlink
        {
            set { _strHyperlink = value; }
            get { return _strHyperlink; }
        }

        /// <summary>
        ///     Web����
        /// </summary>
        public string StrWebHyperlink
        {
            set { _strWebHyperlink = value; }
            get { return _strWebHyperlink; }
        }

        /// <summary>
        ///     �Ƿ�Ϊ���ӵ�ַ
        /// </summary>
        public bool BlnLink { set; get; }

        public bool BlnOk { set; get; }

        /// <summary>
        ///     ����Դ
        /// </summary>
        public DataTable DataSource { set; get; }

        private void frmListParameterSet_Load(object sender, EventArgs e)
        {
            try
            {
                if (LngHyperLinkType > 0)
                {
                    comboBoxHyperLinkType.SelectedIndexChanged -= comboBoxHyperLinkType_SelectedIndexChanged;
                    comboBoxHyperLinkType.SelectedIndex = LngHyperLinkType;
                    comboBoxHyperLinkType.SelectedIndexChanged += comboBoxHyperLinkType_SelectedIndexChanged;
                }
                btnHyperlinkSelect.Enabled = true;
                if (BlnLink)
                {
                    btnHyperlinkSelect.Text = _strHyperlink;
                    txtstrWebHyperlink.Text = _strWebHyperlink;
                }
                else
                {
                    layoutControlItemHyperLinkType.Visibility = LayoutVisibility.Never;
                    layoutControlItemHyperLink.Visibility = LayoutVisibility.Never;
                    layoutstrWebHyperlink.Visibility = LayoutVisibility.Never;
                }

                SetFormStyle();
                SetFilter();

                var dc = new DataColumn("blnSelect", Type.GetType("System.Boolean"));
                dc.DefaultValue = false;
                DataSource.Columns.Add(dc);
                gridControl1.DataSource = DataSource;
                repositoryItemLookUpProvide.DataSource = DataSource;
                SetProvideTypeData(); //�����б������б�

                //����ѡ����
                if (_strFieldNameList != string.Empty)
                    if (LngHyperLinkType != 2)
                    {
                        var strArr = _strFieldNameList.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var str in strArr)
                            for (var i = 0; i < DataSource.Rows.Count; i++)
                                if (TypeUtil.ToString(DataSource.Rows[i]["MetaDataFieldName"]) == str)
                                {
                                    DataSource.Rows[i]["blnSelect"] = true;
                                    break;
                                }
                    }
                    else
                    {
                        if (_strHyperlink != string.Empty)
                        {
                            FillGridControl2Data(_strHyperlink);
                            var strArr = _strFieldNameList.Split(new[] {'&'}, StringSplitOptions.RemoveEmptyEntries);
                            var strProvide = string.Empty;
                            var strReceive = string.Empty;
                            var lngProvideType = 0;
                            foreach (var str in strArr)
                            {
                                strReceive = str.Remove(str.LastIndexOf('='));
                                strProvide = str.Substring(str.LastIndexOf('=') + 1);
                                if ((strProvide.Length > 2) && ('$' == strProvide[strProvide.Length - 2]))
                                {
                                    //������ʷ����

                                    lngProvideType = TypeUtil.ToInt(strProvide.Substring(strProvide.Length - 1));
                                    strProvide = strProvide.Remove(strProvide.Length - 2);
                                }
                                for (var i = 0; i < gridView2.RowCount; i++)
                                    if (TypeUtil.ToString(gridView2.GetRowCellValue(i, "strReceiveCol")) == strReceive)
                                    {
                                        gridView2.SetRowCellValue(i, "strProvideCol", strProvide);
                                        gridView2.SetRowCellValue(i, "lngProvideType", lngProvideType);
                                        break;
                                    }
                            }
                        }
                    }

                if (BlnLink)
                    comboBoxHyperLinkType.SelectedIndex = 2; //Ĭ������Ϊ�б�
            }
            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        private void SetProvideTypeData()
        {
            var provideTypeDt = new DataTable();
            provideTypeDt.Columns.Add(new DataColumn("ID", Type.GetType("System.Int32")));
            provideTypeDt.Columns.Add(new DataColumn("Name", Type.GetType("System.String")));

            var dr = provideTypeDt.NewRow();
            dr["ID"] = 0;
            dr["Name"] = "������";
            provideTypeDt.Rows.Add(dr);

            dr = provideTypeDt.NewRow();
            dr["ID"] = 1;
            dr["Name"] = "��������";
            provideTypeDt.Rows.Add(dr);

            repLookUpEditProvideType.DataSource = provideTypeDt;
        }

        /// <summary>
        ///     �������͸ı�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBoxHyperLinkType_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                LngHyperLinkType = comboBoxHyperLinkType.SelectedIndex;
                btnHyperlinkSelect.Text = "";
                for (var i = 0; i < DataSource.Rows.Count; i++)
                    DataSource.Rows[i]["blnSelect"] = false;

                gridControl2.DataSource = null;

                SetFormStyle();
                SetFilter();
            }
            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }
        }

        private void SetFormStyle()
        {
            if (2 == LngHyperLinkType)
            {
                layoutControlGroup2.Visibility = LayoutVisibility.Never;
                layoutControlGroup3.Visibility = LayoutVisibility.Always;
            }
            else
            {
                layoutControlGroup2.Visibility = LayoutVisibility.Always;
                layoutControlGroup3.Visibility = LayoutVisibility.Never;
            }

            btnHyperlinkSelect.Properties.Buttons[0].Enabled = true;
            if ((3 == LngHyperLinkType) || (0 == LngHyperLinkType))
                btnHyperlinkSelect.Properties.Buttons[0].Enabled = false;
        }

        private void SetFilter()
        {
            if (DataSource != null)
                switch (LngHyperLinkType)
                {
                    case 1: //��Ƭ
                        DataSource.DefaultView.RowFilter = "isnull(lngKeyFieldType,0)=2 and strTableAlias='" +
                                                           _strTableAlias + "'";
                        break;
                    case 2: //�б�
                        DataSource.DefaultView.RowFilter = "";
                        break;
                    case 3: //����
                        DataSource.DefaultView.RowFilter = "isnull(MetaDataFieldName,'') like '%_strRequest' ";
                            //and strTableAlias='" + _strTableAlias + "'
                        break;
                    case 4: //��ַ
                        DataSource.DefaultView.RowFilter = "";
                        break;
                    default:
                        DataSource.DefaultView.RowFilter = "isnull(lngKeyFieldType,0)=2";
                        break;
                }
        }

        private void btnHyperlinkSelect_ButtonClick(object sender, ButtonPressedEventArgs e)
        {
            try
            {
                if (0 == e.Button.Index)
                {
                    var multiDlgRef = new frmGenericRef();
                    multiDlgRef.StrFkCode = "AllListRef";
                    multiDlgRef.BlnSelectStr = true;
                    multiDlgRef.BlnMulti = false;
                    multiDlgRef.StrSelectValue = _strKeyValue;
                    multiDlgRef.ShowDialog();
                    if (multiDlgRef.BlnOk)
                    {
                        _strKeyValue = multiDlgRef.StrSelectValue;
                        btnHyperlinkSelect.Text = "ListEx;ListID=" + multiDlgRef.SelectValue;
                        if ((2 == LngHyperLinkType) && (multiDlgRef.StrSelectDisplay != string.Empty) && (Model != null))
                        {
                            //���ӵ��б�

                            var str = btnHyperlinkSelect.Text;
                            FillGridControl2Data(str);
                        }
                    }
                }
                else if (1 == e.Button.Index)
                {
                    btnHyperlinkSelect.Text = "";
                }
            }
            catch (Exception ex)
            {
                MessageShowUtil.ShowErrow(ex);
            }

            #region �����·���

            //try
            //{
            //    if (0 == e.Button.Index)
            //    {
            //        frmRequestLib frm = new frmRequestLib();
            //        frm.LngHyperLinkType = _lngHyperLinkType;
            //        frm.ShowDialog();

            //        if (frm.BlnOk)
            //        {
            //            btnHyperlinkSelect.Text = frm.RequestLibVo.StrRequest;
            //            this.txtstrWebHyperlink.Text = frm.RequestLibVo.StrWebRequest;
            //            if (2 == _lngHyperLinkType && frm.RequestLibVo.StrRequest != string.Empty && _Model != null)
            //            {   //���ӵ��б�

            //                string str = frm.RequestLibVo.StrRequest;
            //                FillGridControl2Data(str);
            //            }
            //        }
            //    }
            //    else if (1 == e.Button.Index)
            //    {
            //        btnHyperlinkSelect.Text = "";
            //    }
            //}
            //catch (Exception ex)
            //{
            //    ShowMsg(ex.Message);
            //}

            #endregion
        }

        private void FillGridControl2Data(string str)
        {
            str = str.Substring(str.LastIndexOf("ListID="));
            var strs = str.Split(new[] {'&'}, StringSplitOptions.RemoveEmptyEntries);
            if (strs.Length > 0)
            {
                var strListId = strs[0].Substring(7);
                var strSql = "";
                if (Model.GetDbType() == "sqlserver")
                    strSql =
                        string.Format(@"select '' as strProvideCol,MetaDataFieldName strReceiveCol,strListDisplayFieldNameCHS,0 as lngProvideType from BFT_ListMetaData 
                    left join BFT_ListDisplayEx on BFT_ListDisplayEx.listdataid=BFT_ListMetaData.listdataid and BFT_ListDisplayEx.strListDisplayFieldName=BFT_ListMetaData.MetaDataFieldName
                    where BFT_ListMetaData.listdataid in (
                    select top 1 listdataID from BFT_ListDataEx 
                    where listid={0} ) and blnReceivePara=1", strListId);
                if (Model.GetDbType() == "mysql")
                    strSql =
                        string.Format(@"select '' as strProvideCol,MetaDataFieldName strReceiveCol,strListDisplayFieldNameCHS,0 as lngProvideType from BFT_ListMetaData 
                    left join BFT_ListDisplayEx on BFT_ListDisplayEx.listdataid=BFT_ListMetaData.listdataid and BFT_ListDisplayEx.strListDisplayFieldName=BFT_ListMetaData.MetaDataFieldName
                    where BFT_ListMetaData.listdataid in (
                    select  listdataID from BFT_ListDataEx 
                    where listid={0} ) and blnReceivePara=1", strListId); //�˴���������ӣ���ô�б�������Ϊ������������Ϊû�м�top 1
                var grid2Dt = Model.GetDataTable(strSql);
                gridControl2.DataSource = grid2Dt;
            }
        }


        private void gridView1_CellValueChanged(object sender, CellValueChangedEventArgs e)
        {
        }

        private void repositoryItemCheckEditSelect_CheckedChanged(object sender, EventArgs e)
        {
            //this.gridView1.CloseEditor();
            //this.gridView1.UpdateCurrentRow();
        }

        private void tlbOK_ItemClick(object sender, ItemClickEventArgs e)
        {
            var strSelect = "";
            UpdateGridControlData();
            if (LngHyperLinkType == 2)
            {
                //�б�
                var strProvide = string.Empty;
                //�����߲���1={�ṩ�߲���1}&�����߲���2={�ṩ�߲���2}...
                for (var i = 0; i < gridView2.RowCount; i++)
                {
                    strProvide = TypeUtil.ToString(gridView2.GetRowCellValue(i, "strProvideCol"));
                    if (strProvide != string.Empty)
                        strSelect += TypeUtil.ToString(gridView2.GetRowCellValue(i, "strReceiveCol")) + "=" + strProvide
                                     + "$" + TypeUtil.ToString(gridView2.GetRowCellValue(i, "lngProvideType"))
                                     + "&";
                }
            }
            else
            {
                //��Ƭ�����ݡ���ַ
                for (var i = 0; i < DataSource.Rows.Count; i++)
                    if (TypeUtil.ToBool(DataSource.Rows[i]["blnSelect"]))
                        strSelect += TypeUtil.ToString(DataSource.Rows[i]["MetaDataFieldName"]) + ",";
            }

            _strHyperlink = btnHyperlinkSelect.Text.Trim();
            _strWebHyperlink = txtstrWebHyperlink.Text.Trim();
            if (strSelect == string.Empty)
                if (!BlnLink || (LngHyperLinkType > 0))
                {
                    //ShowMsg("��ѡ����������У�");
                    //return;
                }

            if (strSelect != string.Empty)
                _strFieldNameList = strSelect.Remove(strSelect.Length - 1);
            else
                _strFieldNameList = "";

            if (LngHyperLinkType == 3)
            {
                if (_strFieldNameList.Contains(","))
                {
                    _strFieldNameList = "";
                    MessageShowUtil.ShowInfo("�������Ͳ�����ֻ������һ�У�");
                    return;
                }
            }
            else if ((LngHyperLinkType == 1) || (LngHyperLinkType == 2))
            {
                if (_strHyperlink == "")
                {
                    MessageShowUtil.ShowInfo("���ӵ�ַ����Ϊ�գ�");
                    return;
                }
            }

            BlnOk = true;
            Close();
        }

        private void tlbCancel_ItemClick(object sender, ItemClickEventArgs e)
        {
            _strFieldNameList = string.Empty;
            _strHyperlink = string.Empty;
            txtstrWebHyperlink.Text = string.Empty;
            Close();
        }

        private void UpdateGridControlData()
        {
            gridView1.CloseEditor();
            gridView1.UpdateCurrentRow();
            gridView2.CloseEditor();
            gridView2.UpdateCurrentRow();
        }

        private void gridView1_CustomDrawRowIndicator(object sender, RowIndicatorCustomDrawEventArgs e)
        {
            var rowHandle = e.RowHandle + 1;
            if (e.Info.IsRowIndicator && (rowHandle > 0))
                e.Info.DisplayText = rowHandle.ToString();
        }

        private void gridView2_CustomDrawRowIndicator(object sender, RowIndicatorCustomDrawEventArgs e)
        {
            var rowHandle = e.RowHandle + 1;
            if (e.Info.IsRowIndicator && (rowHandle > 0))
                e.Info.DisplayText = rowHandle.ToString();
        }


        private void tlbDelete_ItemClick(object sender, ItemClickEventArgs e)
        {
            btnHyperlinkSelect.Text = "";
            gridControl2.DataSource = null;
            LngHyperLinkType = 0;
        }
    }
}