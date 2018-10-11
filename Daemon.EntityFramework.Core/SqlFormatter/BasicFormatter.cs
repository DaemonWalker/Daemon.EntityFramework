using System;
using System.Collections.Generic;
using System.Text;

namespace Daemon.EntityFramework.Core.SqlFormatter
{
    public class BasicFormatter : IFormatter
    {
        protected static readonly HashSet<string> beginClauses = new HashSet<string>();
        protected static readonly HashSet<string> dml = new HashSet<string>();
        protected static readonly HashSet<string> endClauses = new HashSet<string>();
        protected const string IndentString = "    ";
        protected const string Initial = "\n    ";
        protected static readonly HashSet<string> logical = new HashSet<string>();
        protected static readonly HashSet<string> misc = new HashSet<string>();
        protected static readonly HashSet<string> quantifiers = new HashSet<string>();

        static BasicFormatter()
        {
            beginClauses.Add("left");
            beginClauses.Add("right");
            beginClauses.Add("inner");
            beginClauses.Add("outer");
            beginClauses.Add("group");
            beginClauses.Add("order");
            endClauses.Add("where");
            endClauses.Add("set");
            endClauses.Add("having");
            endClauses.Add("join");
            endClauses.Add("from");
            endClauses.Add("by");
            endClauses.Add("join");
            endClauses.Add("into");
            endClauses.Add("union");
            logical.Add("and");
            logical.Add("or");
            logical.Add("when");
            logical.Add("else");
            logical.Add("end");
            quantifiers.Add("in");
            quantifiers.Add("all");
            quantifiers.Add("exists");
            quantifiers.Add("some");
            quantifiers.Add("any");
            dml.Add("insert");
            dml.Add("update");
            dml.Add("delete");
            misc.Add("select");
            misc.Add("on");
        }

        public virtual string Format(string source)
        {
            return new FormatProcess(source).Perform();
        }

        private class FormatProcess
        {
            private bool afterBeginBeforeEnd;
            private bool afterBetween;
            private readonly List<bool> afterByOrFromOrSelects = new List<bool>();
            private bool afterByOrSetOrFromOrSelect;
            private bool afterInsert;
            private bool afterOn;
            private bool beginLine = true;
            private bool endCommandFound;
            private int indent = 1;
            private int inFunction;
            private string lastToken;
            private string lcToken;
            private readonly List<int> parenCounts = new List<int>();
            private int parensSinceSelect;
            private readonly StringBuilder result = new StringBuilder();
            private string token;
            private readonly IEnumerator<string> tokens;

            public FormatProcess(string sql)
            {
                this.tokens = new StringTokenizer(sql, "()+*/-=<>'`\"[],; \n\r\f\t", true).GetEnumerator();
            }

            private void BeginNewClause()
            {
                if (!this.afterBeginBeforeEnd)
                {
                    if (this.afterOn)
                    {
                        this.indent--;
                        this.afterOn = false;
                    }
                    this.indent--;
                    this.Newline();
                }
                this.Out();
                this.beginLine = false;
                this.afterBeginBeforeEnd = true;
            }

            private void CloseParen()
            {
                if (this.endCommandFound)
                {
                    this.Out();
                }
                else
                {
                    this.parensSinceSelect--;
                    if (this.parensSinceSelect < 0)
                    {
                        this.indent--;
                        int num = this.parenCounts[this.parenCounts.Count - 1];
                        this.parenCounts.RemoveAt(this.parenCounts.Count - 1);
                        this.parensSinceSelect = num;
                        bool flag = this.afterByOrFromOrSelects[this.afterByOrFromOrSelects.Count - 1];
                        this.afterByOrFromOrSelects.RemoveAt(this.afterByOrFromOrSelects.Count - 1);
                        this.afterByOrSetOrFromOrSelect = flag;
                    }
                    if (this.inFunction > 0)
                    {
                        this.inFunction--;
                        this.Out();
                    }
                    else
                    {
                        if (!this.afterByOrSetOrFromOrSelect)
                        {
                            this.indent--;
                            this.Newline();
                        }
                        this.Out();
                    }
                    this.beginLine = false;
                }
            }

            private void CommaAfterByOrFromOrSelect()
            {
                this.Out();
                this.Newline();
            }

            private void CommaAfterOn()
            {
                this.Out();
                this.indent--;
                this.Newline();
                this.afterOn = false;
                this.afterByOrSetOrFromOrSelect = true;
            }

            private void EndNewClause()
            {
                if (!this.afterBeginBeforeEnd)
                {
                    this.indent--;
                    if (this.afterOn)
                    {
                        this.indent--;
                        this.afterOn = false;
                    }
                    this.Newline();
                }
                this.Out();
                if (!"union".Equals(this.lcToken))
                {
                    this.indent++;
                }
                this.Newline();
                this.afterBeginBeforeEnd = false;
                this.afterByOrSetOrFromOrSelect = ("by".Equals(this.lcToken) || "set".Equals(this.lcToken)) || "from".Equals(this.lcToken);
            }

            private void ExtractStringEnclosedBy(string stringDelimiter)
            {
                while (this.tokens.MoveNext())
                {
                    string current = this.tokens.Current;
                    this.token = this.token + current;
                    if (stringDelimiter.Equals(current))
                    {
                        return;
                    }
                }
            }

            private static bool IsFunctionName(string tok)
            {
                char c = tok[0];
                return (((((((char.IsLetter(c) || (c.CompareTo('$') == 0)) || (c.CompareTo('_') == 0)) || ('"' == c)) && !BasicFormatter.logical.Contains(tok)) && (!BasicFormatter.endClauses.Contains(tok) && !BasicFormatter.quantifiers.Contains(tok))) && !BasicFormatter.dml.Contains(tok)) && !BasicFormatter.misc.Contains(tok));
            }

            private bool IsMultiQueryDelimiter(string delimiter)
            {
                return ";".Equals(delimiter);
            }

            private static bool IsWhitespace(string token)
            {
                return (" \n\r\f\t".IndexOf(token) >= 0);
            }

            private void Logical()
            {
                if ("end".Equals(this.lcToken))
                {
                    this.indent--;
                }
                this.Newline();
                this.Out();
                this.beginLine = false;
            }

            private void Misc()
            {
                this.Out();
                if ("between".Equals(this.lcToken))
                {
                    this.afterBetween = true;
                }
                if (this.afterInsert)
                {
                    this.Newline();
                    this.afterInsert = false;
                }
                else
                {
                    this.beginLine = false;
                    if ("case".Equals(this.lcToken))
                    {
                        this.indent++;
                    }
                }
            }

            private void Newline()
            {
                this.result.Append("\n");
                for (int i = 0; i < this.indent; i++)
                {
                    this.result.Append("    ");
                }
                this.beginLine = true;
            }

            private void On()
            {
                this.indent++;
                this.afterOn = true;
                this.Newline();
                this.Out();
                this.beginLine = false;
            }

            private void OpenParen()
            {
                if (this.endCommandFound)
                {
                    this.Out();
                }
                else
                {
                    if (IsFunctionName(this.lastToken) || (this.inFunction > 0))
                    {
                        this.inFunction++;
                    }
                    this.beginLine = false;
                    if (this.inFunction > 0)
                    {
                        this.Out();
                    }
                    else
                    {
                        this.Out();
                        if (!this.afterByOrSetOrFromOrSelect)
                        {
                            this.indent++;
                            this.Newline();
                            this.beginLine = true;
                        }
                    }
                    this.parensSinceSelect++;
                }
            }

            private void Out()
            {
                this.result.Append(this.token);
            }

            public string Perform()
            {
                this.result.Append("\n    ");
                while (this.tokens.MoveNext())
                {
                    this.token = this.tokens.Current;
                    this.lcToken = this.token.ToLowerInvariant();
                    if ("'".Equals(this.token))
                    {
                        this.ExtractStringEnclosedBy("'");
                    }
                    else if ("\"".Equals(this.token))
                    {
                        this.ExtractStringEnclosedBy("\"");
                    }
                    if (this.IsMultiQueryDelimiter(this.token))
                    {
                        this.StartingNewQuery();
                    }
                    else if (this.afterByOrSetOrFromOrSelect && ",".Equals(this.token))
                    {
                        this.CommaAfterByOrFromOrSelect();
                    }
                    else if (this.afterOn && ",".Equals(this.token))
                    {
                        this.CommaAfterOn();
                    }
                    else if ("(".Equals(this.token))
                    {
                        this.OpenParen();
                    }
                    else if (")".Equals(this.token))
                    {
                        this.CloseParen();
                    }
                    else if (BasicFormatter.beginClauses.Contains(this.lcToken))
                    {
                        this.BeginNewClause();
                    }
                    else if (BasicFormatter.endClauses.Contains(this.lcToken))
                    {
                        this.EndNewClause();
                    }
                    else if ("select".Equals(this.lcToken))
                    {
                        this.Select();
                    }
                    else if (BasicFormatter.dml.Contains(this.lcToken))
                    {
                        this.UpdateOrInsertOrDelete();
                    }
                    else if ("values".Equals(this.lcToken))
                    {
                        this.Values();
                    }
                    else if ("on".Equals(this.lcToken))
                    {
                        this.On();
                    }
                    else if (this.afterBetween && this.lcToken.Equals("and"))
                    {
                        this.Misc();
                        this.afterBetween = false;
                    }
                    else if (BasicFormatter.logical.Contains(this.lcToken))
                    {
                        this.Logical();
                    }
                    else if (IsWhitespace(this.token))
                    {
                        this.White();
                    }
                    else
                    {
                        this.Misc();
                    }
                    if (!IsWhitespace(this.token))
                    {
                        this.lastToken = this.lcToken;
                    }
                }
                return this.result.ToString();
            }

            private void Select()
            {
                this.Out();
                this.indent++;
                this.Newline();
                this.parenCounts.Insert(this.parenCounts.Count, this.parensSinceSelect);
                this.afterByOrFromOrSelects.Insert(this.afterByOrFromOrSelects.Count, this.afterByOrSetOrFromOrSelect);
                this.parensSinceSelect = 0;
                this.afterByOrSetOrFromOrSelect = true;
                this.endCommandFound = false;
            }

            private void StartingNewQuery()
            {
                this.Out();
                this.indent = 1;
                this.endCommandFound = true;
                this.Newline();
            }

            private void UpdateOrInsertOrDelete()
            {
                this.Out();
                this.indent++;
                this.beginLine = false;
                if ("update".Equals(this.lcToken))
                {
                    this.Newline();
                }
                if ("insert".Equals(this.lcToken))
                {
                    this.afterInsert = true;
                }
                this.endCommandFound = false;
            }

            private void Values()
            {
                this.indent--;
                this.Newline();
                this.Out();
                this.indent++;
                this.Newline();
            }

            private void White()
            {
                if (!this.beginLine)
                {
                    this.result.Append(" ");
                }
            }
        }
    }
}
