using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace DreadScripts.ControllerEditor;

internal class ImporterThread
{
	internal readonly VRCPhysBone requestThread;

	internal readonly Transform printerThread;

	internal readonly List<EventThread> _WriterThread;

	internal readonly int paramsThread;

	internal List<List<EventThread>> _ListenerThread;

	private static ImporterThread ConcatStatus;

	[SpecialName]
	internal IEnumerable<Matrix4x4> PublishRecord()
	{
		return _WriterThread.Select((EventThread b) => b._CallbackThread);
	}

	internal ImporterThread(VRCPhysBone first)
	{
		requestThread = first;
		printerThread = first.GetRootTransform();
		_WriterThread = new List<EventThread>();
		SetupRecord(printerThread, 0);
		paramsThread = _WriterThread.Max((EventThread b) => b.m_PrototypeThread);
	}

	internal void SetupRecord(Transform info, int ord)
	{
		bool flag = false;
		EventThread eventThread = new EventThread();
		EventThread ruleThread = null;
		EventThread eventThread2 = null;
		Quaternion q = info.rotation;
		List<Transform> list = new List<Transform>();
		for (int i = 0; i < info.childCount; i++)
		{
			Transform child = info.GetChild(i);
			if (!requestThread.ignoreTransforms.Contains(child))
			{
				list.Add(child);
			}
		}
		bool issuerThread;
		if (issuerThread = list.Count == 0)
		{
			if (requestThread.endpointPosition != Vector3.zero)
			{
				Vector3 pos = info.TransformPoint(requestThread.endpointPosition);
				q = info.rotation * Quaternion.FromToRotation(Vector3.up, Vector3.Normalize(requestThread.endpointPosition));
				EventThread obj = new EventThread
				{
					infoThread = this,
					m_FacadeThread = printerThread,
					_CallbackThread = Matrix4x4.TRS(pos, q, info.lossyScale),
					m_PrototypeThread = ord + 1,
					indexerThread = true,
					issuerThread = true,
					m_SingletonThread = eventThread
				};
				ruleThread = obj;
				eventThread2 = obj;
			}
			else if (_WriterThread.Count != 0)
			{
				q = _WriterThread[_WriterThread.Count - 1]._CallbackThread.rotation;
			}
		}
		else if (list.Count > 1)
		{
			if (requestThread.multiChildType != VRCPhysBoneBase.MultiChildType.Average)
			{
				if (requestThread.multiChildType == VRCPhysBoneBase.MultiChildType.Ignore)
				{
					flag = true;
				}
			}
			else
			{
				Vector3 zero = Vector3.zero;
				foreach (Transform item in list)
				{
					zero += item.position;
				}
				zero /= (float)list.Count;
				Vector3 toDirection = zero - info.position;
				q = info.rotation * Quaternion.FromToRotation(info.up, toDirection);
				eventThread2 = (ruleThread = new EventThread
				{
					infoThread = this,
					m_FacadeThread = printerThread,
					_CallbackThread = Matrix4x4.TRS(zero, q, info.lossyScale),
					m_PrototypeThread = ord + 1,
					indexerThread = true,
					issuerThread = true,
					m_SingletonThread = eventThread
				});
			}
		}
		if (!flag)
		{
			eventThread.infoThread = this;
			eventThread.m_FacadeThread = printerThread;
			eventThread.m_AdvisorThread = info;
			eventThread._CallbackThread = Matrix4x4.TRS(info.position, q, info.lossyScale);
			eventThread.m_PrototypeThread = ord;
			eventThread.issuerThread = issuerThread;
			eventThread.m_RuleThread = ruleThread;
			EventThread eventThread3 = _WriterThread.LastOrDefault();
			if (eventThread3 != null && !eventThread3.issuerThread && eventThread3.m_RuleThread == null)
			{
				eventThread3.m_RuleThread = eventThread;
				eventThread.m_SingletonThread = eventThread3;
			}
			_WriterThread.Add(eventThread);
		}
		if (eventThread2 != null)
		{
			_WriterThread.Add(eventThread2);
		}
		foreach (Transform item2 in list)
		{
			SetupRecord(item2, ord + 1);
		}
	}

	internal void EnableRecord()
	{
		HashSet<EventThread> hashSet = new HashSet<EventThread>();
		_ListenerThread = new List<List<EventThread>>();
		foreach (EventThread item in _WriterThread)
		{
			if (!hashSet.Contains(item))
			{
				List<EventThread> list = new List<EventThread>();
				for (EventThread eventThread = item; eventThread != null; eventThread = eventThread.m_RuleThread)
				{
					list.Add(eventThread);
					hashSet.Add(eventThread);
				}
				_ListenerThread.Add(list);
			}
		}
	}

	internal static bool CollectStatus()
	{
		return ConcatStatus == null;
	}
}
