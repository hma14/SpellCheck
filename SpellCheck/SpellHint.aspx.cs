using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace SpellCheck
{
    /// <summary>
    /// Spell Checking class
    /// </summary>
    public partial class SpellHint : System.Web.UI.Page
    {
        #region private members
        int iCurrentCount = 0;
        int iWordCount;
        int intErrors = 0;
        string [] arrWords;        
        string strAlternative = string.Empty;       
        string [] strDicArray;
        const string dicFile = "en_dic.txt";
        Dictionary<string, bool> dicStringBool;
        Dictionary<char, string> dicDictinaryName;
       
        #endregion

        #region Page Load Event
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.QueryString["content"] == string.Empty)
                return;

            if (!IsPostBack)
            {
                iCurrentCount = 0;
                iWordCount = 0;
                intErrors = 0;            
                hfCurrentCount.Value = "0";
                hfParentTextBoxId.Value = Request.QueryString["textboxId"];
                hfParentButtonId.Value = Request.QueryString["buttonId"];
                
                hfFinal.Value = Server.UrlDecode(Request.QueryString["content"]);
                hfToBeChecked.Value = Request.QueryString["content"];

                btnNext.Attributes.Add("OnClick", 
                    String.Format("{0};this.disabled = true;document.getElementById('spinner').style.display = '';", 
                    ClientScript.GetPostBackEventReference(btnNext, null)));

                //Regex rgx = new Regex(@"\W+"); 
                Regex rgx = new Regex("[,.;?!-=\"\'`]");
                string strTemp = rgx.Replace(hfToBeChecked.Value, " ");
                rgx = new Regex(@"\s+");
                strTemp = rgx.Replace(strTemp, " ");
                strTemp = strTemp.TrimStart();
                strTemp = strTemp.TrimEnd();

                hfToBeChecked.Value = strTemp;

                // Mapping first character with dictionary text file in which it only 
                // contains words started with this character
                const string alphabets = "abcdefghijklmnopqrstuvwxyz";
                dicDictinaryName = new Dictionary<char, string>();
                for (int i = 0; i < alphabets.Length; i++)
                {
                    dicDictinaryName[alphabets[i]] = alphabets[i] + "_dic.txt";
                }


                arrWords = strTemp.Split();
                iWordCount = arrWords.Length;
                hfWordCount.Value = iWordCount.ToString();
                hfErrors.Value = "0";

               
                // If word contains non-alphabet characters, do nothing 
                if (PrepForSpellCheck(arrWords[iCurrentCount]) == false)
                {
                    btnNext_Click(sender, e);
                }

                // check the current word and if invalid, suggest alternatives
                tbCurrentWord.Text = arrWords[iCurrentCount];
                string beginOfString = arrWords[iCurrentCount].Substring(0, 1).ToLower();
                Regex reg = new Regex(@"^[a-z]+$");
                if (reg.IsMatch(beginOfString))
                {
                    LoadDictArray(beginOfString);
                }
                else
                {
                    btnNext_Click(sender, e);
                }

                // For invalid characters appared inside a word, just move on
                if (SpellCheck(arrWords[iCurrentCount]) == true)
                {
                    btnNext_Click(sender, e);
                }
                // If the word is valid, just add to the final string and move on                
                else
                {
                    // Increment the number of invalid words
                    intErrors++;
                    hfErrors.Value = intErrors.ToString();

                    // Clear out list from previous suggestions
                    lbHint.Items.Clear();

                    // Add each suggested word to the list
                    foreach (string str in Suggest(arrWords[iCurrentCount]))
                    {
                        lbHint.Items.Add(new ListItem(str, str));
                    }
                   
                    // add the original item to the list of suggestions in case it's not in our dictionary
                    lbHint.Items.Insert(0, new ListItem(arrWords[iCurrentCount], arrWords[iCurrentCount]));

                    // select the original item in the list by default
                    lbHint.SelectedIndex = 0;
                }
            }
            else
            {                
                arrWords = hfToBeChecked.Value.Split();
                iWordCount = int.Parse(hfWordCount.Value);
                iCurrentCount = int.Parse(hfCurrentCount.Value);
                intErrors = int.Parse(hfErrors.Value);
            }

            // If we've checked all the words in the original text
            if (iCurrentCount == iWordCount)
            {
                // Notify the user and close the spell check window                
                finishedChecking(hfFinal.Value);
            }
        }
        #endregion

        #region btnNext_Click

        /// <summary>
        /// Event fired when click Next button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnNext_Click(object sender, EventArgs e)
        {         
            // Clear out the Current word and manual word textboxes, ready for the next one
            
            tbManual.Text = string.Empty;
            lbHint.Items.Clear();

            iCurrentCount = int.Parse(hfCurrentCount.Value);
            iCurrentCount++;
            // If we've checked all the words in the original text
            if (iCurrentCount == iWordCount)
            {
                // Notify the user, copy modified content back to the parent textbox
                // and close the spell check window
                finishedChecking(hfFinal.Value);
                return;
            }
            hfCurrentCount.Value = iCurrentCount.ToString();
            arrWords = hfToBeChecked.Value.Split();
            tbCurrentWord.Text = arrWords[iCurrentCount];
            string beginOfString = arrWords[iCurrentCount].Substring(0, 1).ToLower();
            Regex reg = new Regex(@"^[a-z]+$");
            if (reg.IsMatch(beginOfString))
            {
                LoadDictArray(beginOfString);
            }
            else
            {
                btnNext_Click(sender, e);
            }
            
            if (PrepForSpellCheck(arrWords[iCurrentCount]) == false)
            {
                btnNext_Click(sender, e);
            }
            else if (SpellCheck(arrWords[iCurrentCount]) == true)
            {              
                btnNext_Click(sender, e);
            }
#if false
            else if (IsNumeric(arrWords[iCurrentCount]) == true)
            {
                btnNext_Click(sender, e);
            }
#endif
            else
            {
                intErrors++;
                hfErrors.Value = intErrors.ToString();
                lbHint.Items.Clear();

                // Add each suggested word to the list
                foreach (string str in Suggest(arrWords[iCurrentCount]))
                {
                    lbHint.Items.Add(new ListItem(str, str));
                }
                
                // add the original item to the list of suggestions in case it's not in our dictionary
                lbHint.Items.Insert(0, new ListItem(arrWords[iCurrentCount], arrWords[iCurrentCount]));

                // select the original item in the list by default
                lbHint.SelectedIndex = 0;
            }                    
        }
        #endregion

        #region SpellCheck
        /// <summary>
        /// Check Word Spelling
        /// </summary>
        /// <param name="strWord">Word to be checked on spelling</param>
        /// <returns>Correct spelling returns true, otherwise, false</returns>
        private bool SpellCheck(string strWord)
        {

#if true
            try
            {
                if (dicStringBool[strWord.ToLower()] == true)
                    return true;
            }
            catch (Exception)
            {
                return false;
            }
           
            return false;

#else
            // Binary Search
            bool retValue = false;
            int iFirst;
            int iLast;
            int iMiddle;
            if (strWord.Length > 1)
            {
                iFirst = 0;
                iLast = strDicArray.Length - 1;
                do
                {
                    iMiddle = (iFirst + iLast) / 2;
                    //if (strDicArray[iMiddle].ToLower() == strWord.ToLower())
                    if (String.Compare(strWord.ToLower(), strDicArray[iMiddle].ToLower()) == 0)
                    {
                        retValue = true;
                        break;
                    }
                    else if (String.Compare(strWord.ToLower(), strDicArray[iMiddle].ToLower()) < 0)
                        iFirst = iMiddle + 1;
                    else
                        iLast = iMiddle - 1;
                } while (iFirst <= iLast);
            }
            else
                retValue = true;

            return retValue;
#endif
        }
        #endregion

        #region btnStop_Click
        /// <summary>
        /// To Handle Stop Checking Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnStop_Click(object sender, EventArgs e)
        {
            stopChecking(hfFinal.Value);            
        }
        #endregion

        #region Handle for Stop Checking Event
        /// <summary>
        /// Write back to parent window with the results so far done by the spell checking
        /// </summary>
        /// <param name="strFormName">Form name for in the parent window</param>
        /// <param name="strTextboxName">Textbox name in the parent window</param>
        /// <param name="strText">Text to be written back to the textbox in parent window</param>
        private void stopChecking(string strText)
        {
            tblMain.Visible = false;
            if (strText.Contains("\""))
                strText = strText.Replace("\"", "\\\"");

            strText = strText.Replace("\r", "\\r");
            strText = strText.Replace("\n", "\\n");

            // Insert the onload javascript to alert the user that the check has completed
            // and update the source textbox and close the spellchecker window
            StringBuilder sbJS = new StringBuilder();
            sbJS.Append("<script language=\"javascript\" type=\"text/javascript\">");
            sbJS.Append("alert(\"The spell check was stopped successfully (" + intErrors + " error(s) were fixed).\");");
            sbJS.Append("window.opener.document.getElementById('" + hfParentTextBoxId.Value + "').value = \"" + strText + "\";");
            sbJS.Append("window.opener.document.getElementById('" + hfParentButtonId.Value + "').disabled = false;");
            sbJS.Append("self.close();");
            sbJS.Append("</script>");

            Page.ClientScript.RegisterStartupScript(typeof(Page), "onload", sbJS.ToString());
        }
        #endregion

        #region finishedChecking
        /// <summary>
        /// Write back to parent window with the results done by the spell checking
        /// </summary>
        /// <param name="strFormName">Form name for in the parent window</param>
        /// <param name="strTextboxName">Textbox name in the parent window</param>
        /// <param name="strText">Text to be written back to the textbox in parent window</param>
        private void finishedChecking(string strText)
        {
            tblMain.Visible = false;
            if (strText.Contains("\""))
                strText = strText.Replace("\"", "\\\"");

            strText = strText.Replace("\r", "\\r");
            strText = strText.Replace("\n", "\\n");

            // Insert the onload javascript to alert the user that the check has completed
            // and update the source textbox and close the spellchecker window
            StringBuilder sbJS = new StringBuilder();
            sbJS.Append("<script language=\"javascript\" type=\"text/javascript\">");
            sbJS.Append("alert(\"The spell check has completed successfully (" + intErrors + " error(s) found).\");");
            sbJS.Append("window.opener.document.getElementById('" + hfParentTextBoxId.Value + "').value = \"" + strText + "\";");
            sbJS.Append("window.opener.document.getElementById('" + hfParentButtonId.Value + "').disabled = false;");            
            sbJS.Append("self.close();");
            sbJS.Append("</script>");

            Page.ClientScript.RegisterStartupScript(typeof(Page), "onload", sbJS.ToString());
        }
        #endregion

        #region LoadDictArray
        /// <summary>
        /// Load all words into memory, then find out a group of words that started with the
        /// character same as the first character of the word to be checked against. 
        /// Finally, load these words into array.
        /// </summary>
        /// <param name="strPath">Dictionary text file including path</param>
        /// <param name="strBegin">First character of the word to be check against</param>
        
#if false
        private void LoadDictArray(string strBegin)
        {
            string strCurrent = string.Empty;
            string strFinal = string.Empty;
            string root = Server.MapPath(".");
            string firstChar = string.Empty;
            StreamReader srDictionary = new StreamReader(root + @"/En_Dictionary/en_dic.txt");

            while (srDictionary.Peek() != -1)
            {
                strCurrent = srDictionary.ReadLine();

                if (strCurrent.Substring(0, 1).ToLower() == strBegin.ToLower())
                {
                    strFinal += strCurrent + '\n';
                }
            }
            srDictionary.Close();
            strDicArray = strFinal.Split('\n');       
            
            dicStringBool = new Dictionary<string, bool>();
            for (int i = 0; i < strDicArray.Length; i++)
            {
                dicStringBool[strDicArray[i].ToLower()] = true;
            }
        }
#else

        private void LoadDictArray(string strBegin)
        {           
            string strFinal = string.Empty;
            string root = Server.MapPath(".");
            string firstChar = string.Empty;
            try
            {
                using (StreamReader srDictionary = new StreamReader(root + @"/En_Dictionary/" + strBegin + "_dic.txt"))
                {
                    while (srDictionary.Peek() > -1)
                    {
                        strFinal += srDictionary.ReadLine() + '\n';
                    }
                    srDictionary.Close();
                    strDicArray = strFinal.Split('\n');

                    dicStringBool = new Dictionary<string, bool>();
                    for (int i = 0; i < strDicArray.Length; i++)
                    {
                        dicStringBool[strDicArray[i].ToLower()] = true;
                    }
                }
            }
            catch (Exception e)
            {
                lblError.Text = string.Format("The process failed: {0}\n{1}", e.ToString(), e.Message);               
            }
        }
#endif
        #endregion

        #region PrepForSpellCheck
        /// <summary>
        /// Check out that if the given word belong to alphabet set, in order to guarantee that
        /// they must be alphabet-only words before processing the spell check.
        /// </summary>
        /// <param name="strWord">Word to be spell-checked against</param>
        /// <returns>True for alphabet-only, otherwise is false</returns>
        private bool PrepForSpellCheck(string strWord)
        {
            const string strValidChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            Dictionary<char, bool> dic = new Dictionary<char, bool>();

            for (int i = 0; i < strValidChars.Length; i++)
            {
                dic[strValidChars[i]] = true;
            }

            
            for (int i = 0; i < strWord.Length; i++)
            {
                try
                {                  
                    if (dic[strWord[i]] == true) continue;
                }
                catch
                {
                    return false;
                }                      
            }
            return true;
        }
        #endregion

        #region IsNumeric
        /// <summary>
        /// Verify if the given word is numerics
        /// </summary>
        /// <param name="strNumber">Given word</param>
        /// <returns>True for numeric-only word, otherwise is false</returns>
        private bool IsNumeric(string strNumber)
        {
            int result;
            return int.TryParse(strNumber, out result);
        }
        #endregion

        #region Suggest
        /// <summary>
        /// Provide suggestion words if the given word not match in the dictionary
        /// </summary>
        /// <param name="strWord">Given word</param>
        /// <returns>Array of suggested words</returns>
        private string[] Suggest(string strWord)
        {            
            string strSuggestions = string.Empty;
            int intMaxSuggestions = 10;
            int intSuggestionCount = 0;
            string[] strSuggestionArray;
            double[] dblSimilarityArray;
            double dblSimilarity = 0;
            string strSoundex = Soundex(strWord);

            int i = 0;
            do
            {
                if (strDicArray[i].Length > 0)
                {
                    if (Soundex(strDicArray[i]) == strSoundex)
                    {
                        if (strSuggestions == string.Empty)
                            strSuggestions = strDicArray[i];
                        else
                            strSuggestions += "|" + strDicArray[i];
                    }
                    i++;
                }
                else
                    break;
               
            } while (i < strDicArray.Length);

            string [] SuggestArray = strSuggestions.Split('|');

            if (SuggestArray.Length < intMaxSuggestions)
            {
                intSuggestionCount = SuggestArray.Length;
            }
            else
            {
                intSuggestionCount = intMaxSuggestions - 1;
            }

            strSuggestionArray = new string[intSuggestionCount];
            dblSimilarityArray = new double[intSuggestionCount];
            
            foreach (string suggest in SuggestArray)
            {
                dblSimilarity = WordSimilarity(strWord, suggest);
                i = intSuggestionCount-1;
                while (dblSimilarity > dblSimilarityArray[i])
                {
                    if (i < intSuggestionCount-1)
                    {
                        strSuggestionArray[i + 1] = strSuggestionArray[i];
                        dblSimilarityArray[i + 1] = dblSimilarityArray[i];
                    }                   
                    strSuggestionArray[i] = suggest;
                    dblSimilarityArray[i] = dblSimilarity;
                    i--;
                    if (i == -1)
                        break;

                } 
            }

            SuggestArray = strSuggestionArray;

            return SuggestArray;

        }
        #endregion

        #region Soundex
        /// <summary>
        /// Algorithm of creating Soundex
        /// </summary>
        /// <param name="strString">Given word</param>
        /// <returns>Soundex string</returns>
        private string Soundex(string strString)
        {
            string strLetter;
            string strCode;
            string strSoundex;

            
            strSoundex = strString.Substring(0, 1).ToUpper();
            for (int i = 1; i < strString.Length; i++)
            {
                strLetter = strString.Substring(i, 1).ToUpper();
                switch (strLetter)
                {
                    case "B":
                    case "P":
                        strCode = "1";
                        break;
                    case "F":
                    case "V":
                        strCode = "2";
                        break;
                    case "C":
                    case "K":
                    case "S":
                        strCode = "3";
                        break;
                    case "G":
                    case "J":
                        strCode = "4";
                        break;
                    case "Q":
                    case "X":
                    case "Z":
                        strCode = "5";
                        break;
                    case "D":
                    case "T":
                        strCode = "6";
                        break;
                    case "L":
                        strCode = "7";
                        break;
                    case "M":
                    case "N":
                        strCode = "8";
                        break;
                    case "R":
                        strCode = "9";
                        break;
                    default:
                        strCode = "";
                        break;
                }

                if (strSoundex.Substring(strSoundex.Length - 1, 1) != strCode)
                    strSoundex += strCode;
            }
            return strSoundex;
        }
        #endregion

        #region WordSimilarity
        /// <summary>
        /// Calculate Similarity rate between a word and the word spelling-check against to
        /// </summary>
        /// <param name="strWord">Word spelling-check against</param>
        /// <param name="strSimilarWord">Word considered to be similar to the given word</param>
        /// <returns></returns>
        private double WordSimilarity(string strWord, string strSimilarWord)
        {
            int intWordLen = strWord.Length;
            int intSimilarWordLen = strSimilarWord.Length;
            int intMaxBonus = 3;
            double dblPerfectValue = intWordLen + intWordLen + intMaxBonus;
            int intSimilarity = intMaxBonus - Math.Abs(intWordLen - intSimilarWordLen);
            int intShorterOne = (intWordLen < intSimilarWordLen ? intWordLen : intSimilarWordLen);

            for (int i = 1; i < intWordLen; i++)
            {
                if (i < intSimilarWordLen)
                {
                    if (strWord.Substring(i, 1).ToLower() == strSimilarWord.Substring(i, 1).ToLower())
                    {
                        intSimilarity++;
                    }

                    if (strWord.Substring(intWordLen - i, 1).ToLower() == 
                        strSimilarWord.Substring(intSimilarWordLen - i, 1).ToLower())
                    {
                        intSimilarity++;
                    }
                }
            }         
            return (double)intSimilarity / dblPerfectValue;
        }
        #endregion

        #region lbHint_SelectedIndexChanged
        /// <summary>
        /// Handle to the event triggered by listbox selected item changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void lbHint_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tbManual.Text == string.Empty)
            {
                hfFinal.Value = replaceFirst(hfFinal.Value, arrWords[iCurrentCount], lbHint.SelectedItem.Text);
                tbCurrentWord.Text = lbHint.SelectedItem.Text;           
            }                    
        }
        #endregion

        #region tbManual_TextChanged
        /// <summary>
        /// Handle the event triggered by user manually entering a word 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void tbManual_TextChanged(object sender, EventArgs e)
        {            
            if (tbManual.Text != string.Empty)
            {
                hfFinal.Value = replaceFirst(hfFinal.Value, arrWords[iCurrentCount], tbManual.Text.Trim());
                tbCurrentWord.Text = tbManual.Text.Trim();
            }
        }
        #endregion

        #region replaceFirst
        /// <summary>
        /// Replace first occurrence of a word in a string
        /// </summary>
        /// <param name="text">string from which a word to be searched</param>
        /// <param name="search">word to be replaced</param>
        /// <param name="replace">word for replacing</param>
        /// <returns>Modified input string</returns>
        private string replaceFirst(string text, string search, string replace)
        {
            int pos = searchWord(text, search);
            if (pos < 0)
                return text;
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }
        #endregion

        #region searchWord
        /// <summary>
        /// Search a word position in a string
        /// </summary>
        /// <param name="strs">Input string</param>
        /// <param name="str">Word to be searched</param>
        /// <returns>First positon of the word found in the input string</returns>
        private int searchWord(string strs, string str)
        {
            int i = 0, j = 0;            
            while (i < strs.Length)
            {
                while (strs[i] == str[j])
                {
                    i++;
                    j++;
                    if (j == str.Length)
                        if (i == strs.Length || Char.IsLetter(strs[i]) == false)
                            return i - str.Length;
                        else
                            break;
                    if (strs[i] != str[j])
                        break;                    
                }
                j = 0;
                i++;
            }
            return -1;
        }

        #endregion
    }
}