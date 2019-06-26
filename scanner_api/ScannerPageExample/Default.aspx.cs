using ClientScanner.TiffImage;
using System;
using System.IO;
using System.Web.UI;

namespace ScannerPageExample
{
    public partial class Default : System.Web.UI.Page
    {
        int threshold;
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void scanFlatbedButton_Click(object sender, EventArgs e)
        {
            try
            {
                threshold = int.Parse(txtthreashold.Text);
                if(threshold!=0)
                {
                    var scanner = new ClientScanner.ClientScanner.ClientScanner("localhost");
                    var img1 = scanner.Scan(CheckBox1.Checked, allowduplexCheckbox.Checked, int.Parse(DropDownList1.SelectedItem.Value), threshold);
                    if (!string.IsNullOrEmpty(TextBox1.Text))
                    {
                        int index = int.Parse(TextBox1.Text);

                        img1.Append(savePathTextbox.Text, index);
                    }
                    else
                    {
                        img1.Append(savePathTextbox.Text);
                    }
                }
            }
            catch (Exception ex)
            {
                var err = string.Format("alert('{0}')", ex.Message);
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", err, true);
            }
        }

        protected void scanADFButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (threshold != 0)
                {
                    threshold = int.Parse(txtthreashold.Text);
                    var scanner = new ClientScanner.ClientScanner.ClientScanner("localhost");
                    var img1 = scanner.Scan(true, allowduplexCheckbox.Checked, int.Parse(DropDownList1.SelectedItem.Value), threshold);
                    img1.Append(savePathTextbox.Text);
                }
            }
            catch (Exception ex)
            {
                var err = string.Format("alert('{0}')", ex.Message);
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", err, true);
            }
        }

        protected void Button1_Click(object sender, EventArgs e)
        {


            
            
        }

        protected void Button2_Click(object sender, EventArgs e)
        {
            //
            if (!string.IsNullOrEmpty(TextBox2.Text))
            {
                int index = int.Parse(TextBox2.Text);

                TiffImage.RemovePage(index,savePathTextbox.Text);
             
            }
        }
    }
}