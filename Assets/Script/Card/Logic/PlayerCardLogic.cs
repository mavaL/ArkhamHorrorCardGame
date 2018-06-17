using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCardLogic : MonoBehaviour
{
	public virtual bool CanPlayEvent() { return true; }
	public virtual void	OnReveal(Card card) { }
	public virtual void	OnDiscard(Card card) { }
}
