using UnityEngine;
using System.Collections;

namespace mattatz.EvolvingVirtualCreatures {

	/// <summary>
	/// Outputs the world-up direction expressed in the body's local coordinate frame.
	/// When the creature is perfectly upright the output is (0, 1, 0).
	/// When tipped 90° to the right it becomes roughly (−1, 0, 0), etc.
	/// This gives the neural network a direct, unambiguous signal for which way
	/// is "up" relative to its own body — the key information needed to learn to stand.
	/// </summary>
	public class GravitySensor : Sensor {

		readonly Transform bodyTransform;

		public GravitySensor (Transform bodyTransform) {
			this.bodyTransform = bodyTransform;
		}

		public override int OutputCount () {
			return 3;
		}

		public override float[] Output () {
			if (bodyTransform == null) return new float[] { 0f, 0f, 0f };
			Vector3 localUp = bodyTransform.InverseTransformDirection(Vector3.up);
			return new float[] { localUp.x, localUp.y, localUp.z };
		}

	}

}
