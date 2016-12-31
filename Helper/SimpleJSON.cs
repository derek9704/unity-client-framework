/* * * * *
 * A simple JSON Parser / builder
 * ------------------------------
 * 
 * It mainly has been written as a simple JSON parser. It can build a JSON string
 * from the node-tree, or generate a node tree from any valid JSON string.
 * 
 * Features / attributes:
 * - provides strongly typed node classes and lists / dictionaries
 * - provides easy access to class members / array items / data values
 * - the parser ignores data types. Each value is a string.
 * - only double quotes (") are used for quoting strings.
 * - values and names are not restricted to quoted strings. They simply add up and are trimmed.
 * - There are only 3 types: arrays(JsonArray), objects(JsonClass) and values(JsonData)
 * - provides "casting" properties to easily convert to / from those types:
 *   int / float / double / bool
 * - provides a common interface for each node so no explicit casting is required.
 * - the parser try to avoid errors, but if malformed JSON is parsed the result is undefined
 * - Added internal JsonLazyCreator class which simplifies the construction of a JSON tree
 *   Now you can simple reference any item that doesn't exist yet and it will return a JsonLazyCreator
 *   The class determines the required type by it's further use, creates the type and removes itself.
 * 
 * * * * */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class JsonNode
{
    #region common interface
	public virtual void Add (string aKey, JsonNode aItem)
	{
	}

	public virtual JsonNode this [int aIndex] { get { return null; } set { }
	}

	public virtual JsonNode this [string aKey] { get { return null; } set { }
	}

	public virtual string Value {
		get { return "";   }
		set { }
	}

	public virtual ICollection<string> Keys {
		get { return null; }
	}

	public virtual int Count                   { get { return 0; } }
 
	public virtual void Add (JsonNode aItem)
	{
		Add ("", aItem);
	}
 
	public virtual JsonNode Remove (string aKey)
	{
		return null;
	}

	public virtual JsonNode Remove (int aIndex)
	{
		return null;
	}

	public virtual JsonNode Remove (JsonNode aNode)
	{
		return aNode;
	}

	public virtual IEnumerator GetEnumerator ()
	{
		yield break;
	}
 
	public virtual string ToJson ()
	{
		return "JsonNode";
	}

    #endregion common interface

    #region operators
	public static implicit operator JsonNode (string s)
	{
		return new JsonData (s);
	}

	public static implicit operator JsonNode (int s)
	{
		return new JsonData (s.ToString());
	}

	public static implicit operator JsonNode (float s)
	{
		return new JsonData (s.ToString());
	}

	public static implicit operator JsonNode (bool s)
	{
		return new JsonData (s.ToString());
	}

	public static implicit operator string (JsonNode d)
	{
		return (d == null) ? null : d.Value;
	}

	public static implicit operator int (JsonNode d)
	{
		int v = 0;
		if (int.TryParse (d.Value, out v))
			return v;
		return 0;
	}

	public static implicit operator float (JsonNode d)
	{
		float v = 0.0f;
		if (float.TryParse (d.Value, out v))
			return v;
		return 0.0f;
	}

	public static implicit operator double (JsonNode d)
	{
		double v = 0.0;
		if (double.TryParse (d.Value, out v))
			return v;
		return 0.0;
	}

	public static implicit operator bool (JsonNode d)
	{
		bool v = false;
		if (d.Value == "null") return v;
		if (bool.TryParse (d.Value, out v))
			return v;
		return !(d is JsonLazyCreator);
	}

	public static bool operator ==(JsonNode a, object b)
	{
		if (b == null && a is JsonLazyCreator)
			return true;
		if(b is string || b is float || b is int || b is double) {
			return a.Value == b.ToString();
		}
		else return System.Object.ReferenceEquals(a,b);
	}
	
	public static bool operator !=(JsonNode a, object b)
	{
		return !(a == b);
	}

	public override bool Equals (object obj)
	{
		return System.Object.ReferenceEquals (this, obj);
	}

	public override int GetHashCode ()
	{
		return base.GetHashCode ();
	}
        #endregion operators
 
	internal static string Escape (string aText)
	{
		string result = "";
		foreach (char c in aText) {
			switch (c) {
			case '\\':
				result += "\\\\";
				break;
			case '\"':
				result += "\\\"";
				break;
			case '\n':
				result += "\\n";
				break;
			case '\r':
				result += "\\r";
				break;
			case '\t':
				result += "\\t";
				break;
			case '\b':
				result += "\\b";
				break;
			case '\f':
				result += "\\f";
				break;
			default   :
				result += c;
				break;
			}
		}
		return result;
	}

} // End of JsonNode

public class JsonArray : JsonNode, IEnumerable
{
	private List<JsonNode> m_List = new List<JsonNode> ();

	public override JsonNode this [int aIndex] {
		get {
			if (aIndex < 0 || aIndex >= m_List.Count)
				return new JsonLazyCreator (this);
			return m_List [aIndex];
		}
		set {
			if (aIndex < 0 || aIndex >= m_List.Count)
				m_List.Add (value);
			else
				m_List [aIndex] = value;
		}
	}

	public override JsonNode this [string aKey] {
		get{ return new JsonLazyCreator (this);}
		set{ m_List.Add (value); }
	}

	public override int Count {
		get { return m_List.Count; }
	}

	public override void Add (string aKey, JsonNode aItem)
	{
		m_List.Add (aItem);
	}

	public override JsonNode Remove (int aIndex)
	{
		if (aIndex < 0 || aIndex >= m_List.Count)
			return null;
		JsonNode tmp = m_List [aIndex];
		m_List.RemoveAt (aIndex);
		return tmp;
	}

	public override JsonNode Remove (JsonNode aNode)
	{
		m_List.Remove (aNode);
		return aNode;
	}

	public override IEnumerator GetEnumerator ()
	{
		foreach (JsonNode N in m_List)
			yield return N;
	}

	public override string ToJson ()
	{
		string result = "[ ";
		foreach (JsonNode N in m_List) {
			if (result.Length > 2)
				result += ", ";
			result += N.ToJson ();
		}
		result += " ]";
		return result;
	}

} // End of JsonArray

public class JsonClass : JsonNode, IEnumerable
{
	private Dictionary<string,JsonNode> m_Dict = new Dictionary<string,JsonNode> ();

	public override JsonNode this [string aKey] {
		get {
			if (m_Dict.ContainsKey (aKey))
				return m_Dict [aKey];
			else
				return new JsonLazyCreator (this, aKey);
		}
		set {
			if (m_Dict.ContainsKey (aKey))
				m_Dict [aKey] = value;
			else
				m_Dict.Add (aKey, value);
		}
	}

	public override JsonNode this [int aIndex] {
		get {
			if (aIndex < 0 || aIndex >= m_Dict.Count)
				return null;
			return m_Dict.ElementAt (aIndex).Value;
		}
		set {
			if (aIndex < 0 || aIndex >= m_Dict.Count)
				return;
			string key = m_Dict.ElementAt (aIndex).Key;
			m_Dict [key] = value;
		}
	}

	public override int Count {
		get { return m_Dict.Count; }
	}

	public override ICollection<string> Keys {
		get { return m_Dict.Keys; }
	}
 
	public override void Add (string aKey, JsonNode aItem)
	{
		if (!string.IsNullOrEmpty (aKey)) {
			if (m_Dict.ContainsKey (aKey))
				m_Dict [aKey] = aItem;
			else
				m_Dict.Add (aKey, aItem);
		} else
			m_Dict.Add (Guid.NewGuid ().ToString (), aItem);
	}
 
	public override JsonNode Remove (string aKey)
	{
		if (!m_Dict.ContainsKey (aKey))
			return null;
		JsonNode tmp = m_Dict [aKey];
		m_Dict.Remove (aKey);
		return tmp;        
	}

	public override JsonNode Remove (int aIndex)
	{
		if (aIndex < 0 || aIndex >= m_Dict.Count)
			return null;
		var item = m_Dict.ElementAt (aIndex);
		m_Dict.Remove (item.Key);
		return item.Value;
	}

	public override JsonNode Remove (JsonNode aNode)
	{
		try {
			var item = m_Dict.Where (k => k.Value == aNode).First ();
			m_Dict.Remove (item.Key);
			return aNode;
		} catch {
			return null;
		}
	}
 
	public override IEnumerator GetEnumerator ()
	{
		foreach (KeyValuePair<string, JsonNode> N in m_Dict)
			yield return N;
	}

	public override string ToJson ()
	{
		string result = "{";
		foreach (KeyValuePair<string, JsonNode> N in m_Dict) {
			if (result.Length > 2)
				result += ", ";
			result += "\"" + Escape (N.Key) + "\":" + N.Value.ToJson ();
		}
		result += "}";
		return result;
	}

} // End of JsonClass
 
public class JsonData : JsonNode
{
	private string m_Data;

	public override string Value {
		get { return m_Data; }
		set { m_Data = value; }
	}

	public JsonData (string aData)
	{
		m_Data = aData;
	}

	public override string ToJson ()
	{
		return "\"" + Escape (m_Data) + "\"";
	}
  
} // End of JsonData

internal class JsonLazyCreator : JsonNode
{
	private JsonNode m_Node = null;
	private string m_Key = null;
	
	public JsonLazyCreator (JsonNode aNode)
	{
		m_Node = aNode;
		m_Key = null;
	}

	public JsonLazyCreator (JsonNode aNode, string aKey)
	{
		m_Node = aNode;
		m_Key = aKey;
	}
	
	private void Set (JsonNode aVal)
	{
		if (m_Key == null) {
			m_Node.Add (aVal);
		} else {
			m_Node.Add (m_Key, aVal);
		}
		m_Node = null; // Be GC friendly.
	}
	
	public override JsonNode this [int aIndex] {
		get {
			return new JsonLazyCreator (this);
		}
		set {
			var tmp = new JsonArray ();
			tmp.Add (value);
			Set (tmp);
		}
	}
	
	public override JsonNode this [string aKey] {
		get {
			return new JsonLazyCreator (this, aKey);
		}
		set {
			var tmp = new JsonClass ();
			tmp.Add (aKey, value);
			Set (tmp);
		}
	}

	public override void Add (JsonNode aItem)
	{
		var tmp = new JsonArray ();
		tmp.Add (aItem);
		Set (tmp);
	}

	public override void Add (string aKey, JsonNode aItem)
	{
		var tmp = new JsonClass ();
		tmp.Add (aKey, aItem);
		Set (tmp);
	}

	public static bool operator ==(JsonLazyCreator a, object b)
	{
		if (b == null)
			return true;
		return System.Object.ReferenceEquals(a,b);
	}
	
	public static bool operator !=(JsonLazyCreator a, object b)
	{
		return !(a == b);
	}

	public override bool Equals (object obj)
	{
		if (obj == null)
			return true;
		return System.Object.ReferenceEquals (this, obj);
	}

	public override int GetHashCode ()
	{
		return base.GetHashCode ();
	}
	
	public override string ToJson ()
	{
		return "";
	}
	
} // End of JsonLazyCreator
 
public static class Json
{
	public static JsonNode Parse (IList<string> obj)
	{
		JsonArray ctx = new JsonArray();
		foreach (string elem in obj)
			ctx.Add(elem);
		return ctx;
	}

	public static JsonNode Parse (string aJSON)
	{
		Stack<JsonNode> stack = new Stack<JsonNode> ();
		JsonNode ctx = null;
		int i = 0;
		string Token = "";
		string TokenName = "";
		bool QuoteMode = false;
		while (i < aJSON.Length) {
			switch (aJSON [i]) {
			case '{':
				if (QuoteMode) {
					Token += aJSON [i];
					break;
				}
				stack.Push (new JsonClass ());
				if (ctx != null) {
					TokenName = TokenName.Trim ();
					if (ctx is JsonArray)
						ctx.Add (stack.Peek ());
					else if (TokenName != "")
						ctx.Add (TokenName, stack.Peek ());
				}
				TokenName = "";
				Token = "";
				ctx = stack.Peek ();
				break;
					
			case '[':
				if (QuoteMode) {
					Token += aJSON [i];
					break;
				}
					
				stack.Push (new JsonArray ());
				if (ctx != null) {
					TokenName = TokenName.Trim ();
					if (ctx is JsonArray)
						ctx.Add (stack.Peek ());
					else if (TokenName != "")
						ctx.Add (TokenName, stack.Peek ());
				}
				TokenName = "";
				Token = "";
				ctx = stack.Peek ();
				break;
					
			case '}':
			case ']':
				if (QuoteMode) {
					Token += aJSON [i];
					break;
				}
				if (stack.Count == 0)
					throw new Exception ("JSON Parse: Too many closing brackets");
					
				stack.Pop ();
				if (Token != "") {
					TokenName = TokenName.Trim ();
					if (ctx is JsonArray)
						ctx.Add (Token);
					else if (TokenName != "")
						ctx.Add (TokenName, Token);
				}
				TokenName = "";
				Token = "";
				if (stack.Count > 0)
					ctx = stack.Peek ();
				break;
					
			case ':':
				if (QuoteMode) {
					Token += aJSON [i];
					break;
				}
				TokenName = Token;
				Token = "";
				break;
					
			case '"':
				QuoteMode ^= true;
				break;
					
			case ',':
				if (QuoteMode) {
					Token += aJSON [i];
					break;
				}
				if (Token != "") {
					if (ctx is JsonArray)
						ctx.Add (Token);
					else if (TokenName != "")
						ctx.Add (TokenName, Token);
				}
				TokenName = "";
				Token = "";
				break;
					
			case '\r':
			case '\n':
				break;
					
			case ' ':
			case '\t':
				if (QuoteMode)
					Token += aJSON [i];
				break;
					
			case '\\':
				++i;
				if (QuoteMode) {
					char C = aJSON [i];
					switch (C) {
					case 't':
						Token += '\t';
						break;
					case 'r':
						Token += '\r';
						break;
					case 'n':
						Token += '\n';
						break;
					case 'b':
						Token += '\b';
						break;
					case 'f':
						Token += '\f';
						break;
					case 'u':
						{
							string s = aJSON.Substring (i + 1, 4);
							Token += (char)int.Parse (s, System.Globalization.NumberStyles.AllowHexSpecifier);
							i += 4;
							break;
						}
					default  :
						Token += C;
						break;
					}
				}
				break;
					
			default:
				Token += aJSON [i];
				break;
			}
			++i;
		}
		if (QuoteMode) {
			throw new Exception ("JSON Parse: Quotation marks seems to be messed up.");
		}
		return ctx;
	}


}
