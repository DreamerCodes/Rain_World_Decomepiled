using System;
using RWCustom;
using UnityEngine;

public class CentipedeGraphics : GraphicsModule
{
	public Centipede centipede;

	public float lightFlash;

	public LightSource lightSource;

	private int totSegs;

	private int totalSecondarySegments;

	private int wingPairs;

	private float darkness;

	private float lastDarkness;

	public float hue;

	public float saturation;

	private Color blackColor;

	public Vector2[,] bodyRotations;

	public Limb[,] legs;

	public float[] legLengths;

	public float[] wingLengths;

	public GenericBodyPart[,,] whiskers;

	public float walkCycle;

	public float bodyDir;

	public float lastBodyDir;

	public float wingFlapCycle;

	public float lastWingFlapCycle;

	public float wingSwimCycle;

	public float lastWingSwimCycle;

	public float defaultRotat;

	private float wingsFolded;

	private float lastWingsFolded;

	public ChunkDynamicSoundLoop soundLoop;

	public int TubeSprite => wingPairs;

	public int FirstLegSprite => wingPairs + 1 + 8 + totalSecondarySegments + totSegs;

	public int TotalSprites => 9 + totalSecondarySegments + totSegs * (centipede.AquaCenti ? 7 : 6) + wingPairs * 2;

	private Color ShellColor
	{
		get
		{
			if (centipede.abstractCreature.IsVoided())
			{
				return Color.Lerp(RainWorld.SaturatedGold, blackColor, darkness);
			}
			return Color.Lerp(Custom.HSL2RGB(hue, saturation, 0.5f), blackColor, darkness);
		}
	}

	private Color SecondaryShellColor
	{
		get
		{
			if (centipede.abstractCreature.IsVoided())
			{
				return Color.Lerp(RainWorld.SaturatedGold, blackColor, 0.3f + 0.7f * darkness);
			}
			return Color.Lerp(Custom.HSL2RGB(hue, saturation, 0.3f), blackColor, 0.3f + 0.7f * darkness);
		}
	}

	public int SecondarySegmentSprite(int s)
	{
		return wingPairs + 1 + s;
	}

	public int SegmentSprite(int s)
	{
		return wingPairs + 1 + totalSecondarySegments + s;
	}

	public int WhiskerSprite(int end, int side, int pos)
	{
		return wingPairs + 1 + totalSecondarySegments + totSegs + end * 4 + side * 2 + pos;
	}

	public int LegSprite(int segment, int side, int part)
	{
		return FirstLegSprite + segment * 4 + side * 2 + part;
	}

	public int ShellSprite(int s, int t)
	{
		return wingPairs + 1 + 8 + totalSecondarySegments + totSegs * 5 + s + totSegs * t;
	}

	public int WingSprite(int side, int wing)
	{
		if (side == 0)
		{
			return wing;
		}
		return wingPairs + 1 + 8 + totalSecondarySegments + totSegs * (centipede.AquaCenti ? 7 : 6) + wing;
	}

	public CentipedeGraphics(PhysicalObject ow)
		: base(ow, internalContainers: false)
	{
		cullRange = 200f + (ow as Centipede).size * 200f;
		centipede = ow as Centipede;
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(centipede.abstractCreature.ID.RandomSeed);
		totSegs = base.owner.bodyChunks.Length;
		totalSecondarySegments = base.owner.bodyChunks.Length - 1;
		defaultRotat = Mathf.Lerp(-5f, 5f, UnityEngine.Random.value);
		bodyRotations = new Vector2[3, 2];
		for (int i = 0; i < bodyRotations.GetLength(0); i++)
		{
			bodyRotations[i, 0] = Custom.DegToVec(defaultRotat);
			bodyRotations[i, 1] = Custom.DegToVec(defaultRotat);
		}
		if (centipede.Centiwing)
		{
			wingPairs = centipede.bodyChunks.Length;
			hue = Mathf.Lerp(0.28f, 0.38f, UnityEngine.Random.value);
			saturation = 0.5f;
			wingLengths = new float[totSegs];
			for (int j = 0; j < totSegs; j++)
			{
				float num = (float)j / (float)(totSegs - 1);
				float num2 = Mathf.Sin(Mathf.Pow(Mathf.InverseLerp(0.5f, 0f, num), 0.75f) * (float)Math.PI);
				num2 *= 1f - num;
				float num3 = Mathf.Sin(Mathf.Pow(Mathf.InverseLerp(1f, 0.5f, num), 0.75f) * (float)Math.PI);
				num3 *= num;
				num2 = 0.5f + 0.5f * num2;
				num3 = 0.5f + 0.5f * num3;
				wingLengths[j] = Mathf.Lerp(3f, Custom.LerpMap(centipede.size, 0.5f, 1f, 60f, 80f), Mathf.Max(num2, num3) - Mathf.Sin(num * (float)Math.PI) * 0.25f);
			}
		}
		else if (centipede.AquaCenti)
		{
			wingPairs = centipede.bodyChunks.Length;
			hue = Mathf.Lerp(0.5f, 0.6f, UnityEngine.Random.value);
			saturation = Mathf.Lerp(0.8f, 1f, UnityEngine.Random.value);
			wingLengths = new float[totSegs];
			for (int k = 0; k < totSegs; k++)
			{
				float num4 = (float)k / (float)(totSegs - 1);
				float num5 = Mathf.Sin(Mathf.Pow(Mathf.InverseLerp(0.4f, 0f, num4), 0.75f) * (float)Math.PI);
				num5 *= 1f - num4;
				float num6 = Mathf.Cos(Mathf.Pow(Mathf.InverseLerp(0.6f, 0.4f, num4), 0.75f) * (float)Math.PI);
				num6 *= num4;
				num5 = 0.5f + 0.5f * num5;
				num6 = 0.5f + 0.5f * num6;
				wingLengths[k] = Mathf.Lerp(3f, Custom.LerpMap(centipede.size, 0.5f, 1f, 100f, 130f), Mathf.Max(num5, num6)) * 0.75f;
				wingLengths[k] = Mathf.Clamp(wingLengths[k], 60f, 80f);
			}
		}
		else if (centipede.Red)
		{
			wingPairs = centipede.bodyChunks.Length;
			hue = Mathf.Lerp(-0.02f, 0.01f, UnityEngine.Random.value);
			saturation = 0.9f + 0.1f * UnityEngine.Random.value;
		}
		else if (ModManager.MSC && centipede.Small && centipede.abstractCreature.superSizeMe)
		{
			hue = Mathf.Lerp(0.28f, 0.38f, UnityEngine.Random.value);
			saturation = 0.85f;
		}
		else
		{
			hue = Mathf.Lerp(0.04f, 0.1f, UnityEngine.Random.value);
			saturation = 0.9f;
		}
		legs = new Limb[totSegs, 2];
		legLengths = new float[totSegs];
		for (int l = 0; l < totSegs; l++)
		{
			float num7 = (float)l / (float)(totSegs - 1);
			legLengths[l] = Mathf.Lerp(10f, 25f, Mathf.Sin(num7 * (float)Math.PI));
			legLengths[l] *= Mathf.Lerp(0.5f, 1.5f, centipede.size) * (centipede.Centiwing ? 0.65f : 1f);
			for (int m = 0; m < 2; m++)
			{
				legs[l, m] = new Limb(this, ow.bodyChunks[l], l * 2 + m, 2f, 0.5f, 0.9f, 7f, 0.8f);
			}
		}
		lastDarkness = -1f;
		whiskers = new GenericBodyPart[2, 2, 2];
		bodyParts = new BodyPart[totSegs * 2 + 8];
		int num8 = 0;
		for (int n = 0; n < totSegs; n++)
		{
			for (int num9 = 0; num9 < 2; num9++)
			{
				bodyParts[num8] = legs[n, num9];
				num8++;
			}
		}
		for (int num10 = 0; num10 < 2; num10++)
		{
			for (int num11 = 0; num11 < 2; num11++)
			{
				for (int num12 = 0; num12 < 2; num12++)
				{
					whiskers[num10, num11, num12] = new GenericBodyPart(this, 1f, 0.5f, 0.9f, (num10 == 0) ? base.owner.bodyChunks[0] : base.owner.bodyChunks[base.owner.bodyChunks.Length - 1]);
					bodyParts[num8] = whiskers[num10, num11, num12];
					num8++;
				}
			}
		}
		soundLoop = new ChunkDynamicSoundLoop(centipede.mainBodyChunk);
		UnityEngine.Random.state = state;
	}

	public override void Update()
	{
		base.Update();
		soundLoop.Update();
		if (centipede.shockCharge > 0f)
		{
			soundLoop.sound = SoundID.Centipede_Electric_Charge_LOOP;
			soundLoop.Volume = Mathf.InverseLerp(0f, 0.1f, centipede.shockCharge);
			soundLoop.Pitch = Mathf.Lerp(0.5f, 1.5f, centipede.shockCharge);
		}
		else if (centipede.dead)
		{
			soundLoop.Volume = 0f;
		}
		else if (centipede.flying)
		{
			soundLoop.sound = SoundID.Centiwing_Fly_LOOP;
			soundLoop.Volume = Mathf.Lerp(0.75f, 1f, centipede.size);
			soundLoop.Pitch = Mathf.InverseLerp(1.2f, 0.8f, centipede.size) * Custom.LerpMap(Vector2.Dot((centipede.moveToPos - centipede.mainBodyChunk.pos).normalized, new Vector2(0f, 1f)), 0f, 1f, 1f, 1.4f, 1.5f);
		}
		else if (!centipede.AquaCenti)
		{
			soundLoop.sound = SoundID.Centipede_Crawl_LOOP;
			soundLoop.Volume = ((!centipede.moving) ? 0f : (Mathf.InverseLerp(Vector2.Distance(centipede.mainBodyChunk.lastPos, centipede.mainBodyChunk.pos), 0.5f, 2f) * Mathf.Lerp(0.5f, 1f, centipede.size)));
			soundLoop.Pitch = Mathf.Lerp(1.2f, 0.8f, centipede.size);
		}
		else
		{
			soundLoop.sound = SoundID.Centipede_Crawl_LOOP;
			soundLoop.Volume = (centipede.moving ? (Mathf.InverseLerp(Vector2.Distance(centipede.mainBodyChunk.lastPos, centipede.mainBodyChunk.pos), 0.5f, 2f) * Mathf.Lerp(0.5f, 1f, centipede.size)) : 0f);
			soundLoop.Pitch = Mathf.Lerp(1.2f, 0.8f, centipede.size);
		}
		if (centipede.moving && centipede.Consious)
		{
			walkCycle += (centipede.bodyDirection ? (-1f) : 1f) / 10f;
		}
		lastWingFlapCycle = wingFlapCycle;
		lastWingSwimCycle = wingSwimCycle;
		if (centipede.Consious)
		{
			wingFlapCycle += Mathf.Pow(centipede.wingsStartedUp, 3f);
			if (centipede.Submersion > 0f)
			{
				wingSwimCycle = 0f;
			}
			else
			{
				wingSwimCycle = Mathf.Lerp(wingSwimCycle, 0f, 0.05f);
			}
		}
		else
		{
			wingSwimCycle = Mathf.Lerp(wingSwimCycle, 0f, 0.05f);
		}
		lastWingsFolded = wingsFolded;
		wingsFolded = 1f - centipede.wingsStartedUp;
		lastBodyDir = bodyDir;
		bodyDir = Mathf.Lerp(bodyDir, centipede.bodyDirection ? (-1f) : 1f, 0.1f);
		for (int i = 0; i < bodyRotations.GetLength(0); i++)
		{
			bodyRotations[i, 1] = bodyRotations[i, 0];
			int num = i switch
			{
				1 => base.owner.bodyChunks.Length / 2, 
				0 => 0, 
				_ => base.owner.bodyChunks.Length - 1, 
			};
			if (!centipede.flying || centipede.room.aimap.getTerrainProximity(centipede.bodyChunks[i].pos) < 2)
			{
				bodyRotations[i, 0] = Vector3.Slerp(bodyRotations[i, 0], BestBodyRotatAtChunk(num), centipede.moving ? 0.4f : 0.01f);
				continue;
			}
			Vector2 vector = new Vector2(0f, 0f);
			if (num > 0)
			{
				vector += Custom.DirVec(centipede.bodyChunks[num].pos, centipede.bodyChunks[num - 1].pos);
			}
			if (num < bodyRotations.GetLength(0) - 1)
			{
				vector -= Custom.DirVec(centipede.bodyChunks[num].pos, centipede.bodyChunks[num + 1].pos);
			}
			vector += new Vector2(0f, (num == centipede.HeadIndex) ? 0.8f : 0.1f);
			bodyRotations[i, 0] = Vector3.Slerp(bodyRotations[i, 0], vector.normalized, (num == centipede.HeadIndex) ? 0.5f : 0.1f);
		}
		Vector2 p = base.owner.bodyChunks[0].pos + Custom.DirVec(base.owner.bodyChunks[1].pos, base.owner.bodyChunks[0].pos);
		for (int j = 0; j < totSegs; j++)
		{
			float num2 = (float)j / (float)(totSegs - 1);
			Vector2 pos = base.owner.bodyChunks[j].pos;
			Vector2 vector2 = Custom.DirVec(p, pos);
			Vector2 vector3 = Custom.PerpendicularVector(vector2);
			Vector2 vector4 = RotatAtChunk(j, 1f);
			float num3 = 0.5f + 0.5f * Mathf.Sin((walkCycle + (float)j / 10f) * (float)Math.PI * 2f);
			for (int k = 0; k < 2; k++)
			{
				legs[j, k].Update();
				Vector2 vector5 = base.owner.bodyChunks[j].pos + vector3 * ((k == 0) ? (-1f) : 1f) * vector4.y * centipede.bodyChunks[j].rad;
				float a = Mathf.Lerp(-1f, 1f, num2);
				a = Mathf.Lerp(a, centipede.bodyDirection ? (-1f) : 1f, Mathf.Abs(num3 - 0.5f));
				Vector2 vector6 = Vector3.Slerp(vector2 * a, Vector3.Slerp(vector3 * ((k == 0) ? (-1f) : 1f) * vector4.y, vector3 * vector4.x, Mathf.Abs(vector4.x)), Mathf.Lerp(0.5f + 0.5f * Mathf.Sin(num2 * (float)Math.PI), 0f, Mathf.Abs(num3 - 0.5f) * 2f)).normalized;
				Vector2 vector7 = vector5 + vector6 * legLengths[j];
				legs[j, k].ConnectToPoint(vector5, legLengths[j], push: false, 0f, centipede.bodyChunks[j].vel, 0.1f, 0f);
				if (centipede.flying)
				{
					legs[j, k].mode = Limb.Mode.Dangle;
					legs[j, k].vel += vector6 * 6f;
					legs[j, k].vel = Vector2.Lerp(legs[j, k].vel, vector7 - legs[j, k].pos, 0.5f);
					continue;
				}
				if (centipede.Consious && !legs[j, k].reachedSnapPosition)
				{
					legs[j, k].FindGrip(centipede.room, vector5, vector5, legLengths[j] * 1.5f, vector7, -2, -2, behindWalls: true);
				}
				if (!centipede.Consious || !Custom.DistLess(legs[j, k].pos, legs[j, k].absoluteHuntPos, legLengths[j] * 1.5f))
				{
					legs[j, k].mode = Limb.Mode.Dangle;
					legs[j, k].vel += vector6 * 13f;
					legs[j, k].vel = Vector2.Lerp(legs[j, k].vel, vector7 - legs[j, k].pos, 0.5f);
				}
				else
				{
					legs[j, k].vel += vector6 * 5f;
				}
			}
		}
		for (int l = 0; l < 2; l++)
		{
			Vector2 vector8 = ((l != 0) ? base.owner.bodyChunks[base.owner.bodyChunks.Length - 1].pos : base.owner.bodyChunks[0].pos);
			for (int m = 0; m < 2; m++)
			{
				for (int n = 0; n < 2; n++)
				{
					whiskers[l, n, m].Update();
					whiskers[l, n, m].ConnectToPoint(vector8, WhiskerLength(n), push: false, 0f, new Vector2(0f, 0f), 0f, 0f);
					whiskers[l, n, m].vel += (vector8 + WhiskerDir(l, m, n, 1f) * WhiskerLength(n) - whiskers[l, n, m].pos) / 30f;
					whiskers[l, n, m].vel += WhiskerDir(l, m, n, 1f);
					whiskers[l, n, m].vel.y -= 0.3f;
					if (centipede.Consious && !centipede.moving)
					{
						whiskers[l, n, m].pos += Custom.RNV() * Mathf.Lerp(0.5f, 1.5f, centipede.size) * ((centipede.bodyDirection == (l == 0)) ? 2f : 0.8f);
					}
				}
			}
		}
		if (lightSource != null)
		{
			lightSource.stayAlive = true;
			lightSource.setPos = centipede.mainBodyChunk.pos;
			lightSource.setRad = 300f * Mathf.Pow(lightFlash * UnityEngine.Random.value, 0.1f) * Mathf.Lerp(0.5f, 2f, centipede.size);
			lightSource.setAlpha = Mathf.Pow(lightFlash * UnityEngine.Random.value, 0.1f);
			float a2 = lightFlash * UnityEngine.Random.value;
			a2 = Mathf.Lerp(a2, 1f, 0.5f * (1f - centipede.room.Darkness(centipede.mainBodyChunk.pos)));
			lightSource.color = new Color(a2, a2, 1f);
			if (lightFlash <= 0f)
			{
				lightSource.Destroy();
			}
			if (lightSource.slatedForDeletetion)
			{
				lightSource = null;
			}
		}
		else if (lightFlash > 0f)
		{
			lightSource = new LightSource(centipede.mainBodyChunk.pos, environmentalLight: false, new Color(1f, 1f, 1f), centipede);
			lightSource.affectedByPaletteDarkness = 0f;
			lightSource.requireUpKeep = true;
			centipede.room.AddObject(lightSource);
		}
		if (lightFlash > 0f)
		{
			lightFlash = Mathf.Max(0f, lightFlash - 1f / 30f);
		}
	}

	public Vector2 BestBodyRotatAtChunk(int chunk)
	{
		Vector2 v = ((chunk == 0) ? Custom.DirVec(centipede.bodyChunks[0].pos, centipede.bodyChunks[1].pos) : ((chunk != centipede.bodyChunks.Length - 1) ? Custom.DirVec(centipede.bodyChunks[centipede.bodyChunks.Length / 2 - 1].pos, centipede.bodyChunks[centipede.bodyChunks.Length / 2 + 1].pos) : Custom.DirVec(centipede.bodyChunks[centipede.bodyChunks.Length - 2].pos, centipede.bodyChunks[centipede.bodyChunks.Length - 1].pos)));
		v = Custom.PerpendicularVector(v);
		float num = 0f;
		if (centipede.room.GetTile(centipede.bodyChunks[chunk].pos + v * 20f).Solid)
		{
			num += 1f;
		}
		if (centipede.room.GetTile(centipede.bodyChunks[chunk].pos - v * 20f).Solid)
		{
			num -= 1f;
		}
		if (num == 0f && (centipede.room.GetTile(centipede.bodyChunks[chunk].pos).verticalBeam || centipede.room.GetTile(centipede.bodyChunks[chunk].pos).horizontalBeam))
		{
			return new Vector2(defaultRotat * 0.01f, -1f).normalized;
		}
		return Vector3.Slerp(new Vector2(-1f, 0.15f).normalized, new Vector2(1f, 0.15f).normalized, Mathf.InverseLerp(-1f, 1f, num + defaultRotat * 0.01f));
	}

	public Vector2 RotatAtChunk(int chunk, float timeStacker)
	{
		if (chunk <= centipede.bodyChunks.Length / 2)
		{
			return Vector3.Slerp(Vector3.Slerp(bodyRotations[0, 1], bodyRotations[0, 0], timeStacker), Vector3.Slerp(bodyRotations[1, 1], bodyRotations[1, 0], timeStacker), Mathf.InverseLerp(0f, centipede.bodyChunks.Length / 2, chunk));
		}
		return Vector3.Slerp(Vector3.Slerp(bodyRotations[1, 1], bodyRotations[1, 0], timeStacker), Vector3.Slerp(bodyRotations[2, 1], bodyRotations[2, 0], timeStacker), Mathf.InverseLerp(centipede.bodyChunks.Length / 2, centipede.bodyChunks.Length - 1, chunk));
	}

	public Vector2 WhiskerDir(int end, int side, int part, float timeStacker)
	{
		Vector2 vector;
		Vector2 vector2;
		if (end == 0)
		{
			vector = Custom.DirVec(Vector2.Lerp(base.owner.bodyChunks[1].lastPos, base.owner.bodyChunks[1].pos, timeStacker), Vector2.Lerp(base.owner.bodyChunks[0].lastPos, base.owner.bodyChunks[0].pos, timeStacker));
			vector2 = RotatAtChunk(0, timeStacker);
		}
		else
		{
			vector = Custom.DirVec(Vector2.Lerp(base.owner.bodyChunks[base.owner.bodyChunks.Length - 2].lastPos, base.owner.bodyChunks[base.owner.bodyChunks.Length - 2].pos, timeStacker), Vector2.Lerp(base.owner.bodyChunks[base.owner.bodyChunks.Length - 1].lastPos, base.owner.bodyChunks[base.owner.bodyChunks.Length - 1].pos, timeStacker));
			vector2 = RotatAtChunk(base.owner.bodyChunks.Length - 1, timeStacker);
		}
		Vector2 vector3 = Custom.PerpendicularVector(vector) * ((end == 0) ? (-1f) : 1f);
		return (vector + (Vector2)Vector3.Slerp(vector3 * ((side == 0) ? (-1f) : 1f) * vector2.y * ((part == 0) ? 0.4f : 1.4f), vector3 * vector2.x * ((part == 0) ? 0.25f : (-0.5f)), Mathf.Abs(vector2.x))).normalized;
	}

	public float WhiskerLength(int part)
	{
		if (centipede.AquaCenti)
		{
			return 72f;
		}
		if (centipede.Red)
		{
			if (part != 0)
			{
				return 48f;
			}
			return 44f;
		}
		return ((part == 0) ? 17f : 43f) * Mathf.Lerp(0.5f, 1.5f, centipede.size) * (centipede.Small ? 0.75f : 1f);
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[TotalSprites];
		sLeaser.sprites[TubeSprite] = TriangleMesh.MakeLongMesh(base.owner.bodyChunks.Length, pointyTip: false, customColor: false);
		for (int i = 0; i < base.owner.bodyChunks.Length; i++)
		{
			sLeaser.sprites[SegmentSprite(i)] = new FSprite("CentipedeSegment");
			sLeaser.sprites[SegmentSprite(i)].scaleY = base.owner.bodyChunks[i].rad * 1.8f * (1f / 12f);
			for (int j = 0; j < ((!centipede.AquaCenti) ? 1 : 2); j++)
			{
				sLeaser.sprites[ShellSprite(i, j)] = new FSprite("pixel");
				if (j == 1)
				{
					sLeaser.sprites[ShellSprite(i, j)].shader = rCam.room.game.rainWorld.Shaders["AquapedeBody"];
					sLeaser.sprites[ShellSprite(i, j)].alpha = UnityEngine.Random.value;
				}
			}
			for (int k = 0; k < 2; k++)
			{
				sLeaser.sprites[LegSprite(i, k, 0)] = new FSprite("CentipedeLegA");
				sLeaser.sprites[LegSprite(i, k, 1)] = new VertexColorSprite("CentipedeLegB");
			}
		}
		for (int l = 0; l < totalSecondarySegments; l++)
		{
			sLeaser.sprites[SecondarySegmentSprite(l)] = new FSprite("pixel");
			sLeaser.sprites[SecondarySegmentSprite(l)].scaleY = base.owner.bodyChunks[l].rad;
		}
		for (int m = 0; m < 2; m++)
		{
			for (int n = 0; n < 2; n++)
			{
				for (int num = 0; num < 2; num++)
				{
					sLeaser.sprites[WhiskerSprite(m, n, num)] = TriangleMesh.MakeLongMesh(4, pointyTip: true, customColor: false);
				}
			}
			if (!centipede.Centiwing && !centipede.AquaCenti)
			{
				continue;
			}
			for (int num2 = 0; num2 < wingPairs; num2++)
			{
				if (centipede.AquaCenti)
				{
					sLeaser.sprites[WingSprite(m, num2)] = new CustomFSprite("AquapedeWing" + (UnityEngine.Random.Range(0, 4) + 1));
				}
				else
				{
					sLeaser.sprites[WingSprite(m, num2)] = new CustomFSprite("CentipedeWing");
				}
				sLeaser.sprites[WingSprite(m, num2)].shader = rCam.room.game.rainWorld.Shaders["CicadaWing"];
			}
		}
		if (centipede.Red)
		{
			for (int num3 = 0; num3 < wingPairs; num3++)
			{
				sLeaser.sprites[WingSprite(1, num3)] = new FSprite("CentipedeSegment");
				sLeaser.sprites[WingSprite(0, num3)] = new FSprite("Cicada8body");
				sLeaser.sprites[WingSprite(0, num3)].anchorY = 0.55f;
			}
		}
		AddToContainer(sLeaser, rCam, null);
		base.InitiateSprites(sLeaser, rCam);
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		base.AddToContainer(sLeaser, rCam, newContatiner);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		if (culled)
		{
			return;
		}
		lastDarkness = darkness;
		darkness = rCam.room.Darkness(Vector2.Lerp(centipede.mainBodyChunk.lastPos, centipede.mainBodyChunk.pos, timeStacker));
		darkness *= 1f - 0.5f * rCam.room.LightSourceExposure(Vector2.Lerp(centipede.mainBodyChunk.lastPos, centipede.mainBodyChunk.pos, timeStacker));
		if (lastDarkness != darkness)
		{
			ApplyPalette(sLeaser, rCam, rCam.currentPalette);
		}
		Vector2 vector = new Vector2(0f, 0f);
		Vector2 vector2 = Vector2.Lerp(base.owner.bodyChunks[0].lastPos, base.owner.bodyChunks[0].pos, timeStacker);
		vector2 += Custom.DirVec(Vector2.Lerp(base.owner.bodyChunks[1].lastPos, base.owner.bodyChunks[1].pos, timeStacker), vector2) * 10f;
		for (int i = 0; i < base.owner.bodyChunks.Length; i++)
		{
			for (int j = 0; j < ((!centipede.AquaCenti) ? 1 : 2); j++)
			{
				if (centipede.Small)
				{
					sLeaser.sprites[ShellSprite(i, j)].isVisible = centipede.BitesLeft > i;
					if (j == 0)
					{
						sLeaser.sprites[SegmentSprite(i)].isVisible = centipede.BitesLeft > i;
						if (i > 0)
						{
							sLeaser.sprites[SecondarySegmentSprite(i - 1)].isVisible = centipede.BitesLeft > i;
						}
					}
				}
				else
				{
					sLeaser.sprites[ShellSprite(i, j)].isVisible = centipede.CentiState.shells[i];
				}
			}
			float num = (float)i / (float)(base.owner.bodyChunks.Length - 1);
			Vector2 normalized = RotatAtChunk(i, timeStacker).normalized;
			Vector2 vector3 = Vector2.Lerp(base.owner.bodyChunks[i].lastPos, base.owner.bodyChunks[i].pos, timeStacker);
			Vector2 vector4 = ((i < base.owner.bodyChunks.Length - 1) ? Vector2.Lerp(base.owner.bodyChunks[i + 1].lastPos, base.owner.bodyChunks[i + 1].pos, timeStacker) : (vector3 + Custom.DirVec(vector2, vector3) * 10f));
			Vector2 normalized2 = (vector2 - vector4).normalized;
			Vector2 vector5 = Custom.PerpendicularVector(normalized2);
			float num2 = Vector2.Distance(vector3, vector2) / 4f;
			float num3 = Vector2.Distance(vector3, vector4) / 4f;
			float num4 = 3f;
			float num5 = 3f;
			if (i == 0 || centipede.Small)
			{
				num4 = 1f;
			}
			else if (i == base.owner.bodyChunks.Length - 1 || centipede.Small)
			{
				num5 = 1f;
			}
			(sLeaser.sprites[TubeSprite] as TriangleMesh).MoveVertice(i * 4, vector3 - vector5 * num4 + normalized2 * num2 - camPos);
			(sLeaser.sprites[TubeSprite] as TriangleMesh).MoveVertice(i * 4 + 1, vector3 + vector5 * num4 + normalized2 * num2 - camPos);
			(sLeaser.sprites[TubeSprite] as TriangleMesh).MoveVertice(i * 4 + 2, vector3 - vector5 * num5 - normalized2 * num3 - camPos);
			(sLeaser.sprites[TubeSprite] as TriangleMesh).MoveVertice(i * 4 + 3, vector3 + vector5 * num5 - normalized2 * num3 - camPos);
			float num6 = Mathf.Clamp(Mathf.Sin(num * (float)Math.PI), 0f, 1f);
			num6 *= Mathf.Lerp(1f, 0.5f, centipede.size);
			if (centipede.Centiwing)
			{
				num6 = Mathf.Lerp(0.6f, 0.3f, Mathf.Pow(Mathf.Clamp(Mathf.Sin(num * (float)Math.PI), 0f, 1f), 2f));
			}
			else if (centipede.AquaCenti)
			{
				num6 = Mathf.Lerp(0.8f, 0.2f, Mathf.Pow(Mathf.Clamp(Mathf.Sin(num * (float)Math.PI), 0f, 1f), 2f));
			}
			sLeaser.sprites[SegmentSprite(i)].x = vector3.x - camPos.x;
			sLeaser.sprites[SegmentSprite(i)].y = vector3.y - camPos.y;
			sLeaser.sprites[SegmentSprite(i)].rotation = Custom.VecToDeg((vector2 - vector4).normalized);
			sLeaser.sprites[SegmentSprite(i)].scaleX = base.owner.bodyChunks[i].rad * Mathf.Lerp(1f, Mathf.Lerp(1.5f, 0.9f, Mathf.Abs(normalized.x)), num6) * 2f * 0.0625f;
			for (int k = 0; k < ((!centipede.AquaCenti) ? 1 : 2); k++)
			{
				if (normalized.y > 0f)
				{
					sLeaser.sprites[ShellSprite(i, k)].scaleX = base.owner.bodyChunks[i].rad * Mathf.Lerp(1f, Mathf.Lerp(1.5f, 0.9f, Mathf.Abs(normalized.x)), num6) * 1.8f * normalized.y * (1f / 14f);
					sLeaser.sprites[ShellSprite(i, k)].scaleY = base.owner.bodyChunks[i].rad * (centipede.Red ? 1.7f : 1.5f) * (1f / 11f);
					float num7 = Mathf.InverseLerp(-0.5f, 0.5f, Vector3.Dot(normalized2, Custom.DegToVec(30f) * normalized.x));
					num7 *= Mathf.Max(Mathf.InverseLerp(0.3f, 0.05f, Mathf.Abs(-0.5f - normalized.x)), Mathf.InverseLerp(0.3f, 0.05f, Mathf.Abs(0.5f - normalized.x)));
					num7 *= Mathf.Pow(1f - darkness, 2f);
					if (k == 0)
					{
						if (centipede.abstractCreature.IsVoided())
						{
							sLeaser.sprites[ShellSprite(i, k)].color = Color.Lerp(RainWorld.SaturatedGold, blackColor, darkness);
						}
						else
						{
							sLeaser.sprites[ShellSprite(i, k)].color = Color.Lerp(Custom.HSL2RGB(hue, saturation, 0.5f + 0.25f * num7), blackColor, darkness);
						}
					}
					else
					{
						sLeaser.sprites[ShellSprite(i, k)].color = Color.Lerp(Custom.HSL2RGB(hue, saturation, 0.5f + 0.25f * num7), blackColor, 0.5f);
						sLeaser.sprites[ShellSprite(i, k)].color = Color.Lerp(sLeaser.sprites[ShellSprite(i, k)].color, new Color(0.4392157f, 0.07450981f, 0f), 0.25f);
					}
					sLeaser.sprites[ShellSprite(i, k)].element = Futile.atlasManager.GetElementWithName("CentipedeBackShell");
					sLeaser.sprites[ShellSprite(i, k)].x = (vector3 + Custom.PerpendicularVector(normalized2) * normalized.x * base.owner.bodyChunks[i].rad * 1.1f).x - camPos.x;
					sLeaser.sprites[ShellSprite(i, k)].y = (vector3 + Custom.PerpendicularVector(normalized2) * normalized.x * base.owner.bodyChunks[i].rad * 1.1f).y - camPos.y;
				}
				else
				{
					sLeaser.sprites[ShellSprite(i, k)].scaleX = base.owner.bodyChunks[i].rad * -1.8f * normalized.y * (1f / 14f);
					sLeaser.sprites[ShellSprite(i, k)].scaleY = base.owner.bodyChunks[i].rad * 1.3f * (1f / 11f);
					if (k == 0)
					{
						sLeaser.sprites[ShellSprite(i, k)].color = SecondaryShellColor;
					}
					else
					{
						sLeaser.sprites[ShellSprite(i, k)].color = blackColor;
					}
					sLeaser.sprites[ShellSprite(i, k)].element = Futile.atlasManager.GetElementWithName("CentipedeBellyShell");
					sLeaser.sprites[ShellSprite(i, k)].x = (vector3 - Custom.PerpendicularVector(normalized2) * normalized.x * base.owner.bodyChunks[i].rad).x - camPos.x;
					sLeaser.sprites[ShellSprite(i, k)].y = (vector3 - Custom.PerpendicularVector(normalized2) * normalized.x * base.owner.bodyChunks[i].rad).y - camPos.y;
				}
				sLeaser.sprites[ShellSprite(i, k)].rotation = Custom.VecToDeg((vector2 - vector4).normalized);
			}
			if (i > 0)
			{
				sLeaser.sprites[SecondarySegmentSprite(i - 1)].x = Mathf.Lerp(vector2.x, vector3.x, 0.5f) - camPos.x;
				sLeaser.sprites[SecondarySegmentSprite(i - 1)].y = Mathf.Lerp(vector2.y, vector3.y, 0.5f) - camPos.y;
				sLeaser.sprites[SecondarySegmentSprite(i - 1)].rotation = Custom.VecToDeg(Vector3.Slerp(vector, normalized2, 0.5f));
				sLeaser.sprites[SecondarySegmentSprite(i - 1)].scaleX = base.owner.bodyChunks[i].rad * Mathf.Lerp(0.9f, Mathf.Lerp(1.1f, 0.8f, Mathf.Abs(normalized.x)), num6) * 2f;
			}
			if (centipede.Red)
			{
				Vector2 vector6 = Custom.DegToVec(Custom.VecToDeg(normalized) + ((normalized.x > 0f) ? (-90f) : 90f));
				if (vector6.y > 0f && centipede.CentiState.shells[i])
				{
					sLeaser.sprites[WingSprite(1, i)].isVisible = true;
					sLeaser.sprites[WingSprite(1, i)].scaleX = base.owner.bodyChunks[i].rad * Mathf.Lerp(1f, Mathf.Lerp(1.5f, 0.9f, Mathf.Abs(vector6.x)), num6) * 0.7f * vector6.y * (1f / 14f);
					sLeaser.sprites[WingSprite(1, i)].scaleY = base.owner.bodyChunks[i].rad * 1.2f * (1f / 11f);
					float num8 = Mathf.InverseLerp(-0.5f, 0.5f, Vector3.Dot(normalized2, Custom.DegToVec(30f) * vector6.x));
					num8 *= Mathf.Max(Mathf.InverseLerp(0.3f, 0.05f, Mathf.Abs(-0.5f - vector6.x)), Mathf.InverseLerp(0.3f, 0.05f, Mathf.Abs(0.5f - vector6.x)));
					num8 *= Mathf.Pow(1f - darkness, 2f);
					if (centipede.abstractCreature.IsVoided())
					{
						sLeaser.sprites[WingSprite(1, i)].color = Color.Lerp(RainWorld.SaturatedGold, blackColor, 0.3f + 0.7f * darkness * (1f - num8));
					}
					else
					{
						sLeaser.sprites[WingSprite(1, i)].color = Color.Lerp(Custom.HSL2RGB(hue, saturation, 0.5f + 0.25f * num8), blackColor, 0.3f + 0.7f * darkness * (1f - num8));
					}
					sLeaser.sprites[WingSprite(1, i)].x = (vector3 + Custom.PerpendicularVector(normalized2) * vector6.x * base.owner.bodyChunks[i].rad * 1.2f).x - camPos.x;
					sLeaser.sprites[WingSprite(1, i)].y = (vector3 + Custom.PerpendicularVector(normalized2) * vector6.x * base.owner.bodyChunks[i].rad * 1.2f).y - camPos.y;
					sLeaser.sprites[WingSprite(1, i)].rotation = Custom.VecToDeg((vector2 - vector4).normalized);
				}
				else
				{
					sLeaser.sprites[WingSprite(1, i)].isVisible = false;
				}
				if (centipede.CentiState.shells[i])
				{
					sLeaser.sprites[WingSprite(0, i)].isVisible = true;
					float num9 = Mathf.Pow(Mathf.Abs(normalized.x), 0.5f) * Mathf.Sign(normalized.x);
					sLeaser.sprites[WingSprite(0, i)].x = (vector3 + Custom.PerpendicularVector(normalized2) * num9 * base.owner.bodyChunks[i].rad * 1.1f).x - camPos.x;
					sLeaser.sprites[WingSprite(0, i)].y = (vector3 + Custom.PerpendicularVector(normalized2) * num9 * base.owner.bodyChunks[i].rad * 1.1f).y - camPos.y;
					sLeaser.sprites[WingSprite(0, i)].rotation = Custom.VecToDeg(Vector3.Slerp((num < 0.5f) ? normalized2 : (-normalized2), Custom.PerpendicularVector(normalized2) * Mathf.Sign(num9), 0.3f + 0.7f * Mathf.Sin(num * (float)Math.PI)));
					float num10 = Mathf.InverseLerp(-0.5f, 0.5f, Vector3.Dot(normalized2, Custom.DegToVec(30f) * normalized.x));
					num10 *= Mathf.Max(Mathf.InverseLerp(0.3f, 0.05f, Mathf.Abs(-0.5f - normalized.x)), Mathf.InverseLerp(0.3f, 0.05f, Mathf.Abs(0.5f - normalized.x)));
					num10 *= Mathf.Pow(1f - darkness, 2f);
					if (centipede.abstractCreature.IsVoided())
					{
						sLeaser.sprites[WingSprite(0, i)].color = Color.Lerp(RainWorld.SaturatedGold, blackColor, darkness);
					}
					else
					{
						sLeaser.sprites[WingSprite(0, i)].color = Color.Lerp(Custom.HSL2RGB(hue, saturation, 0.5f + 0.25f * num10), blackColor, darkness);
					}
					sLeaser.sprites[WingSprite(0, i)].scaleY = Mathf.Abs(num9) * Mathf.Lerp(-0.25f, -0.6f, Mathf.Sin(num * (float)Math.PI));
					sLeaser.sprites[WingSprite(0, i)].scaleX = Mathf.Lerp(0.15f, 0.25f, Mathf.Sin(num * (float)Math.PI));
				}
				else
				{
					sLeaser.sprites[WingSprite(0, i)].isVisible = false;
				}
			}
			vector2 = vector3;
			vector = normalized2;
			if (!centipede.AquaCenti)
			{
				for (int l = 0; l < 2; l++)
				{
					Vector2 vector7 = vector3 - vector5 * ((l == 0) ? (-1f) : 1f) * normalized.y * centipede.bodyChunks[i].rad;
					Vector2 vector8 = Vector2.Lerp(legs[i, l].lastPos, legs[i, l].pos, timeStacker);
					float f = Mathf.Lerp(-1f, 1f, Mathf.Clamp(num - bodyDir * 0.4f, 0f, 1f)) * Mathf.Lerp((l == 0) ? 1f : (-1f), 0f - normalized.x, Mathf.Abs(normalized.x));
					f = Mathf.Pow(Mathf.Abs(f), 0.2f) * Mathf.Sign(f);
					Vector2 vector9 = Custom.InverseKinematic(vector7, vector8, legLengths[i] / 2f, legLengths[i] / 2f, f);
					sLeaser.sprites[LegSprite(i, l, 0)].x = vector7.x - camPos.x;
					sLeaser.sprites[LegSprite(i, l, 0)].y = vector7.y - camPos.y;
					sLeaser.sprites[LegSprite(i, l, 0)].rotation = Custom.AimFromOneVectorToAnother(vector7, vector9);
					sLeaser.sprites[LegSprite(i, l, 0)].scaleY = Vector2.Distance(vector7, vector9) / 27f;
					sLeaser.sprites[LegSprite(i, l, 0)].anchorY = 0.1f;
					sLeaser.sprites[LegSprite(i, l, 1)].anchorY = 0.1f;
					sLeaser.sprites[LegSprite(i, l, 0)].scaleX = (0f - Mathf.Sign(f)) * (centipede.Red ? 1.3f : 1f);
					sLeaser.sprites[LegSprite(i, l, 1)].scaleX = (0f - Mathf.Sign(f)) * (centipede.Red ? 1.3f : 1f);
					sLeaser.sprites[LegSprite(i, l, 1)].x = vector9.x - camPos.x;
					sLeaser.sprites[LegSprite(i, l, 1)].y = vector9.y - camPos.y;
					sLeaser.sprites[LegSprite(i, l, 1)].rotation = Custom.AimFromOneVectorToAnother(vector9, vector8);
					sLeaser.sprites[LegSprite(i, l, 1)].scaleY = Vector2.Distance(vector9, vector8) / 25f;
				}
			}
		}
		for (int m = 0; m < 2; m++)
		{
			Vector2 vector10;
			Vector2 vector11;
			if (m == 0)
			{
				vector10 = Vector2.Lerp(base.owner.bodyChunks[0].lastPos, base.owner.bodyChunks[0].pos, timeStacker);
				vector11 = Custom.DirVec(Vector2.Lerp(base.owner.bodyChunks[1].lastPos, base.owner.bodyChunks[1].pos, timeStacker), vector10);
			}
			else
			{
				vector10 = Vector2.Lerp(base.owner.bodyChunks[base.owner.bodyChunks.Length - 1].lastPos, base.owner.bodyChunks[base.owner.bodyChunks.Length - 1].pos, timeStacker);
				vector11 = Custom.DirVec(Vector2.Lerp(base.owner.bodyChunks[base.owner.bodyChunks.Length - 2].lastPos, base.owner.bodyChunks[base.owner.bodyChunks.Length - 2].pos, timeStacker), vector10);
			}
			for (int n = 0; n < 2; n++)
			{
				for (int num11 = 0; num11 < 2; num11++)
				{
					Vector2 vector12 = Vector2.Lerp(whiskers[m, num11, n].lastPos, whiskers[m, num11, n].pos, timeStacker);
					vector2 = vector10;
					float num12 = (centipede.Small ? 0.5f : 1f);
					float num13 = 1f;
					for (int num14 = 0; num14 < 4; num14++)
					{
						Vector2 vector13 = Custom.Bezier(vector10, vector10 + vector11 * Vector2.Distance(vector10, vector12) * 0.7f, vector12, vector12, (float)num14 / 3f);
						num13 *= 0.7f;
						Vector2 normalized3 = (vector13 - vector2).normalized;
						Vector2 vector14 = Custom.PerpendicularVector(normalized3);
						float num15 = Vector2.Distance(vector13, vector2) / ((num14 == 0) ? 1f : 5f);
						(sLeaser.sprites[WhiskerSprite(m, n, num11)] as TriangleMesh).MoveVertice(num14 * 4, vector2 - vector14 * num12 + normalized3 * num15 - camPos);
						(sLeaser.sprites[WhiskerSprite(m, n, num11)] as TriangleMesh).MoveVertice(num14 * 4 + 1, vector2 + vector14 * num12 + normalized3 * num15 - camPos);
						if (num14 < 3)
						{
							(sLeaser.sprites[WhiskerSprite(m, n, num11)] as TriangleMesh).MoveVertice(num14 * 4 + 2, vector13 - vector14 * num12 - normalized3 * num15 - camPos);
							(sLeaser.sprites[WhiskerSprite(m, n, num11)] as TriangleMesh).MoveVertice(num14 * 4 + 3, vector13 + vector14 * num12 - normalized3 * num15 - camPos);
						}
						else
						{
							(sLeaser.sprites[WhiskerSprite(m, n, num11)] as TriangleMesh).MoveVertice(num14 * 4 + 2, vector13 + normalized3 * 2.1f - camPos);
						}
						vector2 = vector13;
					}
				}
			}
			if (!centipede.Centiwing && !centipede.AquaCenti)
			{
				continue;
			}
			for (int num16 = 0; num16 < wingPairs; num16++)
			{
				Vector2 vector15 = ((num16 != 0) ? Custom.DirVec(ChunkDrawPos(num16 - 1, timeStacker), ChunkDrawPos(num16, timeStacker)) : Custom.DirVec(ChunkDrawPos(0, timeStacker), ChunkDrawPos(1, timeStacker)));
				Vector2 vector16 = Custom.PerpendicularVector(vector15);
				Vector2 vector17 = RotatAtChunk(num16, timeStacker);
				Vector2 vector18 = WingPos(m, num16, vector15, vector16, vector17, timeStacker);
				Vector2 vector19 = ChunkDrawPos(num16, timeStacker) + centipede.bodyChunks[num16].rad * ((m == 0) ? (-1f) : 1f) * vector16 * vector17.y;
				Vector2 lhs = Custom.DegToVec(Custom.AimFromOneVectorToAnother(vector18, vector19) + Custom.VecToDeg(vector17));
				float a = Mathf.InverseLerp(0.85f, 1f, Vector2.Dot(lhs, Custom.DegToVec(45f))) * Mathf.Abs(Vector2.Dot(Custom.DegToVec(45f + Custom.VecToDeg(vector17)), vector15));
				Vector2 lhs2 = Custom.DegToVec(Custom.AimFromOneVectorToAnother(vector19, vector18) + Custom.VecToDeg(vector17));
				float b = Mathf.InverseLerp(0.85f, 1f, Vector2.Dot(lhs2, Custom.DegToVec(45f))) * Mathf.Abs(Vector2.Dot(Custom.DegToVec(45f + Custom.VecToDeg(vector17)), -vector15));
				a = Mathf.Pow(Mathf.Max(a, b), 0.5f);
				float num17 = 2f;
				if (centipede.AquaCenti)
				{
					num17 = 5f;
				}
				(sLeaser.sprites[WingSprite(m, num16)] as CustomFSprite).MoveVertice(1, vector18 + vector15 * num17 - camPos);
				(sLeaser.sprites[WingSprite(m, num16)] as CustomFSprite).MoveVertice(0, vector18 - vector15 * num17 - camPos);
				(sLeaser.sprites[WingSprite(m, num16)] as CustomFSprite).MoveVertice(2, vector19 + vector15 * num17 - camPos);
				(sLeaser.sprites[WingSprite(m, num16)] as CustomFSprite).MoveVertice(3, vector19 - vector15 * num17 - camPos);
				(sLeaser.sprites[WingSprite(m, num16)] as CustomFSprite).verticeColors[0] = Custom.HSL2RGB(0.99f - 0.4f * Mathf.Pow(a, 2f), 1f, 0.5f + 0.5f * a, 0.5f + 0.5f * a);
				(sLeaser.sprites[WingSprite(m, num16)] as CustomFSprite).verticeColors[1] = Custom.HSL2RGB(0.99f - 0.4f * Mathf.Pow(a, 2f), 1f, 0.5f + 0.5f * a, 0.5f + 0.5f * a);
				(sLeaser.sprites[WingSprite(m, num16)] as CustomFSprite).verticeColors[2] = Color.Lerp(new Color(blackColor.r, blackColor.g, blackColor.b), new Color(1f, 1f, 1f), 0.5f * a);
				(sLeaser.sprites[WingSprite(m, num16)] as CustomFSprite).verticeColors[3] = Color.Lerp(new Color(blackColor.r, blackColor.g, blackColor.b), new Color(1f, 1f, 1f), 0.5f * a);
			}
		}
	}

	private Vector2 WingPos(int side, int wing, Vector2 dr, Vector2 prp, Vector2 chunkRotat, float timeStacker)
	{
		float t = (float)wing / (float)(wingPairs - 1);
		float f = VerticalWingFlapAtChunk(wing, timeStacker);
		float num = HorizontalWingFlapAtChunk(wing, chunkRotat, timeStacker);
		Vector2 vector = dr * Mathf.Lerp(Mathf.Lerp(-1f, 1f, t), num * chunkRotat.y, 0.5f * Mathf.Abs(chunkRotat.y)) * Mathf.Abs(chunkRotat.y);
		Vector2 a = Vector2.Lerp(prp * ((side == 0) ? (-1f) : 1f) * chunkRotat.y, prp * Mathf.Pow(Mathf.Abs(f), 0.5f) * Mathf.Sign(f) * Mathf.Abs(chunkRotat.x), Mathf.Abs(chunkRotat.x)) + vector;
		a = Vector2.Lerp(a, a.normalized, Mathf.Pow(Mathf.Abs(chunkRotat.y), 2f));
		Vector2 b = (dr * Mathf.Lerp(1f, -1f, t) + Vector2.Lerp(prp * ((side == 0) ? (-1f) : 1f) * chunkRotat.y, prp * (0f - chunkRotat.x), Mathf.Abs(chunkRotat.x))) * 0.5f;
		a = Vector2.Lerp(a, b, Mathf.Lerp(lastWingsFolded, wingsFolded, timeStacker));
		Vector2 vector2 = ChunkDrawPos(wing, timeStacker);
		if (centipede.AquaCenti)
		{
			Vector2 vector3 = Custom.PerpendicularVector(vector2, vector2 + a) * Mathf.Cos((Mathf.Lerp(lastWingSwimCycle, wingSwimCycle, timeStacker) + (float)wing * Custom.LerpMap(chunkRotat.y, -1f, 1f, 1.8f, 0.6f)) * (float)Math.PI * 0.3f) * (wingLengths[wing] / 4f);
			return vector2 + a * wingLengths[wing] + vector3;
		}
		return vector2 + a * wingLengths[wing];
	}

	private float VerticalWingFlapAtChunk(int chunk, float timeStacker)
	{
		return Mathf.Sin((Mathf.Lerp(lastWingFlapCycle, wingFlapCycle, timeStacker) + (float)chunk * 1.8f) * (float)Math.PI * 0.3f + ((chunk % 2 == 10) ? ((float)Math.PI) : 0f)) * (1f - Mathf.Lerp(lastWingsFolded, wingsFolded, timeStacker));
	}

	private float HorizontalWingFlapAtChunk(int chunk, Vector2 chunkRotat, float timeStacker)
	{
		return Mathf.Cos((Mathf.Lerp(lastWingFlapCycle, wingFlapCycle, timeStacker) + (float)chunk * Custom.LerpMap(chunkRotat.y, -1f, 1f, 1.8f, 0.6f)) * (float)Math.PI * 0.3f) * (1f - Mathf.Lerp(lastWingsFolded, wingsFolded, timeStacker));
	}

	private Vector2 ChunkDrawPos(int indx, float timeStacker)
	{
		return Vector2.Lerp(centipede.bodyChunks[indx].lastPos, centipede.bodyChunks[indx].pos, timeStacker);
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		if (centipede.Glower != null)
		{
			centipede.Glower.color = Color.Lerp(new Color(palette.waterColor1.r, palette.waterColor1.g, palette.waterColor1.b, 1f), new Color(0.7f, 0.7f, 1f, 1f), 0.25f);
		}
		blackColor = palette.blackColor;
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			sLeaser.sprites[i].color = blackColor;
		}
		for (int j = 0; j < totalSecondarySegments; j++)
		{
			Mathf.Sin((float)j / (float)(totalSecondarySegments - 1) * (float)Math.PI);
			if (centipede.abstractCreature.IsVoided())
			{
				sLeaser.sprites[SecondarySegmentSprite(j)].color = Color.Lerp(RainWorld.SaturatedGold, blackColor, Mathf.Lerp(0.4f, 1f, darkness));
			}
			sLeaser.sprites[SecondarySegmentSprite(j)].color = Color.Lerp(Custom.HSL2RGB(hue, 1f, 0.2f), blackColor, Mathf.Lerp(0.4f, 1f, darkness));
		}
		for (int k = 0; k < base.owner.bodyChunks.Length; k++)
		{
			for (int l = 0; l < 2; l++)
			{
				(sLeaser.sprites[LegSprite(k, l, 1)] as VertexColorSprite).verticeColors[0] = SecondaryShellColor;
				(sLeaser.sprites[LegSprite(k, l, 1)] as VertexColorSprite).verticeColors[1] = SecondaryShellColor;
				(sLeaser.sprites[LegSprite(k, l, 1)] as VertexColorSprite).verticeColors[2] = blackColor;
				(sLeaser.sprites[LegSprite(k, l, 1)] as VertexColorSprite).verticeColors[3] = blackColor;
			}
		}
	}
}
