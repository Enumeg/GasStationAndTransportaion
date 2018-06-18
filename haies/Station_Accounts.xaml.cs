using System;
using System.Windows;
using System.Windows.Controls;
using System.Data;
using System.Drawing.Printing;
using System.Drawing;

namespace haies
{
    /// <summary>
    /// Interaction logic for Station_Accounts.xaml
    /// </summary>
    public partial class Station_Accounts : Page
    {
        private readonly StationAccounts _stationAccounts;

        private readonly CPrinting.PrintedDocument _pd;

        private DataTable _outcomeSales;

        int _rowIndex, page_num;
        public Station_Accounts()
        {
            InitializeComponent();
            _stationAccounts = new StationAccounts();
            _pd = new CPrinting.PrintedDocument();
            _pd.PrintPage += new PrintPageEventHandler(PD_PrintPage);
            GetAcconts();
        }

        private void GetAcconts()
        {
            if (Date_TB.Value == null) return;
            var date = Date_TB.Value.Value.Date;
            Get_Balance(date);
            Pumps_DG.ItemsSource = _stationAccounts.ListPumps(date);
            Sales_DG.ItemsSource = _stationAccounts.ListSales(date);
            Gas_DG.ItemsSource = _stationAccounts.GetGasBalance(date);
            Outcome_DG.ItemsSource = _stationAccounts.ListOutcome(date);
        }

        private void Get_Balance(DateTime date)
        {
            if (Date_TB.Value == null) return;
            var model = _stationAccounts.GetAccounts(date);
            Last_Bal_TK.Text = model.LastBalance.ToString("0.00");
            Total_Sales_TK.Text = model.Sales.ToString("0.00");
            Sales_Tax_TK.Text = model.SalesTax.ToString("0.00");
            Net_Sales_TK.Text = model.NetSales.ToString("0.00");
            Futures_Sales_TK.Text = model.FutureSales.ToString("0.00");
            cash_Sales_TK.Text = model.CashSales.ToString("0.00");
            Total_Outcome_TK.Text = model.Outcome.ToString("0.00");
            Total_Purchases_TK.Text = model.Purchases.ToString("0.00");
            Purchases_Tax_TK.Text = model.PurchasesTax.ToString("0.00");
            Net_Purchases_TK.Text = model.NetPurchases.ToString("0.00");
            Bank_TK.Text = model.Bank.ToString("0.00");
            Payments_TK.Text = model.Payment.ToString("0.00");
            Total_Income_TK.Text = model.Income.ToString("0.00");
            Balance_TK.Text = model.Balance.ToString("0.00");
        }

        private void Date_TB_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                if (this.IsLoaded)
                {
                    GetAcconts();
                }
            }
            catch
            {

            }
        }

        private void PD_PrintPage(object sender, PrintPageEventArgs e)
        {
            try
            {
                e.HasMorePages = Print(e);
            }
            catch
            {

            }
        }

        private bool Print(PrintPageEventArgs e)
        {

            StringFormat Center = new StringFormat();
            Center.LineAlignment = StringAlignment.Center;
            Center.Alignment = StringAlignment.Center;
            StringFormat Right = new StringFormat();
            Right.LineAlignment = StringAlignment.Center;
            Right.Alignment = StringAlignment.Far; ;
            Font Header_Font = new Font("Cambria", 16);
            Font Cell_Font = new Font("Cambria", 14);
            Font Numbers_Font = new Font("Tahoma", 10);
            Brush Foreground = Brushes.Black;
            Pen Border_Pen = new Pen(Foreground, 2);
            Pen Cells_Pen = new Pen(Foreground, 1);
            Pen Spliter_Pen = new Pen(Foreground, 3);
            float Temp_Height = e.MarginBounds.Top, Temp_Width = e.MarginBounds.Left, pumps_height = 0;
            int j = 0;
            int[] Columns_width = new int[] { 30, 30, 110, 110, 72, 72, 78, 75, 75, 75 };
            try
            {
                if (_rowIndex == 0)
                {
                    e.Graphics.DrawString("Station Accounts For " + Date_TB.Value.Value.ToString("dd/MM/yyyy"), Header_Font, Foreground, new RectangleF(e.MarginBounds.Left, Temp_Height, e.MarginBounds.Width, 30), Center);
                    Temp_Height += 30;
                    e.Graphics.DrawString("Counter Reads", Header_Font, Foreground, new RectangleF(e.MarginBounds.Left, Temp_Height, 424, 30), Center);
                    e.Graphics.DrawString("Gas Balance", Header_Font, Foreground, new RectangleF(e.MarginBounds.Left + 424, Temp_Height, e.MarginBounds.Width - 424, 30), Center);
                    e.Graphics.DrawLine(Border_Pen, e.MarginBounds.Left, Temp_Height, e.MarginBounds.Right, Temp_Height);
                    Temp_Height += 30;
                    e.Graphics.DrawLine(Border_Pen, e.MarginBounds.Left, Temp_Height, e.MarginBounds.Right, Temp_Height);
                    for (int i = 1; i < 7; i++)
                    {
                        e.Graphics.DrawString(Pumps_DG.Columns[i].Header.ToString(), Cell_Font, Foreground, new RectangleF(Temp_Width, Temp_Height, Columns_width[i - 1], 45), Center);
                        Temp_Width += Columns_width[i - 1];
                    }
                    for (int i = 0; i < 4; i++)
                    {
                        e.Graphics.DrawString(Gas_DG.Columns[i].Header.ToString(), Cell_Font, Foreground, new RectangleF(Temp_Width, Temp_Height, Columns_width[i + 6], 45), Center);
                        Temp_Width += Columns_width[i + 6];
                    }
                    Temp_Height += 45;
                    e.Graphics.DrawLine(Cells_Pen, e.MarginBounds.Left, Temp_Height, e.MarginBounds.Right, Temp_Height);
                    Temp_Width = e.MarginBounds.Left;

                    foreach (DataRowView row in Pumps_DG.Items)
                    {
                        if (row[2].ToString() == "")
                            e.Graphics.FillRectangle(Brushes.DarkGray, new RectangleF(e.MarginBounds.Left, Temp_Height + 1, 424, 29));


                        for (int i = 1; i < 3; i++)
                        {
                            e.Graphics.DrawString(row[i].ToString(), Cell_Font, Foreground, new RectangleF(Temp_Width, Temp_Height, Columns_width[i - 1], 30), Center);
                            Temp_Width += Columns_width[i - 1];
                        }
                        for (int i = 3; i < 7; i++)
                        {
                            e.Graphics.DrawString(row[i].ToString(), Numbers_Font, Foreground, new RectangleF(Temp_Width, Temp_Height, Columns_width[i - 1] - 3, 30), Right);
                            Temp_Width += Columns_width[i - 1];
                        }
                        if (j < 5)
                        {
                            e.Graphics.DrawString(((DataRowView)Gas_DG.Items[j])[0].ToString(), Numbers_Font, Foreground, new RectangleF(Temp_Width, Temp_Height, Columns_width[6], 30), Center);
                            Temp_Width += Columns_width[6];

                            for (int i = 1; i < 4; i++)
                            {
                                e.Graphics.DrawString(((DataRowView)Gas_DG.Items[j])[i].ToString(), Numbers_Font, Foreground, new RectangleF(Temp_Width, Temp_Height, Columns_width[i + 6] - 3, 30), Right);
                                Temp_Width += Columns_width[i + 6];
                            }
                        }
                        Temp_Height += 30;
                        e.Graphics.DrawLine(Cells_Pen, e.MarginBounds.Left, Temp_Height, e.MarginBounds.Right, Temp_Height);
                        Temp_Width = e.MarginBounds.Left;
                        j++;
                    }
                    pumps_height = Temp_Height + 30;
                    foreach (int i in Columns_width)
                    {
                        Temp_Width += i;
                        e.Graphics.DrawLine(Cells_Pen, Temp_Width, e.MarginBounds.Top + 60, Temp_Width, Temp_Height);
                    }
                    e.Graphics.DrawLine(Border_Pen, e.MarginBounds.Left, e.MarginBounds.Top + 30, e.MarginBounds.Left, Temp_Height);
                    e.Graphics.DrawLine(Border_Pen, e.MarginBounds.Right, e.MarginBounds.Top + 30, e.MarginBounds.Right, Temp_Height);
                    e.Graphics.DrawLine(Spliter_Pen, e.MarginBounds.Left + 424, e.MarginBounds.Top + 30, e.MarginBounds.Left + 424, Temp_Height);
                    e.Graphics.DrawLine(Border_Pen, e.MarginBounds.Left, Temp_Height, e.MarginBounds.Right, Temp_Height);
                    _outcomeSales = App.Get_Printed_Table(new CPrinting.CPrinting(), Sales_DG, Outcome_DG, new string[] { "", "" });
                }
                Columns_width = new int[] { 245, 30, 79, 70, 200, 103 };
                Temp_Width = e.MarginBounds.Left;
                e.Graphics.DrawString("Outcome", Header_Font, Foreground, new RectangleF(e.MarginBounds.Left, Temp_Height, 424, 30), Center);
                e.Graphics.DrawString("Sales", Header_Font, Foreground, new RectangleF(e.MarginBounds.Left + 424, Temp_Height, e.MarginBounds.Width - 424, 30), Center);
                Temp_Height += 30;
                e.Graphics.DrawLine(Border_Pen, e.MarginBounds.Left, Temp_Height, e.MarginBounds.Right, Temp_Height);

                for (int i = 1; i < 5; i++)
                {
                    if (Sales_DG.Columns[i].Header.ToString() == "Gas")
                    {
                        e.Graphics.DrawString("Gas", new Font("Cambria", 10), Foreground, new RectangleF(Temp_Width, Temp_Height, Columns_width[i - 1], 35), Center);
                    }
                    else
                    {
                        e.Graphics.DrawString(Sales_DG.Columns[i].Header.ToString(), Cell_Font, Foreground, new RectangleF(Temp_Width, Temp_Height, Columns_width[i - 1], 35), Center);
                    }
                    Temp_Width += Columns_width[i - 1];
                }
                for (int i = 0; i < 2; i++)
                {
                    e.Graphics.DrawString(Outcome_DG.Columns[i].Header.ToString(), Cell_Font, Foreground, new RectangleF(Temp_Width, Temp_Height, Columns_width[i + 4], 35), Center);
                    Temp_Width += Columns_width[i + 4];
                }
                Temp_Height += 35;
                e.Graphics.DrawLine(Cells_Pen, e.MarginBounds.Left, Temp_Height, e.MarginBounds.Right, Temp_Height);
                while (_rowIndex < _outcomeSales.Rows.Count)
                {

                    if (Temp_Height + 30 > e.MarginBounds.Bottom)
                    {
                        Temp_Width = e.MarginBounds.Left;
                        foreach (int i in Columns_width)
                        {
                            Temp_Width += i;
                            e.Graphics.DrawLine(Cells_Pen, Temp_Width, pumps_height, Temp_Width, Temp_Height);
                        }
                        e.Graphics.DrawLine(Border_Pen, e.MarginBounds.Left, e.MarginBounds.Top + 375, e.MarginBounds.Left, Temp_Height);
                        e.Graphics.DrawLine(Border_Pen, e.MarginBounds.Right, e.MarginBounds.Top + 375, e.MarginBounds.Right, Temp_Height);
                        e.Graphics.DrawLine(Spliter_Pen, e.MarginBounds.Left + 424, e.MarginBounds.Top + 375, e.MarginBounds.Left + 424, Temp_Height);
                        e.Graphics.DrawLine(Border_Pen, e.MarginBounds.Left, Temp_Height, e.MarginBounds.Right, Temp_Height);
                        page_num++;
                        return true;
                    }

                    DataRow row = _outcomeSales.Rows[_rowIndex];

                    Temp_Width = e.MarginBounds.Left;
                    j = 0;
                    foreach (DataColumn column in _outcomeSales.Columns)
                    {
                        switch (j)
                        {
                            case 0:
                            case 4:
                                e.Graphics.DrawString(row[column].ToString(), new Font("Cambria", 12), Foreground, new RectangleF(Temp_Width, Temp_Height, Columns_width[j], 30), Center);
                                break;
                            default:
                                e.Graphics.DrawString(row[column].ToString(), Numbers_Font, Foreground, new RectangleF(Temp_Width - 3, Temp_Height, Columns_width[j], 30), Right);
                                break;
                        }
                        Temp_Width += Columns_width[j];
                        j++;
                    }
                    Temp_Height += 28;
                    e.Graphics.DrawLine(Cells_Pen, e.MarginBounds.Left, Temp_Height, e.MarginBounds.Right, Temp_Height);
                    _rowIndex++;
                }

                Temp_Width = e.MarginBounds.Left;
                if (page_num == 0)
                {
                    foreach (int i in Columns_width)
                    {
                        Temp_Width += i;
                        e.Graphics.DrawLine(Cells_Pen, Temp_Width, pumps_height, Temp_Width, Temp_Height);
                    }
                    e.Graphics.DrawLine(Border_Pen, e.MarginBounds.Left, e.MarginBounds.Top + 375, e.MarginBounds.Left, Temp_Height);
                    e.Graphics.DrawLine(Border_Pen, e.MarginBounds.Right, e.MarginBounds.Top + 375, e.MarginBounds.Right, Temp_Height);
                    e.Graphics.DrawLine(Spliter_Pen, e.MarginBounds.Left + 424, e.MarginBounds.Top + 375, e.MarginBounds.Left + 424, Temp_Height);
                    e.Graphics.DrawLine(Border_Pen, e.MarginBounds.Left, Temp_Height, e.MarginBounds.Right, Temp_Height);
                }
                else
                {
                    foreach (int i in Columns_width)
                    {
                        Temp_Width += i;
                        e.Graphics.DrawLine(Cells_Pen, Temp_Width, e.MarginBounds.Top + 30, Temp_Width, Temp_Height);
                    }
                    e.Graphics.DrawLine(Border_Pen, e.MarginBounds.Left, e.MarginBounds.Top, e.MarginBounds.Left, Temp_Height);
                    e.Graphics.DrawLine(Border_Pen, e.MarginBounds.Right, e.MarginBounds.Top, e.MarginBounds.Right, Temp_Height);
                    e.Graphics.DrawLine(Spliter_Pen, e.MarginBounds.Left + 424, e.MarginBounds.Top, e.MarginBounds.Left + 424, Temp_Height);
                    e.Graphics.DrawLine(Border_Pen, e.MarginBounds.Left, Temp_Height, e.MarginBounds.Right, Temp_Height);
                    e.Graphics.DrawLine(Border_Pen, e.MarginBounds.Left, e.MarginBounds.Top, e.MarginBounds.Right, e.MarginBounds.Top);

                }
                Temp_Height += 20;
                e.Graphics.DrawString("Money To be Deposit in bank: " + Total_Income_TK.Text, Header_Font, Foreground, new RectangleF(e.MarginBounds.Left, Temp_Height, e.MarginBounds.Width, 30));
                _rowIndex = 0;
                page_num = 0;
                return false;
            }
            catch
            {
                return false;
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _pd.DefaultPageSettings.Margins = new Margins(50, 50, 50, 50);
                CPrinting.CPrinting Printer = new CPrinting.CPrinting();
                Printer.PrintDocument = _pd;
                Printer.print();
            }
            catch
            {

            }
        }
    }
}
