using Sandbox;

[Library( "weapon_fists", Title = "Fists", Spawnable = false )]
public partial class WeaponFists : Weapon
{
	public override string ViewModelPath => "models/weapons/fists/v_fists.vmdl";
	public override float PrimaryRate => 0.9f;
	public override float ReloadTime => 0f;

	public override void Spawn()
	{
		base.Spawn();
	}

	public override void ActiveStart( Entity ent )
	{
		base.ActiveStart( ent );
		if ( !IsClient ) return;
		ViewModelEntity.FieldOfView = 48;
	}
	private async void AttackAsync( float delay )
	{
		//this is awesome
		await GameTask.DelaySeconds( delay );
		Melee( 1000f, 90f );


	}
	public override void AttackPrimary()
	{

		base.AttackPrimary();
		TimeSincePrimaryAttack = 0;

		(Owner as AnimEntity)?.SetAnimBool( "b_attack", true );
		ShootEffects();
		AttackAsync( 0.34f );
	}

	public override void AttackSecondary()
	{
		base.AttackSecondary();
		TimeSincePrimaryAttack = 0;
		TimeSinceSecondaryAttack = 0;
	}


	/// <summary>
	/// Custom ShootBullets function for melee.
	/// </summary>
	/// <param name="damage"></param>
	/// <param name="range"></param>
	/// <returns></returns>
	public bool Melee( float damage, float range = DefaultBulletRange )
	{
		var pos = Owner.EyePos;
		var forward = Owner.EyeRot.Forward;
		forward = forward.Normal;


		foreach ( var tr in TraceBullet( pos, pos + forward * range, 15f ) )
		{
			if ( tr.Entity.IsValid() )
			{
				tr.Surface.DoBulletImpact( tr );
			}
			else
			{
				continue;
			}

			if ( !IsServer ) continue;
			using ( Prediction.Off() )
			{
				var damageInfo = DamageInfo.FromBullet( tr.EndPos, forward * 100 * 1, damage )
					.UsingTraceResult( tr )
					.WithAttacker( Owner )
					.WithWeapon( this );

				tr.Entity.TakeDamage( damageInfo );
				PlaySound( "punch" );
				return true;


			}


		}

		if ( IsServer )
		{
			PlaySound( "punch_miss" );
		}


		return false;
	}




	[ClientRpc]
	protected override void ShootEffects()
	{
		Host.AssertClient();


		if ( Owner == Local.Pawn )
		{
			new Sandbox.ScreenShake.Perlin( 0.5f, 4.0f, 1.0f, 0.5f );

		}

		ViewModelEntity?.SetAnimBool( "fire", true );
		CrosshairPanel?.CreateEvent( "fire" );
	}

	public override void SimulateAnimator( PawnAnimator anim )
	{
		anim.SetParam( "holdtype", 4 ); // TODO this is shit
		anim.SetParam( "aimat_weight", 1f );
	}

	public override void OnCarryDrop( Entity dropper )
	{
		Delete();
	}
}