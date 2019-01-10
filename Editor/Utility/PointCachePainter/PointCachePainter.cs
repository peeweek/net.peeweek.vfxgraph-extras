using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.VFX.Utils
{
    public class PointCachePainter : EditorWindow
    {
        [MenuItem("Window/Visual Effects/Utilities/Point Cache Painter")]
        static void Open()
        {
            GetWindow<PointCachePainter>();
        }

        bool enabled = false;
        float radius = 10.0f;
        List<Vector3> points;
        List<Vector3> normals;

        private void OnEnable()
        {
            SceneView.onSceneGUIDelegate += this.OnSceneGUI;
            points = new List<Vector3>();
            normals = new List<Vector3>();
        }

        private void OnDisable()
        {
            SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
        }

        void OnGUI()
        {
            titleContent = GUIContent.none;
            enabled = EditorGUILayout.Toggle("Enabled", enabled);
            radius = EditorGUILayout.FloatField("Radius", radius);
        }

        void DrawAllPoints()
        {
            var evt = Event.current.type;
            Handles.color = Color.red;
            foreach (var point in points)
            {
                Handles.DotHandleCap(0, point, Quaternion.identity, 0.03f, evt);
            }

            Handles.color = Color.green;
            for (int i = 0; i < points.Count; i++)
            {
                Handles.DrawLine(points[i], points[i] + (normals[i] * 0.2f));
            }
        }

        void OnSceneGUI(SceneView sceneView)
        {
            if (!enabled)
                return;

            DrawAllPoints();

            var camera = sceneView.camera;
            Tools.current = Tool.None;
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

            if (Event.current.alt || (Event.current.type != EventType.MouseDown && Event.current.type != EventType.MouseDrag))
                return;

            // convert GUI coordinates to screen coordinates
            Vector3 screenPosition = Event.current.mousePosition;
            screenPosition.y = camera.pixelHeight - screenPosition.y;

            var t = Random.Range(-Mathf.PI, Mathf.PI);
            var r = Random.Range(0.0f, radius);

            var offset = new Vector3(Mathf.Sin(t), Mathf.Cos(t), 0.0f) * r;
            Ray ray = camera.ScreenPointToRay(screenPosition + offset);
            RaycastHit hit;
            // use a different Physics.Raycast() override if necessary
            if (Physics.Raycast(ray, out hit))
            {
                // do stuff here using hit.point
                // tell the event system you consumed the click
                points.Add(hit.point);
                normals.Add(hit.normal);
                Event.current.Use();
                SceneView.RepaintAll();
            }
        }

    }

}
