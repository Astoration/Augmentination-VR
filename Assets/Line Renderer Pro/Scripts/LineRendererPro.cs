using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheKnightsOfUnity.LineRendererPro
{
    /// <summary>
    /// Line Renderer Pro - advanced line renderer for Unity.
    /// </summary>
    [RequireComponent(typeof (MeshRenderer), typeof (MeshFilter)), ExecuteInEditMode, AddComponentMenu("Effects/Line Renderer Pro")]
    public class LineRendererPro : MonoBehaviour
    {
        /// <summary>
        /// Defines line points. Contains information about position and size.
        /// </summary>
        [Serializable]
        public struct LinePoint
        {
            [Tooltip("Point position.")] public Vector3 position;

            [Tooltip("Point width.")] public float width;

            [Tooltip("Point color.")] public Color color;

            [Tooltip("Point rounded factor.")] public int rounded;
        }

        private MeshFilter _meshFilter;

        /// <summary>
        /// Mesh filter attached to this game object.
        /// </summary>
        public MeshFilter meshFilter
        {
            get { return _meshFilter ?? (_meshFilter = GetComponent<MeshFilter>()); }
        }

        private MeshCollider _meshCollider;
        
        /// <summary>
        /// Mesh collider attached to this game object.
        /// </summary>
        public MeshCollider meshCollider
        {
            get
            {
                return _meshCollider ?? (_meshCollider = GetComponent<MeshCollider>());
            }
        }

        /// <summary>
        /// List of line points.
        /// </summary>
        [Tooltip("Line points.")] public List<LinePoint> linePoints = new List<LinePoint>(new[]
        {
            new LinePoint
            {
                color = Color.white,
                position = Vector3.zero,
                width = 1.0f
            },
            new LinePoint
            {
                color = Color.white,
                position = Vector3.forward,
                width = 1.0f
            }
        });

        /// <summary>
        /// If true then surface is drawn from both sides.
        /// </summary>
        [Tooltip("Draw surface from both sides.")] public bool doubleSided = true;

        /// <summary>
        /// If true then material scales with segment length.
        /// </summary>
        [Tooltip("Is material scaled with segment length.")] public bool scaleMaterialWithLength = true;

        /// <summary>
        /// If true then line loops.
        /// </summary>
        [Tooltip("Should line loop (the last point will be connected with the first one by segment).")] 
        public bool loop  = false;

        /// <summary>
        /// If true then mesh is updated in next Update.
        /// </summary>
        private bool _dirty;

        protected virtual void Awake()
        {
            // Update mesh at the beggining
            UpdateMesh();
        }

        protected virtual void Update()
        {
            // Check if mesh should be updated
            if (_dirty)
            {
                // Update mesh
                UpdateMesh();

                // Mark that mesh has been updated
                _dirty = false;
            }
        }

        protected virtual void OnDestroy()
        {
            // Get shared mesh from mesh filter
            var mesh = meshFilter.sharedMesh;

            // Check if mesh is created and belongs to this game object
            // Recognizes the mesh by name (which should equal to this game object instance id)
            if (mesh != null && mesh.name == gameObject.GetInstanceID().ToString())
            {
                // If it is delete it
                DestroyImmediate(mesh);
            }
        }

        /// <summary>
        /// Call it when you have modified line points.
        /// </summary>
        public void SetDirty()
        {
            _dirty = true;
        }

        /// <summary>
        /// Updates the mesh.
        /// </summary>
        public void UpdateMesh()
        {
            // Get shared mesh from mesh filter
            var mesh = meshFilter.sharedMesh;

            // Check if mesh is created and belongs to this game object
            // Recognizes the mesh by name (which should equal to this game object instance id)
            if (mesh == null || mesh.name != gameObject.GetInstanceID().ToString())
            {
                // If the mesh was empty or not recognized then create new one and name it with this game object instance id
                mesh = new Mesh {name = gameObject.GetInstanceID().ToString()};
            }
            else
            {
                // If the mesh was recognized then clear it's content
                mesh.Clear(false);
            }

            // Create lists for data that will be filed by generators
            var listVertices = new List<Vector3>();
            var listColors = new List<Color>();
            var listTriangles = new List<int>();
            var listUV = new List<Vector2>();
            var listUV2 = new List<Vector2>();

            // Iterate through all of the line points
            for (var i = 0; i < linePoints.Count + (loop ? 0 : -1); i++)
            {
                // Generate mesh segment
                GenerateMeshSegment(i, listVertices, listColors, listTriangles, listUV, listUV2);
            }

            // Assign generated data to mesh
            mesh.vertices = listVertices.ToArray();
            mesh.triangles = listTriangles.ToArray();
            mesh.uv = listUV.ToArray();
            mesh.uv2 = listUV2.ToArray();
            mesh.colors = listColors.ToArray();

            // Recalculate mesh normals and bounds
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            // Set mesh to mesh filter
            meshFilter.sharedMesh = mesh;

            if (meshCollider != null)
            {
                meshCollider.sharedMesh = null;
                meshCollider.sharedMesh = mesh;
            }
        }

        /// <summary>
        /// Generates mesh UV for line point <paramref name="i"/> segment.
        /// </summary>
        /// <param name="i">The number of segment's line point.</param>
        /// <param name="vertices">Array of segment vertices.</param>
        /// <param name="rightAttach">Is segment attached to previous right corner.</param>
        /// <param name="uv">Output array of first UV data.</param>
        /// <param name="uv2">Output array of second UV data.</param>
        private void GenerateMeshSegmentUV(int i, IList<Vector3> vertices, bool rightAttach, out Vector2[] uv, out Vector2[] uv2)
        {
            // Trick that allows to map trapezoid texture on segment

            var y1 =
                Mathf.Cos(Mathf.Deg2Rad*
                          Vector3.Angle((vertices[2] - vertices[0]).normalized, CalculatePointDirection(i)))*
                Vector3.Distance(vertices[0], vertices[2]);

            var y2 =
                Mathf.Cos(Mathf.Deg2Rad*
                          Vector3.Angle((vertices[3] - vertices[1]).normalized, CalculatePointDirection(i)))*
                Vector3.Distance(vertices[3], vertices[1]);

            float uvLength = scaleMaterialWithLength ? 1.0f : Mathf.Min(y1, y2);

            var x1 = Mathf.Sin(Mathf.Deg2Rad * Vector3.Angle((vertices[3] - vertices[2]).normalized, vertices[1] - vertices[2])) * Vector3.Distance(vertices[3], vertices[2]);

            var x2 = Mathf.Sin(Mathf.Deg2Rad * Vector3.Angle((vertices[1] - vertices[0]).normalized, vertices[3] - vertices[0])) * Vector3.Distance(vertices[1], vertices[0]);

            uv = new Vector2[vertices.Count];
            uv[0] = Vector2.zero;
            uv[1] = new Vector2(x2, 0);
            uv[2] = new Vector2(0, y1);
            uv[3] = new Vector2(x1, y2);

            uv2 = new Vector2[vertices.Count];
            uv2[0].x = uv2[1].x = uv[1].x;
            uv2[2].x = uv2[3].x = uv[3].x;
            uv2[0].y = uv2[2].y = uvLength;
            uv2[3].y = uv2[1].y = uvLength;


            float tll = 0.0f;

            float tlr = 0.0f;

            float amount = (vertices.Count - 4) / 2.0f;

            for (int j = 4; j < vertices.Count - 1; j += 2)
            {
                float l = scaleMaterialWithLength ? Mathf.Max(Vector3.Distance(vertices[j - 1], vertices[j + 1]), Vector3.Distance(vertices[j - 2], vertices[j])) : 1.0f / amount;

                if (j > 0)
                {

                    float ll = (rightAttach ? l : Mathf.Lerp(0.0f, l, (j - 4) / 2.0f / amount));
                    float lr = (rightAttach ? Mathf.Lerp(0.0f, l, (j - 4) / 2.0f / amount) : l);

                    uv[j] = new Vector2(0.0f,
                        uv[2].y + tll + ll);
                    uv[j + 1] = new Vector2(uv2[3].x,
                        uv[3].y + tlr + lr);
                    uv2[j] = new Vector2(uv[j + 1].x, uv2[2].y);
                    uv2[j + 1] = new Vector2(uv[j + 1].x, uv2[3].y);
                    tll += ll;
                    tlr += lr;
                }

                
            }
        }

        /// <summary>
        /// Generates mesh segment for line point <paramref name="i"/>.
        /// </summary>
        /// <param name="i">The number of segment's line point.</param>
        /// <param name="listVertices">The list of mesh vertices.</param>
        /// <param name="listColors">The list of mesh vertices color.</param>
        /// <param name="listTriangles">The list of mesh triangles.</param>
        /// <param name="listUV">The list of mesh uv.</param>
        /// <param name="listUV2">The list of mesh second uv.</param>
        private void GenerateMeshSegment(int i, List<Vector3> listVertices, List<Color> listColors,
            List<int> listTriangles, List<Vector2> listUV, List<Vector2> listUV2)
        {
            if (linePoints.Count > 0)
            {
                while (i < 0)
                    i += linePoints.Count;
            }

            i = i % linePoints.Count;

            int next = (i + 1)%linePoints.Count;

            // Get the start index of triangle vertex
            var triangleStartIndex = listVertices.Count;

            var vertices = new List<Vector3>();
            var colors = new List<Color>();

            vertices.AddRange(new[]
            {
                CalculatePointPosition(i, Vector3.left, 0.0f),
                CalculatePointPosition(i, Vector3.right, 0.0f)
            });

            colors.AddRange(new[]
            {
                linePoints[i].color,
                linePoints[i].color,
            });

            int round = linePoints[next].rounded;

            vertices.AddRange(new[]
            {
                CalculatePointPosition(next, Vector3.left, 0.0f),
                CalculatePointPosition(next, Vector3.right, 0.0f)
            });

            bool rightAttach = false;

            if (round > 0)
            {
                if (Vector3.Distance(vertices[0], vertices[2]) > Vector3.Distance(vertices[1], vertices[3]))
                {
                    rightAttach = true;

                    vertices[2] = vertices[3] + Quaternion.LookRotation(CalculatePointDirection(i)) * Vector3.left * linePoints[next].width * 2.0f;
                }
                else
                {

                    vertices[3] = vertices[2] + Quaternion.LookRotation(CalculatePointDirection(i)) * Vector3.right * linePoints[next].width * 2.0f;
                }
            }

            colors.AddRange(new[]
            {
                linePoints[next].color,
                linePoints[next].color
            });

            for (int j = 0; j < round; j ++)
            {
                float step = ((1.0f / round) * (j + 1));

                var q = Quaternion.Lerp(Quaternion.LookRotation(CalculatePointDirection(i)),
                    Quaternion.LookRotation(CalculatePointDirection(next)), step);

                if (rightAttach)
                {
                    vertices.AddRange(new []
                    {
                        vertices[3] + q * Vector3.left * linePoints[next].width * 2.0f,
                        vertices[3]
                    });
                }
                else
                {
                    vertices.AddRange(new []
                    {
                        vertices[2],
                        vertices[2] + q * Vector3.right * linePoints[next].width * 2.0f,
                    });
                }

                colors.AddRange(new[]
                {
                    linePoints[next].color,
                    linePoints[next].color,
                });
            }

            // Generate UVs
            Vector2[] uv;
            Vector2[] uv2;

            GenerateMeshSegmentUV(i, vertices, rightAttach, out uv, out uv2);

            float angle = Quaternion.FromToRotation(vertices[2] - vertices[0], vertices[3] - vertices[2]).eulerAngles.y;

            if (angle > 180.0f && round <= 0)
            {
                for (var j = 0; j < vertices.Count; j += 2)
                {
                    if (j > (round * 2) + 1)
                    {
                        var t = vertices[j];
                        vertices[j] = vertices[j + 1];
                        vertices[j + 1] = t;
                    }

                    var tuv = uv[j];
                    uv[j] = uv[j + 1];
                    uv[j + 1] = tuv;

                    var tuv2 = uv2[j];
                    uv2[j] = uv2[j + 1];
                    uv2[j + 1] = tuv2;
                }
            }

            for (var j = 0; j < vertices.Count - 2; j+=2)
            {
                listTriangles.AddRange(new[] { triangleStartIndex + j + 1, triangleStartIndex + j, triangleStartIndex + j + 2 });
                listTriangles.AddRange(new[] { triangleStartIndex + j + 2, triangleStartIndex + j + 3, triangleStartIndex + j + 1 });
            }

            listVertices.AddRange(vertices);
            listColors.AddRange(colors);
            listUV.AddRange(uv);
            listUV2.AddRange(uv2);

            if (doubleSided)
            {
                // If mesh is double sided then add the same data with inverted triangles
                triangleStartIndex = listVertices.Count;

                listVertices.AddRange(vertices);
                listColors.AddRange(colors);
                listUV.AddRange(uv);
                listUV2.AddRange(uv2);

                for (var j = 0; j < vertices.Count - 2; j += 2)
                {
                    listTriangles.AddRange(new[] { triangleStartIndex + j + 2, triangleStartIndex + j, triangleStartIndex + j + 1 });
                    listTriangles.AddRange(new[] { triangleStartIndex + j + 1, triangleStartIndex + j + 3, triangleStartIndex + j + 2 });
                }
            }
        }

        /// <summary>
        /// Returns the direction to which the point <paramref name="i"/> is facing to.
        /// </summary>
        /// <param name="i">Number of point.</param>
        public Vector3 CalculatePointDirection(int i)
        {
            var dir = Vector3.zero;

            if (linePoints.Count > 0)
            {
                while (i < 0)
                    i += linePoints.Count;
            }

            i = i % linePoints.Count;

            if (i < linePoints.Count - 1 || loop)
            {
                dir = (linePoints[(i + 1)%linePoints.Count].position - linePoints[i].position).normalized;
            }
            else if (i > 0)
            {
                dir = CalculatePointDirection(linePoints.Count - 2);
            }

            return dir;
        }

        /// <summary>
        /// Transforms position relative to point(of number <paramref name="i"/>) <paramref name="local"/> to position placed in this transform local space.
        /// </summary>
        /// <param name="i">Number of point.</param>
        /// <param name="local">Position relative to point.</param>
        /// <param name="directionLerp">Lerp factor between previous (-1.0f) - current (0.0f) - next (1.0f) direction.</param>
        public Vector3 CalculatePointPosition(int i, Vector3 local, float directionLerp)
        {
            if (linePoints.Count > 0)
            {
                while (i < 0)
                    i += linePoints.Count;
            }

            i = i % linePoints.Count;

            Quaternion direction;

            if (directionLerp <= 0.0f)
            {
                direction = Quaternion.Lerp(Quaternion.LookRotation(CalculatePointDirection(i - 1)),
                    Quaternion.LookRotation(CalculatePointDirection(i)), directionLerp + 1.0f);
            }
            else
            {
                direction = Quaternion.Lerp(Quaternion.LookRotation(CalculatePointDirection(i)),
                    Quaternion.LookRotation(CalculatePointDirection(i + 1)), directionLerp);
            }

            var pos = linePoints[i].position;

            pos += (direction*local)*linePoints[i].width;

            return pos;
        }
    }
}