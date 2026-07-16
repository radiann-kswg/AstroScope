using UnityEngine;

namespace AstroScope
{
	/// <summary>
	/// Generates a procedural starfield shell around the scene and rotates it slowly
	/// to give the impression of a living night sky.
	/// </summary>
	public class StarfieldRenderer : MonoBehaviour
	{
		private const float WarmStarChance = 0.22f;
		private const float BlueStarChance = 0.28f;

		[SerializeField]
		private int starCount = 1600;

		[SerializeField]
		private float minRadius = 45f;

		[SerializeField]
		private float maxRadius = 70f;

		[SerializeField]
		private float minStarSize = 0.08f;

		[SerializeField]
		private float maxStarSize = 0.34f;

		[SerializeField]
		private float rotationDegreesPerSecond = 0.25f;

		[SerializeField]
		private int randomSeed = 78;

		private void Start()
		{
			BuildStarMesh();
		}

		private void Update()
		{
			transform.Rotate(Vector3.up, rotationDegreesPerSecond * Time.deltaTime, Space.World);
		}

		/// <summary>
		/// Builds a single static mesh containing one small quad per star, tangent to the sky sphere.
		/// </summary>
		private void BuildStarMesh()
		{
			var random = new System.Random(randomSeed);

			var vertices = new Vector3[starCount * 4];
			var colors = new Color[starCount * 4];
			var uv = new Vector2[starCount * 4];
			var triangles = new int[starCount * 6];

			for (int i = 0; i < starCount; i++)
			{
				Vector3 direction = RandomOnUnitSphere(random);
				float radius = Mathf.Lerp(minRadius, maxRadius, (float)random.NextDouble());
				Vector3 center = direction * radius;

				// Build a tangent basis so the quad faces the scene center.
				Vector3 normal = -direction;
				Vector3 tangent = Vector3.Cross(normal, Vector3.up);
				if (tangent.sqrMagnitude < 0.001f)
				{
					tangent = Vector3.Cross(normal, Vector3.right);
				}

				tangent.Normalize();
				Vector3 bitangent = Vector3.Cross(normal, tangent);

				float size = Mathf.Lerp(minStarSize, maxStarSize, Mathf.Pow((float)random.NextDouble(), 2f));
				Color starColor = PickStarColor(random);

				int vertexIndex = i * 4;
				vertices[vertexIndex] = center + (-tangent - bitangent) * size;
				vertices[vertexIndex + 1] = center + (tangent - bitangent) * size;
				vertices[vertexIndex + 2] = center + (tangent + bitangent) * size;
				vertices[vertexIndex + 3] = center + (-tangent + bitangent) * size;

				uv[vertexIndex] = new Vector2(0f, 0f);
				uv[vertexIndex + 1] = new Vector2(1f, 0f);
				uv[vertexIndex + 2] = new Vector2(1f, 1f);
				uv[vertexIndex + 3] = new Vector2(0f, 1f);

				colors[vertexIndex] = starColor;
				colors[vertexIndex + 1] = starColor;
				colors[vertexIndex + 2] = starColor;
				colors[vertexIndex + 3] = starColor;

				int triangleIndex = i * 6;
				triangles[triangleIndex] = vertexIndex;
				triangles[triangleIndex + 1] = vertexIndex + 2;
				triangles[triangleIndex + 2] = vertexIndex + 1;
				triangles[triangleIndex + 3] = vertexIndex;
				triangles[triangleIndex + 4] = vertexIndex + 3;
				triangles[triangleIndex + 5] = vertexIndex + 2;
			}

			var mesh = new Mesh
			{
				name = "Starfield",
				indexFormat = UnityEngine.Rendering.IndexFormat.UInt32,
				vertices = vertices,
				colors = colors,
				uv = uv,
				triangles = triangles
			};
			mesh.RecalculateBounds();

			var meshFilter = gameObject.AddComponent<MeshFilter>();
			meshFilter.sharedMesh = mesh;

			var meshRenderer = gameObject.AddComponent<MeshRenderer>();
			meshRenderer.sharedMaterial = CosmicVisualUtility.CreateAdditiveMaterial(Color.white, true);
			meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			meshRenderer.receiveShadows = false;
		}

		private static Vector3 RandomOnUnitSphere(System.Random random)
		{
			// Marsaglia method for uniform sampling on a sphere.
			double x1;
			double x2;
			double squareSum;
			do
			{
				x1 = random.NextDouble() * 2.0 - 1.0;
				x2 = random.NextDouble() * 2.0 - 1.0;
				squareSum = x1 * x1 + x2 * x2;
			}
			while (squareSum >= 1.0);

			double factor = 2.0 * System.Math.Sqrt(1.0 - squareSum);
			return new Vector3(
				(float)(x1 * factor),
				(float)(x2 * factor),
				(float)(1.0 - 2.0 * squareSum));
		}

		private static Color PickStarColor(System.Random random)
		{
			double roll = random.NextDouble();
			float brightness = Mathf.Lerp(0.35f, 1f, (float)random.NextDouble());

			if (roll < WarmStarChance)
			{
				return new Color(1f, 0.85f, 0.65f, brightness);
			}

			if (roll < WarmStarChance + BlueStarChance)
			{
				return new Color(0.7f, 0.82f, 1f, brightness);
			}

			return new Color(1f, 1f, 1f, brightness);
		}
	}
}
