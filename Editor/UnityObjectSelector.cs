using System;
using UnityEditor;

namespace BedtimeCore.EditorHistory
{
    public class UnityObjectSelector : IHistorySelector
    {
        public UnityObjectSelector()
        {
            Selection.selectionChanged += () => AddToHistory?.Invoke(Selection.activeObject);    
        }
        
        public Type Type => typeof(UnityEngine.Object);
        
        public bool Select(object selection)
        {
            if(selection is UnityEngine.Object obj)
            {
                Selection.activeObject = obj;
                return true;
            }
            return false;
        }

        public event Action<object> AddToHistory;
    }
}