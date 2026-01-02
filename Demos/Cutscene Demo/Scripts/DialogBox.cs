using Godot;

public partial class DialogBox : CanvasLayer
{
	private const float PortraitPadding = 12.0f;
	[Export] private Label _characterName;
	[Export] private Label _dialogueText;
	[Export] private TextureRect _characterPortrait;
	private Vector2 _portraitBaseSize = Vector2.Zero;
	private Vector2 _portraitBasePosition = Vector2.Zero;
	private bool _portraitLayoutCached;

	public override void _Ready()
	{
		if (_characterPortrait == null)
		{
			GD.PrintErr("DialogBox portrait node is not assigned.");
			return;
		}

		_characterPortrait.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
		_characterPortrait.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
		_characterPortrait.Scale = Vector2.One;
		CallDeferred(nameof(CachePortraitLayout));
	}
	
	public void UpdateDialogue(
		string character,
		string dialogue,
		Texture2D portrait = null,
		float portraitWidth = 0.0f,
		float portraitHeight = 0.0f)
	{
		GD.Print($"Updating dialog: Character: {character}, Dialogue: {dialogue}");
		
		if (_characterPortrait == null)
		{
			GD.PrintErr("DialogBox portrait node is not assigned.");
			return;
		}

		_characterName.Text = character;
		_dialogueText.Text = dialogue;

		if (portrait != null)
		{
			_characterPortrait.Texture = portrait;
		}
		else
		{
			_characterPortrait.Texture = null; // Clear the portrait if no image is provided
		}

		ApplyPortraitSizing(portraitWidth, portraitHeight);
	}

	private void CachePortraitLayout()
	{
		if (_characterPortrait == null)
		{
			return;
		}

		_portraitBaseSize = _characterPortrait.Size;
		if (_portraitBaseSize == Vector2.Zero)
		{
			_portraitBaseSize = _characterPortrait.CustomMinimumSize;
		}

		if (_portraitBaseSize == Vector2.Zero)
		{
			_portraitBaseSize = new Vector2(256.0f, 256.0f);
		}

		_portraitBasePosition = _characterPortrait.Position;
		_portraitLayoutCached = true;
	}

	private void ApplyPortraitSizing(float portraitWidth, float portraitHeight)
	{
		if (_characterPortrait == null)
		{
			return;
		}

		if (!_portraitLayoutCached)
		{
			CachePortraitLayout();
		}

		if (_portraitBaseSize == Vector2.Zero)
		{
			return;
		}

		Vector2 newSize = _portraitBaseSize;
		if (portraitWidth > 0.0f && portraitHeight > 0.0f)
		{
			newSize = new Vector2(portraitWidth, portraitHeight);
		}

		var parent = _characterPortrait.GetParent() as Control;
		float baseRight = _portraitBasePosition.X + _portraitBaseSize.X;
		if (parent != null)
		{
			float maxWidth = Mathf.Max(0.0f, parent.Size.X - (PortraitPadding * 2.0f));
			float maxHeight = Mathf.Max(0.0f, parent.Size.Y - (PortraitPadding * 2.0f));
			if (maxWidth > 0.0f && newSize.X > maxWidth)
			{
				newSize.X = maxWidth;
			}
			if (maxHeight > 0.0f && newSize.Y > maxHeight)
			{
				newSize.Y = maxHeight;
			}

			baseRight = Mathf.Min(baseRight, parent.Size.X - PortraitPadding);
		}

		float targetBottom = 0.0f;

		_characterPortrait.Size = newSize;
		float targetX = baseRight - newSize.X;
		if (parent != null)
		{
			float minX = PortraitPadding;
			float maxX = parent.Size.X - newSize.X - PortraitPadding;
			targetX = maxX < minX ? minX : Mathf.Clamp(targetX, minX, maxX);
		}

		_characterPortrait.Position = new Vector2(targetX, targetBottom - newSize.Y);
	}
}
