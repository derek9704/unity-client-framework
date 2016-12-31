using UnityEngine;

public enum SceneType
{
	battle, battle2, loading, login, town, mine
}

public enum GemType
{
	blueGem, greenGem, redGem, yellowGem
}

public enum HeroAction
{
	attack, hurt, die, idle, run, win, skill_attack, skill_cast
}

public enum BattleType
{
	mine, arena
}


public class Consts {

	public const int SCREEN_WIDTH = 960;

	public const int SCREEN_HEIGHT = 600;

	public static string STREAMINGASSETS_ADDR =
	#if UNITY_STANDALONE_WIN || UNITY_EDITOR
	"file://" + Application.dataPath + "/StreamingAssets/";
	#elif UNITY_IPHONE
	Application.dataPath + "/Raw/";
	#elif UNITY_ANDROID
	"jar:file://" + Application.dataPath + "!/assets/";
	#else
	string.Empty;
	#endif

	public static string REQUEST_ADDR = "";

	public static string LOGIN_ADDR = "";

	public static bool OPEN_HEARTBEAT =
	#if UNITY_EDITOR
	false;
	#else
	true;
	#endif

}
