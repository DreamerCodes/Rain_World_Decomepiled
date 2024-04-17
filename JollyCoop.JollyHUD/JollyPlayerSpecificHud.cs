using System;
using System.Collections.Generic;
using System.Linq;
using HUD;
using RWCustom;
using UnityEngine;

namespace JollyCoop.JollyHUD;

public class JollyPlayerSpecificHud : HudPart
{
	public class JollyDeathBump : JollyPointer
	{
		public FSprite symbolSprite;

		public int counter = -20;

		private float blink;

		private float lastBlink;

		public bool removeAsap;

		private Vector2 pingPosition;

		private Vector2 lastPingPosition;

		public bool PlayerHasExplosiveSpearInThem
		{
			get
			{
				if (jollyHud.RealizedPlayer == null)
				{
					return false;
				}
				if (jollyHud.RealizedPlayer.abstractCreature.stuckObjects.Count == 0)
				{
					return false;
				}
				for (int i = 0; i < jollyHud.RealizedPlayer.abstractCreature.stuckObjects.Count; i++)
				{
					if (jollyHud.RealizedPlayer.abstractCreature.stuckObjects[i].A is AbstractSpear && (jollyHud.RealizedPlayer.abstractCreature.stuckObjects[i].A as AbstractSpear).explosive)
					{
						return true;
					}
				}
				return false;
			}
		}

		public JollyDeathBump(JollyPlayerSpecificHud jollyHud)
			: base(jollyHud)
		{
			base.jollyHud = jollyHud;
			SetPosToPlayer();
			gradient = new FSprite("Futile_White");
			gradient.shader = jollyHud.hud.rainWorld.Shaders["FlatLight"];
			if ((jollyHud.abstractPlayer.state as PlayerState).slugcatCharacter != SlugcatStats.Name.Night)
			{
				gradient.color = new Color(0f, 0f, 0f);
			}
			jollyHud.hud.fContainers[0].AddChild(gradient);
			gradient.alpha = 0f;
			gradient.x = -1000f;
			symbolSprite = new FSprite("Multiplayer_Death");
			symbolSprite.color = PlayerGraphics.DefaultSlugcatColor((jollyHud.abstractPlayer.state as PlayerState).slugcatCharacter);
			jollyHud.hud.fContainers[0].AddChild(symbolSprite);
			symbolSprite.alpha = 0f;
			symbolSprite.x = -1000f;
		}

		public void SetPosToPlayer()
		{
			lastPingPosition = pingPosition;
		}

		public override void Update()
		{
			base.Update();
			lastAlpha = alpha;
			lastBlink = blink;
			lastPingPosition = pingPosition;
			pingPosition = bodyPos + new Vector2(0f, 10f);
			if (!jollyHud.PlayerRoomBeingViewed)
			{
				slatedForDeletion = true;
			}
			if (counter < 0)
			{
				SetPosToPlayer();
				if (jollyHud.RealizedPlayer == null || jollyHud.RealizedPlayer.room == null || !jollyHud.RealizedPlayer.room.ViewedByAnyCamera(jollyHud.RealizedPlayer.mainBodyChunk.pos, 200f) || removeAsap || jollyHud.RealizedPlayer.grabbedBy.Count > 0)
				{
					counter = 0;
					removeAsap = true;
					jollyHud.hud.PlaySound(SoundID.UI_Multiplayer_Player_Dead_A);
					jollyHud.hud.PlaySound(SoundID.UI_Multiplayer_Player_Dead_B);
				}
				else if (Custom.DistLess(jollyHud.RealizedPlayer.bodyChunks[0].pos, jollyHud.RealizedPlayer.bodyChunks[0].lastLastPos, 6f) && Custom.DistLess(jollyHud.RealizedPlayer.bodyChunks[1].pos, jollyHud.RealizedPlayer.bodyChunks[1].lastLastPos, 6f) && !PlayerHasExplosiveSpearInThem)
				{
					counter++;
				}
			}
			counter++;
			if (removeAsap)
			{
				counter += 10;
			}
			if (counter < 40)
			{
				alpha = Mathf.Sin(Mathf.InverseLerp(0f, 40f, counter) * (float)Math.PI);
				blink = Custom.LerpAndTick(blink, 1f, 0.07f, 1f / 30f);
				if (counter == 5)
				{
					if (!removeAsap)
					{
						jollyHud.hud.fadeCircles.Add(new FadeCircle(jollyHud.hud, 10f, 10f, 0.82f, 30f, 4f, bodyPos, jollyHud.hud.fContainers[1]));
					}
					jollyHud.hud.PlaySound(SoundID.UI_Multiplayer_Player_Dead_A);
				}
			}
			else if (counter == 40)
			{
				if (!removeAsap)
				{
					FadeCircle fadeCircle = new FadeCircle(jollyHud.hud, 20f, 30f, 0.94f, 60f, 4f, bodyPos, jollyHud.hud.fContainers[1]);
					fadeCircle.alphaMultiply = 0.5f;
					fadeCircle.fadeThickness = false;
					jollyHud.hud.fadeCircles.Add(fadeCircle);
					alpha = 1f;
					blink = 0f;
				}
				jollyHud.hud.PlaySound(SoundID.UI_Multiplayer_Player_Dead_B);
			}
			else if (counter <= 220)
			{
				alpha = Mathf.InverseLerp(220f, 110f, counter);
			}
			else if (counter > 220)
			{
				slatedForDeletion = true;
			}
		}

		public override void Draw(float timeStacker)
		{
			Vector2 vector = Vector2.Lerp(lastPingPosition, pingPosition, timeStacker) + new Vector2(0.01f, 0.01f);
			float num = Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(lastAlpha, alpha, timeStacker)), 0.7f);
			gradient.x = vector.x;
			gradient.y = vector.y + 10f;
			gradient.scale = Mathf.Lerp(80f, 110f, num) / 16f;
			gradient.alpha = 0.17f * Mathf.Pow(num, 2f);
			symbolSprite.x = vector.x;
			symbolSprite.y = Mathf.Min(vector.y + Custom.SCurve(Mathf.InverseLerp(40f, 130f, (float)counter + timeStacker), 0.8f) * 80f, jollyHud.Camera.sSize.y - 30f);
			Color color = jollyHud.playerColor;
			if (counter % 6 < 2 && lastBlink > 0f)
			{
				color = ((!((jollyHud.abstractPlayer.state as PlayerState).slugcatCharacter == SlugcatStats.Name.White)) ? Color.Lerp(color, new Color(1f, 1f, 1f), Mathf.InverseLerp(0f, 0.5f, Mathf.Lerp(lastBlink, blink, timeStacker))) : Color.Lerp(color, new Color(0.9f, 0.9f, 0.9f), Mathf.InverseLerp(0f, 0.5f, Mathf.Lerp(lastBlink, blink, timeStacker))));
			}
			symbolSprite.color = color;
			symbolSprite.alpha = num;
		}

		public override void ClearSprites()
		{
			base.ClearSprites();
			gradient.RemoveFromContainer();
			symbolSprite.RemoveFromContainer();
		}
	}

	public class JollyOffRoom : JollyPointer
	{
		private List<FSprite> sprites;

		private Vector2 localPos;

		private Vector2 playerPos;

		private Vector2 drawPos;

		private Vector2 lastDrawPos;

		private Vector2 roomPos;

		public Color drawcolor;

		private float scale;

		private float screenSizeX;

		private float screenSizeY;

		private Vector2 middleScreen;

		private Vector2 rectangleSize;

		private float diagScale;

		public float uAlpha;

		public JollyOffRoom(JollyPlayerSpecificHud jollyHud)
			: base(jollyHud)
		{
			hidden = true;
			sprites = new List<FSprite>();
			timer = 0;
			scale = 1.25f;
			InitiateSprites();
			screenSizeX = jollyHud.hud.rainWorld.options.ScreenSize.x;
			screenSizeY = jollyHud.hud.rainWorld.options.ScreenSize.y;
			middleScreen = new Vector2(screenSizeX / 2f, screenSizeY / 2f);
			rectangleSize = new Vector2(screenSizeX - (float)(2 * screenEdge), screenSizeY - (float)(2 * screenEdge));
			diagScale = Mathf.Abs(Vector2.Distance(Vector2.zero, middleScreen));
		}

		private void InitiateSprites()
		{
			sprites.Add(new FSprite("GuidanceSlugcat")
			{
				shader = jollyHud.hud.rainWorld.Shaders["Hologram"],
				scale = scale
			});
			sprites.Add(new FSprite("Futile_White")
			{
				shader = jollyHud.hud.rainWorld.Shaders["FlatLight"],
				alpha = 0f,
				x = -1000f
			});
			for (int i = 0; i < sprites.Count; i++)
			{
				sprites[i].color = jollyHud.playerColor;
				sprites[i].alpha = 0.1f;
				jollyHud.fContainer.AddChild(sprites[i]);
			}
		}

		public override void Update()
		{
			base.Update();
			if (base.PlayerState.permaDead)
			{
				slatedForDeletion = true;
				return;
			}
			if (jollyHud.RealizedPlayer != null)
			{
				playerPos = jollyHud.abstractPlayer.world.RoomToWorldPos(jollyHud.RealizedPlayer.mainBodyChunk.pos, jollyHud.abstractPlayer.Room.index);
				roomPos = jollyHud.abstractPlayer.world.RoomToWorldPos(jollyHud.camPos, jollyHud.Camera.room.abstractRoom.index);
			}
			lastDrawPos = drawPos;
			drawPos = playerPos - roomPos;
			float num = Mathf.Abs(Vector2.Distance(drawPos, middleScreen));
			scale = Mathf.Lerp(0.65f, 1.65f, Mathf.Pow(diagScale / num, 1.2f));
			float num2 = middleScreen.x - rectangleSize.x / 2f;
			float num3 = middleScreen.x + rectangleSize.x / 2f;
			float num4 = middleScreen.y - rectangleSize.y / 2f;
			float num5 = middleScreen.y + rectangleSize.y / 2f;
			if (num2 < drawPos.x && drawPos.x < num3 && num4 < drawPos.y && drawPos.y < num5)
			{
				float b = Mathf.Abs(drawPos.x - num2);
				float num6 = Mathf.Abs(drawPos.x - num3);
				float num7 = Mathf.Abs(drawPos.y - num4);
				float d = Mathf.Abs(drawPos.y - num5);
				float smallestNumber = GetSmallestNumber(num7, b, num6, d);
				if (AreClose(smallestNumber, b))
				{
					drawPos.x = num2;
				}
				else if (AreClose(smallestNumber, num6))
				{
					drawPos.x = num3;
				}
				else if (AreClose(smallestNumber, num7))
				{
					drawPos.y = num4;
				}
				else
				{
					drawPos.y = num5;
				}
			}
			drawPos.x = Mathf.Clamp(drawPos.x, screenEdge, screenSizeX - (float)screenEdge);
			drawPos.y = Mathf.Clamp(drawPos.y, screenEdge, screenSizeY - (float)screenEdge);
			if (jollyHud.PlayerRoomBeingViewed || jollyHud.inShortcut || !knownPos || forceHide)
			{
				hidden = true;
			}
			else if (hidden)
			{
				hidden = false;
				lastDrawPos = drawPos;
			}
			alpha = ((!hidden) ? 0.85f : 0f);
		}

		public override void Draw(float timeStacker)
		{
			base.Draw(timeStacker);
			if (hidden)
			{
				sprites[0].isVisible = false;
				sprites[1].isVisible = false;
				return;
			}
			if (UnityEngine.Random.value > Mathf.InverseLerp(0.5f, 0.75f, alpha))
			{
				for (int i = 0; i < sprites.Count; i++)
				{
					sprites[i].isVisible = false;
				}
				return;
			}
			uAlpha = Mathf.SmoothStep(lastAlpha, alpha, timeStacker);
			Vector2 vector = Vector2.Lerp(drawPos, lastDrawPos, timeStacker);
			for (int j = 0; j < sprites.Count; j++)
			{
				sprites[j].isVisible = true;
				sprites[j].x = vector.x;
				sprites[j].y = vector.y;
				sprites[j].scale = scale;
				sprites[j].color = jollyHud.playerColor;
			}
			sprites[1].scale = Mathf.Lerp(80f, 110f, 1f) / 16f;
			sprites[0].alpha = Mathf.Lerp(sprites[0].alpha, uAlpha, timeStacker * 0.5f);
			sprites[1].alpha = Mathf.Lerp(sprites[1].alpha, 0.15f * Mathf.Pow(uAlpha, 2f), timeStacker);
		}

		private float GetSmallestNumber(float a, float b, float c, float d)
		{
			return Mathf.Min(a, Mathf.Min(b, Mathf.Min(c, d)));
		}

		private bool AreClose(float a, float b)
		{
			return (double)Mathf.Abs(a - b) <= 0.01;
		}

		public override void ClearSprites()
		{
			base.ClearSprites();
			JollyCustom.Log("JollyOfscreen: Clearing sprites");
			for (int i = 0; i < sprites.Count; i++)
			{
				sprites[i].RemoveFromContainer();
			}
		}
	}

	public class JollyPlayerArrow : JollyPointer
	{
		public bool pointing;

		private float blink;

		private int fadeAwayCounter;

		public bool hide;

		private FLabel label;

		public int lastRoom = -1;

		public string playerName;

		private int shortcutWaitTime;

		private int initialWaitTime;

		private Color mainColor;

		private Color inverColor;

		private int frequency;

		public JollyPlayerArrow(JollyPlayerSpecificHud jollyHud)
			: base(jollyHud)
		{
			JollyCustom.Log("Adding Player pointer to " + base.PlayerState.playerNumber);
			bodyPos = new Vector2(0f, 0f);
			lastBodyPos = bodyPos;
			blink = 1f;
			playerName = string.Empty;
			mainColor = jollyHud.playerColor;
			inverColor = JollyCustom.GenerateClippedInverseColor(mainColor);
			gradient = new FSprite("Futile_White")
			{
				shader = base.jollyHud.hud.rainWorld.Shaders["FlatLight"],
				alpha = 0f,
				x = -1000f,
				color = inverColor
			};
			jollyHud.fContainer.AddChild(gradient);
			mainSprite = new FSprite("Multiplayer_Arrow");
			jollyHud.fContainer.AddChild(mainSprite);
			label = new FLabel(Custom.GetFont(), playerName);
			jollyHud.fContainer.AddChild(label);
			initialWaitTime = Player.InitialShortcutWaitTime;
		}

		public override void Update()
		{
			base.Update();
			blink = Mathf.Max(0f, blink - 0.0125f);
			alpha = Custom.LerpAndTick(alpha, Mathf.InverseLerp(80f, 20f, fadeAwayCounter), 0.08f, 1f / 30f);
			if (jollyHud.Camera.room == null)
			{
				hide = true;
			}
			lastRoom = jollyHud.Camera.cameraNumber;
			if (base.PlayerState.permaDead)
			{
				slatedForDeletion = true;
				hide = true;
			}
			if (playerName == string.Empty)
			{
				playerName = JollyCustom.GetPlayerName(jollyHud.playerNumber);
				label.text = playerName;
				size.x = 5 * playerName.Length;
			}
			mainColor = jollyHud.playerColor;
			if (!jollyHud.PlayerRoomBeingViewed || forceHide || !knownPos)
			{
				hide = true;
			}
			else
			{
				hide = false;
			}
			if (hide)
			{
				alpha = 0f;
				lastAlpha = 0f;
				mainColor = jollyHud.playerColor;
			}
			pointing = false;
			if (jollyHud.RealizedPlayer == null)
			{
				return;
			}
			PhysicalObject objectPointed = jollyHud.RealizedPlayer.objectPointed;
			if (objectPointed != null && objectPointed.jollyBeingPointedCounter > 35)
			{
				blink = 1f;
				fadeAwayCounter = 0;
				timer = 0;
				pointing = true;
			}
			if (pointing && timer < 20)
			{
				blink = 1f;
				fadeAwayCounter = 0;
				timer = 0;
			}
			if ((jollyHud.RealizedPlayer.RevealMap || jollyHud.RealizedPlayer.showKarmaFoodRainTime > 0 || nearEdge || jollyHud.RealizedPlayer.inShortcut) && timer > 20)
			{
				blink = 1f;
				fadeAwayCounter = 0;
				timer = 0;
			}
			if (timer > 10 && !Custom.DistLess(jollyHud.RealizedPlayer.firstChunk.lastPos, jollyHud.RealizedPlayer.firstChunk.pos, 3f))
			{
				fadeAwayCounter++;
			}
			if (fadeAwayCounter > 0)
			{
				fadeAwayCounter++;
			}
			timer++;
			frequency++;
			frequency %= 40;
			if (timer > 100)
			{
				timer = 100;
			}
			if (fadeAwayCounter > 100)
			{
				fadeAwayCounter = 100;
			}
			try
			{
				ShortcutHandler.ShortCutVessel shortCutVessel = jollyHud.Camera.game.shortcuts?.transportVessels?.FirstOrDefault((ShortcutHandler.ShortCutVessel x) => x.creature == jollyHud.RealizedPlayer);
				if (shortCutVessel != null)
				{
					shortcutWaitTime = shortCutVessel.wait;
				}
			}
			catch (Exception ex)
			{
				JollyCustom.Log(ex.ToString());
			}
			hidden = Mathf.Abs(alpha) - 0.05f < 0f;
		}

		public Vector2 ClampScreenEdge(Vector2 input)
		{
			input.x = Mathf.Clamp(input.x, screenEdge, jollyHud.hud.rainWorld.options.ScreenSize.x - (float)screenEdge);
			input.y = Mathf.Clamp(input.y, screenEdge, jollyHud.hud.rainWorld.options.ScreenSize.y - (float)screenEdge);
			return input;
		}

		public override void Draw(float timeStacker)
		{
			base.Draw(timeStacker);
			Vector2 input = Vector2.Lerp(lastBodyPos, bodyPos, timeStacker) + new Vector2(0.01f, 40f);
			Vector2 input2 = Vector2.Lerp(lastTargetPos, targetPos, timeStacker) + new Vector2(0.01f, 40f);
			input = ClampScreenEdge(input);
			input2 = ClampScreenEdge(input2);
			float rotation = 0f;
			if (Custom.Dist(bodyPos, input) > 20f)
			{
				rotation = Custom.AimFromOneVectorToAnother(bodyPos, input);
				rotation = Mathf.Round(rotation / 90f) * 90f;
			}
			if (mainSprite != null)
			{
				mainSprite.x = input.x;
				mainSprite.y = input.y;
				mainSprite.rotation = rotation;
			}
			if (label != null)
			{
				label.x = input.x;
				label.y = input2.y + 20f;
			}
			float num = Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(lastAlpha, alpha, timeStacker)), 0.7f);
			if (shortcutWaitTime > 0 && !hide)
			{
				num = 1f;
				alpha = 1f;
				float num2 = (float)frequency / 40f;
				float t = (float)shortcutWaitTime / (float)initialWaitTime;
				float num3 = Mathf.Max(0f, Mathf.Pow(Mathf.Lerp(17f, 4f, t), 1.2f));
				float t2 = 0.5f * (0.8f + Mathf.Sin(num3 * num2));
				float t3 = Mathf.Lerp(0.01f, 0.6f, t2);
				mainColor = Color.Lerp(jollyHud.playerColor, JollyCustom.GenerateClippedInverseColor(jollyHud.playerColor), t3);
			}
			gradient.x = input.x;
			gradient.y = input.y + 10f;
			gradient.y = input.y;
			gradient.scale = Mathf.Lerp(80f, 110f, num) / 16f;
			gradient.alpha = 0.17f * Mathf.Pow(num, 2f);
			label.color = mainColor;
			mainSprite.color = mainColor;
			label.alpha = (pointing ? 0f : num);
			mainSprite.alpha = num;
		}

		public override void ClearSprites()
		{
			base.ClearSprites();
			label.RemoveFromContainer();
			mainSprite.RemoveFromContainer();
			gradient.RemoveFromContainer();
		}
	}

	public abstract class JollyPart
	{
		public JollyPlayerSpecificHud jollyHud;

		public Vector2 bodyPos;

		public Vector2 lastBodyPos;

		public Vector2 targetPos;

		public Vector2 lastTargetPos;

		public bool slatedForDeletion;

		public bool hidden;

		public bool lastHidden;

		public bool forceHide;

		public bool knownPos;

		public JollyPart(JollyPlayerSpecificHud jollyHud)
		{
			this.jollyHud = jollyHud;
		}

		public virtual void Update()
		{
		}

		public virtual void Draw(float timeStacker)
		{
		}

		public virtual void ClearSprites()
		{
		}
	}

	public abstract class JollyPointer : JollyPart
	{
		public FSprite gradient;

		public FSprite mainSprite;

		public float lastAlpha;

		public float alpha;

		public int timer;

		protected int screenEdge = 25;

		public IntVector2 size;

		private int collidingCounter;

		protected bool nearHorizontalEdge;

		protected bool nearVerticalEdge;

		protected bool nearEdge;

		public bool nearXEdgeR;

		public bool nearXEdgeL;

		public bool nearYEdgeU;

		public bool nearYEdgeB;

		public PlayerState PlayerState => jollyHud.PlayerState;

		public JollyPointer(JollyPlayerSpecificHud jollyHud)
			: base(jollyHud)
		{
			size = new IntVector2(30, 20);
			targetPos = bodyPos;
		}

		public override void Update()
		{
			base.Update();
			knownPos = false;
			forceHide = false;
			lastBodyPos = bodyPos;
			lastTargetPos = targetPos;
			lastAlpha = alpha;
			lastHidden = hidden;
			if (jollyHud.RealizedPlayer == null)
			{
				return;
			}
			if (jollyHud.RealizedPlayer.room == null)
			{
				Vector2? vector = jollyHud.Camera.game.shortcuts.OnScreenPositionOfInShortCutCreature(jollyHud.Camera.room, jollyHud.RealizedPlayer);
				if (vector.HasValue)
				{
					bodyPos = vector.Value - jollyHud.Camera.pos;
					knownPos = true;
				}
			}
			else
			{
				if (this is JollyPlayerArrow && jollyHud.RealizedPlayer.objectPointed != null && jollyHud.RealizedPlayer.objectPointed.jollyBeingPointedCounter > 35)
				{
					bodyPos = jollyHud.RealizedPlayer.objectPointed.bodyChunks[0].pos - jollyHud.Camera.pos;
				}
				else
				{
					bodyPos = Vector2.Lerp(jollyHud.RealizedPlayer.bodyChunks[0].pos, jollyHud.RealizedPlayer.bodyChunks[1].pos, 1f / 3f) - jollyHud.Camera.pos;
				}
				knownPos = true;
			}
			nearXEdgeR = (double)(bodyPos.x - jollyHud.hud.rainWorld.options.ScreenSize.x) > -0.1;
			nearXEdgeL = (double)bodyPos.x < 0.1;
			nearHorizontalEdge = nearXEdgeR || nearXEdgeL;
			nearYEdgeU = (double)(bodyPos.y - jollyHud.hud.rainWorld.options.ScreenSize.y) > -0.1;
			nearYEdgeB = (double)bodyPos.y < 0.1;
			nearVerticalEdge = nearYEdgeU || nearYEdgeB;
			nearEdge = nearHorizontalEdge || nearVerticalEdge;
			if (hidden)
			{
				targetPos = bodyPos;
				lastTargetPos = targetPos;
				return;
			}
			List<KeyValuePair<JollyPointer, Vector2>> list = jollyHud.hud.pointerPositions.Where((KeyValuePair<JollyPointer, Vector2> e) => !e.Key.Equals(this)).ToList();
			float num = bodyPos.y;
			bool flag = false;
			foreach (KeyValuePair<JollyPointer, Vector2> item in list)
			{
				JollyPointer key = item.Key;
				Vector2 value = item.Value;
				if (key.hidden)
				{
					continue;
				}
				int num2 = (int)bodyPos.x;
				int num3 = (int)value.x;
				if (key.jollyHud.playerNumber > jollyHud.playerNumber)
				{
					targetPos = bodyPos;
					continue;
				}
				bool num4 = num2 < num3 + key.size.x && num2 + size.x > num3;
				bool flag2 = (nearXEdgeL && key.nearXEdgeL) || (nearXEdgeR && key.nearXEdgeR);
				bool flag3 = (nearYEdgeB && key.nearYEdgeB) || (nearYEdgeU && key.nearYEdgeU);
				if (num4 || flag2 || flag3)
				{
					int num5 = (int)value.y + key.size.y;
					int num6 = (int)bodyPos.y;
					if ((num6 < num5 && (float)(num6 + size.y) > value.y) || flag3 || flag2)
					{
						flag = true;
						collidingCounter++;
						num = Mathf.Max(num6, num5);
						jollyHud.hud.pointerPositions[this] = new Vector2(targetPos.x, num);
					}
				}
			}
			float t = ((collidingCounter > 5) ? (Mathf.Pow(collidingCounter, 1.4f) / 10f) : 0f);
			targetPos.y = Mathf.SmoothStep(bodyPos.y, num, t);
			if (!flag)
			{
				collidingCounter = Mathf.Max(0, collidingCounter / 4);
			}
		}
	}

	public AbstractCreature abstractPlayer;

	public JollyPlayerArrow playerArrow;

	public JollyDeathBump deathBump;

	public JollyOffRoom offRoom;

	public int playerNumber;

	private bool inShortcutLast;

	private bool inShortcut;

	private Color playerColor;

	private Vector2 camPos;

	private FContainer fContainer;

	public List<JollyPart> parts;

	private bool addedDeathBumpThisSession;

	public PlayerState PlayerState => abstractPlayer.state as PlayerState;

	public RoomCamera Camera => abstractPlayer.world.game.cameras[0];

	public bool PlayerRoomBeingViewed => abstractPlayer.Room == Camera.room?.abstractRoom;

	public Player RealizedPlayer => abstractPlayer.realizedCreature as Player;

	public JollyPlayerSpecificHud(global::HUD.HUD hud, FContainer fContainer, AbstractCreature player)
		: base(hud)
	{
		abstractPlayer = player;
		playerNumber = PlayerState.playerNumber;
		inShortcut = false;
		inShortcutLast = inShortcut;
		playerColor = PlayerGraphics.SlugcatColor(PlayerState.slugcatCharacter);
		this.fContainer = fContainer;
		parts = new List<JollyPart>();
		playerArrow = new JollyPlayerArrow(this);
		parts.Add(playerArrow);
		offRoom = new JollyOffRoom(this);
		parts.Add(offRoom);
		addedDeathBumpThisSession = false;
	}

	public override void ClearSprites()
	{
		base.ClearSprites();
		for (int i = 0; i < parts.Count; i++)
		{
			parts[i].ClearSprites();
		}
	}

	public override void Draw(float timeStacker)
	{
		base.Draw(timeStacker);
		for (int i = 0; i < parts.Count; i++)
		{
			parts[i].Draw(timeStacker);
		}
	}

	public void AddDeathBump()
	{
		if (PlayerRoomBeingViewed && abstractPlayer.realizedCreature != null)
		{
			deathBump = new JollyDeathBump(this);
			parts.Add(deathBump);
		}
		else
		{
			hud.PlaySound(SoundID.UI_Multiplayer_Player_Dead_A);
			hud.PlaySound(SoundID.UI_Multiplayer_Player_Dead_B);
		}
		addedDeathBumpThisSession = true;
	}

	public override void Update()
	{
		base.Update();
		inShortcutLast = inShortcut;
		camPos = Camera.pos;
		if (RealizedPlayer != null)
		{
			inShortcut = RealizedPlayer.inShortcut;
		}
		if ((PlayerState.dead || PlayerState.permaDead) && !addedDeathBumpThisSession)
		{
			AddDeathBump();
		}
		for (int num = parts.Count - 1; num >= 0; num--)
		{
			if (parts[num].slatedForDeletion)
			{
				if (parts[num] == playerArrow)
				{
					playerArrow = null;
				}
				else if (parts[num] == offRoom)
				{
					offRoom = null;
				}
				else if (parts[num] == deathBump)
				{
					deathBump = null;
				}
				parts[num].ClearSprites();
				parts.RemoveAt(num);
			}
			else if (Camera.InCutscene && Camera.cutsceneType == RoomCamera.CameraCutsceneType.VoidSea)
			{
				parts[num].slatedForDeletion = true;
			}
			else
			{
				parts[num].forceHide = (Camera.InCutscene && Camera.cutsceneType == RoomCamera.CameraCutsceneType.EndingOE) || Camera.cutsceneType == RoomCamera.CameraCutsceneType.HunterStart;
				parts[num].Update();
			}
		}
	}
}
