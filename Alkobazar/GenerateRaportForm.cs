using Alkobazar.model;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Alkobazar
{
    public partial class GenerateRaportForm : Form
    {
        private alkoDbEntities db = new alkoDbEntities();

        public GenerateRaportForm()
        {
            InitializeComponent();
        }



        private void button_cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button_generate_Click(object sender, EventArgs e)
        {
            int yPoint = 170;
            PdfDocument document = new PdfDocument();
            document.Info.Title = "Raport";
            PdfPage page = document.AddPage();
            PdfSharp.Drawing.XGraphics gfx = XGraphics.FromPdfPage(page);
            XFont font_bold = new XFont("Verdana", 10, XFontStyle.BoldItalic);

            DateTime date_from = dateTimePicker_from.Value;
            DateTime date_to = dateTimePicker_to.Value;

           var orders =  db.orders.Where(o => o.deadline >= date_from && o.deadline <= date_to);

            if (!orders.Any())
            {
                MessageBox.Show("There is no orders added to raport !");
            }
            else
            {
                XFont font_regular = new XFont("Verdana", 10, XFontStyle.Bold);



                gfx.DrawString("Raport from: " + date_from.ToString("d") + " to: " +date_to.ToString("d"), font_regular, XBrushes.Black, new XRect(60, 80, page.Width.Point, page.Height.Point), XStringFormats.TopLeft);

                gfx.DrawString("Created: ", font_regular, XBrushes.Black, new XRect(60, 150, page.Width.Point, page.Height.Point), XStringFormats.TopLeft);
                gfx.DrawString("Deadline: ", font_regular, XBrushes.Black, new XRect(160, 150, page.Width.Point, page.Height.Point), XStringFormats.TopLeft);
                gfx.DrawString("Customer: ", font_regular, XBrushes.Black, new XRect(260, 150, page.Width.Point, page.Height.Point), XStringFormats.TopLeft);
                gfx.DrawString("Employee: ", font_regular, XBrushes.Black, new XRect(360, 150, page.Width.Point, page.Height.Point), XStringFormats.TopLeft);
                font_regular = new XFont("Verdana", 10, XFontStyle.Regular);

                foreach (order order in orders)
                {
                    gfx.DrawString(order.create_timestamp.ToString("d"), font_regular, XBrushes.Black, new XRect(60, yPoint, page.Width.Point, page.Height.Point), XStringFormats.TopLeft);
                    gfx.DrawString(order.deadline.ToString("d"), font_regular, XBrushes.Black, new XRect(160, yPoint, page.Width.Point, page.Height.Point), XStringFormats.TopLeft);
                    gfx.DrawString(order.customer.company_name, font_regular, XBrushes.Black, new XRect(260, yPoint, page.Width.Point, page.Height.Point), XStringFormats.TopLeft);
                    gfx.DrawString(order.employee.firstname +" "+ order.employee.lastname, font_regular, XBrushes.Black, new XRect(360, yPoint, page.Width.Point, page.Height.Point), XStringFormats.TopLeft);
                    gfx.DrawLine(XPens.Black, 0, yPoint, 1000, yPoint);
                    yPoint += 30;
                }
                gfx.DrawLine(XPens.Black, 0, yPoint, 1000, yPoint);

                SaveFileDialog sfd = new SaveFileDialog
                {
                    Filter = "PDF (*.pdf)|*.pdf",
                    FileName = "Output.pdf"
                };
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        document.Save(sfd.FileName);
                        this.Close();
                    }
                    catch (Exception ex)
                    {
                        Console.Write(ex.Message);
                        MessageBox.Show("You cannot generate raport unless previous one is closed !");
                    }
                }                     
                
            }
            
            }


    }
}
