using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityObject = UnityEngine.Object;

namespace BedtimeCore.EditorHistory
{
	[InitializeOnLoad]
	internal class EditorHistory
	{
		static EditorHistory()
		{
			Selection.selectionChanged += OnSelectionChanged;
			EditorApplication.update += OnUpdate;
			EditorSceneManager.sceneOpened += OnSceneChanged;
			EditorSceneManager.sceneSaved += OnSceneSaved;
			AddObject(Selection.activeObject);
		}

		public static event Action<int> OnHistoryUpdated;

		public static bool IsNavigating { get; private set; }

		public static List<HistoryObject> HistoryObjects => History.HistoryObjects;

		public static int Location
		{
			get => IsNavigating ? History.Location : HistoryObjects.Count - 1;

			private set
			{
				History.Location = ClampLocation(value);
				OnHistoryUpdated?.Invoke(Location);
			}
		}

		public static void SetSelection(int location, bool setActive = true)
		{
			EditorWindow focus = EditorWindow.focusedWindow;
			if (Application.isPlaying && focus != null)
			{
				var focusType = focus.GetType();
				if (focusType.FullName == _gameViewTypeName)
				{
					return;
				}
			}

			if (HistoryObjects.Count > 0)
			{
				IsNavigating = setActive;
				_selectionWasSet = setActive;
				Selection.activeObject = HistoryObjects[ClampLocation(location)].Selection;
				Location = location;
			}
		}

		private static void Navigate(NavigationDirection direction, int amount = 1)
		{
			var totalAmount = amount * (direction == NavigationDirection.Forward ? 1 : -1);
			if (!IsNavigating)
			{
				SetSelection(HistoryObjects.Count - 1);
			}
			else if (Location - ClampLocation(Location + totalAmount) != 0)
			{
				var target = HistoryObjects[ClampLocation(Location + totalAmount)];
				if (!target.Exists)
				{
					Navigate(direction, amount + 1);
				}
				else
				{
					SetSelection(Location + totalAmount);
				}
			}
		}

		private static int ClampLocation(int location)
		{
			location = location >= HistoryObjects.Count - 1 ? HistoryObjects.Count - 1 : location;
			location = location < 0 ? 0 : location;
			return location;
		}

		private static void OnUpdate()
		{
			if (!InternalEditorUtility.isApplicationActive)
			{
				return;
			}

			bool forward = GetKey(NavigationDirection.Forward);
			bool backward = GetKey(NavigationDirection.Backward);

			if (backward && !_navigationLastState[NavigationDirection.Backward])
			{
				Navigate(NavigationDirection.Backward);
			}

			if (forward && !_navigationLastState[NavigationDirection.Forward])
			{
				Navigate(NavigationDirection.Forward);
			}

			_navigationLastState[NavigationDirection.Forward] = forward;
			_navigationLastState[NavigationDirection.Backward] = backward;
		}

		private static void ClearInFront()
		{
			for (int i = HistoryObjects.Count - 1; i > Location; i--)
			{
				HistoryObjects.RemoveAt(i);
			}
		}

		private static void AddObject(UnityObject obj)
		{
			if (obj == null)
			{
				SetSelection(HistoryObjects.Count - 1, false);
				return;
			}

			var entry = new HistoryObject(obj);
			ClearInFront();
			if (HistoryObjects.Count > 0 && HistoryObjects[ClampLocation(Location)].Selection == entry.Selection)
			{
				return;
			}

			HistoryObjects.Add(entry);
			Location = HistoryObjects.Count - 1;
			History.Save();
		}

		private static void OnSelectionChanged()
		{
			UnityObject selection = Selection.activeObject;
			IsNavigating = selection != null;

			if (_selectionWasSet)
			{
				_selectionWasSet = false;
				return;
			}

			AddObject(selection);
		}

		private static void OnSceneChanged(Scene scene, OpenSceneMode mode)
		{
			for (var i = 0; i < HistoryObjects.Count; i++)
			{
				HistoryObjects[i] = HistoryObjects[i].UpdateSelection(scene.name);
			}
		}

		private static void OnSceneSaved(Scene scene)
		{
			for (var i = 0; i < HistoryObjects.Count; i++)
			{
				HistoryObjects[i] =	HistoryObjects[i].UpdateName();
			}
		}

		private static History History => History.instance;

		private static readonly string _gameViewTypeName = "UnityEditor.GameView";

		private static bool _selectionWasSet;

		private static readonly Dictionary<NavigationDirection, bool> _navigationLastState = new Dictionary<NavigationDirection, bool>()
		{
			{NavigationDirection.Backward, false},
			{NavigationDirection.Forward, false},
		};

#if UNITY_EDITOR_WIN

		[DllImport("user32.dll")]
		private static extern short GetAsyncKeyState(ushort virtualKeyCode);

#endif
		
		private static bool GetKey(NavigationDirection direction)
		{
#if UNITY_EDITOR_WIN
			return (GetAsyncKeyState((ushort) direction) & (1 << 16)) != 0;
#else
			return false;
#endif
		}
	}
}