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

			//new
			// Segment may have been Destroy()'d during reproduction.
			// Avoid MissingReferenceException by returning neutral values.
			if (segment == null) return new float[] { 0f, 0f, 0f };

			// Anchor can also be destroyed / not yet created
			if (!segment.Root && segment.Anchor == null) return new float[] { 0f, 0f, 0f };
			//new

			Vector3 angles;
			if(segment.Root) {
				angles = segment.transform.localRotation.eulerAngles / 360f;
			} else {
				angles = segment.Anchor.localRotation.eulerAngles / 360f;
			}
			return new float[] { angles.x, angles.y, angles.z };
		}

	}

}

