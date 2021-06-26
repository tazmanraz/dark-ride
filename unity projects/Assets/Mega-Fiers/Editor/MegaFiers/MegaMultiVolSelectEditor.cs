using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MegaMultiVolSelect))]
public class MegaMultiVolSelectEditor : MegaModifierEditor
{
	public override string GetHelpString() { return "Multi Vol Select Modifier by Chris West"; }
	//public override Texture LoadImage() { return (Texture)EditorGUIUtility.LoadRequired("MegaFiers\\bend_help.png"); }

	public override bool DisplayCommon() { return false; }

	public override bool Inspector()
	{
		MegaMultiVolSelect mod = (MegaMultiVolSelect)target;

		EditorGUIUtility.LookLikeControls();
		mod.ModEnabled = EditorGUILayout.Toggle("Enabled", mod.ModEnabled);
		mod.freezeSelection = EditorGUILayout.Toggle("Freeze Selection", mod.freezeSelection);
		mod.useCurrentVerts = EditorGUILayout.Toggle("Use Stack Verts", mod.useCurrentVerts);

		mod.displayWeights = EditorGUILayout.Toggle("Show Weights", mod.displayWeights);
		mod.gizCol = EditorGUILayout.ColorField("Gizmo Col", mod.gizCol);
		mod.gizSize = EditorGUILayout.FloatField("Gizmo Size", mod.gizSize);

		if ( GUILayout.Button("Add Volume") )
		{
			mod.volumes.Add(MegaVolume.Create());
		}

		for ( int v = 0; v < mod.volumes.Count; v++ )
		{
			MegaVolume vol = mod.volumes[v];

			vol.volType = (MegaVolumeType)EditorGUILayout.EnumPopup("Type", vol.volType);

			if ( vol.volType == MegaVolumeType.Sphere )
			{
				vol.radius = EditorGUILayout.FloatField("Radius", vol.radius);
			}
			else
			{
				vol.boxsize = EditorGUILayout.Vector3Field("Size", vol.boxsize);
			}

			vol.weight = EditorGUILayout.Slider("Weight", vol.weight, 0.0f, 1.0f);
			vol.falloff = EditorGUILayout.FloatField("Falloff", vol.falloff);
			vol.origin = EditorGUILayout.Vector3Field("Origin", vol.origin);
			vol.target = (Transform)EditorGUILayout.ObjectField("Target", vol.target, typeof(Transform), true);

			if ( GUILayout.Button("Delete Volume") )
			{
				mod.volumes.RemoveAt(v);
				v--;
			}
		}

		return false;
	}

	// option to use base verts or current stack verts for distance calc
	// flag to display weights
	// size of weights and color of gizmo

	public override void DrawSceneGUI()
	{
		MegaMultiVolSelect mod = (MegaMultiVolSelect)target;

		if ( !mod.ModEnabled )
			return;

		MegaModifiers mc = mod.gameObject.GetComponent<MegaModifiers>();

		float[] sel = mod.GetSel();

		if ( mc != null && sel != null )
		{
			//Color col = Color.black;

			Matrix4x4 tm = mod.gameObject.transform.localToWorldMatrix;
			Handles.matrix = tm;	//Matrix4x4.identity;

			if ( mod.displayWeights )
			{
				for ( int i = 0; i < sel.Length; i++ )
				{
					float w = sel[i];
					if ( w > 0.001f )
					{
						if ( w > 0.5f )
							Handles.color = Color.Lerp(Color.green, Color.red, (w - 0.5f) * 2.0f);
						else
							Handles.color = Color.Lerp(Color.blue, Color.green, w * 2.0f);

						Handles.DotCap(i, mc.sverts[i], Quaternion.identity, mod.gizSize);
					}
				}
			}

			for ( int v = 0; v < mod.volumes.Count; v++ )
			{
				MegaVolume vol = mod.volumes[v];

				Handles.color = mod.gizCol;	//new Color(0.5f, 0.5f, 0.5f, 0.2f);

				// Draw box if box type
				if ( vol.volType == MegaVolumeType.Sphere )
				{
					//Handles.SphereCap(0, tm.MultiplyPoint(vol.origin), Quaternion.identity, vol.radius * 2.0f);
					Handles.SphereCap(0, vol.origin, Quaternion.identity, vol.radius * 2.0f);
				}
				//else
				//	Handles.CubeCap(.DrawCube(0, tm.MultiplyPoint(vol.origin), Quaternion.identity, vol.radius * 2.0f);

				//Handles.matrix = tm;
				vol.origin = Handles.PositionHandle(vol.origin, Quaternion.identity);
				Handles.matrix = Matrix4x4.identity;
			}
		}
	}
}