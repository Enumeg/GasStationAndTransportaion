using System;
using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Source;
using System.Drawing;

namespace haies
{
    /// <summary>
    /// Interaction logic for daily_report.xaml
    /// </summary>
    public partial class Transactions : Page
    {
        int Dir;
        string title;
        string[] titles, signes, data;
        ToWord toWord;

        public Transactions()
        {
            InitializeComponent();
            Type_CB.SelectedIndex = 0;
            Get_Safe();

        }
        private void Get_Safe()
        {
            try
            {
                decimal balance = Get_Balance();
                DB db = new DB();
                db.AddCondition("trc_date", From_DTP.Value.Value.Date, false, ">=", "SD");
                db.AddCondition("trc_date", To_DTP.Value.Value.Date, false, "<=", "ED");
                var dt = db.SelectTable(@"select * from transactions where trc_date>=@SD and trc_date<=@ED");
                dt.Columns.Add("Balance"); dt.Columns.Add("income"); dt.Columns.Add("outcome"); dt.Columns.Add("Type"); dt.Columns.Add("Person");
                var dr = dt.NewRow();
                dr["Balance"] = balance;
                dr["trc_description"] = "رصيد ما قبله";
                dr["trc_date"] = From_DTP.Value.Value.Date.AddDays(-1);
                dt.Rows.InsertAt(dr, 0);
                foreach (DataRow row in dt.Rows)
                {
                    row["Type"] = row["trc_direction"].ToString() == "" ? "" : row["trc_direction"].ToString() == "0" ? "قبض" : "صرف";
                    var reff = row["trc_ref"].ToString();

                    if (reff == "2")
                    {
                        row["Person"] = row["trc_person"];
                    }
                    else if (reff == "1")
                    {
                        row["Person"] = Get_Name("cars", "car", row["trc_person"].ToString());
                    }
                    else if (reff == "")
                    {

                    }
                    else
                    {
                        row["Person"] = Get_Name("persons", "per", row["trc_person"].ToString());
                    }
                    if (row["trc_description"].ToString() == "رصيد ما قبله")
                    {
                        continue;
                    }
                    if (row["trc_direction"].ToString() == "1")
                    {
                        row["outcome"] = row["trc_value"];
                        balance -= decimal.Parse(row["trc_value"].ToString());
                    }
                    else
                    {
                        row["income"] = row["trc_value"];
                        balance += decimal.Parse(row["trc_value"].ToString());

                    }
                    row["Balance"] = balance;

                }
                if (dt.Rows.Count > 0)
                    dt.Rows.Add(null, null, To_DTP.Value.Value.Date, null, "رصيد ما بعده", null, null, null, null, null, balance);


                Safe_DG.ItemsSource = dt.DefaultView;

            }
            catch (Exception ex)
            {

            }

        }

        private decimal Get_Balance()
        {
            decimal balance = 0;
            try
            {
                DB db = new DB();
                db.AddCondition("trc_date", From_DTP.Value.Value.Date, false, "<", "SD");
                var value = db.Select(@"select COALESCE(sum(trc_value),0) -(select COALESCE(sum(trc_value),0) from transactions where trc_direction = 1 and trc_date<@SD) 
                                          from transactions where trc_direction = 0 and trc_date<@SD ");
                balance = decimal.Parse(value.ToString());


            }
            catch
            {

            }
            return balance;
        }

        private void Get_Transactions()
        {
            try
            {
                DB db = new DB();
                db.AddCondition("trc_date", From_DTP.Value.Value.Date, false, ">=", "SD");
                db.AddCondition("trc_date", To_DTP.Value.Value.Date, false, "<=", "ED");
                db.AddCondition("trc_direction", Dir);
                var dt = db.SelectTable(@"select t.*,c.* from transactions t left join cheque c on chq_trc_id=trc_id 
                                            where trc_date>=@SD and trc_date<=@ED and trc_direction=@trc_direction");
                dt.Columns.Add("Pay"); dt.Columns.Add("Person");
                foreach (DataRow row in dt.Rows)
                {
                    if (int.Parse(row["trc_pay"].ToString()) == 1)
                    {
                        row["Pay"] = string.Format("{0} | {1} | {2}", row["chq_number"], row["chq_date"], row["chq_bank"]);
                    }
                    else
                    {
                        row["Pay"] = "نقداً";
                    }
                    var reff = int.Parse(row["trc_ref"].ToString());
                    if (reff == 2)
                    {
                        row["Person"] = row["trc_person"];
                    }
                    else if (reff == 1)
                    {
                        row["Person"] = Get_Name("cars", "car", row["trc_person"].ToString());
                    }
                    else
                    {
                        row["Person"] = Get_Name("persons", "per", row["trc_person"].ToString());
                    }
                }
                if (dt.Rows.Count > 0)
                    dt.Rows.Add(null, null, To_DTP.Value.Value.Date, dt.Compute("Sum(trc_value)", ""), "الإجمالي", null, null);


                Transactions_DG.ItemsSource = dt.DefaultView;

            }
            catch
            {

            }
        }

        private string Get_Name(string table, string prefix, string id)
        {
            string name = "";
            try
            {
                DB db = new DB();
                db.AddCondition(prefix + "_id", id);
                name = db.Select(string.Format("select {1}_name from {0}", table, prefix)).ToString();

            }
            catch (Exception)
            {

                throw;
            }
            return name;
        }
        private string Get_User(string Id)
        {
            try
            {
                var db = new DB();
                db.AddCondition("user_id", Id);
                return db.Select("select user_name from users").ToString();
            }
            catch (Exception)
            {

                return "";
            }
        }
        private void EP_Add(object sender, EventArgs e)
        {
            try
            {
                Add_Transactions F = new Add_Transactions(Dir);
                F.ShowDialog();
                Get_Transactions();
            }
            catch
            {

            }
        }

        private void EP_Edit(object sender, EventArgs e)
        {
            try
            {
                Add_Transactions F = new Add_Transactions(Dir, ((DataRowView)Transactions_DG.SelectedItem)["trc_id"]);
                F.ShowDialog();
                Get_Transactions();
            }
            catch
            {

            }
        }

        private void EP_Delete(object sender, EventArgs e)
        {
            try
            {
                if (Message.Show("هل تريد حذف هذا الحساب", MessageBoxButton.YesNoCancel, 5) == MessageBoxResult.Yes)
                {
                    var row = ((DataRowView)Transactions_DG.SelectedItem);

                    DB db = new DB("transactions");
                    db.AddCondition("trc_id", row["trc_id"]);
                    if (db.Delete())
                    {
                        var log = new Log();
                        log.Columns.Add(new Column("التاريـخ", row["trc_date"]));
                        log.Columns.Add(new Column("الرقــــم", row["trc_number"]));
                        log.Columns.Add(new Column("القيـمــة", row["trc_value"]));
                        log.Columns.Add(new Column("البيـــان", row["trc_description"]));
                        log.Columns.Add(new Column(Person_CB.Header.ToString(), row["trc_person"]));
                        log.Columns.Add(new Column("النوع", row["Pay"]));
                        log.CreateLog(Dir == 0 ? "إذن قبض" : "إذن صرف");
                        Get_Transactions();
                    }
                }
            }
            catch
            {

            }
        }

        private void From_DTP_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                if (this.IsLoaded)
                    if (Type_CB.SelectedIndex == 0)
                    {
                        Get_Safe();
                    }
                    else
                    {
                        Get_Transactions();
                    }
            }
            catch
            {

            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (this.IsLoaded)
                {
                    Dir = Type_CB.SelectedIndex - 1;
                    Person_CB.Header = Dir == 0 ? "وصلني من" : "المستلم";

                    if (Type_CB.SelectedIndex == 0)
                    {
                        Main_GD.RowDefinitions[2].Height = new GridLength(1, GridUnitType.Star);
                        Main_GD.RowDefinitions[5].Height = new GridLength(40);
                        Main_GD.RowDefinitions[3].Height = Main_GD.RowDefinitions[4].Height = new GridLength(0);
                        Get_Safe();
                    }
                    else
                    {
                        Main_GD.RowDefinitions[2].Height = Main_GD.RowDefinitions[5].Height = new GridLength(0);
                        Main_GD.RowDefinitions[3].Height = new GridLength(1, GridUnitType.Star);
                        Main_GD.RowDefinitions[4].Height = new GridLength(40);
                        Get_Transactions();
                    }
                }
            }
            catch
            {

            }
        }

        private void Print()
        {
            try
            {
                var printer = new CPrinting.CPrinting();
                printer.PrintDocument.PrintPage += PrintDocument_PrintPage;
                printer.PrintDocument.DefaultPageSettings.Margins = new System.Drawing.Printing.Margins(5, 5, 5, 5);
                this.title = Dir == 0 ? "سند  قبض \n\r PAYMENT VOUCHER" : "سند صرف \n\r RECEIPT VOUCHER";
                if (Dir == 1)
                {
                    titles = new string[] { "أنا الموقع أدناه", "I, The undersigned", "استلمت من", "Received from", "مبلغ وقدره", "The sum of", "نقداً/شيك", "cash/cheque", "وذلك مقابل", "on accountant of" };
                    signes = new string[] { "توقيع المحاسب", "توقيع المستلم" };
                    data = new string[5];
                }
                else
                {
                    titles = new string[] { "وصلني من", "Received from", "مبلغ وقدره", "The sum of", "نقداً/شيك", "cash/cheque", "وذلك مقابل", "on accountant of" };
                    signes = new string[] { "", "توقيع المستلم" };
                    data = new string[4];
                }
                printer.print();

            }
            catch (Exception ex)
            {

                throw;
            }
        }

        void PrintDocument_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            var trans = (DataRowView)Transactions_DG.SelectedItem;
            var transValue = trans["trc_value"].ToString();
            toWord = new ToWord(decimal.Parse(transValue), new CurrencyInfo(CurrencyInfo.Currencies.SaudiArabia));
            var right = new StringFormat(StringFormatFlags.DirectionRightToLeft);
            right.Alignment = StringAlignment.Near;
            right.LineAlignment = StringAlignment.Center;
            var center = new StringFormat(StringFormatFlags.DirectionRightToLeft);
            center.Alignment = StringAlignment.Center;
            center.LineAlignment = StringAlignment.Center;
            var left = new StringFormat();
            left.Alignment = StringAlignment.Near;
            left.LineAlignment = StringAlignment.Center;
            float currheight = 195f;
            var values = transValue.Contains(".") ? transValue.Split('.') : new string[] { transValue, "00" };
            data[0] = trans["Person"].ToString(); data[data.Length - 1] = trans["trc_description"].ToString();
            data[data.Length - 2] = trans["trc_pay"].ToString() == "0" ? "نقداً" :
                string.Format("  رقم: {0}   تاريخ: {1}   بنك: {2}", trans["chq_number"], trans["chq_date"], trans["chq_bank"]);
            data[data.Length - 3] = toWord.ConvertToArabic();
            if (data.Length > 4)
                data[1] = Get_User(trans["trc_user_id"].ToString());
            e.Graphics.DrawImage(System.Drawing.Image.FromFile(".\\header.jpg"), e.MarginBounds.Left, e.MarginBounds.Top, e.MarginBounds.Width, 160);
            e.Graphics.DrawString("هلله", new Font("Arial", 12), Brushes.Black, e.MarginBounds.Right - 40, currheight, right);
            e.Graphics.DrawString("ريال", new Font("Arial", 12), Brushes.Black, e.MarginBounds.Right - 100, currheight, right);
            e.Graphics.DrawString(values[1], new Font("Arial", 12), Brushes.Black, e.MarginBounds.Right - 35, currheight + 35, right);
            e.Graphics.DrawString(values[0], new Font("Arial", 12), Brushes.Black, e.MarginBounds.Right - 110, currheight + 35, right);

            e.Graphics.DrawString(this.title, new Font("Arial", 14), Brushes.Black, new RectangleF(e.MarginBounds.Left, currheight, e.MarginBounds.Width, 70), center);
            e.Graphics.DrawString("التاريخ :" + trans["trc_date"], new Font("Arial", 14), Brushes.Black, e.MarginBounds.Left + 20, currheight, left);
            e.Graphics.DrawString(trans["trc_number"].ToString(), new Font("Tahoma", 20), Brushes.Black, e.MarginBounds.Left + 20, currheight + 35, left);

            e.Graphics.DrawRectangle(new Pen(Brushes.Black, 1f), e.MarginBounds.Right - 70, currheight + 20, 40, 30);
            e.Graphics.DrawRectangle(new Pen(Brushes.Black, 1f), e.MarginBounds.Right - 180, currheight + 20, 90, 30);

            currheight += 100;
            for (int i = 0; i < titles.Length; i += 2)
            {
                e.Graphics.DrawString(titles[i], new Font("Arial", 14), Brushes.Black, e.MarginBounds.Right - 20, currheight, right);
                if (i / 2 == data.Length - 3)
                    e.Graphics.DrawString(data[i / 2], new Font("Arial", 12), Brushes.Black, e.MarginBounds.Right - 150, currheight, right);
                else
                    e.Graphics.DrawString(data[i / 2], new Font("Arial", 14), Brushes.Black, e.MarginBounds.Right - 150, currheight, right);
                e.Graphics.DrawString(titles[i + 1], new Font("Arial", 14), Brushes.Black, e.MarginBounds.Left + 20, currheight, left);
                currheight += 35;
            }
            currheight += 15;
            e.Graphics.DrawString(signes[0], new Font("Arial", 14), Brushes.Black, e.MarginBounds.Right - 50, currheight, right);
            e.Graphics.DrawString(signes[1], new Font("Arial", 14), Brushes.Black, e.MarginBounds.Width / 2 - 50, currheight, right);
            currheight += 35;
            var boxheight = (int)currheight - 150;
            e.Graphics.DrawImage(System.Drawing.Image.FromFile(".\\rounded.png"), new Rectangle((int)e.MarginBounds.Left, 160, (int)e.MarginBounds.Width, boxheight));

            currheight += 20;

            e.Graphics.DrawLine(new Pen(Brushes.Black, 1f), e.MarginBounds.Left, currheight, e.MarginBounds.Width, currheight);

            currheight += 20;

            e.Graphics.DrawImage(System.Drawing.Image.FromFile(".\\header.jpg"), e.MarginBounds.Left, currheight, e.MarginBounds.Width, 160);
            currheight += 190;
            e.Graphics.DrawString("هلله", new Font("Arial", 12), Brushes.Black, e.MarginBounds.Right - 40, currheight, right);
            e.Graphics.DrawString("ريال", new Font("Arial", 12), Brushes.Black, e.MarginBounds.Right - 100, currheight, right);
            e.Graphics.DrawString(values[1], new Font("Arial", 12), Brushes.Black, e.MarginBounds.Right - 35, currheight + 35, right);
            e.Graphics.DrawString(values[0], new Font("Arial", 12), Brushes.Black, e.MarginBounds.Right - 110, currheight + 35, right);

            e.Graphics.DrawString(this.title, new Font("Arial", 14), Brushes.Black, new RectangleF(e.MarginBounds.Left, currheight, e.MarginBounds.Width, 70), center);
            e.Graphics.DrawString("التاريخ :" + trans["trc_date"], new Font("Arial", 14), Brushes.Black, e.MarginBounds.Left + 20, currheight, left);
            e.Graphics.DrawString(trans["trc_number"].ToString(), new Font("Tahoma", 20), Brushes.Black, e.MarginBounds.Left + 20, currheight + 35, left);

            e.Graphics.DrawRectangle(new Pen(Brushes.Black, 1f), e.MarginBounds.Right - 70, currheight + 20, 40, 30);
            e.Graphics.DrawRectangle(new Pen(Brushes.Black, 1f), e.MarginBounds.Right - 180, currheight + 20, 90, 30);

            currheight += 100;
            for (int i = 0; i < titles.Length; i += 2)
            {
                e.Graphics.DrawString(titles[i], new Font("Arial", 14), Brushes.Black, e.MarginBounds.Right - 20, currheight, right);
                if (i / 2 == data.Length - 3)
                    e.Graphics.DrawString(data[i / 2], new Font("Arial", 12), Brushes.Black, e.MarginBounds.Right - 150, currheight, right);
                else
                    e.Graphics.DrawString(data[i / 2], new Font("Arial", 14), Brushes.Black, e.MarginBounds.Right - 150, currheight, right);
                e.Graphics.DrawString(titles[i + 1], new Font("Arial", 14), Brushes.Black, e.MarginBounds.Left + 20, currheight, left);
                currheight += 35;
            }
            currheight += 15;
            e.Graphics.DrawString(signes[0], new Font("Arial", 14), Brushes.Black, e.MarginBounds.Right - 50, currheight, right);
            e.Graphics.DrawString(signes[1], new Font("Arial", 14), Brushes.Black, e.MarginBounds.Width / 2 - 50, currheight, right);
            currheight += 35;
            e.Graphics.DrawImage(System.Drawing.Image.FromFile(".\\rounded.png"), new Rectangle((int)e.MarginBounds.Left, boxheight + 345, (int)e.MarginBounds.Width, boxheight));

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Print();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                var printer = new CPrinting.CPrinting();
                printer.Get_Printed_Table(Transactions_DG);
                printer.BackgroundImage = System.Drawing.Image.FromFile(@".\PL.jpeg");
                printer.PrintDocument.DefaultPageSettings.Margins.Top = printer.PrintDocument.DefaultPageSettings.Landscape ? 165 : 230;
                printer.print();
            }
            catch (Exception)
            {

            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            try
            {
                var printer = new CPrinting.CPrinting();
                printer.PrintDocument.DefaultPageSettings.Landscape = true;
                printer.columnsWidth.Add("Person", 300);
                printer.columnsWidth.Add("trc_description", 300);
                printer.PrintDocument.DefaultPageSettings.Margins = new System.Drawing.Printing.Margins(40, 40, 40, 40);
                printer.Get_Printed_Table(Safe_DG);
                printer.header.Add(string.Format(" اليوميه  من {0} إلى {1}", From_DTP.Value.Value.ToString("yyyy/MM/dd"), To_DTP.Value.Value.ToString("yyyy/MM/dd"))); ;
                printer.print();
            }
            catch (Exception)
            {

            }
        }



        private void Person_TB_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Type_CB.SelectedIndex == 0)
            {
                ((DataView)Safe_DG.ItemsSource).RowFilter = Person_TB.Text == "" ? "" : "Person LIKE '" + Person_TB.Text +"*'";            
            }
            else
            {
                ((DataView)Transactions_DG.ItemsSource).RowFilter = Person_TB.Text == "" ? "" : "Person LIKE '" + Person_TB.Text + "*'";          
            }


        }


    }
}
