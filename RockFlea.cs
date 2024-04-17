using RWCustom;
using UnityEngine;

public class RockFlea : CosmeticInsect
{
	public Vector2? sitPos;

	public float stressed;

	public float bodyMove;

	public float lastBodyMove;

	public float size;

	public Vector2 rot;

	public Vector2 lastRot;

	public Vector2[,] legs;

	public float colorFac;

	private float swim;

	public RockFlea(Room room, Vector2 pos)
		: base(room, pos, Type.RockFlea)
	{
		creatureAvoider = new CreatureAvoider(this, 20, 300f, 0.2f);
		size = Custom.ClampedRandomVariation(0.75f, 0.25f, 0.5f);
		colorFac = Random.value;
		legs = new Vector2[2, 2];
	}

	public override void Update(bool eu)
	{
		if (room != null && room.PointDeferred(pos))
		{
			return;
		}
		lastBodyMove = bodyMove;
		lastRot = rot;
		base.Update(eu);
		if (!sitPos.HasValue && !submerged)
		{
			vel.y -= 0.8f;
		}
		if (submerged)
		{
			vel *= 0.9f;
			for (int i = 0; i < 2; i++)
			{
				legs[i, 1] = legs[i, 0];
				if (!sitPos.HasValue)
				{
					legs[i, 0] = Vector3.Slerp(legs[i, 0], Custom.DegToVec(Custom.VecToDeg(rot) + Mathf.Lerp(80f, 170f, swim) * (((float)i == 0f) ? (-1f) : 1f)), 0.8f);
				}
			}
			return;
		}
		for (int j = 0; j < 2; j++)
		{
			legs[j, 1] = legs[j, 0];
			if (!sitPos.HasValue)
			{
				legs[j, 0] = Vector3.Slerp(legs[j, 0], (-vel + Custom.PerpendicularVector(vel.normalized) * ((j == 0) ? (-1f) : 1f) * -5f + new Vector2((j == 0) ? (-1f) : 1f, 0.7f)).normalized, 0.2f);
			}
		}
	}

	public override void Reset(Vector2 resetPos)
	{
		base.Reset(resetPos);
		sitPos = null;
	}

	public override void Act()
	{
		base.Act();
		if (wantToBurrow)
		{
			if (sitPos.HasValue)
			{
				sitPos = null;
			}
			if (submerged)
			{
				rot = Vector3.Slerp(rot, new Vector2(0f, -1f), 0.3f);
			}
			return;
		}
		float num = Mathf.Pow(creatureAvoider.FleeSpeed, 0.3f);
		if (num > stressed)
		{
			stressed = Custom.LerpAndTick(stressed, num, 0.1f, 0.05f);
		}
		else
		{
			stressed = Custom.LerpAndTick(stressed, num, 0.02f, 0.005f);
		}
		if (submerged)
		{
			swim += 1f / Mathf.Lerp(40f, 10f, stressed);
			if (swim > 1f)
			{
				swim = 0f;
			}
			vel += rot * Mathf.Lerp(1.2f, 3f, stressed) * size * Mathf.Pow(swim, 2f - stressed);
			rot += Custom.RNV() * 0.2f;
			if (creatureAvoider.currentWorstCrit != null)
			{
				rot -= Custom.DirVec(pos, creatureAvoider.currentWorstCrit.DangerPos) * creatureAvoider.FleeSpeed;
			}
			if (pos.x < 0f)
			{
				rot.x += 1f;
			}
			else if (pos.x > room.PixelWidth)
			{
				rot.x -= 1f;
			}
			if (pos.y < 0f)
			{
				rot.y += 1f;
			}
			if (wantToBurrow)
			{
				rot.y -= 0.5f;
			}
			rot.Normalize();
			return;
		}
		bodyMove = ((bodyMove < 0.5f) ? Mathf.Lerp(0.5f, 1f, Random.value) : Mathf.Lerp(0f, 0.5f, Random.value));
		if (Random.value < 0.025f && room.GetTile(room.GetTilePosition(pos) + new IntVector2(-1, 0)).Solid && room.GetTile(room.GetTilePosition(pos) + new IntVector2(1, 0)).Solid)
		{
			stressed = 1f;
		}
		if (sitPos.HasValue)
		{
			pos = sitPos.Value;
			vel *= 0f;
			if (Random.value < 1f / Mathf.Lerp(400f, 15f, Mathf.Pow(stressed, 0.5f)) || (creatureAvoider.currentWorstCrit != null && Custom.DistLess(pos, creatureAvoider.currentWorstCrit.DangerPos, 30f)))
			{
				Jump();
			}
		}
		else
		{
			rot = vel.normalized;
		}
	}

	public void Jump()
	{
		if (sitPos.HasValue && !burrowPos.HasValue)
		{
			float num = Mathf.Pow(Random.value, 0.75f) * ((Random.value < 0.5f) ? (-1f) : 1f);
			if (base.OutOfBounds)
			{
				num = Mathf.Pow(Random.value, 0.75f) * ((mySwarm.placedObject.pos.x > pos.x) ? 1f : (-1f));
			}
			else if (creatureAvoider.currentWorstCrit != null)
			{
				num = Mathf.Pow(Random.value, 0.75f) * ((creatureAvoider.currentWorstCrit.DangerPos.x < pos.x) ? 1f : (-1f));
			}
			if (room.GetTile(room.GetTilePosition(sitPos.Value) + new IntVector2((int)Mathf.Sign(num), 0)).Solid)
			{
				num *= -1f;
			}
			Vector2 vector = Custom.DegToVec(Mathf.Lerp(-20f, 20f, Random.value) + Mathf.Lerp(30f, 60f, Random.value) * num);
			pos = sitPos.Value + vector * 2f;
			vel = vector * Mathf.Lerp(11f, 19f, Mathf.Pow(Random.value, Mathf.Lerp(1.2f, 0.6f, stressed))) * size;
			sitPos = null;
		}
	}

	public override void WallCollision(IntVector2 dir, bool first)
	{
		if (submerged)
		{
			rot -= dir.ToVector2();
			rot.Normalize();
		}
		else
		{
			if (sitPos.HasValue || dir.y >= 1 || wantToBurrow)
			{
				return;
			}
			sitPos = pos;
			Vector2 vector = Custom.RNV() * 0.1f;
			for (int i = 0; i < 8; i++)
			{
				if (room.GetTile(pos + Custom.eightDirections[i].ToVector2() * 20f).Solid)
				{
					vector += Custom.eightDirections[i].ToVector2().normalized;
				}
			}
			legs[0, 0] = (vector + new Vector2(-1f, 0.7f)).normalized;
			legs[1, 0] = (vector + new Vector2(1f, 0.7f)).normalized;
			rot = (Custom.PerpendicularVector(vector) * ((Random.value < 0.5f) ? (-1f) : 1f) + Custom.RNV() * 0.2f).normalized;
		}
	}

	public override void EmergeFromGround(Vector2 emergePos)
	{
		base.EmergeFromGround(emergePos);
		pos = emergePos;
		sitPos = emergePos + new Vector2(0f, 4f);
		Jump();
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		base.InitiateSprites(sLeaser, rCam);
		sLeaser.sprites = new FSprite[3];
		sLeaser.sprites[0] = new FSprite("Circle20");
		sLeaser.sprites[0].anchorY = 0.2f;
		for (int i = 0; i < 2; i++)
		{
			sLeaser.sprites[1 + i] = new FSprite("pixel");
			sLeaser.sprites[1 + i].anchorY = 0f;
		}
		AddToContainer(sLeaser, rCam, null);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		if (room == null || !room.PointDeferred(pos))
		{
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
			Vector2 vector = Vector2.Lerp(lastPos, pos, timeStacker);
			float num = Mathf.Lerp(lastInGround, inGround, timeStacker);
			Vector2 v = Vector3.Slerp(lastRot, rot, timeStacker);
			vector.y -= 5f * num * size;
			sLeaser.sprites[0].x = vector.x - camPos.x;
			sLeaser.sprites[0].y = vector.y - camPos.y;
			sLeaser.sprites[0].rotation = Custom.VecToDeg(v);
			sLeaser.sprites[0].scaleX = 4.5f * size * (1f - num) / 20f;
			sLeaser.sprites[0].scaleY = (6f + Mathf.Lerp(lastBodyMove, bodyMove, timeStacker) * 3f) * size * (1f - num) / 20f;
			for (int i = 0; i < 2; i++)
			{
				Vector2 v2 = Vector3.Slerp(legs[i, 1], legs[i, 0], timeStacker);
				sLeaser.sprites[1 + i].x = vector.x + Custom.PerpendicularVector(v2).x * ((i == 0) ? (-1f) : 1f) * 2f * size - camPos.x;
				sLeaser.sprites[1 + i].y = vector.y + Custom.PerpendicularVector(v2).y * ((i == 0) ? (-1f) : 1f) * 2f * size - camPos.y;
				sLeaser.sprites[1 + i].rotation = Custom.VecToDeg(v2);
				sLeaser.sprites[1 + i].scaleY = (submerged ? 7f : 5f) * size * (1f - num);
			}
		}
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		base.ApplyPalette(sLeaser, rCam, palette);
		Color color = Color.Lerp(palette.blackColor, palette.texture.GetPixel(6, 2), colorFac);
		sLeaser.sprites[0].color = color;
		sLeaser.sprites[1].color = color;
		sLeaser.sprites[2].color = color;
	}
}
