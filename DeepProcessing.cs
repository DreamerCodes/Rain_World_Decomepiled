using RWCustom;
using UnityEngine;

public class DeepProcessing : UpdatableAndDeletable, IDrawable
{
	private PlacedObject placedObject;

	public Vector2[] quad;

	public Vector2[] verts;

	public bool meshDirty;

	private int gridDiv = 1;

	private float power = 1f;

	public DeepProcessing(PlacedObject placedObject)
	{
		this.placedObject = placedObject;
		quad = new Vector2[4];
		quad[0] = placedObject.pos;
		quad[1] = placedObject.pos + (placedObject.data as PlacedObject.QuadObjectData).handles[0];
		quad[2] = placedObject.pos + (placedObject.data as PlacedObject.QuadObjectData).handles[1];
		quad[3] = placedObject.pos + (placedObject.data as PlacedObject.QuadObjectData).handles[2];
		gridDiv = GetIdealGridDiv();
		meshDirty = true;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (quad[0] != placedObject.pos || quad[1] != placedObject.pos + (placedObject.data as PlacedObject.QuadObjectData).handles[0] || quad[2] != placedObject.pos + (placedObject.data as PlacedObject.QuadObjectData).handles[1] || quad[3] != placedObject.pos + (placedObject.data as PlacedObject.QuadObjectData).handles[2])
		{
			meshDirty = true;
		}
		if (Random.value < 1f / 14f)
		{
			if (power > room.ElectricPower)
			{
				power = Mathf.Max((Random.value < 0.2f) ? 0f : room.ElectricPower, power - 1f / Mathf.Lerp(1f, 4f, Random.value));
			}
			else if (power < room.ElectricPower)
			{
				power = Mathf.Min((Random.value < 0.2f) ? 1f : room.ElectricPower, power + 1f / Mathf.Lerp(1f, 4f, Random.value));
			}
		}
	}

	public int GetIdealGridDiv()
	{
		float num = 0f;
		for (int i = 0; i < 3; i++)
		{
			if (Vector2.Distance(quad[i], quad[i + 1]) > num)
			{
				num = Vector2.Distance(quad[i], quad[i + 1]);
			}
		}
		if (Vector2.Distance(quad[0], quad[3]) > num)
		{
			num = Vector2.Distance(quad[0], quad[3]);
		}
		return Mathf.Clamp(Mathf.RoundToInt(num / 150f), 1, 20);
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[1];
		TriangleMesh triangleMesh = TriangleMesh.MakeGridMesh("Futile_White", gridDiv);
		sLeaser.sprites[0] = triangleMesh;
		sLeaser.sprites[0].shader = rCam.room.game.rainWorld.Shaders["DeepProcessing"];
		verts = new Vector2[(sLeaser.sprites[0] as TriangleMesh).vertices.Length];
		AddToContainer(sLeaser, rCam, null);
		meshDirty = true;
	}

	private void UpdateVerts(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		quad[0] = placedObject.pos;
		quad[1] = placedObject.pos + (placedObject.data as PlacedObject.QuadObjectData).handles[0];
		quad[2] = placedObject.pos + (placedObject.data as PlacedObject.QuadObjectData).handles[1];
		quad[3] = placedObject.pos + (placedObject.data as PlacedObject.QuadObjectData).handles[2];
		int idealGridDiv = GetIdealGridDiv();
		if (idealGridDiv != gridDiv)
		{
			gridDiv = idealGridDiv;
			sLeaser.sprites[0].RemoveFromContainer();
			InitiateSprites(sLeaser, rCam);
		}
		for (int i = 0; i <= gridDiv; i++)
		{
			for (int j = 0; j <= gridDiv; j++)
			{
				Vector2 a = Vector2.Lerp(quad[0], quad[1], (float)j / (float)gridDiv);
				Vector2 b = Vector2.Lerp(quad[1], quad[2], (float)i / (float)gridDiv);
				Vector2 b2 = Vector2.Lerp(quad[3], quad[2], (float)j / (float)gridDiv);
				Vector2 a2 = Vector2.Lerp(quad[0], quad[3], (float)i / (float)gridDiv);
				verts[j * (gridDiv + 1) + i] = Custom.LineIntersection(a, b2, a2, b);
			}
		}
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		if (meshDirty)
		{
			UpdateVerts(sLeaser, rCam);
			meshDirty = false;
		}
		for (int i = 0; i < verts.Length; i++)
		{
			(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i, verts[i] - camPos);
		}
		sLeaser.sprites[0].color = new Color((placedObject.data as PlacedObject.DeepProcessingData).fromDepth, (placedObject.data as PlacedObject.DeepProcessingData).toDepth, power, (placedObject.data as PlacedObject.DeepProcessingData).intensity);
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		sLeaser.sprites[0].RemoveFromContainer();
		rCam.ReturnFContainer("Foreground").AddChild(sLeaser.sprites[0]);
	}
}
