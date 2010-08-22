<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="aboutTitle" ContentPlaceHolderID="TitleContent" runat="server">
    NGem About Us
</asp:Content>

<asp:Content ID="aboutContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2>About </h2>
    <p>
        NGem was created by <a href="http://www.lennybacon.com/" target="_blank">Daniel Fisher</a>
        (info [at] lennybacon.com) and <a href="http://www.philipproplesch.de/" target="_blank">
        Philip Proplesch</a> (philip [at] proplesch.de). 
    </p>
</asp:Content>
