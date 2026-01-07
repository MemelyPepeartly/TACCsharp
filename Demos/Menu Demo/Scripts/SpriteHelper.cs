using Godot;
using TACCsharp.TACC.Models;

namespace TACCsharp.Demos.Menu_Demo.Scripts
{
	public partial class SpriteHelper : Node
	{
		private const string WalkSheetPath = "res://Demos/Assets/Sprites/walkcycle/BODY_male.png";
		private const string SpriteId = "walk_demo_sprite";
		private const float FrameSize = 64f;
		private const float SpriteScale = 2f;
		private const float MoveSpeed = 160f;
		private const float WalkFps = 8f;
		private static readonly Vector2 FallbackSpawn = new Vector2(480f, 300f);

		private readonly Stem _stem;
		private SpriteLeaf _spriteLeaf;
		private AnimatedSprite2D _animatedSprite;
		private Vector2 _position = FallbackSpawn;
		private int _directionRow = 2;
		private bool _isMoving;
		private bool _isActive;

		public SpriteHelper(Stem stem)
		{
			_stem = stem;
			_spriteLeaf = _stem.GetNodeOrNull<SpriteLeaf>("SpriteLeaf");
			if (_spriteLeaf == null)
			{
				GD.PrintErr("SpriteLeaf not found in Stem.");
			}
		}

		public void StartWalkDemo()
		{
			if (_spriteLeaf == null)
			{
				return;
			}

			SetActive(true);
			_spriteLeaf.ClearSprites();
			_directionRow = 2;
			_isMoving = false;
			_position = GetViewportCenter();
			UpsertSprite(_directionRow);
			_animatedSprite = _spriteLeaf.GetSpriteNode(SpriteId) as AnimatedSprite2D;
			if (_animatedSprite != null)
			{
				_animatedSprite.Stop();
				_animatedSprite.Frame = 0;
				_animatedSprite.Position = _position;
			}
		}

		public void SetActive(bool isActive)
		{
			_isActive = isActive;
			ProcessMode = isActive ? Node.ProcessModeEnum.Inherit : Node.ProcessModeEnum.Disabled;

			if (_spriteLeaf == null)
			{
				return;
			}

			_spriteLeaf.Visible = isActive;
			_spriteLeaf.ProcessMode = isActive ? Node.ProcessModeEnum.Inherit : Node.ProcessModeEnum.Disabled;
			if (!isActive)
			{
				_spriteLeaf.ClearSprites();
				_animatedSprite = null;
			}
		}

		public override void _Process(double delta)
		{
			if (!_isActive || _spriteLeaf == null)
			{
				return;
			}

			Vector2 input = GetMovementInput();
			bool moving = input != Vector2.Zero;
			int row = _directionRow;

			if (moving)
			{
				row = GetDirectionRow(input);
				_position += input.Normalized() * MoveSpeed * (float)delta;
				ClampToViewport();
			}

			if (row != _directionRow)
			{
				_directionRow = row;
				UpsertSprite(row);
				_animatedSprite = _spriteLeaf.GetSpriteNode(SpriteId) as AnimatedSprite2D;
			}

			if (_animatedSprite == null)
			{
				return;
			}

			_animatedSprite.Position = _position;

			if (moving != _isMoving)
			{
				_isMoving = moving;
				if (_isMoving)
				{
					_animatedSprite.Play();
				}
				else
				{
					_animatedSprite.Stop();
					_animatedSprite.Frame = 0;
				}
			}
		}

		private Vector2 GetMovementInput()
		{
			float x = 0f;
			float y = 0f;

			if (Input.IsKeyPressed(Key.A))
			{
				x -= 1f;
			}

			if (Input.IsKeyPressed(Key.D))
			{
				x += 1f;
			}

			if (Input.IsKeyPressed(Key.W))
			{
				y -= 1f;
			}

			if (Input.IsKeyPressed(Key.S))
			{
				y += 1f;
			}

			return new Vector2(x, y);
		}

		private int GetDirectionRow(Vector2 input)
		{
			if (Mathf.Abs(input.Y) >= Mathf.Abs(input.X))
			{
				return input.Y < 0f ? 0 : 2;
			}

			return input.X < 0f ? 1 : 3;
		}

		private void UpsertSprite(int row)
		{
			var spriteData = new SpriteData
			{
				Id = SpriteId,
				Animation = "walk",
				Frame = 0,
				Position = new Vector2Data { X = _position.X, Y = _position.Y },
				Scale = new Vector2Data { X = SpriteScale, Y = SpriteScale },
				Centered = true,
				Sheet = new SpriteSheetData
				{
					Path = WalkSheetPath,
					FrameSize = new Vector2Data { X = FrameSize, Y = FrameSize },
					Row = row,
					Start = 0,
					Count = 9,
					Loop = true,
					Fps = WalkFps
				}
			};

			_spriteLeaf.UpsertSprite(spriteData);
		}

		private Vector2 GetViewportCenter()
		{
			var viewport = GetViewport();
			if (viewport == null)
			{
				return FallbackSpawn;
			}

			Vector2 size = viewport.GetVisibleRect().Size;
			if (size == Vector2.Zero)
			{
				return FallbackSpawn;
			}

			return size * 0.5f;
		}

		private void ClampToViewport()
		{
			var viewport = GetViewport();
			if (viewport == null)
			{
				return;
			}

			Vector2 size = viewport.GetVisibleRect().Size;
			if (size == Vector2.Zero)
			{
				return;
			}

			float margin = FrameSize * SpriteScale * 0.5f;
			_position = new Vector2(
				Mathf.Clamp(_position.X, margin, size.X - margin),
				Mathf.Clamp(_position.Y, margin, size.Y - margin));
		}
	}
}
