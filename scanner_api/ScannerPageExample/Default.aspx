<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ScannerPageExample.Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
        </div>
        <asp:Button ID="scanFlatbedButton" runat="server" OnClick="scanFlatbedButton_Click" Text="Scan Flatbed" />
        <asp:Button ID="scanADFButton" runat="server" OnClick="scanADFButton_Click" Text="Scan ADF" />
        <asp:Button ID="Button1" runat="server" OnClick="scanFlatbedButton_Click" Text="Insert image" />
        <asp:TextBox ID="TextBox1" runat="server"></asp:TextBox>
        <asp:CheckBox ID="CheckBox1" runat="server" Text="Use Adf" />
        <asp:TextBox ID="txtthreashold" runat="server" ClientIDMode="Static"></asp:TextBox>
        <br />
        <asp:Button ID="Button2" runat="server" Text="Remove image" OnClick="Button2_Click" />
        <asp:TextBox ID="TextBox2" runat="server"></asp:TextBox>
        <br />
        <asp:CheckBox ID="allowduplexCheckbox" runat="server" Text="Use Duplex" />
        <br />
        <asp:DropDownList ID="DropDownList1" runat="server">
            <asp:ListItem Value="1">Color</asp:ListItem>
            <asp:ListItem Value="2">GrayScale</asp:ListItem>
            <asp:ListItem Value="4">Black&amp;White</asp:ListItem>
        </asp:DropDownList>
        <br />
        Image path*<br />
        <asp:TextBox ID="savePathTextbox" runat="server"></asp:TextBox>
        <br />
        <br />
        *Note: In case the image in Image path exist, the scanned image will be appended to the bottom of it</form>
    <p>
    </p>
</body>
</html>
