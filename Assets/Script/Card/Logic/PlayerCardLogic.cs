using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCardLogic : MonoBehaviour
{
	// Is this player card been played/revealed
	protected bool	m_isActive = false;

	public virtual bool CanTrigger() { return true; }
	// Called when gain card. If return false, don't add to hand
	public virtual bool OnGainCard() { return true; }
	// Called when play card
	public virtual void	OnReveal(Card card)
	{
		Player.Get().m_currentAction.Pop();
	}
	public virtual void	OnDiscard(Card card)
	{
		m_isActive = false;
	}
	public virtual void AddAssetResource(int num) { }
	public virtual int GetAssetResource() { return 0; }
	public virtual bool HasUseLimit() { return false; }
	public virtual void OnUseReactiveAsset() { }
	public virtual void OnPlayReactiveEvent(Card card) { }
	public virtual void OnSkillTest(int result) { }
	public virtual string GetLog() { return ""; }
}
