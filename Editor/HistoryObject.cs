using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BedtimeCore.EditorHistory
{
	[Serializable]
	internal struct HistoryObject
	{
		[SerializeField]
		private Object _selection;

		public HistoryObject(Object selection) => _selection = selection;

		public bool Exists => Selection != null;
		
		public Object Selection
		{
			get => _selection;
			set => _selection = value;
		}
	}
}