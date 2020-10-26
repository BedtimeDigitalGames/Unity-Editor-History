using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System;
using System.Runtime.InteropServices;
using UnityObject = UnityEngine.Object;
using System.IO;
using BedtimeCore.Persistence;
using BedtimeCore.Reflection;

namespace BedtimeCore.Editor
{
	[InitializeOnLoad]
	public class EditorHistory
	{
		static EditorHistory()
		{
			Selection.selectionChanged += HandleSelectionChanged;
			EditorApplication.update += OnUpdate;
			EditorApplication.hierarchyChanged += HandleHierarchyChanged;
			InitializeHistory();
		}

		const string FILENAME = "history.json";

#if UNITY_EDITOR_WIN
		[DllImport("user32.dll")]
		private static extern short GetAsyncKeyState(ushort virtualKeyCode);
#endif

		[Serializable]
		private class HistoryWrapper
		{
			public int location;
			public bool locked;
			public List<HistoryEntity> history = new List<HistoryEntity>(32);
		}

		[Serializable]
		public struct HistoryEntity
		{
			[SerializeField]
			public UnityObject selection;

			public bool Exists
			{
				get
				{
					return selection != null;
				}
			}

			public bool Active
			{
				get
				{
					if (!Exists)
					{
						return false;
					}
					var GO = selection as GameObject;
					if (GO != null)
					{
						return GO.activeInHierarchy;
					}
					return true;
				}
			}

			public Type Type
			{
				get
				{
					return type;
				}
			}

			public string Name
			{
				get
				{
					return Exists ? selection.name : "";
				}
			}

			public HistoryEntity(UnityObject selection)
			{
				this.selection = selection;
				this.type = selection.GetType();
			}

			private Type type;
		}

		public enum NavigationDirection : ushort { Forward = 0x06, Backward = 0x05 };
		
		private static bool ValidTempPath
		{
			get
			{
				return (Directory.Exists(TempPath));
			}
		}

		public static Action<int> OnHistoryUpdated;
		
		private static Type gameView = ReflectionUtility.GetType("UnityEditor.GameView");
		private static HistoryWrapper data = new HistoryWrapper();
		private static bool selectionWasSet;

		private static readonly Dictionary<NavigationDirection, bool> navigationLastState = new Dictionary<NavigationDirection, bool>()
		{
			{NavigationDirection.Backward, false},
			{NavigationDirection.Forward, false},
		};

		private static ValueStore<string> TempPath = new ValueStore<string>("EditorHistoryTemp", "", new EditorStorage());

		public static bool HistoryActive { get; private set; }

		public static int Location
		{
			get
			{
				return HistoryActive ? data.location : History.Count - 1;
			}

			private set
			{
				data.location = ClampLocation(value);
				OnHistoryUpdated?.Invoke(Location);
			}
		}

		public static List<HistoryEntity> History => data.history;

		public static bool Locked
		{
			get => data.locked;
			set
			{
				data.locked = value;
				WriteHistory();
			}
		}

		public static void Clear()
		{
			History.Clear();
			AddObject(Selection.activeObject);
		}

		public static void SetSelection(int location, bool setActive = true)
		{
			var focus = EditorWindow.focusedWindow;
			if (Application.isPlaying && focus != null && focus.GetType() == gameView)
			{
				return;
			}

			if (History.Count > 0)
			{
				HistoryActive = setActive;
				selectionWasSet = setActive;
				Selection.activeObject = History[ClampLocation(location)].selection;
				Location = location;
			}
		}

		public static void Navigate(NavigationDirection direction)
		{
			int amount = direction == NavigationDirection.Forward ? 1 : -1;
			if (!HistoryActive)
			{
				SetSelection(History.Count - 1);
			}
			else if (Location - ClampLocation(Location + amount) != 0)
			{
				SetSelection(Location + amount);
			}
		}

		private static void InitializeHistory()
		{
			if (!ValidTempPath)
			{
				TempPath.Value = FileUtil.GetUniqueTempPathInProject();
				TempPath.Save();
				Directory.CreateDirectory(TempPath);
				var dirInfo = new DirectoryInfo(TempPath);
				dirInfo.Attributes &= ~FileAttributes.ReadOnly;
			}
			else
			{
				var path = Path.Combine(TempPath, FILENAME);
				if (File.Exists(path))
				{
					var json = File.ReadAllText(path);
					if (!string.IsNullOrEmpty(json))
					{
						var obj = JsonUtility.FromJson<HistoryWrapper>(json);
						if (obj != null)
						{
							data = obj;
						}
					}
				}
			}

			AddObject(Selection.activeObject);
		}

		private static void HandleHierarchyChanged()
		{
			List<HistoryEntity> toRemove = new List<HistoryEntity>();
			for (int i = 0; i < History.Count; i++)
			{
				if (!History[i].Exists)
				{
					toRemove.Add(History[i]);
				}
			}
			for (int i = 0; i < toRemove.Count; i++)
			{
				History.Remove(toRemove[i]);
			}
		}

		private static int ClampLocation(int location)
		{
			location = location >= History.Count - 1 ? History.Count - 1 : location;
			location = location < 0 ? 0 : location;
			return location;
		}

		private static void WriteHistory()
		{
			if (!ValidTempPath)
			{
				return;
			}
			var path = Path.Combine(TempPath, FILENAME);
			using (var stream = new FileStream(path, FileMode.Create))
			{
				using (var writer = new StreamWriter(stream))
				{
					var output = JsonUtility.ToJson(data, false);
					writer.Write(output);
				}
			}
		}

		private static bool GetKey(NavigationDirection direction)
		{
#if UNITY_EDITOR_WIN
			return (GetAsyncKeyState((ushort)direction) & (1 << 16)) != 0;
#else
			return false;
#endif
		}

		private static void OnUpdate()
		{
			if (!UnityEditorInternal.InternalEditorUtility.isApplicationActive)
			{
				return;
			}

			var forward = GetKey(NavigationDirection.Forward);
			var backward = GetKey(NavigationDirection.Backward);

			if (backward && !navigationLastState[NavigationDirection.Backward])
			{
				Navigate(NavigationDirection.Backward);
			}
			if (forward && !navigationLastState[NavigationDirection.Forward])
			{
				Navigate(NavigationDirection.Forward);
			}

			navigationLastState[NavigationDirection.Forward] = forward;
			navigationLastState[NavigationDirection.Backward] = backward;
		}

		private static void ClearInFront()
		{
			for (int i = History.Count - 1; i > Location; i--)
			{
				History.RemoveAt(i);
			}
		}

		private static void AddObject(UnityObject obj)
		{
			if (obj == null || Locked)
			{
				SetSelection(History.Count - 1, false);
				return;
			}

			var entry = new HistoryEntity(obj);
			ClearInFront();
			if (History.Count > 0 && History[ClampLocation(Location)].selection == entry.selection)
			{
				return;
			}
			History.Add(entry);
			Location = History.Count - 1;
			WriteHistory();
		}

		private static void HandleSelectionChanged()
		{
			var selection = Selection.activeObject;
			HistoryActive = selection != null;

			if (selectionWasSet)
			{
				selectionWasSet = false;
				return;
			}
			AddObject(selection);
		}
	}
}