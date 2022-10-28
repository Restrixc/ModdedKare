#define UNITY_ASSERTIONS
using System.Collections.Generic;
using UnityEngine;

namespace RichTextSubstringHelper;

public class RichTextSubStringMaker
{
	private string originalText;

	private string middleText;

	private string textWithoutTags;

	private Stack<RichTextTag> tagStack;

	private int consumedLength;

	private static readonly char[] tagBrackets = new char[2] { '<', '>' };

	public RichTextSubStringMaker(string original)
	{
		originalText = original;
		middleText = "";
		tagStack = new Stack<RichTextTag>();
		consumedLength = 0;
	}

	public string GetText()
	{
		return textWithoutTags;
	}

	public string GetRichText()
	{
		if (tagStack.Count == 0)
		{
			return middleText;
		}
		string ret = middleText;
		Queue<RichTextTag> copiedQueue = new Queue<RichTextTag>(tagStack);
		while (copiedQueue.Count != 0)
		{
			ret += copiedQueue.Dequeue().endTag;
		}
		return ret;
	}

	public bool IsConsumable()
	{
		return consumedLength < originalText.Length;
	}

	public bool Consume()
	{
		Debug.Assert(IsConsumable());
		char peekedOriginChar = PeekNextOriginChar();
		bool isStartTag = peekedOriginChar == '<' && IsNextTagBracketClosing(consumedLength + 1);
		bool isEndTag = peekedOriginChar == '<' && PeekNextNextOriginChar() == '/';
		if (isStartTag || isEndTag)
		{
			if (isEndTag)
			{
				ConsumeEndTag();
			}
			else if (isStartTag)
			{
				ConsumeStartTag();
			}
			if (IsConsumable())
			{
				return Consume();
			}
			return false;
		}
		textWithoutTags += peekedOriginChar;
		ConsumeRawChar();
		return true;
	}

	private char? PeekNextNextOriginChar()
	{
		if (originalText.Length <= consumedLength + 1)
		{
			return null;
		}
		return originalText[consumedLength + 1];
	}

	private char PeekNextOriginChar()
	{
		return originalText[consumedLength];
	}

	private bool IsNextTagBracketClosing(int charIndexToStartSearch = 0)
	{
		int bracketIndex = originalText.IndexOfAny(tagBrackets, charIndexToStartSearch);
		return bracketIndex != -1 && originalText[bracketIndex] == '>';
	}

	private void ConsumeStartTag()
	{
		Debug.Assert(PeekNextOriginChar() == '<');
		string tagName = "";
		bool tagNameComplete = false;
		ConsumeRawChar();
		while (true)
		{
			char? consumedChar = ConsumeRawChar();
			if (!consumedChar.HasValue)
			{
				Debug.LogError("Cannot close start tag");
				return;
			}
			if (consumedChar == '>')
			{
				break;
			}
			if (!tagNameComplete)
			{
				if (!char.IsLetterOrDigit(consumedChar.Value))
				{
					tagNameComplete = true;
					continue;
				}
				string text = tagName;
				char? c = consumedChar;
				tagName = text + c;
			}
		}
		if (tagName == "")
		{
			Debug.LogWarning("Empty tag name");
		}
		tagStack.Push(new RichTextTag
		{
			tagName = tagName
		});
	}

	private void ConsumeEndTag()
	{
		Debug.Assert(PeekNextOriginChar() == '<');
		Debug.Assert(PeekNextNextOriginChar() == '/');
		string tagName = "";
		bool tagNameComplete = false;
		ConsumeRawChar();
		ConsumeRawChar();
		while (true)
		{
			char? consumedChar = ConsumeRawChar();
			if (!consumedChar.HasValue)
			{
				Debug.LogError("Cannot close start tag");
				return;
			}
			if (consumedChar == '>')
			{
				break;
			}
			if (!tagNameComplete)
			{
				if (!char.IsLetterOrDigit(consumedChar.Value))
				{
					tagNameComplete = true;
					continue;
				}
				string text = tagName;
				char? c = consumedChar;
				tagName = text + c;
			}
		}
		if (tagName == "")
		{
			Debug.LogWarning("Empty tag name");
		}
		if (tagStack.Count == 0)
		{
			Debug.LogError("Could not pop tag " + tagName);
		}
		if (tagStack.Peek().tagName != tagName)
		{
			Debug.LogError("Could not pop tag " + tagName + " expeted " + tagStack.Peek().tagName);
		}
		tagStack.Pop();
	}

	private char? ConsumeRawChar()
	{
		if (consumedLength > originalText.Length)
		{
			return null;
		}
		char peeked = PeekNextOriginChar();
		middleText += peeked;
		consumedLength++;
		return peeked;
	}
}
