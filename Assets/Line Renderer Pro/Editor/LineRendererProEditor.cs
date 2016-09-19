using System;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace TheKnightsOfUnity.LineRendererPro
{
    [CustomEditor(typeof (LineRendererPro))]
    public class LineRendererProEditor : Editor
    {
        private ReorderableList _pointList;

        private LineRendererPro lineRendererPro
        {
            get { return target as LineRendererPro; }
        }

        private void OnEnable()
        {
            _pointList = new ReorderableList(serializedObject, serializedObject.FindProperty("linePoints"), true, true,
                true, true)
            {
                drawHeaderCallback = rect =>
                {
                    EditorGUI.LabelField(rect,
                        "Points (amount : " + serializedObject.FindProperty("linePoints").arraySize + ")");
                }
            };

            _pointList.drawElementCallback = (rect, index, active, focused) =>
            {
                EditorGUI.BeginChangeCheck();

                var arrayElement = _pointList.serializedProperty.GetArrayElementAtIndex(index);

                float maxWidth = rect.width;

                rect.width = Mathf.Max(150, maxWidth*0.3f);

                EditorGUI.PropertyField(rect, arrayElement.FindPropertyRelative("position"), new GUIContent(), true);

                rect.x += rect.width;

                rect.width = 15;

                EditorGUI.LabelField(rect, "W");

                rect.x += rect.width;

                rect.width = Mathf.Max(30, maxWidth*0.1f);

                EditorGUI.PropertyField(rect, arrayElement.FindPropertyRelative("width"), new GUIContent(), true);

                rect.x += rect.width;

                rect.width = 13;

                EditorGUI.LabelField(rect, "R");

                rect.x += rect.width;

                rect.width = Mathf.Max(30, maxWidth * 0.1f);

                EditorGUI.PropertyField(rect, arrayElement.FindPropertyRelative("rounded"), new GUIContent(), true);

                rect.x += rect.width + 10;

                rect.width = Mathf.Max(50, maxWidth*0.1f);

                EditorGUI.PropertyField(rect, arrayElement.FindPropertyRelative("color"), new GUIContent(), true);

                serializedObject.ApplyModifiedProperties();

                if (EditorGUI.EndChangeCheck())
                {
                    lineRendererPro.SetDirty();
                }
            };

            _pointList.onAddCallback = list =>
            {
                list.serializedProperty.InsertArrayElementAtIndex(list.count);

                if (list.count == 1)
                {
                    var arrayElement = list.serializedProperty.GetArrayElementAtIndex(0);

                    arrayElement.FindPropertyRelative("color").colorValue = Color.white;
                    arrayElement.FindPropertyRelative("width").floatValue = 1.0f;
                }

                lineRendererPro.SetDirty();
            };
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("loop"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("doubleSided"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("scaleMaterialWithLength"));

            if (EditorGUI.EndChangeCheck())
            {
                lineRendererPro.SetDirty();
            }

            _pointList.DoLayoutList();

            if (lineRendererPro.meshFilter != null && lineRendererPro.meshFilter.sharedMesh != null && GUILayout.Button("Center points"))
            {
                var sum = lineRendererPro.transform.TransformPoint(lineRendererPro.meshFilter.sharedMesh.bounds.center);

                var temp = new GameObject {hideFlags = HideFlags.HideAndDontSave};

                temp.transform.SetParent(lineRendererPro.transform.parent);
                temp.transform.localRotation = lineRendererPro.transform.localRotation;
                temp.transform.localScale = lineRendererPro.transform.localScale;
                temp.transform.position = sum;

                for (int i = 0; i < _pointList.count; i++)
                {
                    var prop = _pointList.serializedProperty.GetArrayElementAtIndex(i).FindPropertyRelative("position");

                    prop.vector3Value = temp.transform.InverseTransformPoint(lineRendererPro.transform.TransformPoint(prop.vector3Value));
                }

                lineRendererPro.transform.position = sum;
            }

            serializedObject.ApplyModifiedProperties();
        }

        private enum LineTool
        {
            None,
            Modify,
            Remove
        }

        private static LineTool _lineTool = LineTool.Modify;

        private void OnSceneGUI()
        {
            if (_lineTool == LineTool.None)
            {
                Tools.hidden = false;
            }
            else if (_lineTool == LineTool.Modify)
            {
                Tools.hidden = true;

                if (_pointList != null)
                {
                    for (var i = 0; i < lineRendererPro.linePoints.Count; i++)
                    {
                        var arrayElement = _pointList.serializedProperty.GetArrayElementAtIndex(i);

                        var positionProperty = arrayElement.FindPropertyRelative("position");

                        var sizeProperty = arrayElement.FindPropertyRelative("width");

                        var roundedProperty = arrayElement.FindPropertyRelative("rounded");

                        var realPosition = lineRendererPro.transform.TransformPoint(positionProperty.vector3Value);

                        var handleSize = HandleUtility.GetHandleSize(realPosition)*0.2f;

                        if (Tools.current == Tool.Move)
                        {
                            EditorGUI.BeginChangeCheck();

                            positionProperty.vector3Value =
                                lineRendererPro.transform.InverseTransformPoint(Handles.DoPositionHandle(realPosition,
                                    Quaternion.identity));

                            if (EditorGUI.EndChangeCheck())
                            {
                                _pointList.index = i;
                            }
                        }
                        else if (Tools.current == Tool.Scale)
                        {
                            EditorGUI.BeginChangeCheck();

                            float scaleHandleSize = handleSize*0.85f;

                            var dir = Quaternion.LookRotation(lineRendererPro.CalculatePointDirection(i));

                            var leftDir = dir*Vector3.left;

                            var leftPoint =
                                lineRendererPro.transform.TransformPoint(lineRendererPro.CalculatePointPosition(i, Vector3.left, 0.0f));

                            var newLeftPoint = lineRendererPro.transform.InverseTransformPoint(Handles.Slider(leftPoint,
                                 lineRendererPro.transform.TransformDirection(leftDir), scaleHandleSize,
                                Handles.SphereCap, 0.0f));

                            var rightDir = dir*Vector3.right;

                            var rightPoint =
                                lineRendererPro.transform.TransformPoint(lineRendererPro.CalculatePointPosition(i,
                                    Vector3.right, 0.0f));

                            var newRightPoint =
                                lineRendererPro.transform.InverseTransformPoint(Handles.Slider(rightPoint,
                                     lineRendererPro.transform.TransformDirection(rightDir), scaleHandleSize,
                                    Handles.SphereCap, 0.0f));

                            float xSize = sizeProperty.floatValue;

                            float xLeftSize = (newLeftPoint - lineRendererPro.linePoints[i].position).normalized == leftDir ? Vector3.Distance(lineRendererPro.linePoints[i].position, newLeftPoint) : 0.0f;

                            float xRightSize = (newRightPoint - lineRendererPro.linePoints[i].position).normalized == rightDir ? Vector3.Distance(lineRendererPro.linePoints[i].position, newRightPoint) : 0.0f;

                            xSize = Mathf.Abs(xSize - xLeftSize) > Mathf.Abs(xSize - xRightSize) ? xLeftSize : xRightSize;

                            sizeProperty.floatValue = xSize;

                            if (EditorGUI.EndChangeCheck())
                            {
                                _pointList.index = i;
                            }
                        }
                        else if (Tools.current == Tool.Rotate)
                        {
                            Handles.BeginGUI();

                            EditorGUI.BeginChangeCheck();

                            roundedProperty.intValue = EditorGUI.IntSlider(HandleUtility.WorldPointToSizedRect(lineRendererPro.transform.TransformPoint(lineRendererPro.linePoints[i].position), new GUIContent(""), new GUIStyle() { fixedWidth = 200.0f, fixedHeight = 20.0f}), roundedProperty.intValue, 0, 30);

                            if (EditorGUI.EndChangeCheck())
                            {
                                _pointList.index = i;
                            }

                            Handles.EndGUI();
                        }
                    }

                    serializedObject.ApplyModifiedProperties();



                    if (Event.current.type == EventType.keyDown && Event.current.keyCode == KeyCode.Space)
                    {
                        Event.current.Use();

                        _pointList.serializedProperty.InsertArrayElementAtIndex(_pointList.serializedProperty.arraySize);

                        var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                        var position = ray.GetPoint(10.0f);

                        RaycastHit info;

                        if (Physics.Raycast(ray, out info))
                            position = info.point;

                        _pointList.serializedProperty.GetArrayElementAtIndex(_pointList.serializedProperty.arraySize - 1)
                            .FindPropertyRelative("position").vector3Value =
                            lineRendererPro.transform.InverseTransformPoint(position);

                        Repaint();
                    }
                }
            }
            else if (_lineTool == LineTool.Remove)
            {
                Tools.hidden = true;

                if (_pointList != null)
                {
                    for (var i = 0; i < _pointList.serializedProperty.arraySize; i++)
                    {
                        var arrayElement = _pointList.serializedProperty.GetArrayElementAtIndex(i);

                        var positionProperty = arrayElement.FindPropertyRelative("position");

                        var realPosition = lineRendererPro.transform.TransformPoint(positionProperty.vector3Value);

                        var handleSize = HandleUtility.GetHandleSize(realPosition)*0.2f;

                        Handles.color = new Color(1.0f, 0.0f, 0.0f);

                        if (Handles.Button(realPosition, Quaternion.identity, handleSize, handleSize, Handles.SphereCap))
                        {
                            _pointList.serializedProperty.DeleteArrayElementAtIndex(i);

                            break;
                        }

                    }

                    serializedObject.ApplyModifiedProperties();
                }
            }

            Handles.BeginGUI();
            GUILayout.BeginArea(new Rect(Screen.width - 200, Screen.height - 70, 190, 50));
            {
                GUILayout.BeginVertical(EditorStyles.textArea);
                {
                    _lineTool = (LineTool) GUILayout.Toolbar((int) _lineTool, Enum.GetNames(typeof (LineTool)));

                    if (Event.current.type == EventType.keyDown && Event.current.keyCode == KeyCode.Z)
                    {
                        Event.current.Use();

                        if (_lineTool == LineTool.None)
                            _lineTool = LineTool.Modify;
                        else if (_lineTool == LineTool.Modify)
                            _lineTool = LineTool.Remove;
                        else if (_lineTool == LineTool.Remove)
                            _lineTool = LineTool.None;
                    }
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndArea();
            Handles.EndGUI();

            lineRendererPro.SetDirty();
        }

        private void OnDisable()
        {
            Tools.hidden = false;
        }

        [MenuItem("GameObject/3D Object/Line Renderer Pro")]
        public static void CreateLineRendererPro()
        {
            Vector3 pos = SceneView.lastActiveSceneView.camera.transform.position + SceneView.lastActiveSceneView.camera.transform.forward * 10.0f;

            var go = new GameObject(GameObjectUtility.GetUniqueNameForSibling(null, "Line"), typeof (LineRendererPro));
            go.transform.position = pos;
            go.transform.rotation = Quaternion.identity;
            Selection.activeGameObject = go;
        }

    }
}
