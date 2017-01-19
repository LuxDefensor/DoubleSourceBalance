using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DoubleSourceBalance
{
    public partial class formMain : Form
    {
        private Controller c;
        private Color BackIN = Color.LightGreen;
        private Color BackOUT = Color.Pink;

        public formMain()
        {
            InitializeComponent();
            c = new Controller();
            this.Load += FormMain_Load;
            lstBalances.SelectedIndexChanged += LstBalances_SelectedIndexChanged;
            btnCalc.Click += BtnCalc_Click;
        }

        private void BtnCalc_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            int leftRow = 2;
            int rightRow = 2;
            double totalIN = 0;
            double totalOUT = 0;
            double consumption = 0;
            if (lstBalances.SelectedIndex >= 0)
            {
                dgvResult.Rows.Clear();
                Balance b = c.Balances[lstBalances.SelectedIndex];
                dgvResult.RowCount = Math.Max(
                    b.Components.Count(comp => comp.Side == BalanceSides.IN),
                    b.Components.Count(comp => comp.Side == BalanceSides.OUT)) + 2;
                foreach (BalanceComponent component in b.Components)
                {
                    try
                    {
                        consumption = DataProvider.GetConsumption(component.Source,
                            component, dtpFrom.Value, dtpTill.Value);
                    }
                    catch (Exception ex)
                    {
                        formErrorMessage dlg = new formErrorMessage("Опреация: вычисление потребления",
                            new Tuple<string, string>(
                                ex.Message,
                                (ex.InnerException != null) ?
                                    ex.InnerException.Message :
                                    "No additional info"));
                        dlg.ShowDialog();
                        this.Cursor = Cursors.Default;
                        return;
                    }
                    switch (component.Side)
                    {
                        case BalanceSides.IN:
                            dgvResult.Rows[leftRow].Cells[0].Value = component.Name;
                            dgvResult.Rows[leftRow].Cells[1].Value = consumption.ToString("N0",
                                System.Globalization.CultureInfo.CurrentCulture.NumberFormat);
                            totalIN += consumption;
                            leftRow++;
                            break;
                        case BalanceSides.OUT:
                            dgvResult.Rows[rightRow].Cells[3].Value = component.Name;
                            dgvResult.Rows[rightRow].Cells[2].Value = consumption.ToString("N0",
                                System.Globalization.CultureInfo.CurrentCulture.NumberFormat);
                            totalOUT += consumption;
                            rightRow++;
                            break;
                    }
                    dgvResult.Refresh();
                }
                dgvResult.Rows[1].DefaultCellStyle.Font =
                    new Font(dgvResult.DefaultCellStyle.Font, FontStyle.Bold);
                dgvResult.Rows[1].Cells[0].Value = "ИТОГО:";
                dgvResult.Rows[1].Cells[1].Value = totalIN.ToString("N0",
                    System.Globalization.CultureInfo.CurrentCulture.NumberFormat);
                dgvResult.Rows[1].Cells[2].Value = totalOUT.ToString("N0",
                    System.Globalization.CultureInfo.CurrentCulture.NumberFormat);
                if (totalIN == 0 && totalOUT == 0)
                {
                    formErrorMessage dlg = new formErrorMessage("Опреация: вычисление небаланса",
                            "На ноль делить нельзя!");
                    dlg.ShowDialog();
                    this.Cursor = Cursors.Default;
                    return;
                }
                dgvResult.Rows[0].DefaultCellStyle.Font =
                    new Font(dgvResult.DefaultCellStyle.Font, FontStyle.Bold);
                dgvResult.Rows[0].DefaultCellStyle.BackColor = dgvResult.DefaultCellStyle.BackColor;
                dgvResult.Rows[0].Cells[0].Value = "Небаланс=";
                double disbalance = Math.Abs(totalIN - totalOUT);
                dgvResult.Rows[0].Cells[1].Value = disbalance.ToString("N0",
                    System.Globalization.CultureInfo.CurrentCulture.NumberFormat) +
                    " кВт-ч";
                disbalance = 2 * disbalance / (totalIN + totalOUT);
                dgvResult.Rows[0].Cells[2].Value = disbalance.ToString("P2",
                    System.Globalization.CultureInfo.CurrentCulture.NumberFormat);
                this.Cursor = Cursors.Default;
            }
        }

        private void LstBalances_SelectedIndexChanged(object sender, EventArgs e)
        {
            dgvComponents.Rows.Clear();
            FillComponents(lstBalances.SelectedIndex);
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
            DateTime yesterday = DateTime.Today.AddDays(-1).Date;
            dtpFrom.Value = new DateTime(yesterday.Year, yesterday.Month, 1);
            dtpTill.Value = yesterday;
            lstBalances.DataSource = c.Balances;
            dgvResult.Columns[0].DefaultCellStyle.BackColor = BackIN;
            dgvResult.Columns[1].DefaultCellStyle.BackColor = BackIN;
            dgvResult.Columns[2].DefaultCellStyle.BackColor = BackOUT;
            dgvResult.Columns[3].DefaultCellStyle.BackColor = BackOUT;
            if (c.Balances.Count > 0)
                FillComponents(0);
        }

        private void FillComponents(int balanceNo)
        {
            string group;
            Color back;
            Balance b = c.Balances[balanceNo];
            dgvComponents.RowCount = b.Components.Count;
            int i = 0;
            foreach (BalanceComponent c in b.Components)
            {
                group = c.Side == BalanceSides.IN ? "приём" : "отдача";
                if (group == "приём")
                    back = BackIN;
                else
                    back = BackOUT;
                foreach (DataGridViewCell cell in dgvComponents.Rows[i].Cells)
                    cell.Style.BackColor = back;
                dgvComponents.Rows[i].Cells[0].Value = c.Channel;
                dgvComponents.Rows[i].Cells[1].Value = c.Name;
                dgvComponents.Rows[i].Cells[2].Value = c.Method == CalculateMethods.integral ?
                    "показаниям" : "получасовкам";
                dgvComponents.Rows[i].Cells[3].Value = group;
                dgvComponents.Rows[i].Cells[4].Value = c.Sign;
                if (c.Sign == "-1")
                {
                    dgvComponents.Rows[i].Cells[4].Style.BackColor = Color.Black;
                    dgvComponents.Rows[i].Cells[4].Style.ForeColor = Color.White;
                }
                i++;
            }
        }
    }
}
