using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreacheryLogic : MonoBehaviour
{
	// Is this treachery card been played/revealed?
	private bool	m_isActive = false;
	public virtual void	OnReveal() { }
	public virtual void OnSkillTest() { }
}
