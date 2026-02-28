# HierarchyDiff

**English** | [简体中文](README.zh-cn.md)

A WPF-based tree-structured data difference viewing tool.

![HierarchyDiff](Documents/HierarchyDiff.png)

## Usage

Use the following command to compare two files:

```Powershell
HierarchyDiff.exe <FilePath1> <FilePath2>
```

Therefore, it can also be configured in version control tools (such as TortoiseGit) to provide customized Diff functionality for `*.xml` format files:

![TortoiseGit](Documents/TortoiseGit.png)

## Extension

This project provides the `FileFormat` type as the base class for "file types". By inheriting from this class, you can describe more types of tree-structured data files. The specific steps are as follows:

* Reference the `HierarchyDiff.FileFormat.Xml` project to create a new Class Library project for the new file type.
* Inherit from `FileFormat` and implement its pure virtual functions.

**Load File**

```C#
public abstract object? Load(string filePath);
```

Load a file and return an object that can describe the file. Subsequent interfaces will use this object to retrieve tree-structured data information.

**Save File**

```C#
public abstract void Save(object document, string filePath);
```

Save a file, used for persisting edits to the document.

**Get Root Node**

```C#
protected abstract object GetRoot(object document);
```

Get the root node. The parameter is the object returned by the Load method that describes the file.

**Get Child Nodes of a Given Node**

```C#
protected abstract IEnumerable<object> GetChildren(object node);
```

Get the child nodes of a given node. It is particularly important to note: for example, in XML there are two concepts: `elements` and `attributes`. This tool does not distinguish between them. In other words, this function also needs to enumerate XML `attributes`, where `attributes` are treated as leaf nodes without children.

**Compare Two Nodes**

```C#
public abstract float Compare(object a, object b);
```

The input parameters are two nodes. The return value is a floating-point number used to describe the degree of match between them. Typically, the return value should be in the range [0, 1]. Taking the XML file type as an example:

```C#
public override float Compare(object a, object b)
{
	if (a == null || b == null) return a == b ? 1 : 0;
	if (a.GetType() != b.GetType()) { return 0; }
	switch (a)
	{
		case XmlElement:
		{
			var element0 = (XmlElement)a;
			var element1 = (XmlElement)b;
			return
				element0.Name == element1.Name
				? 1
				: 0;
		}
		case XmlAttribute:
		{
			var attribute0 = (XmlAttribute)a;
			var attribute1 = (XmlAttribute)b;
			if (attribute0.Name != attribute1.Name) return 0.1f;
			if (attribute0.Value != attribute1.Value) return 0.5f;
			return 1;
		}
		case XmlDocument:
		{
			return 1;
		}
		default:
			throw new NotSupportedException();
	}
}
```

A return value of 0 indicates that the two nodes do not match at all, while 1 indicates a perfect match. For example, in the code above, if the node types are inconsistent, it directly returns 0. When both types are `XmlElement` and the element names match, it returns 1; otherwise, it returns 0. If both types are `XmlAttribute` and the attribute names do not match, it returns 0.1, indicating that when `Attribute` names are inconsistent, they may be recognized as "modified" rather than deleting the original `Attribute` and adding a new one, thereby achieving a certain degree of fuzzy matching. The specific return values can be adjusted according to actual circumstances.

**Convert Node to String**

```C#
public abstract string ToString(object? node);
```

Mostly used for debugging.

**Get Node Name**

```C#
public abstract string GetName(object? node);
```

**Get Node Value**

```C#
public abstract string? GetValue(object? node);
```
Get the value of a node. This project currently does not distinguish between value types; all values are treated as strings.

**Set Node Value**

```C#
public abstract bool SetValue(object? node, string value);
```

**Get Node Display Style**

```C#
public abstract TreeNodeStyle GetStyle(object? node, TreeNodeStyle? style);
```

Allows customization of node display styles (including font, font size, line height, and other properties). The input parameter is the default style; if customization is needed, you can create a new `TreeNodeStyle` object, modify the necessary property data, and return it.

## TODO
- [ ] Addition, deletion, and modification of nodes and attributes
- [ ] Merge functionality