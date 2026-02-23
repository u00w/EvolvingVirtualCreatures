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

		// Time-averaged fitness: accumulated over all ticks since last Setup().
		// Using a running average ensures selection reflects sustained performance
		// rather than the creature's state at one arbitrary snapshot moment.
		int fitnessSteps = 0;
		float fitnessSum = 0f;

		public SampleCreature(Segment body)
		{
			this.body = body;

			// Capture spawn transform immediately so each creature keeps its own origin/target
			origin = body.transform.position;
			forward = body.transform.forward;
			target = origin + forward * distance;

			// Gravity sensor first — tells the network world-up in body-local space.
			// Added before GetAllSegments so its 3 outputs come first in the input vector.
			sensors.Add(new GravitySensor(body.transform));
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

			// Gravity sensor first — tells the network world-up in body-local space.
			// Added before GetAllSegments so its 3 outputs come first in the input vector.
			sensors.Add(new GravitySensor(body.transform));
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

			// Reset time-average accumulators for the new generation
			fitnessSteps = 0;
			fitnessSum = 0f;
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
			const float baseline = 0.001f;

			// Compute instantaneous fitness for this tick
			float instant = baseline;

			// --- Standing reward ---
			// upDot: 1 when body's up-axis aligns with world-up (upright), 0 sideways, -1 inverted.
			// standingFactor smoothly scales from 0 (fallen/sideways) to 1 (perfectly upright).
			// It is reused as a gate on the walking reward so that rolling or slithering
			// creatures gain nothing from forward displacement.
			float upDot = Vector3.Dot(body.transform.up.normalized, Vector3.up);
			float standingFactor = Mathf.Clamp01(upDot);
			const float standingRewardScale = 2.0f;
			instant += standingFactor * standingRewardScale;

			// --- Height reward ---
			// Reward the body staying near its spawn height (i.e., not collapsed onto the floor).
			// origin.y is the Y coordinate where the creature was placed, typically 1.0.
			// Guard against non-positive spawn heights (e.g., a scene with ground at y=0)
			// by falling back to an absolute threshold in that case.
			float heightRatio;
			if (origin.y > 0.1f)
			{
				heightRatio = Mathf.Clamp01(body.transform.position.y / origin.y);
			}
			else
			{
				// Spawn height is near or below zero; reward being above the floor instead.
				heightRatio = Mathf.Clamp01(body.transform.position.y / 1.0f);
			}
			const float heightRewardScale = 0.5f;
			instant += heightRatio * heightRewardScale;

			// --- Walking reward (forward progress gated by standing) ---
			// A creature that rolls or crawls (standingFactor ≈ 0) earns no forward bonus,
			// so the GA is forced to select for upright gaits.
			Vector3 delta = body.transform.position - origin;
			float forwardProgress = Vector3.Dot(delta, forward.normalized); // negative = moving backward
			float forwardReward = Mathf.Max(0f, forwardProgress);
			const float walkRewardScale = 2.0f;
			instant += forwardReward * standingFactor * walkRewardScale;

			// --- Tip penalty ---
			// Penalize leaning more than ~60 degrees from vertical (upDot < 0.5).
			// Threshold is more lenient than before to allow natural swaying,
			// but the penalty scale is stronger to discourage persistent tipping.
			const float tipThreshold = 0.5f;
			const float tipPenaltyScale = 2.0f;
			if (upDot < tipThreshold)
			{
				float t = Mathf.InverseLerp(tipThreshold, -1f, upDot); // 0..1
				instant -= t * tipPenaltyScale;
			}

			// --- Fall penalty ---
			const float minY = -0.25f;
			const float fallPenalty = 10f;
			if (body.transform.position.y < minY) instant -= fallPenalty;

			instant = Mathf.Max(baseline, instant);

			// Time-average: accumulate over every tick since Setup().
			// Selection reads creature.Fitness at the end of the generation, so using
			// the running average rewards creatures that stand/walk consistently, not
			// those that happen to be upright at one lucky snapshot moment.
			fitnessSum += instant;
			fitnessSteps++;
			fitness = fitnessSum / fitnessSteps;

			// Safety clamps
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

			// Two hidden layers of fixed width 32.
			// The previous design set hiddenLayer = inputLayer (45→45→45→45→45→10 = 8550 params).
			// Reducing to 48→32→32→10 = 2880 params cuts the GA search space by 3×,
			// dramatically improving how fast the population converges.
			const int hiddenDepth = 2;
			const int hiddenLayer = 32;

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

