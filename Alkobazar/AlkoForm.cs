using Alkobazar.model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text;

namespace Alkobazar
{
    public partial class AlkoForm : Form
    {
        private service.ImportButtonHandle import = new service.ImportButtonHandle();
        private int _index = 0; // shows selected row index
        private alkoDbEntities db = new alkoDbEntities();

        public AlkoForm()
        {
            InitializeComponent();
        }

        private void AlkoForm_Load(object sender, EventArgs e)
        {
            this.order_itemsTableAdapter.Fill(this.dataSet.order_items);
            this.ordersTableAdapter.Fill(this.dataSet.orders);
            this.employeesTableAdapter.Fill(this.dataSet.employees);
            this.customersTableAdapter.Fill(this.dataSet.customers);
            this.productsTableAdapter.Fill(this.dataSet.products);
            this.grid_products.MultiSelect = false;
            this.grid_order_items.MultiSelect = false;
            this.grid_orders.MultiSelect = false;
            this.grid_customers.MultiSelect = false;
            this.grid_employees.MultiSelect = false;
        }

        private void button_clear_products_Click(object sender, EventArgs e)
        {
            text_quantity_in_stock.Text = "";
            text_alcohol_content.Text = "";
            text_size_in_liters.Text = "";
            text_product_description.Text = "";
            text_product_name.Text = "";
            text_price.Text = "";
        }

        private void button_add_products_Click(object sender, EventArgs e)
        {
            bool flag = true;

            try
            {
                var product = new product
                {
                    quantityInStock = int.Parse(text_quantity_in_stock.Text),
                    alcohol_content = Convert.ToDouble(text_alcohol_content.Text),
                    sizeInLiters = Convert.ToDouble(text_size_in_liters.Text),
                    description = text_product_description.Text,
                    name = text_product_name.Text.ToString(),
                    price = Convert.ToDouble(text_price.Text)
                };

                if (!isProductPresent(product.name, product.sizeInLiters))
                {
                    flag = false;
                    MessageBox.Show("There is already items with this name and size in database ! ");
                }

                if (isProductInputValid(product) && flag)
                {
                    var addConfirm = MessageBox.Show(@"Are you sure you want to add given product", @"Confirm add",
                                                        MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (addConfirm == DialogResult.No)
                        return;

                    db.products.Add(product);
                    db.SaveChanges();
                    this.productsTableAdapter.Fill(this.dataSet.products);
                }
            }
            catch (FormatException ex)
            {
                Console.Write(ex.Message);
                MessageBox.Show("Input data must not be empty nor in incorrect format !");
            }
        }

        private void button_update_products_Click(object sender, EventArgs e)
        {
            int product_id = Convert.ToInt32(grid_products.Rows[_index].Cells[0].Value);

            var product = db.products.Where(p => p.id == product_id).First();

            try
            {
                product.quantityInStock = int.Parse(text_quantity_in_stock.Text);
                product.alcohol_content = Convert.ToDouble(text_alcohol_content.Text);
                product.sizeInLiters = Convert.ToDouble(text_size_in_liters.Text);
                product.description = text_product_description.Text;
                product.name = text_product_name.Text.ToString();
                product.price = Convert.ToDouble(text_price.Text);

                if (isProductInputValid(product))
                {
                    var addConfirm = MessageBox.Show(@"Are you sure you want to update product" + "\n" +
                                                        "\n" + "from:" + "\n" + "\n" +
                                                        grid_products.Rows[_index].Cells[1].Value + ", " + "\n" +
                                                        grid_products.Rows[_index].Cells[2].Value + ", " + "\n" +
                                                        grid_products.Rows[_index].Cells[3].Value + ", " + "\n" +
                                                        grid_products.Rows[_index].Cells[4].Value + ", " + "\n" +
                                                        grid_products.Rows[_index].Cells[5].Value + ", " + "\n" +
                                                        grid_products.Rows[_index].Cells[6].Value + ", " + "\n" +
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

        private void button_delete_products_Click(object sender, EventArgs e)
        {

            var deleteConfirm = MessageBox.Show(@"Are you sure you want to delete the selected product", @"Confirm deletion",
                                                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (deleteConfirm == DialogResult.No)
                return;

            int product_id = Convert.ToInt32(grid_products.Rows[_index].Cells[0].Value);

            db.products.Remove(db.products.Where(p => p.id == product_id).First());
            db.SaveChanges();
            this.productsTableAdapter.Fill(this.dataSet.products);
        }

        private void grid_products_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            _index = e.RowIndex;
            DataGridViewRow row = grid_products.Rows[e.RowIndex];

            text_quantity_in_stock.Text = row.Cells["quantityInStockDataGridViewTextBoxColumn"].Value.ToString();
            text_alcohol_content.Text = row.Cells["alcoholcontentDataGridViewTextBoxColumn"].Value.ToString();
            text_size_in_liters.Text = row.Cells["sizeInLitersDataGridViewTextBoxColumn"].Value.ToString();
            text_product_description.Text = row.Cells["descriptionDataGridViewTextBoxColumn"].Value.ToString();
            text_product_name.Text = row.Cells["nameDataGridViewTextBoxColumn"].Value.ToString();
            text_price.Text = row.Cells["priceDataGridViewTextBoxColumn"].Value.ToString();
        }

        public bool isProductPresent(String name, double size)
        {
            var productCount = db.products.Where(p => p.name == name &&
                                                      p.sizeInLiters == size
                                                ).Count();
            bool result = productCount > 0 ? false : true;

            return result;
        }

        public bool isProductInputValid(product product)
        {
            ValidationContext context = new ValidationContext(product, null, null);
            IList<ValidationResult> errors = new List<ValidationResult>();

            if (!Validator.TryValidateObject(product, context, errors, true))
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

        private void button_refresh_products_Click(object sender, EventArgs e)
        {
            this.productsTableAdapter.Fill(this.dataSet.products);
        }

        private void button_export_products_Click(object sender, EventArgs e)
        {
            SaveToCSV(grid_products);
        }

        private void button_import_products_Click(object sender, EventArgs e)
        {
            GetFromCSV();               
        }

        private List<product> dataTableToList(DataTable dt)
        {
            try
            {
                var convertedList = (from rw in dt.AsEnumerable()
                                     select new product()
                                     {                                         
                                         name = rw["nameDataGridViewTextBoxColumn"].ToString(),
                                         alcohol_content = Convert.ToDouble(rw["alcoholcontentDataGridViewTextBoxColumn"]),
                                         sizeInLiters = Convert.ToDouble(rw["sizeInLitersDataGridViewTextBoxColumn"]),
                                         description = rw["descriptionDataGridViewTextBoxColumn"].ToString(),
                                         quantityInStock = Convert.ToInt32(rw["quantityInStockDataGridViewTextBoxColumn"]),
                                         price = Convert.ToDouble(rw["priceDataGridViewTextBoxColumn"])
                                     }).ToList();
                return convertedList;
            }
            catch(Exception ex)
            {                
                MessageBox.Show(ex.Message);
                return new List<product>();
            }
                
        }


        private void GetFromCSV()
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

                List<product> listOfProducts = dataTableToList(dt);
                int counter = 0;
                foreach (product product in listOfProducts)
                {
                    counter++;
                    try
                    {
                        db.products.Add(product);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message + "\n" + "Row number " + counter + " is not valid !");
                    }
                }
                db.SaveChanges();
                this.productsTableAdapter.Fill(this.dataSet.products);
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
                MessageBox.Show("Data will be exported and you will be notified when it is ready.");
                if (File.Exists(filename))
                {
                    try
                    {
                        File.Delete(filename);
                    }
                    catch (IOException ex)
                    {
                        MessageBox.Show("It wasn't possible to write the data to the disk." + ex.Message);
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


    }



}
