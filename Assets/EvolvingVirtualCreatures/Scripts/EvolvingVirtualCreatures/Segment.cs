//using UnityEngine;

//using System.Collections;
//using System.Collections.Generic;

//namespace mattatz.EvolvingVirtualCreatures {

//	public class Segment : Part, Effector {

//		public List<Segment> Segments { get { return segments; } }

//		public float[] Sensor { 
//			get {
//				var angles = transform.localRotation.eulerAngles / 360f;
//				return new float[] {
//					angles.x, angles.y, angles.z, 
//					front.Contact01, back.Contact01, left.Contact01, right.Contact01, up.Contact01, down.Contact01
//				};
//			} 
//		}
//		public Transform Anchor { get { return anchor; } }
//		public bool Root { get { return root; } }

//		protected CharacterJoint joint;
//		protected SideType side;
//		protected Vector2 forceRange = new Vector2(-100f, 100f);
//        protected float axisForce, swingAxisForce;
//		protected List<Segment> segments = new List<Segment>();

//		protected bool root;
//		protected Transform anchor;
//		protected Vector3 axis;
//		protected Vector3 secondaryAxis;

//		[SerializeField] protected Vector2 axisRange = new Vector2(-90f, 90f);
//		[SerializeField] protected Vector2 secondaryAxisRange = new Vector2(-90f, 90f);

//		[SerializeField, Range(0f, 1f)] float axisAngle = 0.5f;
//		[SerializeField, Range(0f, 1f)] float secondaryAxisAngle = 0.5f;

//		float toAxisAngle, toSecondaryAxisAngle;

//		public void SetRoot(bool flag) {
//			if(flag) {
//				var body = gameObject.AddComponent<Rigidbody>();
//				body.mass = transform.localScale.magnitude;
//				body.interpolation = RigidbodyInterpolation.Interpolate;
//			}
//			root = flag;
//		}

//		public void Init () {
//			/*
//			Body.velocity *= 0f;
//			Body.angularVelocity *= 0f;
//			*/
//			segments.ForEach(s => {
//				s.Init (this);
//			});

//			toAxisAngle = axisAngle;
//			toSecondaryAxisAngle = secondaryAxisAngle;
//		}

//		public void Init (Segment parent) {
//			// Body.velocity *= 0f;
//			// Body.angularVelocity *= 0f;
//			// transform.position = parent.transform.position + Helper.directions[side] * 1.25f;

//			var dir = Helper.directions[side];

//			// var po = Vector3.Scale (dir, parent.transform.localScale) * 0.5f;
//			// var lo = Vector3.Scale (dir, transform.localScale) * 0.5f;
//			// var offset = po + lo + lo * 0.25f;

//			var offset = dir;

//			axis = Helper.directions[Helper.Axis(side)];
//			secondaryAxis = Helper.directions[Helper.SwingAxis(side)];

//			anchor = (new GameObject("Anchor")).transform;
//			if(parent.root) {
//				anchor.parent = parent.transform;
//			} else {
//				anchor.parent = parent.Anchor;
//			}

//			// parent.transform.localPosition * 2f;

//			if(parent.root) {
//				anchor.localPosition = offset * 0.5f;
//			} else {
//				anchor.localPosition = parent.transform.localPosition + Vector3.Scale(parent.transform.localScale, offset) * 0.5f;
//			}

//			transform.parent = anchor;
//			// transform.localPosition = Vector3.Scale(transform.localScale, offset) * 0.5f;
//			transform.localPosition = Vector3.Scale(transform.localScale, offset) * 0.75f;

//			// anchor = - offset * 0.5f;

//			// need to add HingeJoint after setting position
//			/*
//			if(this.joint == null) {
//				ActivateJoint(parent, side);
//			}
//			*/

//			Init ();
//		}

//		/*
//		public void ActivateJoint (Segment parent, SideType side) {
//			var joint = CreateJoint();
//			joint.connectedBody = parent.Body;
//			joint.axis = Helper.directions[Helper.Axis(side)];
//			joint.swingAxis = Helper.directions[Helper.SwingAxis(side)];
//			joint.anchor = Helper.directions[Helper.Inverse(side)] * 0.5f;
//		}
//		*/

//		public void Connect (Segment parent, SideType side) {
//			this.side = side;
//			Init (parent);
//			parent.AddSegment(this);
//		}

//		public void Sensing (List<float> sensors) {
//			var sensor = Sensor;
//			for(int i = 0, n = sensor.Length; i < n; i++) {
//				sensors.Add(sensor[i]);
//			}
//			Segments.ForEach(segment => {
//				segment.Sensing(sensors);
//			});
//		}

//		public void WakeUp () {
//			// Body.isKinematic = false;
//			Segments.ForEach(segment => {
//				segment.WakeUp();
//			});
//		}

//		public void Sleep () {
//			// Body.isKinematic = true;
//			Segments.ForEach(segment => {
//				segment.Sleep();
//			});
//		}

//        void FixedUpdate () {
//			/*
//			if(anchor != null) {
//				anchor.localRotation = 
//					Quaternion.AngleAxis(Mathf.Lerp (axisRange.x, axisRange.y, axisAngle), axis) * 
//					Quaternion.AngleAxis(Mathf.Lerp (secondaryAxisRange.x, secondaryAxisRange.y, secondaryAxisAngle), secondaryAxis);
//			}
//			*/

//			// if(joint == null) return;
//			// Body.AddRelativeTorque(joint.axis * axisForce);
//			// Body.AddRelativeTorque(joint.swingAxis * swingAxisForce);

//			axisAngle = Mathf.Lerp (axisAngle, toAxisAngle, Time.fixedDeltaTime);
//			secondaryAxisAngle = Mathf.Lerp (secondaryAxisAngle, toSecondaryAxisAngle, Time.fixedDeltaTime);
//			Apply();
//        }

//		public void AddSegment (Segment segment) {
//			segments.Add(segment);
//		}

//		public int InputCount() {
//			return 2;
//		}

//		public void Affect(float[] inputs, float dt) {
//			if(anchor != null) {
//				// axisAngle = Mathf.Lerp(axisAngle, inputs[0], dt);
//				// secondaryAxisAngle = Mathf.Lerp(secondaryAxisAngle, inputs[1], dt);

//				toAxisAngle = inputs[0];
//				toSecondaryAxisAngle = inputs[1];
//			}

//			// if(joint == null) return;

//			// joint.axis;
//			// var axisLimit = joint.swing1Limit;
//			// transform.RotateAround(joint.connectedAnchor, joint.axis, Mathf.Lerp(joint.lowTwistLimit.limit, joint.highTwistLimit.limit, inputs[0]));

//            // axisForce = Mathf.Lerp (forceRange.x, forceRange.y, inputs[0]);
//            // swingAxisForce = Mathf.Lerp (forceRange.x, forceRange.y, inputs[1]);
//		}

//		void Apply () {
//			if(anchor == null) return;

//			anchor.localRotation = 
//				Quaternion.AngleAxis(Mathf.Lerp (axisRange.x, axisRange.y, axisAngle), axis) * 
//				Quaternion.AngleAxis(Mathf.Lerp (secondaryAxisRange.x, secondaryAxisRange.y, secondaryAxisAngle), secondaryAxis);
//		}

//		void OnDrawGizmosSelected () {
//			if(anchor == null) return;
//			Gizmos.color = Color.magenta;
//			Gizmos.DrawWireSphere(anchor.position, 0.2f);
//		}

//	}

//}

using UnityEngine;

using System.Collections;
using System.Collections.Generic;

namespace mattatz.EvolvingVirtualCreatures
{

	public class Segment : Part, Effector
	{

		public List<Segment> Segments { get { return segments; } }

		public float[] Sensor
		{
			get
			{
				var angles = transform.localRotation.eulerAngles / 360f;
				return new float[] {
					angles.x, angles.y, angles.z,
					front.Contact01, back.Contact01, left.Contact01, right.Contact01, up.Contact01, down.Contact01
				};
			}
		}
		public Transform Anchor { get { return anchor; } }
		public bool Root { get { return root; } }

		// Expose Rigidbody for Factory position reset and Wake/Sleep control
		public Rigidbody Body
		{
			get
			{
				if (body == null) body = GetComponent<Rigidbody>();
				return body;
			}
		}

		protected Rigidbody body;
		protected ConfigurableJoint configurableJoint;
		protected SideType side;
		protected List<Segment> segments = new List<Segment>();

		protected bool root;
		protected Transform anchor;
		protected Vector3 axis;
		protected Vector3 secondaryAxis;

		[SerializeField] protected Vector2 axisRange = new Vector2(-90f, 90f);
		[SerializeField] protected Vector2 secondaryAxisRange = new Vector2(-90f, 90f);

		[SerializeField, Range(0f, 1f)] float axisAngle = 0.5f;
		[SerializeField, Range(0f, 1f)] float secondaryAxisAngle = 0.5f;

		float toAxisAngle, toSecondaryAxisAngle;

		// Physics tuning constants — conservative values for <8-segment creatures.
		// Increase spring/force if limbs cannot overcome gravity; reduce if unstable.
		const int   SOLVER_ITERATIONS          = 12;
		const int   SOLVER_VELOCITY_ITERATIONS = 4;
		const float JOINT_SPRING               = 100f;
		const float JOINT_DAMPER              = 10f;
		const float JOINT_MAX_FORCE            = 500f;
		const float LINEAR_DAMPING             = 0.5f;
		const float ANGULAR_DAMPING            = 1.0f;

		public void SetRoot(bool flag)
		{
			if (flag)
			{
				body = gameObject.AddComponent<Rigidbody>();
				body.mass = transform.localScale.magnitude;
				body.interpolation = RigidbodyInterpolation.Interpolate;
				// Higher solver iterations reduce jitter in jointed chains (<8 segments)
				body.solverIterations = SOLVER_ITERATIONS;
				body.solverVelocityIterations = SOLVER_VELOCITY_ITERATIONS;
			}
			root = flag;
		}

		public void Init()
		{
			segments.ForEach(s => {
				s.Init(this);
			});

			toAxisAngle = axisAngle;
			toSecondaryAxisAngle = secondaryAxisAngle;
		}

		public void Init(Segment parent)
		{
			// Guard: skip repositioning/component creation on re-init (e.g. from Generate()).
			// Only the first call (when body == null) sets up the full physical segment.
			if (body == null)
			{
				var dir = Helper.directions[side];
				var offset = dir;

				axis = Helper.directions[Helper.Axis(side)];
				secondaryAxis = Helper.directions[Helper.SwingAxis(side)];

				// Anchor is kept for hierarchy organisation and the Anchor property.
				// It is NOT rotated at runtime (joint drives the physics body instead).
				anchor = (new GameObject("Anchor")).transform;
				if (parent.root)
				{
					anchor.parent = parent.transform;
				}
				else
				{
					anchor.parent = parent.Anchor;
				}

				if (parent.root)
				{
					anchor.localPosition = offset * 0.5f;
				}
				else
				{
					anchor.localPosition = parent.transform.localPosition + Vector3.Scale(parent.transform.localScale, offset) * 0.5f;
				}

				transform.parent = anchor;
				transform.localPosition = Vector3.Scale(transform.localScale, offset) * 0.75f;

				// Each child segment needs its own Rigidbody for ConfigurableJoint-based actuation.
				// Conservative parameters for <8-segment creatures to avoid jitter/explosions.
				body = gameObject.AddComponent<Rigidbody>();
				body.mass = transform.localScale.magnitude;
				body.interpolation = RigidbodyInterpolation.Interpolate;
				body.solverIterations = SOLVER_ITERATIONS;
				body.solverVelocityIterations = SOLVER_VELOCITY_ITERATIONS;
				// Light damping prevents free oscillation while still allowing driven motion
				body.linearDamping = LINEAR_DAMPING;
				body.angularDamping = ANGULAR_DAMPING;

				// Create ConfigurableJoint connecting this segment to its parent's Rigidbody.
				configurableJoint = CreateConfigurableJoint(parent);
			}

			Init();
		}

		// Build a ConfigurableJoint that locks linear DOFs and uses a slerp drive to
		// actuate angular motion based on network outputs.  Spring/damper values are
		// deliberately conservative for small (<8-segment) creatures.
		private ConfigurableJoint CreateConfigurableJoint(Segment parent)
		{
			var parentRb = parent.GetComponent<Rigidbody>();
			if (parentRb == null)
			{
				Debug.LogWarning("[Segment] Parent has no Rigidbody — ConfigurableJoint skipped.", this);
				return null;
			}

			var cj = gameObject.AddComponent<ConfigurableJoint>();
			cj.connectedBody = parentRb;

			// Lock all linear motion — limbs stay rigidly attached at their spawn position
			cj.xMotion = ConfigurableJointMotion.Locked;
			cj.yMotion = ConfigurableJointMotion.Locked;
			cj.zMotion = ConfigurableJointMotion.Locked;

			// Angular limits derived from axisRange / secondaryAxisRange (default ±90°).
			// axisRange.x should be negative (low), axisRange.y positive (high).
			cj.angularXMotion = ConfigurableJointMotion.Limited;
			cj.angularYMotion = ConfigurableJointMotion.Limited;
			// Lock Z (twist) to keep motion in the two driven planes
			cj.angularZMotion = ConfigurableJointMotion.Locked;

			// Use range values directly to preserve any intentional asymmetry.
			cj.highAngularXLimit = new SoftJointLimit { limit =  axisRange.y };
			cj.lowAngularXLimit  = new SoftJointLimit { limit =  axisRange.x };
			// ConfigurableJoint angularYLimit is symmetric (no separate high/low);
			// use the larger absolute bound to capture the full secondaryAxisRange.
			cj.angularYLimit = new SoftJointLimit { limit = Mathf.Max(Mathf.Abs(secondaryAxisRange.x), Mathf.Abs(secondaryAxisRange.y)) };

			// Joint axes in child's local space (= world space at identity rotation spawn)
			cj.axis          = axis;
			cj.secondaryAxis = secondaryAxis;

			// Slerp drive: drives all three angular DOFs with a single drive setting.
			// JOINT_SPRING/JOINT_DAMPER/JOINT_MAX_FORCE are conservative values that hold
			// limbs against gravity without explosive behaviour in chains of <8 segments.
			cj.rotationDriveMode = RotationDriveMode.Slerp;
			cj.slerpDrive = new JointDrive
			{
				positionSpring = JOINT_SPRING,
				positionDamper = JOINT_DAMPER,
				maximumForce   = JOINT_MAX_FORCE
			};

			// Disable inter-segment collision and preprocessing instability
			cj.enableCollision    = false;
			cj.enablePreprocessing = false;

			return cj;
		}

		public void Connect(Segment parent, SideType side)
		{
			this.side = side;
			Init(parent);
			parent.AddSegment(this);
		}

		public void Sensing(List<float> sensors)
		{
			var sensor = Sensor;
			for (int i = 0, n = sensor.Length; i < n; i++)
			{
				sensors.Add(sensor[i]);
			}
			Segments.ForEach(segment => {
				segment.Sensing(sensors);
			});
		}

		public void WakeUp()
		{
			if (body != null) body.isKinematic = false;
			Segments.ForEach(segment => {
				segment.WakeUp();
			});
		}

		public void Sleep()
		{
			if (body != null) body.isKinematic = true;
			Segments.ForEach(segment => {
				segment.Sleep();
			});
		}

		void FixedUpdate()
		{
			// Lerp toward network target at ~5× fixedDeltaTime so joints track within
			// a fraction of a second rather than the original ~5 s lag.
			float lerpSpeed = Time.fixedDeltaTime * 5f;
			axisAngle = Mathf.Lerp(axisAngle, toAxisAngle, lerpSpeed);
			secondaryAxisAngle = Mathf.Lerp(secondaryAxisAngle, toSecondaryAxisAngle, lerpSpeed);
			Apply();
		}

		public void AddSegment(Segment segment)
		{
			segments.Add(segment);
		}

		public int InputCount()
		{
			return 2;
		}

		public void Affect(float[] inputs, float dt)
		{
			// Root segment has no joint; non-root segments drive their ConfigurableJoint.
			// Both cases accept the two network outputs (mapped to toAxisAngle /
			// toSecondaryAxisAngle) so the effector list in SampleCreature stays uniform.
			toAxisAngle = inputs[0];
			toSecondaryAxisAngle = inputs[1];
		}

		void Apply()
		{
			if (configurableJoint == null) return;

			float angleX = Mathf.Lerp(axisRange.x, axisRange.y, axisAngle);
			float angleY = Mathf.Lerp(secondaryAxisRange.x, secondaryAxisRange.y, secondaryAxisAngle);

			// Drive joint to desired angles.
			// targetRotation is expressed in joint-local space where X = joint.axis and
			// Y = joint.secondaryAxis.  The GA learns to map network outputs to this space
			// regardless of the sign convention (direct vs. inverse) used by ConfigurableJoint.
			configurableJoint.targetRotation =
				Quaternion.AngleAxis(angleX, Vector3.right) *
				Quaternion.AngleAxis(angleY, Vector3.up);
		}

		void OnDrawGizmosSelected()
		{
			if (anchor == null) return;
			Gizmos.color = Color.magenta;
			Gizmos.DrawWireSphere(anchor.position, 0.2f);
		}

	}

}