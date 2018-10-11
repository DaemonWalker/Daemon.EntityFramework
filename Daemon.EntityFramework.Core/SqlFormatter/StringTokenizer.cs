using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Daemon.EntityFramework.Core.SqlFormatter
{
    public class StringTokenizer : IEnumerable<string>, IEnumerable
    {
        private const string _defaultDelim = " \t\n\r\f";
        private string _delim;
        private string _origin;
        private bool _returnDelim;

        public StringTokenizer(string str)
        {
            this._origin = str;
            this._delim = " \t\n\r\f";
            this._returnDelim = false;
        }

        public StringTokenizer(string str, string delim)
        {
            this._origin = str;
            this._delim = delim;
            this._returnDelim = true;
        }

        public StringTokenizer(string str, string delim, bool returnDelims)
        {
            this._origin = str;
            this._delim = delim;
            this._returnDelim = returnDelims;
        }

        public IEnumerator<string> GetEnumerator()
        {
            return new StringTokenizerEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new StringTokenizerEnumerator(this);
        }

        private class StringTokenizerEnumerator : IEnumerator<string>, IDisposable, IEnumerator
        {
            private int _cursor;
            private string _next;
            private StringTokenizer _stokenizer;

            public StringTokenizerEnumerator(StringTokenizer stok)
            {
                this._stokenizer = stok;
            }

            public void Dispose()
            {
            }

            private string GetNext()
            {
                if (this._cursor >= this._stokenizer._origin.Length)
                {
                    return null;
                }
                char ch = this._stokenizer._origin[this._cursor];
                if (this._stokenizer._delim.IndexOf(ch) != -1)
                {
                    this._cursor++;
                    if (this._stokenizer._returnDelim)
                    {
                        return ch.ToString();
                    }
                    return this.GetNext();
                }
                int length = this._stokenizer._origin.IndexOfAny(this._stokenizer._delim.ToCharArray(), this._cursor);
                if (length == -1)
                {
                    length = this._stokenizer._origin.Length;
                }
                string str = this._stokenizer._origin.Substring(this._cursor, length - this._cursor);
                this._cursor = length;
                return str;
            }

            public bool MoveNext()
            {
                this._next = this.GetNext();
                return (this._next != null);
            }

            public void Reset()
            {
                this._cursor = 0;
            }

            public string Current
            {
                get
                {
                    return this._next;
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return this.Current;
                }
            }
        }
    }
}
