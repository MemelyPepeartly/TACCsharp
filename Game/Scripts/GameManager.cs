using Godot;

public partial class GameManager : Node
{
	private CutsceneLeaf _cutsceneLeaf;

	public override void _Ready()
	{
		_cutsceneLeaf = GetNode<CutsceneLeaf>("CutsceneLeaf");
		
		// Trigger the first cutscene
		StartCutscene("res://Game/Data/Cutscenes/Prologue.json");
	}

	public void StartCutscene(string jsonPath)
	{
		GD.Print("Starting cutscene...");
		_cutsceneLeaf.LoadCutscene(jsonPath);
	}
}
