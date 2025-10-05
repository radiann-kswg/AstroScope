using System.Text;
using UnityEngine;
using TMPro;

public class AstroScopeLogView : MonoBehaviour
{
	[SerializeField] private TMP_Text target;
	private readonly StringBuilder buffer = new();

	private void OnEnable()
	{
		Application.logMessageReceived += HandleLog;
	}

	private void OnDisable()
	{
		Application.logMessageReceived -= HandleLog;
	}

	private void HandleLog(string condition, string stackTrace, LogType type)
	{
		if (type == LogType.Log && condition.StartsWith("[AstroScope]"))
		{
			buffer.AppendLine(condition);
			if (target != null)
			{
				target.text = buffer.ToString();
			}
		}
	}
}