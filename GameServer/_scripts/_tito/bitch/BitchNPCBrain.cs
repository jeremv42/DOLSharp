using DOL.GS;
using DOL.GS.Scripts;

namespace DOL.AI.Brain;

public class BitchNPCBrain : AmteMobBrain
{
	private bool _hasRegen = false;


	protected override void AttackMostWanted()
	{
		base.AttackMostWanted();

		if (Body.TargetObject == null)
			return;

		foreach (var npc in Body.GetNPCsInRadius(1000))
			if (npc is BitchNPC bitch && bitch.Brain is BitchNPCBrain && !bitch.InCombat)
				bitch.StartAttack(Body.TargetObject);
	}

	public override int CalculateAggroLevelToTarget(GameLiving target)
	{
		var aggro = base.CalculateAggroLevelToTarget(target);
		if (!(target is GamePlayer player))
			return aggro;

		if (player.CharacterClass.ClassType == eClassType.Hybrid)
		{
			if (player.CharacterClass.Name == "Healer" || player.CharacterClass.Name == "Cleric")
				aggro *= 2;

			if (player.HealthPercent < 50)
				aggro *= 2;
		}

		return aggro;
	}

	public override void Think()
	{
		if (this.Body.HealthPercent < 50 && !_hasRegen)
		{
			Body.Health = Body.MaxHealth;
			_hasRegen = true;
		}

		if (!Body.InCombat)
			_hasRegen = false;

		base.Think();
	}
}