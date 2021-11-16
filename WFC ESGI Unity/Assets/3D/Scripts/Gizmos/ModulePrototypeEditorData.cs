using System.Collections.Generic;
using UnityEngine;

namespace ESGI.WFC.ThreeDimensions
{
    public class ModulePrototypeEditorData
    {
        public readonly ModulePrototype ModulePrototype;

	private readonly ModulePrototype[] prototypes;

	private readonly Dictionary<ModulePrototype, Mesh> meshes;

	public struct ConnectorHint {
		public readonly Mesh Mesh;
		public readonly int Rotation;

		public ConnectorHint(int rotation, Mesh mesh) {
			this.Rotation = rotation;
			this.Mesh = mesh;
		}
	}

	public ModulePrototypeEditorData(ModulePrototype modulePrototype) {
		this.ModulePrototype = modulePrototype;
		this.prototypes = modulePrototype.transform.parent.GetComponentsInChildren<ModulePrototype>();
		this.meshes = new Dictionary<ModulePrototype, Mesh>();
	}

	private Mesh getMesh(ModulePrototype modulePrototype) {
		if (this.meshes.ContainsKey(modulePrototype)) {
			return this.meshes[modulePrototype];
		}
		var mesh = modulePrototype.GetMesh(false);
		this.meshes[modulePrototype] = mesh;
		return mesh;
	}

	public ConnectorHint GetConnectorHint(int direction) {
		var face = this.ModulePrototype.Faces[direction];
		ModulePrototype cachedProto = null;
		int cachedRotation = 0;
		if (face is HorizontalFaceDetails horizontalFace) {
			foreach (var prototype in this.prototypes) {
				if (face.excludedNeighbours.Contains(prototype)) {
					continue;
				}
				for (int rotation = 0; rotation < 4; rotation++) {
					var otherFace = prototype.Faces[Orientations.Rotate(direction, rotation + 2)] as HorizontalFaceDetails;
					if (otherFace.excludedNeighbours.Contains(this.ModulePrototype)) {
						continue;
					}
					if (otherFace.connector == face.connector && ((horizontalFace.symmetric && otherFace.symmetric) || otherFace.flipped != horizontalFace.flipped)) {
						if (prototype == this.ModulePrototype)
						{
							cachedProto = prototype;
							cachedRotation = rotation;
						}
						else
						{
							return new ConnectorHint(rotation, this.getMesh(prototype));
						}
						
					}
				}
			}
		}

		if (cachedProto != null)
		{
			return new ConnectorHint(cachedRotation, getMesh(cachedProto));
		}

		if (face is VerticalFaceDetails verticalFace) {
			foreach (var prototype in this.prototypes) {
				if (prototype == this.ModulePrototype || face.excludedNeighbours.Contains(prototype)) {
					continue;
				}
				var otherFace = prototype.Faces[(direction + 3) % 6] as VerticalFaceDetails;
				if (otherFace.excludedNeighbours.Contains(this.ModulePrototype) || otherFace.connector != face.connector) {
					continue;
				}

				var rotation = verticalFace.rotation - otherFace.rotation;
				if (prototype == this.ModulePrototype)
				{
					cachedProto = prototype;
					cachedRotation = rotation;
				}
				else
				{
					return new ConnectorHint(rotation, this.getMesh(prototype));
				}
			}
		}
		
		if (cachedProto != null)
		{
			return new ConnectorHint(cachedRotation, getMesh(cachedProto));
		}

		return new ConnectorHint();
	}
    }
}