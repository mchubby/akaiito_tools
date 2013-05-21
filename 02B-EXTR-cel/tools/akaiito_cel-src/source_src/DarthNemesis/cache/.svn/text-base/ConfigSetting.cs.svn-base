//-----------------------------------------------------------------------
// <copyright file="ConfigSetting.cs" company="VAkos">
// Copyright (c) 2007 All Right Reserved
// </copyright>
// <author>Vandra Akos</author>
// <date>2007-01-14</date>
// <summary>Represents a Configuration Node in the XML file.</summary>
//-----------------------------------------------------------------------

namespace DarthNemesis
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using System.Xml;

    /// <summary>
    /// Represents a Configuration Node in the XML file.
    /// </summary>
    public class ConfigSetting
    {
        /// <summary>
        /// The node from the XMLDocument, which it describes.
        /// </summary>
        private XmlNode node;
        
        /// <summary>
        /// Initializes a new instance of the ConfigSetting class.
        /// </summary>
        /// <param name="node">
        /// The XmlNode to describe.
        /// </param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", Justification = "Requires XmlNode-specific properties")]
        public ConfigSetting(XmlNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }
            
            this.node = node;
        }
        
        /// <summary>
        /// Prevents a default instance of the ConfigSetting class from being created.
        /// This class cannot be constructed directly. You will need to give a node to describe.
        /// </summary>
        private ConfigSetting()
        {
            throw new ArgumentException("Cannot be created directly. Needs a node parameter");
        }
        
        /// <summary>
        /// Gets the Name of the element it describes.
        /// </summary>
        /// <value>The Name of the element it describes.</value>
        /// <remarks>Read only property</remarks>
        public string Name
        {
            get
            {
                return this.node.Name;
            }
        }

        /// <summary>
        /// Gets or sets the string value of the specific Configuration Node.
        /// </summary>
        /// <value>String value of the specific Configuration Node.</value>
        public string Value
        {
            get
            {
                XmlNode xmlattrib = this.node.Attributes.GetNamedItem("value");
                if (xmlattrib != null)
                {
                    return xmlattrib.Value;
                }
                else
                {
                    return string.Empty;
                }
            }

            set
            {
                XmlNode xmlattrib = this.node.Attributes.GetNamedItem("value");
                if (!string.IsNullOrEmpty(value))
                {
                    if (xmlattrib == null)
                    {
                        xmlattrib = this.node.Attributes.Append(this.node.OwnerDocument.CreateAttribute("value"));
                    }
                    
                    xmlattrib.Value = value;
                }
                else if (xmlattrib != null)
                {
                    this.node.Attributes.RemoveNamedItem("value");
                }
            }
        }

        /// <summary>
        /// Gets or sets the int value of the specific Configuration Node.
        /// </summary>
        /// <value>The int value of the specific Configuration Node.</value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", Justification = "Default value expected when conversion fails")]
        public int Int32Value
        {
            get
            {
                int i;
                int.TryParse(this.Value, out i);
                return i;
            }
            
            set
            {
                this.Value = value.ToString(CultureInfo.InvariantCulture);
            }
        }
        
        /// <summary>
        /// Gets or sets the long value of the specific Configuration Node.
        /// </summary>
        /// <value>The long value of the specific Configuration Node.</value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", Justification = "Default value expected when conversion fails")]
        public long Int64Value
        {
            get
            {
                long i;
                long.TryParse(this.Value, out i);
                return i;
            }
            
            set
            {
                this.Value = value.ToString(CultureInfo.InvariantCulture);
            }
        }
        
        /// <summary>
        /// Gets or sets a value indicating whether the bool value of the specific Configuration Node is set to true.
        /// </summary>
        /// <value>The bool value of the specific Configuration Node.</value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", Justification = "Default value expected when conversion fails")]
        public bool BooleanValue
        {
            get
            {
                bool b;
                bool.TryParse(this.Value, out b);
                return b;
            }
            
            set
            {
                this.Value = value.ToString();
            }
        }

        /// <summary>
        /// Gets or sets the float value of the specific Configuration Node.
        /// </summary>
        /// <value>The float value of the specific Configuration Node.</value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", Justification = "Default value expected when conversion fails")]
        public float SingleValue
        {
            get
            {
                float f;
                float.TryParse(this.Value, out f);
                return f;
            }
            
            set
            {
                this.Value = value.ToString(CultureInfo.InvariantCulture);
            }
        }
        
        /// <summary>
        /// Gets or sets the double value of the specific Configuration Node.
        /// </summary>
        /// <value>The double value of the specific Configuration Node.</value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", Justification = "Default value expected when conversion fails")]
        public double DoubleValue
        {
            get
            {
                double d;
                double.TryParse(this.Value, out d);
                return d;
            }
            
            set
            {
                this.Value = value.ToString(CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Get a specific child node.
        /// </summary>
        /// <param name="path">
        /// The path to the specific node. Can be either only a name, or a full path separated by '/' or '\'.
        /// </param>
        /// <example>
        /// <code>
        /// XmlConfig conf = new XmlConfig("configuration.xml");
        /// screenname = conf.Settings["screen"].Value;
        /// height = conf.Settings["screen/height"].IntValue;
        ///  // OR
        /// height = conf.Settings["screen"]["height"].IntValue;
        /// </code>
        /// </example>
        /// <returns>
        /// The specific child node
        /// </returns>
        public ConfigSetting this[string path]
        {
            get
            {
                char[] separators = { '/', '\\' };
                path = path.Trim(separators);
                string[] pathsection = path.Split(separators);
                
                XmlNode selectednode = this.node;
                XmlNode newnode;

                foreach (string asection in pathsection)
                {
                    string section = asection;
                    
                    if (asection.Length > 0 && Char.IsNumber(asection[0]))
                    {
                        section = "N." + asection;
                    }
                    
                    string nodename, nodeposstr;
                    int nodeposition;
                    int indexofdiez = section.IndexOf('#');

                    // No position defined, take the first one by default
                    if (indexofdiez == -1)
                    {
                        nodename = section;
                        nodeposition = 1;
                    }
                    else
                    {
                        nodename = section.Substring(0, indexofdiez); // Node name is before the diez character
                        nodeposstr = section.Substring(indexofdiez + 1);
                        
                        // Double diez means he wants to create a new node
                        if (nodeposstr == "#")
                        {
                            nodeposition = this.GetNamedChildrenCount(nodename) + 1;
                        }
                        else
                        {
                            nodeposition = int.Parse(nodeposstr, CultureInfo.InvariantCulture);
                        }
                    }

                    // Verify name
                    foreach (char c in nodename)
                    {
                        if (IsReserved(c))
                        {
                            return null;
                        }
                    }

                    string transformedpath = String.Format(CultureInfo.InvariantCulture, "{0}[{1}]", nodename, nodeposition);
                    newnode = selectednode.SelectSingleNode(transformedpath);

                    while (newnode == null)
                    {
                        XmlElement newelement = selectednode.OwnerDocument.CreateElement(nodename);
                        selectednode.AppendChild(newelement);
                        newnode = selectednode.SelectSingleNode(transformedpath);
                    }
                    
                    selectednode = newnode;
                }

                return new ConfigSetting(selectednode);
            }
        }
        
        /// <summary>
        /// Get a specific child node.
        /// </summary>
        /// <param name="index">
        /// A numeric index to be converted to a node path.
        /// </param>
        /// <example>
        /// <code>
        /// XmlConfig conf = new XmlConfig("configuration.xml");
        /// screenname = conf.Settings["screen"].Value;
        /// height = conf.Settings["screen/height"].IntValue;
        ///  // OR
        /// height = conf.Settings["screen"]["height"].IntValue;
        /// </code>
        /// </example>
        /// <returns>
        /// The specific child node
        /// </returns>
        public ConfigSetting this[int index]
        {
            get
            {
                return this[string.Empty + index];
            }
        }
        
        /// <summary>
        /// Gets the number of children of the specific node.
        /// </summary>
        /// <param name="unique">
        /// If true, get only the number of children with distinct names.
        /// So if it has two nodes with "foo" name, and three nodes
        /// named "bar", the return value will be 2. In the same case, if unique
        /// was false, the return value would have been 2 + 3 = 5.
        /// </param>
        /// <returns>
        /// The number of (uniquely named) children.
        /// </returns>
        public int ChildCount(bool unique)
        {
            IList<string> names = this.ChildrenNames(unique);
            return (names != null) ? names.Count : 0;
        }

        /// <summary>
        /// Gets the names of children of the specific node.
        /// </summary>
        /// <param name="unique">
        /// If true, get only distinct names.
        /// So if it has two nodes with "foo" name, and three nodes
        /// named "bar", the return value will be {"bar","foo"} .
        /// In the same case, if unique was false, the return value
        /// would have been {"bar","bar","bar","foo","foo"}.
        /// </param>
        /// <returns>
        /// An IList object with the names of (uniquely named) children.
        /// </returns>
        public IList<string> ChildrenNames(bool unique)
        {
            if (this.node.ChildNodes.Count == 0)
            {
                return null;
            }
            
            List<string> stringlist = new List<string>();

            foreach (XmlNode achild in this.node.ChildNodes)
            {
                string name = achild.Name;
                if ((!unique) || (!stringlist.Contains(name)))
                {
                    stringlist.Add(name);
                }
            }
            
            stringlist.Sort();
            return stringlist;
        }

        /// <summary>
        /// Retrieves a list of each and every child node.
        /// </summary>
        /// <returns>An IList compatible object describing each and every child node.</returns>
        /// <remarks>Read only property</remarks>
        public IList<ConfigSetting> Children()
        {
            if (this.ChildCount(false) == 0)
            {
                return null;
            }
            
            List<ConfigSetting> list = new List<ConfigSetting>();

            foreach (XmlNode achild in this.node.ChildNodes)
            {
                list.Add(new ConfigSetting(achild));
            }
            
            return list;
        }
        
        /// <summary>
        /// Get all children with the same name, specified in the name parameter.
        /// </summary>
        /// <param name="name">
        /// An alphanumerical string, containing the name of the child nodes to return.
        /// </param>
        /// <returns>
        /// An array with the child nodes with the specified name, or null
        /// if no childs with the specified name exist.
        /// </returns>
        public IList<ConfigSetting> GetNamedChildren(string name)
        {
            foreach (char c in name)
            {
                if (IsReserved(c))
                {
                    throw new ArgumentException("Name MUST be alphanumerical!");
                }
            }
            
            XmlNodeList xmlnl = this.node.SelectNodes(name);
            List<ConfigSetting> css = new List<ConfigSetting>();
            foreach (XmlNode achild in xmlnl)
            {
                css.Add(new ConfigSetting(achild));
            }
            
            return css;
        }

        /// <summary>
        /// Gets the number of childs with the specified name.
        /// </summary>
        /// <param name="name">
        /// An alphanumerical string with the name of the nodes to look for.
        /// </param>
        /// <returns>
        /// An integer with the count of the nodes.
        /// </returns>
        public int GetNamedChildrenCount(string name)
        {
            foreach (char c in name)
            {
                if (IsReserved(c))
                {
                    throw new ArgumentException("Name MUST be alphanumerical!");
                }
            }
            
            return this.node.SelectNodes(name).Count;
        }

        /// <summary>
        /// Check if the node conforms with the config xml restrictions.
        /// 1. No nodes with two children of the same name.
        /// 2. Only alphanumerical names.
        /// </summary>
        /// <returns>
        /// True on success and false on failure.
        /// </returns>
        public bool Validate()
        {
            // Check this node's name for validity
            foreach (char c in this.Name)
            {
                if (IsReserved(c))
                {
                    return false;
                }
            }

            // If there are no children, the node is valid.
            // If there the node has other children, check all of them for validity
            if (this.ChildCount(false) == 0)
            {
                return true;
            }
            else
            {
                foreach (ConfigSetting cs in this.Children())
                {
                    if (!cs.Validate())
                    {
                        return false;
                    }
                }
            }
            
            return true;
        }

        /// <summary>
        /// Removes any empty nodes from the tree,
        /// that is it removes a node, if it hasn't got any
        /// children, or neither of its children have got a value.
        /// </summary>
        public void Clean()
        {
            if (this.ChildCount(false) != 0)
            {
                foreach (ConfigSetting cs in this.Children())
                {
                    cs.Clean();
                }
            }
            
            if (this.ChildCount(false) == 0 && string.IsNullOrEmpty(this.Value))
            {
                this.Remove();
            }
        }
        
        /// <summary>
        /// Remove the specific node from the tree.
        /// </summary>
        public void Remove()
        {
            if (this.node.ParentNode != null)
            {
                this.node.ParentNode.RemoveChild(this.node);
            }
        }

        /// <summary>
        /// Remove all children of the node, but keep the node itself.
        /// </summary>
        public void RemoveChildren()
        {
            this.node.RemoveAll();
        }
        
        private static bool IsReserved(char c)
        {
            return c == '<' || c == '>' || c == '\'' || c == '\"' || c == '&';
        }
    }
}
