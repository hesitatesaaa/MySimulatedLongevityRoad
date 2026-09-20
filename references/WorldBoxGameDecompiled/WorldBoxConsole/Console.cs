using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace WorldBoxConsole;

public class Console : MonoBehaviour
{
	private const int MAX_ELEMENTS = 2500;

	private const int MAX_LINES = 25000;

	private const int MAX_CHARS_PER_LINE = 256;

	private static int _line_num = 0;

	private static int _warning_num = 0;

	private static int _error_num = 0;

	private Text _prefab;

	private static Queue<string> _texts = new Queue<string>(2500);

	private static StringBuilder _log = new StringBuilder();

	private static int _error_repeated = 0;

	private static int _warning_repeated = 0;

	private static Dictionary<string, int> _previous_errors = new Dictionary<string, int>();

	private static Dictionary<string, int> _previous_warnings = new Dictionary<string, int>();

	private static HashSet<string> _stacks = new HashSet<string>();

	private RectTransform _text_group;

	private List<Text> _text_obj = new List<Text>();

	private ObjectPoolGenericMono<Text> _pool_texts;

	private static ConcurrentQueue<LogItem> log_queue = new ConcurrentQueue<LogItem>();

	private void Awake()
	{
		Transform obj = ((Component)this).transform.Find("Scroll View/Viewport/Content");
		_text_group = (RectTransform)(object)((obj is RectTransform) ? obj : null);
		_prefab = ((Component)((Transform)_text_group).Find("CText")).GetComponent<Text>();
		_prefab.text = "";
		_pool_texts = new ObjectPoolGenericMono<Text>(_prefab, (Transform)(object)_text_group);
		((Component)_prefab).gameObject.SetActive(false);
	}

	private void addText()
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		Text next = _pool_texts.getNext();
		((Object)next).name = "CText " + (_text_obj.Count + 1);
		RectTransform component = ((Component)next).GetComponent<RectTransform>();
		((Transform)component).localScale = Vector3.one;
		((Transform)component).localPosition = Vector3.zero;
		_text_obj.Add(next);
		truncateGameObjects();
	}

	private void truncateGameObjects()
	{
		while (_text_obj.Count > 2500)
		{
			_text_obj[0].text = "";
			_pool_texts.release(_text_obj[0]);
			_text_obj.RemoveAt(0);
		}
	}

	internal static bool hasErrors()
	{
		return _error_num > 0;
	}

	private static void truncateTexts()
	{
		if (_texts.Count > 2500)
		{
			while (_texts.Count > 2500)
			{
				_texts.Dequeue();
			}
			_line_num = 0;
		}
	}

	private void OnEnable()
	{
		if (Config.game_loaded)
		{
			_line_num = 0;
			_text_group.SetBottom(0f);
		}
	}

	private void OnDisable()
	{
		_line_num = 0;
		foreach (Text item in _text_obj)
		{
			item.text = "";
		}
		_pool_texts.clear();
		_text_obj.Clear();
	}

	public void Toggle()
	{
		if (((Component)this).gameObject.activeSelf)
		{
			((Component)this).gameObject.SetActive(false);
		}
		else
		{
			((Component)this).gameObject.SetActive(true);
		}
	}

	public void Hide()
	{
		((Component)this).gameObject.SetActive(false);
	}

	public void Show()
	{
		((Component)this).gameObject.SetActive(true);
	}

	public bool isActive()
	{
		return ((Component)this).gameObject.activeSelf;
	}

	public static void HandleLog(string pLogString, string pStackTrace, LogType pLogType)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		if (ThreadHelper.isMainThread())
		{
			ProcessLog(pLogString, pStackTrace, pLogType, DateTime.Now);
		}
		else
		{
			log_queue.Enqueue(new LogItem(pLogString, pStackTrace, pLogType));
		}
	}

	public static void ProcessLog(string pLogString, string pStackTrace, LogType pLogType, DateTime pTime)
	{
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Expected I4, but got Unknown
		if (pLogString.Contains("FIRAPP_DEFAULT"))
		{
			return;
		}
		if (pLogString.Length > 256 && !pLogString.Contains("</"))
		{
			string[] array = pLogString.Split('\n');
			using StringBuilderPool stringBuilderPool = new StringBuilderPool();
			for (int i = 0; i < array.Length; i++)
			{
				while (array[i].Length > 256)
				{
					stringBuilderPool.Append(array[i].Substring(0, 256));
					stringBuilderPool.Append('\n');
					array[i] = array[i].Substring(256);
				}
				stringBuilderPool.Append(array[i]);
				stringBuilderPool.Append('\n');
			}
			pLogString = stringBuilderPool.ToString();
		}
		pLogString = pLogString.Trim(' ', '\n');
		pLogString = ConsoleFormatter.logFormatter(pLogString);
		switch ((int)pLogType)
		{
		case 0:
		case 1:
		case 4:
			if (_error_num == 0)
			{
				_texts.Enqueue(ConsoleFormatter.addSystemInfo().Trim('\n', ' '));
			}
			if (_previous_errors.ContainsKey(pLogString))
			{
				_previous_errors[pLogString]++;
				_error_repeated++;
				_log.Clear();
				return;
			}
			clearRepeat();
			if (!_stacks.Add(pStackTrace))
			{
				pStackTrace = "";
			}
			_log.Append(ConsoleFormatter.logError(_error_num, pLogString, pStackTrace));
			_previous_errors.Add(pLogString, 1);
			_error_num++;
			break;
		case 2:
			if (_previous_warnings.ContainsKey(pLogString))
			{
				_previous_warnings[pLogString]++;
				_warning_repeated++;
				_log.Clear();
				return;
			}
			clearRepeat();
			_log.Append(ConsoleFormatter.logWarning(_warning_num, pLogString));
			_previous_warnings.Add(pLogString, 1);
			_warning_num++;
			break;
		default:
			clearRepeat();
			_log.Append(pLogString);
			break;
		}
		PrependTime(_log, pTime);
		_texts.Enqueue(_log.ToString().Trim('\n', ' '));
		_log.Clear();
		truncateTexts();
	}

	private static void clearRepeat()
	{
		if (_error_repeated > 0)
		{
			_texts.Enqueue("<color=red>( previous errors repeated " + _error_repeated + " times )</color>");
			if (_error_repeated > 10)
			{
				_texts.Enqueue("<color=red>YOU SHOULD RESTART THE GAME</color>");
			}
			_error_repeated = 0;
		}
		if (_warning_repeated > 0)
		{
			_texts.Enqueue("<color=yellow>( previous warning repeated " + _warning_repeated + " times )</color>");
			_warning_repeated = 0;
		}
	}

	private void Update()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		LogItem result;
		while (log_queue.TryDequeue(out result))
		{
			ProcessLog(result.log, result.stack_trace, result.type, result.time);
		}
		if (_line_num == _texts.Count + _error_repeated + _warning_repeated)
		{
			return;
		}
		string text = string.Join('\n', _texts).Trim('\n', ' ');
		if (_error_repeated > 0)
		{
			text = text + "\n<color=red>( previous errors repeated " + _error_repeated + " times )</color>";
			if (_error_repeated > 10)
			{
				text += "\n<color=red>YOU SHOULD RESTART THE GAME</color>";
			}
		}
		else if (_warning_repeated > 0)
		{
			text = text + "\n<color=yellow>( previous warning repeated " + _warning_repeated + " times )</color>";
		}
		string[] array = text.Split('\n');
		if (array.Length > 25000)
		{
			string[] array2 = new string[25000];
			Array.Copy(array, array.Length - 25000, array2, 0, 25000);
			array = array2;
		}
		int num = -1;
		for (int i = 0; i < array.Length; i++)
		{
			int num2 = Mathf.CeilToInt((float)(i + 1) / 10f) - 1;
			for (int j = _text_obj.Count; j < num2 + 1; j++)
			{
				addText();
			}
			Text val = _text_obj[num2];
			if (num2 != num)
			{
				val.text = "";
				num = num2;
			}
			val.text = val.text + "\n" + array[i].Trim('\n', ' ');
			val.text = val.text.Trim('\n', ' ');
		}
		_line_num = _texts.Count + _error_repeated + _warning_repeated;
	}

	public static void PrependTime(StringBuilder pStringBuilder, DateTime pDateTime)
	{
		using StringBuilderPool stringBuilderPool = new StringBuilderPool();
		stringBuilderPool.Append("[").Append("<color=white>").Append((pDateTime.Hour < 10) ? "0" : "")
			.Append(pDateTime.Hour)
			.Append("</color>")
			.Append(':')
			.Append("<color=white>")
			.Append((pDateTime.Minute < 10) ? "0" : "")
			.Append(pDateTime.Minute)
			.Append("</color>")
			.Append(':')
			.Append("<color=white>")
			.Append((pDateTime.Second < 10) ? "0" : "")
			.Append(pDateTime.Second)
			.Append("</color>")
			.Append("] ");
		pStringBuilder.Insert(0, stringBuilderPool.string_builder);
	}

	public void openLogsFolder()
	{
		Application.OpenURL("file://" + Application.persistentDataPath);
	}
}
