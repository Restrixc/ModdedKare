namespace RichTextSubstringHelper;

public static class StringExt
{
	public static string RichTextSubString(this string text, int length, bool withTags = true)
	{
		RichTextSubStringMaker j = new RichTextSubStringMaker(text);
		for (int i = 0; i < length; i++)
		{
			j.Consume();
		}
		if (withTags)
		{
			return j.GetRichText();
		}
		return j.GetText();
	}

	public static int RichTextLength(this string text)
	{
		RichTextSubStringMaker i = new RichTextSubStringMaker(text);
		int length = 0;
		while (i.IsConsumable())
		{
			if (i.Consume())
			{
				length++;
			}
		}
		return length;
	}
}
