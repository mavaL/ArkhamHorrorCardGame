using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCardLogic : MonoBehaviour
{
	// Is this player card been played/revealed
	protected bool	m_isActive = false;

	public virtual bool CanTrigger() { return true; }
	public virtual void	OnReveal(Card card)
	{
		Player.Get().m_currentAction.Pop();
	}
	public virtual void	OnDiscard(Card card) { }
	public virtual void AddAssetResource(int num) { }
	public virtual int GetAssetResource() { return 0; }
	public virtual bool HasUseLimit() { return false; }
	public virtual void OnUseReactiveAsset() { }
	public virtual void OnPlayReactiveEvent(Card card) { }
	public virtual void OnSkillTest(int result) { }
}
