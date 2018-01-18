using Bullzip.PdfWriter;
using Source;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using io = System.IO;
using System.Net.Mail;
using System.Net;
using System.ComponentModel;
namespace haies
{
    /// <summary>
    /// Interaction logic for View_Claims.xaml
    /// </summary>
    public partial class View_Claims : Window
    {
        Claim Claim;
        public View_Claims(Claim claim)
        {
            InitializeComponent();
            Claim = claim;
            Get_Claim();
        }

        private void Get_Claim()
        {
            try
            {
                decimal[] Totals;
                if (Claim.Type == Customer_type.مصنع)
                    Totals = CustomerAccounts.Get_Customers_Accounts(Claim.Type, Claim.Id,Claim.PersonId, Claim.From, Claim.To, Customers_Out_DG, Customers_In_DG);
                else
                    Totals = CustomerAccounts.Get_Customers_Accounts(Claim.Type, Claim.Id, Claim.PersonId, Claim.From, Claim.To, Customer_Out_DG, Customers_In_DG);

                Balance_Before_TB.Text = Totals[0].ToString("0.00");
                Total_TB.Text = Totals[1].ToString("0.00");
                Paid_TB.Text = Totals[2].ToString("0.00");
                Rest_TB.Text = Totals[3].ToString("0.00");
                Balance_After_TB.Text = Totals[4].ToString("0.00");
            }
            catch
            {

            }
        }
        private void generatePDF()
        {
            const string printerName = @"Bullzip PDF Printer";
            string tempFolder = Environment.CurrentDirectory + "\\";
            string sessionGuid = Claim.Id.ToString();

            //
            // Print to file
            //

            // Define printer postscript output file
            string postscriptFileName = tempFolder + sessionGuid + ".ps";

            // Use a PrintDocument to set up event handler and start the print
            var printer = new CPrinting.CPrinting(CPrinting.Directon.RightToLeft);
            //printer.header.Add(string.Format(" مطالبة  {0} \r\n عن الفترة من {1} إلى {2}", Claim.Name,
            // Claim.From.ToString("yyyy/MM/dd"), Claim.To.ToString("yyyy/MM/dd")));
            var ps = printer.PrintDocument.PrinterSettings.PaperSizes.Cast<PaperSize>().FirstOrDefault(b => b.Kind == PaperKind.A3);
            printer.PrintDocument.DefaultPageSettings.Landscape = true;
            printer.PrintDocument.DefaultPageSettings.PaperSize = ps;
            printer.PrintDocument.PrinterSettings.PrinterName = printerName;
            printer.PrintDocument.PrinterSettings.PrintToFile = true;
            printer.PrintDocument.PrinterSettings.PrintFileName = postscriptFileName;
            printer.Get_Printed_Table(Customers_Out_DG, Customers_In_DG, new string[] { "عدد الردود", "المدفوعات" });
            printer.FooterTable.Add("المستحق سـابقاً : ", Balance_Before_TB.Text);
            printer.FooterTable.Add("الإجمالــــــــــي : ", Total_TB.Text);
            printer.FooterTable.Add("المــدفــــــــــوع : ", Paid_TB.Text);
            printer.FooterTable.Add("البــــــــــــــاقـي : ", Rest_TB.Text);
            printer.FooterTable.Add("المستحق حاليـاً : ", Balance_After_TB.Text);
            printer.print();
            string pdfFileName = tempFolder + sessionGuid + ".pdf";
            string statusFileName = tempFolder + sessionGuid + "_status.ini";
            string settingsFileName = tempFolder + sessionGuid + "_settings.ini";

            // Create settings file
            PdfSettings settings = new PdfSettings();
            settings.PrinterName = printerName;
            settings.SetValue("Output", pdfFileName);
            settings.SetValue("ShowSettings", "Never");
            settings.SetValue("ShowSaveAS", "no");
            settings.SetValue("ShowProgress", "no");
            settings.SetValue("ShowProgressFinished", "no");
            settings.SetValue("ShowPDF", "no");
            settings.SetValue("ConfirmOverwrite", "no");
            settings.SetValue("StatusFileEncoding", "Unicode");
            settings.SetValue("RememberLastFileName", "no");
            settings.SetValue("RememberLastFolderName", "no");
            settings.SetValue("SuppressErrors", "no");
            settings.WriteSettings(settingsFileName);

            // Get the location of the gui.exe so that we can launch it
            string gui = PdfUtil.GetPrinterAppFolder(printerName) + @"\gui.exe";

            // Launch gui.exe
            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            processStartInfo.FileName = gui;
            processStartInfo.Arguments = string.Format("printer=\"{0}\" ps=\"{1}\", runonce=\"{2}\"",
                printerName, postscriptFileName, settingsFileName);
            Process p = Process.Start(processStartInfo);

            // Wait for the gui.exe process to exit
            DateTime starttime = DateTime.Now;
            while (!p.HasExited && DateTime.Compare(DateTime.Now, starttime.AddSeconds(30)) < 0)
            {
                System.Threading.Thread.Sleep(200);
            }
            Send();
            // Kill the process if it is still running
            if (!p.HasExited)
            {
                p.Kill();
                throw new Exception("Error creatin PDF file. The gui.exe process did not finish within the time limit.");
            }
        }

        private void Send()
        {
            string pdfFileName = Environment.CurrentDirectory + "\\" + Claim.Id + ".pdf";
            if (io.File.Exists(pdfFileName))
            {
                try
                {
                    DB db = new DB();
                    var row = db.SelectRow("select * from email");

                    List<string> info = new List<string>() { };
                    for (int i = 0; i < 6; i++)
                    {
                        info.Add(row[i].ToString());
                    }
                    SmtpClient s = new SmtpClient(info[1], int.Parse(info[2]));
                    s.EnableSsl = bool.Parse(info[5]);
                    s.Credentials = new NetworkCredential(info[3], info[4]);
                    MailMessage m = new MailMessage();
                    m.From = new MailAddress(info[3]);
                    m.To.Add(new MailAddress(Claim.Email));
                    m.Subject = "مطالبة";
                    m.Body = string.Format(" عن الفترة من {0} إلى {1}", Claim.From.ToString("yyyy/MM/dd"), Claim.To.ToString("yyyy/MM/dd"));
                    m.Attachments.Add(new Attachment(pdfFileName));
                    s.SendCompleted += new SendCompletedEventHandler(SendCompletedCallback);
                    s.SendAsync(m, m.Subject);


                }
                catch
                {

                }
            }
        }

        private static void SendCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                Message.Show("message Cancelled", MessageBoxButton.OK);
            }
            if (e.Error != null)
            {
                Message.Show(e.Error.Message, MessageBoxButton.OK);
            }
            else
            {

                Message.Show("Message sent.", MessageBoxButton.OK);
            }

        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            generatePDF();
        }

    }
}
