using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.XPath;

namespace PolarShadow.Core
{
    internal class XPathNodeCollection : XPathNodeIterator, ICollection<XPathNavigator>
    {
        private readonly List<XPathNavigator> _source;

        private XPathNavigator _current;
        public override XPathNavigator Current => _current;

        private int _index = -1;
        public override int CurrentPosition => _index;
        public override int Count => _source == null ? 0 : _source.Count;

        public bool IsReadOnly => false;

        public XPathNodeCollection()
        {
            _source = new List<XPathNavigator>();
        }

        public XPathNodeCollection(XPathNodeCollection nodes)
        {
            _source = new List<XPathNavigator>(nodes);
        }

        public void Add(XPathNavigator item)
        {
            _source.Add(item);
        }

        public void Clear()
        {
            _source.Clear();
        }

        public override XPathNodeIterator Clone()
        {
            return new XPathNodeCollection(this);
        }

        public bool Contains(XPathNavigator item)
        {
            return _source.Contains(item);
        }

        public void CopyTo(XPathNavigator[] array, int arrayIndex)
        {
            _source.CopyTo(array, arrayIndex);
        }

        public override bool MoveNext()
        {
            if (_index < Count - 1)
            {
                _index++;
                _current = _source[_index];
                return true;
            }
            return false;
        }

        public bool Remove(XPathNavigator item)
        {
            return _source.Remove(item);
        }

        IEnumerator<XPathNavigator> IEnumerable<XPathNavigator>.GetEnumerator()
        {
            return _source.GetEnumerator();
        }
    }
}
