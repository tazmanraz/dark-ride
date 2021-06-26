

using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class MegaBindInf
{
	public float dist;
	public int face;
	public int i0;
	public int i1;
	public int i2;
	public Vector3 bary;
	public float weight;
	public float area;
	//public int[]	indices;
}

// we may need to allow multiple faces and have weights
[System.Serializable]
public class MegaBindVert
{
	public float weight;
	public List<MegaBindInf>	verts = new List<MegaBindInf>();
}

// attempt at a system to wrap a mesh on another for things such as hair, and clothes on a morphing body
[ExecuteInEditMode]
public class MegaWrap : MonoBehaviour
{
	Vector4 Plane(Vector3 v1, Vector3 v2, Vector3 v3)
	{
		Vector3 normal = Vector4.zero;
		normal.x = (v2.y - v1.y) * (v3.z - v1.z) - (v2.z - v1.z) * (v3.y - v1.y);
		normal.y = (v2.z - v1.z) * (v3.x - v1.x) - (v2.x - v1.x) * (v3.z - v1.z);
		normal.z = (v2.x - v1.x) * (v3.y - v1.y) - (v2.y - v1.y) * (v3.x - v1.x);

		normal = normal.normalized;
		return new Vector4(normal.x, normal.y, normal.z, -Vector3.Dot(v2, normal));
	}

	float PlaneDist(Vector3 p, Vector4 plane)
	{
		Vector3 n = plane;
		return Vector3.Dot(n, p) + plane.w;
	}


	float GetDistance(Vector3 p, Vector3 p0, Vector3 p1, Vector3 p2)
	{
		//Vector4 pl = Plane(p0, p1, p2);
		//return PlaneDist(p, pl);

		return MegaNearestPointTest.DistPoint3Triangle3Dbl(p, p0, p1, p2);
	}

	float GetPlaneDistance(Vector3 p, Vector3 p0, Vector3 p1, Vector3 p2)
	{
		Vector4 pl = Plane(p0, p1, p2);
		return PlaneDist(p, pl);

		//return MegaNearestPointTest.DistPoint3Triangle3Dbl(p, p0, p1, p2);
	}

	public Vector3 MyBary(Vector3 p, Vector3 p0, Vector3 p1, Vector3 p2)
	{
		Vector3 bary = Vector3.zero;

		Vector3 normal = FaceNormal(p0, p1, p2);

		// The area of a triangle is 
		float areaABC = Vector3.Dot(normal, Vector3.Cross((p1 - p0), (p2 - p0)));
		float areaPBC = Vector3.Dot(normal, Vector3.Cross((p1 - p), (p2 - p)));
		float areaPCA = Vector3.Dot(normal, Vector3.Cross((p2 - p), (p0 - p)));

		bary.x = areaPBC / areaABC; // alpha
		bary.y = areaPCA / areaABC; // beta
		bary.z = 1.0f - bary.x - bary.y; // gamma
		return bary;
	}

	public Vector3 MyBary1(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
	{
		Vector3 v0 = b - a, v1 = c - a, v2 = p - a;
		float d00 = Vector3.Dot(v0, v0);
		float d01 = Vector3.Dot(v0, v1);
		float d11 = Vector3.Dot(v1, v1);
		float d20 = Vector3.Dot(v2, v0);
		float d21 = Vector3.Dot(v2, v1);
		float denom = d00 * d11 - d01 * d01;

		float w = (d11 * d20 - d01 * d21) / denom;
		float v = (d00 * d21 - d01 * d20) / denom;
		float u = 1.0f - v - w;
		return new Vector3(u, v, w);
	}


	public Vector3 CalcBary(Vector3 p, Vector3 p0, Vector3 p1, Vector3 p2)
	{
		//Debug.Log("MyBary " + MyBary(p, p0, p1, p2));
		//Debug.Log("Bary " + MegaNearestPointTest.mTriangleBary + " oBary " + MegaNearestPointTest.oBary);
		//return MegaNearestPointTest.oBary;	//mTriangleBary;
		return MyBary(p, p0, p1, p2);
	}

	public float CalcArea(Vector3 p0, Vector3 p1, Vector3 p2)
	{
		Vector3 e1 = p1 - p0;
		Vector3 e2 = p2 - p0;
		Vector3 e3 = Vector3.Cross(e1, e2);
		return 0.5f * e3.magnitude;
	}

	public Vector3 FaceNormal(Vector3 p0, Vector3 p1, Vector3 p2)
	{
		Vector3 e1 = p1 - p0;
		Vector3 e2 = p2 - p0;
		return Vector3.Cross(e1, e2);
	}

	public Mesh mesh = null;


	Mesh CloneMesh(Mesh m)
	{
		Mesh clonemesh = new Mesh();
		clonemesh.vertices = m.vertices;
		clonemesh.uv1 = m.uv1;
		clonemesh.uv2 = m.uv2;
		clonemesh.uv = m.uv;
		clonemesh.normals = m.normals;
		clonemesh.tangents = m.tangents;
		clonemesh.colors = m.colors;

		clonemesh.subMeshCount = m.subMeshCount;

		for ( int s = 0; s < m.subMeshCount; s++ )
		{
			clonemesh.SetTriangles(m.GetTriangles(s), s);
		}

		//clonemesh.triangles = mesh.triangles;

		clonemesh.boneWeights = m.boneWeights;
		clonemesh.bindposes = m.bindposes;
		clonemesh.name = m.name + "_copy";
		clonemesh.RecalculateBounds();
		return clonemesh;
	}

	[ContextMenu("Reset Mesh")]
	public void ResetMesh()
	{
		if ( mesh )
		{
			mesh.vertices = startverts;
			mesh.RecalculateBounds();
			mesh.RecalculateNormals();
		}

		target = null;
	}

	public int nomapcount = 0;

#if false
	public void AttachOld(MegaModifyObject modobj)
	{
		if ( mesh && startverts != null )
			mesh.vertices = startverts;

		if ( modobj == null )
		{
			bindverts = null;
			return;
		}

		nomapcount = 0;

		if ( mesh )
			mesh.vertices = startverts;

		MeshFilter mf = GetComponent<MeshFilter>();

		if ( mesh == null )
			mesh = CloneMesh(mf.mesh);

		mf.mesh = mesh;

		//Mesh basemesh = MegaUtils.GetMesh(go);

		verts = mesh.vertices;
		startverts = mesh.vertices;
		freeverts = new Vector3[startverts.Length];
		Vector3[] baseverts = modobj.verts;	//basemesh.vertices;
		int[] basefaces = modobj.tris;	//basemesh.triangles;

		bindverts = new MegaBindVert[verts.Length];

		// matrix to get vertex into local space of target
		Matrix4x4 tm = transform.localToWorldMatrix * modobj.transform.worldToLocalMatrix;

		for ( int i = 0; i < verts.Length; i++ )
		{
			//Debug.Log("i " + i + " " + verts[i]);
			MegaBindVert bv = new MegaBindVert();
			bindverts[i] = bv;

			Vector3 p = tm.MultiplyPoint(verts[i]);
			//Debug.Log("p " + p);

			p = transform.TransformPoint(verts[i]);
			p = modobj.transform.InverseTransformPoint(p);
			freeverts[i] = p;
			//Debug.Log("p " + p);

			float tweight = 0.0f;
			for ( int t = 0; t < basefaces.Length; t += 3 )
			{
				Vector3 p0 = baseverts[basefaces[t]];
				Vector3 p1 = baseverts[basefaces[t + 1]];
				Vector3 p2 = baseverts[basefaces[t + 2]];
				//Debug.Log("Face " + p0 + " " + p1 + " " + p2);

				Vector3 normal = FaceNormal(p0, p1, p2);

				//float dot = Vector3.Dot(normal, p - p0);
#if true
				//Debug.Log("Dot " + dot);
				//if ( dot > 0.0f )
				{
					float dist = GetDistance(p, p0, p1, p2);

					//Debug.Log("Dist " + dist);
					//if ( dist >= 0.0f && dist < maxdist )
					if ( Mathf.Abs(dist) < maxdist )
					{
						MegaBindInf bi = new MegaBindInf();
						bi.dist = GetPlaneDistance(p, p0, p1, p2);	//dist;
						bi.face = t;
						bi.i0 = basefaces[t];
						bi.i1 = basefaces[t + 1];
						bi.i2 = basefaces[t + 2];
						bi.bary = CalcBary(p, p0, p1, p2);
						//Debug.Log("Bary " + bi.bary);
						bi.weight = 1.0f / (1.0f + dist);
						bi.area = normal.magnitude * 0.5f;	//CalcArea(baseverts[basefaces[t]], baseverts[basefaces[t + 1]], baseverts[basefaces[t + 2]]);	// Could calc once at start
						//Debug.Log("Weight " + bi.weight + " area " + bi.area);
						tweight += bi.weight;
						bv.verts.Add(bi);

						if ( maxpoints > 0 && bv.verts.Count >= maxpoints )
							break;
					}
				}
#endif
			}



			if ( tweight == 0.0f )
			{
				nomapcount++;
			}

			//Debug.Log("TWeight " + tweight);
			bv.weight = tweight;
		}
		// for each vert, find faces that are close enough and thatvert is above
		// find closest point on that face
		// calc bary coord and find distance from plane of face
		// calc area of face for weighting
		// keep track of all faces in range for combined weight
	}
#endif

	struct MegaCloseFace
	{
		public int face;
		public float dist;
	}

	public void Attach(MegaModifyObject modobj)
	{
		if ( mesh && startverts != null )
			mesh.vertices = startverts;

		if ( modobj == null )
		{
			bindverts = null;
			return;
		}

		nomapcount = 0;

		if ( mesh )
			mesh.vertices = startverts;

		MeshFilter mf = GetComponent<MeshFilter>();
		Mesh srcmesh = null;

		if ( mf != null )
		{
			skinned = false;
			srcmesh = mf.mesh;
		}
		else
		{
			SkinnedMeshRenderer smesh = (SkinnedMeshRenderer)GetComponent(typeof(SkinnedMeshRenderer));

			if ( smesh != null )
			{
				skinned = true;
				srcmesh = smesh.sharedMesh;
			}
		}

		if ( mesh == null )
			mesh = CloneMesh(srcmesh);	//mf.mesh);

		mf.mesh = mesh;

		//Mesh basemesh = MegaUtils.GetMesh(go);

		verts = mesh.vertices;
		startverts = mesh.vertices;
		freeverts = new Vector3[startverts.Length];
		Vector3[] baseverts = modobj.verts;	//basemesh.vertices;
		int[] basefaces = modobj.tris;	//basemesh.triangles;

		bindverts = new MegaBindVert[verts.Length];

		// matrix to get vertex into local space of target
		Matrix4x4 tm = transform.localToWorldMatrix * modobj.transform.worldToLocalMatrix;


		List<MegaCloseFace> closefaces = new List<MegaCloseFace>();

		for ( int i = 0; i < verts.Length; i++ )
		{
			//Debug.Log("i " + i + " " + verts[i]);
			MegaBindVert bv = new MegaBindVert();
			bindverts[i] = bv;

			Vector3 p = tm.MultiplyPoint(verts[i]);
			//Debug.Log("p " + p);

			p = transform.TransformPoint(verts[i]);
			p = modobj.transform.InverseTransformPoint(p);
			freeverts[i] = p;
			//Debug.Log("p " + p);

			closefaces.Clear();



			for ( int t = 0; t < basefaces.Length; t += 3 )
			{
				Vector3 p0 = baseverts[basefaces[t]];
				Vector3 p1 = baseverts[basefaces[t + 1]];
				Vector3 p2 = baseverts[basefaces[t + 2]];

				//Vector3 normal = FaceNormal(p0, p1, p2);

				float dist = GetDistance(p, p0, p1, p2);

				if ( Mathf.Abs(dist) < maxdist )
				{
					MegaCloseFace cf = new MegaCloseFace();
					cf.dist = Mathf.Abs(dist);
					cf.face = t;

					bool inserted = false;
					for ( int k = 0; k < closefaces.Count; k++ )
					{
						if ( cf.dist < closefaces[k].dist )
						{
							closefaces.Insert(k, cf);
							inserted = true;
							break;
						}
					}

					if ( !inserted )
					{
						closefaces.Add(cf);
					}
				}
			}


			float tweight = 0.0f;
			int maxp = maxpoints;
			if ( maxp == 0 )
			{
				maxp = closefaces.Count;
			}

			for ( int j = 0; j < maxp; j++ )
			{
				int t = closefaces[j].face;

				Vector3 p0 = baseverts[basefaces[t]];
				Vector3 p1 = baseverts[basefaces[t + 1]];
				Vector3 p2 = baseverts[basefaces[t + 2]];
				//Debug.Log("Face " + p0 + " " + p1 + " " + p2);

				Vector3 normal = FaceNormal(p0, p1, p2);

				//float dot = Vector3.Dot(normal, p - p0);
#if true
				//Debug.Log("Dot " + dot);
				//if ( dot > 0.0f )
				{
					float dist = closefaces[j].dist;	//GetDistance(p, p0, p1, p2);

					//Debug.Log("Dist " + dist);
					//if ( dist >= 0.0f && dist < maxdist )
					//if ( Mathf.Abs(dist) < maxdist )
					{
						MegaBindInf bi = new MegaBindInf();
						bi.dist = GetPlaneDistance(p, p0, p1, p2);	//dist;
						bi.face = t;
						bi.i0 = basefaces[t];
						bi.i1 = basefaces[t + 1];
						bi.i2 = basefaces[t + 2];
						bi.bary = CalcBary(p, p0, p1, p2);
						//Debug.Log("Bary " + bi.bary);
						bi.weight = 1.0f / (1.0f + dist);
						bi.area = normal.magnitude * 0.5f;	//CalcArea(baseverts[basefaces[t]], baseverts[basefaces[t + 1]], baseverts[basefaces[t + 2]]);	// Could calc once at start
						//Debug.Log("Weight " + bi.weight + " area " + bi.area);
						tweight += bi.weight;
						bv.verts.Add(bi);

						//if ( maxpoints > 0 && bv.verts.Count >= maxpoints )
						//	break;
					}
				}
#endif
			}

			if ( maxpoints > 0 )
			{
				bv.verts.RemoveRange(maxpoints, bv.verts.Count - maxpoints);
			}


			if ( tweight == 0.0f )
			{
				nomapcount++;
			}

			//Debug.Log("TWeight " + tweight);
			bv.weight = tweight;
		}
		// for each vert, find faces that are close enough and thatvert is above
		// find closest point on that face
		// calc bary coord and find distance from plane of face
		// calc area of face for weighting
		// keep track of all faces in range for combined weight
	}

	public float size = 0.01f;
	public int vertindex = 0;


	public Vector3[]	freeverts;	// position for any vert with no attachments
	public Vector3[]	startverts;
	public Vector3[]	verts;
	public MegaBindVert[]	bindverts;
	public MegaModifyObject	target;
	bool		skinned = false;
	public float	maxdist = 0.0f;
	public int		maxpoints = 4;

	void LateUpdate()
	{
		//mesh.vertices = target.verts;
		//mesh.RecalculateNormals();	// Need Mega method here
		//mesh.RecalculateBounds();
		DoUpdate();
	}

	void CalcSkinVerts()
	{

	}


	public Vector3 GetCoordMine(Vector3 A, Vector3 B, Vector3 C, Vector3 bary)
	{
		Vector3 p = Vector3.zero;
		p.x = (bary.x * A.x) + (bary.y * B.x) + (bary.z * C.x);
		p.y = (bary.x * A.y) + (bary.y * B.y) + (bary.z * C.y);
		p.z = (bary.x * A.z) + (bary.y * B.z) + (bary.z * C.z);

		return p;
	}

	// MegaModifyObject should call this update

	// Weight is 1 / (1 + dist)

	public Vector3 offset = Vector3.zero;

	// If the object is using the same bones as the mesh we dont need to worry about skin vert pos
	void DoUpdate()
	{
		if ( target == null || bindverts == null )
			return;

		// if target is a skinnedmesh then calc new verts now, if on 4.0 we can get them from Unity
		if ( skinned )
		{
			CalcSkinVerts();
		}

		//Matrix4x4 tm = target.transform.localToWorldMatrix * transform.worldToLocalMatrix;
		//Matrix4x4 tm = transform.worldToLocalMatrix * target.transform.localToWorldMatrix;
		//for ( int i = 0; i < 1; i++ )
		Vector3 p = Vector3.zero;
		for ( int i = 0; i < bindverts.Length; i++ )
		{
			if ( bindverts[i].verts.Count > 0 )
			{
				p = Vector3.zero;

				for ( int j = 0; j < bindverts[i].verts.Count; j++ )
				{
					MegaBindInf bi = bindverts[i].verts[j];

					Vector3 p0 = target.sverts[bi.i0];
					Vector3 p1 = target.sverts[bi.i1];
					Vector3 p2 = target.sverts[bi.i2];

					Vector3 cp = GetCoordMine(p0, p1, p2, bi.bary);

					// Now need to add on dist * facenormal
					Vector3 norm = FaceNormal(p0, p1, p2);
					cp += bi.dist * norm.normalized;

					p += cp * (bi.weight / bindverts[i].weight);
					// find delta from original 
				}
			}
			else
				p = freeverts[i];	//startverts[i];

			//p /= (float)bindverts[i].verts.Count;

			p = target.transform.TransformPoint(p);
			verts[i] = transform.InverseTransformPoint(p) + offset;

			//verts[i] = tm.MultiplyPoint(p);
			// for each vert calc the face normal from the face vertices
			// for each face that contributes (can set a limit)
			// calc face normal
			// calc face area for distance scaling
			// calc new pos from bary and normal
			// combine with all faces using weights
			// we have final position store and move on
		}

		mesh.vertices = verts;
		mesh.RecalculateNormals();	// Need Mega method here
		mesh.RecalculateBounds();
	}
}