<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ClientApp.aspx.cs" Inherits="Katana.Sandbox.WebClient.ClientApp" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Katana.Sandbox.WebClient - Client Application</title>
</head>
<body style="background-color:azure">
    <h1>Katana.Sandbox.WebClient</h1>
    <h2>Client Application</h2>
    <form id="form1" runat="server">
        <div>
            Access Token<br />
            <asp:TextBox ID="AccessToken" runat="server" Width="604px"></asp:TextBox>
            <asp:Button ID="Button2" runat="server" OnClick="AuthorizeButton_Click" Text="Authorize" />
            <br />
            <br />
            Refresh Tokensh Token<br />
            <asp:TextBox ID="RefreshToken" runat="server" Width="604px"></asp:TextBox>
            <asp:Button ID="RefreshButton" runat="server" OnClick="RefreshButton_Click" Text="Refresh" />
            <br />
            <br />
    Username
            <asp:TextBox ID="Username" runat="server" style="margin-top: 2px"></asp:TextBox>
&nbsp;Password
            <asp:TextBox ID="Password" runat="server"></asp:TextBox>
&nbsp;<asp:Button ID="ResourceOwnerGrantButton" runat="server" OnClick="ResourceOwnerGrantButton_Click" Text="Resource Owner Grant" />
            <br />
            <br />
            <asp:Button ID="Button1" runat="server" OnClick="Button1_Click" Text="Call Web API" />
        </div>
        <tt>
            <asp:Label ID="Label1" runat="server" Text="All Claims"></asp:Label>
        </tt>
    </form>
</body>
</html>
