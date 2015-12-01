using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace AmarisGate.Helpers
{
    public class DisplayHelper
    {
        public static string BbCodeToHtml(string bbcode, bool withNewLine = false)
        {
            if (withNewLine)
                bbcode = bbcode != null ? string.Join("<br />", bbcode.Split(new[] { "\r\n" }, StringSplitOptions.None)) : "";
            bbcode = bbcode.Replace("[b]", "<strong>");
            bbcode = bbcode.Replace("[/b]", "</strong>");
            return bbcode;
        }
        public static string ThreeDots(string html, int maxLength)
        {
            var text = RemoveHtmlTags(html);
            if (text.Length <= maxLength)
                return html;
            text = text.Substring(0, maxLength - 3);
            return "<span class=\"3-dots\">"
                    + "<span class=\"simple-text\">"
                        + text
                        + " <a href=\"#\" class=\"expand\"><strong>...</strong></a>"
                    + "</span>"
                    + "<span class=\"full-html\" style=\"display:none\">"
                        + html
                        + " <a href=\"#\" class=\"reduce\">(reduce)</a>"
                    + "</span>"
                + "</span>"
                + "<script type=\"text/javascript\">"
                    + "$(document).ready(function () {"
                    + "    $('.expand').on('click', function () {"
                    + "        $(this).closest('.simple-text').css('display', 'none');"
                    + "        $(this).closest('.3-dots').children('.full-html').css('display', 'inline');"
                    + "        return false;"
                    + "    });"
                    + "    $('.reduce').on('click', function () {"
                    + "        $(this).closest('.full-html').css('display', 'none');"
                    + "        $(this).closest('.3-dots').children('.simple-text').css('display', 'inline');"
                    + "        return false;"
                    + "    });"
                    + "});"
                + "</script>"
            ;
        }

        public static string RemoveHtmlTags(string html)
        {
            html = html.Replace("<br />", " ");
            html = html.Replace("<br/>", " ");
            return Regex.Replace(html, "<.*?>", "");
        }
    }

}