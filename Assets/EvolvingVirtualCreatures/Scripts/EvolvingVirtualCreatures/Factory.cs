using mattatz.GeneticAlgorithm;
using mattatz.NeuralNetworks;
using mattatz.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace mattatz.EvolvingVirtualCreatures
{

	public class Factory : MonoBehaviour
	{

		Population population;
		[SerializeField] Text generationLabel;

		[SerializeField] float size = 5f;
		[SerializeField] int count = 50;
		[SerializeField] Vector3 offset = Vector3.up;

		[SerializeField] GameObject scoreLabelPrefab;
		[SerializeField] GameObject segmentPrefab;

		[SerializeField, Range(0.0f, 1.0f)] float mutationRate = 0.2f;

		[SerializeField] float wps = 30f; // working per seconds

		[SerializeField] bool automatic = true;
		[SerializeField] float automaticInterval = 40f;
		Coroutine routine;
		bool stopping = false;

		Dictionary<Creature, TextMesh> scoreLabels = new Dictionary<Creature, TextMesh>();
		Node root;

		void Start()
		{
			// Ignore Creature-vs-Creature collisions (only works if that layer exists)
			int creatureLayer = LayerMask.NameToLayer("Creature");
			if (creatureLayer == -1)
			{
				Debug.LogError("Layer 'Creature' does not exist. Create it in Project Settings > Tags and Layers.");
			}
			else
			{
				Physics.IgnoreLayerCollision(creatureLayer, creatureLayer, true);
			}

			Begin();
		}

		public void Begin()
		{
			population = new Population(mutationRate);

			root = new Node(Vector3.one * 0.5f);
			var leftArm0 = new Node(new Vector3(0.8f, 0.2f, 0.2f));
			var leftArm1 = new Node(new Vector3(0.6f, 0.15f, 0.15f));
			var rightArm0 = new Node(new Vector3(0.8f, 0.2f, 0.2f));
			var rightArm1 = new Node(new Vector3(0.6f, 0.15f, 0.15f));

			root.Connect(leftArm0, SideType.Left);
			root.Connect(rightArm0, SideType.Right);

			leftArm0.Connect(leftArm1, SideType.Left);
			rightArm0.Connect(rightArm1, SideType.Right);

			// Spawn in a grid (prevents overlap)
			int cols = 8;
			float spacing = size;

			for (int i = 0; i < count; i++)
			{
				int x = i % cols;
				int z = i / cols;
				var position = new Vector3((x - cols * 0.5f) * spacing, 0f, z * spacing) + offset;

				SampleCreature creature = CreateCreature(i.ToString(), root, position);
				population.AddCreature(creature);
			}

			population.Setup();

			wps = Mathf.Max(10f, wps);

			float dt = 1f / wps;
			StartCoroutine(Repeat(dt, () => {
				if (!stopping)
				{
					population.Work(dt);
					population.ComputeFitness();
				}
			}));

			if (automatic)
			{
				Automation();
			}
		}

		void Update()
		{
			// safer enumeration than iterating Keys then indexing back into the dictionary
			foreach (var kvp in scoreLabels)
			{
				kvp.Value.text = kvp.Key.Fitness.ToString();
			}
		}

		IEnumerator Repeat(float interval, Action action)
		{
			yield return 0;
			while (true)
			{
				yield return new WaitForSeconds(interval);
				action();
			}
		}

		public void Reproduction()
		{
			population.ComputeFitness();

			var ancestors = new List<Creature>(population.Creatures);

			scoreLabels.Clear();

			// Respawn in same grid
			int cols = 8;
			float spacing = size;

			population.mutationRate = mutationRate;
			population.Reproduction((DNA dna, int index, int total) => {
				int x = index % cols;
				int z = index / cols;
				var position = new Vector3((x - cols * 0.5f) * spacing, 0f, z * spacing) + offset;
				return CreateCreature(index.ToString(), root, position, dna);
			}, new Vector2(-1f, 1f));

			ancestors.ForEach(ancestor => {
				var go = (ancestor as SampleCreature).Body.gameObject;
				Destroy(go);
			});

			population.Setup();
			generationLabel.text = population.Generations.ToString();
		}

		public void Automation()
		{
			if (routine != null) StopCoroutine(routine);
			routine = StartCoroutine(Repeat(automaticInterval, () => {
				if (!stopping) Reproduction();
			}));
		}

		public void Stop()
		{
			stopping = !stopping;
			if (stopping)
			{
				population.Creatures.ForEach(c => c.Sleep());
			}
			else
			{
				population.Creatures.ForEach(c => c.WakeUp());
			}
		}

		SampleCreature CreateCreature(string label, Node root, Vector3 position, DNA dna = null)
		{
			var body = Build(root);

			// Force all spawned segments onto Creature layer
			int creatureLayer = LayerMask.NameToLayer("Creature");
			if (creatureLayer != -1)
			{
				SetLayerRecursively(body.gameObject, creatureLayer);
			}

			body.transform.position = position;

			// Explicitly set the Rigidbody's position as well.
			// In Unity 2020+ Physics.autoSyncTransforms defaults to false, so
			// transform.position alone does not update the physics body that was
			// registered at the origin when AddComponent<Rigidbody>() was called.
			// Without this, the first physics-to-transform sync would snap every
			// creature back to (0,0,0), causing them all to pile up.
			var rb = body.GetComponent<Rigidbody>();
			if (rb != null)
			{
				rb.position = position;
				rb.linearVelocity = Vector3.zero;
				rb.angularVelocity = Vector3.zero;
			}

			SampleCreature creature;
			if (dna == null)
			{
				creature = new SampleCreature(body);
			}
			else
			{
				creature = new SampleCreature(body, dna);
			}

			var tm = Instantiate(scoreLabelPrefab).GetComponent<TextMesh>();
			tm.transform.parent = creature.Body.transform;
			tm.transform.localPosition = Vector3.zero;
			scoreLabels.Add(creature, tm);

			return creature;
		}

		static void SetLayerRecursively(GameObject go, int layer)
		{
			go.layer = layer;
			for (int i = 0; i < go.transform.childCount; i++)
			{
				SetLayerRecursively(go.transform.GetChild(i).gameObject, layer);
			}
		}

		public Segment Build(Node root)
		{
			var parent = Instantiate(segmentPrefab).GetComponent<Segment>();
			parent.SetRoot(true);
			parent.transform.localScale = root.Size;
			var cons = root.GetConnections();
			for (int i = 0, n = cons.Length; i < n; i++)
			{
				var con = cons[i];
				Build(parent, con.Side, con.To);
			}
			return parent;
		}

		void Build(Segment parentSegment, SideType side, Node childNode)
		{
			var cur = Instantiate(segmentPrefab).GetComponent<Segment>();
			cur.transform.localScale = childNode.Size;
			cur.Connect(parentSegment, side);
			var cons = childNode.GetConnections();
			for (int i = 0, n = cons.Length; i < n; i++)
			{
				var con = cons[i];
				Build(cur, con.Side, con.To);
			}
		}

		public void OnTimeScaleChanged(float scale)
		{
			Time.timeScale = scale;
		}

		void OnDrawGizmos()
		{
			if (!Application.isPlaying) return;
			population.Creatures.ForEach(c => c.DrawGizmos());
		}

	}

}
