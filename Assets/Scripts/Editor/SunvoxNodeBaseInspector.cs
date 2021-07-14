namespace SunvoxNodeEditor
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(SunvoxNodeBase))]
    public class SunvoxNodeBaseInspector : Editor
    {
        SunvoxNodeBase node;



        private void OnEnable()
        {
            node = target as SunvoxNodeBase;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (node.TargetModule != null && node.TargetModule.Controllers != null)
            {
                ModuleGUI(node.TargetModule);
            }
        }

        private void ModuleGUI(ModuleBase targetModule)
        {
            for (int i = 0; i < targetModule.Controllers.Length; i++)
            {
                string ctrlName = targetModule.Controllers[i].name;
                int ctrlValue = targetModule.Controllers[i].Value;

                targetModule.Controllers[i].Value = EditorGUILayout.IntSlider(ctrlName, ctrlValue, 0, 32768);
            }
        }
    }
}