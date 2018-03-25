using Alkobazar.model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using System.ComponentModel.DataAnnotations;
using System.IO;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using System.Diagnostics;
using System.Data.Entity;

namespace Alkobazar
{
    public partial class AlkoForm : Form
    {
        private int _index = 0;
        private alkoDbEntities db = new alkoDbEntities();

        public AlkoForm()
        {
            InitializeComponent();
        }

        // INIT LOAD

        private void AlkoForm_Load(object sender, EventArgs e)
        {
            this.order_itemsTableAdapter.Fill(this.dataSet.order_items);
            this.ordersTableAdapter.Fill(this.dataSet.orders);
            this.employeesTableAdapter.Fill(this.dataSet.employees);
            this.customersTableAdapter.Fill(this.dataSet.customers);
            this.productsTableAdapter.Fill(this.dataSet.products);

        }

        // PRODUCTS 

        private void Button_clear_products_Click(object sender, EventArgs e)
        {
            text_quantity_in_stock.Text = "";
            text_product_description.Text = "";
            text_product_name.Text = "";
            text_price.Text = "";
            text_size_in_liters.Text = "";
            text_alcohol_content.Text = "";
        }

        private void Button_add_products_Click(object sender, EventArgs e)
        {
            bool flag = true;

            try
            {
                var product = new product
                {
                    quantityInStock = int.Parse(text_quantity_in_stock.Text.ToString()),
                    alcohol_content = Convert.ToDouble(text_alcohol_content.Text.ToString()),
                    sizeInLiters = Convert.ToDouble(text_size_in_liters.Text.ToString()),
                    description = text_product_description.Text.ToString(),
                    name = text_product_name.Text.ToString(),
                    price = Convert.ToDouble(text_price.Text.ToString())
                };

                if (!IsProductPresent(product.name, product.sizeInLiters))
                {
                    flag = false;
                    MessageBox.Show("There Is already items with this name and size in database ! ");
                }

                if (IsInputValid(product) && flag && ShowConfirmMessage("add", "product"))
                {

                    db.products.Add(product);
                    db.SaveChanges();
                    this.productsTableAdapter.Fill(this.dataSet.products);
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                MessageBox.Show("Check if given data is correct !");
            }


        }

        private void Button_update_products_Click(object sender, EventArgs e)
        {
            int product_id = Convert.ToInt32(Grid_products.Rows[_index].Cells[0].Value);
            var product = db.products.Where(p => p.id == product_id).First();

            try
            {
                product.quantityInStock = int.Parse(text_quantity_in_stock.Text);
                product.alcohol_content = Convert.ToDouble(text_alcohol_content.Text.ToString());
                product.sizeInLiters = Convert.ToDouble(text_size_in_liters.Text.ToString());
                product.description = text_product_description.Text;
                product.name = text_product_name.Text.ToString();
                product.price = Convert.ToDouble(text_price.Text);

                if (IsInputValid(product))
                {
                    var addConfirm = MessageBox.Show(@"Are you sure you want to update product" + "\n" +
                                                        "\n" + "from:" + "\n" + "\n" +
                                                        Grid_products.Rows[_index].Cells[1].Value + ", " + "\n" +
                                                        Grid_products.Rows[_index].Cells[2].Value + ", " + "\n" +
                                                        Grid_products.Rows[_index].Cells[3].Value + ", " + "\n" +
                                                        Grid_products.Rows[_index].Cells[4].Value + ", " + "\n" +
                                                        Grid_products.Rows[_index].Cells[5].Value + ", " + "\n" +
                                                        Grid_products.Rows[_index].Cells[6].Value + ", " + "\n" +
                                                        "\n" + " to :" + "\n" + "\n" +
                                                        text_product_name.Text.ToString() + ", " + "\n" +
                                                        Convert.ToDouble(text_alcohol_content.Text) + ", " + "\n" +
                                                        Convert.ToDouble(text_size_in_liters.Text) + ", " + "\n" +
                                                        Convert.ToDouble(text_price.Text) + ", " + "\n" +
                                                        text_product_description.Text + ", " + "\n" +
                                                        int.Parse(text_quantity_in_stock.Text)
                                                        , @"Confirm update",
                                                        MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (addConfirm == DialogResult.No)
                        return;

                    db.Entry(product).State = EntityState.Modified;
                    db.SaveChanges();
                    this.productsTableAdapter.Fill(this.dataSet.products);
                }
            }
            catch (FormatException ex)
            {
                Console.Write(ex.Message);
                MessageBox.Show("You cannot update product with incorrect data ! ");
            }
        }

        private void Button_delete_products_Click(object sender, EventArgs e)
        {

            if(ShowConfirmMessage("delete", "product"))
            {       
            int product_id = Convert.ToInt32(Grid_products.Rows[_index].Cells[0].Value);
            try
            {
                db.products.Remove(db.products.Where(p => p.id == product_id).First());
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                MessageBox.Show("Something went wrong !");
            }
            this.productsTableAdapter.Fill(this.dataSet.products);
            }
        }

        private void Grid_products_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                _index = e.RowIndex;
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }

            if (_index >= 0)
            {
                DataGridViewRow row = Grid_products.Rows[e.RowIndex];
                text_quantity_in_stock.Text = row.Cells["DataGridViewTextBoxColumn7"].Value.ToString();
                text_alcohol_content.Text = row.Cells["DataGridViewTextBoxColumn3"].Value.ToString();
                text_size_in_liters.Text = row.Cells["DataGridViewTextBoxColumn4"].Value.ToString();
                text_product_description.Text = row.Cells["DataGridViewTextBoxColumn6"].Value.ToString();
                text_product_name.Text = row.Cells["DataGridViewTextBoxColumn2"].Value.ToString();
                text_price.Text = row.Cells["DataGridViewTextBoxColumn5"].Value.ToString();
            }
        }

        private void Button_export_products_Click(object sender, EventArgs e)
        {
            SaveToCSV(Grid_products);
        }

        private void Button_import_products_Click(object sender, EventArgs e)
        {
            GetFromCSV("products");
        }

        public bool IsProductPresent(String name, double size)
        {
            var productCount = db.products.Where(p => p.name == name &&
                                                      p.sizeInLiters == size
                                                ).Count();
            bool result = productCount > 0 ? false : true;

            return result;
        }

        // CUSTOMERS

        private void Grid_customers_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                _index = e.RowIndex;
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }

            if (_index >= 0)
            {
                DataGridViewRow row = Grid_customers.Rows[e.RowIndex];
                text_shipment_address.Text = row.Cells["DataGridViewTextBoxColumn9"].Value.ToString();
                text_company_name.Text = row.Cells["DataGridViewTextBoxColumn8"].Value.ToString();
                text_phone_number.Text = row.Cells["DataGridViewTextBoxColumn10"].Value.ToString();
            }
        }

        private void Button_import_customers_Click(object sender, EventArgs e)
        {
            GetFromCSV("customers");
        }

        private void Button_export_customers_Click(object sender, EventArgs e)
        {
            SaveToCSV(Grid_customers);
        }

        private void Button_add_customers_Click(object sender, EventArgs e)
        {
            bool flag = true;
            try
            {
                var customer = new customer
                {
                    company_name = text_company_name.Text.ToString(),
                    shipment_address = text_shipment_address.Text.ToString(),
                    customer_phone = text_phone_number.Text.ToString()
                };

                if (!IsCustomerPresent(customer.company_name))
                {
                    flag = false;
                    MessageBox.Show("There is already customer with this name in database ! ");
                }

                if (IsInputValid(customer) && flag && ShowConfirmMessage("add", "customer"))
                {     
                    db.customers.Add(customer);
                    db.SaveChanges();
                    this.customersTableAdapter.Fill(this.dataSet.customers);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        public bool IsCustomerPresent(String name)
        {
            var customerCount = db.customers.Where(p => p.company_name == name).Count();
            bool result = customerCount > 0 ? false : true;

            return result;
        }

        private void Button_delete_customers_Click(object sender, EventArgs e)
        {
            if (ShowConfirmMessage("delete", "customer"))
            {
                int customer_id = Convert.ToInt32(Grid_customers.Rows[_index].Cells[0].Value);

                db.customers.Remove(db.customers.Where(p => p.id == customer_id).First());
                db.SaveChanges();
                this.customersTableAdapter.Fill(this.dataSet.customers);
            }
        }

        private void Button_clear_customers_Click(object sender, EventArgs e)
        {
            text_company_name.Text = "";
            text_shipment_address.Text = "";
            text_phone_number.Text = "";
        }

        private void Button_update_customers_Click(object sender, EventArgs e)
        {
            int customer_id = Convert.ToInt32(Grid_customers.Rows[_index].Cells[0].Value);
            var customer = db.customers.Where(p => p.id == customer_id).First();

            try
            {
                customer.company_name = text_company_name.Text.ToString();
                customer.shipment_address = text_shipment_address.Text.ToString();
                customer.customer_phone = text_phone_number.Text.ToString();

                if (IsInputValid(customer))
                {
                    var addConfirm = MessageBox.Show(@"Are you sure you want to update customer" + "\n" +
                                                        "\n" + "from:" + "\n" + "\n" +
                                                        Grid_customers.Rows[_index].Cells[1].Value + ", " + "\n" +
                                                        Grid_customers.Rows[_index].Cells[2].Value + ", " + "\n" +
                                                        Grid_customers.Rows[_index].Cells[3].Value + ", " + "\n" +
                                                        "\n" + " to :" + "\n" + "\n" +
                                                        customer.company_name + ", " + "\n" +
                                                        customer.shipment_address + ", " + "\n" +
                                                        customer.customer_phone
                                                        , @"Confirm update",
                                                        MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (addConfirm == DialogResult.No)
                        return;

                    db.Entry(customer).State = EntityState.Modified;
                    db.SaveChanges();
                    this.customersTableAdapter.Fill(this.dataSet.customers);
                }
            }
            catch (FormatException ex)
            {
                Console.Write(ex.Message);
                MessageBox.Show("You cannot update customer with incorrect data ! ");
            }
        }

        // EMPLOYEES

        private void Button_get_stats_Click(object sender, EventArgs e)
        {
            int employee_id = Convert.ToInt32(Grid_employees.Rows[_index].Cells[0].Value);
            Double totalValue = 0;

            var lIstOfOrders = db.orders.Where(o => o.employee_id == employee_id);

            foreach (order order in lIstOfOrders)
            {
                var listOfItems = db.order_items.Where(o => o.order_id == order.id);
                foreach (order_items item in listOfItems)
                {
                    totalValue += item.order_quantity * item.product.price;
                }
            }
            MessageBox.Show("Employee handled: " + lIstOfOrders.Count() + " orders" + "\n \n" + "Total value: " + totalValue);
        }

        private void Grid_employees_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                _index = e.RowIndex;
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }

            if (_index >= 0)
            {
                DataGridViewRow row = Grid_employees.Rows[e.RowIndex];
                text_firstname.Text = row.Cells["DataGridViewTextBoxColumn13"].Value.ToString();
                text_lastname.Text = row.Cells["DataGridViewTextBoxColumn14"].Value.ToString();
                text_phone.Text = row.Cells["DataGridViewTextBoxColumn16"].Value.ToString();
                text_address.Text = row.Cells["DataGridViewTextBoxColumn15"].Value.ToString();
                text_pesel.Text = row.Cells["DataGridViewTextBoxColumn17"].Value.ToString();
            }

        }

        private void Button_import_employees_Click(object sender, EventArgs e)
        {
            GetFromCSV("employees");
        }

        private void Button_export_employees_Click(object sender, EventArgs e)
        {
            SaveToCSV(Grid_employees);
        }

        private void Button_add_employees_Click(object sender, EventArgs e)
        {
            bool flag = true;
            try
            {
                var employee = new employee
                {
                    address = text_address.Text.ToString(),
                    firstname = text_firstname.Text.ToString(),
                    lastname = text_lastname.Text.ToString(),
                    pesel = text_pesel.Text.ToString(),
                    phone = text_phone.Text.ToString(),

                };

                if (!IsEmployeePresent(employee.pesel))
                {
                    flag = false;
                    MessageBox.Show("There Is already employee with this pesel in database ! ");
                }

                if (IsInputValid(employee) && flag && ShowConfirmMessage("add", "employee"))
                {
                    

                    db.employees.Add(employee);
                    db.SaveChanges();
                    this.employeesTableAdapter.Fill(this.dataSet.employees);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public bool IsEmployeePresent(String pesel)
        {
            var employeeCount = db.employees.Where(p => p.pesel == pesel).Count();
            bool result = employeeCount > 0 ? false : true;

            return result;
        }

        private void Button_delete_employees_Click(object sender, EventArgs e)
        {
            if (ShowConfirmMessage("delete", "employee"))
            {
                int employee_id = Convert.ToInt32(Grid_employees.Rows[_index].Cells[0].Value);

                db.employees.Remove(db.employees.Where(p => p.id == employee_id).First());
                db.SaveChanges();
                this.employeesTableAdapter.Fill(this.dataSet.employees);
            }
        }

        private void Button_clear_employees_Click(object sender, EventArgs e)
        {
            text_firstname.Text = "";
            text_lastname.Text = "";
            text_pesel.Text = "";
            text_phone.Text = "";
            text_address.Text = "";
        }

        private void Button_update_employees_Click(object sender, EventArgs e)
        {
            int employee_id = Convert.ToInt32(Grid_employees.Rows[_index].Cells[0].Value);
            employee employee = db.employees.Where(p => p.id == employee_id).First();
            try
            {
                employee.address = text_address.Text.ToString();
                employee.firstname = text_firstname.Text.ToString();
                employee.lastname = text_lastname.Text.ToString();
                employee.pesel = text_pesel.Text.ToString();
                employee.phone = text_phone.Text.ToString();

                if (IsInputValid(employee))
                {
                    var addConfirm = MessageBox.Show(@"Are you sure you want to update employee" + "\n" +
                                                        "\n" + "from:" + "\n" + "\n" +
                                                        Grid_employees.Rows[_index].Cells[1].Value + ", " + "\n" +
                                                        Grid_employees.Rows[_index].Cells[2].Value + ", " + "\n" +
                                                        Grid_employees.Rows[_index].Cells[3].Value + ", " + "\n" +
                                                        Grid_employees.Rows[_index].Cells[4].Value + ", " + "\n" +
                                                        Grid_employees.Rows[_index].Cells[5].Value + ", " + "\n" +
                                                        "\n" + " to :" + "\n" + "\n" +
                                                        employee.firstname + ", " + "\n" +
                                                        employee.lastname + ", " + "\n" +
                                                        employee.address + ", " + "\n" +
                                                        employee.phone + ", " + "\n" +
                                                        employee.pesel
                                                        , @"Confirm update",
                                                        MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (addConfirm == DialogResult.No)
                        return;

                    db.Entry(employee).State = EntityState.Modified;
                    db.SaveChanges();
                    this.employeesTableAdapter.Fill(this.dataSet.employees);
                }
            }
            catch (FormatException ex)
            {
                Console.Write(ex.Message);
                MessageBox.Show("You cannot update employee with incorrect data ! ");
            }
        }

        // ORDERS

        private void Button_invoice_Click(object sender, EventArgs e)
        {


            if (_index >= 0)
            {
                    int order_id = Convert.ToInt32(Grid_orders.Rows[_index].Cells[0].Value);

                Double total_sum = 0;
                int total_quantity = 0;
                int yPoint = 240;

                PdfDocument document = new PdfDocument();
                document.Info.Title = "INVOICE_" + db.orders.Where(o => o.id == order_id).First().order_number.ToString();
                PdfPage page = document.AddPage();
                XGraphics gfx = XGraphics.FromPdfPage(page);
                DrawImage(gfx, @"resources\bazar_logo.png", 50, 50, 50, 50);

                XFont font_bold = new XFont("Verdana", 10, XFontStyle.BoldItalic);

                var emp = db.orders.Where(o => o.id == order_id).First().employee;
                var cust = db.orders.Where(o => o.id == order_id).First().customer;
                var date = db.orders.Where(o => o.id == order_id).First().deadline;

                gfx.DrawString("Inovice_NR: " + db.orders.Where(o => o.id == order_id).First().order_number.ToString(), font_bold, XBrushes.Black, new XRect(40, 120, page.Width.Point, page.Height.Point), XStringFormats.TopLeft);
                gfx.DrawString("Employee: " + emp.firstname + " " + emp.lastname, font_bold, XBrushes.Black, new XRect(40, 135, page.Width.Point, page.Height.Point), XStringFormats.TopLeft);
                gfx.DrawString("Customer: " + cust.company_name, font_bold, XBrushes.Black, new XRect(40, 150, page.Width.Point, page.Height.Point), XStringFormats.TopLeft);
                gfx.DrawString("Deadline: " + date.ToString("d"), font_bold, XBrushes.Black, new XRect(40, 165, page.Width.Point, page.Height.Point), XStringFormats.TopLeft);

                gfx.DrawString("Product Name", font_bold, XBrushes.Black, new XRect(100, yPoint, page.Width.Point, page.Height.Point), XStringFormats.TopLeft);
                gfx.DrawString("Quantity", font_bold, XBrushes.Black, new XRect(260, yPoint, page.Width.Point, page.Height.Point), XStringFormats.TopLeft);
                gfx.DrawString("Price", font_bold, XBrushes.Black, new XRect(360, yPoint, page.Width.Point, page.Height.Point), XStringFormats.TopLeft);
                gfx.DrawString("Value", font_bold, XBrushes.Black, new XRect(460, yPoint, page.Width.Point, page.Height.Point), XStringFormats.TopLeft);

                XFont font_regular = new XFont("Verdana", 10, XFontStyle.Regular);

                var order_Items = db.order_items.Where(o => o.order_id == order_id);

                if (!order_Items.Any())
                {
                    MessageBox.Show("There Is no products added to order !");
                }
                else
                {
                    foreach (order_items o in order_Items)
                    {
                        yPoint = yPoint + 25;
                        Double price = db.products.Where(p => p.id == o.product_id).First().price;
                        total_sum = total_sum + (o.order_quantity * price);
                        total_quantity = total_quantity + o.order_quantity;

                        gfx.DrawString(o.product.name, font_regular, XBrushes.Black, new XRect(100, yPoint, page.Width.Point, page.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(o.order_quantity.ToString(), font_regular, XBrushes.Black, new XRect(260, yPoint, page.Width.Point, page.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(price.ToString(), font_regular, XBrushes.Black, new XRect(360, yPoint, page.Width.Point, page.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString((o.order_quantity * price).ToString(), font_regular, XBrushes.Black, new XRect(460, yPoint, page.Width.Point, page.Height.Point), XStringFormats.TopLeft);
                    }

                    yPoint = yPoint + 25;
                    gfx.DrawLine(XPens.Black, 0, yPoint, 1000, yPoint);
                    yPoint = yPoint + 5;
                    gfx.DrawString(total_sum.ToString(), font_regular, XBrushes.Black, new XRect(460, yPoint, page.Width.Point, page.Height.Point), XStringFormats.TopLeft);

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
            else
            {
                MessageBox.Show("Choose another one");

            }
           
        }

        private void Button_add_orders_Click(object sender, EventArgs e)
        {
            bool flag = true;

            try
            {
                var order = new order
                {
                    customer_id = Convert.ToInt32(dropDownList_customer.SelectedValue),
                    employee_id = Convert.ToInt32(dropDownList_employee.SelectedValue),
                    create_timestamp = DateTime.Now,
                    deadline = datePicker_deadline.Value,
                    order_number = text_order_number.Text.ToString()
                };

                if (!IsOrderPresent(order.order_number))
                {
                    flag = false;
                    MessageBox.Show("There Is already order with this number in database ! ");
                }

                if (IsInputValid(order) & flag && ShowConfirmMessage("add", "order"))
                {                  
                    db.orders.Add(order);
                    db.SaveChanges();
                    this.ordersTableAdapter.Fill(this.dataSet.orders);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void Button_delete_orders_Click(object sender, EventArgs e)
        {


            if (ShowConfirmMessage("delete", "order"))
            {
                int order_id = Convert.ToInt32(Grid_orders.Rows[_index].Cells[0].Value);

                db.orders.Remove(db.orders.Where(p => p.id == order_id).First());
                db.SaveChanges();
                this.ordersTableAdapter.Fill(this.dataSet.orders);
            }
        }

        public bool IsOrderPresent(String number)
        {
            var orderCount = db.orders.Where(p => p.order_number == number).Count();
            bool result = orderCount > 0 ? false : true;

            return result;
        }

        private void Button_update_orders_Click(object sender, EventArgs e)
        {
            int order_id = Convert.ToInt32(Grid_orders.Rows[_index].Cells[0].Value);
            order order = db.orders.Where(p => p.id == order_id).First();

            try
            {
                order.customer_id = Convert.ToInt32(dropDownList_customer.SelectedValue);
                order.employee_id = Convert.ToInt32(dropDownList_employee.SelectedValue);
                order.create_timestamp = DateTime.Now;
                order.deadline = datePicker_deadline.Value;
                order.order_number = text_order_number.Text.ToString();

                if (IsInputValid(order))
                {
                    var addConfirm = MessageBox.Show(@"Are you sure you want to update employee" + "\n" +
                                                                           "\n" + "from:" + "\n" + "\n" +
                                                                           Grid_orders.Rows[_index].Cells[1].Value + ", " + "\n" +
                                                                           Grid_employees.Rows[_index].Cells[2].Value + ", " + "\n" +
                                                                           Grid_employees.Rows[_index].Cells[4].Value + ", " + "\n" +
                                                                           "\n" + " to :" + "\n" + "\n" +
                                                                           order.customer.id + ", " + "\n" +
                                                                           order.employee.id + ", " + "\n" +
                                                                           order.deadline
                                                                           , @"Confirm update",
                                                                           MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (addConfirm == DialogResult.No)
                        return;


                    db.Entry(order).State = EntityState.Modified;
                    db.SaveChanges();
                    this.ordersTableAdapter.Fill(this.dataSet.orders);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void Grid_orders_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                _index = e.RowIndex;
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }

            if (_index >= 0)
            {
                DataGridViewRow row = Grid_orders.Rows[e.RowIndex];
                dropDownList_customer.SelectedValue = row.Cells["DataGridViewTextBoxColumn20"].Value.ToString();
                dropDownList_employee.SelectedValue = row.Cells["DataGridViewTextBoxColumn21"].Value.ToString();
                datePicker_deadline.Value = Convert.ToDateTime(row.Cells["DataGridViewTextBoxColumn23"].Value.ToString());
                text_order_number.Text = row.Cells["DataGridViewTextBoxColumn19"].Value.ToString();
            }
        }

        private void Button_raport_Click(object sender, EventArgs e)
        {
            var form = new GenerateRaportForm();
            form.Show(this);
        }

        // ORDER ITEMS

        private void Button_add_items_Click(object sender, EventArgs e)
        {
            bool flag = true;
            try
            {

                var order_item = new order_items
                {
                    product_id = Convert.ToInt32(dropDownList_product.SelectedValue),
                    order_id = Convert.ToInt32(dropDownList_order.SelectedValue),
                    order_quantity = Convert.ToInt32(text_order_quantity.Text.ToString())
                };

                product product = db.products.Where(p => p.id == order_item.product_id).First();
                if (order_item.order_quantity > product.quantityInStock)
                {
                    flag = false;
                    MessageBox.Show("There Is not enough products to add them to order ! There Is just " + product.quantityInStock + " left :<");
                }


                if (IsInputValid(order_item) && flag && ShowConfirmMessage("add", "item"))
                {
                    db.order_items.Add(order_item);

                    product.quantityInStock = product.quantityInStock - order_item.order_quantity;
                    db.Entry(product).State = EntityState.Modified;
                    db.SaveChanges();
                    this.order_itemsTableAdapter.Fill(this.dataSet.order_items);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void Button_delete_items_Click(object sender, EventArgs e)
        {
            if (ShowConfirmMessage("delete", "item"))
            {
                try
                {
                    int order_item_id = Convert.ToInt32(Grid_order_items.Rows[_index].Cells[0].Value);
                    product product = db.products.Find(db.order_items.Find(order_item_id).product_id);
                    product.quantityInStock += db.order_items.Where(p => p.id == order_item_id).First().order_quantity;
                    db.order_items.Remove(db.order_items.Where(p => p.id == order_item_id).First());
                    db.SaveChanges();
                    this.order_itemsTableAdapter.Fill(this.dataSet.order_items);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("There Is nothing to delete !");
                    Console.Write(ex.Message);
                }
            }

        }

        private void Grid_order_items_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                _index = e.RowIndex;
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }

            if (_index >= 0)
            {
                DataGridViewRow row = Grid_order_items.Rows[e.RowIndex];
                dropDownList_order.SelectedValue = row.Cells["DataGridViewTextBoxColumn25"].Value.ToString();
                dropDownList_product.SelectedValue = row.Cells["DataGridViewTextBoxColumn26"].Value.ToString();
                text_order_quantity.Text = row.Cells["DataGridViewTextBoxColumn27"].Value.ToString();
            }
        }

        // UTILITY

        public void DrawImage(XGraphics gfx, string jpegSamplePath, int x, int y, int width, int height)
        {
            XImage image = XImage.FromFile(jpegSamplePath);
            gfx.DrawImage(image, x, y, width, height);
        }
        private List<product> ProductsToDataTable(DataTable dt)
        {
            try
            {
                var convertedList = (from rw in dt.AsEnumerable()
                                     select new product()
                                     {
                                         name = rw["DataGridViewTextBoxColumn2"].ToString(),
                                         alcohol_content = Convert.ToDouble(rw["DataGridViewTextBoxColumn3"]),
                                         sizeInLiters = Convert.ToDouble(rw["DataGridViewTextBoxColumn4"]),
                                         description = rw["DataGridViewTextBoxColumn6"].ToString(),
                                         quantityInStock = Convert.ToInt32(rw["DataGridViewTextBoxColumn7"]),
                                         price = Convert.ToDouble(rw["DataGridViewTextBoxColumn5"])
                                     }).ToList();
                return convertedList;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return new List<product>();
            }

        }
        private List<customer> CustomersToDataTable(DataTable dt)
        {
            try
            {
                var convertedList = (from rw in dt.AsEnumerable()
                                     select new customer()
                                     {
                                         shipment_address = rw["DataGridViewTextBoxColumn10"].ToString(),
                                         company_name = rw["DataGridViewTextBoxColumn9"].ToString(),
                                         customer_phone = rw["DataGridViewTextBoxColumn11"].ToString()
                                     }).ToList();
                return convertedList;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return new List<customer>();
            }

        }
        private List<employee> EmployeesToDataTable(DataTable dt)
        {
            try
            {
                var convertedList = (from rw in dt.AsEnumerable()
                                     select new employee()
                                     {
                                         firstname = rw["DataGridViewTextBoxColumn13"].ToString(),
                                         lastname = rw["DataGridViewTextBoxColumn14"].ToString(),
                                         phone = rw["DataGridViewTextBoxColumn16"].ToString(),
                                         address = rw["DataGridViewTextBoxColumn15"].ToString(),
                                         pesel = rw["DataGridViewTextBoxColumn17"].ToString(),
                                     }).ToList();
                return convertedList;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return new List<employee>();
            }

        }
        private void GetFromCSV(string model)
        {
            OpenFileDialog fileDialog = new OpenFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv",
                FilterIndex = 1,
                Multiselect = true
            };

            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                string fileName = fileDialog.FileName;
                string[] str = File.ReadAllLines(fileName);
                DataTable dt = new DataTable();

                string[] temp = str[0].Split(',');

                try
                {
                    foreach (string t in temp)
                    {
                        string tempstr = t;
                        tempstr = tempstr.Trim('\"');
                        dt.Columns.Add(tempstr, typeof(string));

                    }

                    for (int i = 1; i < str.Length; i++)
                    {
                        string[] t = str[i].Split(',');
                        for (int j = 0; j < t.Length; j++)
                        {
                            t[j] = t[j].Trim('\"');
                        }
                        dt.Rows.Add(t);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                int counter = 0;
                switch (model)
                {
                    case "products":
                        List<product> lIstOfProducts = ProductsToDataTable(dt);

                        foreach (product product in lIstOfProducts)
                        {
                            counter++;
                            try
                            {
                                db.products.Add(product);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message + "\n" + "Row number " + counter + " Is not valid !");
                            }
                        }
                        break;
                    case "customers":
                        List<customer> lIstOfCustomers = CustomersToDataTable(dt);
                        foreach (customer customer in lIstOfCustomers)
                        {
                            counter++;
                            try
                            {
                                db.customers.Add(customer);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message + "\n" + "Row number " + counter + " Is not valid !");
                            }
                        }
                        break;
                    case "employees":
                        List<employee> lIstOfEmployees = EmployeesToDataTable(dt);
                        foreach (employee employee in lIstOfEmployees)
                        {
                            counter++;
                            try
                            {
                                db.employees.Add(employee);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message + "\n" + "Row number " + counter + " Is not valid !");
                            }
                        }
                        break;
                }

                db.SaveChanges();

                switch (model)
                {
                    case "employees":
                        this.employeesTableAdapter.Fill(this.dataSet.employees);
                        break;
                    case "customers":
                        this.customersTableAdapter.Fill(this.dataSet.customers);
                        break;
                    case "products":
                        this.productsTableAdapter.Fill(this.dataSet.products);
                        break;
                }
            }
        }
        private void SaveToCSV(DataGridView DGV)
        {
            string filename = "";
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "CSV (*.csv)|*.csv";
            sfd.FileName = "Output.csv";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                MessageBox.Show("Data will be exported and you will be notified when it Is ready.");
                if (File.Exists(filename))
                {
                    try
                    {
                        File.Delete(filename);
                    }
                    catch (IOException ex)
                    {
                        MessageBox.Show("It wasn't possible to write the data to the dIsk." + ex.Message);
                    }
                }
                int columnCount = DGV.ColumnCount;
                string columnNames = "";
                string[] output = new string[DGV.RowCount + 1];
                for (int i = 0; i < columnCount; i++)
                {
                    columnNames += DGV.Columns[i].Name.ToString() + ",";
                }
                output[0] += columnNames;
                for (int i = 1; (i - 1) < DGV.RowCount; i++)
                {
                    for (int j = 0; j < columnCount; j++)
                    {
                        output[i] += DGV.Rows[i - 1].Cells[j].Value.ToString() + ",";
                    }
                }
                System.IO.File.WriteAllLines(sfd.FileName, output, System.Text.Encoding.UTF8);
                MessageBox.Show("Your file was generated and its ready for use.");
            }
        }
        public bool IsInputValid(object obj)
        {
            ValidationContext context = new ValidationContext(obj, null, null);
            IList<ValidationResult> errors = new List<ValidationResult>();

            if (!Validator.TryValidateObject(obj, context, errors, true))
            {
                foreach (ValidationResult result in errors)
                    MessageBox.Show(result.ErrorMessage);
                return false;
            }
            else
            {
                return true;
            }
        }
        private void Button_refresh_Click(object sender, EventArgs e)
        {
            this.productsTableAdapter.Fill(this.dataSet.products);
            this.customersTableAdapter.Fill(this.dataSet.customers);
            this.employeesTableAdapter.Fill(this.dataSet.employees);
        }
        public bool ShowConfirmMessage(String function, String model_name)
        {
            var addConfirm = MessageBox.Show(@"Are you sure you want to " + function + " given " + model_name, @"Confirm add",
                                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (addConfirm == DialogResult.No)
                return false;
            else
                return true;
        }

    }
}
