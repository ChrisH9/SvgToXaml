using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace SvgToXaml
{
    class Program
    {
        static void Main(string[] args)
        {
            string fileName = args[0];

            if (!fileName.EndsWith(".svg"))
            {
                Console.WriteLine("Please input an svg file");
                return;
            }

            // load file
            XDocument xdoc = XDocument.Load(fileName);


            var svgNode = xdoc
                .Elements().FirstOrDefault(x => x.Name.LocalName.Equals("svg", StringComparison.InvariantCultureIgnoreCase));
            
            
            XElement styleNode = svgNode.Elements().FirstOrDefault(x => x.Name.LocalName.Equals("style", StringComparison.InvariantCultureIgnoreCase));

            XElement dataNode = svgNode.Elements().FirstOrDefault(x => x.Name.LocalName.Equals("g", StringComparison.InvariantCultureIgnoreCase));

            StringBuilder xamlBuilder = InitializeBuilder();

            IEnumerable<Style> styles = ParseStyles(styleNode);
            
            ParseData(dataNode, xamlBuilder, styles);

            FinalizeBuilder(xamlBuilder);

            Console.WriteLine(xamlBuilder.ToString());
            // loop through each data element

            // check if path or other

            // get data and style class from each path
        }

        private static StringBuilder InitializeBuilder()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder
                .AppendLine("<DrawingImage>")
                .Append("\t").AppendLine("<DrawingImage.Drawing>")
                .Append("\t").Append("\t").AppendLine("<DrawingGroup>");

            return stringBuilder;
        }
        
        private static StringBuilder FinalizeBuilder(StringBuilder stringBuilder)
        {
            stringBuilder
                .Append("\t").Append("\t").AppendLine("</DrawingGroup>")
                .Append("\t").AppendLine("</DrawingImage.Drawing>")
                .AppendLine("</DrawingImage>");

            return stringBuilder;
        }

        private static IEnumerable<Style> ParseStyles(XElement styleNode)
        {
            string styleString = styleNode.Value;
            var styleSegments = styleString.Split('.');

            var styles = new List<Style>();
            
            foreach (string segment in styleSegments)
            {
                if (!string.IsNullOrWhiteSpace(segment))
                {
                    styles.Add(ParseStyle(segment));
                }
            }
            
            return styles;
        }

        private static Style ParseStyle(string segment)
        {
            int open = segment.IndexOf('{');
            int close = segment.IndexOf('}');

            Style style = new Style();
            style.Name = segment.Substring(0, open);

            string propertyString = segment.Substring(open + 1, close - open - 1);

            string[] properties = propertyString.Split(';');

            foreach (string property in properties)
            {
                string[] kvp = property.Split(':');

                if (kvp[0].Equals("fill", StringComparison.InvariantCultureIgnoreCase))
                {
                    style.Fill = kvp[1];
                }
            }
            
            return style;
        }
        
        private static void ParseData(XElement dataNode, StringBuilder xamlBuilder, IEnumerable<Style> styles)
        {
            IEnumerable<XElement> dataElements = dataNode.Elements();

            foreach (var element in dataElements)
            {
                if (element.Name.LocalName.Equals("path", StringComparison.InvariantCultureIgnoreCase))
                {
                    ConvertPathToXaml(element, xamlBuilder, styles);
                }
                else if (element.Name.LocalName.Equals("circle", StringComparison.InvariantCultureIgnoreCase))
                {
                    ConvertCircleToXaml(element, xamlBuilder, styles);
                }
            }
        }

        private static void ConvertCircleToXaml(XElement element, StringBuilder xamlBuilder, IEnumerable<Style> styles)
        {
            throw new NotImplementedException();
        }

        private static void ConvertPathToXaml(XElement pathNode, StringBuilder xamlBuilder, IEnumerable<Style> styles)
        {
            string className = pathNode.Attributes().FirstOrDefault(x =>
                x.Name.LocalName.Equals("class", StringComparison.InvariantCultureIgnoreCase))?.Value;
            
            string data = pathNode.Attributes().FirstOrDefault(x =>
                x.Name.LocalName.Equals("d", StringComparison.InvariantCultureIgnoreCase))?.Value;

            xamlBuilder.Append("\t\t\t").Append($"<GeometryDrawing Geometry=\"{data}\"");

            Style style = styles.FirstOrDefault(x => x.Name.Equals(className, StringComparison.InvariantCultureIgnoreCase));
            
            if (style?.Fill != null)
            {
                xamlBuilder.Append($" Brush=\"{style.Fill}\"");
            }

            xamlBuilder.AppendLine("/>");
        }
    }
}