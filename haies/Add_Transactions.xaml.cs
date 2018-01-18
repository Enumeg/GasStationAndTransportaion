using System;
using System.Data;
using System.Windows;
using Source;
using System.Drawing;

namespace haies
{
    /// <summary>
    /// Interaction logic for Add_Transactions.xaml
    /// </summary>
    public partial class Add_Transactions : Window
    {
        object Transaction_Id;
        int Dir;
        string title;
        string[] titles, signes, data;
        ToWord toWord;

        public Add_Transactions(int dir, object transaction_Id = null)
        {
            InitializeComponent();

            Pay_CB.SelectedIndex = 0;
            Dir = dir;
            Fill_Persons_CB();
            this.title = dir == 0 ? "سند  قبض \n\r PAYMENT VOUCHER" : "سند صرف \n\r RECEIPT VOUCHER";
            this.Title = dir == 0 ? "سند  قبض" : "سند صرف";
            Transaction_Id = transaction_Id;
            Person_TK.Text = dir == 0 ? "وصلني من" : "المستلم";
            if (dir == 1)
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
            if (transaction_Id != null)
            {
                Get_Transaction_Info();

            }
            else
                Get_Number();

        }

        private void Fill_Persons_CB()
        {
            var db = new DB();
            try
            {
                if (Dir == 0)
                {
                    db.AddCondition("per_status", Status.فعال);
                    db.Fill(Person_TB, "per_id", "per_name", "select p.*,c.* from persons p join customer c on c.cust_per_id=p.per_id order by per_name");
                }
                else
                {
                    db.AddCondition("per_status", Status.فعال);
                    db.Fill(Person_TB, "per_id", "per_name", @"select per_id,per_name,0 typ from persons p join car_owner c on c.cor_per_id=p.per_id where per_status =@per_status union all
                                                               select car_id,car_name,1 from cars order by per_name");

                }
            }
            catch (Exception)
            {


            }
        }
        private void Get_Transaction_Info()
        {
            try
            {
                DB db = new DB("transactions");

                db.AddCondition("trc_id", Transaction_Id);

                DataRow DR = db.SelectRow("select t.*,c.* from transactions t left join cheque c on chq_trc_id=trc_id where trc_id=@trc_id");
                Date_DTP.Value = DateTime.Parse(DR["trc_date"].ToString());
                Value_TB.Text = DR["trc_value"].ToString();
                Description_TB.Text = DR["trc_description"].ToString();
                Number_TB.Text = DR["trc_number"].ToString();
                Pay_CB.SelectedIndex = int.Parse(DR["trc_pay"].ToString());
                if (DR["trc_ref"].ToString() == "2")
                    Person_TB.Text = DR["trc_person"].ToString();
                else
                    Person_TB.SelectedValue = DR["trc_person"];

                if (Pay_CB.SelectedIndex == 1)
                {
                    Chq_Number_TB.Tag = DR["chq_id"];
                    Chq_Date_DTP.Value = DateTime.Parse(DR["chq_date"].ToString());
                    Chq_Number_TB.Text = DR["chq_number"].ToString();
                    Bank_TB.Text = DR["chq_bank"].ToString();

                }

            }
            catch
            {


            }
        }

        private bool Add_Update()
        {
            try
            {
                //create new DB giving table name
                DB DataBase = new DB("transactions");
                //add columns names and inserted or update values
                DataBase.AddColumn("trc_number", Number_TB.Text);
                DataBase.AddColumn("trc_date", Date_DTP.Value.Value.Date);
                DataBase.AddColumn("trc_value", Value_TB.Text);
                DataBase.AddColumn("trc_description", Description_TB.Text);
                DataBase.AddColumn("trc_pay", Pay_CB.SelectedIndex);
                if (Person_TB.SelectedValue != null)
                {
                    DataBase.AddColumn("trc_person", Person_TB.SelectedValue);
                    if (Dir == 1 && ((DataRowView)Person_TB.SelectedItem)["typ"].ToString() == "1")
                    {
                        DataBase.AddColumn("trc_ref", 1);
                    }
                }
                else
                {
                    DataBase.AddColumn("trc_person", Person_TB.Text);
                    DataBase.AddColumn("trc_ref", 2);
                }
                
                //check if id is null if null insert else update
                if (Transaction_Id == null)
                {

                    DataBase.AddColumn("trc_direction", Dir);
                    DataBase.AddColumn("trc_user_id", 6);//App.User_Id

                    //check if this item has inserted before 
                    if (DataBase.IsNotExist("trc_id", "trc_direction", "trc_number"))
                    {
                        if (Pay_CB.SelectedIndex == 1)
                        {
                            if (DataBase.Insert())
                            {
                                var db = SetCheque();
                                db.AddColumn("chq_trc_id", DataBase.Last_Inserted);
                                return db.Insert();
                            }
                            else
                                return false;
                        }
                        else
                        {

                            return DataBase.Insert();
                        }
                    }
                    else
                    {
                        Message.Show("هذا المستند موجود من قبل", MessageBoxButton.OK, 5);
                        return false;
                    }
                }
                else
                {
                    // update row giving the id
                    DataBase.AddCondition("trc_id", Transaction_Id);
                    if (Pay_CB.SelectedIndex == 1)
                    {
                        var db = SetCheque();
                        db.AddColumn("chq_trc_id", Transaction_Id);
                        if (Chq_Number_TB.Tag != null)
                            db.AddCondition("chq_id", Chq_Number_TB.Tag);

                        return db.Execute_Queries(DataBase, db);
                    }
                    else
                    {
                        if (Chq_Number_TB.Tag != null)
                        {
                            var db = new DB("cheque");
                            db.AddCondition("chq_id", Chq_Number_TB.Tag);
                            return db.Execute_Queries(DataBase, db);
                        }
                        else
                            return DataBase.Update();
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        private void Get_Number()
        {
            try
            {
                var db = new DB();
                db.AddCondition("trc_direction", Dir);
                int number = int.Parse(db.Select("select Coalesce(max(trc_number),0) +1 from transactions ").ToString());
                Number_TB.Text = number.ToString().PadLeft(5, '0');
            }
            catch (Exception)
            {

            }
        }
        private DB SetCheque()
        {
            var db = new DB("cheque");
            db.AddColumn("chq_number", Chq_Number_TB.Text);
            db.AddColumn("chq_date", Chq_Date_DTP.Value.Value.Date);
            db.AddColumn("chq_bank", Bank_TB.Text);
            return db;
        }
        private void Add_Update_Transaction_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                //Check for required inputs

                //try to add or update return true if ok
                if (Confirm.Check(Add_Update()))
                {
                    var log = new Log();
                    log.Columns.Add(new Column("التاريـخ", Date_DTP.Value.Value.Date.ToShortDateString()));
                    log.Columns.Add(new Column("الرقــــم", Number_TB.Text));
                    log.Columns.Add(new Column("القيـمــة", Value_TB.Text));
                    log.Columns.Add(new Column("البيـــان", Description_TB.Text));
                    log.Columns.Add(new Column(Person_TK.Text, Person_TB.Text));
                    log.Columns.Add(new Column("النوع", Pay_CB.Text));
                    log.CreateLog(Dir == 0 ? "إذن قبض" : "إذن صرف", Transaction_Id == null);
                    Print();

                    //    //check New checkBox status
                    if ((bool)New.IsChecked)
                    {

                    }
                    else
                    {
                        //close window
                        this.Close();
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
                printer.print();
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        void PrintDocument_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            toWord = new ToWord(decimal.Parse(Value_TB.Text), new CurrencyInfo(CurrencyInfo.Currencies.SaudiArabia));
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
            var values = Value_TB.Text.Contains(".") ? Value_TB.Text.Split('.') : new string[] { Value_TB.Text, "00" };
            data[0] = Person_TB.Text; data[data.Length - 1] = Description_TB.Text;
            data[data.Length - 2] = Pay_CB.SelectedIndex == 0 ? Pay_CB.Text :
                string.Format("  رقم: {0}   تاريخ: {1}   بنك: {2}", Chq_Number_TB.Text, Chq_Date_DTP.Value.Value.ToString("dd/MM/yyyy"), Bank_TB.Text);
            data[data.Length - 3] = toWord.ConvertToArabic();
            if (data.Length > 4)
                data[1] = App.User.Name;
            e.Graphics.DrawImage(Image.FromFile(".\\header.jpg"), e.MarginBounds.Left, e.MarginBounds.Top, e.MarginBounds.Width, 160);
            e.Graphics.DrawString("هلله", new Font("Arial", 12), Brushes.Black, e.MarginBounds.Right - 40, currheight, right);
            e.Graphics.DrawString("ريال", new Font("Arial", 12), Brushes.Black, e.MarginBounds.Right - 100, currheight, right);
            e.Graphics.DrawString(values[1], new Font("Arial", 12), Brushes.Black, e.MarginBounds.Right - 35, currheight + 35, right);
            e.Graphics.DrawString(values[0], new Font("Arial", 12), Brushes.Black, e.MarginBounds.Right - 110, currheight + 35, right);

            e.Graphics.DrawString(this.title, new Font("Arial", 14), Brushes.Black, new RectangleF(e.MarginBounds.Left, currheight, e.MarginBounds.Width, 70), center);
            e.Graphics.DrawString("التاريخ :" + Date_DTP.Value.Value.ToShortDateString(), new Font("Arial", 14), Brushes.Black, e.MarginBounds.Left + 20, currheight, left);
            e.Graphics.DrawString(Number_TB.Text, new Font("Tahoma", 20), Brushes.Black, e.MarginBounds.Left + 20, currheight + 35, left);

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
            e.Graphics.DrawImage(Image.FromFile(".\\rounded.png"), new Rectangle((int)e.MarginBounds.Left, 160, (int)e.MarginBounds.Width, boxheight));

            currheight += 20;

            e.Graphics.DrawLine(new Pen(Brushes.Black, 1f), e.MarginBounds.Left, currheight, e.MarginBounds.Width, currheight);

            currheight += 20;

            e.Graphics.DrawImage(Image.FromFile(".\\header.jpg"), e.MarginBounds.Left, currheight, e.MarginBounds.Width, 160);
            currheight += 190;
            e.Graphics.DrawString("هلله", new Font("Arial", 12), Brushes.Black, e.MarginBounds.Right - 40, currheight, right);
            e.Graphics.DrawString("ريال", new Font("Arial", 12), Brushes.Black, e.MarginBounds.Right - 100, currheight, right);
            e.Graphics.DrawString(values[1], new Font("Arial", 12), Brushes.Black, e.MarginBounds.Right - 35, currheight + 35, right);
            e.Graphics.DrawString(values[0], new Font("Arial", 12), Brushes.Black, e.MarginBounds.Right - 110, currheight + 35, right);

            e.Graphics.DrawString(this.title, new Font("Arial", 14), Brushes.Black, new RectangleF(e.MarginBounds.Left, currheight, e.MarginBounds.Width, 70), center);
            e.Graphics.DrawString("التاريخ :" + Date_DTP.Value.Value.ToShortDateString(), new Font("Arial", 14), Brushes.Black, e.MarginBounds.Left + 20, currheight, left);
            e.Graphics.DrawString(Number_TB.Text, new Font("Tahoma", 20), Brushes.Black, e.MarginBounds.Left + 20, currheight + 35, left);

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
            e.Graphics.DrawImage(Image.FromFile(".\\rounded.png"), new Rectangle((int)e.MarginBounds.Left, boxheight + 345, (int)e.MarginBounds.Width, boxheight));

        }

        private void Pay_CB_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (Pay_CB.SelectedIndex == 1)
            {
                Main_GD.RowDefinitions[6].Height = Main_GD.RowDefinitions[7].Height = Main_GD.RowDefinitions[8].Height = new GridLength(35);
            }
            else
            {
                Main_GD.RowDefinitions[6].Height = Main_GD.RowDefinitions[7].Height = Main_GD.RowDefinitions[8].Height = new GridLength(0);
            }
        }

    }
}
