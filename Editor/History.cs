using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BedtimeCore.EditorHistory
{
	[Serializable]
	internal class History : ScriptableSingleton<History>
	{
		[SerializeField]
		private int _location;

		[SerializeField]
		private bool _locked;

		[SerializeField]
		private List<HistoryObject> _historyObjects = new List<HistoryObject>(32);

		public int Location
		{
			get => _location;
			set => _location = value;
		}

		public bool Locked
		{
			get => _locked;
			set => _locked = value;
		}
		
		public void Save()
		{
			Save(true);
		}

		public List<HistoryObject> HistoryObjects => _historyObjects;
	}
}