<%@ Page Language="C#" %>
<%@ Import Namespace="System.Web" %>
<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>IntegratedPipelineTest</title>
    <script runat="server">
        public string GetOwinPipelineOrder()
        {
            var stackItems = new List<int>(HttpContext.Current.GetOwinContext().Get<Stack<int>>("stack").ToArray());
            stackItems.Reverse();
            return string.Join<int>(";", stackItems.ToArray());
        }
    </script>
</head>
<body>
    <form id="form1" runat="server">
        <%= GetOwinPipelineOrder() %>
    </form>
</body>
</html>