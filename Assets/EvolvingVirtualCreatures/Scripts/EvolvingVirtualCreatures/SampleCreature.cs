////using UnityEngine;
////using Random = UnityEngine.Random;

////using System;
////using System.Linq;
////using System.Collections;
////using System.Collections.Generic;

////using mattatz.GeneticAlgorithm;
////using mattatz.NeuralNetworks;
////using Network = mattatz.NeuralNetworks.Network;

////namespace mattatz.EvolvingVirtualCreatures {

////	public class SampleCreature : Creature {

////		public Segment Body { get { return body; } }

////		protected Network network;

////		protected Segment body;
////		protected List<Sensor> sensors = new List<Sensor>();
////		protected List<Effector> effectors = new List<Effector>();

////		protected Vector3 target;
////		protected float distance = 50f;
////		protected Vector3 origin;
////		protected Vector3 forward;

////		public SampleCreature (Segment body) {
////			this.target = body.transform.position + body.transform.forward * distance;

////			GetAllSegments(body);

////			this.body = body;
////			this.dna = new DNA(GetGenesCount(), new Vector2(-1f, 1f));
////			network = new Network(GetLayersCount());
////		}

////		public SampleCreature (Segment body, DNA dna) {
////			this.target = body.transform.position + body.transform.forward * distance;

////			GetAllSegments(body);

////			this.body = body;
////			this.dna = dna;
////			network = new Network(GetLayersCount(), this.dna.genes);
////		}

////		public override void Setup() {
////			base.Setup();

////			origin = body.transform.position;
////			forward = body.transform.forward;

////			//new
////			// Recompute target every time the creature is (re)setup
////			target = origin + forward * distance;
////			//new
////		}

////		//public override Creature Generate(DNA dna) {
////		//	body.transform.position = origin;
////		//	body.transform.localRotation = Quaternion.identity;
////		//	body.Init();
////		//	return new SampleCreature(body, dna);
////		//}
////		public override Creature Generate(DNA dna)
////		{
////			// Factory/CreateCreature already built and positioned a fresh body for this creature.
////			// So do NOT reset transforms here.
////			body.Init();
////			return new SampleCreature(body, dna);
////		}

////		public override void Work (float dt) {

////			// Sensing
////			var inputs = new List<float>();
////			sensors.ForEach(sensor => {
////				inputs.AddRange(sensor.Output());
////			});

////			float[] output = network.FeedForward(inputs.ToArray());

////			int offset = 0;
////			effectors.ForEach(effector => {
////				int inputCount = effector.InputCount();
////				float[] input = new float[inputCount];
////				Array.Copy(output, offset, input, 0, inputCount);
////				effector.Affect(input, dt);
////				offset += inputCount;
////			});

////		}

////		//OG
////		//public override float ComputeFitness () {

////		//	var d = distance / (body.transform.position - target).magnitude;
////		//	if(d <= 1f) {
////		//		fitness = 0f;
////		//	} else {
////		//		fitness = Mathf.Pow (d, 2f);
////		//	}


////		//	//float magnitude = 1f;

////		//	//fitness = (body.transform.position - origin, forward).magnitude;

////		//	//Extra from here

////		//	// ----- Upright penalty -----
////		//	// 1 when perfectly upright, 0 when sideways, -1 when upside down.
////		//	float upDot = Vector3.Dot(body.transform.up.normalized, Vector3.up);

////		//	// If you want to ignore small leans, allow a tolerance:
////		//	const float uprightDot = 0.7f; // ~45 degrees from upright
////		//	const float tipPenaltyScale = 5f;

////		//	float tipPenalty = 0f;
////		//	if (upDot < uprightDot)
////		//	{
////		//		// Normalize to 0..1 where 0 = at threshold, 1 = fully upside down/sideways depending on upDot
////		//		float t = Mathf.InverseLerp(uprightDot, -1f, upDot); // upDot: uprightDot -> -1
////		//		tipPenalty = t * tipPenaltyScale;
////		//	}

////		//	// ----- Falling penalty -----
////		//	const float minY = -0.25f;   // set based on your ground height
////		//	const float fallPenalty = 50f;

////		//	float fallenPenalty = (body.transform.position.y < minY) ? fallPenalty : 0f;

////		//	// Combine
////		//	fitness = distReward - tipPenalty - fallenPenalty;

////		//	// Keep fitness non-negative (optional, but often helpful for selection logic)
////		//	fitness = Mathf.Max(0f, fitness);

////		//	// Extra above

////		//	return fitness;
////		//}


////		//01
////		//public override float ComputeFitness()
////		//{

////		//	// ---- Base reward: get closer to the target ----
////		//	float toTarget = (body.transform.position - target).magnitude;
////		//	toTarget = Mathf.Max(0.001f, toTarget); // avoid divide-by-zero

////		//	float d = distance / toTarget;

////		//	float distReward;
////		//	if (d <= 1f)
////		//	{
////		//		distReward = 0f;
////		//	}
////		//	else
////		//	{
////		//		distReward = Mathf.Pow(d, 2f);
////		//	}

////		//	// ----- Upright penalty -----
////		//	// 1 when perfectly upright, 0 when sideways, -1 when upside down.
////		//	float upDot = Vector3.Dot(body.transform.up.normalized, Vector3.up);

////		//	// Ignore small leans
////		//	const float uprightDot = 0.7f; // ~45 degrees from upright
////		//	const float tipPenaltyScale = 5f;

////		//	float tipPenalty = 0f;
////		//	if (upDot < uprightDot)
////		//	{
////		//		// 0 at threshold, 1 at fully upside down (upDot = -1)
////		//		float t = Mathf.InverseLerp(uprightDot, -1f, upDot);
////		//		tipPenalty = t * tipPenaltyScale;
////		//	}

////		//	// ----- Falling penalty -----
////		//	const float minY = -0.25f;   // set based on your ground height
////		//	const float fallPenalty = 50f;

////		//	float fallenPenalty = (body.transform.position.y < minY) ? fallPenalty : 0f;

////		//	// Combine
////		//	fitness = distReward - tipPenalty - fallenPenalty;

////		//	// Keep fitness non-negative (optional)
////		//	fitness = Mathf.Max(0f, fitness);

////		//	return fitness;
////		//}

////		//02
////		//public override float ComputeFitness()
////		//{

////		//	// Reward: get closer to the target than you were at the start
////		//	float toTarget = (body.transform.position - target).magnitude;
////		//	float progress = distance - toTarget;          // distance is start-to-target
////		//	float distReward = Mathf.Max(0f, progress);    // only reward moving closer

////		//	// Upright penalty
////		//	float upDot = Vector3.Dot(body.transform.up.normalized, Vector3.up);
////		//	const float uprightDot = 0.7f;
////		//	const float tipPenaltyScale = 1.0f;            // start smaller; increase later

////		//	float tipPenalty = 0f;
////		//	if (upDot < uprightDot)
////		//	{
////		//		float t = Mathf.InverseLerp(uprightDot, -1f, upDot);
////		//		tipPenalty = t * tipPenaltyScale;
////		//	}

////		//	// Falling penalty
////		//	const float minY = -0.25f;
////		//	const float fallPenalty = 5f;                  // start smaller; increase later
////		//	float fallenPenalty = (body.transform.position.y < minY) ? fallPenalty : 0f;

////		//	fitness = distReward - tipPenalty - fallenPenalty;
////		//	if (float.IsNaN(fitness) || float.IsInfinity(fitness)) fitness = 0f;
////		//	fitness = Mathf.Max(0f, fitness);
////		//	return fitness;
////		//}

////		//simple check WORKS
////		//public override float ComputeFitness(){

////		//	// Base: distance from where this creature started
////		//	float dist = (body.transform.position - origin).magnitude;

////		//	// Small baseline so early generations aren't all exactly 0
////		//	fitness = 0.001f + dist;

////		//	// Keep it sane
////		//	if (float.IsNaN(fitness) || float.IsInfinity(fitness)) fitness = 0f;

////		//	return fitness;
////		//}

////		// Fix Gradual
////		//public override float ComputeFitness()
////		//{

////		//	// Always keep a tiny baseline so sum-fitness never becomes 0
////		//	const float baseline = 0.001f;
////		//	//new alive bonus
////		//	const float aliveBonus = 0.05f;
////		//	fitness = baseline + aliveBonus;

////		//	// Reward: forward progress from origin (better than magnitude because it discourages sideways rolling)
////		//	Vector3 delta = body.transform.position - origin;
////		//	float forwardProgress = Vector3.Dot(delta, forward.normalized);   // can be negative
////		//	float forwardReward = Mathf.Max(0f, forwardProgress);             // only reward forward

////		//	// Penalty: tipped over
////		//	float upDot = Vector3.Dot(body.transform.up.normalized, Vector3.up); // 1 upright, 0 sideways, -1 upside down
////		//	const float uprightDot = 0.7f;
////		//	const float tipPenaltyScale = 0.5f; // start small; increase later
////		//	float tipPenalty = 0f;
////		//	if (upDot < uprightDot)
////		//	{
////		//		float t = Mathf.InverseLerp(uprightDot, -1f, upDot);
////		//		tipPenalty = t * tipPenaltyScale;
////		//	}

////		//	// Penalty: fell below ground threshold
////		//	const float minY = -0.25f;
////		//	const float fallPenalty = 2f; // start small; increase later
////		//	float fallenPenalty = (body.transform.position.y < minY) ? fallPenalty : 0f;

////		//	fitness = baseline + forwardReward - tipPenalty - fallenPenalty;

////		//	// Keep it valid
////		//	if (float.IsNaN(fitness) || float.IsInfinity(fitness)) fitness = 0f;

////		//	// Avoid all-zero again
////		//	fitness = Mathf.Max(baseline, fitness);

////		//	return fitness;
////		//}

////		public override float ComputeFitness()
////		{
////			// Baseline so GA never gets "all zero" fitness
////			const float baseline = 0.001f;

////			// Small constant to keep early generations selectable even if they don't move yet
////			const float aliveBonus = 0.1f;

////			fitness = baseline + aliveBonus;

////			// Reward: forward progress from origin
////			Vector3 delta = body.transform.position - origin;
////			float forwardProgress = Vector3.Dot(delta, forward.normalized); // can be negative
////			float forwardReward = Mathf.Max(0f, forwardProgress);           // only reward forward
////			fitness += forwardReward;

////			// Reward: being upright (broad, safe shaping reward)
////			float upDot = Vector3.Dot(body.transform.up.normalized, Vector3.up); // 1 upright, 0 sideways, -1 upside down
////			float upright01 = Mathf.InverseLerp(-1f, 1f, upDot);                 // 0..1
////			const float uprightRewardScale = 0.8f;
////			fitness += upright01 * uprightRewardScale;

////			// Penalty: tipped over past tolerance
////			const float uprightDot = 0.7f;
////			const float tipPenaltyScale = 1.0f; // start small; increase later
////			if (upDot < uprightDot)
////			{
////				float t = Mathf.InverseLerp(uprightDot, -1f, upDot); // 0..1
////				float tipPenalty = t * tipPenaltyScale;
////				fitness -= tipPenalty;
////			}

////			// Penalty: fell below ground threshold
////			const float minY = -0.25f;
////			const float fallPenalty = 8f; // start small; increase later
////			if (body.transform.position.y < minY) fitness -= fallPenalty;

////			// Final safety: keep finite and non-negative
////			if (float.IsNaN(fitness) || float.IsInfinity(fitness)) fitness = baseline;
////			fitness = Mathf.Max(baseline, fitness);

////			return fitness;
////		}

////		public override int GetGenesCount () {
////			var layersCount = GetLayersCount();
////			int count = 0;
////			for(int i = 0, n = layersCount.Length; i < n - 1; i++) {
////				count += layersCount[i] * layersCount[i + 1];
////			}
////			return count;
////		}

////		void GetAllSegments (Segment root) {
////			// sensors.Add(new JointSensor(root.transform));
////			sensors.Add(new AnchorAngleSensor(root));
////			sensors.Add(new PartContactsSensor(root));
////			// sensors.Add(new DirectionSensor(root.transform, target));

////			effectors.Add(root);

////			root.Segments.ForEach(child => {
////				GetAllSegments(child);
////			});
////		}

////		public int[] GetLayersCount () {
////			// target direction eular angles(x,y,z) and each segments rotation eular angles(x,y,z) and contact states(forward,back,right,left,up,down) for each faces
////			// var inputLayer = count * (3 + 6);
////			var inputLayer = sensors.Aggregate(0, (prod, next) => prod + next.OutputCount());

////			const int hiddenDepth = 4;
////			var hiddenLayer = inputLayer;

////			// (each segments - body) axis forces and swing axis forces
////			var outputLayer = effectors.Aggregate(0, (prod, next) => prod + next.InputCount());

////			int[] layersCount = new int[1 + hiddenDepth + 1];
////			layersCount[0] = inputLayer;
////			for(int i = 1; i < hiddenDepth + 1; i++) {
////				layersCount[i] = hiddenLayer;
////			}
////			layersCount[layersCount.Length - 1] = outputLayer;

////			return layersCount;
////		}

////		public override void WakeUp() {
////			body.WakeUp();
////		}

////		public override void Sleep() {
////			body.Sleep();
////		}

////		public override void Destroy () {
////			GameObject.Destroy(Body.gameObject);
////		}

////		public override void DrawGizmos () {
////			// body is a UnityEngine.Object; after Destroy(), it becomes "fake null".
////			// Accessing body.transform then throws MissingReferenceException.
////			if (body == null) return;

////			Gizmos.color = Color.magenta;
////			Gizmos.DrawLine(body.transform.position, target);
////			Gizmos.DrawWireSphere(target, 1f);
////		}

////	}

////}

//using mattatz.GeneticAlgorithm;
//using mattatz.NeuralNetworks;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using UnityEngine;
//using Network = mattatz.NeuralNetworks.Network;
//using Random = UnityEngine.Random;

//namespace mattatz.EvolvingVirtualCreatures
//{

//	public class SampleCreature : Creature
//	{

//		public Segment Body { get { return body; } }

//		protected Network network;

//		protected Segment body;
//		protected List<Sensor> sensors = new List<Sensor>();
//		protected List<Effector> effectors = new List<Effector>();

//		protected Vector3 target;
//		protected float distance = 50f;
//		protected Vector3 origin;
//		protected Vector3 forward;

//		public SampleCreature(Segment body)
//		{
//			// Assign body first so everything else is based on the correct instance
//			this.body = body;

//			// Capture spawn transform immediately (prevents shared/default origin issues)
//			origin = body.transform.position;
//			forward = body.transform.forward;
//			target = origin + forward * distance;

//			GetAllSegments(body);

//			this.dna = new DNA(GetGenesCount(), new Vector2(-1f, 1f));
//			network = new Network(GetLayersCount());
//		}

//		public SampleCreature(Segment body, DNA dna)
//		{
//			// Assign body first so everything else is based on the correct instance
//			this.body = body;

//			// Capture spawn transform immediately (prevents shared/default origin issues)
//			origin = body.transform.position;
//			forward = body.transform.forward;
//			target = origin + forward * distance;

//			GetAllSegments(body);

//			this.dna = dna;
//			network = new Network(GetLayersCount(), this.dna.genes);
//		}

//		public override void Setup()
//		{
//			base.Setup();

//			// Re-capture in case something moved the body before Setup (safe)
//			origin = body.transform.position;
//			forward = body.transform.forward;

//			// Recompute target every time the creature is (re)setup
//			target = origin + forward * distance;
//		}

//		//public override Creature Generate(DNA dna)
//		//{
//		//	// IMPORTANT:
//		//	// Factory/Population.Reproduction creates a NEW body per creature and positions it.
//		//	// So do NOT reset transforms here (doing so makes everyone snap to the same origin).
//		//	body.Init();
//		//	return new SampleCreature(body, dna);
//		//}

//		public override Creature Generate(DNA dna)
//		{
//			// Create a fresh GameObject hierarchy so each creature owns its own body.
//			// (Reusing the same `body` causes multiple creatures to stack/control one object.)
//			var newBodyGO = GameObject.Instantiate(body.gameObject);
//			var newBody = newBodyGO.GetComponent<Segment>();

//			// Spawn it where this creature started (or where it currently is—pick one)
//			newBody.transform.position = origin;
//			newBody.transform.localRotation = Quaternion.identity;

//			newBody.Init();
//			return new SampleCreature(newBody, dna);
//		}

//		public override void Work(float dt)
//		{

//			// Sensing
//			var inputs = new List<float>();
//			sensors.ForEach(sensor => {
//				inputs.AddRange(sensor.Output());
//			});

//			float[] output = network.FeedForward(inputs.ToArray());

//			int offset = 0;
//			effectors.ForEach(effector => {
//				int inputCount = effector.InputCount();
//				float[] input = new float[inputCount];
//				Array.Copy(output, offset, input, 0, inputCount);
//				effector.Affect(input, dt);
//				offset += inputCount;
//			});

//		}

//		public override float ComputeFitness()
//		{
//			// Baseline so GA never gets "all zero" fitness
//			const float baseline = 0.001f;

//			// Small constant to keep early generations selectable even if they don't move yet
//			const float aliveBonus = 0.1f;

//			fitness = baseline + aliveBonus;

//			// Reward: forward progress from origin
//			Vector3 delta = body.transform.position - origin;
//			float forwardProgress = Vector3.Dot(delta, forward.normalized); // can be negative
//			float forwardReward = Mathf.Max(0f, forwardProgress);           // only reward forward
//			fitness += forwardReward;

//			// Reward: being upright (broad, safe shaping reward)
//			float upDot = Vector3.Dot(body.transform.up.normalized, Vector3.up); // 1 upright, 0 sideways, -1 upside down
//			float upright01 = Mathf.InverseLerp(-1f, 1f, upDot);                 // 0..1
//			const float uprightRewardScale = 0.8f;
//			fitness += upright01 * uprightRewardScale;

//			// Penalty: tipped over past tolerance
//			const float uprightDot = 0.7f;
//			const float tipPenaltyScale = 1.0f; // moderate
//			if (upDot < uprightDot)
//			{
//				float t = Mathf.InverseLerp(uprightDot, -1f, upDot); // 0..1
//				float tipPenalty = t * tipPenaltyScale;
//				fitness -= tipPenalty;
//			}

//			// Penalty: fell below ground threshold
//			const float minY = -0.25f;
//			const float fallPenalty = 8f; // meaningful
//			if (body.transform.position.y < minY) fitness -= fallPenalty;

//			// Final safety: keep finite and non-negative
//			if (float.IsNaN(fitness) || float.IsInfinity(fitness)) fitness = baseline;
//			fitness = Mathf.Max(baseline, fitness);

//			return fitness;
//		}

//		public override int GetGenesCount()
//		{
//			var layersCount = GetLayersCount();
//			int count = 0;
//			for (int i = 0, n = layersCount.Length; i < n - 1; i++)
//			{
//				count += layersCount[i] * layersCount[i + 1];
//			}
//			return count;
//		}

//		void GetAllSegments(Segment root)
//		{
//			// sensors.Add(new JointSensor(root.transform));
//			sensors.Add(new AnchorAngleSensor(root));
//			sensors.Add(new PartContactsSensor(root));
//			// sensors.Add(new DirectionSensor(root.transform, target));

//			effectors.Add(root);

//			root.Segments.ForEach(child => {
//				GetAllSegments(child);
//			});
//		}

//		public int[] GetLayersCount()
//		{
//			// target direction eular angles(x,y,z) and each segments rotation eular angles(x,y,z) and contact states(forward,back,right,left,up,down) for each faces
//			// var inputLayer = count * (3 + 6);
//			var inputLayer = sensors.Aggregate(0, (prod, next) => prod + next.OutputCount());

//			const int hiddenDepth = 4;
//			var hiddenLayer = inputLayer;

//			// (each segments - body) axis forces and swing axis forces
//			var outputLayer = effectors.Aggregate(0, (prod, next) => prod + next.InputCount());

//			int[] layersCount = new int[1 + hiddenDepth + 1];
//			layersCount[0] = inputLayer;
//			for (int i = 1; i < hiddenDepth + 1; i++)
//			{
//				layersCount[i] = hiddenLayer;
//			}
//			layersCount[layersCount.Length - 1] = outputLayer;

//			return layersCount;
//		}

//		public override void WakeUp()
//		{
//			body.WakeUp();
//		}

//		public override void Sleep()
//		{
//			body.Sleep();
//		}

//		public override void Destroy()
//		{
//			GameObject.Destroy(Body.gameObject);
//		}

//		public override void DrawGizmos()
//		{
//			// body is a UnityEngine.Object; after Destroy(), it becomes "fake null".
//			// Accessing body.transform then throws MissingReferenceException.
//			if (body == null) return;

//			Gizmos.color = Color.magenta;
//			Gizmos.DrawLine(body.transform.position, target);
//			Gizmos.DrawWireSphere(target, 1f);
//		}

//	}

//}

using UnityEngine;
using Random = UnityEngine.Random;

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using mattatz.GeneticAlgorithm;
using mattatz.NeuralNetworks;
using Network = mattatz.NeuralNetworks.Network;

namespace mattatz.EvolvingVirtualCreatures
{

	public class SampleCreature : Creature
	{

		public Segment Body { get { return body; } }

		protected Network network;

		protected Segment body;
		protected List<Sensor> sensors = new List<Sensor>();
		protected List<Effector> effectors = new List<Effector>();

		protected Vector3 target;
		protected float distance = 50f;
		protected Vector3 origin;
		protected Vector3 forward;

		public SampleCreature(Segment body)
		{
			this.body = body;

			// Capture spawn transform immediately so each creature keeps its own origin/target
			origin = body.transform.position;
			forward = body.transform.forward;
			target = origin + forward * distance;

			GetAllSegments(body);

			this.dna = new DNA(GetGenesCount(), new Vector2(-1f, 1f));
			network = new Network(GetLayersCount());
		}

		public SampleCreature(Segment body, DNA dna)
		{
			this.body = body;

			// Capture spawn transform immediately so each creature keeps its own origin/target
			origin = body.transform.position;
			forward = body.transform.forward;
			target = origin + forward * distance;

			GetAllSegments(body);

			this.dna = dna;
			network = new Network(GetLayersCount(), this.dna.genes);
		}

		public override void Setup()
		{
			base.Setup();

			// Re-capture (safe) and recompute target each (re)setup
			origin = body.transform.position;
			forward = body.transform.forward;
			target = origin + forward * distance;
		}

		public override Creature Generate(DNA dna)
		{
			// IMPORTANT:
			// In this project, Factory.Reproduction() already creates a NEW body per creature via CreateCreature().
			// So we must NOT instantiate or teleport bodies here, and we must NOT reuse a shared body between instances.
			//
			// Returning a new SampleCreature with the same body is only safe if the GA framework never uses this
			// to create additional simultaneously-existing creatures. If it does, you must refactor reproduction.
			body.Init();
			return new SampleCreature(body, dna);
		}

		public override void Work(float dt)
		{

			// Sensing
			var inputs = new List<float>();
			sensors.ForEach(sensor => {
				inputs.AddRange(sensor.Output());
			});

			float[] output = network.FeedForward(inputs.ToArray());

			int offset = 0;
			effectors.ForEach(effector => {
				int inputCount = effector.InputCount();
				float[] input = new float[inputCount];
				Array.Copy(output, offset, input, 0, inputCount);
				effector.Affect(input, dt);
				offset += inputCount;
			});
		}

		public override float ComputeFitness()
		{
			// Baseline so GA never gets "all zero" fitness
			const float baseline = 0.001f;

			// Small constant to keep early generations selectable even if they don't move yet
			const float aliveBonus = 0.1f;

			fitness = baseline + aliveBonus;

			// Reward: forward progress from origin
			Vector3 delta = body.transform.position - origin;
			float forwardProgress = Vector3.Dot(delta, forward.normalized); // can be negative
			float forwardReward = Mathf.Max(0f, forwardProgress);           // only reward forward
			fitness += forwardReward;

			// Reward: being upright
			float upDot = Vector3.Dot(body.transform.up.normalized, Vector3.up); // 1 upright, 0 sideways, -1 upside down
			float upright01 = Mathf.InverseLerp(-1f, 1f, upDot);                 // 0..1
			const float uprightRewardScale = 0.8f;
			fitness += upright01 * uprightRewardScale;

			// Penalty: tipped over past tolerance
			const float uprightDot = 0.7f;
			const float tipPenaltyScale = 1.0f;
			if (upDot < uprightDot)
			{
				float t = Mathf.InverseLerp(uprightDot, -1f, upDot); // 0..1
				float tipPenalty = t * tipPenaltyScale;
				fitness -= tipPenalty;
			}

			// Penalty: fell below ground threshold
			const float minY = -0.25f;
			const float fallPenalty = 8f;
			if (body.transform.position.y < minY) fitness -= fallPenalty;

			// Final safety
			if (float.IsNaN(fitness) || float.IsInfinity(fitness)) fitness = baseline;
			fitness = Mathf.Max(baseline, fitness);

			return fitness;
		}

		public override int GetGenesCount()
		{
			var layersCount = GetLayersCount();
			int count = 0;
			for (int i = 0, n = layersCount.Length; i < n - 1; i++)
			{
				count += layersCount[i] * layersCount[i + 1];
			}
			return count;
		}

		void GetAllSegments(Segment root)
		{
			sensors.Add(new AnchorAngleSensor(root));
			sensors.Add(new PartContactsSensor(root));

			effectors.Add(root);

			root.Segments.ForEach(child => {
				GetAllSegments(child);
			});
		}

		public int[] GetLayersCount()
		{
			var inputLayer = sensors.Aggregate(0, (prod, next) => prod + next.OutputCount());

			const int hiddenDepth = 4;
			var hiddenLayer = inputLayer;

			var outputLayer = effectors.Aggregate(0, (prod, next) => prod + next.InputCount());

			int[] layersCount = new int[1 + hiddenDepth + 1];
			layersCount[0] = inputLayer;
			for (int i = 1; i < hiddenDepth + 1; i++)
			{
				layersCount[i] = hiddenLayer;
			}
			layersCount[layersCount.Length - 1] = outputLayer;

			return layersCount;
		}

		public override void WakeUp()
		{
			body.WakeUp();
		}

		public override void Sleep()
		{
			body.Sleep();
		}

		public override void Destroy()
		{
			GameObject.Destroy(Body.gameObject);
		}

		public override void DrawGizmos()
		{
			if (body == null) return;

			Gizmos.color = Color.magenta;
			Gizmos.DrawLine(body.transform.position, target);
			Gizmos.DrawWireSphere(target, 1f);
		}

	}

}

