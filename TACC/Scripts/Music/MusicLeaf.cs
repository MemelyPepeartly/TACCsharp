using Godot;
using System;
using TACCsharp.TACC.Models;
using TACCsharp.TACC.Serialization;
using TACCsharp.TACC.State;
using FileAccess = Godot.FileAccess;

public partial class MusicLeaf : AudioStreamPlayer, ILeafStateSource
{
	[Export] public string JsonPath { get; set; }

	public string StateKey => LeafStateKeys.Music;
	public event Action<LeafStateSnapshot> StateChanged;

	private string _loadedMusicPath;
	private string _trackPath;
	private bool _loopEnabled = true;
	private const double PlaybackUpdateInterval = 0.25;
	private double _playbackUpdateTimer;

	public override void _Ready()
	{
		AddToGroup(LeafStateGroups.LeafStateSourceGroup);
		Finished += OnFinished;

		if (!string.IsNullOrWhiteSpace(JsonPath))
		{
			LoadMusic(JsonPath);
		}
	}

	public override void _ExitTree()
	{
		Finished -= OnFinished;
	}

	public override void _Process(double delta)
	{
		if (Stream == null || StreamPaused || !Playing)
		{
			_playbackUpdateTimer = 0.0;
			return;
		}

		_playbackUpdateTimer += delta;
		if (_playbackUpdateTimer >= PlaybackUpdateInterval)
		{
			_playbackUpdateTimer = 0.0;
			EmitStateChanged();
		}
	}

	public void LoadMusic(string jsonPath)
	{
		if (!FileAccess.FileExists(jsonPath))
		{
			GD.PrintErr($"Music JSON file not found: {jsonPath}");
			return;
		}

		_loadedMusicPath = jsonPath;

		try
		{
			using var file = FileAccess.Open(jsonPath, FileAccess.ModeFlags.Read);
			string jsonContent = file.GetAsText();
			if (!TaccJson.TryParseDictionary(jsonContent, out var root, out var error))
			{
				GD.PrintErr($"ERROR: Failed to parse music JSON: {error}");
				return;
			}

			var data = MusicData.FromDictionary(root);

			if (data == null)
			{
				GD.PrintErr("ERROR: Music JSON deserialized to null.");
				return;
			}

			ApplyMusic(data);
		}
		catch (Exception ex)
		{
			GD.PrintErr($"ERROR: Failed to load music: {ex.Message}");
		}
	}

	public void PlayMusic()
	{
		if (Stream == null)
		{
			GD.PrintErr("No music loaded to play.");
			return;
		}

		StreamPaused = false;
		if (!Playing)
		{
			Play();
		}

		EmitStateChanged();
	}

	public void PlayMusic(string trackPath, bool? loop = null, float? volumeDb = null, string bus = null, float? pitchScale = null)
	{
		if (!SetStream(trackPath, loop))
		{
			return;
		}

		ApplyOptionalSettings(volumeDb, bus, pitchScale);

		StreamPaused = false;
		Play();
		EmitStateChanged();
	}

	public void StopMusic()
	{
		if (!Playing && !StreamPaused)
		{
			return;
		}

		Stop();
		StreamPaused = false;
		EmitStateChanged();
	}

	public void PauseMusic()
	{
		if (!Playing || StreamPaused)
		{
			return;
		}

		StreamPaused = true;
		EmitStateChanged();
	}

	public void ResumeMusic()
	{
		if (!StreamPaused)
		{
			return;
		}

		StreamPaused = false;
		if (!Playing && Stream != null)
		{
			Play();
		}

		EmitStateChanged();
	}

	public void ClearMusic()
	{
		Stop();
		StreamPaused = false;
		Stream = null;
		_trackPath = null;
		_loadedMusicPath = null;
		EmitStateChanged();
	}

	public void SetVolumeDb(float volumeDb)
	{
		if (Math.Abs(VolumeDb - volumeDb) < 0.01f)
		{
			return;
		}

		VolumeDb = volumeDb;
		EmitStateChanged();
	}

	public void SetPitchScale(float pitchScale)
	{
		if (pitchScale <= 0f)
		{
			GD.PrintErr("PitchScale must be greater than zero.");
			return;
		}

		if (Math.Abs(PitchScale - pitchScale) < 0.001f)
		{
			return;
		}

		PitchScale = pitchScale;
		EmitStateChanged();
	}

	public void SetBus(string bus)
	{
		if (string.IsNullOrWhiteSpace(bus))
		{
			GD.PrintErr("Music bus name is empty.");
			return;
		}

		if (Bus == bus)
		{
			return;
		}

		Bus = bus;
		EmitStateChanged();
	}

	public void SetLoop(bool loop)
	{
		if (_loopEnabled == loop)
		{
			return;
		}

		_loopEnabled = loop;
		if (Stream != null)
		{
			ApplyLoop(Stream, loop);
		}

		EmitStateChanged();
	}

	public LeafStateSnapshot GetStateSnapshot()
	{
		return new MusicStateSnapshot
		{
			MusicPath = _loadedMusicPath ?? JsonPath,
			TrackPath = _trackPath ?? Stream?.ResourcePath,
			IsPlaying = Playing,
			IsPaused = StreamPaused,
			Loop = _loopEnabled,
			VolumeDb = VolumeDb,
			PitchScale = PitchScale,
			Bus = Bus,
			PlaybackPosition = GetPlaybackPositionSafe()
		};
	}

	private void ApplyMusic(MusicData data)
	{
		bool streamLoaded = false;
		bool hasTrackPath = !string.IsNullOrWhiteSpace(data.TrackPath);

		if (hasTrackPath)
		{
			streamLoaded = SetStream(data.TrackPath, data.Loop);
		}

		ApplyOptionalSettings(data.VolumeDb, data.Bus, data.PitchScale);

		if (data.Autoplay == true && (streamLoaded || Stream != null))
		{
			StreamPaused = false;
			if (streamLoaded || !Playing)
			{
				Play();
			}
		}
		else if (streamLoaded)
		{
			Stop();
			StreamPaused = false;
		}

		EmitStateChanged();
	}

	private bool SetStream(string trackPath, bool? loop)
	{
		if (string.IsNullOrWhiteSpace(trackPath))
		{
			GD.PrintErr("Music track path is empty.");
			return false;
		}

		var stream = GD.Load<AudioStream>(trackPath);
		if (stream == null)
		{
			GD.PrintErr($"Music track not found: {trackPath}");
			return false;
		}

		_trackPath = trackPath;

		if (loop.HasValue)
		{
			_loopEnabled = loop.Value;
		}

		ApplyLoop(stream, _loopEnabled);

		Stream = stream;
		return true;
	}

	private void ApplyOptionalSettings(float? volumeDb, string bus, float? pitchScale)
	{
		if (volumeDb.HasValue)
		{
			VolumeDb = volumeDb.Value;
		}

		if (!string.IsNullOrWhiteSpace(bus))
		{
			Bus = bus;
		}

		if (pitchScale.HasValue && pitchScale.Value > 0f)
		{
			PitchScale = pitchScale.Value;
		}
		else if (pitchScale.HasValue)
		{
			GD.PrintErr("PitchScale must be greater than zero.");
		}
	}

	private void ApplyLoop(AudioStream stream, bool loop)
	{
		switch (stream)
		{
			case AudioStreamOggVorbis ogg:
				ogg.Loop = loop;
				break;
			case AudioStreamWav wav:
				wav.LoopMode = loop ? AudioStreamWav.LoopModeEnum.Forward : AudioStreamWav.LoopModeEnum.Disabled;
				break;
			case AudioStreamMP3 mp3:
				mp3.Loop = loop;
				break;
		}
	}

	private double GetPlaybackPositionSafe()
	{
		if (Stream == null)
		{
			return 0.0;
		}

		return GetPlaybackPosition();
	}

	private void OnFinished()
	{
		EmitStateChanged();
	}

	private void EmitStateChanged()
	{
		StateChanged?.Invoke(GetStateSnapshot());
	}
}
