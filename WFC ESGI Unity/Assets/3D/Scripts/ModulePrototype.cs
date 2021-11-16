using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace ESGI.WFC.ThreeDimensions
{
    public class ModulePrototype : MonoBehaviour
    {
        public float probability = 1.0f;
        public bool spawn = true;
        public bool isInterior = false;
        [SerializeField] private MeshFilter meshFilter;
        
        public const float BlockSize = 1f;

        public HorizontalFaceDetails left;
        public VerticalFaceDetails down;
        public HorizontalFaceDetails back;
        public HorizontalFaceDetails right;
        public VerticalFaceDetails up;
        public HorizontalFaceDetails forward;
        
        public FaceDetails[] Faces {
            get {
                return new FaceDetails[] {
                    left,
                    down,
                    back,
                    right,
                    up,
                    forward
                };
            }
        }
        
        public Mesh GetMesh(bool createEmptyFallbackMesh = true) {
            if (meshFilter != null && meshFilter.sharedMesh != null) {
                return meshFilter.sharedMesh;
            }
            return null;
        }

        public bool CompareRotatedVariants(int r1, int r2)
        {
            if (!((VerticalFaceDetails) this.Faces[Orientations.Up]).invariant || !((VerticalFaceDetails) this.Faces[Orientations.Down]).invariant) {
                return false;
            }

            for (int i = 0; i < 4; i++)
            {
                var face1 =
                    (HorizontalFaceDetails) Faces[Orientations.Rotate(Orientations.HorizontalDirections[i], r1)];
                var face2 = (HorizontalFaceDetails) Faces[Orientations.Rotate(Orientations.HorizontalDirections[i], r2)];

                if (face1.connector != face2.connector)
                {
                    return false;
                }
                
                if (!face1.symmetric && !face2.symmetric && face1.flipped != face2.flipped) {
                    return false;
                }
            }

            return true;
        }

        private void Reset()
        {
            up = new VerticalFaceDetails();
            down = new VerticalFaceDetails();
            right = new HorizontalFaceDetails();
            left = new HorizontalFaceDetails();
            forward = new HorizontalFaceDetails();
            back = new HorizontalFaceDetails();

            foreach (var face in Faces)
            {
                face.excludedNeighbours = new List<ModulePrototype>();
            }
        }

#if UNITY_EDITOR
	private static ModulePrototypeEditorData editorData;
	private static GUIStyle style;
	

	[DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.NotInSelectionHierarchy)]
	static void DrawGizmo(ModulePrototype modulePrototype, GizmoType gizmoType) {
		var transform = modulePrototype.transform;
		Vector3 position = transform.position;
		var rotation = transform.rotation;

		if (ModulePrototype.editorData == null || ModulePrototype.editorData.ModulePrototype != modulePrototype) {
			ModulePrototype.editorData = new ModulePrototypeEditorData(modulePrototype);
		}

		Gizmos.color = new Color(1f, 1f, 1f, 0.3f);
		if ((gizmoType & GizmoType.Selected) != 0) {
			for (int i = 0; i < 6; i++) {
				var hint = ModulePrototype.editorData.GetConnectorHint(i);
				if (hint.Mesh != null) {
					Gizmos.DrawMesh(hint.Mesh,
						position + rotation * Orientations.Direction[i].ToVector3() * BlockSize,
						rotation * Quaternion.Euler(Vector3.up * 90f * hint.Rotation));
				}
			}
		}
		for (int i = 0; i < 6; i++) {	
			if (modulePrototype.Faces[i].walkable) {
				Gizmos.color = Color.red;
				Gizmos.DrawLine(position + Vector3.down * 0.1f, position + rotation * Orientations.Rotations[i] * Vector3.forward * BlockSize * 0.5f + Vector3.down * 0.1f);
			}
			if (modulePrototype.Faces[i].isOcclusionPortal) {
				Gizmos.color = Color.blue;

				var dir = rotation * Orientations.Rotations[i] * Vector3.forward;
				Gizmos.DrawWireCube(position + dir, (Vector3.one - new Vector3(Mathf.Abs(dir.x), Mathf.Abs(dir.y), Mathf.Abs(dir.z))) * BlockSize);
			}			
		}

		if (ModulePrototype.style == null) {
			ModulePrototype.style = new GUIStyle();
			ModulePrototype.style.alignment = TextAnchor.MiddleCenter;
		}

		ModulePrototype.style.normal.textColor = Color.black;
		for (int i = 0; i < 6; i++) {
			var face = modulePrototype.Faces[i];
			Handles.Label(position + rotation * Orientations.Rotations[i] * Vector3.forward * BlockSize / 2f, face.ToString(), ModulePrototype.style);
		}
	}
#endif
    }
}
