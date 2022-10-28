using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleJSON;

public class JSONObject : JSONNode
{
	private Dictionary<string, JSONNode> m_Dict = new Dictionary<string, JSONNode>();

	private bool inline = false;

	public override bool Inline
	{
		get
		{
			return inline;
		}
		set
		{
			inline = value;
		}
	}

	public override JSONNodeType Tag => JSONNodeType.Object;

	public override bool IsObject => true;

	public override JSONNode this[string aKey]
	{
		get
		{
			if (m_Dict.ContainsKey(aKey))
			{
				return m_Dict[aKey];
			}
			return new JSONLazyCreator(this, aKey);
		}
		set
		{
			if (value == null)
			{
				value = JSONNull.CreateOrGet();
			}
			if (m_Dict.ContainsKey(aKey))
			{
				m_Dict[aKey] = value;
			}
			else
			{
				m_Dict.Add(aKey, value);
			}
		}
	}

	public override JSONNode this[int aIndex]
	{
		get
		{
			if (aIndex < 0 || aIndex >= m_Dict.Count)
			{
				return null;
			}
			return m_Dict.ElementAt(aIndex).Value;
		}
		set
		{
			if (value == null)
			{
				value = JSONNull.CreateOrGet();
			}
			if (aIndex >= 0 && aIndex < m_Dict.Count)
			{
				string key = m_Dict.ElementAt(aIndex).Key;
				m_Dict[key] = value;
			}
		}
	}

	public override int Count => m_Dict.Count;

	public override IEnumerable<JSONNode> Children
	{
		get
		{
			foreach (KeyValuePair<string, JSONNode> item in m_Dict)
			{
				yield return item.Value;
			}
		}
	}

	public override Enumerator GetEnumerator()
	{
		return new Enumerator(m_Dict.GetEnumerator());
	}

	public override void Add(string aKey, JSONNode aItem)
	{
		if (aItem == null)
		{
			aItem = JSONNull.CreateOrGet();
		}
		if (aKey != null)
		{
			if (m_Dict.ContainsKey(aKey))
			{
				m_Dict[aKey] = aItem;
			}
			else
			{
				m_Dict.Add(aKey, aItem);
			}
		}
		else
		{
			m_Dict.Add(Guid.NewGuid().ToString(), aItem);
		}
	}

	public override JSONNode Remove(string aKey)
	{
		if (!m_Dict.ContainsKey(aKey))
		{
			return null;
		}
		JSONNode tmp = m_Dict[aKey];
		m_Dict.Remove(aKey);
		return tmp;
	}

	public override JSONNode Remove(int aIndex)
	{
		if (aIndex < 0 || aIndex >= m_Dict.Count)
		{
			return null;
		}
		KeyValuePair<string, JSONNode> item = m_Dict.ElementAt(aIndex);
		m_Dict.Remove(item.Key);
		return item.Value;
	}

	public override JSONNode Remove(JSONNode aNode)
	{
		try
		{
			KeyValuePair<string, JSONNode> item = m_Dict.Where((KeyValuePair<string, JSONNode> k) => k.Value == aNode).First();
			m_Dict.Remove(item.Key);
			return aNode;
		}
		catch
		{
			return null;
		}
	}

	public override JSONNode Clone()
	{
		JSONObject node = new JSONObject();
		foreach (KeyValuePair<string, JSONNode> i in m_Dict)
		{
			node.Add(i.Key, i.Value.Clone());
		}
		return node;
	}

	public override bool HasKey(string aKey)
	{
		return m_Dict.ContainsKey(aKey);
	}

	public override JSONNode GetValueOrDefault(string aKey, JSONNode aDefault)
	{
		if (m_Dict.TryGetValue(aKey, out var res))
		{
			return res;
		}
		return aDefault;
	}

	internal override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, JSONTextMode aMode)
	{
		aSB.Append('{');
		bool first = true;
		if (inline)
		{
			aMode = JSONTextMode.Compact;
		}
		foreach (KeyValuePair<string, JSONNode> i in m_Dict)
		{
			if (!first)
			{
				aSB.Append(',');
			}
			first = false;
			if (aMode == JSONTextMode.Indent)
			{
				aSB.AppendLine();
			}
			if (aMode == JSONTextMode.Indent)
			{
				aSB.Append(' ', aIndent + aIndentInc);
			}
			aSB.Append('"').Append(JSONNode.Escape(i.Key)).Append('"');
			if (aMode == JSONTextMode.Compact)
			{
				aSB.Append(':');
			}
			else
			{
				aSB.Append(" : ");
			}
			i.Value.WriteToStringBuilder(aSB, aIndent + aIndentInc, aIndentInc, aMode);
		}
		if (aMode == JSONTextMode.Indent)
		{
			aSB.AppendLine().Append(' ', aIndent);
		}
		aSB.Append('}');
	}
}
