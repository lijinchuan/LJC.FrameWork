using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace LJC.FrameWork.Comm
{
    public static class HTMLHelper
    {
        public static void Init()
        {

        }
        public static HtmlDocument CreateHtmlDocument(string html)
        {
            var doc= new HtmlDocument();
            doc.LoadHtml(html);
            return doc;
        }
        public static HtmlNode GetHTMLByID(this HtmlDocument doc , string id)
        {
            return doc.GetElementbyId(id);
        }
        public static HtmlNode GetHTMLByID(string html, string id)
        {
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);
            return doc.GetElementbyId(id);
        }
        public static IEnumerable<string> GetOuterHTMLByTags(string html, string tag, ref HtmlDocument doc)
        {
            doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);
            return GetOuterHTMLByTags(doc.DocumentNode, tag).ToList();
        }

        public static IEnumerable<string> GetOuterHTMLByTags(this HtmlNode docnode, string tag)
        {
            foreach (HtmlNode node in docnode.ChildNodes)
            {
                if (node.Name.Equals(tag, StringComparison.OrdinalIgnoreCase))
                {
                    yield return node.OuterHtml;
                }
            }
        }

        public static IEnumerable<string> GetInnerHTMLByTags(string html, string tag)
        {
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);
            return GetInnerHTMLByTags(doc.DocumentNode, tag).ToList();
        }

        public static IEnumerable<string> GetInnerHTMLByTags(this HtmlNode docnode, string tag)
        {
            foreach (HtmlNode node in docnode.ChildNodes)
            {
                if (node.Name.Equals(tag, StringComparison.OrdinalIgnoreCase))
                {
                    yield return node.InnerHtml;
                }
            }
        }

        public static IEnumerable<HtmlNode> GetHTMLNodeByTags(string html, string tag, ref HtmlDocument doc)
        {
            doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);
            return GetHTMLNodeByTags(doc.DocumentNode, tag).ToList();
        }

        public static IEnumerable<HtmlNode> GetHTMLNodeByTags(this HtmlNode docnode, string tag)
        {
            foreach (HtmlNode node in docnode.ChildNodes)
            {
                if (node.Name.Equals(tag, StringComparison.OrdinalIgnoreCase))
                {
                    yield return node;
                }
            }
        }

        public static bool TryParseLink(string linkHtml, out KeyValuePair<string, string> kv)
        {
            linkHtml = linkHtml ?? "";
            Regex rg = new Regex("<a\\s[^>]*href=[\"']?([\\w\\W]+)['\"]+\\s?[^>]*>([\\w\\W]*)</a>", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace);
            var m = rg.Match(linkHtml);
            if (m.Success)
            {
                kv = new KeyValuePair<string, string>(m.Groups[1].Value, m.Groups[2].Value);
                return true;
            }
            kv = new KeyValuePair<string, string>();
            return false;

        }

        public static string GetInnerText(string html)
        {
            Regex rg = new Regex(@"<[^>]+>");
            return rg.Replace(html, "").Trim();
        }

        public static HtmlNode NextSibling(this HtmlNode node, string tag)
        {
            if (node == null || node.NextSibling == null)
                return null;

            var next = node.NextSibling;
            while (next != null && !next.Name.Equals(tag,StringComparison.OrdinalIgnoreCase))
            {
                next = next.NextSibling;
            }
            return next.Name.Equals(tag, StringComparison.OrdinalIgnoreCase) ? next : null;
        }


        public static HtmlNode GetElementByIdEx(this HtmlDocument document,string id)
        {
            var idNode=document.GetElementbyId(id);
            if ("form".Equals(idNode.Name, StringComparison.OrdinalIgnoreCase))
            {
                string html = document.DocumentNode.InnerHtml.Replace("<form ", "<div ").Replace("</form>", "</div>");
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(html);
                var subidNode = doc.GetElementbyId(id);
                idNode.AppendChildren(subidNode.ChildNodes);
            }
            return idNode;
        }

        public static IEnumerable<HtmlNode> GetTagsByClass(this HtmlNode docnode, string tagname, string classname)
        {
            var nodes = docnode.SelectNodes(tagname);
            foreach (var node in nodes)
            {
                var cls=node.Attributes["class"];
                if(cls!=null&&(string.Equals(cls.Value,classname,StringComparison.OrdinalIgnoreCase))
                    ||cls.Value.ToLower().Split(new []{" "},StringSplitOptions.RemoveEmptyEntries).Contains(classname.ToLower())
                    )
                {
                    yield return node;
                }
            }
        }

        public static IEnumerable<HtmlNode> GetTagsByClass(string html, string tagname, string classname)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            return GetTagsByClass(doc.DocumentNode, tagname, classname).ToList();
        }

        public static Dictionary<string, string> GetFormById(this HtmlDocument doc, string id)
        {
            Dictionary<string, string> dics = new Dictionary<string, string>();
            var idForm = doc.GetElementbyId(id);
            if (idForm != null)
            {
                var inputs=idForm.SelectNodes("//input");
                foreach (var input in inputs)
                {
                    var name = input.Attributes["name"];
                    if (name!=null&&!string.IsNullOrEmpty(name.Value))
                    {
                        var value=input.Attributes["value"];
                        if (!dics.ContainsKey(name.Value))
                        {
                            dics.Add(name.Value, value == null ? "" : value.Value);
                        }
                        else
                        {
                            var keyname = string.Concat(name.Value, "/");
                            while (dics.ContainsKey(keyname))
                            {
                                keyname = string.Concat(keyname, "/");
                            }
                            dics.Add(keyname, value == null ? "" : value.Value);
                        }
                    }
                }
            }

            return dics;
        }
    }
}
