using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.XPath;

namespace PolarShadow.Core
{
    public struct HtmlElement
    {
        private readonly XPathNavigator _node;
        private readonly XPathNodeIterator _nodes;

        private HtmlValueKind _valueKind;

        public HtmlValueKind ValueKind => _valueKind;

        public HtmlElement(XPathDocument doc) : this(doc.CreateNavigator()) { }

        public HtmlElement(XPathNavigator nav)
        {
            _node = nav;
            _nodes = default;
            _valueKind = HtmlValueKind.Node;
        }

        public HtmlElement(XPathNodeIterator nodes)
        {
            _nodes = nodes;
            _node = default;
            _valueKind = HtmlValueKind.Nodes;
        }

        public HtmlElement Select(string xpath)
        {
            if (_valueKind == HtmlValueKind.Node)
            {
                var result = _node.Select(xpath);
                if (result.Count == 0)
                {
                    return default;
                }
                else if (result.Count == 1)
                {
                    result.MoveNext();
                    return new HtmlElement(result.Current);
                }
                return new HtmlElement(result);
            }
            else if (_valueKind == HtmlValueKind.Nodes)
            {
                XPathNodeCollection nodes = new XPathNodeCollection();
                while (_nodes.MoveNext())
                {
                    var childs = _nodes.Current.Select(xpath);
                    while (childs.MoveNext())
                    {
                        nodes.Add(childs.Current);
                    }
                }
                return new HtmlElement(nodes);
            }

            return default;
        }

        public IEnumerable<HtmlElement> EnumerateNodes()
        {
            if (_valueKind != HtmlValueKind.Nodes)
            {
                throw new InvalidOperationException($"The value kind must be ValueKind.Nodes: {_node?.Name}");
            }

            while (_nodes.MoveNext())
            {
                yield return new HtmlElement(_nodes.Current);
            }
        }

        public string GetValue()
        {
            if (_valueKind == HtmlValueKind.Node)
            {
                return _node.Value;
            }
            else if (_valueKind == HtmlValueKind.Nodes)
            {
                var sb = new StringBuilder();
                var clone = _nodes.Clone();
                while (clone.MoveNext())
                {
                    sb.AppendLine(clone.Current.Value);
                }
                return sb.ToString();
            }

            return default;
        }

        public override string ToString()
        {
            switch (_valueKind)
            {
                case HtmlValueKind.Undefined:
                    return HtmlValueKind.Undefined.ToString();
                case HtmlValueKind.Node:
                    return _node.Name;
                case HtmlValueKind.Nodes:
                    var sb = new StringBuilder();
                    var clone = _nodes.Clone();
                    while (clone.MoveNext())
                    {
                        sb.AppendLine(clone.Current.Name);
                    }
                    return sb.ToString();
                default:
                    break;
            }
            return "";
        }
    }
}
