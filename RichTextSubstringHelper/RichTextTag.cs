namespace RichTextSubstringHelper;

internal class RichTextTag
{
	public string tagName;

	public string endTag => "</" + tagName + ">";
}
