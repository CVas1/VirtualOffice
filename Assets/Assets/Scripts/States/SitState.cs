using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lightbug.CharacterControllerPro.Implementation;


public class SitState : CharacterState
{

    // Write your initialization code here
    protected override void Awake()
    {
        base.Awake();
    }

    // Write your transitions here
	public override void CheckExitTransition()
    {
	}

    // Write your transitions here
	public override bool CheckEnterTransition( CharacterState fromState )
    {
        return base.CheckEnterTransition( fromState );
	}

    // Write your update code here
	public override void UpdateBehaviour( float dt )
    {
        
	}
    
    
}
