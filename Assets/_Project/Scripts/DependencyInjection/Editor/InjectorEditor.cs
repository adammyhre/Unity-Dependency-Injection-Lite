using UnityEditor;
using UnityEngine;

namespace DependencyInjection {
    [CustomEditor(typeof(Injector))]
    public class InjectorEditor : Editor {
        public override void OnInspectorGUI() {
            DrawDefaultInspector();

            Injector injector = (Injector) target;

            if (GUILayout.Button("Validate Dependencies")) {
                injector.ValidateDependencies();
            }

            if (GUILayout.Button("Clear All Injectable Fields")) {
                injector.ClearDependencies();
                EditorUtility.SetDirty(injector);
            }
        }
    }
}