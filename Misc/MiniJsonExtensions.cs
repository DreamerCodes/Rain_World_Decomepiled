using System.Collections.Generic;

public static class MiniJsonExtensions
{
	public static string toJson(this Dictionary<string, object> obj)
	{
		return Json.Serialize(obj);
	}

	public static List<object> listFromJson(this string json)
	{
		return (List<object>)Json.Deserialize(json);
	}

	public static Dictionary<string, object> dictionaryFromJson(this string json)
	{
		return (Dictionary<string, object>)Json.Deserialize(json);
	}
}
