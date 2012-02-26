// -------------------------------------------------------------------------------------
// CSV Reader 1.0
// Copyright (c) 2012, Andrey Shvydky (Breeze Software)
// Dual licensed under the MIT or GPL Version 2 licenses.
// -------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;

namespace Breeze.Data.Csv {
    /// <summary>
    /// This class enumerates rows in CSV stream using small amount of memory
    /// </summary>
    public class CsvReader : IEnumerable<IEnumerable<string>>, IDisposable {
        TextReader input;

        public CsvReader(TextReader input) {
            if (input == null) throw new ArgumentNullException("input");
            this.input = input;
        }

        public IEnumerator<IEnumerable<string>> GetEnumerator() {
            return new RowEnumerator(input, false);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                if (input != null)
                    input.Close();
            }
            input = null;
        }

        public void Dispose() {
            this.Dispose(true);
        }
    }

    internal class RowEnumerator : IEnumerator<IEnumerable<string>> {
        TextReader input;
        ValueEnumerator current;
        bool preventClosing;

        public RowEnumerator(TextReader input, bool preventClosing) {
            if (input == null) throw new ArgumentNullException("input");
            this.input = input;
            this.preventClosing = preventClosing;
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                if (!preventClosing && input != null)
                    input.Close();
            }
            input = null;
        }

        public void Dispose() {
            this.Dispose(true);
        }

        public IEnumerable<string> Current {
            get {
                if (current == null)
                    throw new InvalidOperationException();
                return current;
            }
        }

        object IEnumerator.Current {
            get { return this.Current; }
        }

        public bool MoveNext() {
            // we should pass througth all values
            if (current != null) {
                while (current.MoveNext()) ;
                current.Dispose();
            }
            if (input.Peek() >= 0) {
                current = new ValueEnumerator(input, true);
                return true;
            } else
                return false;
        }

        public void Reset() {
            throw new NotSupportedException();
        }
    }

    internal class ValueEnumerator : IEnumerator<string>, IEnumerable<string> {
        TextReader input;
        bool preventClosing;
        string current;
        bool last = false;

        public ValueEnumerator(TextReader input, bool preventClosing) {
            if (input == null) throw new ArgumentNullException("input");
            this.input = input;
            this.preventClosing = preventClosing;
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                if (!preventClosing && input != null)
                    input.Close();
            }
            input = null;
        }

        public void Dispose() {
            this.Dispose(true);
        }

        public string Current {
            get {
                if (current == null)
                    throw new InvalidOperationException();
                return current;
            }
        }

        object IEnumerator.Current {
            get { return this.Current; }
        }

        public bool MoveNext() {
            if (last) {
                current = null;
                return false;
            }
            int ch = input.Read();
            bool beginQuote = false, endQuote = false;
            StringBuilder sb = new StringBuilder();
            while (true) {
                if (ch < 0) {
                    if (sb.Length > 0) {
                        current = sb.ToString();
                        return true;
                    } else {
                        last = true;
                        current = null;
                        return false;
                    }
                } else if (ch == ',' || ch == '\r' || ch == '\n') {
                    if (beginQuote && !endQuote)
                        sb.Append((char)ch);
                    else {
                        if (ch != ',') {
                            if (ch == '\r') {
                                if (input.Peek() == '\n')
                                    input.Read();
                            }
                            last = true;
                        }
                        current = sb.ToString();
                        return true;
                    }
                } else if (ch == '"') {
                    if (beginQuote && !endQuote) {
                        int next = input.Peek();
                        if (next == '"') {
                            ch = input.Read();
                            sb.Append((char)ch);
                        } else {
                            endQuote = true;
                        }
                    } else if (sb.Length > 0)
                        throw new CsvParseException("If fields are not enclosed with double quotes, then double quotes may not appear inside the fields");
                    else
                        beginQuote = true;
                } else
                    sb.Append((char)ch);
                ch = input.Read();
            }
        }

        public void Reset() {
            throw new NotSupportedException();
        }

        public IEnumerator<string> GetEnumerator() {
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this;
        }
    }

    public class CsvParseException : Exception {
        public CsvParseException(string message)
            : base(message) {
        }
    }
}
