using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using Source;
using System.Windows.Media;
using System.Windows.Controls;
using Microsoft.Windows.Controls;
using System.Globalization;
using System.Windows.Data;
using System.Drawing;
using System.Windows.Input;

namespace haies
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    /// 
    public enum Actions
    {
        Insert, Update, Delete
    }
    public enum Future
    {
        All, Customers, Clients
    };

    public enum Tax_Bouns
    {
        Bouns, Tax, Due, Advance
    };

    public enum Deal_Types
    {
        الصافي, الإجمالي, الشركة
    };

    public enum Maintenance_Types
    {
        شهرى, بالقطعه
    };

    public enum Cement_Units
    {
        طن, كيس
    };

    public enum Operations
    {
        Add, Edit, View
    };

    public enum Customer_type
    {
        مصنع, محطة
    };

    public enum Customer_Payment
    {
        نقدي, أجل, مقدم
    };

    public enum Edit_Mode
    {
        Add, Edit, Change_Password
    };

    public enum Pages
    {
        Cement, Transportations, Station, Employees, Users
    };

    public enum Account_Types
    {
        Salary, Motivations, Reward, Advance_Off, Discount_Off, Due_Off,
        Vacation_Off, Receive_Salary, Receive_Motivations, Receive_Reward,
        Advance, Discount, Due, Vacation
    };

    public enum Status
    {
        أرشيف, فعال
    }

    public partial class App : Application
    {
        public static User User { get; set; }
        public static DataTable Accounts;
        //public static object User_Id;
        //public static object gr_id;
        public static bool Cement_IsLoaded = false;
        public static bool Cars_IsLoaded = false;
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
         
            CultureInfo c = new CultureInfo("ar-EG");
            NumberFormatInfo n = new NumberFormatInfo();
            n.DigitSubstitution = DigitShapes.NativeNational;
            c.NumberFormat = n;
            System.Threading.Thread.CurrentThread.CurrentCulture = c;
            Source.DB.ConnectionString = DB.ConnectionString = ConfigurationManager.ConnectionStrings["haies.Properties.Settings.Con"];
            Confirm.True_String = "تم الحفظ بنجاح";
            Confirm.False_String = "حدث خطأ ما يرجى إعادة المحاولة";
            Confirm.Flow_Direction = FlowDirection.RightToLeft;
            Message.Language = Language.Arabic;
            Accounts = new DataTable();
            Accounts.Columns.Add("Name");
            Accounts.Columns.Add("Id1");
            Accounts.Columns.Add("Id2");
            Accounts.Rows.Add("المكافآت", (int)Account_Types.Reward + 1, (int)Account_Types.Receive_Reward + 1);
            Accounts.Rows.Add("الحسومات", (int)Account_Types.Discount + 1, (int)Account_Types.Discount_Off + 1);
            Accounts.Rows.Add("المستحقات", (int)Account_Types.Due_Off + 1, (int)Account_Types.Due + 1);
            Accounts.Rows.Add("السلف", (int)Account_Types.Advance_Off + 1, (int)Account_Types.Advance + 1);
            Source.DB.OpenConnection();
            var l =new Main(); //new Main(); 
            l.ShowDialog();


        }
        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }
        public static void Set_Style(DependencyObject Container, Operations operation)
        {
            try
            {
                foreach (TextBox tb in FindVisualChildren<TextBox>(Container))
                {
                    tb.Style = operation == Operations.View ? (Style)Application.Current.FindResource("View_TextBox") : (Style)Application.Current.FindResource("Edit_TextBox");
                    if (operation == Operations.Add)
                    {
                        tb.Clear();
                    }
                }
                foreach (ComboBox tb in FindVisualChildren<ComboBox>(Container))
                {
                    tb.Style = operation == Operations.View ? (Style)Application.Current.FindResource("View_ComboBox") : (Style)Application.Current.FindResource("Edit_ComboBox");
                    if (operation == Operations.Add)
                    {
                        tb.SelectedIndex = -1;
                    }
                }
                foreach (DateTimePicker tb in FindVisualChildren<DateTimePicker>(Container))
                {
                    tb.Style = operation == Operations.View ? (Style)Application.Current.FindResource("View_DateTimePicker") : operation == Operations.Add ?
                        (Style)Application.Current.FindResource("Default_DateTimePicker") : (Style)Application.Current.FindResource("Empty_DateTimePicker");
                }

            }
            catch
            {

            }
        }
        public static void Clear_Form(DependencyObject Container)
        {
            try
            {
                foreach (TextBox tb in FindVisualChildren<TextBox>(Container))
                {
                    if (tb.GetType() == typeof(Microsoft.Windows.Controls.WatermarkTextBox))
                    {
                        continue;
                    }
                    tb.Clear();
                }
                foreach (ComboBox tb in FindVisualChildren<ComboBox>(Container))
                {
                    tb.SelectedIndex = -1;
                }
            }
            catch
            {

            }
        }
        public static DataTable Get_Printed_Table(CPrinting.CPrinting Printer, DataGrid DG1, DataGrid DG2 = null, string[] Groups = null)
        {
            DataTable Printed_Table = new DataTable();
            string column_name = "";
            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Center;
            sf.LineAlignment = StringAlignment.Center;
            Type col_type = typeof(string);
            try
            {
                Printer.printedDataTable.Add(Printed_Table);
                Printer.BackgroundImage = System.Drawing.Image.FromFile(@".\PL.jpeg");
                Printer.PrintDocument.DefaultPageSettings.Margins = new System.Drawing.Printing.Margins(40, 65, 230, 100);
                Printer.PrintDocument.DefaultPageSettings.Margins.Top = Printer.PrintDocument.DefaultPageSettings.Landscape ? 165 : 230;
                if (DG2 == null)
                {
                    foreach (DataGridColumn dgc in DG1.Columns)
                    {
                        if (dgc.Visibility == Visibility.Visible && dgc.GetType() == typeof(DataGridTextColumn))
                        {
                            column_name = ((Binding)((DataGridTextColumn)dgc).Binding).Path.Path;
                            col_type = ((DataView)DG1.ItemsSource).Table.Columns[column_name].DataType;
                            Printed_Table.Columns.Add(column_name, col_type);
                            Printed_Table.Columns[column_name].Caption = dgc.Header.ToString();
                            if (col_type == typeof(DateTime))
                            {
                                Printer.columnFormat.Add(column_name, "{0:yyyy/MM/dd}");
                            }
                            if (col_type == typeof(decimal))
                            {
                                Printer.columnsDirection.Add(column_name, sf);
                            }
                        }
                    }
                    foreach (DataRowView row in DG1.ItemsSource)
                    {
                        Printed_Table.Rows.Add();
                        foreach (DataColumn col in Printed_Table.Columns)
                        {
                            Printed_Table.Rows[Printed_Table.Rows.Count - 1][col] = row[col.ColumnName];
                        }
                    }

                }
                else
                {
                    DataView DV1 = DG1.Items.Count < DG2.Items.Count ? DG1.ItemsSource as DataView : DG2.ItemsSource as DataView;
                    DataView DV2 = DG1.Items.Count >= DG2.Items.Count ? DG1.ItemsSource as DataView : DG2.ItemsSource as DataView;
                    DataTable DT1 = DV1.Table; DataTable DT2 = DV2.Table;
                    foreach (DataGridTextColumn dgc in DG1.Columns)
                    {
                        if (dgc.Visibility == Visibility.Visible)
                        {
                            column_name = ((Binding)dgc.Binding).Path.Path;
                            col_type = ((DataView)DG1.ItemsSource).Table.Columns[column_name].DataType;
                            Printed_Table.Columns.Add(column_name, col_type);
                            Printed_Table.Columns[column_name].Caption = dgc.Header.ToString();
                            Printer.Groups.Add(column_name, Groups[0]);
                            if (col_type == typeof(DateTime))
                            {
                                Printer.columnFormat.Add(column_name, "{0:yyyy/MM/dd}");
                            }
                            if (col_type == typeof(decimal))
                            {
                                Printer.columnsDirection.Add(column_name, sf);
                            }
                        }
                    }
                    foreach (DataGridTextColumn dgc in DG2.Columns)
                    {
                        if (dgc.Visibility == Visibility.Visible)
                        {
                            column_name = ((Binding)dgc.Binding).Path.Path;
                            col_type = ((DataView)DG2.ItemsSource).Table.Columns[column_name].DataType;
                            Printed_Table.Columns.Add(column_name, col_type);
                            Printed_Table.Columns[column_name].Caption = dgc.Header.ToString();
                            Printer.Groups.Add(column_name, Groups[1]);
                            if (col_type == typeof(DateTime))
                            {
                                Printer.columnFormat.Add(column_name, "{0:yyyy/MM/dd}");
                            }
                            if (col_type == typeof(decimal))
                            {
                                Printer.columnsDirection.Add(column_name, sf);
                            }
                        }
                    }
                    for (int i = 0; i < DV1.Count; i++)
                    {
                        Printed_Table.Rows.Add();
                        foreach (DataColumn col in Printed_Table.Columns)
                        {
                            if (DT1.Columns.Contains(col.ColumnName))
                            {
                                Printed_Table.Rows[i][col] = DT1.Rows[i][col.ColumnName];
                            }
                            else
                            {
                                Printed_Table.Rows[i][col] = DT2.Rows[i][col.ColumnName];
                            }
                        }
                    }
                    for (int i = DV1.Count; i < DV2.Count; i++)
                    {
                        Printed_Table.Rows.Add();
                        foreach (DataColumn col in Printed_Table.Columns)
                        {
                            if (DT2.Columns.Contains(col.ColumnName))
                            {
                                Printed_Table.Rows[i][col] = DT2.Rows[i][col.ColumnName];
                            }
                        }
                    }


                }
            }
            catch
            {

            }
            return Printed_Table;
        }

    }
}
