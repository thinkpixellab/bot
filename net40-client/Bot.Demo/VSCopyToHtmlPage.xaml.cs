using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using PixelLab.Common;
using PixelLab.Demo.Core;

namespace PixelLab.Wpf.Demo
{
    [DemoMetadata("VS Copy to Html", "This demo takes copy output from Visual Studio and generates HTML for your blog.")]
    public partial class VSCopyToHtmlPage : Page
    {
        public VSCopyToHtmlPage()
        {
            InitializeComponent();
        }

        private void m_richTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            m_textBoxHtml.Clear();

            string html;
            try
            {
                html = getHtml(this.m_richTextBox.Document);
                m_textBoxHtml.SetValue(BackgroundProperty, DependencyProperty.UnsetValue);
            }
            catch (Exception ex)
            {
                if (Util.IsCriticalException(ex))
                {
                    throw;
                }
                else
                {
                    m_textBoxHtml.Background = Brushes.Red;
                    html = ex.ToString();
                }
            }

            m_textBoxHtml.Text = html;
        }

        private static string getHtml(FlowDocument vsContentFlowDocument)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("<div style=\"font-family:monospace;\">");

            foreach (Paragraph paragraph in vsContentFlowDocument.Blocks.Cast<Paragraph>())
            {
                bool firstRun = true;
                bool paragraphEmpty = true;
                string textToUse;

                Run[] runs = Decompose(paragraph.Inlines).ToArray();
                int nonSpaceCount = runs.Select(run => run.Text.Trim().Length).Sum();

                if (nonSpaceCount > 0)
                {
                    foreach (Run run in runs)
                    {
                        textToUse = run.Text;

                        if (run.Text.Length > 0)
                        {
                            paragraphEmpty = false;

                            if (firstRun)
                            {
                                int spaces = initialSpaceCount(textToUse);
                                if (spaces == 0)
                                {
                                    builder.Append("<div>");
                                }
                                else
                                {
                                    builder.AppendFormat(
                                        "<div style=\"margin-left:{0}ex;text-indent:-{1}ex;\">",
                                        spaces + 4,
                                        4);

                                    textToUse = textToUse.Substring(spaces);
                                }
                                firstRun = false;
                            }

                            builder.Append(toCleanedColoredSpan(textToUse, run.Foreground));
                        }
                    } // foreach (Run run in runs)
                } // if(nonSpaceCount > 0)

                if (paragraphEmpty)
                {
                    builder.Append("<div>&nbsp;</div>");
                }
                else
                {
                    builder.Append("</div>");
                    builder.AppendLine();
                }
            }

            builder.Append("</div>");

            return builder.ToString();
        }

        private static int initialSpaceCount(string str)
        {
            int count;
            for (count = 0; count < str.Length && str[count] == ' '; count++)
                ;
            return count;
        }

        private static string toCleanedColoredSpan(string run, Brush foreground)
        {
            SolidColorBrush solidColorBrush = foreground as SolidColorBrush;
            if (solidColorBrush == null)
            {
                return cleanString(run);
            }
            else
            {
                if (solidColorBrush.Color == Colors.Black || (run.Trim().Length == 0))
                {
                    return cleanString(run);
                }
                else
                {
                    string colorString = solidColorBrush.Color.ToString();
                    Debug.Assert(colorString.Length == 9);

                    colorString = colorString.Substring(3);

                    return string.Format("<span style=\"color:#{0};\">{1}</span>",
                        colorString, cleanString(run));
                }
            }
        }

        private static string cleanString(string source)
        {
            source = WebUtility.HtmlEncode(source);
            source = source.Replace("  ", "&nbsp; ");
            return source;
        }

        private static IEnumerable<Run> Decompose(IEnumerable<Inline> inlines)
        {
            foreach (Inline inline in inlines)
            {
                if (inline is Span)
                {
                    foreach (Run run in Decompose(((Span)inline).Inlines))
                    {
                        yield return run;
                    }
                }
                else if (inline is Run)
                {
                    yield return (Run)inline;
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
        }
    }
}