using Godot;

public partial class DialogBox : CanvasLayer
{
	[Export] private Label _characterName;
	[Export] private Label _dialogueText;
	[Export] private TextureRect _characterPortrait;
	
	public void UpdateDialogue(string character, string dialogue, Texture2D portrait = null)
	{
		GD.Print($"Updating dialog: Character: {character}, Dialogue: {dialogue}");
		
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
	}
}
