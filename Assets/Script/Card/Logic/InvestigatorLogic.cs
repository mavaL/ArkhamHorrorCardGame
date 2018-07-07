using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InvestigatorLogic : MonoBehaviour
{
	public virtual void EnterMainGame() { }
	public abstract int OnApplyElderSignEffect();
	public virtual void OnElderSignSkillTestResult(bool bSucceed) { }
}
