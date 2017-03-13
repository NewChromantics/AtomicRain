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
	public RectTransform	World;
	public PlayerRenderer	PlayerTemplate;
	public GameObject		MissileTemplate;

	bool					StateFirstUpdate;
	float					StateTime;
	List<TPlayer>			Players;
	List<TMissile>			Missiles;

	[Range(0,1)]
	public float			JoystickThreshold = 0.01f;

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
	[Range(1,40)]
	public float			MissileFlightDuration = 10;
	[Range(0,100)]
	public float			MissileVelocity = 40;

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
		public string			JoystickName;
		public PlayerRenderer	Sprite;
		public bool				Alive = true;
		public float			Angle = 0;

		public int			GetJoystickLeftRight()
		{
			try
			{
				//	button = return "joystick " + (Joystick+1) + " button " + Button;
				var Reading = Input.GetAxis( JoystickName + " horizontal");
				if ( Reading != 0 )
					Debug.Log( JoystickName + " = " + Reading );
				return (int)Reading;
				//return ( Reading < -100 ) ? -1 : ( Reading > 100 ) ? 1 : 0;
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
		float				InitialVelocity = 30;
		float				Gravity = 9.8f;
		float				TimeScalar = 1;
				float vyt(float v,float a,float t){return v*Mathf.Sin( Mathf.Deg2Rad*a )-Gravity*t;}		float yt(float v,float a,float t){return v*Mathf.Sin(Mathf.Deg2Rad*a)*t-0.5f*Gravity*t*t;}		float vxf(float v,float a){return v*Mathf.Cos(Mathf.Deg2Rad*a);}		float trad(float v,float y){return Mathf.Sqrt(v*v/(Gravity*Gravity)-2*y/Gravity);}

		public TMissile(float FireVelocity,float _TimeScalar)
		{
			TimeScalar = _TimeScalar;
			InitialVelocity = FireVelocity;
		}

		public float		GetTimeUntilFloor()
		{
			return TimeScalar;
		}

		//	from http://hyperphysics.phy-astr.gsu.edu/hbase/traj.html
		public Vector2		GetPathPosition(float Time)
		{
			float tt = Time ;
			float ag = -Angle -270;
			float v = InitialVelocity;
			float HorzVelocity = vxf(v,ag);
			float HorzDistance = vxf(v,ag)*tt;
			float VertVelocity = vyt(v,ag,tt);
			float VertDistance = yt(v,ag,tt);
			return StartPosition + new Vector2( HorzDistance, VertDistance );
		}
	}

	//	get pos in Canvas-World space
	Vector3 GetWorldPos(Transform Obj)
	{
		//return Vector3.zero;
		var Pos = Obj.position;
		var WorldPos = World.worldToLocalMatrix.MultiplyPoint (Pos);
		return WorldPos;
	}

	TState Update_InitGame()
	{
		//	count players and adjust scene
		var JoystickNames = new string[4]{"0","1","2","3"};//Input.GetJoystickNames();
		if ( JoystickNames.Length == 0 )
			return TState.Error_NoPlayers;
		
		Players = new List<TPlayer>();
		//	built in joystick name is the OS name. not the input name
		int JoystickIndex = 1;
		foreach ( var JoystickName in JoystickNames )
		{
			var Player = new TPlayer();
			//Player.JoystickName = JoystickName;
			Player.JoystickName = "joystick " + JoystickIndex;
			Player.Sprite = GameObject.Instantiate( PlayerTemplate );
			Player.Sprite.transform.SetParent( PlayerTemplate.transform.parent, false );
			Player.Sprite.gameObject.SetActive(true);
			Players.Add( Player );

			Debug.Log ("Found joystick " + Player.JoystickName);
			JoystickIndex++;
		}

		return TState.InitRound;
	}

	TState Update_InitRound()
	{
		OnRoundStart.Invoke();

		foreach (var Player in Players)
			Player.Angle = 0;
		
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
			Player.Sprite.SetAngle( Player.Angle );
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

			var Missile = new TMissile( MissileVelocity, MissileFlightDuration );
			Missile.Angle = Player.Angle;
			var Pos3 = GetWorldPos (Player.Sprite.CannonLaunchPos.transform);
			Missile.StartPosition = new Vector2( Pos3.x, Pos3.y );
			Missile.Sprite = GameObject.Instantiate( MissileTemplate );
			Missile.Sprite.transform.SetParent( MissileTemplate.transform.parent, false );
			Missile.Sprite.gameObject.SetActive(true);

			Missiles.Add( Missile );
		}

		return TState.MissileFlight;
	}

	TState Update_MissileFlight()
	{
		float MissileTime = StateTime;

		foreach ( var Missile in Missiles )
		{
			var Pos = Missile.GetPathPosition( MissileTime );
			var Pos3 = Missile.Sprite.transform.localPosition;

			//	wrap
			if (Pos.x < World.rect.xMin)
				Pos.x += World.rect.width;
			if (Pos.x > World.rect.xMax)
				Pos.x -= World.rect.width;
			
			Pos3.x = Pos.x;
			Pos3.y = Pos.y;
			Missile.Sprite.transform.localPosition = Pos3;

			//	check collisions
		}

		UpdatePlayerInput();

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
		if (World == null)
			World = PlayerTemplate.transform.parent.GetComponent<RectTransform>();
		
		PlayerTemplate.gameObject.SetActive(false);
		MissileTemplate.gameObject.SetActive(false);
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
			//Debug.Log("state " + State + "; " + StateTime );
			
		}
	}
}
