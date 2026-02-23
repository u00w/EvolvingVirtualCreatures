using UnityEngine;
using System.Collections;

namespace mattatz.EvolvingVirtualCreatures {

	/// <summary>
	/// Outputs a pair of oscillating signals: [sin(2π·t·freq), cos(2π·t·freq)].
	/// This gives the neural network a built-in clock signal, allowing a stateless
	/// feed-forward network to produce rhythmic, phase-alternating limb motion
	/// — the fundamental requirement for any walking gait.
	/// Without this, the same network input always produces the same output, so
	/// limbs can only hold static poses and the creature cannot walk.
	/// </summary>
	public class TimeSensor : Sensor {

		// Gait frequency in Hz. 1 Hz = one full leg cycle per second; a reasonable
		// starting value for a slow walk. The GA can learn to exploit any frequency.
		readonly float freq;

		public TimeSensor (float frequencyHz = 1f) {
			freq = frequencyHz;
		}

		public override int OutputCount () {
			return 2;
		}

		public override float[] Output () {
			float phase = 2f * Mathf.PI * Time.time * freq;
			return new float[] { Mathf.Sin(phase), Mathf.Cos(phase) };
		}

	}

}
