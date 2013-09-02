<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SpellHint.aspx.cs" Inherits="SpellCheck.SpellHint" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Spell Checking Test</title>
    
    <script src="https://ajax.googleapis.com/ajax/libs/jquery/1.5.2/jquery.min.js" type="text/javascript"></script>
    <script type="text/javascript" >
          //        function displaywaitmsg() {
          //            var objProgress = document.getElementById("lblProgress");
          //            if (objProgress != null && objProgress != undefined) {
          //                objProgress.innerHTML = "Loading...";
          //            }
          //        }

//          function init() {
//              var spinner = document.getElementById("spinner");
//              spinner.style.display = "";
//          };

//          $(document).ready(function () {

//              $('#spinner').bind("ajaxSend", function () {
//                  $(this).show();
//              }).bind("ajaxComplete", function () {
//                  $(this).hide();
//              });

//          });

//          $("#spinner").ajaxStart(function () {
//              $(this).show();
//          }).ajaxStop(function () {
//              $(this).hide();
//          });

//          $(function () {
//              init();
//          });

    </script>
    <style type="text/css">
        #tblMain
        {
            border-collapse: collapse;
            height: 160px;
            width: 96%;
        }
        #subTable
        {
            width: 98%;
        }
        
        .spinner
        {
            position: fixed;
            top: 70%;
            left: 50%;
            margin-left: -50px; /* half width of the spinner gif */
            margin-top: -50px; /* half height of the spinner gif */
            text-align: center;
            z-index: 1234;
            width: 80px; /* width of the spinner gif */
            height: 80px; /*hight of the spinner gif +2px to fix IE8 issue */
        }
        .column1
        {
            width: 30%;
        }
        
        .column2
        {
            width: 70%;
        }
        
        .textBox
        {
            width: 90%;
        }
        #lbHint
        {
            width: 90%;
        }
        #tbCurrentWord, #tbManual
        {
            width: 90%;
        }
        
        #btnNext
        {
            float: right;
            width: 90px;
        }
        
        #btnStop
        {
            float: left;
            width: 90px;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
    <div id="wrapper">
        <div id="spinner" class="spinner" style="display: none;" runat="server">
            <img src="/images/ajax_loader.gif" alt="Loading" class="spinner" />
        </div>
        <table id="tblMain" cellpadding="5" cellspacing="5" runat="server">
            <tr>
                <td class="column1">
                    <asp:Label ID="lblCurrentWord" runat="server" Text="Current Word:"></asp:Label>
                </td>
                <td class="column2">
                    <asp:TextBox ID="tbCurrentWord" runat="server"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td class="column1">
                    <asp:Label ID="lblHint" runat="server" Text="Suggestion Words:"></asp:Label>
                </td>
                <td class="column2">
                    <asp:ListBox ID="lbHint" runat="server" Rows="15" OnSelectedIndexChanged="lbHint_SelectedIndexChanged"
                        AutoPostBack="True" SelectionMode="Single"></asp:ListBox>
                </td>
            </tr>
            <tr>
                <td class="column1">
                    <asp:Label ID="lblManual" runat="server" Text="Or Use:"></asp:Label>
                </td>
                <td class="column2">
                    <asp:TextBox ID="tbManual" runat="server" OnTextChanged="tbManual_TextChanged" postback="true"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td>
                    <asp:Label ID="lblProgress" runat="server" Text=""></asp:Label>
                </td>
                <td>
                    <table id="subTable">
                        <tr>
                            <td>
                                <asp:Button ID="btnStop" runat="server" Text="Stop" OnClick="btnStop_Click" />
                            </td>
                            <%--<td>
                                <asp:Button ID="btnIgnore" runat="server" Text="Ignore" OnClick="btnIgnore_Click" />
                            </td>--%>
                            <td>
                                <asp:Button ID="btnNext" runat="server" Text="Next" OnClick="btnNext_Click" />
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
        </table>
    </div>
    <div class="lblErr">
        <asp:Label ID="lblError" runat="server" Text=""></asp:Label>
    </div>
    <asp:HiddenField ID="hfToBeChecked" runat="server" />
    <asp:HiddenField ID="hfCurrentCount" runat="server" />
    <asp:HiddenField ID="hfWordCount" runat="server" />
    <asp:HiddenField ID="hfErrors" runat="server" />
    <asp:HiddenField ID="hfFinal" runat="server" />
    <asp:HiddenField ID="hfParentTextBoxId" runat="server" />
    <asp:HiddenField ID="hfParentButtonId" runat="server" />
    <br />
    <br />
    </form>

  
</body>
</html>
