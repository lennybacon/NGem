<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    NGem Home Page
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Welcome to NGem! </h2>
    <p>
        NGem ist a gem like 3rdPartyAssembly resolver and packer with support 
        for private gem repositories. 
    </p>
    <p>
        To learn more about NGem visit 
        <a href="http://github.com/lennybacon/NGem" title="Github">Github</a>. 
    </p>
</asp:Content>
