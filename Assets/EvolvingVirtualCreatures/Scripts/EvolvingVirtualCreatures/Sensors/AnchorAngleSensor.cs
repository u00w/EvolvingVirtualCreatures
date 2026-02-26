using UnityEngine;
using System.Collections;

namespace mattatz.EvolvingVirtualCreatures {

	public class AnchorAngleSensor : Sensor {

		protected Segment segment;

		public AnchorAngleSensor (Segment segment) {
			this.segment = segment;
		}

		public override int OutputCount () {
			return 3;
		}

		public override float[] Output () {

			// Segment may have been Destroy()'d during reproduction.
			// Avoid MissingReferenceException by returning neutral values.
			if (segment == null) return new float[] { 0f, 0f, 0f };

			// Read the actual physical orientation of the segment in its parent's local
			// space.  With ConfigurableJoint-driven physics the anchor is no longer
			// rotated at runtime, so transform.localRotation gives the real joint angle
			// (updated by the physics engine after every FixedUpdate) for both root and
			// non-root segments.
			var angles = segment.transform.localRotation.eulerAngles / 360f;
			return new float[] { angles.x, angles.y, angles.z };
		}

	}

}

