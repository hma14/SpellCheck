<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TestPage.aspx.cs" Inherits="SpellCheck.TestPage" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Test Spell Checking Program</title>
    <script type="text/javascript" language="javascript">
    <!--
        function newWindow(textboxId, buttonId, objButton, strURL, strName, w, h) {
            objButton.disabled = true;

            var val = document.getElementById(textboxId).value;
            val = encodeURIComponent(val);          
            if (trim(val) == "") {

                alert("Please enter some text first.");
                val.focus();
                return false;
            }            
            var winl = (screen.width - w) / 2;
            var wint = (screen.height - h) / 2;
            strURL = "SpellHint.aspx?textboxId=" + textboxId + "&buttonId=" + buttonId + "&content=" + val;
            winProperties = 'height=' + h + ',width=' + w + ',top=' + wint + ',left=' + winl + ',scrollbars=no,resizable=no,location=no,menubar=no,titlebar=no,toolbar=no';
            win = window.open(strURL, strName, winProperties);       
            return false;
        }

        function trim(myString) {
            return myString.replace(/^s+/g, '').replace(/s+$/g, '');
        }
         //-->
    </script>
    <style>
        #tbl 
        {
            width:600px;
            border-style:inset;
        }
    </style>
</head>
<body>
    <form id="frmSpellCheck" name="frmSpellCheck" runat="server">
    <div id="spinner" class="spinner" style="display: none;" runat="server">
            <img src="/images/ajax_loader.gif" alt="Loading" class="spinner" />
        </div>
    <table id="tbl" >
        <tr>
            <td>
                <asp:TextBox ID="tbContent" runat="server" Rows="20" Columns="1" Style="width: 99%"
                    TextMode="MultiLine" ></asp:TextBox>
            </td>
        </tr>
        <tr align="right">
            <td>
                <asp:Button ID="btnCheckSpelling" runat="server" Text="Check Spelling"   
                OnClientClick="return newWindow('tbContent',this.id, this, this.href,'CheckSpelling','380','400')"
                />
            </td>
        </tr>
    </table>
    </form>
</body>
</html>

<%--OnClientClick="return newWindow('tbContent',this.id, this, this.href,'CheckSpelling','380','400')"--%> 