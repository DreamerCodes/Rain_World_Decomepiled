using System;
using RWCustom;
using UnityEngine;

public class PuffBallSkin : CosmeticSprite
{
	public Vector2 rotation;

	public Vector2 lastRotation;

	public Vector2 randomDir;

	private Color color;

	private Color color2;

	public float lastLife;

	public float life;

	public float lifeTime;

	public float randomRotat;

	public float flip;

	public float lastFlip;

	public float flipDir;

	public PuffBallSkin(Vector2 pos, Vector2 vel, Color color, Color color2)
	{
		lastPos = pos;
		base.vel = vel;
		this.color = color;
		this.color2 = color2;
		base.pos = pos + vel.normalized * 60f * UnityEngine.Random.value;
		rotation = Custom.RNV();
		lastRotation = Custom.RNV();
		randomDir = Custom.RNV();
		lastLife = 1f;
		life = 1f;
		lifeTime = Mathf.Lerp(140f, 160f, UnityEngine.Random.value);
		randomRotat = UnityEngine.Random.value * 360f;
		flipDir = ((UnityEngine.Random.value < 0.5f) ? (-1f) : 1f);
	}

	public override void Update(bool eu)
	{
		vel *= Custom.LerpMap(vel.magnitude, 1f, 10f, 0.999f, 0.8f);
		vel.y -= 0.1f;
		vel += randomDir * 0.17f;
		randomDir = (randomDir + Custom.RNV() * 0.8f).normalized;
		lastRotation = rotation;
		rotation = (lastRotation + Custom.DirVec(pos, lastPos) * 0.3f).normalized;
		lastFlip = flip;
		flip += flipDir / Mathf.Lerp(6f, 80f, UnityEngine.Random.value);
		lastLife = life;
		life -= 1f / lifeTime;
		if (room.GetTile(pos).Solid)
		{
			life -= 0.05f;
		}
		if (lastLife <= 0f)
		{
			Destroy();
		}
		base.Update(eu);
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[2];
		for (int i = 0; i < 2; i++)
		{
			sLeaser.sprites[i] = new FSprite("Cicada" + UnityEngine.Random.Range(2, 6) + "shield");
			sLeaser.sprites[i].scaleX = ((UnityEngine.Random.value < 0.5f) ? (-1f) : 1f) * Mathf.Lerp(0.5f, 1f, UnityEngine.Random.value);
			sLeaser.sprites[i].scaleY = ((UnityEngine.Random.value < 0.5f) ? (-1f) : 1f) * 1.2f;
		}
		AddToContainer(sLeaser, rCam, null);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Vector2 vector = Vector2.Lerp(lastPos, pos, timeStacker);
		sLeaser.sprites[0].x = vector.x - camPos.x;
		sLeaser.sprites[0].y = vector.y - camPos.y;
		sLeaser.sprites[0].rotation = Custom.VecToDeg(Vector3.Slerp(lastRotation, rotation, timeStacker)) + randomRotat;
		sLeaser.sprites[0].scale = 0.7f * Mathf.InverseLerp(0f, 0.3f, Mathf.Lerp(lastLife, life, timeStacker));
		float num = Mathf.Sin(Mathf.Lerp(lastFlip, flip, timeStacker) * (float)Math.PI * 2f);
		sLeaser.sprites[0].scaleX = num;
		sLeaser.sprites[0].color = ((num > 0f) ? color : color2);
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		base.AddToContainer(sLeaser, rCam, newContatiner);
	}
}
