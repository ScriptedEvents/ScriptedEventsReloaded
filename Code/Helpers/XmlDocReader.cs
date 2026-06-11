using System.Reflection;
using System.Text;
using System.Xml;

namespace SER.Code.Helpers
{
    public static class XmlDocReader
    {
        private static readonly Dictionary<Assembly, XmlDocument?> LoadedDocs = new();

        public static string? GetDescription(MemberInfo member)
        {
            var assembly = member.Module.Assembly;
            if (!LoadedDocs.TryGetValue(assembly, out var doc))
            {
                doc = LoadDocForAssembly(assembly);
                LoadedDocs[assembly] = doc;
            }

            if (doc == null) return null;

            string memberName = GetMemberXmlName(member);
            var node = doc.SelectSingleNode($"//member[@name='{memberName}']/summary");
            
            return node == null ? null : ProcessXmlNodes(node).Trim();
        }

        private static string ProcessXmlNodes(XmlNode node)
        {
            var sb = new StringBuilder();
            foreach (XmlNode child in node.ChildNodes)
            {
                switch (child.NodeType)
                {
                    case XmlNodeType.Text:
                        sb.Append(child.Value);
                        break;
                    case XmlNodeType.Element:
                        switch (child.Name)
                        {
                            case "see":
                            case "seealso":
                                var cref = child.Attributes?["cref"]?.Value;
                                sb.Append(cref is { Length: > 0}
                                    ? FormatCref(cref) 
                                    : ProcessXmlNodes(child));
                                break;
                            case "paramref":
                            case "typeparamref":
                                var name = child.Attributes?["name"]?.Value;
                                if (!string.IsNullOrEmpty(name))
                                    sb.Append(name);
                                break;
                            case "para":
                            default:
                                sb.Append(ProcessXmlNodes(child));
                                break;
                        }
                        break;
                }
            }
            return sb.ToString();
        }

        private static string FormatCref(string cref)
        {
            if (string.IsNullOrEmpty(cref)) return string.Empty;

            // Remove prefix like T:, M:, P:, etc.
            int colonIndex = cref.IndexOf(':');
            string name = colonIndex != -1 ? cref[(colonIndex + 1)..] : cref;

            // Handle methods: M:Namespace.Type.Method(Args) -> Method
            int parenIndex = name.IndexOf('(');
            if (parenIndex != -1)
                name = name[..parenIndex];

            // Get last part of the name
            int lastDot = name.LastIndexOf('.');
            if (lastDot != -1)
                return name[(lastDot + 1)..];

            return name;
        }

        private static XmlDocument? LoadDocForAssembly(Assembly assembly)
        {
            try
            {
                // 1. Try disk
                string dllPath = assembly.Location;
                if (!string.IsNullOrEmpty(dllPath))
                {
                    string xmlPath = Path.ChangeExtension(dllPath, ".xml");
                    if (File.Exists(xmlPath))
                    {
                        var doc = new XmlDocument();
                        doc.Load(xmlPath);
                        return doc;
                    }
                }

                // 2. Try embedded resource (for the plugin itself or its dependencies)
                string resourceName = $"{assembly.GetName().Name}.xml";
                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        var doc = new XmlDocument();
                        doc.Load(stream);
                        return doc;
                    }
                }

                // 2b. Try embedded in the main SER assembly if it's a known dependency
                var serAssembly = typeof(XmlDocReader).Assembly;
                if (assembly != serAssembly)
                {
                    using (var stream = serAssembly.GetManifestResourceStream(resourceName))
                    {
                        if (stream != null)
                        {
                            var doc = new XmlDocument();
                            doc.Load(stream);
                            return doc;
                        }
                    }
                }
                
                return null;
            }
            catch
            {
                return null;
            }
        }

        private static string GetMemberXmlName(MemberInfo member)
        {
            char prefix = member switch
            {
                Type => 'T',
                PropertyInfo => 'P',
                FieldInfo => 'F',
                MethodInfo => 'M',
                _ => '?'
            };

            string fullName = member is Type { FullName: {} name } 
                ? name 
                : $"{member.DeclaringType?.FullName}.{member.Name}";
            
            // XML doc names use dots for nested types too, and have specific formats for methods,
            // but for properties/fields it's usually just TypeFullName.MemberName
            return $"{prefix}:{fullName.Replace('+', '.')}";
        }
    }
}
