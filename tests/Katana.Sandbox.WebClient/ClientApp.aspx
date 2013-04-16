<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ClientApp.aspx.cs" Inherits="Katana.Sandbox.WebClient.ClientApp" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        Access Token<br />
        <asp:TextBox ID="AccessToken" runat="server" Width="604px"></asp:TextBox>
        <br />
        <br />
        <asp:Button ID="Button1" runat="server" OnClick="Button1_Click" Text="Show Claims" />
    </div>
        <asp:Label ID="Label1" runat="server" Text="All Claims"></asp:Label>
    </form>
</body>
</html>
