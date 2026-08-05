using System;
using System.Text;
using UnityEditor;

namespace DreadScripts.ControllerEditor;

internal struct BatchOperationContext
{
	private StringBuilder errorLog;

	internal int currentStep;

	internal int totalSteps;

	internal string m_MethodServer;

	private string _SchemaServer;

	private string title;

	private string info;

	private bool continueOnError;

	internal bool progressBarActive;

	internal bool hasError;

	internal static object InstantiateSystem;

	internal BatchOperationContext Run(Action asset)
	{
		if (errorLog == null)
		{
			errorLog = new StringBuilder();
		}
		try
		{
			asset();
		}
		catch (Exception ex)
		{
			hasError = true;
			string text = _SchemaServer + " - " + title + " - " + info + "\n" + ex.Message;
			if (!string.IsNullOrEmpty(m_MethodServer))
			{
				text = m_MethodServer + " - " + text;
			}
			errorLog.AppendLine("Error occured at step:\n" + text + "\n\n");
			if (!continueOnError)
			{
				if (EditorUtility.DisplayDialog("Uh oh", $"Something went wrong!\n\n{errorLog}Press Copy and send it to whoever is responsible for this.", "Copy", "Heck"))
				{
					EditorGUIUtility.systemCopyBuffer = errorLog.ToString();
				}
				throw;
			}
		}
		finally
		{
			if (progressBarActive)
			{
				EditorUtility.ClearProgressBar();
			}
		}
		return this;
	}

	internal BatchOperationContext SetTitle(string asset)
	{
		title = asset;
		return this;
	}

	internal BatchOperationContext SetInfo(string item)
	{
		info = item;
		return this;
	}

	internal BatchOperationContext MoveContext(string setup)
	{
		_SchemaServer = setup;
		return this;
	}

	internal BatchOperationContext NextStep()
	{
		currentStep++;
		return this;
	}

	internal BatchOperationContext ShowProgressBar()
	{
		progressBarActive = true;
		EditorUtility.DisplayProgressBar(title, $"{info} ({currentStep}/{totalSteps})", (float)currentStep / (float)totalSteps);
		return this;
	}

	internal BatchOperationContext Reset()
	{
		title = (info = (_SchemaServer = string.Empty));
		currentStep = 0;
		errorLog.Clear();
		hasError = false;
		while (progressBarActive)
		{
			EditorUtility.ClearProgressBar();
		}
		return this;
	}

	internal BatchOperationContext SetContinueOnError(bool evaluateinstance)
	{
		continueOnError = evaluateinstance;
		return this;
	}

	internal static bool RevertSystem()
	{
		return InstantiateSystem == null;
	}
}
