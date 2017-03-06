using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class UnityEvent_int : UnityEvent <int> {}



public class Game : MonoBehaviour {

	enum TState
	{
		InitGame,
		InitRound,
		Countdown,
		Fire,
		MissileFlight,
		EndRound,
		EndGame_Winner,
		EndGame_Draw,
		Error_NoPlayers,
	}

	private TState			State = TState.InitGame;
	public GameObject		PlayerTemplate;
	public GameObject		MissileTemplate;

	bool					StateFirstUpdate;
	float					StateTime;
	List<TPlayer>			Players;
	List<TMissile>			Missiles;

	[Header("Round Start")]
	public UnityEvent		OnRoundStart;

	[Header("Countdown")]
	[Range(0,30)]
	public int				CountdownDuration = 10;
	public UnityEvent_int	OnCountdownUpdate;
	public UnityEvent		OnCountdownFinished;
	[Range(1,10)]
	public float			AngleSpeed = 1;
	[Range(1,90)]
	public float			AngleMax = 45;

	[Header("Missile Flight")]
	[Range(0,40)]
	public float			MissileFlightDuration = 10;

	[Header("End Game")]
	[Range(0,40)]
	public float			EndGameDuration = 20;
	public UnityEvent_int	OnEndGameWinner;
	public UnityEvent		OnEndGameDraw;
	public UnityEvent		OnErrorNoPlayers;
	[Range(0,40)]
	public float			ErrorDuration = 20;


	

	class TPlayer
	{
		public string		JoystickName;
		public GameObject	Sprite;
		public bool			Alive = true;
		public float		Angle = 0;

		public int			GetJoystickLeftRight()
		{
			try
			{
				var Reading = Input.GetAxisRaw( JoystickName + " Horizontal");
				Debug.Log( JoystickName + " = " + Reading );
				return ( Reading < -100 ) ? -1 : ( Reading > 100 ) ? 1 : 0;
			}
			catch(System.Exception e)
			{
				Debug.LogException(e);
				return 0;
			}
		}
	}

	class TMissile
	{
		public Vector2		StartPosition;
		public float		Angle = 0;
		public GameObject	Sprite;
		float				InitialVelocity = 10;

				float vyt(float v,float a,float t){return v*Mathf.Sin(a*Mathf.PI/180.0f)-9.8f*t;}		float yt(float v,float a,float t){return v*Mathf.Sin(a*Mathf.PI/180.0f)*t-0.5f*9.8f*t*t;}		float vxf(float v,float a){return v*Mathf.Cos(a*Mathf.PI/180.0f);}		float trad(float v,float y){return Mathf.Sqrt(v*v/(9.8f*9.8f)-2*y/9.8f);}

		//	from http://hyperphysics.phy-astr.gsu.edu/hbase/traj.html
		public Vector2		GetPathPosition(float Time)
		{
			float tt = Time;
			float ag = Angle;
			float v = InitialVelocity;
			float HorzVelocity = vxf(v,ag);
			float HorzDistance = vxf(v,ag)*tt;
			float VertVelocity = vyt(v,ag,tt);
			float VertDistance = yt(v,ag,tt);
			return new Vector2( HorzDistance, VertDistance );
		}
	}

	TState Update_InitGame()
	{
		//	count players and adjust scene
		var JoystickNames = Input.GetJoystickNames();
		if ( JoystickNames.Length == 0 )
			return TState.Error_NoPlayers;

		Players = new List<TPlayer>();
		foreach ( var JoystickName in JoystickNames )
		{
			var Player = new TPlayer();
			Player.JoystickName = JoystickName;
			Player.Sprite = GameObject.Instantiate( PlayerTemplate );
			Player.Sprite.gameObject.SetActive(true);
			Player.Sprite.transform.SetParent( PlayerTemplate.transform.parent, true );
			Players.Add( Player );
		}

		return TState.InitRound;
	}

	TState Update_InitRound()
	{
		OnRoundStart.Invoke();
		return TState.Countdown;
	}

	void UpdatePlayerInput()
	{
		foreach ( var Player in Players )
		{
			if ( !Player.Alive )
				continue;

			var Input = Player.GetJoystickLeftRight();
			Player.Angle += Input * AngleSpeed;
			Player.Angle = Mathf.Clamp( Player.Angle, -AngleMax, AngleMax );
		}

	}

	TState Update_Countdown()
	{
		int TimeRemaining = CountdownDuration - (int)StateTime;

		if ( TimeRemaining <= 0 )
		{
			OnCountdownFinished.Invoke();
			return TState.Fire;
		}

		UpdatePlayerInput();

		OnCountdownUpdate.Invoke( TimeRemaining );
		return TState.Countdown;
	}
	
	TState Update_Fire()
	{
		//	fire each one missile from the active players
		Missiles = new List<TMissile>();
		foreach ( var Player in Players )
		{
			if ( !Player.Alive )
				continue;

			var Missile = new TMissile();
			Missile.Angle = Player.Angle;
			var Pos3 = Player.Sprite.transform.position;
			Missile.StartPosition = new Vector2( Pos3.x, Pos3.y );
			Missile.Sprite = GameObject.Instantiate( MissileTemplate );
			Missiles.Add( Missile );
		}

		return TState.MissileFlight;
	}

	TState Update_MissileFlight()
	{
		float MissileTime = StateTime / MissileFlightDuration;

		foreach ( var Missile in Missiles )
		{
			var Pos = Missile.GetPathPosition( MissileTime );
			var Pos3 = Missile.Sprite.transform.localPosition;
			Pos3.x = Pos.x;
			Pos3.y = Pos.y;
			Missile.Sprite.transform.localPosition = Pos3;

			//	check collisions
		}

		//	check if all missles are dead
		if ( Missiles.Count == 0 )
			return TState.EndRound;

		//	gr: may need to change this to force parabolic to reach the floor by $MissileFlightDuration
		if ( StateTime >= MissileFlightDuration )
			return TState.EndRound;
		
		return TState.MissileFlight;
	}

	TState Update_EndRound()
	{
		var AlivePlayers = 0;
		foreach ( var Player in Players )
		{
			if ( Player.Alive )
				AlivePlayers++;
		}
		
		if ( AlivePlayers == 0 )
			return TState.EndGame_Draw;
		if ( AlivePlayers == 1 )
			return TState.EndGame_Winner;
		else
			return TState.InitRound;
	}

	TState Update_EndGameDraw()
	{
		if ( StateFirstUpdate )
			OnEndGameDraw.Invoke();

		if ( StateTime >= EndGameDuration )
			return TState.InitGame;
		return TState.EndGame_Draw;
	}

	TState Update_EndGameWinner()
	{
		var Winner = -1;
		for ( int i=0;	i<Players.Count;	i++ )
			if ( Players[i].Alive )
				Winner = i;

		if ( StateFirstUpdate )
			OnEndGameWinner.Invoke(Winner);

		if ( StateTime >= EndGameDuration )
			return TState.InitGame;
		return TState.EndGame_Draw;
	}

	TState Update_ErrorNoPlayers()
	{
		if ( StateFirstUpdate )
			OnErrorNoPlayers.Invoke();
		if ( StateTime > ErrorDuration )
			return TState.InitGame;

		return TState.Error_NoPlayers;
	}

	TState UpdateState()
	{
		switch ( State )
		{
			case TState.InitGame:			return Update_InitGame();
			case TState.InitRound:			return Update_InitRound();
			case TState.Countdown:			return Update_Countdown();
			case TState.Fire:				return Update_Fire();
			case TState.MissileFlight:		return Update_MissileFlight();
			case TState.EndRound:			return Update_EndRound();
			case TState.EndGame_Draw:		return Update_EndGameDraw();
			case TState.EndGame_Winner:		return Update_EndGameWinner();
			case TState.Error_NoPlayers:	return Update_ErrorNoPlayers();
		}
		throw new System.Exception("Unhandled state " + State);
		
	}

	void Start()
	{
		PlayerTemplate.SetActive(false);
	}

	void Update ()
	{
		var NewState = UpdateState();
		
		if ( NewState != State )
		{
			Debug.Log("Changing state from " + State + " to " + NewState );
			StateTime = 0;
			StateFirstUpdate = true;
			State = NewState;
		}
		else
		{
			StateFirstUpdate = false;
			StateTime += Time.deltaTime;
			Debug.Log("state " + State + "; " + StateTime );
			
		}
	}
}
